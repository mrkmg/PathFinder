using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Components;
using PathFinder.Interfaces;
using JetBrains.Annotations;

namespace PathFinder.Solvers
{
    public static class AStar
    {
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="thoroughness">How thorough the search should be. Should be a value between 0 and 1.</param>
        [CanBeNull]
        public static IList<T> Solve<T>(T origin, T destination, double thoroughness = 0.5) where T : INode 
        {
            return new AStar<T>(origin, destination, thoroughness).Solve();
        }
        
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="nodeValidator">A function to validate if a node is usable for this solver.</param>
        /// <param name="thoroughness">How thorough the search should be. Should be a value between 0 and 1.</param>
        [CanBeNull]
        public static IList<T> Solve<T>(T origin, T destination, Func<T, T, bool> nodeValidator, double thoroughness = 0.5) where T : INode 
        {
            return new AStar<T>(origin, destination, nodeValidator, thoroughness).Solve();
        }
    }
    
    public class AStar<T> : ISolver<T> where T : INode
    {
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="thoroughness">How thorough the search should be. Should be a value between 0 and 1.</param>
        [CanBeNull]
        public static IList<T> Solve(T origin, T destination, double thoroughness = 0.5)
        {
            return new AStar<T>(origin, destination, thoroughness).Solve();
        }
        
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="nodeValidator">A function to validate if a node is usable for this solver.</param>
        /// <param name="thoroughness">How thorough the search should be. Should be a value between 0 and 1.</param>
        [CanBeNull]
        public static IList<T> Solve(T origin, T destination, Func<T, T, bool> nodeValidator, double thoroughness = 0.5)
        {
            return new AStar<T>(origin, destination, nodeValidator, thoroughness).Solve();
        }
        
        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="thoroughness">How thorough the search should be. Should be a value between 0 and 1.</param>
        public AStar(T origin, T destination, double thoroughness = 0.5)
        {
            Origin = origin;
            Destination = destination;
            Thoroughness = thoroughness;
            _current = _nodeMetas.Get(origin);
            _current.ToCost = origin.EstimatedCostTo(Destination);
            _closest = _current;
            _openNodes = new SortedLinkedList<NodeMetaData>(new NodeMetaComparer()) {_current};
        }

        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="nodeValidator">A function to validate if a node is usable for this solver.</param>
        /// <param name="thoroughness">How thorough the search should be. Should be a value between 0 and 1.</param>
        public AStar(T origin, T destination, Func<T, T, bool> nodeValidator, double thoroughness = 0.5) : this(origin, destination, thoroughness)
        {
            _nodeValidator = nodeValidator;
        }
        
        private readonly HashSet<NodeMetaData> _closedNodes = new();
        private readonly SortedLinkedList<NodeMetaData> _openNodes;
        private readonly NodeMetas _nodeMetas = new();
        private readonly Func<T, T, bool> _nodeValidator;
        private IList<T> _path;
        private NodeMetaData _current;
        private double _thoroughness = 0.5;
        private NodeMetaData _closest;
        
        /// <summary>
        ///     A List of nodes which still need to be checked
        /// </summary>
        public IEnumerable<T> Open => _openNodes.Select(n => n.Node);

        /// <summary>
        ///     A list of nodes which have already been checked
        /// </summary>
        public IEnumerable<T> Closed => _closedNodes.Select(n => n.Node);

        /// <summary>
        ///     Count of open nodes
        /// </summary>
        public int OpenCount => _openNodes.Count;
        
        /// <summary>
        ///     Count of closed nodes
        /// </summary>
        public int ClosedCount => _closedNodes.Count;

        /// <summary>
        ///     The last node to be checked
        /// </summary>
        public T Current => _current.Node;

        /// <summary>
        ///     How thorough the search should be. Should be a value between 0 and 1.
        /// </summary>
        public double Thoroughness
        {
            get => _thoroughness;
            set
            {
                if (value is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(value));
                _thoroughness = value;
            }
        }

        /// <summary>
        ///     The cost to get from the Origin to the Destination via the path
        /// </summary>
        public double Cost => CalcCost();

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

        /// <summary>
        ///     The current best path to get as close as possible to the Destination
        /// </summary>
        public IList<T> CurrentBestPath => BuildPath(_closest.Node);

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
            else if (_openNodes.Count == 0)
                State = SolverState.Failed;
            else
                SetCurrentNode();
        }

        /// <summary>
        ///     Cancel the current solver.
        /// </summary>
        public void Cancel()
        {
            State = SolverState.Failed;
        }

        private void SetCurrentNode()
        {
            _current = _openNodes.PopFirst();
            
            if (_current.ToCost < _closest.ToCost)
                _closest = _current;
            
            _closedNodes.Add(_current);
        }

        private void ProcessNeighbors()
        {
            var neighbors = Current.GetReachableNodes();
            foreach (var neighbor in neighbors)
            {
                var node = _nodeMetas.Get((T) neighbor);
                if (_closedNodes.Contains(node)) continue;
                if (_nodeValidator != null && _nodeValidator(Current, node.Node)) continue;
                ProcessNeighbor(node);
            }
        }

        private void ProcessNeighbor(NodeMetaData neighbor)
        {
            var fromCost = _current.FromCost + Current.RealCostTo(neighbor.Node);

            if (_openNodes.Contains(neighbor))
            {
                if (fromCost >= neighbor.FromCost) return;
                
                var totalCost = fromCost * Thoroughness + neighbor.ToCost * (1 - Thoroughness);
                neighbor.Parent = Current;
                neighbor.FromCost = fromCost;
                neighbor.TotalCost = totalCost;
                _openNodes.Resort(neighbor);
            }
            else
            {
                var toCost = neighbor.Node.EstimatedCostTo(Destination);
                var totalCost = fromCost * Thoroughness + toCost * (1 - Thoroughness);
                neighbor.Parent = Current;
                neighbor.ToCost = toCost;
                neighbor.FromCost = fromCost;
                neighbor.TotalCost = totalCost;
                _openNodes.Add(neighbor);
            }
        }

        private IList<T> GetPath()
        {
            if (State != SolverState.Success) return null;
            return _path ??= BuildPath(Destination);
        }

        private IList<T> BuildPath(T cNode)
        {
            if (Origin.Equals(cNode)) return new List<T> {cNode};
            
            var path = new List<T>();

            while (true)
            {
                path.Add(cNode);
                cNode = _nodeMetas.Get(cNode).Parent;
                if (Origin.Equals(cNode) || cNode == null) break;
            }

            path.Reverse();
            return path;
        }

        private double CalcCost()
        {
            if (Path == null) return 0;
            
            var cost = 0d;
            var last = Path.First();

            foreach (var next in Path.Skip(1))
            {
                cost += last.RealCostTo(next);
                last = next;
            }

            return cost;
        }

        private class NodeMetas
        {
            private readonly Dictionary<T, NodeMetaData> _nodeMetaData = new();

            public NodeMetaData Get(T node)
            {
                return _nodeMetaData.ContainsKey(node) ? _nodeMetaData[node] : BuildNewNodeMeta(node);
            }

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
                if (x.TotalCost == y.TotalCost) return 0;
                return x.TotalCost > y.TotalCost ? 1 : -1;
            }
        }

        private class NodeMetaData : IEqualityComparer<NodeMetaData>, IEquatable<NodeMetaData>
        {
            public readonly T Node;
            public double FromCost;
            public T Parent;
            public double ToCost;
            public double TotalCost;

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
