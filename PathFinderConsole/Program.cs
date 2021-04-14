using System;
using System.Globalization;
using System.Linq;
using PathFinderConsole.Tests.Interactive;
using PathFinderConsole.Tests.Many;

namespace PathFinderConsole
{
    internal class Program
    {
        private static void Main()
        {
//            testSortedLinkedList();
//            return;
            
            while (true)
            {
                Console.ResetColor();
                Console.Clear();
                Console.WriteLine("I, M, Q");
                var key = Console.ReadKey().Key;

                if (key == ConsoleKey.I)
                {
                    var interactiveTest = new InteractiveTest();
                    interactiveTest.Main();
                }
                else if (key == ConsoleKey.M)
                {
                    Console.Clear();

                    Console.WriteLine("Multi Test Run");
                    
                    var numTests = 200;
                    Console.Write("Num Tests (200)? ");
                    var numTestsStr = Console.ReadLine();
                    if (!string.IsNullOrEmpty(numTestsStr)) numTests = Int32.Parse(numTestsStr); 

                    var sizes = new [] {100, 200, 300, 400, 500};
                    Console.Write("Sizes (100, 200, 300, 400, 500)?");
                    var sizesStr = Console.ReadLine();
                    if (!string.IsNullOrEmpty(sizesStr))
                    {
                        sizes = sizesStr.Split(',').Select(s => int.Parse(s.Trim())).ToArray();
                    }


                    var greedyFactors =
                        Enumerable.Sequence(0d, 2d, 0.25d)
                            .ToList();
                    var tStr = greedyFactors
                        .Select(s => s.ToString(CultureInfo.InvariantCulture))
                        .Aggregate((p, a) => p + "," + a);
                    
                    Console.Write($"GreedFactor ({tStr})?");
                    var tInput = Console.ReadLine();
                    if (!string.IsNullOrEmpty(tInput))
                    {
                        greedyFactors = tInput.Split(',').Select(s => double.Parse(s.Trim())).ToList();
                    }
                    
                    var dateStr = DateTime.Now.ToString("yyyyMMdd-HHmm");
                    
                    foreach (var size in sizes)
                    {
                        var manyTest = new ManyTest
                        {
                            NumberOfTests = numTests,
                            CanDiag = true,
                            MapHeight = size,
                            MapWidth = size,
                            OutputFile = $"~/Documents/AStarTests/{dateStr}/{size}x{size} {numTests}.csv",
                            GreedyFactors = greedyFactors
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
