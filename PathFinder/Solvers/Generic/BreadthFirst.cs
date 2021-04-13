using System.Collections.Generic;
using System.Diagnostics;

namespace PathFinder.Solvers.Generic
{
    public sealed class BreadthFirst<T> : SolverBase<T> where T : ITraversableGraphNode<T>
    {
        /// <summary>
        ///     Finds a path between the origin and destination node using the Breadth First method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public static SolverState Solve(T origin, T destination, out IList<T> path, INodeTraverser<T> traverser = null)
        {
            var solver = new BreadthFirst<T>(origin, destination, traverser);
            solver.Start();
            path = solver.State == SolverState.Success ? solver.Path : null;
            return solver.State;
        }

        /// <summary>
        ///     Creates a solver using the Breadth First method.
        /// </summary>
        /// <param name="origin"><see cref="SolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="SolverBase{T}.Destination"/></param>
        /// <param name="traverser">Use <see cref="INodeTraverser{T}"/> to traverse the graph instead of<see cref="ITraversableGraphNode{T}"/>'s default traversing.</param>
        public BreadthFirst(T origin, T destination, INodeTraverser<T> traverser = null) 
            : base(new NodeMetaComparer(), origin, destination, traverser) { }

        private class NodeMetaComparer : Comparer<GraphNodeMetaData<T>>
        {
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
}