Imports Microsoft.Office.Tools.Ribbon

Public Class TaskMasterRibbon
    Private frmTree As frm_TaskTree
    Private frmTTF As TaskTreeForm
    Private Sub Ribbon1_Load(ByVal sender As System.Object, ByVal e As RibbonUIEventArgs) Handles MyBase.Load

    End Sub

    Private Sub btn_RefreshMax_Click(sender As Object, e As RibbonControlEventArgs) Handles btn_RefreshMax.Click
        Globals.ThisAddIn.RefreshToDoID_Max()
    End Sub

    Private Sub btn_TreeView_Click(sender As Object, e As RibbonControlEventArgs) Handles btn_TreeView.Click
        'If frmTree Is Nothing Then
        frmTree = New frm_TaskTree
        frmTree.Init()
        frmTree.Show()
        'Else
        'frmTree.Show()
        'End If
    End Sub

    Private Sub btn_SplitToDoID_Click(sender As Object, e As RibbonControlEventArgs) Handles btn_SplitToDoID.Click
        Globals.ThisAddIn.Refresh_ToDoID_Splits()
    End Sub

    Private Sub Btn_TreeListView_Click(sender As Object, e As RibbonControlEventArgs) Handles Btn_TreeListView.Click
        frmTTF = New TaskTreeForm
        frmTTF.Init_DataModel()
        frmTTF.Show()
    End Sub
End Class
