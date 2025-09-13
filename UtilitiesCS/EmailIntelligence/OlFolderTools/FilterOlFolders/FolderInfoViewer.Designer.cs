namespace UtilitiesCS.EmailIntelligence.OlFolderTools.FilterOlFolders
{
    partial class FolderInfoViewer
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
            this.components = new System.ComponentModel.Container();
            this.Tlp = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.BtnClose = new System.Windows.Forms.Button();
            this.Tlv = new BrightIdeasSoftware.TreeListView();
            this.FolderName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ItemCount = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ItemCountSubFolders = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.FolderSize = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Tlp.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Tlv)).BeginInit();
            this.SuspendLayout();
            // 
            // Tlp
            // 
            this.Tlp.ColumnCount = 1;
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.Tlp.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.Tlp.Controls.Add(this.Tlv, 0, 0);
            this.Tlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tlp.Location = new System.Drawing.Point(0, 0);
            this.Tlp.Name = "Tlp";
            this.Tlp.RowCount = 2;
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.Tlp.Size = new System.Drawing.Size(673, 450);
            this.Tlp.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.BtnClose);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(261, 402);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(150, 46);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // BtnClose
            // 
            this.BtnClose.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BtnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnClose.Location = new System.Drawing.Point(2, 2);
            this.BtnClose.Margin = new System.Windows.Forms.Padding(2);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(146, 44);
            this.BtnClose.TabIndex = 0;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            // 
            // Tlv
            // 
            this.Tlv.AllColumns.Add(this.FolderName);
            this.Tlv.AllColumns.Add(this.FolderSize);
            this.Tlv.AllColumns.Add(this.ItemCount);
            this.Tlv.AllColumns.Add(this.ItemCountSubFolders);
            this.Tlv.CellEditUseWholeCell = false;
            this.Tlv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FolderName,
            this.FolderSize,
            this.ItemCount,
            this.ItemCountSubFolders});
            this.Tlv.Cursor = System.Windows.Forms.Cursors.Default;
            this.Tlv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tlv.HideSelection = false;
            this.Tlv.Location = new System.Drawing.Point(3, 3);
            this.Tlv.Name = "Tlv";
            this.Tlv.ShowGroups = false;
            this.Tlv.Size = new System.Drawing.Size(667, 394);
            this.Tlv.TabIndex = 2;
            this.Tlv.UseCompatibleStateImageBehavior = false;
            this.Tlv.View = System.Windows.Forms.View.Details;
            this.Tlv.VirtualMode = true;
            // 
            // FolderName
            // 
            this.FolderName.AspectName = "Value.Name";
            this.FolderName.Text = "Folder Name";
            this.FolderName.Width = 311;
            // 
            // ItemCount
            // 
            this.ItemCount.AspectName = "Value.ItemCount";
            this.ItemCount.Text = "Count";
            this.ItemCount.Width = 107;
            // 
            // ItemCountSubFolders
            // 
            this.ItemCountSubFolders.AspectName = "Value.ItemCountSubFolders";
            this.ItemCountSubFolders.Text = "Total Count";
            this.ItemCountSubFolders.Width = 88;
            // 
            // FolderSize
            // 
            this.FolderSize.AspectName = "Value.FolderSize";
            this.FolderSize.Text = "Size";
            this.FolderSize.Width = 125;
            // 
            // FolderInfoViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 450);
            this.Controls.Add(this.Tlp);
            this.Name = "FolderInfoViewer";
            this.Text = "Folder Information";
            this.Tlp.ResumeLayout(false);
            this.Tlp.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Tlv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected internal System.Windows.Forms.TableLayoutPanel Tlp;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button BtnClose;
        internal BrightIdeasSoftware.TreeListView Tlv;
        private BrightIdeasSoftware.OLVColumn FolderName;
        private BrightIdeasSoftware.OLVColumn ItemCount;
        private BrightIdeasSoftware.OLVColumn ItemCountSubFolders;
        private BrightIdeasSoftware.OLVColumn FolderSize;
    }
}