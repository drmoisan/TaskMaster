Imports System.Drawing
Imports System.Windows.Forms

Public Class AcControlGroup
    Private Sub AcControlGroup_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint

        If Me.BorderStyle = BorderStyle.FixedSingle Then
            Dim thickness As Integer = 2
            Dim halfThickness As Integer = thickness / 2

            Using p As Pen = New Pen(Color.Black, thickness)
                e.Graphics.DrawRectangle(p, New Rectangle(halfThickness, halfThickness, Me.ClientSize.Width - thickness, Me.ClientSize.Height - thickness))
            End Using
        End If
    End Sub
End Class
