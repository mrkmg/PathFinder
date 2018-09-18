﻿using System;
using System.Collections.Generic;
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
        private readonly IList<decimal> _thoroughnesses;
        private readonly string _outputFile;
        public bool CanDiag = false;
        public int MapHeight = 600;
        public int MapWidth = 600;
        public int MaxSearchSpace = int.MaxValue;
        public bool UseCornerEstimate = true;

        public ManyTest(IList<decimal> thouroughnesses, string outputFile)
        {
            _thoroughnesses = thouroughnesses;
            _outputFile = outputFile;

            _outputFile = _outputFile.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            Console.WriteLine(_outputFile);
        }

        public void Main(int numTests)
        {
            ExportResults(SequenceBuilder.Build(numTests).AsParallel().Select(RunTest));
        }

        private void ExportResults(ParallelQuery<TestResult> results)
        {
            if (File.Exists(_outputFile)) File.Delete(_outputFile);

            using (var fileHandle = File.OpenWrite(_outputFile))
            using (var streamWriter = new StreamWriter(fileHandle))
            {
                streamWriter.WriteLine("TestNum,EstLength,Thorougness,Length,Cost");
                streamWriter.Flush();
                results.ForAll(result =>
                {
                    lock (streamWriter)
                    {
                        foreach (var test in result.Tests)
                            streamWriter.WriteLine(result.TestNum + "," +
                                                   result.EstLength + "," +
                                                   test.Key + "," +
                                                   test.Value.Item1 + "," +
                                                   test.Value.Item2);
                        streamWriter.Flush();
                    }
                });
            }
        }

        private void ThreadPrint(int x, int y, string o, ConsoleColor b = ConsoleColor.Black,
            ConsoleColor f = ConsoleColor.White)
        {
            lock (Console.Out)
            {
                Console.BackgroundColor = b;
                Console.ForegroundColor = f;
                Console.CursorLeft = x;
                Console.CursorTop = y;
                Console.Write(o);
                Console.ResetColor();
            }
        }

        private TestResult RunTest(int testNumber)
        {
            var rnd = new Random();
            var widthPerResult = _thoroughnesses.Count + 5;
            var cols = Math.Floor((float) Console.WindowWidth / widthPerResult);
            var left = (int) (testNumber % cols) * widthPerResult;
            var top = (int) Math.Floor(testNumber / cols) % Console.WindowHeight;
            var map = new World(MapWidth, MapHeight)
            {
                CanCutCorner = CanDiag,
                EstimateType = UseCornerEstimate ? EstimateType.Square : EstimateType.Absolute
            };
            var o = map.GetAllNodes().OrderBy(n => rnd.Next()).First();
            var d = map.GetAllNodes().OrderBy(n => rnd.Next()).First();
            var estLength = (int) o.EstimatedCostTo(d);
            var result = new TestResult
            {
                EstLength = estLength,
                TestNum = testNumber,
                Tests = new Dictionary<decimal, Tuple<double, int>>()
            };

            ThreadPrint(left, top, testNumber.ToString().PadRight(3), ConsoleColor.Red, ConsoleColor.Black);
            ThreadPrint(left + 3, top, new string(' ', widthPerResult - 3));

            var i = 0;
            var cancelRest = false;
            foreach (var t in _thoroughnesses.Select(v => Math.Clamp(v, 0, 1)))
            {
                if (cancelRest) continue;

                var aStar = new AStar<Position>(o, d) {Thoroughness = (double) t};
                var keepGoing = true;
                while (keepGoing)
                    switch (aStar.State)
                    {
                        case SolverState.Running:
                            aStar.Tick();
                            if (aStar.Ticks > MaxSearchSpace)
                            {
                                aStar.Cancel();
                                ThreadPrint(i + 3 + left, top, "*");
                                keepGoing = false;
                                cancelRest = true;
                            }

                            break;
                        case SolverState.Success:
                            result.Tests.Add(t, new Tuple<double, int>(aStar.Cost, aStar.Ticks));
                            ThreadPrint(i + 3 + left, top, "+");
                            keepGoing = false;
                            break;
                        case SolverState.Failed:
                            keepGoing = false;
                            cancelRest = true;
                            ThreadPrint(i + 3 + left, top, "-");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                i++;
            }

            ThreadPrint(left, top, testNumber.ToString().PadRight(3), ConsoleColor.Green, ConsoleColor.Black);
            return result;
        }
    }

    internal struct TestResult
    {
        public int TestNum;
        public int EstLength;
        public Dictionary<decimal, Tuple<double, int>> Tests;
    }
}
