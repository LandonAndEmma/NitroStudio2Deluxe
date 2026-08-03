using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using System;
using System.Collections.Generic;

namespace NitroStudio2.Controls
{
    /// <summary>
    /// The sequence-assembly editor, replacing the Scintilla control the WinForms build hosted.
    /// Carries the same dark theme, syntax colouring and command-index margin.
    /// </summary>
    public class SequenceTextEditor : TextEditor
    {
        private readonly CommandIndexMargin indexMargin = new();

        public SequenceTextEditor()
        {
            Background = new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21));
            Foreground = new SolidColorBrush(Color.FromRgb(0xE7, 0xE7, 0xE7));
            FontFamily = MonospaceFont;
            FontSize = 14; // Scintilla's "Consolas 11pt" in device-independent pixels.
            WordWrap = false;
            ShowLineNumbers = false;
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;

            Options.ShowBoxForControlCharacters = false;
            Options.IndentationSize = 4;
            Options.ConvertTabsToSpaces = false;

            TextArea.Caret.CaretBrush = Brushes.White;
            TextArea.TextView.Options.HighlightCurrentLine = false;
            TextArea.TextView.LineTransformers.Add(new SequenceColorizer());
            indexMargin.Typeface = new Typeface(FontFamily);
            indexMargin.EmSize = FontSize;
            TextArea.LeftMargins.Add(indexMargin);

            TextChanged += (_, _) => indexMargin.Refresh();
        }

        /// <summary>
        /// Avalonia keys ControlThemes by exact type, so without this a subclass of TextEditor
        /// gets no template at all and renders as an empty box.
        /// </summary>
        protected override Type StyleKeyOverride => typeof(TextEditor);

        /// <summary>
        /// Consolas exists only on Windows, so fall back through the usual monospace faces the
        /// other platforms ship.
        /// </summary>
        private static FontFamily MonospaceFont =>
            new("Consolas,DejaVu Sans Mono,Menlo,Liberation Mono,monospace");

        /// <summary>Document text split into lines, as SEQ.FromText expects it.</summary>
        public List<string> Lines
        {
            get
            {
                List<string> lines = [];
                foreach (var line in Document.Lines)
                {
                    lines.Add(Document.GetText(line));
                }
                return lines;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // The WinForms editor swallowed Ctrl+G so Scintilla's go-to-line dialog never opened.
            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.G)
            {
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }
    }
}
