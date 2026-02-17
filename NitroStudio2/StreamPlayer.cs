using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace NitroStudio2
{
    public partial class StreamPlayer : Form
    {
        public string Path;
        public MainWindow MainWindow;
        private IWavePlayer wavePlayer;
        private AudioFileReader audioFileReader;

        public StreamPlayer(MainWindow m, string path, string name)
        {
            InitializeComponent();
            Text = "Stream Player - " + name + ".strm";
            Path = path;
            MainWindow = m;
            try
            {
                wavePlayer = new WaveOutEvent();
                audioFileReader = new AudioFileReader(path);
                wavePlayer.Init(audioFileReader);
                wavePlayer.Play();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show("Error initializing audio playback: " + ex.Message);
            }
        }

        private void onClose(object sender, EventArgs e)
        {
            Thread t = new(delete);
            t.Start();
        }

        private void delete()
        {
            try
            {
                if (wavePlayer != null)
                {
                    wavePlayer.Stop();
                    wavePlayer.Dispose();
                    wavePlayer = null;
                }

                audioFileReader?.Dispose();
                audioFileReader = null;
            }
            catch { }
            try
            {
                File.Delete(Path);
            }
            catch { }
            try
            {
                MainWindow.StreamTempCount--;
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (wavePlayer != null)
                {
                    wavePlayer.Stop();
                    wavePlayer.Dispose();
                    wavePlayer = null;
                }

                audioFileReader?.Dispose();
                audioFileReader = null;
            }
            base.Dispose(disposing);
        }
    }
}
