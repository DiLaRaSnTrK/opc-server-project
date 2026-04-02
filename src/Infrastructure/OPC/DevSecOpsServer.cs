using Core.Models;
using Opc.Ua;
using Opc.Ua.Server;

namespace Infrastructure.OPC
{
    public class DevSecOpsServer : StandardServer
    {
        public CustomNodeManager NodeManager { get; private set; }
        private readonly List<Tag> _tags;

        public DevSecOpsServer(List<Tag> tags)
        {
            _tags = tags;
        }

        protected override MasterNodeManager CreateMasterNodeManager(
            IServerInternal server, ApplicationConfiguration configuration)
        {
            NodeManager = new CustomNodeManager(server, configuration, _tags);
            return new MasterNodeManager(server, configuration, null, NodeManager);
        }
    }
}