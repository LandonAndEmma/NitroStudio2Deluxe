using GotaSequenceLib.Playback;
using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using NitroFileLoader.Instrument;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NitroStudio2
{
    public partial class InstrumentSelector : Form
    {
        public List<int> SelectedInstruments = null;
        private readonly List<RiffWave> wavs = [];
        public Player Player;
        public Mixer Mixer = new();

        public InstrumentSelector(List<RiffWave> waves, List<int> insts, List<string> names)
        {
            InitializeComponent();
            instGrid.CellContentClick += new DataGridViewCellEventHandler(PlayRegionButtonClick);
            wavs = waves;
            Player = new Player(Mixer);
            Player.PrepareForSong(
                new Bank[]
                {
                    new()
                    {
                        Instruments =
                        [
                            new DirectInstrument()
                            {
                                Index = 0,
                                NoteInfo =
                                [
                                    new NoteInfo() { Key = GotaSequenceLib.Notes.gn9 },
                                ],
                            },
                        ],
                    },
                },
                new RiffWave[][] { new RiffWave[1] }
            );
            Player.LoadSong(
                [
                    new GotaSequenceLib.SequenceCommand()
                    {
                        CommandType = GotaSequenceLib.SequenceCommands.ProgramChange,
                        Parameter = (uint)0,
                    },
                    new GotaSequenceLib.SequenceCommand()
                    {
                        CommandType = GotaSequenceLib.SequenceCommands.Note,
                        Parameter = new GotaSequenceLib.NoteParameter()
                        {
                            Note = GotaSequenceLib.Notes.cn4,
                            Length = 48 * 2,
                            Velocity = 127,
                        },
                    },
                    new GotaSequenceLib.SequenceCommand()
                    {
                        CommandType = GotaSequenceLib.SequenceCommands.Fin,
                    },
                ]
            );
            FormClosing += new FormClosingEventHandler(OnClosing);
            int ind = 0;
            foreach (int inst in insts)
            {
                _ = instGrid.Rows.Add(new DataGridViewRow());
                DataGridViewRow v = instGrid.Rows[^1];
                ((DataGridViewTextBoxCell)v.Cells[1]).Value = inst;
                string name = "Instrument " + inst;
                try
                {
                    name = names[ind];
                }
                catch { }
                ind++;
                ((DataGridViewTextBoxCell)v.Cells[2]).Value = name;
                ((DataGridViewCheckBoxCell)v.Cells[3]).Value = true;
            }
        }

        private void finishedButton_Click(object sender, EventArgs e)
        {
            SelectedInstruments = [];
            foreach (DataGridViewRow r in instGrid.Rows)
            {
                if ((bool)((DataGridViewCheckBoxCell)r.Cells[3]).Value)
                {
                    SelectedInstruments.Add(int.Parse(r.Cells[1].Value.ToString()));
                }
            }
            Close();
        }

        public void PlayRegionButtonClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex < 0)
            {
                return;
            }
            Player.Stop();
            Player.WaveArchives[0][0] = wavs[e.RowIndex];
            Player.Play();
        }

        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in instGrid.Rows)
            {
                r.Cells[3].Value = true;
            }
        }

        private void uncheckAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in instGrid.Rows)
            {
                r.Cells[3].Value = false;
            }
        }

        private void OnClosing(object sender, EventArgs e)
        {
            Mixer.Dispose();
            Player.Dispose();
        }
    }
}
