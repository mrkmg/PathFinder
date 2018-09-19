using System;
using System.Linq;
using PathFinderTest.Sequencer;
using PathFinderTest.Tests.Interactive;
using PathFinderTest.Tests.Many;

namespace PathFinderTest
{
    internal class Program
    {
        private static void Main()
        {
            while (true)
            {
                Console.ResetColor();
                Console.Clear();
                Console.WriteLine("I, M, Q");
                var key = Console.ReadKey().Key;
                if (key == ConsoleKey.I)
                {
                    var seq = SequenceBuilder.Build((decimal) 0.5, 0, (decimal) 0.1).ToList();
                    seq.Add(1);
                    var interactiveTest = new InteractiveTest(seq);
                    interactiveTest.Main();
                }
                else if (key == ConsoleKey.M)
                {
                    Console.Clear();

                    var sizes = new [] {100, 200, 300, 400, 500};

                    const int numTests = 200;
                    const int series = 1;



                    foreach (var size in sizes)
                    {
                        var thoroughness = SequenceBuilder.Build(0.6m, 0m, 0.01m)
                            .Concat(SequenceBuilder.Build(1m, 0.7m, 0.1m))
                            .ToList();

                        var manyTest = new ManyTest
                        {
                            NumberOfTests = numTests,
                            CanDiag = true,
                            MapHeight = size,
                            MapWidth = size,
                            MaxSearchSpace = int.MaxValue,
                            OutputFile = $"~/Documents/AStarTests/{size}x{size} {numTests} Series {series}.csv",
                            Thoroughnesses = thoroughness
                        };

                        manyTest.Run();
                    }

                }
                else if (key == ConsoleKey.Q)
                {
                    break;
                }
            }
        }
    }
}
