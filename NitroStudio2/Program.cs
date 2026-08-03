using Avalonia;
using System;

namespace NitroStudio2
{
    internal static class Program
    {
        /// <summary>
        /// Directory the app was launched from. Replaces WinForms' Application.StartupPath;
        /// GotaSequenceLib resolves the Hardware/*.wav PSG samples relative to this.
        /// </summary>
        public static string NitroPath => AppContext.BaseDirectory.TrimEnd(
            System.IO.Path.DirectorySeparatorChar
        );

        [STAThread]
        private static void Main(string[] args) =>
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
    }
}
