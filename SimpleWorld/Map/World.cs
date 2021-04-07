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
        public bool CanRun { get; set; }
        public bool CanCutCorner { get; set; }
        public double MoveCost { get; set; }

        public readonly int XSize;
        public readonly int YSize;

        private readonly Random _random;
        private readonly Position[][] _allNodes;

        public World(int xSize, int ySize, Random random = null, double moveCost = 1)
        {
            XSize = xSize;
            YSize = ySize;
            MoveCost = moveCost;

            _allNodes = new Position[XSize][];

            _random = random ?? new Random();

            var deadSpaceNoiseMap = DeadSpaceNoiseMap();
            var hillsNoiseMap = HillsNoiseMap();
            
            for (var x = 0; x < XSize; x++)
            {
                _allNodes[x] = new Position[YSize];
                for (var y = 0; y < YSize; y++)
                {
                    if (deadSpaceNoiseMap.GetValue(x, y) > 0.5) continue;

                    var point = (hillsNoiseMap.GetValue(x, y) + 1f) / 2;
                    point *= 8f;
                    _allNodes[x][y] = new Position(this, x, y, (int) Math.Round(point) + 1);
                }
            }
        }

        [CanBeNull]
        public Position GetNode(int x, int y)
        {
            if (x < 0 || x >= XSize || y < 0 || y >= YSize) return null;
            return _allNodes[x][y];
        }

        public Position GetNode(Xy xy) => GetNode(xy.X, xy.Y);

        public IEnumerable<Position> GetAllNodes()
        {
            return _allNodes.SelectMany(a => a).Where(a => a != null);
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

        private NoiseMap HillsNoiseMap()
        {
            var noiseMap = new NoiseMap();
            var noiseMapBuilder = new PlaneNoiseMapBuilder
            {
                DestNoiseMap = noiseMap,
                SourceModule = new Blend
                {
                    Source0 = new Clamp
                    {
                        LowerBound = -1,
                        UpperBound = 1,
                        Source0 = new Perlin
                        {
                            Frequency = 1,
                            Lacunarity = 1.5,
                            Quality = NoiseQuality.Best,
                            Persistence = 0.25,
                            Seed = _random.Next()
                        }
                    },
                    Source1 = new Clamp
                    {
                        LowerBound = -1,
                        UpperBound = 1,
                        Source0 = new Perlin
                        {
                            Frequency = 2,
                            Lacunarity = 3.5,
                            Quality = NoiseQuality.Best,
                            Persistence = 0.25,
                            Seed = _random.Next()
                        }
                    },
                    Control = new Constant {ConstantValue = -0.3}
                }
            };
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }
    }
}
