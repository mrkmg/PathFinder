using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace PathFinderGui
{
    public class MapWidget : Drawable
    {
        public int MapWidth => Width / _scale;
        public int MapHeight => Height / _scale;
        
        private Bitmap _bitmap;
        private int _scale;

        public MapWidget(int scale)
        {
            _scale = scale;
            Paint += (sender, args) =>
            {
                if (_bitmap != null)
                    args.Graphics.DrawImage(_bitmap, args.ClipRectangle, args.ClipRectangle);
            };
            LoadComplete += (sender, args) => MakeBitmap();
            SizeChanged += (sender, args) => MakeBitmap();
        }

        public void ChangeScale(int scale)
        {
            _scale = scale;
            MakeBitmap();
            Invalidate();
        }

        private void MakeBitmap()
        {
            _bitmap = null;
            if (Width == 0 || Height == 0) return;
            var data = new Color[Width * Height];
            for (var i = 0; i < data.Length; i++) data[i] = Colors.Black;
            _bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppRgba, data);
        }

        public void DrawMarker(int x, int y, int size, Color color)
        {
            var points = new List<DrawPoint>();
            var ly = size;
            for (var dx = x - size; dx < x + size; dx++)
            {
                points.Add(new DrawPoint {X = dx, Y = y + ly, Color = color});
                points.Add(new DrawPoint {X = dx, Y = y - ly, Color = color});
                ly -= 1;
            }
            
            DrawAll(points.Where(p => p.X > 0 && p.X < MapWidth && p.Y > 0 && p.Y < MapHeight));
        }

        public void DrawPoint(int x, int y, Color color)
        {
            var xMin = x * _scale;
            var yMin = y * _scale;
            var xMax = (x + 1) * _scale;
            var yMax = (y + 1) * _scale;
            using (var bitmapData = _bitmap.Lock())
            {
                for (var ix = xMin; ix < xMax; ix++)
                for (var iy = yMin; iy < yMax; iy++)
                {
                    bitmapData.SetPixel(ix, iy, color);
                }
            }
            Invalidate(new Rectangle(xMin, yMin, _scale, _scale));
        }

        public void DrawAll(IEnumerable<DrawPoint> points)
        {
            var minPoint = new Point(int.MaxValue, int.MaxValue);
            var maxPoint = new Point(0, 0);
            using (var bitmapData = _bitmap.Lock())
            {
                foreach (var drawPoint in points)
                {
                    var xMin = drawPoint.X * _scale;
                    var yMin = drawPoint.Y * _scale;
                    var xMax = (drawPoint.X + 1) * _scale;
                    var yMax = (drawPoint.Y + 1) * _scale;
                    if (xMin < minPoint.X) minPoint.X = xMin;
                    if (yMin < minPoint.Y) minPoint.Y = yMin;
                    if (xMax > maxPoint.X) maxPoint.X = xMax;
                    if (yMax > maxPoint.Y) maxPoint.Y = yMax;
                    for (var ix = xMin; ix < xMax; ix++)
                    for (var iy = yMin; iy < yMax; iy++)
                    {
                        bitmapData.SetPixel(ix, iy, drawPoint.Color);
                    }
                }
            }
            Invalidate(new Rectangle(minPoint, maxPoint));
        }
    }

    public struct DrawPoint
    {
        public int X;
        public int Y;
        public Color Color;
    }
}