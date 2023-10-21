namespace QuickFiler.Test
{
    public partial class Form1 : System.Windows.Forms.Form
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
            this.TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ControlGroup3 = new QuickFiler.ItemViewer();
            this.ControlGroup1 = new QuickFiler.ItemViewer();
            this.ControlGroup2 = new QuickFiler.ItemViewer();
            this.Button2 = new System.Windows.Forms.Button();
            this.Button1 = new System.Windows.Forms.Button();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.MainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.TableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonAdd = new System.Windows.Forms.Button();
            this.TableLayoutPanel1.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.MainLayoutPanel.SuspendLayout();
            this.TableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TableLayoutPanel1
            // 
            this.TableLayoutPanel1.AutoSize = true;
            this.TableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableLayoutPanel1.ColumnCount = 1;
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel1.Controls.Add(this.ControlGroup3, 0, 1);
            this.TableLayoutPanel1.Controls.Add(this.ControlGroup1, 0, 0);
            this.TableLayoutPanel1.Controls.Add(this.ControlGroup2, 0, 2);
            this.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.TableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.TableLayoutPanel1.Name = "TableLayoutPanel1";
            this.TableLayoutPanel1.Padding = new System.Windows.Forms.Padding(20, 19, 20, 19);
            this.TableLayoutPanel1.RowCount = 4;
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 212F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 212F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 212F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel1.Size = new System.Drawing.Size(2328, 674);
            this.TableLayoutPanel1.TabIndex = 0;
            // 
            // ControlGroup3
            // 
            this.ControlGroup3.AutoSize = true;
            this.ControlGroup3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ControlGroup3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ControlGroup3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ControlGroup3.Location = new System.Drawing.Point(32, 243);
            this.ControlGroup3.Margin = new System.Windows.Forms.Padding(12);
            this.ControlGroup3.MinimumSize = new System.Drawing.Size(3722, 350);
            this.ControlGroup3.Name = "ControlGroup3";
            this.ControlGroup3.Padding = new System.Windows.Forms.Padding(6);
            this.ControlGroup3.Size = new System.Drawing.Size(3722, 350);
            this.ControlGroup3.TabIndex = 2;
            // 
            // ControlGroup1
            // 
            this.ControlGroup1.AutoSize = true;
            this.ControlGroup1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ControlGroup1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ControlGroup1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ControlGroup1.Location = new System.Drawing.Point(32, 31);
            this.ControlGroup1.Margin = new System.Windows.Forms.Padding(12);
            this.ControlGroup1.MinimumSize = new System.Drawing.Size(3722, 350);
            this.ControlGroup1.Name = "ControlGroup1";
            this.ControlGroup1.Padding = new System.Windows.Forms.Padding(6);
            this.ControlGroup1.Size = new System.Drawing.Size(3722, 350);
            this.ControlGroup1.TabIndex = 0;
            // 
            // ControlGroup2
            // 
            this.ControlGroup2.AutoSize = true;
            this.ControlGroup2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ControlGroup2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ControlGroup2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ControlGroup2.Location = new System.Drawing.Point(32, 455);
            this.ControlGroup2.Margin = new System.Windows.Forms.Padding(12);
            this.ControlGroup2.MinimumSize = new System.Drawing.Size(3722, 350);
            this.ControlGroup2.Name = "ControlGroup2";
            this.ControlGroup2.Padding = new System.Windows.Forms.Padding(6);
            this.ControlGroup2.Size = new System.Drawing.Size(3722, 350);
            this.ControlGroup2.TabIndex = 1;
            // 
            // Button2
            // 
            this.Button2.Location = new System.Drawing.Point(1194, 6);
            this.Button2.Margin = new System.Windows.Forms.Padding(6);
            this.Button2.Name = "Button2";
            this.Button2.Padding = new System.Windows.Forms.Padding(12);
            this.Button2.Size = new System.Drawing.Size(182, 67);
            this.Button2.TabIndex = 3;
            this.Button2.Text = "Toggle";
            this.Button2.UseVisualStyleBackColor = true;
            this.Button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Button1
            // 
            this.Button1.Location = new System.Drawing.Point(994, 6);
            this.Button1.Margin = new System.Windows.Forms.Padding(6);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(182, 67);
            this.Button1.TabIndex = 2;
            this.Button1.Text = "OK";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // MainPanel
            // 
            this.MainPanel.AutoScroll = true;
            this.MainPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MainPanel.Controls.Add(this.TableLayoutPanel1);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(6, 6);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(6);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(2364, 584);
            this.MainPanel.TabIndex = 1;
            // 
            // MainLayoutPanel
            // 
            this.MainLayoutPanel.ColumnCount = 1;
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.MainLayoutPanel.Controls.Add(this.MainPanel, 0, 0);
            this.MainLayoutPanel.Controls.Add(this.TableLayoutPanel2, 0, 1);
            this.MainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MainLayoutPanel.Margin = new System.Windows.Forms.Padding(6);
            this.MainLayoutPanel.Name = "MainLayoutPanel";
            this.MainLayoutPanel.RowCount = 2;
            this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.MainLayoutPanel.Size = new System.Drawing.Size(2376, 719);
            this.MainLayoutPanel.TabIndex = 2;
            // 
            // TableLayoutPanel2
            // 
            this.TableLayoutPanel2.ColumnCount = 4;
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableLayoutPanel2.Controls.Add(this.ButtonAdd, 0, 0);
            this.TableLayoutPanel2.Controls.Add(this.Button2, 2, 0);
            this.TableLayoutPanel2.Controls.Add(this.Button1, 1, 0);
            this.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel2.Location = new System.Drawing.Point(0, 596);
            this.TableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.TableLayoutPanel2.Name = "TableLayoutPanel2";
            this.TableLayoutPanel2.RowCount = 1;
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel2.Size = new System.Drawing.Size(2376, 123);
            this.TableLayoutPanel2.TabIndex = 2;
            // 
            // ButtonAdd
            // 
            this.ButtonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonAdd.Location = new System.Drawing.Point(760, 6);
            this.ButtonAdd.Margin = new System.Windows.Forms.Padding(6);
            this.ButtonAdd.Name = "ButtonAdd";
            this.ButtonAdd.Padding = new System.Windows.Forms.Padding(12);
            this.ButtonAdd.Size = new System.Drawing.Size(222, 94);
            this.ButtonAdd.TabIndex = 4;
            this.ButtonAdd.Text = "Add Control Group";
            this.ButtonAdd.UseVisualStyleBackColor = true;
            this.ButtonAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2376, 719);
            this.Controls.Add(this.MainLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "Form1";
            this.Text = "Form1";
            this.TableLayoutPanel1.ResumeLayout(false);
            this.TableLayoutPanel1.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.MainLayoutPanel.ResumeLayout(false);
            this.TableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel1;
        internal ItemViewer ControlGroup1;
        internal System.Windows.Forms.Button Button2;
        internal System.Windows.Forms.Button Button1;
        internal ItemViewer ControlGroup3;
        internal ItemViewer ControlGroup2;
        internal System.Windows.Forms.Panel MainPanel;
        internal System.Windows.Forms.TableLayoutPanel MainLayoutPanel;
        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel2;
        internal System.Windows.Forms.Button ButtonAdd;
    }
}