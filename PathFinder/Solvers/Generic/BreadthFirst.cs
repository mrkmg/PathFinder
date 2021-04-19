using System;
using System.Collections.Generic;
using System.Diagnostics;
using PathFinder.Graphs;
using PathFinder.Supplement;

namespace PathFinder.Solvers.Generic
{
    /// <summary>
    /// Convenience methods for the <see cref="BreadthFirst{T}"/> graph solver.
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
        /// <para>The <see cref="INodeTraverser{T}"/> implementation to use when traversing the graph</para>
        /// <para>-or-</para>
        /// <para><c>null</c> to use the <see cref="ITraversableNode{T}"/> implementation on each node.</para>
        /// </param>
        /// <param name="maxTicks">The maximum number of ticks to run before failing.</param>
        /// <returns>The <see cref="SolverState"/> of the solver after running.</returns>
        /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableNode{T}"/> if traverser is null.</typeparam>
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
    /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableNode{T}"/> if traverser is null.</typeparam>
    public sealed class BreadthFirst<T> : SolverBase<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Finds a path between the origin and destination node using the Breadth First method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">
        /// <para>The <see cref="INodeTraverser{T}"/> implementation to use when traversing the graph</para>
        /// <para>-or-</para>
        /// <para><c>null</c> to use the <see cref="ITraversableNode{T}"/> implementation on each node.</para>
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
        /// <para>The <see cref="INodeTraverser{T}"/> implementation to use when traversing the graph</para>
        /// <para>-or-</para>
        /// <para><c>null</c> to use the <see cref="ITraversableNode{T}"/> implementation on each node.</para>
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