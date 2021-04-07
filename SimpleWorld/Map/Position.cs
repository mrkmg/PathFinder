using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PathFinder.Interfaces;

namespace SimpleWorld.Map
{
    public class Position : INode
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public readonly World World;

        public Position(World world, int x, int y, int z)
        {
            World = world;
            X = x;
            Y = y;
            Z = z;
        }

        public double RealCostTo(INode other) =>
            other is Position otherNode 
                ? EstimateCostTo(otherNode) * Math.Pow(Z, World.MoveCost)
                : double.MaxValue;

        public double EstimatedCostTo(INode other) => 
            other is Position otherNode 
                ? EstimateCostTo(otherNode)
                : double.MaxValue;

        // private IEnumerable<Xy> GetReachablePositions()
        // {
        //     yield return new Xy(X - 1, Y);
        //     yield return new Xy(X, Y - 1);
        //     yield return new Xy(X, Y + 1);
        //     yield return new Xy(X + 1, Y);
        //
        //     if (!World.CanCutCorner) yield break;
        //     
        //     yield return new Xy(X + 1, Y - 1);
        //     yield return new Xy(X - 1, Y - 1);
        //     yield return new Xy(X - 1, Y + 1);
        //     yield return new Xy(X + 1, Y + 1);
        // }
        //
        // public IEnumerable<INode> GetReachableNodes() =>
        //     GetReachablePositions()
        //         .Select(World.GetNode)
        //         .Where(l => l != null);

        public IEnumerable<INode> GetReachableNodes()
        {
            for (var x = X-1; x <= X+1; x++)
            for (var y = Y-1; y <= Y+1; y++)
            {
                if (!World.CanCutCorner && x != X && y != Y) continue;
                
                var node = World.GetNode(x, y);
                if (node != null) yield return node;
            }
        }


        public bool Equals(INode other) => other is Position n && X == n.X && Y == n.Y;

        private double EstimateCostTo(Position other)
        {
            var dX = other.X - X;
            var dY = other.Y - Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }

        public override string ToString() => X + "::" + Y;

        public bool Equals(INode x, INode y) => x is Position xn && y is Position yn && xn.Equals(yn);

        public int GetHashCode(INode obj) => obj.GetHashCode();

        public override int GetHashCode() => 17 * (23 + X.GetHashCode()) * (23 + Y.GetHashCode());
    }

    public readonly struct Xy
    {
        public readonly int X;
        public readonly int Y;

        public Xy(int x, int y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Xy other)
        {
            var dX = other.X - X;
            var dY = other.Y - Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }
    }
}
