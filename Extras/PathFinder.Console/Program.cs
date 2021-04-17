using System;
using System.Linq;
using PathFinder.Console.Tests.Interactive;
using PathFinder.Console.Tests.Many;

namespace PathFinder.Console
{
    internal class Program
    {
        private static void Main()
        {
//            testSortedLinkedList();
//            return;
            
            while (true)
            {
                System.Console.ResetColor();
                System.Console.Clear();
                System.Console.WriteLine("I, M, Q");
                var key = System.Console.ReadKey().Key;

                if (key == ConsoleKey.I)
                {
                    var interactiveTest = new InteractiveTest();
                    interactiveTest.Main();
                }
                else if (key == ConsoleKey.M)
                {
                    System.Console.Clear();

                    System.Console.WriteLine("Multi Test Run");
                    
                    var numTests = 200;
                    System.Console.Write($"Num Tests ({numTests})? ");
                    var numTestsStr = System.Console.ReadLine();
                    if (!string.IsNullOrEmpty(numTestsStr)) numTests = Int32.Parse(numTestsStr); 

                    var sizes = new [] {200, 400, 800, 1000, 2000};
                    System.Console.Write($"Sizes ({string.Join(", ", sizes)})?");
                    var sizesStr = System.Console.ReadLine();
                    if (!string.IsNullOrEmpty(sizesStr))
                    {
                        sizes = sizesStr.Split(',').Select(s => int.Parse(s.Trim())).ToArray();
                    }


                    var greedyFactors =
                        EnumerableExtensions.Sequence(0d, 2d, 0.25d)
                            .ToList();
                    System.Console.Write($"GreedFactor ({string.Join(", ", greedyFactors)})?");
                    var tInput = System.Console.ReadLine();
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
