namespace UtilitiesCS.Test
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            SVGControl.SVGImage svgImage1 = new SVGControl.SVGImage();
            this.buttonSVG1 = new SVGControl.ButtonSVG();
            this.SuspendLayout();
            // 
            // buttonSVG1
            // 
            this.buttonSVG1.Image = ((System.Drawing.Image)(resources.GetObject("buttonSVG1.Image")));
            svgImage1.ImagePath = "C:\\Users\\03311352\\source\\repos\\drmoisan\\TaskMaster\\UtilitiesCS.Test\\Resources\\Abs" +
    "tractCube.svg";
            svgImage1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            svgImage1.Size = new System.Drawing.Size(144, 56);
            this.buttonSVG1.ImageSVG = svgImage1;
            this.buttonSVG1.Location = new System.Drawing.Point(83, 81);
            this.buttonSVG1.Name = "buttonSVG1";
            this.buttonSVG1.Size = new System.Drawing.Size(150, 69);
            this.buttonSVG1.TabIndex = 0;
            this.buttonSVG1.Text = "buttonSVG1";
            this.buttonSVG1.UseVisualStyleBackColor = true;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(452, 290);
            this.Controls.Add(this.buttonSVG1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.ResumeLayout(false);

        }


        #endregion

        private SVGControl.ButtonSVG buttonSVG1;
    }
}