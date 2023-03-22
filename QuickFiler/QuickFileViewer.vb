Imports System.Windows.Forms

Public Class QuickFileViewer
    Private _controller As QuickFileController

    Public Sub SetController(controller As QuickFileController)
        _controller = controller
    End Sub

    Private Sub Button_OK_Click(sender As Object, e As EventArgs) Handles L1v2L2h3_ButtonOK.Click
        _controller.Button_OK_Click()
    End Sub

    Private Sub Button_OK_KeyDown(sender As Object, e As KeyEventArgs) Handles L1v2L2h3_ButtonOK.KeyDown
        _controller.Button_OK_KeyDown(sender, e)
    End Sub

    Private Sub Button_OK_KeyUp(sender As Object, e As KeyEventArgs) Handles L1v2L2h3_ButtonOK.KeyUp
        _controller.Button_OK_KeyUp(sender, e)
    End Sub

    Private Sub Button_Undo_Click(sender As Object, e As EventArgs) Handles L1v2L2h4_ButtonUndo.Click
        _controller.Button_Undo_Click()
    End Sub

    Private Sub PanelMain_KeyDown(sender As Object, e As KeyEventArgs) Handles L1v1L2_PanelMain.KeyDown
        _controller.PanelMain_KeyDown(sender, e)
    End Sub

    Private Sub PanelMain_KeyPress(sender As Object, e As KeyPressEventArgs) Handles L1v1L2_PanelMain.KeyPress
        _controller.PanelMain_KeyPress(sender, e)
    End Sub

    Private Sub PanelMain_KeyUp(sender As Object, e As KeyEventArgs) Handles L1v1L2_PanelMain.KeyUp
        _controller.PanelMain_KeyUp(sender, e)
    End Sub

    Private Sub spn_EmailPerLoad_ValueChanged(sender As Object, e As EventArgs) Handles L1v2L2h5_SpnEmailPerLoad.ValueChanged
        _controller.spn_EmailPerLoad_Change()
    End Sub
End Class