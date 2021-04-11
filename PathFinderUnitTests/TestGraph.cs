#nullable enable
using System;
using System.Collections.Generic;
using PathFinder.Solvers.Generic;

namespace PathFinderUnitTests
{
    public class TestGraph
    {
        private int[][] World =
        {
            new[] {1, 1, 1, 1, 1,  1,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 0,  1,  2, 2, 1, 1},
            new[] {1, 1, 1, 1, 0,  1,  5, 5, 2, 1},
            new[] {1, 1, 1, 1, 0,  1,  2, 2, 1, 1},
            new[] {1, 1, 1, 1, 0,  0,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 0,  0,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 10, 10, 1, 1, 1, 1},
            new[] {1, 1, 1, 1, 1,  0,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 1,  0,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 1,  0,  1, 1, 1, 1},
        };

        public TestGraphNode GetNode(int x, int y) => new (x, y, World[x][y], this);
    }

    public readonly struct TestGraphNode : IGraphNode
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Cost;
        public readonly TestGraph TestGraph;

        public TestGraphNode(int x, int y, int cost, TestGraph testGraph)
        {
            X = x;
            Y = y;
            Cost = cost;
            TestGraph = testGraph;
        }

        public double RealCostTo(IGraphNode other)
        {
            if (other is not TestGraphNode node) return double.MaxValue;
            return (Cost + node.Cost) / 2d;
        }

        public double EstimatedCostTo(IGraphNode other)
        {
            if (other is not TestGraphNode node) return double.MaxValue;
            var dx = X - node.X;
            var dy = Y - node.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public IEnumerable<IGraphNode> GetReachableNodes()
        {
            var maxX = Math.Min(9, X + 1);
            var maxY = Math.Min(9, Y + 1);
            for (var x = Math.Max(0, X - 1); x <= maxX; x++)
            for (var y = Math.Max(0, Y - 1); y <= maxY; y++)
                if (x != X || y != Y)
                {
                    var node = TestGraph.GetNode(x, y);
                    if (node.Cost != 0) yield return node;
                }
        }
        
        public bool Equals(IGraphNode? other)
        {
            return other is TestGraphNode n && Equals(n);
        }

        public bool Equals(TestGraphNode other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object? obj)
        {
            return obj is TestGraphNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}