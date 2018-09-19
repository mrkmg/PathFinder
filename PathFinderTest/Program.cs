using System;
using System.Collections.Generic;
using System.Linq;
using PathFinderTest.Sequencer;
using PathFinderTest.Tests.Many;

namespace PathFinderTest
{
    internal class Program
    {
        private static void Main(string[] args)
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
                    var interactiveTest = new InteractiveTest(seq);
                    interactiveTest.Main();
                }
                else if (key == ConsoleKey.M)
                {
                    Console.Clear();

                    const int width = 300;
                    const int height = 300;
                    const int numTests = 200;
                    const int series = 1;
                    var thoroughness = SequenceBuilder.Build(0.6m, 0m, 0.01m)
                        .ToList();

                    var manyTest = new ManyTest
                    {
                        NumberOfTests = numTests,
                        CanDiag = true,
                        MapHeight = height,
                        MapWidth = width,
                        MaxSearchSpace = int.MaxValue,
                        OutputFile = $"~/Documents/AStarTests/{width}x{height} {numTests} Series {series}.csv",
                        Thoroughnesses = thoroughness
                    };

                    manyTest.Run();
                }
                else if (key == ConsoleKey.Q)
                {
                    break;
                }
            }
        }
    }
}
