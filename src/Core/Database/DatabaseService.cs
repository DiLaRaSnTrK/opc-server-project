// Copyright (c) PlaceholderCompany. All rights reserved.

namespace Core.Database
{
    using System;
    using System.Collections.Generic;
    using Core.Helpers;
    using Core.Interfaces;
    using Core.Models;
    using Microsoft.Data.Sqlite;

    /// <summary>SQLite veritabanı işlemlerini yöneten servis.</summary>
    public class DatabaseService : IDisposable
    {
        private readonly string dbPath;
        private readonly ITagUpdater tagUpdater;
        private bool disposed;

        /// <summary>Initializes a new instance of the <see cref="DatabaseService"/> class.</summary>
        public DatabaseService(string dbPath = "system.db", ITagUpdater tagUpdater = null)
        {
            this.dbPath = dbPath;
            this.tagUpdater = tagUpdater;
            this.Initialize();
        }

        // ── CHANNELS ─────────────────────────────────────────────────────────

        /// <summary>Yeni kanal ekler.</summary>
        public int AddChannel(Channel channel)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Channels (Name, Protocol, Description) VALUES (@n, @p, @d); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@n", channel.Name);
            cmd.Parameters.AddWithValue("@p", (int)channel.Protocol);
            cmd.Parameters.AddWithValue("@d", channel.Description ?? string.Empty);
            return (int)(long)cmd.ExecuteScalar();
        }

