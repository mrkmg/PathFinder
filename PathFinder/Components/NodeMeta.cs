using System;
using System.Collections.Generic;
using PathFinder.Interfaces;

namespace PathFinder.Components
{
    internal class NodeMetas<T> where T : INode
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

    internal class NodeMetaData<T> : IEqualityComparer<NodeMetaData<T>>, IEquatable<NodeMetaData<T>> where T : INode
    {
        public readonly T Node;
        public double FromCost;
        public T Parent;
        public double ToCost;
        public double TotalCost;

        public NodeMetaData(T obj) => Node = obj;
        public bool Equals(NodeMetaData<T> x, NodeMetaData<T> y) => y != null && x != null && Node.Equals(x.Node, y.Node);
        public int GetHashCode(NodeMetaData<T> obj) => Node.GetHashCode(obj.Node);
        public bool Equals(NodeMetaData<T> other) => Equals(this, other);
        public override bool Equals(object obj) => obj is NodeMetaData<T> n && Equals(n);
        public override int GetHashCode() => Node.GetHashCode();
        public override string ToString() => Node.ToString();
    }
}
