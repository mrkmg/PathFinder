using System;
using System.Collections.Generic;

namespace PathFinder.Solvers.Generic
{

    /// <summary>
    /// Should return false if a node should not be considered.
    /// <remarks>This is called in a hot path, and should will affect performance.</remarks>
    /// </summary>
    public delegate bool NodeValidator(INode arg1, INode arg2);
    
    /// <summary>
    /// The interface needed for any nodes in a graph to be searched through
    /// in via any the <c>PathFinder.Solvers.Generic</c> namespace.
    /// </summary>
    public interface INode : IEquatable<INode>, IEqualityComparer<INode>
    {
        /// <summary>
        ///     Calculate the real cost to another node.
        /// </summary>
        /// <param name="other">The target node to move to.</param>
        /// <returns>
        ///     The actual cost to move from one node to another.
        /// </returns>
        /// <remarks>
        ///     Should return less than 0 if not possible to move to node.
        /// </remarks>
        double RealCostTo(INode other);

        /// <summary>
        ///     Calculate the estimated cost to another node.
        /// </summary>
        /// <param name="other">The target node to move to.</param>
        /// <returns>The best case (cheapest) cost to get from one node to another.</returns>
        double EstimatedCostTo(INode other);

        /// <summary>
        ///     Get nodes that are accessible from this node.
        /// </summary>
        /// <returns></returns>
        IEnumerable<INode> GetReachableNodes();
    }
}
