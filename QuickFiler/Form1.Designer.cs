namespace QuickFiler
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonSVG1 = new SVGControl.ButtonSVG();
            this.BtnPopOut = new System.Windows.Forms.Button();
            this.buttonSVG2 = new SVGControl.ButtonSVG();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonSVG2);
            this.panel1.Controls.Add(this.buttonSVG1);
            this.panel1.Controls.Add(this.BtnPopOut);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 450);
            this.panel1.TabIndex = 0;
            // 
            // buttonSVG1
            // 
            this.buttonSVG1.Image = ((System.Drawing.Image)(resources.GetObject("buttonSVG1.Image")));
            this.buttonSVG1.ImageSVG.ImagePath = "C:\\Users\\03311352\\source\\repos\\drmoisan\\TaskMaster\\QuickFiler\\Resources\\Applicati" +
    "onFlyout.svg";
            this.buttonSVG1.ImageSVG.Margin = new System.Windows.Forms.Padding(3);
            this.buttonSVG1.ImageSVG.SaveRendering = false;
            this.buttonSVG1.ImageSVG.Size = new System.Drawing.Size(94, 44);
            this.buttonSVG1.Location = new System.Drawing.Point(389, 107);
            this.buttonSVG1.Name = "buttonSVG1";
            this.buttonSVG1.Size = new System.Drawing.Size(100, 50);
            this.buttonSVG1.TabIndex = 4;
            this.buttonSVG1.UseVisualStyleBackColor = true;
            // 
            // BtnPopOut
            // 
            this.BtnPopOut.Image = global::QuickFiler.Properties.Resources.ApplicationFlyout;
            this.BtnPopOut.Location = new System.Drawing.Point(183, 107);
            this.BtnPopOut.Margin = new System.Windows.Forms.Padding(0);
            this.BtnPopOut.Name = "BtnPopOut";
            this.BtnPopOut.Size = new System.Drawing.Size(100, 50);
            this.BtnPopOut.TabIndex = 3;
            this.BtnPopOut.UseVisualStyleBackColor = true;
            // 
            // buttonSVG2
            // 
            this.buttonSVG2.Image = ((System.Drawing.Image)(resources.GetObject("buttonSVG2.Image")));
            this.buttonSVG2.ImageSVG.ImagePath = "C:\\Users\\03311352\\source\\repos\\drmoisan\\TaskMaster\\QuickFiler\\Resources\\FlagDarkR" +
    "ed.svg";
            this.buttonSVG2.ImageSVG.Margin = new System.Windows.Forms.Padding(3);
            this.buttonSVG2.ImageSVG.SaveRendering = false;
            this.buttonSVG2.ImageSVG.Size = new System.Drawing.Size(94, 44);
            this.buttonSVG2.Location = new System.Drawing.Point(350, 200);
            this.buttonSVG2.Name = "buttonSVG2";
            this.buttonSVG2.Size = new System.Drawing.Size(100, 50);
            this.buttonSVG2.TabIndex = 5;
            this.buttonSVG2.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.Button BtnPopOut;
        private SVGControl.ButtonSVG buttonSVG1;
        private SVGControl.ButtonSVG buttonSVG2;
    }
}