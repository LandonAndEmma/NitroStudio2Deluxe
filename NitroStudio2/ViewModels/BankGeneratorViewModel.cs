using CommunityToolkit.Mvvm.Input;
using GotaSequenceLib;
using GotaSequenceLib.Playback;
using NitroFileLoader;
using NitroFileLoader.Instrument;
using NitroStudio2.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using InstrumentType = NitroFileLoader.Instrument.InstrumentType;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// One "copy this instrument into the new bank" row. The last row in the grid is always a
    /// blank placeholder, which is how the WinForms DataGridView's new-row line behaved.
    /// </summary>
    public sealed class BankGeneratorRow : ViewModelBase
    {
        public ObservableCollection<string> InstrumentOptions { get; } = [];

        public ObservableCollection<string> WaveArchiveModeOptions { get; } =
            ["Use Generated Wave Archive", "Reference Original Wave Archive"];

        public string Bank
        {
            get;
            set => SetProperty(ref field, value);
        }

        public string Instrument
        {
            get;
            set => SetProperty(ref field, value);
        }

        public string NewId
        {
            get;
            set => SetProperty(ref field, value);
        } = "";

        public string WaveArchiveMode
        {
            get;
            set => SetProperty(ref field, value);
        }

        /// <summary>True while the row is still the untouched trailing placeholder.</summary>
        public bool IsBlank =>
            Bank is null && Instrument is null && WaveArchiveMode is null && NewId == "";

        /// <summary>Index parsed out of an "[3] - Name" entry.</summary>
        public static int IdOf(string entry)
        {
            return int.Parse(entry.Split('[')[1].Split(']')[0]);
        }
    }

    /// <summary>
    /// Builds a new bank (and optionally a generated wave archive) out of instruments copied
    /// from existing banks. Ported from the WinForms BankGenerator form.
    /// </summary>
    public sealed class BankGeneratorViewModel : ViewModelBase, IDisposable
    {
        private readonly SoundArchive archive;
        private readonly IDialogService dialogs;
        private readonly Action onArchiveChanged;
        private readonly Mixer mixer = new();
        private readonly Player player;
        private bool writingInfo;

        public BankGeneratorViewModel(
            SoundArchive archive,
            IDialogService dialogs,
            Action onArchiveChanged
        )
        {
            this.archive = archive;
            this.dialogs = dialogs;
            this.onArchiveChanged = onArchiveChanged;
            player = new Player(mixer);

            BankOptions =
            [
                .. archive
                    .Banks.Where(x => x.File.Instruments.Count > 0)
                    .Select(w => "[" + w.Index + "] - " + w.Name),
            ];

            AddPlaceholderRow();
            PlayCommand = new RelayCommand<BankGeneratorRow>(Play);
            DeleteRowCommand = new RelayCommand<BankGeneratorRow>(DeleteRow);
            CreateBankCommand = new AsyncRelayCommand(CreateBankAsync);
        }

        /// <summary>True when there is nothing to generate from, matching the form's early exit.</summary>
        public bool HasUsableBanks => BankOptions.Count > 0;

        public IReadOnlyList<string> BankOptions { get; }

        public ObservableCollection<BankGeneratorRow> Rows { get; } = [];

        public ICommand PlayCommand { get; }

        public ICommand DeleteRowCommand { get; }

        public ICommand CreateBankCommand { get; }

        /// <summary>Raised once the bank has been generated, so the view can close.</summary>
        public event EventHandler Finished;

        // ------------------------------------------------------------------ row bookkeeping

        private void AddPlaceholderRow()
        {
            BankGeneratorRow row = new();
            row.PropertyChanged += OnRowChanged;
            Rows.Add(row);
        }

        private void DeleteRow(BankGeneratorRow row)
        {
            // The trailing placeholder is not a real row and cannot be removed.
            if (row is null || ReferenceEquals(row, Rows.LastOrDefault()))
            {
                return;
            }
            row.PropertyChanged -= OnRowChanged;
            _ = Rows.Remove(row);
            NormalizeRows();
        }

        private void OnRowChanged(object sender, PropertyChangedEventArgs e)
        {
            if (writingInfo)
            {
                return;
            }
            // Editing the placeholder promotes it to a real row and starts a fresh placeholder,
            // the way typing into a DataGridView's new-row line did.
            if (ReferenceEquals(sender, Rows.LastOrDefault()))
            {
                AddPlaceholderRow();
            }
            NormalizeRows();
        }

        /// <summary>
        /// Port of InstrumentsChanged: fills in defaults, keeps each row's instrument list in
        /// step with its bank, hands out unique ids, and rewrites the "use existing archive"
        /// label to name the archives that instrument actually pulls from.
        /// </summary>
        private void NormalizeRows()
        {
            if (writingInfo)
            {
                return;
            }
            writingInfo = true;
            try
            {
                List<int> ids = [-1];
                foreach (BankGeneratorRow row in Rows.SkipLast(1))
                {
                    row.Bank ??= BankOptions[0];

                    BankInfo bankInfo = archive
                        .Banks.Where(x => x.Index == BankGeneratorRow.IdOf(row.Bank))
                        .FirstOrDefault();
                    if (bankInfo is null)
                    {
                        continue;
                    }

                    // Rebuild the instrument list for the selected bank, keeping the current
                    // choice when that bank still offers it.
                    string previous = row.Instrument ?? "";
                    row.InstrumentOptions.Clear();
                    foreach (Instrument i in bankInfo.File.Instruments)
                    {
                        row.InstrumentOptions.Add("[" + i.Index + "] - " + i.Type());
                    }
                    row.Instrument = row.InstrumentOptions.Contains(previous)
                        ? previous
                        : row.InstrumentOptions.FirstOrDefault();

                    if (row.NewId == "" || !int.TryParse(row.NewId, out int parsedId))
                    {
                        row.NewId = NextFreeId(ids).ToString();
                    }
                    else if (ids.Contains(parsedId))
                    {
                        row.NewId = NextFreeId(ids).ToString();
                    }
                    ids.Add(int.Parse(row.NewId));

                    bool usingExisting =
                        row.WaveArchiveMode is not null
                        && row.WaveArchiveMode != row.WaveArchiveModeOptions[0];
                    row.WaveArchiveMode ??= row.WaveArchiveModeOptions[0];

                    Instrument instrument = bankInfo
                        .File.Instruments.Where(x =>
                            x.Index == BankGeneratorRow.IdOf(row.Instrument)
                        )
                        .FirstOrDefault();
                    row.WaveArchiveModeOptions[1] = DescribeExistingArchives(bankInfo, instrument);
                    if (usingExisting)
                    {
                        row.WaveArchiveMode = row.WaveArchiveModeOptions[1];
                    }
                }
            }
            finally
            {
                writingInfo = false;
            }
        }

        private static int NextFreeId(List<int> used)
        {
            int candidate = used.Last() + 1;
            while (used.Contains(candidate))
            {
                candidate++;
            }
            return candidate;
        }

        private static string DescribeExistingArchives(BankInfo bank, Instrument instrument)
        {
            List<string> names = [];
            foreach (NoteInfo note in instrument?.NoteInfo ?? [])
            {
                if (note.InstrumentType != InstrumentType.PCM)
                {
                    continue;
                }
                string name = "Null";
                try
                {
                    name = bank.WaveArchives[note.WarId].Name;
                }
                catch { }
                if (!names.Contains(name))
                {
                    names.Add(name);
                }
            }
            string label = "Use Existing Wave Archive" + (names.Count > 1 ? "s" : "") + " (";
            return label + (names.Count < 1 ? "None)" : string.Join(", ", names) + ")");
        }

        // ------------------------------------------------------------------ preview

        private void Play(BankGeneratorRow row)
        {
            if (row is null || row.Bank is null || row.Instrument is null)
            {
                return;
            }
            try
            {
                player.Stop();
                BankInfo bank = archive
                    .Banks.Where(x => x.Index == BankGeneratorRow.IdOf(row.Bank))
                    .FirstOrDefault();
                Instrument instrument = bank
                    .File.Instruments.Where(x => x.Index == BankGeneratorRow.IdOf(row.Instrument))
                    .FirstOrDefault();
                player.PrepareForSong([bank.File], bank.GetAssociatedWaves());
                player.LoadSong(
                    [
                        new SequenceCommand
                        {
                            CommandType = SequenceCommands.ProgramChange,
                            Parameter = (uint)instrument.Index,
                        },
                        new SequenceCommand
                        {
                            CommandType = SequenceCommands.Note,
                            Parameter = new NoteParameter
                            {
                                Note = Notes.cn4,
                                Length = 48 * 2,
                                Velocity = 127,
                            },
                        },
                        new SequenceCommand { CommandType = SequenceCommands.Fin },
                    ]
                );
                player.Play();
            }
            catch { }
        }

        // ------------------------------------------------------------------ generation

        private struct InstrumentInfo
        {
            public BankInfo Bank;
            public Instrument Inst;
            public int NewId;
            public bool UseExistingWar;
        }

        private async Task CreateBankAsync()
        {
            BankInfo bnk = new() { File = new Bank() };
            WaveArchiveInfo war = new() { File = new WaveArchive() };
            List<InstrumentInfo> instruments = [];
            List<string> wars = [];

            foreach (BankGeneratorRow row in Rows.SkipLast(1))
            {
                if (
                    row.Bank is null
                    || row.Instrument is null
                    || row.NewId is null
                    || !int.TryParse(row.NewId, out int id)
                    || row.WaveArchiveMode is null
                )
                {
                    await dialogs.ShowMessageAsync("Grid contains invalid data.");
                    return;
                }

                BankInfo bank = archive
                    .Banks.Where(x => x.Index == BankGeneratorRow.IdOf(row.Bank))
                    .FirstOrDefault();
                Instrument instrument = bank
                    .File.Instruments.Where(x => x.Index == BankGeneratorRow.IdOf(row.Instrument))
                    .FirstOrDefault();
                bool useExistingWar = row.WaveArchiveMode != row.WaveArchiveModeOptions[0];

                instruments.Add(
                    new InstrumentInfo
                    {
                        Bank = bank,
                        Inst = Bank.DuplicateInstrument(instrument),
                        NewId = id,
                        UseExistingWar = useExistingWar,
                    }
                );

                foreach (NoteInfo note in instrument.NoteInfo)
                {
                    if (!useExistingWar || note.InstrumentType != InstrumentType.PCM)
                    {
                        continue;
                    }
                    string name = "Null";
                    try
                    {
                        name = bank.WaveArchives[note.WarId].Name;
                    }
                    catch { }
                    if (!wars.Contains(name) && name != "Null")
                    {
                        wars.Add(name);
                    }
                }
            }

            if (wars.Count > 4)
            {
                await dialogs.ShowMessageAsync(
                    "You can't generate a new bank that uses more than 4 wave archives."
                );
                return;
            }
            bool usesGen = instruments.Any(x => !x.UseExistingWar);
            if (wars.Count > 3)
            {
                await dialogs.ShowMessageAsync(
                    "You can't generate a new bank that uses more than 3 wave archives when creating a generated wave archive."
                );
                return;
            }

            try
            {
                bnk.Index = archive.Banks.Last().Index + 1;
            }
            catch { }
            while (archive.Banks.Any(x => x.Index == bnk.Index))
            {
                bnk.Index++;
            }
            try
            {
                war.Index = archive.WaveArchives.Last().Index + 1;
            }
            catch { }
            while (archive.WaveArchives.Any(x => x.Index == war.Index))
            {
                war.Index++;
            }
            bnk.Name = "GENERATED_BANK_" + bnk.Index;
            war.Name = "GENERATED_WAR_" + war.Index;

            Dictionary<ushort, ushort> warLinks = [];
            if (usesGen)
            {
                warLinks.Add((ushort)war.Index, (ushort)warLinks.Count);
            }
            Dictionary<uint, ushort> swavLinks = [];
            ushort swarNum = usesGen ? (ushort)1 : (ushort)0;
            ushort swavNum = 0;

            foreach (InstrumentInfo info in instruments)
            {
                foreach (
                    NoteInfo note in info.Inst.NoteInfo.Where(x =>
                        x.InstrumentType == InstrumentType.PCM
                    )
                )
                {
                    uint hash;
                    try
                    {
                        hash = (uint)(info.Bank.WaveArchives[note.WarId].Index << 16) | note.WaveId;
                    }
                    catch
                    {
                        continue;
                    }
                    if (info.UseExistingWar)
                    {
                        ushort existing = (ushort)info.Bank.WaveArchives[note.WarId].Index;
                        if (!warLinks.ContainsKey(existing))
                        {
                            warLinks.Add(existing, swarNum++);
                        }
                        note.WarId = warLinks[existing];
                    }
                    else
                    {
                        if (!swavLinks.ContainsKey(hash))
                        {
                            try
                            {
                                war.File.Waves.Add(
                                    info.Bank.WaveArchives[note.WarId].File.Waves[note.WaveId]
                                );
                                swavLinks.Add(hash, swavNum++);
                            }
                            catch { }
                        }
                        note.WarId = 0;
                        note.WaveId = swavLinks[hash];
                    }
                }
                info.Inst.Index = info.NewId;
                bnk.File.Instruments.Add(info.Inst);
            }

            if (warLinks.Count > 4)
            {
                await dialogs.ShowMessageAsync(
                    "Something went wrong, and the max number of wave archives (4) has been exceeded."
                );
                return;
            }

            int bnkWarId = 0;
            foreach (KeyValuePair<ushort, ushort> link in warLinks)
            {
                WaveArchiveInfo linked = archive
                    .WaveArchives.Where(x => x.Index == link.Key)
                    .FirstOrDefault();
                switch (bnkWarId)
                {
                    case 0:
                        bnk.WaveArchives[0] = linked;
                        bnk.ReadingWave0Id = link.Key;
                        break;
                    case 1:
                        bnk.WaveArchives[1] = linked;
                        bnk.ReadingWave1Id = link.Key;
                        break;
                    case 2:
                        bnk.WaveArchives[2] = linked;
                        bnk.ReadingWave2Id = link.Key;
                        break;
                    case 3:
                        bnk.WaveArchives[3] = linked;
                        bnk.ReadingWave3Id = link.Key;
                        break;
                }
                bnkWarId++;
            }

            archive.Banks.Add(bnk);
            if (usesGen)
            {
                archive.WaveArchives.Add(war);
            }
            Finished?.Invoke(this, EventArgs.Empty);
            onArchiveChanged?.Invoke();
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
