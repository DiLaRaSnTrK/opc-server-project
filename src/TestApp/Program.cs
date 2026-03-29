using Core.Database;
using Infrastructure.OPC;
namespace ModbusTestApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var opcTagUpdater = new OpcTagUpdater();
            var db = new DatabaseService("system.db", opcTagUpdater);

            var opcServer = new OpcServerService(opcTagUpdater);
            await opcServer.StartAsync(); // artık async değil, await yok

            // Sahte veri üretimi
            _ = Task.Run(async () =>
            {
                int temp = 20;
                while (true)
                {
                    temp++;
                    opcTagUpdater.UpdateTag("Temperature", temp);
                    Console.WriteLine($"Temperature = {temp}");
                    await Task.Delay(2000);
                }
            });

            // Uygulama kapanmasın
            await Task.Delay(-1);

            /*Console.WriteLine("=== Modbus TCP Test Başlatılıyor ===");

            // 1️⃣ Test için örnek cihaz oluştur
            var device = new Device
            {
                DeviceId = 1,
                Name = "Test PLC",
                IPAddress = "127.0.0.1", // burada kendi PLC IP adresini gir
                Port = 502,
                SlaveId = 1
                *//*IPAddress = "192.168.33.10", // burada kendi PLC IP adresini gir
                Port = 502,
                SlaveId = 33*//*
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
            Console.ReadLine();*/
        }
    }
}
