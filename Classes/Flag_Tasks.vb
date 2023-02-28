Imports Microsoft.VisualBasic
Imports Microsoft.Office.Interop.Outlook

Public Class Flag_Tasks
    Private ReadOnly _todoSelection As List(Of ToDoItem)
    Private ReadOnly _olExplorer As Explorer = Globals.ThisAddIn.Application.ActiveExplorer()
    Private WithEvents _viewer As TaskViewer
    Private ReadOnly _controller As TaskController



    Public Sub New(Optional ItemCollection As Collection = Nothing,
                   Optional blFile As Boolean = True,
                   Optional hWndCaller As IntPtr = Nothing,
                   Optional strNameOfFunctionCalling As String = "")

        _todoSelection = InitializeToDoList(ItemCollection)
        _viewer = New TaskViewer()
        _controller = New TaskController(_viewer, _todoSelection)
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
End Class
