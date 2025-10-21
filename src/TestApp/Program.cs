using System;
using System.Threading.Tasks;
using Infrastructure.Protocols; // ModbusClient
using Core.Interfaces;          // IProtocolClient, ReadResult, DataReceivedEventArgs

namespace TestApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Modbus TCP Test Başlatıldı ===");

            // 1️⃣ Gerçek PLC IP ve port (veya lokal simülatör)
            string plcIp = "192.168.1.10"; // PLC IP adresi
            int plcPort = 502;             // Modbus TCP portu

            // 2️⃣ ModbusClient oluştur
            var client = new ModbusClient(plcIp, plcPort)
            {
                PollIntervalMs = 2000 // 2 saniyede bir veri oku
            };

            // 3️⃣ Event handler: veri geldiğinde yazdır
            client.DataReceived += (s, e) =>
            {
                Console.WriteLine($"[Event] Tag {e.TagId}: {e.Value} ({e.Timestamp:HH:mm:ss})");
            };

            // 4️⃣ Bağlan
            await client.ConnectAsync();

            // 5️⃣ İsteğe bağlı: belirli adreslerden manuel veri oku
            int startAddress = 0;
            int count = 5;
            var result = await client.ReadAsync(startAddress, count);

            if (result.Success)
            {
                Console.WriteLine($"Okunan değerler: {string.Join(", ", result.Values)}");
            }
            else
            {
                Console.WriteLine("Veri okuma başarısız!");
            }

            // 6️⃣ 20 saniye boyunca polling ve event’leri izle
            Console.WriteLine("20 saniye boyunca eventleri izliyoruz...");
            await Task.Delay(20000);

            // 7️⃣ Bağlantıyı kapat
            await client.DisconnectAsync();

            Console.WriteLine("=== Test Sonlandı ===");
        }
    }
}
