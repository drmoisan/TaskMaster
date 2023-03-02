Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Numerics
Imports System.Collections
Imports System.IO
Imports Microsoft.Office.Interop.Outlook
Imports System.Collections.Generic
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports System.Linq.Expressions
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Net

<Serializable()>
Public Class ToDoItem
    Implements ICloneable

    Const PA_TOTAL_WORK As String =
            "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003"

    Private OlObject As Object
    Private _ToDoID As String = ""
    Public _TaskSubject As String = ""
    Public _MetaTaskSubject As String = ""
    Public _MetaTaskLvl As String = ""
    Private _TagProgram As String = ""
    Private _Priority As [OlImportance]
    Private _TaskCreateDate As Date
    Private _StartDate As Date
    Private _Complete As Boolean
    Private _TotalWork As Integer = 0
    Private _ActiveBranch As Boolean = False
    Private _ExpandChildren As String = ""
    Private _ExpandChildrenState As String = ""
    Private _EC2 As Boolean
    Private _VisibleTreeState As Integer
    Private _readonly As Boolean = False
    Private _flags As FlagParser
    Private _flagAsTask As Boolean = True


    Public Function Clone() As Object Implements System.ICloneable.Clone
        Dim cloned_todo As ToDoItem = New ToDoItem(OlObject, True)
        With cloned_todo
            ._ToDoID = _ToDoID
            ._TaskSubject = _TaskSubject
            ._MetaTaskSubject = _MetaTaskSubject
            ._MetaTaskLvl = _MetaTaskLvl
            ._TagProgram = _TagProgram
            ._Priority = _Priority
            ._StartDate = _StartDate
            ._Complete = _Complete
            ._TotalWork = _TotalWork
            ._ActiveBranch = _ActiveBranch
            ._ExpandChildren = _ExpandChildren
            ._ExpandChildrenState = _ExpandChildrenState
            ._EC2 = _EC2
            ._VisibleTreeState = _VisibleTreeState
            ._readonly = _readonly
        End With
        Return cloned_todo
    End Function

    ''' <summary>
    ''' Gets and Sets a flag that when true, prevents saving changes to the underlying [object]
    ''' </summary>
    ''' <returns>Boolean</returns>
    Public Property IsReadOnly As Boolean
        Get
            Return _readonly
        End Get
        Set(value As Boolean)
            _readonly = value
        End Set
    End Property

    ''' <summary>
    ''' Saves all internal variables to the [Object]
    ''' </summary>
    Public Sub ForceSave()
        ' Save the current state of the read only flag
        Dim tmp_readonly_state As Boolean = _readonly

        ' Activate saving
        _readonly = False

        WriteFlagsBatch()

        ToDoID = _ToDoID
        TaskSubject = _TaskSubject
        MetaTaskSubject = _MetaTaskSubject
        MetaTaskLvl = _MetaTaskLvl
        TagProgram = _TagProgram
        Priority = _Priority
        StartDate = _StartDate
        Complete = _Complete
        TotalWork = _TotalWork
        ActiveBranch = _ActiveBranch
        ExpandChildren = _ExpandChildren
        ExpandChildrenState = _ExpandChildrenState
        EC2 = _EC2
        VisibleTreeState = _VisibleTreeState

        If TypeOf (OlObject) Is MailItem Then
            Dim OlMail As MailItem = OlObject
            If OlMail.FlagStatus = OlFlagStatus.olNoFlag And _flagAsTask Then
                OlMail.MarkAsTask(OlMarkInterval.olMarkNoDate)
            ElseIf OlMail.FlagStatus = OlFlagStatus.olFlagMarked And Not _flagAsTask Then
                OlMail.ClearTaskFlag()
            End If
            OlMail.Save()
        End If

        ' Return read only variable to its original state
        _readonly = tmp_readonly_state
    End Sub

    Public Sub New(OlMail As [MailItem])
        OlObject = OlMail

        InitializeMail(OlMail)
        _flags = New FlagParser(OlMail.Categories)
        InitializeCustomFields(OlObject)

    End Sub

    Public Sub New(OlMail As [MailItem], OnDemand As Boolean)
        OlObject = OlMail

        If OnDemand = False Then
            InitializeMail(OlMail)
            _flags = New FlagParser(OlMail.Categories)
            InitializeCustomFields(OlObject)
        End If
    End Sub

    Public Sub New(OlTask As [TaskItem])
        OlObject = OlTask

        InitializeTask(OlTask)
        _flags = New FlagParser(OlTask.Categories)
        InitializeCustomFields(OlObject)

    End Sub

    Public Sub New(OlTask As [TaskItem], OnDemand As Boolean)
        OlObject = OlTask

        If OnDemand = False Then
            InitializeTask(OlTask)
            _flags = New FlagParser(OlTask.Categories)
            InitializeCustomFields(OlObject)
        End If
    End Sub

    Public Sub New(Item As Object, OnDemand As Boolean)

        OlObject = Item
        _flags = New FlagParser(Item.Categories)
        If OnDemand = False Then
            MsgBox("Coding Error: New ToDoItem() is overloaded. Only supply the OnDemand variable if you want to load values on demand")
        End If
    End Sub

    Public Sub New(strID As String)
        _ToDoID = strID
    End Sub

    Private Sub InitializeMail(OlMail As MailItem)
        With OlMail
            If OlMail.TaskSubject.Length <> 0 Then
                _TaskSubject = .TaskSubject
            Else
                _TaskSubject = .Subject
            End If
            _Priority = .Importance
            _TaskCreateDate = .CreationTime
            _StartDate = .TaskStartDate
            _Complete = (.FlagStatus = OlFlagStatus.olFlagComplete)
            If PA_FieldExists(PA_TOTAL_WORK) Then
                _TotalWork = .PropertyAccessor.GetProperty(PA_TOTAL_WORK)
            Else
                _TotalWork = 0
            End If
        End With
    End Sub

    Private Sub InitializeTask(OlTask As TaskItem)
        With OlTask
            _TaskSubject = .Subject
            _Priority = .Importance
            _TaskCreateDate = .CreationTime
            _StartDate = .StartDate
            _Complete = .Complete
            _TotalWork = .TotalWork
        End With
    End Sub

    Private Sub InitializeCustomFields(Item As Object)
        _TagProgram = CustomField("TagProgram")
        _ActiveBranch = CustomField("AB", OlUserPropertyType.olYesNo)
        _EC2 = CustomField("EC2", OlUserPropertyType.olYesNo)
        _ExpandChildren = CustomField("EC")
        _ExpandChildrenState = CustomField("EcState")
    End Sub

    Public Sub WriteFlagsBatch()
        OlObject.Categories = _flags.Combine()
        OlObject.Save()
        CustomField("TagContext", OlUserPropertyType.olKeywords) = _flags.Context(False)
        CustomField("TagPeople", OlUserPropertyType.olKeywords) = _flags.People(False)
        'TODO: Assign ToDoID if project assignment changes
        'TODO: If ID exists and project reassigned, move any children
        CustomField("TagProject", OlUserPropertyType.olKeywords) = _flags.Projects(False)
        CustomField("TagTopic", OlUserPropertyType.olKeywords) = _flags.Topics(False)
        CustomField("KB") = _flags.KB(False)
    End Sub

    Public ReadOnly Property object_item As Object
        Get
            Return OlObject
        End Get
    End Property

    Public Property FlagAsTask As Boolean
        Get
            Return _flagAsTask
        End Get
        Set(value As Boolean)
            If Not OlObject Is Nothing Then
                If TypeOf (OlObject) Is MailItem Then
                    _flagAsTask = value
                    If Not _readonly Then
                        Dim OlMail As MailItem = OlObject
                        If OlMail.FlagStatus = OlFlagStatus.olNoFlag And value Then
                            OlMail.MarkAsTask(OlMarkInterval.olMarkNoDate)
                        ElseIf OlMail.FlagStatus = OlFlagStatus.olFlagMarked And Not value Then
                            OlMail.ClearTaskFlag()
                        End If
                        OlMail.Save()
                    End If
                ElseIf TypeOf (OlObject) Is TaskItem Then
                    _flagAsTask = True
                Else
                    _flagAsTask = False
                End If
            End If
        End Set

    End Property

    Public ReadOnly Property TaskCreateDate As Date
        Get
            TaskCreateDate = _TaskCreateDate
        End Get
    End Property

    Public Property Bullpin As Boolean
        Get
            Return _flags.bullpin
        End Get
        Set(value As Boolean)
            _flags.bullpin = value
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    OlObject.Categories = _flags.Combine()
                    OlObject.Save
                End If
            End If
        End Set
    End Property

    Public Property Today As Boolean
        Get
            Return _flags.today
        End Get
        Set(value As Boolean)
            _flags.today = value
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    OlObject.Categories = _flags.Combine()
                    OlObject.Save
                End If
            End If
        End Set
    End Property

    Public Property ReminderTime As Date
        Get
            Return OlObject.ReminderTime
        End Get
        Set(value As Date)
            If Not _readonly Then
                OlObject.ReminderTime = value
                OlObject.Save()
            End If
        End Set
    End Property

    Public Property DueDate As Date
        Get
            If TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                Return OlMail.TaskDueDate
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                Return OlTask.DueDate
            Else
                Return DateValue("1/1/4501")
            End If
        End Get
        Set(value As Date)
            If Not _readonly Then
                If TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    OlMail.TaskDueDate = value
                    OlMail.Save()
                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    OlTask.DueDate = value
                    OlTask.Save()
                End If
            End If
        End Set
    End Property

    Public Property StartDate As Date
        Get
            Return _TaskCreateDate
        End Get
        Set(value As Date)
            _TaskCreateDate = value
        End Set
    End Property

    Public Property Priority As [OlImportance]
        Get

            If OlObject Is Nothing Then
                _Priority = OlImportance.olImportanceNormal
            ElseIf TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                _Priority = OlMail.Importance
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                _Priority = OlTask.Importance
            End If
            Return _Priority
        End Get
        Set(value As [OlImportance])
            _Priority = value
            If Not _readonly Then
                If OlObject Is Nothing Then
                ElseIf TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    OlMail.Importance = _Priority
                    OlMail.Save()
                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    OlTask.Importance = _Priority
                    OlTask.Save()
                End If
            End If
        End Set
    End Property

    Public Property Complete As Boolean
        Get
            If OlObject Is Nothing Then
                _Complete = False
            ElseIf TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                If (OlMail.FlagStatus = OlFlagStatus.olFlagComplete) Then
                    _Complete = True
                Else
                    _Complete = False
                End If
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                _Complete = OlTask.Complete
            End If
            Return _Complete
        End Get
        Set(value As Boolean)
            _Complete = value
            If Not _readonly Then
                If OlObject Is Nothing Then
                ElseIf TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    If value = True Then
                        OlMail.FlagStatus = OlFlagStatus.olFlagComplete
                    Else
                        OlMail.FlagStatus = OlFlagStatus.olFlagMarked
                    End If
                    OlMail.Save()
                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    OlTask.Complete = value
                    OlTask.Save()
                End If
            End If
        End Set
    End Property

    Public Property TaskSubject As String
        Get
            If _TaskSubject.Length = 0 Then
                If OlObject Is Nothing Then
                    _TaskSubject = ""
                ElseIf TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    _TaskSubject = OlMail.TaskSubject
                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    _TaskSubject = OlTask.Subject
                Else
                    _TaskSubject = ""
                End If
            End If
            Return _TaskSubject
        End Get
        Set(value As String)
            _TaskSubject = value
            If Not _readonly Then
                If OlObject Is Nothing Then
                ElseIf TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    OlMail.TaskSubject = _TaskSubject
                    OlMail.Save()
                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    OlTask.Subject = _TaskSubject
                    OlTask.Save()
                End If
            End If
        End Set
    End Property

    Public Property People(Optional IncludePrefix As Boolean = False) As String
        Get
            EnsureInitialized(CallerName:="People")
            Return _flags.People(IncludePrefix)
        End Get
        Set(value As String)
            ' Set People and sanitize value
            _flags.People = value
            If Not _readonly Then SaveCatsToObj("TagPeople", _flags.People(False))
        End Set
    End Property



    Public Property Project(Optional IncludePrefix As Boolean = False) As String
        Get
            EnsureInitialized(CallerName:="Project")
            Return _flags.Projects(IncludePrefix)
        End Get
        Set(value As String)
            ' Set Projects and sanitize value
            'TODO: Assign ToDoID if project assignment changes
            'TODO: If ID exists and project reassigned, move any children 
            _flags.Projects = value
            If Not _readonly Then SaveCatsToObj("TagProject", _flags.Projects(False))
        End Set
    End Property

    Public Property TagProgram As String
        Get
            If _TagProgram.Length <> 0 Then
                Return _TagProgram
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagProgram = CustomField("TagProgram", OlUserPropertyType.olKeywords)
                Return _TagProgram
            End If

        End Get
        Set(value As String)
            _TagProgram = value
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    CustomField("TagProgram", OlUserPropertyType.olKeywords) = value
                End If
            End If
        End Set
    End Property

    Public Property Context(Optional IncludePrefix As Boolean = False) As String
        Get
            EnsureInitialized(CallerName:="Context")
            Return _flags.Context(IncludePrefix)
        End Get
        Set(value As String)
            ' Set Context and sanitize value
            _flags.Context = value
            If Not _readonly Then SaveCatsToObj("TagContext", _flags.Context(False))
        End Set
    End Property

    Public Property Topic(Optional IncludePrefix As Boolean = False) As String
        Get
            EnsureInitialized(CallerName:="Topic")
            Return _flags.Topics(IncludePrefix)
        End Get
        Set(value As String)
            ' Set Context and sanitize value
            _flags.Topics = value
            If Not _readonly Then SaveCatsToObj("TagTopic", _flags.Topics(False))
        End Set
    End Property

    Public Property KB(Optional IncludePrefix As Boolean = False) As String
        Get
            EnsureInitialized(CallerName:="KB")
            Return _flags.KB(IncludePrefix)
        End Get
        Set(value As String)
            ' Set Context and sanitize value
            _flags.KB = value
            If Not _readonly Then SaveCatsToObj("KB", _flags.KB(False))
        End Set
    End Property

    Private Sub SaveCatsToObj(FieldName As String, FieldValue As String)
        If Not OlObject Is Nothing Then
            CustomField(FieldName, OlUserPropertyType.olKeywords) = FieldValue
            OlObject.Categories = _flags.Combine()
            OlObject.Save
        End If
    End Sub

    Private Sub EnsureInitialized(CallerName As String)
        If _flags Is Nothing Then
            If OlObject Is Nothing Then Throw New ArgumentNullException(
                "Cannot get property " & CallerName & " if both _flags AND olObject are Null")
            _flags = New FlagParser(OlObject.Categories)
        End If
    End Sub

    Public Property TotalWork As Integer
        Get
            If _TotalWork = 0 Then
                If OlObject Is Nothing Then
                    _TotalWork = 0
                ElseIf TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    If PA_FieldExists(PA_TOTAL_WORK) Then
                        _TotalWork = OlMail.PropertyAccessor.GetProperty(PA_TOTAL_WORK)
                    Else
                        _TotalWork = 0
                    End If

                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    _TotalWork = OlTask.TotalWork

                Else
                    _TotalWork = 0
                End If
            End If
            Return _TotalWork

        End Get

        Set(value As Integer)
            _TotalWork = value
            If Not _readonly Then
                If OlObject Is Nothing Then
                ElseIf TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    OlMail.PropertyAccessor.SetProperty(PA_TOTAL_WORK, value)
                    OlMail.Save()
                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    OlTask.TotalWork = value
                    OlTask.Save()
                End If
            End If
        End Set
    End Property

    Public Property ToDoID As String
        Get
            If _ToDoID.Length <> 0 Then
                Return _ToDoID
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _ToDoID = CustomField("ToDoID")
                Return _ToDoID
            End If
        End Get
        Set(strID As String)
            _ToDoID = strID
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    CustomField("ToDoID") = strID
                    SplitID()
                End If
            End If
        End Set
    End Property
    '_VisibleTreeState
    Public Property VisibleTreeStateLVL(ByVal Lvl As Integer) As Boolean
        Get
            Return ((Math.Pow(2, Lvl - 1) & VisibleTreeState) > 0)
        End Get
        Set(value As Boolean)
            If value = True Then
                VisibleTreeState = VisibleTreeState Or Math.Pow(2, Lvl - 1)
            Else
                VisibleTreeState = VisibleTreeState - (VisibleTreeState And Math.Pow(2, Lvl - 1))
            End If
        End Set
    End Property
    Public Property VisibleTreeState As Integer
        Get
            If _VisibleTreeState <> 0 Then
                Return _VisibleTreeState
            ElseIf OlObject Is Nothing Then
                Return -1
            Else
                Dim objProperty As [UserProperty] = OlObject.UserProperties.Find("VTS")
                If objProperty Is Nothing Then
                    CustomField("VTS", OlUserPropertyType.olInteger) = 63 'Binary 111111 for 6 levels
                    _VisibleTreeState = 63
                Else
                    _VisibleTreeState = CustomField("VTS", OlUserPropertyType.olInteger)
                End If
                Return _VisibleTreeState

            End If
        End Get
        Set(intVTS As Integer)
            If Not OlObject Is Nothing Then
                _VisibleTreeState = intVTS
                If Not _readonly Then CustomField("VTS", OlUserPropertyType.olInteger) = intVTS
            End If
        End Set
    End Property

    Public Property ActiveBranch As Boolean
        Get
            If _ActiveBranch = True Then
                Return True
            ElseIf OlObject Is Nothing Then
                Return False
            Else
                If CustomFieldExists("AB") Then
                    _ActiveBranch = CustomField("AB", OlUserPropertyType.olYesNo)
                Else
                    CustomField("AB", OlUserPropertyType.olYesNo) = True
                    _ActiveBranch = True
                End If

                Return _ActiveBranch
            End If
        End Get
        Set(blActive As Boolean)
            _ActiveBranch = blActive
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    CustomField("AB", OlUserPropertyType.olYesNo) = blActive
                End If
            End If
        End Set
    End Property

    Public Property EC2 As Boolean
        Get
            If CustomFieldExists("EC2") Then
                _EC2 = CustomField("EC2")

                If _EC2 = True Then
                    If ExpandChildren = "+" Then
                        ExpandChildren = "-"
                    End If
                Else
                    If ExpandChildren = "-" Then
                        ExpandChildren = "+"
                    End If
                End If
            End If
            Return _EC2
        End Get
        Set(blValue As Boolean)
            _EC2 = blValue
            If Not _readonly Then CustomField("EC2", OlUserPropertyType.olYesNo) = blValue
            _ExpandChildren = ""
            _ExpandChildrenState = ""
        End Set
    End Property

    Public Property EC_Change As Boolean
        Get
            If ExpandChildren.Length = 0 Then
                ExpandChildren = "-"
            End If

            If ExpandChildrenState = ExpandChildren Then
                Return False
            Else
                Return True
            End If
        End Get
        Set(blValue As Boolean)
            If blValue = False Then
                ExpandChildrenState = ExpandChildren
            End If
        End Set
    End Property
    Public Property ExpandChildren As String
        Get
            If _ExpandChildren.Length <> 0 Then
                Return _ExpandChildren
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _ExpandChildren = CustomField("EC")
                Return _ExpandChildren
            End If
        End Get
        Set(strState As String)
            _ExpandChildren = strState
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    CustomField("EC") = strState
                End If
            End If
        End Set
    End Property

    Public Property ExpandChildrenState As String
        Get
            If _ExpandChildrenState.Length <> 0 Then
                Return _ExpandChildrenState
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _ExpandChildrenState = CustomField("EcState")
                Return _ExpandChildrenState
            End If
        End Get
        Set(strState As String)
            _ExpandChildrenState = strState
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    CustomField("EcState") = strState
                End If
            End If
        End Set
    End Property

    Public Sub SplitID()
        Dim strField As String = ""
        Dim strFieldValue As String = ""
        Try
            Dim strToDoID As String = ToDoID
            Dim strToDoID_Len As Long = strToDoID.Length
            If strToDoID_Len > 0 Then
                Dim maxlen As Long = Globals.ThisAddIn.IDList.MaxIDLength

                For i = 2 To maxlen Step 2
                    strField = "ToDoIdLvl" & (i / 2)
                    strFieldValue = "00"
                    If i <= strToDoID_Len Then
                        strFieldValue = Mid(strToDoID, i - 1, 2)
                    End If
                    If Not _readonly Then CustomField(strField) = strFieldValue
                Next
            End If
        Catch
            Debug.WriteLine("Error in Split_ToDoID")
            Debug.WriteLine(Err.Description)
            Debug.WriteLine("Field Name is " & strField)
            Debug.WriteLine("Field Value is " & strFieldValue)
            Stop
        End Try
    End Sub

    Public Property MetaTaskLvl As String
        Get
            If _MetaTaskLvl.Length <> 0 Then
                Return _MetaTaskLvl
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _MetaTaskLvl = CustomField("Meta Task Level")
                Return _MetaTaskLvl
            End If
        End Get
        Set(strLvl As String)
            _MetaTaskLvl = strLvl
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    CustomField("Meta Task Level") = strLvl
                End If
            End If
        End Set
    End Property

    Public Property MetaTaskSubject As String
        Get
            If _MetaTaskSubject.Length <> 0 Then
                Return _MetaTaskSubject
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _MetaTaskSubject = CustomField("Meta Task Subject")
                Return _MetaTaskSubject
            End If
        End Get
        Set(strID As String)
            _MetaTaskSubject = strID
            If Not _readonly Then
                If Not OlObject Is Nothing Then
                    'CustomFieldID_Set("Meta Task Subject", strID, SpecificItem:=OlObject)
                    CustomField("Meta Task Subject") = strID
                End If
            End If
        End Set
    End Property

    Public Sub SwapIDPrefix(strPrefixOld, strPrefixNew)

    End Sub

    Public Function GetItem() As Object
        Return OlObject
    End Function

    Public ReadOnly Property InFolder() As String
        Get
            Dim prefix As String = Globals.ThisAddIn._OlNS.DefaultStore.GetRootFolder.FolderPath & "\"
            Return Replace(OlObject.Parent.FolderPath, prefix, "")
        End Get
    End Property

    Public ReadOnly Property PA_FieldExists(PA_Schema As String) As Boolean
        Get
            Try
                Dim OlPA As [PropertyAccessor] = OlObject.PropertyAccessor
                Dim OlProperty As Object = OlPA.GetProperty(PA_Schema)
                Return True
            Catch
                Return False
            End Try
        End Get
    End Property

    Public ReadOnly Property CustomFieldExists(FieldName As String) As Boolean
        Get
            Dim objProperty As [UserProperty] = OlObject.UserProperties.Find(FieldName)
            If objProperty Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Get
    End Property
    Public Property CustomField(FieldName As String, Optional ByVal OlFieldType As [OlUserPropertyType] = [OlUserPropertyType].olText)
        Get
            Dim objProperty As [UserProperty] = OlObject.UserProperties.Find(FieldName)
            If objProperty Is Nothing Then
                If OlFieldType = OlUserPropertyType.olInteger Then
                    Return 0
                ElseIf OlFieldType = OlUserPropertyType.olYesNo Then
                    Return False
                Else
                    Return ""
                End If

            Else
                If IsArray(objProperty.Value) Then
                    Return FlattenArry(objProperty.Value)
                Else
                    Return objProperty.Value
                End If
            End If
        End Get
        Set(value)
            Dim objProperty As [UserProperty] = OlObject.UserProperties.Find(FieldName)
            If objProperty Is Nothing Then
                Try
                    objProperty = OlObject.UserProperties.Add(FieldName, OlFieldType)
                    objProperty.Value = value
                    OlObject.Save()

                Catch e As System.Exception
                    Debug.WriteLine("Exception in Set User Property: " & FieldName)
                    Debug.WriteLine(e.Message)
                    Debug.WriteLine(e.Source)
                    Debug.WriteLine(e.StackTrace)

                End Try
            Else
                objProperty.Value = value
                OlObject.Save()
            End If

        End Set

    End Property

    Private Function FlattenArry(varBranch() As Object) As String
        Dim i As Integer
        Dim strTemp As String

        strTemp = ""

        For i = 0 To UBound(varBranch)
            If IsArray(varBranch(i)) Then
                strTemp = strTemp & ", " & FlattenArry(varBranch(i))
            Else
                strTemp = strTemp & ", " & varBranch(i)
            End If
        Next i
        If strTemp.Length <> 0 Then strTemp = Right(strTemp, Len(strTemp) - 2)
        FlattenArry = strTemp
    End Function

End Class

