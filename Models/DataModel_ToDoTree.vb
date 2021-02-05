﻿Imports System
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
Imports System.Runtime.Serialization.Formatters.Binary


Public Class DataModel_ToDoTree
    Public Enum LoadOptions
        vbLoadAll = 0
        vbLoadInView = 1
    End Enum

    Private ToDoTree As List(Of TreeNode(Of ToDoItem)) = New List(Of TreeNode(Of ToDoItem))

    Public Sub New()
        ToDoTree = New List(Of TreeNode(Of ToDoItem))
    End Sub

    Public Sub New(DM_ToDoTree As List(Of TreeNode(Of ToDoItem)))
        ToDoTree = DM_ToDoTree
    End Sub

    Public ReadOnly Property ListOfToDoTree As List(Of TreeNode(Of ToDoItem))
        Get
            Return ToDoTree
        End Get
    End Property

    Public Sub LoadTree(LoadType As LoadOptions)
        Dim objItem As Object

        Dim strTemp As String
        Dim strPrev As String
        Dim colItems As Collection
        strPrev = ""
        strTemp = ""

        Try
            '***STEP 1: LOAD RAW OUTLOOK ITEMS TO A LIST AND SORT THEM***
            Dim TreeItems As List(Of Object) = GetToDoList(LoadType)
            TreeItems = MergeSort(Of Object)(TreeItems, AddressOf CompareItemsByToDoID)

            colItems = New Collection
            Dim colNoID = New Collection
            Dim tmpToDo As ToDoItem = Nothing
            Dim ToDoNode As TreeNode(Of ToDoItem)
            Dim NodeParent As TreeNode(Of ToDoItem)


            '***STEP 2: ADD ITEMS TO A FLAT TREE & ASSIGN IDs TO THOSE THAT DON'T HAVE THEM***
            ' Iterate through ToDo items in List
            For Each objItem In TreeItems

                'Cast objItem to temporary ToDoItem
                If TypeOf objItem Is MailItem Then
                    tmpToDo = New ToDoItem(CType(objItem, MailItem))
                ElseIf TypeOf objItem Is TaskItem Then
                    tmpToDo = New ToDoItem(CType(objItem, TaskItem))
                End If

                'Add the temporary ToDoItem to the tree, assigning an ID if missing
                If tmpToDo.ToDoID = "nothing" Then
                    'ToDoTree.AddChild(tmpToDo)
                    ToDoTree.Add(New TreeNode(Of ToDoItem)(tmpToDo))
                Else
                    'ToDoTree.AddChild(tmpToDo, tmpToDo.ToDoID)
                    ToDoTree.Add(New TreeNode(Of ToDoItem)(tmpToDo, tmpToDo.ToDoID))
                End If
            Next

            '***STEP 3: MAKE TREE HIERARCHICAL
            Dim max As Integer = ToDoTree.Count - 1
            Dim i As Integer

            'Loop through the tree from the end to the beginning
            For i = max To 0 Step -1
                ToDoNode = ToDoTree(i)

                'If the ID is bigger than 2 digits, it is a child of someone. 
                'So in that case link it to the proper parent
                'First try cutting off the last two digits, but in the case of
                'Filtered Items, it is possible that the parent is not visible.
                'If the parent is not visible, work recursively to find the next 
                'closest visible parent until you get to the root
                If ToDoNode.ID.Length > 2 Then
                    Dim strID As String = ToDoNode.ID
                    Dim strParentID As String = Mid(strID, 1, strID.Length - 2)
                    Dim blContinue As Boolean = True

                    While blContinue
                        NodeParent = FindChildByID(strParentID, ToDoTree)
                        If Not NodeParent Is Nothing Then
                            NodeParent.InsertChild(ToDoNode, ToDoNode.ID)
                            ToDoTree.Remove(ToDoNode)
                            blContinue = False
                        End If
                        If strParentID.Length > 2 Then
                            strParentID = Mid(strParentID, 1, strParentID.Length - 2)
                        Else
                            blContinue = False
                        End If
                    End While
                End If
            Next i


        Catch
            Debug.WriteLine(Err.Description)
            MsgBox(Err.Description)
        End Try
    End Sub

    Public Function AddChild(ByVal Child As TreeNode(Of ToDoItem), Parent As TreeNode(Of ToDoItem), IDList As cIDList) As TreeNode(Of ToDoItem)
        Parent.Children.Add(Child)
        Dim strSeed As String
        If Parent.Children.Count > 1 Then
            strSeed = Parent.Children(Parent.Children.Count - 2).Value.ToDoID
        Else
            strSeed = Parent.Value.ToDoID & "00"
        End If
        Child.Value.ToDoID = IDList.GetNextAvailableToDoID(strSeed)
        Child.ID = Child.Value.ToDoID
        If Child.Children.Count > 0 Then
            ReNumberChildrenIDs(Child.Children, IDList)
        End If
    End Function

    Public Sub ReNumberIDs(IDList As cIDList)
        'WriteTreeToDisk()


        For Each RootNode In ToDoTree
            For Each Child In RootNode.Children
                If Child.Children.Count > 0 Then ReNumberChildrenIDs(Child.Children, IDList)
            Next
        Next
        'WriteTreeToDisk()
    End Sub

    Public Sub ReNumberChildrenIDs(Children As List(Of TreeNode(Of ToDoItem)), IDList As cIDList)

        Dim i As Integer
        Dim max As Integer = Children.Count - 1
        If max >= 0 Then
            Dim strParentID As String = Children(i).Parent.ID
            For i = 0 To max
                If IDList.UsedIDList.Contains(Children(i).ID) Then IDList.UsedIDList.Remove(Children(i).ID)
            Next i
            For i = 0 To max
                Children(i).ID = IDList.GetNextAvailableToDoID(strParentID & "00")
                Children(i).Value.ToDoID = Children(i).ID
                If Children(i).Children.Count > 0 Then ReNumberChildrenIDs(Children(i).Children, IDList)
            Next
        End If
    End Sub


    Public Function FindChildByID(ID As String, nodes As List(Of TreeNode(Of ToDoItem))) As TreeNode(Of ToDoItem)
        Dim node As TreeNode(Of ToDoItem)
        Dim rnode As TreeNode(Of ToDoItem)

        For Each node In nodes
            If node.ID = ID Then
                Return node
            Else
                rnode = FindChildByID(ID, node.Children)
                If Not rnode Is Nothing Then
                    Return rnode
                End If
            End If
        Next

        Return Nothing

    End Function
    Public Function GetToDoList(LoadType As LoadOptions) As List(Of Object)
        Dim OlItems As Items
        Dim objView As View
        Dim OlFolder As Folder
        Dim strFilter As String
        Dim oStore As Outlook.Store
        Dim objItem As Object
        Dim ListObjects As List(Of Object) = New List(Of Object)

        objView = Globals.ThisAddIn.Application.ActiveExplorer.CurrentView
        strFilter = "@SQL=" & objView.Filter

        For Each oStore In Globals.ThisAddIn.Application.Session.Stores
            OlItems = Nothing
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            If strFilter = "@SQL=" Or LoadType = LoadOptions.vbLoadAll Then
                OlItems = OlFolder.Items
            Else
                OlItems = OlFolder.Items.Restrict(strFilter)
            End If
            For Each objItem In OlItems
                ListObjects.Add(objItem)
            Next
        Next

        Return ListObjects
    End Function
    Private Function CompareItemsByToDoID(ByVal objItemLeft As Object, ByVal objItemRight As Object)
        Dim ToDoIDLeft As String = Globals.ThisAddIn.CustomFieldID_GetValue(objItemLeft, "ToDoID")
        Dim ToDoIDRight As String = Globals.ThisAddIn.CustomFieldID_GetValue(objItemRight, "ToDoID")
        Dim LngLeft As Long = ConvertToDecimal(125, ToDoIDLeft)
        Dim LngRight As Long = ConvertToDecimal(125, ToDoIDRight)

        If ToDoIDRight.Length = 0 Then
            Return -1
        ElseIf ToDoIDLeft.Length = 0 Then
            Return 1
        ElseIf LngLeft < LngRight Then
            Return -1
        Else
            Return 1
        End If
    End Function
    Private Function MergeSort(Of T)(ByVal coll As IList(Of T), ByVal comparison As Comparison(Of T)) As IList(Of T)
        Dim Result As List(Of T) = New List(Of T)()
        Dim Left As Queue(Of T) = New Queue(Of T)()
        Dim Right As Queue(Of T) = New Queue(Of T)()
        If coll.Count <= 1 Then Return coll
        Dim midpoint As Integer = coll.Count / 2

        For i As Integer = 0 To midpoint - 1
            Left.Enqueue(coll(i))
        Next

        For i As Integer = midpoint To coll.Count - 1
            Right.Enqueue(coll(i))
        Next


        Left = New Queue(Of T)(MergeSort(Left.ToList(), comparison))
        Right = New Queue(Of T)(MergeSort(Right.ToList(), comparison))
        Result = Merge(Left, Right, comparison)
        Return Result
    End Function
    Private Function Merge(Of T)(ByVal Left As Queue(Of T), ByVal Right As Queue(Of T), ByVal comparison As Comparison(Of T)) As List(Of T)
        'Dim cmp As Integer = comparison(coll(i), coll(j))

        Dim Result As List(Of T) = New List(Of T)()

        While Left.Count > 0 AndAlso Right.Count > 0
            Dim cmp As Integer = comparison(Left.Peek(), Right.Peek())
            If cmp < 0 Then
                Result.Add(Left.Dequeue())
            Else
                Result.Add(Right.Dequeue())
            End If
        End While

        While Left.Count > 0
            Result.Add(Left.Dequeue())
        End While

        While Right.Count > 0
            Result.Add(Right.Dequeue())
        End While

        Return Result
    End Function
    Public Sub WriteTreeToDisk()
        Dim filename As String = "C:\Users\03311352\Documents\DebugTreeDump.csv"

        Using sw As StreamWriter = New StreamWriter(filename)
            sw.WriteLine("File Dump")
        End Using

        LoopTreeToWrite(ToDoTree, filename, "")
    End Sub
    Private Sub LoopTreeToWrite(nodes As List(Of TreeNode(Of ToDoItem)), filename As String, lineprefix As String)
        If Not nodes Is Nothing Then
            For Each node As TreeNode(Of ToDoItem) In nodes
                AppendLineToCSV(filename, lineprefix & node.Value.ToDoID & " " & node.Value.TaskSubject)
                LoopTreeToWrite(node.Children, filename, lineprefix & node.Value.ToDoID & ",")
            Next
        End If
    End Sub
    Private Sub AppendLineToCSV(filename As String, line As String)
        Using sw As StreamWriter = File.AppendText(filename)
            sw.WriteLine(line)
        End Using
    End Sub


