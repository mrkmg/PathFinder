using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using PathFinderTest.Map;
using PathFinderTest.Sequencer;

namespace PathFinderTest.Tests.Many
{
    internal class ManyTest
    {
        public IList<decimal> Thoroughnesses = new List<decimal> { 0.25m, 0.5m };
        public string OutputFile
        {
            get => _outputFile;
            set => _outputFile = value.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }
        public bool CanDiag = false;
        public int NumberOfTests = 100;
        public int MapHeight = 400;
        public int MapWidth = 400;
        public int MaxSearchSpace = int.MaxValue;
        public bool UseCornerEstimate = true;
        public Random Random = new Random();
        private string _outputFile = "./test.csv";
        private readonly object _fileLock = new { };

        public void Run()
        {
            WriteFileHeader();

            SequenceBuilder
                .Build(NumberOfTests)
                .SelectMany(BuildTest)
                .AsParallel()
                .Select(RunTest)
                .Where(r => r != null)
                .ForAll(WriteResult);
        }

        private IEnumerable<Test> BuildTest(int testId)
        {
            var map = new World(MapWidth, MapHeight)
            {
                CanCutCorner = CanDiag,
                EstimateType = UseCornerEstimate ? EstimateType.Square : EstimateType.Absolute
            };

            Position origin;
            Position destination;

            do
            {
                origin = map.GetAllNodes().OrderBy(n => Random.Next()).First();
                destination = map.GetAllNodes().OrderBy(n => Random.Next()).First();
            } while (AStar<Position>.Solve(origin, destination, 0) != null);

            var estimatedCostTo = (int)origin.EstimatedCostTo(destination);

            var subTestNum = 0;
            foreach (var thoroughness in Thoroughnesses)
            {
                yield return new Test
                {
                    Id = testId,
                    SubId = subTestNum,
                    Origin = origin,
                    Destination = destination,
                    EstimatedCostTo = estimatedCostTo,
                    Thoroughness = thoroughness,
                    World = map
                };

                subTestNum++;
            }
        }

        private TestResult RunTest(Test test)
        {
            var aStar = new AStar<Position>(test.Origin, test.Destination) { Thoroughness = (double)test.Thoroughness };
            var timer = new Stopwatch();
            
            while (true)
                switch (aStar.State)
                {
                    case SolverState.Running:
                        timer.Start();
                        aStar.Tick();
                        timer.Stop();

                        if (aStar.Ticks > MaxSearchSpace)
                        {
                            aStar.Cancel();
                            return null;
                        }

                        break;
                    case SolverState.Success:
                        return new TestResult
                        {
                            TestId = test.Id,
                            SubId = test.SubId,
                            EstimatedCostTo = test.EstimatedCostTo,
                            Thoroughness = test.Thoroughness,
                            PathCost = aStar.Cost,
                            Checks = aStar.Ticks,
                            Ticks = timer.ElapsedTicks,
                            Time = timer.ElapsedTicks
                        };
                    case SolverState.Failed:
                        return null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        private void WriteFileHeader()
        {
            lock (_fileLock)
            {
                File.WriteAllText(OutputFile, "TestId,SubTestId,EstimatedCostTo,Thoroughness,PathCost,Check,Ticks,Time\n");
            }
        }

        private void WriteResult(TestResult result)
        {
            lock (_fileLock)
            {
                File.AppendAllText(OutputFile,
                    $"{result.TestId},{result.SubId},{result.EstimatedCostTo},{result.Thoroughness},{result.PathCost},{result.Checks},{result.Ticks},{result.Time}\n");
            }
            lock (Console.Out)
            {
                Console.WriteLine($"{result.TestId}\t{result.SubId}\t{result.EstimatedCostTo}\t{result.Thoroughness}\t{result.PathCost}\t{result.Checks}\t{result.Ticks}\t{result.Time}");
            }
        }
    }

    internal class Test
    {
        public int Id;
        public int SubId;
        public World World;
        public int EstimatedCostTo;
        public Position Origin;
        public Position Destination;
        public decimal Thoroughness;
    }

    internal class TestResult
    {
        public int TestId;
        public int SubId;
        public int EstimatedCostTo;
        public decimal Thoroughness;
        public double PathCost;
        public int Checks;
        public long Ticks;
        public long Time; 
    }
}
