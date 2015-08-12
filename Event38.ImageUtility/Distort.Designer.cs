namespace Event38.ImageUtility
{
    partial class Distort
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
            this.btnImageOne = new System.Windows.Forms.Button();
            this.btnImageTwo = new System.Windows.Forms.Button();
            this.btnProcess = new System.Windows.Forms.Button();
            this.ImagePathOne = new System.Windows.Forms.Label();
            this.ImagePathTwo = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnImageOne
            // 
            this.btnImageOne.Location = new System.Drawing.Point(35, 54);
            this.btnImageOne.Name = "btnImageOne";
            this.btnImageOne.Size = new System.Drawing.Size(75, 23);
            this.btnImageOne.TabIndex = 0;
            this.btnImageOne.Text = "Select";
            this.btnImageOne.UseVisualStyleBackColor = true;
            this.btnImageOne.Click += new System.EventHandler(this.btnImageOne_Click);
            // 
            // btnImageTwo
            // 
            this.btnImageTwo.Location = new System.Drawing.Point(35, 115);
            this.btnImageTwo.Name = "btnImageTwo";
            this.btnImageTwo.Size = new System.Drawing.Size(75, 23);
            this.btnImageTwo.TabIndex = 1;
            this.btnImageTwo.Text = "Select";
            this.btnImageTwo.UseVisualStyleBackColor = true;
            this.btnImageTwo.Click += new System.EventHandler(this.btnImageTwo_Click);
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(339, 181);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(121, 56);
            this.btnProcess.TabIndex = 2;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // ImagePathOne
            // 
            this.ImagePathOne.AutoSize = true;
            this.ImagePathOne.Location = new System.Drawing.Point(150, 54);
            this.ImagePathOne.Name = "ImagePathOne";
            this.ImagePathOne.Size = new System.Drawing.Size(61, 13);
            this.ImagePathOne.TabIndex = 3;
            this.ImagePathOne.Text = "Image Path";
            // 
            // ImagePathTwo
            // 
            this.ImagePathTwo.AutoSize = true;
            this.ImagePathTwo.Location = new System.Drawing.Point(150, 115);
            this.ImagePathTwo.Name = "ImagePathTwo";
            this.ImagePathTwo.Size = new System.Drawing.Size(61, 13);
            this.ImagePathTwo.TabIndex = 4;
            this.ImagePathTwo.Text = "Image Path";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 203);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "label1";
            // 
            // Distort
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 262);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ImagePathTwo);
            this.Controls.Add(this.ImagePathOne);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.btnImageTwo);
            this.Controls.Add(this.btnImageOne);
            this.Name = "Distort";
            this.Text = "Distort";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnImageOne;
        private System.Windows.Forms.Button btnImageTwo;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Label ImagePathOne;
        private System.Windows.Forms.Label ImagePathTwo;
        private System.Windows.Forms.Label label1;
    }
}