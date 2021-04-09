using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace PathFinderGui
{
    public class BitmapWidget : Drawable
    {
        public int BitmapWidth => Width / _scale;
        public int BitmapHeight => Height / _scale;
        
        private Bitmap _bitmap;
        private int _scale;

        public int Scale
        {
            get => _scale;
            set => ChangeScale(value);

        }
        public event EventHandler IsReady;
        public new EventHandler<BitmapMouseEventArgs> MouseUp;
        public new EventHandler<BitmapMouseEventArgs> MouseDown;
        public new EventHandler<BitmapMouseEventArgs> MouseMove;
        public new EventHandler<BitmapMouseEventArgs> MouseEnter;
        public new EventHandler<BitmapMouseEventArgs> MouseLeave;
        public new EventHandler<BitmapMouseEventArgs> MouseDoubleClick;
        public new EventHandler<BitmapMouseEventArgs> MouseWheel;

        public BitmapWidget(int scale)
        {
            _scale = scale;
            Paint += OnPaint;
            LoadComplete += OnLoadComplete;
            SizeChanged += OnSizeChanged;

            base.MouseUp += (sender, args) => MouseUp?.Invoke(this, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseDown += (sender, args) => MouseDown?.Invoke(this, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseMove += (sender, args) => MouseMove?.Invoke(this, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseEnter += (sender, args) => MouseEnter?.Invoke(this, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseLeave += (sender, args) => MouseLeave?.Invoke(this, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseDoubleClick += (sender, args) => MouseDoubleClick?.Invoke(this, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseMove += (sender, args) => MouseMove?.Invoke(this, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseWheel += (sender, args) => MouseWheel?.Invoke(this, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
        }

        private void OnSizeChanged(object sender, EventArgs args)
        {
            MakeBitmap();
            OnIsReady();
            Invalidate();
        }

        private void OnLoadComplete(object sender, EventArgs args)
        {
            MakeBitmap();
            OnIsReady();
        }

        private void OnPaint(object sender, PaintEventArgs args)
        {
            if (_bitmap != null) args.Graphics.DrawImage(_bitmap, args.ClipRectangle, args.ClipRectangle);
        }

        public void ChangeScale(int scale)
        {
            _scale = scale;
            MakeBitmap();
            OnIsReady();
            Invalidate();
        }

        public void Clear()
        {
            MakeBitmap();
        }

        private void MakeBitmap()
        {
            _bitmap = null;
            if (Width == 0 || Height == 0) return;
            _bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppRgba, Enumerable.Repeat(BackgroundColor, Width * Height - 1));
        }

        public void DrawPoint(DrawPoint point) => DrawPoint(point.X, point.Y, point.Color);
        public void DrawPoint(int x, int y, Color color)
        {
            using (var bitmapData = _bitmap.Lock())
            {
                var space = InternalDrawPoint(x, y, color, bitmapData);
                Invalidate(space);
            }
        }

        public void DrawAll(IEnumerable<DrawPoint> points)
        {
            var isFirstPass = true;
            Rectangle invalidation = default;
            using (var bitmapData = _bitmap.Lock())
            {
                foreach (var drawPoint in points)
                {
                    var space = InternalDrawPoint(drawPoint.X, drawPoint.Y, drawPoint.Color, bitmapData);
                    if (isFirstPass)
                    {
                        invalidation = space;
                        isFirstPass = false;
                        continue;
                    }

                    invalidation.Union(space);
                }
            }
            
            if (!isFirstPass)
                Invalidate(invalidation);
        }

        private Rectangle InternalDrawPoint(int x, int y, Color color, BitmapData bitmapData)
        {
            var screenRect = new Rectangle(x * _scale, y * _scale, _scale - 1, _scale - 1);
            for (var ix = screenRect.Left; ix <= screenRect.Right; ix++)
            for (var iy = screenRect.Top; iy <= screenRect.Bottom; iy++)
            {
                bitmapData.SetPixel(ix, iy, color);
            }

            return screenRect;
        }

        protected virtual void OnIsReady()
        {
            IsReady?.Invoke(this, EventArgs.Empty);
        }
    }

    public struct DrawPoint
    {
        public int X;
        public int Y;
        public Color Color;
    }

    public class BitmapMouseEventArgs : MouseEventArgs
    {
        public Point MapLocation { get; }

        public BitmapMouseEventArgs(MouseEventArgs originalEvent, Point mapLocation) 
            : base(originalEvent.Buttons, originalEvent.Modifiers, originalEvent.Location, originalEvent.Delta, originalEvent.Pressure)
        {
            MapLocation = mapLocation;
        }
    }
}