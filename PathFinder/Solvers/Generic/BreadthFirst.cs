using System;
using System.Collections.Generic;
using System.Diagnostics;
using PathFinder.Graphs;
using PathFinder.Supplement;

namespace PathFinder.Solvers.Generic
{
    /// <summary>
    /// Convenience methods for the BreadthFirst graph solver. See <see cref="BreadthFirst{T}"/>.
    /// </summary>
    public static class BreadthFirst
    {
        /// <summary>
        /// Finds a path between the origin and destination node using the Breadth First method.
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
        /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableGraphNode{T}"/> if traverser is null.</typeparam>
        public static SolverState Solve<T>(T origin, T destination, out IList<T> path, INodeTraverser<T> traverser = null, int maxTicks = 1000000)
            where T : IEquatable<T> 
            => BreadthFirst<T>.Solve(origin, destination, out path, traverser, maxTicks);
    }
    
    /// <summary>
    ///  A graph solver which exhaustively searches a graph by only considering the distance searched
    ///  so far.
    /// </summary>
    /// <remarks>
    /// Useful in cases in which a estimation function is not possible or wildly inaccurate.
    /// </remarks>
    /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableGraphNode{T}"/> if traverser is null.</typeparam>
    public sealed class BreadthFirst<T> : SolverBase<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Finds a path between the origin and destination node using the Breadth First method.
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
            var solver = new BreadthFirst<T>(origin, destination, traverser)
            {
                MaxTicks = maxTicks
            };
            solver.Run();
            path = solver.State == SolverState.Success ? solver.Path : null;
            return solver.State;
        }

        /// <summary>
        /// Creates a solver using the Breadth First method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="traverser">
        /// Use the passed <see cref="INodeTraverser{T}"/> to traverse the graph.
        /// If no traverser if passed, T must extend <see cref="ITraversableGraphNode{T}"/>.
        /// </param>
        public BreadthFirst(T origin, T destination, INodeTraverser<T> traverser = null) 
            : base(origin, destination, traverser) { }

        // breadth-first only cares about the from cost
        [DocsHidden]
        public override int Compare(GraphNodeMetaData<T> x, GraphNodeMetaData<T> y)
        {
            Debug.Assert(x != null && y != null, "Graph Nodes should never be null");
            return x.FromCost >  y.FromCost ?  1
                 : x.FromCost <  y.FromCost ? -1
                 : x.NodeId   == y.NodeId   ?  0
                 : x.NodeId   >  y.NodeId   ?  1 
                 :                            -1;
        }
    }
}