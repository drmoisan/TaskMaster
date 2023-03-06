Imports Microsoft.VisualBasic

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

Public Class TreeNode(Of T)
    Private _Parent As TreeNode(Of T)
    Private ReadOnly _value As T
    Private ReadOnly _children As List(Of TreeNode(Of T)) = New List(Of TreeNode(Of T))()
    Private _ToDoItem As ToDoItem
    Private _ToDoID As String = ""
    Private _TaskSubject As String = ""

    Public Sub New(ByVal value As T)
        _value = value
    End Sub

    Public Sub New(ByVal OlMail As Outlook.MailItem)
        _ToDoItem = New ToDoItem(OlMail)
    End Sub

    Public Sub New(ByVal OlTask As Outlook.TaskItem)
        _ToDoItem = New ToDoItem(OlTask)
        _ToDoID = ToDoID
    End Sub

    Public Sub New(ByVal OlToDo As ToDoItem)
        _ToDoItem = OlToDo
        _ToDoID = ToDoID
    End Sub

    Public Sub New(ByVal strID As String)
        _ToDoID = strID
        _TaskSubject = "Root"
    End Sub

    Public Property TaskSubject As String
        Get
            If _TaskSubject.Length = 0 Then
                _TaskSubject = _ToDoItem.TaskSubject
            End If
            Return _TaskSubject
        End Get
        Set(value As String)
            If Not _ToDoItem Is Nothing Then
                _ToDoItem.TaskSubject = value
            End If
        End Set
    End Property

    Public Property ToDoID As String
        Get
            If _ToDoID.Length = 0 Then
                _ToDoID = _ToDoItem.ToDoID
            End If
            Return _ToDoID
        End Get
        Set(value As String)
            _ToDoID = value
            If Not _ToDoItem Is Nothing Then
                _ToDoItem.ToDoID = value
            End If
        End Set
    End Property

    Public ReadOnly Property ParentID As String
        Get
            If _Parent Is Nothing Then
                Return "nothing"
            Else
                Return _Parent.ToDoID
            End If
        End Get
    End Property

    Default Public ReadOnly Property Item(ByVal i As Integer) As TreeNode(Of T)
        Get
            Return _children(i)
        End Get
    End Property

    Public Property Parent As TreeNode(Of T)
        Get
            Return _Parent
        End Get
        Private Set(ByVal value As TreeNode(Of T))
            _Parent = value
        End Set
    End Property

    Public ReadOnly Property Value As T
        Get
            Return _value
        End Get
    End Property

    Public ReadOnly Property ChildCount As Integer
        Get
            Return _children.Count
        End Get
    End Property

    Public ReadOnly Property NextChildId As String
        Get
            Dim strMaxID As String = _ToDoID & "00"
            Dim lngMaxID As Long = ConvertToDecimal(125, strMaxID)
            Dim strTmpID As String = ""
            Dim lngTmpID As Long = 0
            For Each child In Children
                strTmpID = child.ToDoID
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
        End Get
    End Property

    Public ReadOnly Property Children As ReadOnlyCollection(Of TreeNode(Of T))
        Get
            Return _children.AsReadOnly()
        End Get
    End Property

    Public Function AddChild(ByVal value As T) As TreeNode(Of T)
        Dim node = New TreeNode(Of T)(value) With {
                .Parent = Me
            }
        node.ToDoID = NextChildId()
        _children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal node As TreeNode(Of T)) As TreeNode(Of T)
        node.Parent = Me
        node.ToDoID = NextChildId()
        _children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal node As TreeNode(Of T), ByVal strToDoID As String) As TreeNode(Of T)
        node.Parent = Me
        node.ToDoID = strToDoID
        _children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal value As T, ByVal strToDoID As String) As TreeNode(Of T)
        Dim node = New TreeNode(Of T)(value) With {
                .Parent = Me
            }
        node.ToDoID = strToDoID
        node._ToDoItem = _value
        _children.Add(node)
        Return node
    End Function
    Public Function AddChildren(ParamArray values As T()) As TreeNode(Of T)()
        Return values.[Select](New Func(Of T, TreeNode(Of T))(AddressOf AddChild)).ToArray()
    End Function

    Public Function RemoveChild(ByVal node As TreeNode(Of T)) As Boolean
        Return _children.Remove(node)
    End Function

    Public Sub Traverse(ByVal action As Action(Of T))
        action(Value)

        For Each child In _children
            child.Traverse(action)
        Next
    End Sub

    Public Function FindChildByID(strID As String) As TreeNode(Of T)
        Dim node As TreeNode(Of T)
        Dim rnode As TreeNode(Of T)
        If ToDoID = strID Then
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

    'Public Function FindNode(strID As String) As TreeNode(Of T)
    '    Dim tmpT As T
    '    Dim objTmp As Object
    '    Dim tmpTreeNode As TreeNode(Of T)
    '    Dim FlatTree As IEnumerable(Of T) = Me.Flatten()
    '    For Each tmpT In FlatTree
    '        objTmp = CType(tmpT, Object)
    '        tmpTreeNode = CType(objTmp, TreeNode(Of T))
    '        If tmpTreeNode.ToDoID = strID Then
    '            Return tmpTreeNode
    '        End If
    '    Next
    '    Return Nothing
    'End Function

    'Public Function FlatTree() As IEnumerable(Of TreeNode(Of T))

    'End Function
    Public Function Flatten() As IEnumerable(Of T)
        Return {Value}.Concat(_children.SelectMany(Function(x) x.Flatten()))
    End Function
