﻿using System.Collections.Generic;

namespace PathFinder.Graphs
{
    /// <summary>
    /// The default graph traverser. Uses the <c>from</c> nodes traversing methods.
    /// </summary>
    /// <typeparam name="T"><see cref="ITraversableNode{T}"/></typeparam>
    public class DefaultTraverser<T> : INodeTraverser<T> where T : ITraversableNode<T>
    {
        public double EstimatedCost(T fromNode, T toNode) => fromNode.EstimatedCostTo(toNode);
        public double RealCost(T fromNode, T toNode) => fromNode.RealCostTo(toNode);
        public IEnumerable<T> NeighborNodes(T sourceNode) => sourceNode.NeighborNodes();
    }
}