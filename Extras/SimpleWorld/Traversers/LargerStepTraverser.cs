using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PathFinder.Graphs;
using SimpleWorld.Map;

namespace SimpleWorld.Traversers
{
    // Traverses the grid using larger steps than default.
    // It is "cheaper" to take one large step, rather than many
    // small steps.
    public class LargerStepTraverser : INodeTraverser<Position>
    {
        private readonly int _stepSize;
        private const double StepBaseCost = 0.5; 

        public LargerStepTraverser(int stepSize)
        {
            _stepSize = stepSize;
        }
        
        public double RealCost(Position fromNode, Position toNode)
        {
            return fromNode.RealCostTo(toNode) + StepBaseCost;

        }

        public double EstimatedCost(Position fromNode, Position toNode)
        {
            var estDist = fromNode.EstimatedCostTo(toNode);
            var estNumSteps = Math.Ceiling(estDist / _stepSize);
            return estDist + estNumSteps * StepBaseCost;
        }

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