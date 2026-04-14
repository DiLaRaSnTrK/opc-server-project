// Copyright (c) OPC Server Project. All rights reserved.

// Aşama 8 — ModbusClientWrapper Coverage Testleri
// Ağ bağlantısı gerektirmeyen tüm yolları kapsar:
//   - IP validation (format, whitelist)
//   - Input validation (null tag, geçersiz adres)
//   - GetRegisterCount (tüm tipler)
//   - ConvertRegisters (tüm tipler)
//   - Dispose pattern
//   - ReadResult varsayılan değerleri
//   - DataReceivedEventArgs

namespace Tests
{
    using System;
    using System.Reflection;
    using Core.Interfaces;
    using Core.Models;
    using Core.Protocols;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    /// <summary>
    /// ModbusClientWrapper'ın ağ bağlantısı gerektirmeyen tüm
    /// kod yollarını kapsayan testler.
    /// </summary>
    public class ModbusClientCoverageTests
    {
        // ── CONSTRUCTOR & PROPERTY TESTLERİ ──────────────────────────────────

        [Fact]
        public void Constructor_ValidDevice_ShouldNot_Throw()
        {
            var device = MakeDevice("192.168.1.1");
            var ex = Record.Exception(() =>
                new ModbusClientWrapper(device, NullLogger<ModbusClientWrapper>.Instance));
            Assert.Null(ex);
        }

