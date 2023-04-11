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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderNotFoundViewer));
            this.CreateFolder = new SVGControl.ButtonSVG();
            this.OpenFolder = new SVGControl.ButtonSVG();
            this.Cancel = new SVGControl.ButtonSVG();
            this.NoToAll = new SVGControl.ButtonSVG();
            this.FolderNameTxtBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // CreateFolder
            // 
            this.CreateFolder.Image = ((System.Drawing.Image)(resources.GetObject("CreateFolder.Image")));
            this.CreateFolder.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.CreateFolder.ImageSVG.ImagePath = "C:\\Users\\03311352\\source\\repos\\drmoisan\\TaskMaster\\UtilitiesCS\\Resources\\NewFolde" +
    "r.svg";
            this.CreateFolder.ImageSVG.Margin = new System.Windows.Forms.Padding(7, 0, 7, 15);
            this.CreateFolder.ImageSVG.Size = new System.Drawing.Size(112, 41);
            this.CreateFolder.Location = new System.Drawing.Point(14, 94);
            this.CreateFolder.Name = "CreateFolder";
            this.CreateFolder.Size = new System.Drawing.Size(126, 56);
            this.CreateFolder.TabIndex = 4;
            this.CreateFolder.Text = "    Create     Folder";
            this.CreateFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.CreateFolder.UseVisualStyleBackColor = true;
            this.CreateFolder.Click += new System.EventHandler(this.CreateFolder_Click);
            // 
            // OpenFolder
            // 
            this.OpenFolder.Image = ((System.Drawing.Image)(resources.GetObject("OpenFolder.Image")));
            this.OpenFolder.ImageSVG.ImagePath = "C:\\Users\\03311352\\source\\repos\\drmoisan\\TaskMaster\\UtilitiesCS\\Resources\\OpenFold" +
    "er.svg";
            this.OpenFolder.ImageSVG.Margin = new System.Windows.Forms.Padding(7, 0, 7, 15);
            this.OpenFolder.ImageSVG.Size = new System.Drawing.Size(112, 41);
            this.OpenFolder.Location = new System.Drawing.Point(146, 94);
            this.OpenFolder.Name = "OpenFolder";
            this.OpenFolder.Size = new System.Drawing.Size(126, 56);
            this.OpenFolder.TabIndex = 4;
            this.OpenFolder.Text = "   Open     Folder";
            this.OpenFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.OpenFolder.UseVisualStyleBackColor = true;
            this.OpenFolder.Click += new System.EventHandler(this.OpenFolder_Click);
            // 
            // Cancel
            // 
            this.Cancel.Image = ((System.Drawing.Image)(resources.GetObject("Cancel.Image")));
            this.Cancel.ImageSVG.ImagePath = "C:\\Users\\03311352\\source\\repos\\drmoisan\\TaskMaster\\UtilitiesCS\\Resources\\Cancel.s" +
    "vg";
            this.Cancel.ImageSVG.Margin = new System.Windows.Forms.Padding(7, 0, 7, 15);
            this.Cancel.ImageSVG.SaveRendering = false;
            this.Cancel.ImageSVG.Size = new System.Drawing.Size(112, 41);
            this.Cancel.Location = new System.Drawing.Point(278, 94);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(126, 56);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // NoToAll
            // 
            this.NoToAll.Image = ((System.Drawing.Image)(resources.GetObject("NoToAll.Image")));
            this.NoToAll.ImageSVG.ImagePath = "C:\\Users\\03311352\\source\\repos\\drmoisan\\TaskMaster\\UtilitiesCS\\Resources\\RepeatUn" +
    "tilFailure.svg";
            this.NoToAll.ImageSVG.Margin = new System.Windows.Forms.Padding(7, 0, 7, 15);
            this.NoToAll.ImageSVG.SaveRendering = false;
            this.NoToAll.ImageSVG.Size = new System.Drawing.Size(112, 41);
            this.NoToAll.Location = new System.Drawing.Point(410, 94);
            this.NoToAll.Name = "NoToAll";
            this.NoToAll.Size = new System.Drawing.Size(126, 56);
            this.NoToAll.TabIndex = 5;
            this.NoToAll.Text = "No To All";
            this.NoToAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.NoToAll.UseVisualStyleBackColor = true;
            this.NoToAll.Click += new System.EventHandler(this.NoToAll_Click);
            // 
            // FolderNameTxtBox
            // 
            this.FolderNameTxtBox.BackColor = System.Drawing.SystemColors.Control;
            this.FolderNameTxtBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FolderNameTxtBox.Location = new System.Drawing.Point(16, 23);
            this.FolderNameTxtBox.Multiline = true;
            this.FolderNameTxtBox.Name = "FolderNameTxtBox";
            this.FolderNameTxtBox.Size = new System.Drawing.Size(519, 59);
            this.FolderNameTxtBox.TabIndex = 6;
            this.FolderNameTxtBox.Text = "<FOLDER NAME>";
            // 
            // FolderNotFoundViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(548, 161);
            this.Controls.Add(this.FolderNameTxtBox);
            this.Controls.Add(this.NoToAll);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OpenFolder);
            this.Controls.Add(this.CreateFolder);
            this.Name = "FolderNotFoundViewer";
            this.Text = "Folder Not Found";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private SVGControl.ButtonSVG CreateFolder;
        private SVGControl.ButtonSVG OpenFolder;
        private SVGControl.ButtonSVG Cancel;
        private SVGControl.ButtonSVG NoToAll;
        private System.Windows.Forms.TextBox FolderNameTxtBox;
    }
}