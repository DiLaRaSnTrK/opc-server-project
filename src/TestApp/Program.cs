using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
using Core.Protocols;

namespace ModbusTestApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Modbus TCP Test Başlatılıyor ===");

            // 1️⃣ Test için örnek cihaz oluştur
            var device = new Device
            {
                DeviceId = 1,
                Name = "Test PLC",
                IPAddress = "192.168.33.10", // burada kendi PLC IP adresini gir
                Port = 502,
                SlaveId = 33
            };

            // 2️⃣ Örnek tag oluştur (örneğin 40001 adresi)
            var tag = new Tag
            {
                TagId = 1,
                DeviceId = device.DeviceId,
                Name = "TankSeviye",
                Address = 3302, // çoğu Modbus sistemi 0 tabanlı adres ister (40001 -> 0)
                RegisterType = "HoldingRegister",
                DataType = TagDataType.Int16
            };

            // 3️⃣ Wrapper'ı oluştur
            using var client = new ModbusClientWrapper(device);

            // 4️⃣ Event yakalama (okunan veriler buradan da alınabilir)
            client.DataReceived += (s, e) =>
            {
                Console.WriteLine($"📡 Veri alındı -> TagId: {e.TagId}, Value: {e.Value}, Zaman: {e.Timestamp}");
            };

            try
            {
                // 5️⃣ Bağlantıyı kur
                Console.WriteLine("🔌 Bağlantı kuruluyor...");
                await client.ConnectAsync();

                // 6️⃣ Tag oku
                Console.WriteLine($"📥 {tag.Name} tag'i okunuyor...");
                var result = await client.ReadTagAsync(tag, CancellationToken.None);

                if (result.Success)
                {
                    Console.WriteLine($"✅ Okuma başarılı! Değer: {result.Values[0]}");
                }
                else
                {
                    Console.WriteLine($"❌ Okuma başarısız: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚨 Hata: {ex.Message}");
            }
            finally
            {
                // 7️⃣ Bağlantıyı kapat
                Console.WriteLine("🔒 Bağlantı kapatılıyor...");
                await client.DisconnectAsync();
            }

            Console.WriteLine("✅ Test tamamlandı!");
            Console.ReadLine();
        }
    }
}
