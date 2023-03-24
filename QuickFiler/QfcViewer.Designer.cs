using System.Diagnostics;

namespace QuickFiler
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class QfcViewer : System.Windows.Forms.UserControl
    {

        // UserControl overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is not null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            L1h = new System.Windows.Forms.SplitContainer();
            L1h1L2v = new System.Windows.Forms.TableLayoutPanel();
            L1h1L2v1L3h = new System.Windows.Forms.TableLayoutPanel();
            LblSentOn = new System.Windows.Forms.Label();
            LblSender = new System.Windows.Forms.Label();
            lblCaptionTriage = new System.Windows.Forms.Label();
            LblTriage = new System.Windows.Forms.Label();
            LblCaptionPredicted = new System.Windows.Forms.Label();
            LblActionable = new System.Windows.Forms.Label();
            L1h1L2v2L3h = new System.Windows.Forms.TableLayoutPanel();
            LblConvCt = new System.Windows.Forms.Label();
            lblSubject = new System.Windows.Forms.Label();
            TxtboxBody = new System.Windows.Forms.TextBox();
            LblPos = new System.Windows.Forms.Label();
            LblAcOpen = new System.Windows.Forms.Label();
            L1h2L2v = new System.Windows.Forms.TableLayoutPanel();
            L1h2L2v1h = new System.Windows.Forms.TableLayoutPanel();
            LblAcSearch = new System.Windows.Forms.Label();
            LblSearch = new System.Windows.Forms.Label();
            TxtboxSearch = new System.Windows.Forms.TextBox();
            L1h2L2v1h5Panel = new System.Windows.Forms.Panel();
            LblAcDelete = new System.Windows.Forms.Label();
            BtnDelItem = new System.Windows.Forms.Button();
            L1h2L2v1h4Panel = new System.Windows.Forms.Panel();
            LblAcPopOut = new System.Windows.Forms.Label();
            BtnPopOut = new System.Windows.Forms.Button();
            L1h2L2v1h3Panel = new System.Windows.Forms.Panel();
            LblAcTask = new System.Windows.Forms.Label();
            BtnFlagTask = new System.Windows.Forms.Button();
            L1h2L2v2h = new System.Windows.Forms.TableLayoutPanel();
            LblAcFolder = new System.Windows.Forms.Label();
            LblFolder = new System.Windows.Forms.Label();
            CboFolders = new System.Windows.Forms.ComboBox();
            L1h2L2v3h = new System.Windows.Forms.TableLayoutPanel();
            LblAcConversation = new System.Windows.Forms.Label();
            CbxConversation = new System.Windows.Forms.CheckBox();
            LblMoveOptions = new System.Windows.Forms.Label();
            CbxEmailCopy = new System.Windows.Forms.CheckBox();
            LblSaveOptions = new System.Windows.Forms.Label();
            CbxAttachments = new System.Windows.Forms.CheckBox();
            LblAcAttachments = new System.Windows.Forms.Label();
            LblAcEmail = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)L1h).BeginInit();
            L1h.Panel1.SuspendLayout();
            L1h.Panel2.SuspendLayout();
            L1h.SuspendLayout();
            L1h1L2v.SuspendLayout();
            L1h1L2v1L3h.SuspendLayout();
            L1h1L2v2L3h.SuspendLayout();
            L1h2L2v.SuspendLayout();
            L1h2L2v1h.SuspendLayout();
            L1h2L2v1h5Panel.SuspendLayout();
            L1h2L2v1h4Panel.SuspendLayout();
            L1h2L2v1h3Panel.SuspendLayout();
            L1h2L2v2h.SuspendLayout();
            L1h2L2v3h.SuspendLayout();
            SuspendLayout();
            // 
            // L1h
            // 
            L1h.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h.Location = new System.Drawing.Point(0, 0);
            L1h.Name = "L1h";
            // 
            // L1h.Panel1
            // 
            L1h.Panel1.Controls.Add(L1h1L2v);
            L1h.Panel1MinSize = 425;
            // 
            // L1h.Panel2
            // 
            L1h.Panel2.Controls.Add(L1h2L2v);
            L1h.Panel2MinSize = 575;
            L1h.Size = new System.Drawing.Size(1094, 105);
            L1h.SplitterDistance = 515;
            L1h.TabIndex = 1;
            // 
            // L1h1L2v
            // 
            L1h1L2v.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            L1h1L2v.ColumnCount = 2;
            L1h1L2v.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0f));
            L1h1L2v.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h1L2v.Controls.Add(L1h1L2v1L3h, 1, 0);
            L1h1L2v.Controls.Add(L1h1L2v2L3h, 1, 1);
            L1h1L2v.Controls.Add(TxtboxBody, 1, 2);
            L1h1L2v.Controls.Add(LblPos, 0, 1);
            L1h1L2v.Controls.Add(LblAcOpen, 0, 2);
            L1h1L2v.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h1L2v.Location = new System.Drawing.Point(0, 0);
            L1h1L2v.Name = "L1h1L2v";
            L1h1L2v.RowCount = 3;
            L1h1L2v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h1L2v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0f));
            L1h1L2v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h1L2v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h1L2v.Size = new System.Drawing.Size(515, 105);
            L1h1L2v.TabIndex = 0;
            // 
            // L1h1L2v1L3h
            // 
            L1h1L2v1L3h.ColumnCount = 7;
            L1h1L2v1L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0f));
            L1h1L2v1L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81.0f));
            L1h1L2v1L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27.0f));
            L1h1L2v1L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h1L2v1L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 61.0f));
            L1h1L2v1L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93.0f));
            L1h1L2v1L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0f));
            L1h1L2v1L3h.Controls.Add(LblSentOn, 6, 0);
            L1h1L2v1L3h.Controls.Add(LblSender, 0, 0);
            L1h1L2v1L3h.Controls.Add(lblCaptionTriage, 1, 0);
            L1h1L2v1L3h.Controls.Add(LblTriage, 2, 0);
            L1h1L2v1L3h.Controls.Add(LblCaptionPredicted, 4, 0);
            L1h1L2v1L3h.Controls.Add(LblActionable, 5, 0);
            L1h1L2v1L3h.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h1L2v1L3h.Location = new System.Drawing.Point(50, 0);
            L1h1L2v1L3h.Margin = new System.Windows.Forms.Padding(0);
            L1h1L2v1L3h.Name = "L1h1L2v1L3h";
            L1h1L2v1L3h.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            L1h1L2v1L3h.RowCount = 1;
            L1h1L2v1L3h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h1L2v1L3h.Size = new System.Drawing.Size(465, 20);
            L1h1L2v1L3h.TabIndex = 0;
            // 
            // LblSentOn
            // 
            LblSentOn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            LblSentOn.AutoSize = true;
            LblSentOn.Location = new System.Drawing.Point(392, 0);
            LblSentOn.Margin = new System.Windows.Forms.Padding(0);
            LblSentOn.Name = "LblSentOn";
            LblSentOn.Padding = new System.Windows.Forms.Padding(3);
            LblSentOn.Size = new System.Drawing.Size(70, 19);
            LblSentOn.TabIndex = 6;
            LblSentOn.Text = "<SENTON>";
            // 
            // _lblSender
            // 
            LblSender.AutoSize = true;
            LblSender.Location = new System.Drawing.Point(3, 0);
            LblSender.Margin = new System.Windows.Forms.Padding(0);
            LblSender.Name = "LblSender";
            LblSender.Padding = new System.Windows.Forms.Padding(3);
            LblSender.Size = new System.Drawing.Size(70, 19);
            LblSender.TabIndex = 1;
            LblSender.Text = "<SENDER>";
            // 
            // lblCaptionTriage
            // 
            lblCaptionTriage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lblCaptionTriage.AutoSize = true;
            lblCaptionTriage.Location = new System.Drawing.Point(97, 0);
            lblCaptionTriage.Margin = new System.Windows.Forms.Padding(0);
            lblCaptionTriage.Name = "lblCaptionTriage";
            lblCaptionTriage.Padding = new System.Windows.Forms.Padding(3);
            lblCaptionTriage.Size = new System.Drawing.Size(75, 19);
            lblCaptionTriage.TabIndex = 0;
            lblCaptionTriage.Text = "Triage Group";
            // 
            // LblTriage
            // 
            LblTriage.AutoSize = true;
            LblTriage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            LblTriage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblTriage.Location = new System.Drawing.Point(177, 3);
            LblTriage.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            LblTriage.Name = "LblTriage";
            LblTriage.Size = new System.Drawing.Size(17, 14);
            LblTriage.TabIndex = 2;
            LblTriage.Text = "A";
            // 
            // LblCaptionPredicted
            // 
            LblCaptionPredicted.AutoSize = true;
            LblCaptionPredicted.Location = new System.Drawing.Point(219, 0);
            LblCaptionPredicted.Margin = new System.Windows.Forms.Padding(0);
            LblCaptionPredicted.Name = "LblCaptionPredicted";
            LblCaptionPredicted.Padding = new System.Windows.Forms.Padding(3);
            LblCaptionPredicted.Size = new System.Drawing.Size(61, 19);
            LblCaptionPredicted.TabIndex = 3;
            LblCaptionPredicted.Text = "Predicted:";
            // 
            // LblActionable
            // 
            LblActionable.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            LblActionable.Dock = System.Windows.Forms.DockStyle.Fill;
            LblActionable.Location = new System.Drawing.Point(283, 3);
            LblActionable.Margin = new System.Windows.Forms.Padding(3);
            LblActionable.Name = "LblActionable";
            LblActionable.Size = new System.Drawing.Size(87, 14);
            LblActionable.TabIndex = 4;
            LblActionable.Text = "<ACTIONABL>";
            LblActionable.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // L1h1L2v2L3h
            // 
            L1h1L2v2L3h.ColumnCount = 2;
            L1h1L2v2L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h1L2v2L3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0f));
            L1h1L2v2L3h.Controls.Add(LblConvCt, 1, 0);
            L1h1L2v2L3h.Controls.Add(lblSubject, 0, 0);
            L1h1L2v2L3h.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h1L2v2L3h.Location = new System.Drawing.Point(50, 20);
            L1h1L2v2L3h.Margin = new System.Windows.Forms.Padding(0);
            L1h1L2v2L3h.Name = "L1h1L2v2L3h";
            L1h1L2v2L3h.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            L1h1L2v2L3h.RowCount = 1;
            L1h1L2v2L3h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h1L2v2L3h.Size = new System.Drawing.Size(465, 30);
            L1h1L2v2L3h.TabIndex = 3;
            // 
            // LblConvCt
            // 
            LblConvCt.AutoSize = true;
            LblConvCt.Dock = System.Windows.Forms.DockStyle.Right;
            LblConvCt.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblConvCt.Location = new System.Drawing.Point(408, 0);
            LblConvCt.Margin = new System.Windows.Forms.Padding(0);
            LblConvCt.Name = "LblConvCt";
            LblConvCt.Padding = new System.Windows.Forms.Padding(3);
            LblConvCt.Size = new System.Drawing.Size(54, 30);
            LblConvCt.TabIndex = 3;
            LblConvCt.Text = "<#>";
            // 
            // _lblSubject
            // 
            lblSubject.AutoSize = true;
            lblSubject.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblSubject.Location = new System.Drawing.Point(3, 0);
            lblSubject.Margin = new System.Windows.Forms.Padding(0);
            lblSubject.Name = "lblSubject";
            lblSubject.Padding = new System.Windows.Forms.Padding(3);
            lblSubject.Size = new System.Drawing.Size(138, 30);
            lblSubject.TabIndex = 2;
            lblSubject.Text = "<SUBJECT>";
            // 
            // TxtboxBody
            // 
            TxtboxBody.BackColor = System.Drawing.SystemColors.Control;
            TxtboxBody.BorderStyle = System.Windows.Forms.BorderStyle.None;
            TxtboxBody.Dock = System.Windows.Forms.DockStyle.Fill;
            TxtboxBody.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TxtboxBody.Location = new System.Drawing.Point(53, 53);
            TxtboxBody.Multiline = true;
            TxtboxBody.Name = "TxtboxBody";
            TxtboxBody.ReadOnly = true;
            TxtboxBody.Size = new System.Drawing.Size(459, 49);
            TxtboxBody.TabIndex = 4;
            TxtboxBody.Text = "<BODY>";
            // 
            // LblPos
            // 
            LblPos.BackColor = System.Drawing.SystemColors.HotTrack;
            LblPos.Dock = System.Windows.Forms.DockStyle.Fill;
            LblPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            LblPos.ForeColor = System.Drawing.SystemColors.HighlightText;
            LblPos.Location = new System.Drawing.Point(2, 22);
            LblPos.Margin = new System.Windows.Forms.Padding(2);
            LblPos.Name = "LblPos";
            LblPos.Size = new System.Drawing.Size(46, 26);
            LblPos.TabIndex = 5;
            LblPos.Text = "<Pos#>";
            LblPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblAcOpen
            // 
            LblAcOpen.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            LblAcOpen.AutoSize = true;
            LblAcOpen.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcOpen.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcOpen.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcOpen.Location = new System.Drawing.Point(23, 53);
            LblAcOpen.Margin = new System.Windows.Forms.Padding(3);
            LblAcOpen.Name = "LblAcOpen";
            LblAcOpen.Padding = new System.Windows.Forms.Padding(2);
            LblAcOpen.Size = new System.Drawing.Size(24, 22);
            LblAcOpen.TabIndex = 6;
            LblAcOpen.Text = "O";
            // 
            // L1h2L2v
            // 
            L1h2L2v.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            L1h2L2v.ColumnCount = 1;
            L1h2L2v.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h2L2v.Controls.Add(L1h2L2v1h, 0, 0);
            L1h2L2v.Controls.Add(L1h2L2v2h, 0, 1);
            L1h2L2v.Controls.Add(L1h2L2v3h, 0, 2);
            L1h2L2v.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h2L2v.Location = new System.Drawing.Point(0, 0);
            L1h2L2v.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            L1h2L2v.Name = "L1h2L2v";
            L1h2L2v.RowCount = 3;
            L1h2L2v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0f));
            L1h2L2v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35.0f));
            L1h2L2v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25.0f));
            L1h2L2v.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h2L2v.Size = new System.Drawing.Size(575, 105);
            L1h2L2v.TabIndex = 0;
            // 
            // L1h2L2v1h
            // 
            L1h2L2v1h.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            L1h2L2v1h.ColumnCount = 6;
            L1h2L2v1h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55.0f));
            L1h2L2v1h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h2L2v1h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h2L2v1h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0f));
            L1h2L2v1h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0f));
            L1h2L2v1h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0f));
            L1h2L2v1h.Controls.Add(LblAcSearch, 0, 0);
            L1h2L2v1h.Controls.Add(LblSearch, 0, 0);
            L1h2L2v1h.Controls.Add(TxtboxSearch, 2, 0);
            L1h2L2v1h.Controls.Add(L1h2L2v1h5Panel, 5, 0);
            L1h2L2v1h.Controls.Add(L1h2L2v1h4Panel, 4, 0);
            L1h2L2v1h.Controls.Add(L1h2L2v1h3Panel, 3, 0);
            L1h2L2v1h.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h2L2v1h.Location = new System.Drawing.Point(0, 2);
            L1h2L2v1h.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            L1h2L2v1h.Name = "L1h2L2v1h";
            L1h2L2v1h.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            L1h2L2v1h.RowCount = 1;
            L1h2L2v1h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h2L2v1h.Size = new System.Drawing.Size(575, 26);
            L1h2L2v1h.TabIndex = 0;
            // 
            // LblAcSearch
            // 
            LblAcSearch.Anchor = System.Windows.Forms.AnchorStyles.None;
            LblAcSearch.AutoSize = true;
            LblAcSearch.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcSearch.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcSearch.Location = new System.Drawing.Point(58, 4);
            LblAcSearch.Margin = new System.Windows.Forms.Padding(0);
            LblAcSearch.Name = "LblAcSearch";
            LblAcSearch.Size = new System.Drawing.Size(19, 18);
            LblAcSearch.TabIndex = 10;
            LblAcSearch.Text = "S";
            // 
            // LblSearch
            // 
            LblSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            LblSearch.AutoSize = true;
            LblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblSearch.Location = new System.Drawing.Point(3, 6);
            LblSearch.Margin = new System.Windows.Forms.Padding(0);
            LblSearch.Name = "LblSearch";
            LblSearch.Size = new System.Drawing.Size(51, 13);
            LblSearch.TabIndex = 6;
            LblSearch.Text = "Search:";
            // 
            // TxtboxSearch
            // 
            TxtboxSearch.BackColor = System.Drawing.SystemColors.Menu;
            TxtboxSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            TxtboxSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TxtboxSearch.Location = new System.Drawing.Point(78, 1);
            TxtboxSearch.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            TxtboxSearch.Name = "TxtboxSearch";
            TxtboxSearch.Size = new System.Drawing.Size(344, 24);
            TxtboxSearch.TabIndex = 3;
            // 
            // L1h2L2v1h5Panel
            // 
            L1h2L2v1h5Panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            L1h2L2v1h5Panel.Controls.Add(LblAcDelete);
            L1h2L2v1h5Panel.Controls.Add(BtnDelItem);
            L1h2L2v1h5Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h2L2v1h5Panel.Location = new System.Drawing.Point(522, 0);
            L1h2L2v1h5Panel.Margin = new System.Windows.Forms.Padding(0);
            L1h2L2v1h5Panel.Name = "L1h2L2v1h5Panel";
            L1h2L2v1h5Panel.Size = new System.Drawing.Size(50, 26);
            L1h2L2v1h5Panel.TabIndex = 7;
            // 
            // LblAcDelete
            // 
            LblAcDelete.AutoSize = true;
            LblAcDelete.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcDelete.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcDelete.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcDelete.Location = new System.Drawing.Point(0, 0);
            LblAcDelete.Margin = new System.Windows.Forms.Padding(0);
            LblAcDelete.Name = "LblAcDelete";
            LblAcDelete.Size = new System.Drawing.Size(17, 15);
            LblAcDelete.TabIndex = 2;
            LblAcDelete.Text = "X";
            // 
            // BtnDelItem
            // 
            BtnDelItem.BackColor = System.Drawing.SystemColors.Control;
            BtnDelItem.Dock = System.Windows.Forms.DockStyle.Fill;
            BtnDelItem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(255, 128, 128);
            BtnDelItem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            BtnDelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            BtnDelItem.ForeColor = System.Drawing.SystemColors.ControlText;
            BtnDelItem.Image = My.Resources.Resources.Delete;
            BtnDelItem.Location = new System.Drawing.Point(0, 0);
            BtnDelItem.Margin = new System.Windows.Forms.Padding(0);
            BtnDelItem.Name = "BtnDelItem";
            BtnDelItem.Size = new System.Drawing.Size(50, 26);
            BtnDelItem.TabIndex = 1;
            BtnDelItem.UseVisualStyleBackColor = false;
            // 
            // L1h2L2v1h4Panel
            // 
            L1h2L2v1h4Panel.Controls.Add(LblAcPopOut);
            L1h2L2v1h4Panel.Controls.Add(BtnPopOut);
            L1h2L2v1h4Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h2L2v1h4Panel.Location = new System.Drawing.Point(472, 0);
            L1h2L2v1h4Panel.Margin = new System.Windows.Forms.Padding(0);
            L1h2L2v1h4Panel.Name = "L1h2L2v1h4Panel";
            L1h2L2v1h4Panel.Size = new System.Drawing.Size(50, 26);
            L1h2L2v1h4Panel.TabIndex = 8;
            // 
            // LblAcPopOut
            // 
            LblAcPopOut.AutoSize = true;
            LblAcPopOut.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcPopOut.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcPopOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcPopOut.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcPopOut.Location = new System.Drawing.Point(0, 0);
            LblAcPopOut.Margin = new System.Windows.Forms.Padding(0);
            LblAcPopOut.Name = "LblAcPopOut";
            LblAcPopOut.Size = new System.Drawing.Size(17, 15);
            LblAcPopOut.TabIndex = 3;
            LblAcPopOut.Text = "P";
            // 
            // BtnPopOut
            // 
            BtnPopOut.Dock = System.Windows.Forms.DockStyle.Fill;
            BtnPopOut.Image = My.Resources.Resources.ApplicationFlyout;
            BtnPopOut.Location = new System.Drawing.Point(0, 0);
            BtnPopOut.Margin = new System.Windows.Forms.Padding(0);
            BtnPopOut.Name = "BtnPopOut";
            BtnPopOut.Size = new System.Drawing.Size(50, 26);
            BtnPopOut.TabIndex = 2;
            BtnPopOut.UseVisualStyleBackColor = true;
            // 
            // L1h2L2v1h3Panel
            // 
            L1h2L2v1h3Panel.Controls.Add(LblAcTask);
            L1h2L2v1h3Panel.Controls.Add(BtnFlagTask);
            L1h2L2v1h3Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h2L2v1h3Panel.Location = new System.Drawing.Point(422, 0);
            L1h2L2v1h3Panel.Margin = new System.Windows.Forms.Padding(0);
            L1h2L2v1h3Panel.Name = "L1h2L2v1h3Panel";
            L1h2L2v1h3Panel.Size = new System.Drawing.Size(50, 26);
            L1h2L2v1h3Panel.TabIndex = 9;
            // 
            // LblAcTask
            // 
            LblAcTask.AutoSize = true;
            LblAcTask.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcTask.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcTask.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcTask.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcTask.Location = new System.Drawing.Point(0, 0);
            LblAcTask.Margin = new System.Windows.Forms.Padding(0);
            LblAcTask.Name = "LblAcTask";
            LblAcTask.Size = new System.Drawing.Size(17, 15);
            LblAcTask.TabIndex = 4;
            LblAcTask.Text = "T";
            // 
            // BtnFlagTask
            // 
            BtnFlagTask.Dock = System.Windows.Forms.DockStyle.Fill;
            BtnFlagTask.Image = My.Resources.Resources.FlagDarkRed;
            BtnFlagTask.Location = new System.Drawing.Point(0, 0);
            BtnFlagTask.Margin = new System.Windows.Forms.Padding(0);
            BtnFlagTask.Name = "BtnFlagTask";
            BtnFlagTask.Size = new System.Drawing.Size(50, 26);
            BtnFlagTask.TabIndex = 3;
            BtnFlagTask.UseVisualStyleBackColor = true;
            // 
            // L1h2L2v2h
            // 
            L1h2L2v2h.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            L1h2L2v2h.ColumnCount = 3;
            L1h2L2v2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55.0f));
            L1h2L2v2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h2L2v2h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h2L2v2h.Controls.Add(LblAcFolder, 0, 0);
            L1h2L2v2h.Controls.Add(LblFolder, 0, 0);
            L1h2L2v2h.Controls.Add(CboFolders, 2, 0);
            L1h2L2v2h.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h2L2v2h.Location = new System.Drawing.Point(0, 32);
            L1h2L2v2h.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            L1h2L2v2h.Name = "L1h2L2v2h";
            L1h2L2v2h.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            L1h2L2v2h.RowCount = 1;
            L1h2L2v2h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h2L2v2h.Size = new System.Drawing.Size(575, 31);
            L1h2L2v2h.TabIndex = 1;
            // 
            // LblAcFolder
            // 
            LblAcFolder.Anchor = System.Windows.Forms.AnchorStyles.None;
            LblAcFolder.AutoSize = true;
            LblAcFolder.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcFolder.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcFolder.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcFolder.Location = new System.Drawing.Point(59, 6);
            LblAcFolder.Margin = new System.Windows.Forms.Padding(0);
            LblAcFolder.Name = "LblAcFolder";
            LblAcFolder.Size = new System.Drawing.Size(18, 18);
            LblAcFolder.TabIndex = 11;
            LblAcFolder.Text = "F";
            // 
            // LblFolder
            // 
            LblFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            LblFolder.AutoSize = true;
            LblFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblFolder.Location = new System.Drawing.Point(3, 9);
            LblFolder.Margin = new System.Windows.Forms.Padding(0);
            LblFolder.Name = "LblFolder";
            LblFolder.Size = new System.Drawing.Size(46, 13);
            LblFolder.TabIndex = 5;
            LblFolder.Text = "Folder:";
            // 
            // CboFolders
            // 
            CboFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            CboFolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CboFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.0f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            CboFolders.FormattingEnabled = true;
            CboFolders.Location = new System.Drawing.Point(78, 2);
            CboFolders.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            CboFolders.Name = "CboFolders";
            CboFolders.Size = new System.Drawing.Size(494, 28);
            CboFolders.TabIndex = 6;
            // 
            // L1h2L2v3h
            // 
            L1h2L2v3h.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            L1h2L2v3h.ColumnCount = 9;
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95.0f));
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125.0f));
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92.0f));
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95.0f));
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0f));
            L1h2L2v3h.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100.0f));
            L1h2L2v3h.Controls.Add(LblAcConversation, 0, 0);
            L1h2L2v3h.Controls.Add(CbxConversation, 2, 0);
            L1h2L2v3h.Controls.Add(LblMoveOptions, 0, 0);
            L1h2L2v3h.Controls.Add(CbxEmailCopy, 8, 0);
            L1h2L2v3h.Controls.Add(LblSaveOptions, 4, 0);
            L1h2L2v3h.Controls.Add(CbxAttachments, 6, 0);
            L1h2L2v3h.Controls.Add(LblAcAttachments, 5, 0);
            L1h2L2v3h.Controls.Add(LblAcEmail, 7, 0);
            L1h2L2v3h.Dock = System.Windows.Forms.DockStyle.Fill;
            L1h2L2v3h.Location = new System.Drawing.Point(3, 68);
            L1h2L2v3h.Name = "L1h2L2v3h";
            L1h2L2v3h.RowCount = 1;
            L1h2L2v3h.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0f));
            L1h2L2v3h.Size = new System.Drawing.Size(569, 34);
            L1h2L2v3h.TabIndex = 2;
            // 
            // LblAcConversation
            // 
            LblAcConversation.Anchor = System.Windows.Forms.AnchorStyles.Top;
            LblAcConversation.AutoSize = true;
            LblAcConversation.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcConversation.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcConversation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcConversation.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcConversation.Location = new System.Drawing.Point(95, 2);
            LblAcConversation.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            LblAcConversation.Name = "LblAcConversation";
            LblAcConversation.Size = new System.Drawing.Size(19, 18);
            LblAcConversation.TabIndex = 15;
            LblAcConversation.Text = "C";
            // 
            // CbxConversation
            // 
            CbxConversation.AutoSize = true;
            CbxConversation.Location = new System.Drawing.Point(115, 3);
            CbxConversation.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            CbxConversation.Name = "CbxConversation";
            CbxConversation.Size = new System.Drawing.Size(118, 17);
            CbxConversation.TabIndex = 13;
            CbxConversation.Text = "Entire Conversation";
            CbxConversation.UseVisualStyleBackColor = true;
            // 
            // LblMoveOptions
            // 
            LblMoveOptions.AutoSize = true;
            LblMoveOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblMoveOptions.Location = new System.Drawing.Point(3, 4);
            LblMoveOptions.Margin = new System.Windows.Forms.Padding(3, 4, 0, 3);
            LblMoveOptions.Name = "LblMoveOptions";
            LblMoveOptions.Size = new System.Drawing.Size(89, 13);
            LblMoveOptions.TabIndex = 10;
            LblMoveOptions.Text = "Move Options:";
            // 
            // CbxEmailCopy
            // 
            CbxEmailCopy.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            CbxEmailCopy.AutoSize = true;
            CbxEmailCopy.Location = new System.Drawing.Point(476, 3);
            CbxEmailCopy.Name = "CbxEmailCopy";
            CbxEmailCopy.Size = new System.Drawing.Size(90, 17);
            CbxEmailCopy.TabIndex = 6;
            CbxEmailCopy.Text = "Copy of Email";
            CbxEmailCopy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            CbxEmailCopy.UseVisualStyleBackColor = true;
            // 
            // LblSaveOptions
            // 
            LblSaveOptions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            LblSaveOptions.AutoSize = true;
            LblSaveOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblSaveOptions.Location = new System.Drawing.Point(247, 4);
            LblSaveOptions.Margin = new System.Windows.Forms.Padding(3, 4, 0, 3);
            LblSaveOptions.Name = "LblSaveOptions";
            LblSaveOptions.Size = new System.Drawing.Size(87, 13);
            LblSaveOptions.TabIndex = 8;
            LblSaveOptions.Text = "Save Options:";
            // 
            // CbxAttachments
            // 
            CbxAttachments.AutoSize = true;
            CbxAttachments.Location = new System.Drawing.Point(357, 3);
            CbxAttachments.Name = "CbxAttachments";
            CbxAttachments.Size = new System.Drawing.Size(85, 17);
            CbxAttachments.TabIndex = 12;
            CbxAttachments.Text = "Attachments";
            CbxAttachments.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            CbxAttachments.UseVisualStyleBackColor = true;
            // 
            // LblAcAttachments
            // 
            LblAcAttachments.Anchor = System.Windows.Forms.AnchorStyles.Top;
            LblAcAttachments.AutoSize = true;
            LblAcAttachments.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcAttachments.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcAttachments.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcAttachments.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcAttachments.Location = new System.Drawing.Point(334, 2);
            LblAcAttachments.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            LblAcAttachments.Name = "LblAcAttachments";
            LblAcAttachments.Size = new System.Drawing.Size(19, 18);
            LblAcAttachments.TabIndex = 14;
            LblAcAttachments.Text = "A";
            // 
            // LblAcEmail
            // 
            LblAcEmail.Anchor = System.Windows.Forms.AnchorStyles.Top;
            LblAcEmail.AutoSize = true;
            LblAcEmail.BackColor = System.Drawing.SystemColors.ControlText;
            LblAcEmail.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblAcEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblAcEmail.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblAcEmail.Location = new System.Drawing.Point(449, 2);
            LblAcEmail.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            LblAcEmail.Name = "LblAcEmail";
            LblAcEmail.Size = new System.Drawing.Size(19, 18);
            LblAcEmail.TabIndex = 16;
            LblAcEmail.Text = "E";
            // 
            // ControlGroup
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            Controls.Add(L1h);
            Name = "ControlGroup";
            Size = new System.Drawing.Size(1094, 105);
            L1h.Panel1.ResumeLayout(false);
            L1h.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)L1h).EndInit();
            L1h.ResumeLayout(false);
            L1h1L2v.ResumeLayout(false);
            L1h1L2v.PerformLayout();
            L1h1L2v1L3h.ResumeLayout(false);
            L1h1L2v1L3h.PerformLayout();
            L1h1L2v2L3h.ResumeLayout(false);
            L1h1L2v2L3h.PerformLayout();
            L1h2L2v.ResumeLayout(false);
            L1h2L2v1h.ResumeLayout(false);
            L1h2L2v1h.PerformLayout();
            L1h2L2v1h5Panel.ResumeLayout(false);
            L1h2L2v1h5Panel.PerformLayout();
            L1h2L2v1h4Panel.ResumeLayout(false);
            L1h2L2v1h4Panel.PerformLayout();
            L1h2L2v1h3Panel.ResumeLayout(false);
            L1h2L2v1h3Panel.PerformLayout();
            L1h2L2v2h.ResumeLayout(false);
            L1h2L2v2h.PerformLayout();
            L1h2L2v3h.ResumeLayout(false);
            L1h2L2v3h.PerformLayout();
            Paint += new System.Windows.Forms.PaintEventHandler(ControlGroup_Paint);
            ResumeLayout(false);

        }
        internal System.Windows.Forms.SplitContainer L1h;
        internal System.Windows.Forms.TableLayoutPanel L1h2L2v;
        internal System.Windows.Forms.TableLayoutPanel L1h2L2v1h;
        internal System.Windows.Forms.TableLayoutPanel L1h2L2v2h;
        internal System.Windows.Forms.TextBox TxtboxSearch;
        internal System.Windows.Forms.Label LblFolder;
        internal System.Windows.Forms.Label LblSearch;
        internal System.Windows.Forms.ComboBox CboFolders;
        internal System.Windows.Forms.TableLayoutPanel L1h2L2v3h;
        internal System.Windows.Forms.Label LblMoveOptions;
        internal System.Windows.Forms.CheckBox CbxEmailCopy;
        internal System.Windows.Forms.Label LblSaveOptions;
        internal System.Windows.Forms.TableLayoutPanel L1h1L2v;
        internal System.Windows.Forms.TableLayoutPanel L1h1L2v1L3h;
        internal System.Windows.Forms.Label LblSentOn;
        internal System.Windows.Forms.Label LblSender;
        internal System.Windows.Forms.Label lblCaptionTriage;
        internal System.Windows.Forms.Label LblTriage;
        internal System.Windows.Forms.Label LblCaptionPredicted;
        internal System.Windows.Forms.Label LblActionable;
        internal System.Windows.Forms.TableLayoutPanel L1h1L2v2L3h;
        internal System.Windows.Forms.Label LblConvCt;
        internal System.Windows.Forms.Label lblSubject;
        internal System.Windows.Forms.TextBox TxtboxBody;
        internal System.Windows.Forms.Label LblPos;
        internal System.Windows.Forms.Label LblAcOpen;
        internal System.Windows.Forms.Panel L1h2L2v1h5Panel;
        internal System.Windows.Forms.Panel L1h2L2v1h4Panel;
        internal System.Windows.Forms.Label LblAcPopOut;
        internal System.Windows.Forms.Button BtnPopOut;
        internal System.Windows.Forms.Label LblAcDelete;
        internal System.Windows.Forms.Button BtnDelItem;
        internal System.Windows.Forms.Label LblAcSearch;
        internal System.Windows.Forms.Panel L1h2L2v1h3Panel;
        internal System.Windows.Forms.Label LblAcTask;
        internal System.Windows.Forms.Button BtnFlagTask;
        internal System.Windows.Forms.Label LblAcFolder;
        internal System.Windows.Forms.Label LblAcConversation;
        internal System.Windows.Forms.CheckBox CbxConversation;
        internal System.Windows.Forms.CheckBox CbxAttachments;
        internal System.Windows.Forms.Label LblAcAttachments;
        internal System.Windows.Forms.Label LblAcEmail;
    }
}