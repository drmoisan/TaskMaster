﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TaskTreeForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.TreeListView1 = New BrightIdeasSoftware.TreeListView()
        Me.OlvTaskSubject = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvToDoID = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn1 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn2 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn3 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn6 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn4 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn5 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn7 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.But_ReloadTree = New System.Windows.Forms.Button()
        Me.But_ExpandCollapse = New System.Windows.Forms.Button()
        Me.But_ShowHideComplete = New System.Windows.Forms.Button()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.OlvColumn8 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        CType(Me.TreeListView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.FlowLayoutPanel1.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ImageList1
        '
        Me.ImageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        Me.ImageList1.ImageSize = New System.Drawing.Size(16, 16)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        '
        'TreeListView1
        '
        Me.TreeListView1.AllColumns.Add(Me.OlvTaskSubject)
        Me.TreeListView1.AllColumns.Add(Me.OlvToDoID)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn1)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn2)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn3)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn6)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn4)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn5)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn7)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn8)
        Me.TreeListView1.AllowColumnReorder = True
        Me.TreeListView1.AllowDrop = True
        Me.TreeListView1.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick
        Me.TreeListView1.CellEditUseWholeCell = False
        Me.TreeListView1.CheckBoxes = True
        Me.TreeListView1.CheckedAspectName = "Value.Complete"
        Me.TreeListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.OlvTaskSubject, Me.OlvToDoID, Me.OlvColumn2, Me.OlvColumn3, Me.OlvColumn6, Me.OlvColumn4, Me.OlvColumn7, Me.OlvColumn8})
        Me.TreeListView1.Cursor = System.Windows.Forms.Cursors.Default
        Me.TreeListView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeListView1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TreeListView1.HideSelection = False
        Me.TreeListView1.IsSimpleDragSource = True
        Me.TreeListView1.IsSimpleDropSink = True
        Me.TreeListView1.Location = New System.Drawing.Point(0, 0)
        Me.TreeListView1.Name = "TreeListView1"
        Me.TreeListView1.ShowGroups = False
        Me.TreeListView1.ShowImagesOnSubItems = True
        Me.TreeListView1.Size = New System.Drawing.Size(1369, 503)
        Me.TreeListView1.SmallImageList = Me.ImageList1
        Me.TreeListView1.TabIndex = 0
        Me.TreeListView1.UseCompatibleStateImageBehavior = False
        Me.TreeListView1.UseFiltering = True
        Me.TreeListView1.View = System.Windows.Forms.View.Details
        Me.TreeListView1.VirtualMode = True
        '
        'OlvTaskSubject
        '
        Me.OlvTaskSubject.AspectName = "Value.TaskSubject"
        Me.OlvTaskSubject.Text = "Task Subject"
        Me.OlvTaskSubject.Width = 246
        '
        'OlvToDoID
        '
        Me.OlvToDoID.AspectName = "Value.ToDoID"
        Me.OlvToDoID.Text = "To Do ID"
        Me.OlvToDoID.Width = 117
        '
        'OlvColumn1
        '
        Me.OlvColumn1.AspectName = "Value.TagProject"
        Me.OlvColumn1.DisplayIndex = 1
        Me.OlvColumn1.IsVisible = False
        Me.OlvColumn1.Text = "Project"
        Me.OlvColumn1.Width = 114
        '
        'OlvColumn2
        '
        Me.OlvColumn2.AspectName = "Value.TagTopic"
        Me.OlvColumn2.Text = "Topic"
        Me.OlvColumn2.Width = 94
        '
        'OlvColumn3
        '
        Me.OlvColumn3.AspectName = "Value.TagPeople"
        Me.OlvColumn3.Text = "People"
        Me.OlvColumn3.Width = 122
        '
        'OlvColumn6
        '
        Me.OlvColumn6.AspectName = "Value.MetaTaskLvl"
        Me.OlvColumn6.Text = "Lvl"
        '
        'OlvColumn4
        '
        Me.OlvColumn4.AspectName = "Value.MetaTaskSubject"
        Me.OlvColumn4.Text = "Meta Task"
        Me.OlvColumn4.Width = 187
        '
        'OlvColumn5
        '
        Me.OlvColumn5.AspectName = "Value.TaskCreateDate"
        Me.OlvColumn5.DisplayIndex = 6
        Me.OlvColumn5.IsVisible = False
        Me.OlvColumn5.Text = "Created"
        Me.OlvColumn5.Width = 146
        '
        'OlvColumn7
        '
        Me.OlvColumn7.AspectName = "Value.StartDate"
        Me.OlvColumn7.Text = "Started"
        Me.OlvColumn7.Width = 100
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem1})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(104, 26)
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        Me.ToolStripMenuItem1.Size = New System.Drawing.Size(103, 22)
        Me.ToolStripMenuItem1.Text = "Open"
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.FlowLayoutPanel1.Controls.Add(Me.But_ReloadTree)
        Me.FlowLayoutPanel1.Controls.Add(Me.But_ExpandCollapse)
        Me.FlowLayoutPanel1.Controls.Add(Me.But_ShowHideComplete)
        Me.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.FlowLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        Me.FlowLayoutPanel1.Size = New System.Drawing.Size(1369, 30)
        Me.FlowLayoutPanel1.TabIndex = 1
        '
        'But_ReloadTree
        '
        Me.But_ReloadTree.Location = New System.Drawing.Point(3, 3)
        Me.But_ReloadTree.Name = "But_ReloadTree"
        Me.But_ReloadTree.Size = New System.Drawing.Size(117, 23)
        Me.But_ReloadTree.TabIndex = 0
        Me.But_ReloadTree.Text = "Reload Tree"
        Me.But_ReloadTree.UseVisualStyleBackColor = True
        '
        'But_ExpandCollapse
        '
        Me.But_ExpandCollapse.Location = New System.Drawing.Point(126, 3)
        Me.But_ExpandCollapse.Name = "But_ExpandCollapse"
        Me.But_ExpandCollapse.Size = New System.Drawing.Size(117, 23)
        Me.But_ExpandCollapse.TabIndex = 1
        Me.But_ExpandCollapse.Text = "Expand / Collapse All"
        Me.But_ExpandCollapse.UseVisualStyleBackColor = True
        '
        'But_ShowHideComplete
        '
        Me.But_ShowHideComplete.Location = New System.Drawing.Point(249, 3)
        Me.But_ShowHideComplete.Name = "But_ShowHideComplete"
        Me.But_ShowHideComplete.Size = New System.Drawing.Size(117, 23)
        Me.But_ShowHideComplete.TabIndex = 3
        Me.But_ShowHideComplete.Text = "Show/Hide Complete"
        Me.But_ShowHideComplete.UseVisualStyleBackColor = True
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.SplitContainer1.IsSplitterFixed = True
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        Me.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.AccessibleName = "SplitContainer1Panel1"
        Me.SplitContainer1.Panel1.Controls.Add(Me.FlowLayoutPanel1)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.AccessibleName = "SplitContainer1Panel2"
        Me.SplitContainer1.Panel2.Controls.Add(Me.TreeListView1)
        Me.SplitContainer1.Size = New System.Drawing.Size(1369, 537)
        Me.SplitContainer1.SplitterDistance = 30
        Me.SplitContainer1.TabIndex = 2
        '
        'OlvColumn8
        '
        Me.OlvColumn8.AspectName = "Value.InFolder"
        Me.OlvColumn8.Text = "In Folder"
        Me.OlvColumn8.Width = 103
        '
        'TaskTreeForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1369, 537)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "TaskTreeForm"
        Me.Text = "TaskTreeForm"
        CType(Me.TreeListView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TreeListView1 As BrightIdeasSoftware.TreeListView
    Friend WithEvents OlvToDoID As BrightIdeasSoftware.OLVColumn
    Friend WithEvents OlvTaskSubject As BrightIdeasSoftware.OLVColumn
    Friend WithEvents ImageList1 As Windows.Forms.ImageList
    Friend WithEvents OlvColumn1 As BrightIdeasSoftware.OLVColumn
    Friend WithEvents OlvColumn2 As BrightIdeasSoftware.OLVColumn
    Friend WithEvents OlvColumn3 As BrightIdeasSoftware.OLVColumn
    Friend WithEvents OlvColumn4 As BrightIdeasSoftware.OLVColumn
    Friend WithEvents OlvColumn5 As BrightIdeasSoftware.OLVColumn
    Friend WithEvents ContextMenuStrip1 As Windows.Forms.ContextMenuStrip
    Friend WithEvents ToolStripMenuItem1 As Windows.Forms.ToolStripMenuItem
    Friend WithEvents OlvColumn6 As BrightIdeasSoftware.OLVColumn
    Friend WithEvents FlowLayoutPanel1 As Windows.Forms.FlowLayoutPanel
    Friend WithEvents But_ReloadTree As Windows.Forms.Button
    Friend WithEvents But_ExpandCollapse As Windows.Forms.Button
    Friend WithEvents But_ShowHideComplete As Windows.Forms.Button
    Friend WithEvents SplitContainer1 As Windows.Forms.SplitContainer
    Friend WithEvents OlvColumn7 As BrightIdeasSoftware.OLVColumn
    Friend WithEvents OlvColumn8 As BrightIdeasSoftware.OLVColumn
End Class
