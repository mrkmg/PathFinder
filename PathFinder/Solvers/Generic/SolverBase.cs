using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using PathFinder.Graphs;
using PathFinder.Supplement;

namespace PathFinder.Solvers.Generic
{
    /// <summary>
    /// The base of the Generic Solvers, which handles all the common functionality.
    /// </summary>
    /// <typeparam name="T">The type of the nodes to traverse.</typeparam>
    public abstract class SolverBase<T> : IGraphSolver<T>, IComparer<GraphNodeMetaData<T>> where T : IEquatable<T>
    {
        [DocsHidden]
        protected SolverBase(T origin, T destination, INodeTraverser<T> traverser = null)
        {
            if (origin == null) throw new ArgumentNullException(nameof(origin));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (traverser == null && origin is ITraversableGraphNode<T> && destination is ITraversableGraphNode<T>)
            {
                var defaultTraverserType = typeof(DefaultTraverser<>).MakeGenericType(origin.GetType());
                traverser = (INodeTraverser<T>) Activator.CreateInstance(defaultTraverserType);
            }
            Origin = origin;
            Destination = destination;
            Traverser = traverser ?? throw new ArgumentException("Either Traverser needs to be passed, or T must be an ITraversableGraphNode", nameof(traverser));
            // create the origin node metadata manually as the "GetMeta" method
            // needs to use the CurrentMetaData
            CurrentMetaData = new GraphNodeMetaData<T>(origin, 0)
            {
                ToCost = Traverser.EstimatedCost(origin, Destination),
                PathLength = 0,
            };
            OpenNodes = new C5.IntervalHeap<GraphNodeMetaData<T>>(this) {CurrentMetaData};
            _closest = CurrentMetaData;
            // set to 1 because the origin is node 0
            _nextGraphNodeId = 1;
        }
        
        /// <inheritdoc cref="IGraphSolver{T}.Open"/>
        public IEnumerable<T> Open => OpenNodes.Select(n => n.Node);

        /// <inheritdoc cref="IGraphSolver{T}.Closed"/>
        public IEnumerable<T> Closed => MetaDict.Values
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
        
        [DocsHidden]
        protected C5.IntervalHeap<GraphNodeMetaData<T>> OpenNodes;
        [DocsHidden]
        protected readonly Dictionary<T, GraphNodeMetaData<T>> MetaDict = new Dictionary<T, GraphNodeMetaData<T>>();
        [DocsHidden]
        protected GraphNodeMetaData<T> CurrentMetaData;
        [DocsHidden]
        protected readonly INodeTraverser<T> Traverser;
        
        private IList<T> _path;
        private GraphNodeMetaData<T> _closest;
        private double? _cost;
        private long _nextGraphNodeId;
        private int _remainingTicks;
        
        private void Tick()
        {
            // there is nothing left to search
            if (OpenNodes.Count == 0)
                State = SolverState.Failure;
            // the current "start" ran out of remaining ticks and we did not succeed or fail
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
            
            if (Current.Equals(Destination))
                State = SolverState.Success;
            else
                ProcessNeighbors();
        }
        
        public void Stop() => _remainingTicks = 0;
        
        public SolverState Run(int numTicks = -1)
        {
            if (State != SolverState.Waiting)
                throw new InvalidOperationException("Graph Solver is not in a startable state");
            _remainingTicks = numTicks;
            State = SolverState.Running;
            while (State == SolverState.Running) 
                Tick();
            return State;
        }
        
        [DocsHidden]
        protected virtual void ProcessNeighbor(GraphNodeMetaData<T> neighborMetaData)
        {
            if (neighborMetaData.Status != NodeStatus.New) return;
            neighborMetaData.Status = NodeStatus.Open;
            var didAdd = OpenNodes.Add(ref neighborMetaData.Handle, neighborMetaData);
            Debug.Assert(didAdd, "Failed to add neighborNode to open nodes list. The comparer is probably invalid.");
        }

