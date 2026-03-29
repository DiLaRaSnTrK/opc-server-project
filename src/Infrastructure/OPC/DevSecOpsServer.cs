using Opc.Ua;
using Opc.Ua.Server;

namespace Infrastructure.OPC
{
    // 1. Custom Server sınıfı — StandardServer'ı extend eder
    public class DevSecOpsServer : StandardServer
    {
        public CustomNodeManager NodeManager { get; private set; }

        protected override MasterNodeManager CreateMasterNodeManager(
            IServerInternal server, ApplicationConfiguration configuration)
        {
            NodeManager = new CustomNodeManager(server, configuration);

            return new MasterNodeManager(server, configuration, null, NodeManager);
        }
    }
}
