namespace UtilitiesCS.EmailIntelligence.FolderRemap
{
    partial class FolderRemapViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderRemapViewer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.TlvOriginal = new BrightIdeasSoftware.TreeListView();
            this.OlvNameNotFiltered = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.OlvMap = new BrightIdeasSoftware.FastObjectListView();
            this.Original = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Remapped = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.BtnSave = new System.Windows.Forms.Button();
            this.BtnDiscard = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TlvOriginal)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OlvMap)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitContainer1.Size = new System.Drawing.Size(1804, 894);
            this.splitContainer1.SplitterDistance = 747;
            this.splitContainer1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.TlvOriginal, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(747, 894);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(741, 80);
            this.label1.TabIndex = 1;
            this.label1.Text = "Original";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TlvOriginal
            // 
            this.TlvOriginal.AllColumns.Add(this.OlvNameNotFiltered);
            this.TlvOriginal.AllowColumnReorder = true;
            this.TlvOriginal.AllowDrop = true;
            this.TlvOriginal.CellEditUseWholeCell = false;
            this.TlvOriginal.CheckBoxes = true;
            this.TlvOriginal.CheckedAspectName = "";
            this.TlvOriginal.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OlvNameNotFiltered});
            this.TlvOriginal.Cursor = System.Windows.Forms.Cursors.Default;
            this.TlvOriginal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TlvOriginal.EmptyListMsg = "This folder is completely empty!";
            this.TlvOriginal.EmptyListMsgFont = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TlvOriginal.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TlvOriginal.HideSelection = false;
            this.TlvOriginal.IsSimpleDragSource = true;
            this.TlvOriginal.IsSimpleDropSink = true;
            this.TlvOriginal.Location = new System.Drawing.Point(3, 83);
            this.TlvOriginal.Name = "TlvOriginal";
            this.TlvOriginal.OwnerDraw = false;
            this.TlvOriginal.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.Submenu;
            this.TlvOriginal.ShowCommandMenuOnRightClick = true;
            this.TlvOriginal.ShowGroups = false;
            this.TlvOriginal.ShowImagesOnSubItems = true;
            this.TlvOriginal.ShowItemToolTips = true;
            this.TlvOriginal.Size = new System.Drawing.Size(741, 808);
            this.TlvOriginal.SmallImageList = this.imageList1;
            this.TlvOriginal.TabIndex = 0;
            this.TlvOriginal.UseCompatibleStateImageBehavior = false;
            this.TlvOriginal.UseFilterIndicator = true;
            this.TlvOriginal.UseFiltering = true;
            this.TlvOriginal.UseHotItem = true;
            this.TlvOriginal.View = System.Windows.Forms.View.Details;
            this.TlvOriginal.VirtualMode = true;
            this.TlvOriginal.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.TlvOriginal_ModelCanDrop);
            this.TlvOriginal.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.TlvOriginal_ModelDropped);
            // 
            // OlvNameNotFiltered
            // 
            this.OlvNameNotFiltered.AspectName = "Value.Name";
            this.OlvNameNotFiltered.IsEditable = false;
            this.OlvNameNotFiltered.IsTileViewColumn = true;
            this.OlvNameNotFiltered.Text = "Name";
            this.OlvNameNotFiltered.UseInitialLetterForGroup = true;
            this.OlvNameNotFiltered.Width = 494;
            this.OlvNameNotFiltered.WordWrap = true;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "FolderClosed.png");
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.OlvMap, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1053, 894);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1047, 80);
            this.label2.TabIndex = 4;
            this.label2.Text = "Remapping";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // OlvMap
            // 
            this.OlvMap.AllColumns.Add(this.Original);
            this.OlvMap.AllColumns.Add(this.Remapped);
            this.OlvMap.CellEditUseWholeCell = false;
            this.OlvMap.CheckBoxes = true;
            this.OlvMap.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Original,
            this.Remapped});
            this.OlvMap.Cursor = System.Windows.Forms.Cursors.Default;
            this.OlvMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OlvMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OlvMap.HideSelection = false;
            this.OlvMap.Location = new System.Drawing.Point(3, 83);
            this.OlvMap.Name = "OlvMap";
            this.OlvMap.ShowGroups = false;
            this.OlvMap.ShowImagesOnSubItems = true;
            this.OlvMap.Size = new System.Drawing.Size(1047, 808);
            this.OlvMap.TabIndex = 5;
            this.OlvMap.UseCompatibleStateImageBehavior = false;
            this.OlvMap.View = System.Windows.Forms.View.Details;
            this.OlvMap.VirtualMode = true;
            // 
            // Original
            // 
            this.Original.AspectName = "RelativePath";
            this.Original.Text = "Original";
            this.Original.Width = 486;
            // 
            // Remapped
            // 
            this.Remapped.AspectName = "MappedTo.RelativePath";
            this.Remapped.Text = "Remapped";
            this.Remapped.Width = 437;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.BtnSave);
            this.flowLayoutPanel1.Controls.Add(this.BtnDiscard);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(615, 912);
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
            // FolderRemapViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1828, 1005);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "FolderRemapViewer";
            this.Text = "FilterOlFoldersViewer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TlvOriginal)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OlvMap)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.OLVColumn OlvNameNotFiltered;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button BtnSave;
        private System.Windows.Forms.Button BtnDiscard;
        private System.Windows.Forms.ImageList imageList1;
        internal BrightIdeasSoftware.TreeListView TlvOriginal;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label2;
        internal BrightIdeasSoftware.FastObjectListView OlvMap;
        private BrightIdeasSoftware.OLVColumn Original;
        private BrightIdeasSoftware.OLVColumn Remapped;
    }
}