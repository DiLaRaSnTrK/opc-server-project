using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Database;
using Core.Models;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Tests
{
    // IDisposable arayüzü, test bitince temizlik (dosya silme) yapmak için kullanılır.
    public class DatabaseIntegrationTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly DatabaseService _dbService;

        public DatabaseIntegrationTests()
        {
            // 1. Her test için benzersiz, rastgele bir dosya adı üret
            _testDbPath = $"test_{Guid.NewGuid()}.db";

            // 2. Servisi bu geçici dosya ile başlat
            // Senin yazdığın constructor sayesinde yolu parametre olarak verebiliyoruz.
            _dbService = new DatabaseService(_testDbPath);
        }

        [Fact]
        public void Full_Scenario_Should_Record_History_Correctly()
        {
            // --- ARRANGE (Hazırlık) ---

            // Kanal Oluştur
            var channel = new Channel { Name = "Test Channel", Protocol = ProtocolType.ModbusTCP };
            int chId = _dbService.AddChannel(channel);

            // Cihaz Oluştur
            var device = new Device { ChannelId = chId, Name = "Test PLC", IPAddress = "127.0.0.1", Port = 502 };
            int devId = _dbService.AddDevice(device);

            // Tag Oluştur (Float Tipinde)
            var tag = new Tag
            {
                DeviceId = devId,
                Name = "Basinc_Sensoru",
                Address = 100,
                RegisterType = "HoldingRegister",
                DataType = TagDataType.Float
            };
            int tagId = _dbService.AddTag(tag);
            tag.TagId = tagId; // DB'den dönen ID'yi nesneye ata

            // --- ACT (Eylem) ---

            // Tag'e değer yazalım (Sanki Modbus'tan 12.5 gelmiş gibi)
            // Bu işlem UpdateTagValue -> TagValueParser -> DB Update -> DB Insert History zincirini tetikler.
            _dbService.UpdateTagValue(tag, 12.5);

            // Bir değer daha yazalım (Tarihçede 2 kayıt olsun)
            _dbService.UpdateTagValue(tag, 13.8);

            // --- ASSERT (Doğrulama) ---

            // 1. Tags tablosunda SON değer güncellendi mi?
            var tagsInDb = _dbService.GetTagsByDeviceId(devId);
            var updatedTag = tagsInDb.First(t => t.TagId == tagId);

            // Not: Float karşılaştırmalarında hassasiyet farkı olabileceği için ToString ile veya aralıkla bakılır ama burada direkt deneyelim.
            Assert.Equal((float)13.8, Convert.ToSingle(updatedTag.Value));

            // 2. TagHistory tablosuna kayıt düştü mü? (Grafik için kritik olan yer)
            // Senin yazdığın GetTagHistory metodunu test ediyoruz.
            // Tarih aralığı vermeden son verileri çeken versiyonu kullanalım.
            var history = _dbService.GetTagHistory(tagId);

            Assert.NotNull(history);
            Assert.Equal(2, history.Count); // İki kere update yaptık, 2 satır olmalı.
            Assert.Equal(13.8, history.Last().Value, 4); // Son eklenen (veya sıralamaya göre ilk) 13.8 olmalı
        }

        public void Dispose()
        {
            // --- TEARDOWN (Temizlik) ---
            // Test bittiğinde oluşturduğumuz geçici veritabanı dosyasını siliyoruz.
            // Böylece diskte çöp dosya kalmaz.
            if (File.Exists(_testDbPath))
            {
                try
                {
                    // Bağlantı havuzunu temizlemek gerekebilir bazen ama SQLite dosya kilidi kalkınca silinir.
                    SqliteConnection.ClearAllPools();
                    File.Delete(_testDbPath);
                }
                catch
                {
                    // Silinemezse de testi patlatma, sonraki çalıştırmada temizlenir.
                }
            }
        }
    }
}