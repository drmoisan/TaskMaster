<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class QfcViewer
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.L1h = New System.Windows.Forms.SplitContainer()
        Me.L1h1L2v = New System.Windows.Forms.TableLayoutPanel()
        Me.L1h1L2v1L3h = New System.Windows.Forms.TableLayoutPanel()
        Me.LblSentOn = New System.Windows.Forms.Label()
        Me.LblSender = New System.Windows.Forms.Label()
        Me.lblCaptionTriage = New System.Windows.Forms.Label()
        Me.LblTriage = New System.Windows.Forms.Label()
        Me.LblCaptionPredicted = New System.Windows.Forms.Label()
        Me.LblActionable = New System.Windows.Forms.Label()
        Me.L1h1L2v2L3h = New System.Windows.Forms.TableLayoutPanel()
        Me.LblConvCt = New System.Windows.Forms.Label()
        Me.lblSubject = New System.Windows.Forms.Label()
        Me.TxtboxBody = New System.Windows.Forms.TextBox()
        Me.LblPos = New System.Windows.Forms.Label()
        Me.LblAcOpen = New System.Windows.Forms.Label()
        Me.L1h2L2v = New System.Windows.Forms.TableLayoutPanel()
        Me.L1h2L2v1h = New System.Windows.Forms.TableLayoutPanel()
        Me.LblAcSearch = New System.Windows.Forms.Label()
        Me.LblSearch = New System.Windows.Forms.Label()
        Me.TxtboxSearch = New System.Windows.Forms.TextBox()
        Me.L1h2L2v1h5Panel = New System.Windows.Forms.Panel()
        Me.LblAcDelete = New System.Windows.Forms.Label()
        Me.BtnDelItem = New System.Windows.Forms.Button()
        Me.L1h2L2v1h4Panel = New System.Windows.Forms.Panel()
        Me.LblAcPopOut = New System.Windows.Forms.Label()
        Me.BtnPopOut = New System.Windows.Forms.Button()
        Me.L1h2L2v1h3Panel = New System.Windows.Forms.Panel()
        Me.LblAcTask = New System.Windows.Forms.Label()
        Me.BtnFlagTask = New System.Windows.Forms.Button()
        Me.L1h2L2v2h = New System.Windows.Forms.TableLayoutPanel()
        Me.LblAcFolder = New System.Windows.Forms.Label()
        Me.LblFolder = New System.Windows.Forms.Label()
        Me.CboFolders = New System.Windows.Forms.ComboBox()
        Me.L1h2L2v3h = New System.Windows.Forms.TableLayoutPanel()
        Me.LblAcConversation = New System.Windows.Forms.Label()
        Me.CbxConversation = New System.Windows.Forms.CheckBox()
        Me.LblMoveOptions = New System.Windows.Forms.Label()
        Me.CbxEmailCopy = New System.Windows.Forms.CheckBox()
        Me.LblSaveOptions = New System.Windows.Forms.Label()
        Me.CbxAttachments = New System.Windows.Forms.CheckBox()
        Me.LblAcAttachments = New System.Windows.Forms.Label()
        Me.LblAcEmail = New System.Windows.Forms.Label()
        CType(Me.L1h, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.L1h.Panel1.SuspendLayout()
        Me.L1h.Panel2.SuspendLayout()
        Me.L1h.SuspendLayout()
        Me.L1h1L2v.SuspendLayout()
        Me.L1h1L2v1L3h.SuspendLayout()
        Me.L1h1L2v2L3h.SuspendLayout()
        Me.L1h2L2v.SuspendLayout()
        Me.L1h2L2v1h.SuspendLayout()
        Me.L1h2L2v1h5Panel.SuspendLayout()
        Me.L1h2L2v1h4Panel.SuspendLayout()
        Me.L1h2L2v1h3Panel.SuspendLayout()
        Me.L1h2L2v2h.SuspendLayout()
        Me.L1h2L2v3h.SuspendLayout()
        Me.SuspendLayout()
        '
        'L1h
        '
        Me.L1h.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h.Location = New System.Drawing.Point(0, 0)
        Me.L1h.Name = "L1h"
        '
        'L1h.Panel1
        '
        Me.L1h.Panel1.Controls.Add(Me.L1h1L2v)
        Me.L1h.Panel1MinSize = 425
        '
        'L1h.Panel2
        '
        Me.L1h.Panel2.Controls.Add(Me.L1h2L2v)
        Me.L1h.Panel2MinSize = 575
        Me.L1h.Size = New System.Drawing.Size(1094, 105)
        Me.L1h.SplitterDistance = 515
        Me.L1h.TabIndex = 1
        '
        'L1h1L2v
        '
        Me.L1h1L2v.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.L1h1L2v.ColumnCount = 2
        Me.L1h1L2v.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        Me.L1h1L2v.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h1L2v.Controls.Add(Me.L1h1L2v1L3h, 1, 0)
        Me.L1h1L2v.Controls.Add(Me.L1h1L2v2L3h, 1, 1)
        Me.L1h1L2v.Controls.Add(Me.TxtboxBody, 1, 2)
        Me.L1h1L2v.Controls.Add(Me.LblPos, 0, 1)
        Me.L1h1L2v.Controls.Add(Me.LblAcOpen, 0, 2)
        Me.L1h1L2v.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h1L2v.Location = New System.Drawing.Point(0, 0)
        Me.L1h1L2v.Name = "L1h1L2v"
        Me.L1h1L2v.RowCount = 3
        Me.L1h1L2v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h1L2v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.L1h1L2v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h1L2v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h1L2v.Size = New System.Drawing.Size(515, 105)
        Me.L1h1L2v.TabIndex = 0
        '
        'L1h1L2v1L3h
        '
        Me.L1h1L2v1L3h.ColumnCount = 7
        Me.L1h1L2v1L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.L1h1L2v1L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81.0!))
        Me.L1h1L2v1L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27.0!))
        Me.L1h1L2v1L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h1L2v1L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 61.0!))
        Me.L1h1L2v1L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93.0!))
        Me.L1h1L2v1L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.L1h1L2v1L3h.Controls.Add(Me.LblSentOn, 6, 0)
        Me.L1h1L2v1L3h.Controls.Add(Me.LblSender, 0, 0)
        Me.L1h1L2v1L3h.Controls.Add(Me.lblCaptionTriage, 1, 0)
        Me.L1h1L2v1L3h.Controls.Add(Me.LblTriage, 2, 0)
        Me.L1h1L2v1L3h.Controls.Add(Me.LblCaptionPredicted, 4, 0)
        Me.L1h1L2v1L3h.Controls.Add(Me.LblActionable, 5, 0)
        Me.L1h1L2v1L3h.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h1L2v1L3h.Location = New System.Drawing.Point(50, 0)
        Me.L1h1L2v1L3h.Margin = New System.Windows.Forms.Padding(0)
        Me.L1h1L2v1L3h.Name = "L1h1L2v1L3h"
        Me.L1h1L2v1L3h.Padding = New System.Windows.Forms.Padding(3, 0, 3, 0)
        Me.L1h1L2v1L3h.RowCount = 1
        Me.L1h1L2v1L3h.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h1L2v1L3h.Size = New System.Drawing.Size(465, 20)
        Me.L1h1L2v1L3h.TabIndex = 0
        '
        'LblSentOn
        '
        Me.LblSentOn.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblSentOn.AutoSize = True
        Me.LblSentOn.Location = New System.Drawing.Point(392, 0)
        Me.LblSentOn.Margin = New System.Windows.Forms.Padding(0)
        Me.LblSentOn.Name = "LblSentOn"
        Me.LblSentOn.Padding = New System.Windows.Forms.Padding(3)
        Me.LblSentOn.Size = New System.Drawing.Size(70, 19)
        Me.LblSentOn.TabIndex = 6
        Me.LblSentOn.Text = "<SENTON>"
        '
        '_lblSender
        '
        Me.LblSender.AutoSize = True
        Me.LblSender.Location = New System.Drawing.Point(3, 0)
        Me.LblSender.Margin = New System.Windows.Forms.Padding(0)
        Me.LblSender.Name = "LblSender"
        Me.LblSender.Padding = New System.Windows.Forms.Padding(3)
        Me.LblSender.Size = New System.Drawing.Size(70, 19)
        Me.LblSender.TabIndex = 1
        Me.LblSender.Text = "<SENDER>"
        '
        'lblCaptionTriage
        '
        Me.lblCaptionTriage.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblCaptionTriage.AutoSize = True
        Me.lblCaptionTriage.Location = New System.Drawing.Point(97, 0)
        Me.lblCaptionTriage.Margin = New System.Windows.Forms.Padding(0)
        Me.lblCaptionTriage.Name = "lblCaptionTriage"
        Me.lblCaptionTriage.Padding = New System.Windows.Forms.Padding(3)
        Me.lblCaptionTriage.Size = New System.Drawing.Size(75, 19)
        Me.lblCaptionTriage.TabIndex = 0
        Me.lblCaptionTriage.Text = "Triage Group"
        '
        'LblTriage
        '
        Me.LblTriage.AutoSize = True
        Me.LblTriage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LblTriage.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTriage.Location = New System.Drawing.Point(177, 3)
        Me.LblTriage.Margin = New System.Windows.Forms.Padding(5, 3, 5, 3)
        Me.LblTriage.Name = "LblTriage"
        Me.LblTriage.Size = New System.Drawing.Size(17, 14)
        Me.LblTriage.TabIndex = 2
        Me.LblTriage.Text = "A"
        '
        'LblCaptionPredicted
        '
        Me.LblCaptionPredicted.AutoSize = True
        Me.LblCaptionPredicted.Location = New System.Drawing.Point(219, 0)
        Me.LblCaptionPredicted.Margin = New System.Windows.Forms.Padding(0)
        Me.LblCaptionPredicted.Name = "LblCaptionPredicted"
        Me.LblCaptionPredicted.Padding = New System.Windows.Forms.Padding(3)
        Me.LblCaptionPredicted.Size = New System.Drawing.Size(61, 19)
        Me.LblCaptionPredicted.TabIndex = 3
        Me.LblCaptionPredicted.Text = "Predicted:"
        '
        'LblActionable
        '
        Me.LblActionable.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LblActionable.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LblActionable.Location = New System.Drawing.Point(283, 3)
        Me.LblActionable.Margin = New System.Windows.Forms.Padding(3)
        Me.LblActionable.Name = "LblActionable"
        Me.LblActionable.Size = New System.Drawing.Size(87, 14)
        Me.LblActionable.TabIndex = 4
        Me.LblActionable.Text = "<ACTIONABL>"
        Me.LblActionable.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'L1h1L2v2L3h
        '
        Me.L1h1L2v2L3h.ColumnCount = 2
        Me.L1h1L2v2L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h1L2v2L3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0!))
        Me.L1h1L2v2L3h.Controls.Add(Me.LblConvCt, 1, 0)
        Me.L1h1L2v2L3h.Controls.Add(Me.lblSubject, 0, 0)
        Me.L1h1L2v2L3h.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h1L2v2L3h.Location = New System.Drawing.Point(50, 20)
        Me.L1h1L2v2L3h.Margin = New System.Windows.Forms.Padding(0)
        Me.L1h1L2v2L3h.Name = "L1h1L2v2L3h"
        Me.L1h1L2v2L3h.Padding = New System.Windows.Forms.Padding(3, 0, 3, 0)
        Me.L1h1L2v2L3h.RowCount = 1
        Me.L1h1L2v2L3h.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h1L2v2L3h.Size = New System.Drawing.Size(465, 30)
        Me.L1h1L2v2L3h.TabIndex = 3
        '
        'LblConvCt
        '
        Me.LblConvCt.AutoSize = True
        Me.LblConvCt.Dock = System.Windows.Forms.DockStyle.Right
        Me.LblConvCt.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblConvCt.Location = New System.Drawing.Point(408, 0)
        Me.LblConvCt.Margin = New System.Windows.Forms.Padding(0)
        Me.LblConvCt.Name = "LblConvCt"
        Me.LblConvCt.Padding = New System.Windows.Forms.Padding(3)
        Me.LblConvCt.Size = New System.Drawing.Size(54, 30)
        Me.LblConvCt.TabIndex = 3
        Me.LblConvCt.Text = "<#>"
        '
        '_lblSubject
        '
        Me.lblSubject.AutoSize = True
        Me.lblSubject.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSubject.Location = New System.Drawing.Point(3, 0)
        Me.lblSubject.Margin = New System.Windows.Forms.Padding(0)
        Me.lblSubject.Name = "lblSubject"
        Me.lblSubject.Padding = New System.Windows.Forms.Padding(3)
        Me.lblSubject.Size = New System.Drawing.Size(138, 30)
        Me.lblSubject.TabIndex = 2
        Me.lblSubject.Text = "<SUBJECT>"
        '
        'TxtboxBody
        '
        Me.TxtboxBody.BackColor = System.Drawing.SystemColors.Control
        Me.TxtboxBody.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.TxtboxBody.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TxtboxBody.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtboxBody.Location = New System.Drawing.Point(53, 53)
        Me.TxtboxBody.Multiline = True
        Me.TxtboxBody.Name = "TxtboxBody"
        Me.TxtboxBody.ReadOnly = True
        Me.TxtboxBody.Size = New System.Drawing.Size(459, 49)
        Me.TxtboxBody.TabIndex = 4
        Me.TxtboxBody.Text = "<BODY>"
        '
        'LblPos
        '
        Me.LblPos.BackColor = System.Drawing.SystemColors.HotTrack
        Me.LblPos.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LblPos.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblPos.ForeColor = System.Drawing.SystemColors.HighlightText
        Me.LblPos.Location = New System.Drawing.Point(2, 22)
        Me.LblPos.Margin = New System.Windows.Forms.Padding(2)
        Me.LblPos.Name = "LblPos"
        Me.LblPos.Size = New System.Drawing.Size(46, 26)
        Me.LblPos.TabIndex = 5
        Me.LblPos.Text = "<Pos#>"
        Me.LblPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LblAcOpen
        '
        Me.LblAcOpen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblAcOpen.AutoSize = True
        Me.LblAcOpen.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcOpen.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcOpen.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcOpen.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcOpen.Location = New System.Drawing.Point(23, 53)
        Me.LblAcOpen.Margin = New System.Windows.Forms.Padding(3)
        Me.LblAcOpen.Name = "LblAcOpen"
        Me.LblAcOpen.Padding = New System.Windows.Forms.Padding(2)
        Me.LblAcOpen.Size = New System.Drawing.Size(24, 22)
        Me.LblAcOpen.TabIndex = 6
        Me.LblAcOpen.Text = "O"
        '
        'L1h2L2v
        '
        Me.L1h2L2v.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.L1h2L2v.ColumnCount = 1
        Me.L1h2L2v.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h2L2v.Controls.Add(Me.L1h2L2v1h, 0, 0)
        Me.L1h2L2v.Controls.Add(Me.L1h2L2v2h, 0, 1)
        Me.L1h2L2v.Controls.Add(Me.L1h2L2v3h, 0, 2)
        Me.L1h2L2v.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h2L2v.Location = New System.Drawing.Point(0, 0)
        Me.L1h2L2v.Margin = New System.Windows.Forms.Padding(0, 0, 3, 0)
        Me.L1h2L2v.Name = "L1h2L2v"
        Me.L1h2L2v.RowCount = 3
        Me.L1h2L2v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.L1h2L2v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35.0!))
        Me.L1h2L2v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25.0!))
        Me.L1h2L2v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h2L2v.Size = New System.Drawing.Size(575, 105)
        Me.L1h2L2v.TabIndex = 0
        '
        'L1h2L2v1h
        '
        Me.L1h2L2v1h.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.L1h2L2v1h.ColumnCount = 6
        Me.L1h2L2v1h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55.0!))
        Me.L1h2L2v1h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h2L2v1h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h2L2v1h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        Me.L1h2L2v1h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        Me.L1h2L2v1h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        Me.L1h2L2v1h.Controls.Add(Me.LblAcSearch, 0, 0)
        Me.L1h2L2v1h.Controls.Add(Me.LblSearch, 0, 0)
        Me.L1h2L2v1h.Controls.Add(Me.TxtboxSearch, 2, 0)
        Me.L1h2L2v1h.Controls.Add(Me.L1h2L2v1h5Panel, 5, 0)
        Me.L1h2L2v1h.Controls.Add(Me.L1h2L2v1h4Panel, 4, 0)
        Me.L1h2L2v1h.Controls.Add(Me.L1h2L2v1h3Panel, 3, 0)
        Me.L1h2L2v1h.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h2L2v1h.Location = New System.Drawing.Point(0, 2)
        Me.L1h2L2v1h.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        Me.L1h2L2v1h.Name = "L1h2L2v1h"
        Me.L1h2L2v1h.Padding = New System.Windows.Forms.Padding(3, 0, 3, 0)
        Me.L1h2L2v1h.RowCount = 1
        Me.L1h2L2v1h.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h2L2v1h.Size = New System.Drawing.Size(575, 26)
        Me.L1h2L2v1h.TabIndex = 0
        '
        'LblAcSearch
        '
        Me.LblAcSearch.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.LblAcSearch.AutoSize = True
        Me.LblAcSearch.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcSearch.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcSearch.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcSearch.Location = New System.Drawing.Point(58, 4)
        Me.LblAcSearch.Margin = New System.Windows.Forms.Padding(0)
        Me.LblAcSearch.Name = "LblAcSearch"
        Me.LblAcSearch.Size = New System.Drawing.Size(19, 18)
        Me.LblAcSearch.TabIndex = 10
        Me.LblAcSearch.Text = "S"
        '
        'LblSearch
        '
        Me.LblSearch.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LblSearch.AutoSize = True
        Me.LblSearch.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblSearch.Location = New System.Drawing.Point(3, 6)
        Me.LblSearch.Margin = New System.Windows.Forms.Padding(0)
        Me.LblSearch.Name = "LblSearch"
        Me.LblSearch.Size = New System.Drawing.Size(51, 13)
        Me.LblSearch.TabIndex = 6
        Me.LblSearch.Text = "Search:"
        '
        'TxtboxSearch
        '
        Me.TxtboxSearch.BackColor = System.Drawing.SystemColors.Menu
        Me.TxtboxSearch.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TxtboxSearch.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtboxSearch.Location = New System.Drawing.Point(78, 1)
        Me.TxtboxSearch.Margin = New System.Windows.Forms.Padding(0, 1, 0, 1)
        Me.TxtboxSearch.Name = "TxtboxSearch"
        Me.TxtboxSearch.Size = New System.Drawing.Size(344, 24)
        Me.TxtboxSearch.TabIndex = 3
        '
        'L1h2L2v1h5Panel
        '
        Me.L1h2L2v1h5Panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.L1h2L2v1h5Panel.Controls.Add(Me.LblAcDelete)
        Me.L1h2L2v1h5Panel.Controls.Add(Me.BtnDelItem)
        Me.L1h2L2v1h5Panel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h2L2v1h5Panel.Location = New System.Drawing.Point(522, 0)
        Me.L1h2L2v1h5Panel.Margin = New System.Windows.Forms.Padding(0)
        Me.L1h2L2v1h5Panel.Name = "L1h2L2v1h5Panel"
        Me.L1h2L2v1h5Panel.Size = New System.Drawing.Size(50, 26)
        Me.L1h2L2v1h5Panel.TabIndex = 7
        '
        'LblAcDelete
        '
        Me.LblAcDelete.AutoSize = True
        Me.LblAcDelete.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcDelete.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcDelete.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcDelete.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcDelete.Location = New System.Drawing.Point(0, 0)
        Me.LblAcDelete.Margin = New System.Windows.Forms.Padding(0)
        Me.LblAcDelete.Name = "LblAcDelete"
        Me.LblAcDelete.Size = New System.Drawing.Size(17, 15)
        Me.LblAcDelete.TabIndex = 2
        Me.LblAcDelete.Text = "X"
        '
        'BtnDelItem
        '
        Me.BtnDelItem.BackColor = System.Drawing.SystemColors.Control
        Me.BtnDelItem.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BtnDelItem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.BtnDelItem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red
        Me.BtnDelItem.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnDelItem.ForeColor = System.Drawing.SystemColors.ControlText
        Me.BtnDelItem.Image = Global.QuickFiler.My.Resources.Resources.Delete
        Me.BtnDelItem.Location = New System.Drawing.Point(0, 0)
        Me.BtnDelItem.Margin = New System.Windows.Forms.Padding(0)
        Me.BtnDelItem.Name = "BtnDelItem"
        Me.BtnDelItem.Size = New System.Drawing.Size(50, 26)
        Me.BtnDelItem.TabIndex = 1
        Me.BtnDelItem.UseVisualStyleBackColor = False
        '
        'L1h2L2v1h4Panel
        '
        Me.L1h2L2v1h4Panel.Controls.Add(Me.LblAcPopOut)
        Me.L1h2L2v1h4Panel.Controls.Add(Me.BtnPopOut)
        Me.L1h2L2v1h4Panel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h2L2v1h4Panel.Location = New System.Drawing.Point(472, 0)
        Me.L1h2L2v1h4Panel.Margin = New System.Windows.Forms.Padding(0)
        Me.L1h2L2v1h4Panel.Name = "L1h2L2v1h4Panel"
        Me.L1h2L2v1h4Panel.Size = New System.Drawing.Size(50, 26)
        Me.L1h2L2v1h4Panel.TabIndex = 8
        '
        'LblAcPopOut
        '
        Me.LblAcPopOut.AutoSize = True
        Me.LblAcPopOut.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcPopOut.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcPopOut.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcPopOut.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcPopOut.Location = New System.Drawing.Point(0, 0)
        Me.LblAcPopOut.Margin = New System.Windows.Forms.Padding(0)
        Me.LblAcPopOut.Name = "LblAcPopOut"
        Me.LblAcPopOut.Size = New System.Drawing.Size(17, 15)
        Me.LblAcPopOut.TabIndex = 3
        Me.LblAcPopOut.Text = "P"
        '
        'BtnPopOut
        '
        Me.BtnPopOut.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BtnPopOut.Image = Global.QuickFiler.My.Resources.Resources.ApplicationFlyout
        Me.BtnPopOut.Location = New System.Drawing.Point(0, 0)
        Me.BtnPopOut.Margin = New System.Windows.Forms.Padding(0)
        Me.BtnPopOut.Name = "BtnPopOut"
        Me.BtnPopOut.Size = New System.Drawing.Size(50, 26)
        Me.BtnPopOut.TabIndex = 2
        Me.BtnPopOut.UseVisualStyleBackColor = True
        '
        'L1h2L2v1h3Panel
        '
        Me.L1h2L2v1h3Panel.Controls.Add(Me.LblAcTask)
        Me.L1h2L2v1h3Panel.Controls.Add(Me.BtnFlagTask)
        Me.L1h2L2v1h3Panel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h2L2v1h3Panel.Location = New System.Drawing.Point(422, 0)
        Me.L1h2L2v1h3Panel.Margin = New System.Windows.Forms.Padding(0)
        Me.L1h2L2v1h3Panel.Name = "L1h2L2v1h3Panel"
        Me.L1h2L2v1h3Panel.Size = New System.Drawing.Size(50, 26)
        Me.L1h2L2v1h3Panel.TabIndex = 9
        '
        'LblAcTask
        '
        Me.LblAcTask.AutoSize = True
        Me.LblAcTask.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcTask.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcTask.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcTask.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcTask.Location = New System.Drawing.Point(0, 0)
        Me.LblAcTask.Margin = New System.Windows.Forms.Padding(0)
        Me.LblAcTask.Name = "LblAcTask"
        Me.LblAcTask.Size = New System.Drawing.Size(17, 15)
        Me.LblAcTask.TabIndex = 4
        Me.LblAcTask.Text = "T"
        '
        'BtnFlagTask
        '
        Me.BtnFlagTask.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BtnFlagTask.Image = Global.QuickFiler.My.Resources.Resources.FlagDarkRed
        Me.BtnFlagTask.Location = New System.Drawing.Point(0, 0)
        Me.BtnFlagTask.Margin = New System.Windows.Forms.Padding(0)
        Me.BtnFlagTask.Name = "BtnFlagTask"
        Me.BtnFlagTask.Size = New System.Drawing.Size(50, 26)
        Me.BtnFlagTask.TabIndex = 3
        Me.BtnFlagTask.UseVisualStyleBackColor = True
        '
        'L1h2L2v2h
        '
        Me.L1h2L2v2h.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.L1h2L2v2h.ColumnCount = 3
        Me.L1h2L2v2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55.0!))
        Me.L1h2L2v2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h2L2v2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h2L2v2h.Controls.Add(Me.LblAcFolder, 0, 0)
        Me.L1h2L2v2h.Controls.Add(Me.LblFolder, 0, 0)
        Me.L1h2L2v2h.Controls.Add(Me.CboFolders, 2, 0)
        Me.L1h2L2v2h.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h2L2v2h.Location = New System.Drawing.Point(0, 32)
        Me.L1h2L2v2h.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        Me.L1h2L2v2h.Name = "L1h2L2v2h"
        Me.L1h2L2v2h.Padding = New System.Windows.Forms.Padding(3, 0, 3, 0)
        Me.L1h2L2v2h.RowCount = 1
        Me.L1h2L2v2h.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h2L2v2h.Size = New System.Drawing.Size(575, 31)
        Me.L1h2L2v2h.TabIndex = 1
        '
        'LblAcFolder
        '
        Me.LblAcFolder.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.LblAcFolder.AutoSize = True
        Me.LblAcFolder.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcFolder.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcFolder.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcFolder.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcFolder.Location = New System.Drawing.Point(59, 6)
        Me.LblAcFolder.Margin = New System.Windows.Forms.Padding(0)
        Me.LblAcFolder.Name = "LblAcFolder"
        Me.LblAcFolder.Size = New System.Drawing.Size(18, 18)
        Me.LblAcFolder.TabIndex = 11
        Me.LblAcFolder.Text = "F"
        '
        'LblFolder
        '
        Me.LblFolder.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LblFolder.AutoSize = True
        Me.LblFolder.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblFolder.Location = New System.Drawing.Point(3, 9)
        Me.LblFolder.Margin = New System.Windows.Forms.Padding(0)
        Me.LblFolder.Name = "LblFolder"
        Me.LblFolder.Size = New System.Drawing.Size(46, 13)
        Me.LblFolder.TabIndex = 5
        Me.LblFolder.Text = "Folder:"
        '
        'CboFolders
        '
        Me.CboFolders.Dock = System.Windows.Forms.DockStyle.Fill
        Me.CboFolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CboFolders.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CboFolders.FormattingEnabled = True
        Me.CboFolders.Location = New System.Drawing.Point(78, 2)
        Me.CboFolders.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        Me.CboFolders.Name = "CboFolders"
        Me.CboFolders.Size = New System.Drawing.Size(494, 28)
        Me.CboFolders.TabIndex = 6
        '
        'L1h2L2v3h
        '
        Me.L1h2L2v3h.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.L1h2L2v3h.ColumnCount = 9
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95.0!))
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125.0!))
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92.0!))
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95.0!))
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.L1h2L2v3h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100.0!))
        Me.L1h2L2v3h.Controls.Add(Me.LblAcConversation, 0, 0)
        Me.L1h2L2v3h.Controls.Add(Me.CbxConversation, 2, 0)
        Me.L1h2L2v3h.Controls.Add(Me.LblMoveOptions, 0, 0)
        Me.L1h2L2v3h.Controls.Add(Me.CbxEmailCopy, 8, 0)
        Me.L1h2L2v3h.Controls.Add(Me.LblSaveOptions, 4, 0)
        Me.L1h2L2v3h.Controls.Add(Me.CbxAttachments, 6, 0)
        Me.L1h2L2v3h.Controls.Add(Me.LblAcAttachments, 5, 0)
        Me.L1h2L2v3h.Controls.Add(Me.LblAcEmail, 7, 0)
        Me.L1h2L2v3h.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1h2L2v3h.Location = New System.Drawing.Point(3, 68)
        Me.L1h2L2v3h.Name = "L1h2L2v3h"
        Me.L1h2L2v3h.RowCount = 1
        Me.L1h2L2v3h.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1h2L2v3h.Size = New System.Drawing.Size(569, 34)
        Me.L1h2L2v3h.TabIndex = 2
        '
        'LblAcConversation
        '
        Me.LblAcConversation.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.LblAcConversation.AutoSize = True
        Me.LblAcConversation.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcConversation.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcConversation.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcConversation.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcConversation.Location = New System.Drawing.Point(95, 2)
        Me.LblAcConversation.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        Me.LblAcConversation.Name = "LblAcConversation"
        Me.LblAcConversation.Size = New System.Drawing.Size(19, 18)
        Me.LblAcConversation.TabIndex = 15
        Me.LblAcConversation.Text = "C"
        '
        'CbxConversation
        '
        Me.CbxConversation.AutoSize = True
        Me.CbxConversation.Location = New System.Drawing.Point(115, 3)
        Me.CbxConversation.Margin = New System.Windows.Forms.Padding(0, 3, 3, 3)
        Me.CbxConversation.Name = "CbxConversation"
        Me.CbxConversation.Size = New System.Drawing.Size(118, 17)
        Me.CbxConversation.TabIndex = 13
        Me.CbxConversation.Text = "Entire Conversation"
        Me.CbxConversation.UseVisualStyleBackColor = True
        '
        'LblMoveOptions
        '
        Me.LblMoveOptions.AutoSize = True
        Me.LblMoveOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblMoveOptions.Location = New System.Drawing.Point(3, 4)
        Me.LblMoveOptions.Margin = New System.Windows.Forms.Padding(3, 4, 0, 3)
        Me.LblMoveOptions.Name = "LblMoveOptions"
        Me.LblMoveOptions.Size = New System.Drawing.Size(89, 13)
        Me.LblMoveOptions.TabIndex = 10
        Me.LblMoveOptions.Text = "Move Options:"
        '
        'CbxEmailCopy
        '
        Me.CbxEmailCopy.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CbxEmailCopy.AutoSize = True
        Me.CbxEmailCopy.Location = New System.Drawing.Point(476, 3)
        Me.CbxEmailCopy.Name = "CbxEmailCopy"
        Me.CbxEmailCopy.Size = New System.Drawing.Size(90, 17)
        Me.CbxEmailCopy.TabIndex = 6
        Me.CbxEmailCopy.Text = "Copy of Email"
        Me.CbxEmailCopy.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.CbxEmailCopy.UseVisualStyleBackColor = True
        '
        'LblSaveOptions
        '
        Me.LblSaveOptions.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblSaveOptions.AutoSize = True
        Me.LblSaveOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblSaveOptions.Location = New System.Drawing.Point(247, 4)
        Me.LblSaveOptions.Margin = New System.Windows.Forms.Padding(3, 4, 0, 3)
        Me.LblSaveOptions.Name = "LblSaveOptions"
        Me.LblSaveOptions.Size = New System.Drawing.Size(87, 13)
        Me.LblSaveOptions.TabIndex = 8
        Me.LblSaveOptions.Text = "Save Options:"
        '
        'CbxAttachments
        '
        Me.CbxAttachments.AutoSize = True
        Me.CbxAttachments.Location = New System.Drawing.Point(357, 3)
        Me.CbxAttachments.Name = "CbxAttachments"
        Me.CbxAttachments.Size = New System.Drawing.Size(85, 17)
        Me.CbxAttachments.TabIndex = 12
        Me.CbxAttachments.Text = "Attachments"
        Me.CbxAttachments.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.CbxAttachments.UseVisualStyleBackColor = True
        '
        'LblAcAttachments
        '
        Me.LblAcAttachments.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.LblAcAttachments.AutoSize = True
        Me.LblAcAttachments.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcAttachments.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcAttachments.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcAttachments.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcAttachments.Location = New System.Drawing.Point(334, 2)
        Me.LblAcAttachments.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        Me.LblAcAttachments.Name = "LblAcAttachments"
        Me.LblAcAttachments.Size = New System.Drawing.Size(19, 18)
        Me.LblAcAttachments.TabIndex = 14
        Me.LblAcAttachments.Text = "A"
        '
        'LblAcEmail
        '
        Me.LblAcEmail.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.LblAcEmail.AutoSize = True
        Me.LblAcEmail.BackColor = System.Drawing.SystemColors.ControlText
        Me.LblAcEmail.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblAcEmail.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblAcEmail.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.LblAcEmail.Location = New System.Drawing.Point(449, 2)
        Me.LblAcEmail.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        Me.LblAcEmail.Name = "LblAcEmail"
        Me.LblAcEmail.Size = New System.Drawing.Size(19, 18)
        Me.LblAcEmail.TabIndex = 16
        Me.LblAcEmail.Text = "E"
        '
        'ControlGroup
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Controls.Add(Me.L1h)
        Me.Name = "ControlGroup"
        Me.Size = New System.Drawing.Size(1094, 105)
        Me.L1h.Panel1.ResumeLayout(False)
        Me.L1h.Panel2.ResumeLayout(False)
        CType(Me.L1h, System.ComponentModel.ISupportInitialize).EndInit()
        Me.L1h.ResumeLayout(False)
        Me.L1h1L2v.ResumeLayout(False)
        Me.L1h1L2v.PerformLayout()
        Me.L1h1L2v1L3h.ResumeLayout(False)
        Me.L1h1L2v1L3h.PerformLayout()
        Me.L1h1L2v2L3h.ResumeLayout(False)
        Me.L1h1L2v2L3h.PerformLayout()
        Me.L1h2L2v.ResumeLayout(False)
        Me.L1h2L2v1h.ResumeLayout(False)
        Me.L1h2L2v1h.PerformLayout()
        Me.L1h2L2v1h5Panel.ResumeLayout(False)
        Me.L1h2L2v1h5Panel.PerformLayout()
        Me.L1h2L2v1h4Panel.ResumeLayout(False)
        Me.L1h2L2v1h4Panel.PerformLayout()
        Me.L1h2L2v1h3Panel.ResumeLayout(False)
        Me.L1h2L2v1h3Panel.PerformLayout()
        Me.L1h2L2v2h.ResumeLayout(False)
        Me.L1h2L2v2h.PerformLayout()
        Me.L1h2L2v3h.ResumeLayout(False)
        Me.L1h2L2v3h.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents L1h As System.Windows.Forms.SplitContainer
    Friend WithEvents L1h2L2v As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents L1h2L2v1h As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents L1h2L2v2h As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents TxtboxSearch As System.Windows.Forms.TextBox
    Friend WithEvents LblFolder As System.Windows.Forms.Label
    Friend WithEvents LblSearch As System.Windows.Forms.Label
    Friend WithEvents CboFolders As System.Windows.Forms.ComboBox
    Friend WithEvents L1h2L2v3h As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents LblMoveOptions As System.Windows.Forms.Label
    Friend WithEvents CbxEmailCopy As System.Windows.Forms.CheckBox
    Friend WithEvents LblSaveOptions As System.Windows.Forms.Label
    Friend WithEvents L1h1L2v As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents L1h1L2v1L3h As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents LblSentOn As System.Windows.Forms.Label
    Friend WithEvents LblSender As System.Windows.Forms.Label
    Friend WithEvents lblCaptionTriage As System.Windows.Forms.Label
    Friend WithEvents LblTriage As System.Windows.Forms.Label
    Friend WithEvents LblCaptionPredicted As System.Windows.Forms.Label
    Friend WithEvents LblActionable As System.Windows.Forms.Label
    Friend WithEvents L1h1L2v2L3h As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents LblConvCt As System.Windows.Forms.Label
    Friend WithEvents lblSubject As System.Windows.Forms.Label
    Friend WithEvents TxtboxBody As System.Windows.Forms.TextBox
    Friend WithEvents LblPos As System.Windows.Forms.Label
    Friend WithEvents LblAcOpen As System.Windows.Forms.Label
    Friend WithEvents L1h2L2v1h5Panel As System.Windows.Forms.Panel
    Friend WithEvents L1h2L2v1h4Panel As System.Windows.Forms.Panel
    Friend WithEvents LblAcPopOut As System.Windows.Forms.Label
    Friend WithEvents BtnPopOut As System.Windows.Forms.Button
    Friend WithEvents LblAcDelete As System.Windows.Forms.Label
    Friend WithEvents BtnDelItem As System.Windows.Forms.Button
    Friend WithEvents LblAcSearch As System.Windows.Forms.Label
    Friend WithEvents L1h2L2v1h3Panel As System.Windows.Forms.Panel
    Friend WithEvents LblAcTask As System.Windows.Forms.Label
    Friend WithEvents BtnFlagTask As System.Windows.Forms.Button
    Friend WithEvents LblAcFolder As System.Windows.Forms.Label
    Friend WithEvents LblAcConversation As System.Windows.Forms.Label
    Friend WithEvents CbxConversation As System.Windows.Forms.CheckBox
    Friend WithEvents CbxAttachments As System.Windows.Forms.CheckBox
    Friend WithEvents LblAcAttachments As System.Windows.Forms.Label
    Friend WithEvents LblAcEmail As System.Windows.Forms.Label
End Class
