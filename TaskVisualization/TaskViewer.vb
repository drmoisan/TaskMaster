Imports System.Diagnostics
Imports System.Windows.Forms
Imports UtilitiesVB
Imports System

Public Class TaskViewer

    Private WithEvents _mouseFilter As MouseDownFilter
    Private _controller As TaskController

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        KeyPreview = True

        'Attach Handler to capture mouseclick anywhere on form
        _mouseFilter = New MouseDownFilter(Me)
        System.Windows.Forms.Application.AddMessageFilter(_mouseFilter)

    End Sub


    Public Sub SetController(controller As TaskController)
        _controller = controller
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
        If keyData.HasFlag(Keys.Alt) Then
            'If keyData = Keys.Up OrElse keyData = Keys.Down OrElse keyData = Keys.Left OrElse keyData = Keys.Right OrElse keyData = Keys.Alt Then
            Dim sender As Object = Control.FromHandle(msg.HWnd)
            Dim e As New KeyEventArgs(keyData)
            Dim unused = _controller.KeyboardHandler_KeyDown(sender, e)
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub Cancel_Button_Click(sender As Object, e As EventArgs) Handles Cancel_Button.Click
        _controller.Cancel_Action()
    End Sub

    Private Sub PeopleSelection_Click(sender As Object, e As EventArgs) Handles PeopleSelection.Click
        _controller.Assign_People()
    End Sub

    Private Sub CategorySelection_Click(sender As Object, e As EventArgs) Handles CategorySelection.Click
        _controller.Assign_Context()
    End Sub

    Private Sub ProjectSelection_Click(sender As Object, e As EventArgs) Handles ProjectSelection.Click
        _controller.Assign_Project()
    End Sub

    Private Sub TopicSelection_Click(sender As Object, e As EventArgs) Handles TopicSelection.Click
        _controller.Assign_Topic()
    End Sub

    Private Sub ShortcutPersonal_Click(sender As Object, e As EventArgs) Handles ShortcutPersonal.Click
        _controller.Shortcut_Personal()
    End Sub

    Private Sub ShortcutMeeting_Click(sender As Object, e As EventArgs) Handles ShortcutMeeting.Click
        _controller.Shortcut_Meeting()
    End Sub

    Private Sub ShortcutPreRead_Click(sender As Object, e As EventArgs) Handles ShortcutPreRead.Click
        _controller.Shortcut_PreRead()
    End Sub

    Private Sub ShortcutInternet_Click(sender As Object, e As EventArgs) Handles ShortcutInternet.Click
        'TODO: ShortcutInternet_Click -> hook function to controller
    End Sub

    Private Sub ShortcutCalls_Click(sender As Object, e As EventArgs) Handles ShortcutCalls.Click
        _controller.Shortcut_Calls()
    End Sub

    Private Sub ShortcutReadingBusiness_Click(sender As Object, e As EventArgs) Handles ShortcutReadingBusiness.Click
        _controller.Shortcut_ReadingBusiness()
    End Sub

    Private Sub ShortcutEmail_Click(sender As Object, e As EventArgs) Handles ShortcutEmail.Click
        _controller.Shortcut_Email()
    End Sub

    Private Sub ShortcutReadingNews_Click(sender As Object, e As EventArgs) Handles ShortcutNews.Click
        _controller.Shortcut_ReadingNews()
    End Sub

    Private Sub ShortcutUnprocessed_Click(sender As Object, e As EventArgs) Handles ShortcutUnprocessed.Click
        _controller.Shortcut_Unprocessed()
    End Sub

    Private Sub ShortcutWaitingFor_Click(sender As Object, e As EventArgs) Handles ShortcutWaitingFor.Click
        _controller.Shortcut_WaitingFor()
    End Sub

    Private Sub OKButton_Click(sender As Object, e As EventArgs) Handles OKButton.Click
        _controller.OK_Action()
    End Sub

    Private Sub CbxToday_CheckedChanged(sender As Object, e As EventArgs) Handles CbxToday.CheckedChanged
        _controller.Today_Change()
    End Sub

    Private Sub CbxBullpin_CheckedChanged(sender As Object, e As EventArgs) Handles CbxBullpin.CheckedChanged
        _controller.Bullpin_Change()
    End Sub

    Private Sub KbSelector_SelectedIndexChanged(sender As Object, e As EventArgs) Handles KbSelector.SelectedIndexChanged
        _controller.Assign_KB()
    End Sub

    Private Sub PriorityBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PriorityBox.SelectedIndexChanged
        _controller.Assign_Priority()
    End Sub

    Private Sub CbxFlag_CheckedChanged(sender As Object, e As EventArgs) Handles CbxFlagAsTask.CheckedChanged
        If _controller IsNot Nothing Then _controller.FlagAsTask_Change()
    End Sub

    Private Sub TaskViewer_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If _controller IsNot Nothing Then
            Debug.WriteLine(e.KeyCode.ToChar())
            Dim consumed As Boolean = _controller.KeyboardHandler_KeyDown(sender, e)
            If consumed Then
                e.Handled = True
                e.SuppressKeyPress = True
            Else
                e.Handled = False
            End If
        End If
    End Sub

    Private Sub _mouseFilter_FormClicked(sender As Object, e As EventArgs) Handles _mouseFilter.FormClicked
        If _controller IsNot Nothing Then _controller.MouseFilter_FormClicked(sender, e)
    End Sub

    Private Sub TaskName_KeyDown(sender As Object, e As KeyEventArgs) Handles TaskName.KeyDown
        'Debug.WriteLine("task_name_keydown fired with " & e.KeyCode.ToChar)
    End Sub

    Private Sub TaskName_KeyUp(sender As Object, e As KeyEventArgs) Handles TaskName.KeyUp
        'Debug.WriteLine("task_name_keyup fired with " & e.KeyCode.ToChar)
    End Sub

    Private Sub TaskName_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TaskName.KeyPress
        'Debug.WriteLine("task_name_keypress fired with " & e.KeyChar)
        If _controller.SuppressKeystrokes Then
            e.Handled = True
            'Debug.WriteLine("task_name_keypress suppressed keystrokes")
        End If
    End Sub
End Class