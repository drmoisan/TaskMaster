namespace UtilitiesCS
{
    partial class MyBoxViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param _name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.L1Vertical = new System.Windows.Forms.TableLayoutPanel();
            this.L2Bottom = new System.Windows.Forms.TableLayoutPanel();
            this.Button2 = new System.Windows.Forms.Button();
            this.Button1 = new System.Windows.Forms.Button();
            this.L1v1L2h = new System.Windows.Forms.TableLayoutPanel();
            this.svg1 = new SVGControl.SVG();
            this.TextMessage = new System.Windows.Forms.TextBox();
            this.L1Vertical.SuspendLayout();
            this.L2Bottom.SuspendLayout();
            this.L1v1L2h.SuspendLayout();
            this.SuspendLayout();
            // 
            // L1Vertical
            // 
            this.L1Vertical.ColumnCount = 1;
            this.L1Vertical.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1Vertical.Controls.Add(this.L2Bottom, 0, 1);
            this.L1Vertical.Controls.Add(this.L1v1L2h, 0, 0);
            this.L1Vertical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1Vertical.Location = new System.Drawing.Point(0, 0);
            this.L1Vertical.Name = "L1Vertical";
            this.L1Vertical.RowCount = 2;
            this.L1Vertical.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1Vertical.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.L1Vertical.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.L1Vertical.Size = new System.Drawing.Size(245, 143);
            this.L1Vertical.TabIndex = 0;
            // 
            // L2Bottom
            // 
            this.L2Bottom.ColumnCount = 4;
            this.L2Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.L2Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F));
            this.L2Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F));
            this.L2Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.L2Bottom.Controls.Add(this.Button2, 2, 0);
            this.L2Bottom.Controls.Add(this.Button1, 1, 0);
            this.L2Bottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L2Bottom.Location = new System.Drawing.Point(3, 86);
            this.L2Bottom.Name = "L2Bottom";
            this.L2Bottom.RowCount = 1;
            this.L2Bottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L2Bottom.Size = new System.Drawing.Size(239, 54);
            this.L2Bottom.TabIndex = 0;
            // 
            // Button2
            // 
            this.Button2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Button2.Location = new System.Drawing.Point(122, 3);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(109, 48);
            this.Button2.TabIndex = 1;
            this.Button2.Text = "button2";
            this.Button2.UseVisualStyleBackColor = true;
            this.Button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Button1
            // 
            this.Button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Button1.Location = new System.Drawing.Point(7, 3);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(109, 48);
            this.Button1.TabIndex = 0;
            this.Button1.Text = "button1";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // L1v1L2h
            // 
            this.L1v1L2h.ColumnCount = 2;
            this.L1v1L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.L1v1L2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v1L2h.Controls.Add(this.svg1, 0, 0);
            this.L1v1L2h.Controls.Add(this.TextMessage, 1, 0);
            this.L1v1L2h.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1v1L2h.Location = new System.Drawing.Point(3, 3);
            this.L1v1L2h.Name = "L1v1L2h";
            this.L1v1L2h.RowCount = 1;
            this.L1v1L2h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1v1L2h.Size = new System.Drawing.Size(239, 77);
            this.L1v1L2h.TabIndex = 1;
            // 
            // svg1
            // 
            this.svg1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.svg1.ImagePath = "C:\\Users\\03311352\\source\\repos\\drmoisan\\TaskMaster\\UtilitiesCS\\Resources\\wb.svg";
            this.svg1.Location = new System.Drawing.Point(3, 3);
            this.svg1.Name = "svg1";
            this.svg1.Size = new System.Drawing.Size(34, 71);
            this.svg1.TabIndex = 0;
            // 
            // TextMessage
            // 
            this.TextMessage.BackColor = System.Drawing.SystemColors.Control;
            this.TextMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextMessage.Location = new System.Drawing.Point(43, 3);
            this.TextMessage.Multiline = true;
            this.TextMessage.Name = "TextMessage";
            this.TextMessage.Size = new System.Drawing.Size(193, 71);
            this.TextMessage.TabIndex = 1;
            this.TextMessage.Text = "Message Contents";
            // 
            // MyBoxViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(245, 143);
            this.Controls.Add(this.L1Vertical);
            this.Name = "MyBoxViewer";
            this.Text = "FormName";
            this.L1Vertical.ResumeLayout(false);
            this.L2Bottom.ResumeLayout(false);
            this.L1v1L2h.ResumeLayout(false);
            this.L1v1L2h.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel L1Vertical;
        private System.Windows.Forms.Button Button2;
        private System.Windows.Forms.Button Button1;
        private System.Windows.Forms.TableLayoutPanel L1v1L2h;
        private SVGControl.SVG svg1;
        private System.Windows.Forms.TextBox TextMessage;
        internal System.Windows.Forms.TableLayoutPanel L2Bottom;
    }
}