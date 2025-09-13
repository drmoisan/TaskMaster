namespace QuickFiler.Viewers
{
    partial class BayesianPerformanceViewer
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
            this.TlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.F1Score = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.RecallScore = new System.Windows.Forms.Label();
            this.PrecisionScore = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CbFalseNegative = new System.Windows.Forms.CheckBox();
            this.CbFalsePositive = new System.Windows.Forms.CheckBox();
            this.TotalCount = new System.Windows.Forms.Label();
            this.FnCount = new System.Windows.Forms.Label();
            this.FpCount = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ClassSelector = new System.Windows.Forms.ListBox();
            this.OlvVerboseDetails = new BrightIdeasSoftware.ObjectListView();
            this.PredictedClass = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Type = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Probability = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.From = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvDrivers = new BrightIdeasSoftware.ObjectListView();
            this.OlvToken = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvTokenProbability = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvDriverPresence = new BrightIdeasSoftware.ObjectListView();
            this.OlvSubject2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvProbability2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.FlpActions = new System.Windows.Forms.FlowLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.TlpMain.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OlvVerboseDetails)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OlvDrivers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OlvDriverPresence)).BeginInit();
            this.FlpActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // TlpMain
            // 
            this.TlpMain.ColumnCount = 3;
            this.TlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 515F));
            this.TlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 390F));
            this.TlpMain.Controls.Add(this.groupBox2, 3, 0);
            this.TlpMain.Controls.Add(this.groupBox1, 1, 0);
            this.TlpMain.Controls.Add(this.groupBox3, 0, 0);
            this.TlpMain.Controls.Add(this.OlvVerboseDetails, 0, 1);
            this.TlpMain.Controls.Add(this.OlvDrivers, 1, 1);
            this.TlpMain.Controls.Add(this.OlvDriverPresence, 1, 2);
            this.TlpMain.Controls.Add(this.FlpActions, 0, 3);
            this.TlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TlpMain.Location = new System.Drawing.Point(0, 0);
            this.TlpMain.Name = "TlpMain";
            this.TlpMain.RowCount = 4;
            this.TlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            this.TlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.TlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.TlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.TlpMain.Size = new System.Drawing.Size(2740, 1283);
            this.TlpMain.TabIndex = 10;
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
            this.groupBox2.Location = new System.Drawing.Point(2367, 10);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(10);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(363, 210);
            this.groupBox2.TabIndex = 15;
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
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.CbFalseNegative);
            this.groupBox1.Controls.Add(this.CbFalsePositive);
            this.groupBox1.Controls.Add(this.TotalCount);
            this.groupBox1.Controls.Add(this.FnCount);
            this.groupBox1.Controls.Add(this.FpCount);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(1851, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(489, 210);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Class Counts";
            // 
            // CbFalseNegative
            // 
            this.CbFalseNegative.AutoSize = true;
            this.CbFalseNegative.Location = new System.Drawing.Point(30, 94);
            this.CbFalseNegative.Name = "CbFalseNegative";
            this.CbFalseNegative.Size = new System.Drawing.Size(228, 35);
            this.CbFalseNegative.TabIndex = 7;
            this.CbFalseNegative.Text = "False Negative";
            this.CbFalseNegative.UseVisualStyleBackColor = true;
            // 
            // CbFalsePositive
            // 
            this.CbFalsePositive.AutoSize = true;
            this.CbFalsePositive.Location = new System.Drawing.Point(30, 53);
            this.CbFalsePositive.Name = "CbFalsePositive";
            this.CbFalsePositive.Size = new System.Drawing.Size(216, 35);
            this.CbFalsePositive.TabIndex = 6;
            this.CbFalsePositive.Text = "False Positive";
            this.CbFalsePositive.UseVisualStyleBackColor = true;
            // 
            // TotalCount
            // 
            this.TotalCount.AutoSize = true;
            this.TotalCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TotalCount.Location = new System.Drawing.Point(325, 135);
            this.TotalCount.Name = "TotalCount";
            this.TotalCount.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.TotalCount.Size = new System.Drawing.Size(87, 35);
            this.TotalCount.TabIndex = 5;
            this.TotalCount.Text = "#,###";
            this.TotalCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.ClassSelector);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(10, 10);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(10);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1815, 210);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Class Selector";
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
            this.ClassSelector.Size = new System.Drawing.Size(1792, 124);
            this.ClassSelector.TabIndex = 5;
            this.ClassSelector.SelectedIndexChanged += new System.EventHandler(this.ClassSelector_SelectedIndexChanged);
            // 
            // OlvVerboseDetails
            // 
            this.OlvVerboseDetails.AllColumns.Add(this.PredictedClass);
            this.OlvVerboseDetails.AllColumns.Add(this.Type);
            this.OlvVerboseDetails.AllColumns.Add(this.Probability);
            this.OlvVerboseDetails.AllColumns.Add(this.From);
            this.OlvVerboseDetails.AllColumns.Add(this.olvColumn1);
            this.OlvVerboseDetails.CellEditUseWholeCell = false;
            this.OlvVerboseDetails.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.PredictedClass,
            this.Type,
            this.Probability,
            this.From,
            this.olvColumn1});
            this.OlvVerboseDetails.Cursor = System.Windows.Forms.Cursors.Default;
            this.OlvVerboseDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OlvVerboseDetails.FullRowSelect = true;
            this.OlvVerboseDetails.HideSelection = false;
            this.OlvVerboseDetails.Location = new System.Drawing.Point(10, 240);
            this.OlvVerboseDetails.Margin = new System.Windows.Forms.Padding(10);
            this.OlvVerboseDetails.Name = "OlvVerboseDetails";
            this.TlpMain.SetRowSpan(this.OlvVerboseDetails, 2);
            this.OlvVerboseDetails.Size = new System.Drawing.Size(1815, 912);
            this.OlvVerboseDetails.TabIndex = 12;
            this.OlvVerboseDetails.UseCompatibleStateImageBehavior = false;
            this.OlvVerboseDetails.View = System.Windows.Forms.View.Details;
            this.OlvVerboseDetails.SelectionChanged += new System.EventHandler(this.OlvVerboseDetails_SelectionChanged);
            // 
            // PredictedClass
            // 
            this.PredictedClass.AspectName = "Key.Source.FolderInfo.Name";
            this.PredictedClass.Text = "Predicted Class";
            this.PredictedClass.Width = 342;
            // 
            // Type
            // 
            this.Type.AspectName = "Value";
            this.Type.IsEditable = false;
            this.Type.Text = "Type";
            this.Type.Width = 329;
            // 
            // Probability
            // 
            this.Probability.AspectName = "Key.Probability";
            this.Probability.AspectToStringFormat = "{0:P2}";
            this.Probability.Text = "Probability";
            this.Probability.Width = 212;
            // 
            // From
            // 
            this.From.AspectName = "Key.Source.Sender.Name";
            this.From.Text = "From";
            this.From.Width = 314;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "Key.Source.Subject";
            this.olvColumn1.Text = "Subject";
            this.olvColumn1.Width = 545;
            // 
            // OlvDrivers
            // 
            this.OlvDrivers.AllColumns.Add(this.OlvToken);
            this.OlvDrivers.AllColumns.Add(this.OlvTokenProbability);
            this.OlvDrivers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OlvDrivers.CellEditUseWholeCell = false;
            this.OlvDrivers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OlvToken,
            this.OlvTokenProbability});
            this.TlpMain.SetColumnSpan(this.OlvDrivers, 2);
            this.OlvDrivers.Cursor = System.Windows.Forms.Cursors.Default;
            this.OlvDrivers.HasCollapsibleGroups = false;
            this.OlvDrivers.HideSelection = false;
            this.OlvDrivers.Location = new System.Drawing.Point(1845, 240);
            this.OlvDrivers.Margin = new System.Windows.Forms.Padding(10);
            this.OlvDrivers.Name = "OlvDrivers";
            this.OlvDrivers.Size = new System.Drawing.Size(885, 539);
            this.OlvDrivers.TabIndex = 11;
            this.OlvDrivers.UseCompatibleStateImageBehavior = false;
            this.OlvDrivers.View = System.Windows.Forms.View.Details;
            this.OlvDrivers.SelectionChanged += new System.EventHandler(this.OlvDrivers_SelectionChanged);
            // 
            // OlvToken
            // 
            this.OlvToken.AspectName = "Item1";
            this.OlvToken.Groupable = false;
            this.OlvToken.Text = "Driver";
            this.OlvToken.Width = 498;
            // 
            // OlvTokenProbability
            // 
            this.OlvTokenProbability.AspectName = "Item2";
            this.OlvTokenProbability.AspectToStringFormat = "{0:P2}";
            this.OlvTokenProbability.Groupable = false;
            this.OlvTokenProbability.Text = "Probability";
            this.OlvTokenProbability.Width = 229;
            // 
            // OlvDriverPresence
            // 
            this.OlvDriverPresence.AllColumns.Add(this.OlvSubject2);
            this.OlvDriverPresence.AllColumns.Add(this.OlvProbability2);
            this.OlvDriverPresence.CellEditUseWholeCell = false;
            this.OlvDriverPresence.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OlvSubject2,
            this.OlvProbability2});
            this.TlpMain.SetColumnSpan(this.OlvDriverPresence, 2);
            this.OlvDriverPresence.Cursor = System.Windows.Forms.Cursors.Default;
            this.OlvDriverPresence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OlvDriverPresence.HideSelection = false;
            this.OlvDriverPresence.Location = new System.Drawing.Point(1845, 799);
            this.OlvDriverPresence.Margin = new System.Windows.Forms.Padding(10);
            this.OlvDriverPresence.Name = "OlvDriverPresence";
            this.OlvDriverPresence.Size = new System.Drawing.Size(885, 353);
            this.OlvDriverPresence.TabIndex = 10;
            this.OlvDriverPresence.UseCompatibleStateImageBehavior = false;
            this.OlvDriverPresence.View = System.Windows.Forms.View.Details;
            // 
            // OlvSubject2
            // 
            this.OlvSubject2.AspectName = "Item1";
            this.OlvSubject2.Text = "Subject";
            this.OlvSubject2.Width = 630;
            // 
            // OlvProbability2
            // 
            this.OlvProbability2.AspectName = "Item2";
            this.OlvProbability2.AspectToStringFormat = "{0:P2}";
            this.OlvProbability2.Text = "Probability";
            this.OlvProbability2.Width = 179;
            // 
            // FlpActions
            // 
            this.FlpActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.FlpActions.AutoSize = true;
            this.TlpMain.SetColumnSpan(this.FlpActions, 3);
            this.FlpActions.Controls.Add(this.button1);
            this.FlpActions.Location = new System.Drawing.Point(1212, 1165);
            this.FlpActions.Name = "FlpActions";
            this.FlpActions.Size = new System.Drawing.Size(316, 115);
            this.FlpActions.TabIndex = 16;
            // 
            // button1
            // 
            this.button1.Image = global::QuickFiler.Properties.Resources.SolutionFolderSwitch1;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(10, 10);
            this.button1.Margin = new System.Windows.Forms.Padding(10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(296, 104);
            this.button1.TabIndex = 15;
            this.button1.Text = "ReSort Item";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // BayesianPerformanceViewer
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2740, 1283);
            this.Controls.Add(this.TlpMain);
            this.Name = "BayesianPerformanceViewer";
            this.Text = "BayesianClassViewer";
            this.TlpMain.ResumeLayout(false);
            this.TlpMain.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OlvVerboseDetails)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OlvDrivers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OlvDriverPresence)).EndInit();
            this.FlpActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal BrightIdeasSoftware.ObjectListView OlvDrivers;
        internal BrightIdeasSoftware.OLVColumn OlvToken;
        internal BrightIdeasSoftware.OLVColumn OlvTokenProbability;
        internal BrightIdeasSoftware.ObjectListView OlvDriverPresence;
        private BrightIdeasSoftware.OLVColumn OlvSubject2;
        private BrightIdeasSoftware.OLVColumn OlvProbability2;
        private System.Windows.Forms.GroupBox groupBox1;
        protected internal System.Windows.Forms.CheckBox CbFalseNegative;
        protected internal System.Windows.Forms.CheckBox CbFalsePositive;
        public System.Windows.Forms.Label TotalCount;
        public System.Windows.Forms.Label FnCount;
        public System.Windows.Forms.Label FpCount;
        private System.Windows.Forms.GroupBox groupBox3;
        protected internal System.Windows.Forms.ListBox ClassSelector;
        protected internal BrightIdeasSoftware.ObjectListView OlvVerboseDetails;
        internal BrightIdeasSoftware.OLVColumn PredictedClass;
        internal BrightIdeasSoftware.OLVColumn Type;
        internal BrightIdeasSoftware.OLVColumn Probability;
        internal BrightIdeasSoftware.OLVColumn From;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.Label F1Score;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label RecallScore;
        public System.Windows.Forms.Label PrecisionScore;
        internal System.Windows.Forms.TableLayoutPanel TlpMain;
        private System.Windows.Forms.FlowLayoutPanel FlpActions;
        private System.Windows.Forms.Button button1;
    }
}