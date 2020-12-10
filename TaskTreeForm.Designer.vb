<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
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
        Me.OlvColumn1 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn2 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn3 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn6 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn4 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvColumn5 = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvToDoID = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        CType(Me.TreeListView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip1.SuspendLayout()
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
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn1)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn2)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn3)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn6)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn4)
        Me.TreeListView1.AllColumns.Add(Me.OlvColumn5)
        Me.TreeListView1.AllColumns.Add(Me.OlvToDoID)
        Me.TreeListView1.AllowDrop = True
        Me.TreeListView1.CellEditUseWholeCell = False
        Me.TreeListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.OlvTaskSubject, Me.OlvColumn1, Me.OlvColumn2, Me.OlvColumn3, Me.OlvColumn6, Me.OlvColumn4, Me.OlvColumn5, Me.OlvToDoID})
        Me.TreeListView1.Cursor = System.Windows.Forms.Cursors.Default
        Me.TreeListView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeListView1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TreeListView1.HideSelection = False
        Me.TreeListView1.IsSimpleDragSource = True
        Me.TreeListView1.IsSimpleDropSink = True
        Me.TreeListView1.Location = New System.Drawing.Point(0, 0)
        Me.TreeListView1.Name = "TreeListView1"
        Me.TreeListView1.ShowGroups = False
        Me.TreeListView1.Size = New System.Drawing.Size(1369, 537)
        Me.TreeListView1.SmallImageList = Me.ImageList1
        Me.TreeListView1.TabIndex = 0
        Me.TreeListView1.UseCompatibleStateImageBehavior = False
        Me.TreeListView1.View = System.Windows.Forms.View.Details
        Me.TreeListView1.VirtualMode = True
        '
        'OlvTaskSubject
        '
        Me.OlvTaskSubject.AspectName = "Value.TaskSubject"
        Me.OlvTaskSubject.Text = "Task Subject"
        Me.OlvTaskSubject.Width = 246
        '
        'OlvColumn1
        '
        Me.OlvColumn1.AspectName = "Value.TagProject"
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
        Me.OlvColumn5.Text = "Created"
        Me.OlvColumn5.Width = 146
        '
        'OlvToDoID
        '
        Me.OlvToDoID.AspectName = "Value.ToDoID"
        Me.OlvToDoID.Text = "To Do ID"
        Me.OlvToDoID.Width = 117
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
        'TaskTreeForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1369, 537)
        Me.Controls.Add(Me.TreeListView1)
        Me.Name = "TaskTreeForm"
        Me.Text = "TaskTreeForm"
        CType(Me.TreeListView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip1.ResumeLayout(False)
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
End Class
