using System.Collections.Generic;

namespace PathFinder.Graphs
{
    /// <summary>
    /// The default graph traverser. Uses the <c>from</c> nodes traversing methods.
    /// </summary>
    /// <typeparam name="T"><see cref="ITraversableGraphNode{T}"/></typeparam>
    public class DefaultTraverser<T> : INodeTraverser<T> where T : ITraversableGraphNode<T>
    {
        public double EstimatedCost(T from, T to) => from.EstimatedCostTo(to);
        public double RealCost(T from, T to) => from.RealCostTo(to);
        public IEnumerable<T> TraversableNodes(T sourceNode) => sourceNode.TraversableNodes();
    }
}