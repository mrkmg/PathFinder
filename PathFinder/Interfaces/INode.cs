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
        ///     Get nodes that are accessible from this node.
        /// </summary>
        /// <returns></returns>
        IEnumerable<INode> GetNeighbors();
    }

    public interface ISimpleNode : IEquatable<ISimpleNode>, IEqualityComparer<ISimpleNode>
    {
        /// <summary>
        ///     Calculate the real cost to another node.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        double RealCostTo(ISimpleNode other);
        
        /// <summary>
        ///     Get nodes that are accessible from this node.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ISimpleNode> GetNeighbors();
    }
}
