using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace NitroStudio2.Services
{
    /// <summary>
    /// Loads the images extracted out of the WinForms .resx files by tools/extract-assets.py.
    /// Bitmaps are cached because the same menu and tree icons are reused across every window.
    /// </summary>
    public static class Assets
    {
        private static readonly string Root =
            "avares://" + Assembly.GetExecutingAssembly().GetName().Name + "/Assets/";

        private static readonly ConcurrentDictionary<string, Bitmap> Cache = new();

        /// <summary>
        /// The 19 tree icons, in the order the WinForms ImageList held them. Several entries
        /// share a file because the artwork is byte-identical; Assets holds one copy of each.
        /// </summary>
        public static readonly string[] TreeIconNames =
        [
            "Blank",
            "Version",
            "Sequence",
            "SequenceArchive",
            "Bank",
            "WaveArchive",
            "Player",
            "Group",
            "StreamPlayer",
            "Stream",
            "Record",
            "RecordArc",
            "Lookup",
            "RecordRegion",
            "Wave",
            "Ranged",
            "Regional",
            "Psg",
            "BankGenerator",
        ];

        /// <summary>Loads an asset by file name, e.g. "Save.png". Assets/ has no subfolders.</summary>
        public static Bitmap Bitmap(string fileName)
        {
            return Cache.GetOrAdd(
                fileName,
                p => new Bitmap(AssetLoader.Open(new Uri(Root + p)))
            );
        }

        /// <summary>An icon by name, without its extension. Menu, track and window icons all
        /// come from the same flat set, since many of them are the same picture.</summary>
        public static Bitmap Icon(string name)
        {
            return Bitmap(name + ".png");
        }

        public static Bitmap Menu(string name)
        {
            return Icon(name);
        }

        public static Bitmap Track(string name)
        {
            return Icon(name);
        }

        public static Bitmap WindowIcon(string name)
        {
            return Icon(name);
        }

        /// <summary>Tree icon by the same index the WinForms TreeNode ImageIndex used.</summary>
        public static Bitmap TreeIcon(int index)
        {
            return index >= 0 && index < TreeIconNames.Length
                ? Icon(TreeIconNames[index])
                : Icon("Blank");
        }
    }
}