End Class

Public Class ToDoItem
    Private OlObject As Object
    Private _ToDoID As String = ""
    Public _TaskSubject As String = ""
    Public _MetaTaskTubject As String = ""
    Private _TagContext As String = ""
    Private _TagProgram As String = ""
    Private _TagProject As String = ""
    Private _TagPeople As String = ""
    Private _Priority As Outlook.OlImportance

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
        _Priority = OlMail.Importance
    End Sub
    Public Sub New(OlTask As Outlook.TaskItem)
        OlObject = OlTask

        _TaskSubject = OlTask.Subject
        _TagContext = CustomFieldID_GetValue(OlObject, "TagContext")
        _TagProgram = CustomFieldID_GetValue(OlObject, "TagProgram")
        _TagProject = CustomFieldID_GetValue(OlObject, "TagProject")
        _TagPeople = CustomFieldID_GetValue(OlObject, "TagPeople")
        _Priority = OlTask.Importance
    End Sub

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
                ElseIf TypeOf OlObject Is MailItem Then
                    Dim OlMail As MailItem = OlObject
                    _TaskSubject = OlMail.TaskSubject
                ElseIf TypeOf OlObject Is TaskItem Then
                    Dim OlTask As TaskItem = OlObject
                    _TaskSubject = OlTask.Subject
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
            Else
                Return CustomFieldID_GetValue(OlObject, "TagPeople")
            End If

        End Get
        Set(value As String)
            _TagPeople = value
            CustomFieldID_Set("TagPeople", value, SpecificItem:=OlObject)
        End Set
    End Property

    Public Property TagProject As String
        Get
            If _TagProject.Length <> 0 Then
                Return _TagProject
            Else
                Return CustomFieldID_GetValue(OlObject, "TagProject")
            End If

        End Get
        Set(value As String)
            _TagProject = value
            CustomFieldID_Set("TagProject", value, SpecificItem:=OlObject)
        End Set
    End Property

    Public Property TagProgram As String
        Get
            If _TagProgram.Length <> 0 Then
                Return _TagProgram
            Else
                Return CustomFieldID_GetValue(OlObject, "TagProgram")
            End If

        End Get
        Set(value As String)
            _TagProgram = value
            CustomFieldID_Set("TagProgram", value, SpecificItem:=OlObject)
        End Set
    End Property

    Public Property TagContext As String
        Get
            If _TagContext.Length <> 0 Then
                Return _TagContext
            Else
                Return CustomFieldID_GetValue(OlObject, "TagContext")
            End If

        End Get
        Set(value As String)
            _TagContext = value
            CustomFieldID_Set("TagContext", value, SpecificItem:=OlObject)
        End Set
    End Property

    Public Property ToDoID As String
        Get
            If _ToDoID.Length = 0 Then
                Return CustomFieldID_GetValue(OlObject, "ToDoID")
            Else
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
