Imports Microsoft.Office.Tools.Ribbon

Public Class TaskMasterRibbon
    Private frmTTF As TaskTreeForm
    Private blHook As Boolean = True

    Private Sub btn_RefreshMax_Click(sender As Object, e As RibbonControlEventArgs) Handles btn_RefreshMax.Click
        Globals.ThisAddIn.RefreshIDList()
        MsgBox("ID Refresh Complete")
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

    End Sub

    Private Sub but_CompressIDs_Click(sender As Object, e As RibbonControlEventArgs) Handles but_CompressIDs.Click
        Globals.ThisAddIn.CompressToDoIDs()
        MsgBox("ID Compression Complete")
    End Sub

    Private Sub Button1_Click(sender As Object, e As RibbonControlEventArgs) Handles Button1.Click
        'Globals.ThisAddIn.FixToDoIDs()
    End Sub

    Private Sub BTN_Hook_Click(sender As Object, e As RibbonControlEventArgs) Handles BTN_Hook.Click
        If blHook = True Then
            Globals.ThisAddIn.Events_Unhook()
            blHook = False
            Me.BTN_Hook.Label = "Hook Events"
            MsgBox("Events Disconnected")
        Else
            Globals.ThisAddIn.Events_Hook()
            Me.BTN_Hook.Label = "UnHook Events"
            blHook = True
            MsgBox("Hooked Events")
        End If
    End Sub

    Private Sub btnHideHeadersNoChildren_Click(sender As Object, e As RibbonControlEventArgs) Handles btnHideHeadersNoChildren.Click
        Dim DMtmp = New DataModel_ToDoTree(New List(Of TreeNode(Of ToDoItem)))
        DMtmp.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadInView)
        DMtmp.HideEmptyHeadersInView()
    End Sub
End Class
