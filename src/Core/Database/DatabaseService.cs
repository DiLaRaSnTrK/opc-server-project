using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Core.Models;

namespace Core.Database
{
    public class DatabaseService : IDisposable
    {
        private readonly string _dbPath;

        public DatabaseService(string dbPath = "system.db")
        {
            _dbPath = dbPath;
            Initialize();
        }

        private void Initialize()
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
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
            );
            ";
            cmd.ExecuteNonQuery();
        }

        #region Channels
        public int AddChannel(Channel channel)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Channels (Name, Protocol, Description) VALUES (@n, @p, @d); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@n", channel.Name);
            cmd.Parameters.AddWithValue("@p", (int)channel.Protocol);
            cmd.Parameters.AddWithValue("@d", channel.Description ?? "");
            var id = (long)cmd.ExecuteScalar();
            return (int)id;
        }

        public List<Channel> GetChannels()
        {
            var list = new List<Channel>();
            using var con = new SqliteConnection($"Data Source={_dbPath}");
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
                    Description = r.IsDBNull(3) ? "" : r.GetString(3)
                });
            }
            return list;
        }
        #endregion

        #region Devices
        public int AddDevice(Device device)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO Devices (ChannelId, Name, IPAddress, Port, SlaveId, Description) 
            VALUES (@ch, @n, @ip, @port, @slave, @desc); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@ch", device.ChannelId);
            cmd.Parameters.AddWithValue("@n", device.Name);
            cmd.Parameters.AddWithValue("@ip", device.IPAddress ?? "");
            cmd.Parameters.AddWithValue("@port", device.Port);
            cmd.Parameters.AddWithValue("@slave", device.SlaveId);
            cmd.Parameters.AddWithValue("@desc", device.Description ?? "");
            var id = (long)cmd.ExecuteScalar();
            return (int)id;
        }

        public List<Device> GetDevicesByChannelId(int channelId)
        {
            var list = new List<Device>();
            using var con = new SqliteConnection($"Data Source={_dbPath}");
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
                    IPAddress = r.IsDBNull(3) ? "" : r.GetString(3),
                    Port = r.IsDBNull(4) ? 0 : r.GetInt32(4),
                    SlaveId = r.IsDBNull(5) ? (byte)0 : (byte)r.GetInt32(5),
                    Description = r.IsDBNull(6) ? "" : r.GetString(6)
                });
            }
            return list;
        }
        #endregion

        #region Tags
        public int AddTag(Tag tag)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO Tags (DeviceId, Name, Address, RegisterType, DataType, Description, LastValue, LastUpdated)
            VALUES (@dev, @n, @addr, @reg, @dt, @desc, @lv, @lu); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@dev", tag.DeviceId);
            cmd.Parameters.AddWithValue("@n", tag.Name);
            cmd.Parameters.AddWithValue("@addr", tag.Address);
            cmd.Parameters.AddWithValue("@reg", tag.RegisterType ?? "");
            cmd.Parameters.AddWithValue("@dt", (int)tag.DataType);
            cmd.Parameters.AddWithValue("@desc", tag.Description ?? "");
            cmd.Parameters.AddWithValue("@lv", ConvertTagValue(tag));
            cmd.Parameters.AddWithValue("@lu", tag.LastUpdated?.ToString("o") ?? (object)DBNull.Value);
            var id = (long)cmd.ExecuteScalar();
            return (int)id;
        }

        public List<Tag> GetTagsByDeviceId(int deviceId)
        {
            var list = new List<Tag>();
            using var con = new SqliteConnection($"Data Source={_dbPath}");
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
                    RegisterType = r.IsDBNull(4) ? "" : r.GetString(4),
                    DataType = r.IsDBNull(5) ? TagDataType.Int16 : (TagDataType)r.GetInt32(5),
                    Description = r.IsDBNull(6) ? "" : r.GetString(6),
                    Value = r.IsDBNull(7) ? double.NaN : r.GetDouble(7),
                    LastUpdated = r.IsDBNull(8) ? (DateTime?)null : DateTime.Parse(r.GetString(8))
                };
                list.Add(t);
            }
            return list;
        }

        public void UpdateTagValue(Tag tag, double rawValue)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();

            object dbValue = rawValue;

            switch (tag.DataType)
            {
                case TagDataType.Bool:
                    dbValue = rawValue == 1 ? 1 : 0;
                    break;

                case TagDataType.Int16:
                    dbValue = (short)rawValue;
                    break;

                case TagDataType.UInt16:
                    dbValue = (ushort)rawValue;
                    break;

                case TagDataType.Int32:
                    dbValue = (int)rawValue;
                    break;

                case TagDataType.UInt32:
                    dbValue = (uint)rawValue;
                    break;

                case TagDataType.Float:
                    dbValue = (float)rawValue;
                    break;

                case TagDataType.Double:
                    dbValue = rawValue;
                    break;
            }

            using var cmd = con.CreateCommand();
            cmd.CommandText =
                "UPDATE Tags SET LastValue = @v, LastUpdated = @lu WHERE TagId = @id";

            cmd.Parameters.AddWithValue("@v", dbValue);
            cmd.Parameters.AddWithValue("@lu", DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("@id", tag.TagId);

            cmd.ExecuteNonQuery();
        }

        #endregion
        public void InsertHistory(int tagId, double value, DateTime ts)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText =
                "INSERT INTO TagHistory (TagId, Value, Timestamp) VALUES (@id, @v, @ts)";

            cmd.Parameters.AddWithValue("@id", tagId);
            cmd.Parameters.AddWithValue("@v", value);
            cmd.Parameters.AddWithValue("@ts", ts.ToString("o"));

            cmd.ExecuteNonQuery();
        }

        // Tarih aralığına göre veri getiren yeni metot
        public List<(DateTime Timestamp, double Value)> GetTagHistory(int tagId, DateTime startDate, DateTime endDate)
        {
            var list = new List<(DateTime, double)>();
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();

            // SQLite'ta tarihler string (ISO8601) tutulduğu için string karşılaştırması yapıyoruz.
            // Başlangıç gününün sabahı (00:00) ile bitiş gününün gecesi (23:59) arasını alıyoruz.
            cmd.CommandText = @"
        SELECT Timestamp, Value 
        FROM TagHistory 
        WHERE TagId=@id 
          AND Timestamp >= @start 
          AND Timestamp <= @end 
        ORDER BY Timestamp ASC"; // Grafikte soldan sağa akış için ASC önemli

            cmd.Parameters.AddWithValue("@id", tagId);
            cmd.Parameters.AddWithValue("@start", startDate.ToString("o")); // ISO format
            cmd.Parameters.AddWithValue("@end", endDate.ToString("o"));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParse(reader.GetString(0), out DateTime dt))
                {
                    list.Add((dt, reader.GetDouble(1)));
                }
            }
            return list;
        }


        public void Dispose()
        {
            // nothing to dispose currently
        }

        private object ConvertTagValue(Tag tag)
        {
            if (tag.Value == null)
                return DBNull.Value;

            switch (tag.DataType)
            {
                case TagDataType.Float:
                    if (double.TryParse(tag.Value.ToString(), out double f))
                        return f;
                    return DBNull.Value;

                case TagDataType.Int16:
                case TagDataType.UInt16:
                    if (int.TryParse(tag.Value.ToString(), out int i))
                        return i;
                    return DBNull.Value;

                case TagDataType.Bool:
                    if (bool.TryParse(tag.Value.ToString(), out bool b))
                        return b ? 1 : 0;
                    return DBNull.Value;

                default:
                    return DBNull.Value;
            }
        }

        public void DeleteChannel(int channelId)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Channels WHERE ChannelId=@id";
            cmd.Parameters.AddWithValue("@id", channelId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteDevice(int deviceId)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Devices WHERE DeviceId=@id";
            cmd.Parameters.AddWithValue("@id", deviceId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteTag(int tagId)
        {
            using var con = new SqliteConnection($"Data Source={_dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Tags WHERE TagId=@id";
            cmd.Parameters.AddWithValue("@id", tagId);
            cmd.ExecuteNonQuery();
        }


    }

}
