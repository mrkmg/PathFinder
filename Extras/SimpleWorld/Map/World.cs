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
        public struct InitializationOptions
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

        public static readonly InitializationOptions DefaultInit = new ()
        {
            F1 = 2,
            L1 = 2,
            P1 = 0.25d,
            SX1 = 1, SY1 = 1,
            
            F2 = 9,
            L2 = 3.5d,
            P2 = 0.25d,
            SX2 = 1, SY2 = 1,
            
            Ratio12 = 0,
        };
        
        public bool CanCutCorner { get; set; } = true;
        public double MoveCost { get; set; }
        public int MaxStepSize { get; set; } = 2;

        public readonly int XSize;
        public readonly int YSize;

        private readonly Random _random;
        private readonly Position[][] _allNodes;

        public World(int xSize, int ySize, Random random = null, InitializationOptions? init = null, double moveCost = 1)
        {
            XSize = xSize;
            YSize = ySize;
            MoveCost = moveCost;
            _allNodes = new Position[XSize][];

            _random = random ?? new Random();
            
            Standard(init ?? DefaultInit);
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

        // TODO - Implement more types of world
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local
        // ReSharper disable UnusedVariable
        private void Maze(int cellSize = 5, int wallSize = 1)
        {
            var width = XSize / cellSize;
            var height = YSize / cellSize;

            var cx = _random.Next(width);
            var cy = _random.Next(height);
        }
        // ReSharper restore UnusedVariable
        // ReSharper restore UnusedParameter.Local
        // ReSharper enable UnusedMember.Local

        private void Standard(InitializationOptions initializationOptions)
        {
            var deadSpaceNoiseMap = DeadSpaceNoiseMap();
            var hillsNoiseMap = HillsNoiseMap(initializationOptions);
            
            for (var x = 0; x < XSize; x++)
            {
                _allNodes[x] = new Position[YSize];
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
            var noiseSource = new Perlin {Seed = _random.Next()};
            var noiseMapBuilder = new PlaneNoiseMapBuilder {DestNoiseMap = noiseMap, SourceModule = noiseSource};
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }

        private NoiseMap HillsNoiseMap(InitializationOptions initializationOptions)
        {
            var noiseMap = new NoiseMap();
            var noiseMapBuilder = new PlaneNoiseMapBuilder {
                DestNoiseMap = noiseMap,
                SourceModule = new Blend {
                    Source0 = new Clamp {
                        LowerBound = -1,
                        UpperBound = 1,
                        Source0 = new ScalePoint { 
                            XScale = initializationOptions.SX1, 
                            ZScale = initializationOptions.SY1,
                            Source0 = new Perlin {
                                Frequency = initializationOptions.F1,
                                Lacunarity = initializationOptions.L1,
                                Quality = NoiseQuality.Fast,
                                Persistence = initializationOptions.P1,
                                Seed = _random.Next() }}},
                    Source1 = new Clamp {
                        LowerBound = -1,
                        UpperBound = 1,
                        Source0 = new ScalePoint {
                            XScale = initializationOptions.SX2, 
                            ZScale = initializationOptions.SY2,
                            Source0 = new Perlin {
                                Frequency = initializationOptions.F2,
                                Lacunarity = initializationOptions.L2,
                                Quality = NoiseQuality.Fast,
                                Persistence = initializationOptions.P2,
                                Seed = _random.Next() }}},
                    Control = new Constant {ConstantValue = initializationOptions.Ratio12}}};
            
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }
    }
}
