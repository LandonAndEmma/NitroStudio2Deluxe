using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using NitroFileLoader;
using Timer = System.Windows.Forms.Timer;

namespace NitroStudio2
{
    public class WaveArchiveEditor : EditorBase
    {
        public WaveArchive WA => File as WaveArchive;
        public GotaSoundIO.Sound.Playback.StreamPlayer Player;
        public bool PositionBarFree = true;
        public Timer Timer = new Timer();

        public WaveArchiveEditor(MainWindow mainWindow)
            : base(typeof(WaveArchive), "Wave Archive", "war", "Wave Archive Editor", mainWindow)
        {
            Init();
        }

        public WaveArchiveEditor(string fileToOpen)
            : base(
                typeof(WaveArchive),
                "Wave Archive",
                "war",
                "Wave Archive Editor",
                fileToOpen,
                null
            )
        {
            Init();
        }

        public WaveArchiveEditor(IOFile fileToOpen, MainWindow mainWindow, string fileName)
            : base(
                typeof(WaveArchive),
                "Wave Archive",
                "war",
                "Wave Archive Editor",
                fileToOpen,
                mainWindow,
                fileName
            )
        {
            Init();
        }

        public void Init()
        {
            Player = new GotaSoundIO.Sound.Playback.StreamPlayer();
            Icon = Properties.Resources.War;
            tree.Nodes.RemoveAt(0);
            tree.Nodes.Add("root", "Wave Archive", 5, 5);
            UpdateNodes();
            tree.Nodes[0].Expand();
            FormClosing += new FormClosingEventHandler(WAClosing);
            soundPlayerLabel.Text = "Sound Player Deluxe™";
            kermalisPlayButton.Click += new EventHandler(PlayClick);
            kermalisPauseButton.Click += new EventHandler(PauseClick);
            kermalisStopButton.Click += new EventHandler(StopClick);
            kermalisVolumeSlider.ValueChanged += new EventHandler(VolumeChanged);
            kermalisLoopBox.CheckedChanged += new EventHandler(LoopChanged);
            kermalisPosition.MouseUp += new MouseEventHandler(PositionMouseUp);
            kermalisPosition.MouseDown += new MouseEventHandler(PositionMouseDown);
            tree.KeyPress += new KeyPressEventHandler(KeyPress);
            Timer.Tick += PositionTick;
            Timer.Interval = 10;
            Timer.Start();
        }

        public override void DoInfoStuff()
        {
            base.DoInfoStuff();
            void HideStuff()
            {
                kermalisSoundPlayerPanel.Hide();
                kermalisSoundPlayerPanel.SendToBack();
            }
            if (!FileOpen || File == null)
            {
                return;
            }
            if (tree.SelectedNode.Parent != null)
            {
                blankPanel.BringToFront();
                blankPanel.Show();
                kermalisSoundPlayerPanel.BringToFront();
                kermalisSoundPlayerPanel.Show();
                status.Text =
                    "Wave "
                    + tree.SelectedNode.Index
                    + " Selected. "
                    + (WA.Waves[tree.SelectedNode.Index].Loops ? "(Loops)" : "(Doesn't Loop)")
                    + " File Is "
                    + MainWindow.GetBytesSize(WA.Waves[tree.SelectedNode.Index])
                    + ".";
            }
            else
            {
                HideStuff();
                noInfoPanel.BringToFront();
                noInfoPanel.Show();
                status.Text = "No Valid Info Selected!";
            }
        }

