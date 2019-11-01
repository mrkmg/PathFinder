using System;
using System.Linq;
using System.Text;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using SimpleWorld.Map;

namespace PathFinderTest.Tests.InsertCosting
{
    public class InsertTesting
    {
        public static void Run()
        {
            var r = new Random();
            var map = new World(1000, 1000)
            {
                CanCutCorner = false
            };

            Position origin;
            Position destination;

            AStar<Position> aStarSolver;
            do
            {
                origin = map.GetAllNodes().OrderBy(n => r.Next()).First();
                destination = map.GetAllNodes().OrderBy(n => r.Next()).First();
                
                aStarSolver = new AStar<Position>(origin, destination, 1);
                aStarSolver.Solve();
            } while (aStarSolver.State == SolverState.Failed); 
            
            Console.Clear();
            
            foreach (var performanceCounter in aStarSolver.InsertCosts.OrderBy(s => s.Length))
            {
                Console.Write($"{performanceCounter.Checks},{performanceCounter.Length}  ");
            }

            Console.ReadKey();
        }
    }
}