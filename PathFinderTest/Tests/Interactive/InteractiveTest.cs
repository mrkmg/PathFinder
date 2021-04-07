using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PathFinder.Interfaces;
using PathFinder.Solvers;
using SimpleWorld.Map;
using PathFinderTest.Sequencer;

namespace PathFinderTest.Tests.Interactive
{
    internal class InteractiveTest
    {
        private bool _bigJumps;
        private bool _canDiag;
        private bool _showSearch;
        private bool _slowMode;
        private int _seed;

        private HashSet<Position> _seenClosed;
        private HashSet<Position> _seenOpen;

        private readonly IList<double> _thoroughnesses;

        private World _world;
        private IWorldWriter _worldWriter;

        public InteractiveTest()
        {
            _thoroughnesses = 
                SequenceBuilder.Build(0.8m, 0m, 0.1m)
                .ToDouble()
                .ToList();
        }

        private bool BigJumpCheck(Position a, Position b) => !_bigJumps || Math.Abs(a.Z - b.Z) <= 1;

        public void Main()
        {

            while (true)
            {
                
                _seed = (new Random().Next(10000000, 99999999));
                
                if (ShowMenu()) break;

                var rnd = new Random(_seed);
                MakeWorld(rnd);
                
                var worldSize = Math.Sqrt(_world.XSize * _world.XSize + _world.YSize + _world.YSize);
                var targetSize = (int)(worldSize * 0.95);
                
                Position randomFromNode = null;
                Position randomToNode = null;

                IList<Position> path = null;

                var tries = 0;
                while (path == null)
                {
                    tries++;

                    if (tries > 5000) break;
                    
                    do randomFromNode = _world.GetNode(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                    while (randomFromNode == null);
                    
                    do randomToNode = _world.GetNode(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                    while (randomToNode == null);

                    var x = Math.Abs(randomFromNode.X - randomToNode.X);
                    var y = Math.Abs(randomFromNode.Y - randomToNode.Y);
                    var dist = Math.Sqrt(x*x + y*y);
                    
                    if (dist < targetSize) continue;
                    
                    path = AStar.Solve(randomFromNode, randomToNode, 0.0d);
                }
                
                if (path == null) continue;

                var aStars = new Dictionary<double, AStar<Position>>();

                _worldWriter.DrawWorld();
                _worldWriter.DrawSeed(_seed);

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

        private void MakeWorld(Random rnd)
        {
            _world = new World(Console.WindowWidth - 1, Console.WindowHeight, rnd)
            {
                CanCutCorner = _canDiag,
            };

            _worldWriter = new SimpleWorldWriter(_world, _thoroughnesses.Count);
        }

        private void DrawPath(IEnumerable<Position> path, int testNumber)
        {
            foreach (var node in path)
            {
                _worldWriter.DrawPosition(node.X, node.Y, testNumber);
                Thread.Sleep(1);
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
                Console.WriteLine($"(v) Seed: {_seed}");
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
                    case ConsoleKey.V:
                        Console.WriteLine("\nNew Seed?");
                        _seed = int.Parse(Console.ReadLine() ?? "0");
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
                aStar = new AStar<Position>(origin, dest, thoroughness);
            
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
                var path = aStar.Solve();
                timer.Stop();
                
                DrawPath(path, 0);
                ClearPath(path);
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
