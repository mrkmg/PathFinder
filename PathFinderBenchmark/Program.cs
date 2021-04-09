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
                    a.Size = int.Parse(args[1]);
                    a.Thoroughness = double.Parse(args[2]);
                    a.Seed = 500;
                    a.MoveFactor = 1.0;
                    a.Setup();
                    a.Baseline();
                    break;                    
            }
        }
    }
}