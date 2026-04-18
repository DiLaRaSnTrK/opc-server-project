// Copyright (c) OPC Server Project. All rights reserved.

using Core.Models;
using Opc.Ua;
using Opc.Ua.Server;

namespace Infrastructure.OPC
{
    public class CustomNodeManager : CustomNodeManager2
    {
        private readonly Dictionary<string, BaseDataVariableState> nodeTags = new();
        private readonly List<Tag> tagDefinitions;

        public CustomNodeManager(
            IServerInternal server,
            ApplicationConfiguration config,
            List<Tag> tags)
            : base(server, config, "https://devsecops.opc.server/")
        {
            this.tagDefinitions = tags ?? new List<Tag>();
        }

        public override void CreateAddressSpace(
            IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                var root = CreateFolderNode(
                    "DevSecOpsData", "DevSecOps Tags", externalReferences);

                foreach (var tag in tagDefinitions)
                {
                    // DataType'a göre default değer belirle
                    object defaultValue = tag.DataType switch
                    {
                        TagDataType.Bool => (object)false,
                        TagDataType.Float => (object)0.0f,
                        TagDataType.Double => (object)0.0,
                        TagDataType.Int16 => (object)(short)0,
                        TagDataType.UInt16 => (object)(ushort)0,
                        TagDataType.Int32 => (object)0,
                        TagDataType.UInt32 => (object)(uint)0,
                        _ => (object)0
                    };

                    AddVariable(root, tag.Name, defaultValue);
                    Console.WriteLine($"[OPC] Node oluşturuldu: {tag.Name} ({tag.DataType})");
                }
            }
        }

        private FolderState CreateFolderNode(
            string name,
            string displayName,
            IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            var folder = new FolderState(null)
            {
                NodeId = new NodeId(name, NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = displayName,
                TypeDefinitionId = ObjectTypeIds.FolderType
            };

            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var refs))
                externalReferences[ObjectIds.ObjectsFolder] = refs = new List<IReference>();

            refs.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, folder.NodeId));
            folder.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);

            AddPredefinedNode(SystemContext, folder);
            return folder;
        }

        public void AddTagNode(Tag tag)
        {
            lock (Lock)
            {
                // Zaten varsa ekleme
                if (nodeTags.ContainsKey(tag.Name))
                {
                    Console.WriteLine($"[OPC] Node zaten var: {tag.Name}");
                    return;
                }

                // Root klasörü bul
                var folderId = new NodeId("DevSecOpsData", NamespaceIndex);
                var folder = FindPredefinedNode<FolderState>(folderId);

                if (folder == null)
                {
                    Console.WriteLine($"[OPC] Root klasör bulunamadı!");
                    return;
                }

                object defaultValue = tag.DataType switch
                {
                    TagDataType.Bool => (object)false,
                    TagDataType.Float => (object)0.0f,
                    TagDataType.Double => (object)0.0,
                    TagDataType.Int16 => (object)(short)0,
                    TagDataType.UInt16 => (object)(ushort)0,
                    TagDataType.Int32 => (object)0,
                    TagDataType.UInt32 => (object)(uint)0,
                    _ => (object)0
                };

                AddVariable(folder, tag.Name, defaultValue);
                Console.WriteLine($"[OPC] Runtime node eklendi: {tag.Name}");
            }
        }

        public void RemoveTagNode(string tagName)
        {
            lock (Lock)
            {
                if (!nodeTags.TryGetValue(tagName, out var variable))
                    return;

                var folderId = new NodeId("DevSecOpsData", NamespaceIndex);
                var folder = FindPredefinedNode<FolderState>(folderId);

                folder?.RemoveReference(ReferenceTypeIds.Organizes, false, variable.NodeId);

                // ✅ Düzeltildi
                RemovePredefinedNode(SystemContext, variable, new List<LocalReference>());
                nodeTags.Remove(tagName);

                Console.WriteLine($"[OPC] Node silindi: {tagName}");
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
            nodeTags[name] = variable;
        }

        public void UpdateTag(string tagName, object value)
        {
            if (nodeTags.TryGetValue(tagName, out var variable))
            {
                variable.Value = value;
                variable.Timestamp = DateTime.UtcNow;
                variable.StatusCode = StatusCodes.Good;
                variable.ClearChangeMasks(SystemContext, false);
                variable.ClearChangeMasks(SystemContext, false);
            }
            else
            {
                Console.WriteLine($"[OPC] Tag bulunamadı: {tagName}");
            }

        }
    }
}