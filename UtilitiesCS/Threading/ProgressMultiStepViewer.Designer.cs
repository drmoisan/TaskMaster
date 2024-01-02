namespace UtilitiesCS.Threading
{
    partial class ProgressMultiStepViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressMultiStepViewer));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Bar = new System.Windows.Forms.ProgressBar();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.headerDuration = new System.Windows.Forms.TextBox();
            this.JobName01 = new System.Windows.Forms.TextBox();
            this.headerProgress = new System.Windows.Forms.Label();
            this.pictureBoxSVG1 = new SVGControl.PictureBoxSVG();
            this.headerJobName = new System.Windows.Forms.Label();
            this.headerNumber = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSVG1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 550F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.Controls.Add(this.Bar, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.ButtonCancel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.headerDuration, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.JobName01, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.headerProgress, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBoxSVG1, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.headerJobName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.headerNumber, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1495, 239);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // Bar
            // 
            this.Bar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Bar.Location = new System.Drawing.Point(648, 83);
            this.Bar.Name = "Bar";
            this.Bar.Size = new System.Drawing.Size(544, 71);
            this.Bar.Step = 1;
            this.Bar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.Bar.TabIndex = 1;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel1.SetColumnSpan(this.ButtonCancel, 5);
            this.ButtonCancel.Location = new System.Drawing.Point(657, 163);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(180, 71);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(1339, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 37);
            this.label1.TabIndex = 4;
            this.label1.Text = "mm:ss";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // headerDuration
            // 
            this.headerDuration.BackColor = System.Drawing.SystemColors.Control;
            this.headerDuration.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.headerDuration.Cursor = System.Windows.Forms.Cursors.Default;
            this.headerDuration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerDuration.Location = new System.Drawing.Point(1298, 3);
            this.headerDuration.Multiline = true;
            this.headerDuration.Name = "headerDuration";
            this.headerDuration.ReadOnly = true;
            this.headerDuration.Size = new System.Drawing.Size(194, 74);
            this.headerDuration.TabIndex = 5;
            this.headerDuration.TabStop = false;
            this.headerDuration.Text = "Duration\r\n(mm:ss)";
            this.headerDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // JobName01
            // 
            this.JobName01.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.JobName01.BackColor = System.Drawing.SystemColors.Control;
            this.JobName01.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.JobName01.Location = new System.Drawing.Point(103, 98);
            this.JobName01.Margin = new System.Windows.Forms.Padding(3, 18, 3, 18);
            this.JobName01.Name = "JobName01";
            this.JobName01.Size = new System.Drawing.Size(539, 44);
            this.JobName01.TabIndex = 6;
            this.JobName01.WordWrap = false;
            // 
            // headerProgress
            // 
            this.headerProgress.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.headerProgress.AutoSize = true;
            this.headerProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerProgress.Location = new System.Drawing.Point(847, 21);
            this.headerProgress.Name = "headerProgress";
            this.headerProgress.Size = new System.Drawing.Size(145, 37);
            this.headerProgress.TabIndex = 4;
            this.headerProgress.Text = "Progress";
            this.headerProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBoxSVG1
            // 
            this.pictureBoxSVG1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBoxSVG1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxSVG1.Image")));
            this.pictureBoxSVG1.Location = new System.Drawing.Point(1209, 83);
            this.pictureBoxSVG1.Name = "pictureBoxSVG1";
            this.pictureBoxSVG1.Size = new System.Drawing.Size(71, 71);
            this.pictureBoxSVG1.TabIndex = 7;
            this.pictureBoxSVG1.TabStop = false;
            // 
            // headerJobName
            // 
            this.headerJobName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.headerJobName.AutoSize = true;
            this.headerJobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerJobName.Location = new System.Drawing.Point(290, 21);
            this.headerJobName.Name = "headerJobName";
            this.headerJobName.Size = new System.Drawing.Size(164, 37);
            this.headerJobName.TabIndex = 4;
            this.headerJobName.Text = "Job Name";
            this.headerJobName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // headerNumber
            // 
            this.headerNumber.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.headerNumber.AutoSize = true;
            this.headerNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerNumber.Location = new System.Drawing.Point(32, 21);
            this.headerNumber.Name = "headerNumber";
            this.headerNumber.Size = new System.Drawing.Size(35, 37);
            this.headerNumber.TabIndex = 4;
            this.headerNumber.Text = "#";
            this.headerNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(32, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 37);
            this.label5.TabIndex = 4;
            this.label5.Text = "#";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProgressMultiStepViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1495, 239);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(1521, 310);
            this.Name = "ProgressMultiStepViewer";
            this.Text = "Progress Viewer";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSVG1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.ProgressBar Bar;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox headerDuration;
        private System.Windows.Forms.TextBox JobName01;
        private System.Windows.Forms.Label headerProgress;
        private SVGControl.PictureBoxSVG pictureBoxSVG1;
        private System.Windows.Forms.Label headerJobName;
        private System.Windows.Forms.Label headerNumber;
        private System.Windows.Forms.Label label5;
    }
}