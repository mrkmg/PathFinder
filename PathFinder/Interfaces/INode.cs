using System;
using System.Collections.Generic;

namespace PathFinder.Interfaces
{
    public interface INode : IEquatable<INode>, IEqualityComparer<INode>
    {
        /// <summary>
        ///     Calculate the real cost to another node.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        double RealCostTo(INode other);

        /// <summary>
        ///     Calculate the estimated cost to another node.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        double EstimatedCostTo(INode other);

        /// <summary>
        ///     Get nodes that are accessable from this node.
        /// </summary>
        /// <returns></returns>
        IEnumerable<INode> GetNeighbors();
    }
}
