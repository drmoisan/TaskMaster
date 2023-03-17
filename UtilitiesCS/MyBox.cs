//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace UtilitiesCS
//{
//    public class MyBox : Form
//    {

//        public MyBox(string title, string message, Dictionary<string, Delegate> map)
//        {
//            InitializeComponent();
//        }

//        private System.ComponentModel.IContainer components = null;
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        private void InitializeComponent()
//        {
//            this.L1Vertical = new System.Windows.Forms.TableLayoutPanel();
//            this.L2Bottom = new System.Windows.Forms.TableLayoutPanel();
//            this.Button1 = new System.Windows.Forms.Button();
//            this.Button2 = new System.Windows.Forms.Button();
//            this.TextMessage = new System.Windows.Forms.TextBox();
//            this.L1Vertical.SuspendLayout();
//            this.L2Bottom.SuspendLayout();
//            this.SuspendLayout();
//            // 
//            // L1Vertical
//            // 
//            this.L1Vertical.ColumnCount = 1;
//            this.L1Vertical.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
//            this.L1Vertical.Controls.Add(this.L2Bottom, 0, 1);
//            this.L1Vertical.Controls.Add(this.TextMessage, 0, 0);
//            this.L1Vertical.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.L1Vertical.Location = new System.Drawing.Point(0, 0);
//            this.L1Vertical.Name = "L1Vertical";
//            this.L1Vertical.RowCount = 2;
//            this.L1Vertical.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
//            this.L1Vertical.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
//            this.L1Vertical.Size = new System.Drawing.Size(241, 143);
//            this.L1Vertical.TabIndex = 0;
//            // 
//            // L2Bottom
//            // 
//            this.L2Bottom.ColumnCount = 2;
//            this.L2Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F));
//            this.L2Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F));
//            this.L2Bottom.Controls.Add(this.Button2, 1, 0);
//            this.L2Bottom.Controls.Add(this.Button1, 0, 0);
//            this.L2Bottom.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.L2Bottom.Location = new System.Drawing.Point(3, 86);
//            this.L2Bottom.Name = "L2Bottom";
//            this.L2Bottom.RowCount = 1;
//            this.L2Bottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
//            this.L2Bottom.Size = new System.Drawing.Size(235, 54);
//            this.L2Bottom.TabIndex = 0;
//            // 
//            // Button1
//            // 
//            this.Button1.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.Button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
//            this.Button1.Location = new System.Drawing.Point(3, 3);
//            this.Button1.Name = "Button1";
//            this.Button1.Size = new System.Drawing.Size(109, 48);
//            this.Button1.TabIndex = 0;
//            this.Button1.Text = "button1";
//            this.Button1.UseVisualStyleBackColor = true;
//            this.Button1.Click += new System.EventHandler(this.Button1_Click);
//            // 
//            // Button2
//            // 
//            this.Button2.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.Button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
//            this.Button2.Location = new System.Drawing.Point(118, 3);
//            this.Button2.Name = "Button2";
//            this.Button2.Size = new System.Drawing.Size(114, 48);
//            this.Button2.TabIndex = 1;
//            this.Button2.Text = "button2";
//            this.Button2.UseVisualStyleBackColor = true;
//            // 
//            // TextMessage
//            // 
//            this.TextMessage.BackColor = System.Drawing.SystemColors.Menu;
//            this.TextMessage.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.TextMessage.Location = new System.Drawing.Point(3, 3);
//            this.TextMessage.Multiline = true;
//            this.TextMessage.Name = "TextMessage";
//            this.TextMessage.Size = new System.Drawing.Size(235, 77);
//            this.TextMessage.TabIndex = 1;
//            // 
//            // MyBox
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.ClientSize = new System.Drawing.Size(241, 143);
//            this.Controls.Add(this.L1Vertical);
//            this.Name = "MyBox";
//            this.Text = "FormName";
//            this.L1Vertical.ResumeLayout(false);
//            this.L1Vertical.PerformLayout();
//            this.L2Bottom.ResumeLayout(false);
//            this.ResumeLayout(false);

//        }



//        private System.Windows.Forms.TableLayoutPanel L1Vertical;
//        private System.Windows.Forms.TableLayoutPanel L2Bottom;
//        private System.Windows.Forms.Button Button2;
//        private System.Windows.Forms.Button Button1;
//        private System.Windows.Forms.TextBox TextMessage;
//    }

//    //private void Button1_Click(object sender, EventArgs e)
//    //{
//    //    this.Dialogue
//    //}

//}