Option Explicit On
Imports System.Drawing
Imports System.Windows.Forms

Public Class cCheckBoxClass


    'By declaring Public WithEvents we can handle
    'events "collectively". In this case it is
    'the click event on a date label, and by
    'doing it this way we avoid writing click
    'events for each and every data label.
    Public WithEvents ctrlCB As CheckBox
    Public TrigByKeyChg As Boolean
    Private TrigByValChg As Boolean
    Private objUserForm As Object
    Private strTagPrefix As String
    Private strTemp As String


    Friend Function Init(objUF As Object, strPrefix As String)
        objUserForm = objUF
        strTagPrefix = strPrefix
        Return True
    End Function

    Private Sub ctrlCB_Click() Handles ctrlCB.Click
        If Not TrigByKeyChg Then
            strTemp = strTagPrefix & ctrlCB.Text
            objUserForm.ToggleChoice(strTemp)
            objUserForm.FocusCheckbox(ctrlCB)
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
                objUserForm.Select_Ctrl_By_Number(1)

            Case Keys.Up
                objUserForm.Select_Ctrl_By_Number(-1)

            Case Keys.Enter
                objUserForm.OK_Action()
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
