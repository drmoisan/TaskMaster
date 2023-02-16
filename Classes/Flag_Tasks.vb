Imports Microsoft.VisualBasic
Imports Microsoft.Office.Interop.Outlook

Public Class Flag_Tasks
    Private _ToDoSelection As List(Of ToDoItem)
    Private _OlExplorer As Explorer = Globals.ThisAddIn.Application.ActiveExplorer()
    Private _ItemCollection As Collection
    Private WithEvents _viewer As TaskViewer
    Private _controller As TaskController



    Public Sub New(Optional ItemCollection As Collection = Nothing,
                   Optional blFile As Boolean = True,
                   Optional hWndCaller As IntPtr = Nothing,
                   Optional strNameOfFunctionCalling As String = "")

        _ToDoSelection = InitializeToDoList(ItemCollection)
        _viewer = New TaskViewer()
        _controller = New TaskController(_viewer, _ToDoSelection)
    End Sub

    Public Sub Run()
        _controller.LoadValues()
        _viewer.Show()
    End Sub




    Private Function InitializeToDoList(ItemCollection As Collection) As List(Of ToDoItem)
        If ItemCollection Is Nothing Then ItemCollection = GetSelection()
        Dim ToDoSelection As List(Of ToDoItem) = New List(Of ToDoItem)()
        For Each ObjItem In ItemCollection
            Dim tmpToDo As ToDoItem = New ToDoItem(Item:=ObjItem, OnDemand:=True)
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
        For Each obj In _OlExplorer.Selection
            ItemCollection.Add(obj)
        Next obj
        Return ItemCollection
    End Function
End Class
