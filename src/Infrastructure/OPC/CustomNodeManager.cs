using Opc.Ua;
using Opc.Ua.Server;

namespace Infrastructure.OPC
{
    // 2. Custom NodeManager — Tag'leri AddressSpace'e ekler
    public class CustomNodeManager : CustomNodeManager2
    {
        private readonly Dictionary<string, BaseDataVariableState> _tags = new();

        public CustomNodeManager(IServerInternal server, ApplicationConfiguration config)
            : base(server, config, "http://devsecops.opc.server/")
        {
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                var root = CreateObjectNode(
                    null,
                    "DevSecOpsData",
                    "DevSecOps Tags",
                    NodeId.Null,
                    externalReferences
                );

                // Örnek tag ekle
                AddVariable(root, "Temperature", 0.0);
                AddVariable(root, "Pressure", 0.0);
                AddVariable(root, "Status", false);
            }
        }

        private void AddVariable(NodeState parent, string name, object defaultValue)
        {
            var variable = new BaseDataVariableState(parent)
            {
                NodeId = new NodeId(name, NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = name,
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                DataType = TypeInfo.GetDataTypeId(defaultValue),
                ValueRank = ValueRanks.Scalar,
                Value = defaultValue,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Historizing = false,
                StatusCode = StatusCodes.Good,
                Timestamp = DateTime.UtcNow
            };
            variable.AddReference(ReferenceTypeIds.Organizes, true, parent.NodeId);
            parent.AddReference(ReferenceTypeIds.Organizes, false, variable.NodeId);
            AddPredefinedNode(SystemContext, variable);
            _tags[name] = variable;
        }

        // OpcTagUpdater'dan çağrılacak metod
        public void UpdateTag(string tagName, object value)
        {
            if (_tags.TryGetValue(tagName, out var variable))
            {
                variable.Value = value;
                variable.Timestamp = DateTime.UtcNow;
                variable.ClearChangeMasks(SystemContext, false);
            }
        }

        private FolderState CreateObjectNode(
            NodeState parent, string name, string displayName,
            NodeId referenceType, IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            var folder = new FolderState(parent)
            {
                NodeId = new NodeId(name, NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = displayName,
                TypeDefinitionId = ObjectTypeIds.FolderType
            };
            if (parent == null)
            {
                if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var references))
                    externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
                references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, folder.NodeId));
                folder.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            }
            AddPredefinedNode(SystemContext, folder);
            return folder;
        }
    }
}
