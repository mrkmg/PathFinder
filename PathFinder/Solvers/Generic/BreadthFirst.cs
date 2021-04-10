using System.Collections.Generic;
using System.Diagnostics;

namespace PathFinder.Solvers.Generic
{
    public sealed class BreadthFirst<T> : GenericSolverBase<T> where T : INode
    {
        /// <summary>
        ///     Finds a path between the origin and destination node.
        /// </summary>
        /// <param name="origin"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericSolverBase{T}.Destination"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve(T origin, T destination, NodeValidator nodeValidator, out IList<T> path)
        {
            var solver = new BreadthFirst<T>(origin, destination, nodeValidator);
            solver.Start();
            path = solver.State == SolverState.Success ? solver.Path : null;
            return solver.State;
        }

        /// <summary>
        ///     Finds a path between the origin and destination node.
        /// </summary>
        /// <param name="origin"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="path">The resulting path if <see cref="SolverState.Success"/>, otherwise <c>null</c></param>
        public static SolverState Solve(T origin, T destination, out IList<T> path)
            => Solve(origin, destination, null, out path);
 
        /// <summary>
        ///     Creates a solver using the Breadth-First method.
        /// </summary>
        /// <param name="origin"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericSolverBase{T}.Origin"/></param>
        public BreadthFirst(T origin, T destination) 
            : base(new NodeMetaComparer(), origin, destination) { }
        
        /// <summary>
        ///     Creates a solver using the Breadth-First method.
        /// </summary>
        /// <param name="origin"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        public BreadthFirst(T origin, T destination, NodeValidator nodeValidator) 
            : base(new NodeMetaComparer(), origin, destination, nodeValidator) { }

        protected override void ProcessNeighbor(NodeMetaData<T> neighborMetaData)
        {
            if (neighborMetaData.Status != NodeStatus.New) return;
            
            Debug.Assert(neighborMetaData.Parent == null);
            neighborMetaData.Parent = Current;
            neighborMetaData.FromCost = _currentMetaData.FromCost + neighborMetaData.Node.RealCostTo(Destination);
            neighborMetaData.Status = NodeStatus.Open;
            _openNodes.Add(neighborMetaData);
        }

        private class NodeMetaComparer : Comparer<NodeMetaData<T>>
        {
            public override int Compare(NodeMetaData<T> x, NodeMetaData<T> y)
            {
                return x.FromCost > y.FromCost ? 1
                    : x.FromCost < y.FromCost ? -1
                    : x.ToCost < y.ToCost ? -1
                    : x.ToCost > y.ToCost ? 1
                    : x.NodeId == y.NodeId ? 0
                    : x.NodeId > y.NodeId ? 1 
                    : -1;
            }
        }
    }
}