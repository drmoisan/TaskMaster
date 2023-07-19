<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frm_TaskTree
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
        Me.TaskTree = New System.Windows.Forms.TreeView()
        Me.SuspendLayout()
        '
        'TaskTree
        '
        Me.TaskTree.AllowDrop = True
        Me.TaskTree.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TaskTree.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TaskTree.Location = New System.Drawing.Point(0, 0)
        Me.TaskTree.Name = "TaskTree"
        Me.TaskTree.Size = New System.Drawing.Size(800, 450)
        Me.TaskTree.TabIndex = 0
        '
        'frm_TaskTree
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.TaskTree)
        Me.Name = "frm_TaskTree"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TaskTree As Windows.Forms.TreeView
End Class
