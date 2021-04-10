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

        [Params(400)]
        public int Size;
        
        [Params(1)]
        public double Greed;

        // [Params(4967, 4969, 4973, 4987, 4993, 4999, 5003)]
        [Params(111111, 222222, 333333)]
        public int Seed;

        [Params(0.1, 1.0, 3.0)]
        public double MoveFactor;
        
        [IterationSetup]
        public void Setup()
        {
            _world = new World(Size, Size, new Random(Seed), MoveFactor);
            _from = _world.GetPosition(1, 1);
            _to = _world.GetPosition(Size-1, Size-1);
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            AStar<Position>.Solve(_from, _to, Greed, out _);
        }

        // [Benchmark]
        // public void Test1()
        // {
        //     new AStarTest1<Position>(_from, _to, GreedFactor).Solve();
        // }

        // [Benchmark]
        // public void Test2()
        // {
        //     new AStarTest2<Position>(_from, _to, GreedFactor).Solve();
        // }
    }
}