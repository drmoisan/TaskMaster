namespace UtilitiesCS.EmailIntelligence.SubjectMap
{
    partial class SubjectMapMetrics
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
            this.DlvMetrics = new BrightIdeasSoftware.DataListView();
            this.FolderName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Path = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Subjects = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Emails = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.DlvMetrics)).BeginInit();
            this.SuspendLayout();
            // 
            // DlvMetrics
            // 
            this.DlvMetrics.AllColumns.Add(this.FolderName);
            this.DlvMetrics.AllColumns.Add(this.Path);
            this.DlvMetrics.AllColumns.Add(this.Subjects);
            this.DlvMetrics.AllColumns.Add(this.Emails);
            this.DlvMetrics.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DlvMetrics.CellEditUseWholeCell = false;
            this.DlvMetrics.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FolderName,
            this.Path,
            this.Subjects,
            this.Emails});
            this.DlvMetrics.Cursor = System.Windows.Forms.Cursors.Default;
            this.DlvMetrics.DataSource = null;
            this.DlvMetrics.HasCollapsibleGroups = false;
            this.DlvMetrics.HideSelection = false;
            this.DlvMetrics.Location = new System.Drawing.Point(13, 108);
            this.DlvMetrics.Name = "DlvMetrics";
            this.DlvMetrics.ShowGroups = false;
            this.DlvMetrics.Size = new System.Drawing.Size(1287, 620);
            this.DlvMetrics.TabIndex = 0;
            this.DlvMetrics.UseCompatibleStateImageBehavior = false;
            this.DlvMetrics.View = System.Windows.Forms.View.Details;
            // 
            // FolderName
            // 
            this.FolderName.AspectName = "FolderName";
            this.FolderName.Text = "Name";
            this.FolderName.Width = 188;
            // 
            // Path
            // 
            this.Path.AspectName = "FolderPath";
            this.Path.Text = "Path";
            this.Path.Width = 200;
            // 
            // Subjects
            // 
            this.Subjects.AspectName = "SubjectCount";
            this.Subjects.Text = "Subject Count";
            this.Subjects.Width = 242;
            // 
            // Emails
            // 
            this.Emails.AspectName = "EmailCount";
            this.Emails.Text = "Email Count";
            this.Emails.Width = 294;
            // 
            // SubjectMapMetrics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1312, 740);
            this.Controls.Add(this.DlvMetrics);
            this.Name = "SubjectMapMetrics";
            this.Text = "SubjectMapMetrics";
            ((System.ComponentModel.ISupportInitialize)(this.DlvMetrics)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private BrightIdeasSoftware.OLVColumn FolderName;
        private BrightIdeasSoftware.OLVColumn Path;
        private BrightIdeasSoftware.OLVColumn Subjects;
        private BrightIdeasSoftware.OLVColumn Emails;
        private BrightIdeasSoftware.DataListView DlvMetrics;
    }
}