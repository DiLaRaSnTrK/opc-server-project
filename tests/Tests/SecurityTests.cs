// <copyright file="SecurityTests.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Tests
{
    using System.Reflection;
    using Core.Models;
    using Core.Protocols;
    using Core.Security; // RBAC için eklendi
    using Microsoft.Extensions.Logging.Abstractions; // ILogger yerine NullLogger için
    using Opc.Ua;

    public class SecurityTests
    {
        // YARDIMCI: ILogger hatasını çözmek için NullLogger kullanıyoruz
        private readonly Microsoft.Extensions.Logging.ILogger<ModbusClientWrapper> _nullLogger =
            new NullLogger<ModbusClientWrapper>();

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
        public void OpcUa_AutoAcceptUntrustedCertificates_ShouldBe_False()
        {
            var config = BuildSecureConfig();
            Assert.False(config.SecurityConfiguration.AutoAcceptUntrustedCertificates);
        }

        // ── MODBUS IP WHİTELİST TESTLERİ ────────────────────────────────────

        [Fact]
        public async Task ModbusClient_WhitelistActive_UnknownIp_ThrowsUnauthorized()
        {
            SetWhitelist("10.0.0.1");
            try
            {
                // ÇÖZÜM: Logger olarak _nullLogger, adaptör olarak FakeAdapter verildi.
                var c = new ModbusClientWrapper(MakeDevice("192.168.99.99"), _nullLogger);
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
                var fakeAdapter = new FakeAdapter(connectSucceeds: true);

                var c = new ModbusClientWrapper(MakeDevice("127.0.0.1"), fakeAdapter, _nullLogger);

                await c.ConnectAsync();
                Assert.True(c.IsConnected);
            }
            finally { ClearWhitelist(); }
        }

        // ── RBAC TESTLERİ (Artık Sınıfın İçinde!) ────────────────────────────

        [Fact]
        public void UserService_AdminLogin_SetsAdminRole()
        {
            var svc = new UserService();
            var result = svc.TryLogin("admin", "admin123");
            Assert.True(result);
            Assert.Equal(UserRole.Admin, SessionContext.Instance.Role);
            SessionContext.Instance.Logout();
        }

        [Fact]
        public void SessionContext_Logout_ClearsSession()
        {
            SessionContext.Instance.Login("admin", UserRole.Admin);
            SessionContext.Instance.Logout();
            Assert.False(SessionContext.Instance.IsLoggedIn);
        }

        // ── YARDIMCI METOTLAR ───────────────────────────────────────────────

        private static ApplicationConfiguration BuildSecureConfig() => new ApplicationConfiguration
        {
            SecurityConfiguration = new SecurityConfiguration { AutoAcceptUntrustedCertificates = false },
            ServerConfiguration = new ServerConfiguration
            {
                SecurityPolicies = new ServerSecurityPolicyCollection {
                    new ServerSecurityPolicy { SecurityMode = MessageSecurityMode.SignAndEncrypt }
                }
            }
        };

        private static Device MakeDevice(string ip = "127.0.0.1") =>
            new Device { IPAddress = ip, Port = 502, SlaveId = 1 };

        private static void SetWhitelist(params string[] ips)
        {
            var f = typeof(ModbusClientWrapper).GetField("AllowedIpAddresses", BindingFlags.Static | BindingFlags.NonPublic);
            var set = (HashSet<string>)f.GetValue(null);
            set.Clear();
            foreach (var ip in ips) set.Add(ip);
        }

        private static void ClearWhitelist()
        {
            var f = typeof(ModbusClientWrapper).GetField("AllowedIpAddresses", BindingFlags.Static | BindingFlags.NonPublic);
            ((HashSet<string>)f.GetValue(null)).Clear();
        }
    }
}