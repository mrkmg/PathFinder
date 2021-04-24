using System;
using BenchmarkDotNet.Running;
using static JetBrains.Profiler.Api.MeasureProfiler;

namespace PathFinder.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0) return;
            switch (args[0])
            {
                case "benchmark":
                    BenchmarkRunner.Run<AStarBenchmark>();
                    break;
                case "profile":
                    Profile(int.Parse(args[1]), double.Parse(args[2]), args[3], args[4]);
                    break;
            }
        }

        public static void Profile(int size, double greed, string type, string profileType)
        {

            var a = new AStarBenchmark
            {
                Size = size, 
                Greed = greed, 
                Seed = 333333, 
                MoveFactor = 1.0
            };

            if (profileType == "setup") StartCollectingData();
            switch (type)
            {
                case "standard":
                    a.SetupStandard();
                    break;
                case "maze":
                    a.SetupAsMaze();
                    break;
                default:
                    throw new ArgumentException("Unknown profile type", nameof(profileType));
                    
            }
            if (profileType == "setup") StopCollectingData();
            
            if (profileType == "solve") StartCollectingData();
            a.Baseline();
            if (profileType == "solve") StopCollectingData();
        }
    }
}