Imports Microsoft.Office.Interop.Outlook
Imports Tags
Imports ToDoModel
Imports UtilitiesVB
Imports UtilitiesCS
Imports TaskVisualization
Imports System.Runtime.CompilerServices

<Assembly: InternalsVisibleTo("TaskVisualization.Test")>
Public Class FlagTasks

    Private ReadOnly _todoSelection As List(Of ToDoItem)
    Private ReadOnly _olExplorer As Explorer
    Private WithEvents _viewer As TaskViewer
    Private ReadOnly _controller As TaskController
    Private ReadOnly _defaultsToDo As New ToDoDefaults()
    Private ReadOnly _autoAssign As AutoAssign
    Private ReadOnly _flagsToSet As TaskController.FlagsToSet
    Private ReadOnly _globals As IApplicationGlobals


    Public Sub New(AppGlobals As IApplicationGlobals,
                   Optional ItemList As IList = Nothing,
                   Optional blFile As Boolean = True,
                   Optional hWndCaller As IntPtr = Nothing,
                   Optional strNameOfFunctionCalling As String = "")

        _globals = AppGlobals
        _olExplorer = AppGlobals.Ol.App.ActiveExplorer
        _todoSelection = InitializeToDoList(ItemList)
        _flagsToSet = GetFlagsToSet(_todoSelection.Count)
        _viewer = New TaskViewer()
        '_defaultsToDo = New ToDoDefaults()
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
        _viewer.ShowDialog()
    End Sub

    Private Function InitializeToDoList(ItemList As IList) As List(Of ToDoItem)
        If ItemList Is Nothing Then ItemList = GetSelection()
        Dim ToDoSelection As New List(Of ToDoItem)()
        For Each ObjItem In ItemList
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
    Private Function GetSelection() As IList
        Dim ItemList As New List(Of Object)
        For Each obj In _olExplorer.Selection
            ItemList.Add(obj)
        Next obj
        Return ItemList
    End Function

    Private Function GetFlagsToSet(selectionCount As Integer) As TaskController.FlagsToSet
        If selectionCount > 1 Then

            Dim excludedMembers = {TaskController.FlagsToSet.all, TaskController.FlagsToSet.none}
            Dim symbolsDict = [Enum].GetValues(GetType(TaskController.FlagsToSet)) _
                                    .Cast(Of TaskController.FlagsToSet)() _
                                    .ToList() _
                                    .AsEnumerable() _
                                    .Where(Function(x) excludedMembers.Contains(x) = False) _
                                    .Select(Function(x) x) _
                                    .ToDictionary(
                                    Function(x) [Enum].GetName(GetType(TaskController.FlagsToSet), x),
                                    Function(x) x)

            Dim symbolSelectionDict = (From x In symbolsDict Select x.Key).ToDictionary(
                Function(x) x, Function(x) False).ToSortedDictionary()

            Dim listSelections As New List(Of String)

            Using optionsViewer As New TagViewer
                Dim flagController As New TagController(viewer_instance:=optionsViewer,
                                                        dictOptions:=symbolSelectionDict,
                                                        autoAssigner:=Nothing,
                                                        prefixes:=_defaultsToDo.PrefixList)
                optionsViewer.ShowDialog()
                If flagController._exit_type <> "Cancel" Then
                    listSelections = flagController.GetSelections()
                End If
            End Using
            If listSelections.Count = 0 Then
                Return TaskController.FlagsToSet.all
            Else
                Dim flag As TaskController.FlagsToSet
                Dim flagsList = (From x In listSelections Where [Enum].TryParse(x, flag) Select [Enum].Parse(GetType(TaskController.FlagsToSet), x)).ToList().OfType(Of TaskController.FlagsToSet)()
                'Dim flagsList2 = flagsList.OfType(Of TaskController.FlagsToSet)()
                'Dim flagsList = (From x In symbolsDict Where listSelections.Contains(x.Key) Select x.Value).ToList()
                'Dim selectedFlags As TaskController.FlagsToSet = GenericBitwise(Of TaskController.FlagsToSet).And(flagsList)
                Dim selectedFlags As TaskController.FlagsToSet = GenericBitwise(Of TaskController.FlagsToSet).[Or](flagsList)
                Return selectedFlags
            End If
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
