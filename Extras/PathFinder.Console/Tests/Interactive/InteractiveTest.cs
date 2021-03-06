﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PathFinder.Solvers.Generic;
using SimpleWorld.Map;

namespace PathFinder.Console.Tests.Interactive
{
    internal class InteractiveTest
    {
        private bool _bigJumps;
        private bool _showSearch;
        private bool _slowMode;
        private int _seed;

        private HashSet<Position> _seenClosed;
        private HashSet<Position> _seenOpen;
        
        private readonly IList<double> _greedyFactors;

        private World _world;
        private IWorldWriter _worldWriter;

        public InteractiveTest()
        {
            _greedyFactors = 
                EnumerableExtensions.Sequence(0d, 2d, 0.25d)
                .ToList();
        }

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
                    
                    do randomFromNode = _world.GetPosition(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                    while (randomFromNode == null);
                    
                    do randomToNode = _world.GetPosition(rnd.Next(0, _world.XSize - 1), rnd.Next(0, _world.YSize - 1));
                    while (randomToNode == null);

                    var x = Math.Abs(randomFromNode.X - randomToNode.X);
                    var y = Math.Abs(randomFromNode.Y - randomToNode.Y);
                    var dist = Math.Sqrt(x*x + y*y);
                    
                    if (dist < targetSize) continue;

                    Greedy<Position>.Solve(randomFromNode, randomToNode, out path);
                }
                
                if (path == null) continue;

                var aStars = new Dictionary<double, AStar<Position>>();

                _worldWriter.DrawWorld();
                _worldWriter.DrawSeed(_seed);

                var i = 0;
                foreach (var greedyFactor in _greedyFactors)
                {
                    var timer = new Stopwatch();
                    var aStar = RunPathFinder(randomFromNode, randomToNode, greedyFactor, timer);
                    aStars.Add(greedyFactor, aStar);
                    _worldWriter.WriteResult(i, greedyFactor, aStar.PathCost, aStar.Ticks, timer.ElapsedTicks);
                    i++;
                }

                IList<Position> previous = null;
                while (true)
                {
                    var key = System.Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Enter) break;

                    if (!int.TryParse(key.KeyChar.ToString(), out var num) && num < _greedyFactors.Count && aStars[_greedyFactors[num]].State == SolverState.Success) continue;

                    if (previous != null) ClearPath(previous);
                    
                    if (num < _greedyFactors.Count)
                    {
                        previous = aStars[_greedyFactors[num]].Path;
                        if (previous != null)
                            DrawPath(previous, num);
                    }

                }
            }
        }

        private void MakeWorld(Random rnd)
        {
            _world = new World(System.Console.WindowWidth - 1, System.Console.WindowHeight, rnd);

            _worldWriter = new SimpleWorldWriter(_world, _greedyFactors.Count);
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
                System.Console.ResetColor();
                System.Console.Clear();
                System.Console.WriteLine("(s) Show Search: " + _showSearch);
                System.Console.WriteLine("(l) Slow Mode: " + _slowMode);
                System.Console.WriteLine("(j) Big Jumps: " + _bigJumps);
                System.Console.WriteLine($"(v) Seed: {_seed}");
                System.Console.WriteLine("(ENTER) Run");
                System.Console.WriteLine("(q) Quit");
                var key = System.Console.ReadKey();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        return true;
                    case ConsoleKey.Enter:
                        return false;
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
                        System.Console.WriteLine("\nNew Seed?");
                        _seed = int.Parse(System.Console.ReadLine() ?? "0");
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
                    aStar.Run(1);
                    timer.Stop();

                    if (_slowMode) Thread.Sleep(20);
                    PrintFinding(aStar);
                    
                    if (!System.Console.KeyAvailable || System.Console.ReadKey().Key != ConsoleKey.N) continue;

                    aStar.Stop();
                    break;
                }

                foreach (var node in _seenOpen) _worldWriter.DrawPosition(node.X, node.Y);
                foreach (var node in _seenClosed) _worldWriter.DrawPosition(node.X, node.Y);
                _worldWriter.DrawPosition(aStar.Current.X, aStar.Current.Y);
            }
            else
            {
                timer.Start();
                aStar.Run();
                timer.Stop();
                
                DrawPath(aStar.Path, 0);
                ClearPath(aStar.Path);
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

        private void PrintEndPoints(IGraphSolver<Position> astar)
        {
            _worldWriter.DrawPosition(astar.Origin.X, astar.Origin.Y, PositionType.End);
            _worldWriter.DrawPosition(astar.Destination.X, astar.Destination.Y, PositionType.End);
        }
    }
}
