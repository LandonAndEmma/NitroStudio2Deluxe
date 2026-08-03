using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;
using NitroStudio2.Services;
using NitroStudio2.ViewModels;
using System;

namespace NitroStudio2.Views
{
    /// <summary>
    /// The window every editor shares, ported from the WinForms EditorBase form: menu bar, the
    /// info panel and tree either side of a splitter, and the status bar. Which panels appear and
    /// what the tree contains comes entirely from the bound <see cref="EditorViewModelBase"/>.
    /// </summary>
    public partial class EditorWindow : Window
    {
        public EditorWindow()
        {
            InitializeComponent();
            Icon = new WindowIcon(Assets.WindowIcon("App"));
            HookPositionBar();
            HookPlayKey();
        }

        /// <summary>
        /// Binds a view model to this window. Separate from the constructor because the view
        /// model needs a dialog service that is parented to the window.
        /// </summary>
        public void Attach(EditorViewModelBase viewModel)
        {
            DataContext = viewModel;
            viewModel.CloseRequested += (_, _) => Close();
            Piano.Pressed += (_, _) => viewModel.OnPianoPress(Piano.NoteDown);
            Piano.Released += (_, _) => viewModel.OnPianoRelease();

            if (viewModel is SoundArchiveViewModel archive)
            {
                // Dialogs the archive editor needs but should not construct itself.
                archive.ShowRecorderRequested = vm =>
                    new SequenceRecorderWindow(vm).ShowDialog(this);
                archive.ShowInstrumentSelectorRequested = vm =>
                    new InstrumentSelectorWindow(vm).ShowDialog(this);
                archive.ShowWaveMapperRequested = vm =>
                    new WaveMapperWindow(vm).ShowDialog(this);
                OpenBankGeneratorRequested = () =>
                {
                    if (archive.SA is null)
                    {
                        return;
                    }
                    BankGeneratorViewModel vm = new(
                        archive.SA,
                        new DialogService(this),
                        () =>
                        {
                            archive.UpdateNodes();
                            archive.DoInfoStuff();
                        }
                    );
                    new BankGeneratorWindow(vm).Show(this);
                };
                ExportSdkProjectRequested = () => _ = archive.ExportSdkProjectAsync();

                // Tools menu and tree double-click open the per-type editors. The dialog service
                // has to be built against the new window, so the view model cannot be constructed
                // until that window exists: passing null here left every File menu command in a
                // Tools-launched editor throwing on the first await.
                // archive.SA is read when the menu is used, not now, so an editor opened after a
                // file is loaded still gets it. The WinForms editors took the MainWindow itself
                // for this reason; without it the preview bank pickers have nothing to list and
                // playing anything reports that no SDAT is connected.
                OpenSequenceEditorRequested = () =>
                    OpenEditor(dialogs => new SequenceEditorViewModel(dialogs, archive.SA));
                OpenSequenceArchiveEditorRequested = () =>
                    OpenEditor(dialogs => new SequenceArchiveEditorViewModel(dialogs, archive.SA));
                OpenBankEditorRequested = () =>
                    OpenEditor(dialogs => new BankEditorViewModel(dialogs, archive.SA));
                OpenWaveArchiveEditorRequested = () =>
                    OpenEditor(dialogs => new WaveArchiveEditorViewModel(dialogs, archive.SA));
                archive.OpenEntryEditorRequested = entry => OpenEntry(archive, entry);
                archive.OpenSequenceArchiveRequested = info => OpenEntry(archive, info);
            }

            if (viewModel is SequenceTextEditorViewModelBase text)
            {
                text.ShowRecorderRequested = vm =>
                    new SequenceRecorderWindow(vm).ShowDialog(this);
                // The editor owns the text; the control mirrors it both ways.
                SequenceEditor.Text = text.SequenceText;
                text.TextReplaced += (_, _) => SequenceEditor.Text = text.SequenceText;
                SequenceEditor.TextChanged += (_, _) => text.SequenceText = SequenceEditor.Text;
            }

            if (viewModel is BankEditorViewModel bank)
            {
                bank.ShowWaveMapperRequested = vm => new WaveMapperWindow(vm).ShowDialog(this);
                bank.ColorRegionRequested = (color, start, end) =>
                    Piano.ColorRegion(color, start, end);
                bank.ResetPianoColorsRequested = Piano.ResetColors;
            }
        }

