using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PathFinder.Solvers.Generic
{
    public static class AStar
    {
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        /// <param name="greedFactor"><see cref="AStar{T}.GreedFactor"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve<T>(T origin, T destination, NodeValidator nodeValidator, double greedFactor, out IList<T> path) where T : IGraphNode 
            => AStar<T>.Solve(origin, destination, nodeValidator, greedFactor, out path);

        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve<T>(T origin, T destination, out IList<T> path) where T : IGraphNode
            => AStar<T>.Solve(origin, destination, 1, out path);

        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve<T>(T origin, T destination, NodeValidator nodeValidator, out IList<T> path) where T : IGraphNode
            => AStar<T>.Solve(origin, destination, nodeValidator, 1, out path);
        
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="AStar{T}.GreedFactor"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve<T>(T origin, T destination, double greedFactor, out IList<T> path) where T : IGraphNode
            => AStar<T>.Solve(origin, destination, null, greedFactor, out path);
    }
    
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
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve(T origin, T destination, double greedFactor, out IList<T> path)
            => Solve(origin, destination, null, greedFactor, out path);

        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        public AStar(T origin, T destination, double greedFactor = 1) 
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
            foreach (var nodeMetaData in Meta.Values)
            {
                nodeMetaData.Status = NodeStatus.Open;
            }
            CurrentMetaData = GetMeta(Origin);
            OpenNodes = new SortedSet<GraphNodeMetaData<T>>(Meta.Values, Comparer) {CurrentMetaData};
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

        private double _greedFactor = 1;

        protected override void ProcessNeighbor(GraphNodeMetaData<T> neighborMetaData)
        {
            
            if (neighborMetaData.Status == NodeStatus.Open)
            {
                var fromCost = CurrentMetaData.FromCost + CurrentMetaData.Node.RealCostTo(neighborMetaData.Node);
                if (fromCost >= neighborMetaData.FromCost) return;
                OpenNodes.Remove(neighborMetaData);
                neighborMetaData.Parent = CurrentMetaData;
                neighborMetaData.FromCost = fromCost;
            }
            neighborMetaData.Status = NodeStatus.Open;
            neighborMetaData.TotalCost = neighborMetaData.FromCost + neighborMetaData.ToCost * _greedFactor;
            var didAdd = OpenNodes.Add(neighborMetaData);
            Debug.Assert(didAdd, "Failed to add neighborNode to open nodes list. The comparer is probably invalid.");
        }

        private class NodeMetaComparer : Comparer<GraphNodeMetaData<T>>
        {
            public override int Compare(GraphNodeMetaData<T> x, GraphNodeMetaData<T> y)
            {
                Debug.Assert(x != null && y != null, "Graph Nodes should never be null");
                return x.TotalCost <  y.TotalCost ? -1 
                     : x.TotalCost >  y.TotalCost ?  1 
                     : x.FromCost  <  y.FromCost  ? -1
                     : x.FromCost  >  y.FromCost  ?  1
                     : x.ToCost    <  y.ToCost    ? -1
                     : x.ToCost    >  y.ToCost    ?  1
                     : x.NodeId    == y.NodeId    ?  0
                     : x.NodeId    <  y.NodeId    ? -1 
                     :                               1;
            }
        }
    }
}