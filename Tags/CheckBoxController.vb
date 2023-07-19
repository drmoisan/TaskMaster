Option Explicit On
Imports System.Windows.Forms

Public Class CheckBoxController


    'By declaring Public WithEvents we can handle
    'events "collectively". In this case it is
    'the click event on a date label, and by
    'doing it this way we avoid writing click
    'events for each and every data label.
    Public WithEvents ctrlCB As CheckBox
    Public TrigByKeyChg As Boolean
    Private TrigByValChg As Boolean
    Private _parent As TagController
    Private strTagPrefix As String
    Private strTemp As String


    Friend Function Init(parent As TagController, strPrefix As String)
        _parent = parent
        strTagPrefix = strPrefix
        Return True
    End Function

    Private Sub ctrlCB_Click() Handles ctrlCB.Click
        If Not TrigByKeyChg Then
            strTemp = strTagPrefix & ctrlCB.Text
            _parent.ToggleChoice(strTemp)
            _parent.FocusCheckbox(ctrlCB)
        ElseIf TrigByValChg Then
            TrigByKeyChg = False
            TrigByValChg = False
        Else
            TrigByValChg = True
            ctrlCB.Checked = Not ctrlCB.Checked
        End If
        'Me.ctrlCB.Value = Not Me.ctrlCB.Value
    End Sub

    Private Sub ctrlCB_KeyDown(sender As Object, e As KeyEventArgs) Handles ctrlCB.KeyDown
        Select Case e.KeyCode
            Case Keys.Down
                _parent.Select_Ctrl_By_Offset(1)

            Case Keys.Up
                _parent.Select_Ctrl_By_Offset(-1)

            Case Keys.End
                _parent.Select_Last_Control()

            Case Keys.Home
                _parent.Select_First_Control()

            Case Keys.PageDown
                _parent.Select_PageDown()

            Case Keys.PageUp
                _parent.Select_PageUp()

            Case Keys.Enter
                _parent.OK_Action()
        End Select
    End Sub

    Private Sub ctrlCB_GotFocus(sender As Object, e As EventArgs) Handles ctrlCB.GotFocus
        Dim ctrl = TryCast(sender, Control)
        Dim tmp_color As Drawing.Color = ctrl.BackColor
        ctrl.BackColor = ctrl.ForeColor
        ctrl.ForeColor = tmp_color
    End Sub

    Private Sub ctrlCB_LostFocus(sender As Object, e As EventArgs) Handles ctrlCB.LostFocus
        Dim ctrl = TryCast(sender, Control)
        Dim tmp_color As Drawing.Color = ctrl.BackColor
        ctrl.BackColor = ctrl.ForeColor
        ctrl.ForeColor = tmp_color
    End Sub

    Private Sub ctrlCB_PreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs) Handles ctrlCB.PreviewKeyDown
        Select Case e.KeyCode
            Case Keys.Down
                e.IsInputKey = True
            Case Keys.Up
                e.IsInputKey = True
        End Select
    End Sub
End Class
