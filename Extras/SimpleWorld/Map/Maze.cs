using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleWorld.Map
{
    [Flags]
    public enum Direction : byte { North = 0x1, West = 0x2, South = 0x4, East = 0x8 };

    public class Maze
    {
        public int LineWeight = 50;
        public int TurnWeight = 50;
        public int ForkWeight = 50;
        
        public int TotalWeight => LineWeight + TurnWeight + ForkWeight;
        public readonly int Width;
        public readonly int Height;
        public readonly byte[,] Grid;
        private readonly Random _random;

        private List<(int X, int Y)> _open;

        private readonly int _startX;
        private readonly int _startY;

        public Maze(int width, int height, Random random, int? startX = null, int? startY = null)
        {
            Width = width;
            Height = height;
            Grid = new byte[width, height];
            _random = random;
            _startX = startX ?? Width / 2;
            _startY = startY ?? Height / 2;
        }

        public void Generate()
        {
            _open = new List<(int X, int Y)>(Width * Height);
            
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                _open.Add((x, y));
            }

            Grid[_startX, _startY] = (byte) RandomDirection();
            SnakePathFrom(_startX, _startY);
            
            FillEmpty();
        }

        private void FillEmpty()
        {
            while (_open.Any())
            {
                foreach (var (cx, cy) in _open)
                {
                    Debug.Assert(Grid[cx, cy] == 0);
                    var i = 0;
                    var d = RandomDirection();
                    var didFind = false;
                    do
                    {
                        if (IsClosedNodeInDirection(cx, cy, d))
                        {
                            var (nx, ny) = GetPositionInDirection(cx, cy, d);
                            Grid[nx, ny] |= (byte) OppositeDirection(d);
                            Grid[cx, cy] |= (byte) d;
                            SnakePathFrom(cx, cy);
                            didFind = true;
                            break;
                        }

                        d = d switch
                        {
                            Direction.North => Direction.West,
                            Direction.West => Direction.South,
                            Direction.South => Direction.East,
                            Direction.East => Direction.North,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    } while (++i < 4);

                    if (didFind) break;
                }
            }
        }

        private void SnakePathFrom(int x, int y)
        {
            var exits = new List<(int X, int Y, Direction D)>{(x, y, (Direction) Grid[x, y])};
            _open.Remove((x, y));

            // get the start point
            while (exits.Any())
            {
                var (cx, cy, previousDirection) = exits[0];
                exits.RemoveAt(0);
                _open.Remove((cx, cy));
                
                while (true)
                {
                    Direction currentDirection = previousDirection;
                    var mode = RandomMode();

                    var triedTurn = false;
                    var triedLine = false;
                    var foundMove = false;
                    while (!foundMove && (!triedTurn || !triedLine))
                    {
                        if (mode == Mode.Line)
                        {
                            triedLine = true;
                            var nextDirection = OppositeDirection(previousDirection);
                            if (!IsOpenInDirection(cx, cy, nextDirection))
                            {
                                if (!triedTurn)
                                    mode = Mode.Turn;
                                continue;
                            }
                            Grid[cx, cy] |= (byte) nextDirection;
                            currentDirection = nextDirection;
                            foundMove = true;
                        }

                        if (mode == Mode.Turn)
                        {
                            triedTurn = true;
                            var nextDirection = RandomDirection(new[] {previousDirection, OppositeDirection(previousDirection)});
                            
                            if (!IsOpenInDirection(cx, cy, nextDirection)) 
                                nextDirection = OppositeDirection(nextDirection);

                            if (!IsOpenInDirection(cx, cy, nextDirection))
                            {
                                if (!triedLine)
                                    mode = Mode.Line;
                                continue;
                            }
                            Grid[cx, cy] |= (byte) nextDirection;
                            currentDirection = nextDirection;
                            foundMove = true;
                        }
                        
                        if (mode == Mode.Fork)
                        {
                            var nextDirection = RandomDirection(new[] {previousDirection});
                            var exitDirection = RandomDirection(new[] {previousDirection, nextDirection});
                            if (!IsOpenInDirection(cx, cy, nextDirection) || !IsOpenInDirection(cx, cy, exitDirection)) 
                            {
                                mode = _random.Next(2) == 0 ? Mode.Turn : Mode.Line;
                                continue;
                            }
                            Grid[cx, cy] |= (byte) (nextDirection | exitDirection);
                            var (ex, ey) = GetPositionInDirection(cx, cy, exitDirection);
                            exits.Add((ex, ey, exitDirection));
                            Grid[ex, ey] = (byte) OppositeDirection(exitDirection);
                            currentDirection = nextDirection;
                            foundMove = true;
                        }
                    }

                    if (!foundMove) break;
                    
                    previousDirection = OppositeDirection(currentDirection);
                    if (!IsOpenInDirection(cx, cy, currentDirection)) break;
                    (cx, cy) = GetPositionInDirection(cx, cy, currentDirection);
                    Grid[cx, cy] = (byte) previousDirection;
                    _open.Remove((cx, cy));
                }
            }
        }

        private Mode RandomMode()
        {
            var r = _random.Next(TotalWeight);

            if (r < LineWeight) return Mode.Line;
            if (r < LineWeight + TurnWeight) return Mode.Turn;
            return Mode.Fork;
        }

        private bool IsClosedNodeInDirection(int x, int y, Direction d)
        {
            var (dx, dy) = GetPositionInDirection(x, y, d);
            if (dx < 0 || dx >= Width || dy < 0 || dy >= Height) return false;
            return Grid[dx, dy] != 0;
        }

        private bool IsOpenInDirection(int x, int y, Direction d)
        {
            var (dx, dy) = GetPositionInDirection(x, y, d);
            if (dx < 0 || dx >= Width || dy < 0 || dy >= Height) return false;
            return Grid[dx, dy] == 0;
        }

        private static (int X, int Y) GetPositionInDirection(int x, int y, Direction d) => d switch
        {
            Direction.North => (x, y - 1),
            Direction.South => (x, y + 1),
            Direction.East => (x + 1, y),
            Direction.West => (x - 1, y),
            _ => throw new Exception("No open direction")
        };

        private static Direction OppositeDirection(Direction d) =>
            d switch {
                Direction.North => Direction.South,
                Direction.West => Direction.East,
                Direction.South => Direction.North,
                Direction.East => Direction.West,
                _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
            };

        private Direction RandomDirection() =>
            _random.Next(4) switch
            {
                0 => Direction.North,
                1 => Direction.South,
                2 => Direction.East,
                3 => Direction.West,
                _ => throw new ArgumentOutOfRangeException()
            };

        private Direction RandomDirection(ICollection<Direction> notOneOf)
        {
            if (notOneOf.Count >= 4) throw new Exception("No possible direction");
            while (true)
            {
                var d = RandomDirection();
                if (!notOneOf.Contains(d)) return d;
            }
        }


        private enum Mode
        {
            Line,
            Turn, 
            Fork
        };
    }
}
