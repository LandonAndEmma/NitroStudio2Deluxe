using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.Sound.Formats;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NitroStudio2
{
    public partial class SequenceRecorder : Form
    {
        public Mixer Mixer = new();
        private readonly Player Player;
        private readonly List<SequenceCommand> commands;
        private readonly int seqStart;
        private readonly string filePath;

        public SequenceRecorder(
            PlayableBank[] banks,
            RiffWave[][] wars,
            List<SequenceCommand> commands,
            int startIndex,
            string filePath
        )
        {
            InitializeComponent();
            Player = new Player(Mixer);
            Player.PrepareForSong(banks, wars);
            this.commands = commands;
            seqStart = startIndex;
            this.filePath = filePath;
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            Player.LoadSong(commands, seqStart);
            Player.NumLoops = (long)loopsBox.Value;
            Player.DontFadeSong = !fadeBox.Checked;
            Player.Record(filePath);
            Close();
        }
    }
}
