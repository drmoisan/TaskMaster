Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports BrightIdeasSoftware
Imports System.Collections
Imports System.IO
Imports Microsoft.Office.Interop.Outlook
Imports System.Collections.Generic
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports Microsoft.Office.Core
Imports System.Linq.Expressions

Public Class TreeNode(Of T)
    Private _Parent As TreeNode(Of T)
    Private ReadOnly _value As T
    'Private ReadOnly _children As List(Of TreeNode(Of T)) = New List(Of TreeNode(Of T))()
    Private _children As List(Of TreeNode(Of T)) = New List(Of TreeNode(Of T))()
    Public ID As String

    Public Sub New(ByVal value As T)
        _value = value
    End Sub

    Public Sub New(ByVal value As T, ByVal strID As String)
        _value = value
        ID = strID
    End Sub

    Default Public ReadOnly Property Item(ByVal i As Integer) As TreeNode(Of T)
        Get
            Return _children(i)
        End Get
    End Property

    Public ReadOnly Property ParentID As String
        Get
            If _Parent Is Nothing Then
                Return "Root"
            Else
                Return _Parent.ID
            End If
        End Get
    End Property
    Public Property Parent As TreeNode(Of T)
        Get
            Return _Parent
        End Get
        Set(value As TreeNode(Of T))
            _Parent = value
        End Set
    End Property

    Public ReadOnly Property Value As T
        Get
            Return _value
        End Get
    End Property

    Public Function IsAncestor(ByVal model As TreeNode(Of T)) As Boolean
        If Me Is model Then Return True
        If _Parent Is Nothing Then Return False
        Return _Parent.IsAncestor(model)
    End Function

    Public ReadOnly Property ChildCount As Integer
        Get
            Return _children.Count
        End Get
    End Property

    'Public ReadOnly Property NextChildId As String
    '    Get
    Public Function NextChildID() As String
        Dim strMaxID As String = ID & "00"
        Dim lngMaxID As Long = ConvertToDecimal(125, strMaxID)
        Dim strTmpID As String = ""
        Dim lngTmpID As Long = 0
        For Each child In Children
            strTmpID = child.ID
            lngTmpID = ConvertToDecimal(125, strTmpID)
            If lngTmpID > lngMaxID Then
                lngMaxID = lngTmpID
            End If
        Next child

        Dim blContinue As Boolean = True
        While blContinue
            lngMaxID += 1
            strMaxID = ConvertToBase(125, lngMaxID)
            If Globals.ThisAddIn.UsedIDList.Contains(strMaxID) = False Then
                blContinue = False
            End If
        End While
        Globals.ThisAddIn.UsedIDList_Append(strMaxID)
        Return strMaxID
    End Function
    '    End Get
    'End Property


    Public Property Children As List(Of TreeNode(Of T))
        Get
            Return _children
        End Get
        Set(value As List(Of TreeNode(Of T)))
            _children = value
        End Set
    End Property

    'Public ReadOnly Property Children As ReadOnlyCollection(Of TreeNode(Of T))
    '    Get
    '        Return _children.AsReadOnly()
    '    End Get
    'End Property

    Public Function AddChild(ByVal value As T) As TreeNode(Of T)
        Dim node = New TreeNode(Of T)(value) With {
                .Parent = Me
            }
        node.ID = NextChildId()
        _children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal node As TreeNode(Of T)) As TreeNode(Of T)
        node.Parent = Me
        node.ID = NextChildId()
        _children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal node As TreeNode(Of T), ByVal strID As String) As TreeNode(Of T)
        node.Parent = Me
        node.ID = strID
        _children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal value As T, ByVal strID As String) As TreeNode(Of T)
        Dim node = New TreeNode(Of T)(value) With {
                .Parent = Me
            }
        node.ID = strID
        _children.Add(node)
        Return node
    End Function
    Public Function AddChildren(ParamArray values As T()) As TreeNode(Of T)()
        Return values.[Select](New Func(Of T, TreeNode(Of T))(AddressOf AddChild)).ToArray()
    End Function

    Public Function RemoveChild(ByVal node As TreeNode(Of T)) As Boolean
        Return _children.Remove(node)
    End Function
    'Public Sub SubsituteIDPrefix(ByVal strOld, ByVal strNew)
    '    If Mid(ID, 1, strOld.Length) = strOld Then
    '        ID = strNew & Mid(ID, strOld.Length + 1, ID.Length - strOld.Length)
    '    End If

    '    For Each child In _children
    '        child.SubsituteIDPrefix(strOld, strNew)
    '    Next

    'End Sub
    Public Sub Traverse(ByVal action As Action(Of T))
        action(Value)

        For Each child In _children
            child.Traverse(action)
        Next
    End Sub

    Public Function FindChildByID(strID As String) As TreeNode(Of T)
        Dim node As TreeNode(Of T)
        Dim rnode As TreeNode(Of T)
        If ID = strID Then
            Return Me
        Else
            For Each node In Children
                rnode = node.FindChildByID(strID)
                If Not rnode Is Nothing Then
                    Return rnode
                End If
            Next
            Return Nothing
        End If
    End Function

    Public Function Flatten() As IEnumerable(Of T)
        Return {Value}.Concat(_children.SelectMany(Function(x) x.Flatten()))
    End Function
