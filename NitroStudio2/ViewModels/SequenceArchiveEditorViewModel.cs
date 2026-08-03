using GotaSequenceLib;
using GotaSoundIO.IO;
using NitroFileLoader;
using NitroStudio2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Editor for a .ssar sequence archive. Ported from the WinForms SequenceArchiveEditor: the
    /// same assembly text and track strip as the sequence editor, plus a picker for which of the
    /// archive's sequences to preview.
    /// </summary>
    public sealed class SequenceArchiveEditorViewModel : SequenceTextEditorViewModelBase
    {
        public SequenceArchiveEditorViewModel(IDialogService dialogs, SoundArchive archive = null)
            : base(
                dialogs,
                typeof(SequenceArchive),
                "Sequence Archive",
                "sar",
                "Sequence Archive Editor",
                archive
            )
        {
            Status = "Editing A Sequence Archive.";
            ShowSeqArcSeqPanel = true;
            SeqArcSeqPanel.SequenceComboEdited = SequenceComboChanged;
            SeqArcSeqPanel.SequenceIdEdited = SequenceIdChanged;
        }

        public SequenceArchive SAR => File as SequenceArchive;

        /// <summary>Opens a sequence archive that lives inside a sound archive.</summary>
        public void LoadEmbedded(IOFile file, string fileName)
        {
            ExtFile = file;
            File = (IOFile)Activator.CreateInstance(file.GetType());
            File.Read(file.Write());
            FilePath = "";
            FileName = fileName;
            FileOpen = true;
            Title = EditorName + " - " + (fileName ?? "{ Null File Name }") + ".ssar";
            LoadSequenceText(fileName ?? "Sequence");
        }

        public void LoadSequenceText(string name = "Sequence")
        {
            if (File is null)
            {
                SetText("{ NULL FILE INFO }");
                return;
            }
            SAR.ReadCommandData(true);
            SAR.Name = name;
            SetText(string.Join("\n", SAR.ToText()));
            PopulateSequenceOptions();
        }

        // ------------------------------------------------------------------ preview sequence

        /// <summary>
        /// The archive's sequence names, falling back to "Sequence_N" for unnamed entries.
        /// Port of GetSequenceNames.
        /// </summary>
        private List<(string Name, int Index)> SequenceNames()
        {
            List<(string, int)> names = [];
            int num = 0;
            foreach (SequenceArchiveSequence s in SAR?.Sequences ?? [])
            {
                string name = s.Name;
                int index = s.Index;
                if (string.IsNullOrEmpty(name))
                {
                    index = num;
                    name = "Sequence_" + index;
                }
                names.Add((name, index));
                num++;
            }
            return names;
        }

        private void PopulateSequenceOptions()
        {
            List<(string Name, int Index)> names = SequenceNames();
            string previous = SeqArcSeqPanel.Sequence?[(SeqArcSeqPanel.Sequence.IndexOf(' ') + 1)..];

            SeqArcSeqPanel.WritingInfo = true;
            SeqArcSeqPanel.SequenceOptions.Clear();
            foreach ((string name, int index) in names)
            {
                SeqArcSeqPanel.SequenceOptions.Add("[" + index + "] " + name);
            }
            // Keep the same sequence selected across a reload where possible.
            string match = SeqArcSeqPanel.SequenceOptions.FirstOrDefault(o =>
                o[(o.IndexOf(' ') + 1)..] == previous
            );
            SeqArcSeqPanel.Sequence = match ?? SeqArcSeqPanel.SequenceOptions.FirstOrDefault();
            SeqArcSeqPanel.SequenceId = SeqArcSeqPanel.Sequence is null
                ? 0
                : int.Parse(SeqArcSeqPanel.Sequence.Split('[')[1].Split(']')[0]);
            SeqArcSeqPanel.WritingInfo = false;
            FollowPreviewSequenceBank();
        }

        private void SequenceComboChanged()
        {
            if (SeqArcSeqPanel.Sequence is null)
            {
                return;
            }
            SeqArcSeqPanel.WritingInfo = true;
            SeqArcSeqPanel.SequenceId =
                int.Parse(SeqArcSeqPanel.Sequence.Split('[')[1].Split(']')[0]);
            SeqArcSeqPanel.WritingInfo = false;
            FollowPreviewSequenceBank();
        }

        /// <summary>
        /// Points the bank picker at whichever bank the chosen preview sequence uses. Each
        /// sequence in the archive carries its own bank, so leaving the picker where it was
        /// showed a bank that had nothing to do with what pressing Play would sound.
        /// </summary>
        private void FollowPreviewSequenceBank()
        {
            SequenceArchiveSequence s = SAR?.Sequences.FirstOrDefault(x =>
                x.Index == (int)SeqArcSeqPanel.SequenceId
            );
            if (s is null)
            {
                return;
            }
            SelectPreviewBank(s.Bank is null ? s.ReadingBankId : (uint)s.Bank.Index);
        }

        private void SequenceIdChanged()
        {
            string match = SeqArcSeqPanel.SequenceOptions.FirstOrDefault(o =>
                o.StartsWith("[" + (int)SeqArcSeqPanel.SequenceId + "] ", StringComparison.Ordinal)
            );
            if (match is null)
            {
                return;
            }
            SeqArcSeqPanel.WritingInfo = true;
            SeqArcSeqPanel.Sequence = match;
            SeqArcSeqPanel.WritingInfo = false;
            FollowPreviewSequenceBank();
        }

        // ------------------------------------------------------------------ compile and play

        protected override bool CompileText()
        {
            try
            {
                SAR.FromText([.. SequenceText.Replace('\r', '\n').Split('\n')], Archive);
            }
            catch (Exception e)
            {
                _ = Dialogs.ShowMessageAsync(e.Message);
                return false;
            }
            if (SAR.WritingCommandSuccess)
            {
                PopulateSequenceOptions();
            }
            return SAR.WritingCommandSuccess;
        }

        /// <summary>The selected sequence's commands, starting at its label offset.</summary>
        protected override (List<SequenceCommand>, int) SongToPlay()
        {
            SequenceArchiveSequence s = SAR.Sequences.FirstOrDefault(x =>
                x.Index == (int)SeqArcSeqPanel.SequenceId
            );
            if (s is null)
            {
                _ = Dialogs.ShowMessageAsync("The given preview sequence does not exist.");
                return (null, 0);
            }
            if (!SAR.PublicLabels.ContainsKey(s.Name ?? ""))
            {
                return (SAR.Commands, 0);
            }
            return (SAR.Commands, SAR.PublicLabels[s.Name]);
        }

        /// <summary>
        /// Falls back to the preview sequence's own bank when the bank picker is left at -1,
        /// matching the WinForms PlayClick.
        /// </summary>
        protected override BankInfo ResolvePreviewBank()
        {
            if (SeqBankPanel.BankId != -1)
            {
                return base.ResolvePreviewBank();
            }
            SequenceArchiveSequence s = SAR.Sequences.FirstOrDefault(x =>
                x.Index == (int)SeqArcSeqPanel.SequenceId
            );
            return s?.Bank ?? Archive?.Banks.FirstOrDefault(x => x.Index == s?.ReadingBankId);
        }

        protected override string SongName()
        {
            return System.IO.Path.GetFileNameWithoutExtension(SAR.Name ?? "Sequence Archive");
        }

        protected override void SaveMidi(string path)
        {
            SAR.SaveMIDI(path);
        }

        public override async Task NewAsync()
        {
            await base.NewAsync();
            if (FileOpen)
            {
                LoadSequenceText("New Sequence Archive");
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
            SAR.WriteCommandData();
            await base.SaveAsync();
        }

        public override async Task ImportFileAsync()
        {
            if (!await FileTestAsync(false, true))
            {
                return;
            }
            string path = await Dialogs.OpenFileAsync(
                "Supported Sound Files|*.ssar;*.mus|Sound Sequence Archive|*.ssar|Music List|*.mus"
            );
            if (path == "")
            {
                return;
            }
            string name = SAR.Name;
            if (path.EndsWith(".ssar"))
            {
                File = (IOFile)Activator.CreateInstance(FileType);
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
            SAR.WriteCommandData();
            string path = await Dialogs.SaveFileAsync(
                "Supported Sound Files|*.ssar;*.mus|Sound Sequence Archive|*.ssar|Music List|*.mus",
                SongName() + ".ssar"
            );
            if (path == "")
            {
                return;
            }
            if (path.EndsWith(".mus"))
            {
                System.IO.File.WriteAllLines(path, SAR.ToText());
            }
            else
            {
                SAR.Write(path);
            }
        }
    }
}
