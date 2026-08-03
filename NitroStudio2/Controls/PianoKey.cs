using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering;
using System;
using System.Diagnostics;

namespace NitroStudio2.Controls
{
    public enum PianoKeyOrientation
    {
        Vertical,
        HorizontalLeft,
        HorizontalRight,
    }

    public enum PianoKeyShape
    {
        LShape,
        LShapeBackwards,
        TShape,
        RectShape,
    }

    /// <summary>
    /// One key of the on-screen piano, drawn as an L, backwards-L, T or plain rectangle so
    /// neighbouring white keys interlock around the black ones.
    ///
    /// Ported from the WinForms control of the same name. The point generators are unchanged;
    /// what changes is the plumbing: System.Drawing's GraphicsPath/Region becomes a
    /// StreamGeometry used both for drawing and, through <see cref="ICustomHitTest"/>, for
    /// hit-testing, which is what keeps a white key's notch from swallowing clicks meant for
    /// the black key sitting in it.
    /// </summary>
    public class PianoKey : Control, ICustomHitTest
    {
        private const int PointCountLShape = 7;
        private const int PointCountTShape = 9;
        private const int PointCountRectShape = 5;
        private const double FlatKeyOffset = 0.6666666;

        public static readonly StyledProperty<PianoKeyOrientation> OrientationProperty =
            AvaloniaProperty.Register<PianoKey, PianoKeyOrientation>(
                nameof(Orientation),
                PianoKeyOrientation.Vertical
            );

        public static readonly StyledProperty<PianoKeyShape> ShapeProperty =
            AvaloniaProperty.Register<PianoKey, PianoKeyShape>(
                nameof(Shape),
                PianoKeyShape.LShape
            );

        public static readonly StyledProperty<Color> KeyOnColorProperty =
            AvaloniaProperty.Register<PianoKey, Color>(nameof(KeyOnColor), Colors.Blue);

        public static readonly StyledProperty<Color> KeyOffColorProperty =
            AvaloniaProperty.Register<PianoKey, Color>(nameof(KeyOffColor), Colors.White);

        private static readonly IPen BorderPen = new Pen(Brushes.Black, 2.0);

        private Point[] points = [];
        private StreamGeometry geometry;
        private bool keyOn;

        static PianoKey()
        {
            AffectsRender<PianoKey>(KeyOnColorProperty, KeyOffColorProperty);
            AffectsGeometry(OrientationProperty, ShapeProperty);
        }

        public PianoKey()
        {
            Width = 19;
            Height = 51;
        }

        /// <summary>Raised whenever the key goes down or comes back up.</summary>
        public event EventHandler StateChanged;

        public PianoKeyOrientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public PianoKeyShape Shape
        {
            get => GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        public Color KeyOnColor
        {
            get => GetValue(KeyOnColorProperty);
            set => SetValue(KeyOnColorProperty, value);
        }

        public Color KeyOffColor
        {
            get => GetValue(KeyOffColorProperty);
            set => SetValue(KeyOffColorProperty, value);
        }

        public bool IsKeyOn() => keyOn;

        public void TurnKeyOn()
        {
            if (!keyOn)
            {
                keyOn = true;
                StateChanged?.Invoke(this, EventArgs.Empty);
                InvalidateVisual();
            }
        }

        public void TurnKeyOff()
        {
            if (keyOn)
            {
                keyOn = false;
                StateChanged?.Invoke(this, EventArgs.Empty);
                InvalidateVisual();
            }
        }

        // ------------------------------------------------------------------ input

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                TurnKeyOn();
                // Avalonia captures the pointer to the pressed control, which would stop every
                // other key seeing enter and leave for the rest of the drag. Releasing capture
                // is what lets a held pointer glide across the keyboard, and is the same thing
                // the WinForms control did with Capture = false.
                e.Pointer.Capture(null);
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            TurnKeyOff();
            base.OnPointerReleased(e);
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            // Gliding across the keyboard with the button held plays each key in turn. This
            // relies on OnPointerPressed having released the pointer capture, without which no
            // other key would ever see this event during a drag.
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                TurnKeyOn();
            }
            base.OnPointerEntered(e);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            TurnKeyOff();
            base.OnPointerExited(e);
        }

        /// <summary>Only the drawn polygon is clickable, not the whole bounding box.</summary>
        public bool HitTest(Point point) => Geometry?.FillContains(point) ?? false;

        // ------------------------------------------------------------------ rendering

        public override void Render(DrawingContext context)
        {
            StreamGeometry g = Geometry;
            if (g is null)
            {
                return;
            }
            // Clip to the key before stroking it. Avalonia centres a pen on the path, so half of
            // the 2px outline would fall outside the key: a black key would render 10px wide in
            // the 8px gap and overlap its neighbours. WinForms got this for free by assigning the
            // same polygon to Control.Region, which clipped the outer half away.
            using (context.PushGeometryClip(g))
            {
                context.DrawGeometry(
                    new SolidColorBrush(keyOn ? KeyOnColor : KeyOffColor),
                    BorderPen,
                    g
                );
            }
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            geometry = null;
            base.OnSizeChanged(e);
        }

