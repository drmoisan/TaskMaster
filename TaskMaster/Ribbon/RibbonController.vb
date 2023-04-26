Imports Microsoft.Office.Tools.Ribbon
Imports ToDoModel
Imports UtilitiesVB
Imports UtilitiesCS
Imports System.Diagnostics
Imports TaskVisualization
Imports Outlook = Microsoft.Office.Interop.Outlook
Imports TaskTree

Public Class RibbonController
    Private _viewer As RibbonViewer
    Private _globals As IApplicationGlobals
    Private blHook As Boolean = True
    Private _quickfileLegacy As QuickFiler.Legacy.QfcLauncher
    Private _quickFiler As QuickFiler.Interfaces.IQfcHomeController

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
        Dim taskTreeController As TaskTreeController = New TaskTreeController(_globals, taskTreeViewer, dataModel)
        taskTreeViewer.Show()
    End Sub

    Friend Sub LoadQuickFilerOld()
        Dim loaded As Boolean = False
        If _quickfileLegacy IsNot Nothing Then loaded = _quickfileLegacy.Loaded
        If loaded = False Then
            _quickfileLegacy = New QuickFiler.Legacy.QfcLauncher(_globals, AddressOf ReleaseQuickFilerLegacy)
            _quickfileLegacy.Run()
        End If
    End Sub

    Friend Sub LoadQuickFiler()
        Dim loaded As Boolean = False
        If _quickFiler IsNot Nothing Then loaded = _quickFiler.Loaded
        If loaded = False Then
            _quickFiler = New QuickFiler.Controllers.QfcHomeController(_globals, AddressOf ReleaseQuickFiler)
            _quickFiler.Run()
        End If
    End Sub

    Private Sub ReleaseQuickFilerLegacy()
        _quickfileLegacy = Nothing
    End Sub

    Private Sub ReleaseQuickFiler()
        _quickFiler = Nothing
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

    Friend Sub Runtest()
        'UtilitiesCS.Examples.MSDemoConv.DemoConversation(_globals.Ol.App.ActiveExplorer.Selection.Item(1))
        Dim ObjItem As Object = _globals.Ol.App.ActiveExplorer.Selection.Item(1)
        Dim conv As Outlook.Conversation = ObjItem.GetConversation()
        Dim df As Microsoft.Data.Analysis.DataFrame = conv.GetDataFrame()
        Debug.WriteLine(df.PrettyText())
        df.Display()
        'Dim table As Outlook.Table = conv.GetTable(WithFolder:=True, WithStore:=True)
        'table.EnumerateTable()
    End Sub

End Class
