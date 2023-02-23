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

    Private Sub category_selection_Click(sender As Object, e As EventArgs) Handles category_selection.Click
        _controller.Assign_Context()
    End Sub

    Private Sub project_selection_Click(sender As Object, e As EventArgs) Handles project_selection.Click
        _controller.Assign_Project()
    End Sub

    Private Sub topic_selection_Click(sender As Object, e As EventArgs) Handles topic_selection.Click
        _controller.Assign_Topic()
    End Sub

    Private Sub Cat_Deskwork_Click(sender As Object, e As EventArgs) Handles Cat_Deskwork.Click
        _controller.Shortcut_Personal()
    End Sub

    Private Sub Cat_Agenda_Click(sender As Object, e As EventArgs) Handles Cat_Agenda.Click
        _controller.Shortcut_Meeting()
    End Sub

    Private Sub Cat_PreRead_Click(sender As Object, e As EventArgs) Handles Cat_PreRead.Click
        _controller.Shortcut_PreRead()
    End Sub

    Private Sub Cat_Internet_Click(sender As Object, e As EventArgs) Handles Cat_Internet.Click

    End Sub

    Private Sub Cat_Calls_Click(sender As Object, e As EventArgs) Handles Cat_Calls.Click
        _controller.Shortcut_Calls()
    End Sub

    Private Sub Cat_ReadingBusiness_Click(sender As Object, e As EventArgs) Handles Cat_ReadingBusiness.Click
        _controller.Shortcut_ReadingBusiness()
    End Sub

    Private Sub Cat_Email_Click(sender As Object, e As EventArgs) Handles Cat_Email.Click
        _controller.Shortcut_Email()
    End Sub

    Private Sub Cat_ReadingOther_Click(sender As Object, e As EventArgs) Handles Cat_ReadingOther.Click
        _controller.Shortcut_ReadingNews()
    End Sub

    Private Sub Cat_Unprocessed_Click(sender As Object, e As EventArgs) Handles Cat_Unprocessed.Click
        _controller.Shortcut_Unprocessed()
    End Sub

    Private Sub Cat_WaitingFor_Click(sender As Object, e As EventArgs) Handles Cat_WaitingFor.Click
        _controller.Shortcut_WaitingFor()
    End Sub

    Private Sub OK_Button_Click(sender As Object, e As EventArgs) Handles OK_Button.Click
        _controller.OK_Action()
    End Sub

    Private Sub cbx_today_CheckedChanged(sender As Object, e As EventArgs) Handles cbx_today.CheckedChanged
        _controller.Today_Change()
    End Sub

    Private Sub cbx_bullpin_CheckedChanged(sender As Object, e As EventArgs) Handles cbx_bullpin.CheckedChanged
        _controller.Bullpin_Change()
    End Sub

    Private Sub kb_selector_SelectedIndexChanged(sender As Object, e As EventArgs) Handles kb_selector.SelectedIndexChanged
        _controller.Assign_KB()
    End Sub

    Private Sub Priority_Box_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Priority_Box.SelectedIndexChanged
        _controller.Assign_Priority()
    End Sub

    Private Sub cbxFlag_CheckedChanged(sender As Object, e As EventArgs) Handles cbxFlag.CheckedChanged
        If Not _controller Is Nothing Then _controller.FlagAsTask_Change()
    End Sub
End Class