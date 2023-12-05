namespace UtilitiesCS.EmailIntelligence.FolderRemap
{
    partial class FolderSelector
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
            this.TlvOriginal = new BrightIdeasSoftware.TreeListView();
            this.OlvNameNotFiltered = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.TlvOriginal)).BeginInit();
            this.SuspendLayout();
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
            this.TlvOriginal.Location = new System.Drawing.Point(0, 0);
            this.TlvOriginal.Name = "TlvOriginal";
            this.TlvOriginal.OwnerDraw = false;
            this.TlvOriginal.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.Submenu;
            this.TlvOriginal.ShowCommandMenuOnRightClick = true;
            this.TlvOriginal.ShowGroups = false;
            this.TlvOriginal.ShowImagesOnSubItems = true;
            this.TlvOriginal.ShowItemToolTips = true;
            this.TlvOriginal.Size = new System.Drawing.Size(800, 831);
            this.TlvOriginal.TabIndex = 1;
            this.TlvOriginal.UseCompatibleStateImageBehavior = false;
            this.TlvOriginal.UseFilterIndicator = true;
            this.TlvOriginal.UseFiltering = true;
            this.TlvOriginal.UseHotItem = true;
            this.TlvOriginal.View = System.Windows.Forms.View.Details;
            this.TlvOriginal.VirtualMode = true;
            // 
            // OlvNameNotFiltered
            // 
            this.OlvNameNotFiltered.AspectName = "Value.Name";
            this.OlvNameNotFiltered.FillsFreeSpace = true;
            this.OlvNameNotFiltered.IsEditable = false;
            this.OlvNameNotFiltered.IsTileViewColumn = true;
            this.OlvNameNotFiltered.Text = "Name";
            this.OlvNameNotFiltered.UseInitialLetterForGroup = true;
            this.OlvNameNotFiltered.Width = 494;
            this.OlvNameNotFiltered.WordWrap = true;
            // 
            // FolderSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 831);
            this.Controls.Add(this.TlvOriginal);
            this.Name = "FolderSelector";
            this.Text = "FolderSelector";
            ((System.ComponentModel.ISupportInitialize)(this.TlvOriginal)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal BrightIdeasSoftware.TreeListView TlvOriginal;
        private BrightIdeasSoftware.OLVColumn OlvNameNotFiltered;
    }
}