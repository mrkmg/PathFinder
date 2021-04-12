using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using SimpleWorld.Map;

namespace PathFinderGui.Widgets
{
    public class MapWidget : LayeredBitmapWidget
    {
        private IList<Position> _lastBestPath;
        
        public MapWidget() : base(1, 4)
        {
        }
        
        public void DrawWorld(World world)
        {
            if (world == null) return;
            DrawAllPoints(0, world.GetAllNodes().AsMapDrawPoints());
        }

        public void ClearRunning()
        {
            _lastBestPath = null;
            ClearLayer(1);
        }

        public void DrawRunning(FrameData frameData)
        {
            DrawAllPoints(1, frameData.CheckedPositions.AsSearchDrawPoints());
            DrawAllPoints(1, _lastBestPath != null
                ? BestPathDiff(frameData.CurrentBestPath)
                : frameData.CurrentBestPath.AsPathDrawPoints());
            _lastBestPath = frameData.CurrentBestPath;
        }

        public void ClearMarkers(Position startPoint, Position endPoint)
        {
            if (startPoint != null)
            {
                var p = startPoint
                    .ToMarkerPoints(10 / Scale)
                    .Where(IsPointInBitmap)
                    .Select(xy => new DrawPoint(xy.x, xy.y, Colors.Transparent));
                DrawAllPoints(2, p);
            }

            if (endPoint != null)
            {
                var p = endPoint
                    .ToMarkerPoints(10 / Scale)
                    .Where(IsPointInBitmap)
                    .Select(xy => new DrawPoint(xy.x, xy.y, Colors.Transparent));
                DrawAllPoints(2, p);
            }
        }

        public void DrawMarkers(Position startPoint, Position endPoint)
        {
            var p1 = startPoint
                .ToMarkerPoints(10 / Scale)
                .Where(IsPointInBitmap)
                .Select(xy => xy.AsMarkerDrawPoint());

            DrawAllPoints(2, p1);

            var p2 = endPoint
                .ToMarkerPoints(10 / Scale)
                .Where(IsPointInBitmap)
                .Select(xy => xy.AsMarkerDrawPoint());

            DrawAllPoints(2, p2);
        }

        public void ClearPath() => ClearLayer(3);

        public void DrawPath(IEnumerable<Position> path) 
            => DrawAllPoints(3, path.AsPathDrawPoints());

        private bool IsPointInBitmap((int x, int y) xy) 
            => xy.x > 0 && xy.x < BitmapWidth && xy.y > 0 && xy.y < BitmapHeight;

        private IEnumerable<DrawPoint> BestPathDiff(IEnumerable<Position> newBestPath)
        {
            var i = -1;
            foreach (var position in newBestPath)
            {
                i++;
                
                if (i >= _lastBestPath.Count)
                {
                    yield return position.AsPathDrawPoint();
                    continue;
                }
                
                if (_lastBestPath[i].Equals(position))
                {
                    continue;
                }

                yield return _lastBestPath[i].AsSearchDrawPoint();
                yield return position.AsPathDrawPoint();
            }
            while (++i < _lastBestPath.Count)
                yield return _lastBestPath[i].AsSearchDrawPoint();
        }
    }
    
     internal static class MainFormExtensions
    {
        private static Color GetColorForLevel(int level)
        {
            var intensity = level switch
            {
                0 => 0,
                1 => 200,
                2 => 175,
                3 => 150,
                4 => 125,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
            return Color.FromArgb(0, intensity, 0);
        }
        
        internal static IEnumerable<DrawPoint> AsMapDrawPoints(this IEnumerable<Position> positions) 
            => positions.Select(AsMapDrawPoint);

        internal static IEnumerable<DrawPoint> AsSearchDrawPoints(this IEnumerable<Position> positions) 
            => positions.Where(p => p != null).Select(AsSearchDrawPoint);

        internal static IEnumerable<DrawPoint> AsPathDrawPoints(this IEnumerable<Position> positions) 
            => positions.Where(p => p != null).Select(AsPathDrawPoint);

        internal static DrawPoint AsPathDrawPoint(this Position p) 
            => new(p.X, p.Y, Color.FromArgb(255, 0, 0, 200));
        
        internal static DrawPoint AsSearchDrawPoint(this Position p) 
            => new(p.X, p.Y, Color.FromArgb(0, 0, 50, 100));
        
        internal static DrawPoint AsMapDrawPoint(this Position p) 
            => new (p.X, p.Y, GetColorForLevel(p.Z));

        internal static IEnumerable<(int x, int y)> ToMarkerPoints(this Position position, int size)
        {
            for (var i = 0; i <= size; i++)
            {
                
                yield return (position.X + i, position.Y + i);
                yield return (position.X + i, position.Y - i);
                yield return (position.X - i, position.Y + i);
                yield return (position.X - i, position.Y - i);
            }
        }

        internal static DrawPoint AsMarkerDrawPoint(this (int x, int y) xy)
            => new(xy.x, xy.y, Color.FromArgb(150, 0, 150));
    }
}