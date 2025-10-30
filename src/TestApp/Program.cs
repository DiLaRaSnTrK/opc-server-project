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

            // 1️⃣ PLC veya sanal Modbus Slave IP ve port
            string plcIp = "127.0.0.1"; // Sanal PLC localhost
            int plcPort = 502;          // Modbus TCP standart port

            // 2️⃣ ModbusClient oluştur ve polling interval ayarla
            var client = new ModbusClient(plcIp, plcPort)
            {
                PollIntervalMs = 2000 // 2 saniyede bir polling
            };

            // 3️⃣ Event handler: veri geldiğinde yazdır
            client.DataReceived += (s, e) =>
            {
                Console.WriteLine($"[Event] Tag {e.TagId}: {e.Value} ({e.Timestamp:HH:mm:ss})");
            };

            try
            {
                // 4️⃣ Bağlan
                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Bağlantı kurulamadı: " + ex.Message);
                return;
            }

            // 5️⃣ Manuel register okuma örneği
            int[] realAddresses = { 40001, 40002, 40003, 40004, 40005 };
            foreach (var addr in realAddresses)
            {
                int offset = addr - 0; // Modbus offset hesaplama
                try
                {
                    var result = await client.ReadAsync(offset, 1);
                    if (result.Success)
                        Console.WriteLine($"Adres {addr} (Offset {offset}) -> Değer: {result.Values[0]}");
                    else
                        Console.WriteLine($"❌ Adres {addr} okunamadı!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Adres {addr} okunamadı: {ex.Message}");
                }
            }

            // 6️⃣ 20 saniye boyunca polling ve eventleri izle
            Console.WriteLine("📡 20 saniye boyunca olayları dinliyoruz...");
            await Task.Delay(20000);

            // 7️⃣ Bağlantıyı kapat
            await client.DisconnectAsync();
            Console.WriteLine("=== Test Sonlandı ===");
        }
    }
}
