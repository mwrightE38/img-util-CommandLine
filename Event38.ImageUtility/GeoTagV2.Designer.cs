namespace Event38.ImageUtility
{
    partial class GeoTagV2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeoTagV2));
            this.StatusUpdate = new System.Windows.Forms.Label();
            this.LogFilePath = new System.Windows.Forms.Label();
            this.ImagePath = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnLogFile = new System.Windows.Forms.Button();
            this.btnImageFolder = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StatusUpdate
            // 
            this.StatusUpdate.AutoSize = true;
            this.StatusUpdate.Location = new System.Drawing.Point(23, 214);
            this.StatusUpdate.Name = "StatusUpdate";
            this.StatusUpdate.Size = new System.Drawing.Size(70, 13);
            this.StatusUpdate.TabIndex = 11;
            this.StatusUpdate.Text = "Status: 0 of 0";
            // 
            // LogFilePath
            // 
            this.LogFilePath.AutoSize = true;
            this.LogFilePath.Location = new System.Drawing.Point(160, 80);
            this.LogFilePath.Name = "LogFilePath";
            this.LogFilePath.Size = new System.Drawing.Size(69, 13);
            this.LogFilePath.TabIndex = 10;
            this.LogFilePath.Text = "Log File Path";
            this.LogFilePath.Click += new System.EventHandler(this.LogFilePath_Click_1);
            // 
            // ImagePath
            // 
            this.ImagePath.AutoSize = true;
            this.ImagePath.Location = new System.Drawing.Point(160, 29);
            this.ImagePath.Name = "ImagePath";
            this.ImagePath.Size = new System.Drawing.Size(61, 13);
            this.ImagePath.TabIndex = 9;
            this.ImagePath.Text = "Image Path";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(293, 131);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(114, 64);
            this.button3.TabIndex = 8;
            this.button3.Text = "Process";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.SystemColors.Highlight;
            this.progressBar1.Location = new System.Drawing.Point(26, 115);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(203, 46);
            this.progressBar1.TabIndex = 2;
            this.progressBar1.Click += new System.EventHandler(this.progressBar1_Click);
            // 
            // btnLogFile
            // 
            this.btnLogFile.Location = new System.Drawing.Point(26, 63);
            this.btnLogFile.Name = "btnLogFile";
            this.btnLogFile.Size = new System.Drawing.Size(112, 46);
            this.btnLogFile.TabIndex = 7;
            this.btnLogFile.Text = "Select";
            this.btnLogFile.UseVisualStyleBackColor = true;
            this.btnLogFile.Click += new System.EventHandler(this.btnLogFile_Click_1);
            // 
            // btnImageFolder
            // 
            this.btnImageFolder.Location = new System.Drawing.Point(26, 12);
            this.btnImageFolder.Name = "btnImageFolder";
            this.btnImageFolder.Size = new System.Drawing.Size(112, 46);
            this.btnImageFolder.TabIndex = 6;
            this.btnImageFolder.Text = "Select";
            this.btnImageFolder.UseVisualStyleBackColor = true;
            this.btnImageFolder.Click += new System.EventHandler(this.btnImageFolder_Click_1);
            // 
            // GeoTagV2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 239);
            this.Controls.Add(this.StatusUpdate);
            this.Controls.Add(this.LogFilePath);
            this.Controls.Add(this.ImagePath);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btnLogFile);
            this.Controls.Add(this.btnImageFolder);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GeoTagV2";
            this.Text = "GeoTagV2";
            this.Load += new System.EventHandler(this.GeoTagV2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label StatusUpdate;
        private System.Windows.Forms.Label LogFilePath;
        private System.Windows.Forms.Label ImagePath;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnLogFile;
        private System.Windows.Forms.Button btnImageFolder;
    }
}