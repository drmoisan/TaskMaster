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
        Me.SuspendLayout()
        '
        'OptionsPanel
        '
        Me.OptionsPanel.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.OptionsPanel.AutoScroll = True
        Me.OptionsPanel.Location = New System.Drawing.Point(29, 45)
        Me.OptionsPanel.Name = "OptionsPanel"
        Me.OptionsPanel.Size = New System.Drawing.Size(393, 369)
        Me.OptionsPanel.TabIndex = 0
        '
        'button_ok
        '
        Me.button_ok.Location = New System.Drawing.Point(66, 420)
        Me.button_ok.Name = "button_ok"
        Me.button_ok.Size = New System.Drawing.Size(70, 30)
        Me.button_ok.TabIndex = 1
        Me.button_ok.Text = "OK"
        Me.button_ok.UseVisualStyleBackColor = True
        '
        'button_cancel
        '
        Me.button_cancel.Location = New System.Drawing.Point(150, 420)
        Me.button_cancel.Name = "button_cancel"
        Me.button_cancel.Size = New System.Drawing.Size(70, 30)
        Me.button_cancel.TabIndex = 2
        Me.button_cancel.Text = "Cancel"
        Me.button_cancel.UseVisualStyleBackColor = True
        '
        'button_new
        '
        Me.button_new.Location = New System.Drawing.Point(234, 420)
        Me.button_new.Name = "button_new"
        Me.button_new.Size = New System.Drawing.Size(70, 30)
        Me.button_new.TabIndex = 3
        Me.button_new.Text = "New"
        Me.button_new.UseVisualStyleBackColor = True
        '
        'button_autoassign
        '
        Me.button_autoassign.Location = New System.Drawing.Point(318, 420)
        Me.button_autoassign.Name = "button_autoassign"
        Me.button_autoassign.Size = New System.Drawing.Size(70, 30)
        Me.button_autoassign.TabIndex = 4
        Me.button_autoassign.Text = "AutoAssign"
        Me.button_autoassign.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBox1.Location = New System.Drawing.Point(32, 9)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(272, 26)
        Me.TextBox1.TabIndex = 5
        '
        'TagViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(454, 461)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.button_autoassign)
        Me.Controls.Add(Me.button_new)
        Me.Controls.Add(Me.button_cancel)
        Me.Controls.Add(Me.button_ok)
        Me.Controls.Add(Me.OptionsPanel)
        Me.Name = "TagViewer"
        Me.Text = "Tags"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents OptionsPanel As Windows.Forms.Panel
    Friend WithEvents button_ok As Windows.Forms.Button
    Friend WithEvents button_cancel As Windows.Forms.Button
    Friend WithEvents button_new As Windows.Forms.Button
    Friend WithEvents button_autoassign As Windows.Forms.Button
    Friend WithEvents TextBox1 As Windows.Forms.TextBox
End Class
