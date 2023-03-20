Imports System.Windows.Forms

Public Class QuickFileViewer
    Private _controller As QuickFileController

    Public Sub SetController(controller As QuickFileController)
        _controller = controller
    End Sub

    Private Sub Button_OK_Click(sender As Object, e As EventArgs) Handles Button_OK.Click
        _controller.Button_OK_Click()
    End Sub

    Private Sub Button_OK_KeyDown(sender As Object, e As KeyEventArgs) Handles Button_OK.KeyDown
        _controller.Button_OK_KeyDown(sender, e)
    End Sub

    Private Sub Button_OK_KeyUp(sender As Object, e As KeyEventArgs) Handles Button_OK.KeyUp
        _controller.Button_OK_KeyUp(sender, e)
    End Sub

    Private Sub Button_Undo_Click(sender As Object, e As EventArgs) Handles Button_Undo.Click
        _controller.Button_Undo_Click()
    End Sub

    Private Sub PanelMain_KeyDown(sender As Object, e As KeyEventArgs) Handles PanelMain.KeyDown
        _controller.PanelMain_KeyDown(sender, e)
    End Sub

    Private Sub PanelMain_KeyPress(sender As Object, e As KeyPressEventArgs) Handles PanelMain.KeyPress
        _controller.PanelMain_KeyPress(sender, e)
    End Sub

    Private Sub PanelMain_KeyUp(sender As Object, e As KeyEventArgs) Handles PanelMain.KeyUp
        _controller.PanelMain_KeyUp(sender, e)
    End Sub

    Private Sub spn_EmailPerLoad_ValueChanged(sender As Object, e As EventArgs) Handles spn_EmailPerLoad.ValueChanged
        _controller.spn_EmailPerLoad_Change()
    End Sub
End Class