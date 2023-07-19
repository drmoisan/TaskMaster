Imports Microsoft.Office.Tools.Ribbon
Imports ToDoModel
Imports UtilitiesVB

Public Class TaskRibbonViewer
    Private _controller As TaskRibbonController

    Public Sub SetController(Controller As TaskRibbonController)
        _controller = Controller
    End Sub

    'COMPLETE: 2023-02-24 Hook up FlagTask button to the class
    Private Sub BtnRefreshIDList_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnRefreshIDList.Click
        _controller.RefreshIDList()
    End Sub

    Private Sub BtnSplitToDoID_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnSplitToDoID.Click
        _controller.SplitToDoID()
    End Sub

    Private Sub BtnLoadTree_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnLoadTree.Click
        _controller.LoadTaskTree()
    End Sub

    Private Sub BtnReviseDictionary_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnReviseDictionary.Click
        _controller.ReviseProjectInfo()
    End Sub

    Private Sub BtnCompressIDs_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnCompressIDs.Click
        _controller.CompressIDs()
    End Sub

    Private Sub BtnMigrateIDs_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnMigrateIDs.Click
        'Globals.ThisAddIn.MigrateToDoIDs()
    End Sub

    Private Sub BtnHookToggle_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnHookToggle.Click
        _controller.ToggleEventsHook()
    End Sub

    Private Sub BtnHideHeadersNoChildren_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnHideHeadersNoChildren.Click
        _controller.HideHeadersNoChildren()
    End Sub

    Private Sub BtnFlagTask_Click(sender As Object, e As RibbonControlEventArgs) Handles BtnFlagTask.Click
        _controller.FlagAsTask()
    End Sub
End Class
