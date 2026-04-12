// <copyright file="ModbusClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Core.Protocols
{
    using System.Net;
    using Core.Interfaces;
    using Core.Models;
    using EasyModbus;
    using Microsoft.Extensions.Logging;
    /// <summary>
    /// EasyModbus istemci sarmalayıcı.
    /// Aşama 7 güvenlik iyileştirmeleri:
    ///   - IP whitelist doğrulaması (T-04, T-12)
    ///   - Yapılandırılmış loglama: Console.WriteLine → ILogger (T-06)
    ///   - Bağlantı timeout parametresi eklendi
    ///   - Sessiz catch blokları kapatıldı — hatalar loglanıyor
    /// </summary>
    public class ModbusClientWrapper : IProtocolClient, IDisposable
    {
        private readonly Device _device;
        private readonly ModbusClient _client;
        private readonly ILogger<ModbusClientWrapper> _logger;
        private bool _disposed;

        // ── IP WHİTELİST (T-04, T-12) ─────────────────────────────────────────
        // Boş bırakılırsa whitelist devre dışı (geliştirme ortamı).
        // Üretimde appsettings veya ortam değişkeninden doldurulmalı.
        private static readonly HashSet<string> AllowedIpAddresses = new(StringComparer.OrdinalIgnoreCase)
        {
            // Örnek: "192.168.1.100", "10.0.0.50"
            // Üretimde bu listeyi konfigürasyondan okuyun.
        };

        public bool IsConnected => _client?.Connected ?? false;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public ModbusClientWrapper(Device device, ILogger<ModbusClientWrapper> logger = null)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _logger = logger;
            _client = new ModbusClient(device.IPAddress, device.Port)
            {
                // ── BAĞLANTI TIMEOUT ──────────────────────────────────────────
                // Önceki: Varsayılan (sonsuz bekleme riski)
                // Şimdi: 3 saniye — yanıtsız PLC pipeline'ı bloklamaz
                ConnectionTimeout = 3000
            };
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            // ── IP WHİTELİST KONTROLÜ (T-04, T-12) ───────────────────────────
            // Whitelist doluysa yalnızca listede olan IP'lere bağlan
            if (AllowedIpAddresses.Count > 0 &&
                !AllowedIpAddresses.Contains(_device.IPAddress))
            {
                var msg = $"[MODBUS] Bağlantı reddedildi: {_device.IPAddress} whitelist dışında.";
                _logger?.LogWarning(msg);
                throw new UnauthorizedAccessException(msg);
            }

            // IP format doğrulaması
            if (!IPAddress.TryParse(_device.IPAddress, out _))
            {
                var msg = $"[MODBUS] Geçersiz IP adresi: '{_device.IPAddress}'";
                _logger?.LogError(msg);
                throw new ArgumentException(msg);
            }

            await Task.Run(() =>
            {
                try
                {
                    if (!_client.Connected)
                    {
                        _client.UnitIdentifier = _device.SlaveId;
                        _client.Connect();
                        _logger?.LogInformation(
                            "[MODBUS] Bağlandı: {DeviceName} ({IP}:{Port})",
                            _device.Name, _device.IPAddress, _device.Port);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex,
                        "[MODBUS] Bağlantı başarısız: {DeviceName} ({IP}:{Port})",
                        _device.Name, _device.IPAddress, _device.Port);
                    throw new InvalidOperationException(
                        $"Modbus bağlantısı başarısız: {ex.Message}", ex);
                }
            }, ct);
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (_client.Connected)
                    {
                        _client.Disconnect();
                        _logger?.LogInformation(
                            "[MODBUS] Bağlantı kapatıldı: {DeviceName}", _device.Name);
                    }
                }
                catch (Exception ex)
                {
                    // ── SESSIZ CATCH KAPATILDI (T-06) ─────────────────────────
                    // Önceki: Console.WriteLine (güvenlik olayları kayboluyordu)
                    // Şimdi:  Warning olarak loglanıyor
                    _logger?.LogWarning(ex,
                        "[MODBUS] Bağlantı kapatılırken hata: {DeviceName}", _device.Name);
                }
            });
        }

        public async Task<ReadResult> ReadTagAsync(Tag tag, CancellationToken ct = default)
        {
            if (tag == null)
                return new ReadResult { Success = false, ErrorMessage = "Tag null" };

            // ── INPUT VALIDATION (T-04) ────────────────────────────────────────
            if (tag.Address < 0 || tag.Address > 65535)
            {
                _logger?.LogWarning(
                    "[MODBUS] Geçersiz adres: Tag={TagName} Address={Address}",
                    tag.Name, tag.Address);
                return new ReadResult { Success = false, ErrorMessage = "Geçersiz Modbus adresi" };
            }

            int maxRetries = 2;
            string lastError = string.Empty;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (!IsConnected)
                        await ConnectAsync(ct);

                    int quantity = GetRegisterCount(tag.DataType);
                    double value = 0;

                    await Task.Run(() =>
                    {
                        switch (tag.RegisterType)
                        {
                            case "HoldingRegister":
                                var hr = _client.ReadHoldingRegisters(tag.Address, quantity);
                                value = ConvertRegisters(hr, tag.DataType);
                                break;
                            case "InputRegister":
                                var ir = _client.ReadInputRegisters(tag.Address, quantity);
                                value = ConvertRegisters(ir, tag.DataType);
                                break;
                            case "Coil":
                                value = _client.ReadCoils(tag.Address, 1)[0] ? 1 : 0;
                                break;
                            case "DiscreteInput":
                                value = _client.ReadDiscreteInputs(tag.Address, 1)[0] ? 1 : 0;
                                break;
                            default:
                                throw new NotSupportedException(
                                    $"Desteklenmeyen register tipi: {tag.RegisterType}");
                        }
                    }, ct);

                    DataReceived?.Invoke(this, new DataReceivedEventArgs
                    {
                        TagId = tag.TagId,
                        Value = value,
                        Timestamp = DateTime.Now
                    });

                    return new ReadResult { Success = true, Values = new[] { value } };
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;

                    // ── RETRY LOGLAMA (T-06) ───────────────────────────────────
                    _logger?.LogWarning(
                        "[MODBUS] Okuma hatası (deneme {Attempt}/{Max}): " +
                        "Tag={TagName} | Hata={Error}",
                        i + 1, maxRetries, tag.Name, ex.Message);

                    await DisconnectAsync();
                }
            }

            _logger?.LogError(
                "[MODBUS] Tag okunamadı (tüm denemeler tükendi): " +
                "Tag={TagName} | SonHata={Error}",
                tag.Name, lastError);

            return new ReadResult
            {
                Success = false,
                ErrorMessage = $"Okuma başarısız ({maxRetries} deneme): {lastError}"
            };
        }

        private int GetRegisterCount(TagDataType dt) => dt switch
        {
            TagDataType.Bool => 1,
            TagDataType.Int16 => 1,
            TagDataType.UInt16 => 1,
            TagDataType.Float => 2,
            TagDataType.Int32 => 2,
            TagDataType.UInt32 => 2,
            TagDataType.Double => 4,
            _ => 1
        };

        private double ConvertRegisters(int[] r, TagDataType type)
        {
            if (r == null || r.Length == 0) return 0;
            return type switch
            {
                TagDataType.Bool => r[0] == 1 ? 1 : 0,
                TagDataType.Int16 => (short)r[0],
                TagDataType.UInt16 => r[0],
                TagDataType.Int32 => CombineToInt32(r[0], r[1]),
                TagDataType.UInt32 => (uint)CombineToInt32(r[0], r[1]),
                TagDataType.Float => CombineToFloat(r[0], r[1]),
                TagDataType.Double => CombineToDouble(r),
                _ => r[0]
            };
        }

        private float CombineToFloat(int r1, int r2)
        {
            byte[] bytes = { (byte)(r1 >> 8), (byte)(r1 & 0xFF),
                             (byte)(r2 >> 8), (byte)(r2 & 0xFF) };
            return BitConverter.ToSingle(bytes, 0);
        }

        private int CombineToInt32(int r1, int r2) => (r1 << 16) | (r2 & 0xFFFF);

        private double CombineToDouble(int[] r)
        {
            byte[] bytes =
            {
                (byte)(r[0] >> 8), (byte)(r[0] & 0xFF),
                (byte)(r[1] >> 8), (byte)(r[1] & 0xFF),
                (byte)(r[2] >> 8), (byte)(r[2] & 0xFF),
                (byte)(r[3] >> 8), (byte)(r[3] & 0xFF)
            };
            return BitConverter.ToDouble(bytes, 0);
        }

        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                if (_client.Connected)
                    _client.Disconnect();
            }
            catch { }
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
