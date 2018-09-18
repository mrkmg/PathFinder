using System.Collections.Generic;
using PathFinder.Interfaces;

namespace PathFinder.Components
{
    public class NodeMetas<T> where T : INode
    {
        private readonly Dictionary<T, NodeMetaData<T>> _nodeMetaData = new Dictionary<T, NodeMetaData<T>>();

        public NodeMetaData<T> Get(T node)
        {
            return _nodeMetaData.ContainsKey(node) ? _nodeMetaData[node] : BuildNewNodeMeta(node);
        }

        private NodeMetaData<T> BuildNewNodeMeta(T node)
        {
            var nodeMeta = new NodeMetaData<T>(node);
            _nodeMetaData.Add(node, nodeMeta);
            return nodeMeta;
        }
    }

    public class NodeMetaData<T> where T : INode
    {
        public readonly T Obj;
        public double FromCost;
        public T Parent;
        public double ToCost;
        public double TotalCost;

        public NodeMetaData(T obj)
        {
            Obj = obj;
        }
    }
}
