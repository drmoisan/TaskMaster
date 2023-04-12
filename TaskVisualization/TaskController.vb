Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook
Imports Tags
Imports ToDoModel
Imports UtilitiesVB


Public Class TaskController

    Declare Auto Function PostMessage Lib "user32.dll" (
        ByVal hWnd As IntPtr,
        ByVal msg As Integer,
        ByVal wParam As Integer,
        ByVal lParam As Integer) As Boolean

    Public Const WM_LBUTTONDOWN As Integer = &H201

    Private WithEvents _viewer As TaskViewer
    Private ReadOnly _todo_list As List(Of ToDoItem)
    Private ReadOnly _active As ToDoItem
    Private _options As FlagsToSet
    Private ReadOnly _dict_categories As SortedDictionary(Of String, Boolean)
    Private _exit_type As String = "Cancel"
    Private ReadOnly _xlCtrlCaptions As Dictionary(Of Label, String)
    Private ReadOnly _xlCtrlLookup As Dictionary(Of Label, Control)
    Private ReadOnly _xlCtrlOptions As Dictionary(Of Label, Boolean)
    Private _xlCtrlsActive As Dictionary(Of Label, Char)
    Private _altActive As Boolean = False
    Private _altLevel As Integer = 0
    Private ReadOnly _keyCapture As String = ""
    Private ReadOnly _defaults As ToDoDefaults
    Private ReadOnly _autoAssign As IAutoAssign

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
    ''' <param name="FormInstance">Instance of TaskViewer</param>
    ''' <param name="ToDoSelection">List of ToDoItems</param>
    ''' <param name="FlagOptions">Enumeration of fields to activate</param>
    Public Sub New(FormInstance As TaskViewer,
                   OlCategories As Categories,
                   ToDoSelection As List(Of ToDoItem),
                   Defaults As ToDoDefaults,
                   AutoAssign As IAutoAssign,
                   Optional FlagOptions As FlagsToSet = FlagsToSet.all)

        'Save parameters to internal variables
        _viewer = FormInstance
        _todo_list = ToDoSelection
        _options = FlagOptions
        _defaults = Defaults
        _autoAssign = AutoAssign

        'Activate this controller within the viewer
        With FormInstance
            .SetController(Me)
            .AcceptButton = .OKButton
            .CancelButton = .Cancel_Button
        End With


        'First ToDoItem in list is cloned to _active and set to readonly
        _active = _todo_list(0).Clone()
        _active.IsReadOnly = True

        'All color categories in Outlook.Namespace are loaded to a sorted dictionary
        _dict_categories = New SortedDictionary(Of String, Boolean)
        For Each cat As Category In OlCategories
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
        '_viewer.Show()
        'LoadFromFile values into viewer by field
        _viewer.TaskName.Text = _active.TaskSubject
        If _active.Context <> "" Then _viewer.CategorySelection.Text = _active.Context
        If _active.People <> "" Then _viewer.PeopleSelection.Text = _active.People
        If _active.Project <> "" Then _viewer.ProjectSelection.Text = _active.Project
        If _active.Topic <> "" Then _viewer.TopicSelection.Text = _active.Topic

        Select Case _active.Priority
            Case OlImportance.olImportanceHigh
                _viewer.PriorityBox.SelectedItem = "High"
            Case OlImportance.olImportanceLow
                _viewer.PriorityBox.SelectedItem = "Low"
            Case OlImportance.olImportanceNormal
                _viewer.PriorityBox.SelectedItem = "Normal"
        End Select

        _viewer.KbSelector.SelectedItem = If(_active.KB = "", "Backlog", _active.KB)

        If _active.TotalWork = 0 Then _active.TotalWork = _defaults.DefaultTaskLength
        _viewer.duration.Text = CStr(_active.TotalWork)

        If _active.ReminderTime <> DateValue("1/1/4501") Then
            _viewer.DtReminder.Value = _active.ReminderTime
            _viewer.DtReminder.Checked = True
        End If
        If _active.DueDate <> DateValue("1/1/4501") Then
            _viewer.DtDuedate.Value = _active.DueDate
            _viewer.DtDuedate.Checked = True
        End If

        'Deactivate accelerator controls
        ToggleXl((From x In _xlCtrlLookup
                  Select x).ToDictionary(
                  Function(x) x.Key, Function(x) "A"c),
                  ForceState.force_off)

        'Deactivate controls that are not set in _options
        If _options <> FlagsToSet.all Then ActivateOptions()

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
                If _viewer.TaskName.Text <> "" Then _active.TaskSubject = _viewer.TaskName.Text
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

        Using viewer As New TagViewer
            Dim controller As New TagController(viewer_instance:=viewer,
                                                                dictOptions:=filtered_cats,
                                                                autoAssigner:=_autoAssign,
                                                                prefixes:=_defaults.PrefixList,
                                                                selections:=selections,
                                                                prefix_key:=prefix.Key,
                                                                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.People = controller.SelectionString()
                _viewer.PeopleSelection.Text = _active.People(False)
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
        Dim unused1 = selections.Remove("")

        Using viewer As New TagViewer
            Dim controller As New TagController(viewer_instance:=viewer,
                                                dictOptions:=filtered_cats,
                                                autoAssigner:=_autoAssign,
                                                prefixes:=_defaults.PrefixList,
                                                selections:=selections,
                                                prefix_key:=prefix.Key,
                                                objItemObject:=_active.object_item)
            viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.Context = controller.SelectionString()
                _viewer.CategorySelection.Text = _active.Context(False)
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
        Dim unused1 = selections.Remove("")

        Using viewer As New TagViewer
            Dim controller As New TagController(viewer_instance:=viewer,
                                                                dictOptions:=filtered_cats,
                                                                autoAssigner:=_autoAssign,
                                                                prefixes:=_defaults.PrefixList,
                                                                selections:=selections,
                                                                prefix_key:=prefix.Key,
                                                                objItemObject:=_active.object_item)
            Dim unused = viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.Project = controller.SelectionString()
                _viewer.ProjectSelection.Text = _active.Project(False)
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
        Dim unused1 = selections.Remove("")

        Using viewer As New TagViewer
            Dim controller As New TagController(viewer_instance:=viewer,
                                                                dictOptions:=filtered_cats,
                                                                autoAssigner:=_autoAssign,
                                                                prefixes:=_defaults.PrefixList,
                                                                selections:=selections,
                                                                prefix_key:=prefix.Key,
                                                                objItemObject:=_active.object_item)
            Dim unused = viewer.ShowDialog()
            If controller._exit_type <> "Cancel" Then
                _active.Topic = controller.SelectionString()
                _viewer.TopicSelection.Text = _active.Topic(False)
            End If
        End Using
    End Sub

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
    Public Sub Assign_KB()
        _active.KB = _viewer.KbSelector.SelectedItem.ToString()
    End Sub

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
    Public Sub Assign_Priority()
        Dim TmpStr As String = _viewer.PriorityBox.SelectedItem.ToString()
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
        _active.Today = _viewer.CbxToday.Checked
    End Sub

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
    Public Sub Bullpin_Change()
        _active.Bullpin = _viewer.CbxBullpin.Checked
    End Sub

    ''' <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
    Public Sub FlagAsTask_Change()
        _active.FlagAsTask = _viewer.CbxFlagAsTask.Checked
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
        Dim prefix = _defaults.PrefixList.Find(Function(x) x.Key = "Context")
        _viewer.CategorySelection.Text = prefix.Value & "Personal"
        _active.Context = prefix.Value & "Personal"

        prefix = _defaults.PrefixList.Find(Function(x) x.Key = "Project")
        _viewer.ProjectSelection.Text = prefix.Value & "Personal - Other"
        _active.Project = prefix.Value & "Personal - Other"
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
        SetFlag("READ: " & _viewer.TaskName.Text, FlagsToSet.taskname)
        SetFlag("15", FlagsToSet.worktime)
        Dim unused = _viewer.Duration.Focus()
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
            If e.KeyCode >= Keys.A And e.KeyCode <= Keys.Z Then
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
            Return _viewer.CategorySelection.Text <> "[Category Label]" Or
            _viewer.PeopleSelection.Text <> "[Assigned People Flagged]" Or
            _viewer.ProjectSelection.Text <> "[ Projects Flagged ]" Or
            _viewer.TopicSelection.Text <> "[Other Topics Tagged]"
        End Get
    End Property

    ''' <summary>
    ''' Activates or deactivates controls on _viewer based on _options set in class
    ''' </summary>
    Private Sub ActivateOptions()
        If _options.HasFlag(FlagsToSet.all) Then
            _viewer.ShortcutMeeting.Enabled = True
            _viewer.ShortcutCalls.Enabled = True
            _viewer.ShortcutPersonal.Enabled = True
            _viewer.ShortcutEmail.Enabled = True
            _viewer.ShortcutInternet.Enabled = True
            _viewer.ShortcutReadingBusiness.Enabled = True
            _viewer.ShortcutNews.Enabled = True
            _viewer.ShortcutUnprocessed.Enabled = True
            _viewer.ShortcutWaitingFor.Enabled = True
        Else
            _viewer.ShortcutMeeting.Enabled = False
            _viewer.ShortcutCalls.Enabled = False
            _viewer.ShortcutPersonal.Enabled = False
            _viewer.ShortcutEmail.Enabled = False
            _viewer.ShortcutInternet.Enabled = False
            _viewer.ShortcutReadingBusiness.Enabled = False
            _viewer.ShortcutNews.Enabled = False
            _viewer.ShortcutUnprocessed.Enabled = False
            _viewer.ShortcutWaitingFor.Enabled = False
            _viewer.ShortcutPreRead.Enabled = False

            _viewer.ShortcutMeeting.Visible = False
            _viewer.ShortcutCalls.Visible = False
            _viewer.ShortcutPersonal.Visible = False
            _viewer.ShortcutEmail.Visible = False
            _viewer.ShortcutInternet.Visible = False
            _viewer.ShortcutReadingBusiness.Visible = False
            _viewer.ShortcutNews.Visible = False
            _viewer.ShortcutUnprocessed.Visible = False
            _viewer.ShortcutWaitingFor.Visible = False
            _viewer.ShortcutPreRead.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.context) Then
            _viewer.CategorySelection.Enabled = True
            _viewer.LblContext.Enabled = True
        Else
            _viewer.CategorySelection.Enabled = False
            _viewer.LblContext.Enabled = False

            _viewer.CategorySelection.Visible = False
            _viewer.LblContext.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.people) Then
            _viewer.PeopleSelection.Enabled = True
            _viewer.LblPeople.Enabled = True
        Else
            _viewer.PeopleSelection.Enabled = False
            _viewer.LblPeople.Enabled = False

            _viewer.PeopleSelection.Visible = False
            _viewer.LblPeople.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.projects) Then
            _viewer.ProjectSelection.Enabled = True
            _viewer.LblProject.Enabled = True
        Else
            _viewer.ProjectSelection.Enabled = False
            _viewer.LblProject.Enabled = False

            _viewer.ProjectSelection.Visible = False
            _viewer.LblProject.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.topics) Then
            _viewer.TopicSelection.Enabled = True
            _viewer.LblTopic.Enabled = True
        Else
            _viewer.TopicSelection.Enabled = False
            _viewer.LblTopic.Enabled = False

            _viewer.TopicSelection.Visible = False
            _viewer.LblTopic.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.priority) Then
            _viewer.PriorityBox.Enabled = True
            _viewer.LblPriority.Enabled = True
        Else
            _viewer.PriorityBox.Enabled = False
            _viewer.LblPriority.Enabled = False

            _viewer.PriorityBox.Visible = False
            _viewer.LblPriority.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.taskname) Then
            _viewer.TaskName.Enabled = True
            _viewer.LblTaskname.Enabled = True
        Else
            _viewer.TaskName.Enabled = False
            _viewer.LblTaskname.Enabled = False

            _viewer.TaskName.Visible = False
            _viewer.LblTaskname.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.worktime) Then
            _viewer.Duration.Enabled = True
            _viewer.LblDuration.Enabled = True
        Else
            _viewer.Duration.Enabled = False
            _viewer.LblDuration.Enabled = False

            _viewer.Duration.Visible = False
            _viewer.LblDuration.Visible = False
        End If

        _viewer.CbxToday.Enabled = _options.HasFlag(FlagsToSet.today)
        _viewer.CbxToday.Visible = _options.HasFlag(FlagsToSet.today)

        _viewer.CbxBullpin.Enabled = _options.HasFlag(FlagsToSet.bullpin)
        _viewer.CbxBullpin.Visible = _options.HasFlag(FlagsToSet.bullpin)


        If _options.HasFlag(FlagsToSet.kbf) Then
            _viewer.KbSelector.Enabled = True
            _viewer.LblKbf.Enabled = True
        Else
            _viewer.KbSelector.Enabled = False
            _viewer.LblKbf.Enabled = False

            _viewer.KbSelector.Visible = False
            _viewer.LblKbf.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.duedate) Then
            _viewer.DtDuedate.Enabled = True
            _viewer.LblDuedate.Enabled = True
        Else
            _viewer.DtDuedate.Enabled = False
            _viewer.LblDuedate.Enabled = False

            _viewer.DtDuedate.Visible = False
            _viewer.LblDuedate.Visible = False
        End If

        If _options.HasFlag(FlagsToSet.reminder) Then
            _viewer.DtReminder.Enabled = True
            _viewer.LblReminder.Enabled = True
        Else
            _viewer.DtReminder.Enabled = False
            _viewer.LblReminder.Enabled = False

            _viewer.DtReminder.Visible = False
            _viewer.LblReminder.Visible = False
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
                _viewer.CategorySelection.Text = _active.Context(False)
            Case FlagsToSet.people
                _active.People = value
                _viewer.PeopleSelection.Text = _active.People(False)
            Case FlagsToSet.projects
                _active.Project = value
                _viewer.ProjectSelection.Text = _active.Project(False)
            Case FlagsToSet.topics
                _active.Topic = value
                _viewer.TopicSelection.Text = _active.Topic(False)
            Case FlagsToSet.taskname
                _active.TaskSubject = value
                _viewer.TaskName.Text = value
            Case FlagsToSet.worktime
                _viewer.Duration.Text = value
                'Note that _active is set after OK click
        End Select

    End Sub

    ''' <summary>
    ''' Method grabs the work Duration out of a text box, converts to an integer, 
    ''' and sets totalwork on the ToDoItem. 
    ''' </summary>
    ''' <exception cref="ArgumentOutOfRangeException">Duration must be >= 0 </exception>
    ''' <exception cref="InvalidCastException">Value must be an integer </exception>
    Private Sub CaptureDuration()
        Dim duration As Integer
        Try
            duration = CInt(_viewer.Duration.Text)
            If duration < 0 Then
                Throw New ArgumentOutOfRangeException("Duration cannot be negative")
            End If
        Catch ex As InvalidCastException
            Dim unused1 = MsgBox("Could not convert to integer. Please put a positive integer in the duration box")
            duration = -1
        Catch ex As ArgumentOutOfRangeException
            Dim unused = MsgBox(ex.Message)
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
        If TypeOf ctrl Is Button Then
            Dim btn As Button = TryCast(ctrl, Button)
            btn.PerformClick()
        ElseIf TypeOf ctrl Is TextBox Then
            Dim txt As TextBox = TryCast(ctrl, TextBox)
            txt.Select()
            txt.SelectionStart = txt.Text.Length

        ElseIf TypeOf ctrl Is ComboBox Then
            Dim combo As ComboBox = ctrl
            combo.Select()
            combo.DroppedDown = True

        ElseIf TypeOf ctrl Is DateTimePicker Then
            Dim dt As DateTimePicker = ctrl

            Dim x As Integer = dt.Width - 10
            Dim y As Integer = dt.Height / 2
            Dim lParam As Integer = x + (y * &H10000)
            Dim unused = PostMessage(dt.Handle, WM_LBUTTONDOWN, 1, lParam)

        ElseIf TypeOf ctrl Is Label Then

            If lbl.Equals(_viewer.XlPeople) Then
                Assign_People()
            ElseIf lbl.Equals(_viewer.XlProject) Then
                Assign_Project()
            ElseIf lbl.Equals(_viewer.XlTopic) Then
                Assign_Topic()
            ElseIf lbl.Equals(_viewer.XlContext) Then
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
                'Empty character is only passed if Alt key is pressed again.
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
            xlCtrlOptions.Add(.XlTopic, _options.HasFlag(FlagsToSet.topics))
            xlCtrlOptions.Add(.XlProject, _options.HasFlag(FlagsToSet.projects))
            xlCtrlOptions.Add(.XlPeople, _options.HasFlag(FlagsToSet.people))
            xlCtrlOptions.Add(.XlContext, _options.HasFlag(FlagsToSet.context))
            xlCtrlOptions.Add(.XlTaskname, _options.HasFlag(FlagsToSet.taskname))
            xlCtrlOptions.Add(.XlImportance, _options.HasFlag(FlagsToSet.priority))
            xlCtrlOptions.Add(.XlKanban, _options.HasFlag(FlagsToSet.kbf))
            xlCtrlOptions.Add(.XlWorktime, _options.HasFlag(FlagsToSet.worktime))
            xlCtrlOptions.Add(.XlOk, True)
            xlCtrlOptions.Add(.XlCancel, True)
            xlCtrlOptions.Add(.XlReminder, _options.HasFlag(FlagsToSet.reminder))
            xlCtrlOptions.Add(.XlDuedate, _options.HasFlag(FlagsToSet.duedate))
            xlCtrlOptions.Add(.XlScWaiting, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScUnprocessed, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScNews, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScEmail, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScReadingbusiness, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScCalls, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScInternet, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScPreread, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScMeeting, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScPersonal, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScBullpin, _options.HasFlag(FlagsToSet.all))
            xlCtrlOptions.Add(.XlScToday, _options.HasFlag(FlagsToSet.all))
        End With
        Return xlCtrlOptions
    End Function

    Private Function CreateCaptionLookup() As Dictionary(Of Label, String)
        Dim xlCtrlCaptions = New Dictionary(Of Label, String)
        With _viewer
            xlCtrlCaptions.Add(.XlTopic, .LblTopic.Text)
            xlCtrlCaptions.Add(.XlProject, .LblProject.Text)
            xlCtrlCaptions.Add(.XlPeople, .LblPeople.Text)
            xlCtrlCaptions.Add(.XlContext, .LblContext.Text)
            xlCtrlCaptions.Add(.XlTaskname, .LblTaskname.Text)
            xlCtrlCaptions.Add(.XlImportance, .LblPriority.Text)
            xlCtrlCaptions.Add(.XlKanban, .LblKbf.Text)
            xlCtrlCaptions.Add(.XlWorktime, .LblDuration.Text)
            xlCtrlCaptions.Add(.XlOk, .OKButton.Text)
            xlCtrlCaptions.Add(.XlCancel, .Cancel_Button.Text)
            xlCtrlCaptions.Add(.XlReminder, .LblReminder.Text)
            xlCtrlCaptions.Add(.XlDuedate, .LblDuedate.Text)

            xlCtrlCaptions.Add(.XlScWaiting, .ShortcutWaitingFor.Text)
            xlCtrlCaptions.Add(.XlScUnprocessed, .ShortcutUnprocessed.Text)
            xlCtrlCaptions.Add(.XlScNews, .ShortcutNews.Text)
            xlCtrlCaptions.Add(.XlScEmail, .ShortcutEmail.Text)
            xlCtrlCaptions.Add(.XlScReadingbusiness, .ShortcutReadingBusiness.Text)
            xlCtrlCaptions.Add(.XlScCalls, .ShortcutCalls.Text)
            xlCtrlCaptions.Add(.XlScInternet, .ShortcutInternet.Text)
            xlCtrlCaptions.Add(.XlScPreread, .ShortcutPreRead.Text)
            xlCtrlCaptions.Add(.XlScMeeting, .ShortcutMeeting.Text)
            xlCtrlCaptions.Add(.XlScPersonal, .ShortcutPersonal.Text)
            xlCtrlCaptions.Add(.XlScBullpin, .CbxBullpin.Text)
            xlCtrlCaptions.Add(.XlScToday, .CbxToday.Text)
        End With
        Return xlCtrlCaptions
    End Function

    Private Function CreateControlLookup() As Dictionary(Of Label, Control)
        Dim xlCtrlLookup = New Dictionary(Of Label, Control)
        With _viewer
            xlCtrlLookup.Add(.XlTopic, .LblTopic)
            xlCtrlLookup.Add(.XlProject, .LblProject)
            xlCtrlLookup.Add(.XlPeople, .LblPeople)
            xlCtrlLookup.Add(.XlContext, .LblContext)
            xlCtrlLookup.Add(.XlTaskname, .TaskName)
            xlCtrlLookup.Add(.XlImportance, .PriorityBox)
            xlCtrlLookup.Add(.XlKanban, .KbSelector)
            xlCtrlLookup.Add(.XlWorktime, .Duration)
            xlCtrlLookup.Add(.XlOk, .OKButton)
            xlCtrlLookup.Add(.XlCancel, .Cancel_Button)
            xlCtrlLookup.Add(.XlReminder, .DtReminder)
            xlCtrlLookup.Add(.XlDuedate, .DtDuedate)

            xlCtrlLookup.Add(.XlScWaiting, .ShortcutWaitingFor)
            xlCtrlLookup.Add(.XlScUnprocessed, .ShortcutUnprocessed)
            xlCtrlLookup.Add(.XlScNews, .ShortcutNews)
            xlCtrlLookup.Add(.XlScEmail, .ShortcutEmail)
            xlCtrlLookup.Add(.XlScReadingbusiness, .ShortcutReadingBusiness)
            xlCtrlLookup.Add(.XlScCalls, .ShortcutCalls)
            xlCtrlLookup.Add(.XlScInternet, .ShortcutInternet)
            xlCtrlLookup.Add(.XlScPreread, .ShortcutPreRead)
            xlCtrlLookup.Add(.XlScMeeting, .ShortcutMeeting)
            xlCtrlLookup.Add(.XlScPersonal, .ShortcutPersonal)
            xlCtrlLookup.Add(.XlScBullpin, .CbxBullpin)
            xlCtrlLookup.Add(.XlScToday, .CbxToday)
        End With
        Return xlCtrlLookup
    End Function

#End Region

End Class
