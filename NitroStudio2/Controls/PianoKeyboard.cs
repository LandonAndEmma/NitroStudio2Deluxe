using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GotaSequenceLib;
using System;
using System.Collections.Generic;

namespace NitroStudio2.Controls
{
    /// <summary>
    /// The 85-key on-screen piano from the bank and sequence editors.
    ///
    /// The key table is the one the WinForms EditorBase carried, unchanged: each entry is the
    /// note, its x offset, size, outline shape, and whether it is a black key. Positions are in
    /// the original 565x46 design space, which the control scales to fit.
    /// </summary>
    public class PianoKeyboard : Control
    {
        /// <summary>Design-space size the key coordinates below were laid out in.</summary>
        public const double DesignWidth = 565;
        public const double DesignHeight = 46;

        // Every black key is 8x29. The WinForms designer had left C# and F# a pixel shorter than
        // the other three, so the first black key of each group visibly sat high; that was an
        // artifact of hand-placing them, not something worth reproducing.
        private static readonly (Notes Note, int X, int Width, int Height, PianoKeyShape Shape, bool IsBlack)[] KeyDefs =
        {
            (Notes.cn7, 466, 12, 42, PianoKeyShape.LShape, false),
            (Notes.en7, 488, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cs7, 474, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.dn7, 477, 12, 42, PianoKeyShape.TShape, false),
            (Notes.ds7, 485, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.fn7, 499, 12, 42, PianoKeyShape.LShape, false),
            (Notes.fs7, 507, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.gn7, 510, 12, 42, PianoKeyShape.TShape, false),
            (Notes.gs7, 518, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.an7, 521, 12, 42, PianoKeyShape.TShape, false),
            (Notes.as7, 529, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.bn7, 532, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cn6, 389, 12, 42, PianoKeyShape.LShape, false),
            (Notes.en6, 411, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cs6, 397, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.dn6, 400, 12, 42, PianoKeyShape.TShape, false),
            (Notes.ds6, 408, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.fn6, 422, 12, 42, PianoKeyShape.LShape, false),
            (Notes.fs6, 430, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.gn6, 433, 12, 42, PianoKeyShape.TShape, false),
            (Notes.gs6, 441, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.an6, 444, 12, 42, PianoKeyShape.TShape, false),
            (Notes.as6, 452, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.bn6, 455, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cn1, 4, 12, 42, PianoKeyShape.LShape, false),
            (Notes.cs1, 12, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.dn1, 15, 12, 42, PianoKeyShape.TShape, false),
            (Notes.ds1, 23, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.en1, 26, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.fn1, 37, 12, 42, PianoKeyShape.LShape, false),
            (Notes.fs1, 45, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.gn1, 48, 12, 42, PianoKeyShape.TShape, false),
            (Notes.gs1, 56, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.an1, 59, 12, 42, PianoKeyShape.TShape, false),
            (Notes.as1, 67, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.bn1, 70, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cn2, 81, 12, 42, PianoKeyShape.LShape, false),
            (Notes.cs2, 89, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.dn2, 92, 12, 42, PianoKeyShape.TShape, false),
            (Notes.ds2, 100, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.en2, 103, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.fn2, 114, 12, 42, PianoKeyShape.LShape, false),
            (Notes.fs2, 122, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.gn2, 125, 12, 42, PianoKeyShape.TShape, false),
            (Notes.gs2, 133, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.an2, 136, 12, 42, PianoKeyShape.TShape, false),
            (Notes.as2, 144, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.bn2, 147, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cn3, 158, 12, 42, PianoKeyShape.LShape, false),
            (Notes.cs3, 166, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.dn3, 169, 12, 42, PianoKeyShape.TShape, false),
            (Notes.ds3, 177, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.en3, 180, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.fn3, 191, 12, 42, PianoKeyShape.LShape, false),
            (Notes.fs3, 199, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.gn3, 202, 12, 42, PianoKeyShape.TShape, false),
            (Notes.gs3, 210, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.an3, 213, 12, 42, PianoKeyShape.TShape, false),
            (Notes.as3, 221, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.bn3, 224, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cn4, 235, 12, 42, PianoKeyShape.LShape, false),
            (Notes.cs4, 243, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.dn4, 246, 12, 42, PianoKeyShape.TShape, false),
            (Notes.ds4, 254, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.en4, 257, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.fn4, 268, 12, 42, PianoKeyShape.LShape, false),
            (Notes.fs4, 276, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.gn4, 279, 12, 42, PianoKeyShape.TShape, false),
            (Notes.gs4, 287, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.an4, 290, 12, 42, PianoKeyShape.TShape, false),
            (Notes.as4, 298, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.bn4, 301, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cn5, 312, 12, 42, PianoKeyShape.LShape, false),
            (Notes.cs5, 320, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.dn5, 323, 12, 42, PianoKeyShape.TShape, false),
            (Notes.ds5, 331, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.en5, 334, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.fn5, 345, 12, 42, PianoKeyShape.LShape, false),
            (Notes.fs5, 353, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.gn5, 356, 12, 42, PianoKeyShape.TShape, false),
            (Notes.gs5, 364, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.an5, 367, 12, 42, PianoKeyShape.TShape, false),
            (Notes.as5, 375, 8, 29, PianoKeyShape.RectShape, true),
            (Notes.bn5, 378, 12, 42, PianoKeyShape.LShapeBackwards, false),
            (Notes.cn8, 543, 12, 42, PianoKeyShape.RectShape, false),
        };

