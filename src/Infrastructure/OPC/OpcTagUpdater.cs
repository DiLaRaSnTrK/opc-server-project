using Core.Interfaces;
using Core.Models;

namespace Infrastructure.OPC
{
    public class OpcTagUpdater : ITagUpdater
    {
        private CustomNodeManager _nodeManager;

        // Server başladıktan sonra NodeManager'ı buraya bağla
        public void SetNodeManager(CustomNodeManager nodeManager)
        {
            _nodeManager = nodeManager;
        }

        public void UpdateTag(string tagName, object value)
        {
            _nodeManager?.UpdateTag(tagName, value);
        }

        public void AddTagNode(Tag tag)
        {
            _nodeManager?.AddTagNode(tag);
        }

        public void RemoveTagNode(string tagName)
        {
            _nodeManager?.RemoveTagNode(tagName);
        }
    }
}