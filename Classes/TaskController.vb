Imports System.Runtime.Remoting.Contexts
Imports Microsoft.Office.Interop.Outlook
Imports System
Imports System.Linq
Imports System.Collections.Generic


Public Class TaskController

    Private _viewer As TaskViewer
    Private _todo_list As List(Of ToDoItem)
    Private _active As ToDoItem
    Private _options As FlagsToSet
    Private _dict_categories As SortedDictionary(Of String, Boolean)
    Private _exit_type As String

    <Flags>
    Public Enum FlagsToSet
        none = 0
        context = 1
        people = 2
        projects = 4
        topics = 8
        priority = 16
        taskname = 32
        worktime = 64
        today = 128
        bullpin = 256
        kbf = 512
        duedate = 1024
        reminder = 2048
        all = 4095
    End Enum

    Public Sub New(form_instance As TaskViewer,
                   ToDoSelection As List(Of ToDoItem),
                   Optional flag_options As FlagsToSet = FlagsToSet.all)

        'Setting callback controlling class to the current one
        form_instance.SetController(Me)

        'Initialize internal variables
        _viewer = form_instance
        _todo_list = ToDoSelection

        '_active is set to a readonly deep copy of the first list entry
        _active = _todo_list(0).Clone()
        _active.IsReadOnly = True

        _dict_categories = New SortedDictionary(Of String, Boolean)
        For Each cat As Category In Globals.ThisAddIn._OlNS.Categories
            _dict_categories.Add(cat.Name, False)
        Next

        'Set the default work duration if it is not set
        If _active.TotalWork = 0 Then _active.TotalWork = My.Settings.Default_Task_Length

        ' Bypass activation logic if it is all active
        If flag_options.HasFlag(FlagsToSet.all) Then
            _options = flag_options
        Else
            Options = flag_options
        End If

    End Sub


    ''' <summary>
    ''' Function loads the latest values stored in _active into the task viewer
    ''' </summary>
    Public Sub LoadValues()

        _viewer.task_name.Text = _active.TaskSubject

        If _active.TagContext <> "" Then _viewer.category_selection.Text = _active.TagContext
        If _active.TagPeople <> "" Then _viewer.people_selection.Text = _active.TagPeople
        If _active.TagProject <> "" Then _viewer.project_selection.Text = _active.TagProject
        If _active.TagTopic <> "" Then _viewer.topic_selection.Text = _active.TagTopic

        Select Case _active.Priority
            Case OlImportance.olImportanceHigh
                _viewer.Priority_Box.SelectedItem = "High"
            Case OlImportance.olImportanceLow
                _viewer.Priority_Box.SelectedItem = "Low"
            Case OlImportance.olImportanceNormal
                _viewer.Priority_Box.SelectedItem = "Normal"
        End Select

        If _active.KB = "" Then
            _viewer.kb_selector.SelectedItem = "Backlog"
        Else
            _viewer.kb_selector.SelectedItem = _active.KB
        End If

        _viewer.duration.Text = CStr(_active.TotalWork)

        If _active.ReminderTime <> DateValue("1/1/4501") Then
            _viewer.dt_reminder.Value = _active.ReminderTime
            _viewer.dt_reminder.Checked = True
        End If
        If _active.DueDate <> DateValue("1/1/4501") Then
            _viewer.dt_duedate.Value = _active.DueDate
            _viewer.dt_duedate.Checked = True
        End If

    End Sub

    Public Sub Assign_People()
        Dim prefix As String = "Tag PPL "

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix)
                             Select x).ToDictionary(
                             Function(x) x.Key,
                             Function(x) x.Value)
        Dim filtered_cats_sorted As SortedDictionary(Of String, Boolean) =
            New SortedDictionary(Of String, Boolean)(filtered_cats)

        'Dim filtered_categories2 = _dict_categories.Where(
        '    Function(x) x.Key.Contains(prefix)).[Select](Function(x) x)
        'Dim filtered = filtered_categories.ToDictionary(Function(x) x.Key, Function(x) x.Value)

        'Dim viewer As TagViewer = New TagViewer

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.TagPeople.Split(","), Function(x) x.Trim()).ToList()

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(
                viewer_instance:=viewer,
                dictOptions:=filtered_cats_sorted,
                selections:=selections,
                tag_prefix:=prefix,
                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.TagPeople = controller.SelectionString()
            End If
        End Using

        _viewer.people_selection.Text = _active.TagPeople
    End Sub

    Public Sub Cancel_Action()
        _viewer.Hide()
        _exit_type = "Cancel"
        _viewer.Dispose()
    End Sub

    Private Sub ActivateOptions()
        If _options.HasFlag(FlagsToSet.all) Then
            _viewer.Cat_Agenda.Enabled = True
            _viewer.Cat_Calls.Enabled = True
            _viewer.Cat_Deskwork.Enabled = True
            _viewer.Cat_Email.Enabled = True
            _viewer.Cat_Internet.Enabled = True
            _viewer.Cat_ReadingBusiness.Enabled = True
            _viewer.Cat_ReadingOther.Enabled = True
            _viewer.Cat_Unprocessed.Enabled = True
            _viewer.Cat_WaitingFor.Enabled = True
        Else
            _viewer.Cat_Agenda.Enabled = False
            _viewer.Cat_Calls.Enabled = False
            _viewer.Cat_Deskwork.Enabled = False
            _viewer.Cat_Email.Enabled = False
            _viewer.Cat_Internet.Enabled = False
            _viewer.Cat_ReadingBusiness.Enabled = False
            _viewer.Cat_ReadingOther.Enabled = False
            _viewer.Cat_Unprocessed.Enabled = False
            _viewer.Cat_WaitingFor.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.context) Then
            _viewer.category_selection.Enabled = True
            _viewer.lbl_context.Enabled = True
        Else
            _viewer.category_selection.Enabled = False
            _viewer.lbl_context.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.people) Then
            _viewer.people_selection.Enabled = True
            _viewer.lbl_people.Enabled = True
        Else
            _viewer.people_selection.Enabled = False
            _viewer.lbl_people.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.projects) Then
            _viewer.project_selection.Enabled = True
            _viewer.lbl_project.Enabled = True
        Else
            _viewer.project_selection.Enabled = False
            _viewer.lbl_project.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.topics) Then
            _viewer.topic_selection.Enabled = True
            _viewer.lbl_topic.Enabled = True
        Else
            _viewer.topic_selection.Enabled = False
            _viewer.lbl_topic.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.priority) Then
            _viewer.Priority_Box.Enabled = True
            _viewer.lbl_priority.Enabled = True
        Else
            _viewer.Priority_Box.Enabled = False
            _viewer.lbl_priority.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.taskname) Then
            _viewer.task_name.Enabled = True
            _viewer.lbl_taskname.Enabled = True
        Else
            _viewer.task_name.Enabled = False
            _viewer.lbl_taskname.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.worktime) Then
            _viewer.duration.Enabled = True
            _viewer.lbl_duration.Enabled = True
        Else
            _viewer.duration.Enabled = False
            _viewer.lbl_duration.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.today) Then
            _viewer.cbx_today.Enabled = True
        Else
            _viewer.cbx_today.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.bullpin) Then
            _viewer.cbx_bullpin.Enabled = True
        Else
            _viewer.cbx_bullpin.Enabled = False
        End If


        If _options.HasFlag(FlagsToSet.kbf) Then
            _viewer.kb_selector.Enabled = True
            _viewer.lbl_kbf.Enabled = True
        Else
            _viewer.kb_selector.Enabled = False
            _viewer.lbl_kbf.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.duedate) Then
            _viewer.dt_duedate.Enabled = True
            _viewer.lbl_duedate.Enabled = True
        Else
            _viewer.dt_duedate.Enabled = False
            _viewer.lbl_duedate.Enabled = False
        End If

        If _options.HasFlag(FlagsToSet.reminder) Then
            _viewer.dt_reminder.Enabled = True
            _viewer.lbl_reminder.Enabled = True
        Else
            _viewer.dt_reminder.Enabled = False
            _viewer.lbl_reminder.Enabled = False
        End If

    End Sub

    Public Property Options As FlagsToSet
        Get
            Return _options
        End Get
        Set(value As FlagsToSet)
            _options = value
            ActivateOptions()
        End Set
    End Property

End Class
