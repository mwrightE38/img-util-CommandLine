namespace Event38.ImageUtility
{
    partial class ImageMosaic
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
            this.LogFilePath = new System.Windows.Forms.Label();
            this.ImagePath = new System.Windows.Forms.Label();
            this.btnLogFile = new System.Windows.Forms.Button();
            this.btnImageFolder = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // LogFilePath
            // 
            this.LogFilePath.AutoSize = true;
            this.LogFilePath.Location = new System.Drawing.Point(130, 76);
            this.LogFilePath.Name = "LogFilePath";
            this.LogFilePath.Size = new System.Drawing.Size(69, 13);
            this.LogFilePath.TabIndex = 8;
            this.LogFilePath.Text = "Log File Path";
            // 
            // ImagePath
            // 
            this.ImagePath.AutoSize = true;
            this.ImagePath.Location = new System.Drawing.Point(130, 22);
            this.ImagePath.Name = "ImagePath";
            this.ImagePath.Size = new System.Drawing.Size(61, 13);
            this.ImagePath.TabIndex = 7;
            this.ImagePath.Text = "Image Path";
            // 
            // btnLogFile
            // 
            this.btnLogFile.Location = new System.Drawing.Point(27, 76);
            this.btnLogFile.Name = "btnLogFile";
            this.btnLogFile.Size = new System.Drawing.Size(75, 23);
            this.btnLogFile.TabIndex = 6;
            this.btnLogFile.Text = "Select";
            this.btnLogFile.UseVisualStyleBackColor = true;
            this.btnLogFile.Click += new System.EventHandler(this.btnLogFile_Click);
            // 
            // btnImageFolder
            // 
            this.btnImageFolder.Location = new System.Drawing.Point(27, 22);
            this.btnImageFolder.Name = "btnImageFolder";
            this.btnImageFolder.Size = new System.Drawing.Size(75, 23);
            this.btnImageFolder.TabIndex = 5;
            this.btnImageFolder.Text = "Select";
            this.btnImageFolder.UseVisualStyleBackColor = true;
            this.btnImageFolder.Click += new System.EventHandler(this.btnImageFolder_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Event38.ImageUtility.Properties.Resources.No_image_available;
            this.pictureBox1.Location = new System.Drawing.Point(378, 22);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(402, 468);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 188);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Average Hight";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(115, 185);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(99, 20);
            this.textBox1.TabIndex = 11;
            // 
            // ImageMosaic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(805, 502);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.LogFilePath);
            this.Controls.Add(this.ImagePath);
            this.Controls.Add(this.btnLogFile);
            this.Controls.Add(this.btnImageFolder);
            this.Name = "ImageMosaic";
            this.Text = "ImageMosaic";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LogFilePath;
        private System.Windows.Forms.Label ImagePath;
        private System.Windows.Forms.Button btnLogFile;
        private System.Windows.Forms.Button btnImageFolder;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
    }
}