End Class


Public Class ToDoItem
    Private OlObject As Object
    Private _ToDoID As String = ""
    Public _TaskSubject As String = ""
    Public _MetaTaskSubject As String = ""
    Public _MetaTaskLvl As String = ""
    Private _TagContext As String = ""
    Private _TagProgram As String = ""
    Private _TagProject As String = ""
    Private _TagPeople As String = ""
    Private _TagTopic As String = ""
    Private _Priority As Outlook.OlImportance
    Private _TaskCreateDate As Date
    Private _StartDate As Date
    Private _Complete As Boolean

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
        _StartDate = OlMail.TaskStartDate
        _Complete = (OlMail.FlagStatus = OlFlagStatus.olFlagComplete)
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
        _StartDate = OlTask.StartDate
        _Complete = OlTask.Complete
    End Sub

    Public Sub New(strID As String)
        _ToDoID = strID
    End Sub

    Public ReadOnly Property TaskCreateDate As Date
        Get
            TaskCreateDate = _TaskCreateDate
        End Get
    End Property

    Public Property StartDate As Date
        Get
            Return _TaskCreateDate
        End Get
        Set(value As Date)
            _TaskCreateDate = value
        End Set
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

    Public Property Complete As Boolean
        Get

            If OlObject Is Nothing Then
                _Complete = False
            ElseIf TypeOf OlObject Is MailItem Then
                Dim OlMail As MailItem = OlObject
                _Complete = (OlMail.FlagStatus = OlFlagStatus.olFlagComplete)
            ElseIf TypeOf OlObject Is TaskItem Then
                Dim OlTask As TaskItem = OlObject
                _Complete = OlTask.Complete
            End If
            Return _Complete
        End Get
        Set(value As Boolean)
            _Complete = value
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
                OlTask.Complete = _Complete
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
    Public Property MetaTaskLvl As String
        Get
            If _MetaTaskLvl.Length <> 0 Then
                Return _MetaTaskLvl
            ElseIf OlObject Is Nothing Then
                Return ""
            Else
                _MetaTaskLvl = CustomFieldID_GetValue(OlObject, "Meta Task Level")
                Return _MetaTaskLvl
            End If
        End Get
        Set(strLvl As String)
            _MetaTaskLvl = strLvl
            If Not OlObject Is Nothing Then
                CustomFieldID_Set("Meta Task Level", strLvl, SpecificItem:=OlObject)
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

    Public Function GetItem() As Object
        Return OlObject
    End Function

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

