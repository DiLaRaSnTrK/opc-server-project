using Core.Interfaces;

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
    }
}