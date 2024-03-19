namespace UtilitiesCS
{
    partial class ProgressViewer
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
            this.Bar = new System.Windows.Forms.ProgressBar();
            this.JobName = new System.Windows.Forms.Label();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Bar
            // 
            this.Bar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Bar.Location = new System.Drawing.Point(45, 110);
            this.Bar.Name = "Bar";
            this.Bar.Size = new System.Drawing.Size(1305, 71);
            this.Bar.Step = 1;
            this.Bar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.Bar.TabIndex = 0;
            // 
            // JobName
            // 
            this.JobName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.JobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.JobName.Location = new System.Drawing.Point(45, 34);
            this.JobName.Name = "JobName";
            this.JobName.Size = new System.Drawing.Size(1521, 37);
            this.JobName.TabIndex = 1;
            this.JobName.Text = "Job Executing";
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ButtonCancel.Location = new System.Drawing.Point(1386, 110);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(180, 71);
            this.ButtonCancel.TabIndex = 2;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ProgressViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1603, 228);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.JobName);
            this.Controls.Add(this.Bar);
            this.MinimumSize = new System.Drawing.Size(1000, 299);
            this.Name = "ProgressViewer";
            this.Text = "Progress Viewer";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ProgressBar Bar;
        private System.Windows.Forms.Button ButtonCancel;
        public System.Windows.Forms.Label JobName;
    }
}