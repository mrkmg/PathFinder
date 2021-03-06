﻿using System;
using System.Collections.Generic;
using PathFinder.Graphs;

namespace SimpleWorld.Map
{
    public class Position : ITraversableNode<Position>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Cost;
        public readonly World World;

        private IList<Position> _neighborsCache;

        public Position(World world, int x, int y, int cost)
        {
            World = world;
            X = x;
            Y = y;
            Cost = cost;
        }

        public double RealCostTo(Position other) 
            => StraightLineDistanceTo(other) * Math.Pow((Cost + other.Cost) / 2d, World.MoveCost);

        public double EstimatedCostTo(Position other) 
            => BestCaseCornering(other);

        public IEnumerable<Position> NeighborNodes() 
            => _neighborsCache ??= FindNeighbors();

        private double BestCaseCornering(Position other)
        {
            var dX = Math.Abs(other.X - X);
            var dY = Math.Abs(other.Y - Y);
            var distCornering = Math.Min(dX, dY);
            var distStraight = Math.Max(dX, dY) - distCornering;
            return Math.Sqrt(distCornering * distCornering * 2) + distStraight;
        }
        
        private double StraightLineDistanceTo(Position other)
        {
            var dX = other.X - X;
            var dY = other.Y - Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }

        private IList<Position> FindNeighbors()
        {
            var list = new List<Position>(8);
            for (var x = X-1; x <= X+1; x++)
            for (var y = Y-1; y <= Y+1; y++)
            {
                if (x == X && y == Y) continue;

                var node = World.GetPosition(x, y);
                if (node != null) list.Add(node);
            }
            return list;
        }

        public override string ToString() 
            => $"Position<{Cost}>({X},{Y})";
        
        public bool Equals(Position other) 
            => other != null && X == other.X && Y == other.Y && World.Equals(other.World);
        
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ (World != null ? World.GetHashCode() : 0);
                return hashCode;
            }
        }
        
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
