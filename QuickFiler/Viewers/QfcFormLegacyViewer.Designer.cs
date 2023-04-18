using System;
using System.Diagnostics;

namespace QuickFiler
{
    public partial class QfcFormLegacyViewer : System.Windows.Forms.Form
    {
        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is not null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        private void InitializeComponent()
        {
            this.L1v = new System.Windows.Forms.TableLayoutPanel();
            this.L1v2L2h = new System.Windows.Forms.TableLayoutPanel();
            this.KeyboardDialog = new System.Windows.Forms.TextBox();
            this.L1v2L2h3_ButtonOK = new System.Windows.Forms.Button();
            this.L1v2L2h4_ButtonCancel = new System.Windows.Forms.Button();
            this.L1v2L2h4_ButtonUndo = new System.Windows.Forms.Button();
            this.L1v2L2h5_SpnEmailPerLoad = new System.Windows.Forms.NumericUpDown();
            this.L1v1L2_PanelMain = new System.Windows.Forms.Panel();
            this.L1v1L2L3v = new System.Windows.Forms.TableLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.L1v.SuspendLayout();
            this.L1v2L2h.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.L1v2L2h5_SpnEmailPerLoad)).BeginInit();
            this.L1v1L2_PanelMain.SuspendLayout();
            this.L1v1L2L3v.SuspendLayout();
            this.SuspendLayout();
            // 
            // L1v
            // 
            this.L1v.ColumnCount = 1;
            this.L1v.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v.Controls.Add(this.L1v2L2h, 0, 1);
            this.L1v.Controls.Add(this.L1v1L2_PanelMain, 0, 0);
            this.L1v.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v.Location = new System.Drawing.Point(0, 0);
            this.L1v.Name = "L1v";
            this.L1v.RowCount = 2;
            this.L1v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.L1v.Size = new System.Drawing.Size(914, 274);
            this.L1v.TabIndex = 0;
            // 
            // L1v2L2h
            // 
            this.L1v2L2h.ColumnCount = 7;
            this.L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.L1v2L2h.Controls.Add(this.KeyboardDialog, 0, 0);
            this.L1v2L2h.Controls.Add(this.L1v2L2h3_ButtonOK, 2, 0);
            this.L1v2L2h.Controls.Add(this.L1v2L2h4_ButtonCancel, 3, 0);
            this.L1v2L2h.Controls.Add(this.L1v2L2h4_ButtonUndo, 4, 0);
            this.L1v2L2h.Controls.Add(this.L1v2L2h5_SpnEmailPerLoad, 5, 0);
            this.L1v2L2h.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v2L2h.Location = new System.Drawing.Point(3, 221);
            this.L1v2L2h.Name = "L1v2L2h";
            this.L1v2L2h.RowCount = 1;
            this.L1v2L2h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v2L2h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.L1v2L2h.Size = new System.Drawing.Size(908, 50);
            this.L1v2L2h.TabIndex = 0;
            // 
            // KeyboardDialog
            // 
            this.KeyboardDialog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.KeyboardDialog.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyboardDialog.Location = new System.Drawing.Point(7, 3);
            this.KeyboardDialog.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            this.KeyboardDialog.Name = "KeyboardDialog";
            this.KeyboardDialog.Size = new System.Drawing.Size(140, 40);
            this.KeyboardDialog.TabIndex = 5;
            this.KeyboardDialog.Visible = false;
            this.KeyboardDialog.TextChanged += new System.EventHandler(this.AcceleratorDialogue_TextChanged);
            this.KeyboardDialog.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AcceleratorDialogue_KeyDown);
            this.KeyboardDialog.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AcceleratorDialogue_KeyUp);
            // 
            // L1v2L2h3_ButtonOK
            // 
            this.L1v2L2h3_ButtonOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v2L2h3_ButtonOK.Location = new System.Drawing.Point(301, 3);
            this.L1v2L2h3_ButtonOK.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            this.L1v2L2h3_ButtonOK.Name = "L1v2L2h3_ButtonOK";
            this.L1v2L2h3_ButtonOK.Size = new System.Drawing.Size(146, 44);
            this.L1v2L2h3_ButtonOK.TabIndex = 0;
            this.L1v2L2h3_ButtonOK.Text = "OK";
            this.L1v2L2h3_ButtonOK.UseVisualStyleBackColor = true;
            this.L1v2L2h3_ButtonOK.Click += new System.EventHandler(this.Button_OK_Click);
            this.L1v2L2h3_ButtonOK.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Button_OK_KeyDown);
            this.L1v2L2h3_ButtonOK.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Button_OK_KeyUp);
            // 
            // L1v2L2h4_ButtonCancel
            // 
            this.L1v2L2h4_ButtonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v2L2h4_ButtonCancel.Location = new System.Drawing.Point(461, 3);
            this.L1v2L2h4_ButtonCancel.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            this.L1v2L2h4_ButtonCancel.Name = "L1v2L2h4_ButtonCancel";
            this.L1v2L2h4_ButtonCancel.Size = new System.Drawing.Size(146, 44);
            this.L1v2L2h4_ButtonCancel.TabIndex = 1;
            this.L1v2L2h4_ButtonCancel.Text = "CANCEL";
            this.L1v2L2h4_ButtonCancel.UseVisualStyleBackColor = true;
            this.L1v2L2h4_ButtonCancel.Click += new System.EventHandler(this.L1v2L2h4_ButtonCancel_Click);
            // 
            // L1v2L2h4_ButtonUndo
            // 
            this.L1v2L2h4_ButtonUndo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v2L2h4_ButtonUndo.Location = new System.Drawing.Point(617, 3);
            this.L1v2L2h4_ButtonUndo.Name = "L1v2L2h4_ButtonUndo";
            this.L1v2L2h4_ButtonUndo.Size = new System.Drawing.Size(54, 44);
            this.L1v2L2h4_ButtonUndo.TabIndex = 2;
            this.L1v2L2h4_ButtonUndo.Text = "Undo";
            this.L1v2L2h4_ButtonUndo.UseVisualStyleBackColor = true;
            this.L1v2L2h4_ButtonUndo.Click += new System.EventHandler(this.Button_Undo_Click);
            // 
            // L1v2L2h5_SpnEmailPerLoad
            // 
            this.L1v2L2h5_SpnEmailPerLoad.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.L1v2L2h5_SpnEmailPerLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.L1v2L2h5_SpnEmailPerLoad.Location = new System.Drawing.Point(681, 4);
            this.L1v2L2h5_SpnEmailPerLoad.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            this.L1v2L2h5_SpnEmailPerLoad.Name = "L1v2L2h5_SpnEmailPerLoad";
            this.L1v2L2h5_SpnEmailPerLoad.Size = new System.Drawing.Size(66, 41);
            this.L1v2L2h5_SpnEmailPerLoad.TabIndex = 3;
            this.L1v2L2h5_SpnEmailPerLoad.ValueChanged += new System.EventHandler(this.spn_EmailPerLoad_ValueChanged);
            // 
            // L1v1L2_PanelMain
            // 
            this.L1v1L2_PanelMain.AutoScroll = true;
            this.L1v1L2_PanelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.L1v1L2_PanelMain.Controls.Add(this.L1v1L2L3v);
            this.L1v1L2_PanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v1L2_PanelMain.Location = new System.Drawing.Point(3, 3);
            this.L1v1L2_PanelMain.Name = "L1v1L2_PanelMain";
            this.L1v1L2_PanelMain.Size = new System.Drawing.Size(908, 212);
            this.L1v1L2_PanelMain.TabIndex = 1;
            this.L1v1L2_PanelMain.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PanelMain_KeyUp);
            this.L1v1L2_PanelMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PanelMain_KeyDown);
            this.L1v1L2_PanelMain.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PanelMain_KeyPress);
            // 
            // L1v1L2L3v
            // 
            this.L1v1L2L3v.AutoSize = true;
            this.L1v1L2L3v.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.L1v1L2L3v.ColumnCount = 1;
            this.L1v1L2L3v.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v1L2L3v.Controls.Add(this.button1, 0, 0);
            this.L1v1L2L3v.Dock = System.Windows.Forms.DockStyle.Top;
            this.L1v1L2L3v.Location = new System.Drawing.Point(0, 0);
            this.L1v1L2L3v.Margin = new System.Windows.Forms.Padding(0);
            this.L1v1L2L3v.Name = "L1v1L2L3v";
            this.L1v1L2L3v.Padding = new System.Windows.Forms.Padding(10);
            this.L1v1L2L3v.RowCount = 1;
            this.L1v1L2L3v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v1L2L3v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.L1v1L2L3v.Size = new System.Drawing.Size(906, 49);
            this.L1v1L2L3v.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // QfcFormLegacyViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(914, 274);
            this.Controls.Add(this.L1v);
            this.Name = "QfcFormLegacyViewer";
            this.Text = "Quick File";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.QuickFileViewer_Closing);
            this.Resize += new System.EventHandler(this.QuickFileViewer_Resize);
            this.L1v.ResumeLayout(false);
            this.L1v2L2h.ResumeLayout(false);
            this.L1v2L2h.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.L1v2L2h5_SpnEmailPerLoad)).EndInit();
            this.L1v1L2_PanelMain.ResumeLayout(false);
            this.L1v1L2_PanelMain.PerformLayout();
            this.L1v1L2L3v.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        internal System.Windows.Forms.TableLayoutPanel L1v;
        internal System.Windows.Forms.TableLayoutPanel L1v2L2h;
        internal System.Windows.Forms.Button L1v2L2h3_ButtonOK;
        internal System.Windows.Forms.Button L1v2L2h4_ButtonCancel;
        internal System.Windows.Forms.Button L1v2L2h4_ButtonUndo;
        internal System.Windows.Forms.NumericUpDown L1v2L2h5_SpnEmailPerLoad;
        internal System.Windows.Forms.Panel L1v1L2_PanelMain;
        internal System.Windows.Forms.TableLayoutPanel L1v1L2L3v;
        internal System.Windows.Forms.TextBox KeyboardDialog;
        private System.Windows.Forms.Button button1;
    }
}