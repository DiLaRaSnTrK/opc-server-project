// <copyright file="ModbusClient.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using EasyModbus;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// EasyModbus istemci sarmalayıcı.
    /// Aşama 7 güvenlik iyileştirmeleri:
    ///   T-04/T-12 IP whitelist ve format doğrulaması
    ///   T-06      Yapılandırılmış loglama (ILogger)
    ///   T-04      Input validation (adres aralığı kontrolü)
    /// </summary>
    public class ModbusClientWrapper : IProtocolClient, IDisposable
    {
        // IP whitelist — boşsa tüm IP'lere izin verilir (geliştirme modu)
        // Üretimde konfigürasyondan doldurulmalıdır.
        private static readonly HashSet<string> AllowedIpAddresses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly Device device;
        private readonly ModbusClient client;
        private readonly ILogger<ModbusClientWrapper>? logger;
        private bool disposed;

        /// <summary>Initializes a new instance of the <see cref="ModbusClientWrapper"/> class.</summary>
        public ModbusClientWrapper(Device device, ILogger<ModbusClientWrapper>? logger = null)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
            this.logger = logger;
            this.client = new ModbusClient(device.IPAddress, device.Port)
            {
                ConnectionTimeout = 3000,
            };
        }

        /// <inheritdoc/>
        public event EventHandler<DataReceivedEventArgs>? DataReceived;

        /// <summary>Bağlantı durumunu döndürür.</summary>
        public bool IsConnected => this.client?.Connected ?? false;

        /// <inheritdoc/>
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            if (AllowedIpAddresses.Count > 0 &&
                !AllowedIpAddresses.Contains(this.device.IPAddress))
            {
                var msg = $"[MODBUS] Bağlantı reddedildi: {this.device.IPAddress} whitelist dışında.";
                this.logger?.LogWarning("{Message}", msg);
                throw new UnauthorizedAccessException(msg);
            }

            if (!IPAddress.TryParse(this.device.IPAddress, out _))
            {
                var msg = $"[MODBUS] Geçersiz IP adresi: '{this.device.IPAddress}'";
                this.logger?.LogError("{Message}", msg);
                throw new ArgumentException(msg);
            }

            await Task.Run(
                () =>
                {
                    try
                    {
                        if (!this.client.Connected)
                        {
                            this.client.UnitIdentifier = this.device.SlaveId;
                            this.client.Connect();
                            this.logger?.LogInformation(
                                "[MODBUS] Bağlandı: {DeviceName} ({IP}:{Port})",
                                this.device.Name,
                                this.device.IPAddress,
                                this.device.Port);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger?.LogError(
                            ex,
                            "[MODBUS] Bağlantı başarısız: {DeviceName} ({IP}:{Port})",
                            this.device.Name,
                            this.device.IPAddress,
                            this.device.Port);
                        throw new InvalidOperationException($"Modbus bağlantısı başarısız: {ex.Message}", ex);
                    }
                },
                ct);
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (this.client.Connected)
                    {
                        this.client.Disconnect();
                        this.logger?.LogInformation(
                            "[MODBUS] Bağlantı kapatıldı: {DeviceName}", this.device.Name);
                    }
                }
                catch (Exception ex)
                {
                    this.logger?.LogWarning(
                        ex, "[MODBUS] Bağlantı kapatılırken hata: {DeviceName}", this.device.Name);
                }
            });
        }

        /// <inheritdoc/>
        public async Task<ReadResult> ReadTagAsync(Tag tag, CancellationToken ct = default)
        {
            if (tag == null)
            {
                return new ReadResult { Success = false, ErrorMessage = "Tag null" };
            }

            if (tag.Address < 0 || tag.Address > 65535)
            {
                this.logger?.LogWarning(
                    "[MODBUS] Geçersiz adres: Tag={TagName} Address={Address}",
                    tag.Name,
                    tag.Address);
                return new ReadResult { Success = false, ErrorMessage = "Geçersiz Modbus adresi" };
            }

            int maxRetries = 2;
            string lastError = string.Empty;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (!this.IsConnected)
                    {
                        await this.ConnectAsync(ct);
                    }

                    int quantity = GetRegisterCount(tag.DataType);
                    double value = 0;

                    await Task.Run(
                        () =>
                        {
                            switch (tag.RegisterType)
                            {
                                case "HoldingRegister":
                                    var hr = this.client.ReadHoldingRegisters(tag.Address, quantity);
                                    value = ConvertRegisters(hr, tag.DataType);
                                    break;

                                case "InputRegister":
                                    var ir = this.client.ReadInputRegisters(tag.Address, quantity);
                                    value = ConvertRegisters(ir, tag.DataType);
                                    break;

                                case "Coil":
                                    value = this.client.ReadCoils(tag.Address, 1)[0] ? 1 : 0;
                                    break;

                                case "DiscreteInput":
                                    value = this.client.ReadDiscreteInputs(tag.Address, 1)[0] ? 1 : 0;
                                    break;

                                default:
                                    throw new NotSupportedException(
                                        $"Desteklenmeyen register tipi: {tag.RegisterType}");
                            }
                        },
                        ct);

                    this.DataReceived?.Invoke(
                        this,
                        new DataReceivedEventArgs
                        {
                            TagId = tag.TagId,
                            Value = value,
                            Timestamp = DateTime.Now,
                        });

                    return new ReadResult { Success = true, Values = new[] { value } };
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    this.logger?.LogWarning(
                        "[MODBUS] Okuma hatası (deneme {Attempt}/{Max}): Tag={TagName}",
                        i + 1,
                        maxRetries,
                        tag.Name);
                    await this.DisconnectAsync();
                }
            }

            this.logger?.LogError(
                "[MODBUS] Tag okunamadı: Tag={TagName} | SonHata={Error}",
                tag.Name,
                lastError);

            return new ReadResult
            {
                Success = false,
                ErrorMessage = $"Okuma başarısız ({maxRetries} deneme): {lastError}",
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Kaynakları serbest bırakır.</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                if (this.client.Connected)
                {
                    this.client.Disconnect();
                }
            }
            catch
            {
                // Dispose sırasında hata yutulur
            }

            this.disposed = true;
        }

        private static int GetRegisterCount(TagDataType dt) => dt switch
        {
            TagDataType.Bool => 1,
            TagDataType.Int16 => 1,
            TagDataType.UInt16 => 1,
            TagDataType.Float => 2,
            TagDataType.Int32 => 2,
            TagDataType.UInt32 => 2,
            TagDataType.Double => 4,
            _ => 1,
        };

        private static double ConvertRegisters(int[] r, TagDataType type)
        {
            if (r == null || r.Length == 0)
            {
                return 0;
            }

            return type switch
            {
                TagDataType.Bool => r[0] == 1 ? 1 : 0,
                TagDataType.Int16 => (short)r[0],
                TagDataType.UInt16 => r[0],
                TagDataType.Int32 => CombineToInt32(r[0], r[1]),
                TagDataType.UInt32 => (uint)CombineToInt32(r[0], r[1]),
                TagDataType.Float => CombineToFloat(r[0], r[1]),
                TagDataType.Double => CombineToDouble(r),
                _ => r[0],
            };
        }

        private static float CombineToFloat(int r1, int r2)
        {
            byte[] bytes =
            {
                (byte)(r1 >> 8), (byte)(r1 & 0xFF),
                (byte)(r2 >> 8), (byte)(r2 & 0xFF),
            };
            return BitConverter.ToSingle(bytes, 0);
        }

        private static int CombineToInt32(int r1, int r2) => (r1 << 16) | (r2 & 0xFFFF);

        private static double CombineToDouble(int[] r)
        {
            byte[] bytes =
            {
                (byte)(r[0] >> 8), (byte)(r[0] & 0xFF),
                (byte)(r[1] >> 8), (byte)(r[1] & 0xFF),
                (byte)(r[2] >> 8), (byte)(r[2] & 0xFF),
                (byte)(r[3] >> 8), (byte)(r[3] & 0xFF),
            };
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}
