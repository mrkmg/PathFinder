using System;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Running;

namespace PathFinderBenchmark
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
                    var a = new AStarBenchmark();
                    a.Size = 1000;
                    a.Thoroughness = 0.5;
                    a.Setup();
                    a.Solve();
                    break;                    
            }
        }
    }
}