using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using PathFinderTest.Map;
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
                    var seq = SequenceBuilder.Build((decimal)0.6, (decimal)0, (decimal)0.1).ToList();
                    var interactiveTest = new InteractiveTest(seq);
                    interactiveTest.Main();
                }
                else if (key == ConsoleKey.M)
                {
                    Console.Clear();
                    Console.WriteLine("Input a file path to write results to");
                    var outputFile = Console.ReadLine().Trim();
                    Console.WriteLine("Number of tests to run?");
                    var numTest = int.Parse(Console.ReadLine().Trim());
                    var seq = SequenceBuilder.Build((decimal)1, (decimal)0, (decimal)0.05).ToList();
                    var manyTest = new ManyTest(seq, outputFile);
                    manyTest.Main(numTest);
                }
                else if (key == ConsoleKey.Q)
                {
                    break;
                }
            }


        }
    }
}
