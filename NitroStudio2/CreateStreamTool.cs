using GotaSoundIO.Sound;
using GotaSoundIO.Sound.Encoding;
using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using System;
using System.IO;
using System.Windows.Forms;

namespace NitroStudio2
{
    public partial class CreateStreamTool : Form
    {
        private readonly bool SwavMode;

        public CreateStreamTool(bool swavMode)
        {
            InitializeComponent();
            outputFormat.SelectedIndex = 2;
            SwavMode = swavMode;
            if (SwavMode)
            {
                Text = "Create Wave";
                Icon = Properties.Resources.Wav;
            }
        }

        private void impFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new()
            {
                RestoreDirectory = true,
                Filter = "Supported Sound Files|*.wav;*.swav;*.strm"
            };
            _ = o.ShowDialog();
            if (o.FileName != "")
            {
                impFileBox.Text = o.FileName;
                impFileBox.SelectionStart = outFileBox.Text.Length;
                impFileBox.ScrollToCaret();
                impFileBox.Refresh();
            }
        }

        private void outFileButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new()
            {
                RestoreDirectory = true,
                Filter = SwavMode ? "Sound Wave|*.swav" : "Sound Stream|*.strm"
            };
            _ = s.ShowDialog();
            if (s.FileName != "")
            {
                outFileBox.Text = s.FileName;
                outFileBox.SelectionStart = outFileBox.Text.Length;
                outFileBox.ScrollToCaret();
                outFileBox.Refresh();
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (impFileBox.Text.Equals(""))
            {
                _ = MessageBox.Show("No Input File Selected!");
                return;
            }
            if (outFileBox.Text.Equals(""))
            {
                _ = MessageBox.Show("No Output File Selected!");
                return;
            }
            SoundFile s = SwavMode ? new Wave() : new NitroFileLoader.Stream();
            SoundFile i = Path.GetExtension(impFileBox.Text) switch
            {
                ".swav" => new Wave(),
                ".strm" => new NitroFileLoader.Stream(),
                _ => new RiffWave(),
            };
            i.Read(impFileBox.Text);
            Type convType = outputFormat.SelectedIndex switch
            {
                0 => typeof(PCM8Signed),
                1 => typeof(PCM16),
                _ => typeof(ImaAdpcm),
            };
            s.FromOtherStreamFile(i, convType);
            s.Write(outFileBox.Text);
        }
    }
}
