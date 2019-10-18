using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using PathFinderTest.Map;
using PathFinderTest.Sequencer;

namespace PathFinderTest.Tests.Interactive
{
    internal class InteractiveTest
    {
        private bool _bigJumps;
        private bool _canDiag;
        private bool _showSearch = true;
        private bool _slowMode;

        private HashSet<Position> _seenClosed;
        private HashSet<Position> _seenOpen;

        private readonly IList<double> _thoroughnesses;

        private World _world;
        private IWorldWriter _worldWriter;

        public InteractiveTest()
        {
            _thoroughnesses = 
                SequenceBuilder.Build(0.3m, 0m, 0.1m)
                .Concat(SequenceBuilder.Build(0.5m, 0.35m, 0.05m))
                .Select(n => (double)n)
                .ToList();
            _thoroughnesses.Add(1);
        }

        private bool BigJumpCheck(Position a, Position b) => !_bigJumps || Math.Abs(a.Z - b.Z) <= 1;

        public void Main()
        {
            var rnd = new Random();

            while (true)
            {
                if (ShowMenu()) break;

                MakeWorld();
                
                Position randomFromNode;
                Position randomToNode;

                IList<Position> path;
                do
                {
                    randomFromNode = _world.GetAllNodes().OrderBy(n => rnd.Next()).First();
                    randomToNode = _world.GetAllNodes().OrderBy(n => rnd.Next()).First();
                    path = AStar.Solve(randomFromNode, randomToNode, BigJumpCheck, 0.0);
                } while (path == null || path.Count < 100);
                
                var aStars = new Dictionary<double, AStar<Position>>();

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

                IList<Position> previous = null;
                while (true)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Enter) break;

                    if (!int.TryParse(key.KeyChar.ToString(), out var num) && num < _thoroughnesses.Count && aStars[_thoroughnesses[num]].State == SolverState.Success) continue;

                    if (previous != null) ClearPath(previous);
                    
                    if (num < _thoroughnesses.Count)
                    {
                        previous = aStars[_thoroughnesses[num]].Path;
                        if (previous != null)
                            DrawPath(previous, num);
                    }

                }
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
            }
        }

        private void ClearPath(IEnumerable<Position> path)
        {
            foreach (var node in path)
            {
                _worldWriter.DrawPosition(node.X, node.Y, PositionType.Normal);
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
                Console.WriteLine("(l) Slow Mode: " + _slowMode);
                Console.WriteLine("(j) Big Jumps: " + _bigJumps);
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
                    case ConsoleKey.L:
                        _slowMode = !_slowMode;
                        break;
                    case ConsoleKey.S:
                        _showSearch = !_showSearch;
                        break;
                    case ConsoleKey.J:
                        _bigJumps = !_bigJumps;
                        break;
                }
            }
        }

        private AStar<Position> RunPathFinder(Position origin, Position dest, double thoroughness, Stopwatch timer)
        {
            AStar<Position> aStar;
            
            if (_bigJumps)
                aStar = new AStar<Position>(origin, dest, thoroughness);
            else
                aStar = new AStar<Position>(origin, dest, BigJumpCheck, thoroughness);
            
            _seenClosed = new HashSet<Position> {origin};
            _seenOpen = new HashSet<Position> {origin};
            PrintEndPoints(aStar);

            if (_showSearch)
            {
                while (aStar.State == SolverState.Running)
                {
                    timer.Start();
                    aStar.Tick();
                    timer.Stop();

                    if (_slowMode) Thread.Sleep(20);
                    PrintFinding(aStar);
                    
                    if (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.N) continue;

                    aStar.Cancel();
                    break;
                }

                foreach (var node in _seenOpen) _worldWriter.DrawPosition(node.X, node.Y);
                foreach (var node in _seenClosed) _worldWriter.DrawPosition(node.X, node.Y);
                _worldWriter.DrawPosition(aStar.Current.X, aStar.Current.Y);
            }
            else
            {
                timer.Start();
                aStar.Solve();
                timer.Stop();
            }
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

            foreach (var closedNode in closedNodes)
            {
                _seenOpen.Remove(closedNode);
                _worldWriter.DrawPosition(closedNode.X, closedNode.Y, PositionType.Closed);

                if (!closedNode.Equals(astar.Current)) _seenClosed.Add(closedNode);
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
