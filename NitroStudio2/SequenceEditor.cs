using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.IO;
using NitroFileLoader;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static GotaSequenceLib.Playback.Player;

namespace NitroStudio2
{
    public class SequenceEditor : EditorBase
    {
        public Player Player;
        public Mixer Mixer = new();
        private const int BACK_COLOR = 0x2F2F2F;
        private const int FORE_COLOR = 0xB7B7B7;
        public Sequence SEQ => File as Sequence;
        private int prevLine = -1;
        private bool prevLineBlank = true;
        public bool PositionBarFree = true;
        public Timer Timer = new();

        public SequenceEditor(MainWindow mainWindow)
            : base(typeof(Sequence), "Sequence", "seq", "Sequence Editor", mainWindow)
        {
            Init();
            LoadSequenceText();
        }

        public SequenceEditor(string fileToOpen)
            : base(typeof(Sequence), "Sequence", "seq", "Sequence Editor", fileToOpen, null)
        {
            Init();
            LoadSequenceText(Path.GetFileNameWithoutExtension(fileToOpen));
        }

        public SequenceEditor(IOFile fileToOpen, MainWindow mainWindow, string fileName)
            : base(
                typeof(Sequence),
                "Sequence",
                "seq",
                "Sequence Editor",
                fileToOpen,
                mainWindow,
                fileName
            )
        {
            Init();
            LoadSequenceText(fileName);
        }

        public void Init()
        {
            Icon = Properties.Resources.Seq;
            tree.SendToBack();
            tree.Hide();
            sequenceEditorPanel.BringToFront();
            sequenceEditorPanel.Show();
            seqBankPanel.BringToFront();
            seqBankPanel.Show();
            kermalisSoundPlayerPanel.Show();
            Player = new Player(Mixer);
            Player.NotePressed += new NotePressedHandler(NotePressed);
            Player.NoteReleased += new NotePressedHandler(NoteReleased);
            track0Box.CheckedChanged += new EventHandler(Track0CheckChanged);
            track1Box.CheckedChanged += new EventHandler(Track1CheckChanged);
            track2Box.CheckedChanged += new EventHandler(Track2CheckChanged);
            track3Box.CheckedChanged += new EventHandler(Track3CheckChanged);
            track4Box.CheckedChanged += new EventHandler(Track4CheckChanged);
            track5Box.CheckedChanged += new EventHandler(Track5CheckChanged);
            track6Box.CheckedChanged += new EventHandler(Track6CheckChanged);
            track7Box.CheckedChanged += new EventHandler(Track7CheckChanged);
            track8Box.CheckedChanged += new EventHandler(Track8CheckChanged);
            track9Box.CheckedChanged += new EventHandler(Track9CheckChanged);
            track10Box.CheckedChanged += new EventHandler(Track10CheckChanged);
            track11Box.CheckedChanged += new EventHandler(Track11CheckChanged);
            track12Box.CheckedChanged += new EventHandler(Track12CheckChanged);
            track13Box.CheckedChanged += new EventHandler(Track13CheckChanged);
            track14Box.CheckedChanged += new EventHandler(Track14CheckChanged);
            track15Box.CheckedChanged += new EventHandler(Track15CheckChanged);
            track0Solo.Click += new EventHandler(Track0Solo);
            track1Solo.Click += new EventHandler(Track1Solo);
            track2Solo.Click += new EventHandler(Track2Solo);
            track3Solo.Click += new EventHandler(Track3Solo);
            track4Solo.Click += new EventHandler(Track4Solo);
            track5Solo.Click += new EventHandler(Track5Solo);
            track6Solo.Click += new EventHandler(Track6Solo);
            track7Solo.Click += new EventHandler(Track7Solo);
            track8Solo.Click += new EventHandler(Track8Solo);
            track9Solo.Click += new EventHandler(Track9Solo);
            track10Solo.Click += new EventHandler(Track10Solo);
            track11Solo.Click += new EventHandler(Track11Solo);
            track12Solo.Click += new EventHandler(Track12Solo);
            track13Solo.Click += new EventHandler(Track13Solo);
            track14Solo.Click += new EventHandler(Track14Solo);
            track15Solo.Click += new EventHandler(Track15Solo);
            Mixer.Volume = .75f;
            sequenceEditor.Insert += scintilla_Insert;
            sequenceEditor.Delete += scintilla_Delete;
            kermalisPlayButton.Click += new EventHandler(PlayClick);
            kermalisPauseButton.Click += new EventHandler(PauseClick);
            kermalisStopButton.Click += new EventHandler(StopClick);
            kermalisVolumeSlider.ValueChanged += new EventHandler(VolumeChanged);
            kermalisLoopBox.CheckedChanged += new EventHandler(LoopChanged);
            kermalisPosition.MouseUp += new MouseEventHandler(PositionMouseUp);
            kermalisPosition.MouseDown += new MouseEventHandler(PositionMouseDown);
            FormClosing += new FormClosingEventHandler(SEClosing);
            Load += new System.EventHandler(SequenceEditor_Load);
            seqEditorBankBox.ValueChanged += new EventHandler(BankBoxChanged);
            seqEditorBankComboBox.SelectedIndexChanged += new EventHandler(BankComboChanged);
            status.Text = "Editing A Sequence.";
            sequenceEditor.KeyDown += new KeyEventHandler(genericScintilla_KeyDown);
            splitContainer1.SplitterDistance += 20;
            Timer.Tick += PositionTick;
            Timer.Interval = 1000 / 30;
            Timer.Start();
            exportMidiButton.Click += new EventHandler(ExportMidi);
            exportWavButton.Click += new EventHandler(ExportWav);
            if (MainWindow == null || MainWindow.SA == null)
            {
                seqEditorBankComboBox.Enabled = false;
                seqEditorBankBox.Enabled = false;
                splitContainer1.SplitterDistance = 0;
                splitContainer1.IsSplitterFixed = true;
            }
            else
            {
                MainWindow.PopulateBankBox(MainWindow.SA, seqEditorBankComboBox);
                WritingInfo = true;
                seqEditorBankComboBox.SelectedIndex = 0;
                seqEditorBankBox.Value = 0;
                WritingInfo = false;
                try
                {
                    seqEditorBankComboBox.SelectedIndex = 1;
                }
                catch { }
            }
        }

