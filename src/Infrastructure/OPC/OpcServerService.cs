using Core.Database;
using Core.Models;
using Opc.Ua;
using Opc.Ua.Configuration;

namespace Infrastructure.OPC
{
    public class OpcServerService
    {
        private readonly OpcTagUpdater _opcTagUpdater;
        private readonly DatabaseService _db;
        private DevSecOpsServer _server;

        public OpcServerService(OpcTagUpdater opcTagUpdater, DatabaseService db)
        {
            _opcTagUpdater = opcTagUpdater;
            _db = db;
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

            Console.WriteLine($"[OPC] {allTags.Count} tag DB'den yüklendi.");

            // Config
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
                    TrustedIssuerCertificates = new CertificateTrustList  // ← bu eksikti
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities"
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates"
                    },
                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 1024
                },
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ServerConfiguration = new ServerConfiguration
                {
                    BaseAddresses = new StringCollection { "opc.tcp://0.0.0.0:4840" },
                    SecurityPolicies = new ServerSecurityPolicyCollection
                    {
                        new ServerSecurityPolicy
                        {
                            SecurityMode      = MessageSecurityMode.None,
                            SecurityPolicyUri = SecurityPolicies.None
                        }
                    }
                },
                DisableHiResClock = true
            };

            await config.Validate(ApplicationType.Server);

            var application = new ApplicationInstance(config);
            await application.CheckApplicationInstanceCertificatesAsync(false, 2048);

            // Tag listesiyle server'ı başlat
            _server = new DevSecOpsServer(allTags);
            await application.Start(_server);

            // NodeManager'ı OpcTagUpdater'a bağla
            _opcTagUpdater.SetNodeManager(_server.NodeManager);

            Console.WriteLine($"[OPC] Server çalışıyor: opc.tcp://localhost:4840");
        }

        public void Stop() => _server?.Stop();
    }
}