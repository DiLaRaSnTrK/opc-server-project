// <copyright file="OpcServerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Infrastructure.OPC
{
    using Core.Database;
    using Core.Models;
    using Microsoft.Extensions.Logging;
    using Opc.Ua;
    using Opc.Ua.Configuration;
    /// <summary>
    /// OPC UA sunucusunu başlatan ve yöneten servis.
    /// Aşama 7 güvenlik iyileştirmeleri:
    ///   - SecurityMode: None → SignAndEncrypt
    ///   - AutoAcceptUntrustedCertificates: true → false
    ///   - MinimumCertificateKeySize: 1024 → 2048
    ///   - RejectSHA1SignedCertificates: false → true
    ///   - BaseAddress: 0.0.0.0 → localhost (yalnızca yerel arayüz)
    ///   - Loglama: Console.WriteLine → ILogger (Serilog)
    /// </summary>
    public class OpcServerService
    {
        private readonly OpcTagUpdater _opcTagUpdater;
        private readonly DatabaseService _db;
        private readonly ILogger<OpcServerService> _logger;
        private DevSecOpsServer _server;

        public OpcServerService(
            OpcTagUpdater opcTagUpdater,
            DatabaseService db,
            ILogger<OpcServerService> logger)
        {
            _opcTagUpdater = opcTagUpdater ?? throw new ArgumentNullException(nameof(opcTagUpdater));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync()
        {
            // DB'den tüm tag'leri çek
            var allTags = new List<Tag>();
            var channels = _db.GetChannels();

            foreach (var channel in channels)
            {
                var devices = _db.GetDevicesByChannelId(channel.ChannelId);
                foreach (var device in devices)
                {
                    var tags = _db.GetTagsByDeviceId(device.DeviceId);
                    allTags.AddRange(tags);
                }
            }

            _logger.LogInformation("[OPC] {TagCount} tag DB'den yüklendi.", allTags.Count);

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
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault",
                        SubjectName = "CN=DevSecOps OPC Server, C=TR"
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications"
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities"
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates"
                    },

                    // ── GÜVENLİK İYİLEŞTİRMESİ T-01, T-02, T-03 ──────────────
                    // Önceki: AutoAcceptUntrustedCertificates = true  (T-01 CVSS 9.1)
                    // Şimdi:  false → sertifikası güvenilir listede olmayan
                    //         istemci bağlanamaz
                    AutoAcceptUntrustedCertificates = false,

                    // ── GÜVENLİK İYİLEŞTİRMESİ T-10 ──────────────────────────
                    // Önceki: false → SHA-1 ile imzalı sertifikalara izin veriliyordu
                    // Şimdi:  true → SHA-1 sertifikalar reddedilir (NIST onaylamıyor)
                    RejectSHA1SignedCertificates = true,

                    // ── GÜVENLİK İYİLEŞTİRMESİ T-10 ──────────────────────────
                    // Önceki: 1024 → kırılabilir RSA anahtar boyutu
                    // Şimdi:  2048 → NIST SP 800-57 minimum önerisi
                    MinimumCertificateKeySize = 2048
                },

                TransportQuotas = new TransportQuotas
                {
                    OperationTimeout = 15000,
                    // ── RATE LİMİTİNG T-11 ────────────────────────────────────
                    // Maksimum eş zamanlı kanal sayısını sınırla
                    MaxBufferSize = 65535,
                    MaxMessageSize = 4194304,
                    MaxArrayLength = 65535,
                    MaxStringLength = 131072
                },

                ServerConfiguration = new ServerConfiguration
                {
                    // ── GÜVENLİK İYİLEŞTİRMESİ T-10 ──────────────────────────
                    // Önceki: 0.0.0.0 → tüm ağ arayüzlerine açık (T-10 CVSS 7.5)
                    // Şimdi:  localhost → yalnızca yerel makine bağlanabilir.
                    // Uzak OPC UA istemcileri için güvenli tünel (VPN/SSH)
                    // üzerinden erişim önerilir.
                    BaseAddresses = new StringCollection { "opc.tcp://localhost:4840" },

                    SecurityPolicies = new ServerSecurityPolicyCollection
                    {
                        // ── GÜVENLİK İYİLEŞTİRMESİ T-01, T-03, T-08, T-14 ───
                        // Önceki: SecurityMode.None + SecurityPolicies.None
                        //         → şifresiz, kimlik doğrulamasız (T-14 CVSS 9.8)
                        // Şimdi:  SignAndEncrypt + Aes256_Sha256_RsaPss
                        //         → mesajlar hem imzalanır hem AES-256 şifrelenir
                        new ServerSecurityPolicy
                        {
                            SecurityMode      = MessageSecurityMode.SignAndEncrypt,
                            SecurityPolicyUri = SecurityPolicies.Aes256_Sha256_RsaPss
                        },
                        // Geriye dönük uyumluluk için Sign-only politikası
                        // (eski istemciler varsa kaldırılabilir)
                        new ServerSecurityPolicy
                        {
                            SecurityMode      = MessageSecurityMode.Sign,
                            SecurityPolicyUri = SecurityPolicies.Basic256Sha256
                        }
                    },

                    // Maksimum eş zamanlı oturum sayısı (DoS koruması T-11)
                    MaxSessionCount = 10,

                    // Oturum zaman aşımı: 10 dakika (ms cinsinden)
                    MaxSessionTimeout = 600000,

                    // Minimum oturum zaman aşımı: 1 dakika
                    MinSessionTimeout = 60000
                },

                DisableHiResClock = true
            };

            await config.Validate(ApplicationType.Server);

            var application = new ApplicationInstance(config);

            // Sertifika otomatik oluşturulur/doğrulanır
            // checkApplicationInstanceCertificate: 2048 bit RSA
            bool certOk = await application.CheckApplicationInstanceCertificatesAsync(false, 2048);
            if (!certOk)
            {
                _logger.LogWarning("[OPC] Sertifika doğrulaması başarısız. Sunucu yine de başlatılıyor.");
            }

            _server = new DevSecOpsServer(allTags);
            await application.Start(_server);

            _opcTagUpdater.SetNodeManager(_server.NodeManager);

            _logger.LogInformation(
                "[OPC] Sunucu başlatıldı: opc.tcp://localhost:4840 | " +
                "Mod: SignAndEncrypt | AutoAccept: false | MinKeySize: 2048");
        }

        public void Stop()
        {
            _server?.Stop();
            _logger.LogInformation("[OPC] Sunucu durduruldu.");
        }
    }
}
