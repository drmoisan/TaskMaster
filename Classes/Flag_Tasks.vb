Imports Microsoft.VisualBasic
Imports Microsoft.Office.Interop.Outlook
Imports ToDoModel
Imports Tags
Imports UtilitiesVB

Public Class Flag_Tasks


    Private ReadOnly _todoSelection As List(Of ToDoItem)
    Private ReadOnly _olExplorer As Explorer = Globals.ThisAddIn.Application.ActiveExplorer()
    Private WithEvents _viewer As TaskViewer
    Private ReadOnly _controller As TaskController
    Private _defaultsToDo As ToDoDefaults
    Private _autoAssign As AutoAssign
    Private _flagsToSet As TaskController.FlagsToSet


    Public Sub New(Optional ItemCollection As Collection = Nothing,
                   Optional blFile As Boolean = True,
                   Optional hWndCaller As IntPtr = Nothing,
                   Optional strNameOfFunctionCalling As String = "")

        _todoSelection = InitializeToDoList(ItemCollection)
        _flagsToSet = GetFlagsToSet(_todoSelection.Count)
        _viewer = New TaskViewer()
        _defaultsToDo = New ToDoDefaults()
        _autoAssign = New AutoAssign()
        _controller = New TaskController(_viewer,
                                         _todoSelection,
                                         _defaultsToDo,
                                         _autoAssign,
                                         _flagsToSet)

    End Sub

    Public Sub Run()
        _controller.LoadInitialValues()
        _viewer.Show()
    End Sub

    Private Function InitializeToDoList(ItemCollection As Collection) As List(Of ToDoItem)
        If ItemCollection Is Nothing Then ItemCollection = GetSelection()
        Dim ToDoSelection As List(Of ToDoItem) = New List(Of ToDoItem)()
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
        Dim ItemCollection As Collection = New Collection
        For Each obj In _olExplorer.Selection
            ItemCollection.Add(obj)
        Next obj
        Return ItemCollection
    End Function

    Private Function GetFlagsToSet(selectionCount As Integer) As TaskController.FlagsToSet
        If selectionCount > 1 Then
            Return TaskController.FlagsToSet.all
        Else
            MsgBox("GetFlagsToSet Not Implemented. Setting all Flags.")
            Return TaskController.FlagsToSet.all
        End If
    End Function

    Private Class AutoAssign
        Implements IAutoAssign

        Public ReadOnly Property FilterList As List(Of String) Implements IAutoAssign.FilterList
            Get
                If Globals.ThisAddIn.CCOCatList Is Nothing Then
                    Flag_Fields_Categories.CCOCatList_Load()
                End If
                Return Globals.ThisAddIn.CCOCatList
            End Get
        End Property

        Public Function AutoFind(objItem As Object) As Collection Implements IAutoAssign.AutoFind
            Return AutoFile.AutoFindPeople(objItem:=objItem,
                                           ppl_dict:=Globals.ThisAddIn.DictPPL,
                                           emailRootFolder:=Globals.ThisAddIn.EmailRoot,
                                           stagingPath:=Globals.ThisAddIn.StagingPath,
                                           blExcludeFlagged:=False)
        End Function

        Public Function AddChoicesToDict(olMail As MailItem,
                                         prefixes As List(Of IPrefix),
                                         prefixKey As String) As Collection Implements IAutoAssign.AddChoicesToDict

            Return AutoFile.dictPPL_AddMissingEntries(OlMail:=olMail,
                                ppl_dict:=Globals.ThisAddIn.DictPPL,
                                prefixes:=prefixes,
                                prefixKey:=prefixKey,
                                emailRootFolder:=Globals.ThisAddIn.EmailRoot,
                                stagingPath:=Globals.ThisAddIn.StagingPath,
                                filename_dictppl:=Globals.ThisAddIn.FilenameDictPpl)

        End Function

        Public Function AddColorCategory(prefix As IPrefix, categoryName As String) As Category Implements IAutoAssign.AddColorCategory
            Return CreateCategory(OlNS:=Globals.ThisAddIn.OlNS,
                                  prefix:=prefix,
                                  newCatName:=categoryName)
        End Function
    End Class

End Class
