using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using ImmutableGeometry;

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
        private readonly List<RoomTemplate> _rooms = new ();
        private readonly Queue<(Point Point, NodeFlag Direction)> _openPathExits = new ();
        private readonly Queue<(Point Point, NodeFlag Direction)> _roomExits = new ();
        
        private static readonly NodeFlag[] Directions = {NodeFlag.North, NodeFlag.South, NodeFlag.West, NodeFlag.East};

        private static readonly Dictionary<NodeFlag, Vector> DirectionVectors = new ()
        {
            {NodeFlag.North, new Vector(0, -1)}, {NodeFlag.South, new Vector(0, 1)},
            {NodeFlag.West, new Vector(-1, 0)}, {NodeFlag.East, new Vector(1, 0)},
        };
        
        public Maze(Size size, Random random = null, Point? startPoint = null)
        {
            Size = size;
            Grid = new NodeFlag[size.Width, size.Height];
            _random = random ?? new Random();
            StartPoint = startPoint ?? size / 2;
        }

        /// <summary>
        /// Adds a random room to the maze
        /// </summary>
        public void AddRoom()
        {
            var roomTemplate = RoomTemplates.AllTemplates[_random.Next(RoomTemplates.AllTemplates.Length)];
            
            for (var triesLeft = 50; triesLeft > 0; triesLeft--)
            {
                // Randomly flip/scale/rotate/translate room
                var room = roomTemplate
                    .Mirror(_random.Next(2) == 0, _random.Next(2) == 0)
                    // .Rotate((double)_random.Next(4)/2 * Math.PI)
                    .Scale(1 + _random.NextDouble() * Math.Min(Size.Width / 24, Size.Height / 24))
                    .Translate(new Vector(
                        Size.Width / 2 + _random.Next(-(Size.Width / 2), Size.Width / 2),
                        Size.Height / 2 + _random.Next(-(Size.Height / 2), Size.Height / 2)));

                // get min/max with buffer of room
                var bounds = room.Shape.Bounds.Inflate(4, 4);
                
                // the room is too close to the center
                if (bounds.Contains(StartPoint)) continue;

                // the room is too close to an edge
                if (!new Rectangle(0, 0, Size.Width, Size.Height).Contains(bounds))
                    continue;
                
                // if any other room bounds intersects bounds
                if (_rooms.Any(r => r.Shape.Bounds.IntersectsWith(bounds))) continue;

                AddRoom(room);
                break;
            }
        }

        /// <summary>
        /// Adds the given room to the maze. 
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void AddRoom(RoomTemplate template)
        {
            foreach (var point in template.Shape.Points.Except(template.Shape.EdgePoints))
            {
                if (IsOpen(point)) Grid[point.X, point.Y] |= NodeFlag.Room | NodeFlag.North | NodeFlag.East | NodeFlag.West | NodeFlag.South;
            }
            
            foreach (var point in template.Shape.EdgePoints)
            {
                Grid[point.X, point.Y] |= NodeFlag.RoomEdge;
                
                foreach (var d in Directions)
                {
                    var p = point + DirectionVectors[d];
                    if (!IsRoom(p) && !IsRoomEdge(p)) continue;
                    Grid[p.X, p.Y] |= OppositeDirection(d);
                    Grid[point.X, point.Y] |= d;
                }
            }

            foreach (var exit in template.Exits)
            {
                foreach (var d in Directions)
                {
                    var p = exit + DirectionVectors[d];
                    if (!IsRoom(p)) continue;
                    Grid[exit.X, exit.Y] |= OppositeDirection(d);
                    _roomExits.Enqueue((exit, OppositeDirection(d)));
                }
            }
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

        private void ProcessRoomExits()
        {
            while (_roomExits.Count > 0)
            {
                var (exitPoint, exitDirection) = _roomExits.Dequeue();
                var neighborPoint = exitPoint + DirectionVectors[exitDirection];
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
                    var neighbor = point + DirectionVectors[d];
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

                    if (!IsOpen(point + DirectionVectors[nextDirection.Value])) break;
                    point = point + DirectionVectors[nextDirection.Value];
                    previousDirection = OppositeDirection(nextDirection.Value);
                    Grid[point.X, point.Y] = previousDirection;
                    _open.Remove(point);
                    
                    if (IsOpen(point + DirectionVectors[NodeFlag.North]))
                        _open.Add(point + DirectionVectors[NodeFlag.North]);
                    if (IsOpen(point + DirectionVectors[NodeFlag.South]))
                        _open.Add(point + DirectionVectors[NodeFlag.South]);
                    if (IsOpen(point + DirectionVectors[NodeFlag.East]))
                        _open.Add(point + DirectionVectors[NodeFlag.East]);
                    if (IsOpen(point + DirectionVectors[NodeFlag.West]))
                        _open.Add(point + DirectionVectors[NodeFlag.West]);
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
            if (!IsOpen(point + DirectionVectors[toDirection])) return null;
            Grid[point.X, point.Y] |= toDirection;
            return toDirection;
        }

        private NodeFlag? TryCarveTurn(Point point, NodeFlag fromNodeFlag)
        {
            var toDirection = RandomDirection(new[] {fromNodeFlag, OppositeDirection(fromNodeFlag)});

            if (!IsOpen(point + DirectionVectors[toDirection]))
                toDirection = OppositeDirection(toDirection);

            if (!IsOpen(point + DirectionVectors[toDirection])) return null;
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
            if (!IsOpen(point + DirectionVectors[nextDirection]) || !IsOpen(point + DirectionVectors[exitDirection])) return null;
            Grid[point.X, point.Y] |= nextDirection | exitDirection;
            var exitPoint = point + DirectionVectors[exitDirection];
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
        
        private bool IsInMaze(Point point) => IsInMaze(point.X, point.Y);
        
        private bool IsInMaze(int x, int y) => x >= 0 && x < Size.Width && y >= 0 && y < Size.Height;
        
        private bool IsClosed(Point point) => IsClosed(point.X, point.Y);
        
        private bool IsClosed(int x, int y) => IsInMaze(x, y) && Grid[x, y] != 0;
        
        private bool IsRoom(Point point) => IsRoom(point.X, point.Y);
        
        private bool IsRoomEdge(Point point) => IsRoomEdge(point.X, point.Y);
        
        private bool IsRoomEdge(int x, int y) => IsInMaze(x, y) && Grid[x, y].IsRoomEdge();
        
        private bool IsRoom(int x, int y) => IsInMaze(x, y) && Grid[x, y].IsRoom();
        
        private bool IsOpen(Point point) => IsOpen(point.X, point.Y);
        
        private bool IsOpen(int x, int y) => IsInMaze(x, y) && Grid[x, y] == 0;
        
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

    public class RoomTemplate
    {
        public readonly Shape Shape;
        public readonly Points Exits;

        public RoomTemplate(IEnumerable<Point> polygonPoints, IEnumerable<Point> exits) :
            this(new Shape(polygonPoints), exits) { }

        public RoomTemplate(Shape shape, IEnumerable<Point> exits)
        {
            Shape = shape;
            Exits = new(exits);
        }
    }

    public static class DrawingExtensions
    {
        public static RoomTemplate Scale(this RoomTemplate te, double amount) =>
            new (te.Shape.Scale(amount), te.Exits.Scale(amount, amount));
    
        public static RoomTemplate Translate(this RoomTemplate te, Vector amount) =>
            new (te.Shape.Translate(amount), te.Exits.Translate(amount));
    
        public static RoomTemplate Mirror(this RoomTemplate te, bool x, bool y) =>
            new (te.Shape.Mirror(x, y), te.Exits.Mirror(x, y));
    
        public static RoomTemplate Rotate(this RoomTemplate te, double amount) =>
            new (
                te.Shape.Rotate(amount), 
                te.Exits.Translate(-te.Shape.Bounds.Center.X, -te.Shape.Bounds.Center.Y)
                        .Rotate(amount)
                        .Translate(te.Shape.Bounds.Center.X, te.Shape.Bounds.Center.Y)
            );
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