<Serializable()>
Public Class cIDList

    Public UsedIDList As List(Of String)

    Public Sub New(ByVal listUsedID As List(Of String))
        Me.UsedIDList = listUsedID
    End Sub

    Public Sub RePopulate()
        Dim ObjItem As Object = New Object
        Dim DM As DataModel_ToDoTree = New DataModel_ToDoTree
        Dim ToDoList As List(Of Object) = DM.GetToDoList(DataModel_ToDoTree.LoadOptions.vbLoadAll)
        UsedIDList = New List(Of String)
        For Each ObjItem In ToDoList
            Dim strID As String = CustomFieldID_GetValue(ObjItem, "ToDoID")
            If UsedIDList.Contains(strID) = False And strID.Length <> 0 Then UsedIDList.Add(strID)
        Next

        ToDoList = Nothing
        DM = Nothing
    End Sub

    'Public Sub CondenseIDs()
    '    Dim DM As DataModel_ToDoTree = New DataModel_ToDoTree
    '    DM.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadAll)

    '    Dim ToDoTree As List(Of TreeNode(Of ToDoItem)) = DM.ListOfToDoTree
    'End Sub

    Public Function GetNextAvailableToDoID(strSeed As String) As String
        Dim blContinue As Boolean = True
        Dim lngMaxID As Long = ConvertToDecimal(125, strSeed)
        Dim strMaxID As String = ""

        While blContinue
            lngMaxID += 1
            strMaxID = ConvertToBase(125, lngMaxID)
            If UsedIDList.Contains(strMaxID) = False Then
                blContinue = False
            End If
        End While
        UsedIDList.Add(strMaxID)
        Return strMaxID
    End Function

    Public Function GetMaxToDoID() As String
        Dim strMaxID = UsedIDList.Max()
        Dim lngMaxID As Long = ConvertToDecimal(125, strMaxID)
        lngMaxID += 1
        strMaxID = ConvertToBase(125, lngMaxID)
        UsedIDList.Add(strMaxID)

        Return strMaxID
    End Function

    Public Sub Save(FileName_IDList As String)
        If Not Directory.Exists(Path.GetDirectoryName(FileName_IDList)) Then
            Directory.CreateDirectory(Path.GetDirectoryName(FileName_IDList))
        End If
        Dim TestFileStream As Stream = File.Create(FileName_IDList)
        Dim serializer As New BinaryFormatter
        serializer.Serialize(TestFileStream, Me)
        TestFileStream.Close()
    End Sub

    Public Function ConvertToBase(nbase As Integer, ByVal num As Long, Optional intMinDigits As Integer = 2) As String
        Dim chars As String
        Dim r As Long
        Dim newNumber As String
        Dim maxBase As Integer
        Dim i As Integer

        chars = "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
        maxBase = Len(chars)

        ' check if we can convert to this base
        If (nbase > maxBase) Then
            ConvertToBase = ""
        Else

            ' in r we have the offset of the char that was converted to the new base
            newNumber = ""
            While num >= nbase
                r = num Mod nbase
                newNumber = Mid(chars, r + 1, 1) & newNumber
                num \= nbase
            End While

            newNumber = Mid(chars, num + 1, 1) & newNumber

            For i = 1 To (Len(newNumber) Mod intMinDigits)
                newNumber = CStr(0) & newNumber
            Next i

            ConvertToBase = newNumber
        End If
    End Function

    Public Function ConvertToDecimal(nbase As Integer, ByVal strBase As String) As Long
        Dim chars As String
        Dim i As Integer
        Dim intLoc As Integer
        Dim lngTmp As Long

        chars = "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
        lngTmp = 0

        For i = 1 To Len(strBase)
            lngTmp *= nbase
            intLoc = InStr(chars, Mid(strBase, i, 1))
            lngTmp += intLoc - 1
        Next i

        ConvertToDecimal = lngTmp
    End Function

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

    Public Function FlattenArry(varBranch() As Object) As String
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