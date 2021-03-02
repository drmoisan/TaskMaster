Imports Microsoft.Office.Tools.Ribbon

Public Class TaskMasterRibbon
    Private frmTTF As TaskTreeForm
    Private blHook As Boolean = True

    Private Sub btn_RefreshMax_Click(sender As Object, e As RibbonControlEventArgs) Handles btn_RefreshMax.Click
        Globals.ThisAddIn.RefreshIDList()
    End Sub

    Private Sub btn_SplitToDoID_Click(sender As Object, e As RibbonControlEventArgs) Handles btn_SplitToDoID.Click
        Globals.ThisAddIn.Refresh_ToDoID_Splits()
    End Sub

    Private Sub Btn_TreeListView_Click(sender As Object, e As RibbonControlEventArgs) Handles Btn_TreeListView.Click
        frmTTF = New TaskTreeForm
        frmTTF.Init_DataModel()
        frmTTF.Show()
    End Sub

    Private Sub but_Dictionary_Click(sender As Object, e As RibbonControlEventArgs) Handles but_Dictionary.Click
        Dim projinfoview As ProjectInfoWindow = New ProjectInfoWindow(Globals.ThisAddIn.ProjInfo)
        projinfoview.Show()
        'Original Function
        'Dim strMsg As StringBuilder = New StringBuilder
        'Dim strKey As String
        'Dim i As Integer = 0
        'Dim reversedict As SortedDictionary(Of String, String) = New SortedDictionary(Of String, String)
        'For Each strKey In Globals.ThisAddIn.ProjDict.ProjectDictionary.Keys
        '    Try
        '        reversedict.Add(Globals.ThisAddIn.ProjDict.ProjectDictionary(strKey), strKey)
        '    Catch
        '        MsgBox("Can't add:" + Globals.ThisAddIn.ProjDict.ProjectDictionary(strKey) + ", " + strKey)
        '        Err.Clear()
        '    End Try
        'Next
        'For Each strKey In reversedict.Keys
        '    strMsg.AppendLine(i & " " & strKey & " " & reversedict(strKey))
        '    i += 1
        'Next
        'i = CInt(InputBox(strMsg.ToString()))
        'Dim strKeyToDelete As String = reversedict.Keys(i)
        'Dim response As MsgBoxResult = MsgBox("Delete key: " & strKeyToDelete & "?", vbYesNo)
        'If response = vbYes Then
        '    Globals.ThisAddIn.ProjDict.ProjectDictionary.Remove(strKeyToDelete)
        '    Globals.ThisAddIn.SaveDict()
        'End If
    End Sub

    Private Sub but_CompressIDs_Click(sender As Object, e As RibbonControlEventArgs) Handles but_CompressIDs.Click
        Globals.ThisAddIn.CompressToDoIDs()
    End Sub

    Private Sub Button1_Click(sender As Object, e As RibbonControlEventArgs) Handles Button1.Click
        'Globals.ThisAddIn.FixToDoIDs()
    End Sub

    Private Sub BTN_Hook_Click(sender As Object, e As RibbonControlEventArgs) Handles BTN_Hook.Click
        If blHook = True Then
            Globals.ThisAddIn.Events_Unhook()
            blHook = False
            Me.BTN_Hook.Label = "UnHook Events"
            MsgBox("Events Disconnected")
        Else
            Globals.ThisAddIn.Events_Hook()
            Me.BTN_Hook.Label = "Hook Events"
            blHook = True
            MsgBox("Hooked Events")
        End If
    End Sub
End Class
