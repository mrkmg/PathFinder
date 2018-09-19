using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using PathFinderTest.Map;

namespace PathFinderTest.Tests.Interactive
{
    internal class InteractiveTest
    {
        private bool _canDiag = true;

        private HashSet<Position> _seenClosed;
        private HashSet<Position> _seenOpen;

        private bool _showSearch;
        private readonly IList<decimal> _thoroughnesses;

        private World _world;
        private IWorldWriter _worldWriter;

        public InteractiveTest(IList<decimal> thoroughnesses)
        {
            _thoroughnesses = thoroughnesses;
        }

        public void Main()
        {
            var rnd = new Random();

            while (true)
            {
                if (ShowMenu()) break;
                MakeWorld();

                Position randomFromNode;
                Position randomToNode;

                do
                {
                    randomFromNode = _world.GetAllNodes().OrderBy(n => rnd.Next()).First();
                    randomToNode = _world.GetAllNodes().OrderBy(n => rnd.Next()).First();
                } while (randomFromNode.EstimatedCostTo(randomToNode) < 100);


                var aStars = new Dictionary<decimal, AStar<Position>>();

                _worldWriter.DrawWorld();

                var i = 0;
                foreach (var thoroughness in _thoroughnesses)
                {
                    var timer = new Stopwatch();
                    var aStar = RunPathFinder(randomFromNode, randomToNode, thoroughness, timer);
                    aStars.Add(thoroughness, aStar);
                    _worldWriter.WriteResult(i, thoroughness, aStar.Cost, aStar.Ticks, timer.ElapsedTicks);
                    i++;
                }

                i = 0;
                foreach (var thoroughness in _thoroughnesses)
                {
                    if (aStars[thoroughness].State == SolverState.Success) DrawPath(aStars[thoroughness].Path, i);
                    i++;
                }

                while (Console.ReadKey().Key != ConsoleKey.Enter) ;
            }
        }

        private void MakeWorld()
        {
            _world = new World(Console.WindowWidth - 1, Console.WindowHeight)
            {
                CanCutCorner = _canDiag
            };

            _worldWriter = new SimpleWorldWriter(_world, _thoroughnesses.Count);
        }

        private void DrawPath(IEnumerable<Position> path, int testNumber)
        {
            foreach (var node in path)
            {
                _worldWriter.DrawPosition(node.X, node.Y, testNumber);
                Thread.Sleep(5);
            }
        }

        private bool ShowMenu()
        {
            while (true)
            {
                Console.ResetColor();
                Console.Clear();
                Console.WriteLine("(d) Can Diag: " + _canDiag);
                Console.WriteLine("(s) Show Search: " + _showSearch);
                Console.WriteLine("(ENTER) Run");
                Console.WriteLine("(q) Quit");
                var key = Console.ReadKey();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        return true;
                    case ConsoleKey.Enter:
                        return false;
                    case ConsoleKey.D:
                        _canDiag = !_canDiag;
                        break;
                    case ConsoleKey.S:
                        _showSearch= !_showSearch;
                        break;
                }
            }
        }

        private AStar<Position> RunPathFinder(Position origin, Position dest, decimal thoroughness, Stopwatch timer)
        {
            var aStar = new AStar<Position>(origin, dest) {Thoroughness = (double) thoroughness};
            _seenClosed = new HashSet<Position> {origin};
            _seenOpen = new HashSet<Position> {origin};
            PrintEndPoints(aStar);

            while (aStar.State == SolverState.Running)
            {
                timer.Start();
                aStar.Tick();
                timer.Stop();

                if (_showSearch) PrintFinding(aStar);

                if (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.N) continue;

                aStar.Cancel();
                break;
            }

            foreach (var node in _seenOpen) _worldWriter.DrawPosition(node.X, node.Y);
            foreach (var node in _seenClosed) _worldWriter.DrawPosition(node.X, node.Y);
            _worldWriter.DrawPosition(aStar.Current.X, aStar.Current.Y);

            return aStar;
        }

        private void PrintFinding(AStar<Position> astar)
        {
            var openNodes = astar.Open.Where(n => !_seenOpen.Contains(n));
            var closedNodes = astar.Closed.Where(n => !_seenClosed.Contains(n));

            foreach (var openNode in openNodes)
            {
                _seenOpen.Add(openNode);
                _worldWriter.DrawPosition(openNode.X, openNode.Y, PositionType.Open);
            }

            foreach (var closednode in closedNodes)
            {
                _seenOpen.Remove(closednode);
                _worldWriter.DrawPosition(closednode.X, closednode.Y, PositionType.Closed);

                if (!closednode.Equals(astar.Current)) _seenClosed.Add(closednode);
            }

            _worldWriter.DrawPosition(astar.Current.X, astar.Current.Y, PositionType.Current);
        }

        private void PrintEndPoints(ISolver<Position> astar)
        {
            _worldWriter.DrawPosition(astar.Origin.X, astar.Origin.Y, PositionType.End);
            _worldWriter.DrawPosition(astar.Destination.X, astar.Destination.Y, PositionType.End);
        }
    }
}
