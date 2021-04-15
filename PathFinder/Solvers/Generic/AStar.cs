using System;
using System.Collections.Generic;
using System.Diagnostics;
using PathFinder.Graphs;

namespace PathFinder.Solvers.Generic
{
    public static class AStar
    {
        /// <summary>
        ///     Finds a path between the origin and destination node using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public static SolverState Solve<T>(T origin, T destination, out IList<T> path, INodeTraverser<T> traverser = null) 
            where T : ITraversableGraphNode<T> 
            => AStar<T>.Solve(origin, destination, out path, traverser);

        /// <summary>
        ///     Finds a path between the origin and destination node using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="AStar{T}.GreedFactor"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public static SolverState Solve<T>(T origin, T destination, double greedFactor, out IList<T> path, INodeTraverser<T> traverser = null)
            where T : ITraversableGraphNode<T>
            => AStar<T>.Solve(origin, destination, greedFactor, out path, traverser);
    }
    
    /// <summary>
    /// A* is a graph solver to find the cheapest path between two nodes which
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AStar<T> : SolverBase<T> where T : ITraversableGraphNode<T>
    {
        /// <summary>
        ///     Finds a path between the origin and destination node using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public static SolverState Solve(T origin, T destination, out IList<T> path, INodeTraverser<T> traverser = null)
        {
            var solver = new AStar<T>(origin, destination, traverser);
            solver.Start();
            path = solver.State == SolverState.Success ? solver.Path : null;
            return solver.State;
        }
        
        /// <summary>
        ///     Finds a path between the origin and destination node using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="AStar{T}.GreedFactor"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public static SolverState Solve(T origin, T destination, double greedFactor, out IList<T> path, INodeTraverser<T> traverser = null)
        {
            var solver = new AStar<T>(origin, destination, greedFactor, traverser);
            solver.Start();
            path = solver.State == SolverState.Success ? solver.Path : null;
            return solver.State;
        }

        /// <summary>
        ///     Creates a solver using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public AStar(T origin, T destination, INodeTraverser<T> traverser = null) 
            : base(new NodeMetaComparer(), origin, destination, traverser) { }

        /// <summary>
        ///     Creates a solver using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="AStar{T}.GreedFactor"/></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public AStar(T origin, T destination, double greedFactor, INodeTraverser<T> traverser = null)
            : base(new NodeMetaComparer(), origin, destination, traverser)
        {
            _greedFactor = greedFactor;
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
                var fromCost = CurrentMetaData.FromCost + Traverser.RealCost(CurrentMetaData.Node, neighborMetaData.Node);
                if (fromCost >= neighborMetaData.FromCost) return;
                OpenNodes.Delete(neighborMetaData.Handle);
                neighborMetaData.Parent = CurrentMetaData;
                neighborMetaData.PathLength = CurrentMetaData.PathLength + 1;
                neighborMetaData.FromCost = fromCost;
            }
            neighborMetaData.Status = NodeStatus.Open;
            neighborMetaData.TotalCost = neighborMetaData.FromCost + neighborMetaData.ToCost * _greedFactor;
            var didAdd = OpenNodes.Add(ref neighborMetaData.Handle, neighborMetaData);
            Debug.Assert(didAdd, "Failed to add neighborNode to open nodes list. The comparer is probably invalid.");
        }

        private class NodeMetaComparer : Comparer<GraphNodeMetaData<T>>
        {
            
            /**
             * For aStar, we want the node with a lowest total cost first
             * If total costs are the same, then the node which has a lower "to cost"
             * if to costs are the same, use the latest found node
             */
            public override int Compare(GraphNodeMetaData<T> x, GraphNodeMetaData<T> y)
            {
                Debug.Assert(x != null && y != null, "Graph Nodes should never be null");
                return x.TotalCost <  y.TotalCost ? -1
                     : x.TotalCost >  y.TotalCost ?  1
                     : x.ToCost    <  y.ToCost    ? -1
                     : x.ToCost    >  y.ToCost    ?  1
                     : x.NodeId    == y.NodeId    ?  0
                     : x.NodeId    <  y.NodeId    ?  1
                     :                              -1;
            }
        }
    }
}