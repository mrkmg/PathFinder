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
        private Position[][] _allNodes;
        public bool CanCutCorner = false;

        public int XSize;
        public int YSize;

        private readonly Random _random;

        public World(int xSize, int ySize, Random random = null)
        {
            XSize = xSize;
            YSize = ySize;

            _allNodes = new Position[XSize][];

            _random = random ?? new Random();

            var wallNoiseMap = MakeSimpleNoiseMap();
            var hillsNoiseMap = MakeTestNoiseMap();


            for (var x = 0; x < XSize; x++)
            {
                _allNodes[x] = new Position[YSize];
                for (var y = 0; y < YSize; y++)
                {
//                    if (wallNoiseMap.GetValue(x, y) > 0.3) continue;

                    var point = (hillsNoiseMap.GetValue(x, y) + 1f) / 2;
                    point *= 8f;
                    _allNodes[x][y] = new Position(x, y, (int) Math.Round(point) + 1) {World = this};
                }
            }
        }

        [CanBeNull]
        public Position GetNode(int x, int y)
        {
            if (x < 0 || x >= XSize || y < 0 || y >= YSize) return null;
            return _allNodes[x][y];
        }

        public IEnumerable<Position> GetAllNodes()
        {
            return _allNodes.SelectMany(a => a).Where(a => a != null);
        }

        private NoiseMap MakeSimpleNoiseMap()
        {
            var noiseMap = new NoiseMap();
            var noiseSource = new Perlin {Seed = _random.Next()};
            var noiseMapBuilder = new PlaneNoiseMapBuilder {DestNoiseMap = noiseMap, SourceModule = noiseSource};
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }

        private NoiseMap MakeTestNoiseMap()
        {
            var noiseMap = new NoiseMap();
            var noiseMapBuilder = new PlaneNoiseMapBuilder
            {
                DestNoiseMap = noiseMap,
                SourceModule = new Clamp
                {
                    LowerBound = -1,
                    UpperBound = 1,
                    Source0 = new ScaleBias
                    {
                        Scale = 0.5,
                            // overall Slope
                        Source0 = new ScaleBias
                        {
                            Scale = 2,
                            Source0 = new Perlin
                            {
                                Frequency = 0.5,
                                Lacunarity = 3,
                                Seed = _random.Next()
                            }
                        },
                    }
                }
            };
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }
    }
}
