using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NitroStudio2 {
    public partial class StreamPlayer : Form {
        public string Path;
        public MainWindow MainWindow;
        private IWavePlayer wavePlayer;
        private AudioFileReader audioFileReader;

        public StreamPlayer(MainWindow m, string path, string name) {
            InitializeComponent();
            Text = "Stream Player - " + name + ".strm";
            Path = path;
            MainWindow = m;

            try {
                wavePlayer = new WaveOutEvent();
                audioFileReader = new AudioFileReader(path);
                wavePlayer.Init(audioFileReader);
                wavePlayer.Play();
            } catch (Exception ex) {
                MessageBox.Show("Error initializing audio playback: " + ex.Message);
            }
        }

        private void onClose(object sender, EventArgs e) {
            Thread t = new Thread(delete);
            t.Start();
        }

        private void delete() {
            try {
                if (wavePlayer != null) {
                    wavePlayer.Stop();
                    wavePlayer.Dispose();
                    wavePlayer = null;
                }
                if (audioFileReader != null) {
                    audioFileReader.Dispose();
                    audioFileReader = null;
                }
            } catch { }

            try { File.Delete(Path); } catch { }
            try { MainWindow.StreamTempCount--; } catch { }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (wavePlayer != null) {
                    wavePlayer.Stop();
                    wavePlayer.Dispose();
                    wavePlayer = null;
                }
                if (audioFileReader != null) {
                    audioFileReader.Dispose();
                    audioFileReader = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
