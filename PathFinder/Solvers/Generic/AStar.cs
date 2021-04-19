using System;
using System.Collections.Generic;
using System.Diagnostics;
using PathFinder.Graphs;
using PathFinder.Supplement;

namespace PathFinder.Solvers.Generic
{
    /// <summary>
    /// Convenience methods for the A* graph solver. See <see cref="AStar{T}"/>.
    /// </summary>
    public static class AStar
    {
        /// <summary>
        /// Finds a path between the origin and destination node using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">
        /// Use the passed <see cref="INodeTraverser{T}"/> to traverse the graph.
        /// If no traverser if passed, T must extend <see cref="ITraversableGraphNode{T}"/>.
        /// </param>
        /// <returns>The <see cref="SolverState"/> of the solver after running.</returns>
        /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableGraphNode{T}"/> if traverser is null.</typeparam>
        public static SolverState Solve<T>(T origin, T destination, out IList<T> path, INodeTraverser<T> traverser = null) 
            where T : IEquatable<T> 
            => AStar<T>.Solve(origin, destination, out path, traverser);

        /// <summary>
        /// Finds a path between the origin and destination node using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="AStar{T}.GreedFactor"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">
        /// Use the passed <see cref="INodeTraverser{T}"/> to traverse the graph.
        /// If no traverser if passed, T must extend <see cref="ITraversableGraphNode{T}"/>.
        /// </param>
        /// <returns>The <see cref="SolverState"/> of the solver after running.</returns>
        /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableGraphNode{T}"/> if traverser is null.</typeparam>
        public static SolverState Solve<T>(T origin, T destination, double greedFactor, out IList<T> path, INodeTraverser<T> traverser = null)
            where T : IEquatable<T>
            => AStar<T>.Solve(origin, destination, greedFactor, out path, traverser);
    }
    
    /// <summary>
    /// A Graph Solver using the A* method, with a configurable <see cref="GreedFactor"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The A* method is considered the most optimal "blind" graph solver, but only in cases where
    /// the distance between arbitrary nodes can be estimated.
    /// </para>
    /// <para>
    /// The <c>GreedFactor</c> determines how much weight is given to the distance estimate to
    /// the <c>Destination</c>.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableGraphNode{T}"/> if traverser is null.</typeparam>
    public sealed class AStar<T> : SolverBase<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Finds a path between the origin and destination node using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">
        /// Use the passed <see cref="INodeTraverser{T}"/> to traverse the graph.
        /// If no traverser if passed, T must extend <see cref="ITraversableGraphNode{T}"/>.
        /// </param>
        /// <param name="maxTicks">The maximum number of ticks to run before failing.</param>
        /// <returns>The <see cref="SolverState"/> of the solver after running.</returns>
        public static SolverState Solve(T origin, T destination, out IList<T> path, INodeTraverser<T> traverser = null, int maxTicks = 1000000)
        {
            var solver = new AStar<T>(origin, destination, traverser)
            {
                MaxTicks = maxTicks
            };
            solver.Run();
            path = solver.State == SolverState.Success ? solver.Path : null;
            return solver.State;
        }
        
        /// <summary>
        /// Finds a path between the origin and destination node using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="AStar{T}.GreedFactor"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">
        /// Use the passed <see cref="INodeTraverser{T}"/> to traverse the graph.
        /// If no traverser if passed, T must extend <see cref="ITraversableGraphNode{T}"/>.
        /// </param>
        /// <param name="maxTicks">The maximum number of ticks to run before failing.</param>
        /// <returns>The <see cref="SolverState"/> of the solver after running.</returns>
        public static SolverState Solve(T origin, T destination, double greedFactor, out IList<T> path, INodeTraverser<T> traverser = null, int maxTicks = 1000000)
        {
            var solver = new AStar<T>(origin, destination, greedFactor, traverser)
            {
                MaxTicks = maxTicks
            };
            solver.Run();
            path = solver.State == SolverState.Success ? solver.Path : null;
            return solver.State;
        }

        /// <summary>
        /// Creates a solver using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="traverser">
        /// Use the passed <see cref="INodeTraverser{T}"/> to traverse the graph.
        /// If no traverser if passed, T must extend <see cref="ITraversableGraphNode{T}"/>.
        /// </param>
        public AStar(T origin, T destination, INodeTraverser<T> traverser = null) 
            : base(origin, destination, traverser) { }

        /// <summary>
        /// Creates a solver using the AStar method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="AStar{T}.GreedFactor"/></param>
        /// <param name="traverser">
        /// Use the passed <see cref="INodeTraverser{T}"/> to traverse the graph.
        /// If no traverser if passed, T must extend <see cref="ITraversableGraphNode{T}"/>.
        /// </param>
        public AStar(T origin, T destination, double greedFactor, INodeTraverser<T> traverser = null)
            : base(origin, destination, traverser)
        {
            _greedFactor = greedFactor;
        }

        /// <summary>
        /// <para>How thorough the search should be.</para>
        /// <para>Must be greater than or equal to 0.</para>
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>0 is breadth-first search.</item>
        /// <item>1 is traditional A*.</item>
        /// <item>>1 favors obtaining the goal over finding the cheapest path.</item>
        /// </list>
        /// </remarks>
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
            // in astar, we can recheck an open node to see if the new path is cheaper to get to it
            if (neighborMetaData.Status == NodeStatus.Open)
            {
                // recalculate the from cost of the neighbor if coming from the current node
                var fromCost = CurrentMetaData.FromCost + Traverser.RealCost(CurrentMetaData.Node, neighborMetaData.Node);
                // only replace the neighbor's parent if it is cheaper to get to it from the current node
                if (fromCost > neighborMetaData.FromCost) return;
                // remove from open nodes as it will be re-added later with new costs
                OpenNodes.Delete(neighborMetaData.Handle);
                // set the new data on the neighbor to reflect its new parent
                neighborMetaData.Parent = CurrentMetaData;
                neighborMetaData.PathLength = CurrentMetaData.PathLength + 1;
                neighborMetaData.FromCost = fromCost;
            }
            neighborMetaData.Status = NodeStatus.Open;
            neighborMetaData.TotalCost = neighborMetaData.FromCost + neighborMetaData.ToCost * _greedFactor;
            var didAdd = OpenNodes.Add(ref neighborMetaData.Handle, neighborMetaData);
            Debug.Assert(didAdd, "Failed to add neighborNode to open nodes list. The comparer is probably invalid.");
        }
        
        // For aStar, we want the node with a lowest total cost first
        // If total costs are the same, then the node which has a lower "to cost"
        // if to costs are the same, use the latest found node
        [DocsHidden]
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