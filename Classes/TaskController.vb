Imports System.Runtime.Remoting.Contexts
Imports Microsoft.Office.Interop.Outlook
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Diagnostics
Imports UtilitiesVB
Imports ToDoModel
Imports Tags


Public Class TaskController

    Declare Auto Function PostMessage Lib "user32.dll" (
        ByVal hWnd As IntPtr,
        ByVal msg As Int32,
        ByVal wParam As Int32,
        ByVal lParam As Int32) As Boolean

    Const WM_LBUTTONDOWN As Int32 = &H201

    Private WithEvents _viewer As TaskViewer
    Private ReadOnly _todo_list As List(Of ToDoItem)
    Private ReadOnly _active As ToDoItem
    Private _options As FlagsToSet
    Private ReadOnly _dict_categories As SortedDictionary(Of String, Boolean)
    Private _exit_type As String = "Cancel"
    Private _xlCtrlCaptions As Dictionary(Of Label, String)
    Private _xlCtrlLookup As Dictionary(Of Label, Control)
    Private _xlCtrlOptions As Dictionary(Of Label, Boolean)
    Private _xlCtrlsActive As Dictionary(Of Label, Char)
    Private _altActive As Boolean = False
    Private _altLevel As Integer = 0
    Private _keyCapture As String = ""
    Private _defaults As ToDoDefaults
    Private _autoAssign As IAutoAssign

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

    Private Enum ForceState
        none = 0
        force_on = 1
        force_off = 2
    End Enum

