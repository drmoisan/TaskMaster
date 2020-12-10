<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Tags
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
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.OptionsFrame = New System.Windows.Forms.Panel()
        Me.Button_OK = New System.Windows.Forms.Button()
        Me.CommandButton3 = New System.Windows.Forms.Button()
        Me.Button_New = New System.Windows.Forms.Button()
        Me.Button_AutoAssign = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(12, 6)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(237, 20)
        Me.TextBox1.TabIndex = 0
        '
        'OptionsFrame
        '
        Me.OptionsFrame.AutoScroll = True
        Me.OptionsFrame.Location = New System.Drawing.Point(12, 32)
        Me.OptionsFrame.Name = "OptionsFrame"
        Me.OptionsFrame.Size = New System.Drawing.Size(393, 363)
        Me.OptionsFrame.TabIndex = 1
        '
        'Button_OK
        '
        Me.Button_OK.Location = New System.Drawing.Point(46, 401)
        Me.Button_OK.Name = "Button_OK"
        Me.Button_OK.Size = New System.Drawing.Size(75, 37)
        Me.Button_OK.TabIndex = 2
        Me.Button_OK.Text = "OK"
        Me.Button_OK.UseVisualStyleBackColor = True
        '
        'CommandButton3
        '
        Me.CommandButton3.Location = New System.Drawing.Point(127, 401)
        Me.CommandButton3.Name = "CommandButton3"
        Me.CommandButton3.Size = New System.Drawing.Size(75, 37)
        Me.CommandButton3.TabIndex = 3
        Me.CommandButton3.Text = "Cancel"
        Me.CommandButton3.UseVisualStyleBackColor = True
        '
        'Button_New
        '
        Me.Button_New.Location = New System.Drawing.Point(208, 401)
        Me.Button_New.Name = "Button_New"
        Me.Button_New.Size = New System.Drawing.Size(75, 37)
        Me.Button_New.TabIndex = 4
        Me.Button_New.Text = "New"
        Me.Button_New.UseVisualStyleBackColor = True
        '
        'Button_AutoAssign
        '
        Me.Button_AutoAssign.Location = New System.Drawing.Point(289, 401)
        Me.Button_AutoAssign.Name = "Button_AutoAssign"
        Me.Button_AutoAssign.Size = New System.Drawing.Size(75, 37)
        Me.Button_AutoAssign.TabIndex = 5
        Me.Button_AutoAssign.Text = "AutoAssign"
        Me.Button_AutoAssign.UseVisualStyleBackColor = True
        '
        'Tags
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(417, 450)
        Me.Controls.Add(Me.Button_AutoAssign)
        Me.Controls.Add(Me.Button_New)
        Me.Controls.Add(Me.CommandButton3)
        Me.Controls.Add(Me.Button_OK)
        Me.Controls.Add(Me.OptionsFrame)
        Me.Controls.Add(Me.TextBox1)
        Me.Name = "Tags"
        Me.Text = "Tags"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents TextBox1 As Windows.Forms.TextBox
    Friend WithEvents OptionsFrame As Windows.Forms.Panel
    Friend WithEvents Button_OK As Windows.Forms.Button
    Friend WithEvents CommandButton3 As Windows.Forms.Button
    Friend WithEvents Button_New As Windows.Forms.Button
    Friend WithEvents Button_AutoAssign As Windows.Forms.Button
End Class
