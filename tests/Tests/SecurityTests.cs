// <copyright file="SecurityTests.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using Core.Protocols;
    using Opc.Ua;
    using Xunit;

    public class SecurityTests
    {
        // ── OPC UA KONFİGÜRASYON TESTLERİ ──────────────────────────────────

        [Fact]
        public void OpcUa_SecurityMode_ShouldBe_SignAndEncrypt()
        {
            var config = BuildSecureConfig();
            Assert.Contains(
                config.ServerConfiguration.SecurityPolicies,
                p => p.SecurityMode == MessageSecurityMode.SignAndEncrypt);
        }

        [Fact]
        public void OpcUa_SecurityPolicy_ShouldNot_Allow_None()
        {
            var config = BuildSecureConfig();
            Assert.DoesNotContain(
                config.ServerConfiguration.SecurityPolicies,
                p => p.SecurityMode == MessageSecurityMode.None);
        }

        [Fact]
        public void OpcUa_AutoAcceptUntrustedCertificates_ShouldBe_False()
        {
            var config = BuildSecureConfig();
            Assert.False(config.SecurityConfiguration.AutoAcceptUntrustedCertificates);
        }

        [Fact]
        public void OpcUa_MinimumCertificateKeySize_ShouldBe_2048OrMore()
        {
            var config = BuildSecureConfig();
            Assert.True(config.SecurityConfiguration.MinimumCertificateKeySize >= 2048);
        }

        [Fact]
        public void OpcUa_RejectSha1Certificates_ShouldBe_True()
        {
            var config = BuildSecureConfig();
            Assert.True(config.SecurityConfiguration.RejectSHA1SignedCertificates);
        }

        [Fact]
        public void OpcUa_BaseAddress_ShouldBe_Localhost_Not_AllInterfaces()
        {
            var config = BuildSecureConfig();
            Assert.DoesNotContain(config.ServerConfiguration.BaseAddresses, a => a.Contains("0.0.0.0"));
            Assert.Contains(config.ServerConfiguration.BaseAddresses, a => a.Contains("localhost"));
        }

        [Fact]
        public void OpcUa_MaxSessionCount_ShouldBe_Limited()
        {
            var config = BuildSecureConfig();
            Assert.True(config.ServerConfiguration.MaxSessionCount is > 0 and <= 100);
        }

        // ── MODBUS TESTLERİ ────────────────────────────────────

        [Fact]
        public async Task ModbusClient_WhitelistActive_UnknownIp_ThrowsUnauthorized()
        {
            SetWhitelist("10.0.0.1");
            try
            {
                var c = new ModbusClientWrapper(MakeDevice("192.168.99.99"), new FakeAdapter());
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => c.ConnectAsync());
            }
            finally { ClearWhitelist(); }
        }

        [Fact]
        public async Task ModbusClient_WhitelistActive_KnownIp_Passes()
        {
            SetWhitelist("127.0.0.1");
            try
            {
                var c = new ModbusClientWrapper(MakeDevice("127.0.0.1"), new FakeAdapter());
                await c.ConnectAsync();
                Assert.True(c.IsConnected);
            }
            finally { ClearWhitelist(); }
        }

        [Fact]
        public async Task ModbusClient_WhitelistEmpty_AnyValidIp_Connects()
        {
            ClearWhitelist();
            var c = new ModbusClientWrapper(MakeDevice("192.168.1.100"), new FakeAdapter());
            await c.ConnectAsync();
            Assert.True(c.IsConnected);
        }

        [Theory]
        [InlineData("gecersiz")]
        [InlineData("999.999.999.999")]
        [InlineData("")]
        public async Task ModbusClient_InvalidIpFormat_ThrowsArgumentException(string ip)
        {
            var c = new ModbusClientWrapper(MakeDevice(ip), new FakeAdapter());
            await Assert.ThrowsAsync<ArgumentException>(() => c.ConnectAsync());
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(65536)]
        [InlineData(100000)]
        public async Task ModbusClient_InvalidAddress_ReturnsFailResult(int address)
        {
            var c = new ModbusClientWrapper(MakeDevice(), new FakeAdapter());
            var result = await c.ReadTagAsync(MakeTag(address));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task ModbusClient_NullTag_ReturnsFailResult()
        {
            var c = new ModbusClientWrapper(MakeDevice(), new FakeAdapter());
            var result = await c.ReadTagAsync(null!);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task ModbusClient_ValidRead_ReturnsSuccess()
        {
            var c = new ModbusClientWrapper(MakeDevice(), new FakeAdapter(holdingRegisters: new[] { 42 }));
            var result = await c.ReadTagAsync(MakeTag(100, "HoldingRegister"));
            Assert.True(result.Success);
            Assert.Equal(42.0, result.Values[0]);
        }

        [Fact]
        public void ReadResult_DefaultValues_NotNull()
        {
            var r = new ReadResult();
            Assert.False(r.Success);
            Assert.NotNull(r.Values);
            Assert.NotNull(r.ErrorMessage);
        }

        // ── RBAC TESTLERİ ────────────────────────────────────

        [Fact]
        public void UserService_AdminLogin_SetsAdminRole()
        {
            var svc = new Core.Security.UserService();
            svc.TryLogin("admin", "admin123");

            Assert.True(Core.Security.SessionContext.Instance.IsAdmin);
            Core.Security.SessionContext.Instance.Logout();
        }

        [Fact]
        public void UserService_OperatorLogin_SetsOperatorRole()
        {
            var svc = new Core.Security.UserService();
            svc.TryLogin("operator", "operator123");

            Assert.False(Core.Security.SessionContext.Instance.IsAdmin);
            Core.Security.SessionContext.Instance.Logout();
        }

        [Fact]
        public void SessionContext_ConcurrentRead_IsThreadSafe()
        {
            var svc = new Core.Security.UserService();
            svc.TryLogin("operator", "operator123");

            var results = new System.Collections.Concurrent.ConcurrentBag<bool>();

            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() =>
                    results.Add(Core.Security.SessionContext.Instance.IsLoggedIn));
            }

            Task.WaitAll(tasks);
            Assert.All(results, r => Assert.True(r));

            Core.Security.SessionContext.Instance.Logout();
        }

        // ── YARDIMCI ─────────────────────────────────────────

        private static ApplicationConfiguration BuildSecureConfig() => new ApplicationConfiguration
        {
            ApplicationName = "DevSecOps OPC Server",
            ApplicationUri = "urn:localhost:DevSecOpsOPCServer",
            ApplicationType = ApplicationType.Server,
            SecurityConfiguration = new SecurityConfiguration
            {
                AutoAcceptUntrustedCertificates = false,
                RejectSHA1SignedCertificates = true,
                MinimumCertificateKeySize = 2048,
            },
            ServerConfiguration = new ServerConfiguration
            {
                BaseAddresses = new StringCollection { "opc.tcp://localhost:4840" },
                SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.SignAndEncrypt,
                        SecurityPolicyUri = SecurityPolicies.Aes256_Sha256_RsaPss,
                    }
                },
                MaxSessionCount = 10,
            },
        };

        private static Device MakeDevice(string ip = "127.0.0.1") =>
            new Device { DeviceId = 1, Name = "Test", IPAddress = ip, Port = 502, SlaveId = 1 };

        private static Tag MakeTag(int address, string registerType = "HoldingRegister") =>
            new Tag { TagId = 1, Name = "T", Address = address, RegisterType = registerType, DataType = TagDataType.Int16 };

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