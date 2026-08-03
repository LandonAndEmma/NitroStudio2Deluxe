using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using System.Globalization;

namespace NitroStudio2.Controls
{
    /// <summary>
    /// The left margin of the sequence editor, showing each line's command index rather than its
    /// line number, so the numbers line up with the commands the file actually stores.
    ///
    /// WinForms got this from Scintilla's MarginType.RightText, writing a MarginText string onto
    /// every line. AvaloniaEdit has no per-line margin text, so the indices are computed from the
    /// document (see <see cref="SequenceHighlighting.CommandIndices"/>) and drawn here.
    /// </summary>
    public sealed class CommandIndexMargin : AbstractMargin
    {
        /// <summary>Width Scintilla's margin 0 was fixed at.</summary>
        private const double MarginWidth = 35;

        private static readonly IBrush Background =
            new SolidColorBrush(Color.FromRgb(0x2F, 0x2F, 0x2F));

        private static readonly IBrush Foreground =
            new SolidColorBrush(Color.FromRgb(0xB7, 0xB7, 0xB7));

        private int[] indices = [];

        public CommandIndexMargin()
        {
            ClipToBounds = true;
        }

        /// <summary>Font the numbers are drawn in; the editor keeps this in step with its own.</summary>
        public Typeface Typeface { get; set; } = new(FontFamily.Default);

        public double EmSize { get; set; } = 14;

        /// <summary>Recomputes the indices; call whenever the document text changes.</summary>
        public void Refresh()
        {
            if (Document is null)
            {
                indices = [];
            }
            else
            {
                string[] lines = new string[Document.LineCount];
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = Document.GetText(Document.Lines[i]);
                }
                indices = SequenceHighlighting.CommandIndices(lines);
            }
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize) =>
            new(MarginWidth, 0);

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Background, new Rect(Bounds.Size));

            TextView textView = TextView;
            if (textView?.VisualLinesValid != true)
            {
                return;
            }


            foreach (VisualLine visualLine in textView.VisualLines)
            {
                int lineNumber = visualLine.FirstDocumentLine.LineNumber;
                if (lineNumber < 1 || lineNumber > indices.Length)
                {
                    continue;
                }

                FormattedText text = new(
                    indices[lineNumber - 1].ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface,
                    EmSize,
                    Foreground
                );
                double y =
                    visualLine.GetTextLineVisualYPosition(
                        visualLine.TextLines[0],
                        VisualYPosition.TextTop
                    ) - textView.VerticalOffset;
                // Right-aligned, matching Scintilla's MarginType.RightText.
                context.DrawText(text, new Point(Bounds.Width - text.Width - 4, y));
            }
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView is not null)
            {
                oldTextView.VisualLinesChanged -= OnVisualLinesChanged;
            }
            base.OnTextViewChanged(oldTextView, newTextView);
            if (newTextView is not null)
            {
                newTextView.VisualLinesChanged += OnVisualLinesChanged;
            }
            Refresh();
        }

        private void OnVisualLinesChanged(object sender, System.EventArgs e) =>
            InvalidateVisual();
    }
}
