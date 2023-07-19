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
            OptionsPanel = new System.Windows.Forms.Panel();
            OptionsPanel.KeyDown += new System.Windows.Forms.KeyEventHandler(OptionsPanel_KeyDown);
            OptionsPanel.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(OptionsPanel_PreviewKeyDown);
            button_ok = new System.Windows.Forms.Button();
            button_ok.Click += new EventHandler(button_ok_Click);
            button_cancel = new System.Windows.Forms.Button();
            button_cancel.Click += new EventHandler(button_cancel_Click);
            button_new = new System.Windows.Forms.Button();
            button_new.Click += new EventHandler(button_new_Click);
            button_autoassign = new System.Windows.Forms.Button();
            button_autoassign.Click += new EventHandler(button_autoassign_Click);
            TextBox1 = new System.Windows.Forms.TextBox();
            TextBox1.TextChanged += new EventHandler(TextBox1_TextChanged);
            TextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(TextBox1_KeyDown);
            TextBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(TextBox1_KeyUp);
            Hide_Archive = new System.Windows.Forms.CheckBox();
            Hide_Archive.CheckedChanged += new EventHandler(Hide_Archive_CheckedChanged);
            TableLayoutMaster = new System.Windows.Forms.TableLayoutPanel();
            TableLayoutTopPanel = new System.Windows.Forms.TableLayoutPanel();
            TableLayoutBottomPanel = new System.Windows.Forms.TableLayoutPanel();
            TableLayoutMaster.SuspendLayout();
            TableLayoutTopPanel.SuspendLayout();
            TableLayoutBottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // OptionsPanel
            // 
            OptionsPanel.AutoScroll = true;
            OptionsPanel.AutoSize = true;
            OptionsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            OptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            OptionsPanel.Location = new System.Drawing.Point(6, 42);
            OptionsPanel.Margin = new System.Windows.Forms.Padding(6);
            OptionsPanel.Name = "OptionsPanel";
            OptionsPanel.Size = new System.Drawing.Size(442, 373);
            OptionsPanel.TabIndex = 0;
            // 
            // button_ok
            // 
            button_ok.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button_ok.Location = new System.Drawing.Point(61, 3);
            button_ok.Name = "button_ok";
            button_ok.Size = new System.Drawing.Size(70, 28);
            button_ok.TabIndex = 1;
            button_ok.Text = "OK";
            button_ok.UseVisualStyleBackColor = true;
            // 
            // button_cancel
            // 
            button_cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button_cancel.Location = new System.Drawing.Point(146, 3);
            button_cancel.Name = "button_cancel";
            button_cancel.Size = new System.Drawing.Size(70, 28);
            button_cancel.TabIndex = 2;
            button_cancel.Text = "Cancel";
            button_cancel.UseVisualStyleBackColor = true;
            // 
            // button_new
            // 
            button_new.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button_new.Location = new System.Drawing.Point(231, 3);
            button_new.Name = "button_new";
            button_new.Size = new System.Drawing.Size(70, 28);
            button_new.TabIndex = 3;
            button_new.Text = "New";
            button_new.UseVisualStyleBackColor = true;
            // 
            // button_autoassign
            // 
            button_autoassign.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button_autoassign.Location = new System.Drawing.Point(316, 3);
            button_autoassign.Name = "button_autoassign";
            button_autoassign.Size = new System.Drawing.Size(70, 28);
            button_autoassign.TabIndex = 4;
            button_autoassign.Text = "AutoAssign";
            button_autoassign.UseVisualStyleBackColor = true;
            // 
            // TextBox1
            // 
            TextBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            TextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.0f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TextBox1.Location = new System.Drawing.Point(3, 3);
            TextBox1.Name = "TextBox1";
            TextBox1.Size = new System.Drawing.Size(317, 26);
            TextBox1.TabIndex = 5;
            // 
            // Hide_Archive
            // 
            Hide_Archive.Anchor = System.Windows.Forms.AnchorStyles.Right;
            Hide_Archive.AutoSize = true;
            Hide_Archive.Checked = true;
            Hide_Archive.CheckState = System.Windows.Forms.CheckState.Checked;
            Hide_Archive.Location = new System.Drawing.Point(358, 6);
            Hide_Archive.Name = "Hide_Archive";
            Hide_Archive.Size = new System.Drawing.Size(87, 17);
            Hide_Archive.TabIndex = 6;
            Hide_Archive.Text = "Hide Archive";
            Hide_Archive.UseVisualStyleBackColor = true;
            // 
            // TableLayoutMaster
            // 
            TableLayoutMaster.AutoSize = true;
            TableLayoutMaster.ColumnCount = 1;
            TableLayoutMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            TableLayoutMaster.Controls.Add(TableLayoutTopPanel, 0, 0);
            TableLayoutMaster.Controls.Add(TableLayoutBottomPanel, 0, 2);
            TableLayoutMaster.Controls.Add(OptionsPanel, 0, 1);
            TableLayoutMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            TableLayoutMaster.Location = new System.Drawing.Point(0, 0);
            TableLayoutMaster.Name = "TableLayoutMaster";
            TableLayoutMaster.RowCount = 3;
            TableLayoutMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36.0f));
            TableLayoutMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            TableLayoutMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0f));
            TableLayoutMaster.Size = new System.Drawing.Size(454, 461);
            TableLayoutMaster.TabIndex = 7;
            // 
            // TableLayoutTopPanel
            // 
            TableLayoutTopPanel.ColumnCount = 2;
            TableLayoutTopPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.4375f));
            TableLayoutTopPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.5625f));
            TableLayoutTopPanel.Controls.Add(TextBox1, 0, 0);
            TableLayoutTopPanel.Controls.Add(Hide_Archive, 1, 0);
            TableLayoutTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            TableLayoutTopPanel.Location = new System.Drawing.Point(3, 3);
            TableLayoutTopPanel.Name = "TableLayoutTopPanel";
            TableLayoutTopPanel.RowCount = 1;
            TableLayoutTopPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            TableLayoutTopPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0f));
            TableLayoutTopPanel.Size = new System.Drawing.Size(448, 30);
            TableLayoutTopPanel.TabIndex = 6;
            // 
            // TableLayoutBottomPanel
            // 
            TableLayoutBottomPanel.ColumnCount = 6;
            TableLayoutBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0f));
            TableLayoutBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85.0f));
            TableLayoutBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85.0f));
            TableLayoutBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85.0f));
            TableLayoutBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85.0f));
            TableLayoutBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0f));
            TableLayoutBottomPanel.Controls.Add(button_ok, 1, 0);
            TableLayoutBottomPanel.Controls.Add(button_autoassign, 4, 0);
            TableLayoutBottomPanel.Controls.Add(button_cancel, 2, 0);
            TableLayoutBottomPanel.Controls.Add(button_new, 3, 0);
            TableLayoutBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            TableLayoutBottomPanel.Location = new System.Drawing.Point(3, 424);
            TableLayoutBottomPanel.Name = "TableLayoutBottomPanel";
            TableLayoutBottomPanel.RowCount = 1;
            TableLayoutBottomPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            TableLayoutBottomPanel.Size = new System.Drawing.Size(448, 34);
            TableLayoutBottomPanel.TabIndex = 7;
            // 
            // TagViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(454, 461);
            Controls.Add(TableLayoutMaster);
            Name = "TagViewer";
            Text = "Tags";
            TableLayoutMaster.ResumeLayout(false);
            TableLayoutMaster.PerformLayout();
            TableLayoutTopPanel.ResumeLayout(false);
            TableLayoutTopPanel.PerformLayout();
            TableLayoutBottomPanel.ResumeLayout(false);
            KeyDown += new System.Windows.Forms.KeyEventHandler(TagViewer_KeyDown);
            ResumeLayout(false);
            PerformLayout();

        }

        internal System.Windows.Forms.Panel OptionsPanel;
        internal System.Windows.Forms.Button button_ok;
        internal System.Windows.Forms.Button button_cancel;
        internal System.Windows.Forms.Button button_new;
        internal System.Windows.Forms.Button button_autoassign;
        internal System.Windows.Forms.TextBox TextBox1;
        internal System.Windows.Forms.CheckBox Hide_Archive;
        internal System.Windows.Forms.TableLayoutPanel TableLayoutMaster;
        internal System.Windows.Forms.TableLayoutPanel TableLayoutTopPanel;
        internal System.Windows.Forms.TableLayoutPanel TableLayoutBottomPanel;
    }
}