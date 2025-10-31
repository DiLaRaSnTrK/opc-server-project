using System;
using System.Threading.Tasks;
using Infrastructure.Protocols; // ModbusClient
using Core.Interfaces;         // IProtocolClient, ReadResult, DataReceivedEventArgs

namespace TestApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Modbus TCP Polling Test Başlatıldı ===");

            // 1️⃣ PLC veya sanal Modbus Slave IP ve port
            string plcIp = "192.168.33.10"; // RTU'nuzun IP adresi
            int plcPort = 502;              // Modbus TCP standart port

            // 2️⃣ ModbusClient oluştur ve Unit ID ayarla
            var client = new ModbusClient(plcIp, plcPort)
            {
                PollIntervalMs = 2000, // 2 saniyede bir polling
                UnitId = 33        // RTU'nuzun Slave ID'si (Scadepack için varsayılan 1)
            };

            // 3️⃣ Event handler
            client.DataReceived += (s, e) =>
            {
                Console.WriteLine($"[Event] Tag {e.TagId}: {e.Value} ({e.Timestamp:HH:mm:ss})");
            };

            try
            {
                // 4️⃣ Bağlantıyı kur ve polling’i başlat
                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Bağlantı kurulamadı: " + ex.Message);
                return;
            }

            // 5️⃣ 30 saniye boyunca polling’i izleyelim
            Console.WriteLine("📡 30 saniye boyunca polling devam ediyor...");
            await Task.Delay(30000);

            // 6️⃣ Bağlantıyı kapat
            await client.DisconnectAsync();
            Console.WriteLine("=== Polling Test Sonlandı ===");
        }
    }
}