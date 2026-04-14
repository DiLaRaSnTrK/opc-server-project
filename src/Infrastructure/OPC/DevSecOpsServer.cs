// Copyright (c) OPC Server Project. All rights reserved.

namespace Infrastructure.OPC
{
    using System.Collections.Generic;
    using Core.Models;
    using Opc.Ua;
    using Opc.Ua.Server;

    /// <summary>OPC UA sunucu ana sınıfı.</summary>
    public class DevSecOpsServer : StandardServer
    {
        private readonly List<Tag> tags;

        /// <summary>Initializes a new instance of the <see cref="DevSecOpsServer"/> class.</summary>
        public DevSecOpsServer(List<Tag> tags)
        {
            this.tags = tags;
        }

        /// <summary>Node manager örneği.</summary>
        public CustomNodeManager? NodeManager { get; private set; }

        /// <inheritdoc/>
        protected override MasterNodeManager CreateMasterNodeManager(
            IServerInternal server, ApplicationConfiguration configuration)
        {
            this.NodeManager = new CustomNodeManager(server, configuration, this.tags);
            return new MasterNodeManager(server, configuration, null, this.NodeManager);
        }
    }
}