End Class

Public Class ToDoItem
    Private OlObject As Object
    Private _ToDoID As String = ""
    Public _TaskSubject As String = ""
    Public _MetaTaskSubject As String = ""
    Private _TagContext As String = ""
    Private _TagProgram As String = ""
    Private _TagProject As String = ""
    Private _TagPeople As String = ""
    Private _TagTopic As String = ""
    Private _Priority As Outlook.OlImportance
    Private _TaskCreateDate As Date

    Public Sub New(OlMail As Outlook.MailItem)
        OlObject = OlMail
        If OlMail.TaskSubject.Length <> 0 Then
            _TaskSubject = OlMail.TaskSubject
        Else
            _TaskSubject = OlMail.Subject
        End If
        _TagContext = CustomFieldID_GetValue(OlObject, "TagContext")
        _TagProgram = CustomFieldID_GetValue(OlObject, "TagProgram")
        _TagProject = CustomFieldID_GetValue(OlObject, "TagProject")
        _TagPeople = CustomFieldID_GetValue(OlObject, "TagPeople")
        _TagTopic = CustomFieldID_GetValue(OlObject, "TagTopic")
        _Priority = OlMail.Importance
        _TaskCreateDate = OlMail.CreationTime
    End Sub
    Public Sub New(OlTask As Outlook.TaskItem)
        OlObject = OlTask

        _TaskSubject = OlTask.Subject
        _TagContext = CustomFieldID_GetValue(OlObject, "TagContext")
        _TagProgram = CustomFieldID_GetValue(OlObject, "TagProgram")
        _TagProject = CustomFieldID_GetValue(OlObject, "TagProject")
        _TagPeople = CustomFieldID_GetValue(OlObject, "TagPeople")
        _TagTopic = CustomFieldID_GetValue(OlObject, "TagTopic")
        _Priority = OlTask.Importance
        _TaskCreateDate = OlTask.CreationTime
    End Sub

    Public Sub New(strID As String)
        _ToDoID = strID
    End Sub

    Public ReadOnly Property TaskCreateDate As Date
        Get
            TaskCreateDate = _TaskCreateDate
        End Get
    End Property
    Public Property Priority As Outlook.OlImportance
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
        Set(value As Outlook.OlImportance)
            _Priority = value
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
        End Set
    End Property

    Public Property TagPeople As String
        Get
            If _TagPeople.Length <> 0 Then
                Return _TagPeople
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagPeople = CustomFieldID_GetValue(OlObject, "TagPeople")
                Return _TagPeople
            End If
        End Get
        Set(value As String)
            _TagPeople = value
            If Not OlObject Is Nothing Then
                CustomFieldID_Set("TagPeople", value, SpecificItem:=OlObject)
            End If
        End Set
    End Property

    Public Property TagProject As String
        Get
            If _TagProject.Length <> 0 Then
                Return _TagProject
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagProject = CustomFieldID_GetValue(OlObject, "TagProject")
                Return _TagProject
            End If

        End Get
        Set(value As String)
            _TagProject = value
            If Not OlObject Is Nothing Then
                CustomFieldID_Set("TagProject", value, SpecificItem:=OlObject)
            End If
        End Set
    End Property

    Public Property TagProgram As String
        Get
            If _TagProgram.Length <> 0 Then
                Return _TagProgram
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagProgram = CustomFieldID_GetValue(OlObject, "TagProgram")
                Return _TagProgram
            End If

        End Get
        Set(value As String)
            _TagProgram = value
            If Not OlObject Is Nothing Then
                CustomFieldID_Set("TagProgram", value, SpecificItem:=OlObject)
            End If
        End Set
    End Property

    Public Property TagContext As String
        Get
            If _TagContext.Length <> 0 Then
                Return _TagContext
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagContext = CustomFieldID_GetValue(OlObject, "TagContext")
                Return _TagContext
            End If

        End Get
        Set(value As String)
            _TagContext = value
            If Not OlObject Is Nothing Then
                CustomFieldID_Set("TagContext", value, SpecificItem:=OlObject)
            End If
        End Set
    End Property

    Public Property TagTopic As String
        Get
            If _TagTopic.Length <> 0 Then
                Return _TagTopic
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _TagTopic = CustomFieldID_GetValue(OlObject, "TagTopic")
                Return _TagTopic
            End If

        End Get
        Set(value As String)
            _TagTopic = value
            If Not OlObject Is Nothing Then
                CustomFieldID_Set("TagTopic", value, SpecificItem:=OlObject)
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
                _ToDoID = CustomFieldID_GetValue(OlObject, "ToDoID")
                Return _ToDoID
            End If
        End Get
        Set(strID As String)
            _ToDoID = strID
            If Not OlObject Is Nothing Then
                CustomFieldID_Set("ToDoID", strID, SpecificItem:=OlObject)
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
                _MetaTaskSubject = CustomFieldID_GetValue(OlObject, "Meta Task Subject")
                Return _MetaTaskSubject
            End If
        End Get
        Set(strID As String)
            _MetaTaskSubject = strID
            If Not OlObject Is Nothing Then
                CustomFieldID_Set("Meta Task Subject", strID, SpecificItem:=OlObject)
            End If
        End Set
    End Property

    Public Sub SwapIDPrefix(strPrefixOld, strPrefixNew)

    End Sub

    Private Function CustomFieldID_GetValue(objItem As Object, ByVal UserDefinedFieldName As String) As String
        Dim OlMail As Outlook.MailItem
        Dim OlTask As Outlook.TaskItem
        Dim OlAppt As Outlook.AppointmentItem
        Dim objProperty As Outlook.UserProperty


        If objItem Is Nothing Then
            Return ""
        ElseIf TypeOf objItem Is Outlook.MailItem Then
            OlMail = objItem
            objProperty = OlMail.UserProperties.Find(UserDefinedFieldName)

        ElseIf TypeOf objItem Is Outlook.TaskItem Then
            OlTask = objItem
            objProperty = OlTask.UserProperties.Find(UserDefinedFieldName)
        ElseIf TypeOf objItem Is Outlook.AppointmentItem Then
            OlAppt = objItem
            objProperty = OlAppt.UserProperties.Find(UserDefinedFieldName)
        Else
            objProperty = Nothing
            MsgBox("Unsupported object type")
        End If

        If objProperty Is Nothing Then
            Return ""
        Else
            If IsArray(objProperty.Value) Then
                Return FlattenArry(objProperty.Value)
            Else
                Return objProperty.Value
            End If
        End If

        OlMail = Nothing
        OlTask = Nothing
        OlAppt = Nothing
        objProperty = Nothing

    End Function
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

    Private Function CustomFieldID_Set(ByVal UserDefinedFieldName As String,
                               Optional ByVal Value As String = "",
                               Optional ByVal IsCustomEntry As Boolean = False,
                               Optional ByRef SpecificItem As Object = Nothing,
                               Optional ByVal olUPType As Outlook.OlUserPropertyType =
                               Outlook.OlUserPropertyType.olText) As Boolean

        Dim myCollection As Object
        Dim Msg As Outlook.MailItem
        Dim oTask As Outlook.TaskItem
        Dim oMail As Outlook.MailItem
        Dim OlAppointment As Outlook.AppointmentItem
        Dim objProperty As Outlook.UserProperty


        Try
            If Not SpecificItem Is Nothing Then
                If TypeOf SpecificItem Is MailItem Then
                    oMail = SpecificItem
                    objProperty = oMail.UserProperties.Find(UserDefinedFieldName)
                    If objProperty Is Nothing Then objProperty = oMail.UserProperties.Add(UserDefinedFieldName, olUPType)
                    objProperty.Value = Value
                    oMail.Save()
                End If
                If TypeOf SpecificItem Is TaskItem Then
                    oTask = SpecificItem
                    objProperty = oTask.UserProperties.Find(UserDefinedFieldName)
                    If objProperty Is Nothing Then objProperty = oTask.UserProperties.Add(UserDefinedFieldName, olUPType)
                    objProperty.Value = Value
                    oTask.Save()
                End If
                If TypeOf SpecificItem Is Outlook.AppointmentItem Then
                    OlAppointment = SpecificItem
                    objProperty = OlAppointment.UserProperties.Find(UserDefinedFieldName)
                    If objProperty Is Nothing Then objProperty = OlAppointment.UserProperties.Add(UserDefinedFieldName, olUPType)
                    objProperty.Value = Value
                    OlAppointment.Save()
                End If
            End If
            CustomFieldID_Set = True
        Catch
            Debug.WriteLine("Exception caught: ", Err)
            CustomFieldID_Set = False
            Err.Clear()
        Finally
            Msg = Nothing
            objProperty = Nothing
            myCollection = Nothing
            oTask = Nothing
            oMail = Nothing
            OlAppointment = Nothing
        End Try

    End Function

End Class


'Public Class ToDoItem
'        Implements INotifyPropertyChanged

'Public ToDoID As String
'    Public Function ChildCount() As Integer

'    End Function


'    Public ReadOnly Property Children As List(Of ToDoItem_WithChildren)
'        Get
'        End Get
'    End Property


'    Public Sub New(ObjItem As Object)

'    End Sub

'    '    Public Class ModelWithChildren {
'    '    Public int ChildCount { Get { ... } }
'    '    Public List<ModelWithChildren> Children { Get { ... } }
'    '    Public String Label { Get; Set; }
'    '    Public ModelWithChildren Parent { Get; Set; }
'    '    Public String ParentLabel { Get { ... } }
'    '}

'    '#Region "Implementation of INotifyPropertyChanged"

'    '        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'    '        Private Sub OnPropertyChanged(ByVal propertyName As String)
'    '            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'    '        End Sub

'    '#End Region

'End Class
