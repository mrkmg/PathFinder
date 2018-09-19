using System;
using System.Collections.Generic;
using System.Linq;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;

namespace PathFinderTest.Map
{
    public class World
    {
        public Dictionary<int, Dictionary<int, Position>> AllNodes = new Dictionary<int, Dictionary<int, Position>>();
        public bool CanCutCorner = false;

        public int XSize;
        public int YSize;

        private readonly Random _random;

        public World(int xSize, int ySize, Random random = null)
        {
            XSize = xSize;
            YSize = ySize;

            _random = random ?? new Random();

            var wallNoiseMap = MakeSimpleNoiseMap();
            var hillsNoiseMap = MakeTestNoiseMap();


            for (var x = 0; x < XSize; x++)
            {
                AllNodes.Add(x, new Dictionary<int, Position>());
                for (var y = 0; y < YSize; y++)
                {
                    if (wallNoiseMap.GetValue(x, y) > 0.3) continue;

                    var point = (hillsNoiseMap.GetValue(x, y) + 1f) / 2;
                    point *= 8f;
                    AllNodes[x].Add(y, new Position(x, y, (int) Math.Round(point) + 1) {World = this});
                }
            }
        }

        public IEnumerable<Position> GetAllNodes()
        {
            return AllNodes.Values.SelectMany(t => t.Values);
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
                        Source0 = new Add
                        {
                            // overall Slope
                            Source0 = new ScaleBias
                            {
                                Scale = 2,
                                Source0 = new Perlin {Seed = _random.Next()}
                            },
                            // ridge
                            Source1 = new ScalePoint
                            {
                                XScale = 10,
                                YScale = 10,
                                Source0 = new Curve
                                {
                                    Source0 = new Perlin {Seed = _random.Next()},
                                    ControlPoints = new List<Curve.ControlPoint>
                                    {
                                        new Curve.ControlPoint(-1, -1),
                                        new Curve.ControlPoint(-0.5, 0),
                                        new Curve.ControlPoint(0.5, 0),
                                        new Curve.ControlPoint(1, 1)
                                    }
                                }
                            }
                        }
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
