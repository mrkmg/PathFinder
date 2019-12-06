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
            return (new AStar<T>(origin, destination, thoroughness)).Solve();
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
            return (new AStar<T>(origin, destination, nodeValidator, thoroughness)).Solve();
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
            return (new AStar<T>(origin, destination, thoroughness)).Solve();
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
            return (new AStar<T>(origin, destination, nodeValidator, thoroughness)).Solve();
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
            _openNodes = new OpenNodeStoreSortedLinkedList<NodeMetaData> {_current};
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
        
        private readonly HashSet<NodeMetaData> _closedNodes = new HashSet<NodeMetaData>();
        private readonly IOpenNodeStore<NodeMetaData> _openNodes;
        private readonly NodeMetas _nodeMetas = new NodeMetas();
        private readonly Func<T, T, bool> _nodeValidator = null;
        private IList<T> _path;
        private NodeMetaData _current;
        private double _thoroughness = 0.5;
        
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
                if (value < 0 || value > 1) throw new Exception("Invalid Value");
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

        private static int NodeComparer(NodeMetaData x, NodeMetaData y)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (x.TotalCost == y.TotalCost) return 0;
            return x.TotalCost > y.TotalCost ? 1 : -1;
        }

        private void SetCurrentNode()
        {
            _current = _openNodes.PopLowest();
            _closedNodes.Add(_current);
        }

        private void ProcessNeighbors()
        {
            
            var neighborsEnumerable = Current
                .GetNeighbors()
                .Select(neighbor => _nodeMetas.Get((T)neighbor))
                .Where(n => !_closedNodes.Contains(n));
            if (_nodeValidator != null) 
                neighborsEnumerable = neighborsEnumerable.Where(n => _nodeValidator(Current, n.Node));
            neighborsEnumerable.ForAll(ProcessNeighbor);
        }

        private void ProcessNeighbor(NodeMetaData neighbor)
        {
            var fromCost = _current.FromCost + Current.RealCostTo(neighbor.Node);

            if (_openNodes.Contains(neighbor))
            {
                if (fromCost < neighbor.FromCost)
                {
                    var toCost = neighbor.Node.EstimatedCostTo(Destination);
                    var totalCost = fromCost * Thoroughness + toCost * (1 - Thoroughness);
                    neighbor.Parent = Current;
                    neighbor.ToCost = toCost;
                    neighbor.FromCost = fromCost;
                    neighbor.TotalCost = totalCost;
                    _openNodes.Resort(neighbor);
                }
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
            return _path ??= BuildPath();
        }

        private IList<T> BuildPath()
        {
            var path = new List<T> {Destination};
            var cNode = Destination;

            do
            {
                cNode = _nodeMetas.Get(cNode).Parent;
                path.Add(cNode);
            } while (!cNode.Equals(Origin));

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
            private readonly Dictionary<T, NodeMetaData> _nodeMetaData = new Dictionary<T, NodeMetaData>();

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

        private class NodeMetaData : IEqualityComparer<NodeMetaData>, IEquatable<NodeMetaData>, IComparable<NodeMetaData>
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

            public int CompareTo(NodeMetaData other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return TotalCost.CompareTo(other.TotalCost);
            }
        }
    }
}
