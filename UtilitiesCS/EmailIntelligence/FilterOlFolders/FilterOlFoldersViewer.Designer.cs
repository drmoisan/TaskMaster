namespace UtilitiesCS
{
    partial class FilterOlFoldersViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterOlFoldersViewer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.TlvNotFiltered = new BrightIdeasSoftware.TreeListView();
            this.olvColumnName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageListSmall = new System.Windows.Forms.ImageList(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.TlvFiltered = new BrightIdeasSoftware.TreeListView();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.BtnSave = new System.Windows.Forms.Button();
            this.BtnDiscard = new System.Windows.Forms.Button();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TlvNotFiltered)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TlvFiltered)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.TlvNotFiltered);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.TlvFiltered);
            this.splitContainer1.Size = new System.Drawing.Size(1830, 894);
            this.splitContainer1.SplitterDistance = 911;
            this.splitContainer1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(328, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 51);
            this.label1.TabIndex = 1;
            this.label1.Text = "Not Filtered";
            // 
            // TlvNotFiltered
            // 
            this.TlvNotFiltered.AllColumns.Add(this.olvColumnName);
            this.TlvNotFiltered.AllowColumnReorder = true;
            this.TlvNotFiltered.AllowDrop = true;
            this.TlvNotFiltered.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TlvNotFiltered.CellEditUseWholeCell = false;
            this.TlvNotFiltered.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnName});
            this.TlvNotFiltered.Cursor = System.Windows.Forms.Cursors.Default;
            this.TlvNotFiltered.EmptyListMsg = "This folder is completely empty!";
            this.TlvNotFiltered.EmptyListMsgFont = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TlvNotFiltered.HideSelection = false;
            this.TlvNotFiltered.IsSimpleDragSource = true;
            this.TlvNotFiltered.IsSimpleDropSink = true;
            this.TlvNotFiltered.Location = new System.Drawing.Point(3, 73);
            this.TlvNotFiltered.Name = "TlvNotFiltered";
            this.TlvNotFiltered.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.Submenu;
            this.TlvNotFiltered.ShowCommandMenuOnRightClick = true;
            this.TlvNotFiltered.ShowGroups = false;
            this.TlvNotFiltered.ShowImagesOnSubItems = true;
            this.TlvNotFiltered.ShowItemToolTips = true;
            this.TlvNotFiltered.Size = new System.Drawing.Size(905, 818);
            this.TlvNotFiltered.SmallImageList = this.imageListSmall;
            this.TlvNotFiltered.TabIndex = 0;
            this.TlvNotFiltered.UseCompatibleStateImageBehavior = false;
            this.TlvNotFiltered.UseFilterIndicator = true;
            this.TlvNotFiltered.UseFiltering = true;
            this.TlvNotFiltered.UseHotItem = true;
            this.TlvNotFiltered.View = System.Windows.Forms.View.Details;
            this.TlvNotFiltered.VirtualMode = true;
            // 
            // olvColumnName
            // 
            this.olvColumnName.AspectName = "Value.OlFolder.Name";
            this.olvColumnName.IsEditable = false;
            this.olvColumnName.IsTileViewColumn = true;
            this.olvColumnName.Text = "Name";
            this.olvColumnName.UseInitialLetterForGroup = true;
            this.olvColumnName.Width = 863;
            this.olvColumnName.WordWrap = true;
            // 
            // imageListSmall
            // 
            this.imageListSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSmall.ImageStream")));
            this.imageListSmall.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSmall.Images.SetKeyName(0, "compass");
            this.imageListSmall.Images.SetKeyName(1, "down");
            this.imageListSmall.Images.SetKeyName(2, "user");
            this.imageListSmall.Images.SetKeyName(3, "find");
            this.imageListSmall.Images.SetKeyName(4, "folder");
            this.imageListSmall.Images.SetKeyName(5, "movie");
            this.imageListSmall.Images.SetKeyName(6, "music");
            this.imageListSmall.Images.SetKeyName(7, "no");
            this.imageListSmall.Images.SetKeyName(8, "readonly");
            this.imageListSmall.Images.SetKeyName(9, "public");
            this.imageListSmall.Images.SetKeyName(10, "recycle");
            this.imageListSmall.Images.SetKeyName(11, "spanner");
            this.imageListSmall.Images.SetKeyName(12, "star");
            this.imageListSmall.Images.SetKeyName(13, "tick");
            this.imageListSmall.Images.SetKeyName(14, "archive");
            this.imageListSmall.Images.SetKeyName(15, "system");
            this.imageListSmall.Images.SetKeyName(16, "hidden");
            this.imageListSmall.Images.SetKeyName(17, "temporary");
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(370, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(174, 51);
            this.label2.TabIndex = 2;
            this.label2.Text = "Filtered";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TlvFiltered
            // 
            this.TlvFiltered.AllColumns.Add(this.olvColumn1);
            this.TlvFiltered.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TlvFiltered.CellEditUseWholeCell = false;
            this.TlvFiltered.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.TlvFiltered.Cursor = System.Windows.Forms.Cursors.Default;
            this.TlvFiltered.HideSelection = false;
            this.TlvFiltered.Location = new System.Drawing.Point(3, 73);
            this.TlvFiltered.Name = "TlvFiltered";
            this.TlvFiltered.ShowGroups = false;
            this.TlvFiltered.Size = new System.Drawing.Size(908, 818);
            this.TlvFiltered.TabIndex = 1;
            this.TlvFiltered.UseCompatibleStateImageBehavior = false;
            this.TlvFiltered.View = System.Windows.Forms.View.Details;
            this.TlvFiltered.VirtualMode = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.BtnSave);
            this.flowLayoutPanel1.Controls.Add(this.BtnDiscard);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(628, 912);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(596, 90);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // BtnSave
            // 
            this.BtnSave.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BtnSave.Location = new System.Drawing.Point(3, 3);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(292, 84);
            this.BtnSave.TabIndex = 0;
            this.BtnSave.Text = "Save Filter";
            this.BtnSave.UseVisualStyleBackColor = true;
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BtnDiscard
            // 
            this.BtnDiscard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BtnDiscard.Location = new System.Drawing.Point(301, 3);
            this.BtnDiscard.Name = "BtnDiscard";
            this.BtnDiscard.Size = new System.Drawing.Size(292, 84);
            this.BtnDiscard.TabIndex = 1;
            this.BtnDiscard.Text = "Discard Changes";
            this.BtnDiscard.UseVisualStyleBackColor = true;
            this.BtnDiscard.Click += new System.EventHandler(this.BtnDiscard_Click);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "Value.OlFolder.Name";
            this.olvColumn1.Text = "Name";
            this.olvColumn1.Width = 849;
            // 
            // FilterOlFoldersViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1854, 1005);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "FilterOlFoldersViewer";
            this.Text = "FilterOlFoldersViewer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TlvNotFiltered)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TlvFiltered)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.TreeListView TlvNotFiltered;
        private BrightIdeasSoftware.OLVColumn olvColumnName;
        private BrightIdeasSoftware.TreeListView TlvFiltered;
        private System.Windows.Forms.ImageList imageListSmall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button BtnSave;
        private System.Windows.Forms.Button BtnDiscard;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
    }
}