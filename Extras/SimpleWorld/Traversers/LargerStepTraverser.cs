using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PathFinder.Graphs;
using SimpleWorld.Map;

namespace SimpleWorld.Traversers
{
    public class LargerStepTraverser : INodeTraverser<Position>
    {
        private readonly int _stepSize;
        private readonly double _stepSizeBonusFactor;

        public LargerStepTraverser(int stepSize)
        {
            _stepSize = stepSize;
            _stepSizeBonusFactor = Math.Sqrt(stepSize * 4d) * stepSize;
        }
        
        public double RealCost(Position fromNode, Position toNode)
        {
            var originalCost = fromNode.RealCostTo(toNode);
            var distanceCost = EstimatedCost(fromNode, toNode);
            var ratio = (_stepSizeBonusFactor - distanceCost) / _stepSizeBonusFactor * 0.2d;
            return originalCost * ratio;

        }

        public double EstimatedCost(Position fromNode, Position toNode) 
            => fromNode.EstimatedCostTo(toNode) * 0.2d;

        public IEnumerable<Position> TraversableNodes(Position sourceNode)
        {
            for (var x = sourceNode.X-_stepSize; x <= sourceNode.X+_stepSize; x++)
            for (var y = sourceNode.Y-_stepSize; y <= sourceNode.Y+_stepSize; y++)
            {
                if (x == sourceNode.X && y == sourceNode.Y) continue;

                var node = sourceNode.World.GetPosition(x, y);
                if (node == null) continue;
                if (sourceNode.EstimatedCostTo(node) <= _stepSize) yield return node;
            }
        }
    }
}