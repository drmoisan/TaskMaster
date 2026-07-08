namespace UtilitiesCS.OutlookObjects.Store
{
    partial class DisabledStoresViewer
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
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
            this._dgv = new System.Windows.Forms.DataGridView();
            this._colDisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._colScope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._colReenable = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this._dgv)).BeginInit();
            this.SuspendLayout();
            //
            // _dgv
            //
            this._dgv.AllowUserToAddRows = false;
            this._dgv.AllowUserToDeleteRows = false;
            this._dgv.AutoGenerateColumns = false;
            this._dgv.ColumnHeadersHeightSizeMode = System
                .Windows
                .Forms
                .DataGridViewColumnHeadersHeightSizeMode
                .AutoSize;
            this._dgv.Columns.AddRange(
                new System.Windows.Forms.DataGridViewColumn[]
                {
                    this._colDisplayName,
                    this._colScope,
                    this._colReenable,
                }
            );
            this._dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dgv.Location = new System.Drawing.Point(0, 0);
            this._dgv.Name = "_dgv";
            this._dgv.RowTemplate.Height = 28;
            this._dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dgv.Size = new System.Drawing.Size(600, 360);
            this._dgv.TabIndex = 0;
            this._dgv.CellFormatting +=
                new System.Windows.Forms.DataGridViewCellFormattingEventHandler(
                    this.Dgv_CellFormatting
                );
            //
            // _colDisplayName
            //
            this._colDisplayName.AutoSizeMode = System
                .Windows
                .Forms
                .DataGridViewAutoSizeColumnMode
                .Fill;
            this._colDisplayName.DataPropertyName = "DisplayName";
            this._colDisplayName.HeaderText = "Store";
            this._colDisplayName.Name = "_colDisplayName";
            this._colDisplayName.ReadOnly = true;
            //
            // _colScope
            //
            this._colScope.DataPropertyName = "ScopeLabel";
            this._colScope.HeaderText = "Scope";
            this._colScope.Name = "_colScope";
            this._colScope.ReadOnly = true;
            //
            // _colReenable
            //
            this._colReenable.HeaderText = "Reenable";
            this._colReenable.Name = "_colReenable";
            this._colReenable.Text = "Reenable";
            this._colReenable.UseColumnTextForButtonValue = true;
            //
            // DisabledStoresViewer
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 360);
            this.Controls.Add(this._dgv);
            this.MinimizeBox = false;
            this.Name = "DisabledStoresViewer";
            this.Text = "Disabled Stores";
            ((System.ComponentModel.ISupportInitialize)(this._dgv)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        /// <summary>
        /// Applies a distinct cell style to future-sessions rows so they are visually
        /// distinguished from session-only rows (AC3). Driven by the controller-supplied
        /// <see cref="DisabledStoreRow.IsFutureSession"/> flag on the bound row.
        /// </summary>
        private void Dgv_CellFormatting(
            object sender,
            System.Windows.Forms.DataGridViewCellFormattingEventArgs e
        )
        {
            if (e.RowIndex < 0 || e.RowIndex >= this._dgv.Rows.Count)
            {
                return;
            }

            if (
                this._dgv.Rows[e.RowIndex].DataBoundItem is DisabledStoreRow row
                && row.IsFutureSession
            )
            {
                e.CellStyle.BackColor = System.Drawing.Color.LightGoldenrodYellow;
                e.CellStyle.ForeColor = System.Drawing.Color.SaddleBrown;
            }
        }

        private System.Windows.Forms.DataGridView _dgv;
        private System.Windows.Forms.DataGridViewTextBoxColumn _colDisplayName;
        private System.Windows.Forms.DataGridViewTextBoxColumn _colScope;
        private System.Windows.Forms.DataGridViewButtonColumn _colReenable;
    }
}
