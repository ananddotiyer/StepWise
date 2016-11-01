namespace key_preview {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Screen = new System.Windows.Forms.CheckBox();
            this.Keyboarded = new System.Windows.Forms.CheckBox();
            this.Moused = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.richTextBox1.Enabled = false;
            this.richTextBox1.Location = new System.Drawing.Point(12, 43);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(285, 479);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.clearToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(103, 48);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // Screen
            // 
            this.Screen.AutoSize = true;
            this.Screen.Location = new System.Drawing.Point(180, 10);
            this.Screen.Name = "Screen";
            this.Screen.Size = new System.Drawing.Size(77, 17);
            this.Screen.TabIndex = 5;
            this.Screen.Text = "On Screen";
            this.Screen.UseVisualStyleBackColor = true;
            // 
            // Keyboarded
            // 
            this.Keyboarded.AutoSize = true;
            this.Keyboarded.Location = new System.Drawing.Point(12, 10);
            this.Keyboarded.Name = "Keyboarded";
            this.Keyboarded.Size = new System.Drawing.Size(71, 17);
            this.Keyboarded.TabIndex = 6;
            this.Keyboarded.Text = "&Keyboard";
            this.Keyboarded.UseVisualStyleBackColor = true;
            this.Keyboarded.CheckedChanged += new System.EventHandler(this.Keyboarded_CheckedChanged);
            // 
            // Moused
            // 
            this.Moused.AutoSize = true;
            this.Moused.Location = new System.Drawing.Point(116, 10);
            this.Moused.Name = "Moused";
            this.Moused.Size = new System.Drawing.Size(58, 17);
            this.Moused.TabIndex = 7;
            this.Moused.Text = "&Mouse";
            this.Moused.UseVisualStyleBackColor = true;
            this.Moused.CheckedChanged += new System.EventHandler(this.Moused_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 32);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.Moused);
            this.Controls.Add(this.Keyboarded);
            this.Controls.Add(this.Screen);
            this.Controls.Add(this.richTextBox1);
            this.KeyPreview = true;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Not recording";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.CheckBox Screen;
        private System.Windows.Forms.CheckBox Keyboarded;
        private System.Windows.Forms.CheckBox Moused;

    }
}

