using GotaSoundIO.IO;
using NitroFileLoader;
using NitroStudio2.Models;
using NitroStudio2.Services;
using NitroStudio2.ViewModels.Panels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// The sound archive editor, ported from the WinForms MainWindow.
    ///
    /// The original repeated an eight-way <c>switch</c> on the selected node's parent for every
    /// operation (swap, rename, delete, add, export, ...). Those eight cases differed only in
    /// which <c>SoundArchive</c> list they touched, so they are collapsed here into one
    /// <see cref="Category"/> table; the behaviour of each operation is unchanged.
    /// </summary>
    public sealed partial class SoundArchiveViewModel : EditorViewModelBase, IDisposable
    {
        private readonly SequencePlayback playback = new();

        /// <summary>
        /// Drives the stream preview. Separate from <see cref="playback"/> because a .strm is
        /// decoded audio, not a sequence: it goes through the same StreamPlayer the wave archive
        /// editor uses, which decodes into memory rather than rendering a file first.
        /// </summary>
        private readonly WavePlayback streamPlayback = new();

        public SoundArchiveViewModel(IDialogService dialogs)
            : base(dialogs, typeof(SoundArchive), "Sound Archive", "dat", "Nitro Studio 2")
        {
            ShowToolsMenu = true;
            SequencePlayerPanel = new SoundPlayerPanelViewModel(playback) { PlayRequested = Play };
            StreamPreviewPanel = new SoundPlayerPanelViewModel(streamPlayback, "Stream Player:")
            {
                PlayRequested = PlayStream,
            };
            SoundPlayerPanel = SequencePlayerPanel;
            BuildCategories();
            WirePanels();
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>The transport shown for sequences.</summary>
        public SoundPlayerPanelViewModel SequencePlayerPanel { get; }

        /// <summary>The transport shown for streams. Named apart from StreamPlayerPanel, which
        /// is the info panel for the archive's stream *players*.</summary>
        public SoundPlayerPanelViewModel StreamPreviewPanel { get; }

        /// <summary>The archive being edited, or null when no file is open.</summary>
        public SoundArchive SA => File as SoundArchive;

        // ------------------------------------------------------------------ info panels

        public SettingsPanelViewModel SettingsPanel { get; } = new();

        public BlankPanelViewModel BlankPanel { get; } = new();

        public SequencePanelViewModel SequencePanel { get; } = new();

        public SequenceArchivePanelViewModel SequenceArchivePanel { get; } = new();

        public BankPanelViewModel BankPanel { get; } = new();

        public WaveArchivePanelViewModel WaveArchivePanel { get; } = new();

        public PlayerPanelViewModel PlayerPanel { get; } = new();

        public GroupPanelViewModel GroupPanel { get; } = new();

        public StreamPlayerPanelViewModel StreamPlayerPanel { get; } = new();

        public StreamPanelViewModel StreamPanel { get; } = new();

        private void WirePanels()
        {
            SettingsPanel.Edited = () => Edit(() => SA.SaveSymbols = SettingsPanel.WriteNames);

            IndexPanel.SwapRequested = () => _ = SwapAtIndexAsync();

            ForceUniqueFilePanel.Edited = () =>
                Edit(() =>
                {
                    object entry = SelectedEntry();
                    Selected()?.SetForceUnique?.Invoke(entry, ForceUniqueFilePanel.ForceUniqueFile);
                });

            WaveArchivePanel.Edited = () =>
                Edit(() => ((WaveArchiveInfo)SelectedEntry()).LoadIndividually =
                    WaveArchivePanel.LoadIndividually);

            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                BankPanel.Slots[slot].ComboEdited = () => Edit(() => BankWaveArchiveComboChanged(slot));
                BankPanel.Slots[slot].IdEdited = () => Edit(() => BankWaveArchiveIdChanged(slot));
            }

            PlayerPanel.MaxSequencesEdited = () =>
                Edit(() => ((PlayerInfo)SelectedEntry()).SequenceMax =
                    (ushort)PlayerPanel.MaxSequences);
            PlayerPanel.HeapSizeEdited = () =>
                Edit(() => ((PlayerInfo)SelectedEntry()).HeapSize = (uint)PlayerPanel.HeapSize);
            PlayerPanel.FlagsEdited = () =>
                Edit(() =>
                {
                    PlayerInfo player = (PlayerInfo)SelectedEntry();
                    for (int i = 0; i < 16; i++)
                    {
                        player.ChannelFlags[i] = PlayerPanel.ChannelFlags[i].IsSet;
                    }
                });

            StreamPlayerPanel.ChannelTypeEdited = () =>
                Edit(() =>
                {
                    StreamPlayerInfo p = (StreamPlayerInfo)SelectedEntry();
                    p.IsStereo = StreamPlayerPanel.ChannelType == 1;
                    DoInfoStuff();
                });
            StreamPlayerPanel.LeftChannelEdited = () =>
                Edit(() => ((StreamPlayerInfo)SelectedEntry()).LeftChannel =
                    (byte)StreamPlayerPanel.LeftChannel);
            StreamPlayerPanel.RightChannelEdited = () =>
                Edit(() => ((StreamPlayerInfo)SelectedEntry()).RightChannel =
                    (byte)StreamPlayerPanel.RightChannel);

            StreamPanel.VolumeEdited = () =>
                Edit(() => ((StreamInfo)SelectedEntry()).Volume = (byte)StreamPanel.Volume);
            StreamPanel.PriorityEdited = () =>
                Edit(() => ((StreamInfo)SelectedEntry()).Priority = (byte)StreamPanel.Priority);
            StreamPanel.MonoToStereoEdited = () =>
                Edit(() => ((StreamInfo)SelectedEntry()).MonoToStereo = StreamPanel.MonoToStereo);
            StreamPanel.PlayerComboEdited = () => Edit(StreamPlayerComboChanged);
            StreamPanel.PlayerIdEdited = () => Edit(StreamPlayerIdChanged);

            SequencePanel.VolumeEdited = () =>
                Edit(() => ((SequenceInfo)SelectedEntry()).Volume = (byte)SequencePanel.Volume);
            SequencePanel.ChannelPriorityEdited = () =>
                Edit(() => ((SequenceInfo)SelectedEntry()).ChannelPriority =
                    (byte)SequencePanel.ChannelPriority);
            SequencePanel.PlayerPriorityEdited = () =>
                Edit(() => ((SequenceInfo)SelectedEntry()).PlayerPriority =
                    (byte)SequencePanel.PlayerPriority);
            SequencePanel.BankComboEdited = () => Edit(SequenceBankComboChanged);
            SequencePanel.BankIdEdited = () => Edit(SequenceBankIdChanged);
            SequencePanel.PlayerComboEdited = () => Edit(SequencePlayerComboChanged);
            SequencePanel.PlayerIdEdited = () => Edit(SequencePlayerIdChanged);

            SequenceArchivePanel.OpenFileRequested = OpenSequenceArchiveFile;
        }

        /// <summary>Runs a write-back only when there is a file open and a panel is not loading.</summary>
        private void Edit(Action action)
        {
            if (FileOpen && File is not null && !WritingInfo)
            {
                action();
            }
        }

        // ------------------------------------------------------------------ categories

        /// <summary>
        /// One kind of top-level archive entry, standing in for the eight repeated switch cases.
        /// </summary>
        private sealed class Category
        {
            public string Key;
            public string Title;
            public int Icon;
            public uint MaxId;
            public Func<IList<object>> Items;
            public Func<object, int> GetIndex;
            public Action<object, int> SetIndex;
            public Func<object, string> GetName;
            public Action<object, string> SetName;
            public Func<object, IOFile> GetFile;
            public Func<object, bool> GetForceUnique;
            public Action<object, bool> SetForceUnique;
            public Action Sort;
            public Action<int> Insert;
            public Action<object> Remove;

            /// <summary>Which of the eight context-menu actions this kind of entry offers.</summary>
            public string[] Actions;
        }

        private readonly Dictionary<string, Category> categories = [];

        private static IList<object> Wrap<T>(List<T> list)
        {
            return [.. list.Cast<object>()];
        }

        private void BuildCategories()
        {
            const string full = "AddAbove,AddBelow,Replace,Export,Rename,Delete";
            const string noFile = "AddAbove,AddBelow,Rename,Delete";

            categories["sequences"] = new Category
            {
                Key = "sequences",
                Title = "Sound Sequences",
                Icon = 2,
                MaxId = SoundArchive.MaxSequenceId,
                Items = () => Wrap(SA.Sequences),
                GetIndex = o => ((SequenceInfo)o).Index,
                SetIndex = (o, v) => ((SequenceInfo)o).Index = v,
                GetName = o => ((SequenceInfo)o).Name,
                SetName = (o, v) => ((SequenceInfo)o).Name = v,
                GetFile = o => ((SequenceInfo)o).File,
                GetForceUnique = o => ((SequenceInfo)o).ForceIndividualFile,
                SetForceUnique = (o, v) => ((SequenceInfo)o).ForceIndividualFile = v,
                Sort = () => SA.Sequences = [.. SA.Sequences.OrderBy(x => x.Index)],
                Insert = AddSequence,
                Remove = o => SA.Sequences.Remove((SequenceInfo)o),
                Actions = full.Split(','),
            };
            categories["sequenceArchives"] = new Category
            {
                Key = "sequenceArchives",
                Title = "Sequence Archives",
                Icon = 3,
                MaxId = SoundArchive.MaxSequenceArchiveId,
                Items = () => Wrap(SA.SequenceArchives),
                GetIndex = o => ((SequenceArchiveInfo)o).Index,
                SetIndex = (o, v) => ((SequenceArchiveInfo)o).Index = v,
                GetName = o => ((SequenceArchiveInfo)o).Name,
                SetName = (o, v) => ((SequenceArchiveInfo)o).Name = v,
                GetFile = o => ((SequenceArchiveInfo)o).File,
                GetForceUnique = o => ((SequenceArchiveInfo)o).ForceIndividualFile,
                SetForceUnique = (o, v) => ((SequenceArchiveInfo)o).ForceIndividualFile = v,
                Sort = () => SA.SequenceArchives = [.. SA.SequenceArchives.OrderBy(x => x.Index)],
                Insert = AddSequenceArchive,
                Remove = o => SA.SequenceArchives.Remove((SequenceArchiveInfo)o),
                Actions = full.Split(','),
            };
            categories["banks"] = new Category
            {
                Key = "banks",
                Title = "Instrument Banks",
                Icon = 4,
                MaxId = SoundArchive.MaxBankId,
                Items = () => Wrap(SA.Banks),
                GetIndex = o => ((BankInfo)o).Index,
                SetIndex = (o, v) => ((BankInfo)o).Index = v,
                GetName = o => ((BankInfo)o).Name,
                SetName = (o, v) => ((BankInfo)o).Name = v,
                GetFile = o => ((BankInfo)o).File,
                GetForceUnique = o => ((BankInfo)o).ForceIndividualFile,
                SetForceUnique = (o, v) => ((BankInfo)o).ForceIndividualFile = v,
                Sort = () => SA.Banks = [.. SA.Banks.OrderBy(x => x.Index)],
                Insert = AddBank,
                Remove = o => SA.Banks.Remove((BankInfo)o),
                Actions = full.Split(','),
            };
            categories["waveArchives"] = new Category
            {
                Key = "waveArchives",
                Title = "Wave Archives",
                Icon = 5,
                MaxId = SoundArchive.MaxWaveArchiveId,
                Items = () => Wrap(SA.WaveArchives),
                GetIndex = o => ((WaveArchiveInfo)o).Index,
                SetIndex = (o, v) => ((WaveArchiveInfo)o).Index = v,
                GetName = o => ((WaveArchiveInfo)o).Name,
                SetName = (o, v) => ((WaveArchiveInfo)o).Name = v,
                GetFile = o => ((WaveArchiveInfo)o).File,
                GetForceUnique = o => ((WaveArchiveInfo)o).ForceIndividualFile,
                SetForceUnique = (o, v) => ((WaveArchiveInfo)o).ForceIndividualFile = v,
                Sort = () => SA.WaveArchives = [.. SA.WaveArchives.OrderBy(x => x.Index)],
                Insert = AddWaveArchive,
                Remove = o => SA.WaveArchives.Remove((WaveArchiveInfo)o),
                Actions = full.Split(','),
            };
            categories["players"] = new Category
            {
                Key = "players",
                Title = "Sequence Players",
                Icon = 6,
                MaxId = SoundArchive.MaxPlayerId,
                Items = () => Wrap(SA.Players),
                GetIndex = o => ((PlayerInfo)o).Index,
                SetIndex = (o, v) => ((PlayerInfo)o).Index = v,
                GetName = o => ((PlayerInfo)o).Name,
                SetName = (o, v) => ((PlayerInfo)o).Name = v,
                Sort = () => SA.Players = [.. SA.Players.OrderBy(x => x.Index)],
                Insert = AddSequencePlayer,
                Remove = o => SA.Players.Remove((PlayerInfo)o),
                Actions = noFile.Split(','),
            };
            categories["groups"] = new Category
            {
                Key = "groups",
                Title = "Groups",
                Icon = 7,
                MaxId = SoundArchive.MaxGroupId,
                Items = () => Wrap(SA.Groups),
                GetIndex = o => ((GroupInfo)o).Index,
                SetIndex = (o, v) => ((GroupInfo)o).Index = v,
                GetName = o => ((GroupInfo)o).Name,
                SetName = (o, v) => ((GroupInfo)o).Name = v,
                Sort = () => SA.Groups = [.. SA.Groups.OrderBy(x => x.Index)],
                Insert = AddGroup,
                Remove = o => SA.Groups.Remove((GroupInfo)o),
                Actions = noFile.Split(','),
            };
            categories["streamPlayers"] = new Category
            {
                Key = "streamPlayers",
                Title = "Stream Players",
                Icon = 8,
                MaxId = SoundArchive.MaxStreamPlayerId,
                Items = () => Wrap(SA.StreamPlayers),
                GetIndex = o => ((StreamPlayerInfo)o).Index,
                SetIndex = (o, v) => ((StreamPlayerInfo)o).Index = v,
                GetName = o => ((StreamPlayerInfo)o).Name,
                SetName = (o, v) => ((StreamPlayerInfo)o).Name = v,
                Sort = () => SA.StreamPlayers = [.. SA.StreamPlayers.OrderBy(x => x.Index)],
                Insert = AddStreamPlayer,
                Remove = o => SA.StreamPlayers.Remove((StreamPlayerInfo)o),
                Actions = noFile.Split(','),
            };
            categories["streams"] = new Category
            {
                Key = "streams",
                Title = "Sound Streams",
                Icon = 9,
                MaxId = SoundArchive.MaxStreamId,
                Items = () => Wrap(SA.Streams),
                GetIndex = o => ((StreamInfo)o).Index,
                SetIndex = (o, v) => ((StreamInfo)o).Index = v,
                GetName = o => ((StreamInfo)o).Name,
                SetName = (o, v) => ((StreamInfo)o).Name = v,
                GetFile = o => ((StreamInfo)o).File,
                GetForceUnique = o => ((StreamInfo)o).ForceIndividualFile,
                SetForceUnique = (o, v) => ((StreamInfo)o).ForceIndividualFile = v,
                Sort = () => SA.Streams = [.. SA.Streams.OrderBy(x => x.Index)],
                Insert = AddStream,
                Remove = o => SA.Streams.Remove((StreamInfo)o),
                Actions = full.Split(','),
            };
        }

        /// <summary>Category the selection belongs to, or null for a root or nested node.</summary>
        private Category Selected()
        {
            return SelectedNode?.Parent is not null
            && categories.TryGetValue(SelectedNode.Parent.Name, out Category category)
                ? category
                : null;
        }

        /// <summary>The archive entry the selected node stands for.</summary>
        private object SelectedEntry()
        {
            Category category = Selected();
            if (category is null)
            {
                return null;
            }
            int id = IdFromNode(SelectedNode);
            return category.Items().FirstOrDefault(x => category.GetIndex(x) == id);
        }

        /// <summary>Pulls the id out of a node's "[3] NAME" label, as GetIdFromNode did.</summary>
        public static int IdFromNode(EditorTreeNode node)
        {
            return int.Parse(node.Text.Split('[')[1].Split(']')[0]);
        }

        /// <summary>Human-readable file size, ported from MainWindow.GetBytesSize.</summary>
        public static string GetBytesSize(IOFile file)
        {
            long byteCount = file.Write().Length;
            string[] suffixes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB"];
            if (byteCount == 0)
            {
                return "0" + suffixes[0];
            }
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + " " + suffixes[place];
        }

        // ------------------------------------------------------------------ tree

        public override void UpdateNodes()
        {
            BeginUpdateNodes();
            if (Nodes.Count < 9)
            {
                Nodes.Clear();
                Nodes.Add(new EditorTreeNode("settings", "Settings", 1));
                foreach (Category category in categories.Values)
                {
                    Nodes.Add(new EditorTreeNode(category.Key, category.Title, category.Icon));
                }
            }

            if (FileOpen && File is not null)
            {
                for (int i = 1; i < Nodes.Count; i++)
                {
                    Nodes[i].ContextActions = [new MenuAction("Add", "New", RootAdd)];
                }
                foreach (Category category in categories.Values)
                {
                    EditorTreeNode root = Nodes.First(n => n.Name == category.Key);
                    foreach (object entry in category.Items())
                    {
                        int index = category.GetIndex(entry);
                        EditorTreeNode node = root.Add(
                            "entry" + index,
                            "[" + index + "] " + category.GetName(entry),
                            category.Icon
                        );
                        node.ContextActions = ActionsFor(category.Actions);

                        // A sequence archive lists the sequences it holds; those only offer
                        // Export and Rename.
                        if (category.Key == "sequenceArchives")
                        {
                            foreach (
                                SequenceArchiveSequence s in ((SequenceArchiveInfo)entry)
                                    .File
                                    .Sequences
                            )
                            {
                                EditorTreeNode child = node.Add(
                                    "entry" + s.Index,
                                    "[" + s.Index + "] " + s.Name,
                                    2
                                );
                                child.ContextActions = ActionsFor(["Export", "Rename"]);
                            }
                        }
                    }
                }
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

        /// <summary>
        /// Builds the subset of the entry context menu a node offers, the way CreateMenuStrip
        /// picked indices out of the shared sarEntryMenu.
        /// </summary>
        private IReadOnlyList<MenuAction> ActionsFor(IEnumerable<string> names)
        {
            List<MenuAction> actions = [];
            foreach (string name in names)
            {
                actions.Add(
                    name switch
                    {
                        "AddAbove" => new MenuAction("Add Above", "New", NodeAddAbove),
                        "AddBelow" => new MenuAction("Add Below", "Open", NodeAddBelow),
                        "Replace" => new MenuAction("Replace", "Import", () => _ = ReplaceAsync()),
                        "Export" => new MenuAction("Export", "Export", () => _ = ExportAsync()),
                        "Rename" => new MenuAction("Rename", "Rename", () => _ = RenameAsync()),
                        _ => new MenuAction("Delete", "Close", () => _ = DeleteAsync()),
                    }
                );
            }
            return actions;
        }

        // ------------------------------------------------------------------ info panel routing

        public override void DoInfoStuff()
        {
            if (!FileOpen || File is null || SelectedNode is null)
            {
                HideStacked();
                ActivePanel = NoInfoPanel;
                Status = "No Valid Info Selected!";
                StopPreviews();
                return;
            }

            WritingInfo = true;
            try
            {
                if (SelectedNode.Parent is null)
                {
                    HideStacked();
                    if (SelectedNode.Name == "settings")
                    {
                        SettingsPanel.WritingInfo = true;
                        SettingsPanel.WriteNames = SA.SaveSymbols;
                        SettingsPanel.WritingInfo = false;
                        ActivePanel = SettingsPanel;
                        Status = "Editing Settings.";
                    }
                    else
                    {
                        ActivePanel = NoInfoPanel;
                        Status = "No Valid Info Selected!";
                    }
                    return;
                }

                if (SelectedNode.Parent.Parent is not null)
                {
                    // A sequence inside a sequence archive.
                    HideStacked();
                    ShowSoundPlayer = true;
                    SoundPlayerPanel = SequencePlayerPanel;
                    ActivePanel = BlankPanel;
                    SequenceArchiveSequence seq = SA
                        .SequenceArchives.First(x => x.Index == IdFromNode(SelectedNode.Parent))
                        .File.Sequences.First(x => x.Index == IdFromNode(SelectedNode));
                    Status = "[" + seq.Index + "] " + seq.Name + " Selected.";
                    return;
                }

                ShowStackedFor(SelectedNode.Parent.Name);
                switch (SelectedNode.Parent.Name)
                {
                    case "sequences":
                        ShowSequence();
                        break;
                    case "sequenceArchives":
                        ShowSequenceArchive();
                        break;
                    case "banks":
                        ShowBank();
                        break;
                    case "waveArchives":
                        ShowWaveArchive();
                        break;
                    case "players":
                        ShowPlayer();
                        break;
                    case "groups":
                        ShowGroup();
                        break;
                    case "streamPlayers":
                        ShowStreamPlayer();
                        break;
                    case "streams":
                        ShowStream();
                        break;
                    default:
                        HideStacked();
                        ActivePanel = NoInfoPanel;
                        Status = "No Valid Info Selected!";
                        break;
                }
            }
            finally
            {
                WritingInfo = false;
            }
        }

        private void HideStacked()
        {
            ShowSoundPlayer = false;
            ShowIndexPanel = false;
            ShowForceUniqueFilePanel = false;
            ShowSeqArcSeqPanel = false;
        }

        /// <summary>
        /// Sets the stacked top sections for a category. Every entry gets the item index; only
        /// those with a file get force-unique; only sequences get the sound player.
        /// </summary>
        private void ShowStackedFor(string key)
        {
            Category category = categories[key];
            ShowSoundPlayer = key is "sequences" or "streams";
            SoundPlayerPanel = key == "streams" ? StreamPreviewPanel : SequencePlayerPanel;
            ShowIndexPanel = true;
            ShowForceUniqueFilePanel = category.SetForceUnique is not null;
            ShowSeqArcSeqPanel = false;

            object entry = SelectedEntry();
            IndexPanel.Maximum = category.MaxId;
            IndexPanel.ItemIndex = category.GetIndex(entry);
            if (category.GetForceUnique is not null)
            {
                ForceUniqueFilePanel.WritingInfo = true;
                ForceUniqueFilePanel.ForceUniqueFile = category.GetForceUnique(entry);
                ForceUniqueFilePanel.WritingInfo = false;
            }
        }

        /// <summary>"[3] NAME Selected. File Is 1.2 KB." as the WinForms status bar showed it.</summary>
        private void SetStatus(int index, string name, IOFile file)
        {
            Status =
                "[" + index + "] " + name + " Selected."
                + (file is null ? "" : " File Is " + GetBytesSize(file) + ".");
        }

        public override void OnClosing()
        {
            Dispose();
            base.OnClosing();
        }

        /// <summary>Silences both transports; moving off an entry should not leave it playing.</summary>
        private void StopPreviews()
        {
            playback.Stop();
            streamPlayback.Stop();
        }

        public void Dispose()
        {
            playback.Dispose();
            streamPlayback.Dispose();
        }
    }
}
