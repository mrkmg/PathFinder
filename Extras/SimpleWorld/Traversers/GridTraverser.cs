using System;
using System.Collections.Generic;
using PathFinder.Graphs;
using SimpleWorld.Map;

namespace SimpleWorld.Traversers
{
    public class GridTraverser : INodeTraverser<Position>
    {
        public double RealCost(Position fromNode, Position toNode)
            => fromNode.RealCostTo(toNode);
        
        public double EstimatedCost(Position fromNode, Position toNode)
        {
            return Math.Abs(fromNode.X - toNode.X) + Math.Abs(fromNode.Y - toNode.Y);
        }

        public IEnumerable<Position> TraversableNodes(Position sourceNode)
        {
            var n1 = sourceNode.World.GetPosition(sourceNode.X - 1, sourceNode.Y);
            var n2 = sourceNode.World.GetPosition(sourceNode.X + 1, sourceNode.Y);
            var n3 = sourceNode.World.GetPosition(sourceNode.X, sourceNode.Y - 1);
            var n4 = sourceNode.World.GetPosition(sourceNode.X, sourceNode.Y + 1);

            if (n1 != null) yield return n1;
            if (n2 != null) yield return n2;
            if (n3 != null) yield return n3;
            if (n4 != null) yield return n4;
        }
    }
}