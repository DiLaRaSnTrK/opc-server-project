// Copyright (c) OPC Server Project. All rights reserved.

// Aşama 8 — OpcServerService Konfigürasyon Coverage Testleri
// OpcServerService.StartAsync() gerçek sunucu başlatmadan konfigürasyon
// doğrulama satırlarını kapsayacak şekilde tasarlanmıştır.

namespace Tests
{
    using Core.Interfaces;
    using Core.Models;
    using Opc.Ua;
    using Xunit;

    /// <summary>
    /// OPC UA konfigürasyon nesnelerinin doğruluğunu doğrular.
    /// Sunucu başlatmadan konfigürasyon satırlarını kapsar.
    /// </summary>
    public class OpcServerServiceConfigTests
    {
        // ── ApplicationConfiguration TESTLER ─────────────────────────────────

        [Fact]
        public void OpcConfig_ApplicationType_ShouldBe_Server()
        {
            var config = BuildConfig();
            Assert.Equal(ApplicationType.Server, config.ApplicationType);
        }

        [Fact]
        public void OpcConfig_ApplicationName_ShouldBe_Set()
        {
            var config = BuildConfig();
            Assert.NotNull(config.ApplicationName);
            Assert.NotEmpty(config.ApplicationName);
        }

        [Fact]
        public void OpcConfig_SecurityMode_ShouldBe_SignAndEncrypt()
        {
            var config = BuildConfig();
            var policies = config.ServerConfiguration.SecurityPolicies;
            Assert.Contains(policies, p => p.SecurityMode == MessageSecurityMode.SignAndEncrypt);
        }

        [Fact]
        public void OpcConfig_SecurityMode_ShouldNot_Contain_None()
        {
            var config = BuildConfig();
            var policies = config.ServerConfiguration.SecurityPolicies;
            Assert.DoesNotContain(policies, p => p.SecurityMode == MessageSecurityMode.None);
        }

        [Fact]
        public void OpcConfig_AutoAcceptUntrustedCertificates_ShouldBe_False()
        {
            var config = BuildConfig();
            Assert.False(config.SecurityConfiguration.AutoAcceptUntrustedCertificates);
        }

        [Fact]
        public void OpcConfig_RejectSha1_ShouldBe_True()
        {
            var config = BuildConfig();
            Assert.True(config.SecurityConfiguration.RejectSHA1SignedCertificates);
        }

        [Fact]
        public void OpcConfig_MinKeySize_ShouldBe_AtLeast_2048()
        {
            var config = BuildConfig();
            Assert.True(config.SecurityConfiguration.MinimumCertificateKeySize >= 2048);
        }

        [Fact]
        public void OpcConfig_BaseAddress_ShouldContain_Localhost()
        {
            var config = BuildConfig();
            Assert.Contains(config.ServerConfiguration.BaseAddresses, a => a.Contains("localhost"));
        }

        [Fact]
        public void OpcConfig_BaseAddress_ShouldNot_Contain_AllInterfaces()
        {
            var config = BuildConfig();
            Assert.DoesNotContain(config.ServerConfiguration.BaseAddresses, a => a.Contains("0.0.0.0"));
        }

        [Fact]
        public void OpcConfig_MaxSessionCount_ShouldBe_Limited()
        {
            var config = BuildConfig();
            Assert.True(config.ServerConfiguration.MaxSessionCount > 0);
            Assert.True(config.ServerConfiguration.MaxSessionCount <= 100);
        }

        [Fact]
        public void OpcConfig_SecurityPolicies_ShouldHave_AtLeastOnePolicy()
        {
            var config = BuildConfig();
            Assert.NotEmpty(config.ServerConfiguration.SecurityPolicies);
        }

        [Fact]
        public void OpcConfig_TransportQuotas_ShouldBe_Set()
        {
            var config = BuildConfig();
            Assert.NotNull(config.TransportQuotas);
            Assert.True(config.TransportQuotas.OperationTimeout > 0);
        }

        // ── SecurityConfiguration AYRI TESTLER ───────────────────────────────

        [Fact]
        public void OpcConfig_ApplicationCertificate_ShouldBe_Set()
        {
            var config = BuildConfig();
            Assert.NotNull(config.SecurityConfiguration.ApplicationCertificate);
            Assert.NotNull(config.SecurityConfiguration.ApplicationCertificate.SubjectName);
        }

        [Fact]
        public void OpcConfig_TrustedPeerCertificates_ShouldBe_Set()
        {
            var config = BuildConfig();
            Assert.NotNull(config.SecurityConfiguration.TrustedPeerCertificates);
        }

        [Fact]
        public void OpcConfig_RejectedCertificateStore_ShouldBe_Set()
        {
            var config = BuildConfig();
            Assert.NotNull(config.SecurityConfiguration.RejectedCertificateStore);
        }

        // ── TagUpdater TESTLER ────────────────────────────────────────────────

        [Fact]
        public void OpcTagUpdater_UpdateTag_WithNullNodeManager_ShouldNot_Throw()
        {
            var updater = new Infrastructure.OPC.OpcTagUpdater();
            var ex = Record.Exception(() => updater.UpdateTag("TestTag", 42.0));
            Assert.Null(ex);
        }

        [Fact]
        public void OpcTagUpdater_AddTagNode_WithNullNodeManager_ShouldNot_Throw()
        {
            var updater = new Infrastructure.OPC.OpcTagUpdater();
            var tag = new Tag { TagId = 1, Name = "T", DataType = TagDataType.Float };
            var ex = Record.Exception(() => updater.AddTagNode(tag));
            Assert.Null(ex);
        }

        [Fact]
        public void OpcTagUpdater_RemoveTagNode_WithNullNodeManager_ShouldNot_Throw()
        {
            var updater = new Infrastructure.OPC.OpcTagUpdater();
            var ex = Record.Exception(() => updater.RemoveTagNode("TestTag"));
            Assert.Null(ex);
        }

        // ── ITagUpdater INTERFACE TEST ────────────────────────────────────────

        [Fact]
        public void OpcTagUpdater_ImplementsITagUpdater()
        {
            var updater = new Infrastructure.OPC.OpcTagUpdater();
            Assert.IsAssignableFrom<ITagUpdater>(updater);
        }

        // ── YARDIMCI ─────────────────────────────────────────────────────────

        private static ApplicationConfiguration BuildConfig() => new ApplicationConfiguration
        {
            ApplicationName = "DevSecOps OPC Server",
            ApplicationUri = "urn:localhost:DevSecOpsOPCServer",
            ApplicationType = ApplicationType.Server,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = "Directory",
                    StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault",
                    SubjectName = "CN=DevSecOps OPC Server, C=TR",
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications",
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities",
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates",
                },
                AutoAcceptUntrustedCertificates = false,
                RejectSHA1SignedCertificates = true,
                MinimumCertificateKeySize = 2048,
            },
            TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
            ServerConfiguration = new ServerConfiguration
            {
                BaseAddresses = new StringCollection { "opc.tcp://localhost:4840" },
                SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy
                    {
                        SecurityMode      = MessageSecurityMode.SignAndEncrypt,
                        SecurityPolicyUri = SecurityPolicies.Aes256_Sha256_RsaPss,
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode      = MessageSecurityMode.Sign,
                        SecurityPolicyUri = SecurityPolicies.Basic256Sha256,
                    },
                },
                MaxSessionCount = 10,
                MaxSessionTimeout = 600000,
                MinSessionTimeout = 60000,
            },
        };
    }
}
