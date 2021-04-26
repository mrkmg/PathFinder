using System;
using BenchmarkDotNet.Attributes;
using PathFinder.Graphs;
using PathFinder.Solvers.Generic;
using SimpleWorld.Map;

namespace PathFinder.Benchmark
{
    [SimpleJob(1, 2, 20, 1)]
    public class AStarBenchmark
    {
        private World _world;
        private Position _from;
        private Position _to;

        [Params(1500)]
        public int Size;
        
        [Params(1)]
        public double Greed;

        // [Params(4967, 4969, 4973, 4987, 4993, 4999, 5003)]
        [Params(333333)]
        public int Seed;

        [Params(1.0)]
        public double MoveFactor;
        
        [IterationSetup]
        public void SetupStandard()
        {
            _world = new World(Size, Size, MoveFactor, World.DefaultStandardInit, new Random(Seed));
            _from = _world.GetPosition(1, 1);
            _to = _world.GetPosition(Size-1, Size-1);
        }

        public void SetupAsMaze()
        {
            _world = new World(Size, Size, MoveFactor, World.DefaultMazeInit, new Random(Seed));
            _from = _world.GetPosition(0, 0);
            _to = _world.GetPosition(Size-2, Size-2);
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            AStar<Position>.Solve(_from, _to, Greed, out _);
        }
        
        [Benchmark]
        public void WithTraverserPassed()
        {
            AStar<Position>.Solve(_from, _to, Greed, out _, new DefaultTraverser<Position>());
        }
    }
}