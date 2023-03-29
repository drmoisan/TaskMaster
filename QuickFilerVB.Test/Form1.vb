Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Dispose()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.ControlGroup1.ToggleAccelerator()
    End Sub

    Private Sub LoadControlGroup()
        Dim _controlGroup As New QuickFiler.QfcViewer()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.TableLayoutPanel1.RowCount += 1
        Me.TableLayoutPanel1.RowStyles.Insert(Me.TableLayoutPanel1.RowCount - 2, New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        Me.TableLayoutPanel1.Controls.Add(_controlGroup, 0, Me.TableLayoutPanel1.RowCount - 2)
        SetControlGroupOptions(_controlGroup)
        Me.TableLayoutPanel1.ResumeLayout(True)

    End Sub

    Private Sub SetControlGroupOptions(group As QfcViewer)
        With group
            .AutoSize = True
            .AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            .BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            .Dock = System.Windows.Forms.DockStyle.Fill
            .Padding = New System.Windows.Forms.Padding(3)
        End With
    End Sub

    Private Sub ButtonAdd_Click(sender As Object, e As EventArgs) Handles ButtonAdd.Click
        LoadControlGroup()
    End Sub
End Class