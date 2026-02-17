using System;
using System.Windows.Forms;

namespace NitroStudio2
{
    public partial class SaveQuitDialog : Form
    {
        private readonly EditorBase parentTwo;

        public SaveQuitDialog(EditorBase parent2)
        {
            InitializeComponent();
            parentTwo = parent2;
        }

        private void SaveQuitDialog_Load(object sender, EventArgs e) { }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            try
            {
                parentTwo.Close();
            }
            catch { }
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            try
            {
                parentTwo.saveToolStripMenuItem_Click(sender, e);
            }
            catch { }
            try
            {
                parentTwo.Close();
            }
            catch { }
        }
    }
}