        private readonly Dictionary<Notes, PianoKey> keys = [];
        private readonly Canvas surface = new() { Width = DesignWidth, Height = DesignHeight };
        private readonly Viewbox scaler;

        public PianoKeyboard()
        {
            foreach ((Notes note, int x, int width, int height, PianoKeyShape shape, bool isBlack) in KeyDefs)
            {
                PianoKey key = new()
                {
                    Name = "pkey" + note,
                    Width = width,
                    Height = height,
                    Shape = shape,
                    Orientation = PianoKeyOrientation.Vertical,
                    KeyOffColor = isBlack ? Colors.Black : Colors.White,
                    KeyOnColor = Colors.Blue,
                    // Black keys sit inside the notches of their white neighbours; drawing them
                    // last keeps their outline on top where the shapes meet.
                    ZIndex = isBlack ? 1 : 0,
                };
                key.StateChanged += OnKeyStateChanged;
                Canvas.SetLeft(key, x);
                Canvas.SetTop(key, 2);
                surface.Children.Add(key);
                keys[note] = key;
            }

            scaler = new Viewbox { Child = surface, Stretch = Stretch.Uniform };
            LogicalChildren.Add(scaler);
            VisualChildren.Add(scaler);
        }

        /// <summary>The note currently held down, as EditorBase's NoteDown field tracked it.</summary>
        public Notes NoteDown { get; private set; }

        /// <summary>Raised when a key goes down; NoteDown holds which one.</summary>
        public event EventHandler Pressed;

        /// <summary>Raised once no key is held any more.</summary>
        public event EventHandler Released;

        public PianoKey GetKey(Notes note)
        {
            return keys.GetValueOrDefault(note);
        }

        /// <summary>
        /// Paints the inclusive note range in the given colour to show an instrument's region.
        /// Black keys get the inverted colour so they stay legible against their neighbours,
        /// with cn8 excluded because it is a white key that happens to be drawn as a rectangle.
        /// </summary>
        public void ColorRegion(Color color, byte start, byte end)
        {
            for (int note = start; note <= end && note <= byte.MaxValue; note++)
            {
                PianoKey key = GetKey((Notes)note);
                if (key is null)
                {
                    continue;
                }
                key.KeyOffColor =
                    key.Shape == PianoKeyShape.RectShape && (Notes)note != Notes.cn8
                        ? Color.FromArgb(255, (byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B))
                        : color;
            }
        }

        /// <summary>Restores every key to its plain black/white colouring.</summary>
        public void ResetColors()
        {
            foreach ((Notes note, _, _, _, _, bool isBlack) in KeyDefs)
            {
                keys[note].KeyOffColor = isBlack ? Colors.Black : Colors.White;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            scaler.Measure(availableSize);
            return scaler.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            scaler.Arrange(new Rect(finalSize));
            return finalSize;
        }

        private void OnKeyStateChanged(object sender, EventArgs e)
        {
            // Same scan EditorBase.PianoChanged did: the first key found down wins.
            foreach (KeyValuePair<Notes, PianoKey> pair in keys)
            {
                if (pair.Value.IsKeyOn())
                {
                    NoteDown = pair.Key;
                    Pressed?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
            Released?.Invoke(this, EventArgs.Empty);
        }
    }
}
