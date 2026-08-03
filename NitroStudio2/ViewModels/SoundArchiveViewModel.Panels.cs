using NitroFileLoader;
using NitroStudio2.ViewModels.Panels;
using System.Collections.Generic;
using System.Linq;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Filling the info panels from the archive, and writing edits back. Straight port of the
    /// corresponding blocks of MainWindow.DoInfoStuff and its change handlers, including the
    /// "FFFF - Blank" / "Other Index" entries the combo boxes carried.
    /// </summary>
    public sealed partial class SoundArchiveViewModel
    {
        private const string BlankEntry = "FFFF - Blank";
        private const string OtherIndexEntry = "Other Index";

        // ------------------------------------------------------------------ combo helpers

        private void PopulateWaveArchiveOptions(IList<string> options)
        {
            options.Clear();
            options.Add(BlankEntry);
            options.Add(OtherIndexEntry);
            foreach (WaveArchiveInfo w in SA.WaveArchives)
            {
                options.Add("[" + w.Index + "] - " + w.Name);
            }
        }

        private string WaveArchiveEntry(ushort id)
        {
            WaveArchiveInfo w = SA.WaveArchives.FirstOrDefault(x => x.Index == id);
            return w is null
                ? (id == 0xFFFF ? BlankEntry : OtherIndexEntry)
                : "[" + w.Index + "] - " + w.Name;
        }

        private void PopulateBankOptions(IList<string> options)
        {
            options.Clear();
            options.Add(OtherIndexEntry);
            foreach (BankInfo b in SA.Banks)
            {
                options.Add("[" + b.Index + "] - " + b.Name);
            }
        }

        private string BankEntry(uint id)
        {
            BankInfo b = SA.Banks.FirstOrDefault(x => x.Index == id);
            return b is null ? OtherIndexEntry : "[" + b.Index + "] - " + b.Name;
        }

        private void PopulatePlayerOptions(IList<string> options)
        {
            options.Clear();
            options.Add(OtherIndexEntry);
            foreach (PlayerInfo p in SA.Players)
            {
                options.Add("[" + p.Index + "] - " + p.Name);
            }
        }

        private string PlayerEntry(byte id)
        {
            PlayerInfo p = SA.Players.FirstOrDefault(x => x.Index == id);
            return p is null ? OtherIndexEntry : "[" + p.Index + "] - " + p.Name;
        }

        private void PopulateStreamPlayerOptions(IList<string> options)
        {
            options.Clear();
            options.Add(OtherIndexEntry);
            foreach (StreamPlayerInfo p in SA.StreamPlayers)
            {
                options.Add("[" + p.Index + "] - " + p.Name);
            }
        }

        private string StreamPlayerEntry(byte id)
        {
            StreamPlayerInfo p = SA.StreamPlayers.FirstOrDefault(x => x.Index == id);
            return p is null ? OtherIndexEntry : "[" + p.Index + "] - " + p.Name;
        }

        /// <summary>Id inside an "[3] - NAME" entry, or null for the blank / other-index rows.</summary>
        private static int? EntryId(string entry)
        {
            return entry is null or BlankEntry or OtherIndexEntry
                ? null
                : int.Parse(entry.Split('[')[1].Split(']')[0]);
        }

        // ------------------------------------------------------------------ populate

        private void ShowSequence()
        {
            SequenceInfo e = (SequenceInfo)SelectedEntry();
            SequencePanel.WritingInfo = true;
            PopulateBankOptions(SequencePanel.BankOptions);
            ushort bankId = e.Bank is null ? e.ReadingBankId : (ushort)e.Bank.Index;
            SequencePanel.Bank = BankEntry(bankId);
            SequencePanel.BankId = bankId;
            SequencePanel.Volume = e.Volume > 127 ? 127 : e.Volume;
            SequencePanel.ChannelPriority = e.ChannelPriority;
            SequencePanel.PlayerPriority = e.PlayerPriority;
            PopulatePlayerOptions(SequencePanel.PlayerOptions);
            byte playerId = e.Player is null ? e.ReadingPlayerId : (byte)e.Player.Index;
            SequencePanel.Player = PlayerEntry(playerId);
            SequencePanel.PlayerId = playerId;
            SequencePanel.WritingInfo = false;
            ActivePanel = SequencePanel;
            SetStatus(e.Index, e.Name, e.File);
        }

        private void ShowSequenceArchive()
        {
            SequenceArchiveInfo e = (SequenceArchiveInfo)SelectedEntry();
            ShowSoundPlayer = false;
            ActivePanel = SequenceArchivePanel;
            SetStatus(e.Index, e.Name, e.File);
        }

        private void ShowBank()
        {
            BankInfo e = (BankInfo)SelectedEntry();
            ShowSoundPlayer = false;
            BankPanel.WritingInfo = true;
            ushort[] ids =
            [
                e.WaveArchives[0] is null ? e.ReadingWave0Id : (ushort)e.WaveArchives[0].Index,
                e.WaveArchives[1] is null ? e.ReadingWave1Id : (ushort)e.WaveArchives[1].Index,
                e.WaveArchives[2] is null ? e.ReadingWave2Id : (ushort)e.WaveArchives[2].Index,
                e.WaveArchives[3] is null ? e.ReadingWave3Id : (ushort)e.WaveArchives[3].Index,
            ];
            for (int i = 0; i < 4; i++)
            {
                PopulateWaveArchiveOptions(BankPanel.Slots[i].Options);
                BankPanel.Slots[i].Selected = WaveArchiveEntry(ids[i]);
                BankPanel.Slots[i].Id = ids[i] == 0xFFFF ? -1 : ids[i];
            }
            BankPanel.WritingInfo = false;
            ActivePanel = BankPanel;
            SetStatus(e.Index, e.Name, e.File);
        }

        private void ShowWaveArchive()
        {
            WaveArchiveInfo e = (WaveArchiveInfo)SelectedEntry();
            ShowSoundPlayer = false;
            WaveArchivePanel.WritingInfo = true;
            WaveArchivePanel.LoadIndividually = e.LoadIndividually;
            WaveArchivePanel.WritingInfo = false;
            ActivePanel = WaveArchivePanel;
            SetStatus(e.Index, e.Name, e.File);
        }

        private void ShowPlayer()
        {
            PlayerInfo e = (PlayerInfo)SelectedEntry();
            ShowSoundPlayer = false;
            PlayerPanel.WritingInfo = true;
            PlayerPanel.MaxSequences = e.SequenceMax;
            PlayerPanel.HeapSize = e.HeapSize;
            for (int i = 0; i < 16; i++)
            {
                PlayerPanel.ChannelFlags[i].IsSet = e.ChannelFlags[i];
            }
            PlayerPanel.WritingInfo = false;
            ActivePanel = PlayerPanel;
            SetStatus(e.Index, e.Name, null);
        }

        private void ShowStreamPlayer()
        {
            StreamPlayerInfo e = (StreamPlayerInfo)SelectedEntry();
            ShowSoundPlayer = false;
            StreamPlayerPanel.WritingInfo = true;
            StreamPlayerPanel.ChannelType = e.IsStereo ? 1 : 0;
            StreamPlayerPanel.LeftChannel = e.LeftChannel;
            StreamPlayerPanel.RightChannel = e.IsStereo ? e.RightChannel : 0;
            StreamPlayerPanel.WritingInfo = false;
            ActivePanel = StreamPlayerPanel;
            SetStatus(e.Index, e.Name, null);
        }

        private void ShowStream()
        {
            StreamInfo e = (StreamInfo)SelectedEntry();
            StreamPanel.WritingInfo = true;
            StreamPanel.MonoToStereo = e.MonoToStereo;
            StreamPanel.Volume = e.Volume;
            StreamPanel.Priority = e.Priority;
            PopulateStreamPlayerOptions(StreamPanel.PlayerOptions);
            byte playerId = e.Player is null ? e.ReadingPlayerId : (byte)e.Player.Index;
            StreamPanel.Player = StreamPlayerEntry(playerId);
            StreamPanel.PlayerId = playerId;
            StreamPanel.WritingInfo = false;
            ActivePanel = StreamPanel;
            SetStatus(e.Index, e.Name, e.File);
        }

        /// <summary>
        /// Previews the selected stream. The .strm is decoded straight into memory by the same
        /// StreamPlayer the wave archive editor uses, so there is no WAV written to disk and no
        /// separate window: the transport at the top of the pane drives it, loop points included.
        /// </summary>
        private void PlayStream()
        {
            if (SelectedEntry() is not StreamInfo { File: not null } e)
            {
                return;
            }
            playback.Stop();
            streamPlayback.LoadWave(e.File);
            streamPlayback.Play();
        }

        // ------------------------------------------------------------------ write-back

        private void SequenceBankComboChanged()
        {
            int? id = EntryId(SequencePanel.Bank);
            if (id is null)
            {
                return;
            }
            SequenceInfo e = (SequenceInfo)SelectedEntry();
            e.Bank = SA.Banks.FirstOrDefault(x => x.Index == id);
            e.ReadingBankId = (ushort)id;
            WritingInfo = true;
            SequencePanel.WritingInfo = true;
            SequencePanel.BankId = id.Value;
            SequencePanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void SequenceBankIdChanged()
        {
            ushort id = (ushort)SequencePanel.BankId;
            SequenceInfo e = (SequenceInfo)SelectedEntry();
            e.Bank = SA.Banks.FirstOrDefault(x => x.Index == id);
            e.ReadingBankId = id;
            WritingInfo = true;
            SequencePanel.WritingInfo = true;
            SequencePanel.Bank = BankEntry(id);
            SequencePanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void SequencePlayerComboChanged()
        {
            int? id = EntryId(SequencePanel.Player);
            if (id is null)
            {
                return;
            }
            SequenceInfo e = (SequenceInfo)SelectedEntry();
            e.Player = SA.Players.FirstOrDefault(x => x.Index == id);
            e.ReadingPlayerId = (byte)id;
            WritingInfo = true;
            SequencePanel.WritingInfo = true;
            SequencePanel.PlayerId = id.Value;
            SequencePanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void SequencePlayerIdChanged()
        {
            byte id = (byte)SequencePanel.PlayerId;
            SequenceInfo e = (SequenceInfo)SelectedEntry();
            e.Player = SA.Players.FirstOrDefault(x => x.Index == id);
            e.ReadingPlayerId = id;
            WritingInfo = true;
            SequencePanel.WritingInfo = true;
            SequencePanel.Player = PlayerEntry(id);
            SequencePanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void StreamPlayerComboChanged()
        {
            int? id = EntryId(StreamPanel.Player);
            if (id is null)
            {
                return;
            }
            StreamInfo e = (StreamInfo)SelectedEntry();
            e.Player = SA.StreamPlayers.FirstOrDefault(x => x.Index == id);
            e.ReadingPlayerId = (byte)id;
            WritingInfo = true;
            StreamPanel.WritingInfo = true;
            StreamPanel.PlayerId = id.Value;
            StreamPanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void StreamPlayerIdChanged()
        {
            byte id = (byte)StreamPanel.PlayerId;
            StreamInfo e = (StreamInfo)SelectedEntry();
            e.Player = SA.StreamPlayers.FirstOrDefault(x => x.Index == id);
            e.ReadingPlayerId = id;
            WritingInfo = true;
            StreamPanel.WritingInfo = true;
            StreamPanel.Player = StreamPlayerEntry(id);
            StreamPanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void BankWaveArchiveComboChanged(int slot)
        {
            WaveArchiveSlotViewModel vm = BankPanel.Slots[slot];
            if (vm.Selected == OtherIndexEntry)
            {
                return;
            }
            ushort id = vm.Selected == BlankEntry ? (ushort)0xFFFF : (ushort)EntryId(vm.Selected);
            SetBankWaveArchive(slot, id);
            WritingInfo = true;
            BankPanel.WritingInfo = true;
            vm.Id = id == 0xFFFF ? -1 : id;
            BankPanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void BankWaveArchiveIdChanged(int slot)
        {
            WaveArchiveSlotViewModel vm = BankPanel.Slots[slot];
            ushort id = vm.Id < 0 ? (ushort)0xFFFF : (ushort)vm.Id;
            SetBankWaveArchive(slot, id);
            WritingInfo = true;
            BankPanel.WritingInfo = true;
            vm.Selected = WaveArchiveEntry(id);
            BankPanel.WritingInfo = false;
            WritingInfo = false;
        }

        private void SetBankWaveArchive(int slot, ushort id)
        {
            BankInfo bank = (BankInfo)SelectedEntry();
            WaveArchiveInfo war = SA.WaveArchives.FirstOrDefault(x => x.Index == id);
            bank.WaveArchives[slot] = war;
            switch (slot)
            {
                case 0:
                    bank.ReadingWave0Id = id;
                    break;
                case 1:
                    bank.ReadingWave1Id = id;
                    break;
                case 2:
                    bank.ReadingWave2Id = id;
                    break;
                default:
                    bank.ReadingWave3Id = id;
                    break;
            }
        }

        // ------------------------------------------------------------------ group grid

        private static readonly string[] SequenceFlagOptions =
        [
            "Sequence",
            "Bank",
            "Wave Archive",
            "Sequence + Bank",
            "Sequence + Wave Archive",
            "Bank + Wave Archive",
            "Sequence + Bank + Wave Archive",
        ];

        private static readonly string[] BankFlagOptions =
            ["Bank", "Wave Archive", "Bank + Wave Archive"];

        private void ShowGroup()
        {
            GroupInfo e = (GroupInfo)SelectedEntry();
            ShowSoundPlayer = false;
            PopulateGroupGrid(e);
            ActivePanel = GroupPanel;
            SetStatus(e.Index, e.Name, null);
        }

        /// <summary>Every archive entry a group may reference, tagged with its kind.</summary>
        private List<string> GroupItemOptions()
        {
            List<string> options = [];
            foreach (SequenceInfo e in SA.Sequences)
            {
                options.Add("[" + e.Index + "] " + e.Name + " (Sequence)");
            }
            foreach (SequenceArchiveInfo e in SA.SequenceArchives)
            {
                options.Add("[" + e.Index + "] " + e.Name + " (Sequence Archive)");
            }
            foreach (BankInfo e in SA.Banks)
            {
                options.Add("[" + e.Index + "] " + e.Name + " (Bank)");
            }
            foreach (WaveArchiveInfo e in SA.WaveArchives)
            {
                options.Add("[" + e.Index + "] " + e.Name + " (Wave Archive)");
            }
            return options;
        }

        private void PopulateGroupGrid(GroupInfo group)
        {
            List<string> items = GroupItemOptions();
            GroupPanel.Entries.Clear();
            foreach (GroupEntry e in group.Entries)
            {
                GroupEntryRow row = new();
                foreach (string option in items)
                {
                    row.ItemOptions.Add(option);
                }

                switch (e.Type)
                {
                    case GroupEntryType.Sequence:
                        SequenceInfo seq = (SequenceInfo)e.Entry;
                        row.Item = "[" + seq.Index + "] " + seq.Name + " (Sequence)";
                        foreach (string flag in SequenceFlagOptions)
                        {
                            row.LoadFlagOptions.Add(flag);
                        }
                        row.LoadFlags = FlagsLabel(e);
                        break;
                    case GroupEntryType.SequenceArchive:
                        SequenceArchiveInfo arc = (SequenceArchiveInfo)e.Entry;
                        row.Item = "[" + arc.Index + "] " + arc.Name + " (Sequence Archive)";
                        row.LoadFlagOptions.Add("Sequence Archive");
                        row.LoadFlags = "Sequence Archive";
                        break;
                    case GroupEntryType.Bank:
                        BankInfo bank = (BankInfo)e.Entry;
                        row.Item = "[" + bank.Index + "] " + bank.Name + " (Bank)";
                        foreach (string flag in BankFlagOptions)
                        {
                            row.LoadFlagOptions.Add(flag);
                        }
                        row.LoadFlags = e.LoadBank && e.LoadWaveArchive
                            ? "Bank + Wave Archive"
                            : e.LoadWaveArchive ? "Wave Archive" : "Bank";
                        break;
                    default:
                        WaveArchiveInfo war = (WaveArchiveInfo)e.Entry;
                        row.Item = "[" + war.Index + "] " + war.Name + " (Wave Archive)";
                        row.LoadFlagOptions.Add("Wave Archive");
                        row.LoadFlags = "Wave Archive";
                        break;
                }
                row.Edited = _ => Edit(GroupEntriesChanged);
                GroupPanel.Entries.Add(row);
            }
            AddGroupPlaceholder();
        }

        /// <summary>
        /// Appends the blank trailing row used to add an entry. Avalonia's DataGrid has no
        /// new-row line, so this stands in for the one the WinForms grid provided; picking an
        /// item promotes it and starts a fresh one. Without it an empty group could never be
        /// filled in.
        /// </summary>
        private void AddGroupPlaceholder()
        {
            GroupEntryRow placeholder = new();
            foreach (string option in GroupItemOptions())
            {
                placeholder.ItemOptions.Add(option);
            }
            placeholder.Edited = _ => Edit(GroupEntriesChanged);
            GroupPanel.Entries.Add(placeholder);
        }

        private static string FlagsLabel(GroupEntry e)
        {
            return e.LoadSequence && e.LoadBank && e.LoadWaveArchive ? "Sequence + Bank + Wave Archive"
            : e.LoadBank && e.LoadWaveArchive ? "Bank + Wave Archive"
            : e.LoadSequence && e.LoadWaveArchive ? "Sequence + Wave Archive"
            : e.LoadSequence && e.LoadBank ? "Sequence + Bank"
            : e.LoadWaveArchive ? "Wave Archive"
            : e.LoadBank ? "Bank"
            : "Sequence";
        }

        /// <summary>
        /// Rebuilds the group's entry list from the grid, re-deriving each row's available load
        /// flags from the kind of item chosen. Port of GroupEntriesChanged.
        /// </summary>
        private void GroupEntriesChanged()
        {
            WritingInfo = true;
            try
            {
                List<GroupEntry> entries = [];
                foreach (GroupEntryRow row in GroupPanel.Entries)
                {
                    if (row.Item is null)
                    {
                        continue;
                    }
                    string previousFlags = row.LoadFlags ?? "";
                    string kind = row.Item.Split('(')[1].Split(')')[0];
                    int id = int.Parse(row.Item.Split('[')[1].Split(']')[0]);

                    GroupEntryType type;
                    object entry;
                    string[] flagOptions;
                    switch (kind)
                    {
                        case "Sequence":
                            type = GroupEntryType.Sequence;
                            entry = SA.Sequences.FirstOrDefault(x => x.Index == id);
                            flagOptions = SequenceFlagOptions;
                            break;
                        case "Sequence Archive":
                            type = GroupEntryType.SequenceArchive;
                            entry = SA.SequenceArchives.FirstOrDefault(x => x.Index == id);
                            flagOptions = ["Sequence Archive"];
                            break;
                        case "Bank":
                            type = GroupEntryType.Bank;
                            entry = SA.Banks.FirstOrDefault(x => x.Index == id);
                            flagOptions = BankFlagOptions;
                            break;
                        default:
                            type = GroupEntryType.WaveArchive;
                            entry = SA.WaveArchives.FirstOrDefault(x => x.Index == id);
                            flagOptions = ["Wave Archive"];
                            break;
                    }

                    row.LoadFlagOptions.Clear();
                    foreach (string option in flagOptions)
                    {
                        row.LoadFlagOptions.Add(option);
                    }
                    row.LoadFlags = flagOptions.Contains(previousFlags)
                        ? previousFlags
                        : flagOptions[0];

                    entries.Add(
                        new GroupEntry
                        {
                            Type = type,
                            Entry = entry,
                            ReadingId = (uint)id,
                            // "Sequence Archive" contains "Sequence", exactly as the original
                            // substring tests did, so both flags end up set for that kind.
                            LoadSequence = row.LoadFlags.Contains("Sequence"),
                            LoadSequenceArchive = row.LoadFlags.Contains("Sequence Archive"),
                            LoadBank = row.LoadFlags.Contains("Bank"),
                            LoadWaveArchive = row.LoadFlags.Contains("Wave Archive"),
                        }
                    );
                }
                ((GroupInfo)SelectedEntry()).Entries = entries;

                // The trailing blank row has just been filled in, so it is a real entry now and
                // a fresh one takes its place, the way the WinForms new-row line behaved.
                if (GroupPanel.Entries.Count == 0 || !GroupPanel.Entries[^1].IsBlank)
                {
                    AddGroupPlaceholder();
                }
            }
            finally
            {
                WritingInfo = false;
            }
        }
    }
}
