namespace QuickFiler
{
    partial class EfcViewer
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
            this.Tlp = new System.Windows.Forms.TableLayoutPanel();
            this.SearchText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnDelItem = new System.Windows.Forms.Button();
            this.FolderListBox = new System.Windows.Forms.ListBox();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.Refresh = new System.Windows.Forms.Button();
            this.Create = new System.Windows.Forms.Button();
            this.SaveAttachments = new System.Windows.Forms.CheckBox();
            this.SaveEmail = new System.Windows.Forms.CheckBox();
            this.SavePictures = new System.Windows.Forms.CheckBox();
            this.MoveConversation = new System.Windows.Forms.CheckBox();
            this.Tlp.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tlp
            // 
            this.Tlp.ColumnCount = 10;
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 254F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00063F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 329F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 329F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 329F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 329F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.99937F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 205F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.Tlp.Controls.Add(this.SearchText, 1, 2);
            this.Tlp.Controls.Add(this.label1, 1, 1);
            this.Tlp.Controls.Add(this.label2, 1, 4);
            this.Tlp.Controls.Add(this.BtnDelItem, 8, 2);
            this.Tlp.Controls.Add(this.FolderListBox, 1, 5);
            this.Tlp.Controls.Add(this.Ok, 3, 7);
            this.Tlp.Controls.Add(this.Cancel, 4, 7);
            this.Tlp.Controls.Add(this.Refresh, 5, 7);
            this.Tlp.Controls.Add(this.Create, 6, 7);
            this.Tlp.Controls.Add(this.SaveAttachments, 1, 7);
            this.Tlp.Controls.Add(this.SaveEmail, 1, 8);
            this.Tlp.Controls.Add(this.SavePictures, 1, 9);
            this.Tlp.Controls.Add(this.MoveConversation, 1, 10);
            this.Tlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tlp.Location = new System.Drawing.Point(0, 0);
            this.Tlp.Margin = new System.Windows.Forms.Padding(5);
            this.Tlp.Name = "Tlp";
            this.Tlp.RowCount = 12;
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 59F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.Tlp.Size = new System.Drawing.Size(1847, 1334);
            this.Tlp.TabIndex = 0;
            // 
            // SearchText
            // 
            this.Tlp.SetColumnSpan(this.SearchText, 7);
            this.SearchText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SearchText.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchText.Location = new System.Drawing.Point(35, 104);
            this.SearchText.Margin = new System.Windows.Forms.Padding(5);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(1563, 49);
            this.SearchText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.Tlp.SetColumnSpan(this.label1, 7);
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(35, 57);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(277, 37);
            this.label1.TabIndex = 1;
            this.label1.Text = "Input Search Text:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.Tlp.SetColumnSpan(this.label2, 7);
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(35, 215);
            this.label2.Margin = new System.Windows.Forms.Padding(5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(264, 37);
            this.label2.TabIndex = 2;
            this.label2.Text = "Matched Folders:";
            // 
            // BtnDelItem
            // 
            this.BtnDelItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnDelItem.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnDelItem.BackColor = System.Drawing.SystemColors.Control;
            this.BtnDelItem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.BtnDelItem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            this.BtnDelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnDelItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.BtnDelItem.Image = global::QuickFiler.Properties.Resources.Delete;
            this.BtnDelItem.Location = new System.Drawing.Point(1645, 104);
            this.BtnDelItem.Margin = new System.Windows.Forms.Padding(5);
            this.BtnDelItem.Name = "BtnDelItem";
            this.BtnDelItem.Size = new System.Drawing.Size(158, 49);
            this.BtnDelItem.TabIndex = 10;
            this.BtnDelItem.TabStop = false;
            this.BtnDelItem.UseVisualStyleBackColor = true;
            // 
            // FolderListBox
            // 
            this.Tlp.SetColumnSpan(this.FolderListBox, 8);
            this.FolderListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FolderListBox.FormattingEnabled = true;
            this.FolderListBox.ItemHeight = 37;
            this.FolderListBox.Location = new System.Drawing.Point(35, 262);
            this.FolderListBox.Margin = new System.Windows.Forms.Padding(5);
            this.FolderListBox.Name = "FolderListBox";
            this.FolderListBox.Size = new System.Drawing.Size(1768, 827);
            this.FolderListBox.TabIndex = 11;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Image = global::QuickFiler.Properties.Resources.Run;
            this.Ok.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Ok.Location = new System.Drawing.Point(301, 1162);
            this.Ok.Margin = new System.Windows.Forms.Padding(15, 14, 15, 14);
            this.Ok.Name = "Ok";
            this.Tlp.SetRowSpan(this.Ok, 4);
            this.Ok.Size = new System.Drawing.Size(299, 103);
            this.Ok.TabIndex = 12;
            this.Ok.Text = "OK";
            this.Ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Ok.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.Image = global::QuickFiler.Properties.Resources.Cancel;
            this.Cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Cancel.Location = new System.Drawing.Point(630, 1162);
            this.Cancel.Margin = new System.Windows.Forms.Padding(15, 14, 15, 14);
            this.Cancel.Name = "Cancel";
            this.Tlp.SetRowSpan(this.Cancel, 4);
            this.Cancel.Size = new System.Drawing.Size(299, 103);
            this.Cancel.TabIndex = 12;
            this.Cancel.Text = "Cancel";
            this.Cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Refresh
            // 
            this.Refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Refresh.Image = global::QuickFiler.Properties.Resources.QuickRefresh1;
            this.Refresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Refresh.Location = new System.Drawing.Point(959, 1162);
            this.Refresh.Margin = new System.Windows.Forms.Padding(15, 14, 15, 14);
            this.Refresh.Name = "Refresh";
            this.Tlp.SetRowSpan(this.Refresh, 4);
            this.Refresh.Size = new System.Drawing.Size(299, 103);
            this.Refresh.TabIndex = 12;
            this.Refresh.Text = "Refresh\r\nPredicted";
            this.Refresh.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Refresh.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.Refresh.UseVisualStyleBackColor = true;
            // 
            // Create
            // 
            this.Create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Create.Image = global::QuickFiler.Properties.Resources.NewFolder1;
            this.Create.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Create.Location = new System.Drawing.Point(1288, 1162);
            this.Create.Margin = new System.Windows.Forms.Padding(15, 14, 15, 14);
            this.Create.Name = "Create";
            this.Tlp.SetRowSpan(this.Create, 4);
            this.Create.Size = new System.Drawing.Size(299, 103);
            this.Create.TabIndex = 12;
            this.Create.Text = "Create\r\nFolder";
            this.Create.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Create.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.Create.UseVisualStyleBackColor = true;
            // 
            // SaveAttachments
            // 
            this.SaveAttachments.AutoSize = true;
            this.SaveAttachments.Location = new System.Drawing.Point(33, 1127);
            this.SaveAttachments.Name = "SaveAttachments";
            this.SaveAttachments.Size = new System.Drawing.Size(228, 39);
            this.SaveAttachments.TabIndex = 13;
            this.SaveAttachments.Text = "Attachments";
            this.SaveAttachments.UseVisualStyleBackColor = true;
            // 
            // SaveEmail
            // 
            this.SaveEmail.AutoSize = true;
            this.SaveEmail.Location = new System.Drawing.Point(33, 1172);
            this.SaveEmail.Name = "SaveEmail";
            this.SaveEmail.Size = new System.Drawing.Size(129, 39);
            this.SaveEmail.TabIndex = 14;
            this.SaveEmail.Text = "Email";
            this.SaveEmail.UseVisualStyleBackColor = true;
            // 
            // SavePictures
            // 
            this.SavePictures.AutoSize = true;
            this.SavePictures.Location = new System.Drawing.Point(33, 1217);
            this.SavePictures.Name = "SavePictures";
            this.SavePictures.Size = new System.Drawing.Size(164, 39);
            this.SavePictures.TabIndex = 15;
            this.SavePictures.Text = "Pictures";
            this.SavePictures.UseVisualStyleBackColor = true;
            // 
            // MoveConversation
            // 
            this.MoveConversation.AutoSize = true;
            this.MoveConversation.Location = new System.Drawing.Point(33, 1262);
            this.MoveConversation.Name = "MoveConversation";
            this.MoveConversation.Size = new System.Drawing.Size(237, 39);
            this.MoveConversation.TabIndex = 16;
            this.MoveConversation.Text = "Conversation";
            this.MoveConversation.UseVisualStyleBackColor = true;
            // 
            // EfcViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(19F, 37F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1847, 1334);
            this.Controls.Add(this.Tlp);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MinimumSize = new System.Drawing.Size(1873, 635);
            this.Name = "EfcViewer";
            this.Text = "Sort Email To Folder";
            this.Tlp.ResumeLayout(false);
            this.Tlp.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel Tlp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.Button BtnDelItem;
        internal System.Windows.Forms.CheckBox SaveAttachments;
        internal System.Windows.Forms.CheckBox SaveEmail;
        internal System.Windows.Forms.CheckBox SavePictures;
        internal System.Windows.Forms.CheckBox MoveConversation;
        internal System.Windows.Forms.Button Ok;
        internal System.Windows.Forms.Button Cancel;
        internal System.Windows.Forms.Button Refresh;
        internal System.Windows.Forms.Button Create;
        internal System.Windows.Forms.ListBox FolderListBox;
        internal System.Windows.Forms.TextBox SearchText;
    }
}