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
        _controller.Cancel_Action()
    End Sub

    Private Sub people_selection_Click(sender As Object, e As EventArgs) Handles people_selection.Click
        _controller.Assign_People()
    End Sub
End Class