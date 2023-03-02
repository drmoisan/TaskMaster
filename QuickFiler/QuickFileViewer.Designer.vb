<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class QuickFileViewer
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
        Me.TableLayoutL1 = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Button_OK = New System.Windows.Forms.Button()
        Me.BUTTON_CANCEL = New System.Windows.Forms.Button()
        Me.Button_Undo = New System.Windows.Forms.Button()
        Me.spn_EmailPerLoad = New System.Windows.Forms.NumericUpDown()
        Me.TableLayoutL1.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.spn_EmailPerLoad, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TableLayoutL1
        '
        Me.TableLayoutL1.ColumnCount = 1
        Me.TableLayoutL1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutL1.Controls.Add(Me.TableLayoutPanel1, 0, 1)
        Me.TableLayoutL1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutL1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutL1.Name = "TableLayoutL1"
        Me.TableLayoutL1.RowCount = 2
        Me.TableLayoutL1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutL1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56.0!))
        Me.TableLayoutL1.Size = New System.Drawing.Size(919, 161)
        Me.TableLayoutL1.TabIndex = 0
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 7
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.Button_OK, 2, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.BUTTON_CANCEL, 3, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Button_Undo, 4, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.spn_EmailPerLoad, 5, 0)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 108)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(913, 50)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'Button_OK
        '
        Me.Button_OK.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Button_OK.Location = New System.Drawing.Point(303, 3)
        Me.Button_OK.Margin = New System.Windows.Forms.Padding(7, 3, 7, 3)
        Me.Button_OK.Name = "Button_OK"
        Me.Button_OK.Size = New System.Drawing.Size(146, 44)
        Me.Button_OK.TabIndex = 0
        Me.Button_OK.Text = "OK"
        Me.Button_OK.UseVisualStyleBackColor = True
        '
        'BUTTON_CANCEL
        '
        Me.BUTTON_CANCEL.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BUTTON_CANCEL.Location = New System.Drawing.Point(463, 3)
        Me.BUTTON_CANCEL.Margin = New System.Windows.Forms.Padding(7, 3, 7, 3)
        Me.BUTTON_CANCEL.Name = "BUTTON_CANCEL"
        Me.BUTTON_CANCEL.Size = New System.Drawing.Size(146, 44)
        Me.BUTTON_CANCEL.TabIndex = 1
        Me.BUTTON_CANCEL.Text = "CANCEL"
        Me.BUTTON_CANCEL.UseVisualStyleBackColor = True
        '
        'Button_Undo
        '
        Me.Button_Undo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Button_Undo.Location = New System.Drawing.Point(619, 3)
        Me.Button_Undo.Name = "Button_Undo"
        Me.Button_Undo.Size = New System.Drawing.Size(54, 44)
        Me.Button_Undo.TabIndex = 2
        Me.Button_Undo.Text = "Undo"
        Me.Button_Undo.UseVisualStyleBackColor = True
        '
        'spn_EmailPerLoad
        '
        Me.spn_EmailPerLoad.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.spn_EmailPerLoad.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spn_EmailPerLoad.Location = New System.Drawing.Point(683, 4)
        Me.spn_EmailPerLoad.Margin = New System.Windows.Forms.Padding(7, 3, 7, 3)
        Me.spn_EmailPerLoad.Name = "spn_EmailPerLoad"
        Me.spn_EmailPerLoad.Size = New System.Drawing.Size(66, 41)
        Me.spn_EmailPerLoad.TabIndex = 3
        '
        'QuickFileViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(919, 161)
        Me.Controls.Add(Me.TableLayoutL1)
        Me.Name = "QuickFileViewer"
        Me.Text = "Quick File"
        Me.TableLayoutL1.ResumeLayout(False)
        Me.TableLayoutPanel1.ResumeLayout(False)
        CType(Me.spn_EmailPerLoad, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TableLayoutL1 As Windows.Forms.TableLayoutPanel
    Friend WithEvents TableLayoutPanel1 As Windows.Forms.TableLayoutPanel
    Friend WithEvents Button_OK As Windows.Forms.Button
    Friend WithEvents BUTTON_CANCEL As Windows.Forms.Button
    Friend WithEvents Button_Undo As Windows.Forms.Button
    Friend WithEvents spn_EmailPerLoad As Windows.Forms.NumericUpDown
End Class
