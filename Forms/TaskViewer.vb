Imports System.Windows
Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook
Imports System.Diagnostics
Imports UtilitiesVB

Public Class TaskViewer

    Private WithEvents _mouseFilter As MouseDownFilter
    Private _controller As TaskController

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.KeyPreview = True

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
            Dim e As KeyEventArgs = New KeyEventArgs(keyData)
            _controller.KeyboardHandler_KeyDown(sender, e)
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

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

    Private Sub Cat_Personal_Click(sender As Object, e As EventArgs) Handles Cat_Personal.Click
        _controller.Shortcut_Personal()
    End Sub

    Private Sub Cat_Meeting_Click(sender As Object, e As EventArgs) Handles Cat_Meeting.Click
        _controller.Shortcut_Meeting()
    End Sub

    Private Sub Cat_PreRead_Click(sender As Object, e As EventArgs) Handles Cat_PreRead.Click
        _controller.Shortcut_PreRead()
    End Sub

    Private Sub Cat_Internet_Click(sender As Object, e As EventArgs) Handles Cat_Internet.Click
        'TODO: Cat_Internet_Click -> hook function to controller
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

    Private Sub Cat_ReadingNews_Click(sender As Object, e As EventArgs) Handles Cat_News.Click
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

    Private Sub TaskViewer_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If Not _controller Is Nothing Then
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
        If Not _controller Is Nothing Then _controller.MouseFilter_FormClicked(sender, e)
    End Sub

    Private Sub task_name_KeyDown(sender As Object, e As KeyEventArgs) Handles task_name.KeyDown
        'Debug.WriteLine("task_name_keydown fired with " & e.KeyCode.ToChar)
    End Sub

    Private Sub task_name_KeyUp(sender As Object, e As KeyEventArgs) Handles task_name.KeyUp
        'Debug.WriteLine("task_name_keyup fired with " & e.KeyCode.ToChar)
    End Sub

    Private Sub task_name_KeyPress(sender As Object, e As KeyPressEventArgs) Handles task_name.KeyPress
        'Debug.WriteLine("task_name_keypress fired with " & e.KeyChar)
        If _controller.SuppressKeystrokes Then
            e.Handled = True
            'Debug.WriteLine("task_name_keypress suppressed keystrokes")
        End If
    End Sub
End Class