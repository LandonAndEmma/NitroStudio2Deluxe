using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundBank.DLS;
using GotaSoundBank.SF2;
using GotaSoundIO.IO;
using GotaSoundIO.Sound.Formats;
using Microsoft.VisualBasic;
using NitroFileLoader;
using NitroFileLoader.Instrument;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NitroStudio2
{
    public class MainWindow : EditorBase
    {
        public static string NitroPath = Application.StartupPath;
        public SoundArchive SA => File as SoundArchive;
        public int StreamTempCount = 0;
        public Mixer Mixer = new();
        public Player Player;
        public Timer Timer = new();
        public bool PositionBarFree = true;

        public MainWindow()
            : base(typeof(SoundArchive), "Sound Archive", "dat", "Nitro Studio 2", null)
        {
            Init();
            Text = "Nitro Studio 2";
        }

        public MainWindow(string fileToOpen)
            : base(typeof(SoundArchive), "Sound Archive", "dat", "Nitro Studio 2", fileToOpen, null)
        {
            Init();
        }

        public void Init()
        {
            Icon = Properties.Resources.Icon;
            FormClosing += new FormClosingEventHandler(SAClosing);
            toolsToolStripMenuItem.Visible = true;
            writeNamesBox.CheckedChanged += new EventHandler(WriteNamesChanged);
            seqImportModeBox.SelectedIndex = 0;
            seqExportModeBox.SelectedIndex = 0;
            swapAtIndexButton.Click += new EventHandler(SwapAtIndexButtonPressed);
            forceUniqueFileBox.CheckedChanged += new EventHandler(ForceUniqueIdChanged);
            seqVolumeBox.ValueChanged += new EventHandler(SequenceVolumeChanged);
            seqChannelPriorityBox.ValueChanged += new EventHandler(SequenceChannelPriorityChanged);
            seqPlayerPriorityBox.ValueChanged += new EventHandler(SequencePlayerPriorityChanged);
            seqBankBox.ValueChanged += new EventHandler(SequenceBankBoxChanged);
            seqBankComboBox.SelectedIndexChanged += new EventHandler(SequenceBankComboBoxChanged);
            seqPlayerBox.ValueChanged += new EventHandler(SequencePlayerBoxChanged);
            seqPlayerComboBox.SelectedIndexChanged += new EventHandler(
                SequencePlayerComboBoxChanged
            );
            seqArcOpenFileButton.Click += new EventHandler(OpenSeqArcFile);
            bnkWar0Box.ValueChanged += new EventHandler(BnkWar0BoxChanged);
            bnkWar1Box.ValueChanged += new EventHandler(BnkWar1BoxChanged);
            bnkWar2Box.ValueChanged += new EventHandler(BnkWar2BoxChanged);
            bnkWar3Box.ValueChanged += new EventHandler(BnkWar3BoxChanged);
            bnkWar0ComboBox.SelectedValueChanged += new EventHandler(BnkWar0ComboBoxChanged);
            bnkWar1ComboBox.SelectedValueChanged += new EventHandler(BnkWar1ComboBoxChanged);
            bnkWar2ComboBox.SelectedValueChanged += new EventHandler(BnkWar2ComboBoxChanged);
            bnkWar3ComboBox.SelectedValueChanged += new EventHandler(BnkWar3ComboBoxChanged);
            loadIndividuallyBox.CheckedChanged += new EventHandler(WarLoadIndividualChanged);
            playerMaxSequencesBox.ValueChanged += new EventHandler(PlayerSequenceMaxChanged);
            playerHeapSizeBox.ValueChanged += new EventHandler(PlayerHeapSizeChanged);
            playerFlag0Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag1Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag2Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag3Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag4Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag5Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag6Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag7Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag8Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag9Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag10Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag11Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag12Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag13Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag14Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            playerFlag15Box.CheckedChanged += new EventHandler(PlayerFlagsChanged);
            grpEntries.CellValueChanged += new DataGridViewCellEventHandler(GroupEntriesChanged);
            grpEntries.RowsRemoved += new DataGridViewRowsRemovedEventHandler(GroupEntriesChanged);
            stmPlayerChannelType.SelectedIndexChanged += new EventHandler(StreamPlayerTypeChanged);
            stmPlayerLeftChannelBox.ValueChanged += new EventHandler(
                StreamPlayerLeftChannelChanged
            );
            stmPlayerRightChannelBox.ValueChanged += new EventHandler(
                StreamPlayerRightChannelChanged
            );
            stmVolumeBox.ValueChanged += new EventHandler(StreamVolumeChanged);
            stmPriorityBox.ValueChanged += new EventHandler(StreamPriorityChanged);
            stmPlayerBox.ValueChanged += new EventHandler(StreamPlayerBoxChanged);
            stmPlayerComboBox.SelectedIndexChanged += new EventHandler(StreamPlayerComboBoxChanged);
            stmMonoToStereoBox.CheckedChanged += new EventHandler(StreamMonoToStereoChanged);
            Player = new Player(Mixer);
            kermalisPlayButton.Click += new EventHandler(PlayClick);
            kermalisPauseButton.Click += new EventHandler(PauseClick);
            kermalisStopButton.Click += new EventHandler(StopClick);
            kermalisVolumeSlider.ValueChanged += new EventHandler(VolumeChanged);
            kermalisLoopBox.CheckedChanged += new EventHandler(LoopChanged);
            kermalisPosition.MouseUp += new MouseEventHandler(PositionMouseUp);
            kermalisPosition.MouseDown += new MouseEventHandler(PositionMouseDown);
            tree.KeyPress += new KeyPressEventHandler(KeyPress);
            kermalisVolumeSlider.Value = 75;
            Mixer.Volume = .75f;
            Timer.Tick += PositionTick;
            Timer.Interval = 1000 / 30;
            Timer.Start();
        }

        public override void UpdateNodes()
        {
            BeginUpdateNodes();
            if (tree.Nodes.Count < 9)
            {
                tree.Nodes.RemoveAt(0);
                _ = tree.Nodes.Add("settings", "Settings", 1, 1);
                _ = tree.Nodes.Add("sequences", "Sound Sequences", 2, 2);
                _ = tree.Nodes.Add("sequenceArchives", "Sequence Archives", 3, 3);
                _ = tree.Nodes.Add("banks", "Instrument Banks", 4, 4);
                _ = tree.Nodes.Add("waveArchives", "Wave Archives", 5, 5);
                _ = tree.Nodes.Add("players", "Sequence Players", 6, 6);
                _ = tree.Nodes.Add("groups", "Groups", 7, 7);
                _ = tree.Nodes.Add("streamPlayers", "Stream Players", 8, 8);
                _ = tree.Nodes.Add("streams", "Sound Streams", 9, 9);
            }
            if (FileOpen && File != null)
            {
                for (int i = 1; i < 9; i++)
                {
                    tree.Nodes[i].ContextMenuStrip = rootMenu;
                }
                foreach (SequenceInfo e in SA.Sequences)
                {
                    _ = tree.Nodes["sequences"]
                        .Nodes.Add("entry" + e.Index, "[" + e.Index + "] " + e.Name, 2, 2);
                    tree.Nodes["sequences"].Nodes["entry" + e.Index].ContextMenuStrip =
                        CreateMenuStrip(
                            sarEntryMenu,
                            new int[] { 0, 1, 4, 5, 6, 7 },
                            new EventHandler[]
                            {
                                new(AddAbove),
                                new(AddBelow),
                                new(Replace),
                                new(Export),
                                new(Rename),
                                new(Delete),
                            }
                        );
                }
                foreach (SequenceArchiveInfo e in SA.SequenceArchives)
                {
                    _ = tree.Nodes["sequenceArchives"]
                        .Nodes.Add("entry" + e.Index, "[" + e.Index + "] " + e.Name, 3, 3);
                    tree.Nodes["sequenceArchives"].Nodes["entry" + e.Index].ContextMenuStrip =
                        CreateMenuStrip(
                            sarEntryMenu,
                            new int[] { 0, 1, 4, 5, 6, 7 },
                            new EventHandler[]
                            {
                                new(AddAbove),
                                new(AddBelow),
                                new(Replace),
                                new(Export),
                                new(Rename),
                                new(Delete),
                            }
                        );
                    foreach (SequenceArchiveSequence s in e.File.Sequences)
                    {
                        _ = tree.Nodes["sequenceArchives"]
                            .Nodes["entry" + e.Index]
                            .Nodes.Add("entry" + s.Index, "[" + s.Index + "] " + s.Name, 2, 2);
                        tree.Nodes["sequenceArchives"]
                            .Nodes["entry" + e.Index]
                            .Nodes["entry" + s.Index]
                            .ContextMenuStrip = CreateMenuStrip(
                            sarEntryMenu,
                            new int[] { 5, 6 },
                            new EventHandler[]
                            {
                                new(Export),
                                new(Rename),
                            }
                        );
                    }
                }
                foreach (BankInfo e in SA.Banks)
                {
                    _ = tree.Nodes["banks"]
                        .Nodes.Add("entry" + e.Index, "[" + e.Index + "] " + e.Name, 4, 4);
                    tree.Nodes["banks"].Nodes["entry" + e.Index].ContextMenuStrip = CreateMenuStrip(
                        sarEntryMenu,
                        new int[] { 0, 1, 4, 5, 6, 7 },
                        new EventHandler[]
                        {
                            new(AddAbove),
                            new(AddBelow),
                            new(Replace),
                            new(Export),
                            new(Rename),
                            new(Delete),
                        }
                    );
                }
                foreach (WaveArchiveInfo e in SA.WaveArchives)
                {
                    _ = tree.Nodes["waveArchives"]
                        .Nodes.Add("entry" + e.Index, "[" + e.Index + "] " + e.Name, 5, 5);
                    tree.Nodes["waveArchives"].Nodes["entry" + e.Index].ContextMenuStrip =
                        CreateMenuStrip(
                            sarEntryMenu,
                            new int[] { 0, 1, 4, 5, 6, 7 },
                            new EventHandler[]
                            {
                                new(AddAbove),
                                new(AddBelow),
                                new(Replace),
                                new(Export),
                                new(Rename),
                                new(Delete),
                            }
                        );
                }
                foreach (PlayerInfo e in SA.Players)
                {
                    _ = tree.Nodes["players"]
                        .Nodes.Add("entry" + e.Index, "[" + e.Index + "] " + e.Name, 6, 6);
                    tree.Nodes["players"].Nodes["entry" + e.Index].ContextMenuStrip =
                        CreateMenuStrip(
                            sarEntryMenu,
                            new int[] { 0, 1, 6, 7 },
                            new EventHandler[]
                            {
                                new(AddAbove),
                                new(AddBelow),
                                new(Rename),
                                new(Delete),
                            }
                        );
                }
                foreach (GroupInfo e in SA.Groups)
                {
                    _ = tree.Nodes["groups"]
                        .Nodes.Add("entry" + e.Index, "[" + e.Index + "] " + e.Name, 7, 7);
                    tree.Nodes["groups"].Nodes["entry" + e.Index].ContextMenuStrip =
                        CreateMenuStrip(
                            sarEntryMenu,
                            new int[] { 0, 1, 6, 7 },
                            new EventHandler[]
                            {
                                new(AddAbove),
                                new(AddBelow),
                                new(Rename),
                                new(Delete),
                            }
                        );
                }
                foreach (StreamPlayerInfo e in SA.StreamPlayers)
                {
                    _ = tree.Nodes["streamPlayers"]
                        .Nodes.Add("entry" + e.Index, "[" + e.Index + "] " + e.Name, 8, 8);
                    tree.Nodes["streamPlayers"].Nodes["entry" + e.Index].ContextMenuStrip =
                        CreateMenuStrip(
                            sarEntryMenu,
                            new int[] { 0, 1, 6, 7 },
                            new EventHandler[]
                            {
                                new(AddAbove),
                                new(AddBelow),
                                new(Rename),
                                new(Delete),
                            }
                        );
                }
                foreach (StreamInfo e in SA.Streams)
                {
                    _ = tree.Nodes["streams"]
                        .Nodes.Add("entry" + e.Index, "[" + e.Index + "] " + e.Name, 9, 9);
                    tree.Nodes["streams"].Nodes["entry" + e.Index].ContextMenuStrip =
                        CreateMenuStrip(
                            sarEntryMenu,
                            new int[] { 0, 1, 4, 5, 6, 7 },
                            new EventHandler[]
                            {
                                new(AddAbove),
                                new(AddBelow),
                                new(Replace),
                                new(Export),
                                new(Rename),
                                new(Delete),
                            }
                        );
                }
            }
            else
            {
                foreach (TreeNode n in tree.Nodes)
                {
                    n.ContextMenuStrip = null;
                }
            }
            EndUpdateNodes();
        }

        public override void DoInfoStuff()
        {
            base.DoInfoStuff();
            WritingInfo = true;
            void HideStuff()
            {
                kermalisSoundPlayerPanel.Hide();
                indexPanel.Hide();
                forceUniqueFilePanel.Hide();
                kermalisSoundPlayerPanel.SendToBack();
                indexPanel.SendToBack();
                forceUniqueFilePanel.SendToBack();
            }
            if (!FileOpen || File == null)
            {
                HideStuff();
                if (Player != null)
                {
                    StopClick(this, null);
                }
                return;
            }
            bool panelSelected = false;
            if (tree.SelectedNode.Parent == null)
            {
                if (tree.SelectedNode == tree.Nodes["settings"])
                {
                    HideStuff();
                    settingsPanel.BringToFront();
                    settingsPanel.Show();
                    writeNamesBox.Checked = SA.SaveSymbols;
                    status.Text = "Editing Settings.";
                    panelSelected = true;
                }
            }
            else
            {
                panelSelected = true;
                if (tree.SelectedNode.Parent.Parent == null)
                {
                    if (tree.SelectedNode.Parent.Name == "sequences")
                    {
                        seqPanel.BringToFront();
                        indexPanel.Show();
                        forceUniqueFilePanel.Show();
                        kermalisSoundPlayerPanel.Show();
                        seqPanel.Show();
                        SequenceInfo e = SA
                            .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault();
                        itemIndexBox.Value = e.Index;
                        forceUniqueFileBox.Checked = e.ForceIndividualFile;
                        PopulateBankBox(SA, seqBankComboBox);
                        SetBankIndex(
                            SA,
                            seqBankComboBox,
                            e.Bank == null ? e.ReadingBankId : (ushort)e.Bank.Index
                        );
                        seqBankBox.Value = e.Bank == null ? e.ReadingBankId : (ushort)e.Bank.Index;
                        seqVolumeBox.Value = e.Volume > 127 ? 127 : e.Volume;
                        seqChannelPriorityBox.Value = e.ChannelPriority;
                        seqPlayerPriorityBox.Value = e.PlayerPriority;
                        PopulatePlayerBox(SA, seqPlayerComboBox);
                        SetPlayerIndex(
                            SA,
                            seqPlayerComboBox,
                            e.Player == null ? e.ReadingPlayerId : (byte)e.Player.Index
                        );
                        seqPlayerBox.Value =
                            e.Player == null ? e.ReadingPlayerId : (byte)e.Player.Index;
                        status.Text =
                            "["
                            + e.Index
                            + "] "
                            + e.Name
                            + " Selected. File Is "
                            + GetBytesSize(e.File)
                            + ".";
                    }
                    else if (tree.SelectedNode.Parent.Name == "sequenceArchives")
                    {
                        kermalisSoundPlayerPanel.Hide();
                        seqArcPanel.BringToFront();
                        indexPanel.Show();
                        forceUniqueFilePanel.Show();
                        seqArcPanel.Show();
                        SequenceArchiveInfo e = SA
                            .SequenceArchives.Where(x =>
                                x.Index == GetIdFromNode(tree.SelectedNode)
                            )
                            .FirstOrDefault();
                        itemIndexBox.Value = e.Index;
                        forceUniqueFileBox.Checked = e.ForceIndividualFile;
                        status.Text =
                            "["
                            + e.Index
                            + "] "
                            + e.Name
                            + " Selected. File Is "
                            + GetBytesSize(e.File)
                            + ".";
                    }
                    else if (tree.SelectedNode.Parent.Name == "banks")
                    {
                        kermalisSoundPlayerPanel.Hide();
                        bankPanel.BringToFront();
                        indexPanel.Show();
                        forceUniqueFilePanel.Show();
                        bankPanel.Show();
                        BankInfo e = SA
                            .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault();
                        itemIndexBox.Value = e.Index;
                        forceUniqueFileBox.Checked = e.ForceIndividualFile;
                        PopulateWaveArchiveBox(SA, bnkWar0ComboBox);
                        PopulateWaveArchiveBox(SA, bnkWar1ComboBox);
                        PopulateWaveArchiveBox(SA, bnkWar2ComboBox);
                        PopulateWaveArchiveBox(SA, bnkWar3ComboBox);
                        SetWaveArchiveIndex(
                            SA,
                            bnkWar0ComboBox,
                            e.WaveArchives[0] == null
                                ? e.ReadingWave0Id
                                : (ushort)e.WaveArchives[0].Index
                        );
                        SetWaveArchiveIndex(
                            SA,
                            bnkWar1ComboBox,
                            e.WaveArchives[1] == null
                                ? e.ReadingWave1Id
                                : (ushort)e.WaveArchives[1].Index
                        );
                        SetWaveArchiveIndex(
                            SA,
                            bnkWar2ComboBox,
                            e.WaveArchives[2] == null
                                ? e.ReadingWave2Id
                                : (ushort)e.WaveArchives[2].Index
                        );
                        SetWaveArchiveIndex(
                            SA,
                            bnkWar3ComboBox,
                            e.WaveArchives[3] == null
                                ? e.ReadingWave3Id
                                : (ushort)e.WaveArchives[3].Index
                        );
                        SetWaveArchiveIndex(
                            SA,
                            bnkWar0Box,
                            e.WaveArchives[0] == null
                                ? e.ReadingWave0Id
                                : (ushort)e.WaveArchives[0].Index
                        );
                        SetWaveArchiveIndex(
                            SA,
                            bnkWar1Box,
                            e.WaveArchives[1] == null
                                ? e.ReadingWave1Id
                                : (ushort)e.WaveArchives[1].Index
                        );
                        SetWaveArchiveIndex(
                            SA,
                            bnkWar2Box,
                            e.WaveArchives[2] == null
                                ? e.ReadingWave2Id
                                : (ushort)e.WaveArchives[2].Index
                        );
                        SetWaveArchiveIndex(
                            SA,
                            bnkWar3Box,
                            e.WaveArchives[3] == null
                                ? e.ReadingWave3Id
                                : (ushort)e.WaveArchives[3].Index
                        );
                        status.Text =
                            "["
                            + e.Index
                            + "] "
                            + e.Name
                            + " Selected. File Is "
                            + GetBytesSize(e.File)
                            + ".";
                    }
                    else if (tree.SelectedNode.Parent.Name == "waveArchives")
                    {
                        kermalisSoundPlayerPanel.Hide();
                        warPanel.BringToFront();
                        indexPanel.Show();
                        forceUniqueFilePanel.Show();
                        warPanel.Show();
                        WaveArchiveInfo e = SA
                            .WaveArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault();
                        itemIndexBox.Value = e.Index;
                        forceUniqueFileBox.Checked = e.ForceIndividualFile;
                        loadIndividuallyBox.Checked = e.LoadIndividually;
                        status.Text =
                            "["
                            + e.Index
                            + "] "
                            + e.Name
                            + " Selected. File Is "
                            + GetBytesSize(e.File)
                            + ".";
                    }
                    else if (tree.SelectedNode.Parent.Name == "players")
                    {
                        kermalisSoundPlayerPanel.Hide();
                        playerPanel.BringToFront();
                        indexPanel.Show();
                        playerPanel.Show();
                        PlayerInfo e = SA
                            .Players.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault();
                        itemIndexBox.Value = e.Index;
                        playerMaxSequencesBox.Value = e.SequenceMax;
                        playerHeapSizeBox.Value = e.HeapSize;
                        playerFlag0Box.Checked = e.ChannelFlags[0];
                        playerFlag1Box.Checked = e.ChannelFlags[1];
                        playerFlag2Box.Checked = e.ChannelFlags[2];
                        playerFlag3Box.Checked = e.ChannelFlags[3];
                        playerFlag4Box.Checked = e.ChannelFlags[4];
                        playerFlag5Box.Checked = e.ChannelFlags[5];
                        playerFlag6Box.Checked = e.ChannelFlags[6];
                        playerFlag7Box.Checked = e.ChannelFlags[7];
                        playerFlag8Box.Checked = e.ChannelFlags[8];
                        playerFlag9Box.Checked = e.ChannelFlags[9];
                        playerFlag10Box.Checked = e.ChannelFlags[10];
                        playerFlag11Box.Checked = e.ChannelFlags[11];
                        playerFlag12Box.Checked = e.ChannelFlags[12];
                        playerFlag13Box.Checked = e.ChannelFlags[13];
                        playerFlag14Box.Checked = e.ChannelFlags[14];
                        playerFlag15Box.Checked = e.ChannelFlags[15];
                        status.Text = "[" + e.Index + "] " + e.Name + " Selected.";
                    }
                    else if (tree.SelectedNode.Parent.Name == "groups")
                    {
                        kermalisSoundPlayerPanel.Hide();
                        grpPanel.BringToFront();
                        indexPanel.Show();
                        grpPanel.Show();
                        GroupInfo e = SA
                            .Groups.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault();
                        itemIndexBox.Value = e.Index;
                        PopulateGroupGrid(grpEntries, e);
                        status.Text = "[" + e.Index + "] " + e.Name + " Selected.";
                    }
                    else if (tree.SelectedNode.Parent.Name == "streamPlayers")
                    {
                        kermalisSoundPlayerPanel.Hide();
                        streamPlayerPanel.BringToFront();
                        indexPanel.Show();
                        streamPlayerPanel.Show();
                        StreamPlayerInfo e = SA
                            .StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault();
                        itemIndexBox.Value = e.Index;
                        stmPlayerChannelType.SelectedIndex = e.IsStereo ? 1 : 0;
                        stmPlayerLeftChannelBox.Value = e.LeftChannel;
                        if (e.IsStereo)
                        {
                            leftChannelLabel.Text = "Channel:";
                            rightChannelLabel.Text = "Right Channel:";
                            stmPlayerRightChannelBox.Value = e.RightChannel;
                            rightChannelLabel.Enabled = true;
                            stmPlayerRightChannelBox.Enabled = true;
                        }
                        else
                        {
                            leftChannelLabel.Text = "Left Channel:";
                            rightChannelLabel.Text = "(Doesn't Exist)";
                            stmPlayerRightChannelBox.Value = 0;
                            rightChannelLabel.Enabled = false;
                            stmPlayerRightChannelBox.Enabled = false;
                        }
                        status.Text = "[" + e.Index + "] " + e.Name + " Selected.";
                    }
                    else if (tree.SelectedNode.Parent.Name == "streams")
                    {
                        kermalisSoundPlayerPanel.Hide();
                        stmPanel.BringToFront();
                        indexPanel.Show();
                        forceUniqueFilePanel.Show();
                        stmPanel.Show();
                        StreamInfo e = SA
                            .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault();
                        itemIndexBox.Value = e.Index;
                        forceUniqueFileBox.Checked = e.ForceIndividualFile;
                        stmMonoToStereoBox.Checked = e.MonoToStereo;
                        stmVolumeBox.Value = e.Volume;
                        stmPriorityBox.Value = e.Priority;
                        PopulateStreamPlayerBox(SA, stmPlayerComboBox);
                        SetStreamPlayerIndex(
                            SA,
                            stmPlayerComboBox,
                            e.Player == null ? e.ReadingPlayerId : (byte)e.Player.Index
                        );
                        stmPlayerBox.Value = e.Player == null ? e.ReadingPlayerId : e.Player.Index;
                        status.Text =
                            "["
                            + e.Index
                            + "] "
                            + e.Name
                            + " Selected. File Is "
                            + GetBytesSize(e.File)
                            + ".";
                    }
                }
                else
                {
                    indexPanel.Hide();
                    forceUniqueFilePanel.Hide();
                    indexPanel.SendToBack();
                    forceUniqueFilePanel.SendToBack();
                    blankPanel.BringToFront();
                    kermalisSoundPlayerPanel.Show();
                    blankPanel.Show();
                    SequenceArchiveSequence e = SA
                        .SequenceArchives.Where(x =>
                            x.Index == GetIdFromNode(tree.SelectedNode.Parent)
                        )
                        .FirstOrDefault()
                        .File.Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    status.Text = "[" + e.Index + "] " + e.Name + " Selected.";
                }
            }
            if (!panelSelected)
            {
                HideStuff();
                noInfoPanel.BringToFront();
                noInfoPanel.Show();
                status.Text = "No Valid Info Selected!";
            }
            WritingInfo = false;
        }

        public override void NodeMouseDoubleClick()
        {
            base.NodeMouseDoubleClick();
            if (tree.SelectedNode.Parent != null)
            {
                if (tree.SelectedNode.Parent == tree.Nodes["sequences"])
                {
                    SequenceInfo e = SA
                        .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    SequenceEditor ed = new(e.File, this, e.Name);
                    SetBankIndex(
                        SA,
                        ed.seqEditorBankComboBox,
                        e.Bank == null ? e.ReadingBankId : (uint)e.Bank.Index
                    );
                    ed.seqEditorBankBox.Value =
                        e.Bank == null ? e.ReadingBankId : (uint)e.Bank.Index;
                    ed.Show();
                }
                else if (tree.SelectedNode.Parent == tree.Nodes["sequenceArchives"])
                {
                    SequenceArchiveInfo e = SA
                        .SequenceArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    SequenceArchiveEditor ed = new(e.File, this, e.Name);
                    ed.Show();
                }
                else if (tree.SelectedNode.Parent == tree.Nodes["banks"])
                {
                    BankInfo e = SA
                        .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    BankEditor ed = new(e.File, this, e.Name);
                    SetWaveArchiveIndex(
                        SA,
                        ed.war0ComboBox,
                        e.WaveArchives[0] == null
                            ? e.ReadingWave0Id
                            : (ushort)e.WaveArchives[0].Index
                    );
                    SetWaveArchiveIndex(
                        SA,
                        ed.war1ComboBox,
                        e.WaveArchives[1] == null
                            ? e.ReadingWave1Id
                            : (ushort)e.WaveArchives[1].Index
                    );
                    SetWaveArchiveIndex(
                        SA,
                        ed.war2ComboBox,
                        e.WaveArchives[2] == null
                            ? e.ReadingWave2Id
                            : (ushort)e.WaveArchives[2].Index
                    );
                    SetWaveArchiveIndex(
                        SA,
                        ed.war3ComboBox,
                        e.WaveArchives[3] == null
                            ? e.ReadingWave3Id
                            : (ushort)e.WaveArchives[3].Index
                    );
                    SetWaveArchiveIndex(
                        SA,
                        ed.war0Box,
                        e.WaveArchives[0] == null
                            ? e.ReadingWave0Id
                            : (ushort)e.WaveArchives[0].Index
                    );
                    SetWaveArchiveIndex(
                        SA,
                        ed.war1Box,
                        e.WaveArchives[1] == null
                            ? e.ReadingWave1Id
                            : (ushort)e.WaveArchives[1].Index
                    );
                    SetWaveArchiveIndex(
                        SA,
                        ed.war2Box,
                        e.WaveArchives[2] == null
                            ? e.ReadingWave2Id
                            : (ushort)e.WaveArchives[2].Index
                    );
                    SetWaveArchiveIndex(
                        SA,
                        ed.war3Box,
                        e.WaveArchives[3] == null
                            ? e.ReadingWave3Id
                            : (ushort)e.WaveArchives[3].Index
                    );
                    ed.LoadWaveArchives();
                    ed.Show();
                }
                else if (tree.SelectedNode.Parent == tree.Nodes["waveArchives"])
                {
                    WaveArchiveInfo e = SA
                        .WaveArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    WaveArchiveEditor ed = new(e.File, this, e.Name);
                    ed.Show();
                }
                else if (tree.SelectedNode.Parent == tree.Nodes["streams"])
                {
                    StreamInfo s = SA
                        .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    RiffWave r = new();
                    r.FromOtherStreamFile(s.File);
                    r.Write(MainWindow.NitroPath + "/" + "tmpStream" + StreamTempCount++ + ".wav");
                    StreamPlayer p = new(
                        this,
                        MainWindow.NitroPath + "/" + "tmpStream" + (StreamTempCount - 1) + ".wav",
                        s.Name
                    );
                    p.Show();
                }
            }
        }

        public static int GetIdFromNode(TreeNode n)
        {
            return int.Parse(n.Text.Split('[')[1].Split(']')[0]);
        }

        public void WriteNamesChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA.SaveSymbols = writeNamesBox.Checked;
            }
        }

        public static string GetBytesSize(IOFile f)
        {
            long byteCount = f.Write().Length;
            string[] suf = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
            {
                return "0" + suf[0];
            }

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }

        public void SwapAtIndexButtonPressed(object sender, EventArgs e)
        {
            int index = (int)itemIndexBox.Value;
            int bakIndex = GetIdFromNode(tree.SelectedNode);
            switch (tree.SelectedNode.Parent.Name)
            {
                case "sequences":
                    if ((uint)index > SoundArchive.MaxSequenceId)
                    {
                        _ = MessageBox.Show("Index is outside the max possible Id!");
                    }
                    SequenceInfo prevSeq = SA.Sequences.Where(x => x.Index == index).FirstOrDefault();
                    SA.Sequences.Where(x => x.Index == bakIndex).FirstOrDefault().Index = index;
                    _ = prevSeq?.Index = bakIndex;
                    SA.Sequences = SA.Sequences.OrderBy(x => x.Index).ToList();
                    UpdateNodes();
                    foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                    {
                        if (n.Text.Contains("[" + index + "]"))
                        {
                            tree.SelectedNode = n;
                        }
                    }
                    DoInfoStuff();
                    break;
                case "sequenceArchives":
                    if ((uint)index > SoundArchive.MaxSequenceArchiveId)
                    {
                        _ = MessageBox.Show("Index is outside the max possible Id!");
                    }
                    SequenceArchiveInfo prevSeqArc = SA
                        .SequenceArchives.Where(x => x.Index == index)
                        .FirstOrDefault();
                    SA.SequenceArchives.Where(x => x.Index == bakIndex).FirstOrDefault().Index =
                        index;
                    _ = prevSeqArc?.Index = bakIndex;
                    SA.SequenceArchives = SA.SequenceArchives.OrderBy(x => x.Index).ToList();
                    UpdateNodes();
                    foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                    {
                        if (n.Text.Contains("[" + index + "]"))
                        {
                            tree.SelectedNode = n;
                        }
                    }
                    DoInfoStuff();
                    break;
                case "banks":
                    if ((uint)index > SoundArchive.MaxBankId)
                    {
                        _ = MessageBox.Show("Index is outside the max possible Id!");
                    }
                    BankInfo prevBnk = SA.Banks.Where(x => x.Index == index).FirstOrDefault();
                    SA.Banks.Where(x => x.Index == bakIndex).FirstOrDefault().Index = index;
                    _ = prevBnk?.Index = bakIndex;
                    SA.Banks = SA.Banks.OrderBy(x => x.Index).ToList();
                    UpdateNodes();
                    foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                    {
                        if (n.Text.Contains("[" + index + "]"))
                        {
                            tree.SelectedNode = n;
                        }
                    }
                    DoInfoStuff();
                    break;
                case "waveArchives":
                    if ((uint)index > SoundArchive.MaxWaveArchiveId)
                    {
                        _ = MessageBox.Show("Index is outside the max possible Id!");
                    }
                    WaveArchiveInfo prevWar = SA.WaveArchives.Where(x => x.Index == index).FirstOrDefault();
                    SA.WaveArchives.Where(x => x.Index == bakIndex).FirstOrDefault().Index = index;
                    _ = prevWar?.Index = bakIndex;
                    SA.WaveArchives = SA.WaveArchives.OrderBy(x => x.Index).ToList();
                    UpdateNodes();
                    foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                    {
                        if (n.Text.Contains("[" + index + "]"))
                        {
                            tree.SelectedNode = n;
                        }
                    }
                    DoInfoStuff();
                    break;
                case "players":
                    if ((uint)index > SoundArchive.MaxPlayerId)
                    {
                        _ = MessageBox.Show("Index is outside the max possible Id!");
                    }
                    PlayerInfo prevPly = SA.Players.Where(x => x.Index == index).FirstOrDefault();
                    SA.Players.Where(x => x.Index == bakIndex).FirstOrDefault().Index = index;
                    _ = prevPly?.Index = bakIndex;
                    SA.Players = SA.Players.OrderBy(x => x.Index).ToList();
                    UpdateNodes();
                    foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                    {
                        if (n.Text.Contains("[" + index + "]"))
                        {
                            tree.SelectedNode = n;
                        }
                    }
                    DoInfoStuff();
                    break;
                case "groups":
                    if ((uint)index > SoundArchive.MaxGroupId)
                    {
                        _ = MessageBox.Show("Index is outside the max possible Id!");
                    }
                    GroupInfo prevGrp = SA.Groups.Where(x => x.Index == index).FirstOrDefault();
                    SA.Groups.Where(x => x.Index == bakIndex).FirstOrDefault().Index = index;
                    _ = prevGrp?.Index = bakIndex;
                    SA.Groups = SA.Groups.OrderBy(x => x.Index).ToList();
                    UpdateNodes();
                    foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                    {
                        if (n.Text.Contains("[" + index + "]"))
                        {
                            tree.SelectedNode = n;
                        }
                    }
                    DoInfoStuff();
                    break;
                case "streamPlayers":
                    if ((uint)index > SoundArchive.MaxStreamPlayerId)
                    {
                        _ = MessageBox.Show("Index is outside the max possible Id!");
                    }
                    StreamPlayerInfo prevStmPly = SA.StreamPlayers.Where(x => x.Index == index).FirstOrDefault();
                    SA.StreamPlayers.Where(x => x.Index == bakIndex).FirstOrDefault().Index = index;
                    _ = prevStmPly?.Index = bakIndex;
                    SA.StreamPlayers = SA.StreamPlayers.OrderBy(x => x.Index).ToList();
                    UpdateNodes();
                    foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                    {
                        if (n.Text.Contains("[" + index + "]"))
                        {
                            tree.SelectedNode = n;
                        }
                    }
                    DoInfoStuff();
                    break;
                case "streams":
                    if ((uint)index > SoundArchive.MaxStreamId)
                    {
                        _ = MessageBox.Show("Index is outside the max possible Id!");
                    }
                    StreamInfo prevStm = SA.Streams.Where(x => x.Index == index).FirstOrDefault();
                    SA.Streams.Where(x => x.Index == bakIndex).FirstOrDefault().Index = index;
                    _ = prevStm?.Index = bakIndex;
                    SA.Streams = SA.Streams.OrderBy(x => x.Index).ToList();
                    UpdateNodes();
                    foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                    {
                        if (n.Text.Contains("[" + index + "]"))
                        {
                            tree.SelectedNode = n;
                        }
                    }
                    DoInfoStuff();
                    break;
            }
        }

        public void ForceUniqueIdChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                switch (tree.SelectedNode.Parent.Name)
                {
                    case "sequences":
                        SA
                            .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault()
                            .ForceIndividualFile = forceUniqueFileBox.Checked;
                        break;
                    case "sequenceArchives":
                        SA
                            .SequenceArchives.Where(x =>
                                x.Index == GetIdFromNode(tree.SelectedNode)
                            )
                            .FirstOrDefault()
                            .ForceIndividualFile = forceUniqueFileBox.Checked;
                        break;
                    case "banks":
                        SA
                            .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault()
                            .ForceIndividualFile = forceUniqueFileBox.Checked;
                        break;
                    case "waveArchives":
                        SA
                            .WaveArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault()
                            .ForceIndividualFile = forceUniqueFileBox.Checked;
                        break;
                    case "streams":
                        SA
                            .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault()
                            .ForceIndividualFile = forceUniqueFileBox.Checked;
                        break;
                }
            }
        }

        public void WarLoadIndividualChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .WaveArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .LoadIndividually = loadIndividuallyBox.Checked;
            }
        }

        public static void PopulateWaveArchiveBox(SoundArchive a, ComboBox c)
        {
            c.Items.Clear();
            _ = c.Items.Add("FFFF - Blank");
            _ = c.Items.Add("Other Index");
            foreach (WaveArchiveInfo w in a.WaveArchives)
            {
                _ = c.Items.Add("[" + w.Index + "] - " + w.Name);
            }
        }

        public static void SetWaveArchiveIndex(SoundArchive a, ComboBox c, ushort id)
        {
            WaveArchiveInfo e = a.WaveArchives.Where(x => x.Index == id).FirstOrDefault();
            if (e == null)
            {
                c.SelectedIndex = id == 0xFFFF ? 0 : 1;
            }
            else
            {
                c.SelectedItem = "[" + e.Index + "] - " + e.Name;
            }
        }

        public static void SetWaveArchiveIndex(SoundArchive a, NumericUpDown n, ushort id)
        {
            n.Value = id == 0xFFFF ? -1 : id;
        }

        public void BnkWar0BoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingWave0Id = (ushort)(bnkWar0Box.Value == -1 ? 0xFFFF : bnkWar0Box.Value);
                WritingInfo = true;
                SetWaveArchiveIndex(
                    SA,
                    bnkWar0ComboBox,
                    (ushort)(bnkWar0Box.Value == -1 ? 0xFFFF : bnkWar0Box.Value)
                );
                WritingInfo = false;
                SetNewWaveArchiveInBank(SA, GetIdFromNode(tree.SelectedNode), 0);
            }
        }

        public void BnkWar1BoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingWave1Id = (ushort)(bnkWar1Box.Value == -1 ? 0xFFFF : bnkWar1Box.Value);
                WritingInfo = true;
                SetWaveArchiveIndex(
                    SA,
                    bnkWar1ComboBox,
                    (ushort)(bnkWar1Box.Value == -1 ? 0xFFFF : bnkWar1Box.Value)
                );
                WritingInfo = false;
                SetNewWaveArchiveInBank(SA, GetIdFromNode(tree.SelectedNode), 1);
            }
        }

        public void BnkWar2BoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingWave2Id = (ushort)(bnkWar2Box.Value == -1 ? 0xFFFF : bnkWar2Box.Value);
                WritingInfo = true;
                SetWaveArchiveIndex(
                    SA,
                    bnkWar2ComboBox,
                    (ushort)(bnkWar2Box.Value == -1 ? 0xFFFF : bnkWar2Box.Value)
                );
                WritingInfo = false;
                SetNewWaveArchiveInBank(SA, GetIdFromNode(tree.SelectedNode), 2);
            }
        }

        public void BnkWar3BoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingWave3Id = (ushort)(bnkWar3Box.Value == -1 ? 0xFFFF : bnkWar3Box.Value);
                WritingInfo = true;
                SetWaveArchiveIndex(
                    SA,
                    bnkWar3ComboBox,
                    (ushort)(bnkWar3Box.Value == -1 ? 0xFFFF : bnkWar3Box.Value)
                );
                WritingInfo = false;
                SetNewWaveArchiveInBank(SA, GetIdFromNode(tree.SelectedNode), 3);
            }
        }

        public void BnkWar0ComboBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                ushort val = (ushort)bnkWar0ComboBox.SelectedIndex;
                if (val == 0)
                {
                    val = 0xFFFF;
                }
                else if (val == 1)
                {
                    return;
                }
                else
                {
                    val = ushort.Parse(
                        ((string)bnkWar0ComboBox.SelectedItem).Split('[')[1].Split(']')[0]
                    );
                }
                SA
                    .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingWave0Id = val;
                WritingInfo = true;
                SetWaveArchiveIndex(SA, bnkWar0Box, val);
                WritingInfo = false;
                SetNewWaveArchiveInBank(SA, GetIdFromNode(tree.SelectedNode), 0);
            }
        }

        public void BnkWar1ComboBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                ushort val = (ushort)bnkWar1ComboBox.SelectedIndex;
                if (val == 0)
                {
                    val = 0xFFFF;
                }
                else if (val == 1)
                {
                    return;
                }
                else
                {
                    val = ushort.Parse(
                        ((string)bnkWar1ComboBox.SelectedItem).Split('[')[1].Split(']')[0]
                    );
                }
                SA
                    .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingWave1Id = val;
                WritingInfo = true;
                SetWaveArchiveIndex(SA, bnkWar1Box, val);
                WritingInfo = false;
                SetNewWaveArchiveInBank(SA, GetIdFromNode(tree.SelectedNode), 1);
            }
        }

        public void BnkWar2ComboBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                ushort val = (ushort)bnkWar2ComboBox.SelectedIndex;
                if (val == 0)
                {
                    val = 0xFFFF;
                }
                else if (val == 1)
                {
                    return;
                }
                else
                {
                    val = ushort.Parse(
                        ((string)bnkWar2ComboBox.SelectedItem).Split('[')[1].Split(']')[0]
                    );
                }
                SA
                    .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingWave2Id = val;
                WritingInfo = true;
                SetWaveArchiveIndex(SA, bnkWar2Box, val);
                WritingInfo = false;
                SetNewWaveArchiveInBank(SA, GetIdFromNode(tree.SelectedNode), 2);
            }
        }

        public void BnkWar3ComboBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                ushort val = (ushort)bnkWar3ComboBox.SelectedIndex;
                if (val == 0)
                {
                    val = 0xFFFF;
                }
                else if (val == 1)
                {
                    return;
                }
                else
                {
                    val = ushort.Parse(
                        ((string)bnkWar3ComboBox.SelectedItem).Split('[')[1].Split(']')[0]
                    );
                }
                SA
                    .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingWave3Id = val;
                WritingInfo = true;
                SetWaveArchiveIndex(SA, bnkWar3Box, val);
                WritingInfo = false;
                SetNewWaveArchiveInBank(SA, GetIdFromNode(tree.SelectedNode), 3);
            }
        }

        public static void SetNewWaveArchiveInBank(SoundArchive s, int bankId, int warId)
        {
            BankInfo b = s.Banks.Where(x => x.Index == bankId).FirstOrDefault();
            switch (warId)
            {
                case 0:
                    b.WaveArchives[warId] = s
                        .WaveArchives.Where(x => x.Index == b.ReadingWave0Id)
                        .FirstOrDefault();
                    break;
                case 1:
                    b.WaveArchives[warId] = s
                        .WaveArchives.Where(x => x.Index == b.ReadingWave1Id)
                        .FirstOrDefault();
                    break;
                case 2:
                    b.WaveArchives[warId] = s
                        .WaveArchives.Where(x => x.Index == b.ReadingWave2Id)
                        .FirstOrDefault();
                    break;
                case 3:
                    b.WaveArchives[warId] = s
                        .WaveArchives.Where(x => x.Index == b.ReadingWave3Id)
                        .FirstOrDefault();
                    break;
            }
        }

        public void PopulateGroupGrid(DataGridView v, GroupInfo g)
        {
            v.Rows.Clear();
            DataGridViewComboBoxColumn c = v.Columns[0] as DataGridViewComboBoxColumn;
            c.Items.Clear();
            foreach (SequenceInfo e in SA.Sequences)
            {
                _ = c.Items.Add("[" + e.Index + "] " + e.Name + " (Sequence)");
            }
            foreach (SequenceArchiveInfo e in SA.SequenceArchives)
            {
                _ = c.Items.Add("[" + e.Index + "] " + e.Name + " (Sequence Archive)");
            }
            foreach (BankInfo e in SA.Banks)
            {
                _ = c.Items.Add("[" + e.Index + "] " + e.Name + " (Bank)");
            }
            foreach (WaveArchiveInfo e in SA.WaveArchives)
            {
                _ = c.Items.Add("[" + e.Index + "] " + e.Name + " (Wave Archive)");
            }
            foreach (GroupEntry e in g.Entries)
            {
                _ = v.Rows.Add(new DataGridViewRow());
                switch (e.Type)
                {
                    case GroupEntryType.Sequence:
                        ((DataGridViewComboBoxCell)v.Rows[^2].Cells[0]).Value =
                            "["
                            + (e.Entry as SequenceInfo).Index
                            + "] "
                            + (e.Entry as SequenceInfo).Name
                            + " (Sequence)";
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Sequence"
                        );
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Bank"
                        );
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Wave Archive"
                        );
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Sequence + Bank"
                        );
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Sequence + Wave Archive"
                        );
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Bank + Wave Archive"
                        );
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Sequence + Bank + Wave Archive"
                        );
                        ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Value = e.LoadSequence && e.LoadBank && e.LoadWaveArchive
                            ? "Sequence + Bank + Wave Archive"
                            : e.LoadBank && e.LoadWaveArchive
                                ? "Bank + Wave Archive"
                                : e.LoadSequence && e.LoadWaveArchive
                                ? "Sequence + Wave Archive"
                                : e.LoadSequence && e.LoadBank ? "Sequence + Bank" : e.LoadWaveArchive ? "Wave Archive" : e.LoadBank ? "Bank" : "Sequence";
                        break;
                    case GroupEntryType.SequenceArchive:
                        ((DataGridViewComboBoxCell)v.Rows[^2].Cells[0]).Value =
                            "["
                            + (e.Entry as SequenceArchiveInfo).Index
                            + "] "
                            + (e.Entry as SequenceArchiveInfo).Name
                            + " (Sequence Archive)";
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Sequence Archive"
                        );
                        ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Value =
                            "Sequence Archive";
                        break;
                    case GroupEntryType.Bank:
                        ((DataGridViewComboBoxCell)v.Rows[^2].Cells[0]).Value =
                            "["
                            + (e.Entry as BankInfo).Index
                            + "] "
                            + (e.Entry as BankInfo).Name
                            + " (Bank)";
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Bank"
                        );
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Wave Archive"
                        );
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Bank + Wave Archive"
                        );
                        ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Value = e.LoadBank && e.LoadWaveArchive ? "Bank + Wave Archive" : e.LoadWaveArchive ? "Wave Archive" : "Bank";
                        break;
                    case GroupEntryType.WaveArchive:
                        ((DataGridViewComboBoxCell)v.Rows[^2].Cells[0]).Value =
                            "["
                            + (e.Entry as WaveArchiveInfo).Index
                            + "] "
                            + (e.Entry as WaveArchiveInfo).Name
                            + " (Wave Archive)";
                        _ = ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Items.Add(
                            "Wave Archive"
                        );
                        ((DataGridViewComboBoxCell)v.Rows[^2].Cells[1]).Value =
                            "Wave Archive";
                        break;
                }
            }
        }

        public void GroupEntriesChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                WritingInfo = true;
                List<GroupEntry> entries = [];
                for (int i = 1; i < grpEntries.Rows.Count; i++)
                {
                    DataGridViewComboBoxCell itemCell = (DataGridViewComboBoxCell)grpEntries.Rows[i - 1].Cells[0];
                    DataGridViewComboBoxCell flagsCell = (DataGridViewComboBoxCell)grpEntries.Rows[i - 1].Cells[1];
                    GroupEntryType t = GroupEntryType.WaveArchive;
                    object entry = null;
                    uint readingId = 0;
                    bool loadWar = false;
                    bool loadBnk = false;
                    bool loadSeqArc = false;
                    bool loadSeq = false;
                    string bakFlags = "";
                    try
                    {
                        bakFlags = (string)flagsCell.Value;
                    }
                    catch { }
                    try
                    {
                        flagsCell.Value = flagsCell.Items[0];
                    }
                    catch
                    {
                        bakFlags = "";
                    }
                    while (flagsCell.Items.Count > 1)
                    {
                        flagsCell.Items.RemoveAt(flagsCell.Items.Count - 1);
                    }
                    switch (((string)itemCell.Value).Split('(')[1].Split(')')[0])
                    {
                        case "Sequence":
                            t = GroupEntryType.Sequence;
                            entry = SA
                                .Sequences.Where(x =>
                                    x.Index
                                    == int.Parse(
                                        ((string)itemCell.Value).Split('[')[1].Split(']')[0]
                                    )
                                )
                                .FirstOrDefault();
                            readingId = (uint)(entry as SequenceInfo).Index;
                            if (flagsCell.Items.Count < 1)
                            {
                                _ = flagsCell.Items.Add("Sequence");
                            }
                            else
                            {
                                flagsCell.Items[0] = "Sequence";
                            }
                            _ = flagsCell.Items.Add("Bank");
                            _ = flagsCell.Items.Add("Wave Archive");
                            _ = flagsCell.Items.Add("Sequence + Bank");
                            _ = flagsCell.Items.Add("Sequence + Wave Archive");
                            _ = flagsCell.Items.Add("Bank + Wave Archive");
                            _ = flagsCell.Items.Add("Sequence + Bank + Wave Archive");
                            break;
                        case "Sequence Archive":
                            t = GroupEntryType.SequenceArchive;
                            entry = SA
                                .SequenceArchives.Where(x =>
                                    x.Index
                                    == int.Parse(
                                        ((string)itemCell.Value).Split('[')[1].Split(']')[0]
                                    )
                                )
                                .FirstOrDefault();
                            readingId = (uint)(entry as SequenceArchiveInfo).Index;
                            if (flagsCell.Items.Count < 1)
                            {
                                _ = flagsCell.Items.Add("Sequence Archive");
                            }
                            else
                            {
                                flagsCell.Items[0] = "Sequence Archive";
                            }
                            break;
                        case "Bank":
                            t = GroupEntryType.Bank;
                            entry = SA
                                .Banks.Where(x =>
                                    x.Index
                                    == int.Parse(
                                        ((string)itemCell.Value).Split('[')[1].Split(']')[0]
                                    )
                                )
                                .FirstOrDefault();
                            readingId = (uint)(entry as BankInfo).Index;
                            if (flagsCell.Items.Count < 1)
                            {
                                _ = flagsCell.Items.Add("Bank");
                            }
                            else
                            {
                                flagsCell.Items[0] = "Bank";
                            }
                            _ = flagsCell.Items.Add("Wave Archive");
                            _ = flagsCell.Items.Add("Bank + Wave Archive");
                            break;
                        case "Wave Archive":
                            t = GroupEntryType.WaveArchive;
                            entry = SA
                                .WaveArchives.Where(x =>
                                    x.Index
                                    == int.Parse(
                                        ((string)itemCell.Value).Split('[')[1].Split(']')[0]
                                    )
                                )
                                .FirstOrDefault();
                            readingId = (uint)(entry as WaveArchiveInfo).Index;
                            if (flagsCell.Items.Count < 1)
                            {
                                _ = flagsCell.Items.Add("Wave Archive");
                            }
                            else
                            {
                                flagsCell.Items[0] = "Wave Archive";
                            }
                            break;
                    }
                    flagsCell.Value = flagsCell.Items.Contains(bakFlags) ? bakFlags : flagsCell.Items[0];
                    loadSeq = ((string)flagsCell.Value).Contains("Sequence");
                    loadSeqArc = ((string)flagsCell.Value).Contains("Sequence Archive");
                    loadBnk = ((string)flagsCell.Value).Contains("Bank");
                    loadWar = ((string)flagsCell.Value).Contains("Wave Archive");
                    entries.Add(
                        new GroupEntry()
                        {
                            Type = t,
                            Entry = entry,
                            ReadingId = readingId,
                            LoadSequence = loadSeq,
                            LoadSequenceArchive = loadSeqArc,
                            LoadBank = loadBnk,
                            LoadWaveArchive = loadWar,
                        }
                    );
                }
                SA
                    .Groups.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .Entries = entries;
                WritingInfo = false;
            }
        }

        public void StreamPlayerTypeChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                WritingInfo = true;
                if (stmPlayerChannelType.SelectedIndex == 0)
                {
                    leftChannelLabel.Text = "Channel:";
                    rightChannelLabel.Text = "(Doesn't Exist)";
                    stmPlayerRightChannelBox.Value = 0;
                    rightChannelLabel.Enabled = false;
                    stmPlayerRightChannelBox.Enabled = false;
                }
                else
                {
                    leftChannelLabel.Text = "Left Channel:";
                    rightChannelLabel.Text = "Right Channel:";
                    rightChannelLabel.Enabled = true;
                    stmPlayerRightChannelBox.Enabled = true;
                    if (
                        SA.StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault()
                            .LeftChannel != 15
                    )
                    {
                        SA
                            .StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                            .FirstOrDefault()
                            .RightChannel = (byte)(
                            SA.StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                                .FirstOrDefault()
                                .LeftChannel + 1
                        );
                    }
                    stmPlayerRightChannelBox.Value = SA
                        .StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault()
                        .RightChannel;
                }
                WritingInfo = false;
                SA
                    .StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .IsStereo = stmPlayerChannelType.SelectedIndex == 1;
            }
        }

        public void StreamPlayerLeftChannelChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .LeftChannel = (byte)stmPlayerLeftChannelBox.Value;
            }
        }

        public void StreamPlayerRightChannelChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .RightChannel = (byte)stmPlayerRightChannelBox.Value;
            }
        }

        public static void PopulateStreamPlayerBox(SoundArchive a, ComboBox c)
        {
            c.Items.Clear();
            _ = c.Items.Add("Other Index");
            foreach (StreamPlayerInfo w in a.StreamPlayers)
            {
                _ = c.Items.Add("[" + w.Index + "] - " + w.Name);
            }
        }

        public static void SetStreamPlayerIndex(SoundArchive a, ComboBox c, byte id)
        {
            StreamPlayerInfo e = a.StreamPlayers.Where(x => x.Index == id).FirstOrDefault();
            if (e == null)
            {
                c.SelectedIndex = 0;
            }
            else
            {
                c.SelectedItem = "[" + e.Index + "] - " + e.Name;
            }
        }

        public void StreamVolumeChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .Volume = (byte)stmVolumeBox.Value;
            }
        }

        public void StreamPriorityChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .Priority = (byte)stmPriorityBox.Value;
            }
        }

        public void StreamMonoToStereoChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .MonoToStereo = stmMonoToStereoBox.Checked;
            }
        }

        public void StreamPlayerComboBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                if (stmPlayerComboBox.SelectedIndex != 0)
                {
                    WritingInfo = true;
                    byte index = byte.Parse(
                        ((string)stmPlayerComboBox.SelectedItem).Split('[')[1].Split(']')[0]
                    );
                    stmPlayerBox.Value = index;
                    SA
                        .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault()
                        .ReadingPlayerId = index;
                    SA
                        .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault()
                        .Player = SA.StreamPlayers.Where(x => x.Index == index).FirstOrDefault();
                    WritingInfo = false;
                }
            }
        }

        public void StreamPlayerBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                WritingInfo = true;
                SetStreamPlayerIndex(SA, stmPlayerComboBox, (byte)stmPlayerBox.Value);
                SA
                    .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .Player = SA
                    .StreamPlayers.Where(x => x.Index == (byte)stmPlayerBox.Value)
                    .FirstOrDefault();
                SA
                    .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingPlayerId = (byte)stmPlayerBox.Value;
                WritingInfo = false;
            }
        }

        public void PlayerSequenceMaxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Players.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .SequenceMax = (ushort)playerMaxSequencesBox.Value;
            }
        }

        public void PlayerHeapSizeChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Players.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .HeapSize = (uint)playerHeapSizeBox.Value;
            }
        }

        public void PlayerFlagsChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                PlayerInfo p = SA
                    .Players.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault();
                p.ChannelFlags[0] = playerFlag0Box.Checked;
                p.ChannelFlags[1] = playerFlag1Box.Checked;
                p.ChannelFlags[2] = playerFlag2Box.Checked;
                p.ChannelFlags[3] = playerFlag3Box.Checked;
                p.ChannelFlags[4] = playerFlag4Box.Checked;
                p.ChannelFlags[5] = playerFlag5Box.Checked;
                p.ChannelFlags[6] = playerFlag6Box.Checked;
                p.ChannelFlags[7] = playerFlag7Box.Checked;
                p.ChannelFlags[8] = playerFlag8Box.Checked;
                p.ChannelFlags[9] = playerFlag9Box.Checked;
                p.ChannelFlags[10] = playerFlag10Box.Checked;
                p.ChannelFlags[11] = playerFlag11Box.Checked;
                p.ChannelFlags[12] = playerFlag12Box.Checked;
                p.ChannelFlags[13] = playerFlag13Box.Checked;
                p.ChannelFlags[14] = playerFlag14Box.Checked;
                p.ChannelFlags[15] = playerFlag15Box.Checked;
            }
        }

        public static void PopulateBankBox(SoundArchive a, ComboBox c)
        {
            c.Items.Clear();
            _ = c.Items.Add("Other Index");
            foreach (BankInfo w in a.Banks)
            {
                _ = c.Items.Add("[" + w.Index + "] - " + w.Name);
            }
        }

        public static void SetBankIndex(SoundArchive a, ComboBox c, uint id)
        {
            BankInfo e = a.Banks.Where(x => x.Index == id).FirstOrDefault();
            if (e == null)
            {
                c.SelectedIndex = 0;
            }
            else
            {
                c.SelectedItem = "[" + e.Index + "] - " + e.Name;
            }
        }

        public static void PopulatePlayerBox(SoundArchive a, ComboBox c)
        {
            c.Items.Clear();
            _ = c.Items.Add("Other Index");
            foreach (PlayerInfo w in a.Players)
            {
                _ = c.Items.Add("[" + w.Index + "] - " + w.Name);
            }
        }

        public static void SetPlayerIndex(SoundArchive a, ComboBox c, byte id)
        {
            PlayerInfo e = a.Players.Where(x => x.Index == id).FirstOrDefault();
            if (e == null)
            {
                c.SelectedIndex = 0;
            }
            else
            {
                c.SelectedItem = "[" + e.Index + "] - " + e.Name;
            }
        }

        public void SequenceVolumeChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .Volume = (byte)seqVolumeBox.Value;
            }
        }

        public void SequenceChannelPriorityChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ChannelPriority = (byte)seqChannelPriorityBox.Value;
            }
        }

        public void SequencePlayerPriorityChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                SA
                    .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .PlayerPriority = (byte)seqPlayerPriorityBox.Value;
            }
        }

        public void SequenceBankComboBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                if (seqBankComboBox.SelectedIndex != 0)
                {
                    WritingInfo = true;
                    ushort index = ushort.Parse(
                        ((string)seqBankComboBox.SelectedItem).Split('[')[1].Split(']')[0]
                    );
                    seqBankBox.Value = index;
                    SA
                        .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault()
                        .ReadingBankId = index;
                    SA
                        .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault()
                        .Bank = SA.Banks.Where(x => x.Index == index).FirstOrDefault();
                    WritingInfo = false;
                }
            }
        }

        public void SequenceBankBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                WritingInfo = true;
                SetBankIndex(SA, seqBankComboBox, (ushort)seqBankBox.Value);
                SA
                    .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .Bank = SA
                    .Banks.Where(x => x.Index == (ushort)seqBankBox.Value)
                    .FirstOrDefault();
                SA
                    .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingBankId = (ushort)seqBankBox.Value;
                WritingInfo = false;
            }
        }

        public void SequencePlayerComboBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                if (seqPlayerComboBox.SelectedIndex != 0)
                {
                    WritingInfo = true;
                    byte index = byte.Parse(
                        ((string)seqPlayerComboBox.SelectedItem).Split('[')[1].Split(']')[0]
                    );
                    seqPlayerBox.Value = index;
                    SA
                        .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault()
                        .ReadingPlayerId = index;
                    SA
                        .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault()
                        .Player = SA.Players.Where(x => x.Index == index).FirstOrDefault();
                    WritingInfo = false;
                }
            }
        }

        public void SequencePlayerBoxChanged(object sender, EventArgs e)
        {
            if (FileOpen && File != null && !WritingInfo)
            {
                WritingInfo = true;
                SetPlayerIndex(SA, seqPlayerComboBox, (byte)seqPlayerBox.Value);
                SA
                    .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .Player = SA
                    .Players.Where(x => x.Index == (byte)seqPlayerBox.Value)
                    .FirstOrDefault();
                SA
                    .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault()
                    .ReadingPlayerId = (byte)seqPlayerBox.Value;
                WritingInfo = false;
            }
        }

        public void PlayClick(object sender, EventArgs e)
        {
            if (tree.SelectedNode.Parent.Name == "sequences")
            {
                SequenceInfo s = SA
                    .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault();
                try
                {
                    Player.PrepareForSong(
                        new PlayableBank[] { s.Bank.File },
                        s.Bank.GetAssociatedWaves()
                    );
                }
                catch
                {
                    _ = MessageBox.Show("Sequence entry has no valid bank hooked up to it!");
                    return;
                }
                s.File.ReadCommandData();
                Player.LoadSong(s.File.Commands);
                kermalisPosition.Maximum = (int)Player.MaxTicks;
                kermalisPosition.TickFrequency = kermalisPosition.Maximum / 10;
                kermalisPosition.LargeChange = kermalisPosition.Maximum / 20;
                Player.Play();
            }
            else
            {
                SequenceArchiveInfo a = SA
                    .SequenceArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode.Parent))
                    .FirstOrDefault();
                SequenceArchiveSequence s = a
                    .File.Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                    .FirstOrDefault();
                try
                {
                    Player.PrepareForSong(
                        new PlayableBank[] { s.Bank.File },
                        s.Bank.GetAssociatedWaves()
                    );
                }
                catch
                {
                    _ = MessageBox.Show("Sequence Archive entry has no valid bank hooked up to it!");
                    return;
                }
                a.File.ReadCommandData(true);
                Player.LoadSong(
                    a.File.Commands,
                    a.File.PublicLabels.Values.ElementAt(a.File.Sequences.IndexOf(s))
                );
                kermalisPosition.Maximum = (int)Player.MaxTicks;
                kermalisPosition.TickFrequency = kermalisPosition.Maximum / 10;
                kermalisPosition.LargeChange = kermalisPosition.Maximum / 20;
                Player.Play();
            }
        }

        public void PositionTick(object sender, EventArgs e)
        {
            if (Player != null && PositionBarFree)
            {
                kermalisPosition.Value =
                    Player.GetCurrentPosition() > kermalisPosition.Maximum
                        ? kermalisPosition.Maximum
                        : (int)Player.GetCurrentPosition();
            }
        }

        public void PositionMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PositionBarFree = false;
            }
        }

        public void PositionMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Player != null && Player.Events != null)
            {
                Player.SetCurrentPosition(kermalisPosition.Value);
                PositionBarFree = true;
            }
        }

        public void PauseClick(object sender, EventArgs e)
        {
            Player.Pause();
        }

        public void StopClick(object sender, EventArgs e)
        {
            Player.Stop();
        }

        public void VolumeChanged(object sender, EventArgs e)
        {
            Mixer.Volume = kermalisVolumeSlider.Value / 100f;
        }

        public void LoopChanged(object sender, EventArgs e)
        {
            Player.NumLoops = kermalisLoopBox.Checked ? 0xFFFFFFFF : 0;
        }

        public void SAClosing(object sender, FormClosingEventArgs e)
        {
            Player.Stop();
            Player.Dispose();
            Mixer.Dispose();
            Timer.Stop();
            Environment.Exit(Environment.ExitCode);
        }

        public new void KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ' && tree.SelectedNode.Parent != null)
            {
                if (
                    tree.SelectedNode.Parent.Parent != null
                    || tree.SelectedNode.Parent.Name == "sequences"
                )
                {
                    PlayClick(sender, e);
                }
            }
        }

        public void AddAbove(object sender, EventArgs e)
        {
            if (tree.SelectedNode.Parent.Name.Equals("sequences"))
            {
                int ind = GetNextAvailablePreviousId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxSequenceId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequence(ind);
                SA.Sequences = SA.Sequences.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("sequenceArchives"))
            {
                int ind = GetNextAvailablePreviousId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxSequenceArchiveId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequenceArchive(ind);
                SA.SequenceArchives = SA.SequenceArchives.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("banks"))
            {
                int ind = GetNextAvailablePreviousId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxBankId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddBank(ind);
                SA.Banks = SA.Banks.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("waveArchives"))
            {
                int ind = GetNextAvailablePreviousId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxWaveArchiveId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddWaveArchive(ind);
                SA.WaveArchives = SA.WaveArchives.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("players"))
            {
                int ind = GetNextAvailablePreviousId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxPlayerId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequencePlayer(ind);
                SA.Players = SA.Players.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("groups"))
            {
                int ind = GetNextAvailablePreviousId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxGroupId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddGroup(ind);
                SA.Groups = SA.Groups.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("streamPlayers"))
            {
                int ind = GetNextAvailablePreviousId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxStreamPlayerId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddStreamPlayer(ind);
                SA.StreamPlayers = SA.StreamPlayers.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("streams"))
            {
                int ind = GetNextAvailablePreviousId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxStreamId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddStream(ind);
                SA.Streams = SA.Streams.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
        }

        public void AddBelow(object sender, EventArgs e)
        {
            if (tree.SelectedNode.Parent.Name.Equals("sequences"))
            {
                int ind = GetNextAvailableForwardId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxSequenceId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequence(ind);
                SA.Sequences = SA.Sequences.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("sequenceArchives"))
            {
                int ind = GetNextAvailableForwardId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxSequenceArchiveId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequenceArchive(ind);
                SA.SequenceArchives = SA.SequenceArchives.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("banks"))
            {
                int ind = GetNextAvailableForwardId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxBankId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddBank(ind);
                SA.Banks = SA.Banks.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("waveArchives"))
            {
                int ind = GetNextAvailableForwardId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxWaveArchiveId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddWaveArchive(ind);
                SA.WaveArchives = SA.WaveArchives.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("players"))
            {
                int ind = GetNextAvailableForwardId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxPlayerId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequencePlayer(ind);
                SA.Players = SA.Players.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("groups"))
            {
                int ind = GetNextAvailableForwardId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxGroupId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddGroup(ind);
                SA.Groups = SA.Groups.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("streamPlayers"))
            {
                int ind = GetNextAvailableForwardId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxStreamPlayerId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddStreamPlayer(ind);
                SA.StreamPlayers = SA.StreamPlayers.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Parent.Name.Equals("streams"))
            {
                int ind = GetNextAvailableForwardId(
                    GetIdFromNode(tree.SelectedNode),
                    SoundArchive.MaxStreamId,
                    tree.SelectedNode.Parent.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddStream(ind);
                SA.Streams = SA.Streams.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Parent.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
        }

        public void Replace(object sender, EventArgs e)
        {
            OpenFileDialog o = new()
            {
                RestoreDirectory = true
            };
            int ind = GetIdFromNode(tree.SelectedNode);
            switch (tree.SelectedNode.Parent.Name)
            {
                case "sequences":
                    o.Filter =
                        "Supported Sound Files|*.sseq;*.smft;*.mid|Sound Sequence|*.sseq|SMF Text|*.smft|MIDI|*.mid";
                    break;
                case "sequenceArchives":
                    o.Filter =
                        "Sequence Archive|*.ssar;*.mus|Sound Sequence Archive|*.ssar|Music List|*.mus";
                    break;
                case "banks":
                    o.Filter =
                        "Supported Bank Files|*.sbnk;*.sf2;*.dls|Sound Bank|*.sbnk|Soundfont|*.sf2|Downloadable Sounds|*.dls";
                    break;
                case "waveArchives":
                    o.Filter = "Sound Wave Archive|*.swar";
                    break;
                case "streams":
                    o.Filter =
                        "Supported Sound Files|*.strm;*.swav;*.wav|Stream|*.strm|Sound Wave|*.swav|Wave|*.wav";
                    break;
            }
            if (o.ShowDialog() == DialogResult.OK)
            {
                switch (Path.GetExtension(o.FileName))
                {
                    case ".sseq":
                        SA.Sequences.Where(x => x.Index == ind).FirstOrDefault().File =
                            new Sequence();
                        SA.Sequences.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.Read(o.FileName);
                        DoInfoStuff();
                        break;
                    case ".smft":
                        SequenceInfo seqInfo = SA.Sequences.Where(x => x.Index == ind).FirstOrDefault();
                        seqInfo.File = new Sequence();
                        seqInfo.File.FromText(System.IO.File.ReadAllLines(o.FileName).ToList());
                        seqInfo.File.WriteCommandData();
                        break;
                    case ".mid":
                        switch (seqImportModeBox.SelectedIndex)
                        {
                            case 0:
                                SA.Sequences.Where(x => x.Index == ind).FirstOrDefault().File =
                                    new Sequence();
                                SA.Sequences.Where(x => x.Index == ind)
                                    .FirstOrDefault()
                                    .File.FromMIDI(o.FileName);
                                break;
                            case 1:
                                if (!System.IO.File.Exists(NitroPath + "/midi2sseq.exe"))
                                {
                                    _ = MessageBox.Show("Cannot find midi2sseq.exe!");
                                    return;
                                }
                                System.IO.File.Copy(o.FileName, "temp.mid", true);
                                Process pro = new();
                                pro.StartInfo.FileName = NitroPath + "/midi2sseq.exe";
                                pro.StartInfo.Arguments = "temp.mid temp.sseq";
                                _ = pro.Start();
                                pro.WaitForExit();
                                SA.Sequences.Where(x => x.Index == ind).FirstOrDefault().File =
                                    new Sequence();
                                SA.Sequences.Where(x => x.Index == ind)
                                    .FirstOrDefault()
                                    .File.Read("temp.sseq");
                                System.IO.File.Delete("temp.mid");
                                System.IO.File.Delete("temp.sseq");
                                break;
                            case 2:
                                if (!System.IO.File.Exists(NitroPath + "/smfconv.exe"))
                                {
                                    _ = MessageBox.Show("Cannot find smfconv.exe!");
                                    return;
                                }
                                if (!System.IO.File.Exists(NitroPath + "/seqconv.exe"))
                                {
                                    _ = MessageBox.Show("Cannot find seqconv.exe!");
                                    return;
                                }
                                System.IO.File.Copy(o.FileName, "temp.mid", true);
                                Process pr = new();
                                pr.StartInfo.FileName = NitroPath + "/smfconv.exe";
                                pr.StartInfo.Arguments = "temp.mid";
                                pr.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                _ = pr.Start();
                                pr.WaitForExit();
                                Process p = new();
                                p.StartInfo.FileName = NitroPath + "/seqconv.exe";
                                p.StartInfo.Arguments = "temp.smft";
                                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                _ = p.Start();
                                p.WaitForExit();
                                SA.Sequences.Where(x => x.Index == ind).FirstOrDefault().File =
                                    new Sequence();
                                SA.Sequences.Where(x => x.Index == ind)
                                    .FirstOrDefault()
                                    .File.Read("temp.sseq");
                                System.IO.File.Delete("temp.mid");
                                System.IO.File.Delete("temp.smft");
                                System.IO.File.Delete("temp.sseq");
                                break;
                        }
                        break;
                    case ".ssar":
                        SequenceArchiveInfo seqArcInfo = SA
                            .SequenceArchives.Where(x => x.Index == ind)
                            .FirstOrDefault();
                        seqArcInfo.File = new SequenceArchive();
                        seqArcInfo.File.Read(o.FileName);
                        seqArcInfo.File.ReadCommandData(true);
                        seqArcInfo.File.FromText(seqArcInfo.File.ToText().ToList(), SA);
                        UpdateNodes();
                        DoInfoStuff();
                        break;
                    case ".mus":
                        SequenceArchiveInfo seqArcInfo2 = SA
                            .SequenceArchives.Where(x => x.Index == ind)
                            .FirstOrDefault();
                        seqArcInfo2.File = new SequenceArchive();
                        seqArcInfo2.File.FromText(
                            System.IO.File.ReadAllLines(o.FileName).ToList(),
                            SA
                        );
                        seqArcInfo2.File.WriteCommandData();
                        UpdateNodes();
                        DoInfoStuff();
                        break;
                    case ".sbnk":
                        SA.Banks.Where(x => x.Index == ind).FirstOrDefault().File = new Bank();
                        SA.Banks.Where(x => x.Index == ind).FirstOrDefault().File.Read(o.FileName);
                        DoInfoStuff();
                        break;
                    case ".sf2":
                        SoundFont sf2 = new(o.FileName);
                        ReplaceBankWithSoundFont(
                            SA.Banks.Where(x => x.Index == ind).FirstOrDefault(),
                            sf2
                        );
                        DoInfoStuff();
                        return;
                    case ".dls":
                        DownloadableSounds dls = new(o.FileName);
                        ReplaceBankWithDLS(
                            SA.Banks.Where(x => x.Index == ind).FirstOrDefault(),
                            dls
                        );
                        DoInfoStuff();
                        return;
                    case ".swar":
                        SA.WaveArchives.Where(x => x.Index == ind).FirstOrDefault().File =
                            new WaveArchive();
                        SA.WaveArchives.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.Read(o.FileName);
                        DoInfoStuff();
                        break;
                    case ".strm":
                        SA.Streams.Where(x => x.Index == ind).FirstOrDefault().File =
                            new NitroFileLoader.Stream();
                        SA.Streams.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.Read(o.FileName);
                        DoInfoStuff();
                        break;
                    case ".swav":
                        SA.Streams.Where(x => x.Index == ind).FirstOrDefault().File =
                            new NitroFileLoader.Stream();
                        Wave swav = new();
                        swav.Read(o.FileName);
                        SA.Streams.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.FromOtherStreamFile(swav);
                        DoInfoStuff();
                        break;
                    case ".wav":
                        SA.Streams.Where(x => x.Index == ind).FirstOrDefault().File =
                            new NitroFileLoader.Stream();
                        RiffWave riff = new();
                        riff.Read(o.FileName);
                        SA.Streams.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.FromOtherStreamFile(riff);
                        DoInfoStuff();
                        break;
                }
            }
        }

        public void Export(object sender, EventArgs e)
        {
            SaveFileDialog s = new()
            {
                RestoreDirectory = true,
                FileName = tree.SelectedNode.Text[(tree.SelectedNode.Text.IndexOf(' ') + 1)..]
            };
            int ind = GetIdFromNode(tree.SelectedNode);
            switch (tree.SelectedNode.Parent.Name)
            {
                case "sequences":
                    s.Filter =
                        "Supported Sound Files|*.sseq;*.smft;*.mid;*.wav|Sound Sequence|*.sseq|SMF Text|*.smft|MIDI|*.mid|Wave|*.wav";
                    s.FileName += ".sseq";
                    break;
                case "sequenceArchives":
                    s.Filter =
                        "Sequence Archive|*.ssar;*.mus|Sound Sequence Archive|*.ssar|Music List|*.mus";
                    s.FileName += ".ssar";
                    break;
                case "banks":
                    s.Filter =
                        "Supported Bank Files|*.sbnk;*.sf2;*.dls|Sound Bank|*.sbnk|Soundfont|*.sf2|Downloadable Sounds|*.dls";
                    s.FileName += ".sbnk";
                    break;
                case "waveArchives":
                    s.Filter = "Sound Wave Archive|*.swar";
                    s.FileName += ".swar";
                    break;
                case "streams":
                    s.Filter =
                        "Supported Sound Files|*.strm;*.swav;*.wav|Stream|*.strm|Sound Wave|*.swav|Wave|*.wav";
                    s.FileName += ".strm";
                    break;
            }
            if (tree.SelectedNode.Parent.Parent != null)
            {
                s.Filter =
                    "Supported Sound Files|*.sseq;*.smft;*.mid;*.wav|Sound Sequence|*.sseq|SMF Text|*.smft|MIDI|*.mid|Wave|*.wav";
                s.FileName += ".sseq";
            }
            if (s.ShowDialog() == DialogResult.OK)
            {
                switch (Path.GetExtension(s.FileName))
                {
                    case ".sseq":
                        if (tree.SelectedNode.Parent.Parent == null)
                        {
                            SA.Sequences.Where(x => x.Index == ind)
                                .FirstOrDefault()
                                .File.Write(s.FileName);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        break;
                    case ".smft":
                        if (tree.SelectedNode.Parent.Parent == null)
                        {
                            SA.Sequences.Where(x => x.Index == ind)
                                .FirstOrDefault()
                                .File.ReadCommandData();
                            SA.Sequences.Where(x => x.Index == ind).FirstOrDefault().File.Name = SA
                                .Sequences.Where(x => x.Index == ind)
                                .FirstOrDefault()
                                .Name;
                            System.IO.File.WriteAllLines(
                                s.FileName,
                                SA.Sequences.Where(x => x.Index == ind)
                                    .FirstOrDefault()
                                    .File.ToText()
                            );
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        break;
                    case ".mid":
                        if (tree.SelectedNode.Parent.Parent == null)
                        {
                            switch (seqExportModeBox.SelectedIndex)
                            {
                                case 0:
                                    SA.Sequences.Where(x => x.Index == ind)
                                        .FirstOrDefault()
                                        .File.SaveMIDI(s.FileName);
                                    break;
                                case 1:
                                    if (!System.IO.File.Exists(NitroPath + "/sseq2midi.exe"))
                                    {
                                        _ = MessageBox.Show("Cannot find sseq2midi.exe!");
                                        return;
                                    }
                                    SA.Sequences.Where(x => x.Index == ind)
                                        .FirstOrDefault()
                                        .File.Write("temp.sseq");
                                    Process pro = new();
                                    pro.StartInfo.FileName = NitroPath + "/sseq2midi.exe";
                                    pro.StartInfo.Arguments = "temp.sseq";
                                    pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    _ = pro.Start();
                                    pro.WaitForExit();
                                    if (
                                        System.IO.File.Exists(s.FileName)
                                        && s.FileName != "temp.mid"
                                    )
                                    {
                                        System.IO.File.Delete(s.FileName);
                                    }
                                    System.IO.File.Move("temp.mid", s.FileName);
                                    System.IO.File.Delete("temp.sseq");
                                    break;
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        break;
                    case ".wav":
                        if (tree.SelectedNode.Parent.Name == "sequences")
                        {
                            SequenceInfo seq = SA
                                .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                                .FirstOrDefault();
                            seq.File.ReadCommandData();
                            try
                            {
                                SequenceRecorder rec = new(
                                    new PlayableBank[] { seq.Bank.File },
                                    seq.Bank.GetAssociatedWaves(),
                                    seq.File.Commands,
                                    0,
                                    s.FileName
                                );
                                _ = rec.ShowDialog();
                            }
                            catch
                            {
                                _ = MessageBox.Show(
                                    "Sequence entry has no valid bank hooked up to it!"
                                );
                                return;
                            }
                        }
                        else if (tree.SelectedNode.Parent.Name == "streams")
                        {
                            RiffWave wav = new();
                            wav.FromOtherStreamFile(
                                SA.Streams.Where(x => x.Index == ind).FirstOrDefault().File
                            );
                            wav.Write(s.FileName);
                        }
                        else
                        {
                            SequenceArchiveInfo a = SA
                                .SequenceArchives.Where(x =>
                                    x.Index == GetIdFromNode(tree.SelectedNode.Parent)
                                )
                                .FirstOrDefault();
                            SequenceArchiveSequence seq = a
                                .File.Sequences.Where(x =>
                                    x.Index == GetIdFromNode(tree.SelectedNode)
                                )
                                .FirstOrDefault();
                            a.File.ReadCommandData(true);
                            try
                            {
                                SequenceRecorder rec = new(
                                    new PlayableBank[] { seq.Bank.File },
                                    seq.Bank.GetAssociatedWaves(),
                                    a.File.Commands,
                                    a.File.PublicLabels.Values.ElementAt(
                                        a.File.Sequences.IndexOf(seq)
                                    ),
                                    s.FileName
                                );
                                _ = rec.ShowDialog();
                            }
                            catch
                            {
                                _ = MessageBox.Show(
                                    "Sequence entry has no valid bank hooked up to it!"
                                );
                                return;
                            }
                        }
                        break;
                    case ".ssar":
                        SA.SequenceArchives.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.Write(s.FileName);
                        break;
                    case ".mus":
                        SequenceArchive sa = new();
                        SequenceArchive other = SA
                            .SequenceArchives.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File;
                        sa.Read(other.Write());
                        for (int i = 0; i < sa.Sequences.Count; i++)
                        {
                            sa.Sequences[i].Name = other.Sequences[i].Name;
                            sa.Sequences[i].Bank = other.Sequences[i].Bank;
                            sa.Sequences[i].Player = other.Sequences[i].Player;
                        }
                        uint[] vals = sa.Labels.Values.ToArray();
                        string[] bakNames = sa.Labels.Keys.ToArray();
                        sa.Labels = [];
                        int valInd = 0;
                        foreach (SequenceArchiveSequence saa in sa.Sequences)
                        {
                            sa.Labels.Add(
                                saa.Name ?? bakNames[valInd],
                                vals[valInd++]
                            );
                        }
                        sa.ReadCommandData(true);
                        sa.Name = SA
                            .SequenceArchives.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .Name;
                        System.IO.File.WriteAllLines(s.FileName, sa.ToText());
                        break;
                    case ".sbnk":
                        SA.Banks.Where(x => x.Index == ind).FirstOrDefault().File.Write(s.FileName);
                        break;
                    case ".sf2":
                        SoundFont sf2 = SA
                            .Banks.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.ToSoundFont(
                                SA,
                                SA.Banks.Where(x => x.Index == ind).FirstOrDefault()
                            );
                        sf2.Write(s.FileName);
                        break;
                    case ".dls":
                        DownloadableSounds dls = SA
                            .Banks.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.ToDLS(SA, SA.Banks.Where(x => x.Index == ind).FirstOrDefault());
                        dls.Write(s.FileName);
                        break;
                    case ".swar":
                        SA.WaveArchives.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.Write(s.FileName);
                        break;
                    case ".strm":
                        SA.Streams.Where(x => x.Index == ind)
                            .FirstOrDefault()
                            .File.Write(s.FileName);
                        break;
                    case ".swav":
                        Wave swav = new();
                        swav.FromOtherStreamFile(
                            SA.Streams.Where(x => x.Index == ind).FirstOrDefault().File
                        );
                        swav.Write(s.FileName);
                        break;
                }
            }
        }

        public void Rename(object sender, EventArgs e)
        {
            string newName = Interaction.InputBox(
                "Rename the entry:",
                "Renamer",
                tree.SelectedNode.Text[(tree.SelectedNode.Text.IndexOf(' ') + 1)..]
            );
            int index = GetIdFromNode(tree.SelectedNode);
            if (newName == "")
            {
                return;
            }
            switch (tree.SelectedNode.Parent.Name)
            {
                case "sequences":
                    if (SA.Sequences.Where(x => x.Name.Equals(newName)).Count() > 0)
                    {
                        _ = MessageBox.Show("An entry of the same name already exists!");
                        return;
                    }
                    SA.Sequences.Where(x => x.Index == index).FirstOrDefault().Name = newName;
                    break;
                case "sequenceArchives":
                    if (SA.SequenceArchives.Where(x => x.Name.Equals(newName)).Count() > 0)
                    {
                        _ = MessageBox.Show("An entry of the same name already exists!");
                        return;
                    }
                    SA.SequenceArchives.Where(x => x.Index == index).FirstOrDefault().Name =
                        newName;
                    break;
                case "banks":
                    if (SA.Banks.Where(x => x.Name.Equals(newName)).Count() > 0)
                    {
                        _ = MessageBox.Show("An entry of the same name already exists!");
                        return;
                    }
                    SA.Banks.Where(x => x.Index == index).FirstOrDefault().Name = newName;
                    break;
                case "waveArchives":
                    if (SA.WaveArchives.Where(x => x.Name.Equals(newName)).Count() > 0)
                    {
                        _ = MessageBox.Show("An entry of the same name already exists!");
                        return;
                    }
                    SA.WaveArchives.Where(x => x.Index == index).FirstOrDefault().Name = newName;
                    break;
                case "players":
                    if (SA.Players.Where(x => x.Name.Equals(newName)).Count() > 0)
                    {
                        _ = MessageBox.Show("An entry of the same name already exists!");
                        return;
                    }
                    SA.Players.Where(x => x.Index == index).FirstOrDefault().Name = newName;
                    break;
                case "groups":
                    if (SA.Groups.Where(x => x.Name.Equals(newName)).Count() > 0)
                    {
                        _ = MessageBox.Show("An entry of the same name already exists!");
                        return;
                    }
                    SA.Groups.Where(x => x.Index == index).FirstOrDefault().Name = newName;
                    break;
                case "streamPlayers":
                    if (SA.StreamPlayers.Where(x => x.Name.Equals(newName)).Count() > 0)
                    {
                        _ = MessageBox.Show("An entry of the same name already exists!");
                        return;
                    }
                    SA.StreamPlayers.Where(x => x.Index == index).FirstOrDefault().Name = newName;
                    break;
                case "streams":
                    if (SA.Streams.Where(x => x.Name.Equals(newName)).Count() > 0)
                    {
                        _ = MessageBox.Show("An entry of the same name already exists!");
                        return;
                    }
                    SA.Streams.Where(x => x.Index == index).FirstOrDefault().Name = newName;
                    break;
            }
            if (tree.SelectedNode.Parent.Parent != null)
            {
                SequenceArchiveInfo sar = SA
                    .SequenceArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode.Parent))
                    .FirstOrDefault();
                if (sar.File.Sequences.Where(x => x.Name.Equals(newName)).Count() > 0)
                {
                    _ = MessageBox.Show("An entry of the same name already exists!");
                }
                sar.File.Sequences.Where(x => x.Index == index).FirstOrDefault().Name = newName;
            }
            UpdateNodes();
            DoInfoStuff();
        }

        public void Delete(object sender, EventArgs e)
        {
            switch (tree.SelectedNode.Parent.Name)
            {
                case "sequences":
                    SequenceInfo x1 = SA
                        .Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    for (int i = 0; i < SA.Groups.Count; i++)
                    {
                        while (SA.Groups[i].Entries.Where(x => x.Entry == x1).Count() > 0)
                        {
                            _ = SA.Groups[i]
                                .Entries.Remove(
                                    SA.Groups[i].Entries.Where(x => x.Entry == x1).FirstOrDefault()
                                );
                        }
                    }
                    _ = SA.Sequences.Remove(x1);
                    break;
                case "sequenceArchives":
                    SequenceArchiveInfo x2 = SA
                        .SequenceArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    for (int i = 0; i < SA.Groups.Count; i++)
                    {
                        while (SA.Groups[i].Entries.Where(x => x.Entry == x2).Count() > 0)
                        {
                            _ = SA.Groups[i]
                                .Entries.Remove(
                                    SA.Groups[i].Entries.Where(x => x.Entry == x2).FirstOrDefault()
                                );
                        }
                    }
                    _ = SA.SequenceArchives.Remove(x2);
                    break;
                case "banks":
                    BankInfo x3 = SA
                        .Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    for (int i = 0; i < SA.Groups.Count; i++)
                    {
                        while (SA.Groups[i].Entries.Where(x => x.Entry == x3).Count() > 0)
                        {
                            _ = SA.Groups[i]
                                .Entries.Remove(
                                    SA.Groups[i].Entries.Where(x => x.Entry == x3).FirstOrDefault()
                                );
                        }
                    }
                    _ = SA.Banks.Remove(x3);
                    break;
                case "waveArchives":
                    WaveArchiveInfo x4 = SA
                        .WaveArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    for (int i = 0; i < SA.Groups.Count; i++)
                    {
                        while (SA.Groups[i].Entries.Where(x => x.Entry == x4).Count() > 0)
                        {
                            _ = SA.Groups[i]
                                .Entries.Remove(
                                    SA.Groups[i].Entries.Where(x => x.Entry == x4).FirstOrDefault()
                                );
                        }
                    }
                    _ = SA.WaveArchives.Remove(x4);
                    break;
                case "players":
                    PlayerInfo x5 = SA
                        .Players.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    _ = SA.Players.Remove(x5);
                    break;
                case "groups":
                    GroupInfo x6 = SA
                        .Groups.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    _ = SA.Groups.Remove(x6);
                    break;
                case "streamPlayers":
                    StreamPlayerInfo x7 = SA
                        .StreamPlayers.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    _ = SA.StreamPlayers.Remove(x7);
                    break;
                case "streams":
                    StreamInfo x8 = SA
                        .Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                        .FirstOrDefault();
                    _ = SA.Streams.Remove(x8);
                    break;
            }
            UpdateNodes();
            DoInfoStuff();
        }

        public int GetNextAvailableForwardId(int preferredId, uint maxId, string root)
        {
            int id = preferredId;
            bool rootHasId()
            {
                foreach (TreeNode n in tree.Nodes[root].Nodes)
                {
                    if (n.Text.Contains("[" + id + "]"))
                    {
                        return true;
                    }
                }
                return false;
            }
            while (id <= maxId && rootHasId())
            {
                id++;
            }
            if (id > maxId)
            {
                id = 0;
                while (id < preferredId && rootHasId())
                {
                    id++;
                }
                if (id == preferredId)
                {
                    _ = MessageBox.Show("There are no more available slots for the item!");
                    return -1;
                }
            }
            return id < 0 ? -1 : id;
        }

        public int GetNextAvailablePreviousId(int preferredId, uint maxId, string root)
        {
            int id = preferredId;
            bool rootHasId()
            {
                foreach (TreeNode n in tree.Nodes[root].Nodes)
                {
                    if (n.Text.Contains("[" + id + "]"))
                    {
                        return true;
                    }
                }
                return false;
            }
            while (id >= 0 && rootHasId())
            {
                id--;
            }
            if (id < 0)
            {
                id = (int)maxId;
                while (id > preferredId && rootHasId())
                {
                    id--;
                }
                if (id == preferredId)
                {
                    _ = MessageBox.Show("There are no more available slots for the item!");
                    return -1;
                }
            }
            return id < 0 ? -1 : id;
        }

        public override void RootAdd()
        {
            if (tree.SelectedNode.Name.Equals("sequences"))
            {
                int ind = GetNextAvailableForwardId(
                    SA.Sequences.Count > 0 ? SA.Sequences.Last().Index + 1 : 0,
                    SoundArchive.MaxSequenceId,
                    tree.SelectedNode.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequence(ind);
                SA.Sequences = SA.Sequences.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Name.Equals("sequenceArchives"))
            {
                int ind = GetNextAvailableForwardId(
                    SA.SequenceArchives.Count > 0 ? SA.SequenceArchives.Last().Index + 1 : 0,
                    SoundArchive.MaxSequenceArchiveId,
                    tree.SelectedNode.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequenceArchive(ind);
                SA.SequenceArchives = SA.SequenceArchives.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Name.Equals("banks"))
            {
                int ind = GetNextAvailableForwardId(
                    SA.Banks.Count > 0 ? SA.Banks.Last().Index + 1 : 0,
                    SoundArchive.MaxBankId,
                    tree.SelectedNode.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddBank(ind);
                SA.Banks = SA.Banks.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Name.Equals("waveArchives"))
            {
                int ind = GetNextAvailableForwardId(
                    SA.WaveArchives.Count > 0 ? SA.WaveArchives.Last().Index + 1 : 0,
                    SoundArchive.MaxWaveArchiveId,
                    tree.SelectedNode.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddWaveArchive(ind);
                SA.WaveArchives = SA.WaveArchives.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Name.Equals("players"))
            {
                int ind = GetNextAvailableForwardId(
                    SA.Players.Count > 0 ? SA.Players.Last().Index + 1 : 0,
                    SoundArchive.MaxPlayerId,
                    tree.SelectedNode.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddSequencePlayer(ind);
                SA.Players = SA.Players.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Name.Equals("groups"))
            {
                int ind = GetNextAvailableForwardId(
                    SA.Groups.Count > 0 ? SA.Groups.Last().Index + 1 : 0,
                    SoundArchive.MaxGroupId,
                    tree.SelectedNode.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddGroup(ind);
                SA.Groups = SA.Groups.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Name.Equals("streamPlayers"))
            {
                int ind = GetNextAvailableForwardId(
                    SA.StreamPlayers.Count > 0 ? SA.StreamPlayers.Last().Index + 1 : 0,
                    SoundArchive.MaxStreamPlayerId,
                    tree.SelectedNode.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddStreamPlayer(ind);
                SA.StreamPlayers = SA.StreamPlayers.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
            else if (tree.SelectedNode.Name.Equals("streams"))
            {
                int ind = GetNextAvailableForwardId(
                    SA.Streams.Count > 0 ? SA.Streams.Last().Index + 1 : 0,
                    SoundArchive.MaxStreamId,
                    tree.SelectedNode.Name
                );
                if (ind == -1)
                {
                    return;
                }
                AddStream(ind);
                SA.Streams = SA.Streams.OrderBy(x => x.Index).ToList();
                UpdateNodes();
                foreach (TreeNode n in tree.SelectedNode.Nodes)
                {
                    if (n.Text.Contains("[" + ind + "]"))
                    {
                        tree.SelectedNode = n;
                    }
                }
                DoInfoStuff();
            }
        }

        public void OpenSeqArcFile(object sender, EventArgs e)
        {
            SequenceArchiveInfo f = SA
                .SequenceArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode))
                .FirstOrDefault();
            SequenceArchiveEditor ed = new(f.File, this, f.Name);
            ed.Show();
        }

        public void AddSequence(int index)
        {
            if (SA.Banks.Count < 1)
            {
                _ = MessageBox.Show("There must be at least one bank in order to add a sequence.");
                return;
            }
            if (SA.Players.Count < 1)
            {
                _ = MessageBox.Show(
                    "There must be at least one sequence player in order to add a sequence."
                );
                return;
            }
            SequenceInfo e = new()
            {
                Bank = SA.Banks[0],
                Player = SA.Players[0],
                Name = "SEQ_" + index,
                Index = index
            };
            int nameIndex = index;
            while (SA.Sequences.Where(x => x.Name.Equals("SEQ_" + nameIndex)).Count() > 0)
            {
                e.Name = "SEQ_" + nameIndex++;
            }
            e.File = new Sequence()
            {
                RawData = new byte[] { 0xFF },
                Labels = [],
            };
            SA.Sequences.Add(e);
        }

        public void AddSequenceArchive(int index)
        {
            if (SA.Banks.Count < 1)
            {
                _ = MessageBox.Show(
                    "There must be at least one bank in order to add a sequence archive."
                );
                return;
            }
            if (SA.Players.Count < 1)
            {
                _ = MessageBox.Show(
                    "There must be at least one sequence player in order to add a sequence archive."
                );
                return;
            }
            SequenceArchiveInfo e = new()
            {
                Name = "SEQARC_" + index,
                Index = index
            };
            int nameIndex = index;
            while (SA.Sequences.Where(x => x.Name.Equals("SEQARC_" + nameIndex)).Count() > 0)
            {
                e.Name = "SEQARC_" + nameIndex++;
            }
            e.File = new SequenceArchive()
            {
                RawData = new byte[0],
                Labels = [],
            };
            SA.SequenceArchives.Add(e);
        }

        public void AddBank(int index)
        {
            BankInfo e = new()
            {
                File = new Bank(),
                Name = "BANK_" + index,
                Index = index
            };
            int nameIndex = index;
            while (SA.Banks.Where(x => x.Name.Equals("BANK_" + nameIndex)).Count() > 0)
            {
                e.Name = "BANK_" + nameIndex++;
            }
            SA.Banks.Add(e);
        }

        public void AddWaveArchive(int index)
        {
            WaveArchiveInfo e = new()
            {
                File = new WaveArchive(),
                Name = "WAR_" + index,
                Index = index
            };
            int nameIndex = index;
            while (SA.WaveArchives.Where(x => x.Name.Equals("WAR_" + nameIndex)).Count() > 0)
            {
                e.Name = "WAR_" + nameIndex++;
            }
            SA.WaveArchives.Add(e);
        }

        public void AddSequencePlayer(int index)
        {
            PlayerInfo e = new()
            {
                Name = "PLAYER_" + index,
                Index = index,
                ChannelFlags = new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                }
            };
            int nameIndex = index;
            while (SA.Players.Where(x => x.Name.Equals("PLAYER_" + nameIndex)).Count() > 0)
            {
                e.Name = "PLAYER_" + nameIndex++;
            }
            SA.Players.Add(e);
        }

        public void AddGroup(int index)
        {
            GroupInfo e = new()
            {
                Name = "GROUP_" + index,
                Index = index,
                Entries = []
            };
            int nameIndex = index;
            while (SA.Groups.Where(x => x.Name.Equals("GROUP_" + nameIndex)).Count() > 0)
            {
                e.Name = "GROUP_" + nameIndex++;
            }
            SA.Groups.Add(e);
        }

        public void AddStreamPlayer(int index)
        {
            StreamPlayerInfo e = new()
            {
                Name = "STRM_PLAYER_" + index,
                Index = index
            };
            int nameIndex = index;
            while (
                SA.StreamPlayers.Where(x => x.Name.Equals("STRM_PLAYER_" + nameIndex)).Count() > 0
            )
            {
                e.Name = "STRM_PLAYER_" + nameIndex++;
            }
            SA.StreamPlayers.Add(e);
        }

        public void AddStream(int index)
        {
            if (SA.StreamPlayers.Count < 1)
            {
                _ = MessageBox.Show("The must be at least one stream player in order to add a stream.");
                return;
            }
            OpenFileDialog o = new()
            {
                RestoreDirectory = true,
                Filter = "Supported Audio Files|*.wav;*.swav;*.strm"
            };
            _ = o.ShowDialog();
            NitroFileLoader.Stream s = new();
            if (o.FileName != "")
            {
                switch (Path.GetExtension(o.FileName))
                {
                    case ".wav":
                        RiffWave r = new();
                        r.Read(o.FileName);
                        s.FromOtherStreamFile(r);
                        break;
                    case ".swav":
                        Wave w = new();
                        w.Read(o.FileName);
                        s.FromOtherStreamFile(w);
                        break;
                    case ".strm":
                        s.Read(o.FileName);
                        break;
                }
            }
            else
            {
                return;
            }
            StreamInfo e = new()
            {
                Name = "STRM_" + index,
                Index = index,
                Player = SA.StreamPlayers[0],
                File = s
            };
            int nameIndex = index;
            while (SA.Streams.Where(x => x.Name.Equals("STRM_" + nameIndex)).Count() > 0)
            {
                e.Name = "STRM_" + nameIndex++;
            }
            SA.Streams.Add(e);
        }

        public void ReplaceBankWithDLS(BankInfo b, DownloadableSounds d)
        {
            List<RiffWave> wavSamples = [];
            List<int> instIds = [];
            List<string> instNames = [];
            foreach (GotaSoundBank.DLS.Instrument i in d.Instruments)
            {
                if (i.Regions.Count > 0)
                {
                    instIds.Add((int)(i.InstrumentId + (i.BankId * 128)));
                    instNames.Add(i.Name);
                    wavSamples.Add(d.Waves[(int)i.Regions[0].WaveId]);
                }
            }
            InstrumentSelector sel = new(wavSamples, instIds, instNames);
            _ = sel.ShowDialog();
            instIds = sel.SelectedInstruments;
            if (instIds == null)
            {
                return;
            }
            List<GotaSoundBank.DLS.Instrument> insts = [];
            foreach (int id in instIds)
            {
                insts.Add(
                    d.Instruments.Where(x => x.InstrumentId == id % 128 && x.BankId == id / 128)
                        .FirstOrDefault()
                );
            }
            wavSamples = [];
            List<string> md5s = [];
            List<WaveArchiveInfo> wars = b.WaveArchives.Where(x => x != null).ToList();
            Dictionary<uint, int> otherWavId = [];
            foreach (GotaSoundBank.DLS.Instrument inst in insts)
            {
                foreach (Region r in inst.Regions)
                {
                    RiffWave wav = d.Waves[(int)r.WaveId];
                    wav.Loops = r.Loops;
                    wav.LoopStart = r.LoopStart;
                    wav.LoopEnd =
                        r.LoopLength == 0 ? (uint)wav.Audio.NumSamples : r.LoopStart + r.LoopLength;
                    string md5 = wav.Md5Sum;
                    if (!md5s.Contains(md5))
                    {
                        wavSamples.Add(wav);
                        md5s.Add(md5);
                        otherWavId.Add(r.WaveId, otherWavId.Count);
                    }
                    else if (!otherWavId.ContainsKey(r.WaveId))
                    {
                        otherWavId.Add(r.WaveId, md5s.IndexOf(md5));
                    }
                }
            }
            WaveMapper wm = new(wavSamples, wars);
            _ = wm.ShowDialog();
            List<ushort> warMap = wm.WarMap;
            if (warMap == null)
            {
                return;
            }
            Dictionary<int, Tuple<ushort, ushort>> swavMap =
                [];
            foreach (RiffWave w in wavSamples)
            {
                Wave wav = new();
                wav.FromOtherStreamFile(w);
                WaveArchiveInfo war = SA
                    .WaveArchives.Where(x => x.Index == warMap[wavSamples.IndexOf(w)])
                    .FirstOrDefault();
                string md5 = wav.Md5Sum;
                if (war.File.Waves.Where(x => x.Md5Sum.Equals(md5)).Count() < 1)
                {
                    war.File.Waves.Add(wav);
                }
                swavMap.Add(
                    wavSamples.IndexOf(w),
                    new Tuple<ushort, ushort>(
                        (ushort)b.WaveArchives.ToList().IndexOf(war),
                        (ushort)
                            war.File.Waves.IndexOf(
                                war.File.Waves.Where(x => x.Md5Sum.Equals(md5)).FirstOrDefault()
                            )
                    )
                );
            }
            b.File.Instruments = [];
            foreach (GotaSoundBank.DLS.Instrument inst in insts)
            {
                NitroFileLoader.Instrument.Instrument i = inst.Regions.Count < 2 && inst.Regions.Where(x => x.NoteLow == 0).Count() > 0
                    ? new DirectInstrument()
                    : inst.Regions.Count < 9
                    && inst.Regions.Where(x => x.NoteLow == 0).Count() > 0
                        ? new KeySplitInstrument()
                        : new DrumSetInstrument();
                i.Index = (int)(inst.InstrumentId + (inst.BankId * 128));
                List<Region> regions = inst.Regions.OrderBy(x => x.NoteLow).ToList();
                if (regions[0].NoteLow != 0 && (i as DrumSetInstrument) != null)
                {
                    (i as DrumSetInstrument).Min = (byte)regions[0].NoteLow;
                }
                foreach (Region r in regions)
                {
                    NoteInfo n = new();
                    Tuple<ushort, ushort> dir = swavMap[otherWavId[r.WaveId]];
                    n.WarId = dir.Item1;
                    n.WaveId = dir.Item2;
                    n.InstrumentType = NitroFileLoader.Instrument.InstrumentType.PCM;
                    n.BaseNote = (byte)(r.RootNote + (r.Tuning / 65536d / 12));
                    n.Key = (Notes)r.NoteHigh;
                    n.Attack = 127;
                    n.Decay = 127;
                    n.Sustain = 127;
                    n.Release = 127;
                    n.Pan = 64;
                    foreach (Articulator a in r.Articulators)
                    {
                        foreach (Connection c in a.Connections)
                        {
                            if (c.DestinationConnection == DestinationConnection.EG1AttackTime)
                            {
                                if (c.Scale != int.MinValue)
                                {
                                    n.Attack = Bank.GetNearestTableIndex(
                                        Bank.TimecentsToMilliseconds(c.Scale / 65536),
                                        Bank.AttackTable
                                    );
                                }
                            }
                            if (c.DestinationConnection == DestinationConnection.EG1DecayTime)
                            {
                                if (c.Scale != int.MinValue)
                                {
                                    n.Decay = Bank.GetNearestTableIndex(
                                        Bank.TimecentsToMilliseconds(c.Scale / 65536),
                                        Bank.MaxReleaseTimes
                                    );
                                }
                            }
                            if (c.DestinationConnection == DestinationConnection.EG1SustainLevel)
                            {
                                n.Sustain = Bank.Fraction2Sustain(c.Scale / 65536 / 1000d);
                            }
                            if (c.DestinationConnection == DestinationConnection.EG1ReleaseTime)
                            {
                                if (c.Scale != int.MinValue)
                                {
                                    n.Release = Bank.GetNearestTableIndex(
                                        Bank.TimecentsToMilliseconds(c.Scale / 65536),
                                        Bank.MaxReleaseTimes
                                    );
                                }
                            }
                            if (c.DestinationConnection == DestinationConnection.Pan)
                            {
                                n.Pan = Bank.SetPan(c.Scale / 65536);
                            }
                        }
                    }
                    i.NoteInfo.Add(n);
                }
                b.File.Instruments.Add(i);
            }
        }

        public void ReplaceBankWithSoundFont(BankInfo b, SoundFont s)
        {
            ReplaceBankWithDLS(b, new DownloadableSounds(s));
        }

        public override void importFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            OpenFileDialog o = new()
            {
                RestoreDirectory = true,
                Filter = "Sound Archive|*.sdat;*.dsxe|All Files|*.*"
            };
            if (o.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string path = o.FileName;
            File = (IOFile)Activator.CreateInstance(FileType);
            File.Read(path);
        }

        public override void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            SaveFileDialog s = new()
            {
                RestoreDirectory = true,
                Filter = "Sound Archive|*.sdat;*.dsxe|All Files|*.*",
                OverwritePrompt = false
            };
            if (s.ShowDialog() == DialogResult.OK)
            {
                SA.Write(s.FileName);
            }
        }
    }
}
