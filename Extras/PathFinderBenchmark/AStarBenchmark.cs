using System;
using BenchmarkDotNet.Attributes;
using PathFinder.Solvers.Generic;
using SimpleWorld.Map;

namespace PathFinderBenchmark
{
    [SimpleJob(1, 2, 20, 1)]
    public class AStarBenchmark
    {
        private World _world;
        private Position _from;
        private Position _to;

        [Params(1500)]
        public int Size;
        
        [Params(0, 0.5, 1, 1.5)]
        public double Greed;

        // [Params(4967, 4969, 4973, 4987, 4993, 4999, 5003)]
        [Params(111111)]
        public int Seed;

        [Params(1.0)]
        public double MoveFactor;
        
        [IterationSetup]
        public void Setup()
        {
            _world = new World(Size, Size, new Random(Seed), null, MoveFactor);
            _from = _world.GetPosition(1, 1);
            _to = _world.GetPosition(Size-1, Size-1);
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            AStar<Position>.Solve(_from, _to, Greed, out _);
        }
    }
}