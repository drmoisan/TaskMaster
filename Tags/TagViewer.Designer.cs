using System;
using System.Diagnostics;

namespace Tags
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class TagViewer : System.Windows.Forms.Form
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
            this.L1v2L2_OptionsPanel = new System.Windows.Forms.Panel();
            this.TemplateCheckBox = new System.Windows.Forms.CheckBox();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonNew = new System.Windows.Forms.Button();
            this.ButtonAutoAssign = new System.Windows.Forms.Button();
            this.SearchText = new System.Windows.Forms.TextBox();
            this.HideArchive = new System.Windows.Forms.CheckBox();
            this.L1v_TlpMaster = new System.Windows.Forms.TableLayoutPanel();
            this.L1v1L2h_TlpTop = new System.Windows.Forms.TableLayoutPanel();
            this.L1v3L2h_TlpBottom = new System.Windows.Forms.TableLayoutPanel();
            this.L1v2L2_OptionsPanel.SuspendLayout();
            this.L1v_TlpMaster.SuspendLayout();
            this.L1v1L2h_TlpTop.SuspendLayout();
            this.L1v3L2h_TlpBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // L1v2L2_OptionsPanel
            // 
            this.L1v2L2_OptionsPanel.AutoScroll = true;
            this.L1v2L2_OptionsPanel.AutoSize = true;
            this.L1v2L2_OptionsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.L1v2L2_OptionsPanel.Controls.Add(this.TemplateCheckBox);
            this.L1v2L2_OptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v2L2_OptionsPanel.Location = new System.Drawing.Point(12, 81);
            this.L1v2L2_OptionsPanel.Margin = new System.Windows.Forms.Padding(12);
            this.L1v2L2_OptionsPanel.Name = "L1v2L2_OptionsPanel";
            this.L1v2L2_OptionsPanel.Size = new System.Drawing.Size(884, 717);
            this.L1v2L2_OptionsPanel.TabIndex = 0;
            // 
            // TemplateCheckBox
            // 
            this.TemplateCheckBox.BackColor = System.Drawing.SystemColors.Control;
            this.TemplateCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TemplateCheckBox.Location = new System.Drawing.Point(6, 6);
            this.TemplateCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.TemplateCheckBox.Name = "TemplateCheckBox";
            this.TemplateCheckBox.Padding = new System.Windows.Forms.Padding(3);
            this.TemplateCheckBox.Size = new System.Drawing.Size(813, 47);
            this.TemplateCheckBox.TabIndex = 0;
            this.TemplateCheckBox.Text = "checkBox1";
            this.TemplateCheckBox.UseVisualStyleBackColor = false;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ButtonOk.Location = new System.Drawing.Point(123, 6);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(6);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(140, 53);
            this.ButtonOk.TabIndex = 1;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ButtonCancel.Location = new System.Drawing.Point(293, 6);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(6);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(140, 53);
            this.ButtonCancel.TabIndex = 2;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ButtonNew
            // 
            this.ButtonNew.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ButtonNew.Location = new System.Drawing.Point(463, 6);
            this.ButtonNew.Margin = new System.Windows.Forms.Padding(6);
            this.ButtonNew.Name = "ButtonNew";
            this.ButtonNew.Size = new System.Drawing.Size(140, 53);
            this.ButtonNew.TabIndex = 3;
            this.ButtonNew.Text = "New";
            this.ButtonNew.UseVisualStyleBackColor = true;
            // 
            // ButtonAutoAssign
            // 
            this.ButtonAutoAssign.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ButtonAutoAssign.Location = new System.Drawing.Point(633, 6);
            this.ButtonAutoAssign.Margin = new System.Windows.Forms.Padding(6);
            this.ButtonAutoAssign.Name = "ButtonAutoAssign";
            this.ButtonAutoAssign.Size = new System.Drawing.Size(140, 53);
            this.ButtonAutoAssign.TabIndex = 4;
            this.ButtonAutoAssign.Text = "AutoAssign";
            this.ButtonAutoAssign.UseVisualStyleBackColor = true;
            // 
            // SearchText
            // 
            this.SearchText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SearchText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchText.Location = new System.Drawing.Point(6, 6);
            this.SearchText.Margin = new System.Windows.Forms.Padding(6);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(706, 44);
            this.SearchText.TabIndex = 5;
            // 
            // HideArchive
            // 
            this.HideArchive.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.HideArchive.AutoSize = true;
            this.HideArchive.Checked = true;
            this.HideArchive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HideArchive.Location = new System.Drawing.Point(724, 14);
            this.HideArchive.Margin = new System.Windows.Forms.Padding(6);
            this.HideArchive.Name = "HideArchive";
            this.HideArchive.Size = new System.Drawing.Size(166, 29);
            this.HideArchive.TabIndex = 6;
            this.HideArchive.Text = "Hide Archive";
            this.HideArchive.UseVisualStyleBackColor = true;
            // 
            // L1v_TlpMaster
            // 
            this.L1v_TlpMaster.AutoSize = true;
            this.L1v_TlpMaster.ColumnCount = 1;
            this.L1v_TlpMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v_TlpMaster.Controls.Add(this.L1v1L2h_TlpTop, 0, 0);
            this.L1v_TlpMaster.Controls.Add(this.L1v3L2h_TlpBottom, 0, 2);
            this.L1v_TlpMaster.Controls.Add(this.L1v2L2_OptionsPanel, 0, 1);
            this.L1v_TlpMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v_TlpMaster.Location = new System.Drawing.Point(0, 0);
            this.L1v_TlpMaster.Margin = new System.Windows.Forms.Padding(6);
            this.L1v_TlpMaster.Name = "L1v_TlpMaster";
            this.L1v_TlpMaster.RowCount = 3;
            this.L1v_TlpMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.L1v_TlpMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v_TlpMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.L1v_TlpMaster.Size = new System.Drawing.Size(908, 887);
            this.L1v_TlpMaster.TabIndex = 7;
            // 
            // L1v1L2h_TlpTop
            // 
            this.L1v1L2h_TlpTop.ColumnCount = 2;
            this.L1v1L2h_TlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v1L2h_TlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 178F));
            this.L1v1L2h_TlpTop.Controls.Add(this.SearchText, 0, 0);
            this.L1v1L2h_TlpTop.Controls.Add(this.HideArchive, 1, 0);
            this.L1v1L2h_TlpTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v1L2h_TlpTop.Location = new System.Drawing.Point(6, 6);
            this.L1v1L2h_TlpTop.Margin = new System.Windows.Forms.Padding(6);
            this.L1v1L2h_TlpTop.Name = "L1v1L2h_TlpTop";
            this.L1v1L2h_TlpTop.RowCount = 1;
            this.L1v1L2h_TlpTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v1L2h_TlpTop.Size = new System.Drawing.Size(896, 57);
            this.L1v1L2h_TlpTop.TabIndex = 6;
            // 
            // L1v3L2h_TlpBottom
            // 
            this.L1v3L2h_TlpBottom.ColumnCount = 6;
            this.L1v3L2h_TlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.L1v3L2h_TlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.L1v3L2h_TlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.L1v3L2h_TlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.L1v3L2h_TlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.L1v3L2h_TlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.L1v3L2h_TlpBottom.Controls.Add(this.ButtonOk, 1, 0);
            this.L1v3L2h_TlpBottom.Controls.Add(this.ButtonAutoAssign, 4, 0);
            this.L1v3L2h_TlpBottom.Controls.Add(this.ButtonCancel, 2, 0);
            this.L1v3L2h_TlpBottom.Controls.Add(this.ButtonNew, 3, 0);
            this.L1v3L2h_TlpBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v3L2h_TlpBottom.Location = new System.Drawing.Point(6, 816);
            this.L1v3L2h_TlpBottom.Margin = new System.Windows.Forms.Padding(6);
            this.L1v3L2h_TlpBottom.Name = "L1v3L2h_TlpBottom";
            this.L1v3L2h_TlpBottom.RowCount = 1;
            this.L1v3L2h_TlpBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v3L2h_TlpBottom.Size = new System.Drawing.Size(896, 65);
            this.L1v3L2h_TlpBottom.TabIndex = 7;
            // 
            // TagViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 887);
            this.Controls.Add(this.L1v_TlpMaster);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "TagViewer";
            this.Text = "Tags";
            this.L1v2L2_OptionsPanel.ResumeLayout(false);
            this.L1v_TlpMaster.ResumeLayout(false);
            this.L1v_TlpMaster.PerformLayout();
            this.L1v1L2h_TlpTop.ResumeLayout(false);
            this.L1v1L2h_TlpTop.PerformLayout();
            this.L1v3L2h_TlpBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal System.Windows.Forms.Panel L1v2L2_OptionsPanel;
        internal System.Windows.Forms.Button ButtonOk;
        internal System.Windows.Forms.Button ButtonCancel;
        internal System.Windows.Forms.Button ButtonNew;
        internal System.Windows.Forms.Button ButtonAutoAssign;
        internal System.Windows.Forms.TextBox SearchText;
        internal System.Windows.Forms.CheckBox HideArchive;
        internal System.Windows.Forms.TableLayoutPanel L1v_TlpMaster;
        internal System.Windows.Forms.TableLayoutPanel L1v1L2h_TlpTop;
        internal System.Windows.Forms.TableLayoutPanel L1v3L2h_TlpBottom;
        internal System.Windows.Forms.CheckBox TemplateCheckBox;
    }
}