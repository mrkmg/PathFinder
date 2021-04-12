using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PathFinder.Solvers.Generic
{
    
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="T"><see cref="IGraphNode"/></typeparam>
    public abstract class GenericGraphSolverBase<T> : IGraphSolver<T> where T : IGraphNode
    {
        protected GenericGraphSolverBase(IComparer<GraphNodeMetaData<T>> comparer, T origin, T destination)
        {
            Origin = origin;
            Destination = destination;
            CurrentMetaData = new GraphNodeMetaData<T>(origin, 0) {ToCost = origin.EstimatedCostTo(Destination)};
            _closest = CurrentMetaData;
            _lastGraphNodeId = 1;
            Comparer = comparer;
            OpenNodes = new SortedSet<GraphNodeMetaData<T>>(comparer) {CurrentMetaData};
        }

        protected GenericGraphSolverBase(IComparer<GraphNodeMetaData<T>> comparer, T origin, T destination, NodeValidator nodeValidator) 
            : this(comparer, origin, destination)
        {
            _nodeValidator = nodeValidator;
        }
        
        /// <inheritdoc cref="IGraphSolver{T}.Open"/>
        public IEnumerable<T> Open => OpenNodes.Select(n => n.Node);

        /// <inheritdoc cref="IGraphSolver{T}.Closed"/>
        public IEnumerable<T> Closed => Meta.Values
            .Where(n => n.Status == NodeStatus.Closed)
            .Select(n => n.Node);

        /// <inheritdoc cref="IGraphSolver{T}.OpenCount"/>
        public int OpenCount => OpenNodes.Count;
        
        /// <inheritdoc cref="IGraphSolver{T}.ClosedCount"/>
        public int ClosedCount { get; private set; }
        
        /// <inheritdoc cref="IGraphSolver{T}.Current"/>
        public T Current => CurrentMetaData.Node;

        /// <inheritdoc cref="IGraphSolver{T}.PathCost"/>
        public double PathCost => GetCost();

        /// <inheritdoc cref="IGraphSolver{T}.State"/>
        public SolverState State { get; protected set; } = SolverState.Waiting;

        /// <inheritdoc cref="IGraphSolver{T}.Ticks"/>
        public int Ticks { get; private set; }

        /// <inheritdoc cref="IGraphSolver{T}.Origin"/>
        public T Origin { get; }

        /// <inheritdoc cref="IGraphSolver{T}.Destination"/>
        public T Destination { get; }
        
        /// <inheritdoc cref="IGraphSolver{T}.Path"/>
        public IList<T> Path => GetPath();

        /// <inheritdoc cref="IGraphSolver{T}.CurrentBestPath"/>
        public IList<T> CurrentBestPath => BuildPath(_closest.Node);

        /// <inheritdoc cref="IGraphSolver{T}.MaxTicks"/>
        public int MaxTicks { get; set; } = int.MaxValue;
        
        protected SortedSet<GraphNodeMetaData<T>> OpenNodes;
        protected readonly Dictionary<T, GraphNodeMetaData<T>> Meta = new Dictionary<T, GraphNodeMetaData<T>>();
        protected GraphNodeMetaData<T> CurrentMetaData;
        protected readonly IComparer<GraphNodeMetaData<T>> Comparer;
        
        private readonly NodeValidator _nodeValidator;
        private IList<T> _path;
        private GraphNodeMetaData<T> _closest;
        private double? _cost;
        private long _lastGraphNodeId;

        private int _remainingTicks;
        
        private void Tick()
        {
            if (OpenNodes.Count == 0)
                State = SolverState.Failure;
            else if (_remainingTicks == 0)
                State = SolverState.Waiting;
            else if (Ticks > MaxTicks)
                State = SolverState.Failure;

            if (State != SolverState.Running)
                return;

            if (_remainingTicks > 0)
                _remainingTicks--;
            
            Ticks++;

            SetCurrentNode();
            ProcessNeighbors();
        }
        
        public void Stop() => _remainingTicks = 0;

        public SolverState Start(int numTicks = -1)
        {
            if (State != SolverState.Waiting)
                throw new InvalidOperationException("Graph Solver is not in a startable state");
            _remainingTicks = numTicks;
            State = SolverState.Running;
            while (State == SolverState.Running) 
                Tick();
            return State;
        }

        public Task<SolverState> StartAsync(int ticks = -1) => new Task<SolverState>(() => Start(ticks));

        protected virtual void ProcessNeighbor(GraphNodeMetaData<T> neighborMetaData)
        {
            if (neighborMetaData.Status != NodeStatus.New) return;
            neighborMetaData.Status = NodeStatus.Open;
            var didAdd = OpenNodes.Add(neighborMetaData);
            Debug.Assert(didAdd, "Failed to add neighborNode to open nodes list. The comparer is probably invalid.");
        }

        [NotNull]
        protected GraphNodeMetaData<T> GetMeta(T node)
        {
            if (Meta.TryGetValue(node, out var meta)) return meta;
            meta = new GraphNodeMetaData<T>(node, _lastGraphNodeId++)
            {
                ToCost = node.EstimatedCostTo(Destination),
                FromCost = CurrentMetaData.FromCost + CurrentMetaData.Node.RealCostTo(node),
                Parent = CurrentMetaData
            };
            Meta.Add(node, meta);
            return meta;
        }
        
        private void SetCurrentNode()
        {
            CurrentMetaData = OpenNodes.Min;
            var didRemove = OpenNodes.Remove(CurrentMetaData);
            Debug.Assert(didRemove);
            ClosedCount++;
            CurrentMetaData.Status = NodeStatus.Closed;
            if (CurrentMetaData.ToCost < _closest.ToCost)
                _closest = CurrentMetaData;
        }

        private void ProcessNeighbors()
        {
            var neighbors = Current.GetReachableNodes();
            foreach (var neighbor in neighbors)
            {
                if (neighbor == null) throw new ArgumentNullException(nameof(neighbor), "Neighbors can not be null");
                var neighborMetaData = GetMeta((T) neighbor);
                if (neighborMetaData.Status == NodeStatus.Closed) continue;
                if (CurrentMetaData.Equals(neighborMetaData)) continue;
                if (_nodeValidator != null && _nodeValidator(Current, neighborMetaData.Node)) continue;
                ProcessNeighbor(neighborMetaData);
                if (!neighbor.Equals(Destination)) continue;
                State = SolverState.Success;
                return;
            }
        }

        private IList<T> GetPath()
        {
            if (State != SolverState.Success) return null;
            if (_path != null) return _path;
            return _path ??= BuildPath(Destination);
        }

        private double GetCost()
        {
            if (State != SolverState.Success) return -1;
            if (_cost.HasValue) return _cost.Value;
            
            _cost = 0d;
            var last = Path!.First();

            foreach (var next in Path.Skip(1))
            {
                _cost += last.RealCostTo(next);
                last = next;
            }

            return _cost.Value;
        }

        private IList<T> BuildPath(T node)
        {
            if (Origin.Equals(node)) return new List<T> {node};
            
            var path = new List<T>();

            var cMeta = GetMeta(node);

            while (true)
            {
                path.Add(cMeta.Node);
                cMeta = cMeta.Parent;
                if (cMeta == null || Origin.Equals(cMeta.Node)) break;
            }

            path.Reverse();
            return path;
        }
    }
    
    public enum NodeStatus
    {
        New,
        Open,
        Closed
    }

    public class GraphNodeMetaData<T> : IEquatable<GraphNodeMetaData<T>> where T : IGraphNode
    {
        public readonly T Node;
        public readonly long NodeId;
        public double FromCost;
        [CanBeNull]
        public GraphNodeMetaData<T> Parent;
        public double ToCost;
        public double TotalCost;
        public NodeStatus Status;

        public GraphNodeMetaData(T obj, long nodeId)
        {
            Node = obj;
            NodeId = nodeId;
            Status = NodeStatus.New;
        }
        public override bool Equals(object obj) => obj is GraphNodeMetaData<T> n && Equals(n);
        public bool Equals(GraphNodeMetaData<T> other) => other != null && Node.Equals(other.Node);
        public override int GetHashCode() => Node.GetHashCode();
        public override string ToString() => $"GraphNode[{Node.ToString()}]";
    }
}