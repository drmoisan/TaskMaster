using QuickFiler.Viewers;
using SVGControl;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EfcViewer));
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
            this.Ok = new SVGControl.ButtonSVG();
            this.Cancel = new SVGControl.ButtonSVG();
            this.RefreshPredicted = new SVGControl.ButtonSVG();
            this.NewFolder = new SVGControl.ButtonSVG();
            this.LblAcEmail = new System.Windows.Forms.Label();
            this.LblAcFilters = new System.Windows.Forms.Label();
            this.LblAcOk = new System.Windows.Forms.Label();
            this.LblAcCancel = new System.Windows.Forms.Label();
            this.LblAcRefresh = new System.Windows.Forms.Label();
            this.LblAcNewFolder = new System.Windows.Forms.Label();
            this.FilterMenuStrip = new System.Windows.Forms.MenuStrip();
            this.FiltersMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.emptyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.NewFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditFiltersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveOptionsStrip = new System.Windows.Forms.MenuStrip();
            this.MoveOptionsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ConversationMenuItem = new QuickFiler.Viewers.ToolStripMenuItemCb();
            this.SaveAttachmentsMenuItem = new QuickFiler.Viewers.ToolStripMenuItemCb();
            this.SaveEmailMenuItem = new QuickFiler.Viewers.ToolStripMenuItemCb();
            this.SavePicturesMenuItem = new QuickFiler.Viewers.ToolStripMenuItemCb();
            this.ItemViewer = new QuickFiler.ItemViewer();
            this.L0vh_TLP.SuspendLayout();
            this.Tlp.SuspendLayout();
            this.FilterMenuStrip.SuspendLayout();
            this.MoveOptionsStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // L0vh_TLP
            // 
            this.L0vh_TLP.ColumnCount = 3;
            this.L0vh_TLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.L0vh_TLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L0vh_TLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.L0vh_TLP.Controls.Add(this.Tlp, 1, 2);
            this.L0vh_TLP.Controls.Add(this.ItemViewer, 1, 1);
            this.L0vh_TLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L0vh_TLP.Location = new System.Drawing.Point(0, 0);
            this.L0vh_TLP.Name = "L0vh_TLP";
            this.L0vh_TLP.RowCount = 4;
            this.L0vh_TLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.L0vh_TLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 660F));
            this.L0vh_TLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L0vh_TLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.L0vh_TLP.Size = new System.Drawing.Size(1589, 1175);
            this.L0vh_TLP.TabIndex = 0;
            // 
            // Tlp
            // 
            this.Tlp.ColumnCount = 15;
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00063F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 219F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 219F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 219F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 219F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.99937F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
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
            this.Tlp.Controls.Add(this.LblAcEmail, 0, 7);
            this.Tlp.Controls.Add(this.LblAcFilters, 0, 8);
            this.Tlp.Controls.Add(this.LblAcOk, 3, 7);
            this.Tlp.Controls.Add(this.LblAcCancel, 5, 7);
            this.Tlp.Controls.Add(this.LblAcRefresh, 7, 7);
            this.Tlp.Controls.Add(this.LblAcNewFolder, 9, 7);
            this.Tlp.Controls.Add(this.FilterMenuStrip, 1, 8);
            this.Tlp.Controls.Add(this.MoveOptionsStrip, 1, 7);
            this.Tlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tlp.Location = new System.Drawing.Point(23, 684);
            this.Tlp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Tlp.Name = "Tlp";
            this.Tlp.RowCount = 10;
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.Tlp.Size = new System.Drawing.Size(1543, 467);
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
            this.LblAcTrash.Location = new System.Drawing.Point(1389, 52);
            this.LblAcTrash.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcTrash.Name = "LblAcTrash";
            this.LblAcTrash.Size = new System.Drawing.Size(22, 22);
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
            this.LblAcFolderList.Location = new System.Drawing.Point(2, 150);
            this.LblAcFolderList.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.LblAcFolderList.Name = "LblAcFolderList";
            this.LblAcFolderList.Size = new System.Drawing.Size(22, 22);
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
            this.LblAcSearch.Location = new System.Drawing.Point(2, 52);
            this.LblAcSearch.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcSearch.Name = "LblAcSearch";
            this.LblAcSearch.Size = new System.Drawing.Size(23, 22);
            this.LblAcSearch.TabIndex = 17;
            this.LblAcSearch.Text = "S";
            // 
            // SearchText
            // 
            this.Tlp.SetColumnSpan(this.SearchText, 11);
            this.SearchText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SearchText.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchText.Location = new System.Drawing.Point(30, 48);
            this.SearchText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(1330, 34);
            this.SearchText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.Tlp.SetColumnSpan(this.label1, 11);
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(30, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Search Text:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.Tlp.SetColumnSpan(this.label2, 11);
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(30, 117);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(164, 25);
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
            this.BtnDelItem.Image = ((System.Drawing.Image)(resources.GetObject("BtnDelItem.Image")));
            this.BtnDelItem.Location = new System.Drawing.Point(1435, 48);
            this.BtnDelItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.BtnDelItem.Name = "BtnDelItem";
            this.BtnDelItem.Size = new System.Drawing.Size(105, 30);
            this.BtnDelItem.TabIndex = 10;
            this.BtnDelItem.TabStop = false;
            this.BtnDelItem.UseVisualStyleBackColor = true;
            // 
            // FolderListBox
            // 
            this.Tlp.SetColumnSpan(this.FolderListBox, 14);
            this.FolderListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FolderListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FolderListBox.FormattingEnabled = true;
            this.FolderListBox.ItemHeight = 20;
            this.FolderListBox.Location = new System.Drawing.Point(30, 150);
            this.FolderListBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.FolderListBox.Name = "FolderListBox";
            this.FolderListBox.Size = new System.Drawing.Size(1510, 181);
            this.FolderListBox.TabIndex = 11;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Image = ((System.Drawing.Image)(resources.GetObject("Ok.Image")));
            this.Ok.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Ok.ImageSVG.ImagePath = "../QuickFiler/Resources/Run.svg";
            this.Ok.ImageSVG.Margin = new System.Windows.Forms.Padding(3);
            this.Ok.ImageSVG.SaveRendering = false;
            this.Ok.ImageSVG.Size = new System.Drawing.Size(191, 61);
            this.Ok.ImageSVG.UseDefaultImage = false;
            this.Ok.Location = new System.Drawing.Point(326, 377);
            this.Ok.Margin = new System.Windows.Forms.Padding(11, 8, 11, 8);
            this.Ok.Name = "Ok";
            this.Tlp.SetRowSpan(this.Ok, 4);
            this.Ok.Size = new System.Drawing.Size(197, 67);
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
            this.Cancel.Image = ((System.Drawing.Image)(resources.GetObject("Cancel.Image")));
            this.Cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Cancel.ImageSVG.ImagePath = "../QuickFiler/Resources/Cancel.svg";
            this.Cancel.ImageSVG.Margin = new System.Windows.Forms.Padding(3);
            this.Cancel.ImageSVG.SaveRendering = false;
            this.Cancel.ImageSVG.Size = new System.Drawing.Size(191, 61);
            this.Cancel.ImageSVG.UseDefaultImage = false;
            this.Cancel.Location = new System.Drawing.Point(572, 377);
            this.Cancel.Margin = new System.Windows.Forms.Padding(11, 8, 11, 8);
            this.Cancel.Name = "Cancel";
            this.Tlp.SetRowSpan(this.Cancel, 4);
            this.Cancel.Size = new System.Drawing.Size(197, 67);
            this.Cancel.TabIndex = 12;
            this.Cancel.Text = "Cancel";
            this.Cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // RefreshPredicted
            // 
            this.RefreshPredicted.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.RefreshPredicted.Image = ((System.Drawing.Image)(resources.GetObject("RefreshPredicted.Image")));
            this.RefreshPredicted.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RefreshPredicted.ImageSVG.ImagePath = "../QuickFiler/Resources/QuickRefresh.svg";
            this.RefreshPredicted.ImageSVG.Margin = new System.Windows.Forms.Padding(6);
            this.RefreshPredicted.ImageSVG.SaveRendering = false;
            this.RefreshPredicted.ImageSVG.Size = new System.Drawing.Size(185, 55);
            this.RefreshPredicted.ImageSVG.UseDefaultImage = false;
            this.RefreshPredicted.Location = new System.Drawing.Point(818, 377);
            this.RefreshPredicted.Margin = new System.Windows.Forms.Padding(11, 8, 11, 8);
            this.RefreshPredicted.Name = "RefreshPredicted";
            this.Tlp.SetRowSpan(this.RefreshPredicted, 4);
            this.RefreshPredicted.Size = new System.Drawing.Size(197, 67);
            this.RefreshPredicted.TabIndex = 12;
            this.RefreshPredicted.Text = "Refresh\r\nPredicted";
            this.RefreshPredicted.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RefreshPredicted.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.RefreshPredicted.UseVisualStyleBackColor = true;
            // 
            // NewFolder
            // 
            this.NewFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.NewFolder.Image = ((System.Drawing.Image)(resources.GetObject("NewFolder.Image")));
            this.NewFolder.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NewFolder.ImageSVG.ImagePath = "../QuickFiler/Resources/NewFolder.svg";
            this.NewFolder.ImageSVG.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.NewFolder.ImageSVG.SaveRendering = false;
            this.NewFolder.ImageSVG.Size = new System.Drawing.Size(191, 58);
            this.NewFolder.ImageSVG.UseDefaultImage = false;
            this.NewFolder.Location = new System.Drawing.Point(1064, 377);
            this.NewFolder.Margin = new System.Windows.Forms.Padding(11, 8, 11, 8);
            this.NewFolder.Name = "NewFolder";
            this.Tlp.SetRowSpan(this.NewFolder, 4);
            this.NewFolder.Size = new System.Drawing.Size(197, 67);
            this.NewFolder.TabIndex = 12;
            this.NewFolder.Text = "New \r\nFolder";
            this.NewFolder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.NewFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.NewFolder.UseVisualStyleBackColor = true;
            // 
            // LblAcEmail
            // 
            this.LblAcEmail.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcEmail.AutoSize = true;
            this.LblAcEmail.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcEmail.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcEmail.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcEmail.Location = new System.Drawing.Point(0, 387);
            this.LblAcEmail.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.LblAcEmail.Name = "LblAcEmail";
            this.LblAcEmail.Size = new System.Drawing.Size(26, 22);
            this.LblAcEmail.TabIndex = 21;
            this.LblAcEmail.Text = "M";
            // 
            // LblAcFilters
            // 
            this.LblAcFilters.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcFilters.AutoSize = true;
            this.LblAcFilters.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcFilters.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcFilters.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcFilters.Location = new System.Drawing.Point(2, 415);
            this.LblAcFilters.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.LblAcFilters.Name = "LblAcFilters";
            this.LblAcFilters.Size = new System.Drawing.Size(22, 22);
            this.LblAcFilters.TabIndex = 22;
            this.LblAcFilters.Text = "F";
            // 
            // LblAcOk
            // 
            this.LblAcOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcOk.AutoSize = true;
            this.LblAcOk.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcOk.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcOk.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcOk.Location = new System.Drawing.Point(290, 387);
            this.LblAcOk.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.LblAcOk.Name = "LblAcOk";
            this.LblAcOk.Size = new System.Drawing.Size(23, 22);
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
            this.LblAcCancel.Location = new System.Drawing.Point(536, 387);
            this.LblAcCancel.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.LblAcCancel.Name = "LblAcCancel";
            this.LblAcCancel.Size = new System.Drawing.Size(23, 22);
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
            this.LblAcRefresh.Location = new System.Drawing.Point(781, 387);
            this.LblAcRefresh.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.LblAcRefresh.Name = "LblAcRefresh";
            this.LblAcRefresh.Size = new System.Drawing.Size(24, 22);
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
            this.LblAcNewFolder.Location = new System.Drawing.Point(1027, 387);
            this.LblAcNewFolder.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.LblAcNewFolder.Name = "LblAcNewFolder";
            this.LblAcNewFolder.Size = new System.Drawing.Size(24, 22);
            this.LblAcNewFolder.TabIndex = 24;
            this.LblAcNewFolder.Text = "N";
            // 
            // FilterMenuStrip
            // 
            this.FilterMenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.FilterMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FiltersMenu});
            this.FilterMenuStrip.Location = new System.Drawing.Point(27, 411);
            this.FilterMenuStrip.Name = "FilterMenuStrip";
            this.FilterMenuStrip.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.FilterMenuStrip.Size = new System.Drawing.Size(169, 26);
            this.FilterMenuStrip.TabIndex = 26;
            this.FilterMenuStrip.Text = "menuStrip1";
            // 
            // FiltersMenu
            // 
            this.FiltersMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectToolStripMenuItem1,
            this.manageToolStripMenuItem1});
            this.FiltersMenu.Name = "FiltersMenu";
            this.FiltersMenu.Size = new System.Drawing.Size(62, 24);
            this.FiltersMenu.Text = "&Filters";
            // 
            // selectToolStripMenuItem1
            // 
            this.selectToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.emptyToolStripMenuItem});
            this.selectToolStripMenuItem1.Name = "selectToolStripMenuItem1";
            this.selectToolStripMenuItem1.Size = new System.Drawing.Size(146, 26);
            this.selectToolStripMenuItem1.Text = "Select";
            // 
            // emptyToolStripMenuItem
            // 
            this.emptyToolStripMenuItem.CheckOnClick = true;
            this.emptyToolStripMenuItem.Name = "emptyToolStripMenuItem";
            this.emptyToolStripMenuItem.Size = new System.Drawing.Size(134, 26);
            this.emptyToolStripMenuItem.Text = "Empty";
            // 
            // manageToolStripMenuItem1
            // 
            this.manageToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewFilterMenuItem,
            this.EditFiltersMenuItem});
            this.manageToolStripMenuItem1.Name = "manageToolStripMenuItem1";
            this.manageToolStripMenuItem1.Size = new System.Drawing.Size(146, 26);
            this.manageToolStripMenuItem1.Text = "Manage";
            // 
            // NewFilterMenuItem
            // 
            this.NewFilterMenuItem.Name = "NewFilterMenuItem";
            this.NewFilterMenuItem.Size = new System.Drawing.Size(216, 26);
            this.NewFilterMenuItem.Text = "Add New Filter";
            // 
            // EditFiltersMenuItem
            // 
            this.EditFiltersMenuItem.Name = "EditFiltersMenuItem";
            this.EditFiltersMenuItem.Size = new System.Drawing.Size(216, 26);
            this.EditFiltersMenuItem.Text = "Edit Existing Filters";
            // 
            // MoveOptionsStrip
            // 
            this.MoveOptionsStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.MoveOptionsStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MoveOptionsMenu});
            this.MoveOptionsStrip.Location = new System.Drawing.Point(27, 383);
            this.MoveOptionsStrip.Name = "MoveOptionsStrip";
            this.MoveOptionsStrip.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.MoveOptionsStrip.Size = new System.Drawing.Size(169, 26);
            this.MoveOptionsStrip.TabIndex = 25;
            this.MoveOptionsStrip.Text = "menuStrip1";
            // 
            // MoveOptionsMenu
            // 
            this.MoveOptionsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ConversationMenuItem,
            this.SaveAttachmentsMenuItem,
            this.SaveEmailMenuItem,
            this.SavePicturesMenuItem});
            this.MoveOptionsMenu.Name = "MoveOptionsMenu";
            this.MoveOptionsMenu.Size = new System.Drawing.Size(116, 24);
            this.MoveOptionsMenu.Text = "&Move Options";
            // 
            // ConversationMenuItem
            // 
            this.ConversationMenuItem.CheckOnClick = true;
            this.ConversationMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ConversationMenuItem.Image")));
            this.ConversationMenuItem.Name = "ConversationMenuItem";
            this.ConversationMenuItem.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.ConversationMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.ConversationMenuItem.Size = new System.Drawing.Size(310, 26);
            this.ConversationMenuItem.Text = "Move &Conversation";
            // 
            // SaveAttachmentsMenuItem
            // 
            this.SaveAttachmentsMenuItem.CheckOnClick = true;
            this.SaveAttachmentsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("SaveAttachmentsMenuItem.Image")));
            this.SaveAttachmentsMenuItem.Name = "SaveAttachmentsMenuItem";
            this.SaveAttachmentsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.A)));
            this.SaveAttachmentsMenuItem.Size = new System.Drawing.Size(310, 26);
            this.SaveAttachmentsMenuItem.Text = "Save &Attachments";
            // 
            // SaveEmailMenuItem
            // 
            this.SaveEmailMenuItem.CheckOnClick = true;
            this.SaveEmailMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("SaveEmailMenuItem.Image")));
            this.SaveEmailMenuItem.Name = "SaveEmailMenuItem";
            this.SaveEmailMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.M)));
            this.SaveEmailMenuItem.Size = new System.Drawing.Size(310, 26);
            this.SaveEmailMenuItem.Text = "Save E&mail Copy";
            // 
            // SavePicturesMenuItem
            // 
            this.SavePicturesMenuItem.CheckOnClick = true;
            this.SavePicturesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("SavePicturesMenuItem.Image")));
            this.SavePicturesMenuItem.Name = "SavePicturesMenuItem";
            this.SavePicturesMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
            this.SavePicturesMenuItem.Size = new System.Drawing.Size(310, 26);
            this.SavePicturesMenuItem.Text = "Save &Pictures";
            // 
            // ItemViewer
            // 
            this.ItemViewer.AutoSize = true;
            this.ItemViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ItemViewer.Controller = null;
            this.ItemViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemViewer.Location = new System.Drawing.Point(28, 28);
            this.ItemViewer.Margin = new System.Windows.Forms.Padding(8);
            this.ItemViewer.MinimumSize = new System.Drawing.Size(1011, 126);
            this.ItemViewer.Name = "ItemViewer";
            this.ItemViewer.Size = new System.Drawing.Size(1533, 644);
            this.ItemViewer.TabIndex = 2;
            // 
            // EfcViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1589, 1175);
            this.Controls.Add(this.L0vh_TLP);
            this.MainMenuStrip = this.MoveOptionsStrip;
            this.MinimumSize = new System.Drawing.Size(900, 668);
            this.Name = "EfcViewer";
            this.Text = "EfcViewer";
            this.L0vh_TLP.ResumeLayout(false);
            this.L0vh_TLP.PerformLayout();
            this.Tlp.ResumeLayout(false);
            this.Tlp.PerformLayout();
            this.FilterMenuStrip.ResumeLayout(false);
            this.FilterMenuStrip.PerformLayout();
            this.MoveOptionsStrip.ResumeLayout(false);
            this.MoveOptionsStrip.PerformLayout();
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
        internal ButtonSVG Ok;
        internal ButtonSVG Cancel;
        internal ButtonSVG RefreshPredicted;
        internal ButtonSVG NewFolder;
        internal System.Windows.Forms.Label LblAcEmail;
        internal System.Windows.Forms.Label LblAcFilters;
        internal System.Windows.Forms.Label LblAcOk;
        internal System.Windows.Forms.Label LblAcCancel;
        internal System.Windows.Forms.Label LblAcRefresh;
        internal System.Windows.Forms.Label LblAcNewFolder;
        internal System.Windows.Forms.TableLayoutPanel L0vh_TLP;
        internal ItemViewer ItemViewer;
        private System.Windows.Forms.MenuStrip FilterMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem FiltersMenu;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem emptyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem1;
        internal System.Windows.Forms.MenuStrip MoveOptionsStrip;
        public ToolStripMenuItemCb ConversationMenuItem;
        public ToolStripMenuItemCb SaveAttachmentsMenuItem;
        public ToolStripMenuItemCb SaveEmailMenuItem;
        public ToolStripMenuItemCb SavePicturesMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem MoveOptionsMenu;
        internal System.Windows.Forms.ToolStripMenuItem NewFilterMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem EditFiltersMenuItem;
    }
}