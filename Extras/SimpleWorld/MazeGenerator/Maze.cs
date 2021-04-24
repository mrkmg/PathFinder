using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleWorld.MazeGenerator
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
        public int TurnWeight = 30;
        public int ForkWeight = 20;
        public bool FillEmpty = true;

        public bool IsGenerated { get; private set; }
        
        public readonly int Width;
        public readonly int Height;
        public readonly int StartX;
        public readonly int StartY;
        public readonly NodeFlag[,] Grid;
        
        private readonly Random _random;
        private readonly HashSet<(int X, int Y)> _open = new (new NodeComparer());
        private readonly List<(int MinX, int MinY, int MaxX, int MaxY)> _roomRects = new ();
        private readonly Queue<(int X, int Y, NodeFlag Direction)> _openPathExits = new ();
        private readonly Queue<(int X, int Y, NodeFlag Direction)> _roomExits = new ();


        public Maze(int width, int height, Random random = null, int? startX = null, int? startY = null)
        {
            Width = width;
            Height = height;
            Grid = new NodeFlag[width, height];
            _random = random ?? new Random();
            StartX = startX ?? Width / 2;
            StartY = startY ?? Height / 2;
        }

        /// <summary>
        /// Adds a random room to the maze
        /// </summary>
        public void AddRoom()
        {
            static bool Within(int v, int min, int max) => v >= min && v <= max;
            static bool PointWithinRect(int px, int py, int minx, int miny, int maxx, int maxy) =>
                Within(px, minx, maxx) && Within(py, miny, maxy);
            const int buffer = 3;

            var templateAndExits = _random.Next(5) switch
            {
                0 => RoomTemplates.Square,
                1 => RoomTemplates.H,
                2 => RoomTemplates.L,
                3 => RoomTemplates.U,
                4 => RoomTemplates.Plus,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            for (var triesLeft = 50; triesLeft > 0; triesLeft--)
            {
                // Randomly flip/scale/rotate/translate room
                templateAndExits = templateAndExits.Flip(_random.Next(2) == 0, _random.Next(2) == 0);
                if (_random.Next(2) == 0)
                    templateAndExits = templateAndExits.Rotate();
                var (template, exits) = templateAndExits
                    .Scale(1 + _random.NextDouble() * Math.Min(Width / 20, Height / 20))
                    .Translate((
                        Width / 2 + _random.Next(-(Width / 2), Width / 2), 
                        Height / 2 + _random.Next(-(Height / 2), Height / 2)));
                
                // get min/max with buffer of room
                var (minX, minY) = template.Min().Translate((-buffer, -buffer));
                var (maxX, maxY) = template.Max().Translate((buffer, buffer));
                
                // the room is too close to the center
                if (PointWithinRect(StartX, StartY, minX, minY, maxX, maxY)) continue;

                // the room is too close to an edge
                if (minX < buffer || 
                    maxX >= Width - buffer || 
                    minY < buffer ||
                    maxY >= Height - buffer)
                    continue;
                
                // if any other room intersects current room
                if (_roomRects.Any(o =>
                {
                    var (oMinX, oMinY, oMaxX, oMaxY) = o;

                    return 
                        // other corner is inside current
                        PointWithinRect(oMinX, oMinY, minX, minY, maxX, maxY) ||
                        PointWithinRect(oMinX, oMaxY, minX, minY, maxX, maxY) ||
                        PointWithinRect(oMaxX, oMinY, minX, minY, maxX, maxY) ||
                        PointWithinRect(oMaxX, oMaxY, minX, minY, maxX, maxY) ||
                        // current corner is inside other
                        PointWithinRect(minX, minY, oMinX, oMinY, oMaxX, oMaxY) ||
                        PointWithinRect(minX, maxY, oMinX, oMinY, oMaxX, oMaxY) ||
                        PointWithinRect(maxX, minY, oMinX, oMinY, oMaxX, oMaxY) ||
                        PointWithinRect(maxX, maxY, oMinX, oMinY, oMaxX, oMaxY) ||
                        // rooms cross each other in centers
                        oMinX < minX && oMaxX > maxX && oMinY > minY && oMaxY < maxY ||
                        minX < oMinX && maxX > oMaxX && minY > oMinY && maxY < oMaxY;
                })) continue;

                AddRoom(template, exits);
                break;
            }
        }

        /// <summary>
        /// Adds the given room to the maze. 
        /// </summary>
        /// <param name="corners">a list of points which represents the corners.</param>
        /// <param name="exits">a collection of points which represents exits of the room.</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddRoom(IList<(int X, int Y)> corners, ICollection<(int X, int Y)> exits)
        {
            var currentPoint = corners[corners.Count - 1];

            var (minX, minY) = corners[0];
            var (maxX, maxY) = corners[0];
            var wallPoints = new HashSet<(int, int)>(new NodeComparer());
            foreach (var targetPoint in corners)
            {
                if (currentPoint.X != targetPoint.X && currentPoint.Y != targetPoint.Y)
                    throw new ArgumentException("Invalid edges. Edges must not be diagonal", nameof(corners));
                if (currentPoint.X < minX) minX = currentPoint.X;
                if (currentPoint.X > maxX) maxX = currentPoint.X;
                if (currentPoint.Y < minY) minY = currentPoint.Y;
                if (currentPoint.Y > maxY) maxY = currentPoint.Y;
                var points = MakeRoomWall(currentPoint, targetPoint);
                foreach (var p in points) wallPoints.Add(p);
                currentPoint = targetPoint;
            }
            _roomRects.Add((minX, minY, maxX, maxY));

            (int X, int Y)? insidePoint = null;
            for (var x = minX + 1; x < maxX && insidePoint == null; x++)
            for (var y = minY + 1; y < maxY && insidePoint == null; y++)
            {
                if (Grid[x, y] != 0) continue;
                var intersections = 0;
                var lastWasIntersection = false;
                for (var tX = x; tX <= maxX; tX++)
                {
                    if (wallPoints.Contains((tX, y)))
                    {
                        if (!lastWasIntersection)
                            intersections++;
                        lastWasIntersection = true;
                    }
                    else
                    {
                        lastWasIntersection = false;
                    }
                }

                if (intersections % 2 == 1) 
                    insidePoint = (x, y);
            }

            if (!insidePoint.HasValue) return;
            
            // makes exits a hashset to improve lookup performance
            FillRoom(insidePoint.Value.X, insidePoint.Value.Y, new HashSet<(int X, int Y)>(exits, new NodeComparer()));
        }

        public void Generate()
        {
            if (IsGenerated)
                throw new InvalidOperationException("Already generated");
            IsGenerated = true;


            if (Grid[StartX, StartY] != 0)
                throw new InvalidOperationException(
                    "Start Point is invalid and already assigned. Likely a room is intersecting the point");
            
            // add first node to open path exits list
            // TODO: check to see if that direction is valid
            Grid[StartX, StartY] = RandomDirection();
            _openPathExits.Enqueue((StartX, StartY, Grid[StartX, StartY]));
            
            ProcessOpenExits();
            
            if (FillEmpty)
                CloseEmptyNodes();
            
            ProcessRoomExits();
        }

        private List<(int X, int Y)> MakeRoomWall((int X, int Y) start, (int X, int Y) end)
        {
            NodeFlag direction;
            if (start.X < end.X) direction = NodeFlag.East;
            else if (start.X > end.X) direction = NodeFlag.West;
            else if (start.Y < end.Y) direction = NodeFlag.South;
            else if (start.Y > end.Y) direction = NodeFlag.North;
            else throw new Exception("Failed to find direction");
            var points = new List<(int X, int Y)>();
            while (start.X != end.X || start.Y != end.Y)
            {
                points.Add(start);
                Grid[start.X, start.Y] |= direction | NodeFlag.Room | NodeFlag.RoomEdge;
                start = GetPositionInDirection(start.X, start.Y, direction);
                // TODO: Fix this as currently it will always throw
                // if (IsRoomEdge(start.X, start.Y)) 
                //     throw new Exception("Room walls intersect.");
                Grid[start.X, start.Y] |= OppositeDirection(direction);
            }

            return points;
        }

        private static readonly NodeFlag[] Directions = {NodeFlag.North, NodeFlag.South, NodeFlag.West, NodeFlag.East};
        private void FillRoom(int sX, int sY, ICollection<(int X, int Y)> exits)
        {
            var openRoomPositions = new Queue<(int X, int Y)>();
            var seenRoomPositions = new HashSet<(int X, int Y)>(new NodeComparer());
            openRoomPositions.Enqueue((sX, sY));
            while (openRoomPositions.Count > 0)
            {
                var (x, y) = openRoomPositions.Dequeue();
                Debug.Assert(x != StartX || y != StartY);
                Grid[x, y] |= NodeFlag.Room | NodeFlag.North | NodeFlag.East | NodeFlag.West | NodeFlag.South;
                foreach (var direction in Directions)
                {
                    var neighbor = GetPositionInDirection(x, y, direction);
                    if (IsRoomEdge(neighbor.X, neighbor.Y))
                    {
                        Grid[neighbor.X, neighbor.Y] |= OppositeDirection(direction);
                        if (exits.Contains(neighbor))
                            _roomExits.Enqueue((neighbor.X, neighbor.Y, direction));
                    }
                    else if (IsOpen(neighbor.X, neighbor.Y) && !seenRoomPositions.Contains(neighbor))
                    {
                        openRoomPositions.Enqueue(neighbor);
                        seenRoomPositions.Add(neighbor);
                    }
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
                var didFind = false;
                foreach (var (x, y) in _open)
                {
                    Debug.Assert(Grid[x, y] == 0);
                    var i = 0;
                    var d = RandomDirection();
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
                if (!didFind) break;
            }
        }

        private void ProcessOpenExits()
        {

            // keep going while we have "exits" from this path
            while (_openPathExits.Count > 0)
            {
                // get the first exit in the path
                var (cx, cy, previousDirection) = _openPathExits.Dequeue();

                while (true)
                {

                    var nextDirection = TryCarve(cx, cy, previousDirection);
                    if (nextDirection == null) break;

                    if (!IsOpenInDirection(cx, cy, nextDirection.Value)) break;
                    (cx, cy) = GetPositionInDirection(cx, cy, nextDirection.Value);
                    previousDirection = OppositeDirection(nextDirection.Value);
                    Grid[cx, cy] = previousDirection;
                    _open.Remove((cx, cy));
                    
                    if (IsOpenInDirection(cx, cy, NodeFlag.North))
                        _open.Add(GetPositionInDirection(cx, cy, NodeFlag.North));
                    if (IsOpenInDirection(cx, cy, NodeFlag.South))
                        _open.Add(GetPositionInDirection(cx, cy, NodeFlag.South));
                    if (IsOpenInDirection(cx, cy, NodeFlag.East))
                        _open.Add(GetPositionInDirection(cx, cy, NodeFlag.East));
                    if (IsOpenInDirection(cx, cy, NodeFlag.West))
                        _open.Add(GetPositionInDirection(cx, cy, NodeFlag.West));
                }
            }
        }

        private NodeFlag? TryCarve(int x, int y, NodeFlag previousDirection)
        {
            // get a random mode (from the weights) then try to find a move using that mode.
            // If not, check other modes.
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

        private bool IsInMaze(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        private bool IsClosed(int x, int y) => IsInMaze(x, y) && Grid[x, y] != 0;
        private bool IsRoomEdge(int x, int y) => IsInMaze(x, y) && Grid[x, y].IsRoomEdge();
        private bool IsOpen(int x, int y) => IsInMaze(x, y) && Grid[x, y] == 0;

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