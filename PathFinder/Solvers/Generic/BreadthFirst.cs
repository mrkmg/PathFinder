using System.Collections.Generic;
using System.Diagnostics;

namespace PathFinder.Solvers.Generic
{
    public sealed class BreadthFirst<T> : GenericGraphSolverBase<T> where T : IGraphNode
    {
        /// <summary>
        ///     Finds a path between the origin and destination node.
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Destination"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve(T origin, T destination, NodeValidator nodeValidator, out IList<T> path)
        {
            var solver = new BreadthFirst<T>(origin, destination, nodeValidator);
            path = solver.Start() == SolverState.Success ? solver.Path : null;
            return solver.State;
        }

        /// <summary>
        ///     Finds a path between the origin and destination node.
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve(T origin, T destination, out IList<T> path)
            => Solve(origin, destination, null, out path);
 
        /// <summary>
        ///     Creates a solver using the Breadth-First method.
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        public BreadthFirst(T origin, T destination) 
            : base(new NodeMetaComparer(), origin, destination) { }
        
        /// <summary>
        ///     Creates a solver using the Breadth-First method.
        /// </summary>
        /// <param name="origin"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericGraphSolverBase{T}.Origin"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        public BreadthFirst(T origin, T destination, NodeValidator nodeValidator) 
            : base(new NodeMetaComparer(), origin, destination, nodeValidator) { }

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