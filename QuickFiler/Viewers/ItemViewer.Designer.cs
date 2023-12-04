namespace QuickFiler
{
    partial class ItemViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Microsoft.Web.WebView2.WinForms.CoreWebView2CreationProperties coreWebView2CreationProperties1 = new Microsoft.Web.WebView2.WinForms.CoreWebView2CreationProperties();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemViewer));
            this.L0vh_Tlp = new System.Windows.Forms.TableLayoutPanel();
            this.MoveOptionsStrip = new System.Windows.Forms.MenuStrip();
            this.MoveOptionsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.LblAcSearch = new System.Windows.Forms.Label();
            this.CboFolders = new System.Windows.Forms.ComboBox();
            this.LblSearch = new System.Windows.Forms.Label();
            this.TxtboxSearch = new System.Windows.Forms.TextBox();
            this.L0v2h2_WebView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.LblConvCt = new System.Windows.Forms.Label();
            this.LblItemNumber = new System.Windows.Forms.Label();
            this.LblSender = new System.Windows.Forms.Label();
            this.LblTriage = new System.Windows.Forms.Label();
            this.lblCaptionTriage = new System.Windows.Forms.Label();
            this.LblCaptionPredicted = new System.Windows.Forms.Label();
            this.LblActionable = new System.Windows.Forms.Label();
            this.LblSentOn = new System.Windows.Forms.Label();
            this.LblSubject = new System.Windows.Forms.Label();
            this.LblAcOpen = new System.Windows.Forms.Label();
            this.L1h0L2hv3h_TlpBodyToggle = new System.Windows.Forms.TableLayoutPanel();
            this.TopicThread = new BrightIdeasSoftware.FastObjectListView();
            this.sender = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.SentDate = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.infolder = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.TxtboxBody = new System.Windows.Forms.TextBox();
            this.LblAcBody = new System.Windows.Forms.Label();
            this.LblAcFolder = new System.Windows.Forms.Label();
            this.LblFolder = new System.Windows.Forms.Label();
            this.L1h1L2v1h3Panel = new System.Windows.Forms.Panel();
            this.LblAcReply = new System.Windows.Forms.Label();
            this.LblAcReplyAll = new System.Windows.Forms.Label();
            this.LblAcFwd = new System.Windows.Forms.Label();
            this.BtnReply = new System.Windows.Forms.Button();
            this.BtnReplyAll = new System.Windows.Forms.Button();
            this.BtnForward = new System.Windows.Forms.Button();
            this.LblAcDelete = new System.Windows.Forms.Label();
            this.BtnDelItem = new System.Windows.Forms.Button();
            this.LblAcPopOut = new System.Windows.Forms.Label();
            this.BtnPopOut = new System.Windows.Forms.Button();
            this.LblAcTask = new System.Windows.Forms.Label();
            this.BtnFlagTask = new System.Windows.Forms.Button();
            this.LblAcMoveOptions = new System.Windows.Forms.Label();
            this.ConversationMenuItem = new QuickFiler.Viewers.ToolStripMenuItemCb();
            this.SaveAttachmentsMenuItem = new QuickFiler.Viewers.ToolStripMenuItemCb();
            this.SaveEmailMenuItem = new QuickFiler.Viewers.ToolStripMenuItemCb();
            this.SavePicturesMenuItem = new QuickFiler.Viewers.ToolStripMenuItemCb();
            this.L0vh_Tlp.SuspendLayout();
            this.MoveOptionsStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.L0v2h2_WebView2)).BeginInit();
            this.L1h0L2hv3h_TlpBodyToggle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TopicThread)).BeginInit();
            this.L1h1L2v1h3Panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // L0vh_Tlp
            // 
            this.L0vh_Tlp.ColumnCount = 15;
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 545F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 122F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.L0vh_Tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 622F));
            this.L0vh_Tlp.Controls.Add(this.MoveOptionsStrip, 12, 0);
            this.L0vh_Tlp.Controls.Add(this.LblAcSearch, 11, 2);
            this.L0vh_Tlp.Controls.Add(this.CboFolders, 13, 4);
            this.L0vh_Tlp.Controls.Add(this.LblSearch, 12, 2);
            this.L0vh_Tlp.Controls.Add(this.TxtboxSearch, 13, 2);
            this.L0vh_Tlp.Controls.Add(this.L0v2h2_WebView2, 1, 5);
            this.L0vh_Tlp.Controls.Add(this.LblConvCt, 10, 1);
            this.L0vh_Tlp.Controls.Add(this.LblItemNumber, 0, 0);
            this.L0vh_Tlp.Controls.Add(this.LblSender, 1, 0);
            this.L0vh_Tlp.Controls.Add(this.LblTriage, 4, 0);
            this.L0vh_Tlp.Controls.Add(this.lblCaptionTriage, 3, 0);
            this.L0vh_Tlp.Controls.Add(this.LblCaptionPredicted, 6, 0);
            this.L0vh_Tlp.Controls.Add(this.LblActionable, 7, 0);
            this.L0vh_Tlp.Controls.Add(this.LblSentOn, 9, 0);
            this.L0vh_Tlp.Controls.Add(this.LblSubject, 1, 1);
            this.L0vh_Tlp.Controls.Add(this.LblAcOpen, 0, 3);
            this.L0vh_Tlp.Controls.Add(this.L1h0L2hv3h_TlpBodyToggle, 1, 3);
            this.L0vh_Tlp.Controls.Add(this.LblAcBody, 0, 5);
            this.L0vh_Tlp.Controls.Add(this.LblAcFolder, 11, 4);
            this.L0vh_Tlp.Controls.Add(this.LblFolder, 12, 4);
            this.L0vh_Tlp.Controls.Add(this.L1h1L2v1h3Panel, 14, 0);
            this.L0vh_Tlp.Controls.Add(this.LblAcMoveOptions, 11, 0);
            this.L0vh_Tlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L0vh_Tlp.Location = new System.Drawing.Point(0, 0);
            this.L0vh_Tlp.Name = "L0vh_Tlp";
            this.L0vh_Tlp.RowCount = 6;
            this.L0vh_Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.L0vh_Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.L0vh_Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.L0vh_Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.L0vh_Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 97F));
            this.L0vh_Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L0vh_Tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.L0vh_Tlp.Size = new System.Drawing.Size(2396, 907);
            this.L0vh_Tlp.TabIndex = 0;
            // 
            // MoveOptionsStrip
            // 
            this.MoveOptionsStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.MoveOptionsStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.MoveOptionsStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MoveOptionsMenu});
            this.MoveOptionsStrip.Location = new System.Drawing.Point(1427, 0);
            this.MoveOptionsStrip.Name = "MoveOptionsStrip";
            this.L0vh_Tlp.SetRowSpan(this.MoveOptionsStrip, 2);
            this.MoveOptionsStrip.Size = new System.Drawing.Size(200, 48);
            this.MoveOptionsStrip.TabIndex = 43;
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
            this.MoveOptionsMenu.Size = new System.Drawing.Size(186, 40);
            this.MoveOptionsMenu.Text = "&Move Options";
            // 
            // LblAcSearch
            // 
            this.LblAcSearch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LblAcSearch.AutoSize = true;
            this.LblAcSearch.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcSearch.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcSearch.Location = new System.Drawing.Point(1389, 70);
            this.LblAcSearch.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcSearch.Name = "LblAcSearch";
            this.L0vh_Tlp.SetRowSpan(this.LblAcSearch, 2);
            this.LblAcSearch.Size = new System.Drawing.Size(35, 33);
            this.LblAcSearch.TabIndex = 10;
            this.LblAcSearch.Text = "S";
            // 
            // CboFolders
            // 
            this.L0vh_Tlp.SetColumnSpan(this.CboFolders, 2);
            this.CboFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CboFolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboFolders.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CboFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboFolders.FormattingEnabled = true;
            this.CboFolders.Location = new System.Drawing.Point(1627, 123);
            this.CboFolders.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.CboFolders.Name = "CboFolders";
            this.CboFolders.Size = new System.Drawing.Size(769, 41);
            this.CboFolders.TabIndex = 42;
            // 
            // LblSearch
            // 
            this.LblSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LblSearch.AutoSize = true;
            this.LblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblSearch.Location = new System.Drawing.Point(1433, 74);
            this.LblSearch.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.LblSearch.Name = "LblSearch";
            this.L0vh_Tlp.SetRowSpan(this.LblSearch, 2);
            this.LblSearch.Size = new System.Drawing.Size(93, 25);
            this.LblSearch.TabIndex = 6;
            this.LblSearch.Text = "Search:";
            // 
            // TxtboxSearch
            // 
            this.TxtboxSearch.BackColor = System.Drawing.SystemColors.Menu;
            this.L0vh_Tlp.SetColumnSpan(this.TxtboxSearch, 2);
            this.TxtboxSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TxtboxSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtboxSearch.Location = new System.Drawing.Point(1630, 65);
            this.TxtboxSearch.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.TxtboxSearch.Name = "TxtboxSearch";
            this.TxtboxSearch.Size = new System.Drawing.Size(763, 41);
            this.TxtboxSearch.TabIndex = 41;
            // 
            // L0v2h2_WebView2
            // 
            this.L0v2h2_WebView2.AllowExternalDrop = true;
            this.L0v2h2_WebView2.BackgroundImage = global::QuickFiler.Properties.Resources.AppStartPageBackground;
            this.L0vh_Tlp.SetColumnSpan(this.L0v2h2_WebView2, 14);
            coreWebView2CreationProperties1.AdditionalBrowserArguments = null;
            coreWebView2CreationProperties1.BrowserExecutableFolder = null;
            coreWebView2CreationProperties1.IsInPrivateModeEnabled = null;
            coreWebView2CreationProperties1.Language = null;
            coreWebView2CreationProperties1.ProfileName = null;
            coreWebView2CreationProperties1.UserDataFolder = null;
            this.L0v2h2_WebView2.CreationProperties = coreWebView2CreationProperties1;
            this.L0v2h2_WebView2.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            this.L0v2h2_WebView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L0v2h2_WebView2.Location = new System.Drawing.Point(103, 216);
            this.L0v2h2_WebView2.Name = "L0v2h2_WebView2";
            this.L0v2h2_WebView2.Size = new System.Drawing.Size(2290, 688);
            this.L0v2h2_WebView2.TabIndex = 40;
            this.L0v2h2_WebView2.ZoomFactor = 1D;
            this.L0v2h2_WebView2.ParentChanged += new System.EventHandler(this.L0v2h2_WebView2_ParentChanged);
            // 
            // LblConvCt
            // 
            this.LblConvCt.AutoSize = true;
            this.LblConvCt.Dock = System.Windows.Forms.DockStyle.Right;
            this.LblConvCt.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblConvCt.Location = new System.Drawing.Point(1303, 35);
            this.LblConvCt.Margin = new System.Windows.Forms.Padding(0);
            this.LblConvCt.Name = "LblConvCt";
            this.LblConvCt.Padding = new System.Windows.Forms.Padding(6);
            this.L0vh_Tlp.SetRowSpan(this.LblConvCt, 2);
            this.LblConvCt.Size = new System.Drawing.Size(84, 58);
            this.LblConvCt.TabIndex = 14;
            this.LblConvCt.Text = "99";
            // 
            // LblItemNumber
            // 
            this.LblItemNumber.BackColor = System.Drawing.SystemColors.HotTrack;
            this.LblItemNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LblItemNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblItemNumber.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.LblItemNumber.Location = new System.Drawing.Point(4, 4);
            this.LblItemNumber.Margin = new System.Windows.Forms.Padding(4, 4, 4, 19);
            this.LblItemNumber.Name = "LblItemNumber";
            this.LblItemNumber.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.L0vh_Tlp.SetRowSpan(this.LblItemNumber, 3);
            this.LblItemNumber.Size = new System.Drawing.Size(92, 70);
            this.LblItemNumber.TabIndex = 6;
            this.LblItemNumber.Text = "[#]";
            this.LblItemNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblSender
            // 
            this.LblSender.AutoSize = true;
            this.LblSender.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblSender.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LblSender.Location = new System.Drawing.Point(106, 0);
            this.LblSender.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.LblSender.MinimumSize = new System.Drawing.Size(539, 0);
            this.LblSender.Name = "LblSender";
            this.LblSender.Size = new System.Drawing.Size(539, 33);
            this.LblSender.TabIndex = 7;
            this.LblSender.Text = "[SENDER NAME]";
            // 
            // LblTriage
            // 
            this.LblTriage.AutoSize = true;
            this.LblTriage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LblTriage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblTriage.Location = new System.Drawing.Point(782, 0);
            this.LblTriage.Margin = new System.Windows.Forms.Padding(0);
            this.LblTriage.Name = "LblTriage";
            this.LblTriage.Size = new System.Drawing.Size(31, 31);
            this.LblTriage.TabIndex = 9;
            this.LblTriage.Text = "A";
            // 
            // lblCaptionTriage
            // 
            this.lblCaptionTriage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCaptionTriage.AutoSize = true;
            this.lblCaptionTriage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaptionTriage.Location = new System.Drawing.Point(698, 0);
            this.lblCaptionTriage.Margin = new System.Windows.Forms.Padding(0);
            this.lblCaptionTriage.Name = "lblCaptionTriage";
            this.lblCaptionTriage.Size = new System.Drawing.Size(84, 29);
            this.lblCaptionTriage.TabIndex = 8;
            this.lblCaptionTriage.Text = "Triage";
            // 
            // LblCaptionPredicted
            // 
            this.LblCaptionPredicted.AutoSize = true;
            this.LblCaptionPredicted.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblCaptionPredicted.Location = new System.Drawing.Point(849, 0);
            this.LblCaptionPredicted.Margin = new System.Windows.Forms.Padding(0);
            this.LblCaptionPredicted.Name = "LblCaptionPredicted";
            this.LblCaptionPredicted.Size = new System.Drawing.Size(117, 35);
            this.LblCaptionPredicted.TabIndex = 10;
            this.LblCaptionPredicted.Text = "Predicted:";
            // 
            // LblActionable
            // 
            this.LblActionable.AutoSize = true;
            this.LblActionable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.LblActionable.Location = new System.Drawing.Point(971, 0);
            this.LblActionable.Margin = new System.Windows.Forms.Padding(0);
            this.LblActionable.Name = "LblActionable";
            this.LblActionable.Size = new System.Drawing.Size(90, 35);
            this.LblActionable.TabIndex = 11;
            this.LblActionable.Text = "Deleted";
            // 
            // LblSentOn
            // 
            this.LblSentOn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblSentOn.AutoSize = true;
            this.L0vh_Tlp.SetColumnSpan(this.LblSentOn, 2);
            this.LblSentOn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.LblSentOn.Location = new System.Drawing.Point(1131, 0);
            this.LblSentOn.Margin = new System.Windows.Forms.Padding(0);
            this.LblSentOn.MinimumSize = new System.Drawing.Size(256, 0);
            this.LblSentOn.Name = "LblSentOn";
            this.LblSentOn.Size = new System.Drawing.Size(256, 29);
            this.LblSentOn.TabIndex = 12;
            this.LblSentOn.Text = "12/31/2999 12:59 PM";
            // 
            // LblSubject
            // 
            this.LblSubject.AutoSize = true;
            this.L0vh_Tlp.SetColumnSpan(this.LblSubject, 9);
            this.LblSubject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LblSubject.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblSubject.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LblSubject.Location = new System.Drawing.Point(100, 35);
            this.LblSubject.Margin = new System.Windows.Forms.Padding(0);
            this.LblSubject.Name = "LblSubject";
            this.L0vh_Tlp.SetRowSpan(this.LblSubject, 2);
            this.LblSubject.Size = new System.Drawing.Size(1187, 58);
            this.LblSubject.TabIndex = 13;
            this.LblSubject.Text = "[SUBJECT]";
            // 
            // LblAcOpen
            // 
            this.LblAcOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblAcOpen.AutoSize = true;
            this.LblAcOpen.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcOpen.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblAcOpen.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcOpen.Location = new System.Drawing.Point(62, 93);
            this.LblAcOpen.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcOpen.Name = "LblAcOpen";
            this.L0vh_Tlp.SetRowSpan(this.LblAcOpen, 2);
            this.LblAcOpen.Size = new System.Drawing.Size(38, 33);
            this.LblAcOpen.TabIndex = 22;
            this.LblAcOpen.Text = "O";
            // 
            // L1h0L2hv3h_TlpBodyToggle
            // 
            this.L1h0L2hv3h_TlpBodyToggle.ColumnCount = 2;
            this.L0vh_Tlp.SetColumnSpan(this.L1h0L2hv3h_TlpBodyToggle, 10);
            this.L1h0L2hv3h_TlpBodyToggle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1h0L2hv3h_TlpBodyToggle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0F));
            this.L1h0L2hv3h_TlpBodyToggle.Controls.Add(this.TopicThread, 1, 0);
            this.L1h0L2hv3h_TlpBodyToggle.Controls.Add(this.TxtboxBody, 0, 0);
            this.L1h0L2hv3h_TlpBodyToggle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.L1h0L2hv3h_TlpBodyToggle.Location = new System.Drawing.Point(103, 96);
            this.L1h0L2hv3h_TlpBodyToggle.Name = "L1h0L2hv3h_TlpBodyToggle";
            this.L1h0L2hv3h_TlpBodyToggle.RowCount = 1;
            this.L0vh_Tlp.SetRowSpan(this.L1h0L2hv3h_TlpBodyToggle, 2);
            this.L1h0L2hv3h_TlpBodyToggle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.L1h0L2hv3h_TlpBodyToggle.Size = new System.Drawing.Size(1281, 114);
            this.L1h0L2hv3h_TlpBodyToggle.TabIndex = 38;
            // 
            // TopicThread
            // 
            this.TopicThread.AllColumns.Add(this.sender);
            this.TopicThread.AllColumns.Add(this.SentDate);
            this.TopicThread.AllColumns.Add(this.infolder);
            this.TopicThread.AllowDrop = true;
            this.TopicThread.BackColor = System.Drawing.SystemColors.Control;
            this.TopicThread.CellEditEnterChangesRows = true;
            this.TopicThread.CellEditUseWholeCell = false;
            this.TopicThread.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.sender,
            this.SentDate,
            this.infolder});
            this.TopicThread.Cursor = System.Windows.Forms.Cursors.Default;
            this.TopicThread.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TopicThread.EmptyListMsg = "The fast list is empty";
            this.TopicThread.FullRowSelect = true;
            this.TopicThread.HideSelection = false;
            this.TopicThread.Location = new System.Drawing.Point(1284, 3);
            this.TopicThread.MultiSelect = false;
            this.TopicThread.Name = "TopicThread";
            this.TopicThread.ShowGroups = false;
            this.TopicThread.Size = new System.Drawing.Size(1, 108);
            this.TopicThread.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.TopicThread.TabIndex = 9;
            this.TopicThread.UseCompatibleStateImageBehavior = false;
            this.TopicThread.View = System.Windows.Forms.View.Details;
            this.TopicThread.VirtualMode = true;
            this.TopicThread.Visible = false;
            // 
            // sender
            // 
            this.sender.AspectName = "SenderName";
            this.sender.FillsFreeSpace = true;
            this.sender.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.sender.MinimumWidth = 200;
            this.sender.Text = "From";
            this.sender.Width = 200;
            // 
            // SentDate
            // 
            this.SentDate.AspectName = "SentDate";
            this.SentDate.FillsFreeSpace = true;
            this.SentDate.MinimumWidth = 250;
            this.SentDate.Text = "Received";
            this.SentDate.Width = 250;
            // 
            // infolder
            // 
            this.infolder.AspectName = "Folder";
            this.infolder.FillsFreeSpace = true;
            this.infolder.MinimumWidth = 200;
            this.infolder.Text = "In Folder";
            this.infolder.Width = 200;
            // 
            // TxtboxBody
            // 
            this.TxtboxBody.BackColor = System.Drawing.SystemColors.Control;
            this.TxtboxBody.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TxtboxBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TxtboxBody.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtboxBody.Location = new System.Drawing.Point(12, 0);
            this.TxtboxBody.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.TxtboxBody.Multiline = true;
            this.TxtboxBody.Name = "TxtboxBody";
            this.TxtboxBody.ReadOnly = true;
            this.TxtboxBody.Size = new System.Drawing.Size(1269, 114);
            this.TxtboxBody.TabIndex = 6;
            this.TxtboxBody.TabStop = false;
            this.TxtboxBody.Text = "[BODY]";
            // 
            // LblAcBody
            // 
            this.LblAcBody.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblAcBody.AutoSize = true;
            this.LblAcBody.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcBody.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcBody.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblAcBody.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcBody.Location = new System.Drawing.Point(65, 213);
            this.LblAcBody.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcBody.Name = "LblAcBody";
            this.LblAcBody.Size = new System.Drawing.Size(35, 33);
            this.LblAcBody.TabIndex = 39;
            this.LblAcBody.Text = "B";
            // 
            // LblAcFolder
            // 
            this.LblAcFolder.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LblAcFolder.AutoSize = true;
            this.LblAcFolder.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcFolder.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcFolder.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcFolder.Location = new System.Drawing.Point(1390, 126);
            this.LblAcFolder.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.LblAcFolder.Name = "LblAcFolder";
            this.LblAcFolder.Size = new System.Drawing.Size(34, 33);
            this.LblAcFolder.TabIndex = 37;
            this.LblAcFolder.Text = "F";
            // 
            // LblFolder
            // 
            this.LblFolder.AutoSize = true;
            this.LblFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblFolder.Location = new System.Drawing.Point(1433, 130);
            this.LblFolder.Margin = new System.Windows.Forms.Padding(6, 14, 0, 0);
            this.LblFolder.Name = "LblFolder";
            this.LblFolder.Size = new System.Drawing.Size(86, 25);
            this.LblFolder.TabIndex = 27;
            this.LblFolder.Text = "Folder:";
            // 
            // L1h1L2v1h3Panel
            // 
            this.L1h1L2v1h3Panel.Controls.Add(this.LblAcReply);
            this.L1h1L2v1h3Panel.Controls.Add(this.LblAcReplyAll);
            this.L1h1L2v1h3Panel.Controls.Add(this.LblAcFwd);
            this.L1h1L2v1h3Panel.Controls.Add(this.BtnReply);
            this.L1h1L2v1h3Panel.Controls.Add(this.BtnReplyAll);
            this.L1h1L2v1h3Panel.Controls.Add(this.BtnForward);
            this.L1h1L2v1h3Panel.Controls.Add(this.LblAcDelete);
            this.L1h1L2v1h3Panel.Controls.Add(this.BtnDelItem);
            this.L1h1L2v1h3Panel.Controls.Add(this.LblAcPopOut);
            this.L1h1L2v1h3Panel.Controls.Add(this.BtnPopOut);
            this.L1h1L2v1h3Panel.Controls.Add(this.LblAcTask);
            this.L1h1L2v1h3Panel.Controls.Add(this.BtnFlagTask);
            this.L1h1L2v1h3Panel.Location = new System.Drawing.Point(1773, 0);
            this.L1h1L2v1h3Panel.Margin = new System.Windows.Forms.Padding(0);
            this.L1h1L2v1h3Panel.Name = "L1h1L2v1h3Panel";
            this.L0vh_Tlp.SetRowSpan(this.L1h1L2v1h3Panel, 2);
            this.L1h1L2v1h3Panel.Size = new System.Drawing.Size(610, 58);
            this.L1h1L2v1h3Panel.TabIndex = 9;
            // 
            // LblAcReply
            // 
            this.LblAcReply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblAcReply.AutoSize = true;
            this.LblAcReply.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcReply.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcReply.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblAcReply.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcReply.Location = new System.Drawing.Point(10, 0);
            this.LblAcReply.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcReply.Name = "LblAcReply";
            this.LblAcReply.Size = new System.Drawing.Size(30, 27);
            this.LblAcReply.TabIndex = 14;
            this.LblAcReply.Text = "R";
            // 
            // LblAcReplyAll
            // 
            this.LblAcReplyAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblAcReplyAll.AutoSize = true;
            this.LblAcReplyAll.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcReplyAll.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcReplyAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblAcReplyAll.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcReplyAll.Location = new System.Drawing.Point(107, 0);
            this.LblAcReplyAll.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcReplyAll.Name = "LblAcReplyAll";
            this.LblAcReplyAll.Size = new System.Drawing.Size(27, 27);
            this.LblAcReplyAll.TabIndex = 13;
            this.LblAcReplyAll.Text = "L";
            // 
            // LblAcFwd
            // 
            this.LblAcFwd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblAcFwd.AutoSize = true;
            this.LblAcFwd.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcFwd.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcFwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblAcFwd.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcFwd.Location = new System.Drawing.Point(207, 0);
            this.LblAcFwd.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcFwd.Name = "LblAcFwd";
            this.LblAcFwd.Size = new System.Drawing.Size(35, 27);
            this.LblAcFwd.TabIndex = 12;
            this.LblAcFwd.Text = "W";
            // 
            // BtnReply
            // 
            this.BtnReply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnReply.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnReply.Image = ((System.Drawing.Image)(resources.GetObject("BtnReply.Image")));
            this.BtnReply.Location = new System.Drawing.Point(7, 0);
            this.BtnReply.Margin = new System.Windows.Forms.Padding(0);
            this.BtnReply.Name = "BtnReply";
            this.BtnReply.Size = new System.Drawing.Size(100, 58);
            this.BtnReply.TabIndex = 11;
            this.BtnReply.TabStop = false;
            this.BtnReply.UseVisualStyleBackColor = true;
            // 
            // BtnReplyAll
            // 
            this.BtnReplyAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnReplyAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnReplyAll.Image = ((System.Drawing.Image)(resources.GetObject("BtnReplyAll.Image")));
            this.BtnReplyAll.Location = new System.Drawing.Point(107, 0);
            this.BtnReplyAll.Margin = new System.Windows.Forms.Padding(0);
            this.BtnReplyAll.Name = "BtnReplyAll";
            this.BtnReplyAll.Size = new System.Drawing.Size(100, 58);
            this.BtnReplyAll.TabIndex = 10;
            this.BtnReplyAll.TabStop = false;
            this.BtnReplyAll.UseVisualStyleBackColor = true;
            // 
            // BtnForward
            // 
            this.BtnForward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnForward.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnForward.Image = ((System.Drawing.Image)(resources.GetObject("BtnForward.Image")));
            this.BtnForward.Location = new System.Drawing.Point(207, 0);
            this.BtnForward.Margin = new System.Windows.Forms.Padding(0);
            this.BtnForward.Name = "BtnForward";
            this.BtnForward.Size = new System.Drawing.Size(100, 58);
            this.BtnForward.TabIndex = 9;
            this.BtnForward.TabStop = false;
            this.BtnForward.UseVisualStyleBackColor = true;
            // 
            // LblAcDelete
            // 
            this.LblAcDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblAcDelete.AutoSize = true;
            this.LblAcDelete.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcDelete.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold);
            this.LblAcDelete.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcDelete.Location = new System.Drawing.Point(507, 0);
            this.LblAcDelete.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcDelete.Name = "LblAcDelete";
            this.LblAcDelete.Size = new System.Drawing.Size(29, 27);
            this.LblAcDelete.TabIndex = 8;
            this.LblAcDelete.Text = "X";
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
            this.BtnDelItem.Location = new System.Drawing.Point(507, 0);
            this.BtnDelItem.Margin = new System.Windows.Forms.Padding(0);
            this.BtnDelItem.Name = "BtnDelItem";
            this.BtnDelItem.Size = new System.Drawing.Size(100, 58);
            this.BtnDelItem.TabIndex = 7;
            this.BtnDelItem.TabStop = false;
            this.BtnDelItem.UseVisualStyleBackColor = true;
            // 
            // LblAcPopOut
            // 
            this.LblAcPopOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblAcPopOut.AutoSize = true;
            this.LblAcPopOut.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcPopOut.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcPopOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold);
            this.LblAcPopOut.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcPopOut.Location = new System.Drawing.Point(407, 0);
            this.LblAcPopOut.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcPopOut.Name = "LblAcPopOut";
            this.LblAcPopOut.Size = new System.Drawing.Size(29, 27);
            this.LblAcPopOut.TabIndex = 6;
            this.LblAcPopOut.Text = "P";
            // 
            // BtnPopOut
            // 
            this.BtnPopOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnPopOut.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnPopOut.Image = global::QuickFiler.Properties.Resources.ApplicationFlyout;
            this.BtnPopOut.Location = new System.Drawing.Point(407, 0);
            this.BtnPopOut.Margin = new System.Windows.Forms.Padding(0);
            this.BtnPopOut.Name = "BtnPopOut";
            this.BtnPopOut.Size = new System.Drawing.Size(100, 58);
            this.BtnPopOut.TabIndex = 5;
            this.BtnPopOut.TabStop = false;
            this.BtnPopOut.UseVisualStyleBackColor = true;
            // 
            // LblAcTask
            // 
            this.LblAcTask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LblAcTask.AutoSize = true;
            this.LblAcTask.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcTask.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcTask.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblAcTask.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcTask.Location = new System.Drawing.Point(307, 0);
            this.LblAcTask.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcTask.Name = "LblAcTask";
            this.LblAcTask.Size = new System.Drawing.Size(28, 27);
            this.LblAcTask.TabIndex = 4;
            this.LblAcTask.Text = "T";
            // 
            // BtnFlagTask
            // 
            this.BtnFlagTask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnFlagTask.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnFlagTask.Image = global::QuickFiler.Properties.Resources.FlagDarkRed;
            this.BtnFlagTask.Location = new System.Drawing.Point(307, 0);
            this.BtnFlagTask.Margin = new System.Windows.Forms.Padding(0);
            this.BtnFlagTask.Name = "BtnFlagTask";
            this.BtnFlagTask.Size = new System.Drawing.Size(100, 58);
            this.BtnFlagTask.TabIndex = 3;
            this.BtnFlagTask.TabStop = false;
            this.BtnFlagTask.UseVisualStyleBackColor = true;
            // 
            // LblAcMoveOptions
            // 
            this.LblAcMoveOptions.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LblAcMoveOptions.AutoSize = true;
            this.LblAcMoveOptions.BackColor = System.Drawing.SystemColors.ControlText;
            this.LblAcMoveOptions.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblAcMoveOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Bold);
            this.LblAcMoveOptions.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.LblAcMoveOptions.Location = new System.Drawing.Point(1387, 12);
            this.LblAcMoveOptions.Margin = new System.Windows.Forms.Padding(0);
            this.LblAcMoveOptions.Name = "LblAcMoveOptions";
            this.L0vh_Tlp.SetRowSpan(this.LblAcMoveOptions, 2);
            this.LblAcMoveOptions.Size = new System.Drawing.Size(39, 33);
            this.LblAcMoveOptions.TabIndex = 10;
            this.LblAcMoveOptions.Text = "M";
            // 
            // ConversationMenuItem
            // 
            this.ConversationMenuItem.CheckOnClick = true;
            this.ConversationMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ConversationMenuItem.Image")));
            this.ConversationMenuItem.Name = "ConversationMenuItem";
            this.ConversationMenuItem.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.ConversationMenuItem.Size = new System.Drawing.Size(355, 44);
            this.ConversationMenuItem.Text = "Move &Conversation";
            // 
            // SaveAttachmentsMenuItem
            // 
            this.SaveAttachmentsMenuItem.CheckOnClick = true;
            this.SaveAttachmentsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("SaveAttachmentsMenuItem.Image")));
            this.SaveAttachmentsMenuItem.Name = "SaveAttachmentsMenuItem";
            this.SaveAttachmentsMenuItem.Size = new System.Drawing.Size(355, 44);
            this.SaveAttachmentsMenuItem.Text = "Save &Attachments";
            // 
            // SaveEmailMenuItem
            // 
            this.SaveEmailMenuItem.CheckOnClick = true;
            this.SaveEmailMenuItem.Image = global::QuickFiler.Properties.Resources.CheckBoxChecked;
            this.SaveEmailMenuItem.Name = "SaveEmailMenuItem";
            this.SaveEmailMenuItem.Size = new System.Drawing.Size(355, 44);
            this.SaveEmailMenuItem.Text = "Save E&mail Copy";
            // 
            // SavePicturesMenuItem
            // 
            this.SavePicturesMenuItem.CheckOnClick = true;
            this.SavePicturesMenuItem.Image = global::QuickFiler.Properties.Resources.CheckBoxChecked;
            this.SavePicturesMenuItem.Name = "SavePicturesMenuItem";
            this.SavePicturesMenuItem.Size = new System.Drawing.Size(355, 44);
            this.SavePicturesMenuItem.Text = "Save &Pictures";
            // 
            // ItemViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.L0vh_Tlp);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimumSize = new System.Drawing.Size(1516, 197);
            this.Name = "ItemViewer";
            this.Size = new System.Drawing.Size(2396, 907);
            this.L0vh_Tlp.ResumeLayout(false);
            this.L0vh_Tlp.PerformLayout();
            this.MoveOptionsStrip.ResumeLayout(false);
            this.MoveOptionsStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.L0v2h2_WebView2)).EndInit();
            this.L1h0L2hv3h_TlpBodyToggle.ResumeLayout(false);
            this.L1h0L2hv3h_TlpBodyToggle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TopicThread)).EndInit();
            this.L1h1L2v1h3Panel.ResumeLayout(false);
            this.L1h1L2v1h3Panel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.Label LblItemNumber;
        internal System.Windows.Forms.Label LblSender;
        internal System.Windows.Forms.Label lblCaptionTriage;
        internal System.Windows.Forms.Label LblTriage;
        internal System.Windows.Forms.Label LblCaptionPredicted;
        internal System.Windows.Forms.Label LblActionable;
        internal System.Windows.Forms.Label LblSentOn;
        internal System.Windows.Forms.Label LblSubject;
        internal System.Windows.Forms.Label LblConvCt;
        internal System.Windows.Forms.Label LblAcOpen;
        internal System.Windows.Forms.Label LblFolder;
        internal System.Windows.Forms.Label LblAcSearch;
        internal System.Windows.Forms.Label LblSearch;
        internal System.Windows.Forms.Label LblAcFolder;
        internal System.Windows.Forms.TextBox TxtboxBody;
        public BrightIdeasSoftware.FastObjectListView TopicThread;
        private BrightIdeasSoftware.OLVColumn sender;
        internal BrightIdeasSoftware.OLVColumn SentDate;
        private BrightIdeasSoftware.OLVColumn infolder;
        internal System.Windows.Forms.Label LblAcBody;
        internal Microsoft.Web.WebView2.WinForms.WebView2 L0v2h2_WebView2;
        public System.Windows.Forms.TableLayoutPanel L0vh_Tlp;
        internal System.Windows.Forms.TableLayoutPanel L1h0L2hv3h_TlpBodyToggle;
        internal System.Windows.Forms.Panel L1h1L2v1h3Panel;
        internal System.Windows.Forms.Label LblAcDelete;
        internal System.Windows.Forms.Button BtnDelItem;
        internal System.Windows.Forms.Label LblAcPopOut;
        internal System.Windows.Forms.Button BtnPopOut;
        internal System.Windows.Forms.Label LblAcTask;
        internal System.Windows.Forms.Button BtnFlagTask;
        internal System.Windows.Forms.Button BtnForward;
        internal System.Windows.Forms.Label LblAcReply;
        internal System.Windows.Forms.Label LblAcReplyAll;
        internal System.Windows.Forms.Label LblAcFwd;
        internal System.Windows.Forms.Button BtnReply;
        internal System.Windows.Forms.Button BtnReplyAll;
        internal System.Windows.Forms.ComboBox CboFolders;
        internal System.Windows.Forms.TextBox TxtboxSearch;
        internal System.Windows.Forms.MenuStrip MoveOptionsStrip;
        internal System.Windows.Forms.ToolStripMenuItem MoveOptionsMenu;
        public Viewers.ToolStripMenuItemCb ConversationMenuItem;
        public Viewers.ToolStripMenuItemCb SaveAttachmentsMenuItem;
        public Viewers.ToolStripMenuItemCb SaveEmailMenuItem;
        public Viewers.ToolStripMenuItemCb SavePicturesMenuItem;
        internal System.Windows.Forms.Label LblAcMoveOptions;
    }
}
