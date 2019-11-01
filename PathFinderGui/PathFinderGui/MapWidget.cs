using System;
using System.Collections.Generic;
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
            var data = new Color[Width * Height];
            for (var i = 0; i < data.Length; i++) data[i] = Colors.Black;
            _bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppRgba, data);
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
            using (var bitmapData = _bitmap.Lock())
            {
                foreach (var drawPoint in points)
                {
                    var xMin = drawPoint.X * _scale;
                    var yMin = drawPoint.Y * _scale;
                    var xMax = (drawPoint.X + 1) * _scale;
                    var yMax = (drawPoint.Y + 1) * _scale;
                    for (var ix = xMin; ix < xMax; ix++)
                    for (var iy = yMin; iy < yMax; iy++)
                    {
                        bitmapData.SetPixel(ix, iy, drawPoint.Color);
                    }
                }
            }
            Invalidate();
        }
    }

    public struct DrawPoint
    {
        public int X;
        public int Y;
        public Color Color;
    }
}