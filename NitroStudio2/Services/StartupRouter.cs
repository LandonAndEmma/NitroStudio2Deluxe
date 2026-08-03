using Avalonia.Controls;
using NitroStudio2.ViewModels;
using NitroStudio2.Views;
using System.IO;

namespace NitroStudio2.Services
{
    /// <summary>
    /// Chooses the window to open at startup from the file the app was launched with, the way
    /// the WinForms Program.Main switched on the argument's extension.
    /// </summary>
    public static class StartupRouter
    {
        /// <summary>Path passed on the command line, or null when launched with no arguments.</summary>
        public static string StartupFile { get; private set; }

        public static Window CreateStartupWindow(string[] args)
        {
            StartupFile = args.Length > 0 ? args[0] : null;

            if (StartupFile is not null)
            {
                switch (Path.GetExtension(StartupFile))
                {
                    case ".sseq":
                        return OpenEditor(new SequenceEditorFactory(), StartupFile);
                    case ".ssar":
                        return OpenEditor(new SequenceArchiveEditorFactory(), StartupFile);
                    case ".sbnk":
                        return OpenEditor(new BankEditorFactory(), StartupFile);
                    case ".swar":
                        return OpenEditor(new WaveArchiveEditorFactory(), StartupFile);
                }
            }

            EditorWindow window = new();
            SoundArchiveViewModel viewModel = new(new DialogService(window));
            window.Attach(viewModel);
            if (StartupFile is not null && Path.GetExtension(StartupFile) == ".sdat")
            {
                viewModel.OpenStartupFile(StartupFile);
            }
            return window;
        }

        // Each editor is built the same way: make the window, give the view model a dialog
        // service parented to it, then open the file.

        private interface IEditorFactory
        {
            EditorViewModelBase Create(IDialogService dialogs);
        }

        private sealed class SequenceEditorFactory : IEditorFactory
        {
            public EditorViewModelBase Create(IDialogService dialogs) => new SequenceEditorViewModel(dialogs);
        }

        private sealed class SequenceArchiveEditorFactory : IEditorFactory
        {
            public EditorViewModelBase Create(IDialogService dialogs) =>
                new SequenceArchiveEditorViewModel(dialogs);
        }

        private sealed class BankEditorFactory : IEditorFactory
        {
            public EditorViewModelBase Create(IDialogService dialogs) => new BankEditorViewModel(dialogs);
        }

        private sealed class WaveArchiveEditorFactory : IEditorFactory
        {
            public EditorViewModelBase Create(IDialogService dialogs) =>
                new WaveArchiveEditorViewModel(dialogs);
        }

        private static Window OpenEditor(IEditorFactory factory, string path)
        {
            EditorWindow window = new();
            EditorViewModelBase viewModel = factory.Create(new DialogService(window));
            window.Attach(viewModel);
            viewModel.OpenFile(path);
            return window;
        }
    }
}
