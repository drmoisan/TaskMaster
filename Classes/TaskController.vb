Imports Microsoft.Office.Interop.Outlook

Public Class TaskController

    Private _viewer As TaskViewer
    Private _todo_list As List(Of ToDoItem)
    Private _active As ToDoItem

    Public Sub New(form_instance As TaskViewer, ToDoSelection As List(Of ToDoItem))
        _viewer = form_instance
        _todo_list = ToDoSelection
        _active = _todo_list(0)
        form_instance.SetController(Me)
        If _active.TotalWork = 0 Then _active.TotalWork = My.Settings.Default_Task_Length
    End Sub

    Public Sub LoadValues()

        _viewer.Task_Name.Text = _active.TaskSubject

        If _active.TagContext <> "" Then _viewer.Category_Selection.Text = _active.TagContext
        If _active.TagPeople <> "" Then _viewer.People_Selection.Text = _active.TagPeople
        If _active.TagProject <> "" Then _viewer.Project_Selection.Text = _active.TagProject
        If _active.TagTopic <> "" Then _viewer.Topic_Selection.Text = _active.TagTopic

        Select Case _active.Priority
            Case OlImportance.olImportanceHigh
                _viewer.Priority_Box.SelectedItem = "High"
            Case OlImportance.olImportanceLow
                _viewer.Priority_Box.SelectedItem = "Low"
            Case OlImportance.olImportanceNormal
                _viewer.Priority_Box.SelectedItem = "Normal"
        End Select

        Select Case _active.KB
            Case "Backlog"
                _viewer.KB_Selector.SelectedItem = "Backlog"
            Case "Planned"
                _viewer.KB_Selector.SelectedItem = "Planned"
            Case "InProgress"
                _viewer.KB_Selector.SelectedItem = "InProgress"
            Case "Completed"
                _viewer.KB_Selector.SelectedItem = "Completed"
            Case ""
                _viewer.KB_Selector.SelectedItem = "Backlog"
        End Select

        _viewer.Duration.Text = CStr(_active.TotalWork)

        If _active.ReminderTime <> DateValue("1/1/4501") Then
            _viewer.DT_Reminder.Value = _active.ReminderTime
            _viewer.DT_Reminder.Checked = True
        End If
        If _active.DueDate <> DateValue("1/1/4501") Then
            _viewer.DT_DueDate.Value = _active.DueDate
            _viewer.DT_DueDate.Checked = True
        End If

    End Sub

End Class
