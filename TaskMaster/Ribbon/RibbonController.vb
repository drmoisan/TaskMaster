Imports Microsoft.Office.Tools.Ribbon
Imports ToDoModel
Imports UtilitiesVB

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
        _globals.ToDo.IDList_Refresh()
        MsgBox("ID Refresh Complete")
    End Sub

    Friend Sub SplitToDoID()
        Globals.ThisAddIn.Refresh_ToDoID_Splits()
    End Sub

    Friend Sub LoadTaskTree()
        Dim taskTreeViewer As TaskTreeForm = New TaskTreeForm
        Dim dataModel As TreeOfToDoItems = New TreeOfToDoItems(New List(Of TreeNode(Of ToDoItem)))
        dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, _globals.Ol.App)
        Dim taskTreeController As TaskTreeController = New TaskTreeController(taskTreeViewer, dataModel)
        taskTreeViewer.Show()
    End Sub

    Friend Sub ReviseProjectInfo()
        Dim _projInfoView As New ProjectInfoWindow(Globals.ThisAddIn.ProjInfo)
        _projInfoView.Show()
    End Sub

    Friend Sub CompressIDs()

        CompressToDoIDs(IDList:=_globals.ToDo.IDList, Application:=_globals.Ol.App)
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
