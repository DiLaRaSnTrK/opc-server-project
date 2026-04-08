using System.Net.Sockets;
using Core.Interfaces;
using Core.Models;
using EasyModbus;

namespace Core.Protocols
{
    public class ModbusClientWrapper : IProtocolClient, IDisposable
    {
        private readonly Device _device;
        private ModbusClient _client;
        private bool _disposed;

        private readonly SemaphoreSlim _lock = new(1, 1);

        private bool _isConnected;

        public bool IsConnected => _isConnected;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public ModbusClientWrapper(Device device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _client = CreateNewClient();
        }

        private ModbusClient CreateNewClient()
        {
            return new ModbusClient(_device.IPAddress, _device.Port)
            {
                UnitIdentifier = _device.SlaveId,
                ConnectionTimeout = 3000
            };
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);
            try
            {
                await ConnectInternalAsync(ct);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task ConnectInternalAsync(CancellationToken ct)
        {
            if (_isConnected) return;

            await Task.Run(() =>
            {

                ForceReplaceClient();

                try
                {
                    _client.Connect();
                    _isConnected = true;
                }
                catch (Exception ex)
                {
                    _isConnected = false;
                    throw new InvalidOperationException(
                        $"Modbus bağlantısı başarısız [{_device.IPAddress}:{_device.Port}]: {ex.Message}", ex);
                }
            }, ct);
        }

        public async Task DisconnectAsync()
        {
            await _lock.WaitAsync();
            try
            {
                ForceReplaceClient();
            }
            finally
            {
                _lock.Release();
            }
        }

        private void ForceReplaceClient()
        {
            _isConnected = false;
            try { _client.Disconnect(); } catch { }


            _client = CreateNewClient();
        }


        public async Task<ReadResult> ReadTagAsync(Tag tag, CancellationToken ct = default)
        {
            if (tag == null) 
                return new ReadResult { Success = false, ErrorMessage = "Tag null" };
        
            int maxRetries = 2;
            string lastError = "";
        
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (!IsConnected) await ConnectAsync(ct);
        
                    // Karmaşıklığı artıran Switch ve Task.Run kısmını yeni metoda taşıdık
                    double value = await ExecuteModbusRead(tag, ct);
        
                    InvokeDataReceived(tag.TagId, value);
        
                    return new ReadResult { Success = true, Values = new[] { value } };
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    await DisconnectAsync(); // Bağlantıyı sıfırla, sonraki döngüde tekrar bağlanacak
                }
            }
        
            return new ReadResult { Success = false, ErrorMessage = $"Okuma başarısız: {lastError}" };
        }
        
        // Karmaşıklığı bölen yardımcı metod (Extract Method)
        private async Task<double> ExecuteModbusRead(Tag tag, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int addr = tag.Address;
                int qty = GetRegisterCount(tag.DataType);
        
                return tag.RegisterType switch
                {
                    "HoldingRegister" => ConvertRegisters(_client.ReadHoldingRegisters(addr, qty), tag.DataType),
                    "InputRegister"   => ConvertRegisters(_client.ReadInputRegisters(addr, qty), tag.DataType),
                    "Coil"            => _client.ReadCoils(addr, 1)[0] ? 1.0 : 0.0,
                    "DiscreteInput"   => _client.ReadDiscreteInputs(addr, 1)[0] ? 1.0 : 0.0,
                    _                 => throw new NotSupportedException($"Unsupported register type: {tag.RegisterType}")
                };
            }, ct);
        }
        
        // Event fırlatma mantığını da sadeleştirdik
        private void InvokeDataReceived(int tagId, double value)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs
            {
                TagId = tagId,
                Value = value,
                Timestamp = DateTime.Now
            });
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
            _ => 1
        };

        private static double ConvertRegisters(int[] r, TagDataType type)
        {
            if (r is null || r.Length == 0) return 0;

            return type switch
            {
                TagDataType.Bool => r[0] != 0 ? 1.0 : 0.0,
                TagDataType.Int16 => (short)r[0],
                TagDataType.UInt16 => (ushort)r[0],
                TagDataType.Int32 => CombineToInt32(r[0], r[1]),
                TagDataType.UInt32 => (uint)CombineToInt32(r[0], r[1]),
                TagDataType.Float => CombineToFloat(r[0], r[1]),
                TagDataType.Double => CombineToDouble(r),
                _ => r[0]
            };
        }

        private static float CombineToFloat(int r0, int r1)
        {
            byte[] b =
            {
                (byte)(r0 >> 8), (byte)(r0 & 0xFF),
                (byte)(r1 >> 8), (byte)(r1 & 0xFF)
            };
            return BitConverter.ToSingle(b, 0);
        }

        private static int CombineToInt32(int r0, int r1)
            => (r0 << 16) | (r1 & 0xFFFF);

        private static double CombineToDouble(int[] r)
        {
            byte[] b =
            {
                (byte)(r[0] >> 8), (byte)(r[0] & 0xFF),
                (byte)(r[1] >> 8), (byte)(r[1] & 0xFF),
                (byte)(r[2] >> 8), (byte)(r[2] & 0xFF),
                (byte)(r[3] >> 8), (byte)(r[3] & 0xFF)
            };
            return BitConverter.ToDouble(b, 0);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { _client.Disconnect(); } catch { }
            _lock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
