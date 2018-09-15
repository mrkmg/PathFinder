using System.Collections.Generic;
using PathFinder.Interfaces;

namespace PathFinder.Components
{
    public class Node<T> where T : INode
    {
        public readonly T Obj;

        public Node<T> Parent;
        public double TotalCost;
        public double ToCost;
        public double FromCost;

        public Node(T obj)
        {
            Obj = obj;
            Obj = obj;
        }

        public double RealCostTo(Node<T> node) => Obj.RealCostTo(node.Obj);
        public double EstimatedCostTo(Node<T> node) => Obj.EstimatedCostTo(node.Obj);
        public IEnumerable<T> Neighbors() => (IEnumerable<T>) Obj.GetNeighbors();

        public override bool Equals(object obj) => obj is Node<T> n && Obj.Equals(n.Obj);
        public override int GetHashCode() => Obj.GetHashCode();
        public static bool operator ==(Node<T> node1, Node<T> node2) => node1.Equals(node2);
        public static bool operator !=(Node<T> node1, Node<T> node2) => !node1.Equals(node2);
    }
}
