using GotaSequenceLib;
using GotaSoundIO.IO;
using NitroFileLoader;
using NitroStudio2.Services;
using NitroStudio2.ViewModels.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Shared behaviour of the two text-based editors: the sequence editor and the sequence
    /// archive editor. Both show the assembly text on the right, a preview bank plus the
    /// 16-track mute/solo strip on the left, and drive the same player.
    /// </summary>
    public abstract class SequenceTextEditorViewModelBase : EditorViewModelBase, IDisposable
    {
        private readonly SequencePlayback playback = new();

        protected SequenceTextEditorViewModelBase(
            IDialogService dialogs,
            Type fileType,
            string extensionDescription,
            string extension,
            string editorName,
            SoundArchive archive
        )
            : base(dialogs, fileType, extensionDescription, extension, editorName)
        {
            Archive = archive;
            Playback = playback;
            SoundPlayerPanel = new SoundPlayerPanelViewModel(playback) { PlayRequested = Play };

            // The text editors have no tree; the assembly text fills the right-hand pane.
            RightPane = RightPaneMode.SequenceEditor;
            ShowSoundPlayer = true;

            for (int i = 0; i < 16; i++)
            {
                TrackViewModel track = SeqBankPanel.Tracks[i];
                track.EnabledChanged = TrackEnabledChanged;
                track.SoloRequested = TrackSolo;
            }
            SeqBankPanel.ExportMidiRequested = () => _ = ExportMidiAsync();
            SeqBankPanel.ExportWavRequested = () => _ = ExportWavAsync();
            SeqBankPanel.BankComboEdited = BankComboChanged;
            SeqBankPanel.BankIdEdited = BankIdChanged;

            playback.Player.NotePressed += (_, e) => SetTrackState(e.TrackId, "NoteDown");
            playback.Player.NoteReleased += (_, e) => SetTrackState(e.TrackId, "Idle");
            playback.Volume = 75;

            ActivePanel = SeqBankPanel;
            PopulateBankOptions();
        }

        /// <summary>The archive this file came from, or null when opened standalone.</summary>
        protected SoundArchive Archive { get; }

        /// <summary>The transport this editor drives. Public so the view and tests can observe it.</summary>
        public SequencePlayback Playback { get; }

        public SequenceBankPanelViewModel SeqBankPanel { get; } = new();

        /// <summary>Assembly text shown in the editor; the window keeps this in step.</summary>
        public string SequenceText { get; set; } = "";

        /// <summary>Raised when the view model replaces the text, so the view can reload it.</summary>
        public event EventHandler TextReplaced;

        protected void SetText(string text)
        {
            SequenceText = text;
            TextReplaced?.Invoke(this, EventArgs.Empty);
        }

        public override void DoInfoStuff() { }

        public override void UpdateNodes() { }

        // ------------------------------------------------------------------ preview bank

        private void PopulateBankOptions()
        {
            if (Archive is null)
            {
                // Nothing to preview against, so the bank picker is inert. The pane itself stays:
                // the track mutes, the solo buttons and the two export commands all still work,
                // and the original's Init() showed seqBankPanel whether or not it had a parent.
                SeqBankPanel.BankSelectionEnabled = false;
                return;
            }
            SeqBankPanel.WritingInfo = true;
            SeqBankPanel.BankOptions.Clear();
            SeqBankPanel.BankOptions.Add("Other Index");
            foreach (BankInfo b in Archive.Banks)
            {
                SeqBankPanel.BankOptions.Add("[" + b.Index + "] - " + b.Name);
            }
            SeqBankPanel.Bank = SeqBankPanel.BankOptions.Count > 1
                ? SeqBankPanel.BankOptions[1]
                : SeqBankPanel.BankOptions[0];
            SeqBankPanel.BankId = SeqBankPanel.BankOptions.Count > 1 ? BankIdOf(SeqBankPanel.Bank) : 0;
            SeqBankPanel.WritingInfo = false;
        }

        private static int BankIdOf(string entry)
        {
            return entry is null or "Other Index"
                ? 0
                : int.Parse(entry.Split('[')[1].Split(']')[0]);
        }

        private void BankComboChanged()
        {
            SeqBankPanel.WritingInfo = true;
            SeqBankPanel.BankId = BankIdOf(SeqBankPanel.Bank);
            SeqBankPanel.WritingInfo = false;
        }

        private void BankIdChanged()
        {
            BankInfo b = Archive?.Banks.FirstOrDefault(x => x.Index == (int)SeqBankPanel.BankId);
            SeqBankPanel.WritingInfo = true;
            SeqBankPanel.Bank = b is null ? "Other Index" : "[" + b.Index + "] - " + b.Name;
            SeqBankPanel.WritingInfo = false;
        }

        /// <summary>The bank chosen for previewing, or null when it does not exist.</summary>
        protected virtual BankInfo ResolvePreviewBank()
        {
            return Archive?.Banks.FirstOrDefault(x => x.Index == (int)SeqBankPanel.BankId);
        }

        /// <summary>
        /// Points the preview at a specific bank. A sequence opened out of an archive should
        /// preview against the bank the archive assigns it, not against whichever bank happens to
        /// be first; the WinForms editor did this with a SetBankIndex call at the open site.
        /// </summary>
        public void SelectPreviewBank(uint bankId)
        {
            if (Archive is null)
            {
                return;
            }
            BankInfo b = Archive.Banks.FirstOrDefault(x => x.Index == bankId);
            SeqBankPanel.WritingInfo = true;
            SeqBankPanel.BankId = bankId;
            SeqBankPanel.Bank = b is null ? "Other Index" : "[" + b.Index + "] - " + b.Name;
            SeqBankPanel.WritingInfo = false;
        }

        // ------------------------------------------------------------------ tracks

        private void TrackEnabledChanged(int index)
        {
            TrackViewModel track = SeqBankPanel.Tracks[index];
            track.State = track.IsEnabled ? "Idle" : "Mute";
            playback.Mixer.Mutes[index] = !track.IsEnabled;
        }

        /// <summary>
        /// Solo toggles: if this track is the only one enabled, re-enable everything; otherwise
        /// enable only this one. Port of the sixteen Track*Solo handlers.
        /// </summary>
        private void TrackSolo(int index)
        {
            bool isolated =
                SeqBankPanel.Tracks[index].IsEnabled
                && !SeqBankPanel.Tracks.Where((_, i) => i != index).Any(t => t.IsEnabled);
            for (int i = 0; i < 16; i++)
            {
                SeqBankPanel.Tracks[i].IsEnabled = isolated || i == index;
            }
        }

        private void SetTrackState(int index, string state)
        {
            if (index >= 0 && index < 16 && SeqBankPanel.Tracks[index].IsEnabled)
            {
                SeqBankPanel.Tracks[index].State = state;
            }
        }

        private void ResetTrackStates()
        {
            foreach (TrackViewModel track in SeqBankPanel.Tracks)
            {
                track.State = track.IsEnabled ? "Idle" : "Mute";
            }
        }

        // ------------------------------------------------------------------ playback

        /// <summary>Parses the editor text back into commands. False when it does not compile.</summary>
        protected abstract bool CompileText();

        /// <summary>Commands and start offset the player should use for the current selection.</summary>
        protected abstract (List<SequenceCommand> Commands, int Start) SongToPlay();

        private void Play()
        {
            _ = PlayAsync();
        }

        /// <summary>Space previews the sequence, matching the transport's Play button.</summary>
        public override void PlaySelected()
        {
            Play();
        }

        private async Task PlayAsync()
        {
            if (!CompileText())
            {
                return;
            }
            if (Archive is null)
            {
                await Dialogs.ShowMessageAsync(
                    "There must be an SDAT connected to this file to play it."
                );
                return;
            }
            BankInfo b = ResolvePreviewBank();
            if (b is null)
            {
                await Dialogs.ShowMessageAsync("Bank is not valid or doesn't exist.");
                return;
            }
            (List<SequenceCommand> commands, int start) = SongToPlay();
            if (commands is null)
            {
                return;
            }
            _ = playback.LoadSong(b.File, b.GetAssociatedWaves(), commands, start);
            playback.Play();
        }

        /// <summary>Stopping clears the note lights, as the WinForms StopClick did.</summary>
        public void Stop()
        {
            playback.Stop();
            ResetTrackStates();
        }

        // ------------------------------------------------------------------ export

        private async Task ExportMidiAsync()
        {
            if (!CompileText())
            {
                return;
            }
            string path = await Dialogs.SaveFileAsync("MIDI|*.mid", SongName() + ".mid");
            if (path != "")
            {
                SaveMidi(path);
            }
        }

        private async Task ExportWavAsync()
        {
            if (!CompileText())
            {
                return;
            }
            if (Archive is null)
            {
                await Dialogs.ShowMessageAsync(
                    "There must be an SDAT connected to this file to record it."
                );
                return;
            }
            BankInfo b = ResolvePreviewBank();
            if (b is null)
            {
                await Dialogs.ShowMessageAsync("Bank is not valid or doesn't exist.");
                return;
            }
            string path = await Dialogs.SaveFileAsync("Wave File|*.wav", SongName() + ".wav");
            if (path == "")
            {
                return;
            }
            (List<SequenceCommand> commands, int start) = SongToPlay();
            if (commands is null)
            {
                return;
            }
            SequenceRecorderViewModel recorder =
                new([b.File], b.GetAssociatedWaves(), commands, start, path);
            await (ShowRecorderRequested?.Invoke(recorder) ?? Task.CompletedTask);
        }

        /// <summary>Set by the host so exporting to WAV can show the recorder dialog.</summary>
        public Func<SequenceRecorderViewModel, Task> ShowRecorderRequested { get; set; }

        protected abstract string SongName();

        protected abstract void SaveMidi(string path);

        public override void OnClosing()
        {
            Dispose();
            base.OnClosing();
        }

        public void Dispose()
        {
            playback.Dispose();
        }
    }

    /// <summary>Editor for a single .sseq sequence. Ported from the WinForms SequenceEditor.</summary>
    public sealed class SequenceEditorViewModel : SequenceTextEditorViewModelBase
    {
        public SequenceEditorViewModel(IDialogService dialogs, SoundArchive archive = null)
            : base(dialogs, typeof(Sequence), "Sequence", "seq", "Sequence Editor", archive)
        {
            Status = "Editing A Sequence.";
        }

        public Sequence SEQ => File as Sequence;

        /// <summary>Opens a sequence that lives inside a sound archive.</summary>
        public void LoadEmbedded(IOFile file, string fileName)
        {
            ExtFile = file;
            File = (IOFile)Activator.CreateInstance(file.GetType());
            File.Read(file.Write());
            FilePath = "";
            FileName = fileName;
            FileOpen = true;
            Title = EditorName + " - " + (fileName ?? "{ Null File Name }") + ".sseq";
            LoadSequenceText(fileName ?? "Sequence");
        }

        /// <summary>Reads the sequence's commands and shows them as assembly text.</summary>
        public void LoadSequenceText(string name = "Sequence")
        {
            if (File is null)
            {
                SetText("{ NULL FILE INFO }");
                return;
            }
            SEQ.ReadCommandData();
            SEQ.Name = name;
            SetText(string.Join("\n", SEQ.ToText()));
        }

        protected override bool CompileText()
        {
            try
            {
                SEQ.FromText([.. SequenceText.Replace('\r', '\n').Split('\n')]);
            }
            catch (Exception e)
            {
                _ = Dialogs.ShowMessageAsync(e.Message);
                return false;
            }
            return SEQ.WritingCommandSuccess;
        }

        protected override (List<SequenceCommand>, int) SongToPlay()
        {
            return (SEQ.Commands, 0);
        }

        protected override string SongName()
        {
            return System.IO.Path.GetFileNameWithoutExtension(SEQ.Name ?? "Sequence");
        }

        protected override void SaveMidi(string path)
        {
            SEQ.SaveMIDI(path);
        }

        public override async Task NewAsync()
        {
            await base.NewAsync();
            if (FileOpen)
            {
                LoadSequenceText("New Sequence");
            }
        }

        public override async Task OpenAsync()
        {
            await base.OpenAsync();
            if (FileOpen)
            {
                LoadSequenceText(FileName);
            }
        }

        public override void OpenFile(string path)
        {
            base.OpenFile(path);
            LoadSequenceText(FileName);
        }

        public override async Task SaveAsync()
        {
            if (!CompileText())
            {
                return;
            }
            SEQ.WriteCommandData();
            await base.SaveAsync();
        }

        public override async Task BlankFileAsync()
        {
            if (!await FileTestAsync(false, true))
            {
                return;
            }
            string name = SEQ.Name;
            File = (IOFile)Activator.CreateInstance(FileType);
            SEQ.RawData = [];
            LoadSequenceText(name);
        }

        public override async Task ImportFileAsync()
        {
            if (!await FileTestAsync(false, true))
            {
                return;
            }
            string path = await Dialogs.OpenFileAsync(
                "Supported Sound Files|*.sseq;*.smft|Sound Sequence|*.sseq|SMF Text Format|*.smft"
            );
            if (path == "")
            {
                return;
            }
            if (path.EndsWith(".sseq"))
            {
                string name = SEQ.Name;
                File = (IOFile)Activator.CreateInstance(FileType);
                SEQ.Name = name;
                File.Read(path);
                LoadSequenceText(name);
            }
            else
            {
                SetText(System.IO.File.ReadAllText(path));
            }
        }

        public override async Task ExportFileAsync()
        {
            if (!CompileText())
            {
                return;
            }
            SEQ.WriteCommandData();
            string path = await Dialogs.SaveFileAsync(
                "Supported Sound Files|*.sseq;*.smft|Sound Sequence|*.sseq|SMF Text Format|*.smft",
                SongName() + ".sseq"
            );
            if (path == "")
            {
                return;
            }
            if (path.EndsWith(".smft"))
            {
                System.IO.File.WriteAllText(path, SequenceText);
            }
            else
            {
                SEQ.Write(path);
            }
        }
    }
}
