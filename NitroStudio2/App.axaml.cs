using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NitroStudio2.Services;

namespace NitroStudio2
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = StartupRouter.CreateStartupWindow(desktop.Args ?? []);
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
