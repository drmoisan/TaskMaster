using System;

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
            this.L0vh_TLP = new System.Windows.Forms.TableLayoutPanel();
            this.Tlp = new System.Windows.Forms.TableLayoutPanel();
            this.LblAcTrash = new System.Windows.Forms.Label();
            this.LblAcFolderList = new System.Windows.Forms.Label();
            this.LblAcSearch = new System.Windows.Forms.Label();
            this.SearchText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnDelItem = new System.Windows.Forms.Button();
            this.FolderListBox = new System.Windows.Forms.ListBox();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.RefreshPredicted = new System.Windows.Forms.Button();
            this.NewFolder = new System.Windows.Forms.Button();
            this.SaveAttachments = new System.Windows.Forms.CheckBox();
            this.SaveEmail = new System.Windows.Forms.CheckBox();
            this.SavePictures = new System.Windows.Forms.CheckBox();
            this.MoveConversation = new System.Windows.Forms.CheckBox();
            this.LblAcAttachments = new System.Windows.Forms.Label();
            this.LblAcEmail = new System.Windows.Forms.Label();
            this.LblAcPictures = new System.Windows.Forms.Label();
            this.LblAcConversation = new System.Windows.Forms.Label();
            this.LblAcOk = new System.Windows.Forms.Label();
            this.LblAcCancel = new System.Windows.Forms.Label();
            this.LblAcRefresh = new System.Windows.Forms.Label();
            this.LblAcNewFolder = new System.Windows.Forms.Label();
            this.ItemViewer = new QuickFiler.ItemViewer();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.OptionsStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.moveOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.attachmentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.conversationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.picturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.categoryOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.L0vh_TLP.SuspendLayout();
            this.Tlp.SuspendLayout();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // L0vh_TLP
            // 
            this.L0vh_TLP.ColumnCount = 3;
            this.L0vh_TLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.L0vh_TLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L0vh_TLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.L0vh_TLP.Controls.Add(this.Tlp, 1, 2);
            this.L0vh_TLP.Controls.Add(this.ItemViewer, 1, 1);
            this.L0vh_TLP.Controls.Add(this.MenuStrip, 1, 0);
            this.L0vh_TLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L0vh_TLP.Location = new System.Drawing.Point(0, 0);
            this.L0vh_TLP.Name = "L0vh_TLP";
            this.L0vh_TLP.RowCount = 4;
            this.L0vh_TLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.L0vh_TLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1030F));
            this.L0vh_TLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L0vh_TLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.L0vh_TLP.Size = new System.Drawing.Size(2084, 1824);
            this.L0vh_TLP.TabIndex = 0;
            // 
            // Tlp
            // 
            this.Tlp.ColumnCount = 15;
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 254F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00063F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 329F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 329F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 329F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 329F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.99937F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 173F));
            this.Tlp.Controls.Add(this.LblAcTrash, 13, 1);
            this.Tlp.Controls.Add(this.LblAcFolderList, 0, 4);
            this.Tlp.Controls.Add(this.LblAcSearch, 0, 1);
            this.Tlp.Controls.Add(this.SearchText, 1, 1);
            this.Tlp.Controls.Add(this.label1, 1, 0);
            this.Tlp.Controls.Add(this.label2, 1, 3);
            this.Tlp.Controls.Add(this.BtnDelItem, 14, 1);
            this.Tlp.Controls.Add(this.FolderListBox, 1, 4);
            this.Tlp.Controls.Add(this.Ok, 4, 6);
            this.Tlp.Controls.Add(this.Cancel, 6, 6);
            this.Tlp.Controls.Add(this.RefreshPredicted, 8, 6);
            this.Tlp.Controls.Add(this.NewFolder, 10, 6);
            this.Tlp.Controls.Add(this.SaveAttachments, 1, 6);
            this.Tlp.Controls.Add(this.SaveEmail, 1, 7);
            this.Tlp.Controls.Add(this.SavePictures, 1, 8);
            this.Tlp.Controls.Add(this.MoveConversation, 1, 9);
            this.Tlp.Controls.Add(this.LblAcAttachments, 0, 6);
            this.Tlp.Controls.Add(this.LblAcEmail, 0, 7);
            this.Tlp.Controls.Add(this.LblAcPictures, 0, 8);
            this.Tlp.Controls.Add(this.LblAcConversation, 0, 9);
            this.Tlp.Controls.Add(this.LblAcOk, 3, 7);
            this.Tlp.Controls.Add(this.LblAcCancel, 5, 7);
            this.Tlp.Controls.Add(this.LblAcRefresh, 7, 7);
            this.Tlp.Controls.Add(this.LblAcNewFolder, 9, 7);
            this.Tlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tlp.Location = new System.Drawing.Point(35, 1085);
            this.Tlp.Margin = new System.Windows.Forms.Padding(5);
            this.Tlp.Name = "Tlp";
            this.Tlp.RowCount = 10;
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
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.Tlp.Size = new System.Drawing.Size(2014, 704);
            this.Tlp.TabIndex = 1;
            // 
            // LblAcTrash
            // 
            this.LblAcTrash.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LblAcTrash.AutoSize = true;
            this.LblAcTrash.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcTrash.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcTrash.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcTrash.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcTrash.Location = new System.Drawing.Point(1805, 82);
            this.LblAcTrash.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcTrash.Name = "LblAcTrash";
            this.LblAcTrash.Size = new System.Drawing.Size(34, 33);
            this.LblAcTrash.TabIndex = 19;
            this.LblAcTrash.Text = "T";
            // 
            // LblAcFolderList
            // 
            this.LblAcFolderList.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcFolderList.AutoSize = true;
            this.LblAcFolderList.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcFolderList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcFolderList.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcFolderList.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcFolderList.Location = new System.Drawing.Point(3, 232);
            this.LblAcFolderList.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcFolderList.Name = "LblAcFolderList";
            this.LblAcFolderList.Size = new System.Drawing.Size(34, 33);
            this.LblAcFolderList.TabIndex = 18;
            this.LblAcFolderList.Text = "F";
            // 
            // LblAcSearch
            // 
            this.LblAcSearch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LblAcSearch.AutoSize = true;
            this.LblAcSearch.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcSearch.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcSearch.Location = new System.Drawing.Point(2, 82);
            this.LblAcSearch.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcSearch.Name = "LblAcSearch";
            this.LblAcSearch.Size = new System.Drawing.Size(35, 33);
            this.LblAcSearch.TabIndex = 17;
            this.LblAcSearch.Text = "S";
            // 
            // SearchText
            // 
            this.Tlp.SetColumnSpan(this.SearchText, 11);
            this.SearchText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SearchText.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchText.Location = new System.Drawing.Point(45, 74);
            this.SearchText.Margin = new System.Windows.Forms.Padding(5);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(1715, 49);
            this.SearchText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.Tlp.SetColumnSpan(this.label1, 11);
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(45, 27);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(197, 37);
            this.label1.TabIndex = 1;
            this.label1.Text = "Search Text:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.Tlp.SetColumnSpan(this.label2, 11);
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(45, 185);
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
            this.BtnDelItem.Location = new System.Drawing.Point(1852, 74);
            this.BtnDelItem.Margin = new System.Windows.Forms.Padding(5);
            this.BtnDelItem.Name = "BtnDelItem";
            this.BtnDelItem.Size = new System.Drawing.Size(158, 49);
            this.BtnDelItem.TabIndex = 10;
            this.BtnDelItem.TabStop = false;
            this.BtnDelItem.UseVisualStyleBackColor = true;
            // 
            // FolderListBox
            // 
            this.Tlp.SetColumnSpan(this.FolderListBox, 14);
            this.FolderListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FolderListBox.FormattingEnabled = true;
            this.FolderListBox.ItemHeight = 25;
            this.FolderListBox.Location = new System.Drawing.Point(45, 232);
            this.FolderListBox.Margin = new System.Windows.Forms.Padding(5);
            this.FolderListBox.Name = "FolderListBox";
            this.FolderListBox.Size = new System.Drawing.Size(1965, 257);
            this.FolderListBox.TabIndex = 11;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Image = global::QuickFiler.Properties.Resources.Run;
            this.Ok.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Ok.Location = new System.Drawing.Point(346, 562);
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
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Image = global::QuickFiler.Properties.Resources.Cancel;
            this.Cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Cancel.Location = new System.Drawing.Point(715, 562);
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
            // RefreshPredicted
            // 
            this.RefreshPredicted.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.RefreshPredicted.Image = global::QuickFiler.Properties.Resources.QuickRefresh1;
            this.RefreshPredicted.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RefreshPredicted.Location = new System.Drawing.Point(1084, 562);
            this.RefreshPredicted.Margin = new System.Windows.Forms.Padding(15, 14, 15, 14);
            this.RefreshPredicted.Name = "RefreshPredicted";
            this.Tlp.SetRowSpan(this.RefreshPredicted, 4);
            this.RefreshPredicted.Size = new System.Drawing.Size(299, 103);
            this.RefreshPredicted.TabIndex = 12;
            this.RefreshPredicted.Text = "Refresh\r\nPredicted";
            this.RefreshPredicted.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RefreshPredicted.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.RefreshPredicted.UseVisualStyleBackColor = true;
            // 
            // NewFolder
            // 
            this.NewFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.NewFolder.Image = global::QuickFiler.Properties.Resources.NewFolder1;
            this.NewFolder.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NewFolder.Location = new System.Drawing.Point(1453, 562);
            this.NewFolder.Margin = new System.Windows.Forms.Padding(15, 14, 15, 14);
            this.NewFolder.Name = "NewFolder";
            this.Tlp.SetRowSpan(this.NewFolder, 4);
            this.NewFolder.Size = new System.Drawing.Size(299, 103);
            this.NewFolder.TabIndex = 12;
            this.NewFolder.Text = "New \r\nFolder";
            this.NewFolder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.NewFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.NewFolder.UseVisualStyleBackColor = true;
            // 
            // SaveAttachments
            // 
            this.SaveAttachments.AutoSize = true;
            this.SaveAttachments.Location = new System.Drawing.Point(43, 527);
            this.SaveAttachments.Name = "SaveAttachments";
            this.SaveAttachments.Size = new System.Drawing.Size(163, 29);
            this.SaveAttachments.TabIndex = 13;
            this.SaveAttachments.Text = "Attachments";
            this.SaveAttachments.UseVisualStyleBackColor = true;
            // 
            // SaveEmail
            // 
            this.SaveEmail.AutoSize = true;
            this.SaveEmail.Location = new System.Drawing.Point(43, 572);
            this.SaveEmail.Name = "SaveEmail";
            this.SaveEmail.Size = new System.Drawing.Size(97, 29);
            this.SaveEmail.TabIndex = 14;
            this.SaveEmail.Text = "Email";
            this.SaveEmail.UseVisualStyleBackColor = true;
            // 
            // SavePictures
            // 
            this.SavePictures.AutoSize = true;
            this.SavePictures.Location = new System.Drawing.Point(43, 617);
            this.SavePictures.Name = "SavePictures";
            this.SavePictures.Size = new System.Drawing.Size(122, 29);
            this.SavePictures.TabIndex = 15;
            this.SavePictures.Text = "Pictures";
            this.SavePictures.UseVisualStyleBackColor = true;
            // 
            // MoveConversation
            // 
            this.MoveConversation.AutoSize = true;
            this.MoveConversation.Location = new System.Drawing.Point(43, 662);
            this.MoveConversation.Name = "MoveConversation";
            this.MoveConversation.Size = new System.Drawing.Size(171, 29);
            this.MoveConversation.TabIndex = 16;
            this.MoveConversation.Text = "Conversation";
            this.MoveConversation.UseVisualStyleBackColor = true;
            // 
            // LblAcAttachments
            // 
            this.LblAcAttachments.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcAttachments.AutoSize = true;
            this.LblAcAttachments.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcAttachments.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcAttachments.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcAttachments.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcAttachments.Location = new System.Drawing.Point(2, 529);
            this.LblAcAttachments.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcAttachments.Name = "LblAcAttachments";
            this.LblAcAttachments.Size = new System.Drawing.Size(35, 33);
            this.LblAcAttachments.TabIndex = 20;
            this.LblAcAttachments.Text = "A";
            // 
            // LblAcEmail
            // 
            this.LblAcEmail.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcEmail.AutoSize = true;
            this.LblAcEmail.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcEmail.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcEmail.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcEmail.Location = new System.Drawing.Point(0, 574);
            this.LblAcEmail.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcEmail.Name = "LblAcEmail";
            this.LblAcEmail.Size = new System.Drawing.Size(39, 33);
            this.LblAcEmail.TabIndex = 21;
            this.LblAcEmail.Text = "M";
            // 
            // LblAcPictures
            // 
            this.LblAcPictures.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcPictures.AutoSize = true;
            this.LblAcPictures.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcPictures.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcPictures.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcPictures.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcPictures.Location = new System.Drawing.Point(2, 619);
            this.LblAcPictures.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcPictures.Name = "LblAcPictures";
            this.LblAcPictures.Size = new System.Drawing.Size(35, 33);
            this.LblAcPictures.TabIndex = 22;
            this.LblAcPictures.Text = "P";
            // 
            // LblAcConversation
            // 
            this.LblAcConversation.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcConversation.AutoSize = true;
            this.LblAcConversation.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcConversation.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcConversation.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcConversation.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcConversation.Location = new System.Drawing.Point(1, 664);
            this.LblAcConversation.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcConversation.Name = "LblAcConversation";
            this.LblAcConversation.Size = new System.Drawing.Size(37, 33);
            this.LblAcConversation.TabIndex = 23;
            this.LblAcConversation.Text = "C";
            // 
            // LblAcOk
            // 
            this.LblAcOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcOk.AutoSize = true;
            this.LblAcOk.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcOk.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcOk.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcOk.Location = new System.Drawing.Point(293, 574);
            this.LblAcOk.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcOk.Name = "LblAcOk";
            this.LblAcOk.Size = new System.Drawing.Size(35, 33);
            this.LblAcOk.TabIndex = 24;
            this.LblAcOk.Text = "K";
            // 
            // LblAcCancel
            // 
            this.LblAcCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcCancel.AutoSize = true;
            this.LblAcCancel.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcCancel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcCancel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcCancel.Location = new System.Drawing.Point(662, 574);
            this.LblAcCancel.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcCancel.Name = "LblAcCancel";
            this.LblAcCancel.Size = new System.Drawing.Size(35, 33);
            this.LblAcCancel.TabIndex = 24;
            this.LblAcCancel.Text = "X";
            // 
            // LblAcRefresh
            // 
            this.LblAcRefresh.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcRefresh.AutoSize = true;
            this.LblAcRefresh.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcRefresh.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcRefresh.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcRefresh.Location = new System.Drawing.Point(1030, 574);
            this.LblAcRefresh.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcRefresh.Name = "LblAcRefresh";
            this.LblAcRefresh.Size = new System.Drawing.Size(37, 33);
            this.LblAcRefresh.TabIndex = 24;
            this.LblAcRefresh.Text = "R";
            // 
            // LblAcNewFolder
            // 
            this.LblAcNewFolder.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcNewFolder.AutoSize = true;
            this.LblAcNewFolder.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcNewFolder.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcNewFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcNewFolder.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcNewFolder.Location = new System.Drawing.Point(1399, 574);
            this.LblAcNewFolder.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.LblAcNewFolder.Name = "LblAcNewFolder";
            this.LblAcNewFolder.Size = new System.Drawing.Size(37, 33);
            this.LblAcNewFolder.TabIndex = 24;
            this.LblAcNewFolder.Text = "N";
            // 
            // ItemViewer
            // 
            this.ItemViewer.AutoSize = true;
            this.ItemViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ItemViewer.Controller = null;
            this.ItemViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemViewer.Location = new System.Drawing.Point(36, 56);
            this.ItemViewer.Margin = new System.Windows.Forms.Padding(6);
            this.ItemViewer.MinimumSize = new System.Drawing.Size(1516, 197);
            this.ItemViewer.Name = "ItemViewer";
            this.ItemViewer.Size = new System.Drawing.Size(2012, 1018);
            this.ItemViewer.TabIndex = 2;
            // 
            // MenuStrip
            // 
            this.MenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OptionsStrip});
            this.MenuStrip.Location = new System.Drawing.Point(30, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(2024, 42);
            this.MenuStrip.TabIndex = 3;
            this.MenuStrip.Text = "menuStrip1";
            // 
            // OptionsStrip
            // 
            this.OptionsStrip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveOptionsToolStripMenuItem,
            this.categoryOptionsToolStripMenuItem});
            this.OptionsStrip.Name = "OptionsStrip";
            this.OptionsStrip.Size = new System.Drawing.Size(118, 38);
            this.OptionsStrip.Text = "&Options";
            // 
            // moveOptionsToolStripMenuItem
            // 
            this.moveOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.attachmentsToolStripMenuItem,
            this.conversationToolStripMenuItem,
            this.emailToolStripMenuItem,
            this.picturesToolStripMenuItem});
            this.moveOptionsToolStripMenuItem.Name = "moveOptionsToolStripMenuItem";
            this.moveOptionsToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.moveOptionsToolStripMenuItem.Text = "&Move Options";
            // 
            // attachmentsToolStripMenuItem
            // 
            this.attachmentsToolStripMenuItem.Name = "attachmentsToolStripMenuItem";
            this.attachmentsToolStripMenuItem.Size = new System.Drawing.Size(287, 44);
            this.attachmentsToolStripMenuItem.Text = "Attachments";
            // 
            // conversationToolStripMenuItem
            // 
            this.conversationToolStripMenuItem.Name = "conversationToolStripMenuItem";
            this.conversationToolStripMenuItem.Size = new System.Drawing.Size(287, 44);
            this.conversationToolStripMenuItem.Text = "Conversation";
            // 
            // emailToolStripMenuItem
            // 
            this.emailToolStripMenuItem.Name = "emailToolStripMenuItem";
            this.emailToolStripMenuItem.Size = new System.Drawing.Size(287, 44);
            this.emailToolStripMenuItem.Text = "Email";
            // 
            // picturesToolStripMenuItem
            // 
            this.picturesToolStripMenuItem.Name = "picturesToolStripMenuItem";
            this.picturesToolStripMenuItem.Size = new System.Drawing.Size(287, 44);
            this.picturesToolStripMenuItem.Text = "Pictures";
            // 
            // categoryOptionsToolStripMenuItem
            // 
            this.categoryOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectToolStripMenuItem,
            this.manageToolStripMenuItem});
            this.categoryOptionsToolStripMenuItem.Name = "categoryOptionsToolStripMenuItem";
            this.categoryOptionsToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.categoryOptionsToolStripMenuItem.Text = "&Category Options";
            // 
            // selectToolStripMenuItem
            // 
            this.selectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noneToolStripMenuItem});
            this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            this.selectToolStripMenuItem.Size = new System.Drawing.Size(234, 44);
            this.selectToolStripMenuItem.Text = "&Select";
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.Size = new System.Drawing.Size(206, 44);
            this.noneToolStripMenuItem.Text = "None";
            // 
            // manageToolStripMenuItem
            // 
            this.manageToolStripMenuItem.Name = "manageToolStripMenuItem";
            this.manageToolStripMenuItem.Size = new System.Drawing.Size(234, 44);
            this.manageToolStripMenuItem.Text = "&Manage";
            // 
            // EfcViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2084, 1824);
            this.Controls.Add(this.L0vh_TLP);
            this.MinimumSize = new System.Drawing.Size(2110, 1895);
            this.Name = "EfcViewer";
            this.Text = "EfcViewer";
            this.L0vh_TLP.ResumeLayout(false);
            this.L0vh_TLP.PerformLayout();
            this.Tlp.ResumeLayout(false);
            this.Tlp.PerformLayout();
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.TableLayoutPanel Tlp;
        internal System.Windows.Forms.Label LblAcTrash;
        internal System.Windows.Forms.Label LblAcFolderList;
        internal System.Windows.Forms.Label LblAcSearch;
        internal System.Windows.Forms.TextBox SearchText;
        internal System.Windows.Forms.Label label1;
        internal System.Windows.Forms.Label label2;
        internal System.Windows.Forms.Button BtnDelItem;
        internal System.Windows.Forms.ListBox FolderListBox;
        internal System.Windows.Forms.Button Ok;
        internal System.Windows.Forms.Button Cancel;
        internal System.Windows.Forms.Button RefreshPredicted;
        internal System.Windows.Forms.Button NewFolder;
        internal System.Windows.Forms.CheckBox SaveAttachments;
        internal System.Windows.Forms.CheckBox SaveEmail;
        internal System.Windows.Forms.CheckBox SavePictures;
        internal System.Windows.Forms.CheckBox MoveConversation;
        internal System.Windows.Forms.Label LblAcAttachments;
        internal System.Windows.Forms.Label LblAcEmail;
        internal System.Windows.Forms.Label LblAcPictures;
        internal System.Windows.Forms.Label LblAcConversation;
        internal System.Windows.Forms.Label LblAcOk;
        internal System.Windows.Forms.Label LblAcCancel;
        internal System.Windows.Forms.Label LblAcRefresh;
        internal System.Windows.Forms.Label LblAcNewFolder;
        internal System.Windows.Forms.TableLayoutPanel L0vh_TLP;
        internal ItemViewer ItemViewer;
        private System.Windows.Forms.ToolStripMenuItem moveOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem attachmentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem conversationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emailToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem picturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem categoryOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem OptionsStrip;
        internal System.Windows.Forms.MenuStrip MenuStrip;
    }
}