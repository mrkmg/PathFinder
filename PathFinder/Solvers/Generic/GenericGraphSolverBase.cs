using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PathFinder.Solvers.Generic
{
    
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="T"><see cref="IGraphNode"/></typeparam>
    public abstract class GenericGraphSolverBase<T> : IGraphSolver<T> where T : IGraphNode
    {

        protected GenericGraphSolverBase(IComparer<NodeMetaData<T>> comparer, T origin, T destination)
        {
            Origin = origin;
            Destination = destination;
            _currentMetaData = _meta.Get(origin);
            _currentMetaData.ToCost = origin.EstimatedCostTo(Destination);
            _closest = _currentMetaData;
            _comparer = comparer;
            _openNodes = new SortedSet<NodeMetaData<T>>(comparer) {_currentMetaData};
        }

        protected GenericGraphSolverBase(IComparer<NodeMetaData<T>> comparer, T origin, T destination, NodeValidator nodeValidator) 
            : this(comparer, origin, destination)
        {
            _nodeValidator = nodeValidator;
        }
        
        protected SortedSet<NodeMetaData<T>> _openNodes;
        protected readonly NodeMetaDataStore<T> _meta = new NodeMetaDataStore<T>();
        private readonly NodeValidator _nodeValidator;
        private IList<T> _path;
        protected NodeMetaData<T> _currentMetaData;
        protected readonly IComparer<NodeMetaData<T>> _comparer;
        private NodeMetaData<T> _closest;
        private double? _cost;
        
        /// <inheritdoc cref="IGraphSolver{T}.Open"/>
        public IEnumerable<T> Open => _openNodes.Select(n => n.Node);

        /// <inheritdoc cref="IGraphSolver{T}.Closed"/>
        public IEnumerable<T> Closed => _meta
            .Where(n => n.Status == NodeStatus.Closed)
            .Select(n => n.Node);

        /// <inheritdoc cref="IGraphSolver{T}.OpenCount"/>
        public int OpenCount => _openNodes.Count;
        
        /// <inheritdoc cref="IGraphSolver{T}.ClosedCount"/>
        public int ClosedCount { get; private set; }
        
        /// <inheritdoc cref="IGraphSolver{T}.Current"/>
        public T Current => _currentMetaData.Node;

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

        private int _remainingTicks;
        
        private void Tick()
        {
            if (_openNodes.Count == 0)
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
                throw new Exception("Not in a startable state");
            if (numTicks == 0) return State;
            _remainingTicks = numTicks;
            State = SolverState.Running;
            while (State == SolverState.Running) 
                Tick();
            return State;
        }

        public Task<SolverState> StartAsync(int ticks = -1) => new Task<SolverState>(() => Start(ticks));

        private void SetCurrentNode()
        {
            _currentMetaData = _openNodes.Min;
            var didRemove = _openNodes.Remove(_currentMetaData);
            Debug.Assert(didRemove);
            ClosedCount++;
            _currentMetaData.Status = NodeStatus.Closed;
            if (_currentMetaData.ToCost < _closest.ToCost)
                _closest = _currentMetaData;
        }

        private void ProcessNeighbors()
        {
            var neighbors = Current.GetReachableNodes();
            foreach (var neighbor in neighbors)
            {
                if (neighbor == null) throw new ArgumentNullException(nameof(neighbor), "Neighbors can not be null");
                var neighborMetaData = _meta.Get((T) neighbor);
                if (neighborMetaData.Status == NodeStatus.Closed) continue;
                if (_nodeValidator != null && _nodeValidator(Current, neighborMetaData.Node)) continue;
                ProcessNeighbor(neighborMetaData);
                if (!neighbor.Equals(Destination)) continue;
                State = SolverState.Success;
                return;
            }
        }

        protected abstract void ProcessNeighbor(NodeMetaData<T> neighborMetaData);

        private IList<T> GetPath()
        {
            if (State != SolverState.Success) return null;
            return _path ??= BuildPath(Destination);
        }

        private double GetCost()
        {
            if (State != SolverState.Success) return 0;
            return _cost ??= CalcCost();
        }

        private IList<T> BuildPath(T cNode)
        {
            if (Origin.Equals(cNode)) return new List<T> {cNode};
            
            var path = new List<T>();

            while (true)
            {
                path.Add(cNode);
                cNode = _meta.Get(cNode).Parent;
                if (Origin.Equals(cNode) || cNode == null) break;
            }

            path.Reverse();
            return path;
        }

        private double CalcCost()
        {
            var cost = 0d;
            var last = Path.First();

            foreach (var next in Path.Skip(1))
            {
                cost += last.RealCostTo(next);
                last = next;
            }

            return cost;
        }
    }
    
    public enum NodeStatus
    {
        New,
        Open,
        Closed
    }

    public class NodeMetaData<T> : IEqualityComparer<NodeMetaData<T>>, IEquatable<NodeMetaData<T>> where T : IGraphNode
    {
        public readonly T Node;
        public readonly long NodeId;
        public double FromCost;
        public T Parent;
        public double ToCost;
        public double TotalCost;
        public NodeStatus Status;

        public NodeMetaData(T obj, int nodeId)
        {
            Node = obj;
            NodeId = nodeId;
            Status = NodeStatus.New;
        }
        public int GetHashCode(NodeMetaData<T> obj) => Node.GetHashCode(obj.Node);
        public bool Equals(NodeMetaData<T> x, NodeMetaData<T> y) => y != null && x != null && x.Equals(y);
        public override bool Equals(object obj) => obj is NodeMetaData<T> n && Equals(n);
        public bool Equals(NodeMetaData<T> other) => other != null && Node.Equals(other.Node);
        public override int GetHashCode() => Node.GetHashCode();
        public override string ToString() => Node.ToString();
    }
    
    public class NodeMetaDataStore<T> : IEnumerable<NodeMetaData<T>> where T : IGraphNode
    {
        private readonly Dictionary<T, NodeMetaData<T>> _nodeMetaData = new Dictionary<T, NodeMetaData<T>>();
        private int currentNodeId;

        public NodeMetaData<T> Get(T node) => _nodeMetaData.ContainsKey(node) ? _nodeMetaData[node] : BuildNewNodeMeta(node);

        private NodeMetaData<T> BuildNewNodeMeta(T node)
        {
            var nodeMeta = new NodeMetaData<T>(node, currentNodeId++);
            _nodeMetaData.Add(node, nodeMeta);
            return nodeMeta;
        }

        IEnumerator<NodeMetaData<T>> IEnumerable<NodeMetaData<T>>.GetEnumerator()
        {
            return (IEnumerator<NodeMetaData<T>>) GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable) _nodeMetaData.Values).GetEnumerator();
        }
    }
}