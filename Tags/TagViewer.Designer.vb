<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TagViewer
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
        Me.OptionsPanel = New System.Windows.Forms.Panel()
        Me.button_ok = New System.Windows.Forms.Button()
        Me.button_cancel = New System.Windows.Forms.Button()
        Me.button_new = New System.Windows.Forms.Button()
        Me.button_autoassign = New System.Windows.Forms.Button()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Hide_Archive = New System.Windows.Forms.CheckBox()
        Me.TableLayoutMaster = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutTopPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutBottomPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutMaster.SuspendLayout()
        Me.TableLayoutTopPanel.SuspendLayout()
        Me.TableLayoutBottomPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'OptionsPanel
        '
        Me.OptionsPanel.AutoScroll = True
        Me.OptionsPanel.AutoSize = True
        Me.OptionsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.OptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.OptionsPanel.Location = New System.Drawing.Point(6, 42)
        Me.OptionsPanel.Margin = New System.Windows.Forms.Padding(6)
        Me.OptionsPanel.Name = "OptionsPanel"
        Me.OptionsPanel.Size = New System.Drawing.Size(442, 373)
        Me.OptionsPanel.TabIndex = 0
        '
        'button_ok
        '
        Me.button_ok.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.button_ok.Location = New System.Drawing.Point(61, 3)
        Me.button_ok.Name = "button_ok"
        Me.button_ok.Size = New System.Drawing.Size(70, 28)
        Me.button_ok.TabIndex = 1
        Me.button_ok.Text = "OK"
        Me.button_ok.UseVisualStyleBackColor = True
        '
        'button_cancel
        '
        Me.button_cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.button_cancel.Location = New System.Drawing.Point(146, 3)
        Me.button_cancel.Name = "button_cancel"
        Me.button_cancel.Size = New System.Drawing.Size(70, 28)
        Me.button_cancel.TabIndex = 2
        Me.button_cancel.Text = "Cancel"
        Me.button_cancel.UseVisualStyleBackColor = True
        '
        'button_new
        '
        Me.button_new.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.button_new.Location = New System.Drawing.Point(231, 3)
        Me.button_new.Name = "button_new"
        Me.button_new.Size = New System.Drawing.Size(70, 28)
        Me.button_new.TabIndex = 3
        Me.button_new.Text = "New"
        Me.button_new.UseVisualStyleBackColor = True
        '
        'button_autoassign
        '
        Me.button_autoassign.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.button_autoassign.Location = New System.Drawing.Point(316, 3)
        Me.button_autoassign.Name = "button_autoassign"
        Me.button_autoassign.Size = New System.Drawing.Size(70, 28)
        Me.button_autoassign.TabIndex = 4
        Me.button_autoassign.Text = "AutoAssign"
        Me.button_autoassign.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.TextBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBox1.Location = New System.Drawing.Point(3, 3)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(317, 26)
        Me.TextBox1.TabIndex = 5
        '
        'Hide_Archive
        '
        Me.Hide_Archive.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.Hide_Archive.AutoSize = True
        Me.Hide_Archive.Checked = True
        Me.Hide_Archive.CheckState = System.Windows.Forms.CheckState.Checked
        Me.Hide_Archive.Location = New System.Drawing.Point(358, 6)
        Me.Hide_Archive.Name = "Hide_Archive"
        Me.Hide_Archive.Size = New System.Drawing.Size(87, 17)
        Me.Hide_Archive.TabIndex = 6
        Me.Hide_Archive.Text = "Hide Archive"
        Me.Hide_Archive.UseVisualStyleBackColor = True
        '
        'TableLayoutMaster
        '
        Me.TableLayoutMaster.AutoSize = True
        Me.TableLayoutMaster.ColumnCount = 1
        Me.TableLayoutMaster.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutMaster.Controls.Add(Me.TableLayoutTopPanel, 0, 0)
        Me.TableLayoutMaster.Controls.Add(Me.TableLayoutBottomPanel, 0, 2)
        Me.TableLayoutMaster.Controls.Add(Me.OptionsPanel, 0, 1)
        Me.TableLayoutMaster.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutMaster.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutMaster.Name = "TableLayoutMaster"
        Me.TableLayoutMaster.RowCount = 3
        Me.TableLayoutMaster.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36.0!))
        Me.TableLayoutMaster.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutMaster.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        Me.TableLayoutMaster.Size = New System.Drawing.Size(454, 461)
        Me.TableLayoutMaster.TabIndex = 7
        '
        'TableLayoutTopPanel
        '
        Me.TableLayoutTopPanel.ColumnCount = 2
        Me.TableLayoutTopPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.4375!))
        Me.TableLayoutTopPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.5625!))
        Me.TableLayoutTopPanel.Controls.Add(Me.TextBox1, 0, 0)
        Me.TableLayoutTopPanel.Controls.Add(Me.Hide_Archive, 1, 0)
        Me.TableLayoutTopPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutTopPanel.Location = New System.Drawing.Point(3, 3)
        Me.TableLayoutTopPanel.Name = "TableLayoutTopPanel"
        Me.TableLayoutTopPanel.RowCount = 1
        Me.TableLayoutTopPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutTopPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutTopPanel.Size = New System.Drawing.Size(448, 30)
        Me.TableLayoutTopPanel.TabIndex = 6
        '
        'TableLayoutBottomPanel
        '
        Me.TableLayoutBottomPanel.ColumnCount = 6
        Me.TableLayoutBottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutBottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85.0!))
        Me.TableLayoutBottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85.0!))
        Me.TableLayoutBottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85.0!))
        Me.TableLayoutBottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85.0!))
        Me.TableLayoutBottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutBottomPanel.Controls.Add(Me.button_ok, 1, 0)
        Me.TableLayoutBottomPanel.Controls.Add(Me.button_autoassign, 4, 0)
        Me.TableLayoutBottomPanel.Controls.Add(Me.button_cancel, 2, 0)
        Me.TableLayoutBottomPanel.Controls.Add(Me.button_new, 3, 0)
        Me.TableLayoutBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutBottomPanel.Location = New System.Drawing.Point(3, 424)
        Me.TableLayoutBottomPanel.Name = "TableLayoutBottomPanel"
        Me.TableLayoutBottomPanel.RowCount = 1
        Me.TableLayoutBottomPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutBottomPanel.Size = New System.Drawing.Size(448, 34)
        Me.TableLayoutBottomPanel.TabIndex = 7
        '
        'TagViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(454, 461)
        Me.Controls.Add(Me.TableLayoutMaster)
        Me.Name = "TagViewer"
        Me.Text = "Tags"
        Me.TableLayoutMaster.ResumeLayout(False)
        Me.TableLayoutMaster.PerformLayout()
        Me.TableLayoutTopPanel.ResumeLayout(False)
        Me.TableLayoutTopPanel.PerformLayout()
        Me.TableLayoutBottomPanel.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents OptionsPanel As Windows.Forms.Panel
    Friend WithEvents button_ok As Windows.Forms.Button
    Friend WithEvents button_cancel As Windows.Forms.Button
    Friend WithEvents button_new As Windows.Forms.Button
    Friend WithEvents button_autoassign As Windows.Forms.Button
    Friend WithEvents TextBox1 As Windows.Forms.TextBox
    Friend WithEvents Hide_Archive As Windows.Forms.CheckBox
    Friend WithEvents TableLayoutMaster As Windows.Forms.TableLayoutPanel
    Friend WithEvents TableLayoutTopPanel As Windows.Forms.TableLayoutPanel
    Friend WithEvents TableLayoutBottomPanel As Windows.Forms.TableLayoutPanel
End Class
