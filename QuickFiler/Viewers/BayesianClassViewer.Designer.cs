namespace QuickFiler.Viewers
{
    partial class BayesianClassViewer
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
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ErrCount = new System.Windows.Forms.Label();
            this.FnCount = new System.Windows.Forms.Label();
            this.FpCount = new System.Windows.Forms.Label();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.F1Score = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.RecallScore = new System.Windows.Forms.Label();
            this.PrecisionScore = new System.Windows.Forms.Label();
            this.ClassSelector = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.objectListView1 = new BrightIdeasSoftware.ObjectListView();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton1.Location = new System.Drawing.Point(30, 53);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(215, 35);
            this.radioButton1.TabIndex = 1;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "False Positive";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.ErrCount);
            this.groupBox1.Controls.Add(this.FnCount);
            this.groupBox1.Controls.Add(this.FpCount);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(1873, 30);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(489, 223);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Class Errors";
            // 
            // ErrCount
            // 
            this.ErrCount.AutoSize = true;
            this.ErrCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrCount.Location = new System.Drawing.Point(325, 135);
            this.ErrCount.Name = "ErrCount";
            this.ErrCount.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.ErrCount.Size = new System.Drawing.Size(87, 35);
            this.ErrCount.TabIndex = 5;
            this.ErrCount.Text = "#,###";
            this.ErrCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FnCount
            // 
            this.FnCount.AutoSize = true;
            this.FnCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FnCount.Location = new System.Drawing.Point(325, 94);
            this.FnCount.Name = "FnCount";
            this.FnCount.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.FnCount.Size = new System.Drawing.Size(87, 35);
            this.FnCount.TabIndex = 3;
            this.FnCount.Text = "#,###";
            this.FnCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FpCount
            // 
            this.FpCount.AutoSize = true;
            this.FpCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FpCount.Location = new System.Drawing.Point(325, 53);
            this.FpCount.Name = "FpCount";
            this.FpCount.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.FpCount.Size = new System.Drawing.Size(87, 35);
            this.FpCount.TabIndex = 2;
            this.FpCount.Text = "#,###";
            this.FpCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton2.Location = new System.Drawing.Point(30, 94);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(227, 35);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "False Negative";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.F1Score);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.RecallScore);
            this.groupBox2.Controls.Add(this.PrecisionScore);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(2388, 30);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(377, 223);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Class Scores";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(24, 135);
            this.label9.Name = "label9";
            this.label9.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.label9.Size = new System.Drawing.Size(46, 35);
            this.label9.TabIndex = 5;
            this.label9.Text = "F1";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(24, 94);
            this.label8.Name = "label8";
            this.label8.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.label8.Size = new System.Drawing.Size(90, 35);
            this.label8.TabIndex = 3;
            this.label8.Text = "Recall";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // F1Score
            // 
            this.F1Score.AutoSize = true;
            this.F1Score.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.F1Score.Location = new System.Drawing.Point(250, 135);
            this.F1Score.Name = "F1Score";
            this.F1Score.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.F1Score.Size = new System.Drawing.Size(87, 35);
            this.F1Score.TabIndex = 5;
            this.F1Score.Text = "#,###";
            this.F1Score.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(24, 53);
            this.label7.Name = "label7";
            this.label7.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.label7.Size = new System.Drawing.Size(126, 35);
            this.label7.TabIndex = 2;
            this.label7.Text = "Precision";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // RecallScore
            // 
            this.RecallScore.AutoSize = true;
            this.RecallScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RecallScore.Location = new System.Drawing.Point(250, 94);
            this.RecallScore.Name = "RecallScore";
            this.RecallScore.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.RecallScore.Size = new System.Drawing.Size(87, 35);
            this.RecallScore.TabIndex = 3;
            this.RecallScore.Text = "#,###";
            this.RecallScore.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // PrecisionScore
            // 
            this.PrecisionScore.AutoSize = true;
            this.PrecisionScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PrecisionScore.Location = new System.Drawing.Point(250, 53);
            this.PrecisionScore.Name = "PrecisionScore";
            this.PrecisionScore.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.PrecisionScore.Size = new System.Drawing.Size(87, 35);
            this.PrecisionScore.TabIndex = 2;
            this.PrecisionScore.Text = "#,###";
            this.PrecisionScore.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ClassSelector
            // 
            this.ClassSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ClassSelector.BackColor = System.Drawing.SystemColors.Control;
            this.ClassSelector.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ClassSelector.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClassSelector.FormattingEnabled = true;
            this.ClassSelector.ItemHeight = 31;
            this.ClassSelector.Location = new System.Drawing.Point(17, 43);
            this.ClassSelector.Name = "ClassSelector";
            this.ClassSelector.Size = new System.Drawing.Size(1800, 155);
            this.ClassSelector.TabIndex = 5;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.ClassSelector);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(24, 30);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1823, 223);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Class Selector";
            // 
            // objectListView1
            // 
            this.objectListView1.CellEditUseWholeCell = false;
            this.objectListView1.HideSelection = false;
            this.objectListView1.Location = new System.Drawing.Point(24, 273);
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.Size = new System.Drawing.Size(2741, 856);
            this.objectListView1.TabIndex = 7;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            // 
            // BayesianClassViewer
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2790, 1283);
            this.Controls.Add(this.objectListView1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "BayesianClassViewer";
            this.Text = "BayesianClassViewer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label FpCount;
        public System.Windows.Forms.Label ErrCount;
        public System.Windows.Forms.Label FnCount;
        public System.Windows.Forms.Label PrecisionScore;
        public System.Windows.Forms.Label RecallScore;
        public System.Windows.Forms.Label F1Score;
        private System.Windows.Forms.ListBox ClassSelector;
        private System.Windows.Forms.GroupBox groupBox3;
        private BrightIdeasSoftware.ObjectListView objectListView1;
    }
}