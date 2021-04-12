#nullable enable
using System;
using System.Collections.Generic;
using PathFinder.Solvers.Generic;

namespace PathFinderUnitTests
{
    public class TestGraph
    {
        private readonly int[][] _world =
        {
            new[] {1, 1, 1, 1, 1,  1,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 0,  1,  2, 2, 1, 1},
            new[] {1, 1, 1, 1, 0,  10, 5, 5, 2, 1},
            new[] {1, 1, 1, 1, 0,  8,  2, 2, 1, 1},
            new[] {1, 1, 1, 1, 0,  0,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 0,  0,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 10, 10, 1, 1, 1, 1},
            new[] {1, 1, 1, 1, 1,  0,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 1,  0,  1, 1, 1, 1},
            new[] {1, 1, 1, 1, 1,  0,  1, 1, 1, 1}
        };

        public TestGraphNode GetNode(int x, int y) => new (x, y, _world[x][y], this);
    }

    public readonly struct TestGraphNode : ITraversableGraphNode<TestGraphNode>
    {
        public readonly int X;
        public readonly int Y;
        private readonly int _cost;
        private readonly TestGraph _testGraph;

        public TestGraphNode(int x, int y, int cost, TestGraph testGraph)
        {
            X = x;
            Y = y;
            _cost = cost;
            _testGraph = testGraph;
        }

        public double RealCostTo(TestGraphNode other)
        {
            return (_cost + other._cost) / 2d;
        }

        public double EstimatedCostTo(TestGraphNode other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public IEnumerable<TestGraphNode> GetReachableNodes()
        {
            var maxX = Math.Min(9, X + 1);
            var maxY = Math.Min(9, Y + 1);
            for (var x = Math.Max(0, X - 1); x <= maxX; x++)
            for (var y = Math.Max(0, Y - 1); y <= maxY; y++)
                if (x != X || y != Y)
                {
                    var node = _testGraph.GetNode(x, y);
                    if (node._cost != 0) yield return node;
                }
        }
        public bool Equals(TestGraphNode other)
        {
            return other != null && X == other.X && Y == other.Y;
        }

        public override bool Equals(object? obj)
        {
            return obj is TestGraphNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(TestGraphNode left, TestGraphNode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TestGraphNode left, TestGraphNode right)
        {
            return !(left == right);
        }
    }
}