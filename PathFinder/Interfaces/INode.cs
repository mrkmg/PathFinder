using System.Collections.Generic;

namespace PathFinder.Interfaces
{
    public interface INode
    {
        double RealCostTo(INode node);
        double EstimatedCostTo(INode node);
        IEnumerable<INode> GetNeighbors();
    }
}
