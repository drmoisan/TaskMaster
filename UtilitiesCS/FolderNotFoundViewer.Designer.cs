namespace UtilitiesCS
{
    partial class FolderNotFoundViewer
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
            SVGControl.SvgImageSelector svgImageSelector1 = new SVGControl.SvgImageSelector();
            SVGControl.SvgImageSelector svgImageSelector2 = new SVGControl.SvgImageSelector();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.buttonSVG1 = new SVGControl.ButtonSVG();
            this.buttonSVG2 = new SVGControl.ButtonSVG();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Image = global::UtilitiesCS.Properties.Resources.ExceptionPublic;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(36, 369);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(252, 108);
            this.button1.TabIndex = 0;
            this.button1.Text = "Create New Folder";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(300, 369);
            this.button2.Margin = new System.Windows.Forms.Padding(6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(252, 108);
            this.button2.TabIndex = 1;
            this.button2.Text = "Find Existing Folder";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(564, 369);
            this.button3.Margin = new System.Windows.Forms.Padding(6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(252, 108);
            this.button3.TabIndex = 2;
            this.button3.Text = "Cancel";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(828, 369);
            this.button4.Margin = new System.Windows.Forms.Padding(6);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(252, 108);
            this.button4.TabIndex = 3;
            this.button4.Text = "No To All";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // buttonSVG1
            // 
            svgImageSelector1.AutoSize = SVGControl.AutoSize.Disabled;
            svgImageSelector1.ImagePath = null;
            svgImageSelector1.Margin = new System.Windows.Forms.Padding(3);
            svgImageSelector1.Size = new System.Drawing.Size(144, 63);
            this.buttonSVG1.ImageSVG = svgImageSelector1;
            this.buttonSVG1.Location = new System.Drawing.Point(36, 207);
            this.buttonSVG1.Name = "buttonSVG1";
            this.buttonSVG1.Size = new System.Drawing.Size(252, 108);
            this.buttonSVG1.TabIndex = 4;
            this.buttonSVG1.Text = "Create New Folder";
            this.buttonSVG1.UseVisualStyleBackColor = true;
            // 
            // buttonSVG2
            // 
            svgImageSelector2.AutoSize = SVGControl.AutoSize.Disabled;
            svgImageSelector2.ImagePath = null;
            svgImageSelector2.Margin = new System.Windows.Forms.Padding(3);
            svgImageSelector2.Size = new System.Drawing.Size(144, 63);
            this.buttonSVG2.ImageSVG = svgImageSelector2;
            this.buttonSVG2.Location = new System.Drawing.Point(300, 207);
            this.buttonSVG2.Name = "buttonSVG2";
            this.buttonSVG2.Size = new System.Drawing.Size(252, 108);
            this.buttonSVG2.TabIndex = 5;
            this.buttonSVG2.Text = "Create New Folder";
            this.buttonSVG2.UseVisualStyleBackColor = true;
            // 
            // FolderNotFoundViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1122, 525);
            this.Controls.Add(this.buttonSVG2);
            this.Controls.Add(this.buttonSVG1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "FolderNotFoundViewer";
            this.Text = "Folder Not Found";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private SVGControl.ButtonSVG buttonSVG1;
        private SVGControl.ButtonSVG buttonSVG2;
    }
}