using System;
using System.Diagnostics;

namespace QuickFiler.Test
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class Form1 : System.Windows.Forms.Form
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
        private System.ComponentModel.IContainer components = null;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            Button2 = new System.Windows.Forms.Button();
            Button2.Click += new EventHandler(Button2_Click);
            Button1 = new System.Windows.Forms.Button();
            Button1.Click += new EventHandler(Button1_Click);
            MainPanel = new System.Windows.Forms.Panel();
            MainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            TableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            ControlGroup3 = new QfcItemViewer();
            ControlGroup1 = new QfcItemViewer();
            ControlGroup2 = new QfcItemViewer();
            ButtonAdd = new System.Windows.Forms.Button();
            ButtonAdd.Click += new EventHandler(ButtonAdd_Click);
            TableLayoutPanel1.SuspendLayout();
            MainPanel.SuspendLayout();
            MainLayoutPanel.SuspendLayout();
            TableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // TableLayoutPanel1
            // 
            TableLayoutPanel1.AutoSize = true;
            TableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            TableLayoutPanel1.ColumnCount = 1;
            TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            TableLayoutPanel1.Controls.Add(ControlGroup3, 0, 1);
            TableLayoutPanel1.Controls.Add(ControlGroup1, 0, 0);
            TableLayoutPanel1.Controls.Add(ControlGroup2, 0, 2);
            TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            TableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            TableLayoutPanel1.Name = "TableLayoutPanel1";
            TableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
            TableLayoutPanel1.RowCount = 4;
            TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0f));
            TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0f));
            TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0f));
            TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            TableLayoutPanel1.Size = new System.Drawing.Size(1163, 350);
            TableLayoutPanel1.TabIndex = 0;
            // 
            // Button2
            // 
            Button2.Location = new System.Drawing.Point(597, 3);
            Button2.Name = "Button2";
            Button2.Padding = new System.Windows.Forms.Padding(6);
            Button2.Size = new System.Drawing.Size(91, 35);
            Button2.TabIndex = 3;
            Button2.Text = "Toggle";
            Button2.UseVisualStyleBackColor = true;
            // 
            // Button1
            // 
            Button1.Location = new System.Drawing.Point(497, 3);
            Button1.Name = "Button1";
            Button1.Size = new System.Drawing.Size(91, 35);
            Button1.TabIndex = 2;
            Button1.Text = "OK";
            Button1.UseVisualStyleBackColor = true;
            // 
            // MainPanel
            // 
            MainPanel.AutoScroll = true;
            MainPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            MainPanel.Controls.Add(TableLayoutPanel1);
            MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            MainPanel.Location = new System.Drawing.Point(3, 3);
            MainPanel.Name = "MainPanel";
            MainPanel.Size = new System.Drawing.Size(1182, 304);
            MainPanel.TabIndex = 1;
            // 
            // MainLayoutPanel
            // 
            MainLayoutPanel.ColumnCount = 1;
            MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            MainLayoutPanel.Controls.Add(MainPanel, 0, 0);
            MainLayoutPanel.Controls.Add(TableLayoutPanel2, 0, 1);
            MainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            MainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            MainLayoutPanel.Name = "MainLayoutPanel";
            MainLayoutPanel.RowCount = 2;
            MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64.0f));
            MainLayoutPanel.Size = new System.Drawing.Size(1188, 374);
            MainLayoutPanel.TabIndex = 2;
            // 
            // TableLayoutPanel2
            // 
            TableLayoutPanel2.ColumnCount = 4;
            TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0f));
            TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100.0f));
            TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100.0f));
            TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0f));
            TableLayoutPanel2.Controls.Add(ButtonAdd, 0, 0);
            TableLayoutPanel2.Controls.Add(Button2, 2, 0);
            TableLayoutPanel2.Controls.Add(Button1, 1, 0);
            TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            TableLayoutPanel2.Location = new System.Drawing.Point(0, 310);
            TableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            TableLayoutPanel2.Name = "TableLayoutPanel2";
            TableLayoutPanel2.RowCount = 1;
            TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            TableLayoutPanel2.Size = new System.Drawing.Size(1188, 64);
            TableLayoutPanel2.TabIndex = 2;
            // 
            // ControlGroup3
            // 
            ControlGroup3.AutoSize = true;
            ControlGroup3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            ControlGroup3.Dock = System.Windows.Forms.DockStyle.Fill;
            ControlGroup3.Location = new System.Drawing.Point(13, 123);
            ControlGroup3.Name = "ControlGroup3";
            ControlGroup3.Padding = new System.Windows.Forms.Padding(3);
            ControlGroup3.Size = new System.Drawing.Size(1137, 104);
            ControlGroup3.TabIndex = 2;
            // 
            // ControlGroup1
            // 
            ControlGroup1.AutoSize = true;
            ControlGroup1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            ControlGroup1.Dock = System.Windows.Forms.DockStyle.Fill;
            ControlGroup1.Location = new System.Drawing.Point(13, 13);
            ControlGroup1.Name = "ControlGroup1";
            ControlGroup1.Padding = new System.Windows.Forms.Padding(3);
            ControlGroup1.Size = new System.Drawing.Size(1137, 104);
            ControlGroup1.TabIndex = 0;
            // 
            // ControlGroup2
            // 
            ControlGroup2.AutoSize = true;
            ControlGroup2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            ControlGroup2.Dock = System.Windows.Forms.DockStyle.Fill;
            ControlGroup2.Location = new System.Drawing.Point(13, 233);
            ControlGroup2.Name = "ControlGroup2";
            ControlGroup2.Padding = new System.Windows.Forms.Padding(3);
            ControlGroup2.Size = new System.Drawing.Size(1137, 104);
            ControlGroup2.TabIndex = 1;
            // 
            // ButtonAdd
            // 
            ButtonAdd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            ButtonAdd.Location = new System.Drawing.Point(380, 3);
            ButtonAdd.Name = "ButtonAdd";
            ButtonAdd.Padding = new System.Windows.Forms.Padding(6);
            ButtonAdd.Size = new System.Drawing.Size(111, 49);
            ButtonAdd.TabIndex = 4;
            ButtonAdd.Text = "Add Control Group";
            ButtonAdd.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1188, 374);
            Controls.Add(MainLayoutPanel);
            Name = "Form1";
            Text = "Form1";
            TableLayoutPanel1.ResumeLayout(false);
            TableLayoutPanel1.PerformLayout();
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            MainLayoutPanel.ResumeLayout(false);
            TableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);

        }

        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel1;
        internal QfcItemViewer ControlGroup1;
        internal System.Windows.Forms.Button Button2;
        internal System.Windows.Forms.Button Button1;
        internal QfcItemViewer ControlGroup3;
        internal QfcItemViewer ControlGroup2;
        internal System.Windows.Forms.Panel MainPanel;
        internal System.Windows.Forms.TableLayoutPanel MainLayoutPanel;
        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel2;
        internal System.Windows.Forms.Button ButtonAdd;
    }
}