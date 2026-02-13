using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
namespace Multimedia.UI
{
    public enum PianoKeyOrientation
    {
        Vertical,
        HorizontalLeft,
        HorizontalRight
    }
    public enum PianoKeyShape
    {
        LShape,
        LShapeBackwards,
        TShape,
        RectShape
    }
	public class PianoKey : Control
	{
        #region Constants
        private const int PointCountLShape = 7;
        private const int PointCountTShape = 9;
        private const int PointCountRectShape = 5;
        private const double FlatKeyOffset = 0.6666666;
        #endregion
        #region Fields
        private PianoKeyOrientation orientation;
        private PianoKeyShape shape;
        private bool keyOn = false;
        private SolidBrush keyOnBrush = new SolidBrush(Color.Blue);
        private SolidBrush keyOffBrush = new SolidBrush(Color.White);
        private Pen borderPen = new Pen(Color.Black, 2.0f);
        private Point[] points;
        private System.ComponentModel.Container components = null;
        #endregion
        #region Events
        public event EventHandler StateChanged;
        #endregion
        #region Construction
        public PianoKey()
        {
            InitializeComponent();
            this.orientation = PianoKeyOrientation.Vertical;
            this.shape = PianoKeyShape.LShape;
            Size = new Size(19, 51);
        }        
        #endregion
        #region Methods
        public void TurnKeyOn()
        {
            if(!IsKeyOn())
            {
                keyOn = true;
                if(StateChanged != null)
                {
                    StateChanged(this, new EventArgs());
                }
                Invalidate(Region);
            }
        }
        public void TurnKeyOff()
        {
            if(IsKeyOn())
            {
                keyOn = false;
                if(StateChanged != null)
                {
                    StateChanged(this, new EventArgs());
                }
                Invalidate(Region);
            }
        }
        public bool IsKeyOn()
        {
            return keyOn;
        }
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}
		#region Component Designer generated code
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
        protected override void OnMouseEnter(EventArgs e)
        {
            if(Control.MouseButtons == MouseButtons.Left)
            {
                TurnKeyOn();
            }
            base.OnMouseEnter (e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if(IsKeyOn())
            {
                TurnKeyOff();
            }
            base.OnMouseLeave (e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            TurnKeyOn();
            base.OnMouseDown (e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            TurnKeyOff();
            base.OnMouseUp (e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if(IsKeyOn())
            {
                if(!Region.IsVisible(new Point(e.X, e.Y)))
                {
                    TurnKeyOff();
                    Capture = false;
                }
            }
            base.OnMouseMove (e);
        }
		protected override void OnPaint(PaintEventArgs pe)
		{
            if(IsKeyOn())
            {
                pe.Graphics.FillRegion(keyOnBrush, Region);
            }
            else
            {
                pe.Graphics.FillRegion(keyOffBrush, Region);                
            }           
            pe.Graphics.DrawPolygon(borderPen, points);             
			base.OnPaint(pe);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            InitPoints();
            CreateRegion();
            base.OnSizeChanged (e);
        }
        private void InitPoints()
        {
            if(Orientation == PianoKeyOrientation.HorizontalLeft ||
                Orientation == PianoKeyOrientation.HorizontalRight)
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
            switch(shape)
            {
                case PianoKeyShape.LShape:
                    InitPointsHorzLShape();
                    break;
                case PianoKeyShape.LShapeBackwards:
                    InitPointsHorzLShapeBackwards();
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
            if(Orientation == PianoKeyOrientation.HorizontalRight)
            {
                FlipHorizontally();
            }
        }
        private void InitPointsVert()
        {
            switch(shape)
            {
                case PianoKeyShape.LShape:
                    InitPointsVertLShape();
                    break;
                case PianoKeyShape.LShapeBackwards:
                    InitPointsVertLShapeBackwards();
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
            points[0].X = 0;
            points[0].Y = (int)Math.Round(Size.Height * (1.0 - FlatKeyOffset));
            points[1].X = 0;
            points[1].Y = Size.Height;
            points[2].X = Size.Width;
            points[2].Y = Size.Height;
            points[3].X = Size.Width;
            points[3].Y = 0;
            points[4].X = (int)Math.Round(Size.Width * FlatKeyOffset);
            points[4].Y = 0;
            points[5].X = points[4].X;
            points[5].Y = points[0].Y;
            points[6] = points[0];            
        }
        private void InitPointsHorzLShapeBackwards()
        {
            InitPointsHorzLShape();
            FlipVertically();
        }
        private void InitPointsVertLShape()
        {
            points = new Point[PointCountLShape];
            points[0].X = 0;
            points[0].Y = 0;
            points[1].X = (int)Math.Round(Size.Width * FlatKeyOffset);
            points[1].Y = 0;
            points[2].X = points[1].X;
            points[2].Y = (int)Math.Round(Size.Height * FlatKeyOffset);
            points[3].X = Size.Width;
            points[3].Y = points[2].Y;
            points[4].X = Size.Width;
            points[4].Y = Size.Height;
            points[5].X = 0;
            points[5].Y = Size.Height;
            points[6].X = points[0].X;
            points[6].Y = points[0].Y;            
        }
        private void InitPointsVertLShapeBackwards()
        {
            InitPointsVertLShape();
            FlipHorizontally();
        }
        private void InitPointsHorzTShape()
        {
            points = new Point[PointCountTShape];
            points[0].X = 0;
            points[0].Y = (int)Math.Round(Size.Height * (1.0 - FlatKeyOffset));
            points[1].X = (int)Math.Round(Size.Width * FlatKeyOffset);
            points[1].Y = points[0].Y;
            points[2].X = points[1].X;
            points[2].Y = 0;
            points[3].X = Size.Width;
            points[3].Y = 0;
            points[4].X = Size.Width;
            points[4].Y = Size.Height;
            points[5].X = points[1].X;
            points[5].Y = Size.Height;
            points[6].X = points[1].X;
            points[6].Y = (int)Math.Round(Size.Height * FlatKeyOffset);
            points[7].X = 0;
            points[7].Y = points[6].Y;
            points[8] = points[0];            
        }
        private void InitPointsVertTShape()
        {
            points = new Point[PointCountTShape];
            points[0].X = (int)Math.Round(Size.Width * (1.0 - FlatKeyOffset));
            points[0].Y = 0;
            points[1].X = (int)Math.Round(Size.Width * FlatKeyOffset);
            points[1].Y = 0;
            points[2].X = points[1].X;
            points[2].Y = (int)Math.Round(Size.Height * FlatKeyOffset);
            points[3].X = Size.Width;
            points[3].Y = points[2].Y;
            points[4].X = Size.Width;
            points[4].Y = Size.Height;
            points[5].X = 0;
            points[5].Y = Size.Height;
            points[6].X = 0;
            points[6].Y = points[2].Y;
            points[7].X = points[0].X;
            points[7].Y = points[2].Y;
            points[8] = points[0];            
        }
        private void InitPointsRectShape()
        {
            points = new Point[PointCountRectShape];
            points[0].X = 0;
            points[0].Y = 0;
            points[1].X = Size.Width;
            points[1].Y = 0;
            points[2].X = Size.Width;
            points[2].Y = Size.Height;
            points[3].X = 0;
            points[3].Y = Size.Height;
            points[4] = points[0];
        }
        private void FlipHorizontally()
        {
            for(int i = 0; i < points.Length; i++)
            {
                points[i].X = Size.Width - points[i].X;
            }
        }
        private void FlipVertically()
        {
            for(int i = 0; i < points.Length; i++)
            {
                points[i].Y = Size.Height - points[i].Y;
            }
        }
        private void CreateRegion()
        {  
            byte[] types = new byte[points.Length];
            for(int i = 0; i < types.Length; i++)
            {
                types[i] = (byte)PathPointType.Line;
            }
            GraphicsPath path = new GraphicsPath(points, types);
            Region = new Region(path); 
            Invalidate(Region);
        }   
        #endregion
        #region Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public PianoKeyOrientation Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if(orientation == PianoKeyOrientation.Vertical)
                {
                    if(value == PianoKeyOrientation.HorizontalLeft ||
                        value == PianoKeyOrientation.HorizontalRight)
                    {
                        orientation = value;
                        Size = new Size(Height, Width);
                    }
                }  
                else
                {
                    if(value == PianoKeyOrientation.Vertical)
                    {
                        orientation = value;
                        Size = new Size(Height, Width);
                    }
                    else
                    {
                        orientation = value;
                        InitPoints();
                        CreateRegion();
                    }
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public PianoKeyShape Shape
        {
            get
            {
                return shape;
            }
            set
            {
                shape = value;
                InitPoints();
                CreateRegion();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color KeyOnColor
        {
            get
            {
                return keyOnBrush.Color;
            }
            set
            {
                keyOnBrush.Color = value;
                if(IsKeyOn())
                {
                    Invalidate(Region);
                }
            }
        }  
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color KeyOffColor
        {
            get
            {
                return keyOffBrush.Color;
            }
            set
            {
                keyOffBrush.Color = value;
                if(!IsKeyOn())
                {
                    Invalidate(Region);
                }
            }
        }
        #endregion
    }
}