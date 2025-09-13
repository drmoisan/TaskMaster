namespace QuickFiler
{
    partial class QfcFormViewer
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
            this._l1v_TableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.L1v1L2h_TableLayout = new System.Windows.Forms.TableLayoutPanel();
            this._l1v1L2h5_BtnSkip = new System.Windows.Forms.Button();
            this.ButtonFilters = new System.Windows.Forms.Button();
            this._l1v1L2h2_ButtonOK = new System.Windows.Forms.Button();
            this._l1v1L2h3_ButtonCancel = new System.Windows.Forms.Button();
            this._l1v1L2h4_ButtonUndo = new System.Windows.Forms.Button();
            this._l1v1L2h5_SpnEmailPerLoad = new System.Windows.Forms.NumericUpDown();
            this._l1v0L2_PanelMain = new System.Windows.Forms.Panel();
            this._l1v0L2L3v_TableLayout = new System.Windows.Forms.TableLayoutPanel();
            this._QfcItemViewerTemplate = new QuickFiler.ItemViewer();
            this._qfcItemViewerExpandedTemplate = new QuickFiler.ItemViewerExpanded();
            this.WorkerInternal = new System.ComponentModel.BackgroundWorker();
            this._l1v_TableLayout.SuspendLayout();
            this.L1v1L2h_TableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._l1v1L2h5_SpnEmailPerLoad)).BeginInit();
            this._l1v0L2_PanelMain.SuspendLayout();
            this._l1v0L2L3v_TableLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // L1v_TableLayout
            // 
            this._l1v_TableLayout.ColumnCount = 1;
            this._l1v_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._l1v_TableLayout.Controls.Add(this.L1v1L2h_TableLayout, 0, 1);
            this._l1v_TableLayout.Controls.Add(this._l1v0L2_PanelMain, 0, 0);
            this._l1v_TableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._l1v_TableLayout.Location = new System.Drawing.Point(0, 0);
            this._l1v_TableLayout.Margin = new System.Windows.Forms.Padding(6);
            this._l1v_TableLayout.Name = "L1v_TableLayout";
            this._l1v_TableLayout.RowCount = 2;
            this._l1v_TableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._l1v_TableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 108F));
            this._l1v_TableLayout.Size = new System.Drawing.Size(2165, 1293);
            this._l1v_TableLayout.TabIndex = 0;
            // 
            // L1v1L2h_TableLayout
            // 
            this.L1v1L2h_TableLayout.ColumnCount = 8;
            this.L1v1L2h_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.L1v1L2h_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320F));
            this.L1v1L2h_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320F));
            this.L1v1L2h_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.L1v1L2h_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.L1v1L2h_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.L1v1L2h_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.L1v1L2h_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.L1v1L2h_TableLayout.Controls.Add(this._l1v1L2h5_BtnSkip, 5, 0);
            this.L1v1L2h_TableLayout.Controls.Add(this.ButtonFilters, 4, 0);
            this.L1v1L2h_TableLayout.Controls.Add(this._l1v1L2h2_ButtonOK, 1, 0);
            this.L1v1L2h_TableLayout.Controls.Add(this._l1v1L2h3_ButtonCancel, 2, 0);
            this.L1v1L2h_TableLayout.Controls.Add(this._l1v1L2h4_ButtonUndo, 3, 0);
            this.L1v1L2h_TableLayout.Controls.Add(this._l1v1L2h5_SpnEmailPerLoad, 6, 0);
            this.L1v1L2h_TableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v1L2h_TableLayout.Location = new System.Drawing.Point(6, 1191);
            this.L1v1L2h_TableLayout.Margin = new System.Windows.Forms.Padding(6);
            this.L1v1L2h_TableLayout.Name = "L1v1L2h_TableLayout";
            this.L1v1L2h_TableLayout.RowCount = 1;
            this.L1v1L2h_TableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v1L2h_TableLayout.Size = new System.Drawing.Size(2153, 96);
            this.L1v1L2h_TableLayout.TabIndex = 0;
            // 
            // L1v1L2h5_BtnSkip
            // 
            this._l1v1L2h5_BtnSkip.Dock = System.Windows.Forms.DockStyle.Fill;
            this._l1v1L2h5_BtnSkip.Enabled = false;
            this._l1v1L2h5_BtnSkip.Location = new System.Drawing.Point(1402, 6);
            this._l1v1L2h5_BtnSkip.Margin = new System.Windows.Forms.Padding(6);
            this._l1v1L2h5_BtnSkip.Name = "L1v1L2h5_BtnSkip";
            this._l1v1L2h5_BtnSkip.Size = new System.Drawing.Size(148, 84);
            this._l1v1L2h5_BtnSkip.TabIndex = 5;
            this._l1v1L2h5_BtnSkip.Text = "Skip Group";
            this._l1v1L2h5_BtnSkip.UseVisualStyleBackColor = true;
            // 
            // ButtonFilters
            // 
            this.ButtonFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonFilters.Location = new System.Drawing.Point(1242, 6);
            this.ButtonFilters.Margin = new System.Windows.Forms.Padding(6);
            this.ButtonFilters.Name = "ButtonFilters";
            this.ButtonFilters.Size = new System.Drawing.Size(148, 84);
            this.ButtonFilters.TabIndex = 4;
            this.ButtonFilters.Text = "Filters";
            this.ButtonFilters.UseVisualStyleBackColor = true;
            // 
            // L1v1L2h2_ButtonOK
            // 
            this._l1v1L2h2_ButtonOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this._l1v1L2h2_ButtonOK.Location = new System.Drawing.Point(450, 6);
            this._l1v1L2h2_ButtonOK.Margin = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this._l1v1L2h2_ButtonOK.Name = "L1v1L2h2_ButtonOK";
            this._l1v1L2h2_ButtonOK.Size = new System.Drawing.Size(292, 84);
            this._l1v1L2h2_ButtonOK.TabIndex = 0;
            this._l1v1L2h2_ButtonOK.Text = "OK";
            this._l1v1L2h2_ButtonOK.UseVisualStyleBackColor = true;
            // 
            // L1v1L2h3_ButtonCancel
            // 
            this._l1v1L2h3_ButtonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._l1v1L2h3_ButtonCancel.Location = new System.Drawing.Point(770, 6);
            this._l1v1L2h3_ButtonCancel.Margin = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this._l1v1L2h3_ButtonCancel.Name = "L1v1L2h3_ButtonCancel";
            this._l1v1L2h3_ButtonCancel.Size = new System.Drawing.Size(292, 84);
            this._l1v1L2h3_ButtonCancel.TabIndex = 1;
            this._l1v1L2h3_ButtonCancel.Text = "CANCEL";
            this._l1v1L2h3_ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // L1v1L2h4_ButtonUndo
            // 
            this._l1v1L2h4_ButtonUndo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._l1v1L2h4_ButtonUndo.Location = new System.Drawing.Point(1082, 6);
            this._l1v1L2h4_ButtonUndo.Margin = new System.Windows.Forms.Padding(6);
            this._l1v1L2h4_ButtonUndo.Name = "L1v1L2h4_ButtonUndo";
            this._l1v1L2h4_ButtonUndo.Size = new System.Drawing.Size(148, 84);
            this._l1v1L2h4_ButtonUndo.TabIndex = 2;
            this._l1v1L2h4_ButtonUndo.Text = "Undo";
            this._l1v1L2h4_ButtonUndo.UseVisualStyleBackColor = true;
            // 
            // L1v1L2h5_SpnEmailPerLoad
            // 
            this._l1v1L2h5_SpnEmailPerLoad.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._l1v1L2h5_SpnEmailPerLoad.Enabled = false;
            this._l1v1L2h5_SpnEmailPerLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._l1v1L2h5_SpnEmailPerLoad.Location = new System.Drawing.Point(1570, 11);
            this._l1v1L2h5_SpnEmailPerLoad.Margin = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this._l1v1L2h5_SpnEmailPerLoad.Name = "L1v1L2h5_SpnEmailPerLoad";
            this._l1v1L2h5_SpnEmailPerLoad.Size = new System.Drawing.Size(132, 74);
            this._l1v1L2h5_SpnEmailPerLoad.TabIndex = 3;
            // 
            // L1v0L2_PanelMain
            // 
            this._l1v0L2_PanelMain.AutoScroll = true;
            this._l1v0L2_PanelMain.AutoSize = true;
            this._l1v0L2_PanelMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._l1v0L2_PanelMain.Controls.Add(this._l1v0L2L3v_TableLayout);
            this._l1v0L2_PanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this._l1v0L2_PanelMain.Location = new System.Drawing.Point(6, 6);
            this._l1v0L2_PanelMain.Margin = new System.Windows.Forms.Padding(6);
            this._l1v0L2_PanelMain.Name = "L1v0L2_PanelMain";
            this._l1v0L2_PanelMain.Size = new System.Drawing.Size(2153, 1173);
            this._l1v0L2_PanelMain.TabIndex = 1;
            // 
            // L1v0L2L3v_TableLayout
            // 
            this._l1v0L2L3v_TableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._l1v0L2L3v_TableLayout.ColumnCount = 2;
            this._l1v0L2L3v_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this._l1v0L2L3v_TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._l1v0L2L3v_TableLayout.Controls.Add(this._QfcItemViewerTemplate, 0, 0);
            this._l1v0L2L3v_TableLayout.Controls.Add(this._qfcItemViewerExpandedTemplate, 0, 1);
            this._l1v0L2L3v_TableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this._l1v0L2L3v_TableLayout.Location = new System.Drawing.Point(0, 0);
            this._l1v0L2L3v_TableLayout.Margin = new System.Windows.Forms.Padding(6);
            this._l1v0L2L3v_TableLayout.Name = "L1v0L2L3v_TableLayout";
            this._l1v0L2L3v_TableLayout.RowCount = 3;
            this._l1v0L2L3v_TableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 221F));
            this._l1v0L2L3v_TableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1218F));
            this._l1v0L2L3v_TableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._l1v0L2L3v_TableLayout.Size = new System.Drawing.Size(2119, 1337);
            this._l1v0L2L3v_TableLayout.TabIndex = 0;
            // 
            // QfcItemViewerTemplate
            // 
            this._QfcItemViewerTemplate.AutoSize = true;
            this._QfcItemViewerTemplate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._l1v0L2L3v_TableLayout.SetColumnSpan(this._QfcItemViewerTemplate, 2);
            this._QfcItemViewerTemplate.Controller = null;
            this._QfcItemViewerTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this._QfcItemViewerTemplate.Location = new System.Drawing.Point(12, 12);
            this._QfcItemViewerTemplate.Margin = new System.Windows.Forms.Padding(12);
            this._QfcItemViewerTemplate.MinimumSize = new System.Drawing.Size(1370, 183);
            this._QfcItemViewerTemplate.Name = "QfcItemViewerTemplate";
            this._QfcItemViewerTemplate.Size = new System.Drawing.Size(2095, 197);
            this._QfcItemViewerTemplate.TabIndex = 0;
            // 
            // QfcItemViewerExpandedTemplate
            // 
            this._qfcItemViewerExpandedTemplate.AutoSize = true;
            this._qfcItemViewerExpandedTemplate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._l1v0L2L3v_TableLayout.SetColumnSpan(this._qfcItemViewerExpandedTemplate, 2);
            this._qfcItemViewerExpandedTemplate.Controller = null;
            this._qfcItemViewerExpandedTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this._qfcItemViewerExpandedTemplate.Location = new System.Drawing.Point(6, 227);
            this._qfcItemViewerExpandedTemplate.Margin = new System.Windows.Forms.Padding(6);
            this._qfcItemViewerExpandedTemplate.MinimumSize = new System.Drawing.Size(1516, 197);
            this._qfcItemViewerExpandedTemplate.Name = "QfcItemViewerExpandedTemplate";
            this._qfcItemViewerExpandedTemplate.Size = new System.Drawing.Size(2107, 1206);
            this._qfcItemViewerExpandedTemplate.TabIndex = 1;
            // 
            // QfcFormViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(2165, 1293);
            this.Controls.Add(this._l1v_TableLayout);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimumSize = new System.Drawing.Size(2191, 402);
            this.Name = "QfcFormViewer";
            this.Text = "Quick File";
            this._l1v_TableLayout.ResumeLayout(false);
            this._l1v_TableLayout.PerformLayout();
            this.L1v1L2h_TableLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._l1v1L2h5_SpnEmailPerLoad)).EndInit();
            this._l1v0L2_PanelMain.ResumeLayout(false);
            this._l1v0L2L3v_TableLayout.ResumeLayout(false);
            this._l1v0L2L3v_TableLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TableLayoutPanel _l1v_TableLayout;
        public System.Windows.Forms.TableLayoutPanel L1v1L2h_TableLayout;
        public System.Windows.Forms.Button _l1v1L2h2_ButtonOK;
        public System.Windows.Forms.Button _l1v1L2h3_ButtonCancel;
        public System.Windows.Forms.Button _l1v1L2h4_ButtonUndo;
        public System.Windows.Forms.NumericUpDown _l1v1L2h5_SpnEmailPerLoad;
        public System.Windows.Forms.Panel _l1v0L2_PanelMain;
        public System.Windows.Forms.TableLayoutPanel _l1v0L2L3v_TableLayout;
        public ItemViewer _QfcItemViewerTemplate;
        public System.ComponentModel.BackgroundWorker WorkerInternal;
        public System.Windows.Forms.Button ButtonFilters;
        public System.Windows.Forms.Button _l1v1L2h5_BtnSkip;
        public ItemViewerExpanded _qfcItemViewerExpandedTemplate;
    }
}