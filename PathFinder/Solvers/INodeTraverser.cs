using System.Collections.Generic;
using PathFinder.Solvers.Generic;

namespace PathFinder.Solvers
{
    public interface INodeTraverser<T> where T : ITraversableGraphNode<T>
    {
        double EstimatedCost(T fromNode, T toNode);
        double RealCost(T fromNode, T toNode);
        bool CanTraverse(T fromNode, T toNode);
        IEnumerable<T> TraversableNodes(T sourceNode);
    }
}