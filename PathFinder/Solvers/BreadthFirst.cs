using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PathFinder.Components;
using PathFinder.Interfaces;

namespace PathFinder.Solvers
{
    public sealed class BreadthFirst<T> : AStarCore<T> where T : INode
    {
        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="AStarCore{T}.Origin"/></param>
        /// <param name="destination"><see cref="AStarCore{T}.Origin"/></param>
        /// <param name="nodeValidator">A function to validate if a node is usable for this solver.</param>
        public static IList<T> Solve(T origin, T destination, Func<T, T, bool> nodeValidator) 
            => new Greedy<T>(origin, destination, nodeValidator).Solve();
        
        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="AStarCore{T}.Origin"/></param>
        /// <param name="destination"><see cref="AStarCore{T}.Origin"/></param>
        public static IList<T> Solve(T origin, T destination) 
            => new Greedy<T>(origin, destination).Solve();
 
        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="AStarCore{T}.Origin"/></param>
        /// <param name="destination"><see cref="AStarCore{T}.Origin"/></param>
        public BreadthFirst(T origin, T destination) 
            : base(new NodeMetaComparer(), origin, destination) { }
        
        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="AStarCore{T}.Origin"/></param>
        /// <param name="destination"><see cref="AStarCore{T}.Origin"/></param>
        /// <param name="nodeValidator">A function to validate if a node is usable for this solver.</param>
        public BreadthFirst(T origin, T destination, Func<T, T, bool> nodeValidator) 
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
                if (ReferenceEquals(x, y)) return 0;
                return x.FromCost > y.FromCost ? 1
                    : x.FromCost < y.FromCost ? -1 
                    : 0;
            }
        }
    }
}