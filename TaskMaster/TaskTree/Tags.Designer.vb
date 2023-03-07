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
        Me.HideArchiveList = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(16, 7)
        Me.TextBox1.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(315, 22)
        Me.TextBox1.TabIndex = 0
        '
        'OptionsFrame
        '
        Me.OptionsFrame.AutoScroll = True
        Me.OptionsFrame.Location = New System.Drawing.Point(16, 39)
        Me.OptionsFrame.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.OptionsFrame.Name = "OptionsFrame"
        Me.OptionsFrame.Size = New System.Drawing.Size(524, 447)
        Me.OptionsFrame.TabIndex = 1
        '
        'Button_OK
        '
        Me.Button_OK.Location = New System.Drawing.Point(61, 494)
        Me.Button_OK.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Button_OK.Name = "Button_OK"
        Me.Button_OK.Size = New System.Drawing.Size(100, 46)
        Me.Button_OK.TabIndex = 2
        Me.Button_OK.Text = "OK"
        Me.Button_OK.UseVisualStyleBackColor = True
        '
        'CommandButton3
        '
        Me.CommandButton3.Location = New System.Drawing.Point(169, 494)
        Me.CommandButton3.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.CommandButton3.Name = "CommandButton3"
        Me.CommandButton3.Size = New System.Drawing.Size(100, 46)
        Me.CommandButton3.TabIndex = 3
        Me.CommandButton3.Text = "Cancel"
        Me.CommandButton3.UseVisualStyleBackColor = True
        '
        'Button_New
        '
        Me.Button_New.Location = New System.Drawing.Point(277, 494)
        Me.Button_New.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Button_New.Name = "Button_New"
        Me.Button_New.Size = New System.Drawing.Size(100, 46)
        Me.Button_New.TabIndex = 4
        Me.Button_New.Text = "New"
        Me.Button_New.UseVisualStyleBackColor = True
        '
        'Button_AutoAssign
        '
        Me.Button_AutoAssign.Location = New System.Drawing.Point(385, 494)
        Me.Button_AutoAssign.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Button_AutoAssign.Name = "Button_AutoAssign"
        Me.Button_AutoAssign.Size = New System.Drawing.Size(100, 46)
        Me.Button_AutoAssign.TabIndex = 5
        Me.Button_AutoAssign.Text = "AutoAssign"
        Me.Button_AutoAssign.UseVisualStyleBackColor = True
        '
        'HideArchiveList
        '
        Me.HideArchiveList.AutoSize = True
        Me.HideArchiveList.Checked = True
        Me.HideArchiveList.CheckState = System.Windows.Forms.CheckState.Checked
        Me.HideArchiveList.Location = New System.Drawing.Point(364, 7)
        Me.HideArchiveList.Name = "HideArchiveList"
        Me.HideArchiveList.Size = New System.Drawing.Size(136, 21)
        Me.HideArchiveList.TabIndex = 6
        Me.HideArchiveList.Text = "Hide Archive List"
        Me.HideArchiveList.UseVisualStyleBackColor = True
        '
        'Tags
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(556, 554)
        Me.Controls.Add(Me.HideArchiveList)
        Me.Controls.Add(Me.Button_AutoAssign)
        Me.Controls.Add(Me.Button_New)
        Me.Controls.Add(Me.CommandButton3)
        Me.Controls.Add(Me.Button_OK)
        Me.Controls.Add(Me.OptionsFrame)
        Me.Controls.Add(Me.TextBox1)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
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
    Friend WithEvents HideArchiveList As Windows.Forms.CheckBox
End Class
