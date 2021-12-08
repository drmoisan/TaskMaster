Option Explicit On
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
    End Function

    Private Sub ctrlCB_Click() Handles ctrlCB.Click
        If Not TrigByKeyChg Then
            strTemp = strTagPrefix & ctrlCB.Text
            objUserForm.ToggleChoice(strTemp)
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
        Dim newpos As Integer
        Select Case e.KeyCode
        'Case vbKeyRight
        '    newpos = lActivePos + 1
        '    If newpos <= colLabelEvent.Count Then _
        '        colLabelEvent(newpos).GenerateClick
        'Case vbKeyLeft
        '    newpos = lActivePos - 1
        '    If newpos >= 1 Then _
        '        colLabelEvent(newpos).GenerateClick
            Case Keys.Down

                newpos = objUserForm.intFocus + 1
                If newpos <= objUserForm.colCheckbox.Count Then
                    objUserForm.colCheckboxEvent.item(newpos).TrigByKeyChg = True
                    objUserForm.colCheckbox.item(newpos).SetFocus
                    objUserForm.intFocus = newpos
                End If

            'newpos = Tags.intFocus + 1
            'If newpos <= Tags.colCheckbox.Count Then
            '    Tags.colCheckboxEvent.Item(newpos).TrigByKeyChg = True
            '    Tags.colCheckbox.Item(newpos).SetFocus
            '    Tags.intFocus = newpos
            'End If
            Case Keys.Up
                'Case vbKeyUp
                newpos = objUserForm.intFocus - 1
                If newpos >= 1 Then
                    objUserForm.colCheckboxEvent.item(newpos).TrigByKeyChg = True
                    objUserForm.colCheckbox.item(newpos).SetFocus
                    objUserForm.intFocus = newpos
                End If

                'newpos = Tags.intFocus - 1
                'If newpos >= 1 Then
                '    Tags.colCheckboxEvent.Item(newpos).TrigByKeyChg = True
                '    Tags.colCheckbox.Item(newpos).SetFocus
                '    Tags.intFocus = newpos
                '
                'End If
            Case Keys.Enter
                objUserForm.Call_OK
        End Select
    End Sub

End Class
