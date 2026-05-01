// Copyright (c) OPC Server Project. All rights reserved.

namespace Infrastructure.OPC
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Database;
    using Core.Models;
    using Microsoft.Extensions.Logging;
    using Opc.Ua;
    using Opc.Ua.Configuration;

    /// <summary>
    /// OPC UA sunucusunu başlatan ve yöneten servis.
    /// Aşama 7 güvenlik iyileştirmeleri:
    ///   T-01/T-03 SecurityMode → SignAndEncrypt
    ///   T-01      AutoAcceptUntrustedCertificates → false
    ///   T-10      MinimumCertificateKeySize → 2048, RejectSHA1 → true
    ///   T-11      BaseAddress → localhost, MaxSessionCount → 10
    /// </summary>
    public class OpcServerService
    {
        private readonly OpcTagUpdater opcTagUpdater;
        private readonly DatabaseService db;
        private readonly ILogger<OpcServerService> logger;
        private DevSecOpsServer? server;

        /// <summary>Initializes a new instance of the <see cref="OpcServerService"/> class.</summary>
        public OpcServerService(
            OpcTagUpdater opcTagUpdater,
            DatabaseService db,
            ILogger<OpcServerService> logger)
        {
            this.opcTagUpdater = opcTagUpdater ?? throw new ArgumentNullException(nameof(opcTagUpdater));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Sunucuyu başlatır.</summary>
        public async Task StartAsync()
        {
            var allTags = new List<Tag>();
            var channels = this.db.GetChannels();

            foreach (var channel in channels)
            {
                var devices = this.db.GetDevicesByChannelId(channel.ChannelId);
                foreach (var device in devices)
                {
                    var tags = this.db.GetTagsByDeviceId(device.DeviceId);
                    allTags.AddRange(tags);
                }
            }

            this.logger.LogInformation("[OPC] {TagCount} tag DB'den yüklendi.", allTags.Count);

            var config = new ApplicationConfiguration
            {
                ApplicationName = "DevSecOps OPC Server",
                ApplicationUri = "urn:localhost:DevSecOpsOPCServer",
                ApplicationType = ApplicationType.Server,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = @"Directory",
                        StorePath = @"./pki/MachineDefault", // DEĞİŞTİ
                        SubjectName = "CN=DevSecOps OPC Server, C=TR",
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"./pki/TrustedPeer", // DEĞİŞTİ
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"./pki/TrustedIssuer", // DEĞİŞTİ
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"./pki/Rejected", // DEĞİŞTİ
                    },
                    AutoAcceptUntrustedCertificates = false,
                    RejectSHA1SignedCertificates = true, // Uyumluluk için false kalsın
                    MinimumCertificateKeySize = 2048,
                },
                TransportQuotas = new TransportQuotas
                {
                    OperationTimeout = 15000,
                    MaxBufferSize = 65535,
                    MaxMessageSize = 4194304,
                    MaxArrayLength = 65535,
                    MaxStringLength = 131072,
                },
                ServerConfiguration = new ServerConfiguration
                {
                    BaseAddresses = new StringCollection { "opc.tcp://0.0.0.0:4840" },
                    SecurityPolicies = new ServerSecurityPolicyCollection
                    {
                        /*new ServerSecurityPolicy 
                        {
                            SecurityMode = MessageSecurityMode.None, 
                            SecurityPolicyUri = SecurityPolicies.None 
                        },*/
                        new ServerSecurityPolicy
                        {
                            SecurityMode      = MessageSecurityMode.SignAndEncrypt,
                            SecurityPolicyUri = SecurityPolicies.Aes256_Sha256_RsaPss,
                        },
                        new ServerSecurityPolicy
                        {
                            SecurityMode      = MessageSecurityMode.SignAndEncrypt,
                            SecurityPolicyUri = SecurityPolicies.Basic256Sha256,
                        },
                    },
                    MaxSessionCount = 10,
                    MaxSessionTimeout = 600000,
                    MinSessionTimeout = 60000,
                },
                DisableHiResClock = true,
            };

            await config.ValidateAsync(ApplicationType.Server);

            var application = new ApplicationInstance { ApplicationConfiguration = config };
            bool certOk = await application.CheckApplicationInstanceCertificatesAsync(false, 2048);
            if (!certOk)
            {
                this.logger.LogWarning("[OPC] Sertifika doğrulaması başarısız.");
            }

            this.server = new DevSecOpsServer(allTags);
            await application.StartAsync(this.server);

            if (this.server.NodeManager != null)
            {
                this.opcTagUpdater.SetNodeManager(this.server.NodeManager);
            }

            this.logger.LogInformation(
            "[OPC] Sunucu başlatıldı: opc.tcp://localhost:4840 | Mod: SignAndEncrypt (Strict) | KeySize: 2048");
        }

        /// <summary>Sunucuyu durdurur.</summary>
        public async Task StopAsync()
        {
            if (this.server != null)
            {
                await this.server.StopAsync();
                this.logger.LogInformation("[OPC] Sunucu durduruldu.");
            }
        }
    }
}