        public override void UpdateNodes()
        {
            BeginUpdateNodes();
            if (FileOpen && File != null)
            {
                tree.Nodes[0].ContextMenuStrip = rootMenu;
                for (int i = 0; i < WA.Waves.Count; i++)
                {
                    tree.Nodes[0].Nodes.Add("wave" + i, "Wave " + i, 14, 14);
                    tree.Nodes[0].Nodes["wave" + i].ContextMenuStrip = nodeMenu;
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

        public void WAClosing(object sender, FormClosingEventArgs e)
        {
            Player.Dispose();
        }

        public void PlayClick(object sender, EventArgs e)
        {
            Player.Stop();
            Player.LoadStream(WA.Waves[tree.SelectedNode.Index]);
            kermalisPosition.Maximum = (int)Player.GetLength();
            kermalisPosition.TickFrequency = kermalisPosition.Maximum / 10;
            kermalisPosition.LargeChange = kermalisPosition.Maximum / 20;
            Player.Play();
        }

        public void PauseClick(object sender, EventArgs e)
        {
            if (Player != null)
            {
                Player.Pause();
            }
        }

        public void StopClick(object sender, EventArgs e)
        {
            if (Player != null)
            {
                Player.Stop();
            }
        }

        public void VolumeChanged(object sender, EventArgs e) { }

        public void LoopChanged(object sender, EventArgs e)
        {
            Player.Loop = kermalisLoopBox.Checked;
        }

        public void PositionTick(object sender, EventArgs e)
        {
            if (Player != null && PositionBarFree)
            {
                kermalisPosition.Value =
                    Player.GetPosition() > kermalisPosition.Maximum
                        ? kermalisPosition.Maximum
                        : (int)Player.GetPosition();
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
            if (e.Button == MouseButtons.Left && Player != null)
            {
                Player.SetPosition((uint)kermalisPosition.Value);
                PositionBarFree = true;
            }
        }

        public void AddWave(int index)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Supported Audio Files|*.wav;*.swav;*.strm";
            o.RestoreDirectory = true;
            o.ShowDialog();
            if (o.FileName != "")
            {
                Wave w = new Wave();
                switch (Path.GetExtension(o.FileName))
                {
                    case ".wav":
                        RiffWave r = new RiffWave(o.FileName);
                        w.FromOtherStreamFile(r);
                        break;
                    case ".swav":
                        w.Read(o.FileName);
                        break;
                    case ".strm":
                        NitroFileLoader.Stream s = new NitroFileLoader.Stream();
                        s.Read(o.FileName);
                        w.FromOtherStreamFile(s);
                        break;
                    default:
                        MessageBox.Show("Unsupported file format!");
                        return;
                }
                WA.Waves.Insert(index, w);
                UpdateNodes();
                DoInfoStuff();
            }
        }

        public override void RootAdd()
        {
            AddWave(WA.Waves.Count);
        }

        public override void NodeAddAbove()
        {
            AddWave(tree.SelectedNode.Index);
        }

        public override void NodeAddBelow()
        {
            AddWave(tree.SelectedNode.Index + 1);
        }

        public override void NodeMoveUp()
        {
            if (Swap(WA.Waves, tree.SelectedNode.Index, tree.SelectedNode.Index - 1))
            {
                tree.SelectedNode = tree.Nodes[0].Nodes[tree.SelectedNode.Index - 1];
                UpdateNodes();
                DoInfoStuff();
            }
        }

        public override void NodeMoveDown()
        {
            if (Swap(WA.Waves, tree.SelectedNode.Index, tree.SelectedNode.Index + 1))
            {
                tree.SelectedNode = tree.Nodes[0].Nodes[tree.SelectedNode.Index + 1];
                UpdateNodes();
                DoInfoStuff();
            }
        }

        public override void NodeReplace()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Supported Audio Files|*.wav;*.swav;*.strm";
            o.RestoreDirectory = true;
            o.ShowDialog();
            if (o.FileName != "")
            {
                Wave w = new Wave();
                switch (Path.GetExtension(o.FileName))
                {
                    case ".wav":
                        RiffWave r = new RiffWave(o.FileName);
                        w.FromOtherStreamFile(r);
                        break;
                    case ".swav":
                        w.Read(o.FileName);
                        break;
                    case ".strm":
                        NitroFileLoader.Stream s = new NitroFileLoader.Stream();
                        s.Read(o.FileName);
                        w.FromOtherStreamFile(s);
                        break;
                    default:
                        MessageBox.Show("Unsupported file format!");
                        return;
                }
                WA.Waves[tree.SelectedNode.Index] = w;
                UpdateNodes();
                DoInfoStuff();
            }
        }

        public override void NodeExport()
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter =
                "Supported Audio Files|*.wav;*.swav;*.strm|Wave|*.wav|Sound Wave|*.swav|Sound Stream|*.strm";
            s.RestoreDirectory = true;
            s.FileName = "Wave " + tree.SelectedNode.Index + ".swav";
            s.ShowDialog();
            if (s.FileName != "")
            {
                Wave w = WA.Waves[tree.SelectedNode.Index];
                switch (Path.GetExtension(s.FileName))
                {
                    case ".wav":
                        RiffWave r = new RiffWave();
                        r.FromOtherStreamFile(w);
                        r.Write(s.FileName);
                        break;
                    case ".swav":
                        w.Write(s.FileName);
                        break;
                    case ".strm":
                        NitroFileLoader.Stream stm = new NitroFileLoader.Stream();
                        stm.FromOtherStreamFile(w);
                        stm.Write(s.FileName);
                        break;
                    default:
                        MessageBox.Show("Unsupported file format!");
                        return;
                }
            }
        }

        public override void NodeDelete()
        {
            WA.Waves.RemoveAt(tree.SelectedNode.Index);
            UpdateNodes();
            DoInfoStuff();
        }

        public new void KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ' && tree.SelectedNode.Parent != null)
            {
                PlayClick(sender, e);
            }
        }
    }
}
