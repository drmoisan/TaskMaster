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
        Me.TableLayoutL2B = New System.Windows.Forms.TableLayoutPanel()
        Me.Button_OK = New System.Windows.Forms.Button()
        Me.BUTTON_CANCEL = New System.Windows.Forms.Button()
        Me.Button_Undo = New System.Windows.Forms.Button()
        Me.spn_EmailPerLoad = New System.Windows.Forms.NumericUpDown()
        Me.AcceleratorDialogue = New System.Windows.Forms.TextBox()
        Me.PanelMain = New System.Windows.Forms.Panel()
        Me.TableLayoutL1.SuspendLayout()
        Me.TableLayoutL2B.SuspendLayout()
        CType(Me.spn_EmailPerLoad, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TableLayoutL1
        '
        Me.TableLayoutL1.ColumnCount = 1
        Me.TableLayoutL1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutL1.Controls.Add(Me.TableLayoutL2B, 0, 1)
        Me.TableLayoutL1.Controls.Add(Me.PanelMain, 0, 0)
        Me.TableLayoutL1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutL1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutL1.Margin = New System.Windows.Forms.Padding(6)
        Me.TableLayoutL1.Name = "TableLayoutL1"
        Me.TableLayoutL1.RowCount = 2
        Me.TableLayoutL1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutL1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 108.0!))
        Me.TableLayoutL1.Size = New System.Drawing.Size(1838, 310)
        Me.TableLayoutL1.TabIndex = 0
        '
        'TableLayoutL2B
        '
        Me.TableLayoutL2B.ColumnCount = 7
        Me.TableLayoutL2B.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutL2B.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 280.0!))
        Me.TableLayoutL2B.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320.0!))
        Me.TableLayoutL2B.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320.0!))
        Me.TableLayoutL2B.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120.0!))
        Me.TableLayoutL2B.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0!))
        Me.TableLayoutL2B.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutL2B.Controls.Add(Me.Button_OK, 2, 0)
        Me.TableLayoutL2B.Controls.Add(Me.BUTTON_CANCEL, 3, 0)
        Me.TableLayoutL2B.Controls.Add(Me.Button_Undo, 4, 0)
        Me.TableLayoutL2B.Controls.Add(Me.spn_EmailPerLoad, 5, 0)
        Me.TableLayoutL2B.Controls.Add(Me.AcceleratorDialogue, 0, 0)
        Me.TableLayoutL2B.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutL2B.Location = New System.Drawing.Point(6, 208)
        Me.TableLayoutL2B.Margin = New System.Windows.Forms.Padding(6)
        Me.TableLayoutL2B.Name = "TableLayoutL2B"
        Me.TableLayoutL2B.RowCount = 1
        Me.TableLayoutL2B.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutL2B.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96.0!))
        Me.TableLayoutL2B.Size = New System.Drawing.Size(1826, 96)
        Me.TableLayoutL2B.TabIndex = 0
        '
        'Button_OK
        '
        Me.Button_OK.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Button_OK.Location = New System.Drawing.Point(607, 6)
        Me.Button_OK.Margin = New System.Windows.Forms.Padding(14, 6, 14, 6)
        Me.Button_OK.Name = "Button_OK"
        Me.Button_OK.Size = New System.Drawing.Size(292, 84)
        Me.Button_OK.TabIndex = 0
        Me.Button_OK.Text = "OK"
        Me.Button_OK.UseVisualStyleBackColor = True
        '
        'BUTTON_CANCEL
        '
        Me.BUTTON_CANCEL.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BUTTON_CANCEL.Location = New System.Drawing.Point(927, 6)
        Me.BUTTON_CANCEL.Margin = New System.Windows.Forms.Padding(14, 6, 14, 6)
        Me.BUTTON_CANCEL.Name = "BUTTON_CANCEL"
        Me.BUTTON_CANCEL.Size = New System.Drawing.Size(292, 84)
        Me.BUTTON_CANCEL.TabIndex = 1
        Me.BUTTON_CANCEL.Text = "CANCEL"
        Me.BUTTON_CANCEL.UseVisualStyleBackColor = True
        '
        'Button_Undo
        '
        Me.Button_Undo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Button_Undo.Location = New System.Drawing.Point(1239, 6)
        Me.Button_Undo.Margin = New System.Windows.Forms.Padding(6)
        Me.Button_Undo.Name = "Button_Undo"
        Me.Button_Undo.Size = New System.Drawing.Size(108, 84)
        Me.Button_Undo.TabIndex = 2
        Me.Button_Undo.Text = "Undo"
        Me.Button_Undo.UseVisualStyleBackColor = True
        '
        'spn_EmailPerLoad
        '
        Me.spn_EmailPerLoad.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.spn_EmailPerLoad.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spn_EmailPerLoad.Location = New System.Drawing.Point(1367, 11)
        Me.spn_EmailPerLoad.Margin = New System.Windows.Forms.Padding(14, 6, 14, 6)
        Me.spn_EmailPerLoad.Name = "spn_EmailPerLoad"
        Me.spn_EmailPerLoad.Size = New System.Drawing.Size(132, 74)
        Me.spn_EmailPerLoad.TabIndex = 3
        '
        'AcceleratorDialogue
        '
        Me.AcceleratorDialogue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.AcceleratorDialogue.Font = New System.Drawing.Font("Microsoft Sans Serif", 21.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.AcceleratorDialogue.Location = New System.Drawing.Point(14, 6)
        Me.AcceleratorDialogue.Margin = New System.Windows.Forms.Padding(14, 6, 14, 6)
        Me.AcceleratorDialogue.Name = "AcceleratorDialogue"
        Me.AcceleratorDialogue.Size = New System.Drawing.Size(285, 73)
        Me.AcceleratorDialogue.TabIndex = 4
        '
        'PanelMain
        '
        Me.PanelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.PanelMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelMain.Location = New System.Drawing.Point(6, 6)
        Me.PanelMain.Margin = New System.Windows.Forms.Padding(6)
        Me.PanelMain.Name = "PanelMain"
        Me.PanelMain.Size = New System.Drawing.Size(1826, 190)
        Me.PanelMain.TabIndex = 1
        '
        'QuickFileViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 25.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1838, 310)
        Me.Controls.Add(Me.TableLayoutL1)
        Me.Margin = New System.Windows.Forms.Padding(6)
        Me.Name = "QuickFileViewer"
        Me.Text = "Quick File"
        Me.TableLayoutL1.ResumeLayout(False)
        Me.TableLayoutL2B.ResumeLayout(False)
        Me.TableLayoutL2B.PerformLayout()
        CType(Me.spn_EmailPerLoad, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TableLayoutL1 As Windows.Forms.TableLayoutPanel
    Friend WithEvents TableLayoutL2B As Windows.Forms.TableLayoutPanel
    Friend WithEvents Button_OK As Windows.Forms.Button
    Friend WithEvents BUTTON_CANCEL As Windows.Forms.Button
    Friend WithEvents Button_Undo As Windows.Forms.Button
    Friend WithEvents spn_EmailPerLoad As Windows.Forms.NumericUpDown
    Friend WithEvents PanelMain As Windows.Forms.Panel
    Friend WithEvents AcceleratorDialogue As Windows.Forms.TextBox
End Class
