using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Interfaces;  // IProtocolClient, ReadResult, DataReceivedEventArgs
using Modbus.Device;    // NModbus4
using Polly;            // Polly for retry policies
using Polly.Retry;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<ModbusClient> _logger;
        private CancellationTokenSource _pollingCts;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        // Polling interval (ms) config edilebilir
        public int PollIntervalMs { get; set; } = 2000;

        public ModbusClient(string ip, int port, ILogger<ModbusClient> logger = null)
        {
            _ip = ip;
            _port = port;
            _logger = logger;
        }

        /// <summary>
        /// PLC’ye bağlanır, retry ve timeout uygular
        /// </summary>
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Log($"Bağlantı başarısız ({retryCount}). {timeSpan.TotalSeconds} sn sonra tekrar denenecek. Hata: {exception.Message}");
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                _tcpClient = new TcpClient
                {
                    ReceiveTimeout = 2000,
                    SendTimeout = 2000
                };
                await _tcpClient.ConnectAsync(_ip, _port); // PLC’ye bağlan
                _master = ModbusIpMaster.CreateIp(_tcpClient); // NModbus master oluştur
                _connected = true;
                Log("✅ Modbus TCP bağlantısı kuruldu!");

                // Polling başlat
                StartPolling(ct);
            });
        }

        /// <summary>
        /// PLC’den belirli adreslerden veri okur
        /// </summary>
        public async Task<ReadResult> ReadAsync(int startAddress, int count, CancellationToken ct = default)
        {
            if (!_connected)
                return new ReadResult { Success = false, Values = Array.Empty<double>() };

            try
            {
                // PLC’den veri oku (blocking NModbus call)
                ushort[] raw = _master.ReadHoldingRegisters((ushort)startAddress, (ushort)count);
                double[] values = Array.ConvertAll(raw, v => (double)v);

                // Event tetikleme (asenkron)
                foreach (var (value, index) in values.Select((v, i) => (v, i)))
                {
                    _ = Task.Run(() => DataReceived?.Invoke(this, new DataReceivedEventArgs
                    {
                        TagId = startAddress + index,
                        Value = value,
                        Timestamp = DateTime.Now
                    }));
                }

                Log($"[ModbusClient] Adres {startAddress}’ten {count} değer okundu: {string.Join(", ", values)}");

                return await Task.FromResult(new ReadResult { Success = true, Values = values });
            }
            catch (Exception ex)
            {
                Log("[ModbusClient] Okuma hatası: " + ex.Message);
                return await Task.FromResult(new ReadResult { Success = false, Values = Array.Empty<double>() });
            }
        }

        /// <summary>
        /// Bağlantıyı kapatır ve polling’i durdurur
        /// </summary>
        public async Task DisconnectAsync()
        {
            StopPolling();
            _master?.Dispose();
            _tcpClient?.Close();
            _connected = false;
            Log("[ModbusClient] Bağlantı sonlandırıldı.");
            await Task.CompletedTask;
        }

        /// <summary>
        /// IDisposable implementasyonu
        /// </summary>
        public void Dispose()
        {
            _ = DisconnectAsync();
        }

        /// <summary>
        /// Sürekli polling başlatır
        /// </summary>
        private void StartPolling(CancellationToken ct = default)
        {
            _pollingCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            CancellationToken token = _pollingCts.Token;

            _ = Task.Run(async () =>
            {
                while (!_pollingCts.IsCancellationRequested)
                {
                    try
                    {
                        await ReadAsync(0, 5, token); // Örnek: adres 0’dan 5 değer oku
                    }
                    catch (Exception ex)
                    {
                        Log("[Polling] Hata: " + ex.Message);
                    }
                    await Task.Delay(PollIntervalMs, token);
                }
            }, token);
        }

        /// <summary>
        /// Polling durdur
        /// </summary>
        private void StopPolling()
        {
            if (_pollingCts != null && !_pollingCts.IsCancellationRequested)
            {
                _pollingCts.Cancel();
                _pollingCts.Dispose();
            }
        }

        /// <summary>
        /// Logger veya console ile mesaj yaz
        /// </summary>
        private void Log(string message)
        {
            if (_logger != null)
                _logger.LogInformation(message);
            else
                Console.WriteLine(message);
        }
    }
}
