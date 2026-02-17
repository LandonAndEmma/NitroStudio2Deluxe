using System;
using System.Windows.Forms;

namespace NitroStudio2
{
    public partial class SaveCloseDialog : Form
    {
        public SaveCloseDialog()
        {
            InitializeComponent();
        }

        private int returnValue = 0;

        private void SaveCloseDialog_Load(object sender, EventArgs e) { }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) { }

        private void YesButton_Click(object sender, EventArgs e)
        {
            returnValue = 0;
            Close();
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            returnValue = 1;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            returnValue = 2;
            Close();
        }

        public int getValue()
        {
            _ = ShowDialog();
            return returnValue;
        }
    }
}
