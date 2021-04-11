﻿using System;
using System.Collections.Generic;
using PathFinder.Solvers.Generic;

namespace SimpleWorld.Map
{
    public class Position : IGraphNode
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

        public double RealCostTo(IGraphNode other)
        {
            if (!(other is Position otherNode)) return double.MaxValue;
            
            var dX = otherNode.X - X;
            var dY = otherNode.Y - Y;
            return Math.Sqrt(dX * dX + dY * dY) * Math.Pow(Z, World.MoveCost);

        }

        public double EstimatedCostTo(IGraphNode other)
        {
            if (!(other is Position otherNode)) return double.MaxValue;
            
            var dX = otherNode.X - X;
            var dY = otherNode.Y - Y;
            return Math.Sqrt(dX * dX + dY * dY);

        }

        public IEnumerable<IGraphNode> GetReachableNodes()
        {
            for (var x = X-1; x <= X+1; x++)
            for (var y = Y-1; y <= Y+1; y++)
            {
                if (!World.CanCutCorner && x != X && y != Y) continue;
                if (x == X && x == y) continue;

                var node = World.GetPosition(x, y);
                if (node != null) yield return node;
            }
        }

        public bool Equals(IGraphNode other) => other is Position n && X == n.X && Y == n.Y;
        public override string ToString() => "Position: " + X + "::" + Y;
        public override int GetHashCode() => 17 * (23 + X.GetHashCode()) * (1427 + Y.GetHashCode());
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