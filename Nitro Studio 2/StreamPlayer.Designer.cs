namespace NitroStudio2 {
    partial class StreamPlayer {
        /// <summary>
        /// Required designer variable.
        /// </summary>
#pragma warning disable CS0414
        private System.ComponentModel.IContainer components = null;
#pragma warning restore CS0414

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusLabel.Location = new System.Drawing.Point(0, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(146, 13);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Now playing with NAudio...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StreamPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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