using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System.Collections.Generic;

namespace NitroStudio2.Controls
{
    /// <summary>
    /// Paints sequence-assembly lines using <see cref="SequenceHighlighting"/>. Replaces
    /// Scintilla's StartStyling/SetStyling byte runs; AvaloniaEdit only asks about lines that
    /// are actually on screen, so the incremental +/-500 character window the old StyleNeeded
    /// handler maintained is no longer needed.
    /// </summary>
    public sealed class SequenceColorizer : DocumentColorizingTransformer
    {
        /// <summary>The palette the WinForms editor set on each Scintilla style slot.</summary>
        private static readonly Dictionary<CommandStyleType, IBrush> Palette = new()
        {
            [CommandStyleType.Regular] = new SolidColorBrush(Color.FromRgb(0xE7, 0xE7, 0xE7)),
            [CommandStyleType.Comment] = new SolidColorBrush(Color.FromRgb(0xAE, 0xAE, 0xAE)),
            [CommandStyleType.Label] = new SolidColorBrush(Color.FromRgb(0xE7, 0xBB, 0x00)),
            [CommandStyleType.Prefix] = new SolidColorBrush(Color.FromRgb(0x4A, 0xF0, 0xB6)),
            [CommandStyleType.Value0] = new SolidColorBrush(Colors.Red),
            [CommandStyleType.Value1] = new SolidColorBrush(Colors.Orange),
            [CommandStyleType.Value2] = new SolidColorBrush(Colors.Yellow),
            [CommandStyleType.Value3] = new SolidColorBrush(Colors.LimeGreen),
            [CommandStyleType.Value4] = new SolidColorBrush(Colors.LightBlue),
            [CommandStyleType.Value5] = new SolidColorBrush(Colors.PaleVioletRed),
        };

        protected override void ColorizeLine(DocumentLine line)
        {
            if (line.Length == 0)
            {
                return;
            }

            string text = CurrentContext.Document.GetText(line);
            CommandStyleType[] styles = SequenceHighlighting.StyleLine(text);

            // Collapse the per-character styles into runs so each colour change is one call.
            int runStart = 0;
            for (int i = 1; i <= styles.Length; i++)
            {
                if (i < styles.Length && styles[i] == styles[runStart])
                {
                    continue;
                }
                if (Palette.TryGetValue(styles[runStart], out IBrush brush))
                {
                    ChangeLinePart(
                        line.Offset + runStart,
                        line.Offset + i,
                        element => element.TextRunProperties.SetForegroundBrush(brush)
                    );
                }
                runStart = i;
            }
        }
    }
}