        /// <summary>Opens another editor in its own window, wired to a dialog service of its own.</summary>
        private void OpenEditor(Func<IDialogService, EditorViewModelBase> build)
        {
            EditorWindow window = new();
            EditorViewModelBase viewModel = build(new DialogService(window));
            window.Attach(viewModel);
            // Nothing has loaded a file, so no panel has been chosen yet; without this the left
            // pane comes up empty instead of saying there is nothing selected.
            viewModel.DoInfoStuff();
            window.Show();
        }

        /// <summary>Opens the editor matching an archive entry, loaded with that entry's file.</summary>
        private void OpenEntry(SoundArchiveViewModel archive, object entry)
        {
            EditorWindow window = new();
            DialogService dialogs = new(window);
            switch (entry)
            {
                case NitroFileLoader.SequenceInfo s:
                {
                    SequenceEditorViewModel vm = new(dialogs, archive.SA);
                    window.Attach(vm);
                    vm.LoadEmbedded(s.File, s.Name);
                    // Preview against the bank the archive gives this sequence. ReadingBankId is
                    // the fallback for an id that pointed at no entry when the archive was read.
                    vm.SelectPreviewBank(s.Bank is null ? s.ReadingBankId : (uint)s.Bank.Index);
                    break;
                }
                case NitroFileLoader.SequenceArchiveInfo s:
                {
                    SequenceArchiveEditorViewModel vm = new(dialogs, archive.SA);
                    window.Attach(vm);
                    vm.LoadEmbedded(s.File, s.Name);
                    break;
                }
                case NitroFileLoader.BankInfo b:
                {
                    BankEditorViewModel vm = new(dialogs, archive.SA);
                    window.Attach(vm);
                    vm.LoadEmbedded(b.File, b.Name, b);
                    break;
                }
                case NitroFileLoader.WaveArchiveInfo w:
                {
                    WaveArchiveEditorViewModel vm = new(dialogs, archive.SA);
                    window.Attach(vm);
                    vm.LoadEmbedded(w.File, w.Name);
                    break;
                }
                default:
                    return;
            }
            window.Show();
        }

        private EditorViewModelBase ViewModel => DataContext as EditorViewModelBase;

        private void OnTreeDoubleTapped(object sender, RoutedEventArgs e) =>
            ViewModel?.NodeMouseDoubleClick();

        /// <summary>
        /// Dragging the position bar must not fight the 30 fps tick, the way the WinForms
        /// PositionBarFree flag worked.
        ///
        /// These are tunnelling handlers registered on the window rather than events on the
        /// Slider itself, because Slider's thumb marks PointerPressed handled before it would
        /// reach a handler on the Slider — so the drag would never be noticed.
        /// </summary>
        private void HookPositionBar()
        {
            AddHandler(
                PointerPressedEvent,
                (_, e) =>
                {
                    if (IsPositionBar(e.Source) && ViewModel?.SoundPlayerPanel is not null)
                    {
                        ViewModel.SoundPlayerPanel.Playback.PositionBarFree = false;
                    }
                },
                RoutingStrategies.Tunnel,
                handledEventsToo: true
            );
            AddHandler(
                PointerReleasedEvent,
                (_, e) =>
                {
                    if (IsPositionBar(e.Source))
                    {
                        ViewModel?.SoundPlayerPanel?.Playback.SeekToPosition();
                    }
                },
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
                handledEventsToo: true
            );
        }

