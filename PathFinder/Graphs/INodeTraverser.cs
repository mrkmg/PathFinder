using System;
using System.Collections.Generic;

namespace PathFinder.Graphs
{
    /// <summary>
    /// Provide an alternate traverser for a graph that any of the solvers
    /// in <see cref="PathFinder.Solvers.Generic"/> may use instead of the
    /// default traverser.
    /// </summary>
    /// <remarks>
    /// A use case to create a separate Node Traverser may be to provide
    /// alternate map traversal patterns for different types of entities
    /// in a game or simulation.
    /// </remarks>
    /// <typeparam name="T"><see cref="ITraversableNode{T}"/></typeparam>
    public interface INodeTraverser<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Calculate the real cost to another node.
        /// </summary>
        /// <param name="fromNode">The node to move from.</param>
        /// <param name="toNode">The node to move to.</param>
        /// <returns>
        /// The actual cost to move from one node to another.
        /// </returns>
        /// <remarks>
        /// Should return less than 0 if not possible to move to node.
        /// </remarks>
        double RealCost(T fromNode, T toNode);
        
        /// <summary>
        /// Calculate the estimated cost to another node.
        /// </summary>
        /// <param name="fromNode">The node to move from.</param>
        /// <param name="toNode">The node to move to.</param>
        /// <returns>
        /// The estimated cost to move to the toNode starting at the fromNode
        /// </returns>
        /// <remarks>
        /// In order for the AStar to work as expected, this should always return the "best case" cost.
        /// </remarks>
        double EstimatedCost(T fromNode, T toNode);
        
        /// <summary>
        /// Get nodes that are neighbors of (immediately traversable to) sourceNode.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> NeighborNodes(T sourceNode);
    }
}