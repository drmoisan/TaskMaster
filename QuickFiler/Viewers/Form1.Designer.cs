namespace QuickFiler.Viewers
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
            this.CboFolders = new System.Windows.Forms.ComboBox();
            this.BtnPopOut = new System.Windows.Forms.Button();
            this.buttonSVG1 = new SVGControl.ButtonSVG();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.CboFolders);
            this.panel1.Controls.Add(this.BtnPopOut);
            this.panel1.Controls.Add(this.buttonSVG1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 450);
            this.panel1.TabIndex = 0;
            // 
            // CboFolders
            // 
            this.CboFolders.BackColor = System.Drawing.Color.Black;
            this.CboFolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboFolders.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.CboFolders.FormattingEnabled = true;
            this.CboFolders.Location = new System.Drawing.Point(92, 258);
            this.CboFolders.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.CboFolders.Name = "CboFolders";
            this.CboFolders.Size = new System.Drawing.Size(652, 41);
            this.CboFolders.TabIndex = 7;
            // 
            // BtnPopOut
            // 
            this.BtnPopOut.BackColor = System.Drawing.Color.DimGray;
            this.BtnPopOut.Image = global::QuickFiler.Properties.Resources.ApplicationFlyout;
            this.BtnPopOut.Location = new System.Drawing.Point(92, 143);
            this.BtnPopOut.Margin = new System.Windows.Forms.Padding(0);
            this.BtnPopOut.Name = "BtnPopOut";
            this.BtnPopOut.Size = new System.Drawing.Size(100, 50);
            this.BtnPopOut.TabIndex = 3;
            this.BtnPopOut.UseVisualStyleBackColor = false;
            // 
            // buttonSVG1
            // 
            this.buttonSVG1.BackColor = System.Drawing.Color.DimGray;
            this.buttonSVG1.Image = ((System.Drawing.Image)(resources.GetObject("buttonSVG1.Image")));
            this.buttonSVG1.ImageSVG.ImagePath = "./QuickFiler/Resources/FlagDarkRed.svg";
            this.buttonSVG1.ImageSVG.Margin = new System.Windows.Forms.Padding(6);
            this.buttonSVG1.ImageSVG.SaveRendering = false;
            this.buttonSVG1.ImageSVG.Size = new System.Drawing.Size(88, 38);
            this.buttonSVG1.Location = new System.Drawing.Point(332, 155);
            this.buttonSVG1.Name = "buttonSVG1";
            this.buttonSVG1.Size = new System.Drawing.Size(100, 50);
            this.buttonSVG1.TabIndex = 0;
            this.buttonSVG1.UseVisualStyleBackColor = false;
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
        private SVGControl.ButtonSVG buttonSVG1;
        internal System.Windows.Forms.Button BtnPopOut;
        internal System.Windows.Forms.ComboBox CboFolders;
    }
}