        [Fact]
        public void Constructor_NullDevice_ShouldThrow_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ModbusClientWrapper(null!, NullLogger<ModbusClientWrapper>.Instance));
        }

        [Fact]
        public void IsConnected_WhenNotConnected_ShouldReturn_False()
        {
            var client = new ModbusClientWrapper(MakeDevice("127.0.0.1"), NullLogger<ModbusClientWrapper>.Instance);
            Assert.False(client.IsConnected);
        }

        // ── IP VALİDASYON TESTLERİ ───────────────────────────────────────────

        [Theory]
        [InlineData("gecersiz")]
        [InlineData("999.999.999.999")]
        [InlineData("abc.def.ghi.jkl")]
        [InlineData("")]
        public async void ConnectAsync_InvalidIpFormat_ShouldThrow_ArgumentException(string ip)
        {
            var client = new ModbusClientWrapper(MakeDevice(ip), NullLogger<ModbusClientWrapper>.Instance);
            await Assert.ThrowsAsync<ArgumentException>(() => client.ConnectAsync());
        }

        [Fact]
        public async void ConnectAsync_WhitelistActive_UnknownIp_ShouldThrow_UnauthorizedAccess()
        {
            SetWhitelist(new[] { "10.0.0.1" });
            try
            {
                var client = new ModbusClientWrapper(MakeDevice("192.168.99.99"), NullLogger<ModbusClientWrapper>.Instance);
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => client.ConnectAsync());
            }
            finally
            {
                SetWhitelist(Array.Empty<string>());
            }
        }

        [Fact]
        public async void ConnectAsync_WhitelistEmpty_ValidIp_ShouldPass_WhitelistCheck()
        {
            SetWhitelist(Array.Empty<string>());

            // Geçerli IP — bağlantı reddini whitelist'e değil TCP hatasına bırakır
            var client = new ModbusClientWrapper(MakeDevice("127.0.0.1"), NullLogger<ModbusClientWrapper>.Instance);

            // WhitelistException değil, bağlantı hatası gelmelidir
            var ex = await Record.ExceptionAsync(() => client.ConnectAsync());
            Assert.IsNotType<UnauthorizedAccessException>(ex);
        }

        // ── READTAG VALİDASYON TESTLERİ ──────────────────────────────────────

        [Fact]
        public async void ReadTagAsync_NullTag_ShouldReturn_FailResult()
        {
            var client = new ModbusClientWrapper(MakeDevice("127.0.0.1"), NullLogger<ModbusClientWrapper>.Instance);
            var result = await client.ReadTagAsync(null!);
            Assert.False(result.Success);
            Assert.NotEmpty(result.ErrorMessage);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(70000)]
        [InlineData(65536)]
        public async void ReadTagAsync_InvalidAddress_ShouldReturn_FailResult(int address)
        {
            var client = new ModbusClientWrapper(MakeDevice("127.0.0.1"), NullLogger<ModbusClientWrapper>.Instance);
            var tag = MakeTag(address);
            var result = await client.ReadTagAsync(tag);
            Assert.False(result.Success);
            Assert.Contains("Geçersiz", result.ErrorMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(65535)]
        public async void ReadTagAsync_ValidAddress_ShouldNot_Return_InvalidAddressError(int address)
        {
            var client = new ModbusClientWrapper(MakeDevice("127.0.0.1"), NullLogger<ModbusClientWrapper>.Instance);
            var tag = MakeTag(address);

            // Geçerli adres — bağlantı hatası alır ama "Geçersiz adres" almaz
            var result = await client.ReadTagAsync(tag);
            Assert.DoesNotContain("Geçersiz", result.ErrorMessage ?? string.Empty);
        }

        // ── GET REGISTER COUNT TESTLERİ ───────────────────────────────────────

        [Theory]
        [InlineData(TagDataType.Bool, 1)]
        [InlineData(TagDataType.Int16, 1)]
        [InlineData(TagDataType.UInt16, 1)]
        [InlineData(TagDataType.Float, 2)]
        [InlineData(TagDataType.Int32, 2)]
        [InlineData(TagDataType.UInt32, 2)]
        [InlineData(TagDataType.Double, 4)]
        public void GetRegisterCount_ShouldReturn_CorrectCount(TagDataType dataType, int expected)
        {
            var method = typeof(ModbusClientWrapper).GetMethod(
                "GetRegisterCount",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (int)method!.Invoke(null, new object[] { dataType })!;
            Assert.Equal(expected, result);
        }

        // ── CONVERT REGISTERS TESTLERİ ────────────────────────────────────────

        [Fact]
        public void ConvertRegisters_NullArray_ShouldReturn_Zero()
        {
            var method = typeof(ModbusClientWrapper).GetMethod(
                "ConvertRegisters",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (double)method!.Invoke(null, new object[] { null!, TagDataType.Int16 })!;
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void ConvertRegisters_EmptyArray_ShouldReturn_Zero()
        {
            var method = typeof(ModbusClientWrapper).GetMethod(
                "ConvertRegisters",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (double)method!.Invoke(null, new object[] { Array.Empty<int>(), TagDataType.Int16 })!;
            Assert.Equal(0.0, result);
        }

        [Theory]
        [InlineData(new[] { 1 }, TagDataType.Bool, 1.0)]
        [InlineData(new[] { 0 }, TagDataType.Bool, 0.0)]
        [InlineData(new[] { 100 }, TagDataType.Int16, 100.0)]
        [InlineData(new[] { 200 }, TagDataType.UInt16, 200.0)]
        public void ConvertRegisters_SingleRegister_ShouldReturn_CorrectValue(
            int[] registers, TagDataType type, double expected)
        {
            var method = typeof(ModbusClientWrapper).GetMethod(
                "ConvertRegisters",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (double)method!.Invoke(null, new object[] { registers, type })!;
            Assert.Equal(expected, result, precision: 3);
        }

        [Fact]
        public void CombineToInt32_ShouldReturn_CorrectValue()
        {
            var method = typeof(ModbusClientWrapper).GetMethod(
                "CombineToInt32",
                BindingFlags.NonPublic | BindingFlags.Static);

            // (1 << 16) | 0 = 65536
            var result = (int)method!.Invoke(null, new object[] { 1, 0 })!;
            Assert.Equal(65536, result);
        }

        [Fact]
        public void CombineToFloat_ShouldReturn_FloatValue()
        {
            var method = typeof(ModbusClientWrapper).GetMethod(
                "CombineToFloat",
                BindingFlags.NonPublic | BindingFlags.Static);

            // 0, 0 → 0.0f
            var result = (float)method!.Invoke(null, new object[] { 0, 0 })!;
            Assert.Equal(0.0f, result);
        }

        // ── DISPOSE TESTLERİ ─────────────────────────────────────────────────

        [Fact]
        public void Dispose_WhenNotConnected_ShouldNot_Throw()
        {
            var client = new ModbusClientWrapper(MakeDevice("127.0.0.1"), NullLogger<ModbusClientWrapper>.Instance);
            var ex = Record.Exception(() => client.Dispose());
            Assert.Null(ex);
        }

        [Fact]
        public void Dispose_CalledTwice_ShouldNot_Throw()
        {
            var client = new ModbusClientWrapper(MakeDevice("127.0.0.1"), NullLogger<ModbusClientWrapper>.Instance);
            client.Dispose();
            var ex = Record.Exception(() => client.Dispose());
            Assert.Null(ex);
        }

        // ── READRESULT & EVENT TESTLERİ ───────────────────────────────────────

        [Fact]
        public void ReadResult_DefaultValues_AllNotNull()
        {
            var r = new ReadResult();
            Assert.False(r.Success);
            Assert.NotNull(r.Values);
            Assert.NotNull(r.ErrorMessage);
            Assert.Empty(r.Values);
            Assert.Empty(r.ErrorMessage);
        }

        [Fact]
        public void ReadResult_SetValues_ShouldStore_Correctly()
        {
            var r = new ReadResult
            {
                Success = true,
                Values = new[] { 42.0, 43.0 },
                ErrorMessage = string.Empty,
            };
            Assert.True(r.Success);
            Assert.Equal(2, r.Values.Length);
            Assert.Equal(42.0, r.Values[0]);
        }

        [Fact]
        public void DataReceivedEventArgs_Properties_ShouldStore_Correctly()
        {
            var now = DateTime.Now;
            var args = new DataReceivedEventArgs
            {
                TagId = 5,
                Value = 99.9,
                Timestamp = now,
            };
            Assert.Equal(5, args.TagId);
            Assert.Equal(99.9, args.Value);
            Assert.Equal(now, args.Timestamp);
        }

        // ── DISCONNECTASYNC TESTİ ─────────────────────────────────────────────

        [Fact]
        public async void DisconnectAsync_WhenNotConnected_ShouldNot_Throw()
        {
            var client = new ModbusClientWrapper(MakeDevice("127.0.0.1"), NullLogger<ModbusClientWrapper>.Instance);
            var ex = await Record.ExceptionAsync(() => client.DisconnectAsync());
            Assert.Null(ex);
        }

        // ── YARDIMCI METOTLAR ─────────────────────────────────────────────────

        private static Device MakeDevice(string ip) => new Device
        {
            DeviceId = 1,
            Name = "TestDevice",
            IPAddress = ip,
            Port = 502,
            SlaveId = 1,
        };

        private static Tag MakeTag(int address) => new Tag
        {
            TagId = 1,
            Name = "TestTag",
            Address = address,
            RegisterType = "HoldingRegister",
            DataType = TagDataType.Int16,
        };

        private static void SetWhitelist(string[] ips)
        {
            var field = typeof(ModbusClientWrapper).GetField(
                "AllowedIpAddresses",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field?.GetValue(null) is System.Collections.Generic.HashSet<string> set)
            {
                set.Clear();
                foreach (var ip in ips)
                {
                    set.Add(ip);
                }
            }
        }
    }
}
