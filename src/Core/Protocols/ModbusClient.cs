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
                    {
                        _client.UnitIdentifier = _device.SlaveId;
                        _client.Connect();
                    }
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
                int quantity = GetRegisterCount(tag.DataType);

                double value = 0;

                await Task.Run(() =>
                {
                    switch (tag.RegisterType)
                    {
                        case "HoldingRegister":
                            var hr = _client.ReadHoldingRegisters(address, quantity);
                            value = ConvertRegisters(hr, tag.DataType);
                            break;

                        case "InputRegister":
                            var ir = _client.ReadInputRegisters(address, quantity);
                            value = ConvertRegisters(ir, tag.DataType);
                            break;

                        case "Coil":
                            value = _client.ReadCoils(address, 1)[0] ? 1 : 0;
                            break;

                        case "DiscreteInput":
                            value = _client.ReadDiscreteInputs(address, 1)[0] ? 1 : 0;
                            break;

                        default:
                            throw new Exception($"Bilinmeyen register tipi: {tag.RegisterType}");
                    }
                }, ct);

                // Event
                DataReceived?.Invoke(this, new DataReceivedEventArgs
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

        // Kaç register okunacağını belirler
        private int GetRegisterCount(TagDataType dt)
        {
            return dt switch
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
        }

        // TÜM TIPLER İÇİN DOĞRU DÖNÜŞÜM
        private double ConvertRegisters(int[] r, TagDataType type)
        {
            if (r == null || r.Length == 0)
                return 0;

            switch (type)
            {
                case TagDataType.Bool:
                    return r[0] == 1 ? 1 : 0;

                case TagDataType.Int16:
                    return (short)r[0];

                case TagDataType.UInt16:
                    return r[0];

                case TagDataType.Int32:
                    return CombineToInt32(r[0], r[1]);

                case TagDataType.UInt32:
                    return (uint)CombineToInt32(r[0], r[1]);

                case TagDataType.Float:
                    return CombineToFloat(r[0], r[1]);

                case TagDataType.Double:
                    return CombineToDouble(r);

                default:
                    return r[0];
            }
        }

        // 2 Register → Float
        private float CombineToFloat(int reg1, int reg2)
        {
            byte[] bytes =
            {
                (byte)(reg1 >> 8),
                (byte)(reg1 & 0xFF),
                (byte)(reg2 >> 8),
                (byte)(reg2 & 0xFF)
            };

            return BitConverter.ToSingle(bytes, 0);
        }

        // 2 Register → INT32
        private int CombineToInt32(int reg1, int reg2)
        {
            return (reg1 << 16) | (reg2 & 0xFFFF);
        }

        // 4 Register → DOUBLE
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
