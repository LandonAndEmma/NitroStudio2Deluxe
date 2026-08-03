using System;
using System.Collections.Generic;

namespace NitroStudio2.Controls
{
    /// <summary>Style slots the sequence text uses, in the order the WinForms editor declared them.</summary>
    public enum CommandStyleType
    {
        Null,
        Regular,
        Comment,
        Label,
        Prefix,
        Value0,
        Value1,
        Value2,
        Value3,
        Value4,
        Value5,
    }

    /// <summary>
    /// The sequence-assembly colouring rules, lifted from SequenceEditor.StyleSeq.
    ///
    /// Scintilla was told to style byte ranges as the lexer walked the whole buffer; AvaloniaEdit
    /// asks per visible line instead, so the same walk now runs over one line and returns a style
    /// for each character. The rules themselves are unchanged: a leading "label:" is one colour,
    /// a ';' comment runs to end of line, a leading conditional/random/variable prefix such as
    /// "_if" gets its own colour, and each further space steps the argument colour along.
    /// </summary>
    public static class SequenceHighlighting
    {
        /// <summary>Prefixes that colour the first token differently, e.g. "_if" or "_tv".</summary>
        private static readonly string[] PrefixTokens = ["_if", "_v", "_r", "_t", "_tv", "_tr"];

        /// <summary>Styles each character of one line. Index 0 (Null) means "leave as default".</summary>
        public static CommandStyleType[] StyleLine(string source)
        {
            string l = source.Replace('\t', ' ');
            CommandStyleType[] styles = new CommandStyleType[l.Length];
            CommandStyleType style = CommandStyleType.Regular;
            bool initialSpaceCut = false;
            string withoutInitialSpace = l;
            int numWhiteSpace = 0;

            for (int j = 0; j < l.Length; j++)
            {
                if (l.Contains(':') && j == 0)
                {
                    int end = l.IndexOf(':') + 1;
                    for (int k = 0; k < end && k < l.Length; k++)
                    {
                        styles[k] = CommandStyleType.Label;
                    }
                    j += end;
                    if (j >= l.Length)
                    {
                        break;
                    }
                }

                bool kill = false;
                while (l[j] == ' ' && !initialSpaceCut)
                {
                    j++;
                    if (j >= l.Length)
                    {
                        kill = true;
                        break;
                    }
                    withoutInitialSpace = l[j..];
                    numWhiteSpace = j;
                }
                initialSpaceCut = true;
                if (kill)
                {
                    break;
                }

                char c = l[j];
                if (c == ';')
                {
                    for (int k = j; k < l.Length; k++)
                    {
                        styles[k] = CommandStyleType.Comment;
                    }
                    break;
                }

                if (c == '_')
                {
                    string token = l[j..].Split(' ')[0];
                    bool afterSpace =
                        withoutInitialSpace.Contains(' ')
                        && j > withoutInitialSpace.IndexOf(' ') + numWhiteSpace;
                    if (!afterSpace && IsPrefixToken(token))
                    {
                        style = CommandStyleType.Prefix;
                    }
                }

                if (c == ' ' && j > 0 && l[j - 1] != ' ')
                {
                    if (style < CommandStyleType.Prefix)
                    {
                        style = CommandStyleType.Prefix;
                    }
                    style++;
                }

                styles[j] = style;
            }
            return styles;
        }

        private static bool IsPrefixToken(string token)
        {
            foreach (string prefix in PrefixTokens)
            {
                if (
                    token.Contains(prefix + " ", StringComparison.Ordinal)
                    || token.EndsWith(prefix, StringComparison.Ordinal)
                )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Per-line command indices for the left margin: each line is numbered with how many
        /// command lines came before it. Blank lines and label lines do not advance the count,
        /// and a ';' comment is stripped before deciding. Port of UpdateLineNumbers.
        /// </summary>
        public static int[] CommandIndices(IReadOnlyList<string> lines)
        {
            int[] indices = new int[lines.Count];
            int sum = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                indices[i] = sum;
                if (IsCommandLine(lines[i]))
                {
                    sum++;
                }
            }
            return indices;
        }

        /// <summary>True when a line carries an actual command rather than a label or blank.</summary>
        public static bool IsCommandLine(string line)
        {
            string s = line;
            if (s.Contains(';'))
            {
                s = s.Split(';')[0];
            }
            s = s.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
            return s != "" && !s.EndsWith(':');
        }
    }
}
