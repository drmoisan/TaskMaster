Imports System.Windows.Forms

Public Class TagViewer
    Private _controller As TagController

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.KeyPreview = False
    End Sub

    Public Sub SetController(controller As TagController)
        _controller = controller
    End Sub

    Private Sub button_ok_Click(sender As Object, e As EventArgs) Handles button_ok.Click
        _controller.OK_Action()
    End Sub

    Private Sub button_new_Click(sender As Object, e As EventArgs) Handles button_new.Click
        _controller.New_Action()
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

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        _controller.TextBox1_KeyDown(sender, e)
    End Sub

    Private Sub TextBox1_KeyUp(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyUp
        _controller.TextBox1_KeyUp(sender, e)
    End Sub

    Private Sub OptionsPanel_KeyDown(sender As Object, e As KeyEventArgs) Handles OptionsPanel.KeyDown
        _controller.OptionsPanel_KeyDown(sender, e)
    End Sub

    Private Sub TagViewer_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        _controller.TagViewer_KeyDown(sender, e)
    End Sub

    Private Sub OptionsPanel_PreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs) Handles OptionsPanel.PreviewKeyDown
        _controller.OptionsPanel_PreviewKeyDown(sender, e)
    End Sub
End Class