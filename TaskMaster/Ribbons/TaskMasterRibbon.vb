Imports Microsoft.Office.Tools.Ribbon
Imports ToDoModel

Public Class TaskMasterRibbon
    Private frmTTF As TaskTreeForm
    Private blHook As Boolean = True

    'COMPLETE: 2023-02-24 Hook up FlagTask button to the class
    Private Sub btn_RefreshMax_Click(sender As Object, e As RibbonControlEventArgs) Handles btn_RefreshMax.Click
        Dim unused1 = Globals.ThisAddIn.RefreshIDList()
        Dim unused = MsgBox("ID Refresh Complete")
    End Sub

    Private Sub btn_SplitToDoID_Click(sender As Object, e As RibbonControlEventArgs) Handles btn_SplitToDoID.Click
        Globals.ThisAddIn.Refresh_ToDoID_Splits()
    End Sub

    Private Sub Btn_TreeListView_Click(sender As Object, e As RibbonControlEventArgs) Handles Btn_TreeListView.Click
        frmTTF = New TaskTreeForm
        Dim unused = frmTTF.Init_DataModel()
        frmTTF.Show()
    End Sub

    Private Sub but_Dictionary_Click(sender As Object, e As RibbonControlEventArgs) Handles but_Dictionary.Click
        Dim projinfoview As New ProjectInfoWindow(Globals.ThisAddIn.ProjInfo)
        projinfoview.Show()

    End Sub

    Private Sub but_CompressIDs_Click(sender As Object, e As RibbonControlEventArgs) Handles but_CompressIDs.Click
        Globals.ThisAddIn.CompressToDoIDs()
        Dim unused = MsgBox("ID Compression Complete")
    End Sub

    Private Sub Button1_Click(sender As Object, e As RibbonControlEventArgs) Handles Button1.Click
        'Globals.ThisAddIn.MigrateToDoIDs()
    End Sub

    Private Sub BTN_Hook_Click(sender As Object, e As RibbonControlEventArgs) Handles BTN_Hook.Click
        If blHook = True Then
            Globals.ThisAddIn.Events_Unhook()
            blHook = False
            BTN_Hook.Label = "Hook Events"
            Dim unused1 = MsgBox("Events Disconnected")
        Else
            Globals.ThisAddIn.Events_Hook()
            BTN_Hook.Label = "UnHook Events"
            blHook = True
            Dim unused = MsgBox("Hooked Events")
        End If
    End Sub

    Private Sub btnHideHeadersNoChildren_Click(sender As Object, e As RibbonControlEventArgs) Handles btnHideHeadersNoChildren.Click
        Dim DMtmp = New DataModel_ToDoTree(New List(Of TreeNode(Of ToDoItem)))
        DMtmp.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadInView, Globals.ThisAddIn.Application)
        DMtmp.HideEmptyHeadersInView()
    End Sub

    Private Sub BTN_FlagTask_Click(sender As Object, e As RibbonControlEventArgs) Handles BTN_FlagTask.Click
        Dim FT As New Flag_Tasks(Globals.ThisAddIn._globals)
        FT.Run()
    End Sub
End Class
