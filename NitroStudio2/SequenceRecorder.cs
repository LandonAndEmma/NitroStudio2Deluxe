using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace NitroStudio2 {
    public partial class SequenceRecorder : Form {
        public Mixer Mixer = new Mixer();
        Player Player;
        private List<SequenceCommand> commands;
        private int seqStart;
        private string filePath;
        public SequenceRecorder(PlayableBank[] banks, RiffWave[][] wars, List<SequenceCommand> commands, int startIndex, string filePath) {
            InitializeComponent();
            Player = new Player(Mixer);
            Player.PrepareForSong(banks, wars);
            this.commands = commands;
            this.seqStart = startIndex;
            this.filePath = filePath;
        }
        private void exportButton_Click(object sender, EventArgs e) {
            Player.LoadSong(commands, seqStart);
            Player.NumLoops = (long)loopsBox.Value;
            Player.DontFadeSong = !fadeBox.Checked;
            Player.Record(filePath);
            Close();
        }
    }
}
