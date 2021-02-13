Imports Microsoft.Office.Tools.Ribbon

Public Class TaskMasterRibbon
    Private frmTree As frm_TaskTree
    Private frmTTF As TaskTreeForm

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
        Dim strMsg As StringBuilder = New StringBuilder
        Dim strKey As String
        Dim i As Integer = 0
        For Each strKey In Globals.ThisAddIn.ProjDict.ProjectDictionary.Keys
            i += 1
            strMsg.AppendLine(i & " " & strKey & " " & Globals.ThisAddIn.ProjDict.ProjectDictionary(strKey))
        Next
        i = CInt(InputBox(strMsg.ToString()))
        Dim strKeyToDelete As String = Globals.ThisAddIn.ProjDict.ProjectDictionary.Keys(i)
        Dim response As MsgBoxResult = MsgBox("Delete key: " & strKeyToDelete & "?", vbYesNo)
        If response = vbYes Then
            Globals.ThisAddIn.ProjDict.ProjectDictionary.Remove(strKeyToDelete)
            Globals.ThisAddIn.SaveDict()
        End If
    End Sub

    Private Sub but_CompressIDs_Click(sender As Object, e As RibbonControlEventArgs) Handles but_CompressIDs.Click
        Globals.ThisAddIn.CompressToDoIDs()
    End Sub
End Class
