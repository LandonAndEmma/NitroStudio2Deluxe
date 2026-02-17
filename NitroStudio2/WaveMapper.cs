using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NitroStudio2
{
    public partial class WaveMapper : Form
    {
        public List<ushort> WarMap = null;
        private readonly List<RiffWave> wavs = [];
        public GotaSoundIO.Sound.Playback.StreamPlayer Player =
            new();

        public WaveMapper(List<RiffWave> waves, List<WaveArchiveInfo> wars, bool hideId = false)
        {
            if (wars.Count < 1)
            {
                _ = MessageBox.Show("The target bank must be hooked up to at least one wave archive.");
                Close();
                return;
            }
            InitializeComponent();
            mapGrid.CellContentClick += new DataGridViewCellEventHandler(PlayRegionButtonClick);
            if (hideId)
            {
                mapGrid.Columns[1].Visible = false;
            }
            FormClosing += new FormClosingEventHandler(OnClosing);
            foreach (WaveArchiveInfo w in wars)
            {
                _ = waveArchive.Items.Add("[" + w.Index + "] " + w.Name);
            }
            int num = 0;
            wavs = waves;
            foreach (RiffWave wav in waves)
            {
                _ = mapGrid.Rows.Add(new DataGridViewRow());
                DataGridViewRow v = mapGrid.Rows[^1];
                ((DataGridViewTextBoxCell)v.Cells[1]).Value = num++;
                ((DataGridViewComboBoxCell)v.Cells[2]).Value = waveArchive.Items[0];
            }
        }

        private void finishedButton_Click(object sender, EventArgs e)
        {
            WarMap = [];
            foreach (DataGridViewRow r in mapGrid.Rows)
            {
                string s = (string)((DataGridViewComboBoxCell)r.Cells[2]).Value;
                WarMap.Add(ushort.Parse(s.Split(']')[0].Split('[')[1]));
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
            Player.LoadStream(wavs[e.RowIndex]);
            Player.Play();
        }

        private void OnClosing(object sender, EventArgs e)
        {
            Player.Dispose();
        }
    }
}
