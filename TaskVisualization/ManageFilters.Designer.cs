namespace TaskVisualization
{
    partial class ManageFilters
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.FiltersOlv = new BrightIdeasSoftware.FastObjectListView();
            this.FilterName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Description = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.BtnAddFilter = new System.Windows.Forms.Button();
            this.BtnEditFilter = new System.Windows.Forms.Button();
            this.BtnDelete = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FiltersOlv)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.FiltersOlv, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(942, 588);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // FiltersOlv
            // 
            this.FiltersOlv.AllColumns.Add(this.FilterName);
            this.FiltersOlv.AllColumns.Add(this.Description);
            this.FiltersOlv.CellEditUseWholeCell = false;
            this.FiltersOlv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FilterName,
            this.Description});
            this.FiltersOlv.Cursor = System.Windows.Forms.Cursors.Default;
            this.FiltersOlv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FiltersOlv.FullRowSelect = true;
            this.FiltersOlv.HideSelection = false;
            this.FiltersOlv.Location = new System.Drawing.Point(3, 3);
            this.FiltersOlv.MultiSelect = false;
            this.FiltersOlv.Name = "FiltersOlv";
            this.FiltersOlv.ShowGroups = false;
            this.FiltersOlv.Size = new System.Drawing.Size(936, 482);
            this.FiltersOlv.TabIndex = 0;
            this.FiltersOlv.UseCompatibleStateImageBehavior = false;
            this.FiltersOlv.View = System.Windows.Forms.View.Details;
            this.FiltersOlv.VirtualMode = true;
            // 
            // FilterName
            // 
            this.FilterName.AspectName = "Name";
            this.FilterName.Text = "Name";
            this.FilterName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FilterName.Width = 348;
            // 
            // Description
            // 
            this.Description.AspectName = "Description";
            this.Description.Text = "Description";
            this.Description.Width = 570;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.BtnAddFilter);
            this.flowLayoutPanel1.Controls.Add(this.BtnEditFilter);
            this.flowLayoutPanel1.Controls.Add(this.BtnDelete);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(177, 491);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(588, 94);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // BtnAddFilter
            // 
            this.BtnAddFilter.Location = new System.Drawing.Point(3, 3);
            this.BtnAddFilter.Name = "BtnAddFilter";
            this.BtnAddFilter.Size = new System.Drawing.Size(190, 65);
            this.BtnAddFilter.TabIndex = 1;
            this.BtnAddFilter.Text = "Add Filter";
            this.BtnAddFilter.UseVisualStyleBackColor = true;
            this.BtnAddFilter.Click += new System.EventHandler(this.BtnAddFilter_Click);
            // 
            // BtnEditFilter
            // 
            this.BtnEditFilter.Location = new System.Drawing.Point(199, 3);
            this.BtnEditFilter.Name = "BtnEditFilter";
            this.BtnEditFilter.Size = new System.Drawing.Size(190, 65);
            this.BtnEditFilter.TabIndex = 2;
            this.BtnEditFilter.Text = "Edit Filter";
            this.BtnEditFilter.UseVisualStyleBackColor = true;
            this.BtnEditFilter.Click += new System.EventHandler(this.BtnEditFilter_Click);
            // 
            // BtnDelete
            // 
            this.BtnDelete.Location = new System.Drawing.Point(395, 3);
            this.BtnDelete.Name = "BtnDelete";
            this.BtnDelete.Size = new System.Drawing.Size(190, 65);
            this.BtnDelete.TabIndex = 3;
            this.BtnDelete.Text = "Delete Filter";
            this.BtnDelete.UseVisualStyleBackColor = true;
            this.BtnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // ManageFilters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(942, 588);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ManageFilters";
            this.Text = "Manage Filters";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FiltersOlv)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        internal BrightIdeasSoftware.FastObjectListView FiltersOlv;
        internal BrightIdeasSoftware.OLVColumn FilterName;
        internal BrightIdeasSoftware.OLVColumn Description;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button BtnAddFilter;
        private System.Windows.Forms.Button BtnEditFilter;
        private System.Windows.Forms.Button BtnDelete;
    }
}