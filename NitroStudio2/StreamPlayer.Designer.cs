using System.Drawing;

namespace NitroStudio2 {
    partial class StreamPlayer {
#pragma warning disable CS0414
        private System.ComponentModel.IContainer components = null;
#pragma warning restore CS0414
        #region Windows Form Designer generated code
        private void InitializeComponent() {
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.statusLabel.AutoSize = true;
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusLabel.Location = new System.Drawing.Point(0, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(146, 13);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Now playing with NAudio...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(632, 317);
            this.Controls.Add(this.statusLabel);
            this.Name = "StreamPlayer";
            this.Text = "s";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.onClose);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
        internal System.Windows.Forms.Label statusLabel;
    }
}