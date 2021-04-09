using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using PathFinder.Components;
using PathFinder.Interfaces;

namespace PathFinder.Solvers
{
    public abstract class AStarCore<T> : ISolver<T> where T : INode
    {
        public AStarCore(IComparer<NodeMetaData<T>> comparer, T origin, T destination)
        {
            Origin = origin;
            Destination = destination;
            _currentMetaData = _meta.Get(origin);
            _currentMetaData.ToCost = origin.EstimatedCostTo(Destination);
            _closest = _currentMetaData;
            _openNodes = new SortedSet<NodeMetaData<T>>(comparer) {_currentMetaData};
        }
        
        public AStarCore(IComparer<NodeMetaData<T>> comparer, T origin, T destination, Func<T, T, bool> nodeValidator) : this(comparer, origin, destination)
        {
            _nodeValidator = nodeValidator;Origin = origin;
        }
        
        protected readonly SortedSet<NodeMetaData<T>> _openNodes;
        protected readonly NodeMetaDataStore<T> _meta = new NodeMetaDataStore<T>();
        private readonly Func<T, T, bool> _nodeValidator;
        private IList<T> _path;
        protected NodeMetaData<T> _currentMetaData;
        private NodeMetaData<T> _closest;
        private double? _cost;
        
        /// <inheritdoc cref="ISolver{T}.Open"/>
        public IEnumerable<T> Open => _openNodes.Select(n => n.Node);

        /// <inheritdoc cref="ISolver{T}.Closed"/>
        public IEnumerable<T> Closed => _meta
            .Where(n => n.Status == NodeStatus.Closed)
            .Select(n => n.Node);

        /// <inheritdoc cref="ISolver{T}.OpenCount"/>
        public int OpenCount => _openNodes.Count;
        
        /// <inheritdoc cref="ISolver{T}.ClosedCount"/>
        public int ClosedCount { get; private set; }
        
        /// <inheritdoc cref="ISolver{T}.Current"/>
        public T Current => _currentMetaData.Node;

        /// <inheritdoc cref="ISolver{T}.PathCost"/>
        public double PathCost => GetCost();

        /// <inheritdoc cref="ISolver{T}.State"/>
        public SolverState State { get; private set; } = SolverState.Running;

        /// <inheritdoc cref="ISolver{T}.Ticks"/>
        public int Ticks { get; private set; }

        /// <inheritdoc cref="ISolver{T}.Origin"/>
        public T Origin { get; }

        /// <inheritdoc cref="ISolver{T}.Destination"/>
        public T Destination { get; }
        
        /// <inheritdoc cref="ISolver{T}.Path"/>
        public IList<T> Path => GetPath();

        /// <inheritdoc cref="ISolver{T}.CurrentBestPath"/>
        public IList<T> CurrentBestPath => BuildPath(_closest.Node);

        /// <summary>
        /// The Maximum number of ticks to run until failing.
        /// </summary>
        public int MaxTicks { get; set; } = 0x1111111;
        
        public void Tick()
        {
            if (State != SolverState.Running) throw new Exception("Not running");

            if (_openNodes.Count == 0 || Ticks > MaxTicks)
            {
                State = SolverState.Stopped;
                return;
            }

            Ticks++;

            ProcessNeighbors();

            if (Current.Equals(Destination))
                State = SolverState.Success;
            else if (_openNodes.Count == 0)
                State = SolverState.Failure;
            else
                SetCurrentNode();
        }

        
        public void Stop()
        {
            State = SolverState.Stopped;
        }

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
                if (neighborMetaData.Equals(_currentMetaData)) continue;
                if (_nodeValidator != null && _nodeValidator(Current, neighborMetaData.Node)) continue;
                ProcessNeighbor(neighborMetaData);
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
}