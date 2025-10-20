using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Interfaces;
using Modbus.Device;
using Polly;

namespace Infrastructure.Protocols
{
    public class ModbusClient : IProtocolClient
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient _tcpClient;
        private ModbusIpMaster _master;
        private bool _connected = false;
        private CancellationTokenSource _cts;
        private Task _pollingTask;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public ModbusClient(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        // 🔁 Bağlantı Kur (Retry + Reconnect)
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, ts, retryCount, context) =>
                    {
                        Console.WriteLine($"❌ Bağlantı hatası ({retryCount}). {ts.TotalSeconds} sn sonra tekrar denenecek. Hata: {exception.Message}");
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_ip, _port);
                _master = ModbusIpMaster.CreateIp(_tcpClient);
                _connected = true;
                Console.WriteLine("✅ Modbus TCP bağlantısı kuruldu!");
            });

            // Polling başlat
            StartPolling();
        }

        // 🔌 Bağlantıyı Kapat
        public Task DisconnectAsync()
        {
            _cts?.Cancel();
            _master?.Dispose();
            _tcpClient?.Close();
            _connected = false;
            Console.WriteLine("🔻 Bağlantı sonlandırıldı.");
            return Task.CompletedTask;
        }

        // 📡 Sürekli okuma döngüsü
        private void StartPolling()
        {
            _cts = new CancellationTokenSource();
            _pollingTask = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        if (_connected)
                        {
                            var result = await ReadAsync(0, 5);
                            if (result.Success)
                                Console.WriteLine($"📈 {DateTime.Now:HH:mm:ss} => {string.Join(", ", result.Values)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Okuma hatası: {ex.Message}");
                        _connected = false;
                        await TryReconnectAsync();
                    }

                    await Task.Delay(2000); // 2 saniyede bir oku
                }
            });
        }

        // 🔄 Bağlantı koparsa yeniden bağlan
        private async Task TryReconnectAsync()
        {
            Console.WriteLine("🔁 Yeniden bağlanılıyor...");
            int attempt = 0;

            while (!_connected && attempt < 5)
            {
                try
                {
                    attempt++;
                    await ConnectAsync();
                    Console.WriteLine("✅ Yeniden bağlantı başarılı!");
                    break;
                }
                catch
                {
                    Console.WriteLine($"⚠️ {attempt}. deneme başarısız, 3 sn sonra tekrar...");
                    await Task.Delay(3000);
                }
            }

            if (!_connected)
                Console.WriteLine("⛔ Yeniden bağlantı denemeleri başarısız oldu.");
        }

        // 📘 Veri okuma
        public Task<ReadResult> ReadAsync(int startAddress, int count, CancellationToken ct = default)
        {
            if (!_connected)
                return Task.FromResult(new ReadResult { Success = false, Values = Array.Empty<double>() });

            try
            {
                ushort[] raw = _master.ReadHoldingRegisters((ushort)startAddress, (ushort)count);
                double[] values = Array.ConvertAll(raw, v => (double)v);

                for (int i = 0; i < values.Length; i++)
                {
                    DataReceived?.Invoke(this, new DataReceivedEventArgs
                    {
                        TagId = startAddress + i,
                        Value = values[i],
                        Timestamp = DateTime.Now
                    });
                }

                return Task.FromResult(new ReadResult { Success = true, Values = values });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Okuma hatası: " + ex.Message);
                return Task.FromResult(new ReadResult { Success = false, Values = Array.Empty<double>() });
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _master?.Dispose();
            _tcpClient?.Close();
        }
    }
}

/*using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Interfaces;  // IProtocolClient, ReadResult, DataReceivedEventArgs
using Modbus.Device;    // NModbus4
using Polly;            // Polly for retry policies
using Polly.Retry;


namespace Infrastructure.Protocols
{
    public class ModbusClient : IProtocolClient
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient _tcpClient;
        private ModbusIpMaster _master;
        private bool _connected = false;
        private readonly Random _rnd = new Random();

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public ModbusClient(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Bağlantı başarısız ({retryCount}). {timeSpan.TotalSeconds} sn sonra tekrar denenecek. Hata: {exception.Message}");
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_ip, _port); // PLC’ye bağlan
                _master = ModbusIpMaster.CreateIp(_tcpClient); // klasik NModbus4 oluşturucu
                _connected = true;
                Console.WriteLine("✅ Modbus TCP bağlantısı kuruldu!");
            });
        }

        public Task DisconnectAsync()
        {
            _master?.Dispose();
            _tcpClient?.Close();
            _connected = false;
            Console.WriteLine($"[ModbusClient] Bağlantı sonlandırıldı.");
            return Task.CompletedTask;
        }

        public Task<ReadResult> ReadAsync(int startAddress, int count, CancellationToken ct = default)
        {
            if (!_connected)
                return Task.FromResult(new ReadResult { Success = false, Values = Array.Empty<double>() });

            try
            {
                // PLC’den veri oku (holding register)
                ushort[] raw = _master.ReadHoldingRegisters((ushort)startAddress, (ushort)count);
                double[] values = Array.ConvertAll(raw, v => (double)v);

                // Event tetikle
                for (int i = 0; i < values.Length; i++)
                {
                    DataReceived?.Invoke(this, new DataReceivedEventArgs
                    {
                        TagId = startAddress + i,
                        Value = values[i],
                        Timestamp = DateTime.Now
                    });
                }

                return Task.FromResult(new ReadResult { Success = true, Values = values });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ModbusClient] Okuma hatası: " + ex.Message);
                return Task.FromResult(new ReadResult { Success = false, Values = Array.Empty<double>() });
            }
        }

        public void Dispose()
        {
            _master?.Dispose();
            _tcpClient?.Close();
        }
    }
}
*/