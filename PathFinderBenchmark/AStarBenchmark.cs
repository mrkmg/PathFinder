using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using PathFinder.Solvers;
using SimpleWorld.Map;

namespace PathFinderBenchmark
{
    [SimpleJob(RuntimeMoniker.CoreRt50)]
    public class AStarBenchmark
    {
        private World _world;
        private Position _from;
        private Position _to;

        [Params(100, 250, 500, 1000)] 
        public int Size;
        
        [Params(0.1, 0.25, 0.5, 1)]
        public double Thoroughness;
        
        [IterationSetup]
        public void Setup()
        {
            _world = new World(Size, Size, new Random(23));
            _from = _world.GetNode(1, 1);
            _to = _world.GetNode(Size-1, Size-1);
        }

        [Benchmark]
        public void Solve()
        {
            AStar.Solve(_from, _to, Thoroughness);
        }
    }
}