        public override void UpdateNodes()
        {
            if (FileOpen && File != null)
            {
                if (MainWindow != null && MainWindow.SA != null)
                {
                    splitContainer1.Panel1.Show();
                }
                sequenceEditor.Enabled = true;
            }
            else
            {
                splitContainer1.Panel1.Hide();
                sequenceEditor.Enabled = false;
            }
        }

        public override void DoInfoStuff() { }

        private void GotoButton_Click(object sender, EventArgs e) { }

        private void genericScintilla_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.G)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void SequenceEditor_Load(object sender, EventArgs e)
        {
            sequenceEditor.Dock = DockStyle.Fill;
            sequenceEditor.WrapMode = WrapMode.None;
            sequenceEditor.IndentationGuides = IndentView.LookBoth;
            sequenceEditor.StyleResetDefault();
            sequenceEditor.Styles[Style.Default].Font = "Consolas";
            sequenceEditor.Styles[Style.Default].Size = 11;
            sequenceEditor.Styles[Style.Default].BackColor = IntToColor(0x212121);
            sequenceEditor.Styles[Style.Default].ForeColor = IntToColor(0xE7E7E7);
            sequenceEditor.CaretForeColor = IntToColor(0xFFFFFF);
            sequenceEditor.StyleClearAll();
            sequenceEditor.ScrollWidth = 1;
            sequenceEditor.ScrollWidthTracking = true;
            sequenceEditor.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            sequenceEditor.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            sequenceEditor.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            sequenceEditor.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);
            sequenceEditor.LexerName = "";
            sequenceEditor.StyleNeeded += new EventHandler<StyleNeededEventArgs>(
                SEQ_StyleNeeded
            );
            sequenceEditor.TextChanged += new EventHandler(SEQ_ChangedText);
            StyleSeq(0, sequenceEditor.Text.Length);
            UpdateLineNumbers(0, sequenceEditor.Text.Length);
        }

        private void SEQ_ChangedText(object sender, EventArgs e)
        {
            string s = sequenceEditor.Lines[sequenceEditor.CurrentLine].Text;
            if (s.Contains(";"))
            {
                _ = s.Split(';')[0];
            }
            string ss = sequenceEditor
                .Lines[sequenceEditor.CurrentLine]
                .Text.Replace(" ", "")
                .Replace("\t", "")
                .Replace("\r", "")
                .Replace("\n", "");
            if (
                sequenceEditor.CurrentLine != prevLine
                || (prevLineBlank != (ss.EndsWith(":") || ss == ""))
            )
            {
                UpdateLineNumbers(sequenceEditor.CurrentLine, sequenceEditor.Lines.Count);
                prevLine = sequenceEditor.CurrentLine;
                prevLineBlank = ss.EndsWith(":") || ss == "";
            }
        }

        public void UpdateSequence()
        {
            try
            {
                List<SequenceCommand> commands = [];
                SEQ.FromText(sequenceEditor.Text.Replace('\r', '\n').Split('\n').ToList());
                UpdateNodes();
            }
            catch (Exception exe)
            {
                _ = MessageBox.Show(exe.Message);
            }
        }

        private void SEQ_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            int startPos = sequenceEditor.GetEndStyled();
            int endPos = e.Position;
            if (startPos >= 500)
            {
                startPos -= 500;
            }
            else
            {
                startPos = 0;
            }
            if ((sequenceEditor.Text.Length - endPos) >= 500)
            {
                endPos += 500;
            }
            else
            {
                endPos = sequenceEditor.Text.Length;
            }
            StyleSeq(startPos, endPos);
        }

        public void StyleSeq(int startPos, int endPos)
        {
            sequenceEditor.Styles[(int)CommandStyleType.Regular].ForeColor = IntToColor(0xE7E7E7);
            sequenceEditor.Styles[(int)CommandStyleType.Comment].ForeColor = IntToColor(0xAEAEAE);
            sequenceEditor.Styles[(int)CommandStyleType.Label].ForeColor = IntToColor(0xE7BB00);
            sequenceEditor.Styles[(int)CommandStyleType.Prefix].ForeColor = IntToColor(0x4AF0B6);
            sequenceEditor.Styles[(int)CommandStyleType.Value0].ForeColor = Color.Red;
            sequenceEditor.Styles[(int)CommandStyleType.Value1].ForeColor = Color.Orange;
            sequenceEditor.Styles[(int)CommandStyleType.Value2].ForeColor = Color.Yellow;
            sequenceEditor.Styles[(int)CommandStyleType.Value3].ForeColor = Color.LimeGreen;
            sequenceEditor.Styles[(int)CommandStyleType.Value4].ForeColor = Color.LightBlue;
            sequenceEditor.Styles[(int)CommandStyleType.Value5].ForeColor = Color.PaleVioletRed;
            int pos = startPos;
            if (endPos > sequenceEditor.Text.Length)
            {
                endPos = sequenceEditor.Text.Length;
            }

            string[] lines = sequenceEditor.Text[startPos..endPos].Split('\n');
            foreach (string s in lines)
            {
                CommandStyleType style = CommandStyleType.Regular;
                bool initialSpaceCut = false;
                string withoutInitialSpace = s.Replace("\t", " ");
                int numWhiteSpace = 0;
                for (int j = 0; j < s.Length; j++)
                {
                    string l = s.Replace("\t", " ");
                    if (l.Contains(":") && j == 0)
                    {
                        sequenceEditor.StartStyling(pos);
                        sequenceEditor.SetStyling(l.IndexOf(':') + 1, (int)CommandStyleType.Label);
                        j += l.IndexOf(':') + 1;
                        if (j >= l.Length)
                        {
                            break;
                        }
                    }
                    bool kill = false;
                    while ((l[j] == ' ') && !initialSpaceCut)
                    {
                        j++;
                        if (j >= l.Length)
                        {
                            kill = true;
                            break;
                        }
                        else
                        {
                            withoutInitialSpace = l[j..];
                            numWhiteSpace = j;
                        }
                    }
                    initialSpaceCut = true;
                    if (kill)
                    {
                        break;
                    }
                    char c = l[j];
                    int ind = j + pos;
                    if (c == ';')
                    {
                        sequenceEditor.StartStyling(ind);
                        sequenceEditor.SetStyling(l.Length - j, (int)CommandStyleType.Comment);
                        break;
                    }
                    if (c == '_')
                    {
                        string p = l[j..].Split(' ')[0];
                        bool afterSpace = false;
                        if (withoutInitialSpace.Contains(" "))
                        {
                            if (j > withoutInitialSpace.IndexOf(" ") + numWhiteSpace)
                            {
                                afterSpace = true;
                            }
                        }
                        if (
                            !afterSpace
                            && (
                                p.Contains("_if ")
                                || p.Contains("_v ")
                                || p.Contains("_r ")
                                || p.Contains("_t ")
                                || p.Contains("_tr ")
                                || p.Contains("_tv ")
                                || p.EndsWith("_if")
                                || p.EndsWith("_v")
                                || p.EndsWith("_t")
                                || p.EndsWith("_tv")
                                || p.EndsWith("_tr")
                                || p.EndsWith("_r")
                            )
                        )
                        {
                            style = CommandStyleType.Prefix;
                        }
                    }
                    if (c == ' ')
                    {
                        if (j > 0)
                        {
                            if (l[j - 1] != ' ')
                            {
                                if (style < CommandStyleType.Prefix)
                                {
                                    style = CommandStyleType.Prefix;
                                }
                                style++;
                            }
                        }
                    }
                    sequenceEditor.StartStyling(ind);
                    sequenceEditor.SetStyling(1, (int)style);
                }
                pos += s.Length + 1;
            }
        }

        public enum CommandStyleType
        {
            Null,
            Regular,
            Comment,
            Label,
            Prefix,
            Value0,
            Value1,
            Value2,
            Value3,
            Value4,
            Value5,
        }

        public static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        public void LoadSequenceText(string name = "Sequence")
        {
            sequenceEditor.Margins[0].Type = MarginType.RightText;
            sequenceEditor.Margins[0].Width = 35;
            if (File != null)
            {
                sequenceEditor.ReadOnly = false;
                SEQ.ReadCommandData();
                SEQ.Name = name;
                sequenceEditor.Text = string.Join("\n", SEQ.ToText());
            }
            else
            {
                sequenceEditor.Text = "{ NULL FILE INFO }";
            }
            UpdateLineNumbers(0, sequenceEditor.Lines.Count);
        }

        private void UpdateLineNumbers(int startingAtLine, int endingAtLine)
        {
            int pastNum = 0;
            if (startingAtLine != 0)
            {
                pastNum = int.Parse(sequenceEditor.Lines[startingAtLine - 1].MarginText);
                string ss = sequenceEditor
                    .Lines[startingAtLine - 1]
                    .Text.Replace(" ", "")
                    .Replace("\t", "")
                    .Replace("\r", "")
                    .Replace("\n", "");
                if (ss != "" && !ss.EndsWith(":"))
                {
                    pastNum++;
                }
            }
            int sum = pastNum;
            if (endingAtLine > sequenceEditor.Lines.Count)
            {
                endingAtLine = sequenceEditor.Lines.Count;
            }
            for (int i = startingAtLine; i < endingAtLine; i++)
            {
                sequenceEditor.Lines[i].MarginStyle = Style.LineNumber;
                string s = sequenceEditor.Lines[i].Text;
                if (s.Contains(";"))
                {
                    s = s.Split(';')[0];
                }
                s = s.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                sequenceEditor.Lines[i].MarginText = "" + sum;
                if (s != "" && !s.EndsWith(":"))
                {
                    sum += 1;
                }
            }
        }

        private void scintilla_Insert(object sender, ModificationEventArgs e)
        {
            if (e.LinesAdded != 0)
            {
                UpdateLineNumbers(0, sequenceEditor.Lines.Count);
            }
        }

        private void scintilla_Delete(object sender, ModificationEventArgs e)
        {
            if (e.LinesAdded != 0)
            {
                UpdateLineNumbers(0, sequenceEditor.Lines.Count);
            }
        }

        public override void newToolStripMenuItem_Click(object sender, EventArgs e)
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
            LoadSequenceText("New Sequence");
        }

        public override void openToolStripMenuItem_Click(object sender, EventArgs e)
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
                LoadSequenceText(Path.GetFileNameWithoutExtension(path));
            }
        }

        public override void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateSequence();
            if (!SEQ.WritingCommandSuccess)
            {
                return;
            }
            SEQ.WriteCommandData();
            base.saveToolStripMenuItem_Click(sender, e);
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
                Filter =
                    "Supported Sound Files|*.sseq;*.smft|Sound Sequence|*.sseq|SMF Text Format|*.smft"
            };
            if (o.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string path = o.FileName;
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
                sequenceEditor.Text = System.IO.File.ReadAllText(path);
            }
        }

        public override void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateSequence();
            if (!SEQ.WritingCommandSuccess)
            {
                return;
            }
            SEQ.WriteCommandData();
            SaveFileDialog s = new()
            {
                RestoreDirectory = true,
                Filter =
                    "Supported Sound Files|*.sseq;*.smft|Sound Sequence|*.sseq|SMF Text Format|*.smft",
                OverwritePrompt = false
            };
            if (s.ShowDialog() == DialogResult.OK)
            {
                if (s.FileName.EndsWith(".smft"))
                {
                    System.IO.File.WriteAllText(s.FileName, sequenceEditor.Text);
                }
                else
                {
                    SEQ.Write(s.FileName);
                }
            }
        }

        public override void blankFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FileTest(sender, e, false, true))
            {
                return;
            }
            string name = SEQ.Name;
            File = (IOFile)Activator.CreateInstance(FileType);
            SEQ.RawData = new byte[0];
            LoadSequenceText(name);
        }

        public void PlayClick(object sender, EventArgs e)
        {
            UpdateSequence();
            if (!SEQ.WritingCommandSuccess)
            {
                return;
            }
            if (MainWindow == null)
            {
                _ = MessageBox.Show("There must be an SDAT connected to this file to play it.");
                return;
            }
            BankInfo b = MainWindow
                .SA.Banks.Where(x => x.Index == (int)seqEditorBankBox.Value)
                .FirstOrDefault();
            if (b == null)
            {
                _ = MessageBox.Show("Bank is not valid or doesn't exist.");
                return;
            }
            Player.PrepareForSong(new PlayableBank[] { b.File }, b.GetAssociatedWaves());
            Player.LoadSong(SEQ.Commands, 0);
            kermalisPosition.Maximum = (int)Player.MaxTicks;
            kermalisPosition.TickFrequency = kermalisPosition.Maximum / 10;
            kermalisPosition.LargeChange = kermalisPosition.Maximum / 20;
            Player.Play();
        }

        public void PauseClick(object sender, EventArgs e)
        {
            Player.Pause();
        }

        public void StopClick(object sender, EventArgs e)
        {
            Player.Stop();
            track0Picture.BackgroundImage = Properties.Resources.Idle;
            track1Picture.BackgroundImage = Properties.Resources.Idle;
            track2Picture.BackgroundImage = Properties.Resources.Idle;
            track3Picture.BackgroundImage = Properties.Resources.Idle;
            track4Picture.BackgroundImage = Properties.Resources.Idle;
            track5Picture.BackgroundImage = Properties.Resources.Idle;
            track6Picture.BackgroundImage = Properties.Resources.Idle;
            track7Picture.BackgroundImage = Properties.Resources.Idle;
            track8Picture.BackgroundImage = Properties.Resources.Idle;
            track9Picture.BackgroundImage = Properties.Resources.Idle;
            track10Picture.BackgroundImage = Properties.Resources.Idle;
            track11Picture.BackgroundImage = Properties.Resources.Idle;
            track12Picture.BackgroundImage = Properties.Resources.Idle;
            track13Picture.BackgroundImage = Properties.Resources.Idle;
            track14Picture.BackgroundImage = Properties.Resources.Idle;
            track15Picture.BackgroundImage = Properties.Resources.Idle;
        }

        public void VolumeChanged(object sender, EventArgs e)
        {
            Mixer.Volume = kermalisVolumeSlider.Value / 100f;
        }

        public void LoopChanged(object sender, EventArgs e)
        {
            Player.NumLoops = kermalisLoopBox.Checked ? 0xFFFFFFFF : 0;
        }

        public void SEClosing(object sender, FormClosingEventArgs e)
        {
            Player.Stop();
            Player.Dispose();
            Mixer.Dispose();
            Timer.Stop();
        }

        public void BankComboChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                WritingInfo = true;
                if (seqEditorBankComboBox.SelectedIndex != 0)
                {
                    seqEditorBankBox.Value = int.Parse(
                        ((string)seqEditorBankComboBox.SelectedItem).Split('[')[1].Split(']')[0]
                    );
                }
                WritingInfo = false;
            }
        }

        public void BankBoxChanged(object sender, EventArgs e)
        {
            if (!WritingInfo)
            {
                WritingInfo = true;
                for (int i = 1; i < seqEditorBankComboBox.Items.Count; i++)
                {
                    if (
                        (int)seqEditorBankBox.Value
                        == int.Parse(
                            ((string)seqEditorBankComboBox.Items[i]).Split('[')[1].Split(']')[0]
                        )
                    )
                    {
                        seqEditorBankComboBox.SelectedIndex = i;
                        WritingInfo = false;
                        return;
                    }
                }
                seqEditorBankComboBox.SelectedIndex = 0;
                WritingInfo = false;
            }
        }

        public override void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.closeToolStripMenuItem_Click(sender, e);
            LoadSequenceText();
        }

        private void NotePressed(object sender, NoteEventArgs e)
        {
            switch (e.TrackId)
            {
                case 0:
                    if (track0Box.Checked)
                    {
                        track0Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 1:
                    if (track1Box.Checked)
                    {
                        track1Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 2:
                    if (track2Box.Checked)
                    {
                        track2Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 3:
                    if (track3Box.Checked)
                    {
                        track3Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 4:
                    if (track4Box.Checked)
                    {
                        track4Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 5:
                    if (track5Box.Checked)
                    {
                        track5Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 6:
                    if (track6Box.Checked)
                    {
                        track6Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 7:
                    if (track7Box.Checked)
                    {
                        track7Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 8:
                    if (track8Box.Checked)
                    {
                        track8Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 9:
                    if (track9Box.Checked)
                    {
                        track9Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 10:
                    if (track10Box.Checked)
                    {
                        track10Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 11:
                    if (track11Box.Checked)
                    {
                        track11Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 12:
                    if (track12Box.Checked)
                    {
                        track12Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 13:
                    if (track13Box.Checked)
                    {
                        track13Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 14:
                    if (track14Box.Checked)
                    {
                        track14Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
                case 15:
                    if (track15Box.Checked)
                    {
                        track15Picture.BackgroundImage = Properties.Resources.NoteDown;
                    }
                    break;
            }
        }

        private void NoteReleased(object sender, NoteEventArgs e)
        {
            switch (e.TrackId)
            {
                case 0:
                    if (track0Box.Checked)
                    {
                        track0Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 1:
                    if (track1Box.Checked)
                    {
                        track1Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 2:
                    if (track2Box.Checked)
                    {
                        track2Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 3:
                    if (track3Box.Checked)
                    {
                        track3Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 4:
                    if (track4Box.Checked)
                    {
                        track4Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 5:
                    if (track5Box.Checked)
                    {
                        track5Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 6:
                    if (track6Box.Checked)
                    {
                        track6Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 7:
                    if (track7Box.Checked)
                    {
                        track7Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 8:
                    if (track8Box.Checked)
                    {
                        track8Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 9:
                    if (track9Box.Checked)
                    {
                        track9Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 10:
                    if (track10Box.Checked)
                    {
                        track10Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 11:
                    if (track11Box.Checked)
                    {
                        track11Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 12:
                    if (track12Box.Checked)
                    {
                        track12Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 13:
                    if (track13Box.Checked)
                    {
                        track13Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 14:
                    if (track14Box.Checked)
                    {
                        track14Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
                case 15:
                    if (track15Box.Checked)
                    {
                        track15Picture.BackgroundImage = Properties.Resources.Idle;
                    }
                    break;
            }
        }

        private void Track0CheckChanged(object sender, EventArgs e)
        {
            bool check = track0Box.Checked;
            track0Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[0] = !check;
        }

        private void Track1CheckChanged(object sender, EventArgs e)
        {
            bool check = track1Box.Checked;
            track1Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[1] = !check;
        }

        private void Track2CheckChanged(object sender, EventArgs e)
        {
            bool check = track2Box.Checked;
            track2Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[2] = !check;
        }

        private void Track3CheckChanged(object sender, EventArgs e)
        {
            bool check = track3Box.Checked;
            track3Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[3] = !check;
        }

        private void Track4CheckChanged(object sender, EventArgs e)
        {
            bool check = track4Box.Checked;
            track4Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[4] = !check;
        }

        private void Track5CheckChanged(object sender, EventArgs e)
        {
            bool check = track5Box.Checked;
            track5Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[5] = !check;
        }

        private void Track6CheckChanged(object sender, EventArgs e)
        {
            bool check = track6Box.Checked;
            track6Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[6] = !check;
        }

        private void Track7CheckChanged(object sender, EventArgs e)
        {
            bool check = track7Box.Checked;
            track7Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[7] = !check;
        }

        private void Track8CheckChanged(object sender, EventArgs e)
        {
            bool check = track8Box.Checked;
            track8Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[8] = !check;
        }

        private void Track9CheckChanged(object sender, EventArgs e)
        {
            bool check = track9Box.Checked;
            track9Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[9] = !check;
        }

        private void Track10CheckChanged(object sender, EventArgs e)
        {
            bool check = track10Box.Checked;
            track10Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[10] = !check;
        }

        private void Track11CheckChanged(object sender, EventArgs e)
        {
            bool check = track11Box.Checked;
            track11Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[11] = !check;
        }

        private void Track12CheckChanged(object sender, EventArgs e)
        {
            bool check = track12Box.Checked;
            track12Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[12] = !check;
        }

        private void Track13CheckChanged(object sender, EventArgs e)
        {
            bool check = track13Box.Checked;
            track13Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[13] = !check;
        }

        private void Track14CheckChanged(object sender, EventArgs e)
        {
            bool check = track14Box.Checked;
            track14Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[14] = !check;
        }

        private void Track15CheckChanged(object sender, EventArgs e)
        {
            bool check = track15Box.Checked;
            track15Picture.BackgroundImage = check ? Properties.Resources.Idle : Properties.Resources.Mute;
            Mixer.Mutes[15] = !check;
        }

        private void Track0Solo(object sender, EventArgs e)
        {
            if (
                track0Box.Checked
                && !(
                    track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = true;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track1Solo(object sender, EventArgs e)
        {
            if (
                track1Box.Checked
                && !(
                    track0Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = true;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track2Solo(object sender, EventArgs e)
        {
            if (
                track2Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = true;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track3Solo(object sender, EventArgs e)
        {
            if (
                track3Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = true;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track4Solo(object sender, EventArgs e)
        {
            if (
                track4Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = true;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track5Solo(object sender, EventArgs e)
        {
            if (
                track5Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = true;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track6Solo(object sender, EventArgs e)
        {
            if (
                track6Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = true;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track7Solo(object sender, EventArgs e)
        {
            if (
                track7Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = true;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track8Solo(object sender, EventArgs e)
        {
            if (
                track8Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = true;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track9Solo(object sender, EventArgs e)
        {
            if (
                track9Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = true;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track10Solo(object sender, EventArgs e)
        {
            if (
                track10Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = true;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track11Solo(object sender, EventArgs e)
        {
            if (
                track11Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = true;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track12Solo(object sender, EventArgs e)
        {
            if (
                track12Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = true;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track13Solo(object sender, EventArgs e)
        {
            if (
                track13Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track14Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = true;
                track14Box.Checked = false;
                track15Box.Checked = false;
            }
        }

        private void Track14Solo(object sender, EventArgs e)
        {
            if (
                track14Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track15Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = true;
                track15Box.Checked = false;
            }
        }

        private void Track15Solo(object sender, EventArgs e)
        {
            if (
                track15Box.Checked
                && !(
                    track0Box.Checked
                    || track1Box.Checked
                    || track2Box.Checked
                    || track3Box.Checked
                    || track4Box.Checked
                    || track5Box.Checked
                    || track6Box.Checked
                    || track7Box.Checked
                    || track8Box.Checked
                    || track9Box.Checked
                    || track10Box.Checked
                    || track11Box.Checked
                    || track12Box.Checked
                    || track13Box.Checked
                    || track14Box.Checked
                )
            )
            {
                track0Box.Checked = true;
                track1Box.Checked = true;
                track2Box.Checked = true;
                track3Box.Checked = true;
                track4Box.Checked = true;
                track5Box.Checked = true;
                track6Box.Checked = true;
                track7Box.Checked = true;
                track8Box.Checked = true;
                track9Box.Checked = true;
                track10Box.Checked = true;
                track11Box.Checked = true;
                track12Box.Checked = true;
                track13Box.Checked = true;
                track14Box.Checked = true;
                track15Box.Checked = true;
            }
            else
            {
                track0Box.Checked = false;
                track1Box.Checked = false;
                track2Box.Checked = false;
                track3Box.Checked = false;
                track4Box.Checked = false;
                track5Box.Checked = false;
                track6Box.Checked = false;
                track7Box.Checked = false;
                track8Box.Checked = false;
                track9Box.Checked = false;
                track10Box.Checked = false;
                track11Box.Checked = false;
                track12Box.Checked = false;
                track13Box.Checked = false;
                track14Box.Checked = false;
                track15Box.Checked = true;
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

        public void ExportMidi(object sender, EventArgs e)
        {
            UpdateSequence();
            if (!SEQ.WritingCommandSuccess)
            {
                return;
            }
            ushort mask = 0;
            for (int i = 0; i < Mixer.Mutes.Length; i++)
            {
                if (!Mixer.Mutes[i])
                {
                    mask |= (ushort)(0b1 << i);
                }
            }
            SaveFileDialog s = new()
            {
                RestoreDirectory = true,
                FileName = Path.GetFileNameWithoutExtension(SEQ.Name) + ".mid",
                Filter = "MIDI|*.mid"
            };
            if (s.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            SEQ.SaveMIDI(s.FileName, mask);
        }

        public void ExportWav(object sender, EventArgs e)
        {
            UpdateSequence();
            if (!SEQ.WritingCommandSuccess)
            {
                return;
            }
            if (MainWindow == null)
            {
                _ = MessageBox.Show("There must be an SDAT connected to this file to record it.");
                return;
            }
            BankInfo b = MainWindow
                .SA.Banks.Where(x => x.Index == (int)seqEditorBankBox.Value)
                .FirstOrDefault();
            if (b == null)
            {
                _ = MessageBox.Show("Bank is not valid or doesn't exist.");
                return;
            }
            SaveFileDialog s = new()
            {
                RestoreDirectory = true,
                FileName = Path.GetFileNameWithoutExtension(SEQ.Name) + ".wav",
                Filter = "Wave File|*.wav"
            };
            if (s.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            SequenceRecorder r = new(
                new PlayableBank[] { b.File },
                b.GetAssociatedWaves(),
                SEQ.Commands,
                0,
                s.FileName
            );
            r.Mixer.Mutes = Mixer.Mutes;
            _ = r.ShowDialog();
        }
    }
}
