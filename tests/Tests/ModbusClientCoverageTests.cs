// Copyright (c) OPC Server Project. All rights reserved.

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using Core.Protocols;
    using Xunit;

    public class ModbusClientCoverageTests
    {
        // ── CONSTRUCTOR ───────────────────────────────────────────────────────

        [Fact]
        public void Constructor_ValidDevice_DoesNotThrow()
        {
            var ex = Record.Exception(() => new ModbusClientWrapper(MakeDevice()));
            Assert.Null(ex);
        }

        [Fact]
        public void Constructor_NullDevice_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ModbusClientWrapper(null!));
        }

        [Fact]
        public void IsConnected_NotConnected_ReturnsFalse()
        {
            Assert.False(new ModbusClientWrapper(MakeDevice()).IsConnected);
        }

        // ── ConnectAsync — WHITELIST SATIRI (L38-L44) ─────────────────────────
        // AllowedIpAddresses.Count > 0 && !Contains → UnauthorizedAccess fırlatır

        [Fact]
        public async Task ConnectAsync_WhitelistActive_UnknownIp_ThrowsUnauthorized()
        {
            SetWhitelist("10.0.0.1");
            try
            {
                var c = new ModbusClientWrapper(MakeDevice("192.168.99.99"));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => c.ConnectAsync());
            }
            finally { ClearWhitelist(); }
        }

        [Fact]
        public async Task ConnectAsync_WhitelistActive_KnownIp_PassesWhitelistCheck()
        {
            // Whitelist'te olan IP — whitelist bloğu geçilir, sonra IP parse edilir
            SetWhitelist("127.0.0.1");
            try
            {
                var c = new ModbusClientWrapper(MakeDevice("127.0.0.1"));
                // Whitelist exception değil, bağlantı hatası (TCP) gelir
                var ex = await Record.ExceptionAsync(() => c.ConnectAsync());
                Assert.IsNotType<UnauthorizedAccessException>(ex);
            }
            finally { ClearWhitelist(); }
        }

        [Fact]
        public async Task ConnectAsync_WhitelistEmpty_AnyIp_PassesCheck()
        {
            ClearWhitelist();
            var c = new ModbusClientWrapper(MakeDevice("127.0.0.1"));
            var ex = await Record.ExceptionAsync(() => c.ConnectAsync());
            Assert.IsNotType<UnauthorizedAccessException>(ex);
        }

        // ── ConnectAsync — IP FORMAT SATIRI (L47-L52) ─────────────────────────
        // IPAddress.TryParse false → ArgumentException fırlatır

        [Theory]
        [InlineData("gecersiz")]
        [InlineData("999.999.999.999")]
        [InlineData("abc.def")]
        [InlineData("")]
        public async Task ConnectAsync_InvalidIpFormat_ThrowsArgumentException(string ip)
        {
            var c = new ModbusClientWrapper(MakeDevice(ip));
            await Assert.ThrowsAsync<ArgumentException>(() => c.ConnectAsync());
        }

        [Fact]
        public async Task ConnectAsync_ValidIpFormat_PassesFormatCheck()
        {
            // Format check geçilir, sonra TCP bağlantısı başarısız olur
            var c = new ModbusClientWrapper(MakeDevice("127.0.0.1"));
            var ex = await Record.ExceptionAsync(() => c.ConnectAsync());
            Assert.IsNotType<ArgumentException>(ex);
        }

        // ── DisconnectAsync — client.Connected = false yolu ───────────────────
        // Bağlı değilken çağrılır → if (client.Connected) bloğu atlanır

        [Fact]
        public async Task DisconnectAsync_WhenNotConnected_DoesNotThrow()
        {
            var c = new ModbusClientWrapper(MakeDevice());
            var ex = await Record.ExceptionAsync(() => c.DisconnectAsync());
            Assert.Null(ex);
        }

        [Fact]
        public async Task DisconnectAsync_CalledMultipleTimes_DoesNotThrow()
        {
            var c = new ModbusClientWrapper(MakeDevice());
            await c.DisconnectAsync();
            var ex = await Record.ExceptionAsync(() => c.DisconnectAsync());
            Assert.Null(ex);
        }

        // ── ReadTagAsync — VALIDATION SATIRLARI ──────────────────────────────

        [Fact]
        public async Task ReadTagAsync_NullTag_ReturnsFail()
        {
            var r = await new ModbusClientWrapper(MakeDevice()).ReadTagAsync(null!);
            Assert.False(r.Success);
            Assert.Equal("Tag null", r.ErrorMessage);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(65536)]
        [InlineData(100000)]
        public async Task ReadTagAsync_InvalidAddress_ReturnsFail(int address)
        {
            var r = await new ModbusClientWrapper(MakeDevice()).ReadTagAsync(MakeTag(address));
            Assert.False(r.Success);
            Assert.Contains("Geçersiz", r.ErrorMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(65535)]
        public async Task ReadTagAsync_ValidAddress_DoesNotReturn_InvalidAddressError(int address)
        {
            var r = await new ModbusClientWrapper(MakeDevice()).ReadTagAsync(MakeTag(address));
            // Geçerli adres → "Geçersiz" hatası değil, bağlantı hatası gelir
            Assert.DoesNotContain("Geçersiz", r.ErrorMessage ?? string.Empty);
        }

        // ── ReadTagAsync — RETRY SONRASI FAIL DÖNÜŞÜ ─────────────────────────
        // Geçerli adres ama bağlantı kurulamaz → 2 deneme tükenince fail döner

        [Fact]
        public async Task ReadTagAsync_CannotConnect_ReturnsFailAfterRetries()
        {
            // 127.0.0.1:9999 — hiçbir şey dinlemiyor, bağlantı hızla başarısız olur
            var c = new ModbusClientWrapper(new Device
            {
                DeviceId = 99,
                Name = "NoServer",
                IPAddress = "127.0.0.1",
                Port = 9999,
                SlaveId = 1,
            });
            var r = await c.ReadTagAsync(MakeTag(100));
            // 2 deneme → tükendi → fail
            Assert.False(r.Success);
            Assert.NotEmpty(r.ErrorMessage);
        }

        // ── ConvertRegisters — INT32, UINT32, FLOAT, DOUBLE DALLARI ──────────

        [Fact]
        public void ConvertRegisters_Int32_CombinesCorrectly()
        {
            // (1 << 16) | 5 = 65541
            var result = InvokeConvert(new[] { 1, 5 }, TagDataType.Int32);
            Assert.Equal(65541.0, result);
        }

        [Fact]
        public void ConvertRegisters_Int32_Zero()
        {
            Assert.Equal(0.0, InvokeConvert(new[] { 0, 0 }, TagDataType.Int32));
        }

        [Fact]
        public void ConvertRegisters_UInt32_PositiveValue()
        {
            // CombineToInt32(0, 5) = 5 → (uint)5 = 5
            var result = InvokeConvert(new[] { 0, 5 }, TagDataType.UInt32);
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void ConvertRegisters_Float_KnownIEEE754()
        {
            // 1.0f değerini bilgisayarın anladığı byte dizisine çevir
            byte[] bytes = BitConverter.GetBytes(1.0f);
            // Byte dizisinden 2 tane 16-bitlik register oluştur
            int reg1 = BitConverter.ToUInt16(bytes, 0);
            int reg2 = BitConverter.ToUInt16(bytes, 2);

            var result = InvokeConvert(new[] { reg1, reg2 }, TagDataType.Float);
            Assert.Equal(1.0f, (float)result, precision: 2);
        }

        [Fact]
        public void ConvertRegisters_Float_Zero()
        {
            Assert.Equal(0.0, InvokeConvert(new[] { 0, 0 }, TagDataType.Float), precision: 5);
        }

        [Fact]
        public void ConvertRegisters_Double_KnownIEEE754()
        {
            // 1.0 double değerini byte dizisine çevir
            byte[] bytes = BitConverter.GetBytes(1.0);
            int[] regs = new int[4];
            // 8 byte'ı 4 tane 16-bitlik register'a böl
            for (int i = 0; i < 4; i++)
                regs[i] = BitConverter.ToUInt16(bytes, i * 2);

            var result = InvokeConvert(regs, TagDataType.Double);
            Assert.Equal(1.0, result, precision: 2);
        }

        [Fact]
        public void ConvertRegisters_Double_Zero()
        {
            Assert.Equal(0.0, InvokeConvert(new[] { 0, 0, 0, 0 }, TagDataType.Double));
        }

        [Fact]
        public void ConvertRegisters_Default_ReturnsFirstRegister()
        {
            // Bilinmeyen tip → default → r[0]
            var result = InvokeConvertRaw(new[] { 77 }, (TagDataType)99);
            Assert.Equal(77.0, result);
        }

        // ── GetRegisterCount — TÜM TIPLER ────────────────────────────────────

        [Theory]
        [InlineData(TagDataType.Bool, 1)]
        [InlineData(TagDataType.Int16, 1)]
        [InlineData(TagDataType.UInt16, 1)]
        [InlineData(TagDataType.Float, 2)]
        [InlineData(TagDataType.Int32, 2)]
        [InlineData(TagDataType.UInt32, 2)]
        [InlineData(TagDataType.Double, 4)]
        public void GetRegisterCount_AllTypes_Correct(TagDataType dt, int expected)
        {
            var m = typeof(ModbusClientWrapper)
                .GetMethod("GetRegisterCount", BindingFlags.NonPublic | BindingFlags.Static)!;
            Assert.Equal(expected, (int)m.Invoke(null, new object[] { dt })!);
        }

        // ── ConvertRegisters — NULL / EMPTY ───────────────────────────────────

        [Fact]
        public void ConvertRegisters_NullArray_ReturnsZero()
        {
            Assert.Equal(0.0, InvokeConvertRaw(null!, TagDataType.Int16));
        }

        [Fact]
        public void ConvertRegisters_EmptyArray_ReturnsZero()
        {
            Assert.Equal(0.0, InvokeConvert(Array.Empty<int>(), TagDataType.Int16));
        }

        // ── ConvertRegisters — Bool, Int16, UInt16 ────────────────────────────

        [Fact]
        public void ConvertRegisters_Bool_1_Returns1() =>
            Assert.Equal(1.0, InvokeConvert(new[] { 1 }, TagDataType.Bool));

        [Fact]
        public void ConvertRegisters_Bool_0_Returns0() =>
            Assert.Equal(0.0, InvokeConvert(new[] { 0 }, TagDataType.Bool));

        [Fact]
        public void ConvertRegisters_Bool_Other_Returns0() =>
            Assert.Equal(0.0, InvokeConvert(new[] { 5 }, TagDataType.Bool));

        [Fact]
        public void ConvertRegisters_Int16_Positive() =>
            Assert.Equal(100.0, InvokeConvert(new[] { 100 }, TagDataType.Int16));

        [Fact]
        public void ConvertRegisters_Int16_Negative() =>
            Assert.Equal(-1.0, InvokeConvert(new[] { 65535 }, TagDataType.Int16)); // (short)65535 = -1

        [Fact]
        public void ConvertRegisters_UInt16_Value() =>
            Assert.Equal(1000.0, InvokeConvert(new[] { 1000 }, TagDataType.UInt16));

        // ── CombineTo metotları ───────────────────────────────────────────────

        [Fact]
        public void CombineToInt32_Basic()
        {
            var m = typeof(ModbusClientWrapper)
                .GetMethod("CombineToInt32", BindingFlags.NonPublic | BindingFlags.Static)!;
            Assert.Equal(65537, (int)m.Invoke(null, new object[] { 1, 1 })!);
        }

        [Fact]
        public void CombineToFloat_Zero()
        {
            var m = typeof(ModbusClientWrapper)
                .GetMethod("CombineToFloat", BindingFlags.NonPublic | BindingFlags.Static)!;
            Assert.Equal(0.0f, (float)m.Invoke(null, new object[] { 0, 0 })!);
        }

        [Fact]
        public void CombineToDouble_Zero()
        {
            var m = typeof(ModbusClientWrapper)
                .GetMethod("CombineToDouble", BindingFlags.NonPublic | BindingFlags.Static)!;
            Assert.Equal(0.0, (double)m.Invoke(null, new object[] { new[] { 0, 0, 0, 0 } })!);
        }

        // ── DISPOSE ───────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_WhenNotConnected_DoesNotThrow()
        {
            var c = new ModbusClientWrapper(MakeDevice());
            Assert.Null(Record.Exception(() => c.Dispose()));
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var c = new ModbusClientWrapper(MakeDevice());
            c.Dispose();
            Assert.Null(Record.Exception(() => c.Dispose()));
        }

        // ── ReadResult & DataReceivedEventArgs ────────────────────────────────

        [Fact]
        public void ReadResult_Defaults_NotNull()
        {
            var r = new ReadResult();
            Assert.False(r.Success);
            Assert.NotNull(r.Values);
            Assert.NotNull(r.ErrorMessage);
        }

        [Fact]
        public void DataReceivedEventArgs_Properties_Set()
        {
            var ts = DateTime.Now;
            var e = new DataReceivedEventArgs { TagId = 7, Value = 3.14, Timestamp = ts };
            Assert.Equal(7, e.TagId);
            Assert.Equal(3.14, e.Value);
            Assert.Equal(ts, e.Timestamp);
        }

        // ── YARDIMCI ─────────────────────────────────────────────────────────

        private static Device MakeDevice(string ip = "127.0.0.1") =>
            new Device { DeviceId = 1, Name = "T", IPAddress = ip, Port = 502, SlaveId = 1 };

        private static Tag MakeTag(int address) =>
            new Tag { TagId = 1, Name = "T", Address = address, RegisterType = "HoldingRegister", DataType = TagDataType.Int16 };

        private static double InvokeConvert(int[] regs, TagDataType type)
        {
            var m = typeof(ModbusClientWrapper)
                .GetMethod("ConvertRegisters", BindingFlags.NonPublic | BindingFlags.Static)!;
            return (double)m.Invoke(null, new object[] { regs, type })!;
        }

        private static double InvokeConvertRaw(int[]? regs, TagDataType type)
        {
            var m = typeof(ModbusClientWrapper)
                .GetMethod("ConvertRegisters", BindingFlags.NonPublic | BindingFlags.Static)!;
            return (double)m.Invoke(null, new object?[] { regs, type })!;
        }

        private static void SetWhitelist(params string[] ips)
        {
            var f = typeof(ModbusClientWrapper)
                .GetField("AllowedIpAddresses", BindingFlags.Static | BindingFlags.NonPublic)!;
            var set = (HashSet<string>)f.GetValue(null)!;
            set.Clear();
            foreach (var ip in ips) set.Add(ip);
        }

        private static void ClearWhitelist()
        {
            var f = typeof(ModbusClientWrapper)
                .GetField("AllowedIpAddresses", BindingFlags.Static | BindingFlags.NonPublic)!;
            ((HashSet<string>)f.GetValue(null)!).Clear();
        }
    }
}
