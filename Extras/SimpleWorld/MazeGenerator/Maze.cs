using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Drawing;

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

        public readonly Size Size;
        public readonly Point StartPoint;
        public readonly NodeFlag[,] Grid;
        
        private readonly Random _random;
        private readonly C5.HashSet<Point> _open = new ();
        private readonly List<Rectangle> _roomRects = new ();
        private readonly Queue<(Point Point, NodeFlag Direction)> _openPathExits = new ();
        private readonly Queue<(Point Point, NodeFlag Direction)> _roomExits = new ();
        
        private static readonly NodeFlag[] Directions = {NodeFlag.North, NodeFlag.South, NodeFlag.West, NodeFlag.East};
        
        public Maze(Size size, Random random = null, Point? startPoint = null)
        {
            Size = size;
            Grid = new NodeFlag[size.Width, size.Height];
            _random = random ?? new Random();
            StartPoint = startPoint ?? new Point(size / 2);
        }

        /// <summary>
        /// Adds a random room to the maze
        /// </summary>
        public void AddRoom()
        {
            const int buffer = 3;

            var template = RoomTemplates.AllTemplates[_random.Next(RoomTemplates.AllTemplates.Length)];
            
            for (var triesLeft = 50; triesLeft > 0; triesLeft--)
            {
                // Randomly flip/scale/rotate/translate room
                var testTemplate = template.Flip(_random.Next(2) == 0, _random.Next(2) == 0);
                if (_random.Next(2) == 0)
                    testTemplate = testTemplate.Rotate();
                testTemplate = testTemplate
                    .Scale(1 + _random.NextDouble() * Math.Min(Size.Width / 24, Size.Height / 24))
                    .Translate(new Size(
                        Size.Width / 2 + _random.Next(-(Size.Width / 2), Size.Width / 2), 
                        Size.Height / 2 + _random.Next(-(Size.Height / 2), Size.Height / 2)));
                
                // get min/max with buffer of room
                var bounds = testTemplate.Polygon.Bounds;
                bounds.Inflate(buffer, buffer);
                
                // the room is too close to the center
                if (bounds.Contains(StartPoint)) continue;

                // the room is too close to an edge
                if (!new Rectangle(0, 0, Size.Width, Size.Height).Contains(bounds))
                    continue;
                
                // if any other room intersects current room
                if (_roomRects.Any(r => r.IntersectsWith(bounds))) continue;

                AddRoom(testTemplate);
                break;
            }
        }

        /// <summary>
        /// Adds the given room to the maze. 
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void AddRoom(RoomTemplate template)
        {
            var currentPoint = template.Polygon[^1];

            var wallPoints = new HashSet<Point>();
            foreach (var targetPoint in template.Polygon)
            {
                if (currentPoint.X != targetPoint.X && currentPoint.Y != targetPoint.Y)
                    throw new ArgumentException("Invalid edges. Edges must not be diagonal", nameof(template));
                var points = MakeRoomWall(currentPoint, targetPoint);
                foreach (var p in points) wallPoints.Add(p);
                currentPoint = targetPoint;
            }
            _roomRects.Add(template.Polygon.Bounds);
            TryFillRoom(template, wallPoints);
        }

        public void Generate()
        {
            if (IsGenerated)
                throw new InvalidOperationException("Already generated");
            IsGenerated = true;


            if (Grid[StartPoint.X, StartPoint.Y] != 0)
                throw new InvalidOperationException(
                    "Start Point is invalid and already assigned. Likely a room is intersecting the point");
            
            // add first node to open path exits list
            // TODO: check to see if that direction is valid
            Grid[StartPoint.X, StartPoint.Y] = RandomDirection();
            _openPathExits.Enqueue((StartPoint, Grid[StartPoint.X, StartPoint.Y]));
            
            ProcessOpenExits();
            if (FillEmpty) CloseEmptyNodes();
            ProcessRoomExits();
        }

        private void TryFillRoom(RoomTemplate room, ICollection<Point> wallPoints)
        {
            for (var x = room.Polygon.Bounds.Left + 1; x < room.Polygon.Bounds.Right; x++)
            for (var y = room.Polygon.Bounds.Top + 1; y < room.Polygon.Bounds.Bottom; y++)
            {
                if (Grid[x, y] != 0) continue;
                var intersections = 0;
                var lastWasIntersection = false;
                for (var tX = x; tX <= room.Polygon.Bounds.Right; tX++)
                {
                    if (wallPoints.Contains(new Point(tX, y)))
                    {
                        if (!lastWasIntersection) intersections++;
                        lastWasIntersection = true;
                    }
                    else
                    {
                        lastWasIntersection = false;
                    }
                }

                if (intersections % 2 != 1) continue;

                FillRoom(new Point(x, y), new HashSet<Point>(room.Exits));
                return;
            }
        }

        private IEnumerable<Point> MakeRoomWall(Point start, Point end)
        {
            NodeFlag direction;
            if (start.X < end.X) direction = NodeFlag.East;
            else if (start.X > end.X) direction = NodeFlag.West;
            else if (start.Y < end.Y) direction = NodeFlag.South;
            else if (start.Y > end.Y) direction = NodeFlag.North;
            else throw new Exception("Failed to find direction");
            var points = new List<Point>();
            for (;;)
            {
                points.Add(start);
                Grid[start.X, start.Y] |= direction | NodeFlag.RoomEdge;
                start = GetPositionInDirection(start, direction);
                Grid[start.X, start.Y] |= OppositeDirection(direction);
                // if (IsRoomEdge(start)) 
                    // throw new Exception("Room walls intersect.");
                if (start != end) continue;
                return points;
            }

        }

        private void FillRoom(Point startPoint, ICollection<Point> exits)
        {
            var openRoomPositions = new Queue<Point>();
            var seenRoomPositions = new HashSet<Point>();
            openRoomPositions.Enqueue(startPoint);
            while (openRoomPositions.Count > 0)
            {
                var point = openRoomPositions.Dequeue();
                Debug.Assert(point != StartPoint);
                Grid[point.X, point.Y] |= NodeFlag.Room | NodeFlag.North | NodeFlag.East | NodeFlag.West | NodeFlag.South;
                foreach (var direction in Directions)
                {
                    var neighbor = GetPositionInDirection(point, direction);
                    if (IsRoomEdge(neighbor))
                    {
                        Grid[neighbor.X, neighbor.Y] |= OppositeDirection(direction);
                        if (exits.Contains(neighbor))
                            _roomExits.Enqueue((neighbor, direction));
                    }
                    else if (IsOpen(neighbor) && !seenRoomPositions.Contains(neighbor))
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
                var (exitPoint, exitDirection) = _roomExits.Dequeue();
                var neighborPoint = GetPositionInDirection(exitPoint, exitDirection);
                if (IsOpen(neighborPoint)) continue;
                Grid[exitPoint.X, exitPoint.Y] |= exitDirection | NodeFlag.RoomExit;
                Grid[neighborPoint.X, neighborPoint.Y] |= OppositeDirection(exitDirection);
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
                var point = _open.Choose();
                Debug.Assert(Grid[point.X, point.Y] == 0);
                var i = 0;
                var ii = _random.Next(4);
                do
                {
                    var d = Directions[(i + ii) % 4];
                    var neighbor = GetPositionInDirection(point, d);
                    if (!IsClosed(neighbor) || IsRoomEdge(neighbor)) continue;
                    
                    Grid[neighbor.X, neighbor.Y] |= OppositeDirection(d);
                    Grid[point.X, point.Y] |= d;
                    _openPathExits.Enqueue((point, d));
                    _open.Remove(point);
                    ProcessOpenExits();
                    didFind = true;
                    break;
                } while (++i < 4);
                Debug.Assert(didFind);
            }
        }

        private void ProcessOpenExits()
        {

            // keep going while we have "exits" from this path
            while (_openPathExits.Count > 0)
            {
                // get the first exit in the path
                var (point, previousDirection) = _openPathExits.Dequeue();

                while (true)
                {
                    var nextDirection = TryCarve(point, previousDirection);
                    if (nextDirection == null) break;

                    if (!IsOpen(GetPositionInDirection(point, nextDirection.Value))) break;
                    point = GetPositionInDirection(point, nextDirection.Value);
                    previousDirection = OppositeDirection(nextDirection.Value);
                    Grid[point.X, point.Y] = previousDirection;
                    _open.Remove(point);
                    
                    if (IsOpen(GetPositionInDirection(point, NodeFlag.North)))
                        _open.Add(GetPositionInDirection(point, NodeFlag.North));
                    if (IsOpen(GetPositionInDirection(point, NodeFlag.South)))
                        _open.Add(GetPositionInDirection(point, NodeFlag.South));
                    if (IsOpen(GetPositionInDirection(point, NodeFlag.East)))
                        _open.Add(GetPositionInDirection(point, NodeFlag.East));
                    if (IsOpen(GetPositionInDirection(point, NodeFlag.West)))
                        _open.Add(GetPositionInDirection(point, NodeFlag.West));
                }
            }
        }

        private NodeFlag? TryCarve(Point point, NodeFlag previousDirection)
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
                        var lineDirection = TryCarveLine(point, previousDirection);
                        if (lineDirection != null)
                            return lineDirection;
                        else
                            mode = Mode.Turn;
                        break;
                    case Mode.Turn when !triedTurn:
                        triedTurn = true;
                        var turnDirection = TryCarveTurn(point, previousDirection);
                        if (turnDirection != null)
                            return turnDirection;
                        else
                            mode = Mode.Line;
                        break;
                    case Mode.Fork:
                        var forkDirection = TryCarveFork(point, previousDirection);
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

        private NodeFlag? TryCarveLine(Point point, NodeFlag fromNodeFlag)
        {
            var toDirection = OppositeDirection(fromNodeFlag);
            if (!IsOpen(GetPositionInDirection(point, toDirection))) return null;
            Grid[point.X, point.Y] |= toDirection;
            return toDirection;
        }

        private NodeFlag? TryCarveTurn(Point point, NodeFlag fromNodeFlag)
        {
            var toDirection = RandomDirection(new[] {fromNodeFlag, OppositeDirection(fromNodeFlag)});

            if (!IsOpen(GetPositionInDirection(point, toDirection)))
                toDirection = OppositeDirection(toDirection);

            if (!IsOpen(GetPositionInDirection(point, toDirection))) return null;
            Grid[point.X, point.Y] |= toDirection;
            return toDirection;
        }

        private NodeFlag? TryCarveFork(Point point, NodeFlag fromNodeFlag)
        {
            var nextDirection = 
                RandomModeLineOrTurn() == Mode.Line ? 
                    OppositeDirection(fromNodeFlag) : 
                    RandomDirection(new[] {fromNodeFlag | OppositeDirection(fromNodeFlag)});
            var exitDirection = RandomDirection(new[] {fromNodeFlag, nextDirection});
            if (!IsOpen(GetPositionInDirection(point, nextDirection)) || !IsOpen(GetPositionInDirection(point, exitDirection))) return null;
            Grid[point.X, point.Y] |= nextDirection | exitDirection;
            var exitPoint = GetPositionInDirection(point, exitDirection);
            Grid[exitPoint.X, exitPoint.Y] = OppositeDirection(exitDirection);
            _openPathExits.Enqueue((exitPoint, exitDirection));
            _open.Remove(exitPoint);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsInMaze(Point point) => IsInMaze(point.X, point.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsInMaze(int x, int y) => x >= 0 && x < Size.Width && y >= 0 && y < Size.Height;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsClosed(Point point) => IsClosed(point.X, point.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsClosed(int x, int y) => IsInMaze(x, y) && Grid[x, y] != 0;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsRoomEdge(Point point) => IsRoomEdge(point.X, point.Y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsRoomEdge(int x, int y) => IsInMaze(x, y) && Grid[x, y].IsRoomEdge();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOpen(Point point) => IsOpen(point.X, point.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOpen(int x, int y) => IsInMaze(x, y) && Grid[x, y] == 0;

        private static Point GetPositionInDirection(Point point, NodeFlag d) => 
            d switch {
                NodeFlag.North => new Point(point.X, point.Y - 1),
                NodeFlag.South => new Point(point.X, point.Y + 1),
                NodeFlag.East => new Point(point.X + 1, point.Y),
                NodeFlag.West => new Point(point.X - 1, point.Y),
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
    }

    // TODO Validate
    public class Polygon : IReadOnlyList<Point>
    {
        public readonly Rectangle Bounds;
        public readonly Point Center;
        
        private readonly IReadOnlyList<Point> _points;

        public Polygon(IEnumerable<Point> points)
        {
            _points = new List<Point>(points);
            var l = int.MaxValue;
            var r = int.MinValue;
            var t = int.MaxValue;
            var b = int.MinValue;
            foreach (var p in _points)
            {
                if (p.X < l) l = p.X;
                if (p.X > r) r = p.X;
                if (p.Y < t) t = p.Y;
                if (p.Y > b) b = p.Y;
            }
            Bounds = Rectangle.FromLTRB(l, t, r, b);
            Center = Bounds.Location;
            Center.Offset(new Point(Bounds.Size / 2));
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _points).GetEnumerator();
        }

        public int Count => _points.Count;

        public Point this[int index] => _points[index];
    }

    public class RoomTemplate
    {
        public readonly Polygon Polygon;
        public readonly IReadOnlyCollection<Point> Exits;

        public RoomTemplate(IEnumerable<Point> polygonPoints, IEnumerable<Point> exits) :
            this(new Polygon(polygonPoints), exits) { }

        public RoomTemplate(Polygon polygon, IEnumerable<Point> exits)
        {
            Polygon = polygon;
            Exits = new List<Point>(exits);
        }
    }

    public static class DrawingExtensions
    {
        public static RoomTemplate Scale(this RoomTemplate te, double amount) =>
            new (te.Polygon.Scale(amount), te.Exits.Scale(amount));

        public static RoomTemplate Translate(this RoomTemplate te, Size amount) =>
            new (te.Polygon.Translate(amount), te.Exits.Translate(amount));

        public static RoomTemplate Flip(this RoomTemplate te, bool x, bool y) =>
            new (te.Polygon.Flip(x, y), te.Exits.Flip(x, y));

        public static RoomTemplate Rotate(this RoomTemplate te) =>
            new (te.Polygon.Rotate(), te.Exits.Rotate());

        public static IEnumerable<Point> Scale(this IEnumerable<Point> template, double amount) =>
            template.Select(point => point.Scale(amount));

        public static IEnumerable<Point> Translate(this IEnumerable<Point> template, Size amount) => 
            template.Select(point => point.Translate(amount));

        public static IEnumerable<Point> Flip(this IEnumerable<Point> template, bool x, bool y) =>
            template.Select(point => point.Flip(x, y));

        public static IEnumerable<Point> Rotate(this IEnumerable<Point> template) =>
            template.Select(point => point.Rotate());

        public static Point Scale(this Point point, double amount) =>
            new ((int) (point.X * amount), (int) (point.Y * amount));

        public static Point Translate(this Point point, Size amount) => 
            new (point.X + amount.Width, point.Y + amount.Height);

        public static Point Flip(this Point point, bool x, bool y) =>
            new (x ? -point.X : point.X, y ? -point.Y : point.Y);

        public static Point Rotate(this Point point) =>
            new (point.Y, point.X);
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