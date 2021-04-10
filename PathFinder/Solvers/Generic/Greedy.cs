using System.Collections.Generic;
using System.Diagnostics;

namespace PathFinder.Solvers.Generic
{
    public sealed class Greedy<T> : GenericSolverBase<T> where T : INode
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
            var solver = new Greedy<T>(origin, destination, nodeValidator);
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
        ///     Creates a solver using the Greedy method.
        /// </summary>
        /// <param name="origin"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericSolverBase{T}.Origin"/></param>
        public Greedy(T origin, T destination) 
            : base(new NodeMetaComparer(), origin, destination) { }
        
        /// <summary>
        ///     Creates a solver using the Greedy method.
        /// </summary>
        /// <param name="origin"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="destination"><see cref="GenericSolverBase{T}.Origin"/></param>
        /// <param name="nodeValidator"><see cref="NodeValidator"/></param>
        public Greedy(T origin, T destination, NodeValidator nodeValidator) 
            : base(new NodeMetaComparer(), origin, destination, nodeValidator) { }

        protected override void ProcessNeighbor(NodeMetaData<T> neighborMetaData)
        {
            if (neighborMetaData.Status != NodeStatus.New) return;
            
            Debug.Assert(neighborMetaData.Parent == null);
            neighborMetaData.Parent = _currentMetaData.Node;
            neighborMetaData.FromCost = _currentMetaData.FromCost + neighborMetaData.Node.RealCostTo(_currentMetaData.Node);
            neighborMetaData.ToCost = neighborMetaData.Node.EstimatedCostTo(Destination);
            neighborMetaData.Status = NodeStatus.Open;
            var didAdd = _openNodes.Add(neighborMetaData);
            Debug.Assert(didAdd);
        }

        private class NodeMetaComparer : Comparer<NodeMetaData<T>>
        {
            public override int Compare(NodeMetaData<T> x, NodeMetaData<T> y)
            {
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

    
}