using CommunityToolkit.Mvvm.Input;
using NitroStudio2.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NitroStudio2.ViewModels.Panels
{
    /// <summary>
    /// Base for the mutually exclusive panels the editor shows on the left of the split, one per
    /// kind of tree selection. In WinForms these were 17 overlapping Panels toggled with
    /// BringToFront/Visible; here the editor binds one ContentControl to <c>ActivePanel</c> and
    /// a DataTemplate per type picks the right layout.
    ///
    /// <c>WritingInfo</c> is the same guard the WinForms editors used: it is set while the panel
    /// is being filled from the file so the change handlers do not write back what they just read.
    /// </summary>
    public abstract class InfoPanelViewModel : ViewModelBase
    {
        /// <summary>True while the panel is being populated, so setters skip their write-back.</summary>
        public bool WritingInfo { get; set; }

        /// <summary>Sets a backing field and, unless populating, runs the write-back.</summary>
        protected bool SetEdited<T>(
            ref T field,
            T value,
            System.Action onEdited,
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null
        )
        {
            if (!SetProperty(ref field, value, propertyName))
            {
                return false;
            }
            if (!WritingInfo)
            {
                onEdited?.Invoke();
            }
            return true;
        }
    }

    /// <summary>Shown for a tree selection with nothing to edit.</summary>
    public sealed class NoInfoPanelViewModel : InfoPanelViewModel { }

    /// <summary>Shown for entries whose info panel is intentionally empty.</summary>
    public sealed class BlankPanelViewModel : InfoPanelViewModel { }

    /// <summary>Sound archive settings: name writing and the sequence import/export backends.</summary>
    public sealed class SettingsPanelViewModel : InfoPanelViewModel
    {
        private bool writeNames = true;
        private int importMode;
        private int exportMode;

        public System.Action Edited { get; set; }

        public bool WriteNames
        {
            get => writeNames;
            set => SetEdited(ref writeNames, value, Edited);
        }

        public IReadOnlyList<string> ImportModes { get; } =
            ["Nitro Studio", "Midi2Sseq", "Nintendo Tools"];

        public int ImportMode
        {
            get => importMode;
            set => SetEdited(ref importMode, value, Edited);
        }

        public IReadOnlyList<string> ExportModes { get; } = ["Nitro Studio", "Sseq2Midi"];

        public int ExportMode
        {
            get => exportMode;
            set => SetEdited(ref exportMode, value, Edited);
        }
    }

    /// <summary>The index an entry is referenced by, plus the swap button.</summary>
    public sealed class IndexPanelViewModel : InfoPanelViewModel
    {
        private decimal itemIndex;

        public IndexPanelViewModel()
        {
            SwapCommand = new RelayCommand(() => SwapRequested?.Invoke());
        }

        public System.Action SwapRequested { get; set; }

        public decimal ItemIndex
        {
            get => itemIndex;
            set => SetProperty(ref itemIndex, value);
        }

        public decimal Maximum { get; set; } = uint.MaxValue;

        public ICommand SwapCommand { get; }
    }

    /// <summary>Whether an entry's file is written out on its own rather than shared.</summary>
    public sealed class ForceUniqueFilePanelViewModel : InfoPanelViewModel
    {
        private bool forceUniqueFile;

        public System.Action Edited { get; set; }

        public bool ForceUniqueFile
        {
            get => forceUniqueFile;
            set => SetEdited(ref forceUniqueFile, value, Edited);
        }
    }

    /// <summary>Wave archive load flag.</summary>
    public sealed class WaveArchivePanelViewModel : InfoPanelViewModel
    {
        private bool loadIndividually;

        public System.Action Edited { get; set; }

        public bool LoadIndividually
        {
            get => loadIndividually;
            set => SetEdited(ref loadIndividually, value, Edited);
        }
    }

    /// <summary>One of the four "wave archive N" rows a bank can reference.</summary>
    public sealed class WaveArchiveSlotViewModel : ViewModelBase
    {
        private string selected;
        private decimal id;

        public WaveArchiveSlotViewModel(int slot)
        {
            Slot = slot;
        }

        public int Slot { get; }

        public string Label => "Wave Archive " + Slot + ":";

        public ObservableCollection<string> Options { get; } = [];

        /// <summary>Runs when the user picks from the combo box.</summary>
        public System.Action ComboEdited { get; set; }

        /// <summary>Runs when the user types an id directly.</summary>
        public System.Action IdEdited { get; set; }

        public bool WritingInfo { get; set; }

        public string Selected
        {
            get => selected;
            set
            {
                if (SetProperty(ref selected, value) && !WritingInfo)
                {
                    ComboEdited?.Invoke();
                }
            }
        }

        public decimal Id
        {
            get => id;
            set
            {
                if (SetProperty(ref id, value) && !WritingInfo)
                {
                    IdEdited?.Invoke();
                }
            }
        }
    }

    /// <summary>Bank info: the four wave archives it draws samples from.</summary>
    public class BankPanelViewModel : InfoPanelViewModel
    {
        public BankPanelViewModel()
        {
            Slots = [new(0), new(1), new(2), new(3)];
        }

        public IReadOnlyList<WaveArchiveSlotViewModel> Slots { get; }

        public new bool WritingInfo
        {
            get => base.WritingInfo;
            set
            {
                base.WritingInfo = value;
                foreach (WaveArchiveSlotViewModel slot in Slots)
                {
                    slot.WritingInfo = value;
                }
            }
        }
    }

    /// <summary>The bank editor's own copy of the wave archive slots, shown on the right pane.</summary>
    public sealed class BankEditorWarsViewModel : BankPanelViewModel { }

    /// <summary>Group contents grid.</summary>
    public sealed class GroupPanelViewModel : InfoPanelViewModel
    {
        public ObservableCollection<GroupEntryRow> Entries { get; } = [];
    }

    /// <summary>One row of the group grid: which file, and how it is loaded.</summary>
    public sealed class GroupEntryRow : ViewModelBase
    {
        private string item;
        private string loadFlags;

        public ObservableCollection<string> ItemOptions { get; } = [];

        public ObservableCollection<string> LoadFlagOptions { get; } = [];

        /// <summary>Runs when the user picks from either combo box, as BankRegionRow's does.</summary>
        public System.Action<GroupEntryRow> Edited { get; set; }

        /// <summary>True while this is the untouched trailing row used to add an entry.</summary>
        public bool IsBlank => Item is null;

        public string Item
        {
            get => item;
            set => Set(ref item, value);
        }

        public string LoadFlags
        {
            get => loadFlags;
            set => Set(ref loadFlags, value);
        }

        private void Set<T>(
            ref T field,
            T value,
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null
        )
        {
            if (SetProperty(ref field, value, propertyName))
            {
                Edited?.Invoke(this);
            }
        }
    }

    /// <summary>Stream player: mono/stereo and which hardware channels it occupies.</summary>
    public sealed class StreamPlayerPanelViewModel : InfoPanelViewModel
    {
        private int channelType;
        private decimal leftChannel;
        private decimal rightChannel;

        public System.Action ChannelTypeEdited { get; set; }

        public System.Action LeftChannelEdited { get; set; }

        public System.Action RightChannelEdited { get; set; }

        public IReadOnlyList<string> ChannelTypes { get; } = ["Mono", "Stereo"];

        public int ChannelType
        {
            get => channelType;
            set => SetEdited(ref channelType, value, ChannelTypeEdited);
        }

        /// <summary>Stereo shows both channel boxes; mono shows only the left one.</summary>
        public bool IsStereo => channelType == 1;

        public decimal LeftChannel
        {
            get => leftChannel;
            set => SetEdited(ref leftChannel, value, LeftChannelEdited);
        }

        public decimal RightChannel
        {
            get => rightChannel;
            set => SetEdited(ref rightChannel, value, RightChannelEdited);
        }
    }

    /// <summary>Stream entry info: volume, priority, player and the mono-to-stereo flag.</summary>
    public sealed class StreamPanelViewModel : InfoPanelViewModel
    {
        private decimal volume;
        private decimal priority;
        private string player;
        private decimal playerId;
        private bool monoToStereo;

        public System.Action VolumeEdited { get; set; }

        public System.Action PriorityEdited { get; set; }

        public System.Action PlayerComboEdited { get; set; }

        public System.Action PlayerIdEdited { get; set; }

        public System.Action MonoToStereoEdited { get; set; }

        public decimal Volume
        {
            get => volume;
            set => SetEdited(ref volume, value, VolumeEdited);
        }

        public decimal Priority
        {
            get => priority;
            set => SetEdited(ref priority, value, PriorityEdited);
        }

        public ObservableCollection<string> PlayerOptions { get; } = [];

        public string Player
        {
            get => player;
            set => SetEdited(ref player, value, PlayerComboEdited);
        }

        public decimal PlayerId
        {
            get => playerId;
            set => SetEdited(ref playerId, value, PlayerIdEdited);
        }

        public bool MonoToStereo
        {
            get => monoToStereo;
            set => SetEdited(ref monoToStereo, value, MonoToStereoEdited);
        }
    }

    /// <summary>Sequence player info: sequence limit, heap size and the 16 channel flags.</summary>
    public sealed class PlayerPanelViewModel : InfoPanelViewModel
    {
        private decimal maxSequences;
        private decimal heapSize;

        public PlayerPanelViewModel()
        {
            ChannelFlags = new ChannelFlag[16];
            for (int i = 0; i < 16; i++)
            {
                ChannelFlags[i] = new ChannelFlag(i, () =>
                {
                    if (!WritingInfo)
                    {
                        FlagsEdited?.Invoke();
                    }
                });
            }
        }

        public System.Action MaxSequencesEdited { get; set; }

        public System.Action HeapSizeEdited { get; set; }

        public System.Action FlagsEdited { get; set; }

        public decimal MaxSequences
        {
            get => maxSequences;
            set => SetEdited(ref maxSequences, value, MaxSequencesEdited);
        }

        public decimal HeapSize
        {
            get => heapSize;
            set => SetEdited(ref heapSize, value, HeapSizeEdited);
        }

        public ChannelFlag[] ChannelFlags { get; }
    }

    /// <summary>One of the player's 16 channel-allowed flags.</summary>
    public sealed class ChannelFlag : ViewModelBase
    {
        private readonly System.Action onChanged;
        private bool isSet;

        public ChannelFlag(int index, System.Action onChanged)
        {
            Index = index;
            this.onChanged = onChanged;
        }

        public int Index { get; }

        public string Label => Index.ToString();

        public bool IsSet
        {
            get => isSet;
            set
            {
                if (SetProperty(ref isSet, value))
                {
                    onChanged?.Invoke();
                }
            }
        }
    }

    /// <summary>Sequence entry info: bank, volume, priorities and player.</summary>
    public sealed class SequencePanelViewModel : InfoPanelViewModel
    {
        private string bank;
        private decimal bankId;
        private decimal volume;
        private decimal channelPriority;
        private decimal playerPriority;
        private string player;
        private decimal playerId;

        public System.Action BankComboEdited { get; set; }

        public System.Action BankIdEdited { get; set; }

        public System.Action VolumeEdited { get; set; }

        public System.Action ChannelPriorityEdited { get; set; }

        public System.Action PlayerPriorityEdited { get; set; }

        public System.Action PlayerComboEdited { get; set; }

        public System.Action PlayerIdEdited { get; set; }

        public ObservableCollection<string> BankOptions { get; } = [];

        public string Bank
        {
            get => bank;
            set => SetEdited(ref bank, value, BankComboEdited);
        }

        public decimal BankId
        {
            get => bankId;
            set => SetEdited(ref bankId, value, BankIdEdited);
        }

        public decimal Volume
        {
            get => volume;
            set => SetEdited(ref volume, value, VolumeEdited);
        }

        public decimal ChannelPriority
        {
            get => channelPriority;
            set => SetEdited(ref channelPriority, value, ChannelPriorityEdited);
        }

        public decimal PlayerPriority
        {
            get => playerPriority;
            set => SetEdited(ref playerPriority, value, PlayerPriorityEdited);
        }

        public ObservableCollection<string> PlayerOptions { get; } = [];

        public string Player
        {
            get => player;
            set => SetEdited(ref player, value, PlayerComboEdited);
        }

        public decimal PlayerId
        {
            get => playerId;
            set => SetEdited(ref playerId, value, PlayerIdEdited);
        }
    }

    /// <summary>
    /// The sequence editor's left pane: which bank to preview with, the 16 track mute/solo
    /// strip, and the WAV/MIDI export buttons.
    /// </summary>
    public sealed class SequenceBankPanelViewModel : InfoPanelViewModel
    {
        private string bank;
        private decimal bankId;

        public SequenceBankPanelViewModel()
        {
            Tracks = new TrackViewModel[16];
            for (int i = 0; i < 16; i++)
            {
                Tracks[i] = new TrackViewModel(i);
            }
            ExportMidiCommand = new RelayCommand(() => ExportMidiRequested?.Invoke());
            ExportWavCommand = new RelayCommand(() => ExportWavRequested?.Invoke());
        }

        public System.Action BankComboEdited { get; set; }

        public System.Action BankIdEdited { get; set; }

        public System.Action ExportMidiRequested { get; set; }

        public System.Action ExportWavRequested { get; set; }

        public ObservableCollection<string> BankOptions { get; } = [];

        public string Bank
        {
            get => bank;
            set => SetEdited(ref bank, value, BankComboEdited);
        }

        public decimal BankId
        {
            get => bankId;
            set => SetEdited(ref bankId, value, BankIdEdited);
        }

        public bool BankSelectionEnabled { get; set; } = true;

        public TrackViewModel[] Tracks { get; }

        public ICommand ExportMidiCommand { get; }

        public ICommand ExportWavCommand { get; }
    }

    /// <summary>One of the 16 sequence tracks: enabled tick box, solo button and state light.</summary>
    public sealed class TrackViewModel : ViewModelBase
    {
        private bool isEnabled = true;
        private string state = "Idle";

        public TrackViewModel(int index)
        {
            Index = index;
            SoloCommand = new RelayCommand(() => SoloRequested?.Invoke(Index));
        }

        public int Index { get; }

        public string Label => "Track " + Index + ":";

        public System.Action<int> EnabledChanged { get; set; }

        public System.Action<int> SoloRequested { get; set; }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (SetProperty(ref isEnabled, value))
                {
                    EnabledChanged?.Invoke(Index);
                }
            }
        }

        /// <summary>"Idle", "Mute" or "NoteDown"; names the image under Assets/Tracks.</summary>
        public string State
        {
            get => state;
            set
            {
                if (SetProperty(ref state, value))
                {
                    OnPropertyChanged(nameof(StateImage));
                }
            }
        }

        public Avalonia.Media.Imaging.Bitmap StateImage => Assets.Track(state);

        public ICommand SoloCommand { get; }
    }

    /// <summary>Sequence archive entry: the button that opens the contained sequence.</summary>
    public sealed class SequenceArchivePanelViewModel : InfoPanelViewModel
    {
        public SequenceArchivePanelViewModel()
        {
            OpenFileCommand = new RelayCommand(() => OpenFileRequested?.Invoke());
        }

        public System.Action OpenFileRequested { get; set; }

        public ICommand OpenFileCommand { get; }
    }

    /// <summary>Sequence archive editor: which contained sequence to preview.</summary>
    public sealed class SequenceArchiveSequencePanelViewModel : InfoPanelViewModel
    {
        private string sequence;
        private decimal sequenceId;

        public System.Action SequenceComboEdited { get; set; }

        public System.Action SequenceIdEdited { get; set; }

        public ObservableCollection<string> SequenceOptions { get; } = [];

        public string Sequence
        {
            get => sequence;
            set => SetEdited(ref sequence, value, SequenceComboEdited);
        }

        public decimal SequenceId
        {
            get => sequenceId;
            set => SetEdited(ref sequenceId, value, SequenceIdEdited);
        }
    }

    /// <summary>Bank editor: instrument type, drum set range and the region grid.</summary>
    public sealed class BankEditorPanelViewModel : InfoPanelViewModel
    {
        private string instrumentType = "Direct";
        private string drumSetStartNote;
        private decimal drumSetStartId;
        private bool canBeDirect = true;
        private bool canBeKeySplit = true;

        public System.Action InstrumentTypeEdited { get; set; }

        public System.Action DrumSetRangeComboEdited { get; set; }

        public System.Action DrumSetRangeIdEdited { get; set; }

        public System.Action RegionsEdited { get; set; }

        /// <summary>"Direct", "Drum Set" or "Key Split"; drives the three radio buttons.</summary>
        public string InstrumentType
        {
            get => instrumentType;
            set
            {
                if (SetEdited(ref instrumentType, value, InstrumentTypeEdited))
                {
                    OnPropertyChanged(nameof(IsDirect));
                    OnPropertyChanged(nameof(IsDrumSet));
                    OnPropertyChanged(nameof(IsKeySplit));
                    OnPropertyChanged(nameof(ShowsDrumSetRange));
                }
            }
        }

        public bool IsDirect
        {
            get => instrumentType == "Direct";
            set
            {
                if (value)
                {
                    InstrumentType = "Direct";
                }
            }
        }

        public bool IsDrumSet
        {
            get => instrumentType == "Drum Set";
            set
            {
                if (value)
                {
                    InstrumentType = "Drum Set";
                }
            }
        }

        public bool IsKeySplit
        {
            get => instrumentType == "Key Split";
            set
            {
                if (value)
                {
                    InstrumentType = "Key Split";
                }
            }
        }

        /// <summary>Only a drum set has a start note.</summary>
        public bool ShowsDrumSetRange => IsDrumSet;

        /// <summary>Direct holds one region, key split at most eight; beyond that only a drum set.</summary>
        public bool CanBeDirect
        {
            get => canBeDirect;
            set => SetProperty(ref canBeDirect, value);
        }

        public bool CanBeKeySplit
        {
            get => canBeKeySplit;
            set => SetProperty(ref canBeKeySplit, value);
        }

        public IReadOnlyList<string> NoteOptions { get; } = NoteNames.All;

        public string DrumSetStartNote
        {
            get => drumSetStartNote;
            set => SetEdited(ref drumSetStartNote, value, DrumSetRangeComboEdited);
        }

        public decimal DrumSetStartId
        {
            get => drumSetStartId;
            set => SetEdited(ref drumSetStartId, value, DrumSetRangeIdEdited);
        }

        public ObservableCollection<BankRegionRow> Regions { get; } = [];
    }

    /// <summary>One row of the bank editor's region grid.</summary>
    public sealed class BankRegionRow : ViewModelBase
    {
        private string endNote;
        private string instrumentType;
        private string waveId = "";
        private string waveArchiveId = "";
        private string baseNote;
        private string attack = "";
        private string decay = "";
        private string sustain = "";
        private string release = "";
        private string pan = "";

        public System.Action<BankRegionRow> Edited { get; set; }

        public System.Action<BankRegionRow> PlayRequested { get; set; }

        public BankRegionRow()
        {
            PlayCommand = new RelayCommand(() => PlayRequested?.Invoke(this));
        }

        public ICommand PlayCommand { get; }

        /// <summary>Previewing needs the surrounding archive's waves, so it is off standalone.</summary>
        public bool CanPlay { get; set; } = true;

        /// <summary>True while this is the untouched trailing row used to add a region.</summary>
        public bool IsBlank =>
            EndNote is null && InstrumentType is null && BaseNote is null && WaveId == "";

        public IReadOnlyList<string> NoteOptions { get; } = NoteNames.All;

        /// <summary>
        /// Shared, not per row: a DataGrid recycles cells, and giving each row its own collection
        /// makes the combo box's ItemsSource change identity every time one is reused, which
        /// makes it drop its selection and write the loss back through the binding.
        /// </summary>
        public static IReadOnlyList<string> InstrumentTypes { get; } =
            ["PCM", "PSG", "Noise", "Direct PCM", "Null"];

        public IReadOnlyList<string> InstrumentTypeOptions => InstrumentTypes;

        public string EndNote
        {
            get => endNote;
            set => Set(ref endNote, value);
        }

        public string InstrumentType
        {
            get => instrumentType;
            set => Set(ref instrumentType, value);
        }

        public string WaveId
        {
            get => waveId;
            set => Set(ref waveId, value);
        }

        public string WaveArchiveId
        {
            get => waveArchiveId;
            set => Set(ref waveArchiveId, value);
        }

        public string BaseNote
        {
            get => baseNote;
            set => Set(ref baseNote, value);
        }

        public string Attack
        {
            get => attack;
            set => Set(ref attack, value);
        }

        public string Decay
        {
            get => decay;
            set => Set(ref decay, value);
        }

        public string Sustain
        {
            get => sustain;
            set => Set(ref sustain, value);
        }

        public string Release
        {
            get => release;
            set => Set(ref release, value);
        }

        public string Pan
        {
            get => pan;
            set => Set(ref pan, value);
        }

        private void Set<T>(
            ref T field,
            T value,
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null
        )
        {
            if (SetProperty(ref field, value, propertyName))
            {
                Edited?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// The transport at the top of the left pane ("Kermalis Sound Player"). Wraps the shared
    /// <see cref="SequencePlayback"/> so the panel is pure binding.
    /// </summary>
    public sealed class SoundPlayerPanelViewModel : InfoPanelViewModel
    {
        public SoundPlayerPanelViewModel(PlaybackTransport playback, string label = "Kermalis Sound Player:")
        {
            Playback = playback;
            Label = label;
            PlayCommand = new RelayCommand(() => PlayRequested?.Invoke());
            PauseCommand = new RelayCommand(playback.Pause);
            StopCommand = new RelayCommand(playback.Stop);
        }

        public PlaybackTransport Playback { get; }

        /// <summary>Panel caption; the wave archive editor called it "Sound Player Deluxe(tm)".</summary>
        public string Label { get; }

        /// <summary>What to play depends on the tree selection, so the editor supplies it.</summary>
        public System.Action PlayRequested { get; set; }

        public ICommand PlayCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand StopCommand { get; }
    }

    /// <summary>Note names in the order the WinForms combo boxes listed them, "cnm1 (0)" style.</summary>
    public static class NoteNames
    {
        public static IReadOnlyList<string> All { get; } = Build();

        private static string[] Build()
        {
            string[] names = ["cn", "cs", "dn", "ds", "en", "fn", "fs", "gn", "gs", "an", "as", "bn"];
            string[] result = new string[128];
            for (int i = 0; i < 128; i++)
            {
                int octave = (i / 12) - 1;
                string suffix = octave < 0 ? "m" + -octave : octave.ToString();
                result[i] = names[i % 12] + suffix + " (" + i + ")";
            }
            return result;
        }
    }
}
