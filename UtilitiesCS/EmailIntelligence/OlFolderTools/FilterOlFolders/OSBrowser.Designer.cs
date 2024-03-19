namespace UtilitiesCS.EmailIntelligence.FilterOlFolders
{
    partial class OSBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OSBrowser));
            this.treeListView = new BrightIdeasSoftware.TreeListView();
            this.olvColumnName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnCreated = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnModified = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnSize = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnFileType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnAttributes = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageListSmall = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).BeginInit();
            this.SuspendLayout();
            // 
            // treeListView
            // 
            this.treeListView.AllColumns.Add(this.olvColumnName);
            this.treeListView.AllColumns.Add(this.olvColumnCreated);
            this.treeListView.AllColumns.Add(this.olvColumnModified);
            this.treeListView.AllColumns.Add(this.olvColumnSize);
            this.treeListView.AllColumns.Add(this.olvColumnFileType);
            this.treeListView.AllColumns.Add(this.olvColumnAttributes);
            this.treeListView.AllowColumnReorder = true;
            this.treeListView.AllowDrop = true;
            this.treeListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeListView.CellEditUseWholeCell = false;
            this.treeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnName,
            this.olvColumnCreated,
            this.olvColumnModified,
            this.olvColumnSize,
            this.olvColumnFileType,
            this.olvColumnAttributes});
            this.treeListView.Cursor = System.Windows.Forms.Cursors.Default;
            this.treeListView.EmptyListMsg = "This folder is completely empty!";
            this.treeListView.EmptyListMsgFont = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeListView.HideSelection = false;
            this.treeListView.IsSimpleDragSource = true;
            this.treeListView.IsSimpleDropSink = true;
            this.treeListView.Location = new System.Drawing.Point(28, 6);
            this.treeListView.Name = "treeListView";
            this.treeListView.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.Submenu;
            this.treeListView.ShowCommandMenuOnRightClick = true;
            this.treeListView.ShowGroups = false;
            this.treeListView.ShowImagesOnSubItems = true;
            this.treeListView.ShowItemToolTips = true;
            this.treeListView.Size = new System.Drawing.Size(1335, 818);
            this.treeListView.TabIndex = 1;
            this.treeListView.UseCompatibleStateImageBehavior = false;
            this.treeListView.UseFilterIndicator = true;
            this.treeListView.UseFiltering = true;
            this.treeListView.UseHotItem = true;
            this.treeListView.View = System.Windows.Forms.View.Details;
            this.treeListView.VirtualMode = true;
            // 
            // olvColumnName
            // 
            this.olvColumnName.AspectName = "Name";
            this.olvColumnName.IsTileViewColumn = true;
            this.olvColumnName.Text = "Name";
            this.olvColumnName.UseInitialLetterForGroup = true;
            this.olvColumnName.Width = 180;
            this.olvColumnName.WordWrap = true;
            // 
            // olvColumnCreated
            // 
            this.olvColumnCreated.AspectName = "CreationTime";
            this.olvColumnCreated.DisplayIndex = 4;
            this.olvColumnCreated.Text = "Created";
            this.olvColumnCreated.Width = 131;
            // 
            // olvColumnModified
            // 
            this.olvColumnModified.AspectName = "LastWriteTime";
            this.olvColumnModified.DisplayIndex = 1;
            this.olvColumnModified.IsTileViewColumn = true;
            this.olvColumnModified.Text = "Modified";
            this.olvColumnModified.Width = 145;
            // 
            // olvColumnSize
            // 
            this.olvColumnSize.AspectName = "Extension";
            this.olvColumnSize.DisplayIndex = 2;
            this.olvColumnSize.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.olvColumnSize.Text = "Size";
            this.olvColumnSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.olvColumnSize.Width = 80;
            // 
            // olvColumnFileType
            // 
            this.olvColumnFileType.DisplayIndex = 3;
            this.olvColumnFileType.IsTileViewColumn = true;
            this.olvColumnFileType.Text = "File Type";
            this.olvColumnFileType.Width = 148;
            // 
            // olvColumnAttributes
            // 
            this.olvColumnAttributes.FillsFreeSpace = true;
            this.olvColumnAttributes.IsEditable = false;
            this.olvColumnAttributes.MinimumWidth = 20;
            this.olvColumnAttributes.Text = "Attributes";
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
            // OSBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 830);
            this.Controls.Add(this.treeListView);
            this.Name = "OSBrowser";
            this.Text = "OSBrowser";
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.TreeListView treeListView;
        private BrightIdeasSoftware.OLVColumn olvColumnName;
        private BrightIdeasSoftware.OLVColumn olvColumnCreated;
        private BrightIdeasSoftware.OLVColumn olvColumnModified;
        private BrightIdeasSoftware.OLVColumn olvColumnSize;
        private BrightIdeasSoftware.OLVColumn olvColumnFileType;
        private BrightIdeasSoftware.OLVColumn olvColumnAttributes;
        private System.Windows.Forms.ImageList imageListSmall;
    }
}