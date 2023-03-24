using System;
using System.Diagnostics;

namespace QuickFiler
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class QuickFileViewer : System.Windows.Forms.Form
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
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            L1v = new System.Windows.Forms.TableLayoutPanel();
            L1v2L2h = new System.Windows.Forms.TableLayoutPanel();
            AcceleratorDialogue = new System.Windows.Forms.TextBox();
            AcceleratorDialogue.KeyDown += new System.Windows.Forms.KeyEventHandler(AcceleratorDialogue_KeyDown);
            AcceleratorDialogue.KeyUp += new System.Windows.Forms.KeyEventHandler(AcceleratorDialogue_KeyUp);
            AcceleratorDialogue.TextChanged += new EventHandler(AcceleratorDialogue_TextChanged);
            L1v2L2h3_ButtonOK = new System.Windows.Forms.Button();
            L1v2L2h3_ButtonOK.Click += new EventHandler(Button_OK_Click);
            L1v2L2h3_ButtonOK.KeyDown += new System.Windows.Forms.KeyEventHandler(Button_OK_KeyDown);
            L1v2L2h3_ButtonOK.KeyUp += new System.Windows.Forms.KeyEventHandler(Button_OK_KeyUp);
            L1v2L2h4_ButtonCancel = new System.Windows.Forms.Button();
            L1v2L2h4_ButtonCancel.Click += new EventHandler(L1v2L2h4_ButtonCancel_Click);
            L1v2L2h4_ButtonUndo = new System.Windows.Forms.Button();
            L1v2L2h4_ButtonUndo.Click += new EventHandler(Button_Undo_Click);
            L1v2L2h5_SpnEmailPerLoad = new System.Windows.Forms.NumericUpDown();
            L1v2L2h5_SpnEmailPerLoad.ValueChanged += new EventHandler(spn_EmailPerLoad_ValueChanged);
            L1v1L2_PanelMain = new System.Windows.Forms.Panel();
            L1v1L2_PanelMain.KeyDown += new System.Windows.Forms.KeyEventHandler(PanelMain_KeyDown);
            L1v1L2_PanelMain.KeyPress += new System.Windows.Forms.KeyPressEventHandler(PanelMain_KeyPress);
            L1v1L2_PanelMain.KeyUp += new System.Windows.Forms.KeyEventHandler(PanelMain_KeyUp);
            L1v1L2L3v = new System.Windows.Forms.TableLayoutPanel();
            L1v.SuspendLayout();
            L1v2L2h.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)L1v2L2h5_SpnEmailPerLoad).BeginInit();
            L1v1L2_PanelMain.SuspendLayout();
            SuspendLayout();
            // 
            // L1v
            // 
            L1v.ColumnCount = 1;
            L1v.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1v.Controls.Add(L1v2L2h, 0, 1);
            L1v.Controls.Add(L1v1L2_PanelMain, 0, 0);
            L1v.Dock = System.Windows.Forms.DockStyle.Fill;
            L1v.Location = new System.Drawing.Point(0, 0);
            L1v.Name = "L1v";
            L1v.RowCount = 2;
            L1v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56.0f));
            L1v.Size = new System.Drawing.Size(919, 274);
            L1v.TabIndex = 0;
            // 
            // L1v2L2h
            // 
            L1v2L2h.ColumnCount = 7;
            L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0f));
            L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140.0f));
            L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0f));
            L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0f));
            L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0f));
            L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0f));
            L1v2L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0f));
            L1v2L2h.Controls.Add(AcceleratorDialogue, 0, 0);
            L1v2L2h.Controls.Add(L1v2L2h3_ButtonOK, 2, 0);
            L1v2L2h.Controls.Add(L1v2L2h4_ButtonCancel, 3, 0);
            L1v2L2h.Controls.Add(L1v2L2h4_ButtonUndo, 4, 0);
            L1v2L2h.Controls.Add(L1v2L2h5_SpnEmailPerLoad, 5, 0);
            L1v2L2h.Dock = System.Windows.Forms.DockStyle.Fill;
            L1v2L2h.Location = new System.Drawing.Point(3, 221);
            L1v2L2h.Name = "L1v2L2h";
            L1v2L2h.RowCount = 1;
            L1v2L2h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1v2L2h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50.0f));
            L1v2L2h.Size = new System.Drawing.Size(913, 50);
            L1v2L2h.TabIndex = 0;
            // 
            // AcceleratorDialogue
            // 
            AcceleratorDialogue.Dock = System.Windows.Forms.DockStyle.Fill;
            AcceleratorDialogue.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            AcceleratorDialogue.Location = new System.Drawing.Point(7, 3);
            AcceleratorDialogue.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            AcceleratorDialogue.Name = "AcceleratorDialogue";
            AcceleratorDialogue.Size = new System.Drawing.Size(142, 40);
            AcceleratorDialogue.TabIndex = 5;
            AcceleratorDialogue.Visible = false;
            // 
            // L1v2L2h3_ButtonOK
            // 
            L1v2L2h3_ButtonOK.Dock = System.Windows.Forms.DockStyle.Fill;
            L1v2L2h3_ButtonOK.Location = new System.Drawing.Point(303, 3);
            L1v2L2h3_ButtonOK.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            L1v2L2h3_ButtonOK.Name = "L1v2L2h3_ButtonOK";
            L1v2L2h3_ButtonOK.Size = new System.Drawing.Size(146, 44);
            L1v2L2h3_ButtonOK.TabIndex = 0;
            L1v2L2h3_ButtonOK.Text = "OK";
            L1v2L2h3_ButtonOK.UseVisualStyleBackColor = true;
            // 
            // L1v2L2h4_ButtonCancel
            // 
            L1v2L2h4_ButtonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            L1v2L2h4_ButtonCancel.Location = new System.Drawing.Point(463, 3);
            L1v2L2h4_ButtonCancel.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            L1v2L2h4_ButtonCancel.Name = "L1v2L2h4_ButtonCancel";
            L1v2L2h4_ButtonCancel.Size = new System.Drawing.Size(146, 44);
            L1v2L2h4_ButtonCancel.TabIndex = 1;
            L1v2L2h4_ButtonCancel.Text = "CANCEL";
            L1v2L2h4_ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // L1v2L2h4_ButtonUndo
            // 
            L1v2L2h4_ButtonUndo.Dock = System.Windows.Forms.DockStyle.Fill;
            L1v2L2h4_ButtonUndo.Location = new System.Drawing.Point(619, 3);
            L1v2L2h4_ButtonUndo.Name = "L1v2L2h4_ButtonUndo";
            L1v2L2h4_ButtonUndo.Size = new System.Drawing.Size(54, 44);
            L1v2L2h4_ButtonUndo.TabIndex = 2;
            L1v2L2h4_ButtonUndo.Text = "Undo";
            L1v2L2h4_ButtonUndo.UseVisualStyleBackColor = true;
            // 
            // L1v2L2h5_SpnEmailPerLoad
            // 
            L1v2L2h5_SpnEmailPerLoad.Anchor = System.Windows.Forms.AnchorStyles.Left;
            L1v2L2h5_SpnEmailPerLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 22.0f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            L1v2L2h5_SpnEmailPerLoad.Location = new System.Drawing.Point(683, 4);
            L1v2L2h5_SpnEmailPerLoad.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            L1v2L2h5_SpnEmailPerLoad.Name = "L1v2L2h5_SpnEmailPerLoad";
            L1v2L2h5_SpnEmailPerLoad.Size = new System.Drawing.Size(66, 41);
            L1v2L2h5_SpnEmailPerLoad.TabIndex = 3;
            // 
            // L1v1L2_PanelMain
            // 
            L1v1L2_PanelMain.AutoScroll = true;
            L1v1L2_PanelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            L1v1L2_PanelMain.Controls.Add(L1v1L2L3v);
            L1v1L2_PanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            L1v1L2_PanelMain.Location = new System.Drawing.Point(3, 3);
            L1v1L2_PanelMain.Name = "L1v1L2_PanelMain";
            L1v1L2_PanelMain.Size = new System.Drawing.Size(913, 212);
            L1v1L2_PanelMain.TabIndex = 1;
            // 
            // L1v1L2L3v
            // 
            L1v1L2L3v.AutoSize = true;
            L1v1L2L3v.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            L1v1L2L3v.ColumnCount = 1;
            L1v1L2L3v.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1v1L2L3v.Dock = System.Windows.Forms.DockStyle.Top;
            L1v1L2L3v.Location = new System.Drawing.Point(0, 0);
            L1v1L2L3v.Margin = new System.Windows.Forms.Padding(0);
            L1v1L2L3v.Name = "L1v1L2L3v";
            L1v1L2L3v.Padding = new System.Windows.Forms.Padding(10);
            L1v1L2L3v.RowCount = 1;
            L1v1L2L3v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1v1L2L3v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1.0f));
            L1v1L2L3v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1.0f));
            L1v1L2L3v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1.0f));
            L1v1L2L3v.Size = new System.Drawing.Size(911, 20);
            L1v1L2L3v.TabIndex = 1;
            // 
            // QuickFileViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(919, 274);
            Controls.Add(L1v);
            Name = "QuickFileViewer";
            Text = "Quick File";
            L1v.ResumeLayout(false);
            L1v2L2h.ResumeLayout(false);
            L1v2L2h.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)L1v2L2h5_SpnEmailPerLoad).EndInit();
            L1v1L2_PanelMain.ResumeLayout(false);
            L1v1L2_PanelMain.PerformLayout();
            Activated += new EventHandler(QuickFileViewer_Activated);
            Closing += new System.ComponentModel.CancelEventHandler(QuickFileViewer_Closing);
            Resize += new EventHandler(QuickFileViewer_Resize);
            ResumeLayout(false);

        }

        internal System.Windows.Forms.TableLayoutPanel L1v;
        internal System.Windows.Forms.TableLayoutPanel L1v2L2h;
        internal System.Windows.Forms.Button L1v2L2h3_ButtonOK;
        internal System.Windows.Forms.Button L1v2L2h4_ButtonCancel;
        internal System.Windows.Forms.Button L1v2L2h4_ButtonUndo;
        internal System.Windows.Forms.NumericUpDown L1v2L2h5_SpnEmailPerLoad;
        internal System.Windows.Forms.Panel L1v1L2_PanelMain;
        internal System.Windows.Forms.TableLayoutPanel L1v1L2L3v;
        internal System.Windows.Forms.TextBox AcceleratorDialogue;
    }
}