using GotaSequenceLib;
using GotaSoundBank.DLS;
using GotaSoundBank.SF2;
using GotaSoundIO.IO;
using NitroFileLoader;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NitroStudio2
{
    public abstract class EditorBase : Form
    {
        public IOFile File;
        public IOFile ExtFile;
        public string FileName;
        public string FilePath;
        public string ExtensionDescription;
        public string Extension;
        public string EditorName;
        public bool FileOpen;
        public Type FileType;
        public EditorBase OtherEditor;
        public bool WritingInfo;
        public Notes NoteDown;
        public Panel pnlPianoKeys;
        private Dictionary<Notes, PianoKey> pianoKeys = new();
        public ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem getHelpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem aboutNitroStudio2ToolStripMenuItem;
        private ToolStripMenuItem bankGeneratorToolStripMenuItem;
        private ToolStripMenuItem exportSDKProjectToolStripMenuItem;
        public Panel settingsPanel;
        private Label label2;
        public CheckBox writeNamesBox;
        private Label label3;
        public ComboBox seqImportModeBox;
        public ComboBox seqExportModeBox;
        private Label label4;
        public Panel indexPanel;
        private Label label5;
        public NumericUpDown itemIndexBox;
        public Button swapAtIndexButton;
        public Panel forceUniqueFilePanel;
        public CheckBox forceUniqueFileBox;
        private Label label8;
        public Panel warPanel;
        public CheckBox loadIndividuallyBox;
        private Label label9;
        public Panel blankPanel;
        public Panel bankPanel;
        private TableLayoutPanel tableLayoutPanel2;
        public ComboBox bnkWar0ComboBox;
        private Label label6;
        public NumericUpDown bnkWar0Box;
        private TableLayoutPanel tableLayoutPanel5;
        public ComboBox bnkWar3ComboBox;
        public NumericUpDown bnkWar3Box;
        private Label label11;
        private TableLayoutPanel tableLayoutPanel4;
        public ComboBox bnkWar2ComboBox;
        public NumericUpDown bnkWar2Box;
        private Label label10;
        private TableLayoutPanel tableLayoutPanel3;
        public ComboBox bnkWar1ComboBox;
        public NumericUpDown bnkWar1Box;
        private Label label7;
        public Panel grpPanel;
        public DataGridView grpEntries;
        private DataGridViewComboBoxColumn item;
        private DataGridViewComboBoxColumn loadFlags;
        public Panel streamPlayerPanel;
        private TableLayoutPanel tableLayoutPanel6;
        public Label leftChannelLabel;
        public ComboBox stmPlayerChannelType;
        private Label label12;
        public Label rightChannelLabel;
        public NumericUpDown stmPlayerLeftChannelBox;
        public NumericUpDown stmPlayerRightChannelBox;
        public Panel stmPanel;
        private Label label13;
        public NumericUpDown stmPriorityBox;
        private Label label14;
        public NumericUpDown stmVolumeBox;
        private Label label15;
        private TableLayoutPanel tableLayoutPanel7;
        public ComboBox stmPlayerComboBox;
        public NumericUpDown stmPlayerBox;
        public CheckBox stmMonoToStereoBox;
        private Label label16;
        public Panel playerPanel;
        public NumericUpDown playerMaxSequencesBox;
        private Label label17;
        public NumericUpDown playerHeapSizeBox;
        private Label label18;
        private Label label19;
        private TableLayoutPanel tableLayoutPanel8;
        public CheckBox[] playerFlagBoxes = new CheckBox[16];
        public CheckBox playerFlag15Box => playerFlagBoxes[15];
        public CheckBox playerFlag14Box => playerFlagBoxes[14];
        public CheckBox playerFlag13Box => playerFlagBoxes[13];
        public CheckBox playerFlag12Box => playerFlagBoxes[12];
        public CheckBox playerFlag11Box => playerFlagBoxes[11];
        public CheckBox playerFlag10Box => playerFlagBoxes[10];
        public CheckBox playerFlag9Box  => playerFlagBoxes[9];
        public CheckBox playerFlag8Box  => playerFlagBoxes[8];
        public CheckBox playerFlag7Box  => playerFlagBoxes[7];
        public CheckBox playerFlag6Box  => playerFlagBoxes[6];
        public CheckBox playerFlag5Box  => playerFlagBoxes[5];
        public CheckBox playerFlag4Box  => playerFlagBoxes[4];
        public CheckBox playerFlag3Box  => playerFlagBoxes[3];
        public CheckBox playerFlag2Box  => playerFlagBoxes[2];
        public CheckBox playerFlag1Box  => playerFlagBoxes[1];
        public CheckBox playerFlag0Box  => playerFlagBoxes[0];
        public Panel kermalisSoundPlayerPanel;
        public Label soundPlayerLabel;
        private TableLayoutPanel tableLayoutPanel9;
        public Button kermalisStopButton;
        public Button kermalisPauseButton;
        public TrackBar kermalisVolumeSlider;
        public Button kermalisPlayButton;
        private Label label21;
        public CheckBox kermalisLoopBox;
        private Label label22;
        public Panel seqPanel;
        private Label label23;
        private TableLayoutPanel tableLayoutPanel10;
        public ComboBox seqBankComboBox;
        public NumericUpDown seqBankBox;
        private Label label24;
        public NumericUpDown seqVolumeBox;
        public NumericUpDown seqChannelPriorityBox;
        private Label label25;
        public NumericUpDown seqPlayerPriorityBox;
        private Label label26;
        private Label label27;
        private TableLayoutPanel tableLayoutPanel11;
        public ComboBox seqPlayerComboBox;
        public NumericUpDown seqPlayerBox;
        public Panel seqBankPanel;
        private Label label28;
        private TableLayoutPanel tableLayoutPanel12;
        public ComboBox seqEditorBankComboBox;
        public NumericUpDown seqEditorBankBox;
        public Panel seqArcPanel;
        public Button seqArcOpenFileButton;
        public Panel seqArcSeqPanel;
        private Label label29;
        private TableLayoutPanel tableLayoutPanel13;
        public ComboBox seqArcSeqComboBox;
        public NumericUpDown seqArcSeqBox;
        public Panel bankEditorPanel;
        private Label label30;
        private TableLayoutPanel tableLayoutPanel14;
        public RadioButton directBox;
        public RadioButton keySplitBox;
        public RadioButton drumSetBox;
        public Label drumSetRangeStartLabel;
        private TableLayoutPanel tableLayoutPanel15;
        public ComboBox drumSetStartRangeComboBox;
        public NumericUpDown drumSetStartRangeBox;
        private Label label32;
        public DataGridView bankRegions;
        public Panel bankEditorWars;
        private TableLayoutPanel tableLayoutPanel16;
        public ComboBox war3ComboBox;
        public NumericUpDown war3Box;
        private Label label31;
        private TableLayoutPanel tableLayoutPanel17;
        public ComboBox war2ComboBox;
        public NumericUpDown war2Box;
        private Label label33;
        private TableLayoutPanel tableLayoutPanel18;
        public ComboBox war1ComboBox;
        public NumericUpDown war1Box;
        private Label label34;
        private TableLayoutPanel tableLayoutPanel19;
        public ComboBox war0ComboBox;
        public NumericUpDown war0Box;
        private Label label35;
        private ToolStripMenuItem sequenceEditorToolStripMenuItem;
        private ToolStripMenuItem sequenceArchiveEditorToolStripMenuItem;
        private ToolStripMenuItem bankEditorToolStripMenuItem;
        private ToolStripMenuItem createStreamToolStripMenuItem;
        private ToolStripMenuItem creaveWaveToolStripMenuItem;
        private ToolStripMenuItem waveArchiveEditorToolStripMenuItem;
        public ToolStripStatusLabel currentNote;
        private TableLayoutPanel tableLayoutPanel1;
        public CheckBox[] trackBoxes     = new CheckBox[16];
        public PictureBox[] trackPictures = new PictureBox[16];
        public Button[] trackSolos       = new Button[16];
        private TableLayoutPanel[] trackPanels = new TableLayoutPanel[16];
        // Named accessors for backward compatibility
        public CheckBox track0Box  => trackBoxes[0];
        public CheckBox track1Box  => trackBoxes[1];
        public CheckBox track2Box  => trackBoxes[2];
        public CheckBox track3Box  => trackBoxes[3];
        public CheckBox track4Box  => trackBoxes[4];
        public CheckBox track5Box  => trackBoxes[5];
        public CheckBox track6Box  => trackBoxes[6];
        public CheckBox track7Box  => trackBoxes[7];
        public CheckBox track8Box  => trackBoxes[8];
        public CheckBox track9Box  => trackBoxes[9];
        public CheckBox track10Box => trackBoxes[10];
        public CheckBox track11Box => trackBoxes[11];
        public CheckBox track12Box => trackBoxes[12];
        public CheckBox track13Box => trackBoxes[13];
        public CheckBox track14Box => trackBoxes[14];
        public CheckBox track15Box => trackBoxes[15];
        public PictureBox track0Picture  => trackPictures[0];
        public PictureBox track1Picture  => trackPictures[1];
        public PictureBox track2Picture  => trackPictures[2];
        public PictureBox track3Picture  => trackPictures[3];
        public PictureBox track4Picture  => trackPictures[4];
        public PictureBox track5Picture  => trackPictures[5];
        public PictureBox track6Picture  => trackPictures[6];
        public PictureBox track7Picture  => trackPictures[7];
        public PictureBox track8Picture  => trackPictures[8];
        public PictureBox track9Picture  => trackPictures[9];
        public PictureBox track10Picture => trackPictures[10];
        public PictureBox track11Picture => trackPictures[11];
        public PictureBox track12Picture => trackPictures[12];
        public PictureBox track13Picture => trackPictures[13];
        public PictureBox track14Picture => trackPictures[14];
        public PictureBox track15Picture => trackPictures[15];
        public Button track0Solo  => trackSolos[0];
        public Button track1Solo  => trackSolos[1];
        public Button track2Solo  => trackSolos[2];
        public Button track3Solo  => trackSolos[3];
        public Button track4Solo  => trackSolos[4];
        public Button track5Solo  => trackSolos[5];
        public Button track6Solo  => trackSolos[6];
        public Button track7Solo  => trackSolos[7];
        public Button track8Solo  => trackSolos[8];
        public Button track9Solo  => trackSolos[9];
        public Button track10Solo => trackSolos[10];
        public Button track11Solo => trackSolos[11];
        public Button track12Solo => trackSolos[12];
        public Button track13Solo => trackSolos[13];
        public Button track14Solo => trackSolos[14];
        public Button track15Solo => trackSolos[15];
        private TableLayoutPanel tableLayoutPanel20;
        private DataGridViewButtonColumn playSampleButton;
        private DataGridViewComboBoxColumn endNote;
        private DataGridViewComboBoxColumn instrumentType;
        private DataGridViewTextBoxColumn waveId;
        private DataGridViewTextBoxColumn waveArchiveId;
        private DataGridViewComboBoxColumn baseNote;
        private DataGridViewTextBoxColumn attack;
        private DataGridViewTextBoxColumn decay;
        private DataGridViewTextBoxColumn sustain;
        private DataGridViewTextBoxColumn release;
        private DataGridViewTextBoxColumn pan;
        private ToolStripMenuItem sF2ToDLSToolStripMenuItem;
        private ToolStripMenuItem dLSToSF2ToolStripMenuItem;
        public TrackBar kermalisPosition;
        private ToolStripMenuItem batchExportMIDIDLSSF2ToolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel36;
        public Button exportWavButton;
        public Button exportMidiButton;
        public static MainWindow MainWindow;

        public EditorBase(
            Type fileType,
            string extensionDescription,
            string extension,
            string editorName,
            MainWindow mainWindow
        )
        {
            InitializeComponent();
            MainWindow = mainWindow;
            FileType = fileType;
            ExtensionDescription = extensionDescription;
            Extension = extension;
            EditorName = editorName;
            Text = EditorName;
            UpdateNodes();
            DoInfoStuff();
        }

        public EditorBase(
            Type fileType,
            string extensionDescription,
            string extension,
            string editorName,
            string fileToOpen,
            MainWindow mainWindow
        )
        {
            InitializeComponent();
            MainWindow = mainWindow;
            FileType = fileType;
            ExtensionDescription = extensionDescription;
            Extension = extension;
            EditorName = editorName;
            File = (IOFile)Activator.CreateInstance(FileType);
            FilePath = fileToOpen;
            Text = EditorName + " - " + Path.GetFileName(fileToOpen);
            FileOpen = true;
            FileName = Path.GetFileNameWithoutExtension(FilePath);
            File.Read(fileToOpen);
            UpdateNodes();
            DoInfoStuff();
        }

        public EditorBase(
            Type fileType,
            string extensionDescription,
            string extension,
            string editorName,
            IOFile fileToOpen,
            MainWindow mainWindow,
            string fileName
        )
        {
            InitializeComponent();
            MainWindow = mainWindow;
            FileType = fileType;
            ExtensionDescription = extensionDescription;
            Extension = extension;
            EditorName = editorName;
            ExtFile = fileToOpen;
            File = (IOFile)Activator.CreateInstance(ExtFile.GetType());
            File.Read(ExtFile.Write());
            FilePath = "";
            string name = fileName;
            name ??= "{ Null File Name }";
            Text = EditorName + " - " + name + ".s" + extension;
            FileOpen = true;
            FileName = fileName;
            UpdateNodes();
            DoInfoStuff();
        }

        public MenuStrip menuStrip;
        public ToolStripMenuItem newToolStripMenuItem;
        public ToolStripMenuItem openToolStripMenuItem;
        public ToolStripMenuItem saveToolStripMenuItem;
        public ToolStripMenuItem saveAsToolStripMenuItem;
        public ToolStripMenuItem closeToolStripMenuItem;
        public ToolStripMenuItem quitToolStripMenuItem;
        public ToolStripMenuItem editToolStripMenuItem;
        public ToolStripMenuItem blankFileToolStripMenuItem;
        public ToolStripMenuItem importFileToolStripMenuItem;
        public ToolStripMenuItem exportFileToolStripMenuItem;
        public SplitContainer splitContainer1;
        public TreeView tree;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private StatusStrip statusStrip;
        public ToolStripStatusLabel status;
        public ImageList treeIcons;
        private System.ComponentModel.IContainer components;
        public ContextMenuStrip rootMenu;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem expandToolStripMenuItem;
        private ToolStripMenuItem collapseToolStripMenuItem;
        public Panel noInfoPanel;
        private Label label1;
        private ToolTip toolTip;
        public ToolStripMenuItem fileMenu;
        public ContextMenuStrip nodeMenu;
        private ToolStripMenuItem addAboveToolStripMenuItem1;
        private ToolStripMenuItem addBelowToolStripMenuItem1;
        private ToolStripMenuItem moveUpToolStripMenuItem1;
        private ToolStripMenuItem moveDownToolStripMenuItem1;
        private ToolStripMenuItem replaceFileToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem1;
        private ToolStripMenuItem exportToolStripMenuItem1;
        public Panel sequenceEditorPanel;
        public Scintilla sequenceEditor;
        private BindingSource bindingSource1;
        public ContextMenuStrip sarEntryMenu;
        private ToolStripMenuItem sarAddAbove;
        private ToolStripMenuItem sarAddBelow;
        private ToolStripMenuItem sarMoveUp;
        private ToolStripMenuItem sarMoveDown;
        private ToolStripMenuItem sarReplace;
        private ToolStripMenuItem sarExport;
        private ToolStripMenuItem sarRename;
        private ToolStripMenuItem sarDelete;

        public void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources =
                new(typeof(EditorBase));
            System.Windows.Forms.TreeNode treeNode1 = new(
                "File Information",
                10,
                10
            );
            menuStrip = new System.Windows.Forms.MenuStrip();
            fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            blankFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sequenceEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sequenceArchiveEditorToolStripMenuItem =
                new System.Windows.Forms.ToolStripMenuItem();
            bankEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            waveArchiveEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bankGeneratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            creaveWaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            createStreamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportSDKProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sF2ToDLSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dLSToSF2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            batchExportMIDIDLSSF2ToolStripMenuItem =
                new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            getHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutNitroStudio2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            seqBankPanel = new System.Windows.Forms.Panel();
            tableLayoutPanel36 = new System.Windows.Forms.TableLayoutPanel();
            exportWavButton = new System.Windows.Forms.Button();
            exportMidiButton = new System.Windows.Forms.Button();
            tableLayoutPanel20 = new System.Windows.Forms.TableLayoutPanel();
            for (int _t = 0; _t < 16; _t++)
            {
                trackPanels[_t]   = new System.Windows.Forms.TableLayoutPanel();
                trackBoxes[_t]    = new System.Windows.Forms.CheckBox();
                trackPictures[_t] = new System.Windows.Forms.PictureBox();
                trackSolos[_t]    = new System.Windows.Forms.Button();
            }
            label28 = new System.Windows.Forms.Label();
            tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
            seqEditorBankComboBox = new System.Windows.Forms.ComboBox();
            seqEditorBankBox = new System.Windows.Forms.NumericUpDown();
            bankEditorPanel = new System.Windows.Forms.Panel();
            bankRegions = new System.Windows.Forms.DataGridView();
            playSampleButton = new System.Windows.Forms.DataGridViewButtonColumn();
            endNote = new System.Windows.Forms.DataGridViewComboBoxColumn();
            instrumentType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            waveId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            waveArchiveId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            baseNote = new System.Windows.Forms.DataGridViewComboBoxColumn();
            attack = new System.Windows.Forms.DataGridViewTextBoxColumn();
            decay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            sustain = new System.Windows.Forms.DataGridViewTextBoxColumn();
            release = new System.Windows.Forms.DataGridViewTextBoxColumn();
            pan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            label32 = new System.Windows.Forms.Label();
            tableLayoutPanel15 = new System.Windows.Forms.TableLayoutPanel();
            drumSetStartRangeComboBox = new System.Windows.Forms.ComboBox();
            drumSetStartRangeBox = new System.Windows.Forms.NumericUpDown();
            drumSetRangeStartLabel = new System.Windows.Forms.Label();
            tableLayoutPanel14 = new System.Windows.Forms.TableLayoutPanel();
            keySplitBox = new System.Windows.Forms.RadioButton();
            drumSetBox = new System.Windows.Forms.RadioButton();
            directBox = new System.Windows.Forms.RadioButton();
            label30 = new System.Windows.Forms.Label();
            seqArcSeqPanel = new System.Windows.Forms.Panel();
            label29 = new System.Windows.Forms.Label();
            tableLayoutPanel13 = new System.Windows.Forms.TableLayoutPanel();
            seqArcSeqComboBox = new System.Windows.Forms.ComboBox();
            seqArcSeqBox = new System.Windows.Forms.NumericUpDown();
            seqArcPanel = new System.Windows.Forms.Panel();
            seqArcOpenFileButton = new System.Windows.Forms.Button();
            seqPanel = new System.Windows.Forms.Panel();
            tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            seqPlayerComboBox = new System.Windows.Forms.ComboBox();
            seqPlayerBox = new System.Windows.Forms.NumericUpDown();
            label27 = new System.Windows.Forms.Label();
            seqPlayerPriorityBox = new System.Windows.Forms.NumericUpDown();
            label26 = new System.Windows.Forms.Label();
            seqChannelPriorityBox = new System.Windows.Forms.NumericUpDown();
            label25 = new System.Windows.Forms.Label();
            seqVolumeBox = new System.Windows.Forms.NumericUpDown();
            label24 = new System.Windows.Forms.Label();
            tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            seqBankComboBox = new System.Windows.Forms.ComboBox();
            seqBankBox = new System.Windows.Forms.NumericUpDown();
            label23 = new System.Windows.Forms.Label();
            playerPanel = new System.Windows.Forms.Panel();
            tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            for (int _i = 0; _i < 16; _i++) playerFlagBoxes[_i] = new System.Windows.Forms.CheckBox();
            label19 = new System.Windows.Forms.Label();
            playerHeapSizeBox = new System.Windows.Forms.NumericUpDown();
            label18 = new System.Windows.Forms.Label();
            playerMaxSequencesBox = new System.Windows.Forms.NumericUpDown();
            label17 = new System.Windows.Forms.Label();
            stmPanel = new System.Windows.Forms.Panel();
            stmMonoToStereoBox = new System.Windows.Forms.CheckBox();
            label16 = new System.Windows.Forms.Label();
            label15 = new System.Windows.Forms.Label();
            tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            stmPlayerComboBox = new System.Windows.Forms.ComboBox();
            stmPlayerBox = new System.Windows.Forms.NumericUpDown();
            stmPriorityBox = new System.Windows.Forms.NumericUpDown();
            label14 = new System.Windows.Forms.Label();
            stmVolumeBox = new System.Windows.Forms.NumericUpDown();
            label13 = new System.Windows.Forms.Label();
            streamPlayerPanel = new System.Windows.Forms.Panel();
            stmPlayerChannelType = new System.Windows.Forms.ComboBox();
            label12 = new System.Windows.Forms.Label();
            tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            stmPlayerLeftChannelBox = new System.Windows.Forms.NumericUpDown();
            stmPlayerRightChannelBox = new System.Windows.Forms.NumericUpDown();
            rightChannelLabel = new System.Windows.Forms.Label();
            leftChannelLabel = new System.Windows.Forms.Label();
            grpPanel = new System.Windows.Forms.Panel();
            grpEntries = new System.Windows.Forms.DataGridView();
            item = new System.Windows.Forms.DataGridViewComboBoxColumn();
            loadFlags = new System.Windows.Forms.DataGridViewComboBoxColumn();
            bankPanel = new System.Windows.Forms.Panel();
            tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            bnkWar3ComboBox = new System.Windows.Forms.ComboBox();
            bnkWar3Box = new System.Windows.Forms.NumericUpDown();
            label11 = new System.Windows.Forms.Label();
            tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            bnkWar2ComboBox = new System.Windows.Forms.ComboBox();
            bnkWar2Box = new System.Windows.Forms.NumericUpDown();
            label10 = new System.Windows.Forms.Label();
            tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            bnkWar1ComboBox = new System.Windows.Forms.ComboBox();
            bnkWar1Box = new System.Windows.Forms.NumericUpDown();
            label7 = new System.Windows.Forms.Label();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            bnkWar0ComboBox = new System.Windows.Forms.ComboBox();
            bnkWar0Box = new System.Windows.Forms.NumericUpDown();
            label6 = new System.Windows.Forms.Label();
            blankPanel = new System.Windows.Forms.Panel();
            warPanel = new System.Windows.Forms.Panel();
            loadIndividuallyBox = new System.Windows.Forms.CheckBox();
            label9 = new System.Windows.Forms.Label();
            forceUniqueFilePanel = new System.Windows.Forms.Panel();
            forceUniqueFileBox = new System.Windows.Forms.CheckBox();
            label8 = new System.Windows.Forms.Label();
            indexPanel = new System.Windows.Forms.Panel();
            swapAtIndexButton = new System.Windows.Forms.Button();
            itemIndexBox = new System.Windows.Forms.NumericUpDown();
            label5 = new System.Windows.Forms.Label();
            settingsPanel = new System.Windows.Forms.Panel();
            seqExportModeBox = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            seqImportModeBox = new System.Windows.Forms.ComboBox();
            label3 = new System.Windows.Forms.Label();
            writeNamesBox = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            noInfoPanel = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            kermalisSoundPlayerPanel = new System.Windows.Forms.Panel();
            kermalisPosition = new System.Windows.Forms.TrackBar();
            tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            label22 = new System.Windows.Forms.Label();
            label21 = new System.Windows.Forms.Label();
            kermalisStopButton = new System.Windows.Forms.Button();
            kermalisPauseButton = new System.Windows.Forms.Button();
            kermalisVolumeSlider = new System.Windows.Forms.TrackBar();
            kermalisLoopBox = new System.Windows.Forms.CheckBox();
            kermalisPlayButton = new System.Windows.Forms.Button();
            soundPlayerLabel = new System.Windows.Forms.Label();
            pnlPianoKeys = new System.Windows.Forms.Panel();
            InitPianoKeys();
            bankEditorWars = new System.Windows.Forms.Panel();
            tableLayoutPanel16 = new System.Windows.Forms.TableLayoutPanel();
            war3ComboBox = new System.Windows.Forms.ComboBox();
            war3Box = new System.Windows.Forms.NumericUpDown();
            label31 = new System.Windows.Forms.Label();
            tableLayoutPanel17 = new System.Windows.Forms.TableLayoutPanel();
            war2ComboBox = new System.Windows.Forms.ComboBox();
            war2Box = new System.Windows.Forms.NumericUpDown();
            label33 = new System.Windows.Forms.Label();
            tableLayoutPanel18 = new System.Windows.Forms.TableLayoutPanel();
            war1ComboBox = new System.Windows.Forms.ComboBox();
            war1Box = new System.Windows.Forms.NumericUpDown();
            label34 = new System.Windows.Forms.Label();
            tableLayoutPanel19 = new System.Windows.Forms.TableLayoutPanel();
            war0ComboBox = new System.Windows.Forms.ComboBox();
            war0Box = new System.Windows.Forms.NumericUpDown();
            label35 = new System.Windows.Forms.Label();
            tree = new System.Windows.Forms.TreeView();
            treeIcons = new System.Windows.Forms.ImageList(components);
            sequenceEditorPanel = new System.Windows.Forms.Panel();
            sequenceEditor = new ScintillaNET.Scintilla();
            openFileDialog = new System.Windows.Forms.OpenFileDialog();
            saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            statusStrip = new System.Windows.Forms.StatusStrip();
            status = new System.Windows.Forms.ToolStripStatusLabel();
            currentNote = new System.Windows.Forms.ToolStripStatusLabel();
            rootMenu = new System.Windows.Forms.ContextMenuStrip(components);
            addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            expandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            collapseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolTip = new System.Windows.Forms.ToolTip(components);
            nodeMenu = new System.Windows.Forms.ContextMenuStrip(components);
            addAboveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            addBelowToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            moveUpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            moveDownToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            replaceFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            bindingSource1 = new System.Windows.Forms.BindingSource(components);
            sarEntryMenu = new System.Windows.Forms.ContextMenuStrip(components);
            sarAddAbove = new System.Windows.Forms.ToolStripMenuItem();
            sarAddBelow = new System.Windows.Forms.ToolStripMenuItem();
            sarMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            sarMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            sarReplace = new System.Windows.Forms.ToolStripMenuItem();
            sarExport = new System.Windows.Forms.ToolStripMenuItem();
            sarRename = new System.Windows.Forms.ToolStripMenuItem();
            sarDelete = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            seqBankPanel.SuspendLayout();
            tableLayoutPanel36.SuspendLayout();
            tableLayoutPanel20.SuspendLayout();
            foreach (var _tp in trackPanels) _tp.SuspendLayout();
            foreach (var _pic in trackPictures) ((System.ComponentModel.ISupportInitialize)_pic).BeginInit();
            tableLayoutPanel12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)seqEditorBankBox).BeginInit();
            bankEditorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bankRegions).BeginInit();
            tableLayoutPanel15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)drumSetStartRangeBox).BeginInit();
            tableLayoutPanel14.SuspendLayout();
            seqArcSeqPanel.SuspendLayout();
            tableLayoutPanel13.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)seqArcSeqBox).BeginInit();
            seqArcPanel.SuspendLayout();
            seqPanel.SuspendLayout();
            tableLayoutPanel11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)seqPlayerBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)seqPlayerPriorityBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)seqChannelPriorityBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)seqVolumeBox).BeginInit();
            tableLayoutPanel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)seqBankBox).BeginInit();
            playerPanel.SuspendLayout();
            tableLayoutPanel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)playerHeapSizeBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)playerMaxSequencesBox).BeginInit();
            stmPanel.SuspendLayout();
            tableLayoutPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)stmPlayerBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)stmPriorityBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)stmVolumeBox).BeginInit();
            streamPlayerPanel.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)stmPlayerLeftChannelBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)stmPlayerRightChannelBox).BeginInit();
            grpPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grpEntries).BeginInit();
            bankPanel.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bnkWar3Box).BeginInit();
            tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bnkWar2Box).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bnkWar1Box).BeginInit();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bnkWar0Box).BeginInit();
            warPanel.SuspendLayout();
            forceUniqueFilePanel.SuspendLayout();
            indexPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)itemIndexBox).BeginInit();
            settingsPanel.SuspendLayout();
            noInfoPanel.SuspendLayout();
            kermalisSoundPlayerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kermalisPosition).BeginInit();
            tableLayoutPanel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kermalisVolumeSlider).BeginInit();
            pnlPianoKeys.SuspendLayout();
            bankEditorWars.SuspendLayout();
            tableLayoutPanel16.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)war3Box).BeginInit();
            tableLayoutPanel17.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)war2Box).BeginInit();
            tableLayoutPanel18.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)war1Box).BeginInit();
            tableLayoutPanel19.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)war0Box).BeginInit();
            sequenceEditorPanel.SuspendLayout();
            statusStrip.SuspendLayout();
            rootMenu.SuspendLayout();
            nodeMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            sarEntryMenu.SuspendLayout();
            SuspendLayout();
            menuStrip.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    fileMenu,
                    editToolStripMenuItem,
                    toolsToolStripMenuItem,
                    helpToolStripMenuItem,
                    aboutToolStripMenuItem,
                }
            );
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(984, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip1";
            fileMenu.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    newToolStripMenuItem,
                    openToolStripMenuItem,
                    saveToolStripMenuItem,
                    saveAsToolStripMenuItem,
                    closeToolStripMenuItem,
                    quitToolStripMenuItem,
                }
            );
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new System.Drawing.Size(37, 20);
            fileMenu.Text = "File";
            newToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.New;
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            newToolStripMenuItem.Text = "New";
            newToolStripMenuItem.Click += new System.EventHandler(
                newToolStripMenuItem_Click
            );
            openToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.Open;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += new System.EventHandler(
                openToolStripMenuItem_Click
            );
            saveToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.Save;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += new System.EventHandler(
                saveToolStripMenuItem_Click
            );
            saveAsToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.Save_As;
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            saveAsToolStripMenuItem.Text = "Save As";
            saveAsToolStripMenuItem.Click += new System.EventHandler(
                saveAsToolStripMenuItem_Click
            );
            closeToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.Close;
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += new System.EventHandler(
                closeToolStripMenuItem_Click
            );
            quitToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.Quit;
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            quitToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            quitToolStripMenuItem.Text = "Quit";
            quitToolStripMenuItem.Click += new System.EventHandler(
                quitToolStripMenuItem_Click
            );
            editToolStripMenuItem.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    blankFileToolStripMenuItem,
                    importFileToolStripMenuItem,
                    exportFileToolStripMenuItem,
                }
            );
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            blankFileToolStripMenuItem.Image = global::NitroStudio2
                .Properties
                .Resources
                .Rename;
            blankFileToolStripMenuItem.Name = "blankFileToolStripMenuItem";
            blankFileToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            blankFileToolStripMenuItem.Text = "Blank File";
            blankFileToolStripMenuItem.Click += new System.EventHandler(
                blankFileToolStripMenuItem_Click
            );
            importFileToolStripMenuItem.Image = global::NitroStudio2
                .Properties
                .Resources
                .Import;
            importFileToolStripMenuItem.Name = "importFileToolStripMenuItem";
            importFileToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            importFileToolStripMenuItem.Text = "Import File";
            importFileToolStripMenuItem.Click += new System.EventHandler(
                importFileToolStripMenuItem_Click
            );
            exportFileToolStripMenuItem.Image = global::NitroStudio2
                .Properties
                .Resources
                .Export;
            exportFileToolStripMenuItem.Name = "exportFileToolStripMenuItem";
            exportFileToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            exportFileToolStripMenuItem.Text = "Export File";
            exportFileToolStripMenuItem.Click += new System.EventHandler(
                exportFileToolStripMenuItem_Click
            );
            toolsToolStripMenuItem.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    sequenceEditorToolStripMenuItem,
                    sequenceArchiveEditorToolStripMenuItem,
                    bankEditorToolStripMenuItem,
                    waveArchiveEditorToolStripMenuItem,
                    bankGeneratorToolStripMenuItem,
                    creaveWaveToolStripMenuItem,
                    createStreamToolStripMenuItem,
                    exportSDKProjectToolStripMenuItem,
                    sF2ToDLSToolStripMenuItem,
                    dLSToSF2ToolStripMenuItem,
                    batchExportMIDIDLSSF2ToolStripMenuItem,
                }
            );
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            toolsToolStripMenuItem.Text = "Tools";
            toolsToolStripMenuItem.Visible = false;
            sequenceEditorToolStripMenuItem.Image =
                (System.Drawing.Image)resources.GetObject("sequenceEditorToolStripMenuItem.Image")
            ;
            sequenceEditorToolStripMenuItem.Name = "sequenceEditorToolStripMenuItem";
            sequenceEditorToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            sequenceEditorToolStripMenuItem.Text = "Sequence Editor";
            sequenceEditorToolStripMenuItem.Click += new System.EventHandler(
                SequenceEditorToolStripMenuItem_Click
            );
            sequenceArchiveEditorToolStripMenuItem.Image =
                (System.Drawing.Image)
                    resources.GetObject("sequenceArchiveEditorToolStripMenuItem.Image")

            ;
            sequenceArchiveEditorToolStripMenuItem.Name =
                "sequenceArchiveEditorToolStripMenuItem";
            sequenceArchiveEditorToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            sequenceArchiveEditorToolStripMenuItem.Text = "Sequence Archive Editor";
            sequenceArchiveEditorToolStripMenuItem.Click += new System.EventHandler(
                SequenceArchiveEditorToolStripMenuItem_Click
            );
            bankEditorToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.Bank;
            bankEditorToolStripMenuItem.Name = "bankEditorToolStripMenuItem";
            bankEditorToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            bankEditorToolStripMenuItem.Text = "Bank Editor";
            bankEditorToolStripMenuItem.Click += new System.EventHandler(
                BankEditorToolStripMenuItem_Click
            );
            waveArchiveEditorToolStripMenuItem.Image =
                (System.Drawing.Image)
                    resources.GetObject("waveArchiveEditorToolStripMenuItem.Image")

            ;
            waveArchiveEditorToolStripMenuItem.Name = "waveArchiveEditorToolStripMenuItem";
            waveArchiveEditorToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            waveArchiveEditorToolStripMenuItem.Text = "Wave Archive Editor";
            waveArchiveEditorToolStripMenuItem.Click += new System.EventHandler(
                WaveArchiveEditorToolStripMenuItem_Click
            );
            bankGeneratorToolStripMenuItem.Image =
                (System.Drawing.Image)resources.GetObject("bankGeneratorToolStripMenuItem.Image")
            ;
            bankGeneratorToolStripMenuItem.Name = "bankGeneratorToolStripMenuItem";
            bankGeneratorToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            bankGeneratorToolStripMenuItem.Text = "Bank Generator";
            bankGeneratorToolStripMenuItem.Click += new System.EventHandler(
                BankGeneratorToolStripMenuItem_Click
            );
            creaveWaveToolStripMenuItem.Image =
                (System.Drawing.Image)resources.GetObject("creaveWaveToolStripMenuItem.Image")
            ;
            creaveWaveToolStripMenuItem.Name = "creaveWaveToolStripMenuItem";
            creaveWaveToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            creaveWaveToolStripMenuItem.Text = "Creave Wave";
            creaveWaveToolStripMenuItem.Click += new System.EventHandler(
                CreaveWaveToolStripMenuItem_Click
            );
            createStreamToolStripMenuItem.Image =
                (System.Drawing.Image)resources.GetObject("createStreamToolStripMenuItem.Image")
            ;
            createStreamToolStripMenuItem.Name = "createStreamToolStripMenuItem";
            createStreamToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            createStreamToolStripMenuItem.Text = "Create Stream";
            createStreamToolStripMenuItem.Click += new System.EventHandler(
                CreateStreamToolStripMenuItem_Click
            );
            exportSDKProjectToolStripMenuItem.Image = global::NitroStudio2
                .Properties
                .Resources
                .NSM;
            exportSDKProjectToolStripMenuItem.Name = "exportSDKProjectToolStripMenuItem";
            exportSDKProjectToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            exportSDKProjectToolStripMenuItem.Text = "Export SDK Project";
            exportSDKProjectToolStripMenuItem.Click += new System.EventHandler(
                ExportSDKProjectToolStripMenuItem_Click
            );
            sF2ToDLSToolStripMenuItem.Image =
                (System.Drawing.Image)resources.GetObject("sF2ToDLSToolStripMenuItem.Image")
            ;
            sF2ToDLSToolStripMenuItem.Name = "sF2ToDLSToolStripMenuItem";
            sF2ToDLSToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            sF2ToDLSToolStripMenuItem.Text = "SF2 To DLS";
            sF2ToDLSToolStripMenuItem.Click += new System.EventHandler(
                sF2ToDLSToolStripMenuItem_Click
            );
            dLSToSF2ToolStripMenuItem.Image =
                (System.Drawing.Image)resources.GetObject("dLSToSF2ToolStripMenuItem.Image")
            ;
            dLSToSF2ToolStripMenuItem.Name = "dLSToSF2ToolStripMenuItem";
            dLSToSF2ToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            dLSToSF2ToolStripMenuItem.Text = "DLS To SF2";
            dLSToSF2ToolStripMenuItem.Click += new System.EventHandler(
                dLSToSF2ToolStripMenuItem_Click
            );
            batchExportMIDIDLSSF2ToolStripMenuItem.Image =
                (System.Drawing.Image)
                    resources.GetObject("batchExportMIDIDLSSF2ToolStripMenuItem.Image")

            ;
            batchExportMIDIDLSSF2ToolStripMenuItem.Name =
                "batchExportMIDIDLSSF2ToolStripMenuItem";
            batchExportMIDIDLSSF2ToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            batchExportMIDIDLSSF2ToolStripMenuItem.Text = "Batch Export MIDI/DLS/SF2";
            helpToolStripMenuItem.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[] { getHelpToolStripMenuItem }
            );
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            getHelpToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.Help;
            getHelpToolStripMenuItem.Name = "getHelpToolStripMenuItem";
            getHelpToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            getHelpToolStripMenuItem.Text = "Get Help";
            getHelpToolStripMenuItem.Click += new System.EventHandler(
                GetHelpToolStripMenuItem_Click
            );
            aboutToolStripMenuItem.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[] { aboutNitroStudio2ToolStripMenuItem }
            );
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            aboutToolStripMenuItem.Text = "About";
            aboutNitroStudio2ToolStripMenuItem.Image = global::NitroStudio2
                .Properties
                .Resources
                .Ico;
            aboutNitroStudio2ToolStripMenuItem.Name = "aboutNitroStudio2ToolStripMenuItem";
            aboutNitroStudio2ToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            aboutNitroStudio2ToolStripMenuItem.Text = "About Nitro Studio 2";
            aboutNitroStudio2ToolStripMenuItem.Click += new System.EventHandler(
                AboutNitroStudio2ToolStripMenuItem_Click
            );
            splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Panel1.Controls.Add(seqBankPanel);
            splitContainer1.Panel1.Controls.Add(bankEditorPanel);
            splitContainer1.Panel1.Controls.Add(seqArcSeqPanel);
            splitContainer1.Panel1.Controls.Add(seqArcPanel);
            splitContainer1.Panel1.Controls.Add(seqPanel);
            splitContainer1.Panel1.Controls.Add(playerPanel);
            splitContainer1.Panel1.Controls.Add(stmPanel);
            splitContainer1.Panel1.Controls.Add(streamPlayerPanel);
            splitContainer1.Panel1.Controls.Add(grpPanel);
            splitContainer1.Panel1.Controls.Add(bankPanel);
            splitContainer1.Panel1.Controls.Add(blankPanel);
            splitContainer1.Panel1.Controls.Add(warPanel);
            splitContainer1.Panel1.Controls.Add(forceUniqueFilePanel);
            splitContainer1.Panel1.Controls.Add(indexPanel);
            splitContainer1.Panel1.Controls.Add(settingsPanel);
            splitContainer1.Panel1.Controls.Add(noInfoPanel);
            splitContainer1.Panel1.Controls.Add(kermalisSoundPlayerPanel);
            splitContainer1.Panel2.Controls.Add(pnlPianoKeys);
            splitContainer1.Panel2.Controls.Add(bankEditorWars);
            splitContainer1.Panel2.Controls.Add(tree);
            splitContainer1.Panel2.Controls.Add(sequenceEditorPanel);
            splitContainer1.Size = new System.Drawing.Size(984, 540);
            splitContainer1.SplitterDistance = 327;
            splitContainer1.TabIndex = 1;
            seqBankPanel.Controls.Add(tableLayoutPanel36);
            seqBankPanel.Controls.Add(tableLayoutPanel20);
            seqBankPanel.Controls.Add(label28);
            seqBankPanel.Controls.Add(tableLayoutPanel12);
            seqBankPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            seqBankPanel.Location = new System.Drawing.Point(0, 334);
            seqBankPanel.Name = "seqBankPanel";
            seqBankPanel.Size = new System.Drawing.Size(325, 204);
            seqBankPanel.TabIndex = 18;
            seqBankPanel.Visible = false;
            tableLayoutPanel36.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel36.ColumnCount = 2;
            _ = tableLayoutPanel36.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            _ = tableLayoutPanel36.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            tableLayoutPanel36.Controls.Add(exportWavButton, 0, 0);
            tableLayoutPanel36.Controls.Add(exportMidiButton, 0, 0);
            tableLayoutPanel36.Location = new System.Drawing.Point(14, 244);
            tableLayoutPanel36.Name = "tableLayoutPanel36";
            tableLayoutPanel36.RowCount = 1;
            _ = tableLayoutPanel36.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            tableLayoutPanel36.Size = new System.Drawing.Size(298, 25);
            tableLayoutPanel36.TabIndex = 30;
            exportWavButton.Dock = System.Windows.Forms.DockStyle.Fill;
            exportWavButton.Location = new System.Drawing.Point(149, 0);
            exportWavButton.Margin = new System.Windows.Forms.Padding(0);
            exportWavButton.Name = "exportWavButton";
            exportWavButton.Size = new System.Drawing.Size(149, 25);
            exportWavButton.TabIndex = 5;
            exportWavButton.Text = "Export WAV";
            exportWavButton.UseVisualStyleBackColor = true;
            exportMidiButton.Dock = System.Windows.Forms.DockStyle.Fill;
            exportMidiButton.Location = new System.Drawing.Point(0, 0);
            exportMidiButton.Margin = new System.Windows.Forms.Padding(0);
            exportMidiButton.Name = "exportMidiButton";
            exportMidiButton.Size = new System.Drawing.Size(149, 25);
            exportMidiButton.TabIndex = 4;
            exportMidiButton.Text = "Export MIDI";
            exportMidiButton.UseVisualStyleBackColor = true;
            tableLayoutPanel20.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            // Track layout panel (outer 2-col x 8-row grid) setup
            tableLayoutPanel20.ColumnCount = 2;
            _ = tableLayoutPanel20.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            _ = tableLayoutPanel20.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel20.RowCount = 8;
            for (int _r = 0; _r < 8; _r++)
                _ = tableLayoutPanel20.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            tableLayoutPanel20.Size = new System.Drawing.Size(298, 176);
            tableLayoutPanel20.TabIndex = 28;

            // Map: trackIndex -> (col, row) in tableLayoutPanel20
            // Even tracks: col=0, row=track/2.  Odd tracks: col=1, row=(track-1)/2
            for (int _t = 0; _t < 16; _t++)
            {
                int _col = _t % 2;       // 0=even, 1=odd
                int _row = _t / 2;
                var _panel = trackPanels[_t];
                var _box   = trackBoxes[_t];
                var _pic   = trackPictures[_t];
                var _solo  = trackSolos[_t];

                // Inner track TableLayoutPanel
                _panel.ColumnCount = 3;
                _ = _panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
                _ = _panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                _ = _panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                _panel.Controls.Add(_box,  0, 0);
                _panel.Controls.Add(_solo, 1, 0);
                _panel.Controls.Add(_pic,  2, 0);
                _panel.Dock    = System.Windows.Forms.DockStyle.Fill;
                _panel.Margin  = new System.Windows.Forms.Padding(0);
                _panel.Name    = "trackPanel" + _t;
                _panel.RowCount = 1;
                _ = _panel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                _ = _panel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                _panel.TabIndex = 26 + _t;

                // CheckBox
                _box.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
                _box.Checked    = true;
                _box.CheckState = System.Windows.Forms.CheckState.Checked;
                _box.Dock       = System.Windows.Forms.DockStyle.Fill;
                _box.Location   = new System.Drawing.Point(3, 3);
                _box.Name       = "track" + _t + "Box";
                _box.Size       = new System.Drawing.Size(68, 16);
                _box.TabIndex   = 2;
                _box.Text       = "Track " + _t + ":";
                _box.UseVisualStyleBackColor = true;

                // PictureBox
                _pic.BackgroundImage       = global::NitroStudio2.Properties.Resources.Idle;
                _pic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                _pic.Dock                  = System.Windows.Forms.DockStyle.Fill;
                _pic.Location              = new System.Drawing.Point(111, 0);
                _pic.Margin                = new System.Windows.Forms.Padding(0);
                _pic.Name                  = "track" + _t + "Picture";
                _pic.Size                  = new System.Drawing.Size(38, 22);
                _pic.TabIndex              = 0;
                _pic.TabStop               = false;

                // Solo Button
                _solo.Dock     = System.Windows.Forms.DockStyle.Fill;
                _solo.Location = new System.Drawing.Point(74, 0);
                _solo.Margin   = new System.Windows.Forms.Padding(0);
                _solo.Name     = "track" + _t + "Solo";
                _solo.Size     = new System.Drawing.Size(37, 22);
                _solo.TabIndex = 3;
                _solo.Text     = "Solo";
                _solo.UseVisualStyleBackColor = true;

                tableLayoutPanel20.Controls.Add(_panel, _col, _row);
            }
            label28.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label28.Location = new System.Drawing.Point(11, 3);
            label28.Name = "label28";
            label28.Size = new System.Drawing.Size(301, 20);
            label28.TabIndex = 25;
            label28.Text = "Preview Bank:";
            label28.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel12.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel12.ColumnCount = 2;
            _ = tableLayoutPanel12.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel12.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel12.Controls.Add(seqEditorBankComboBox, 0, 0);
            tableLayoutPanel12.Controls.Add(seqEditorBankBox, 1, 0);
            tableLayoutPanel12.Location = new System.Drawing.Point(14, 25);
            tableLayoutPanel12.Name = "tableLayoutPanel12";
            tableLayoutPanel12.RowCount = 1;
            _ = tableLayoutPanel12.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel12.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel12.TabIndex = 24;
            seqEditorBankComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            seqEditorBankComboBox.DropDownStyle = System
                .Windows
                .Forms
                .ComboBoxStyle
                .DropDownList;
            seqEditorBankComboBox.FormattingEnabled = true;
            seqEditorBankComboBox.Location = new System.Drawing.Point(3, 3);
            seqEditorBankComboBox.Name = "seqEditorBankComboBox";
            seqEditorBankComboBox.Size = new System.Drawing.Size(247, 21);
            seqEditorBankComboBox.TabIndex = 6;
            toolTip.SetToolTip(seqEditorBankComboBox, "Bank to use with the sequence.");
            seqEditorBankBox.Dock = System.Windows.Forms.DockStyle.Fill;
            seqEditorBankBox.Location = new System.Drawing.Point(256, 3);
            seqEditorBankBox.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            seqEditorBankBox.Name = "seqEditorBankBox";
            seqEditorBankBox.Size = new System.Drawing.Size(39, 20);
            seqEditorBankBox.TabIndex = 7;
            toolTip.SetToolTip(
                seqEditorBankBox,
                "Id of the bank to use with the sequence."
            );
            bankEditorPanel.Controls.Add(bankRegions);
            bankEditorPanel.Controls.Add(label32);
            bankEditorPanel.Controls.Add(tableLayoutPanel15);
            bankEditorPanel.Controls.Add(drumSetRangeStartLabel);
            bankEditorPanel.Controls.Add(tableLayoutPanel14);
            bankEditorPanel.Controls.Add(label30);
            bankEditorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            bankEditorPanel.Location = new System.Drawing.Point(0, 334);
            bankEditorPanel.Name = "bankEditorPanel";
            bankEditorPanel.Size = new System.Drawing.Size(325, 204);
            bankEditorPanel.TabIndex = 21;
            bankEditorPanel.Visible = false;
            bankRegions.Anchor =




                                System.Windows.Forms.AnchorStyles.Top
                                | System.Windows.Forms.AnchorStyles.Bottom
                             | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            bankRegions.AutoSizeColumnsMode = System
                .Windows
                .Forms
                .DataGridViewAutoSizeColumnsMode
                .DisplayedCells;
            bankRegions.ColumnHeadersHeightSizeMode = System
                .Windows
                .Forms
                .DataGridViewColumnHeadersHeightSizeMode
                .AutoSize;
            bankRegions.Columns.AddRange(
                new System.Windows.Forms.DataGridViewColumn[]
                {
                    playSampleButton,
                    endNote,
                    instrumentType,
                    waveId,
                    waveArchiveId,
                    baseNote,
                    attack,
                    decay,
                    sustain,
                    release,
                    pan,
                }
            );
            bankRegions.Location = new System.Drawing.Point(14, 141);
            bankRegions.Name = "bankRegions";
            bankRegions.Size = new System.Drawing.Size(298, 54);
            bankRegions.TabIndex = 26;
            playSampleButton.HeaderText = "Play";
            playSampleButton.Name = "playSampleButton";
            playSampleButton.Text = "Play";
            playSampleButton.UseColumnTextForButtonValue = true;
            playSampleButton.Width = 33;
            endNote.FillWeight = 50F;
            endNote.HeaderText = "End Note";
            endNote.Items.AddRange(
                new object[]
                {
                    "cnm1 (0)",
                    "csm1 (1)",
                    "dnm1 (2)",
                    "dsm1 (3)",
                    "enm1 (4)",
                    "fnm1 (5)",
                    "fsm1 (6)",
                    "gnm1 (7)",
                    "gsm1 (8)",
                    "anm1 (9)",
                    "asm1 (10)",
                    "bnm1 (11)",
                    "cn0 (12)",
                    "cs0 (13)",
                    "dn0 (14)",
                    "ds0 (15)",
                    "en0 (16)",
                    "fn0 (17)",
                    "fs0 (18)",
                    "gn0 (19)",
                    "gs0 (20)",
                    "an0 (21)",
                    "as0 (22)",
                    "bn0 (23)",
                    "cn1 (24)",
                    "cs1 (25)",
                    "dn1 (26)",
                    "ds1 (27)",
                    "en1 (28)",
                    "fn1 (29)",
                    "fs1 (30)",
                    "gn1 (31)",
                    "gs1 (32)",
                    "an1 (33)",
                    "as1 (34)",
                    "bn1 (35)",
                    "cn2 (36)",
                    "cs2 (37)",
                    "dn2 (38)",
                    "ds2 (39)",
                    "en2 (40)",
                    "fn2 (41)",
                    "fs2 (42)",
                    "gn2 (43)",
                    "gs2 (44)",
                    "an2 (45)",
                    "as2 (46)",
                    "bn2 (47)",
                    "cn3 (48)",
                    "cs3 (49)",
                    "dn3 (50)",
                    "ds3 (51)",
                    "en3 (52)",
                    "fn3 (53)",
                    "fs3 (54)",
                    "gn3 (55)",
                    "gs3 (56)",
                    "an3 (57)",
                    "as3 (58)",
                    "bn3 (59)",
                    "cn4 (60)",
                    "cs4 (61)",
                    "dn4 (62)",
                    "ds4 (63)",
                    "en4 (64)",
                    "fn4 (65)",
                    "fs4 (66)",
                    "gn4 (67)",
                    "gs4 (68)",
                    "an4 (69)",
                    "as4 (70)",
                    "bn4 (71)",
                    "cn5 (72)",
                    "cs5 (73)",
                    "dn5 (74)",
                    "ds5 (75)",
                    "en5 (76)",
                    "fn5 (77)",
                    "fs5 (78)",
                    "gn5 (79)",
                    "gs5 (80)",
                    "an5 (81)",
                    "as5 (82)",
                    "bn5 (83)",
                    "cn6 (84)",
                    "cs6 (85)",
                    "dn6 (86)",
                    "ds6 (87)",
                    "en6 (88)",
                    "fn6 (89)",
                    "fs6 (90)",
                    "gn6 (91)",
                    "gs6 (92)",
                    "an6 (93)",
                    "as6 (94)",
                    "bn6 (95)",
                    "cn7 (96)",
                    "cs7 (97)",
                    "dn7 (98)",
                    "ds7 (99)",
                    "en7 (100)",
                    "fn7 (101)",
                    "fs7 (102)",
                    "gn7 (103)",
                    "gs7 (104)",
                    "an7 (105)",
                    "as7 (106)",
                    "bn7 (107)",
                    "cn8 (108)",
                    "cs8 (109)",
                    "dn8 (110)",
                    "ds8 (111)",
                    "en8 (112)",
                    "fn8 (113)",
                    "fs8 (114)",
                    "gn8 (115)",
                    "gs8 (116)",
                    "an8 (117)",
                    "as8 (118)",
                    "bn8 (119)",
                    "cn9 (120)",
                    "cs9 (121)",
                    "dn9 (122)",
                    "ds9 (123)",
                    "en9 (124)",
                    "fn9 (125)",
                    "fs9 (126)",
                    "gn9 (127)",
                }
            );
            endNote.Name = "endNote";
            endNote.Width = 52;
            instrumentType.HeaderText = "Instrument Type";
            instrumentType.Items.AddRange(
                new object[] { "PCM", "PSG", "Noise", "Direct PCM", "Null" }
            );
            instrumentType.Name = "instrumentType";
            instrumentType.Width = 80;
            waveId.HeaderText = "Wave Id/PSG Cycle";
            waveId.MaxInputLength = 5;
            waveId.Name = "waveId";
            waveId.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            waveId.Width = 99;
            waveArchiveId.HeaderText = "Wave Archive Id";
            waveArchiveId.MaxInputLength = 5;
            waveArchiveId.Name = "waveArchiveId";
            waveArchiveId.SortMode = System
                .Windows
                .Forms
                .DataGridViewColumnSortMode
                .NotSortable;
            waveArchiveId.Width = 76;
            baseNote.HeaderText = "Base Note";
            baseNote.Items.AddRange(
                new object[]
                {
                    "cnm1 (0)",
                    "csm1 (1)",
                    "dnm1 (2)",
                    "dsm1 (3)",
                    "enm1 (4)",
                    "fnm1 (5)",
                    "fsm1 (6)",
                    "gnm1 (7)",
                    "gsm1 (8)",
                    "anm1 (9)",
                    "asm1 (10)",
                    "bnm1 (11)",
                    "cn0 (12)",
                    "cs0 (13)",
                    "dn0 (14)",
                    "ds0 (15)",
                    "en0 (16)",
                    "fn0 (17)",
                    "fs0 (18)",
                    "gn0 (19)",
                    "gs0 (20)",
                    "an0 (21)",
                    "as0 (22)",
                    "bn0 (23)",
                    "cn1 (24)",
                    "cs1 (25)",
                    "dn1 (26)",
                    "ds1 (27)",
                    "en1 (28)",
                    "fn1 (29)",
                    "fs1 (30)",
                    "gn1 (31)",
                    "gs1 (32)",
                    "an1 (33)",
                    "as1 (34)",
                    "bn1 (35)",
                    "cn2 (36)",
                    "cs2 (37)",
                    "dn2 (38)",
                    "ds2 (39)",
                    "en2 (40)",
                    "fn2 (41)",
                    "fs2 (42)",
                    "gn2 (43)",
                    "gs2 (44)",
                    "an2 (45)",
                    "as2 (46)",
                    "bn2 (47)",
                    "cn3 (48)",
                    "cs3 (49)",
                    "dn3 (50)",
                    "ds3 (51)",
                    "en3 (52)",
                    "fn3 (53)",
                    "fs3 (54)",
                    "gn3 (55)",
                    "gs3 (56)",
                    "an3 (57)",
                    "as3 (58)",
                    "bn3 (59)",
                    "cn4 (60)",
                    "cs4 (61)",
                    "dn4 (62)",
                    "ds4 (63)",
                    "en4 (64)",
                    "fn4 (65)",
                    "fs4 (66)",
                    "gn4 (67)",
                    "gs4 (68)",
                    "an4 (69)",
                    "as4 (70)",
                    "bn4 (71)",
                    "cn5 (72)",
                    "cs5 (73)",
                    "dn5 (74)",
                    "ds5 (75)",
                    "en5 (76)",
                    "fn5 (77)",
                    "fs5 (78)",
                    "gn5 (79)",
                    "gs5 (80)",
                    "an5 (81)",
                    "as5 (82)",
                    "bn5 (83)",
                    "cn6 (84)",
                    "cs6 (85)",
                    "dn6 (86)",
                    "ds6 (87)",
                    "en6 (88)",
                    "fn6 (89)",
                    "fs6 (90)",
                    "gn6 (91)",
                    "gs6 (92)",
                    "an6 (93)",
                    "as6 (94)",
                    "bn6 (95)",
                    "cn7 (96)",
                    "cs7 (97)",
                    "dn7 (98)",
                    "ds7 (99)",
                    "en7 (100)",
                    "fn7 (101)",
                    "fs7 (102)",
                    "gn7 (103)",
                    "gs7 (104)",
                    "an7 (105)",
                    "as7 (106)",
                    "bn7 (107)",
                    "cn8 (108)",
                    "cs8 (109)",
                    "dn8 (110)",
                    "ds8 (111)",
                    "en8 (112)",
                    "fn8 (113)",
                    "fs8 (114)",
                    "gn8 (115)",
                    "gs8 (116)",
                    "an8 (117)",
                    "as8 (118)",
                    "bn8 (119)",
                    "cn9 (120)",
                    "cs9 (121)",
                    "dn9 (122)",
                    "ds9 (123)",
                    "en9 (124)",
                    "fn9 (125)",
                    "fs9 (126)",
                    "gn9 (127)",
                }
            );
            baseNote.Name = "baseNote";
            baseNote.Width = 57;
            attack.HeaderText = "Attack";
            attack.MaxInputLength = 3;
            attack.Name = "attack";
            attack.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            attack.Width = 44;
            decay.HeaderText = "Decay";
            decay.MaxInputLength = 3;
            decay.Name = "decay";
            decay.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            decay.Width = 44;
            sustain.HeaderText = "Sustain";
            sustain.MaxInputLength = 3;
            sustain.Name = "sustain";
            sustain.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            sustain.Width = 48;
            release.HeaderText = "Release";
            release.MaxInputLength = 3;
            release.Name = "release";
            release.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            release.Width = 52;
            pan.HeaderText = "Pan";
            pan.MaxInputLength = 3;
            pan.Name = "pan";
            pan.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            pan.Width = 32;
            label32.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label32.Location = new System.Drawing.Point(11, 118);
            label32.Name = "label32";
            label32.Size = new System.Drawing.Size(301, 20);
            label32.TabIndex = 25;
            label32.Text = "Regions:";
            label32.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel15.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel15.ColumnCount = 2;
            _ = tableLayoutPanel15.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel15.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel15.Controls.Add(drumSetStartRangeComboBox, 0, 0);
            tableLayoutPanel15.Controls.Add(drumSetStartRangeBox, 1, 0);
            tableLayoutPanel15.Location = new System.Drawing.Point(14, 84);
            tableLayoutPanel15.Name = "tableLayoutPanel15";
            tableLayoutPanel15.RowCount = 1;
            _ = tableLayoutPanel15.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel15.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel15.TabIndex = 24;
            drumSetStartRangeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            drumSetStartRangeComboBox.DropDownStyle = System
                .Windows
                .Forms
                .ComboBoxStyle
                .DropDownList;
            drumSetStartRangeComboBox.FormattingEnabled = true;
            drumSetStartRangeComboBox.Items.AddRange(
                new object[]
                {
                    "cnm1",
                    "csm1",
                    "dnm1",
                    "dsm1",
                    "enm1",
                    "fnm1",
                    "fsm1",
                    "gnm1",
                    "gsm1",
                    "anm1",
                    "asm1",
                    "bnm1",
                    "cn0",
                    "cs0",
                    "dn0",
                    "ds0",
                    "en0",
                    "fn0",
                    "fs0",
                    "gn0",
                    "gs0",
                    "an0",
                    "as0",
                    "bn0",
                    "cn1",
                    "cs1",
                    "dn1",
                    "ds1",
                    "en1",
                    "fn1",
                    "fs1",
                    "gn1",
                    "gs1",
                    "an1",
                    "as1",
                    "bn1",
                    "cn2",
                    "cs2",
                    "dn2",
                    "ds2",
                    "en2",
                    "fn2",
                    "fs2",
                    "gn2",
                    "gs2",
                    "an2",
                    "as2",
                    "bn2",
                    "cn3",
                    "cs3",
                    "dn3",
                    "ds3",
                    "en3",
                    "fn3",
                    "fs3",
                    "gn3",
                    "gs3",
                    "an3",
                    "as3",
                    "bn3",
                    "cn4",
                    "cs4",
                    "dn4",
                    "ds4",
                    "en4",
                    "fn4",
                    "fs4",
                    "gn4",
                    "gs4",
                    "an4",
                    "as4",
                    "bn4",
                    "cn5",
                    "cs5",
                    "dn5",
                    "ds5",
                    "en5",
                    "fn5",
                    "fs5",
                    "gn5",
                    "gs5",
                    "an5",
                    "as5",
                    "bn5",
                    "cn6",
                    "cs6",
                    "dn6",
                    "ds6",
                    "en6",
                    "fn6",
                    "fs6",
                    "gn6",
                    "gs6",
                    "an6",
                    "as6",
                    "bn6",
                    "cn7",
                    "cs7",
                    "dn7",
                    "ds7",
                    "en7",
                    "fn7",
                    "fs7",
                    "gn7",
                    "gs7",
                    "an7",
                    "as7",
                    "bn7",
                    "cn8",
                    "cs8",
                    "dn8",
                    "ds8",
                    "en8",
                    "fn8",
                    "fs8",
                    "gn8",
                    "gs8",
                    "an8",
                    "as8",
                    "bn8",
                    "cn9",
                    "cs9",
                    "dn9",
                    "ds9",
                    "en9",
                    "fn9",
                    "fs9",
                    "gn9",
                }
            );
            drumSetStartRangeComboBox.Location = new System.Drawing.Point(3, 3);
            drumSetStartRangeComboBox.Name = "drumSetStartRangeComboBox";
            drumSetStartRangeComboBox.Size = new System.Drawing.Size(247, 21);
            drumSetStartRangeComboBox.TabIndex = 6;
            toolTip.SetToolTip(
                drumSetStartRangeComboBox,
                "What note to start the drum set range at."
            );
            drumSetStartRangeBox.Dock = System.Windows.Forms.DockStyle.Fill;
            drumSetStartRangeBox.Location = new System.Drawing.Point(256, 3);
            drumSetStartRangeBox.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            drumSetStartRangeBox.Name = "drumSetStartRangeBox";
            drumSetStartRangeBox.Size = new System.Drawing.Size(39, 20);
            drumSetStartRangeBox.TabIndex = 7;
            toolTip.SetToolTip(
                drumSetStartRangeBox,
                "What note to start the drum set range at."
            );
            drumSetRangeStartLabel.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            drumSetRangeStartLabel.Location = new System.Drawing.Point(8, 61);
            drumSetRangeStartLabel.Name = "drumSetRangeStartLabel";
            drumSetRangeStartLabel.Size = new System.Drawing.Size(301, 20);
            drumSetRangeStartLabel.TabIndex = 3;
            drumSetRangeStartLabel.Text = "Drum Set Range Start:";
            drumSetRangeStartLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel14.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel14.ColumnCount = 3;
            _ = tableLayoutPanel14.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    33.33333F
                )
            );
            _ = tableLayoutPanel14.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    33.33333F
                )
            );
            _ = tableLayoutPanel14.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    33.33333F
                )
            );
            tableLayoutPanel14.Controls.Add(keySplitBox, 2, 0);
            tableLayoutPanel14.Controls.Add(drumSetBox, 1, 0);
            tableLayoutPanel14.Controls.Add(directBox, 0, 0);
            tableLayoutPanel14.Location = new System.Drawing.Point(14, 28);
            tableLayoutPanel14.Name = "tableLayoutPanel14";
            tableLayoutPanel14.RowCount = 1;
            _ = tableLayoutPanel14.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel14.Size = new System.Drawing.Size(298, 28);
            tableLayoutPanel14.TabIndex = 2;
            toolTip.SetToolTip(tableLayoutPanel14, "Type of instrument.");
            keySplitBox.Dock = System.Windows.Forms.DockStyle.Fill;
            keySplitBox.Location = new System.Drawing.Point(201, 3);
            keySplitBox.Name = "keySplitBox";
            keySplitBox.Size = new System.Drawing.Size(94, 22);
            keySplitBox.TabIndex = 2;
            keySplitBox.TabStop = true;
            keySplitBox.Text = "Key Split";
            keySplitBox.UseVisualStyleBackColor = true;
            drumSetBox.Dock = System.Windows.Forms.DockStyle.Fill;
            drumSetBox.Location = new System.Drawing.Point(102, 3);
            drumSetBox.Name = "drumSetBox";
            drumSetBox.Size = new System.Drawing.Size(93, 22);
            drumSetBox.TabIndex = 1;
            drumSetBox.TabStop = true;
            drumSetBox.Text = "Drum Set";
            drumSetBox.UseVisualStyleBackColor = true;
            directBox.Dock = System.Windows.Forms.DockStyle.Fill;
            directBox.Location = new System.Drawing.Point(3, 3);
            directBox.Name = "directBox";
            directBox.Size = new System.Drawing.Size(93, 22);
            directBox.TabIndex = 0;
            directBox.TabStop = true;
            directBox.Text = "Direct";
            directBox.UseVisualStyleBackColor = true;
            label30.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label30.Location = new System.Drawing.Point(11, 3);
            label30.Name = "label30";
            label30.Size = new System.Drawing.Size(301, 20);
            label30.TabIndex = 1;
            label30.Text = "Instrument Type:";
            label30.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            seqArcSeqPanel.Controls.Add(label29);
            seqArcSeqPanel.Controls.Add(tableLayoutPanel13);
            seqArcSeqPanel.Dock = System.Windows.Forms.DockStyle.Top;
            seqArcSeqPanel.Location = new System.Drawing.Point(0, 270);
            seqArcSeqPanel.Name = "seqArcSeqPanel";
            seqArcSeqPanel.Size = new System.Drawing.Size(325, 64);
            seqArcSeqPanel.TabIndex = 20;
            seqArcSeqPanel.Visible = false;
            label29.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label29.Location = new System.Drawing.Point(11, 3);
            label29.Name = "label29";
            label29.Size = new System.Drawing.Size(301, 20);
            label29.TabIndex = 25;
            label29.Text = "Preview Sequence:";
            label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel13.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel13.ColumnCount = 2;
            _ = tableLayoutPanel13.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel13.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel13.Controls.Add(seqArcSeqComboBox, 0, 0);
            tableLayoutPanel13.Controls.Add(seqArcSeqBox, 1, 0);
            tableLayoutPanel13.Location = new System.Drawing.Point(14, 25);
            tableLayoutPanel13.Name = "tableLayoutPanel13";
            tableLayoutPanel13.RowCount = 1;
            _ = tableLayoutPanel13.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel13.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel13.TabIndex = 24;
            seqArcSeqComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            seqArcSeqComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            seqArcSeqComboBox.FormattingEnabled = true;
            seqArcSeqComboBox.Location = new System.Drawing.Point(3, 3);
            seqArcSeqComboBox.Name = "seqArcSeqComboBox";
            seqArcSeqComboBox.Size = new System.Drawing.Size(247, 21);
            seqArcSeqComboBox.TabIndex = 6;
            toolTip.SetToolTip(seqArcSeqComboBox, "Sequence to play.");
            seqArcSeqBox.Dock = System.Windows.Forms.DockStyle.Fill;
            seqArcSeqBox.Location = new System.Drawing.Point(256, 3);
            seqArcSeqBox.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            seqArcSeqBox.Name = "seqArcSeqBox";
            seqArcSeqBox.Size = new System.Drawing.Size(39, 20);
            seqArcSeqBox.TabIndex = 7;
            toolTip.SetToolTip(seqArcSeqBox, "Id of the sequence to play.");
            seqArcPanel.Controls.Add(seqArcOpenFileButton);
            seqArcPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            seqArcPanel.Location = new System.Drawing.Point(0, 270);
            seqArcPanel.Name = "seqArcPanel";
            seqArcPanel.Size = new System.Drawing.Size(325, 268);
            seqArcPanel.TabIndex = 19;
            seqArcPanel.Visible = false;
            seqArcOpenFileButton.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            seqArcOpenFileButton.Location = new System.Drawing.Point(14, 6);
            seqArcOpenFileButton.Name = "seqArcOpenFileButton";
            seqArcOpenFileButton.Size = new System.Drawing.Size(298, 25);
            seqArcOpenFileButton.TabIndex = 1;
            seqArcOpenFileButton.Text = "Open File";
            seqArcOpenFileButton.UseVisualStyleBackColor = true;
            seqPanel.Controls.Add(tableLayoutPanel11);
            seqPanel.Controls.Add(label27);
            seqPanel.Controls.Add(seqPlayerPriorityBox);
            seqPanel.Controls.Add(label26);
            seqPanel.Controls.Add(seqChannelPriorityBox);
            seqPanel.Controls.Add(label25);
            seqPanel.Controls.Add(seqVolumeBox);
            seqPanel.Controls.Add(label24);
            seqPanel.Controls.Add(tableLayoutPanel10);
            seqPanel.Controls.Add(label23);
            seqPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            seqPanel.Location = new System.Drawing.Point(0, 270);
            seqPanel.Name = "seqPanel";
            seqPanel.Size = new System.Drawing.Size(325, 268);
            seqPanel.TabIndex = 17;
            seqPanel.Visible = false;
            tableLayoutPanel11.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel11.ColumnCount = 2;
            _ = tableLayoutPanel11.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel11.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel11.Controls.Add(seqPlayerComboBox, 0, 0);
            tableLayoutPanel11.Controls.Add(seqPlayerBox, 1, 0);
            tableLayoutPanel11.Location = new System.Drawing.Point(14, 220);
            tableLayoutPanel11.Name = "tableLayoutPanel11";
            tableLayoutPanel11.RowCount = 1;
            _ = tableLayoutPanel11.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel11.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel11.TabIndex = 23;
            seqPlayerComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            seqPlayerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            seqPlayerComboBox.FormattingEnabled = true;
            seqPlayerComboBox.Location = new System.Drawing.Point(3, 3);
            seqPlayerComboBox.Name = "seqPlayerComboBox";
            seqPlayerComboBox.Size = new System.Drawing.Size(247, 21);
            seqPlayerComboBox.TabIndex = 6;
            toolTip.SetToolTip(seqPlayerComboBox, "Player to play the sequence.");
            seqPlayerBox.Dock = System.Windows.Forms.DockStyle.Fill;
            seqPlayerBox.Location = new System.Drawing.Point(256, 3);
            seqPlayerBox.Maximum = new decimal(new int[] { 31, 0, 0, 0 });
            seqPlayerBox.Name = "seqPlayerBox";
            seqPlayerBox.Size = new System.Drawing.Size(39, 20);
            seqPlayerBox.TabIndex = 7;
            toolTip.SetToolTip(seqPlayerBox, "Id of the player to play the sequence.");
            label27.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label27.Location = new System.Drawing.Point(11, 198);
            label27.Name = "label27";
            label27.Size = new System.Drawing.Size(301, 22);
            label27.TabIndex = 22;
            label27.Text = "Player:";
            label27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            seqPlayerPriorityBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            seqPlayerPriorityBox.Location = new System.Drawing.Point(14, 175);
            seqPlayerPriorityBox.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            seqPlayerPriorityBox.Name = "seqPlayerPriorityBox";
            seqPlayerPriorityBox.Size = new System.Drawing.Size(298, 20);
            seqPlayerPriorityBox.TabIndex = 21;
            toolTip.SetToolTip(
                seqPlayerPriorityBox,
                "If the sounds can not all be played at once, the one with the highest priority wi"
                    + "ll play."
            );
            label26.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label26.Location = new System.Drawing.Point(11, 152);
            label26.Name = "label26";
            label26.Size = new System.Drawing.Size(301, 22);
            label26.TabIndex = 20;
            label26.Text = "Player Priority:";
            label26.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            seqChannelPriorityBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            seqChannelPriorityBox.Location = new System.Drawing.Point(14, 129);
            seqChannelPriorityBox.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            seqChannelPriorityBox.Name = "seqChannelPriorityBox";
            seqChannelPriorityBox.Size = new System.Drawing.Size(298, 20);
            seqChannelPriorityBox.TabIndex = 19;
            toolTip.SetToolTip(
                seqChannelPriorityBox,
                "If the sounds can not all be played at once, the one with the highest priority wi"
                    + "ll play."
            );
            label25.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label25.Location = new System.Drawing.Point(11, 106);
            label25.Name = "label25";
            label25.Size = new System.Drawing.Size(301, 22);
            label25.TabIndex = 18;
            label25.Text = "Channel Priority:";
            label25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            seqVolumeBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            seqVolumeBox.Location = new System.Drawing.Point(14, 82);
            seqVolumeBox.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            seqVolumeBox.Name = "seqVolumeBox";
            seqVolumeBox.Size = new System.Drawing.Size(298, 20);
            seqVolumeBox.TabIndex = 17;
            toolTip.SetToolTip(seqVolumeBox, "The volume of the sequence.");
            label24.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label24.Location = new System.Drawing.Point(11, 59);
            label24.Name = "label24";
            label24.Size = new System.Drawing.Size(301, 22);
            label24.TabIndex = 16;
            label24.Text = "Volume:";
            label24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel10.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel10.ColumnCount = 2;
            _ = tableLayoutPanel10.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel10.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel10.Controls.Add(seqBankComboBox, 0, 0);
            tableLayoutPanel10.Controls.Add(seqBankBox, 1, 0);
            tableLayoutPanel10.Location = new System.Drawing.Point(14, 25);
            tableLayoutPanel10.Name = "tableLayoutPanel10";
            tableLayoutPanel10.RowCount = 1;
            _ = tableLayoutPanel10.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel10.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel10.TabIndex = 15;
            seqBankComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            seqBankComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            seqBankComboBox.FormattingEnabled = true;
            seqBankComboBox.Location = new System.Drawing.Point(3, 3);
            seqBankComboBox.Name = "seqBankComboBox";
            seqBankComboBox.Size = new System.Drawing.Size(247, 21);
            seqBankComboBox.TabIndex = 6;
            toolTip.SetToolTip(seqBankComboBox, "Bank to use with the sequence.");
            seqBankBox.Dock = System.Windows.Forms.DockStyle.Fill;
            seqBankBox.Location = new System.Drawing.Point(256, 3);
            seqBankBox.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            seqBankBox.Name = "seqBankBox";
            seqBankBox.Size = new System.Drawing.Size(39, 20);
            seqBankBox.TabIndex = 7;
            toolTip.SetToolTip(seqBankBox, "Id of the bank to use with the sequence.");
            label23.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label23.Location = new System.Drawing.Point(11, 3);
            label23.Name = "label23";
            label23.Size = new System.Drawing.Size(301, 22);
            label23.TabIndex = 2;
            label23.Text = "Bank:";
            label23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            playerPanel.Controls.Add(tableLayoutPanel8);
            playerPanel.Controls.Add(label19);
            playerPanel.Controls.Add(playerHeapSizeBox);
            playerPanel.Controls.Add(label18);
            playerPanel.Controls.Add(playerMaxSequencesBox);
            playerPanel.Controls.Add(label17);
            playerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            playerPanel.Location = new System.Drawing.Point(0, 270);
            playerPanel.Name = "playerPanel";
            playerPanel.Size = new System.Drawing.Size(325, 268);
            playerPanel.TabIndex = 15;
            playerPanel.Visible = false;
            tableLayoutPanel8.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel8.ColumnCount = 4;
            _ = tableLayoutPanel8.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F)
            );
            _ = tableLayoutPanel8.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F)
            );
            _ = tableLayoutPanel8.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F)
            );
            _ = tableLayoutPanel8.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F)
            );
            // Add player flags to the 4x4 grid
            for (int _i = 0; _i < 16; _i++)
            {
                int _col = _i % 4;
                int _row = _i / 4;
                var _cb  = playerFlagBoxes[_i];
                tableLayoutPanel8.Controls.Add(_cb, _col, _row);
                _cb.AutoSize = true;
                _cb.Location = new System.Drawing.Point(_col * 74 + 3, _row * 25 + 3);
                _cb.Name     = "playerFlag" + _i + "Box";
                _cb.Size     = new System.Drawing.Size(_i < 10 ? 32 : 38, 17);
                _cb.TabIndex = _i;
                _cb.Text     = _i.ToString();
                _cb.UseVisualStyleBackColor = true;
            }
            label19.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label19.Location = new System.Drawing.Point(11, 93);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(301, 22);
            label19.TabIndex = 9;
            label19.Text = "Channel Flags:";
            label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            playerHeapSizeBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            playerHeapSizeBox.Hexadecimal = true;
            playerHeapSizeBox.Location = new System.Drawing.Point(14, 70);
            playerHeapSizeBox.Maximum = new decimal(new int[] { -1, 0, 0, 0 });
            playerHeapSizeBox.Name = "playerHeapSizeBox";
            playerHeapSizeBox.Size = new System.Drawing.Size(298, 20);
            playerHeapSizeBox.TabIndex = 8;
            toolTip.SetToolTip(
                playerHeapSizeBox,
                "How much memory to reserve in the sound heap for the player."
            );
            label18.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label18.Location = new System.Drawing.Point(11, 48);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(301, 22);
            label18.TabIndex = 7;
            label18.Text = "Heap Size:";
            label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            playerMaxSequencesBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            playerMaxSequencesBox.Location = new System.Drawing.Point(14, 25);
            playerMaxSequencesBox.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            playerMaxSequencesBox.Name = "playerMaxSequencesBox";
            playerMaxSequencesBox.Size = new System.Drawing.Size(298, 20);
            playerMaxSequencesBox.TabIndex = 6;
            toolTip.SetToolTip(
                playerMaxSequencesBox,
                "Max number of sequences the player can play."
            );
            label17.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label17.Location = new System.Drawing.Point(11, 3);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(301, 22);
            label17.TabIndex = 5;
            label17.Text = "Max Sequences:";
            label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            stmPanel.Controls.Add(stmMonoToStereoBox);
            stmPanel.Controls.Add(label16);
            stmPanel.Controls.Add(label15);
            stmPanel.Controls.Add(tableLayoutPanel7);
            stmPanel.Controls.Add(stmPriorityBox);
            stmPanel.Controls.Add(label14);
            stmPanel.Controls.Add(stmVolumeBox);
            stmPanel.Controls.Add(label13);
            stmPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            stmPanel.Location = new System.Drawing.Point(0, 270);
            stmPanel.Name = "stmPanel";
            stmPanel.Size = new System.Drawing.Size(325, 268);
            stmPanel.TabIndex = 14;
            stmPanel.Visible = false;
            stmMonoToStereoBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            stmMonoToStereoBox.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            stmMonoToStereoBox.Location = new System.Drawing.Point(11, 171);
            stmMonoToStereoBox.Name = "stmMonoToStereoBox";
            stmMonoToStereoBox.Size = new System.Drawing.Size(301, 24);
            stmMonoToStereoBox.TabIndex = 17;
            toolTip.SetToolTip(
                stmMonoToStereoBox,
                "If the stream is mono, play it through two channels."
            );
            stmMonoToStereoBox.UseVisualStyleBackColor = true;
            label16.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label16.Location = new System.Drawing.Point(11, 149);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(301, 22);
            label16.TabIndex = 16;
            label16.Text = "Mono To Stereo:";
            label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            label15.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label15.Location = new System.Drawing.Point(11, 93);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(301, 22);
            label15.TabIndex = 15;
            label15.Text = "Player:";
            label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel7.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel7.ColumnCount = 2;
            _ = tableLayoutPanel7.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel7.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel7.Controls.Add(stmPlayerComboBox, 0, 0);
            tableLayoutPanel7.Controls.Add(stmPlayerBox, 1, 0);
            tableLayoutPanel7.Location = new System.Drawing.Point(14, 115);
            tableLayoutPanel7.Name = "tableLayoutPanel7";
            tableLayoutPanel7.RowCount = 1;
            _ = tableLayoutPanel7.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel7.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel7.TabIndex = 14;
            stmPlayerComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            stmPlayerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            stmPlayerComboBox.FormattingEnabled = true;
            stmPlayerComboBox.Location = new System.Drawing.Point(3, 3);
            stmPlayerComboBox.Name = "stmPlayerComboBox";
            stmPlayerComboBox.Size = new System.Drawing.Size(247, 21);
            stmPlayerComboBox.TabIndex = 6;
            toolTip.SetToolTip(stmPlayerComboBox, "The player to play the stream.");
            stmPlayerBox.Dock = System.Windows.Forms.DockStyle.Fill;
            stmPlayerBox.Location = new System.Drawing.Point(256, 3);
            stmPlayerBox.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            stmPlayerBox.Name = "stmPlayerBox";
            stmPlayerBox.Size = new System.Drawing.Size(39, 20);
            stmPlayerBox.TabIndex = 7;
            toolTip.SetToolTip(stmPlayerBox, "Id of the player to play the stream.");
            stmPriorityBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            stmPriorityBox.Location = new System.Drawing.Point(14, 70);
            stmPriorityBox.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            stmPriorityBox.Name = "stmPriorityBox";
            stmPriorityBox.Size = new System.Drawing.Size(298, 20);
            stmPriorityBox.TabIndex = 7;
            toolTip.SetToolTip(
                stmPriorityBox,
                "If the sounds can not all be played at once, the one with the highest priority wi"
                    + "ll play."
            );
            label14.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label14.Location = new System.Drawing.Point(11, 48);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(301, 22);
            label14.TabIndex = 6;
            label14.Text = "Priority:";
            label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            stmVolumeBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            stmVolumeBox.Location = new System.Drawing.Point(14, 25);
            stmVolumeBox.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            stmVolumeBox.Name = "stmVolumeBox";
            stmVolumeBox.Size = new System.Drawing.Size(298, 20);
            stmVolumeBox.TabIndex = 5;
            toolTip.SetToolTip(stmVolumeBox, "The volume of the stream.");
            label13.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label13.Location = new System.Drawing.Point(11, 3);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(301, 22);
            label13.TabIndex = 4;
            label13.Text = "Volume:";
            label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            streamPlayerPanel.Controls.Add(stmPlayerChannelType);
            streamPlayerPanel.Controls.Add(label12);
            streamPlayerPanel.Controls.Add(tableLayoutPanel6);
            streamPlayerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            streamPlayerPanel.Location = new System.Drawing.Point(0, 270);
            streamPlayerPanel.Name = "streamPlayerPanel";
            streamPlayerPanel.Size = new System.Drawing.Size(325, 268);
            streamPlayerPanel.TabIndex = 13;
            streamPlayerPanel.Visible = false;
            stmPlayerChannelType.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            stmPlayerChannelType.DropDownStyle = System
                .Windows
                .Forms
                .ComboBoxStyle
                .DropDownList;
            stmPlayerChannelType.FormattingEnabled = true;
            stmPlayerChannelType.Items.AddRange(new object[] { "Mono", "Stereo" });
            stmPlayerChannelType.Location = new System.Drawing.Point(14, 28);
            stmPlayerChannelType.Name = "stmPlayerChannelType";
            stmPlayerChannelType.Size = new System.Drawing.Size(298, 21);
            stmPlayerChannelType.TabIndex = 4;
            toolTip.SetToolTip(stmPlayerChannelType, "If the stream is stereo or mono.");
            label12.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label12.Location = new System.Drawing.Point(11, 3);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(301, 22);
            label12.TabIndex = 3;
            label12.Text = "Channel Type:";
            label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel6.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel6.ColumnCount = 2;
            _ = tableLayoutPanel6.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            _ = tableLayoutPanel6.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            tableLayoutPanel6.Controls.Add(stmPlayerLeftChannelBox, 0, 1);
            tableLayoutPanel6.Controls.Add(stmPlayerRightChannelBox, 0, 1);
            tableLayoutPanel6.Controls.Add(rightChannelLabel, 1, 0);
            tableLayoutPanel6.Controls.Add(leftChannelLabel, 0, 0);
            tableLayoutPanel6.Location = new System.Drawing.Point(14, 55);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 2;
            _ = tableLayoutPanel6.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            _ = tableLayoutPanel6.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            tableLayoutPanel6.Size = new System.Drawing.Size(298, 45);
            tableLayoutPanel6.TabIndex = 0;
            stmPlayerLeftChannelBox.Dock = System.Windows.Forms.DockStyle.Fill;
            stmPlayerLeftChannelBox.Location = new System.Drawing.Point(3, 25);
            stmPlayerLeftChannelBox.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
            stmPlayerLeftChannelBox.Name = "stmPlayerLeftChannelBox";
            stmPlayerLeftChannelBox.Size = new System.Drawing.Size(143, 20);
            stmPlayerLeftChannelBox.TabIndex = 4;
            toolTip.SetToolTip(stmPlayerLeftChannelBox, "Channel to use for the stream.");
            stmPlayerRightChannelBox.Dock = System.Windows.Forms.DockStyle.Fill;
            stmPlayerRightChannelBox.Location = new System.Drawing.Point(152, 25);
            stmPlayerRightChannelBox.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
            stmPlayerRightChannelBox.Name = "stmPlayerRightChannelBox";
            stmPlayerRightChannelBox.Size = new System.Drawing.Size(143, 20);
            stmPlayerRightChannelBox.TabIndex = 3;
            toolTip.SetToolTip(
                stmPlayerRightChannelBox,
                "Channel to use for the stream."
            );
            rightChannelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightChannelLabel.Location = new System.Drawing.Point(152, 0);
            rightChannelLabel.Name = "rightChannelLabel";
            rightChannelLabel.Size = new System.Drawing.Size(143, 22);
            rightChannelLabel.TabIndex = 2;
            rightChannelLabel.Text = "Right Channel:";
            rightChannelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            leftChannelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            leftChannelLabel.Location = new System.Drawing.Point(3, 0);
            leftChannelLabel.Name = "leftChannelLabel";
            leftChannelLabel.Size = new System.Drawing.Size(143, 22);
            leftChannelLabel.TabIndex = 1;
            leftChannelLabel.Text = "Left Channel:";
            leftChannelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            grpPanel.Controls.Add(grpEntries);
            grpPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            grpPanel.Location = new System.Drawing.Point(0, 270);
            grpPanel.Name = "grpPanel";
            grpPanel.Size = new System.Drawing.Size(325, 268);
            grpPanel.TabIndex = 12;
            grpPanel.Visible = false;
            grpEntries.AllowUserToOrderColumns = true;
            grpEntries.AllowUserToResizeRows = false;
            grpEntries.ColumnHeadersHeightSizeMode = System
                .Windows
                .Forms
                .DataGridViewColumnHeadersHeightSizeMode
                .AutoSize;
            grpEntries.Columns.AddRange(
                new System.Windows.Forms.DataGridViewColumn[] { item, loadFlags }
            );
            grpEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            grpEntries.Location = new System.Drawing.Point(0, 0);
            grpEntries.Name = "grpEntries";
            grpEntries.Size = new System.Drawing.Size(325, 268);
            grpEntries.TabIndex = 0;
            item.FillWeight = 1750F;
            item.HeaderText = "Item";
            item.Name = "item";
            item.Width = 175;
            loadFlags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            loadFlags.HeaderText = "Load Flags";
            loadFlags.Name = "loadFlags";
            loadFlags.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            loadFlags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            bankPanel.Controls.Add(tableLayoutPanel5);
            bankPanel.Controls.Add(label11);
            bankPanel.Controls.Add(tableLayoutPanel4);
            bankPanel.Controls.Add(label10);
            bankPanel.Controls.Add(tableLayoutPanel3);
            bankPanel.Controls.Add(label7);
            bankPanel.Controls.Add(tableLayoutPanel2);
            bankPanel.Controls.Add(label6);
            bankPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            bankPanel.Location = new System.Drawing.Point(0, 270);
            bankPanel.Name = "bankPanel";
            bankPanel.Size = new System.Drawing.Size(325, 268);
            bankPanel.TabIndex = 11;
            bankPanel.Visible = false;
            tableLayoutPanel5.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel5.ColumnCount = 2;
            _ = tableLayoutPanel5.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel5.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel5.Controls.Add(bnkWar3ComboBox, 0, 0);
            tableLayoutPanel5.Controls.Add(bnkWar3Box, 1, 0);
            tableLayoutPanel5.Location = new System.Drawing.Point(14, 193);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 1;
            _ = tableLayoutPanel5.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel5.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel5.TabIndex = 13;
            bnkWar3ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            bnkWar3ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            bnkWar3ComboBox.FormattingEnabled = true;
            bnkWar3ComboBox.Location = new System.Drawing.Point(3, 3);
            bnkWar3ComboBox.Name = "bnkWar3ComboBox";
            bnkWar3ComboBox.Size = new System.Drawing.Size(247, 21);
            bnkWar3ComboBox.TabIndex = 6;
            toolTip.SetToolTip(bnkWar3ComboBox, "Wave archive to be used for the bank.");
            bnkWar3Box.Dock = System.Windows.Forms.DockStyle.Fill;
            bnkWar3Box.Location = new System.Drawing.Point(256, 3);
            bnkWar3Box.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            bnkWar3Box.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            bnkWar3Box.Name = "bnkWar3Box";
            bnkWar3Box.Size = new System.Drawing.Size(39, 20);
            bnkWar3Box.TabIndex = 7;
            toolTip.SetToolTip(
                bnkWar3Box,
                "Id of the wave archive to use for this bank."
            );
            label11.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label11.Location = new System.Drawing.Point(11, 171);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(301, 22);
            label11.TabIndex = 12;
            label11.Text = "Wave Archive 3:";
            label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel4.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel4.ColumnCount = 2;
            _ = tableLayoutPanel4.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel4.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel4.Controls.Add(bnkWar2ComboBox, 0, 0);
            tableLayoutPanel4.Controls.Add(bnkWar2Box, 1, 0);
            tableLayoutPanel4.Location = new System.Drawing.Point(14, 137);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            _ = tableLayoutPanel4.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel4.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel4.TabIndex = 11;
            bnkWar2ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            bnkWar2ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            bnkWar2ComboBox.FormattingEnabled = true;
            bnkWar2ComboBox.Location = new System.Drawing.Point(3, 3);
            bnkWar2ComboBox.Name = "bnkWar2ComboBox";
            bnkWar2ComboBox.Size = new System.Drawing.Size(247, 21);
            bnkWar2ComboBox.TabIndex = 6;
            toolTip.SetToolTip(bnkWar2ComboBox, "Wave archive to be used for the bank.");
            bnkWar2Box.Dock = System.Windows.Forms.DockStyle.Fill;
            bnkWar2Box.Location = new System.Drawing.Point(256, 3);
            bnkWar2Box.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            bnkWar2Box.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            bnkWar2Box.Name = "bnkWar2Box";
            bnkWar2Box.Size = new System.Drawing.Size(39, 20);
            bnkWar2Box.TabIndex = 7;
            toolTip.SetToolTip(
                bnkWar2Box,
                "Id of the wave archive to use for this bank."
            );
            label10.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label10.Location = new System.Drawing.Point(11, 115);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(301, 22);
            label10.TabIndex = 10;
            label10.Text = "Wave Archive 2:";
            label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel3.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel3.ColumnCount = 2;
            _ = tableLayoutPanel3.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel3.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel3.Controls.Add(bnkWar1ComboBox, 0, 0);
            tableLayoutPanel3.Controls.Add(bnkWar1Box, 1, 0);
            tableLayoutPanel3.Location = new System.Drawing.Point(14, 81);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            _ = tableLayoutPanel3.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel3.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel3.TabIndex = 9;
            bnkWar1ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            bnkWar1ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            bnkWar1ComboBox.FormattingEnabled = true;
            bnkWar1ComboBox.Location = new System.Drawing.Point(3, 3);
            bnkWar1ComboBox.Name = "bnkWar1ComboBox";
            bnkWar1ComboBox.Size = new System.Drawing.Size(247, 21);
            bnkWar1ComboBox.TabIndex = 6;
            toolTip.SetToolTip(bnkWar1ComboBox, "Wave archive to be used for the bank.");
            bnkWar1Box.Dock = System.Windows.Forms.DockStyle.Fill;
            bnkWar1Box.Location = new System.Drawing.Point(256, 3);
            bnkWar1Box.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            bnkWar1Box.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            bnkWar1Box.Name = "bnkWar1Box";
            bnkWar1Box.Size = new System.Drawing.Size(39, 20);
            bnkWar1Box.TabIndex = 7;
            toolTip.SetToolTip(
                bnkWar1Box,
                "Id of the wave archive to use for this bank."
            );
            label7.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label7.Location = new System.Drawing.Point(11, 59);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(301, 22);
            label7.TabIndex = 8;
            label7.Text = "Wave Archive 1:";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel2.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel2.ColumnCount = 2;
            _ = tableLayoutPanel2.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel2.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel2.Controls.Add(bnkWar0ComboBox, 0, 0);
            tableLayoutPanel2.Controls.Add(bnkWar0Box, 1, 0);
            tableLayoutPanel2.Location = new System.Drawing.Point(14, 25);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            _ = tableLayoutPanel2.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel2.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel2.TabIndex = 7;
            bnkWar0ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            bnkWar0ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            bnkWar0ComboBox.FormattingEnabled = true;
            bnkWar0ComboBox.Location = new System.Drawing.Point(3, 3);
            bnkWar0ComboBox.Name = "bnkWar0ComboBox";
            bnkWar0ComboBox.Size = new System.Drawing.Size(247, 21);
            bnkWar0ComboBox.TabIndex = 6;
            toolTip.SetToolTip(bnkWar0ComboBox, "Wave archive to be used for the bank.");
            bnkWar0Box.Dock = System.Windows.Forms.DockStyle.Fill;
            bnkWar0Box.Location = new System.Drawing.Point(256, 3);
            bnkWar0Box.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            bnkWar0Box.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            bnkWar0Box.Name = "bnkWar0Box";
            bnkWar0Box.Size = new System.Drawing.Size(39, 20);
            bnkWar0Box.TabIndex = 7;
            toolTip.SetToolTip(
                bnkWar0Box,
                "Id of the wave archive to use for this bank."
            );
            label6.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label6.Location = new System.Drawing.Point(11, 3);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(301, 22);
            label6.TabIndex = 2;
            label6.Text = "Wave Archive 0:";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            blankPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            blankPanel.Location = new System.Drawing.Point(0, 270);
            blankPanel.Name = "blankPanel";
            blankPanel.Size = new System.Drawing.Size(325, 268);
            blankPanel.TabIndex = 10;
            blankPanel.Visible = false;
            warPanel.Controls.Add(loadIndividuallyBox);
            warPanel.Controls.Add(label9);
            warPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            warPanel.Location = new System.Drawing.Point(0, 270);
            warPanel.Name = "warPanel";
            warPanel.Size = new System.Drawing.Size(325, 268);
            warPanel.TabIndex = 9;
            warPanel.Visible = false;
            loadIndividuallyBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            loadIndividuallyBox.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            loadIndividuallyBox.Location = new System.Drawing.Point(11, 25);
            loadIndividuallyBox.Name = "loadIndividuallyBox";
            loadIndividuallyBox.Size = new System.Drawing.Size(301, 24);
            loadIndividuallyBox.TabIndex = 1;
            toolTip.SetToolTip(
                loadIndividuallyBox,
                "If the wave archive should be loaded individually."
            );
            loadIndividuallyBox.UseVisualStyleBackColor = true;
            label9.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label9.Location = new System.Drawing.Point(11, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(301, 22);
            label9.TabIndex = 0;
            label9.Text = "Load Individually:";
            label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            forceUniqueFilePanel.Controls.Add(forceUniqueFileBox);
            forceUniqueFilePanel.Controls.Add(label8);
            forceUniqueFilePanel.Dock = System.Windows.Forms.DockStyle.Top;
            forceUniqueFilePanel.Location = new System.Drawing.Point(0, 231);
            forceUniqueFilePanel.Name = "forceUniqueFilePanel";
            forceUniqueFilePanel.Size = new System.Drawing.Size(325, 39);
            forceUniqueFilePanel.TabIndex = 8;
            forceUniqueFilePanel.Visible = false;
            forceUniqueFileBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            forceUniqueFileBox.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            forceUniqueFileBox.Location = new System.Drawing.Point(11, 19);
            forceUniqueFileBox.Name = "forceUniqueFileBox";
            forceUniqueFileBox.Size = new System.Drawing.Size(301, 18);
            forceUniqueFileBox.TabIndex = 1;
            toolTip.SetToolTip(
                forceUniqueFileBox,
                "Write this file in the sound archive as its own file, even if it has the exact sa"
                    + "me data as another one. If this is not checked, files will be shared between ent"
                    + "ries for efficiency."
            );
            forceUniqueFileBox.UseVisualStyleBackColor = true;
            label8.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label8.Location = new System.Drawing.Point(11, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(301, 17);
            label8.TabIndex = 0;
            label8.Text = "Force Unique File:";
            label8.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            indexPanel.Controls.Add(swapAtIndexButton);
            indexPanel.Controls.Add(itemIndexBox);
            indexPanel.Controls.Add(label5);
            indexPanel.Dock = System.Windows.Forms.DockStyle.Top;
            indexPanel.Location = new System.Drawing.Point(0, 150);
            indexPanel.Name = "indexPanel";
            indexPanel.Size = new System.Drawing.Size(325, 81);
            indexPanel.TabIndex = 0;
            indexPanel.Visible = false;
            swapAtIndexButton.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            swapAtIndexButton.Location = new System.Drawing.Point(14, 49);
            swapAtIndexButton.Name = "swapAtIndexButton";
            swapAtIndexButton.Size = new System.Drawing.Size(298, 25);
            swapAtIndexButton.TabIndex = 0;
            swapAtIndexButton.Text = "Swap With Index";
            toolTip.SetToolTip(
                swapAtIndexButton,
                "Swap this entry with the one at the new index. If that entry doesn\'t exist, simpl"
                    + "y just change the index."
            );
            swapAtIndexButton.UseVisualStyleBackColor = true;
            itemIndexBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            itemIndexBox.Location = new System.Drawing.Point(14, 23);
            itemIndexBox.Maximum = new decimal(new int[] { -1, 0, 0, 0 });
            itemIndexBox.Name = "itemIndexBox";
            itemIndexBox.Size = new System.Drawing.Size(298, 20);
            itemIndexBox.TabIndex = 1;
            toolTip.SetToolTip(
                itemIndexBox,
                "The index of the item as referenced to by the game."
            );
            label5.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label5.Location = new System.Drawing.Point(11, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(301, 20);
            label5.TabIndex = 0;
            label5.Text = "Item Index:";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            settingsPanel.Controls.Add(seqExportModeBox);
            settingsPanel.Controls.Add(label4);
            settingsPanel.Controls.Add(seqImportModeBox);
            settingsPanel.Controls.Add(label3);
            settingsPanel.Controls.Add(writeNamesBox);
            settingsPanel.Controls.Add(label2);
            settingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            settingsPanel.Location = new System.Drawing.Point(0, 150);
            settingsPanel.Name = "settingsPanel";
            settingsPanel.Size = new System.Drawing.Size(325, 388);
            settingsPanel.TabIndex = 1;
            settingsPanel.Visible = false;
            seqExportModeBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            seqExportModeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            seqExportModeBox.FormattingEnabled = true;
            seqExportModeBox.Items.AddRange(new object[] { "Nitro Studio", "Sseq2Midi" });
            seqExportModeBox.Location = new System.Drawing.Point(11, 126);
            seqExportModeBox.Name = "seqExportModeBox";
            seqExportModeBox.Size = new System.Drawing.Size(301, 21);
            seqExportModeBox.TabIndex = 5;
            toolTip.SetToolTip(
                seqExportModeBox,
                "What program should be used to export sequences. Nitro Studio is my custom export"
                    + "er, while Sseq2Midi is the exe included. I recommend you use my exporter."
            );
            label4.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label4.Location = new System.Drawing.Point(11, 101);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(301, 22);
            label4.TabIndex = 4;
            label4.Text = "Sequence Export Mode:";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            seqImportModeBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            seqImportModeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            seqImportModeBox.FormattingEnabled = true;
            seqImportModeBox.Items.AddRange(
                new object[] { "Nitro Studio", "Midi2Sseq", "Nintendo Tools" }
            );
            seqImportModeBox.Location = new System.Drawing.Point(11, 77);
            seqImportModeBox.Name = "seqImportModeBox";
            seqImportModeBox.Size = new System.Drawing.Size(301, 21);
            seqImportModeBox.TabIndex = 3;
            toolTip.SetToolTip(
                seqImportModeBox,
                resources.GetString("seqImportModeBox.ToolTip")
            );
            label3.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label3.Location = new System.Drawing.Point(11, 52);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(301, 22);
            label3.TabIndex = 2;
            label3.Text = "Sequence Import Mode:";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            writeNamesBox.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            writeNamesBox.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            writeNamesBox.Location = new System.Drawing.Point(11, 25);
            writeNamesBox.Name = "writeNamesBox";
            writeNamesBox.Size = new System.Drawing.Size(301, 24);
            writeNamesBox.TabIndex = 1;
            toolTip.SetToolTip(
                writeNamesBox,
                "If the editor should export names for the sound archive."
            );
            writeNamesBox.UseVisualStyleBackColor = true;
            label2.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label2.Location = new System.Drawing.Point(11, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(301, 22);
            label2.TabIndex = 0;
            label2.Text = "Write Names:";
            label2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            noInfoPanel.Controls.Add(label1);
            noInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            noInfoPanel.Location = new System.Drawing.Point(0, 150);
            noInfoPanel.Name = "noInfoPanel";
            noInfoPanel.Size = new System.Drawing.Size(325, 388);
            noInfoPanel.TabIndex = 0;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.Location = new System.Drawing.Point(0, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(325, 388);
            label1.TabIndex = 0;
            label1.Text = "No Valid Info Selected!";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            kermalisSoundPlayerPanel.Controls.Add(kermalisPosition);
            kermalisSoundPlayerPanel.Controls.Add(tableLayoutPanel9);
            kermalisSoundPlayerPanel.Controls.Add(kermalisPlayButton);
            kermalisSoundPlayerPanel.Controls.Add(soundPlayerLabel);
            kermalisSoundPlayerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            kermalisSoundPlayerPanel.Location = new System.Drawing.Point(0, 0);
            kermalisSoundPlayerPanel.Name = "kermalisSoundPlayerPanel";
            kermalisSoundPlayerPanel.Size = new System.Drawing.Size(325, 150);
            kermalisSoundPlayerPanel.TabIndex = 16;
            kermalisSoundPlayerPanel.Visible = false;
            kermalisPosition.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            kermalisPosition.LargeChange = 20;
            kermalisPosition.Location = new System.Drawing.Point(14, 118);
            kermalisPosition.Maximum = 100;
            kermalisPosition.Name = "kermalisPosition";
            kermalisPosition.Size = new System.Drawing.Size(298, 45);
            kermalisPosition.TabIndex = 5;
            kermalisPosition.TickFrequency = 5;
            toolTip.SetToolTip(kermalisPosition, "Sound position.");
            tableLayoutPanel9.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel9.ColumnCount = 2;
            _ = tableLayoutPanel9.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            _ = tableLayoutPanel9.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
            );
            tableLayoutPanel9.Controls.Add(label22, 1, 1);
            tableLayoutPanel9.Controls.Add(label21, 0, 1);
            tableLayoutPanel9.Controls.Add(kermalisStopButton, 1, 0);
            tableLayoutPanel9.Controls.Add(kermalisPauseButton, 0, 0);
            tableLayoutPanel9.Controls.Add(kermalisVolumeSlider, 0, 2);
            tableLayoutPanel9.Controls.Add(kermalisLoopBox, 1, 2);
            tableLayoutPanel9.Location = new System.Drawing.Point(14, 49);
            tableLayoutPanel9.Name = "tableLayoutPanel9";
            tableLayoutPanel9.RowCount = 3;
            _ = tableLayoutPanel9.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F)
            );
            _ = tableLayoutPanel9.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F)
            );
            _ = tableLayoutPanel9.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F)
            );
            tableLayoutPanel9.Size = new System.Drawing.Size(298, 63);
            tableLayoutPanel9.TabIndex = 4;
            label22.Dock = System.Windows.Forms.DockStyle.Fill;
            label22.Location = new System.Drawing.Point(152, 27);
            label22.Name = "label22";
            label22.Size = new System.Drawing.Size(143, 15);
            label22.TabIndex = 5;
            label22.Text = "Loop:";
            label22.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            label21.Dock = System.Windows.Forms.DockStyle.Fill;
            label21.Location = new System.Drawing.Point(3, 27);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(143, 15);
            label21.TabIndex = 4;
            label21.Text = "Volume:";
            label21.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            kermalisStopButton.Dock = System.Windows.Forms.DockStyle.Fill;
            kermalisStopButton.Location = new System.Drawing.Point(152, 3);
            kermalisStopButton.Name = "kermalisStopButton";
            kermalisStopButton.Size = new System.Drawing.Size(143, 21);
            kermalisStopButton.TabIndex = 1;
            kermalisStopButton.Text = "Stop";
            kermalisStopButton.UseVisualStyleBackColor = true;
            kermalisPauseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            kermalisPauseButton.Location = new System.Drawing.Point(3, 3);
            kermalisPauseButton.Name = "kermalisPauseButton";
            kermalisPauseButton.Size = new System.Drawing.Size(143, 21);
            kermalisPauseButton.TabIndex = 0;
            kermalisPauseButton.Text = "Pause / Resume";
            kermalisPauseButton.UseVisualStyleBackColor = true;
            kermalisVolumeSlider.Dock = System.Windows.Forms.DockStyle.Fill;
            kermalisVolumeSlider.LargeChange = 10;
            kermalisVolumeSlider.Location = new System.Drawing.Point(3, 45);
            kermalisVolumeSlider.Maximum = 100;
            kermalisVolumeSlider.Name = "kermalisVolumeSlider";
            kermalisVolumeSlider.Size = new System.Drawing.Size(143, 21);
            kermalisVolumeSlider.SmallChange = 5;
            kermalisVolumeSlider.TabIndex = 2;
            kermalisVolumeSlider.TickFrequency = 10;
            kermalisVolumeSlider.Value = 75;
            kermalisLoopBox.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            kermalisLoopBox.Dock = System.Windows.Forms.DockStyle.Fill;
            kermalisLoopBox.Location = new System.Drawing.Point(152, 45);
            kermalisLoopBox.Name = "kermalisLoopBox";
            kermalisLoopBox.Size = new System.Drawing.Size(143, 21);
            kermalisLoopBox.TabIndex = 3;
            kermalisLoopBox.UseVisualStyleBackColor = true;
            kermalisPlayButton.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            kermalisPlayButton.Location = new System.Drawing.Point(14, 25);
            kermalisPlayButton.Name = "kermalisPlayButton";
            kermalisPlayButton.Size = new System.Drawing.Size(298, 20);
            kermalisPlayButton.TabIndex = 3;
            kermalisPlayButton.Text = "Play";
            kermalisPlayButton.UseVisualStyleBackColor = true;
            soundPlayerLabel.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            soundPlayerLabel.Location = new System.Drawing.Point(11, 3);
            soundPlayerLabel.Name = "soundPlayerLabel";
            soundPlayerLabel.Size = new System.Drawing.Size(301, 22);
            soundPlayerLabel.TabIndex = 1;
            soundPlayerLabel.Text = "Kermalis Sound Player:";
            soundPlayerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            pnlPianoKeys.Anchor =


                        System.Windows.Forms.AnchorStyles.Bottom
                        | System.Windows.Forms.AnchorStyles.Right


            ;
            pnlPianoKeys.BackColor = System.Drawing.SystemColors.ControlLightLight;
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cn7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.en7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cs7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.dn7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.ds7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fn7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fs7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gn7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gs7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.an7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.as7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.bn7]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cn6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.en6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cs6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.dn6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.ds6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fn6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fs6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gn6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gs6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.an6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.as6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.bn6]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cn1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cs1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.dn1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.ds1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.en1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fn1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fs1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gn1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gs1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.an1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.as1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.bn1]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cn2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cs2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.dn2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.ds2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.en2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fn2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fs2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gn2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gs2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.an2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.as2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.bn2]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cn3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cs3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.dn3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.ds3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.en3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fn3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fs3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gn3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gs3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.an3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.as3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.bn3]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cn4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cs4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.dn4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.ds4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.en4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fn4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fs4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gn4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gs4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.an4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.as4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.bn4]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cn5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cs5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.dn5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.ds5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.en5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fn5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.fs5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gn5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.gs5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.an5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.as5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.bn5]);
            pnlPianoKeys.Controls.Add(pianoKeys[Notes.cn8]);
            pnlPianoKeys.Location = new System.Drawing.Point(44, 478);
            pnlPianoKeys.Name = "pnlPianoKeys";
            pnlPianoKeys.Size = new System.Drawing.Size(565, 46);
            pnlPianoKeys.TabIndex = 6;
            pnlPianoKeys.Visible = false;
            InitPianoKeysProperties();
            bankEditorWars.Anchor =


                        System.Windows.Forms.AnchorStyles.Top
                        | System.Windows.Forms.AnchorStyles.Right


            ;
            bankEditorWars.BackColor = System.Drawing.SystemColors.ControlLightLight;
            bankEditorWars.Controls.Add(tableLayoutPanel16);
            bankEditorWars.Controls.Add(label31);
            bankEditorWars.Controls.Add(tableLayoutPanel17);
            bankEditorWars.Controls.Add(label33);
            bankEditorWars.Controls.Add(tableLayoutPanel18);
            bankEditorWars.Controls.Add(label34);
            bankEditorWars.Controls.Add(tableLayoutPanel19);
            bankEditorWars.Controls.Add(label35);
            bankEditorWars.Location = new System.Drawing.Point(315, 13);
            bankEditorWars.Name = "bankEditorWars";
            bankEditorWars.Size = new System.Drawing.Size(325, 253);
            bankEditorWars.TabIndex = 13;
            bankEditorWars.Visible = false;
            tableLayoutPanel16.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel16.ColumnCount = 2;
            _ = tableLayoutPanel16.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel16.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel16.Controls.Add(war3ComboBox, 0, 0);
            tableLayoutPanel16.Controls.Add(war3Box, 1, 0);
            tableLayoutPanel16.Location = new System.Drawing.Point(14, 193);
            tableLayoutPanel16.Name = "tableLayoutPanel16";
            tableLayoutPanel16.RowCount = 1;
            _ = tableLayoutPanel16.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel16.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel16.TabIndex = 13;
            war3ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            war3ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            war3ComboBox.FormattingEnabled = true;
            war3ComboBox.Location = new System.Drawing.Point(3, 3);
            war3ComboBox.Name = "war3ComboBox";
            war3ComboBox.Size = new System.Drawing.Size(247, 21);
            war3ComboBox.TabIndex = 6;
            toolTip.SetToolTip(war3ComboBox, "Wave archive to be used for the bank.");
            war3Box.Dock = System.Windows.Forms.DockStyle.Fill;
            war3Box.Location = new System.Drawing.Point(256, 3);
            war3Box.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            war3Box.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            war3Box.Name = "war3Box";
            war3Box.Size = new System.Drawing.Size(39, 20);
            war3Box.TabIndex = 7;
            toolTip.SetToolTip(war3Box, "Id of the wave archive to use for this bank.");
            label31.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label31.Location = new System.Drawing.Point(11, 171);
            label31.Name = "label31";
            label31.Size = new System.Drawing.Size(301, 22);
            label31.TabIndex = 12;
            label31.Text = "Wave Archive 3:";
            label31.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel17.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel17.ColumnCount = 2;
            _ = tableLayoutPanel17.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel17.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel17.Controls.Add(war2ComboBox, 0, 0);
            tableLayoutPanel17.Controls.Add(war2Box, 1, 0);
            tableLayoutPanel17.Location = new System.Drawing.Point(14, 137);
            tableLayoutPanel17.Name = "tableLayoutPanel17";
            tableLayoutPanel17.RowCount = 1;
            _ = tableLayoutPanel17.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel17.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel17.TabIndex = 11;
            war2ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            war2ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            war2ComboBox.FormattingEnabled = true;
            war2ComboBox.Location = new System.Drawing.Point(3, 3);
            war2ComboBox.Name = "war2ComboBox";
            war2ComboBox.Size = new System.Drawing.Size(247, 21);
            war2ComboBox.TabIndex = 6;
            toolTip.SetToolTip(war2ComboBox, "Wave archive to be used for the bank.");
            war2Box.Dock = System.Windows.Forms.DockStyle.Fill;
            war2Box.Location = new System.Drawing.Point(256, 3);
            war2Box.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            war2Box.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            war2Box.Name = "war2Box";
            war2Box.Size = new System.Drawing.Size(39, 20);
            war2Box.TabIndex = 7;
            toolTip.SetToolTip(war2Box, "Id of the wave archive to use for this bank.");
            label33.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label33.Location = new System.Drawing.Point(11, 115);
            label33.Name = "label33";
            label33.Size = new System.Drawing.Size(301, 22);
            label33.TabIndex = 10;
            label33.Text = "Wave Archive 2:";
            label33.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel18.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel18.ColumnCount = 2;
            _ = tableLayoutPanel18.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel18.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel18.Controls.Add(war1ComboBox, 0, 0);
            tableLayoutPanel18.Controls.Add(war1Box, 1, 0);
            tableLayoutPanel18.Location = new System.Drawing.Point(14, 81);
            tableLayoutPanel18.Name = "tableLayoutPanel18";
            tableLayoutPanel18.RowCount = 1;
            _ = tableLayoutPanel18.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel18.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel18.TabIndex = 9;
            war1ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            war1ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            war1ComboBox.FormattingEnabled = true;
            war1ComboBox.Location = new System.Drawing.Point(3, 3);
            war1ComboBox.Name = "war1ComboBox";
            war1ComboBox.Size = new System.Drawing.Size(247, 21);
            war1ComboBox.TabIndex = 6;
            toolTip.SetToolTip(war1ComboBox, "Wave archive to be used for the bank.");
            war1Box.Dock = System.Windows.Forms.DockStyle.Fill;
            war1Box.Location = new System.Drawing.Point(256, 3);
            war1Box.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            war1Box.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            war1Box.Name = "war1Box";
            war1Box.Size = new System.Drawing.Size(39, 20);
            war1Box.TabIndex = 7;
            toolTip.SetToolTip(war1Box, "Id of the wave archive to use for this bank.");
            label34.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label34.Location = new System.Drawing.Point(11, 59);
            label34.Name = "label34";
            label34.Size = new System.Drawing.Size(301, 22);
            label34.TabIndex = 8;
            label34.Text = "Wave Archive 1:";
            label34.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tableLayoutPanel19.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            tableLayoutPanel19.ColumnCount = 2;
            _ = tableLayoutPanel19.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F)
            );
            _ = tableLayoutPanel19.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
            );
            tableLayoutPanel19.Controls.Add(war0ComboBox, 0, 0);
            tableLayoutPanel19.Controls.Add(war0Box, 1, 0);
            tableLayoutPanel19.Location = new System.Drawing.Point(14, 25);
            tableLayoutPanel19.Name = "tableLayoutPanel19";
            tableLayoutPanel19.RowCount = 1;
            _ = tableLayoutPanel19.RowStyles.Add(
                new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)
            );
            tableLayoutPanel19.Size = new System.Drawing.Size(298, 31);
            tableLayoutPanel19.TabIndex = 7;
            war0ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            war0ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            war0ComboBox.FormattingEnabled = true;
            war0ComboBox.Location = new System.Drawing.Point(3, 3);
            war0ComboBox.Name = "war0ComboBox";
            war0ComboBox.Size = new System.Drawing.Size(247, 21);
            war0ComboBox.TabIndex = 6;
            toolTip.SetToolTip(war0ComboBox, "Wave archive to be used for the bank.");
            war0Box.Dock = System.Windows.Forms.DockStyle.Fill;
            war0Box.Location = new System.Drawing.Point(256, 3);
            war0Box.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            war0Box.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            war0Box.Name = "war0Box";
            war0Box.Size = new System.Drawing.Size(39, 20);
            war0Box.TabIndex = 7;
            toolTip.SetToolTip(war0Box, "Id of the wave archive to use for this bank.");
            label35.Anchor =



                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                         | System.Windows.Forms.AnchorStyles.Right


            ;
            label35.Location = new System.Drawing.Point(11, 3);
            label35.Name = "label35";
            label35.Size = new System.Drawing.Size(301, 22);
            label35.TabIndex = 2;
            label35.Text = "Wave Archive 0:";
            label35.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tree.Dock = System.Windows.Forms.DockStyle.Fill;
            tree.ImageIndex = 0;
            tree.ImageList = treeIcons;
            tree.Indent = 12;
            tree.Location = new System.Drawing.Point(0, 0);
            tree.Name = "tree";
            treeNode1.ImageIndex = 10;
            treeNode1.Name = "fileInfo";
            treeNode1.SelectedImageIndex = 10;
            treeNode1.Text = "File Information";
            tree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] { treeNode1 });
            tree.SelectedImageIndex = 0;
            tree.ShowLines = false;
            tree.Size = new System.Drawing.Size(651, 538);
            tree.TabIndex = 0;
            tree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(
                tree_NodeMouseClick
            );
            tree.NodeMouseDoubleClick +=
                new System.Windows.Forms.TreeNodeMouseClickEventHandler(
                    tree_NodeMouseDoubleClick
                );
            tree.KeyUp += new System.Windows.Forms.KeyEventHandler(tree_NodeKey);
            treeIcons.ImageStream =
                (System.Windows.Forms.ImageListStreamer)
                    resources.GetObject("treeIcons.ImageStream")

            ;
            treeIcons.TransparentColor = System.Drawing.Color.Transparent;
            treeIcons.Images.SetKeyName(0, "blank.png");
            treeIcons.Images.SetKeyName(1, "version.png");
            treeIcons.Images.SetKeyName(2, "sseq.png");
            treeIcons.Images.SetKeyName(3, "seqArc.png");
            treeIcons.Images.SetKeyName(4, "bank.png");
            treeIcons.Images.SetKeyName(5, "waveArchive.png");
            treeIcons.Images.SetKeyName(6, "player.png");
            treeIcons.Images.SetKeyName(7, "group.png");
            treeIcons.Images.SetKeyName(8, "streamPlayer.png");
            treeIcons.Images.SetKeyName(9, "strm.png");
            treeIcons.Images.SetKeyName(10, "record.png");
            treeIcons.Images.SetKeyName(11, "recordArc.png");
            treeIcons.Images.SetKeyName(12, "lookup.png");
            treeIcons.Images.SetKeyName(13, "recordRegion.png");
            treeIcons.Images.SetKeyName(14, "wave.png");
            treeIcons.Images.SetKeyName(15, "ranged.png");
            treeIcons.Images.SetKeyName(16, "regional.png");
            treeIcons.Images.SetKeyName(17, "psg.png");
            treeIcons.Images.SetKeyName(18, "whiteNoise.png");
            sequenceEditorPanel.Controls.Add(sequenceEditor);
            sequenceEditorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            sequenceEditorPanel.Location = new System.Drawing.Point(0, 0);
            sequenceEditorPanel.Name = "sequenceEditorPanel";
            sequenceEditorPanel.Size = new System.Drawing.Size(651, 538);
            sequenceEditorPanel.TabIndex = 3;
            sequenceEditorPanel.Visible = false;
            sequenceEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            sequenceEditor.Location = new System.Drawing.Point(0, 0);
            sequenceEditor.Name = "sequenceEditor";
            sequenceEditor.Size = new System.Drawing.Size(651, 538);
            sequenceEditor.TabIndex = 0;
            openFileDialog.RestoreDirectory = true;
            statusStrip.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[] { status, currentNote }
            );
            statusStrip.Location = new System.Drawing.Point(0, 564);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(984, 22);
            statusStrip.TabIndex = 2;
            statusStrip.Text = "statusStrip1";
            status.Name = "status";
            status.Size = new System.Drawing.Size(125, 17);
            status.Text = "No Valid Info Selected!";
            currentNote.Name = "currentNote";
            currentNote.RightToLeft = System.Windows.Forms.RightToLeft.No;
            currentNote.Size = new System.Drawing.Size(0, 17);
            rootMenu.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    addToolStripMenuItem,
                    expandToolStripMenuItem,
                    collapseToolStripMenuItem,
                }
            );
            rootMenu.Name = "rootMenu";
            rootMenu.Size = new System.Drawing.Size(120, 70);
            addToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.New;
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            addToolStripMenuItem.Text = "Add";
            addToolStripMenuItem.Click += new System.EventHandler(
                addToolStripMenuItem_Click
            );
            expandToolStripMenuItem.Image = global::NitroStudio2.Properties.Resources.Save;
            expandToolStripMenuItem.Name = "expandToolStripMenuItem";
            expandToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            expandToolStripMenuItem.Text = "Expand";
            expandToolStripMenuItem.Click += new System.EventHandler(
                expandToolStripMenuItem_Click
            );
            collapseToolStripMenuItem.Image = global::NitroStudio2
                .Properties
                .Resources
                .Save_As;
            collapseToolStripMenuItem.Name = "collapseToolStripMenuItem";
            collapseToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            collapseToolStripMenuItem.Text = "Collapse";
            collapseToolStripMenuItem.Click += new System.EventHandler(
                collapseToolStripMenuItem_Click
            );
            nodeMenu.ImeMode = System.Windows.Forms.ImeMode.Off;
            nodeMenu.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    addAboveToolStripMenuItem1,
                    addBelowToolStripMenuItem1,
                    moveUpToolStripMenuItem1,
                    moveDownToolStripMenuItem1,
                    replaceFileToolStripMenuItem,
                    exportToolStripMenuItem1,
                    deleteToolStripMenuItem1,
                }
            );
            nodeMenu.Name = "contextMenuStrip1";
            nodeMenu.Size = new System.Drawing.Size(139, 158);
            addAboveToolStripMenuItem1.Image = global::NitroStudio2.Properties.Resources.New;
            addAboveToolStripMenuItem1.Name = "addAboveToolStripMenuItem1";
            addAboveToolStripMenuItem1.Size = new System.Drawing.Size(138, 22);
            addAboveToolStripMenuItem1.Text = "Add Above";
            addAboveToolStripMenuItem1.Click += new System.EventHandler(
                addAboveToolStripMenuItem1_Click
            );
            addBelowToolStripMenuItem1.Image = global::NitroStudio2.Properties.Resources.Open;
            addBelowToolStripMenuItem1.Name = "addBelowToolStripMenuItem1";
            addBelowToolStripMenuItem1.Size = new System.Drawing.Size(138, 22);
            addBelowToolStripMenuItem1.Text = "Add Below";
            addBelowToolStripMenuItem1.Click += new System.EventHandler(
                addBelowToolStripMenuItem1_Click
            );
            moveUpToolStripMenuItem1.Image = global::NitroStudio2.Properties.Resources.Save;
            moveUpToolStripMenuItem1.Name = "moveUpToolStripMenuItem1";
            moveUpToolStripMenuItem1.Size = new System.Drawing.Size(138, 22);
            moveUpToolStripMenuItem1.Text = "Move Up";
            moveUpToolStripMenuItem1.Click += new System.EventHandler(
                moveUpToolStripMenuItem1_Click
            );
            moveDownToolStripMenuItem1.Image = global::NitroStudio2
                .Properties
                .Resources
                .Save_As;
            moveDownToolStripMenuItem1.Name = "moveDownToolStripMenuItem1";
            moveDownToolStripMenuItem1.Size = new System.Drawing.Size(138, 22);
            moveDownToolStripMenuItem1.Text = "Move Down";
            moveDownToolStripMenuItem1.Click += new System.EventHandler(
                moveDownToolStripMenuItem1_Click
            );
            replaceFileToolStripMenuItem.Image = global::NitroStudio2
                .Properties
                .Resources
                .Import;
            replaceFileToolStripMenuItem.Name = "replaceFileToolStripMenuItem";
            replaceFileToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            replaceFileToolStripMenuItem.Text = "Replace";
            replaceFileToolStripMenuItem.Click += new System.EventHandler(
                replaceFileToolStripMenuItem_Click
            );
            exportToolStripMenuItem1.Image = global::NitroStudio2.Properties.Resources.Export;
            exportToolStripMenuItem1.Name = "exportToolStripMenuItem1";
            exportToolStripMenuItem1.Size = new System.Drawing.Size(138, 22);
            exportToolStripMenuItem1.Text = "Export";
            exportToolStripMenuItem1.Click += new System.EventHandler(
                exportToolStripMenuItem1_Click
            );
            deleteToolStripMenuItem1.Image = global::NitroStudio2.Properties.Resources.Close;
            deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            deleteToolStripMenuItem1.Size = new System.Drawing.Size(138, 22);
            deleteToolStripMenuItem1.Text = "Delete";
            deleteToolStripMenuItem1.Click += new System.EventHandler(
                deleteToolStripMenuItem1_Click
            );
            sarEntryMenu.ImeMode = System.Windows.Forms.ImeMode.Off;
            sarEntryMenu.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    sarAddAbove,
                    sarAddBelow,
                    sarMoveUp,
                    sarMoveDown,
                    sarReplace,
                    sarExport,
                    sarRename,
                    sarDelete,
                }
            );
            sarEntryMenu.Name = "contextMenuStrip1";
            sarEntryMenu.Size = new System.Drawing.Size(139, 180);
            sarAddAbove.Image = global::NitroStudio2.Properties.Resources.New;
            sarAddAbove.Name = "sarAddAbove";
            sarAddAbove.Size = new System.Drawing.Size(138, 22);
            sarAddAbove.Text = "Add Above";
            sarAddAbove.Click += new System.EventHandler(SarAddAbove_Click);
            sarAddBelow.Image = global::NitroStudio2.Properties.Resources.Open;
            sarAddBelow.Name = "sarAddBelow";
            sarAddBelow.Size = new System.Drawing.Size(138, 22);
            sarAddBelow.Text = "Add Below";
            sarAddBelow.Click += new System.EventHandler(SarAddBelow_Click);
            sarMoveUp.Image = global::NitroStudio2.Properties.Resources.Save;
            sarMoveUp.Name = "sarMoveUp";
            sarMoveUp.Size = new System.Drawing.Size(138, 22);
            sarMoveUp.Text = "Move Up";
            sarMoveUp.Click += new System.EventHandler(SarMoveUp_Click);
            sarMoveDown.Image = global::NitroStudio2.Properties.Resources.Save_As;
            sarMoveDown.Name = "sarMoveDown";
            sarMoveDown.Size = new System.Drawing.Size(138, 22);
            sarMoveDown.Text = "Move Down";
            sarMoveDown.Click += new System.EventHandler(SarMoveDown_Click);
            sarReplace.Image = global::NitroStudio2.Properties.Resources.Import;
            sarReplace.Name = "sarReplace";
            sarReplace.Size = new System.Drawing.Size(138, 22);
            sarReplace.Text = "Replace";
            sarReplace.Click += new System.EventHandler(SarReplace_Click);
            sarExport.Image = global::NitroStudio2.Properties.Resources.Export;
            sarExport.Name = "sarExport";
            sarExport.Size = new System.Drawing.Size(138, 22);
            sarExport.Text = "Export";
            sarExport.Click += new System.EventHandler(SarExport_Click);
            sarRename.Image = global::NitroStudio2.Properties.Resources.Rename;
            sarRename.Name = "sarRename";
            sarRename.Size = new System.Drawing.Size(138, 22);
            sarRename.Text = "Rename";
            sarRename.Click += new System.EventHandler(SarRename_Click);
            sarDelete.Image = global::NitroStudio2.Properties.Resources.Close;
            sarDelete.Name = "sarDelete";
            sarDelete.Size = new System.Drawing.Size(138, 22);
            sarDelete.Text = "Delete";
            sarDelete.Click += new System.EventHandler(SarDelete_Click);
            ClientSize = new System.Drawing.Size(984, 586);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip);
            Controls.Add(statusStrip);
            MainMenuStrip = menuStrip;
            Name = "EditorBase";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            FormClosing += new System.Windows.Forms.FormClosingEventHandler(form_Close);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            seqBankPanel.ResumeLayout(false);
            tableLayoutPanel36.ResumeLayout(false);
            tableLayoutPanel20.ResumeLayout(false);
            foreach (var _tp in trackPanels) _tp.ResumeLayout(false);
            foreach (var _pic in trackPictures) ((System.ComponentModel.ISupportInitialize)_pic).EndInit();
            tableLayoutPanel12.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)seqEditorBankBox).EndInit();
            bankEditorPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bankRegions).EndInit();
            tableLayoutPanel15.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)drumSetStartRangeBox).EndInit();
            tableLayoutPanel14.ResumeLayout(false);
            seqArcSeqPanel.ResumeLayout(false);
            tableLayoutPanel13.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)seqArcSeqBox).EndInit();
            seqArcPanel.ResumeLayout(false);
            seqPanel.ResumeLayout(false);
            tableLayoutPanel11.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)seqPlayerBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)seqPlayerPriorityBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)seqChannelPriorityBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)seqVolumeBox).EndInit();
            tableLayoutPanel10.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)seqBankBox).EndInit();
            playerPanel.ResumeLayout(false);
            tableLayoutPanel8.ResumeLayout(false);
            tableLayoutPanel8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)playerHeapSizeBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)playerMaxSequencesBox).EndInit();
            stmPanel.ResumeLayout(false);
            tableLayoutPanel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)stmPlayerBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)stmPriorityBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)stmVolumeBox).EndInit();
            streamPlayerPanel.ResumeLayout(false);
            tableLayoutPanel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)stmPlayerLeftChannelBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)stmPlayerRightChannelBox).EndInit();
            grpPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grpEntries).EndInit();
            bankPanel.ResumeLayout(false);
            tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bnkWar3Box).EndInit();
            tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bnkWar2Box).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bnkWar1Box).EndInit();
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bnkWar0Box).EndInit();
            warPanel.ResumeLayout(false);
            forceUniqueFilePanel.ResumeLayout(false);
            indexPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)itemIndexBox).EndInit();
            settingsPanel.ResumeLayout(false);
            noInfoPanel.ResumeLayout(false);
            kermalisSoundPlayerPanel.ResumeLayout(false);
            kermalisSoundPlayerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kermalisPosition).EndInit();
            tableLayoutPanel9.ResumeLayout(false);
            tableLayoutPanel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kermalisVolumeSlider).EndInit();
            pnlPianoKeys.ResumeLayout(false);
            bankEditorWars.ResumeLayout(false);
            tableLayoutPanel16.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)war3Box).EndInit();
            tableLayoutPanel17.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)war2Box).EndInit();
            tableLayoutPanel18.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)war1Box).EndInit();
            tableLayoutPanel19.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)war0Box).EndInit();
            sequenceEditorPanel.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            rootMenu.ResumeLayout(false);
            nodeMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            sarEntryMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        public string GetFileOpenerPath(string description, string extension)
        {
            openFileDialog.FileName = "";
            openFileDialog.Filter = description + "|" + "*.s" + extension.ToLower();
            _ = openFileDialog.ShowDialog();
            return openFileDialog.FileName;
        }

        public string GetFileSaverPath(string description, string extension)
        {
            saveFileDialog.FileName = "";
            saveFileDialog.Filter = description + "|" + "*.s" + extension.ToLower();
            _ = saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                if (Path.GetExtension(saveFileDialog.FileName) == "")
                {
                    saveFileDialog.FileName += ".s" + extension.ToLower();
                }
            }
            return saveFileDialog.FileName;
        }

        private void form_Close(object sender, FormClosingEventArgs e)
        {
            OnClosing();
        }

        public virtual void OnClosing() { }

        public ContextMenuStrip CreateMenuStrip(
            ContextMenuStrip orig,
            int[] indices,
            EventHandler[] eventHandlers
        )
        {
            ContextMenuStrip c = new();
            int num = 0;
            foreach (int ind in indices)
            {
                ToolStripItem i = orig.Items[ind];
                _ = c.Items.Add(i.Text, i.Image, eventHandlers[num++]);
            }
            return c;
        }

        #region Updating
        public virtual void DoInfoStuff()
        {
            tree.SelectedNode ??= tree.Nodes[0];
            if (!FileOpen)
            {
                noInfoPanel.BringToFront();
                noInfoPanel.Show();
                status.Text = "No Valid Info Selected!";
            }
        }

        private Stack<int> nodeIndices;
        private List<string> expandedNodes;

        public void BeginUpdateNodes()
        {
            tree.BeginUpdate();
            expandedNodes = collectExpandedNodes(tree.Nodes);
            tree.SelectedNode ??= tree.Nodes[0];
            nodeIndices = new Stack<int>();
            nodeIndices.Push(tree.SelectedNode.Index);
            while (tree.SelectedNode.Parent != null)
            {
                tree.SelectedNode = tree.SelectedNode.Parent;
                nodeIndices.Push(tree.SelectedNode.Index);
            }
            for (int i = 0; i < tree.Nodes.Count; i++)
            {
                tree.Nodes[i].Nodes.Clear();
            }
        }

        public abstract void UpdateNodes();

        public void EndUpdateNodes()
        {
            if (expandedNodes.Count > 0)
            {
                TreeNode IamExpandedNode;
                for (int i = 0; i < expandedNodes.Count; i++)
                {
                    IamExpandedNode = FindNodeByName(tree.Nodes, expandedNodes[i]);
                    expandNodePath(IamExpandedNode);
                }
            }
            tree.SelectedNode = tree.Nodes[nodeIndices.Pop()];
            while (nodeIndices.Count > 0)
            {
                try
                {
                    tree.SelectedNode = tree.SelectedNode.Nodes[nodeIndices.Pop()];
                }
                catch
                {
                    nodeIndices.Clear();
                }
            }
            tree.SelectedNode.EnsureVisible();
            tree.EndUpdate();
        }
        #endregion
        #region fileMenu
        public virtual void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, true))
            {
                return;
            }
            File = (IOFile)Activator.CreateInstance(FileType);
            FilePath = "";
            FileOpen = true;
            ExtFile = null;
            Text = EditorName + " - New " + ExtensionDescription + ".s" + Extension;
            UpdateNodes();
            DoInfoStuff();
        }

        public virtual void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, true))
            {
                return;
            }
            string path = GetFileOpenerPath(ExtensionDescription, Extension);
            if (path != "")
            {
                File = (IOFile)Activator.CreateInstance(FileType);
                ExtFile = null;
                FilePath = path;
                Text = EditorName + " - " + Path.GetFileName(path);
                FileOpen = true;
                File.Read(path);
                UpdateNodes();
                DoInfoStuff();
            }
        }

        public virtual void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            if (ExtFile == null && FilePath == "")
            {
                saveAsToolStripMenuItem_Click(sender, e);
                return;
            }
            if (ExtFile != null)
            {
                ExtFile.Read(File.Write());
                if (MainWindow != null)
                {
                    MainWindow.UpdateNodes();
                    MainWindow.DoInfoStuff();
                }
                if (OtherEditor != null)
                {
                    OtherEditor.UpdateNodes();
                    OtherEditor.DoInfoStuff();
                }
            }
            else
            {
                File.Write(FilePath);
            }
        }

        public void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            string path = GetFileSaverPath(ExtensionDescription, Extension);
            if (path != "")
            {
                FilePath = path;
                ExtFile = null;
                Text = EditorName + " - " + Path.GetFileName(path);
                saveToolStripMenuItem_Click(sender, e);
            }
        }

        public virtual void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, true, true))
            {
                return;
            }
            File = null;
            ExtFile = null;
            FilePath = "";
            FileOpen = false;
            Text = EditorName;
            UpdateNodes();
            DoInfoStuff();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileOpen)
            {
                SaveQuitDialog q = new(this);
                _ = q.ShowDialog();
            }
            else
            {
                Close();
            }
        }
        #endregion
        #region editMenu
        public virtual void blankFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            File = (IOFile)Activator.CreateInstance(FileType);
            UpdateNodes();
            DoInfoStuff();
        }

        public virtual void importFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            string path = GetFileOpenerPath(ExtensionDescription, Extension);
            if (path != "")
            {
                File = (IOFile)Activator.CreateInstance(FileType);
                File.Read(path);
                UpdateNodes();
                DoInfoStuff();
            }
        }

        public virtual void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            string path = GetFileSaverPath(ExtensionDescription, Extension);
            if (path != "")
            {
                File.Write(path);
            }
        }

        private void nullifyFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            if (ExtFile == null)
            {
                _ = MessageBox.Show("You can't nullify data that is not in a parent file!", "Notice:");
                return;
            }
            File = null;
            UpdateNodes();
            DoInfoStuff();
        }

        public bool FileTest(object sender, EventArgs e, bool save, bool forceOpen = false)
        {
            if (FileOpen)
            {
                if (save)
                {
                    SaveCloseDialog c = new();
                    switch (c.getValue())
                    {
                        case 0:
                            saveToolStripMenuItem_Click(sender, e);
                            return true;
                        case 1:
                            return true;
                        default:
                            return false;
                    }
                }
                return true;
            }
            else
            {
                if (forceOpen)
                {
                    _ = MessageBox.Show("There must be a file open to do this!", "Notice:");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        #endregion
        #region nodeShit
        private void expandNodePath(TreeNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.Level != 0)
            {
                node.Expand();
                expandNodePath(node.Parent);
            }
            else
            {
                node.Expand();
            }
        }

        private void tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tree.SelectedNode = tree.GetNodeAt(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Left)
            {
                tree.SelectedNode = tree.GetNodeAt(e.X, e.Y);
            }
            DoInfoStuff();
        }

        private void tree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tree.SelectedNode = tree.GetNodeAt(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Left)
            {
                tree.SelectedNode = tree.GetNodeAt(e.X, e.Y);
            }
            DoInfoStuff();
            NodeMouseDoubleClick();
        }

        public virtual void NodeMouseDoubleClick() { }

        private void tree_NodeKey(object sender, KeyEventArgs e)
        {
            DoInfoStuff();
        }

        private List<string> collectExpandedNodes(TreeNodeCollection Nodes)
        {
            List<string> _lst = [];
            foreach (TreeNode checknode in Nodes)
            {
                if (checknode.IsExpanded)
                {
                    _lst.Add(checknode.Name);
                }

                if (checknode.Nodes.Count > 0)
                {
                    _lst.AddRange(collectExpandedNodes(checknode.Nodes));
                }
            }
            return _lst;
        }

        private TreeNode FindNodeByName(TreeNodeCollection NodesCollection, string Name)
        {
            TreeNode returnNode = null;
            foreach (TreeNode checkNode in NodesCollection)
            {
                if (checkNode.Name == Name)
                {
                    returnNode = checkNode;
                }
                else if (checkNode.Nodes.Count > 0)
                {
                    returnNode = FindNodeByName(checkNode.Nodes, Name);
                }
                if (returnNode != null)
                {
                    return returnNode;
                }
            }
            return returnNode;
        }
        #endregion
        #region soundPlayerDeluxe
        private void playSoundTrack_Click(object sender, EventArgs e)
        {
            Play();
        }

        private void pauseSoundTrack_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void stopSoundTrack_Click(object sender, EventArgs e)
        {
            Stop();
        }

        public virtual void Play() { }

        public virtual void Pause() { }

        public virtual void Stop() { }
        #endregion
        #region otherButtons
        private void forceWaveVersionButton_Click(object sender, EventArgs e)
        {
            ForceWaveVersionButtonClick();
        }

        public virtual void ForceWaveVersionButtonClick() { }
        #endregion
        #region rootMenu
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RootAdd();
        }

        public void expandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tree.SelectedNode.Expand();
        }

        public void collapseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tree.SelectedNode.Collapse();
        }

        public virtual void RootAdd() { }
        #endregion
        #region nodeMenu
        public void addAboveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NodeAddAbove();
        }

        public void addBelowToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NodeAddBelow();
        }

        public void moveUpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NodeMoveUp();
        }

        public void moveDownToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NodeMoveDown();
        }

        public void blankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NodeBlank();
        }

        public void replaceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NodeReplace();
        }

        public void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NodeExport();
        }

        public void nullifyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NodeNullify();
        }

        public void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NodeDelete();
        }

        public virtual void NodeAddAbove() { }

        public virtual void NodeAddBelow() { }

        public virtual void NodeMoveUp() { }

        public virtual void NodeMoveDown() { }

        public virtual void NodeBlank() { }

        public virtual void NodeReplace() { }

        public virtual void NodeExport() { }

        public virtual void NodeNullify() { }

        public virtual void NodeDelete() { }

        public bool Swap<T>(IList<T> objects, int a, int b)
        {
            if (a < 0 || a >= objects.Count || b < 0 || b >= objects.Count)
            {
                return false;
            }
            (objects[b], objects[a]) = (objects[a], objects[b]);
            return true;
        }
        #endregion
        #region warBoxes
        private void vMajBoxWar_ValueChanged(object sender, EventArgs e)
        {
            BoxWarMajChanged();
        }

        private void vMinBoxWar_ValueChanged(object sender, EventArgs e)
        {
            BoxWarMinChanged();
        }

        private void vRevBoxWar_ValueChanged(object sender, EventArgs e)
        {
            BoxWarRevChanged();
        }

        private void vWavMajBox_ValueChanged(object sender, EventArgs e)
        {
            BoxWavMajChanged();
        }

        private void vWavMinBox_ValueChanged(object sender, EventArgs e)
        {
            BoxWavMinChanged();
        }

        private void vWavRevBox_ValueChanged(object sender, EventArgs e)
        {
            BoxWavRevChanged();
        }

        public virtual void BoxWarMajChanged() { }

        public virtual void BoxWarMinChanged() { }

        public virtual void BoxWarRevChanged() { }

        public virtual void BoxWavMajChanged() { }

        public virtual void BoxWavMinChanged() { }

        public virtual void BoxWavRevChanged() { }
        #endregion
        #region warTools
        private void batchExtractWavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WarExtractWave();
        }

        private void batchExtract3dsWavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WarExtractWave3ds();
        }

        private void batchExtractWiiUWavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WarExtractWaveWiiU();
        }

        private void batchExtractSwitchWavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WarExtractWaveSwitch();
        }

        private void batchImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WarBatchImport();
        }

        public virtual void WarExtractWave() { }

        public virtual void WarExtractWave3ds() { }

        public virtual void WarExtractWaveWiiU() { }

        public virtual void WarExtractWaveSwitch() { }

        public virtual void WarBatchImport() { }
        #endregion
        #region grpVersions
        private void grpSeqForceButton_Click(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupForceSequenceVersion();
            }
        }

        private void grpBnkForceButton_Click(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupForceBankVersion();
            }
        }

        private void grpWarForceButton_Click(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupForceWaveArchiveVersion();
            }
        }

        private void grpWsdForceButton_Click(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupForceWaveSoundDataVersion();
            }
        }

        private void grpStpForceButton_Click(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupForcePrefetchVersion();
            }
        }

        private void grpMajBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpMinBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpRevBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpSeqMajBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpSeqMinBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpSeqRevBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpBnkMajBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpBnkMinBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpBnkRevBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpWarMajBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpWarMinBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpWarRevBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpWsdMajBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpWsdMinBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpWsdRevBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpStpMajBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpStpMinBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        private void grpStpRevBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupVersionChanged();
            }
        }

        public virtual void GroupForceSequenceVersion() { }

        public virtual void GroupForceBankVersion() { }

        public virtual void GroupForceWaveArchiveVersion() { }

        public virtual void GroupForceWaveSoundDataVersion() { }

        public virtual void GroupForcePrefetchVersion() { }

        public virtual void GroupVersionChanged() { }
        #endregion
        #region grpFile
        private void grpFileIdComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupFileIdComboChanged();
            }
        }

        private void grpFileIdBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupFileIdNumBoxChanged();
            }
        }

        private void grpEmbedModeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupFileIdEmbedModeChanged();
            }
        }

        public virtual void GroupFileIdComboChanged() { }

        public virtual void GroupFileIdNumBoxChanged() { }

        public virtual void GroupFileIdEmbedModeChanged() { }
        #endregion
        #region grpDependency
        private void grpDepEntryTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupDependencyTypeChanged();
            }
        }

        private void grpDepEntryNumComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupDependencyEntryComboChanged();
            }
        }

        private void grpDepEntryNumBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupDependencyEntryNumBoxChanged();
            }
        }

        private void grpDepLoadFlagsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                GroupDependencyFlagsChanged();
            }
        }

        public virtual void GroupDependencyTypeChanged() { }

        public virtual void GroupDependencyEntryComboChanged() { }

        public virtual void GroupDependencyEntryNumBoxChanged() { }

        public virtual void GroupDependencyFlagsChanged() { }
        #endregion
        #region SARProjectInfo
        private void MaxSeqNumBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void MaxSeqTrackNumBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void MaxStreamNumBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void MaxStreamNumTracksBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void MaxStreamNumChannelsBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void MaxWaveNumBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void MaxWaveNumTracksBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void StreamBufferTimesBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void OptionsPIBox_ValueChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        private void SarIncludeStringBlock_CheckedChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                SarProjectInfoUpdated();
            }
        }

        public virtual void SarProjectInfoUpdated() { }
        #endregion
        public virtual void StmSound3dButton_Click(object sender, EventArgs e) { }

        public virtual void WsdSound3dButton_Click(object sender, EventArgs e) { }

        public virtual void SeqEditSound3dInfoButton_Click(object sender, EventArgs e) { }

        public virtual void SeqEditSoundInfoButton_Click(object sender, EventArgs e) { }

        public virtual void WsdEditSoundInfoButton_Click(object sender, EventArgs e) { }

        public virtual void StmSoundInfoButton_Click(object sender, EventArgs e) { }

        public virtual void FileTypeBox_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void PlayerSoundLimitBox_ValueChanged(object sender, EventArgs e) { }

        public virtual void PlayerEnableSoundLimitBox_CheckedChanged(object sender, EventArgs e) { }

        public virtual void PlayerHeapSizeBox_ValueChanged(object sender, EventArgs e) { }

        public virtual void SarGrpFileIdBox_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void SarWarFileIdBox_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void WarLoadIndividuallyBox_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WarIncludeWaveCountBox_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SarBnkFileIdBox_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void BnkWarsChanged(object sender, EventArgs e) { }

        public virtual void SoundGrpStartIndex_ValueChanged(object sender, EventArgs e) { }

        public virtual void SoundGrpEndIndex_ValueChanged(object sender, EventArgs e) { }

        public virtual void SoundGroupFilesChanged(object sender, EventArgs e) { }

        public virtual void SoundGroupWarsChanged(object sender, EventArgs e) { }

        public virtual void SarSeqFileIdBox_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void SarSeqPlay_Click(object sender, EventArgs e) { }

        public virtual void SarSeqPause_Click(object sender, EventArgs e) { }

        public virtual void SarSeqStop_Click(object sender, EventArgs e) { }

        public virtual void SeqSound3dInfoExists_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqBank0Box_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void SeqBank1Box_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void SeqBank2Box_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void SeqBank3Box_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void SeqOffsetFromLabelButton_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqOffsetManualButton_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqOffsetFromLabelBox_SelectedIndexChanged(
            object sender,
            EventArgs e
        )
        { }

        public virtual void SeqOffsetManualBox_ValueChanged(object sender, EventArgs e) { }

        public virtual void SeqChannelPriorityBox_ValueChanged(object sender, EventArgs e) { }

        public virtual void SeqIsReleasePriorityBox_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC0_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC1_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC2_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC3_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC4_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC5_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC6_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC7_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC8_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC9_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC10_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC11_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC12_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC13_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC14_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SeqC15_CheckedChanged(object sender, EventArgs e) { }

        public virtual void SarWsdFileIdBox_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void SarWsdPlay_Click(object sender, EventArgs e) { }

        public virtual void SarWsdPause_Click(object sender, EventArgs e) { }

        public virtual void SarWsdStop_Click(object sender, EventArgs e) { }

        public virtual void WsdSound3dEnable_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdWaveIndex_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdTracksToAllocate_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdCopyCount_Click(object sender, EventArgs e) { }

        public virtual void WsdChannelPriority_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdFixPriority_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmFileIdBox_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void StmPlay_Click(object sender, EventArgs e) { }

        public virtual void StmPause_Click(object sender, EventArgs e) { }

        public virtual void StmStop_Click(object sender, EventArgs e) { }

        public virtual void StmSound3dEnable_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmWriteTrackInfo_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmUpdateTrackInfo_Click(object sender, EventArgs e) { }

        public virtual void StmTrack0_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack1_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack2_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack3_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack4_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack5_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack6_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack7_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack8_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack9_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack10_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack11_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack12_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack13_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack14_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmTrack15_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmStreamType_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void StmAllocateChannelsNum_ValueChanged(object sender, EventArgs e) { }

        public virtual void StmCopyChannelCountFromFile_Click(object sender, EventArgs e) { }

        public virtual void StmPitch_ValueChanged(object sender, EventArgs e) { }

        public virtual void StmIncludeExtension_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmLoopStartFrame_ValueChanged(object sender, EventArgs e) { }

        public virtual void StmLoopEndFrame_ValueChanged(object sender, EventArgs e) { }

        public virtual void StmCopyExtensionFromFile_Click(object sender, EventArgs e) { }

        public virtual void StmGeneratePrefetch_CheckedChanged(object sender, EventArgs e) { }

        public virtual void StmPrefetchFileIdBox_SelectedIndexChanged(
            object sender,
            EventArgs e
        )
        { }

        public virtual void StmUpdatePrefetchInfo_Click(object sender, EventArgs e) { }

        public virtual void StmCreateUniquePrefetchFile_Click(object sender, EventArgs e) { }

        public virtual void StmSendMain_ValueChanged(object sender, EventArgs e) { }

        public virtual void StmSendA_ValueChanged(object sender, EventArgs e) { }

        public virtual void StmSendB_ValueChanged(object sender, EventArgs e) { }

        public virtual void StmSendC_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackVolume_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackPan_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackSpan_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackSurround_CheckedChanged(object sender, EventArgs e) { }

        public virtual void TrackLPFFrequency_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackBiquadType_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void TrackBiquadValue_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackSendMain_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackSendA_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackSendB_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackSendC_ValueChanged(object sender, EventArgs e) { }

        public virtual void TrackChannelsChanged(object sender, EventArgs e) { }

        public virtual void ByteOrderBox_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void VersionMax_ValueChanged(object sender, EventArgs e) { }

        public virtual void VersionMin_ValueChanged(object sender, EventArgs e) { }

        public virtual void VersionRev_ValueChanged(object sender, EventArgs e) { }

        public virtual void SeqVersionUpdate_Click(object sender, EventArgs e) { }

        public virtual void BankVersionUpdate_Click(object sender, EventArgs e) { }

        public virtual void WarVersionUpdate_Click(object sender, EventArgs e) { }

        public virtual void WsdVersionUpdate_Click(object sender, EventArgs e) { }

        public virtual void GrpVersionUpdate_Click(object sender, EventArgs e) { }

        public virtual void StmVersionUpdate_Click(object sender, EventArgs e) { }

        public virtual void StpVersionUpdate_Click(object sender, EventArgs e) { }

        public virtual void FilesIncludeGroups_CheckedChanged(object sender, EventArgs e) { }

        public virtual void FilesGroupGridCellChanged(object sender, EventArgs e) { }

        public virtual void ReplaceToolStripMenuItem_Click(object sender, EventArgs e) { }

        public virtual void ExportToolStripMenuItem_Click(object sender, EventArgs e) { }

        public virtual void ChangeExternalPathToolStripMenuItem_Click(
            object sender,
            EventArgs e
        )
        { }

        public virtual void SarAddAbove_Click(object sender, EventArgs e) { }

        public virtual void SarAddBelow_Click(object sender, EventArgs e) { }

        public virtual void SarAddInside_Click(object sender, EventArgs e) { }

        public virtual void SarMoveUp_Click(object sender, EventArgs e) { }

        public virtual void SarMoveDown_Click(object sender, EventArgs e) { }

        public virtual void SarReplace_Click(object sender, EventArgs e) { }

        public virtual void SarExport_Click(object sender, EventArgs e) { }

        public virtual void SarRename_Click(object sender, EventArgs e) { }

        public virtual void SarNullify_Click(object sender, EventArgs e) { }

        public virtual void SarDelete_Click(object sender, EventArgs e) { }

        #region WsdEditor
        public virtual void WsdTrackPlay_Click(object sender, EventArgs e) { }

        public virtual void WsdTrackPause_Click(object sender, EventArgs e) { }

        public virtual void WsdTrackStop_Click(object sender, EventArgs e) { }

        public virtual void WsdTrackPlayOnce_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdTrackPlayLoop_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdPlayNext_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdEventGrid_CellChange(object sende, EventArgs e) { }

        public virtual void WsdEntryPlay_Click(object sender, EventArgs e) { }

        public virtual void WsdEntryPause_Click(object sender, EventArgs e) { }

        public virtual void WsdEntryStop_Click(object sender, EventArgs e) { }

        public virtual void WsdEntryPlayOnce_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdEntryPlayLoop_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdEntryPlayNext_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdAttack_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdDecay_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdSustain_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdRelease_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdHold_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdLPF_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdBiquadType_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void WsdBiquadValue_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdSendMain_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdSendA_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdSendB_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdSendC_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdPan_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdSpan_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdPitch_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdReference_CellChanged(object sender, EventArgs e) { }

        public virtual void WsdReferencePlay_Click(object sender, EventArgs e) { }

        public virtual void WsdReferencePause_Click(object sender, EventArgs e) { }

        public virtual void WsdReferenceStop_Click(object sender, EventArgs e) { }

        public virtual void WsdReferencePlayOnce_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdReferencePlayLoop_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdReferencePlayNext_CheckedChanged(object sender, EventArgs e) { }

        public virtual void WsdRefArchiveCombo_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void WsdRefArchiveBox_ValueChanged(object sender, EventArgs e) { }

        public virtual void WsdRefWaveCombo_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void WsdRefWaveBox_ValueChanged(object sender, EventArgs e) { }
        #endregion
        #region VersionChange
        public virtual void VMajBox_ValueChanged(object sender, EventArgs e) { }

        public virtual void VMinBox_ValueChanged(object sender, EventArgs e) { }

        public virtual void VRevBox_ValueChanged(object sender, EventArgs e) { }
        #endregion
        #region NoteInfo
        public virtual void NoteReferenceWave_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void NoteInterpolationType_SelectedIndexChanged(
            object sender,
            EventArgs e
        )
        { }

        public virtual void NotePercussionMode_CheckedChanged(object sender, EventArgs e) { }

        public virtual void NotePitchSemitones_ValueChanged(object sender, EventArgs e) { }

        public virtual void NotePitchCents_ValueChanged(object sender, EventArgs e) { }

        public virtual void NoteVolume_ValueChanged(object sender, EventArgs e) { }

        public virtual void NotePan_ValueChanged(object sender, EventArgs e) { }

        public virtual void NoteSurroundPan_ValueChanged(object sender, EventArgs e) { }

        public virtual void NoteOriginalKey_ValueChanged(object sender, EventArgs e) { }

        public virtual void NoteKeyGroup_SelectedIndexChanged(object sender, EventArgs e) { }

        public virtual void NoteAttack_ValueChanged(object sender, EventArgs e) { }

        public virtual void NoteDecay_ValueChanged(object sender, EventArgs e) { }

        public virtual void NoteSustain_ValueChanged(object sender, EventArgs e) { }

        public virtual void NoteRelease_ValueChanged(object sender, EventArgs e) { }

        public virtual void NoteHold_ValueChanged(object sender, EventArgs e) { }

        public virtual void InstrumentApplyChanges_Click(object sender, EventArgs e) { }
        #endregion
        private void PianoChanged(object sender, EventArgs e)
        {
            foreach (var kvp in pianoKeys)
            {
                if (kvp.Value.IsKeyOn())
                {
                    NoteDown = kvp.Key;
                    OnPianoPress();
                    return;
                }
            }
            OnPianoRelease();
        }

        public virtual void OnPianoPress() { }

        public virtual void OnPianoRelease() { }

        private void ExportStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportStrings();
        }

        public virtual void ExportStrings() { }

        private void SequenceEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SequenceEditor ed = new(this as MainWindow);
            ed.Show();
        }

        private void SequenceArchiveEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SequenceArchiveEditor ed = new(this as MainWindow);
            ed.Show();
        }

        private void BankEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BankEditor ed = new(this as MainWindow);
            ed.Show();
        }

        private void WaveArchiveEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WaveArchiveEditor ed = new(this as MainWindow);
            ed.Show();
        }

        private void BankGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileOpen || File == null)
            {
                _ = MessageBox.Show("There must be a file open to do this!");
                return;
            }
            BankGenerator ed = new(this as MainWindow);
            ed.Show();
        }

        private void CreaveWaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateStreamTool ed = new(true);
            ed.Show();
        }

        private void CreateStreamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateStreamTool ed = new(false);
            ed.Show();
        }

        private void ExportSDKProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new()
            {
                Filter = "Sound Project|*.sprj"
            };
            if (FilePath is not null and not "")
            {
                s.FileName = Path.GetFileNameWithoutExtension(FilePath) + ".sprj";
            }
            s.RestoreDirectory = true;
            if (s.ShowDialog() == DialogResult.OK)
            {
                (File as SoundArchive).ExportSDKProject(
                    Path.GetDirectoryName(s.FileName),
                    Path.GetFileNameWithoutExtension(s.FileName)
                );
            }
        }

        private void GetHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _ = System.Diagnostics.Process.Start("https://gota7.github.io/NitroStudio2/#guide");
            }
            catch { }
        }

        // Piano key data: (Notes, x, w, h, shape, isBlack, tabIndex, text)
        private static readonly (Notes note, int x, int w, int h, PianoKeyShape shape, bool isBlack, int tabIdx, string text)[] PianoKeyDefs =
        {
            (Notes.cn7, 466, 12, 42, PianoKeyShape.LShape, false, 83, null),
            (Notes.en7, 488, 12, 42, PianoKeyShape.LShapeBackwards, false, 84, null),
            (Notes.cs7, 474, 8, 28, PianoKeyShape.RectShape, true, 74, null),
            (Notes.dn7, 477, 12, 42, PianoKeyShape.TShape, false, 73, null),
            (Notes.ds7, 485, 8, 29, PianoKeyShape.RectShape, true, 75, null),
            (Notes.fn7, 499, 12, 42, PianoKeyShape.LShape, false, 76, null),
            (Notes.fs7, 507, 8, 28, PianoKeyShape.RectShape, true, 78, null),
            (Notes.gn7, 510, 12, 42, PianoKeyShape.TShape, false, 77, null),
            (Notes.gs7, 518, 8, 29, PianoKeyShape.RectShape, true, 79, null),
            (Notes.an7, 521, 12, 42, PianoKeyShape.TShape, false, 80, null),
            (Notes.as7, 529, 8, 29, PianoKeyShape.RectShape, true, 81, "pianoKey13"),
            (Notes.bn7, 532, 12, 42, PianoKeyShape.LShapeBackwards, false, 82, null),
            (Notes.cn6, 389, 12, 42, PianoKeyShape.LShape, false, 71, null),
            (Notes.en6, 411, 12, 42, PianoKeyShape.LShapeBackwards, false, 72, null),
            (Notes.cs6, 397, 8, 28, PianoKeyShape.RectShape, true, 62, null),
            (Notes.dn6, 400, 12, 42, PianoKeyShape.TShape, false, 61, null),
            (Notes.ds6, 408, 8, 29, PianoKeyShape.RectShape, true, 63, null),
            (Notes.fn6, 422, 12, 42, PianoKeyShape.LShape, false, 64, null),
            (Notes.fs6, 430, 8, 28, PianoKeyShape.RectShape, true, 66, null),
            (Notes.gn6, 433, 12, 42, PianoKeyShape.TShape, false, 65, null),
            (Notes.gs6, 441, 8, 29, PianoKeyShape.RectShape, true, 67, null),
            (Notes.an6, 444, 12, 42, PianoKeyShape.TShape, false, 68, null),
            (Notes.as6, 452, 8, 29, PianoKeyShape.RectShape, true, 69, "pianoKey13"),
            (Notes.bn6, 455, 12, 42, PianoKeyShape.LShapeBackwards, false, 70, null),
            (Notes.cn1, 4, 12, 42, PianoKeyShape.LShape, false, 0, null),
            (Notes.cs1, 12, 8, 28, PianoKeyShape.RectShape, true, 3, null),
            (Notes.dn1, 15, 12, 42, PianoKeyShape.TShape, false, 1, null),
            (Notes.ds1, 23, 8, 29, PianoKeyShape.RectShape, true, 4, null),
            (Notes.en1, 26, 12, 42, PianoKeyShape.LShapeBackwards, false, 2, null),
            (Notes.fn1, 37, 12, 42, PianoKeyShape.LShape, false, 5, null),
            (Notes.fs1, 45, 8, 28, PianoKeyShape.RectShape, true, 7, null),
            (Notes.gn1, 48, 12, 42, PianoKeyShape.TShape, false, 6, null),
            (Notes.gs1, 56, 8, 29, PianoKeyShape.RectShape, true, 8, null),
            (Notes.an1, 59, 12, 42, PianoKeyShape.TShape, false, 9, null),
            (Notes.as1, 67, 8, 29, PianoKeyShape.RectShape, true, 10, "pianoKey13"),
            (Notes.bn1, 70, 12, 42, PianoKeyShape.LShapeBackwards, false, 11, null),
            (Notes.cn2, 81, 12, 42, PianoKeyShape.LShape, false, 12, null),
            (Notes.cs2, 89, 8, 28, PianoKeyShape.RectShape, true, 15, null),
            (Notes.dn2, 92, 12, 42, PianoKeyShape.TShape, false, 13, null),
            (Notes.ds2, 100, 8, 29, PianoKeyShape.RectShape, true, 16, null),
            (Notes.en2, 103, 12, 42, PianoKeyShape.LShapeBackwards, false, 14, null),
            (Notes.fn2, 114, 12, 42, PianoKeyShape.LShape, false, 17, null),
            (Notes.fs2, 122, 8, 28, PianoKeyShape.RectShape, true, 19, null),
            (Notes.gn2, 125, 12, 42, PianoKeyShape.TShape, false, 18, null),
            (Notes.gs2, 133, 8, 29, PianoKeyShape.RectShape, true, 20, null),
            (Notes.an2, 136, 12, 42, PianoKeyShape.TShape, false, 21, null),
            (Notes.as2, 144, 8, 29, PianoKeyShape.RectShape, true, 22, "pianoKey13"),
            (Notes.bn2, 147, 12, 42, PianoKeyShape.LShapeBackwards, false, 23, null),
            (Notes.cn3, 158, 12, 42, PianoKeyShape.LShape, false, 24, null),
            (Notes.cs3, 166, 8, 28, PianoKeyShape.RectShape, true, 27, null),
            (Notes.dn3, 169, 12, 42, PianoKeyShape.TShape, false, 25, null),
            (Notes.ds3, 177, 8, 29, PianoKeyShape.RectShape, true, 28, null),
            (Notes.en3, 180, 12, 42, PianoKeyShape.LShapeBackwards, false, 26, null),
            (Notes.fn3, 191, 12, 42, PianoKeyShape.LShape, false, 29, null),
            (Notes.fs3, 199, 8, 28, PianoKeyShape.RectShape, true, 31, null),
            (Notes.gn3, 202, 12, 42, PianoKeyShape.TShape, false, 30, null),
            (Notes.gs3, 210, 8, 29, PianoKeyShape.RectShape, true, 32, null),
            (Notes.an3, 213, 12, 42, PianoKeyShape.TShape, false, 33, null),
            (Notes.as3, 221, 8, 29, PianoKeyShape.RectShape, true, 34, "pianoKey13"),
            (Notes.bn3, 224, 12, 42, PianoKeyShape.LShapeBackwards, false, 35, null),
            (Notes.cn4, 235, 12, 42, PianoKeyShape.LShape, false, 36, null),
            (Notes.cs4, 243, 8, 28, PianoKeyShape.RectShape, true, 39, null),
            (Notes.dn4, 246, 12, 42, PianoKeyShape.TShape, false, 37, null),
            (Notes.ds4, 254, 8, 29, PianoKeyShape.RectShape, true, 40, null),
            (Notes.en4, 257, 12, 42, PianoKeyShape.LShapeBackwards, false, 38, null),
            (Notes.fn4, 268, 12, 42, PianoKeyShape.LShape, false, 41, null),
            (Notes.fs4, 276, 8, 28, PianoKeyShape.RectShape, true, 43, null),
            (Notes.gn4, 279, 12, 42, PianoKeyShape.TShape, false, 42, null),
            (Notes.gs4, 287, 8, 29, PianoKeyShape.RectShape, true, 44, null),
            (Notes.an4, 290, 12, 42, PianoKeyShape.TShape, false, 45, null),
            (Notes.as4, 298, 8, 29, PianoKeyShape.RectShape, true, 46, "pianoKey13"),
            (Notes.bn4, 301, 12, 42, PianoKeyShape.LShapeBackwards, false, 47, null),
            (Notes.cn5, 312, 12, 42, PianoKeyShape.LShape, false, 48, null),
            (Notes.cs5, 320, 8, 28, PianoKeyShape.RectShape, true, 51, null),
            (Notes.dn5, 323, 12, 42, PianoKeyShape.TShape, false, 49, null),
            (Notes.ds5, 331, 8, 29, PianoKeyShape.RectShape, true, 52, null),
            (Notes.en5, 334, 12, 42, PianoKeyShape.LShapeBackwards, false, 50, null),
            (Notes.fn5, 345, 12, 42, PianoKeyShape.LShape, false, 53, null),
            (Notes.fs5, 353, 8, 28, PianoKeyShape.RectShape, true, 55, null),
            (Notes.gn5, 356, 12, 42, PianoKeyShape.TShape, false, 54, null),
            (Notes.gs5, 364, 8, 29, PianoKeyShape.RectShape, true, 56, null),
            (Notes.an5, 367, 12, 42, PianoKeyShape.TShape, false, 57, null),
            (Notes.as5, 375, 8, 29, PianoKeyShape.RectShape, true, 58, "pianoKey13"),
            (Notes.bn5, 378, 12, 42, PianoKeyShape.LShapeBackwards, false, 59, null),
            (Notes.cn8, 543, 12, 42, PianoKeyShape.RectShape, false, 60, null),
        };

        private void InitPianoKeys()
        {
            foreach (var (note, x, w, h, shape, isBlack, tabIdx, text) in PianoKeyDefs)
            {
                var key = new PianoKey();
                pianoKeys[note] = key;
            }
        }

        private void InitPianoKeysProperties()
        {
            foreach (var (note, x, w, h, shape, isBlack, tabIdx, text) in PianoKeyDefs)
            {
                var key = pianoKeys[note];
                if (isBlack)
                {
                    key.BackColor    = System.Drawing.Color.Black;
                    key.KeyOffColor  = System.Drawing.Color.Black;
                }
                else
                {
                    key.KeyOffColor = System.Drawing.Color.White;
                }
                key.KeyOnColor  = System.Drawing.Color.Blue;
                key.Location    = new System.Drawing.Point(x, 2);
                key.Name        = "pkey" + note.ToString();
                key.Orientation = PianoKeyOrientation.Vertical;
                key.Shape       = shape;
                key.Size        = new System.Drawing.Size(w, h);
                key.TabIndex    = tabIdx;
                if (text != null) key.Text = text;
                key.StateChanged += new System.EventHandler(PianoChanged);
            }
        }
        public void ColorRegion(Color color, byte start, byte end)
        {
            for (byte b = start; b <= end; b++)
            {
                PianoKey n = GetKey((Notes)b);
                _ = (n?.KeyOffColor = n.Shape == PianoKeyShape.RectShape
                    && !ReferenceEquals(n, pianoKeys.GetValueOrDefault(Notes.cn8))
                    ? Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B)
                    : color);
            }
        }

        public PianoKey GetKey(Notes n)
            => pianoKeys.TryGetValue(n, out var key) ? key : null;

        private void AboutNitroStudio2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutWindow a = new();
            _ = a.ShowDialog();
        }

        private void sF2ToDLSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new()
            {
                RestoreDirectory = true,
                Filter = "Sound Font|*.sf2"
            };
            if (o.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog s = new()
                {
                    Filter = "Downloadable Sounds|*.dls",
                    RestoreDirectory = true,
                    FileName = Path.GetFileNameWithoutExtension(o.FileName) + ".dls"
                };
                if (s.ShowDialog() == DialogResult.OK)
                {
                    SoundFont h = new(o.FileName);
                    new DownloadableSounds(h).Write(s.FileName);
                }
            }
        }

        private void dLSToSF2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new()
            {
                RestoreDirectory = true,
                Filter = "Downloadable Sounds|*.dls"
            };
            if (o.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog s = new()
                {
                    Filter = "Sound Font|*.sf2",
                    RestoreDirectory = true,
                    FileName = Path.GetFileNameWithoutExtension(o.FileName) + ".sf2"
                };
                if (s.ShowDialog() == DialogResult.OK)
                {
                    DownloadableSounds h = new(o.FileName);
                    new SoundFont(h).Write(s.FileName);
                }
            }
        }
    }
}