        /// <summary>Tüm kanalları döndürür.</summary>
        public List<Channel> GetChannels()
        {
            var list = new List<Channel>();
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT ChannelId, Name, Protocol, Description FROM Channels";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Channel
                {
                    ChannelId = r.GetInt32(0),
                    Name = r.GetString(1),
                    Protocol = (ProtocolType)r.GetInt32(2),
                    Description = r.IsDBNull(3) ? string.Empty : r.GetString(3),
                });
            }

            return list;
        }

        /// <summary>Kanalı günceller.</summary>
        public void UpdateChannel(Channel ch)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "UPDATE Channels SET Name=@n, Protocol=@p, Description=@d WHERE ChannelId=@id";
            cmd.Parameters.AddWithValue("@n", ch.Name);
            cmd.Parameters.AddWithValue("@p", (int)ch.Protocol);
            cmd.Parameters.AddWithValue("@d", ch.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@id", ch.ChannelId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Kanalı siler.</summary>
        public void DeleteChannel(int channelId)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Channels WHERE ChannelId=@id";
            cmd.Parameters.AddWithValue("@id", channelId);
            cmd.ExecuteNonQuery();
        }

        // ── DEVICES ──────────────────────────────────────────────────────────

        /// <summary>Yeni cihaz ekler.</summary>
        public int AddDevice(Device device)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO Devices (ChannelId, Name, IPAddress, Port, SlaveId, Description)
                VALUES (@ch, @n, @ip, @port, @slave, @desc); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@ch", device.ChannelId);
            cmd.Parameters.AddWithValue("@n", device.Name);
            cmd.Parameters.AddWithValue("@ip", device.IPAddress ?? string.Empty);
            cmd.Parameters.AddWithValue("@port", device.Port);
            cmd.Parameters.AddWithValue("@slave", device.SlaveId);
            cmd.Parameters.AddWithValue("@desc", device.Description ?? string.Empty);
            return (int)(long)cmd.ExecuteScalar();
        }

        /// <summary>Kanala ait cihazları döndürür.</summary>
        public List<Device> GetDevicesByChannelId(int channelId)
        {
            var list = new List<Device>();
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT DeviceId, ChannelId, Name, IPAddress, Port, SlaveId, Description FROM Devices WHERE ChannelId = @ch";
            cmd.Parameters.AddWithValue("@ch", channelId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Device
                {
                    DeviceId = r.GetInt32(0),
                    ChannelId = r.GetInt32(1),
                    Name = r.GetString(2),
                    IPAddress = r.IsDBNull(3) ? string.Empty : r.GetString(3),
                    Port = r.IsDBNull(4) ? 0 : r.GetInt32(4),
                    SlaveId = r.IsDBNull(5) ? (byte)0 : (byte)r.GetInt32(5),
                    Description = r.IsDBNull(6) ? string.Empty : r.GetString(6),
                });
            }

            return list;
        }

        /// <summary>Cihazı günceller.</summary>
        public void UpdateDevice(Device dev)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"UPDATE Devices SET Name=@n, IPAddress=@ip, Port=@port, SlaveId=@slave, Description=@desc
                WHERE DeviceId=@id";
            cmd.Parameters.AddWithValue("@n", dev.Name);
            cmd.Parameters.AddWithValue("@ip", dev.IPAddress ?? string.Empty);
            cmd.Parameters.AddWithValue("@port", dev.Port);
            cmd.Parameters.AddWithValue("@slave", dev.SlaveId);
            cmd.Parameters.AddWithValue("@desc", dev.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@id", dev.DeviceId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Cihazı siler.</summary>
        public void DeleteDevice(int deviceId)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Devices WHERE DeviceId=@id";
            cmd.Parameters.AddWithValue("@id", deviceId);
            cmd.ExecuteNonQuery();
        }

        // ── TAGS ─────────────────────────────────────────────────────────────

        /// <summary>Yeni tag ekler.</summary>
        public int AddTag(Tag tag)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO Tags (DeviceId, Name, Address, RegisterType, DataType, Description, LastValue, LastUpdated)
                VALUES (@dev, @n, @addr, @reg, @dt, @desc, @lv, @lu); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@dev", tag.DeviceId);
            cmd.Parameters.AddWithValue("@n", tag.Name);
            cmd.Parameters.AddWithValue("@addr", tag.Address);
            cmd.Parameters.AddWithValue("@reg", tag.RegisterType ?? string.Empty);
            cmd.Parameters.AddWithValue("@dt", (int)tag.DataType);
            cmd.Parameters.AddWithValue("@desc", tag.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@lv", ConvertTagValue(tag));
            cmd.Parameters.AddWithValue("@lu", tag.LastUpdated?.ToString("o") ?? (object)DBNull.Value);
            return (int)(long)cmd.ExecuteScalar();
        }

        /// <summary>Cihaza ait tag'leri döndürür.</summary>
        public List<Tag> GetTagsByDeviceId(int deviceId)
        {
            var list = new List<Tag>();
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT TagId, DeviceId, Name, Address, RegisterType, DataType, Description, LastValue, LastUpdated FROM Tags WHERE DeviceId = @dev";
            cmd.Parameters.AddWithValue("@dev", deviceId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var t = new Tag
                {
                    TagId = r.GetInt32(0),
                    DeviceId = r.GetInt32(1),
                    Name = r.GetString(2),
                    Address = r.IsDBNull(3) ? 0 : r.GetInt32(3),
                    RegisterType = r.IsDBNull(4) ? string.Empty : r.GetString(4),
                    DataType = r.IsDBNull(5) ? TagDataType.Int16 : (TagDataType)r.GetInt32(5),
                    Description = r.IsDBNull(6) ? string.Empty : r.GetString(6),
                    Value = r.IsDBNull(7) ? double.NaN : r.GetDouble(7),
                    LastUpdated = r.IsDBNull(8) ? (DateTime?)null : DateTime.Parse(r.GetString(8), System.Globalization.CultureInfo.InvariantCulture),
                };
                list.Add(t);
            }

            return list;
        }

        /// <summary>Tag'in son değerini ve geçmişini günceller.</summary>
        public void UpdateTagValue(Tag tag, double rawValue)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            object dbValue = TagValueParser.Convert(tag.DataType, rawValue);
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                UPDATE Tags SET LastValue = @v, LastUpdated = @lu WHERE TagId = @id;
                INSERT INTO TagHistory (TagId, Value, Timestamp) VALUES (@id, @v, @lu);";
            cmd.Parameters.AddWithValue("@v", dbValue);
            cmd.Parameters.AddWithValue("@lu", DateTime.Now.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("@id", tag.TagId);
            cmd.ExecuteNonQuery();
            this.tagUpdater?.UpdateTag(tag.Name, dbValue);
        }

        /// <summary>Tag'i günceller.</summary>
        public void UpdateTag(Tag tag)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"UPDATE Tags SET Name=@n, Address=@addr, RegisterType=@reg, DataType=@dt, Description=@desc
                WHERE TagId=@id";
            cmd.Parameters.AddWithValue("@n", tag.Name);
            cmd.Parameters.AddWithValue("@addr", tag.Address);
            cmd.Parameters.AddWithValue("@reg", tag.RegisterType ?? string.Empty);
            cmd.Parameters.AddWithValue("@dt", (int)tag.DataType);
            cmd.Parameters.AddWithValue("@desc", tag.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@id", tag.TagId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Tag'i siler.</summary>
        public void DeleteTag(int tagId)
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Tags WHERE TagId=@id";
            cmd.Parameters.AddWithValue("@id", tagId);
            cmd.ExecuteNonQuery();
        }

        // ── HISTORY ──────────────────────────────────────────────────────────

        /// <summary>Son 100 geçmiş kaydını döndürür.</summary>
        public List<(DateTime Timestamp, double Value)> GetTagHistory(int tagId)
        {
            var list = new List<(DateTime, double)>();
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Timestamp, Value FROM TagHistory WHERE TagId=@id ORDER BY Id DESC LIMIT 100";
            cmd.Parameters.AddWithValue("@id", tagId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParse(reader.GetString(0), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dt))
                {
                    list.Add((dt, reader.GetDouble(1)));
                }
            }

            list.Reverse();
            return list;
        }

        /// <summary>Tarih aralığına göre geçmiş döndürür.</summary>
        public List<(DateTime Timestamp, double Value)> GetTagHistory(int tagId, DateTime startDate, DateTime endDate)
        {
            var list = new List<(DateTime, double)>();
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT Timestamp, Value FROM TagHistory
                WHERE TagId=@id AND Timestamp >= @start AND Timestamp <= @end
                ORDER BY Timestamp ASC";
            cmd.Parameters.AddWithValue("@id", tagId);
            cmd.Parameters.AddWithValue("@start", startDate.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("@end", endDate.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParse(reader.GetString(0), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dt))
                {
                    list.Add((dt, reader.GetDouble(1)));
                }
            }

            return list;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Kaynakları serbest bırakır.</summary>
        protected virtual void Dispose(bool disposing)
        {
            this.disposed = true;
        }

        private void Initialize()
        {
            using var con = new SqliteConnection($"Data Source={this.dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Channels (
                    ChannelId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Protocol INTEGER NOT NULL,
                    Description TEXT
                );
                CREATE TABLE IF NOT EXISTS Devices (
                    DeviceId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ChannelId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    IPAddress TEXT,
                    Port INTEGER,
                    SlaveId INTEGER,
                    Description TEXT,
                    FOREIGN KEY(ChannelId) REFERENCES Channels(ChannelId) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS Tags (
                    TagId INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Address INTEGER,
                    RegisterType TEXT,
                    DataType INTEGER,
                    Description TEXT,
                    LastValue REAL,
                    LastUpdated TEXT,
                    FOREIGN KEY(DeviceId) REFERENCES Devices(DeviceId) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS TagHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TagId INTEGER,
                    Value REAL,
                    Timestamp TEXT
                );";
            cmd.ExecuteNonQuery();
        }

        private static object ConvertTagValue(Tag tag)
        {
            if (tag.Value == null)
            {
                return DBNull.Value;
            }

            switch (tag.DataType)
            {
                case TagDataType.Float:
                    if (double.TryParse(tag.Value.ToString(), out double f))
                    {
                        return f;
                    }

                    return DBNull.Value;

                case TagDataType.Int16:
                case TagDataType.UInt16:
                    if (int.TryParse(tag.Value.ToString(), out int i))
                    {
                        return i;
                    }

                    return DBNull.Value;

                case TagDataType.Bool:
                    if (bool.TryParse(tag.Value.ToString(), out bool b))
                    {
                        return b ? 1 : 0;
                    }

                    return DBNull.Value;

                default:
                    return DBNull.Value;
            }
        }
    }
}
