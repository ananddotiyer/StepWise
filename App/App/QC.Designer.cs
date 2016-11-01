namespace App
{
    partial class QC
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.InitiateQC = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(32, 37);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1219, 673);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.Url = new System.Uri("http://plaengqc1.corp.nai.org/qcbin/start_a.htm", System.UriKind.Absolute);
            // 
            // InitiateQC
            // 
            this.InitiateQC.Location = new System.Drawing.Point(1176, 8);
            this.InitiateQC.Name = "InitiateQC";
            this.InitiateQC.Size = new System.Drawing.Size(75, 23);
            this.InitiateQC.TabIndex = 1;
            this.InitiateQC.Text = "Connect";
            this.InitiateQC.UseVisualStyleBackColor = true;
            this.InitiateQC.Click += new System.EventHandler(this.InitiateQC_Click);
            // 
            // QC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1263, 740);
            this.Controls.Add(this.InitiateQC);
            this.Controls.Add(this.webBrowser1);
            this.Name = "QC";
            this.Text = "QC";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QC_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button InitiateQC;
    }
}