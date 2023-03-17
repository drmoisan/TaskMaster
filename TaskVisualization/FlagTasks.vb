Imports Microsoft.Office.Interop.Outlook
Imports Tags
Imports ToDoModel
Imports UtilitiesVB
Imports TaskVisualization

Public Class FlagTasks

    Private ReadOnly _todoSelection As List(Of ToDoItem)
    Private ReadOnly _olExplorer As Explorer
    Private WithEvents _viewer As TaskViewer
    Private ReadOnly _controller As TaskController
    Private ReadOnly _defaultsToDo As ToDoDefaults
    Private ReadOnly _autoAssign As AutoAssign
    Private ReadOnly _flagsToSet As TaskController.FlagsToSet
    Private ReadOnly _globals As IApplicationGlobals


    Public Sub New(AppGlobals As IApplicationGlobals,
                   Optional ItemCollection As Collection = Nothing,
                   Optional blFile As Boolean = True,
                   Optional hWndCaller As IntPtr = Nothing,
                   Optional strNameOfFunctionCalling As String = "")

        _globals = AppGlobals
        _olExplorer = AppGlobals.Ol.App.ActiveExplorer
        _todoSelection = InitializeToDoList(ItemCollection)
        _flagsToSet = GetFlagsToSet(_todoSelection.Count)
        _viewer = New TaskViewer()
        _defaultsToDo = New ToDoDefaults()
        _autoAssign = New AutoAssign(AppGlobals)
        _controller = New TaskController(FormInstance:=_viewer,
                                         OlCategories:=AppGlobals.Ol.NamespaceMAPI.Categories,
                                         ToDoSelection:=_todoSelection,
                                         Defaults:=_defaultsToDo,
                                         AutoAssign:=_autoAssign,
                                         FlagOptions:=_flagsToSet)

    End Sub

    Public Sub Run()
        _controller.LoadInitialValues()
        _viewer.Show()
    End Sub

    Private Function InitializeToDoList(ItemCollection As Collection) As List(Of ToDoItem)
        If ItemCollection Is Nothing Then ItemCollection = GetSelection()
        Dim ToDoSelection As New List(Of ToDoItem)()
        For Each ObjItem In ItemCollection
            Dim tmpToDo As ToDoItem
            If TypeOf ObjItem Is MailItem Then
                Dim OlMail As MailItem = ObjItem
                tmpToDo = New ToDoItem(OlMail)
            ElseIf TypeOf ObjItem Is TaskItem Then
                Dim OlTask As TaskItem = ObjItem
                tmpToDo = New ToDoItem(OlTask)
            Else
                tmpToDo = New ToDoItem(ObjItem, OnDemand:=True)
            End If
            ToDoSelection.Add(tmpToDo)
        Next
        Return ToDoSelection
    End Function

    ''' <summary>
    ''' Adds the Selection from the ActiveExplorer to a new Collection
    ''' </summary>
    ''' <returns>Collection of Outlook Items</returns>
    Private Function GetSelection() As Collection
        Dim ItemCollection As New Collection
        For Each obj In _olExplorer.Selection
            ItemCollection.Add(obj)
        Next obj
        Return ItemCollection
    End Function

    Private Function GetFlagsToSet(selectionCount As Integer) As TaskController.FlagsToSet
        If selectionCount > 1 Then
            Dim unused = MsgBox("GetFlagsToSet Not Implemented. Setting all Flags.")
            Return TaskController.FlagsToSet.all
        Else
            Return TaskController.FlagsToSet.all
        End If
    End Function

    Private Class AutoAssign
        Implements IAutoAssign

        Private ReadOnly _globals As IApplicationGlobals

        Public Sub New(globals As IApplicationGlobals)
            _globals = globals
        End Sub

        Public ReadOnly Property FilterList As List(Of String) Implements IAutoAssign.FilterList
            Get
                Return _globals.TD.CategoryFilters.ToList()
            End Get
        End Property

        Public Function AutoFind(objItem As Object) As Collection Implements IAutoAssign.AutoFind
            Return AutoFile.AutoFindPeople(objItem:=objItem,
                                           ppl_dict:=_globals.TD.DictPPL,
                                           emailRootFolder:=_globals.Ol.EmailRootPath,
                                           dictRemap:=_globals.TD.DictRemap,
                                           blExcludeFlagged:=False)

        End Function

        Public Function AddChoicesToDict(olMail As MailItem,
                                         prefixes As List(Of IPrefix),
                                         prefixKey As String) As Collection Implements IAutoAssign.AddChoicesToDict

            Return AutoFile.dictPPL_AddMissingEntries(OlMail:=olMail,
                                ppl_dict:=_globals.TD.DictPPL,
                                dictRemap:=_globals.TD.DictRemap,
                                prefixes:=prefixes,
                                prefixKey:=prefixKey,
                                emailRootFolder:=_globals.Ol.EmailRootPath,
                                stagingPath:=_globals.FS.FldrStaging,
                                filename_dictppl:=_globals.TD.DictPPL_Filename,
                                dictPPLSave:=AddressOf _globals.TD.DictPPL_Save)

        End Function

        Public Function AddColorCategory(prefix As IPrefix,
                                         categoryName As String) _
                                         As Category Implements IAutoAssign.AddColorCategory

            Return CreateCategory(OlNS:=_globals.Ol.NamespaceMAPI,
                                  prefix:=prefix,
                                  newCatName:=categoryName)
        End Function
    End Class

End Class
