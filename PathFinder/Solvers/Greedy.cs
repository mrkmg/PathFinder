using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PathFinder.Components;
using PathFinder.Interfaces;

namespace PathFinder.Solvers
{
    public class Greedy<T> : ISolver<T> where T : INode
    {
        
        /// <summary>
        ///     A List of nodes which still need to be checked
        /// </summary>
        public IEnumerable<T> Open => _openNodes.Select(n => n.Node);

        /// <summary>
        ///     A list of nodes which have already been checked
        /// </summary>
        public IEnumerable<T> Closed => _closedNodes.Select(n => n.Node);

        /// <summary>
        ///     The last node to be checked
        /// </summary>
        public T Current => _current.Node;

        /// <summary>
        ///     The current state of the solver.
        /// </summary>
        public SolverState State { get; private set; } = SolverState.Running;

        /// <summary>
        ///     The number of ticks this solver has performed.
        /// </summary>
        public int Ticks { get; private set; }

        /// <summary>
        ///     A reference to the origin.
        /// </summary>
        public T Origin { get; }

        /// <summary>
        ///     A reference to the destination.
        /// </summary>
        public T Destination { get; }

        /// <summary>
        ///     The path from the Origin to the Destination
        /// </summary>
        public IList<T> Path => GetPath();

        public IList<T> CurrentBestPath => BuildPath(Current);
        
        private readonly HashSet<NodeMetaData> _closedNodes = new();
        private readonly HashSet<NodeMetaData> _openNodes = new();
        private readonly SortedLinkedList<NodeMetaData> _openNodesSorted;
        private readonly NodeMetas _nodeMetas = new();
        private readonly Func<T, T, bool> _nodeValidator = null;
        private IList<T> _path;
        private NodeMetaData _current;
        
        
        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        public Greedy(T origin, T destination)
        {
            Origin = origin;
            Destination = destination;
            _current = _nodeMetas.Get(origin);
            _openNodes.Add(_current);
            _openNodesSorted = new SortedLinkedList<NodeMetaData>(new NodeMetaComparer()) {_current};
        }
        
        /// <summary>
        ///     Perform one tick
        /// </summary>
        public void Tick()
        {
            if (State != SolverState.Running) throw new Exception("Not running");

            if (_openNodes.Count == 0)
            {
                State = SolverState.Failed;
                return;
            }

            Ticks++;

            ProcessNeighbors();

            if (Current.Equals(Destination))
                State = SolverState.Success;
            else if (_openNodesSorted.First == null)
                State = SolverState.Failed;
            else
                SetCurrentNode();
        }
        
        private void SetCurrentNode()
        {
            Debug.Assert(_openNodesSorted.First != null, "_openNodesSorted.First != null");
            _current = _openNodesSorted.First.Value;
            _openNodesSorted.RemoveFirst();
            _openNodes.Remove(_current);
            _closedNodes.Add(_current);
        }

        private void ProcessNeighbors()
        {
            foreach (var neighbor in Current.GetReachableNodes())
            {
                var node = _nodeMetas.Get((T) neighbor);
                if (_closedNodes.Contains(node)) continue;
                ProcessNeighbor(node);
            }
        }

        private void ProcessNeighbor(NodeMetaData neighbor)
        {
            if (_nodeValidator != null && !_nodeValidator(Current, neighbor.Node)) return;
            if (_openNodes.Contains(neighbor)) return;
            
            _openNodes.Add(neighbor);
            neighbor.Parent = Current;
            neighbor.ToCost = neighbor.Node.EstimatedCostTo(Destination);
            _openNodesSorted.Add(neighbor);
        }

        /// <summary>
        ///     Cancel the current solver.
        /// </summary>
        public void Cancel()
        {
            State = SolverState.Failed;
        }
        
        private IList<T> GetPath()
        {
            if (State != SolverState.Success) return null;
            return _path ??= BuildPath(Destination);
        }

        private IList<T> BuildPath(T cNode)
        {
            var path = new List<T> {cNode};
            do
            {
                cNode = _nodeMetas.Get(cNode).Parent;
                path.Add(cNode);
            } while (!cNode.Equals(Origin));

            path.Reverse();
            return path;
        }
        
        private class NodeMetas
        {
            private readonly Dictionary<T, NodeMetaData> _nodeMetaData = new();

            public NodeMetaData Get(T node) => 
                _nodeMetaData.ContainsKey(node) ? _nodeMetaData[node] : BuildNewNodeMeta(node);

            private NodeMetaData BuildNewNodeMeta(T node)
            {
                var nodeMeta = new NodeMetaData(node);
                _nodeMetaData.Add(node, nodeMeta);
                return nodeMeta;
            }
        }

        private class NodeMetaComparer : Comparer<NodeMetaData>
        {
            public override int Compare(NodeMetaData x, NodeMetaData y)
            {
                if (x == null) return y == null ? 0 : -1;
                if (y == null) return 1;
                if (x.ToCost == y.ToCost) return 0;
                return x.ToCost > y.ToCost ? 1 : -1;
            }
        }

        private class NodeMetaData : IEqualityComparer<NodeMetaData>, IEquatable<NodeMetaData>
        {
            public readonly T Node;
            public T Parent;
            public double ToCost;

            public NodeMetaData(T obj) => Node = obj;
            public bool Equals(NodeMetaData x, NodeMetaData y) => y != null && x != null && Node.Equals(x.Node, y.Node);
            public int GetHashCode(NodeMetaData obj) => Node.GetHashCode(obj.Node);
            public bool Equals(NodeMetaData other) => Equals(this, other);
            public override bool Equals(object obj) => obj is NodeMetaData n && Equals(n);
            public override int GetHashCode() => Node.GetHashCode();
            public override string ToString() => Node.ToString();
        }
    }
}