<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ProjectInfoWindow
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.components = New System.ComponentModel.Container()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.olvProjInfo = New BrightIdeasSoftware.ObjectListView()
        Me.OlvProjectID = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvProjectName = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.OlvProgramName = CType(New BrightIdeasSoftware.OLVColumn(), BrightIdeasSoftware.OLVColumn)
        Me.BTN_CANCEL = New System.Windows.Forms.Button()
        Me.BTN_OK = New System.Windows.Forms.Button()
        Me.ProjectInfoBindingSource = New System.Windows.Forms.BindingSource(Me.components)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        CType(Me.olvProjInfo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ProjectInfoBindingSource, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        Me.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.olvProjInfo)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.BTN_CANCEL)
        Me.SplitContainer1.Panel2.Controls.Add(Me.BTN_OK)
        Me.SplitContainer1.Size = New System.Drawing.Size(800, 450)
        Me.SplitContainer1.SplitterDistance = 399
        Me.SplitContainer1.TabIndex = 0
        '
        'olvProjInfo
        '
        Me.olvProjInfo.AllColumns.Add(Me.OlvProjectID)
        Me.olvProjInfo.AllColumns.Add(Me.OlvProjectName)
        Me.olvProjInfo.AllColumns.Add(Me.OlvProgramName)
        Me.olvProjInfo.AllowColumnReorder = True
        Me.olvProjInfo.AllowDrop = True
        Me.olvProjInfo.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick
        Me.olvProjInfo.CellEditUseWholeCell = False
        Me.olvProjInfo.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.OlvProjectID, Me.OlvProjectName, Me.OlvProgramName})
        Me.olvProjInfo.Cursor = System.Windows.Forms.Cursors.Default
        Me.olvProjInfo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.olvProjInfo.HasCollapsibleGroups = False
        Me.olvProjInfo.HideSelection = False
        Me.olvProjInfo.Location = New System.Drawing.Point(0, 0)
        Me.olvProjInfo.Name = "olvProjInfo"
        Me.olvProjInfo.ShowGroups = False
        Me.olvProjInfo.Size = New System.Drawing.Size(800, 399)
        Me.olvProjInfo.TabIndex = 0
        Me.olvProjInfo.UseCompatibleStateImageBehavior = False
        Me.olvProjInfo.View = System.Windows.Forms.View.Details
        '
        'OlvProjectID
        '
        Me.OlvProjectID.AspectName = "ProjectID"
        Me.OlvProjectID.Groupable = False
        Me.OlvProjectID.Text = "Project ID"
        Me.OlvProjectID.Width = 166
        '
        'OlvProjectName
        '
        Me.OlvProjectName.AspectName = "ProjectName"
        Me.OlvProjectName.Groupable = False
        Me.OlvProjectName.Text = "Project Name"
        Me.OlvProjectName.Width = 318
        '
        'OlvProgramName
        '
        Me.OlvProgramName.AspectName = "ProgramName"
        Me.OlvProgramName.Groupable = False
        Me.OlvProgramName.Text = "Program Name"
        Me.OlvProgramName.Width = 283
        '
        'BTN_CANCEL
        '
        Me.BTN_CANCEL.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.BTN_CANCEL.Location = New System.Drawing.Point(440, 12)
        Me.BTN_CANCEL.Name = "BTN_CANCEL"
        Me.BTN_CANCEL.Size = New System.Drawing.Size(94, 23)
        Me.BTN_CANCEL.TabIndex = 1
        Me.BTN_CANCEL.Text = "CANCEL"
        Me.BTN_CANCEL.UseVisualStyleBackColor = True
        '
        'BTN_OK
        '
        Me.BTN_OK.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.BTN_OK.Location = New System.Drawing.Point(312, 12)
        Me.BTN_OK.Name = "BTN_OK"
        Me.BTN_OK.Size = New System.Drawing.Size(94, 23)
        Me.BTN_OK.TabIndex = 0
        Me.BTN_OK.Text = "OK"
        Me.BTN_OK.UseVisualStyleBackColor = True
        '
        'ProjectInfoBindingSource
        '
        Me.ProjectInfoBindingSource.DataSource = GetType(ToDoModel.ProjectInfo)
        '
        'ProjectInfoWindow
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "ProjectInfoWindow"
        Me.Text = "ProjectInfoWindow"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        CType(Me.olvProjInfo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ProjectInfoBindingSource, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents SplitContainer1 As Windows.Forms.SplitContainer
    Friend WithEvents ProjectInfoBindingSource As Windows.Forms.BindingSource
    Friend WithEvents BTN_CANCEL As Windows.Forms.Button
    Friend WithEvents BTN_OK As Windows.Forms.Button
    Friend WithEvents olvProjInfo As BrightIdeasSoftware.ObjectListView
    Friend WithEvents OlvProjectID As BrightIdeasSoftware.OLVColumn
    Friend WithEvents OlvProjectName As BrightIdeasSoftware.OLVColumn
    Friend WithEvents OlvProgramName As BrightIdeasSoftware.OLVColumn
End Class
