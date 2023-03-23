Imports System.ComponentModel
Imports System.Windows.Forms

Public Class QuickFileViewer
    Private _controller As QuickFileController

    Public Sub SetController(controller As QuickFileController)
        _controller = controller
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
        If keyData.HasFlag(Keys.Alt) Then
            'If keyData = Keys.Up OrElse keyData = Keys.Down OrElse keyData = Keys.Left OrElse keyData = Keys.Right OrElse keyData = Keys.Alt Then
            Dim sender As Object = Control.FromHandle(msg.HWnd)
            Dim e As New KeyEventArgs(keyData)
            _controller.KeyboardHandler_KeyDown(sender, e)
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub Button_OK_Click(sender As Object, e As EventArgs) Handles L1v2L2h3_ButtonOK.Click
        _controller.ButtonOK_Click()
    End Sub

    Private Sub Button_OK_KeyDown(sender As Object, e As KeyEventArgs) Handles L1v2L2h3_ButtonOK.KeyDown
        _controller.Button_OK_KeyDown(sender, e)
    End Sub

    Private Sub Button_OK_KeyUp(sender As Object, e As KeyEventArgs) Handles L1v2L2h3_ButtonOK.KeyUp
        _controller.Button_OK_KeyUp(sender, e)
    End Sub

    Private Sub Button_Undo_Click(sender As Object, e As EventArgs) Handles L1v2L2h4_ButtonUndo.Click
        _controller.ButtonUndo_Click()
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
        _controller.SpnEmailPerLoad_Change()
    End Sub

    Private Sub QuickFileViewer_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        _controller.Viewer_Activate()
    End Sub

    Private Sub L1v2L2h4_ButtonCancel_Click(sender As Object, e As EventArgs) Handles L1v2L2h4_ButtonCancel.Click
        _controller.ButtonCancel_Click()
    End Sub

    Private Sub QuickFileViewer_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        _controller.Cleanup()
    End Sub

    Private Sub AcceleratorDialogue_KeyDown(sender As Object, e As KeyEventArgs) Handles AcceleratorDialogue.KeyDown
        _controller.AcceleratorDialogue_KeyDown(sender, e)
    End Sub

    Private Sub AcceleratorDialogue_KeyUp(sender As Object, e As KeyEventArgs) Handles AcceleratorDialogue.KeyUp
        _controller.AcceleratorDialogue_KeyUp(sender, e)
    End Sub

    Private Sub AcceleratorDialogue_TextChanged(sender As Object, e As EventArgs) Handles AcceleratorDialogue.TextChanged
        _controller.AcceleratorDialogue_Change()
    End Sub
End Class