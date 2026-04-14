// <copyright file="DatabaseServiceTests.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

// Aşama 8 — DatabaseService Genişletilmiş Testleri
// Mevcut: 1 entegrasyon testi, %52.3 coverage
// Hedef:  CRUD, silme, güncelleme, tarih aralığı → %80+ coverage

namespace Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Core.Database;
    using Core.Models;
    using Microsoft.Data.Sqlite;
    using Xunit;

    /// <summary>
    /// DatabaseService CRUD operasyonlarını ve kenar durumları test eder.
    /// Her test izole bir geçici veritabanı kullanır.
    /// </summary>
    public class DatabaseServiceTests : IDisposable
    {
        private readonly string testDbPath;
        private readonly DatabaseService db;

        public DatabaseServiceTests()
        {
            this.testDbPath = $"dbtest_{Guid.NewGuid()}.db";
            this.db = new DatabaseService(this.testDbPath);
        }

        // ── KANAL TESTLERİ ───────────────────────────────────────────────────

        [Fact]
        public void AddChannel_ShouldReturn_ValidId()
        {
            var id = this.db.AddChannel(new Channel
            {
                Name = "Kanal1",
                Protocol = ProtocolType.ModbusTCP,
            });

            Assert.True(id > 0);
        }

        [Fact]
        public void GetChannels_AfterAdd_ShouldReturn_AddedChannel()
        {
            this.db.AddChannel(new Channel { Name = "TestKanal", Protocol = ProtocolType.ModbusTCP });

            var channels = this.db.GetChannels();

            Assert.Single(channels);
            Assert.Equal("TestKanal", channels[0].Name);
            Assert.Equal(ProtocolType.ModbusTCP, channels[0].Protocol);
        }

        [Fact]
        public void UpdateChannel_ShouldChange_ChannelName()
        {
            var id = this.db.AddChannel(new Channel { Name = "Eski", Protocol = ProtocolType.ModbusTCP });

            this.db.UpdateChannel(new Channel { ChannelId = id, Name = "Yeni", Protocol = ProtocolType.ModbusTCP });

            var updated = this.db.GetChannels().First(c => c.ChannelId == id);
            Assert.Equal("Yeni", updated.Name);
        }

        [Fact]
        public void DeleteChannel_ShouldRemove_Channel()
        {
            var id = this.db.AddChannel(new Channel { Name = "Silinecek", Protocol = ProtocolType.ModbusTCP });

            this.db.DeleteChannel(id);

            var channels = this.db.GetChannels();
            Assert.DoesNotContain(channels, c => c.ChannelId == id);
        }

        [Fact]
        public void DeleteChannel_ShouldCascade_DeleteDevices()
        {
            // Kanal ekle
            var chId = this.db.AddChannel(new Channel { Name = "Kanal", Protocol = ProtocolType.ModbusTCP });

            // Cihaz ekle
            this.db.AddDevice(new Device { ChannelId = chId, Name = "Cihaz", IPAddress = "127.0.0.1", Port = 502 });

            // Kanalı sil
            this.db.DeleteChannel(chId);

            // Cascade: cihaz da silinmeli
            var devices = this.db.GetDevicesByChannelId(chId);
            Assert.Empty(devices);
        }

        [Fact]
        public void GetChannels_WhenEmpty_ShouldReturn_EmptyList()
        {
            var channels = this.db.GetChannels();
            Assert.Empty(channels);
        }

        // ── CİHAZ TESTLERİ ───────────────────────────────────────────────────

        [Fact]
        public void AddDevice_ShouldReturn_ValidId()
        {
            var chId = this.db.AddChannel(new Channel { Name = "K", Protocol = ProtocolType.ModbusTCP });

            var devId = this.db.AddDevice(new Device
            {
                ChannelId = chId,
                Name = "PLC1",
                IPAddress = "192.168.1.10",
                Port = 502,
                SlaveId = 1,
            });

            Assert.True(devId > 0);
        }

        [Fact]
        public void GetDevicesByChannelId_ShouldReturn_CorrectDevice()
        {
            var chId = this.db.AddChannel(new Channel { Name = "K", Protocol = ProtocolType.ModbusTCP });
            this.db.AddDevice(new Device { ChannelId = chId, Name = "PLC-A", IPAddress = "10.0.0.1", Port = 502 });

            var devices = this.db.GetDevicesByChannelId(chId);

            Assert.Single(devices);
            Assert.Equal("PLC-A", devices[0].Name);
            Assert.Equal("10.0.0.1", devices[0].IPAddress);
        }

        [Fact]
        public void UpdateDevice_ShouldChange_IpAddress()
        {
            var chId = this.db.AddChannel(new Channel { Name = "K", Protocol = ProtocolType.ModbusTCP });
            var devId = this.db.AddDevice(new Device { ChannelId = chId, Name = "Dev", IPAddress = "1.2.3.4", Port = 502 });

            this.db.UpdateDevice(new Device
            {
                DeviceId = devId,
                ChannelId = chId,
                Name = "Dev",
                IPAddress = "10.10.10.10",
                Port = 503,
                SlaveId = 2,
            });

            var updated = this.db.GetDevicesByChannelId(chId).First();
            Assert.Equal("10.10.10.10", updated.IPAddress);
            Assert.Equal(503, updated.Port);
        }

        [Fact]
        public void DeleteDevice_ShouldRemove_Device()
        {
            var chId = this.db.AddChannel(new Channel { Name = "K", Protocol = ProtocolType.ModbusTCP });
            var devId = this.db.AddDevice(new Device { ChannelId = chId, Name = "Sil", IPAddress = "1.2.3.4", Port = 502 });

            this.db.DeleteDevice(devId);

            var devices = this.db.GetDevicesByChannelId(chId);
            Assert.Empty(devices);
        }

        // ── TAG TESTLERİ ─────────────────────────────────────────────────────

        [Fact]
        public void AddTag_ShouldReturn_ValidId()
        {
            var (_, devId) = this.CreateChannelAndDevice();

            var tagId = this.db.AddTag(new Tag
            {
                DeviceId = devId,
                Name = "Sicaklik",
                Address = 100,
                RegisterType = "HoldingRegister",
                DataType = TagDataType.Float,
            });

            Assert.True(tagId > 0);
        }

        [Fact]
        public void GetTagsByDeviceId_ShouldReturn_CorrectTags()
        {
            var (_, devId) = this.CreateChannelAndDevice();
            this.db.AddTag(new Tag { DeviceId = devId, Name = "T1", Address = 10, RegisterType = "Coil", DataType = TagDataType.Bool });
            this.db.AddTag(new Tag { DeviceId = devId, Name = "T2", Address = 20, RegisterType = "HoldingRegister", DataType = TagDataType.Int16 });

            var tags = this.db.GetTagsByDeviceId(devId);

            Assert.Equal(2, tags.Count);
            Assert.Contains(tags, t => t.Name == "T1");
            Assert.Contains(tags, t => t.Name == "T2");
        }

        [Fact]
        public void UpdateTag_ShouldChange_TagName()
        {
            var (_, devId) = this.CreateChannelAndDevice();
            var tagId = this.db.AddTag(new Tag { DeviceId = devId, Name = "Eski", Address = 1, RegisterType = "Coil", DataType = TagDataType.Bool });

            this.db.UpdateTag(new Tag
            {
                TagId = tagId,
                DeviceId = devId,
                Name = "Yeni",
                Address = 2,
                RegisterType = "Coil",
                DataType = TagDataType.Bool,
            });

            var updated = this.db.GetTagsByDeviceId(devId).First();
            Assert.Equal("Yeni", updated.Name);
            Assert.Equal(2, updated.Address);
        }

        [Fact]
        public void DeleteTag_ShouldRemove_Tag()
        {
            var (_, devId) = this.CreateChannelAndDevice();
            var tagId = this.db.AddTag(new Tag { DeviceId = devId, Name = "Sil", Address = 1, RegisterType = "Coil", DataType = TagDataType.Bool });

            this.db.DeleteTag(tagId);

            var tags = this.db.GetTagsByDeviceId(devId);
            Assert.Empty(tags);
        }

        // ── GEÇMİŞ TESTLERİ ─────────────────────────────────────────────────

        [Fact]
        public void GetTagHistory_WithDateRange_ShouldReturn_OnlyMatchingRecords()
        {
            var (_, devId) = this.CreateChannelAndDevice();
            var tagId = this.db.AddTag(new Tag
            {
                DeviceId = devId,
                Name = "Tarih",
                Address = 1,
                RegisterType = "HoldingRegister",
                DataType = TagDataType.Float,
            });
            var tag = this.db.GetTagsByDeviceId(devId).First();
            tag.TagId = tagId;

            // 3 değer yaz
            this.db.UpdateTagValue(tag, 10.0);
            this.db.UpdateTagValue(tag, 20.0);
            this.db.UpdateTagValue(tag, 30.0);

            var start = DateTime.Now.AddMinutes(-5);
            var end = DateTime.Now.AddMinutes(5);

            var history = this.db.GetTagHistory(tagId, start, end);

            Assert.Equal(3, history.Count);
        }

        [Fact]
        public void GetTagHistory_FutureRange_ShouldReturn_EmptyList()
        {
            var (_, devId) = this.CreateChannelAndDevice();
            var tagId = this.db.AddTag(new Tag
            {
                DeviceId = devId,
                Name = "Tag",
                Address = 1,
                RegisterType = "HoldingRegister",
                DataType = TagDataType.Float,
            });
            var tag = this.db.GetTagsByDeviceId(devId).First();
            tag.TagId = tagId;
            this.db.UpdateTagValue(tag, 5.0);

            // Gelecekte bir tarih aralığı — sonuç boş olmalı
            var start = DateTime.Now.AddHours(1);
            var end = DateTime.Now.AddHours(2);

            var history = this.db.GetTagHistory(tagId, start, end);

            Assert.Empty(history);
        }

        [Fact]
        public void GetTagHistory_LastHundred_ShouldBe_OrderedOldestFirst()
        {
            var (_, devId) = this.CreateChannelAndDevice();
            var tagId = this.db.AddTag(new Tag
            {
                DeviceId = devId,
                Name = "SiraTag",
                Address = 1,
                RegisterType = "HoldingRegister",
                DataType = TagDataType.Double,
            });
            var tag = this.db.GetTagsByDeviceId(devId).First();
            tag.TagId = tagId;

            this.db.UpdateTagValue(tag, 1.0);
            this.db.UpdateTagValue(tag, 2.0);
            this.db.UpdateTagValue(tag, 3.0);

            var history = this.db.GetTagHistory(tagId);

            // GetTagHistory sıralamayı eski→yeni yapar (Reverse çağrısı var)
            Assert.Equal(3, history.Count);
            Assert.True(history[0].Timestamp <= history[2].Timestamp);
        }

        // ── DISPOSE TESTİ ────────────────────────────────────────────────────

        [Fact]
        public void Dispose_ShouldNot_Throw()
        {
            var path = $"dispose_test_{Guid.NewGuid()}.db";
            var service = new DatabaseService(path);

            var ex = Record.Exception(() => service.Dispose());

            Assert.Null(ex);

            // Temizlik
            SqliteConnection.ClearAllPools();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        // ── IDisposable ──────────────────────────────────────────────────────

        public void Dispose()
        {
            if (File.Exists(this.testDbPath))
            {
                try
                {
                    SqliteConnection.ClearAllPools();
                    File.Delete(this.testDbPath);
                }
                catch
                {
                    // Temizlik hatası testi patlatmasın
                }
            }

            GC.SuppressFinalize(this);
        }

        // ── YARDIMCI METOTLAR ────────────────────────────────────────────────

        private (int ChannelId, int DeviceId) CreateChannelAndDevice()
        {
            var chId = this.db.AddChannel(new Channel { Name = "TestKanal", Protocol = ProtocolType.ModbusTCP });
            var devId = this.db.AddDevice(new Device
            {
                ChannelId = chId,
                Name = "TestCihaz",
                IPAddress = "127.0.0.1",
                Port = 502,
            });
            return (chId, devId);
        }
    }
}