#Region "Public Lifecycle Functions"

    ''' <summary>
    ''' Constructor initializes the controller for the TaskViewer
    ''' </summary>
    ''' <param name="form_instance">Instance of TaskViewer</param>
    ''' <param name="ToDoSelection">List of ToDoItems</param>
    ''' <param name="flag_options">Enumeration of fields to activate</param>
    Public Sub New(form_instance As TaskViewer,
                   ToDoSelection As List(Of ToDoItem),
                   Defaults As ToDoDefaults,
                   AutoAssign As IAutoAssign,
                   Optional flag_options As FlagsToSet = FlagsToSet.all)

        'Save parameters to internal variables
        _viewer = form_instance
        _todo_list = ToDoSelection
        _options = flag_options
        _defaults = Defaults
        _autoAssign = AutoAssign

        'Activate this controller within the viewer
        With form_instance
            .SetController(Me)
            .AcceptButton = .OK_Button
            .CancelButton = .Cancel_Button
        End With


        'First ToDoItem in list is cloned to _active and set to readonly
        _active = _todo_list(0).Clone()
        _active.IsReadOnly = True

        'All color categories in Outlook.Namespace are loaded to a sorted dictionary
        _dict_categories = New SortedDictionary(Of String, Boolean)
        For Each cat As Category In Globals.ThisAddIn.OlNS.Categories
            _dict_categories.Add(cat.Name, False)
        Next

        _xlCtrlLookup = CreateControlLookup()
        _xlCtrlOptions = CreateOptionsLookup()
        _xlCtrlCaptions = CreateCaptionLookup()

    End Sub

    ''' <summary>
    ''' Function prepares task viewer by activating desired controls and loading values to them
    ''' </summary>
    Public Sub LoadInitialValues()

        'Load values into viewer by field
        _viewer.task_name.Text = _active.TaskSubject
        If _active.Context <> "" Then _viewer.category_selection.Text = _active.Context
        If _active.People <> "" Then _viewer.people_selection.Text = _active.People
        If _active.Project <> "" Then _viewer.project_selection.Text = _active.Project
        If _active.Topic <> "" Then _viewer.topic_selection.Text = _active.Topic

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

        If _active.TotalWork = 0 Then _active.TotalWork = My.Settings.Default_Task_Length
        _viewer.duration.Text = CStr(_active.TotalWork)

        If _active.ReminderTime <> DateValue("1/1/4501") Then
            _viewer.dt_reminder.Value = _active.ReminderTime
            _viewer.dt_reminder.Checked = True
        End If
        If _active.DueDate <> DateValue("1/1/4501") Then
            _viewer.dt_duedate.Value = _active.DueDate
            _viewer.dt_duedate.Checked = True
        End If

        'Deactivate controls that are not set in _options
        If _options <> FlagsToSet.all Then ActivateOptions()

        'Deactivate accelerator controls
        ToggleXl((From x In _xlCtrlLookup
                  Where _xlCtrlOptions(x.Key)
                  Select x).ToDictionary(
                  Function(x) x.Key, Function(x) "A"c),
                  ForceState.force_off)

    End Sub

    ''' <summary>
    ''' Sets options for which controls / fields to activate using FlagsToSet enumeration
    ''' </summary>
    ''' <returns></returns>
    Public Property Options As FlagsToSet
        Get
            Return _options
        End Get
        Set(value As FlagsToSet)
            _options = value
            ActivateOptions()
        End Set
    End Property

    ''' <summary>
    ''' Method determines if any category has been selected and copies the flags from the 
    ''' sample _active item to all members of _todo_list based on flags set in _options
    ''' </summary>
    Public Sub OK_Action()
        If AnyCategorySelected() Then

            ' Capture the value of the task subject and if not empty write to ToDoItem
            If _options.HasFlag(FlagsToSet.taskname) Then
                If _viewer.task_name.Text <> "" Then _active.TaskSubject = _viewer.task_name.Text
            End If

            ' Capture the worktime, validate and write to ToDoItem
            CaptureDuration()

            _viewer.Hide()

            ' Apply values captured in _active to each member of _todo_list for flags in _options
            ApplyChanges()

            _viewer.Dispose()
        End If
    End Sub

    ''' <summary>
    ''' Handles cancel button click. Sets the controller exit type to 
    ''' "Cancel" and disposes of the viewer
    ''' </summary>
    Public Sub Cancel_Action()
        _viewer.Hide()
        _exit_type = "Cancel"
        _viewer.Dispose()
    End Sub

#End Region

#Region "Public Mouse Events"

    ''' <summary>
    ''' Loads a TagViewer with categories relevant to People for assigment
    ''' </summary>
    Public Sub Assign_People()
        Dim prefix = _defaults.PrefixList.Find(Function(x) x.Key = "People")

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix.Value)
                             Select x).ToSortedDictionary()

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.People.Split(","), Function(x) x.Trim()).ToList()
        selections.Remove("")

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(viewer_instance:=viewer,
                                                                dictOptions:=filtered_cats,
                                                                autoAssigner:=_autoAssign,
                                                                prefixes:=_defaults.PrefixList,
                                                                selections:=selections,
                                                                prefix_key:=prefix.Key,
                                                                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.People = controller.SelectionString()
                _viewer.people_selection.Text = _active.People(False)
            End If
        End Using


    End Sub

    ''' <summary>
    ''' Loads a TagViewer with categories relevant to Context for assigment
    ''' </summary>
    Public Sub Assign_Context()
        Dim prefix = _defaults.PrefixList.Find(Function(x) x.Key = "Context")

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix.Value)
                             Select x).ToSortedDictionary()

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.Context.Split(","), Function(x) x.Trim()).ToList()
        selections.Remove("")

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(viewer_instance:=viewer,
                                                                dictOptions:=filtered_cats,
                                                                autoAssigner:=_autoAssign,
                                                                prefixes:=_defaults.PrefixList,
                                                                selections:=selections,
                                                                prefix_key:=prefix.Key,
                                                                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.Context = controller.SelectionString()
                _viewer.category_selection.Text = _active.Context(False)
            End If
        End Using

    End Sub

    Public Sub Assign_Project()
        Dim prefix = _defaults.PrefixList.Find(Function(x) x.Key = "Project")

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix.Value)
                             Select x).ToSortedDictionary()

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.Project.Split(","), Function(x) x.Trim()).ToList()
        selections.Remove("")

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(viewer_instance:=viewer,
                                                                dictOptions:=filtered_cats,
                                                                autoAssigner:=_autoAssign,
                                                                prefixes:=_defaults.PrefixList,
                                                                selections:=selections,
                                                                prefix_key:=prefix.Key,
                                                                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.Project = controller.SelectionString()
                _viewer.project_selection.Text = _active.Project(False)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Loads a TagViewer with categories relevant to Topics for assigment
    ''' </summary>
    Public Sub Assign_Topic()
        Dim prefix = _defaults.PrefixList.Find(Function(x) x.Key = "Topic")

        Dim filtered_cats = (From x In _dict_categories
                             Where x.Key.Contains(prefix.Value)
                             Select x).ToSortedDictionary()

        Dim selections As List(Of String) = Array.ConvertAll(
            _active.Topic.Split(","), Function(x) x.Trim()).ToList()
        selections.Remove("")

        Using viewer As TagViewer = New TagViewer
            Dim controller As TagController = New TagController(viewer_instance:=viewer,
                                                                dictOptions:=filtered_cats,
                                                                autoAssigner:=_autoAssign,
                                                                prefixes:=_defaults.PrefixList,
                                                                selections:=selections,
                                                                prefix_key:=prefix.Key,
                                                                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.Topic = controller.SelectionString()
                _viewer.topic_selection.Text = _active.Topic(False)
            End If
        End Using
    End Sub

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
    Public Sub Assign_KB()
        _active.KB = _viewer.kb_selector.SelectedItem.ToString()
    End Sub

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
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

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
    Public Sub Today_Change()
        _active.Today = _viewer.cbx_today.Checked
    End Sub

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
    Public Sub Bullpin_Change()
        _active.Bullpin = _viewer.cbx_bullpin.Checked
    End Sub

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
    Public Sub FlagAsTask_Change()
        _active.FlagAsTask = _viewer.cbxFlag.Checked
    End Sub

    Public Sub MouseFilter_FormClicked(sender As Object, e As EventArgs)
        If _altActive Then
            _altActive = False
            ToggleXl(_xlCtrlsActive, ForceState.force_off)
        End If
    End Sub
#End Region

#Region "Public Shortcuts"

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_Personal()
        _viewer.category_selection.Text = My.Settings.Prefix_Context & "Personal"
        _active.Context = My.Settings.Prefix_Context & "Personal"
        _viewer.project_selection.Text = My.Settings.Prefix_Project & "Personal - Other"
        _active.Project = My.Settings.Prefix_Project & "Personal - Other"
    End Sub

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_Meeting()
        SetFlag("Meeting", FlagsToSet.context)
    End Sub

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_Email()
        SetFlag("Email", FlagsToSet.context)
    End Sub

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_Calls()
        SetFlag("Calls", FlagsToSet.context)
    End Sub

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_PreRead()
        SetFlag("PreRead", FlagsToSet.context)
    End Sub

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_WaitingFor()
        SetFlag("Waiting For", FlagsToSet.context)
    End Sub

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_Unprocessed()
        SetFlag("Reading - .Unprocessed > 2 Minutes", FlagsToSet.context)
    End Sub

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_ReadingBusiness()
        SetFlag("Reading - Business", FlagsToSet.context)
    End Sub

    ''' <summary> Sets values to specific fields based on shortcut button </summary>
    Public Sub Shortcut_ReadingNews()
        SetFlag("Reading - News | Articles | Other", FlagsToSet.context)
        SetFlag("Routine - Reading", FlagsToSet.projects)
        SetFlag("READ: " & _viewer.task_name.Text, FlagsToSet.taskname)
        SetFlag("15", FlagsToSet.worktime)
        _viewer.duration.Focus()
    End Sub

#End Region

#Region "Public Keyboard Events and Properties"
    Public Function KeyboardHandler_KeyDown(sender As Object, e As KeyEventArgs) As Boolean

        If e.Alt Then
            Dim tup = RecurseXl(_xlCtrlsActive, _altActive, "", _altLevel)
            _xlCtrlsActive = tup.dictActive
            _altActive = tup.altActive
            _altLevel = tup.level
            Return True
        ElseIf _altActive Then
            If (e.KeyCode >= Keys.A And e.KeyCode <= Keys.Z) Then
                Dim tup = RecurseXl(_xlCtrlsActive,
                                    _altActive,
                                    Char.ToUpper(e.KeyCode.ToChar()),
                                    _altLevel)
                _xlCtrlsActive = tup.dictActive
                _altActive = tup.altActive
                _altLevel = tup.level
            End If

            Return True
        Else
            Return False
        End If

    End Function

    Public ReadOnly Property SuppressKeystrokes As Boolean
        Get
            Return _altActive
        End Get
    End Property
#End Region

#Region "Private Helper Properties and Functions"

    ''' <summary>
    ''' Property determines whether any category contains a value
    ''' </summary>
    ''' <returns>True if any value set in Context, People, Project or Topic. Else returns False</returns>
    Private ReadOnly Property AnyCategorySelected() As Boolean
        Get
            If _viewer.category_selection.Text <> "[Category Label]" Or
            _viewer.people_selection.Text <> "[Assigned People Flagged]" Or
            _viewer.project_selection.Text <> "[ Projects Flagged ]" Or
            _viewer.topic_selection.Text <> "[Other Topics Tagged]" Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    ''' <summary>
    ''' Activates or deactivates controls on _viewer based on _options set in class
    ''' </summary>
    Private Sub ActivateOptions()
        If _options.HasFlag(FlagsToSet.all) Then
            _viewer.Cat_Meeting.Enabled = True
            _viewer.Cat_Calls.Enabled = True
            _viewer.Cat_Personal.Enabled = True
            _viewer.Cat_Email.Enabled = True
            _viewer.Cat_Internet.Enabled = True
            _viewer.Cat_ReadingBusiness.Enabled = True
            _viewer.Cat_News.Enabled = True
            _viewer.Cat_Unprocessed.Enabled = True
            _viewer.Cat_WaitingFor.Enabled = True
        Else
            _viewer.Cat_Meeting.Enabled = False
            _viewer.Cat_Calls.Enabled = False
            _viewer.Cat_Personal.Enabled = False
            _viewer.Cat_Email.Enabled = False
            _viewer.Cat_Internet.Enabled = False
            _viewer.Cat_ReadingBusiness.Enabled = False
            _viewer.Cat_News.Enabled = False
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

    ''' <summary>
    ''' Sets value based on the flag type and value
    ''' </summary>
    ''' <param name="value">Comma separated list of tags</param>
    ''' <param name="flagType">Used to identify field names and tag prefix</param>
    Private Sub SetFlag(value As String, flagType As FlagsToSet)
        Select Case flagType
            Case FlagsToSet.context
                _active.Context = value
                _viewer.category_selection.Text = _active.Context(False)
            Case FlagsToSet.people
                _active.People = value
                _viewer.people_selection.Text = _active.People(False)
            Case FlagsToSet.projects
                _active.Project = value
                _viewer.project_selection.Text = _active.Project(False)
            Case FlagsToSet.topics
                _active.Topic = value
                _viewer.topic_selection.Text = _active.Topic(False)
            Case FlagsToSet.taskname
                _active.TaskSubject = value
                _viewer.task_name.Text = value
            Case FlagsToSet.worktime
                _viewer.duration.Text = value
                'Note that _active is set after OK click
        End Select

    End Sub

    ''' <summary>
    ''' Method grabs the work duration out of a text box, converts to an integer, 
    ''' and sets totalwork on the ToDoItem. 
    ''' </summary>
    ''' <exception cref="ArgumentOutOfRangeException">Duration must be >= 0 </exception>
    ''' <exception cref="InvalidCastException">Value must be an integer </exception>
    Private Sub CaptureDuration()
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
        End If
    End Sub

    ''' <summary>
    ''' Iterates through _todo_list and applies the values in _active for the fields in _options
    ''' </summary>
    Private Sub ApplyChanges()
        For Each c As ToDoItem In _todo_list
            c.FlagAsTask = True
            c.IsReadOnly = True

            If _options.HasFlag(FlagsToSet.context) Then c.Context = _active.Context
            If _options.HasFlag(FlagsToSet.people) Then c.People = _active.People
            If _options.HasFlag(FlagsToSet.projects) Then c.Project = _active.Project
            If _options.HasFlag(FlagsToSet.topics) Then c.Topic = _active.Topic
            If _options.HasFlag(FlagsToSet.today) Then c.Today = _active.Today
            If _options.HasFlag(FlagsToSet.bullpin) Then c.Bullpin = _active.Bullpin
            If _options.HasFlag(FlagsToSet.kbf) Then c.KB = _active.KB

            c.WriteFlagsBatch()
            c.IsReadOnly = False

            If _options.HasFlag(FlagsToSet.priority) Then c.Priority = _active.Priority
            If _options.HasFlag(FlagsToSet.taskname) Then c.TaskSubject = _active.TaskSubject
            If _options.HasFlag(FlagsToSet.worktime) Then c.TotalWork = _active.TotalWork
            If _options.HasFlag(FlagsToSet.duedate) Then c.DueDate = _active.DueDate
            If _options.HasFlag(FlagsToSet.reminder) Then c.ReminderTime = _active.ReminderTime
        Next
    End Sub

    Private Sub ToggleXl(dictLabels As Dictionary(Of Label, Char),
                         state As ForceState)
        Select Case state
            Case ForceState.none
                For Each row In dictLabels
                    row.Key.Visible = Not row.Key.Visible
                Next

            Case ForceState.force_on
                For Each row In dictLabels
                    row.Key.Visible = True
                Next

            Case ForceState.force_off
                For Each row In dictLabels
                    row.Key.Visible = False
                Next
        End Select

    End Sub

    Private Sub UpdateCaptions(dictLabels As Dictionary(Of Label, Char))
        For Each row In dictLabels
            row.Key.Text = row.Value
        Next
    End Sub

    Private Sub ExecuteXlAction(lbl As Label)
        Dim ctrl As Control = _xlCtrlLookup(lbl)
        If TypeOf (ctrl) Is Button Then
            Dim btn As Button = TryCast(ctrl, Button)
            btn.PerformClick()
        ElseIf TypeOf (ctrl) Is TextBox Then
            Dim txt As TextBox = TryCast(ctrl, TextBox)
            txt.Select()
            txt.SelectionStart = txt.Text.Length

        ElseIf TypeOf (ctrl) Is ComboBox Then
            Dim combo As ComboBox = ctrl
            combo.Select()
            combo.DroppedDown = True

        ElseIf TypeOf (ctrl) Is DateTimePicker Then
            Dim dt As DateTimePicker = ctrl

            Dim x As Int32 = dt.Width - 10
            Dim y As Int32 = dt.Height / 2
            Dim lParam As Int32 = x + y * &H10000
            PostMessage(dt.Handle, WM_LBUTTONDOWN, 1, lParam)

        ElseIf TypeOf (ctrl) Is Label Then

            If lbl.Equals(_viewer.xl_people) Then
                Assign_People()
            ElseIf lbl.Equals(_viewer.xl_project) Then
                Assign_Project()
            ElseIf lbl.Equals(_viewer.xl_topic) Then
                Assign_Topic()
            ElseIf lbl.Equals(_viewer.xl_context) Then
                Assign_Context()
            Else
                Throw New ArgumentException("lbl not assigned properly to control", NameOf(lbl))
            End If
        Else
            Throw New ArgumentException("lbl not assigned properly to control", NameOf(lbl))
        End If

    End Sub

    Private Function RecurseXl(dictSeed As Dictionary(Of Label, Char),
                               altActive As Boolean,
                               selectedChar As Char,
                               level As Integer) _
                               As (dictActive As Dictionary(Of Label, Char),
                               altActive As Boolean,
                               level As Integer)

        Dim dictDeactivate As Dictionary(Of Label, Char)
        Dim dictActivate As Dictionary(Of Label, Char)

        If Not altActive Then

            dictActivate = (From x In _xlCtrlCaptions
                            Where _xlCtrlOptions(x.Key)
                            Select x).ToDictionary(
                            Function(x) x.Key,
                            Function(x) Char.ToUpper(x.Value(0)))

            ToggleXl(dictActivate, ForceState.force_on)
            UpdateCaptions(dictActivate)

            Return (dictActive:=dictActivate, altActive:=True, level:=1)

        Else

            If dictSeed Is Nothing Then
                'Ensure that dictSeed is assigned. Alt key should not be
                'active if there is no seed value
                Throw New ArgumentNullException(NameOf(dictSeed))

            ElseIf selectedChar = vbNullChar Then
                'Null character is only passed if Alt key is pressed again.
                'In this case, we should deactivate the accelerator dialogue

                ToggleXl(dictSeed, ForceState.force_off)
                Return (dictActive:=Nothing, altActive:=False, level:=0)

            Else
                'Get accelerator labels that match the key stroke
                dictActivate = (From x In dictSeed
                                Where x.Value = selectedChar
                                Select x).ToDictionary(
                                Function(x) x.Key,
                                Function(x) Char.ToUpper(
                                _xlCtrlCaptions(x.Key)(level)))

                Select Case dictActivate.Count
                    Case 0
                        'If character doesn't match, ignore it
                        Return (dictActive:=dictSeed, altActive:=True, level:=0)

                    Case 1
                        'If only 1 element, we have found a match. 

                        'Turn off all remaining accelerator labels, including the match
                        ToggleXl(dictSeed, ForceState.force_off)

                        'Execute the designated action for the control
                        ExecuteXlAction(dictActivate.First().Key)

                        'Return values to reset the seed values
                        Return (dictActive:=Nothing, altActive:=False, level:=0)

                    Case Else
                        'If more than 1 element, we need to keep searching letters

                        'Get controls to deactivate
                        dictDeactivate = (From x In dictSeed
                                          Where x.Value <> selectedChar
                                          Select x).ToDictionary(
                                          Function(x) x.Key,
                                          Function(x) x.Value)
                        ToggleXl(dictDeactivate, ForceState.force_off)
                        UpdateCaptions(dictActivate)

                        'Return values to seed the next recursion
                        Return (dictActive:=dictActivate, altActive:=True, level:=level + 1)

                End Select

            End If

        End If

    End Function

    Private Function CreateOptionsLookup() As Dictionary(Of Label, Boolean)
        Dim xlCtrlOptions = New Dictionary(Of Label, Boolean)
        With _viewer
            xlCtrlOptions.Add(.xl_topic, _options.HasFlag(FlagsToSet.topics))
            xlCtrlOptions.Add(.xl_project, _options.HasFlag(FlagsToSet.projects))
            xlCtrlOptions.Add(.xl_people, _options.HasFlag(FlagsToSet.people))
            xlCtrlOptions.Add(.xl_context, _options.HasFlag(FlagsToSet.context))
            xlCtrlOptions.Add(.xl_taskname, _options.HasFlag(FlagsToSet.taskname))
            xlCtrlOptions.Add(.xl_importance, _options.HasFlag(FlagsToSet.priority))
            xlCtrlOptions.Add(.xl_kanban, _options.HasFlag(FlagsToSet.kbf))
            xlCtrlOptions.Add(.xl_worktime, _options.HasFlag(FlagsToSet.worktime))
            xlCtrlOptions.Add(.xl_ok, True)
            xlCtrlOptions.Add(.xl_cancel, True)
            xlCtrlOptions.Add(.xl_reminder, _options.HasFlag(FlagsToSet.reminder))
            xlCtrlOptions.Add(.xl_duedate, _options.HasFlag(FlagsToSet.duedate))
            xlCtrlOptions.Add(.xl_sc_waiting, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_unprocessed, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_news, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_email, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_readingbusiness, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_calls, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_internet, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_preread, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_meeting, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_personal, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_bullpin, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.xl_sc_today, _options.HasFlag(FlagsToSet.all))
        End With
        Return xlCtrlOptions
    End Function

    Private Function CreateCaptionLookup() As Dictionary(Of Label, String)
        Dim xlCtrlCaptions = New Dictionary(Of Label, String)
        With _viewer
            xlCtrlCaptions.Add(.xl_topic, .lbl_topic.Text)
            xlCtrlCaptions.Add(.xl_project, .lbl_project.Text)
            xlCtrlCaptions.Add(.xl_people, .lbl_people.Text)
            xlCtrlCaptions.Add(.xl_context, .lbl_context.Text)
            xlCtrlCaptions.Add(.xl_taskname, .lbl_taskname.Text)
            xlCtrlCaptions.Add(.xl_importance, .lbl_priority.Text)
            xlCtrlCaptions.Add(.xl_kanban, .lbl_kbf.Text)
            xlCtrlCaptions.Add(.xl_worktime, .lbl_duration.Text)
            xlCtrlCaptions.Add(.xl_ok, .OK_Button.Text)
            xlCtrlCaptions.Add(.xl_cancel, .Cancel_Button.Text)
            xlCtrlCaptions.Add(.xl_reminder, .lbl_reminder.Text)
            xlCtrlCaptions.Add(.xl_duedate, .lbl_duedate.Text)

            xlCtrlCaptions.Add(.xl_sc_waiting, .Cat_WaitingFor.Text)
            xlCtrlCaptions.Add(.xl_sc_unprocessed, .Cat_Unprocessed.Text)
            xlCtrlCaptions.Add(.xl_sc_news, .Cat_News.Text)
            xlCtrlCaptions.Add(.xl_sc_email, .Cat_Email.Text)
            xlCtrlCaptions.Add(.xl_sc_readingbusiness, .Cat_ReadingBusiness.Text)
            xlCtrlCaptions.Add(.xl_sc_calls, .Cat_Calls.Text)
            xlCtrlCaptions.Add(.xl_sc_internet, .Cat_Internet.Text)
            xlCtrlCaptions.Add(.xl_sc_preread, .Cat_PreRead.Text)
            xlCtrlCaptions.Add(.xl_sc_meeting, .Cat_Meeting.Text)
            xlCtrlCaptions.Add(.xl_sc_personal, .Cat_Personal.Text)
            xlCtrlCaptions.Add(.xl_sc_bullpin, .cbx_bullpin.Text)
            xlCtrlCaptions.Add(.xl_sc_today, .cbx_today.Text)
        End With
        Return xlCtrlCaptions
    End Function

    Private Function CreateControlLookup() As Dictionary(Of Label, Control)
        Dim xlCtrlLookup = New Dictionary(Of Label, Control)
        With _viewer
            xlCtrlLookup.Add(.xl_topic, .lbl_topic)
            xlCtrlLookup.Add(.xl_project, .lbl_project)
            xlCtrlLookup.Add(.xl_people, .lbl_people)
            xlCtrlLookup.Add(.xl_context, .lbl_context)
            xlCtrlLookup.Add(.xl_taskname, .task_name)
            xlCtrlLookup.Add(.xl_importance, .Priority_Box)
            xlCtrlLookup.Add(.xl_kanban, .kb_selector)
            xlCtrlLookup.Add(.xl_worktime, .duration)
            xlCtrlLookup.Add(.xl_ok, .OK_Button)
            xlCtrlLookup.Add(.xl_cancel, .Cancel_Button)
            xlCtrlLookup.Add(.xl_reminder, .dt_reminder)
            xlCtrlLookup.Add(.xl_duedate, .dt_duedate)

            xlCtrlLookup.Add(.xl_sc_waiting, .Cat_WaitingFor)
            xlCtrlLookup.Add(.xl_sc_unprocessed, .Cat_Unprocessed)
            xlCtrlLookup.Add(.xl_sc_news, .Cat_News)
            xlCtrlLookup.Add(.xl_sc_email, .Cat_Email)
            xlCtrlLookup.Add(.xl_sc_readingbusiness, .Cat_ReadingBusiness)
            xlCtrlLookup.Add(.xl_sc_calls, .Cat_Calls)
            xlCtrlLookup.Add(.xl_sc_internet, .Cat_Internet)
            xlCtrlLookup.Add(.xl_sc_preread, .Cat_PreRead)
            xlCtrlLookup.Add(.xl_sc_meeting, .Cat_Meeting)
            xlCtrlLookup.Add(.xl_sc_personal, .Cat_Personal)
            xlCtrlLookup.Add(.xl_sc_bullpin, .cbx_bullpin)
            xlCtrlLookup.Add(.xl_sc_today, .cbx_today)
        End With
        Return xlCtrlLookup
    End Function

#End Region

End Class
