Imports Microsoft.Office.Tools.Ribbon
Imports ToDoModel
Imports UtilitiesVB
Imports QuickFiler
Imports TaskVisualization

Public Class RibbonController
    Private _viewer As RibbonViewer
    Private _globals As IApplicationGlobals
    Private blHook As Boolean = True

    Public Sub New()
    End Sub

    Friend Sub SetGlobals(AppGlobals As IApplicationGlobals)
        _globals = AppGlobals
    End Sub

    Friend Sub SetViewer(Viewer As RibbonViewer)
        _viewer = Viewer
    End Sub

    Friend Sub RefreshIDList()
        '_globals.TD.IDList_Refresh()
        _globals.TD.IDList.RefreshIDList(_globals.Ol.App)
        MsgBox("ID Refresh Complete")
    End Sub

    Friend Sub SplitToDoID()
        Refresh_ToDoID_Splits(_globals.Ol.App)
    End Sub

    Friend Sub LoadTaskTree()
        Dim taskTreeViewer As TaskTreeForm = New TaskTreeForm
        Dim dataModel As TreeOfToDoItems = New TreeOfToDoItems(New List(Of TreeNode(Of ToDoItem)))
        dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, _globals.Ol.App)
        Dim taskTreeController As TaskTreeController = New TaskTreeController(taskTreeViewer, dataModel)
        taskTreeViewer.Show()
    End Sub

    Friend Sub LoadQuickFiler()
        Dim _viewer = New QuickFileViewer()
        Dim _controller = New QuickFileController(_globals, _viewer)

        '_controller.SetAPIOptions()
    End Sub

    Friend Sub LoadQuickFilerOrig()
        Dim _viewer = New QuickFileViewerOrig()
        Dim _controller = New QuickFileControllerOrig(_globals, _viewer)

        '_controller.SetAPIOptions()
    End Sub

    Friend Sub ReviseProjectInfo()
        Dim _projInfoView As New ProjectInfoWindow(Globals.ThisAddIn.ProjInfo)
        _projInfoView.Show()
    End Sub

    Friend Sub CompressIDs()
        _globals.TD.IDList.CompressToDoIDs(_globals.Ol.App)
        MsgBox("ID Compression Complete")
    End Sub

    Private Sub BtnMigrateIDs_Click(sender As Object, e As RibbonControlEventArgs)
        'Globals.ThisAddIn.MigrateToDoIDs()
    End Sub

    Friend Function GetHookButtonText(control As Office.IRibbonControl) As String
        If blHook Then
            Return "Unhook Events"
        Else
            Return "Hook Events"
        End If
    End Function

    Friend Sub ToggleEventsHook(Ribbon As Office.IRibbonUI)
        If blHook = True Then
            Globals.ThisAddIn.Events_Unhook()
            blHook = False
            Ribbon.InvalidateControl("BtnHookToggle")
            MsgBox("Events Disconnected")
        Else
            Globals.ThisAddIn.Events_Hook()
            blHook = True
            Ribbon.InvalidateControl("BtnHookToggle")
            MsgBox("Hooked Events")
        End If
    End Sub

    Friend Sub HideHeadersNoChildren()
        Dim dataTree = New TreeOfToDoItems(New List(Of TreeNode(Of ToDoItem)))
        dataTree.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, Globals.ThisAddIn.Application)
        dataTree.HideEmptyHeadersInView()
    End Sub

    Friend Sub FlagAsTask()
        Dim taskFlagger As New FlagTasks(_globals)
        taskFlagger.Run()
    End Sub

End Class
