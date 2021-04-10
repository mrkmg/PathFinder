using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PathFinder.Solvers.Generic
{
    /// <summary>
    /// A* is a graph solver to find the cheapest path between two nodes which
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AStar<T> : GenericGraphSolverBase<T> where T : IGraphNode
    {
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve(T origin, T destination, NodeValidator nodeValidator, double greedFactor, out IList<T> path)
        {
            var solver = new AStar<T>(origin, destination, nodeValidator, greedFactor);
            path = solver.Start() == SolverState.Success ? solver.Path : null;
            return solver.State;
        }

        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve(T origin, T destination, out IList<T> path)
            => Solve(origin, destination, null, 1, out path);

        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve(T origin, T destination, NodeValidator nodeValidator, out IList<T> path)
            => Solve(origin, destination, nodeValidator, 1, out path);
        
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        public static SolverState Solve(T origin, T destination, double greedFactor, out IList<T> path)
            => Solve(origin, destination, null, greedFactor, out path);

        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        public AStar(T origin, T destination, double greedFactor = 0.5) 
            : base(new NodeMetaComparer(), origin, destination)
        {
            GreedFactor = greedFactor;
        }

        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        public AStar(T origin, T destination, NodeValidator nodeValidator, double greedFactor = 0.5) 
            : base(new NodeMetaComparer(), origin, destination, nodeValidator)
        {
            GreedFactor = greedFactor;
        }

        public void Reset()
        {
            // Todo reset the closed count
            foreach (NodeMetaData<T> nodeMetaData in _meta)
            {
                nodeMetaData.Status = NodeStatus.Open;
            }
            _currentMetaData = _meta.Get(Origin);
            _openNodes = new SortedSet<NodeMetaData<T>>(_meta, _comparer) {_currentMetaData};
            State = SolverState.Waiting;
        }

        /// <summary>
        /// <para>How thorough the search should be.</para>
        /// <para>Must be greater than or equal to 0.</para>
        /// <remarks>
        /// <list type="bullet">
        /// <item>0 is breadth-first search.</item>
        /// <item>1 is traditional A*.</item>
        /// <item>>1 favors obtaining the goal over finding the cheapest path.</item>
        /// </list>
        /// </remarks>
        /// </summary>
        public double GreedFactor
        {
            get => _greedFactor;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _greedFactor = value;
            }
        }

        private double _greedFactor = 0.5;

        protected override void ProcessNeighbor(NodeMetaData<T> neighborMetaData)
        {
            var fromCost = _currentMetaData.FromCost + _currentMetaData.Node.RealCostTo(neighborMetaData.Node);

            if (neighborMetaData.Status == NodeStatus.Open)
            {
                if (fromCost >= neighborMetaData.FromCost) return;
                _openNodes.Remove(neighborMetaData);
            }
            
            Debug.Assert(neighborMetaData.Status != NodeStatus.Closed, "neighborMetaData.Status != NodeStatus.Closed");
            
            neighborMetaData.TotalCost = fromCost + neighborMetaData.ToCost * GreedFactor;
            neighborMetaData.Parent = Current;
            neighborMetaData.FromCost = fromCost;
            _openNodes.Add(neighborMetaData);
        }

        private class NodeMetaComparer : Comparer<NodeMetaData<T>>
        {
            public override int Compare(NodeMetaData<T> x, NodeMetaData<T> y)
            {
                if (ReferenceEquals(x, y)) return 0;
                return x.TotalCost > y.TotalCost ? 1 
                    : x.TotalCost < y.TotalCost ? -1 
                    : x.ToCost > y.ToCost ? 1
                    : x.ToCost < y.ToCost ? -1
                    : x.NodeId == y.NodeId ? 0
                    : x.NodeId > y.NodeId ? 1 
                    : -1;
            }
        }
    }
}