        /// <summary>
        /// Space previews the current selection, as it did in WinForms. This tunnels because
        /// TreeViewItem handles Space itself for selection, and it is ignored while a text box
        /// or the sequence editor has focus so typing a space still types a space.
        /// </summary>
        private void HookPlayKey()
        {
            AddHandler(
                KeyDownEvent,
                (_, e) =>
                {
                    if (e.Key != Key.Space || ViewModel is null)
                    {
                        return;
                    }
                    if (
                        e.Source is Visual visual
                        && visual.GetSelfAndVisualAncestors()
                            .Any(v => v is TextBox or Controls.SequenceTextEditor)
                    )
                    {
                        return;
                    }
                    ViewModel.PlaySelected();
                    e.Handled = true;
                },
                RoutingStrategies.Tunnel
            );
        }

        private static bool IsPositionBar(object source) =>
            source is Visual visual
            && visual.GetSelfAndVisualAncestors()
                .OfType<Slider>()
                .Any(s => (s.Tag as string) == "Position");

        /// <summary>
        /// Credits, previously read from Assets/About/About.txt by a dedicated window. Inlined
        /// here so the text ships with the code that shows it rather than as a loose resource.
        /// </summary>
        private const string AboutText =
            "Nitro Studio 2 Deluxe:\n"
            + "An editor for SDAT.\n"
            + "\n"
            + "Credits:\n"
            + "Nintendo: Images, SDAT Info.\n"
            + "Kermalis: Sequence Player Base\n"
            + "Eugene: Testing, Suggestions.\n"
            + "Goji Goodra: Testing, Suggestions.\n"
            + "Josh: SDAT Research.\n"
            + "Crystal: SDAT Research.\n"
            + "Nintendon: SDAT Research.\n"
            + "DJ Bouche: SDAT Research.\n"
            + "VGMTrans: SDAT Research.\n"
            + "LoveEmu: SDAT Research, Tools.\n"
            + "Gota7: Nitro Studio.\n"
            + "\n"
            + "©2026 Gota7, Lonk, & NitroShell";

        private async void OnAbout(object sender, RoutedEventArgs e) =>
            await new DialogService(this).ShowMessageAsync(AboutText, "About Nitro Studio 2");

        private void OnCreateWave(object sender, RoutedEventArgs e) =>
            new CreateStreamToolWindow(true).Show();

        private void OnCreateStream(object sender, RoutedEventArgs e) =>
            new CreateStreamToolWindow(false).Show();

        // The remaining Tools entries open editors that are wired up with the sound archive
        // window; each concrete editor supplies the handler through these hooks.
        public Action OpenSequenceEditorRequested { get; set; }

        public Action OpenSequenceArchiveEditorRequested { get; set; }

        public Action OpenBankEditorRequested { get; set; }

        public Action OpenWaveArchiveEditorRequested { get; set; }

        public Action OpenBankGeneratorRequested { get; set; }

        public Action ExportSdkProjectRequested { get; set; }

        private void OnOpenSequenceEditor(object sender, RoutedEventArgs e) =>
            OpenSequenceEditorRequested?.Invoke();

        private void OnOpenSequenceArchiveEditor(object sender, RoutedEventArgs e) =>
            OpenSequenceArchiveEditorRequested?.Invoke();

        private void OnOpenBankEditor(object sender, RoutedEventArgs e) =>
            OpenBankEditorRequested?.Invoke();

        private void OnOpenWaveArchiveEditor(object sender, RoutedEventArgs e) =>
            OpenWaveArchiveEditorRequested?.Invoke();

        private void OnOpenBankGenerator(object sender, RoutedEventArgs e) =>
            OpenBankGeneratorRequested?.Invoke();

        private void OnExportSdkProject(object sender, RoutedEventArgs e) =>
            ExportSdkProjectRequested?.Invoke();

        protected override void OnClosed(EventArgs e)
        {
            ViewModel?.OnClosing();
            base.OnClosed(e);
        }
    }
}
