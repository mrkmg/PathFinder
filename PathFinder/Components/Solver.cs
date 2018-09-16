using System.Collections.Generic;
using PathFinder.Interfaces;

namespace PathFinder.Components
{
    public abstract class Solver<T> : ISolver<T> where T : INode
    {
        public SolverState State { get; protected set; } = SolverState.Running;
        public int Ticks { get; protected set; }
        public T Origin => OriginNode.Obj;
        public T Destination => DestinationNode.Obj;

        protected Node<T> OriginNode;
        protected Node<T> DestinationNode;

        private readonly Dictionary<T, Node<T>> _nodeLookup = new Dictionary<T, Node<T>>();

        public Solver(T origin, T destination)
        {
            DestinationNode = GetNode(destination, false);
            OriginNode = GetNode(origin);
        }

        public void Cancel()
        {
            State = SolverState.Failed;
        }
        public abstract void Tick();
        public abstract IList<T> GetPath();

        protected Node<T> GetNode(T obj) => GetNode(obj, true);
        protected Node<T> GetNode(T obj, bool performToCost)
        {
            if (_nodeLookup.ContainsKey(obj))
            {
                return _nodeLookup[obj];
            }

            var node = new Node<T>(obj);
            _nodeLookup.Add(obj, node);
            node.FromCost = 0;
            node.ToCost = performToCost ? node.EstimatedCostTo(DestinationNode) : 0;
            node.TotalCost = node.FromCost + node.ToCost;
            return node;
        }
    }
}
