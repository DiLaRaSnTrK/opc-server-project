using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Models;
using EasyModbus;

namespace Core.Protocols
{
    public class ModbusClientWrapper : IProtocolClient, IDisposable
    {
        private readonly Device _device;
        private readonly ModbusClient _client;
        private bool _disposed;

        public bool IsConnected => _client?.Connected ?? false;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public ModbusClientWrapper(Device device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _client = new ModbusClient(device.IPAddress, device.Port);
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!_client.Connected)
                        _client.UnitIdentifier = _device.SlaveId;
                        _client.Connect();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Modbus bağlantısı başarısız: {ex.Message}", ex);
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
                        _client.Disconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bağlantı kapatılırken hata: {ex.Message}");
                }
            });
        }

        public async Task<ReadResult> ReadTagAsync(Tag tag, CancellationToken ct = default)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            try
            {
                if (!IsConnected)
                    await ConnectAsync(ct);

                int address = tag.Address;
                int quantity = tag.DataType == TagDataType.Float ? 2 : 1;

                double value = 0;

                await Task.Run(() =>
                {
                    switch (tag.RegisterType)
                    {
                        case "HoldingRegister":
                            var hr = _client.ReadHoldingRegisters(address, quantity);
                            value = ConvertToDouble(hr, tag.DataType);
                            break;

                        case "InputRegister":
                            var ir = _client.ReadInputRegisters(address, quantity);
                            value = ConvertToDouble(ir, tag.DataType);
                            break;

                        case "Coil":
                            var coil = _client.ReadCoils(address, 1);
                            value = coil[0] ? 1 : 0;
                            break;

                        case "DiscreteInput":
                            var di = _client.ReadDiscreteInputs(address, 1);
                            value = di[0] ? 1 : 0;
                            break;

                        default:
                            throw new Exception($"Bilinmeyen register tipi: {tag.RegisterType}");
                    }
                }, ct);

                // Event fırlat
                var handler = DataReceived;
                handler?.Invoke(this, new DataReceivedEventArgs
                {
                    TagId = tag.TagId,
                    Value = value,
                    Timestamp = DateTime.Now
                });

                return new ReadResult
                {
                    Success = true,
                    Values = new[] { value }
                };
            }
            catch (Exception ex)
            {
                return new ReadResult
                {
                    Success = false,
                    Values = Array.Empty<double>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        private double ConvertToDouble(int[] registers, TagDataType dataType)
        {
            if (registers == null || registers.Length == 0)
                return 0;

            if (dataType == TagDataType.Float && registers.Length >= 2)
            {
                // Little-endian düzenine göre dönüştürme
                byte[] bytes = new byte[4];
                bytes[0] = (byte)(registers[0] & 0xFF);
                bytes[1] = (byte)(registers[0] >> 8);
                bytes[2] = (byte)(registers[1] & 0xFF);
                bytes[3] = (byte)(registers[1] >> 8);
                return BitConverter.ToSingle(bytes, 0);
            }

            return registers[0];
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (_client.Connected)
                    _client.Disconnect();
            }
            catch
            {
                // Sessizce geç
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
