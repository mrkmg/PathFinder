using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Interfaces;

namespace SimpleWorld.Map
{
    public class Position : INode
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public World World;

        public Position(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double RealCostTo(INode other)
        {
            return other is Position otherNode 
                ? EstimateDistanceTo(otherNode) + ZLevelCostTo(otherNode)
                : double.MaxValue;
        }

        public double EstimatedCostTo(INode other)
        {
            if (other is Position node)
            {
                var dist = EstimateDistanceTo(node);
                if (dist == 0) return 0;
                return dist + ZLevelCostTo(node) / dist;
            }

            return double.MaxValue;
        }

        public IEnumerable<INode> GetNeighbors()
        {
            var possibleLocations = new List<Xy>
            {
                new Xy(X - 1, Y),
                new Xy(X, Y - 1),
                new Xy(X, Y + 1),
                new Xy(X + 1, Y)
            };

            if (World.CanCutCorner)
            {
                possibleLocations.Add(new Xy(X + 1, Y - 1));
                possibleLocations.Add(new Xy(X - 1, Y - 1));
                possibleLocations.Add(new Xy(X - 1, Y + 1));
                possibleLocations.Add(new Xy(X + 1, Y + 1));
            }

            return possibleLocations
                .Select(l => World.GetNode(l.X, l.Y))
                .Where(l => l != null);
        }


        public bool Equals(INode other)
        {
            return other is Position n && X == n.X && Y == n.Y;
        }

        private double EstimateDistanceTo(Position n)
        {
            var dX = Math.Abs(n.X - X);
            var dY = Math.Abs(n.Y - Y);
            return Math.Sqrt(dX * dX + dY * dY);
        }

        private double ZLevelCostTo(Position n)
        {
            return Z > n.Z ? (Z - n.Z) * 2 : (n.Z - Z) * 10;
        }

        public override string ToString()
        {
            return X + "::" + Y;
        }

        public bool Equals(INode x, INode y)
        {
            return x is Position xn && y is Position yn && xn.Equals(yn);
        }

        public int GetHashCode(INode obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return 17 * (23 + X.GetHashCode()) * (23 + Y.GetHashCode());
        }
    }

    internal struct Xy
    {
        public readonly int X;
        public readonly int Y;

        public Xy(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
