<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class QuickFileViewer
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
        Me.L1v = New System.Windows.Forms.TableLayoutPanel()
        Me.L1v2L2h = New System.Windows.Forms.TableLayoutPanel()
        Me.AcceleratorDialogue = New System.Windows.Forms.TextBox()
        Me.L1v2L2h3_ButtonOK = New System.Windows.Forms.Button()
        Me.L1v2L2h4_ButtonCancel = New System.Windows.Forms.Button()
        Me.L1v2L2h4_ButtonUndo = New System.Windows.Forms.Button()
        Me.L1v2L2h5_SpnEmailPerLoad = New System.Windows.Forms.NumericUpDown()
        Me.L1v1L2_PanelMain = New System.Windows.Forms.Panel()
        Me.L1v1L2L3v = New System.Windows.Forms.TableLayoutPanel()
        Me.L1v.SuspendLayout()
        Me.L1v2L2h.SuspendLayout()
        CType(Me.L1v2L2h5_SpnEmailPerLoad, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.L1v1L2_PanelMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'L1v
        '
        Me.L1v.ColumnCount = 1
        Me.L1v.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1v.Controls.Add(Me.L1v2L2h, 0, 1)
        Me.L1v.Controls.Add(Me.L1v1L2_PanelMain, 0, 0)
        Me.L1v.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1v.Location = New System.Drawing.Point(0, 0)
        Me.L1v.Name = "L1v"
        Me.L1v.RowCount = 2
        Me.L1v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56.0!))
        Me.L1v.Size = New System.Drawing.Size(919, 274)
        Me.L1v.TabIndex = 0
        '
        'L1v2L2h
        '
        Me.L1v2L2h.ColumnCount = 7
        Me.L1v2L2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.L1v2L2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140.0!))
        Me.L1v2L2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0!))
        Me.L1v2L2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0!))
        Me.L1v2L2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0!))
        Me.L1v2L2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0!))
        Me.L1v2L2h.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.L1v2L2h.Controls.Add(Me.AcceleratorDialogue, 0, 0)
        Me.L1v2L2h.Controls.Add(Me.L1v2L2h3_ButtonOK, 2, 0)
        Me.L1v2L2h.Controls.Add(Me.L1v2L2h4_ButtonCancel, 3, 0)
        Me.L1v2L2h.Controls.Add(Me.L1v2L2h4_ButtonUndo, 4, 0)
        Me.L1v2L2h.Controls.Add(Me.L1v2L2h5_SpnEmailPerLoad, 5, 0)
        Me.L1v2L2h.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1v2L2h.Location = New System.Drawing.Point(3, 221)
        Me.L1v2L2h.Name = "L1v2L2h"
        Me.L1v2L2h.RowCount = 1
        Me.L1v2L2h.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1v2L2h.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        Me.L1v2L2h.Size = New System.Drawing.Size(913, 50)
        Me.L1v2L2h.TabIndex = 0
        '
        'AcceleratorDialogue
        '
        Me.AcceleratorDialogue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.AcceleratorDialogue.Font = New System.Drawing.Font("Microsoft Sans Serif", 21.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.AcceleratorDialogue.Location = New System.Drawing.Point(7, 3)
        Me.AcceleratorDialogue.Margin = New System.Windows.Forms.Padding(7, 3, 7, 3)
        Me.AcceleratorDialogue.Name = "AcceleratorDialogue"
        Me.AcceleratorDialogue.Size = New System.Drawing.Size(142, 40)
        Me.AcceleratorDialogue.TabIndex = 5
        '
        'L1v2L2h3_ButtonOK
        '
        Me.L1v2L2h3_ButtonOK.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1v2L2h3_ButtonOK.Location = New System.Drawing.Point(303, 3)
        Me.L1v2L2h3_ButtonOK.Margin = New System.Windows.Forms.Padding(7, 3, 7, 3)
        Me.L1v2L2h3_ButtonOK.Name = "L1v2L2h3_ButtonOK"
        Me.L1v2L2h3_ButtonOK.Size = New System.Drawing.Size(146, 44)
        Me.L1v2L2h3_ButtonOK.TabIndex = 0
        Me.L1v2L2h3_ButtonOK.Text = "OK"
        Me.L1v2L2h3_ButtonOK.UseVisualStyleBackColor = True
        '
        'L1v2L2h4_ButtonCancel
        '
        Me.L1v2L2h4_ButtonCancel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1v2L2h4_ButtonCancel.Location = New System.Drawing.Point(463, 3)
        Me.L1v2L2h4_ButtonCancel.Margin = New System.Windows.Forms.Padding(7, 3, 7, 3)
        Me.L1v2L2h4_ButtonCancel.Name = "L1v2L2h4_ButtonCancel"
        Me.L1v2L2h4_ButtonCancel.Size = New System.Drawing.Size(146, 44)
        Me.L1v2L2h4_ButtonCancel.TabIndex = 1
        Me.L1v2L2h4_ButtonCancel.Text = "CANCEL"
        Me.L1v2L2h4_ButtonCancel.UseVisualStyleBackColor = True
        '
        'L1v2L2h4_ButtonUndo
        '
        Me.L1v2L2h4_ButtonUndo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1v2L2h4_ButtonUndo.Location = New System.Drawing.Point(619, 3)
        Me.L1v2L2h4_ButtonUndo.Name = "L1v2L2h4_ButtonUndo"
        Me.L1v2L2h4_ButtonUndo.Size = New System.Drawing.Size(54, 44)
        Me.L1v2L2h4_ButtonUndo.TabIndex = 2
        Me.L1v2L2h4_ButtonUndo.Text = "Undo"
        Me.L1v2L2h4_ButtonUndo.UseVisualStyleBackColor = True
        '
        'L1v2L2h5_SpnEmailPerLoad
        '
        Me.L1v2L2h5_SpnEmailPerLoad.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.L1v2L2h5_SpnEmailPerLoad.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.L1v2L2h5_SpnEmailPerLoad.Location = New System.Drawing.Point(683, 4)
        Me.L1v2L2h5_SpnEmailPerLoad.Margin = New System.Windows.Forms.Padding(7, 3, 7, 3)
        Me.L1v2L2h5_SpnEmailPerLoad.Name = "L1v2L2h5_SpnEmailPerLoad"
        Me.L1v2L2h5_SpnEmailPerLoad.Size = New System.Drawing.Size(66, 41)
        Me.L1v2L2h5_SpnEmailPerLoad.TabIndex = 3
        '
        'L1v1L2_PanelMain
        '
        Me.L1v1L2_PanelMain.AutoScroll = True
        Me.L1v1L2_PanelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.L1v1L2_PanelMain.Controls.Add(Me.L1v1L2L3v)
        Me.L1v1L2_PanelMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.L1v1L2_PanelMain.Location = New System.Drawing.Point(3, 3)
        Me.L1v1L2_PanelMain.Name = "L1v1L2_PanelMain"
        Me.L1v1L2_PanelMain.Size = New System.Drawing.Size(913, 212)
        Me.L1v1L2_PanelMain.TabIndex = 1
        '
        'L1v1L2L3v
        '
        Me.L1v1L2L3v.AutoSize = True
        Me.L1v1L2L3v.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.L1v1L2L3v.ColumnCount = 1
        Me.L1v1L2L3v.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1v1L2L3v.Dock = System.Windows.Forms.DockStyle.Top
        Me.L1v1L2L3v.Location = New System.Drawing.Point(0, 0)
        Me.L1v1L2L3v.Margin = New System.Windows.Forms.Padding(0)
        Me.L1v1L2L3v.Name = "L1v1L2L3v"
        Me.L1v1L2L3v.Padding = New System.Windows.Forms.Padding(10)
        Me.L1v1L2L3v.RowCount = 4
        Me.L1v1L2L3v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        Me.L1v1L2L3v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        Me.L1v1L2L3v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        Me.L1v1L2L3v.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.L1v1L2L3v.Size = New System.Drawing.Size(894, 350)
        Me.L1v1L2L3v.TabIndex = 1
        '
        'QuickFileViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(919, 274)
        Me.Controls.Add(Me.L1v)
        Me.Name = "QuickFileViewer"
        Me.Text = "Quick File"
        Me.L1v.ResumeLayout(False)
        Me.L1v2L2h.ResumeLayout(False)
        Me.L1v2L2h.PerformLayout()
        CType(Me.L1v2L2h5_SpnEmailPerLoad, System.ComponentModel.ISupportInitialize).EndInit()
        Me.L1v1L2_PanelMain.ResumeLayout(False)
        Me.L1v1L2_PanelMain.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents L1v As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents L1v2L2h As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents L1v2L2h3_ButtonOK As System.Windows.Forms.Button
    Friend WithEvents L1v2L2h4_ButtonCancel As System.Windows.Forms.Button
    Friend WithEvents L1v2L2h4_ButtonUndo As System.Windows.Forms.Button
    Friend WithEvents L1v2L2h5_SpnEmailPerLoad As System.Windows.Forms.NumericUpDown
    Friend WithEvents L1v1L2_PanelMain As System.Windows.Forms.Panel
    Friend WithEvents L1v1L2L3v As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents AcceleratorDialogue As System.Windows.Forms.TextBox
End Class
