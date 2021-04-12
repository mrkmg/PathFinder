using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace PathFinderGui.Widgets
{
    public class LayeredBitmapWidget : Drawable
    {
        public int BitmapWidth => Width / _scale;
        public int BitmapHeight => Height / _scale;
        public int Layers { get; private set; }

        private readonly List<Bitmap> _bitmaps = new();
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

        public LayeredBitmapWidget(int scale, int layers = 1)
        {
            Layers = layers;
            _scale = scale;
            
            Paint += OnPaint;
            LoadComplete += OnLoadComplete;
            SizeChanged += OnSizeChanged;
            UnLoad += OnUnLoad;

            base.MouseUp += (sender, args) => MouseUp?.Invoke(sender, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseDown += (sender, args) => MouseDown?.Invoke(sender, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseMove += (sender, args) => MouseMove?.Invoke(sender, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseEnter += (sender, args) => MouseEnter?.Invoke(sender, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseLeave += (sender, args) => MouseLeave?.Invoke(sender, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseDoubleClick += (sender, args) => MouseDoubleClick?.Invoke(sender, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseMove += (sender, args) => MouseMove?.Invoke(sender, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
            base.MouseWheel += (sender, args) => MouseWheel?.Invoke(sender, new BitmapMouseEventArgs(args, new Point(args.Location / _scale)));
        }

        private void OnUnLoad(object sender, EventArgs e)
        {
            foreach (var bitmap in _bitmaps)
            {
                bitmap.Dispose();
            }
        }

        private void OnSizeChanged(object sender, EventArgs args)
        {
            Clear();
            OnIsReady();
        }

        private void OnLoadComplete(object sender, EventArgs args)
        {
            Clear();
            OnIsReady();
        }

        private void OnPaint(object sender, PaintEventArgs args)
        {
            foreach (var bitmap in _bitmaps)
            {
                args.Graphics.DrawImage(bitmap, args.ClipRectangle, args.ClipRectangle);
            }
        }

        public void RemoveLayer(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= Layers)
                throw new ArgumentOutOfRangeException(nameof(layerIndex), layerIndex, "Must be between 0 and layers - 1");
            _bitmaps[layerIndex].Dispose();
            _bitmaps.RemoveAt(layerIndex);
            Layers--;
            Invalidate();
        }

        public int AddLayer()
        {
            Layers++;
            _bitmaps.Add(new Bitmap(Width, Height, PixelFormat.Format32bppRgba, Enumerable.Repeat(Colors.Transparent, Width * Height - 1)));
            return Layers - 1;
        }

        public void ChangeScale(int scale)
        {
            _scale = scale;
            Clear();
            OnIsReady();
        }

        public void Clear()
        {
            MakeBitmaps();
            Invalidate();
        }

        public void ClearLayer(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= Layers)
                throw new ArgumentOutOfRangeException(nameof(layerIndex), layerIndex, "Must be between 0 and layers - 1");
            using (var bd = _bitmaps[layerIndex].Lock())
            {
                bd.SetPixels(Enumerable.Repeat(Colors.Transparent, Width * Height - 1));
            }
            Invalidate();
        }

        private void MakeBitmaps()
        {
            if (Width == 0 || Height == 0) return;
            
            foreach (var bitmap in _bitmaps)
            {
                bitmap.Dispose();
            }
            _bitmaps.Clear();
            for (var i = 0; i < Layers; i++)
            {
                _bitmaps.Add(
                    new Bitmap(Width, Height, PixelFormat.Format32bppRgba,
                        i == 0
                            ? Enumerable.Repeat(BackgroundColor, Width * Height - 1)
                            : Enumerable.Repeat(Colors.Transparent, Width * Height - 1)));     
            }
        }

        public void DrawPoint(int layerIndex, DrawPoint point) => DrawPoint(layerIndex, point.X, point.Y, point.Color);
        public void DrawPoint(int layerIndex, int x, int y, Color color)
        {
            if (layerIndex < 0 || layerIndex >= Layers)
                throw new ArgumentOutOfRangeException(nameof(layerIndex), layerIndex, "Must be between 0 and layers - 1");
            using var bitmapData = _bitmaps[layerIndex].Lock();
            var space = InternalDrawPoint(x, y, color, bitmapData);
            Invalidate(space);
        }

        public void DrawAllPoints(int layerIndex, IEnumerable<DrawPoint> points)
        {
            if (layerIndex < 0 || layerIndex >= Layers)
                throw new ArgumentOutOfRangeException(nameof(layerIndex), layerIndex, "Must be between 0 and layers - 1");
            using var bitmapData = _bitmaps[layerIndex].Lock();
            var isFirstPass = true;
            Rectangle invalidation = default;
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

        private void OnIsReady()
        {
            IsReady?.Invoke(this, EventArgs.Empty);
        }
    }

    public readonly struct DrawPoint
    {
        public readonly int X;
        public readonly int Y;
        public readonly Color Color;

        public DrawPoint(int x, int y, Color color)
        {
            X = x;
            Y = y;
            Color = color;
        }
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