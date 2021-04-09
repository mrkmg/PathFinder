using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using PathFinder.Components;
using PathFinder.Interfaces;

namespace PathFinder.Solvers
{
    public sealed class AStar<T> : AStarCore<T> where T : INode
    {
        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="AStarCore{T}.Origin"/></param>
        /// <param name="destination"><see cref="AStarCore{T}.Destination"/></param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        [CanBeNull]
        public static IList<T> Solve(T origin, T destination, double greedFactor = 0.5) 
            => new AStar<T>(origin, destination, greedFactor).Solve();

        /// <summary>
        ///     Finds a path between the origin and destination node
        /// </summary>
        /// <param name="origin"><see cref="Origin"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="nodeValidator">A function to validate if a node is usable for this solver.</param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        [CanBeNull]
        public static IList<T> Solve(T origin, T destination, Func<T, T, bool> nodeValidator, double greedFactor = 0.5) 
            => new AStar<T>(origin, destination, nodeValidator, greedFactor).Solve();

        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="Origin"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        public AStar(T origin, T destination, double greedFactor = 0.5) 
            : base(new NodeMetaComparer<T>(), origin, destination)
        {
            GreedFactor = greedFactor;
        }

        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin"><see cref="Origin"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="nodeValidator">A function to validate if a node is usable for this solver.</param>
        /// <param name="greedFactor"><see cref="GreedFactor"/></param>
        public AStar(T origin, T destination, Func<T, T, bool> nodeValidator, double greedFactor = 0.5) 
            : base(new NodeMetaComparer<T>(), origin, destination, nodeValidator)
        {
            GreedFactor = greedFactor;
        }
        
        /// <summary>
        /// <para>How thorough the search should be.</para>
        /// <para>Must be greater than or equal to 0.</para>
        /// <remarks>
        /// <list type="bullet">
        /// <item>0 is breadth-first search</item>
        /// <item>1 is traditional A*</item>
        /// <item>>1 favors obtaining the goal at a cost for greedFactor</item>
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
        
        private double _greedFactor = 0.5;
        
        protected override void ProcessNeighbor(NodeMetaData<T> neighborMetaData)
        {
            var fromCost = _currentMetaData.FromCost + _currentMetaData.Node.RealCostTo(neighborMetaData.Node);

            switch (neighborMetaData.Status)
            {
                case NodeStatus.Open when fromCost >= neighborMetaData.FromCost:
                    return;
                case NodeStatus.Open:
                    _openNodes.Remove(neighborMetaData);
                    break;
                case NodeStatus.New:
                    neighborMetaData.ToCost = neighborMetaData.Node.EstimatedCostTo(Destination);
                    neighborMetaData.Status = NodeStatus.Open;
                    break;
                case NodeStatus.Closed:
                    throw new InvalidOperationException("Neighbor can not be closed in this process");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            neighborMetaData.TotalCost = fromCost + neighborMetaData.ToCost * GreedFactor;
            neighborMetaData.Parent = Current;
            neighborMetaData.FromCost = fromCost;
            _openNodes.Add(neighborMetaData);
        }

        private class NodeMetaComparer<T> : Comparer<NodeMetaData<T>> where T : INode
        {
            public override int Compare(NodeMetaData<T> x, NodeMetaData<T> y)
            {
                if (ReferenceEquals(x, y)) return 0;
                return x.TotalCost > y.TotalCost ? 1 
                    : x.TotalCost < y.TotalCost ? -1 
                    : x.ToCost > y.ToCost ? 1
                    : x.ToCost < y.ToCost ? -1 
                    : 0;
            }
        }
    }
}