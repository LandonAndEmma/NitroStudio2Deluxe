using Avalonia.Media;
using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.IO;
using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using NitroFileLoader.Instrument;
using NitroStudio2.Models;
using NitroStudio2.ViewModels;
using NitroStudio2.Services;
using NitroStudio2.ViewModels.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstrumentType = NitroFileLoader.Instrument.InstrumentType;
using NitroInstrument = NitroFileLoader.Instrument.Instrument;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Editor for a .sbnk instrument bank. Ported from the WinForms BankEditor: a tree of
    /// instruments, a region grid, and the on-screen piano for previewing notes.
    /// </summary>
    public sealed class BankEditorViewModel : EditorViewModelBase, IDisposable
    {
        private readonly Mixer mixer = new();
        private readonly Player player;
        private readonly Random random = new();

        /// <summary>The archive this bank belongs to, when opened from the archive editor.</summary>
        private readonly SoundArchive archive;

        public BankEditorViewModel(IDialogService dialogs, SoundArchive archive = null)
            : base(dialogs, typeof(Bank), "Bank", "bnk", "Bank Editor")
        {
            this.archive = archive;
            player = new Player(mixer);

            Nodes.Add(new EditorTreeNode("root", "Bank", 11));
            IndexPanel.Maximum = 32767;
            BankEditorPanel.InstrumentTypeEdited = InstrumentTypeChanged;
            BankEditorPanel.DrumSetRangeComboEdited = DrumSetRangeComboChanged;
            BankEditorPanel.DrumSetRangeIdEdited = DrumSetRangeIdChanged;
            IndexPanel.SwapRequested = SwapAtIndex;

            // The four slots are always shown, as the WinForms editor's bankEditorWars panel was.
            // Standalone there is no archive to name wave archives after, so the lists hold only
            // the two placeholders, but the ids stay visible and editable.
            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                WaveArchivePanel.Slots[slot].ComboEdited = () => WaveArchiveComboChanged(slot);
                WaveArchivePanel.Slots[slot].IdEdited = () => WaveArchiveIdChanged(slot);
            }
            PopulateWaveArchiveSlots();

            // Only an archive can supply the samples to preview with.
            if (archive is not null)
            {
                ShowPiano = true;
                LoadWaveArchives();
            }
            UpdateNodes();
            DoInfoStuff();
        }

        public Bank BK => File as Bank;

        public BankEditorPanelViewModel BankEditorPanel { get; } = new();

        /// <summary>The four wave archive rows shown over the tree, when inside an archive.</summary>
        public BankEditorWarsViewModel WaveArchivePanel { get; } = new();

        /// <summary>Colours the piano to show the selected instrument's regions.</summary>
        public Action<Color, byte, byte> ColorRegionRequested { get; set; }

        public Action ResetPianoColorsRequested { get; set; }

        /// <summary>The archive entry this bank came from, which carries its wave archive ids.</summary>
        private BankInfo bankInfo;

        /// <summary>Opens a bank that lives inside a sound archive.</summary>
        public void LoadEmbedded(IOFile file, string fileName, BankInfo info = null)
        {
            bankInfo = info;
            ExtFile = file;
            File = (IOFile)Activator.CreateInstance(file.GetType());
            File.Read(file.Write());
            FilePath = "";
            FileName = fileName;
            FileOpen = true;
            Title = EditorName + " - " + (fileName ?? "{ Null File Name }") + ".sbnk";
            ApplyBankWaveArchives();
            // The preview player needs the bank, which only exists now that the file is loaded.
            LoadWaveArchives();
            UpdateNodes();
            DoInfoStuff();
        }

        public override void OpenFile(string path)
        {
            base.OpenFile(path);
            LoadWaveArchives();
        }

        // ------------------------------------------------------------------ tree

        /// <summary>Tree label and icon for an instrument, by its type.</summary>
        private static (string Label, int Icon) Describe(NitroInstrument e) =>
            e.Type() switch
            {
                InstrumentType.PCM => ("[" + e.Index + "] PCM Instrument", 14),
                InstrumentType.PSG => ("[" + e.Index + "] PSG Instrument", 17),
                InstrumentType.Noise => ("[" + e.Index + "] Noise Instrument", 18),
                InstrumentType.DirectPCM => ("[" + e.Index + "] Direct PCM Instrument", 14),
                InstrumentType.Null => ("[" + e.Index + "] Null Instrument", 0),
                InstrumentType.DrumSet => ("[" + e.Index + "] Drum Set", 15),
                _ => ("[" + e.Index + "] Key-Split", 16),
            };

        public override void UpdateNodes()
        {
            BeginUpdateNodes();
            if (FileOpen && File is not null)
            {
                Nodes[0].ContextActions = [new MenuAction("Add", "New", RootAdd)];
                foreach (NitroInstrument e in BK.Instruments)
                {
                    (string label, int icon) = Describe(e);
                    Nodes[0].Add("inst" + e.Index, label, icon).ContextActions =
                    [
                        new MenuAction("Add Above", "New", NodeAddAbove),
                        new MenuAction("Add Below", "Open", NodeAddBelow),
                        new MenuAction("Replace", null, NodeReplace),
                        new MenuAction("Export", "Export", NodeExport),
                        new MenuAction("Delete", "Close", NodeDelete),
                    ];
                }
                Nodes[0].IsExpanded = true;
            }
            else
            {
                foreach (EditorTreeNode node in Nodes)
                {
                    node.ContextActions = null;
                }
            }
            EndUpdateNodes();
        }

        private NitroInstrument SelectedInstrument() =>
            SelectedNode?.Parent is null
                ? null
                : BK.Instruments.FirstOrDefault(x =>
                    x.Index == SoundArchiveViewModel.IdFromNode(SelectedNode)
                );

        // ------------------------------------------------------------------ info panel

        public override void DoInfoStuff()
        {
            if (!FileOpen || File is null || SelectedNode?.Parent is null)
            {
                ShowIndexPanel = false;
                // Selecting the bank root shows its wave archives, which is where they belong:
                // they are a property of the bank, not of any one instrument. This does not
                // depend on an archive; without one the slots simply have no names to offer.
                bool onRoot = FileOpen && File is not null;
                ActivePanel = onRoot ? WaveArchivePanel : NoInfoPanel;
                Status = onRoot ? "Editing Bank Wave Archives." : "No Valid Info Selected!";
                return;
            }

            WritingInfo = true;
            BankEditorPanel.WritingInfo = true;
            try
            {
                NitroInstrument e = SelectedInstrument();
                ShowIndexPanel = true;
                IndexPanel.ItemIndex = e.Index;
                PopulateRegionGrid(e);
                ColorNotes(e is DrumSetInstrument drum ? drum.Min : (byte)0, e.NoteInfo);

                BankEditorPanel.InstrumentType = e.Type() switch
                {
                    InstrumentType.DrumSet => "Drum Set",
                    InstrumentType.KeySplit => "Key Split",
                    _ => "Direct",
                };
                byte min = e is DrumSetInstrument d ? d.Min : (byte)0;
                BankEditorPanel.DrumSetStartId = min;
                BankEditorPanel.DrumSetStartNote = NoteNames.All[min];

                // A direct instrument holds one region; a key split holds at most eight.
                BankEditorPanel.CanBeDirect = e.NoteInfo.Count <= 1;
                BankEditorPanel.CanBeKeySplit = e.NoteInfo.Count <= 8;

                ActivePanel = BankEditorPanel;
                Status = "Editing " + SelectedNode.Text + ".";
            }
            finally
            {
                BankEditorPanel.WritingInfo = false;
                WritingInfo = false;
            }
        }

        private void PopulateRegionGrid(NitroInstrument instrument)
        {
            BankEditorPanel.Regions.Clear();
            foreach (NoteInfo e in instrument.NoteInfo)
            {
                BankRegionRow row = new()
                {
                    EndNote = NoteNames.All[(int)e.Key],
                    InstrumentType = e.InstrumentType switch
                    {
                        InstrumentType.PCM => "PCM",
                        InstrumentType.PSG => "PSG",
                        InstrumentType.Noise => "Noise",
                        InstrumentType.DirectPCM => "Direct PCM",
                        _ => "Null",
                    },
                    WaveId = e.WaveId.ToString(),
                    WaveArchiveId = e.WarId.ToString(),
                    BaseNote = NoteNames.All[Math.Min(e.BaseNote, (byte)(NoteNames.All.Count - 1))],
                    Attack = e.Attack.ToString(),
                    Decay = e.Decay.ToString(),
                    Sustain = e.Sustain.ToString(),
                    Release = e.Release.ToString(),
                    Pan = e.Pan.ToString(),
                };
                row.Edited = _ => RegionsChanged();
                row.PlayRequested = PlayRegion;
                row.CanPlay = archive is not null;
                BankEditorPanel.Regions.Add(row);
            }
            AddRegionPlaceholder();
        }

        /// <summary>
        /// Appends the blank trailing row used to add a region. Avalonia's DataGrid has no
        /// new-row line, so the placeholder stands in for the one the WinForms grid provided;
        /// editing it promotes it and starts a fresh one.
        /// </summary>
        private void AddRegionPlaceholder()
        {
            BankRegionRow placeholder = new()
            {
                CanPlay = false,
                Edited = _ => RegionsChanged(),
            };
            BankEditorPanel.Regions.Add(placeholder);
        }

        /// <summary>
        /// Paints each region's key range on the piano. The first gets white, the rest a random
        /// light colour, so adjacent regions stay distinguishable. Port of ColorNotes.
        /// </summary>
        private void ColorNotes(byte start, List<NoteInfo> notes)
        {
            int num = 0;
            foreach (NoteInfo e in notes)
            {
                Color color =
                    num == 0
                        ? Colors.White
                        : Color.FromRgb(
                            (byte)random.Next(75, 256),
                            (byte)random.Next(75, 256),
                            (byte)random.Next(75, 256)
                        );
                ColorRegionRequested?.Invoke(color, start, (byte)e.Key);
                start = (byte)(e.Key + 1);
                num++;
            }
        }

        // ------------------------------------------------------------------ region edits

        /// <summary>
        /// Rebuilds the instrument's regions from the grid, clamping out-of-range values and
        /// promoting the instrument's type when it outgrows direct or key-split. Port of
        /// RegionsChanged.
        /// </summary>
        private void RegionsChanged()
        {
            if (WritingInfo)
            {
                return;
            }
            bool labelChanged = false;
            WritingInfo = true;
            try
            {
                NitroInstrument inst = SelectedInstrument();
                if (inst is null)
                {
                    return;
                }
                // Only the instrument's label in the tree depends on these, so the tree is only
                // rebuilt if one of them actually moved. Rebuilding on every keystroke replaced
                // the selected node with a fresh instance, which dropped the tree selection and
                // reset the grid's scroll position mid-edit.
                InstrumentType typeBefore = inst.Type();
                int countBefore = inst.NoteInfo.Count;

                // The last row is the blank placeholder; WinForms skipped its new-row line the
                // same way, with "for (i = 1; i < Rows.Count; i++)" over Rows[i - 1].
                bool placeholderUsed =
                    BankEditorPanel.Regions.Count > 0 && !BankEditorPanel.Regions[^1].IsBlank;

                List<NoteInfo> regions = [];
                foreach (BankRegionRow row in BankEditorPanel.Regions.SkipLast(placeholderUsed ? 0 : 1))
                {
                    row.EndNote ??= NoteNames.All[127];
                    row.InstrumentType ??= "PCM";
                    row.BaseNote ??= NoteNames.All[60];
                    regions.Add(
                        new NoteInfo
                        {
                            Key = (Notes)NoteNames.All.ToList().IndexOf(row.EndNote),
                            InstrumentType = row.InstrumentType switch
                            {
                                "PSG" => InstrumentType.PSG,
                                "Noise" => InstrumentType.Noise,
                                "Direct PCM" => InstrumentType.DirectPCM,
                                "Null" => InstrumentType.Null,
                                _ => InstrumentType.PCM,
                            },
                            WaveId = (ushort)Clamp(row.WaveId, 0, ushort.MaxValue),
                            WarId = (ushort)Clamp(row.WaveArchiveId, 0, ushort.MaxValue),
                            BaseNote = (byte)NoteNames.All.ToList().IndexOf(row.BaseNote),
                            Attack = (byte)Clamp(row.Attack, 127, 127),
                            Decay = (byte)Clamp(row.Decay, 127, 127),
                            Sustain = (byte)Clamp(row.Sustain, 127, 127),
                            Release = (byte)Clamp(row.Release, 127, 127),
                            Pan = (byte)Clamp(row.Pan, 64, 127),
                        }
                    );
                }

                inst.NoteInfo = regions;
                if (inst.NoteInfo.Count < 1)
                {
                    // An instrument always keeps at least one region.
                    inst.NoteInfo.Add(
                        new NoteInfo
                        {
                            Attack = 127,
                            BaseNote = 60,
                            Decay = 127,
                            InstrumentType = InstrumentType.PCM,
                            Key = Notes.gn9,
                            Pan = 64,
                            Release = 127,
                            Sustain = 127,
                        }
                    );
                    WritingInfo = false;
                    UpdateNodes();
                    DoInfoStuff();
                    return;
                }

                BankEditorPanel.CanBeDirect = regions.Count <= 1;
                BankEditorPanel.CanBeKeySplit = regions.Count <= 8;

                if (regions.Count > 8 && inst.Type() != InstrumentType.DrumSet)
                {
                    ReplaceInstrument(inst, new DrumSetInstrument { Min = 0 });
                    BankEditorPanel.WritingInfo = true;
                    BankEditorPanel.InstrumentType = "Drum Set";
                    BankEditorPanel.DrumSetStartId = 0;
                    BankEditorPanel.DrumSetStartNote = NoteNames.All[0];
                    BankEditorPanel.WritingInfo = false;
                    WritingInfo = false;
                    UpdateNodes();
                    return;
                }
                if (regions.Count > 1 && inst is DirectInstrument)
                {
                    ReplaceInstrument(inst, new KeySplitInstrument());
                    BankEditorPanel.WritingInfo = true;
                    BankEditorPanel.InstrumentType = "Key Split";
                    BankEditorPanel.WritingInfo = false;
                }
                ColorNotes(inst is DrumSetInstrument dr ? dr.Min : (byte)0, inst.NoteInfo);

                if (placeholderUsed)
                {
                    AddRegionPlaceholder();
                }
                labelChanged =
                    SelectedInstrument()?.Type() != typeBefore
                    || inst.NoteInfo.Count != countBefore;
            }
            finally
            {
                WritingInfo = false;
            }
            if (labelChanged)
            {
                UpdateNodes();
            }
        }

        /// <summary>Parses a grid cell, falling back to a default and capping at a maximum.</summary>
        private static int Clamp(string text, int fallback, int maximum) =>
            !int.TryParse(text, out int value) ? fallback : Math.Min(Math.Max(value, 0), maximum);

        /// <summary>Swaps an instrument for one of another type, keeping index, order and regions.</summary>
        private void ReplaceInstrument(NitroInstrument existing, NitroInstrument replacement)
        {
            replacement.Index = existing.Index;
            replacement.Order = existing.Order;
            replacement.NoteInfo = existing.NoteInfo;
            BK.Instruments[BK.Instruments.IndexOf(existing)] = replacement;
        }

        private void InstrumentTypeChanged()
        {
            NitroInstrument inst = SelectedInstrument();
            if (inst is null)
            {
                return;
            }
            ReplaceInstrument(
                inst,
                BankEditorPanel.InstrumentType switch
                {
                    "Drum Set" => new DrumSetInstrument
                    {
                        Min = (byte)BankEditorPanel.DrumSetStartId,
                    },
                    "Key Split" => new KeySplitInstrument(),
                    _ => new DirectInstrument(),
                }
            );
            UpdateNodes();
            DoInfoStuff();
        }

        private void DrumSetRangeIdChanged()
        {
            if (WritingInfo)
            {
                return;
            }
            WritingInfo = true;
            BankEditorPanel.WritingInfo = true;
            BankEditorPanel.DrumSetStartNote = NoteNames.All[(int)BankEditorPanel.DrumSetStartId];
            if (SelectedInstrument() is DrumSetInstrument drum)
            {
                drum.Min = (byte)BankEditorPanel.DrumSetStartId;
            }
            BankEditorPanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void DrumSetRangeComboChanged()
        {
            if (WritingInfo)
            {
                return;
            }
            WritingInfo = true;
            BankEditorPanel.WritingInfo = true;
            int index = NoteNames.All.ToList().IndexOf(BankEditorPanel.DrumSetStartNote);
            BankEditorPanel.DrumSetStartId = index;
            if (SelectedInstrument() is DrumSetInstrument drum)
            {
                drum.Min = (byte)index;
            }
            BankEditorPanel.WritingInfo = false;
            WritingInfo = false;
        }

        // ------------------------------------------------------------------ wave archives

        private void PopulateWaveArchiveSlots()
        {
            WaveArchivePanel.WritingInfo = true;
            foreach (WaveArchiveSlotViewModel slot in WaveArchivePanel.Slots)
            {
                slot.Options.Clear();
                slot.Options.Add("FFFF - Blank");
                slot.Options.Add("Other Index");
                foreach (WaveArchiveInfo w in archive?.WaveArchives ?? [])
                {
                    slot.Options.Add("[" + w.Index + "] - " + w.Name);
                }
                slot.Selected = slot.Options[0];
                slot.Id = -1;
            }
            WaveArchivePanel.WritingInfo = false;
        }

        /// <summary>
        /// Fills the four slots from the bank's own settings. Without this the editor always
        /// opened showing "FFFF - Blank", so a bank's samples could not be previewed and saving
        /// would have wiped its wave archive references.
        /// </summary>
        private void ApplyBankWaveArchives()
        {
            if (archive is null || bankInfo is null)
            {
                return;
            }
            ushort[] ids =
            [
                bankInfo.WaveArchives[0] is null
                    ? bankInfo.ReadingWave0Id
                    : (ushort)bankInfo.WaveArchives[0].Index,
                bankInfo.WaveArchives[1] is null
                    ? bankInfo.ReadingWave1Id
                    : (ushort)bankInfo.WaveArchives[1].Index,
                bankInfo.WaveArchives[2] is null
                    ? bankInfo.ReadingWave2Id
                    : (ushort)bankInfo.WaveArchives[2].Index,
                bankInfo.WaveArchives[3] is null
                    ? bankInfo.ReadingWave3Id
                    : (ushort)bankInfo.WaveArchives[3].Index,
            ];
            WaveArchivePanel.WritingInfo = true;
            for (int i = 0; i < 4; i++)
            {
                WaveArchiveInfo w = archive.WaveArchives.FirstOrDefault(x => x.Index == ids[i]);
                WaveArchivePanel.Slots[i].Selected = w is null
                    ? (ids[i] == 0xFFFF ? "FFFF - Blank" : "Other Index")
                    : "[" + w.Index + "] - " + w.Name;
                WaveArchivePanel.Slots[i].Id = ids[i] == 0xFFFF ? -1 : ids[i];
            }
            WaveArchivePanel.WritingInfo = false;
        }

        private void WaveArchiveComboChanged(int slot)
        {
            WaveArchiveSlotViewModel vm = WaveArchivePanel.Slots[slot];
            if (vm.Selected == "Other Index")
            {
                return;
            }
            int id =
                vm.Selected == "FFFF - Blank"
                    ? -1
                    : int.Parse(vm.Selected.Split('[')[1].Split(']')[0]);
            WaveArchivePanel.WritingInfo = true;
            vm.Id = id;
            WaveArchivePanel.WritingInfo = false;
            LoadWaveArchives();
        }

        private void WaveArchiveIdChanged(int slot)
        {
            WaveArchiveSlotViewModel vm = WaveArchivePanel.Slots[slot];
            WaveArchiveInfo w = archive?.WaveArchives.FirstOrDefault(x => x.Index == vm.Id);
            WaveArchivePanel.WritingInfo = true;
            vm.Selected = w is null
                ? (vm.Id < 0 ? "FFFF - Blank" : "Other Index")
                : "[" + w.Index + "] - " + w.Name;
            WaveArchivePanel.WritingInfo = false;
            LoadWaveArchives();
        }

        /// <summary>Points the preview player at the four chosen wave archives.</summary>
        private void LoadWaveArchives()
        {
            if (archive is null || BK is null)
            {
                return;
            }
            RiffWave[][] riffs = new RiffWave[4][];
            for (int i = 0; i < 4; i++)
            {
                WaveArchiveInfo w = archive.WaveArchives.FirstOrDefault(x =>
                    x.Index == (int)WaveArchivePanel.Slots[i].Id
                );
                if (w is not null)
                {
                    riffs[i] = w.File.GetWaves();
                }
            }
            player.PrepareForSong([BK], riffs);
        }

        // ------------------------------------------------------------------ preview

        /// <summary>Plays the selected instrument at a given note until released.</summary>
        private void PlayNote(Notes note, ushort length)
        {
            NitroInstrument inst = SelectedInstrument();
            if (inst is null || archive is null)
            {
                return;
            }
            if (player.Banks is null || player.Banks.Length < 1)
            {
                // PrepareForSong has not run, so there is nothing to play through yet.
                return;
            }
            player.Stop();
            player.Banks[0] = BK;
            player.LoadSong(
                [
                    new SequenceCommand
                    {
                        CommandType = SequenceCommands.ProgramChange,
                        Parameter = (uint)inst.Index,
                    },
                    new SequenceCommand
                    {
                        CommandType = SequenceCommands.Note,
                        Parameter = new NoteParameter
                        {
                            Note = note,
                            Length = length,
                            Velocity = 127,
                        },
                    },
                    new SequenceCommand { CommandType = SequenceCommands.Fin },
                ]
            );
            player.Play();
        }

        public override void OnPianoPress(Notes note)
        {
            if (SelectedNode?.Parent is null)
            {
                return;
            }
            CurrentNote = "Playing Note " + note + " (" + (int)note + ").";
            PlayNote(note, 0xFFF);
        }

        public override void OnPianoRelease()
        {
            player.Stop();
            CurrentNote = "";
        }

        /// <summary>
        /// The note a region should be previewed at: its base note, which is the pitch the
        /// sample was actually recorded at, so it sounds as intended rather than resampled.
        ///
        /// NoteInfo.Key is the *end* of the region's key range, almost always gn9, so previewing
        /// with it made nearly every instrument shriek. The base note is clamped into the
        /// region's own range because the player picks a region by note: a base note outside the
        /// range would quietly preview a neighbouring region instead of this one.
        /// </summary>
        public Notes PreviewNoteFor(int regionIndex)
        {
            NitroInstrument inst = SelectedInstrument();
            if (inst is null || regionIndex < 0 || regionIndex >= inst.NoteInfo.Count)
            {
                return Notes.cn4;
            }
            NoteInfo region = inst.NoteInfo[regionIndex];
            int low = regionIndex > 0
                ? (byte)inst.NoteInfo[regionIndex - 1].Key + 1
                : inst is DrumSetInstrument drum ? drum.Min : 0;
            int high = (byte)region.Key;
            if (low > high)
            {
                low = high;
            }
            return (Notes)(byte)Math.Clamp((int)region.BaseNote, low, high);
        }

        /// <summary>
        /// Space previews the selected instrument. A drum set or key split holds several
        /// regions; this plays the first, which is the one the grid shows at the top.
        /// </summary>
        public override void PlaySelected()
        {
            NitroInstrument inst = SelectedInstrument();
            if (inst is not null && inst.NoteInfo.Count > 0)
            {
                PlayNote(PreviewNoteFor(0), 48 * 2);
            }
        }

        private void PlayRegion(BankRegionRow row)
        {
            int index = BankEditorPanel.Regions.IndexOf(row);
            NitroInstrument inst = SelectedInstrument();
            if (inst is null || index < 0 || index >= inst.NoteInfo.Count)
            {
                return;
            }
            PlayNote(PreviewNoteFor(index), 48 * 2);
        }

        // ------------------------------------------------------------------ node actions

        public override void RootAdd() => _ = RootAddAsync();

        private async Task RootAddAsync()
        {
            int index = 0;
            try
            {
                index = BK.Instruments.Last().Index + 1;
            }
            catch { }
            if (index > 0xFFFF)
            {
                for (int i = 0; i < 0xFFFF; i++)
                {
                    if (!BK.Instruments.Any(x => x.Index == i))
                    {
                        index = i;
                        break;
                    }
                }
                await Dialogs.ShowMessageAsync("No available slots left!");
            }
            BK.Instruments.Add(
                new DirectInstrument
                {
                    Index = index,
                    NoteInfo = [new NoteInfo { Key = Notes.gn9 }],
                    Order = index,
                }
            );
            BK.Instruments = [.. BK.Instruments.OrderBy(x => x.Index)];
            UpdateNodes();
            DoInfoStuff();
        }

        public override void NodeAddAbove() => AddRelative(above: true);

        public override void NodeAddBelow() => AddRelative(above: false);

        /// <summary>
        /// Inserts a blank instrument next to the selected one, shifting the indices of anything
        /// in the way. Port of NodeAddAbove/NodeAddBelow.
        /// </summary>
        private void AddRelative(bool above)
        {
            NitroInstrument inst = SelectedInstrument();
            if (inst is null)
            {
                return;
            }
            int target = above ? inst.Index : inst.Index + 1;
            if (BK.Instruments.Any(x => x.Index == target) || (above && inst.Index == 0))
            {
                foreach (NitroInstrument i in BK.Instruments)
                {
                    if (i.Index >= target && (above ? i != inst : true))
                    {
                        i.Index++;
                    }
                }
                if (above)
                {
                    inst.Index++;
                }
            }
            BK.Instruments.Add(
                new DirectInstrument
                {
                    Index = target,
                    NoteInfo = [new NoteInfo { Key = Notes.gn9 }],
                    Order = target,
                }
            );
            BK.Instruments = [.. BK.Instruments.OrderBy(x => x.Index)];
            UpdateNodes();
            SelectByIndex(target);
        }

        public override void NodeReplace() => _ = ReplaceInstrumentFileAsync();

        public override void NodeExport() => _ = ExportInstrumentAsync();

        public override void NodeDelete()
        {
            NitroInstrument inst = SelectedInstrument();
            if (inst is not null)
            {
                BK.Instruments.Remove(inst);
            }
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>The four chosen wave archive ids, with -1 mapped back to 0xFFFF.</summary>
        private ushort WarId(int slot) =>
            WaveArchivePanel.Slots[slot].Id == -1
                ? (ushort)0xFFFF
                : (ushort)WaveArchivePanel.Slots[slot].Id;

        private async Task ReplaceInstrumentFileAsync()
        {
            string path = await Dialogs.OpenFileAsync("Nitro Studio Instrument|*.ns2i;*.nist");
            if (path == "")
            {
                return;
            }
            NitroInstrument inst = SelectedInstrument();
            switch (System.IO.Path.GetExtension(path))
            {
                case ".ns2i":
                {
                    // Carries its own samples, so it may need a home archive for each of them.
                    NitroStudio2Instrument file = new();
                    file.Read(path);
                    await file.WriteInstrument(
                        BK,
                        inst.Index,
                        archive,
                        WarId(0),
                        WarId(1),
                        WarId(2),
                        WarId(3),
                        MapWaveAsync
                    );
                    LoadWaveArchives();
                    break;
                }
                case ".nist":
                {
                    NitroStudioInstrument file = new();
                    file.Read(path);
                    if (file.Inst is null)
                    {
                        await Dialogs.ShowMessageAsync("An empty instrument cannot be used!");
                        return;
                    }
                    file.Inst.Index = inst.Index;
                    BK.Instruments[BK.Instruments.IndexOf(inst)] = file.Inst;
                    break;
                }
            }
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>Asks the user which archive an imported sample should live in.</summary>
        private async Task<ushort?> MapWaveAsync(RiffWave wave, List<WaveArchiveInfo> archives)
        {
            if (ShowWaveMapperRequested is null)
            {
                return null;
            }
            WaveMapperViewModel mapper = new([wave], archives, true);
            await ShowWaveMapperRequested(mapper);
            return mapper.WarMap is null ? null : mapper.WarMap[0];
        }

        /// <summary>Set by the host so the wave mapper dialog can be shown from here.</summary>
        public Func<WaveMapperViewModel, Task> ShowWaveMapperRequested { get; set; }

        private async Task ExportInstrumentAsync()
        {
            NitroInstrument inst = SelectedInstrument();
            if (inst is null)
            {
                return;
            }
            string path = await Dialogs.SaveFileAsync(
                "Nitro Studio Instrument|*.ns2i;*.nist",
                "Instrument " + inst.Index + ".ns2i"
            );
            if (path == "")
            {
                return;
            }
            switch (System.IO.Path.GetExtension(path))
            {
                case ".ns2i":
                    new NitroStudio2Instrument(
                        inst,
                        archive,
                        WarId(0),
                        WarId(1),
                        WarId(2),
                        WarId(3)
                    ).Write(path);
                    break;
                case ".nist":
                    new NitroStudioInstrument { Inst = inst }.Write(path);
                    break;
            }
        }

        private void SwapAtIndex()
        {
            NitroInstrument inst = SelectedInstrument();
            if (inst is null)
            {
                return;
            }
            int target = (int)IndexPanel.ItemIndex;
            NitroInstrument occupant = BK.Instruments.FirstOrDefault(x => x.Index == target);
            if (occupant is not null)
            {
                occupant.Index = inst.Index;
            }
            inst.Index = target;
            BK.Instruments = [.. BK.Instruments.OrderBy(x => x.Index)];
            UpdateNodes();
            SelectByIndex(target);
        }

        private void SelectByIndex(int index)
        {
            EditorTreeNode node = Nodes[0]
                .Nodes.FirstOrDefault(n => n.Text.Contains("[" + index + "]"));
            if (node is not null)
            {
                SelectedNode = node;
            }
            DoInfoStuff();
        }

        public override void OnClosing()
        {
            Dispose();
            base.OnClosing();
        }

        public void Dispose()
        {
            try
            {
                player.Dispose();
                mixer.Dispose();
            }
            catch { }
        }
    }
}
