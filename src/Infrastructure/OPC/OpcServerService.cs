using Opc.Ua;
using Opc.Ua.Configuration;

namespace Infrastructure.OPC
{
    // 3. OpcServerService — düzeltilmiş hali
    public class OpcServerService
    {
        private readonly OpcTagUpdater _opcTagUpdater;
        private DevSecOpsServer _server;

        public OpcServerService(OpcTagUpdater opcTagUpdater)
        {
            _opcTagUpdater = opcTagUpdater;
        }

        public async Task StartAsync()
        {
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
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications",
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities",
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates",
                    },
                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 1024
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ServerConfiguration = new ServerConfiguration
                {
                    BaseAddresses = new StringCollection { "opc.tcp://0.0.0.0:4840" },
                    MinRequestThreadCount = 5,
                    MaxRequestThreadCount = 100,
                    MaxQueuedRequestCount = 200,
                    SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.None,
                        SecurityPolicyUri = SecurityPolicies.None
                    }
                }
                },
                DisableHiResClock = true
            };

            await config.Validate(ApplicationType.Server);

            var application = new ApplicationInstance(config);

            // ⚠️ Bu satır kritik — certificate otomatik oluşturulur
            bool certOk = await application.CheckApplicationInstanceCertificatesAsync(false, 2048);
            if (!certOk)
                throw new Exception("Certificate hatası!");

            _server = new DevSecOpsServer();
            await application.Start(_server); // Start() değil, application.Start() kullan
            _opcTagUpdater.SetNodeManager(_server.NodeManager);
            Console.WriteLine("OPC Server çalışıyor: opc.tcp://localhost:4840");
        }

        public async Task StopAsync()
        {
            _server?.Stop();
        }
    }
}