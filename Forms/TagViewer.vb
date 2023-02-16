Public Class TagViewer
    Private _controller As TagController

    Public Sub SetController(controller As TagController)
        _controller = controller
    End Sub

    Private Sub button_ok_Click(sender As Object, e As EventArgs) Handles button_ok.Click
        _controller.OK_Action()
    End Sub

    Private Sub button_new_Click(sender As Object, e As EventArgs) Handles button_new.Click

    End Sub

    Private Sub button_autoassign_Click(sender As Object, e As EventArgs) Handles button_autoassign.Click
        _controller.AutoAssign()
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        _controller.SearchAndReload()
    End Sub

    Private Sub button_cancel_Click(sender As Object, e As EventArgs) Handles button_cancel.Click
        _controller.Cancel_Action()
    End Sub
End Class