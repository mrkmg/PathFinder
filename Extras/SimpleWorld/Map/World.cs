using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;

namespace SimpleWorld.Map
{
    public class World
    {
        public struct StandardInitializationOptions
        {
            public double F1;
            public double L1;
            public double P1;
            public double SX1;
            public double SY1;

            public double F2;
            public double L2;
            public double P2;
            public double SX2;
            public double SY2;
            
            public double Ratio12;
        }

        public struct MazeInitializationOptions
        {
            public int MLW;
            public int MTW;
            public int MFW;
        }

        public static readonly StandardInitializationOptions DefaultStandardInit = new ()
        {
            F1 = 2,
            L1 = 2,
            P1 = 0.25d,
            SX1 = 1, SY1 = 1,
            
            F2 = 9,
            L2 = 3.5d,
            P2 = 0.25d,
            SX2 = 1, SY2 = 1,
            
            Ratio12 = 0
        };

        public static readonly MazeInitializationOptions DefaultMazeInit = new()
        {
            MLW = 50,
            MTW = 50,
            MFW = 50
        };
        
        public double MoveCost { get; private set; }

        public readonly int XSize;
        public readonly int YSize;

        public readonly WorldType Type;

        private readonly Random _random;
        private readonly Position[][] _allNodes;
        private Maze _maze;

        private World(int xSize, int ySize, double moveCost, Random random = null)
        {
            XSize = xSize;
            YSize = ySize;
            MoveCost = moveCost;
            _allNodes = new Position[XSize][];
            for (var i = 0; i < XSize; i++)
                _allNodes[i] = new Position[YSize];

            _random = random ?? new Random();
        }
        
        public World(int xSize, int ySize, double moveCost, StandardInitializationOptions init, Random random = null) 
            : this(xSize, ySize, moveCost, random)
        {
            Type = WorldType.Standard;
            Standard(init);
        }

        public World(int xSize, int ySize, double moveCost, MazeInitializationOptions init, Random random = null) 
            : this(xSize, ySize, moveCost, random)
        {
            Type = WorldType.Maze;
            Maze(init);
        }

        [CanBeNull]
        public Position GetPosition(int x, int y)
        {
            if (x < 0 || x >= XSize || y < 0 || y >= YSize) return null;
            return _allNodes[x][y];
        }

        public Position GetPosition(Xy xy) => GetPosition(xy.X, xy.Y);

        public IEnumerable<Position> GetAllNodes()
        {
	        return _allNodes.SelectMany(a => a).Where(a => a != null);
        }

        private void Maze(MazeInitializationOptions init)
        {

            var cols = XSize / 3;
            var rows = YSize / 3;
            _maze = new Maze(cols, rows, _random, 0, 0)
            {
                LineWeight = init.MLW, 
                TurnWeight = init.MTW, 
                ForkWeight = init.MFW
            };
            _maze.Generate();
            for (var x = 0; x < cols; x++)
			{
				for (var y = 0; y < rows; y++)
				{
					var xOff = x * 3;
					var yOff = y * 3;
					var type = _maze.Grid[x, y];
                    if (type != 0)
					    _allNodes[xOff + 1][yOff + 1] = new Position(this, xOff + 1, yOff + 1, 1);
					if ((type & (byte)Direction.North) != 0)
					{
						_allNodes[xOff + 1][yOff] = new Position(this, xOff + 1, yOff, 1);
					}
					if ((type & (byte)Direction.South) != 0)
					{
						_allNodes[xOff + 1][yOff + 2] = new Position(this, xOff + 1, yOff + 2, 1);
					}
					if ((type & (byte)Direction.West) != 0)
					{
						_allNodes[xOff][yOff + 1] = new Position(this, xOff, yOff + 1, 1);
					}
					if ((type & (byte)Direction.East) != 0)
					{
						_allNodes[xOff + 2][yOff + 1] = new Position(this, xOff + 2, yOff + 1, 1);
					}
				}
			}
		} // Generate

        private void Standard(StandardInitializationOptions standardInitializationOptions)
        {
            var deadSpaceNoiseMap = DeadSpaceNoiseMap();
            var hillsNoiseMap = HillsNoiseMap(standardInitializationOptions);
            
            for (var x = 0; x < XSize; x++)
            {
                for (var y = 0; y < YSize; y++)
                {
                    if (deadSpaceNoiseMap.GetValue(x, y) > 0.5) continue;

                    var point = Math.Pow(hillsNoiseMap.GetValue(x, y) + 1f, 3) / 8;
                    point *= 3f;
                    point = Math.Min(3, Math.Max(0, point));
                    _allNodes[x][y] = new Position(this, x, y, (int) Math.Round(point) + 1);
                }
            }
        }

        private NoiseMap DeadSpaceNoiseMap()
        {
            var noiseMap = new NoiseMap();
            var noiseSource = new Perlin
            {
                Seed = _random.Next(),
                Frequency = _random.NextDouble() * 5d,
                Lacunarity = _random.NextDouble() * 2d + 1.5d,
                Quality = NoiseQuality.Fast,
                Persistence = _random.NextDouble(),
            };
            var noiseMapBuilder = new PlaneNoiseMapBuilder {DestNoiseMap = noiseMap, SourceModule = noiseSource};
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }

        private NoiseMap HillsNoiseMap(StandardInitializationOptions standardInitializationOptions)
        {
            var noiseMap = new NoiseMap();
            var noiseMapBuilder = new PlaneNoiseMapBuilder {
                DestNoiseMap = noiseMap,
                SourceModule = new Blend {
                    Source0 = new Clamp {
                        LowerBound = -1,
                        UpperBound = 1,
                        Source0 = new ScalePoint { 
                            XScale = standardInitializationOptions.SX1, 
                            ZScale = standardInitializationOptions.SY1,
                            Source0 = new Perlin {
                                Frequency = standardInitializationOptions.F1,
                                Lacunarity = standardInitializationOptions.L1,
                                Quality = NoiseQuality.Fast,
                                Persistence = standardInitializationOptions.P1,
                                Seed = _random.Next() }}},
                    Source1 = new Clamp {
                        LowerBound = -1,
                        UpperBound = 1,
                        Source0 = new ScalePoint {
                            XScale = standardInitializationOptions.SX2, 
                            ZScale = standardInitializationOptions.SY2,
                            Source0 = new Perlin {
                                Frequency = standardInitializationOptions.F2,
                                Lacunarity = standardInitializationOptions.L2,
                                Quality = NoiseQuality.Fast,
                                Persistence = standardInitializationOptions.P2,
                                Seed = _random.Next() }}},
                    Control = new Constant {ConstantValue = standardInitializationOptions.Ratio12}}};
            
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }
    }

    public enum WorldType
    {
        Maze,
        Standard
    }
}
