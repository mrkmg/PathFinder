using System;
using System.Collections.Generic;
using PathFinder.Graphs;
using SimpleWorld.Map;

namespace SimpleWorld.Traversers
{
    // Traverses the grid by only moving along the grid in the cardinal directions.
    public class LevelTraverser : INodeTraverser<Position>
    {
        public double RealCost(Position fromNode, Position toNode)
            => fromNode.EstimatedCostTo(toNode) + Math.Abs(fromNode.Cost - toNode.Cost) * fromNode.World.MoveCost;

        public double EstimatedCost(Position fromNode, Position toNode)
            => fromNode.EstimatedCostTo(toNode);

        public IEnumerable<Position> NeighborNodes(Position sourceNode) 
            => sourceNode.NeighborNodes();
    }
}