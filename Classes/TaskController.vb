Imports System.Runtime.Remoting.Contexts
Imports Microsoft.Office.Interop.Outlook
Imports System
Imports System.Linq
Imports System.Collections.Generic


Public Class TaskController

    Private ReadOnly _viewer As TaskViewer
    Private ReadOnly _todo_list As List(Of ToDoItem)
    Private ReadOnly _active As ToDoItem
    Private _options As FlagsToSet
    Private ReadOnly _dict_categories As SortedDictionary(Of String, Boolean)
    Private _exit_type As String = "Cancel"

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
        Dim prefix As String = My.Settings.Prefix_People

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix)
                             Select x).ToSortedDictionary()

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.TagPeople.Split(","), Function(x) x.Trim()).ToList()
        selections.Remove("")

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(
                viewer_instance:=viewer,
                dictOptions:=filtered_cats,
                selections:=selections,
                tag_prefix:=prefix,
                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.TagPeople = controller.SelectionString()
                _viewer.people_selection.Text = _active.TagPeople(False)
            End If
        End Using


    End Sub

    Public Sub Assign_Context()
        Dim prefix As String = My.Settings.Prefix_Context

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix)
                             Select x).ToSortedDictionary()

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.TagContext.Split(","), Function(x) x.Trim()).ToList()
        selections.Remove("")

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(
                viewer_instance:=viewer,
                dictOptions:=filtered_cats,
                selections:=selections,
                tag_prefix:=prefix,
                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.TagContext = controller.SelectionString()
                _viewer.category_selection.Text = _active.TagContext(False)
            End If
        End Using

    End Sub

    Public Sub Assign_Project()
        Dim prefix As String = My.Settings.Prefix_Project

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix)
                             Select x).ToSortedDictionary()

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.TagProject.Split(","), Function(x) x.Trim()).ToList()
        selections.Remove("")

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(
                viewer_instance:=viewer,
                dictOptions:=filtered_cats,
                selections:=selections,
                tag_prefix:=prefix,
                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.TagProject = controller.SelectionString()
                _viewer.category_selection.Text = _active.TagProject(False)
            End If
        End Using
    End Sub

    Public Sub Assign_Topic()
        Dim prefix As String = My.Settings.Prefix_Topic

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix)
                             Select x).ToSortedDictionary()

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.TagTopic.Split(","), Function(x) x.Trim()).ToList()
        selections.Remove("")

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(
                viewer_instance:=viewer,
                dictOptions:=filtered_cats,
                selections:=selections,
                tag_prefix:=prefix,
                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.TagTopic = controller.SelectionString()
                _viewer.topic_selection.Text = _active.TagTopic(False)
            End If
        End Using
    End Sub

    Public Sub Assign_KB()
        _active.KB = _viewer.kb_selector.SelectedItem.ToString()
    End Sub

    Public Sub Assign_Priority()
        Dim TmpStr As String = _viewer.Priority_Box.SelectedItem.ToString()
        If TmpStr = "High" Then
            _active.Priority = OlImportance.olImportanceHigh
        ElseIf TmpStr = "Low" Then
            _active.Priority = OlImportance.olImportanceLow
        Else
            _active.Priority = OlImportance.olImportanceNormal
        End If
    End Sub

    Public Sub OK_Action()
        If _viewer.category_selection.Text <> "[Category Label]" Or
            _viewer.people_selection.Text <> "[Assigned People Flagged]" Or
            _viewer.project_selection.Text <> "[ Projects Flagged ]" Or
            _viewer.topic_selection.Text <> "[Other Topics Tagged]" Then

            Dim duration As Integer
            Try
                duration = CInt(_viewer.duration.Text)
                If duration < 0 Then
                    Throw New ArgumentOutOfRangeException("Duration cannot be negative")
                End If
            Catch ex As InvalidCastException
                MsgBox("Could not convert to integer. Please put a positive integer in the duration box")
                duration = -1
            Catch ex As ArgumentOutOfRangeException
                MsgBox(ex.Message)
                duration = -1
            End Try

            If duration >= 0 Then
                _active.TotalWork = duration
                _viewer.Hide()
                ApplyChanges()
                _viewer.Dispose()
            End If

        End If
    End Sub

    Public Sub Cancel_Action()
        _viewer.Hide()
        _exit_type = "Cancel"
        _viewer.Dispose()
    End Sub

    Public Sub Today_Change()
        _active.Today = _viewer.cbx_today.Checked
    End Sub

    Public Sub Bullpin_Change()
        _active.Bullpin = _viewer.cbx_bullpin.Checked
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

    Public Sub FlagAsTask_Change()
        _active.FlagAsTask = _viewer.cbxFlag.Checked
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

#Region "Shortcuts"
    Public Sub Shortcut_Personal()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Personal"
        _active.TagContext = My.Settings.Prefix_Context & "Personal"
        _viewer.project_selection.Text = My.Settings.Prefix_Project & "Personal - Other"
        _active.TagProject = My.Settings.Prefix_Project & "Personal - Other"
    End Sub

    Public Sub Shortcut_Meeting()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Meeting"
        _active.TagContext = My.Settings.Prefix_Context & "Meeting"
    End Sub

    Public Sub Shortcut_Email()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Email"
        _active.TagContext = My.Settings.Prefix_Context & "Email"
    End Sub

    Public Sub Shortcut_Calls()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Calls"
        _active.TagContext = My.Settings.Prefix_Context & "Calls"
    End Sub

    Public Sub Shortcut_PreRead()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "PreRead"
        _active.TagContext = My.Settings.Prefix_Context & "PreRead"
    End Sub

    Public Sub Shortcut_WaitingFor()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Waiting For"
        _active.TagContext = My.Settings.Prefix_Context & "Waiting For"
    End Sub

    Public Sub Shortcut_Unprocessed()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Reading - .Unprocessed > 2 Minutes"
        _active.TagContext = My.Settings.Prefix_Context & "Reading - .Unprocessed > 2 Minutes"
    End Sub

    Public Sub Shortcut_ReadingBusiness()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Reading - Business"
        _active.TagContext = My.Settings.Prefix_Context & "Reading - Business"
    End Sub

    Public Sub Shortcut_ReadingNews()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Reading - News | Articles | Other"
        _active.TagContext = My.Settings.Prefix_Context & "Reading - News | Articles | Other"

        _viewer.project_selection.Text = My.Settings.Prefix_Project & "Routine - Reading"
        _active.TagProject = My.Settings.Prefix_Project & "Routine - Reading"

        _viewer.task_name.Text = "READ: " & _viewer.task_name.Text
        _active.TaskSubject = _viewer.task_name.Text

        _viewer.duration.Text = "15"
        _active.TotalWork = 15
        _viewer.duration.Focus()
    End Sub

#End Region

    Private Sub ApplyChanges()
        For Each todo As ToDoItem In _todo_list
            ApplyChangesToItem(todo)
        Next
    End Sub

    Private Sub ApplyChangesToItem(current As ToDoItem)
        current.FlagAsTask = True
        current.IsReadOnly = True

        If _options.HasFlag(FlagsToSet.context) Then
            current.TagContext = _active.TagContext
        End If

        If _options.HasFlag(FlagsToSet.people) Then
            current.TagPeople = _active.TagPeople
        End If

        If _options.HasFlag(FlagsToSet.projects) Then
            current.TagProject = _active.TagProject
        End If

        If _options.HasFlag(FlagsToSet.topics) Then
            current.TagTopic = _active.TagTopic
        End If

        If _options.HasFlag(FlagsToSet.today) Then
            current.Today = _active.Today
        End If


        If _options.HasFlag(FlagsToSet.bullpin) Then
            current.Bullpin = _active.Bullpin
        End If

        If _options.HasFlag(FlagsToSet.kbf) Then
            current.KB = _active.KB
        End If

        current.WriteFlagsBatch()
        current.IsReadOnly = False

        If _options.HasFlag(FlagsToSet.priority) Then
            current.Priority = _active.Priority
        End If

        If _options.HasFlag(FlagsToSet.taskname) Then
            current.TaskSubject = _active.TaskSubject
        End If

        If _options.HasFlag(FlagsToSet.worktime) Then
            current.TotalWork = _active.TotalWork
        End If

        If _options.HasFlag(FlagsToSet.duedate) Then
            current.DueDate = _active.DueDate
        End If

        If _options.HasFlag(FlagsToSet.reminder) Then
            current.ReminderTime = _active.ReminderTime
        End If
    End Sub

End Class