        private static void AffectsGeometry(params AvaloniaProperty[] properties)
        {
            foreach (AvaloniaProperty property in properties)
            {
                property.Changed.AddClassHandler<PianoKey>(
                    (key, _) =>
                    {
                        key.geometry = null;
                        key.InvalidateVisual();
                    }
                );
            }
        }

        private StreamGeometry Geometry
        {
            get
            {
                if (geometry is null && Bounds.Width > 0 && Bounds.Height > 0)
                {
                    InitPoints();
                    geometry = BuildGeometry();
                }
                return geometry;
            }
        }

        private StreamGeometry BuildGeometry()
        {
            StreamGeometry g = new();
            using (StreamGeometryContext ctx = g.Open())
            {
                ctx.BeginFigure(points[0], true);
                for (int i = 1; i < points.Length; i++)
                {
                    ctx.LineTo(points[i]);
                }
                ctx.EndFigure(true);
            }
            return g;
        }

        // ------------------------------------------------------------------ key outlines
        // Straight port of the WinForms point generators; only Point/Size types differ.

        private double W => Bounds.Width;

        private double H => Bounds.Height;

        private void InitPoints()
        {
            if (
                Orientation is PianoKeyOrientation.HorizontalLeft
                    or PianoKeyOrientation.HorizontalRight
            )
            {
                InitPointsHorz();
            }
            else
            {
                InitPointsVert();
            }
        }

        private void InitPointsHorz()
        {
            switch (Shape)
            {
                case PianoKeyShape.LShape:
                    InitPointsHorzLShape();
                    break;
                case PianoKeyShape.LShapeBackwards:
                    InitPointsHorzLShape();
                    FlipVertically();
                    break;
                case PianoKeyShape.TShape:
                    InitPointsHorzTShape();
                    break;
                case PianoKeyShape.RectShape:
                    InitPointsRectShape();
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            if (Orientation == PianoKeyOrientation.HorizontalRight)
            {
                FlipHorizontally();
            }
        }

        private void InitPointsVert()
        {
            switch (Shape)
            {
                case PianoKeyShape.LShape:
                    InitPointsVertLShape();
                    break;
                case PianoKeyShape.LShapeBackwards:
                    InitPointsVertLShape();
                    FlipHorizontally();
                    break;
                case PianoKeyShape.TShape:
                    InitPointsVertTShape();
                    break;
                case PianoKeyShape.RectShape:
                    InitPointsRectShape();
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void InitPointsHorzLShape()
        {
            points = new Point[PointCountLShape];
            double notch = Math.Round(H * (1.0 - FlatKeyOffset));
            double stem = Math.Round(W * FlatKeyOffset);
            points[0] = new Point(0, notch);
            points[1] = new Point(0, H);
            points[2] = new Point(W, H);
            points[3] = new Point(W, 0);
            points[4] = new Point(stem, 0);
            points[5] = new Point(stem, notch);
            points[6] = points[0];
        }

        private void InitPointsVertLShape()
        {
            points = new Point[PointCountLShape];
            double stem = Math.Round(W * FlatKeyOffset);
            double notch = Math.Round(H * FlatKeyOffset);
            points[0] = new Point(0, 0);
            points[1] = new Point(stem, 0);
            points[2] = new Point(stem, notch);
            points[3] = new Point(W, notch);
            points[4] = new Point(W, H);
            points[5] = new Point(0, H);
            points[6] = points[0];
        }

        private void InitPointsHorzTShape()
        {
            points = new Point[PointCountTShape];
            double top = Math.Round(H * (1.0 - FlatKeyOffset));
            double stem = Math.Round(W * FlatKeyOffset);
            double bottom = Math.Round(H * FlatKeyOffset);
            points[0] = new Point(0, top);
            points[1] = new Point(stem, top);
            points[2] = new Point(stem, 0);
            points[3] = new Point(W, 0);
            points[4] = new Point(W, H);
            points[5] = new Point(stem, H);
            points[6] = new Point(stem, bottom);
            points[7] = new Point(0, bottom);
            points[8] = points[0];
        }

        private void InitPointsVertTShape()
        {
            points = new Point[PointCountTShape];
            double left = Math.Round(W * (1.0 - FlatKeyOffset));
            double right = Math.Round(W * FlatKeyOffset);
            double notch = Math.Round(H * FlatKeyOffset);
            points[0] = new Point(left, 0);
            points[1] = new Point(right, 0);
            points[2] = new Point(right, notch);
            points[3] = new Point(W, notch);
            points[4] = new Point(W, H);
            points[5] = new Point(0, H);
            points[6] = new Point(0, notch);
            points[7] = new Point(left, notch);
            points[8] = points[0];
        }

        private void InitPointsRectShape()
        {
            points = new Point[PointCountRectShape];
            points[0] = new Point(0, 0);
            points[1] = new Point(W, 0);
            points[2] = new Point(W, H);
            points[3] = new Point(0, H);
            points[4] = points[0];
        }

        private void FlipHorizontally()
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point(W - points[i].X, points[i].Y);
            }
        }

        private void FlipVertically()
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point(points[i].X, H - points[i].Y);
            }
        }
    }
}
