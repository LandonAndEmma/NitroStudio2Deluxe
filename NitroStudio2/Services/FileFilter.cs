using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroStudio2.Services
{
    /// <summary>
    /// Translates the WinForms filter strings the editors already use
    /// ("Sound Sequence|*.sseq|Music List|*.mus") into Avalonia file picker types, so every call
    /// site can keep its filter exactly as written.
    /// </summary>
    public static class FileFilter
    {
        public static IReadOnlyList<FilePickerFileType> Parse(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return [AnyFile];
            }

            string[] parts = filter.Split('|');
            List<FilePickerFileType> types = [];
            for (int i = 0; i + 1 < parts.Length; i += 2)
            {
                string[] patterns = parts[i + 1]
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();
                types.Add(new FilePickerFileType(parts[i]) { Patterns = patterns });
            }
            return types.Count > 0 ? types : [AnyFile];
        }

        public static FilePickerFileType AnyFile { get; } =
            new("All Files") { Patterns = ["*"] };

        /// <summary>
        /// The default extension a save dialog should append when the user types a bare name,
        /// taken from the first pattern of the chosen filter the way WinForms did.
        /// </summary>
        public static string DefaultExtension(string filter)
        {
            IReadOnlyList<FilePickerFileType> types = Parse(filter);
            string pattern = types
                .SelectMany(t => t.Patterns ?? [])
                .FirstOrDefault(p => p.StartsWith("*.", StringComparison.Ordinal) && p != "*.*");
            return pattern is null ? null : pattern[1..];
        }
    }
}
