Imports System.Windows

Public Class TaskViewer
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private _controller As TaskController

    Public Sub SetController(controller As TaskController)
        _controller = controller
    End Sub

    Private Sub Cancel_Button_Click(sender As Object, e As EventArgs) Handles Cancel_Button.Click
        MsgBox("Cancel Click")
    End Sub
End Class