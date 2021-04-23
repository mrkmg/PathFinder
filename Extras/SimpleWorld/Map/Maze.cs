using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleWorld.Map
{
    [Flags]
    public enum NodeFlag
    {
        North = 1, 
        West  = 2, 
        South = 4, 
        East  = 8, 
        Room  = 16,
        RoomEdge = 32,
        RoomExit = 64,
    };

    public class Maze
    {
        public int LineWeight = 50;
        public int TurnWeight = 50;
        public int ForkWeight = 50;
        public bool FillEmpty = true;

        public bool IsGenerated { get; private set; }
        
        public readonly int Width;
        public readonly int Height;
        public readonly NodeFlag[,] Grid;
        
        private readonly Random _random;
        private readonly HashSet<(int X, int Y)> _open = new (new NodeComparer());
        private readonly Queue<(int X, int Y, NodeFlag Direction)> _openPathExits = new ();
        private readonly Queue<(int X, int Y, NodeFlag Direction)> _roomExits = new ();

        private readonly int _startX;
        private readonly int _startY;

        public Maze(int width, int height, Random random = null, int? startX = null, int? startY = null)
        {
            Width = width;
            Height = height;
            Grid = new NodeFlag[width, height];
            _random = random ?? new Random();
            _startX = startX ?? Width / 2;
            _startY = startY ?? Height / 2;
            
            // add all positions to open
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                _open.Add((x, y));
            }
        }

        public void AddRoom(IList<(int X, int Y)> corners, ICollection<(int X, int Y)> exits)
        {
            var currentPoint = corners[corners.Count - 1];

            foreach (var targetPoint in corners)
            {
                if (currentPoint.X != targetPoint.X && currentPoint.Y != targetPoint.Y)
                    throw new ArgumentException("Invalid edges. Edges must not be diagonal", nameof(corners));
                MakeRoomWall(currentPoint, targetPoint);
                currentPoint = targetPoint;
            }
            
            var x = corners[0].X < corners[1].X ? 1 :
                corners[0].X > corners[1].X ? -1 :
                corners[0].X < corners[corners.Count - 1].X ? 1 :
                corners[0].X > corners[corners.Count - 1].X ? -1 :
                throw new Exception("All three corners have same X. Invalid shape");
            var y = corners[0].Y < corners[1].Y ? 1 :
                corners[0].Y > corners[1].Y ? -1 :
                corners[0].Y < corners[corners.Count - 1].Y ? 1 :
                corners[0].Y > corners[corners.Count - 1].Y ? -1 :
                throw new Exception("All three corners have same Y. Invalid shape");
            
            FillRoom(corners[0].X + x, corners[0].Y + y, exits);
        }

        public void Generate()
        {
            if (IsGenerated)
                throw new InvalidOperationException("Already generated");
            IsGenerated = true;

            // add first node to open path exits list
            // TODO: check to see if that direction is valid
            Grid[_startX, _startY] = RandomDirection();
            _openPathExits.Enqueue((_startX, _startY, Grid[_startX, _startY]));
            _open.Remove((_startX, _startY));
            
            ProcessOpenExits();
            
            if (FillEmpty)
                CloseEmptyNodes();
            
            ProcessRoomExits();
        }

        private void MakeRoomWall((int X, int Y) start, (int X, int Y) end)
        {
            NodeFlag direction;
            if (start.X < end.X) direction = NodeFlag.East;
            else if (start.X > end.X) direction = NodeFlag.West;
            else if (start.Y < end.Y) direction = NodeFlag.South;
            else if (start.Y > end.Y) direction = NodeFlag.North;
            else throw new Exception("Failed to find direction");
            
            while (start.X != end.X || start.Y != end.Y)
            {
                _open.Remove(start);
                Grid[start.X, start.Y] |= direction | NodeFlag.Room | NodeFlag.RoomEdge;
                start = GetPositionInDirection(start.X, start.Y, direction);
                // TODO: Fix this as currently it will always throw
                // if (IsRoomEdge(start.X, start.Y)) 
                //     throw new Exception("Room walls intersect.");
                Grid[start.X, start.Y] |= OppositeDirection(direction);
            }
        }

        private static readonly NodeFlag[] Directions = {NodeFlag.North, NodeFlag.South, NodeFlag.West, NodeFlag.East};
        private void FillRoom(int sX, int sY, ICollection<(int X, int Y)> exits)
        {
            var openRoomPositions = new Queue<(int X, int Y)>();
            openRoomPositions.Enqueue((sX, sY));
            while (openRoomPositions.Count > 0)
            {
                var current = openRoomPositions.Dequeue();
                _open.Remove(current);
                Grid[current.X, current.Y] |= NodeFlag.Room | NodeFlag.North | NodeFlag.East | NodeFlag.West | NodeFlag.South;
                foreach (var direction in Directions)
                {
                    var neighbor = GetPositionInDirection(current.X, current.Y, direction);
                    if (IsRoomEdge(neighbor.X, neighbor.Y))
                    {
                        Grid[neighbor.X, neighbor.Y] |= OppositeDirection(direction);
                        if (exits.Any(e => e.X == neighbor.X && e.Y == neighbor.Y))
                            _roomExits.Enqueue((neighbor.X, neighbor.Y, direction));
                    }
                    else if (IsOpen(neighbor.X, neighbor.Y) && !openRoomPositions.Any(o => o.X == neighbor.X && o.Y == neighbor.Y))
                        openRoomPositions.Enqueue(neighbor);
                }
            }
        }

        private void ProcessRoomExits()
        {
            while (_roomExits.Count > 0)
            {
                var (ex, ey, ed) = _roomExits.Dequeue();
                var (nx, ny) = GetPositionInDirection(ex, ey, ed);
                if (IsOpen(nx, ny)) continue;
                Grid[ex, ey] |= ed | NodeFlag.RoomExit;
                Grid[nx, ny] |= OppositeDirection(ed);
            }
        }

        private void CloseEmptyNodes()
        {
            // check if there are any open nodes
            while (_open.Count > 0)
            {
                // find an open node which is "next to" a closed node
                // then carve a path from it to the closed node, then
                // carve a path in open nodes
                foreach (var (x, y) in _open)
                {
                    Debug.Assert(Grid[x, y] == 0);
                    var i = 0;
                    var d = RandomDirection();
                    var didFind = false;
                    do
                    {
                        var (nx, ny) = GetPositionInDirection(x, y, d);
                        if (IsClosed(nx, ny) && !IsRoomEdge(nx, ny))
                        {
                            Grid[nx, ny] |= OppositeDirection(d);
                            Grid[x, y] |= d;
                            _openPathExits.Enqueue((x, y, Grid[x, y]));
                            _open.Remove((x, y));
                            ProcessOpenExits();
                            didFind = true;
                            break;
                        }

                        d = d switch
                        {
                            NodeFlag.North => NodeFlag.West,
                            NodeFlag.West => NodeFlag.South,
                            NodeFlag.South => NodeFlag.East,
                            NodeFlag.East => NodeFlag.North,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    } while (++i < 4);

                    if (didFind) break;
                }
            }
        }

        private void ProcessOpenExits()
        {

            // keep going while we have "exits" from this path
            while (_openPathExits.Count > 0)
            {
                // get the first exit in the path
                var (cx, cy, previousDirection) = _openPathExits.Dequeue();

                // get a random mode (from the weights)
                // try to find a move using that mode. If not, check "lesser"
                // modes. Order is Fork, Turn, Line
                while (true)
                {

                    var nextDirection = TryCarve(cx, cy, previousDirection);
                    if (nextDirection == null) break;
                    
                    if (!IsOpenInDirection(cx, cy, nextDirection.Value)) break;
                    (cx, cy) = GetPositionInDirection(cx, cy, nextDirection.Value);
                    previousDirection = OppositeDirection(nextDirection.Value);
                    Grid[cx, cy] = previousDirection;
                    _open.Remove((cx, cy));
                }
            }
        }

        private NodeFlag? TryCarve(int x, int y, NodeFlag previousDirection)
        {
            var mode = RandomMode();
            var triedTurn = false;
            var triedLine = false;
            while (true)
            {
                switch (mode)
                {
                    case Mode.Line when !triedLine:
                        triedLine = true;
                        var lineDirection = TryCarveLine(x, y, previousDirection);
                        if (lineDirection != null)
                            return lineDirection;
                        else
                            mode = Mode.Turn;
                        break;
                    case Mode.Turn when !triedTurn:
                        triedTurn = true;
                        var turnDirection = TryCurveTurn(x, y, previousDirection);
                        if (turnDirection != null)
                            return turnDirection;
                        else
                            mode = Mode.Line;
                        break;
                    case Mode.Fork:
                        var forkDirection = TryCurveFork(x, y, previousDirection);
                        if (forkDirection != null)
                            return forkDirection;
                        else
                            mode = RandomModeLineOrTurn();
                        break;
                    default:
                        return null;
                        
                }
            }
        }

        private NodeFlag? TryCarveLine(int x, int y, NodeFlag fromNodeFlag)
        {
            var toDirection = OppositeDirection(fromNodeFlag);
            if (!IsOpenInDirection(x, y, toDirection)) return null;
            Grid[x, y] |= toDirection;
            return toDirection;
        }

        private NodeFlag? TryCurveTurn(int x, int y, NodeFlag fromNodeFlag)
        {
            var toDirection = RandomDirection(new[] {fromNodeFlag, OppositeDirection(fromNodeFlag)});

            if (!IsOpenInDirection(x, y, toDirection))
                toDirection = OppositeDirection(toDirection);

            if (!IsOpenInDirection(x, y, toDirection)) return null;
            Grid[x, y] |= toDirection;
            return toDirection;
        }

        private NodeFlag? TryCurveFork(int x, int y, NodeFlag fromNodeFlag)
        {
            var nextDirection = RandomDirection(new[] {fromNodeFlag});
            var exitDirection = RandomDirection(new[] {fromNodeFlag, nextDirection});
            if (!IsOpenInDirection(x, y, nextDirection) || !IsOpenInDirection(x, y, exitDirection)) return null;
            Grid[x, y] |= nextDirection | exitDirection;
            var (ex, ey) = GetPositionInDirection(x, y, exitDirection);
            Grid[ex, ey] = OppositeDirection(exitDirection);
            _openPathExits.Enqueue((ex, ey, exitDirection));
            _open.Remove((ex, ey));
            return nextDirection;

        }

        private Mode RandomMode()
        {
            var r = _random.Next(LineWeight + TurnWeight + ForkWeight);

            if (r < LineWeight) return Mode.Line;
            if (r < LineWeight + TurnWeight) return Mode.Turn;
            return Mode.Fork;
        }
        
        private Mode RandomModeLineOrTurn()
        {
            return _random.Next(LineWeight + TurnWeight) < LineWeight ? Mode.Line : Mode.Turn;
        }

        private bool IsOpenInDirection(int x, int y, NodeFlag d)
        {
            var (dx, dy) = GetPositionInDirection(x, y, d);
            return IsOpen(dx, dy);
        }

        private bool IsClosed(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
            return Grid[x, y] != 0;
        }

        private bool IsRoomEdge(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
            return Grid[x, y].IsRoomEdge();
        }

        private bool IsOpen(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
            return Grid[x, y] == 0;
        }

        private static (int X, int Y) GetPositionInDirection(int x, int y, NodeFlag d) => 
            d switch {
                NodeFlag.North => (x, y - 1),
                NodeFlag.South => (x, y + 1),
                NodeFlag.East => (x + 1, y),
                NodeFlag.West => (x - 1, y),
                _ => throw new Exception("No open direction")
            };

        private static NodeFlag OppositeDirection(NodeFlag d) =>
            d switch {
                NodeFlag.North => NodeFlag.South,
                NodeFlag.West => NodeFlag.East,
                NodeFlag.South => NodeFlag.North,
                NodeFlag.East => NodeFlag.West,
                _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
            };

        private NodeFlag RandomDirection() =>
            _random.Next(4) switch
            {
                0 => NodeFlag.North,
                1 => NodeFlag.South,
                2 => NodeFlag.East,
                3 => NodeFlag.West,
                _ => throw new ArgumentOutOfRangeException()
            };

        private NodeFlag RandomDirection(ICollection<NodeFlag> notOneOf)
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

        private class NodeComparer : IEqualityComparer<(int X, int Y)>
        {
            public bool Equals((int X, int Y) x, (int X, int Y) y)
            {
                return x.X == y.X && x.Y == y.Y;
            }

            public int GetHashCode((int X, int Y) obj)
            {
                unchecked
                {
                    return (obj.X * 397) ^ obj.Y;
                }
            }
        }
    }

    public static class NodeFlagsExtensions
    {
        public static bool Has(this NodeFlag flags, NodeFlag flag) => (flags & flag) != 0;
        public static bool IsRoom(this NodeFlag flags) => Has(flags, NodeFlag.Room);
        public static bool IsRoomEdge(this NodeFlag flags) => Has(flags, NodeFlag.RoomEdge);
        public static bool IsRoomExit(this NodeFlag flags) => Has(flags, NodeFlag.RoomExit);
        public static bool IsRoomFloor(this NodeFlag flags) => IsRoom(flags) && !IsRoomEdge(flags);
    }
}