        [NotNull, DocsHidden]
        protected GraphNodeMetaData<T> GetMeta(T node, bool onlyExisting = false)
        {
            if (MetaDict.TryGetValue(node, out var meta)) return meta;
            if (onlyExisting) throw new InvalidOperationException($"MetaDict data not found for {node}");
            var fromCost = Traverser.RealCost(CurrentMetaData.Node, node);
            meta = new GraphNodeMetaData<T>(node, _nextGraphNodeId++)
            {
                ToCost = Traverser.EstimatedCost(node, Destination),
                FromCost = CurrentMetaData.FromCost + fromCost,
                Parent = CurrentMetaData,
                PathLength = CurrentMetaData.PathLength + 1
            };
            MetaDict.Add(node, meta);
            // If the fromCost has a negative value, it is not actually traversable so
            // close the node to prevent it from being searched further.
            // TODO determine if this should actually be the case
            if (fromCost < 0) meta.Status = NodeStatus.Closed;
            return meta;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCurrentNode()
        {
            // get the best node, remove from open nodes, set to closed
            CurrentMetaData = OpenNodes.DeleteMin();
            ClosedCount++;
            CurrentMetaData.Status = NodeStatus.Closed;
            // check if this node is closest to the target
            if (CurrentMetaData.ToCost < _closest.ToCost)
                _closest = CurrentMetaData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessNeighbors()
        {
            foreach (var neighbor in Traverser.TraversableNodes(Current))
            {
                if (neighbor == null) throw new ArgumentNullException(nameof(neighbor), "Neighbors can not be null");
                var neighborMetaData = GetMeta(neighbor);
                // do not check already checked nodes
                if (neighborMetaData.Status == NodeStatus.Closed) continue;
                // if a node somehow is connected to itself, do not check
                // this should never be reach as it should be closed already
                if (CurrentMetaData.Equals(neighborMetaData)) continue;
                ProcessNeighbor(neighborMetaData);
            }
        }

        private IList<T> GetPath()
        {
            if (State != SolverState.Success) return null;
            return _path ??= BuildPath(Destination);
        }

        
        // used for both the "best path" and the final path
        private IList<T> BuildPath(T node)
        {
            if (Origin.Equals(node)) return new T[0];

            // iterate from the 'node' back to the origin.
            // add them from the back to the front, so the
            // returned path starts at the origin. 
            
            var cMeta = GetMeta(node, true);
            var path = new T[cMeta.PathLength];
            var pathIndex = cMeta.PathLength - 1;
            do
            {
                path[pathIndex--] = cMeta!.Node;
                Debug.Assert(cMeta.Parent != null, "cMeta != null while building path");
                Debug.Assert(!cMeta.Node.Equals(Origin));
                cMeta = cMeta.Parent;
            } while (pathIndex >= 0);
            return path;
        }

        private double GetCost()
        {
            if (State != SolverState.Success) return -1;
            return _cost ??= GetMeta(Destination, true).FromCost;
        }

        public abstract int Compare(GraphNodeMetaData<T> x, GraphNodeMetaData<T> y);
    }

    [DocsHidden]
    public enum NodeStatus
    {
        New,
        Open,
        Closed
    }

    [DocsHidden]
    public class GraphNodeMetaData<T> : IEquatable<GraphNodeMetaData<T>> where T : IEquatable<T>
    {
        public readonly T Node;
        public readonly long NodeId;
        public double FromCost;
        [CanBeNull] public GraphNodeMetaData<T> Parent;
        public double ToCost;
        public double TotalCost;
        public NodeStatus Status;
        public int PathLength;
        public C5.IPriorityQueueHandle<GraphNodeMetaData<T>> Handle;

        public GraphNodeMetaData(T obj, long nodeId)
        {
            Node = obj;
            NodeId = nodeId;
            Status = NodeStatus.New;
        }
        public override bool Equals(object obj) => obj is GraphNodeMetaData<T> n && Equals(n);
        public bool Equals(GraphNodeMetaData<T> other) => other != null && Node.Equals(other.Node);
        public override int GetHashCode() => Node.GetHashCode();
        public override string ToString() => $"GraphNodeMeta<{Status}>[{Node.ToString()}]";
    }
}