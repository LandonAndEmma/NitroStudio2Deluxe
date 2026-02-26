using GotaSoundIO.Sound.Formats;
using System;
using System.IO;
using System.Windows.Forms;

namespace NitroStudio2
{
    internal static class Program
    {
        public static string NitroPath = Application.StartupPath;

        [STAThread]
        private static void Main(string[] args)
        {
            // Enable DPI-aware rendering for high-DPI displays
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0)
            {
                switch (Path.GetExtension(args[0]))
                {
                    case ".sdat":
                        Application.Run(new MainWindow(args[0]));
                        break;
                    case ".sseq":
                        Application.Run(new SequenceEditor(args[0]));
                        break;
                    case ".ssar":
                        Application.Run(new SequenceArchiveEditor(args[0]));
                        break;
                    case ".sbnk":
                        Application.Run(new BankEditor(args[0]));
                        break;
                    case ".swar":
                        Application.Run(new WaveArchiveEditor(args[0]));
                        break;
                    case ".strm":
                        RiffWave r = new();
                        NitroFileLoader.Stream s = new();
                        s.Read(args[0]);
                        r.FromOtherStreamFile(s);
                        r.Write(MainWindow.NitroPath + "/" + "tmpStream" + 0 + ".wav");
                        Application.Run(
                            new StreamPlayer(
                                null,
                                MainWindow.NitroPath + "/" + "tmpStream" + 0 + ".wav",
                                Path.GetFileNameWithoutExtension(args[0])
                            )
                        );
                        break;
                }
            }
            else
            {
                Application.Run(new MainWindow());
            }
        }
    }
}
