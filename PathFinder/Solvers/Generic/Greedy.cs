using System.Collections.Generic;
using System.Diagnostics;
using PathFinder.Graphs;
using PathFinder.Supplement;

namespace PathFinder.Solvers.Generic
{
    /// <summary>
    /// Helper methods for the Greedy graph solver. See <see cref="Greedy{T}"/>.
    /// </summary>
    public static class Greedy
    {
        /// <summary>
        /// Finds a path between the origin and destination node.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        /// <param name="maxTicks">The maximum number of ticks to run before failing.</param>
        /// <returns>The <see cref="SolverState"/> of the solver after running.</returns>
        /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableGraphNode{T}"/></typeparam>
        public static SolverState Solve<T>(T origin, T destination, out IList<T> path, INodeTraverser<T> traverser = null, int maxTicks = 1000000)
            where T : ITraversableGraphNode<T>
            => Greedy<T>.Solve(origin, destination, out path, traverser, maxTicks);
    }
    
    /// <summary>
    /// A graph solver which only considers the remaining estimated distance to the destination.
    /// </summary>
    /// <remarks>
    /// This solver is useful to determine if a path is possible, or in graphs where there are little to no
    /// obstructions or differences in cost to traverse between nodes.
    /// </remarks>
    /// <typeparam name="T">The type of nodes to traverse. Must extend <see cref="ITraversableGraphNode{T}"/></typeparam>
    public sealed class Greedy<T> : SolverBase<T> where T : ITraversableGraphNode<T>
    {
        /// <summary>
        /// Finds a path between the origin and destination node.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        /// <param name="maxTicks">The maximum number of ticks to run before failing.</param>
        /// <returns>The <see cref="SolverState"/> of the solver after running.</returns>
        public static SolverState Solve(T origin, T destination, out IList<T> path, INodeTraverser<T> traverser = null, int maxTicks = 1000000)
        {
            var solver = new Greedy<T>(origin, destination, traverser)
            {
                MaxTicks = maxTicks
            };
            solver.Run();
            path = solver.State == SolverState.Success ? solver.Path : null;
            return solver.State;
        }

        /// <summary>
        /// Creates a solver using the Greedy method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public Greedy(T origin, T destination, INodeTraverser<T> traverser = null) 
            : base(origin, destination, traverser) { }

        // greedy only really cares about the to cost, but we also check
        // from cost to ensure we are not checking more nodes than needed
        [DocsHidden]
        public override int Compare(GraphNodeMetaData<T> x, GraphNodeMetaData<T> y)
        {
            Debug.Assert(x != null && y != null, "Graph Nodes should never be null");
            return x.ToCost   >  y.ToCost   ?  1
                 : x.ToCost   <  y.ToCost   ? -1
                 : x.FromCost >  y.FromCost ?  1
                 : x.FromCost <  y.FromCost ? -1
                 : x.NodeId   == y.NodeId   ?  0
                 : x.NodeId   >  y.NodeId   ?  1 
                 :                            -1;
        }
    }

    
}