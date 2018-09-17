using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Interfaces;

namespace PathFinderTest.Map
{
    public class Position : INode
    {
        public readonly int W;
        public readonly int X;
        public readonly int Y;
        public World World;

        public Position(int x, int y, int w)
        {
            X = x;
            Y = y;
            W = w;
        }

        public bool Equals(INode other) => other is Position n && X == n.X && Y == n.Y;
        public override string ToString() => X + "::" + Y;
        public bool Equals(INode x, INode y) => x is Position xn && y is Position yn && xn.Equals(yn);

        public override int GetHashCode() => 17 * (23 + X.GetHashCode()) * (23 + Y.GetHashCode());

        public double RealCostTo(INode other) => other is Position n ? Math.Abs(W - n.W) * 5 + 1 : double.MaxValue;

        public double EstimatedCostTo(INode other)
            => !(other is Position n) ? double.MaxValue :
                World.EstimateType == EstimateType.Square ? EstimateDistanceToSq(n): EstimateDistanceToAbs(n);

        private double EstimateDistanceToAbs(Position n) => Math.Abs(n.X - X) + Math.Abs(n.Y - Y);
        private double EstimateDistanceToSq(Position n) => Math.Sqrt(Math.Pow(n.X - X, 2) + Math.Pow(n.Y - Y, 2));

        public IEnumerable<INode> GetNeighbors()
        {
            var possibleLocations = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(X - 1, Y),
                new Tuple<int, int>(X, Y - 1),
                new Tuple<int, int>(X, Y + 1),
                new Tuple<int, int>(X + 1, Y)
            };

            if (World.CanCutCorner)
            {
                possibleLocations.Add(new Tuple<int, int>(X + 1, Y - 1));
                possibleLocations.Add(new Tuple<int, int>(X - 1, Y - 1));
                possibleLocations.Add(new Tuple<int, int>(X - 1, Y + 1));
                possibleLocations.Add(new Tuple<int, int>(X + 1, Y + 1));
            }

            return possibleLocations
                .Where(l => World.AllNodes.ContainsKey(l.Item1) && World.AllNodes[l.Item1].ContainsKey(l.Item2))
                .Select(l => World.AllNodes[l.Item1][l.Item2]);
        }
    }
}
