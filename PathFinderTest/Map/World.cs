using System;
using System.Collections.Generic;
using System.Linq;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;

namespace PathFinderTest.Map
{
    class World
    {
        public bool CanCutCorner = false;
        public EstimateType EstimateType = EstimateType.Absolute;

        public int XSize;
        public int YSize;
        public Dictionary<int, Dictionary<int, Position>> AllNodes = new Dictionary<int, Dictionary<int, Position>>();

        public World(int xSize, int ySize, int r)
        {
            XSize = xSize;
            YSize = ySize;

            var wallNoiseMap = makeSimpleNoiseMap();
            var hillsNoiseMap = makeComplexNoiseMap();


            for (var x = 0; x < XSize; x++)
            {
                AllNodes.Add(x, new Dictionary<int, Position>());
                for (var y = 0; y < YSize; y++)
                {
                    if (wallNoiseMap.GetValue(x, y) > 0.3) continue;

                    var point = (hillsNoiseMap.GetValue(x, y) + 1f) / 2;
                    point *= 9f;
                    AllNodes[x].Add(y, new Position(x, y, (int)Math.Round(point) + 1) { World = this});
                }
            }
        }

        public IEnumerable<Position> GetAllNodes()
        {
            return AllNodes.Values.SelectMany(t => t.Values);
        }

        private NoiseMap makeSimpleNoiseMap()
        {
            var noiseMap = new NoiseMap();
            var noiseSource = new Perlin { Seed = new Random().Next() };
            var noiseMapBuilder = new PlaneNoiseMapBuilder { DestNoiseMap = noiseMap, SourceModule = noiseSource };
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }

        private NoiseMap makeComplexNoiseMap()
        {
            var noiseMap = new NoiseMap();
            var perlin = new Perlin { Seed = new Random().Next() };
            var perlin2 = new Perlin {Seed = new Random().Next()};

            var scale = new ScaleBias
            {
                Source0 = perlin,
                Scale = 0.5
            };

            var combine = new Add
            {
                Source0 = scale,
                Source1 = perlin2
            };

            var divide = new ScaleBias()
            {
                Source0 = combine,
                Scale = 0.75
            };


            var noiseMapBuilder = new PlaneNoiseMapBuilder { DestNoiseMap = noiseMap, SourceModule = divide };
            noiseMapBuilder.SetDestSize(XSize, YSize);
            noiseMapBuilder.SetBounds(-3, 3, -2, 2);
            noiseMapBuilder.Build();
            return noiseMap;
        }
    }

    enum EstimateType
    {
        Absolute,
        Square
    }
}
