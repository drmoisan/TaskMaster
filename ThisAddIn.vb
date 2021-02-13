﻿Option Explicit On
Imports Microsoft.Office.Interop.Outlook
Imports System.Diagnostics
Imports System.IO
Imports System
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Security.Authentication.ExtendedProtection


Public Class ThisAddIn

    Public WithEvents OlToDoItems As Outlook.Items
    Public WithEvents OlInboxItems As Outlook.Items
    Private WithEvents OlReminders As Outlook.Reminders


    Private ribTM As TaskMasterRibbon
    Dim FileName_ProjectList As String
    Dim FileName_IDList As String
    Const AppDataFolder = "TaskMaster"
    Public WithEvents ProjDict As ProjectList
    Public WithEvents IDList As cIDList

    Private Sub ThisAddIn_Startup() Handles Me.Startup
        OlToDoItems = Application.GetNamespace("MAPI").GetDefaultFolder(OlDefaultFolders.olFolderToDo).Items
        OlInboxItems = Application.GetNamespace("MAPI").GetDefaultFolder(OlDefaultFolders.olFolderInbox).Items
        OlReminders = Application.Reminders

        FileName_ProjectList = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolder, "ProjectList.bin")
        If File.Exists(FileName_ProjectList) Then
            Dim TestFileStream As Stream = File.OpenRead(FileName_ProjectList)
            Dim deserializer As New BinaryFormatter
            ProjDict = CType(deserializer.Deserialize(TestFileStream), ProjectList)
            TestFileStream.Close()
        Else
            ProjDict = New ProjectList(New Dictionary(Of String, String))
        End If

        FileName_IDList = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolder, "UsedIDList.bin")

        If File.Exists(FileName_IDList) Then
            Dim TestFileStream As Stream = File.OpenRead(FileName_IDList)
            Dim deserializer As New BinaryFormatter
            IDList = CType(deserializer.Deserialize(TestFileStream), cIDList)
            TestFileStream.Close()
        Else
            IDList = New cIDList(New List(Of String))
            IDList.RePopulate()
            IDList.Save(FileName_IDList)
            'Save_IDList()
        End If

        Access_Ribbons_By_Explorer()
    End Sub

    Private Sub Access_Ribbons_By_Explorer()
        Dim ribbonCollection As ThisRibbonCollection = Globals.Ribbons _
            (Globals.ThisAddIn.Application.ActiveExplorer())
        ribTM = ribbonCollection.Ribbon1                                    'Grab handle on on Ribbon

    End Sub



    Public Function RefreshIDList() As Long
        IDList = New cIDList(New List(Of String))
        IDList.RePopulate()
        IDList.Save(FileName_IDList)
        WriteToCSV("C:\Users\03311352\Documents\UsedIDList.csv", IDList.UsedIDList.ToArray)
    End Function



    Public Sub WriteToCSV(filename As String, strOutput() As String)
        If IO.File.Exists(filename) Then IO.File.Delete(filename)
        Using sw As StreamWriter = New StreamWriter(filename)
            For i As Long = LBound(strOutput) To UBound(strOutput)
                sw.WriteLine(strOutput(i))
            Next
        End Using
    End Sub
    Public Sub WriteToCSV(filename As String, strOutput As String)
        If IO.File.Exists(filename) Then IO.File.Delete(filename)
        Using sw As StreamWriter = New StreamWriter(filename)
            sw.WriteLine(strOutput)
        End Using
    End Sub

    Public Sub CompressToDoIDs()
        Dim DM As DataModel_ToDoTree = New DataModel_ToDoTree()
        DM.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadAll)
        'DM.WriteTreeToDisk()
        DM.ReNumberIDs(IDList)
        DM.WriteTreeToDisk()
    End Sub


    Public Function CustomFieldID_GetValue(objItem As Object, ByVal UserDefinedFieldName As String) As String
        Dim OlMail As Outlook.MailItem
        Dim OlTask As Outlook.TaskItem
        Dim OlAppt As Outlook.AppointmentItem
        Dim objProperty As Outlook.UserProperty


        If TypeOf objItem Is Outlook.MailItem Then
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
            CustomFieldID_GetValue = ""
        Else
            If IsArray(objProperty.Value) Then
                CustomFieldID_GetValue = FlattenArry(objProperty.Value)
            Else
                CustomFieldID_GetValue = objProperty.Value
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

    Public Function CustomFieldID_Set(ByVal UserDefinedFieldName As String,
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

    Public Function GetListOfItemsInView_ToDo() As List(Of Object)
        Dim OlItems As Items
        Dim objView As View
        Dim OlFolder As Folder
        Dim strFilter As String
        Dim oStore As Outlook.Store
        Dim objItem As Object


        objView = Application.ActiveExplorer.CurrentView
        strFilter = "@SQL=" & objView.Filter

        OlItems = Nothing
        For Each oStore In Application.Session.Stores
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            If strFilter = "@SQL=" Then
                OlItems = OlFolder.Items
            Else
                OlItems = OlFolder.Items.Restrict(strFilter)
            End If
        Next
        Dim ListObjects As List(Of Object) = New List(Of Object)
        For Each objItem In OlItems
            ListObjects.Add(objItem)
        Next
        'GetItemsInView_ToDo = OlItems
        Return ListObjects
    End Function
    Public Function GetItemsInView_ToDo() As Items
        Dim OlItems As Items
        Dim objView As View
        Dim OlFolder As Folder
        Dim strFilter As String
        Dim oStore As Outlook.Store

        objView = Application.ActiveExplorer.CurrentView
        strFilter = "@SQL=" & objView.Filter

        OlItems = Nothing
        For Each oStore In Application.Session.Stores
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            If strFilter = "@SQL=" Then
                OlItems = OlFolder.Items
            Else
                OlItems = OlFolder.Items.Restrict(strFilter)
            End If
        Next
        GetItemsInView_ToDo = OlItems
    End Function

    Public Function IsChild(strParent As String, strChild As String) As Integer
        Dim i As Integer = 0
        Dim count As Integer = 0
        Dim unbroken As Boolean = True
        For i = 1 To strParent.Length / 2
            If unbroken Then
                If Mid(strParent, i * 2 - 1, 2) = Mid(strChild, i * 2 - 1, 2) Then
                    count = i
                Else
                    unbroken = False
                End If
            End If
        Next
        IsChild = count
    End Function
    Public Function FindParent(itms As Collection, strChild As String) As Object
        Dim strParent As String

        Try
            strParent = Left(strChild, strChild.Length - 2)
            FindParent = itms(strParent)
        Catch
            FindParent = Nothing
            Err.Clear()
        End Try

    End Function
    Public Sub Refresh_ToDoID_Splits()
        Dim objItem As Object
        Dim todo As ToDoItem
        Dim OlItems As Items = GetItemsInView_ToDo()
        For Each objItem In OlItems
            todo = New ToDoItem(objItem, OnDemand:=True)
            todo.SplitID()
        Next
    End Sub



    Private Sub OlToDoItems_ItemChange(Item As Object) Handles OlToDoItems.ItemChange
        Static blIsRunning As Boolean

        If blIsRunning = False Then

            blIsRunning = True
            Dim todo As ToDoItem = New ToDoItem(Item, OnDemand:=True)
            Dim objProperty_ToDoID As Outlook.UserProperty = Item.UserProperties.Find("ToDoID")
            Dim objProperty_Project As Outlook.UserProperty = Item.UserProperties.Find("TagProject")
            Dim strToDoID As String = ""
            Dim strToDoID_root As String = ""
            Dim strProject As String = ""
            Dim strProjectToDo As String = ""


            'AUTOCODE ToDoID based on Project
            'Check to see if the project exists before attempting to autocode the id
            If Not objProperty_Project Is Nothing Then

                'Check to see whether there is an existing ID
                If Not objProperty_ToDoID Is Nothing Then
                    strToDoID = objProperty_ToDoID.Value

                    'Don't autocode branches that existed to another project previously
                    If strToDoID.Length <> 0 And strToDoID.Length <= 4 Then

                        'Get Project Name
                        strProject = todo.TagProject

                        'If IsArray(objProperty_Project.Value) Then
                        '    strProject = FlattenArry(objProperty_Project.Value)
                        'Else
                        '    strProject = objProperty_Project.Value
                        'End If

                        'Check to see if the Project name returned a value before attempting to autocode
                        If strProject.Length <> 0 Then

                            'Check to ensure it is in the dictionary before autocoding
                            If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                strProjectToDo = ProjDict.ProjectDictionary(strProject)
                                If strToDoID.Length = 2 Then
                                    ' Change the Item's todoid to be a node of the project

                                    todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                    'strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                    'CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                    IDList.Save(FileName_IDList)
                                    'Split_ToDoID(objItem:=Item)
                                    todo.SplitID()
                                End If


                            Else 'If it is not in the dictionary, see if this is a project we should add
                                If strToDoID.Length = 4 Then
                                    Dim response As MsgBoxResult = MsgBox("Add Project " & strProject & " to the Master List?", vbYesNo)
                                    If response = vbYes Then
                                        ProjDict.ProjectDictionary.Add(strProject, strToDoID)
                                        SaveDict()
                                    End If
                                End If
                            End If
                        End If

                    ElseIf strToDoID.Length = 0 Then
                        strProject = todo.TagProject
                        'If IsArray(objProperty_Project.Value) Then
                        '    strProject = FlattenArry(objProperty_Project.Value)
                        'Else
                        '    strProject = objProperty_Project.Value
                        'End If
                        If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                            strProjectToDo = ProjDict.ProjectDictionary(strProject)
                            todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                            'strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                            'CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                            IDList.Save(FileName_IDList)
                            'Split_ToDoID(objItem:=Item)
                            todo.SplitID()
                        End If

                    End If
                Else 'In this case, the project name exists but the todo id does not
                    'Get Project Name
                    If IsArray(objProperty_Project.Value) Then
                        strProject = FlattenArry(objProperty_Project.Value)
                    Else
                        strProject = objProperty_Project.Value
                    End If

                    'If the project name is in our dictionary, autoadd the ToDoID to this item
                    If strProject.Length <> 0 Then
                        If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                            strProjectToDo = ProjDict.ProjectDictionary(strProject)
                            'Add the next ToDoID available in that branch
                            todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                            'strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                            'CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                            IDList.Save(FileName_IDList)
                            'Split_ToDoID(objItem:=Item)
                            todo.SplitID()
                            '***NEED CODE HERE***
                            '***NEED CODE HERE***
                            '***NEED CODE HERE***
                        End If
                    End If
                End If


            End If

            'If OlToDoItem_IsMarkedComplete(Item) Then
            'Check to see if todo was just marked complete 
            'If So, adjust Kan Ban fields and categories
            If todo.Complete Then
                If InStr(Item.Categories, "Tag KB Completed") = False Then
                    Dim strCats As String = Replace(Replace(Item.Categories, "Tag KB Backlog", ""), ",,", ",")
                    strCats = Replace(Replace(strCats, "Tag KB InProgress", ""), ",,", ",")
                    strCats = Replace(Replace(strCats, "Tag KB Planned", ""), ",,", ",")
                    While Left(strCats, 1) = ","
                        strCats = Right(strCats, strCats.Length - 1)
                    End While
                    If strCats.Length > 0 Then
                        strCats += ", Tag KB Completed"
                    Else
                        strCats += "Tag KB Completed"
                    End If
                    Item.Categories = strCats
                    Item.Save
                    todo.KB = "Completed"
                End If
            ElseIf todo.KB = "Completed" Then
                Dim strCats As String = Item.Categories

                'Strip Completed from categories
                If InStr(strCats, "Tag KB Completed") = True Then
                    strCats = Replace(Replace(strCats, "Tag KB Completed", ""), ",,", ",")
                End If
                Dim strReplace As String = ""
                Dim strKB As String = ""

                If InStr(strCats, "Tag A Top Priority Today") = True Then
                    strReplace = "Tag KB InProgress"
                    strKB = "InProgress"
                ElseIf InStr(strCats, "Tag Bullpin Priorities") = True Then
                    strReplace = "Tag KB Planned"
                    strKB = "Planned"
                Else
                    strReplace = "Tag KB Backlog"
                    strKB = "Backlog"
                End If
                If strCats.Length > 0 Then
                    strCats += ", " & strReplace
                Else
                    strCats = strReplace
                End If
                Item.Categories = strCats
                Item.Save
                todo.KB = strKB

            End If
            blIsRunning = False
        End If

        ''AUTOCODE ToDoID based on Project
        ''Check to see if the project exists before attempting to autocode the id
        'If Not objProperty_Project Is Nothing Then

        '    'Check to see whether there is an existing ID
        '    If Not objProperty_ToDoID Is Nothing Then
        '        strToDoID = objProperty_ToDoID.Value

        '        'Don't autocode branches that existed to another project previously
        '        If strToDoID.Length <> 0 And strToDoID.Length <= 4 Then

        '            'Get Project Name
        '            If IsArray(objProperty_Project.Value) Then
        '                strProject = FlattenArry(objProperty_Project.Value)
        '            Else
        '                strProject = objProperty_Project.Value
        '            End If

        '            'Check to see if the Project name returned a value before attempting to autocode
        '            If strProject.Length <> 0 Then

        '                'Check to ensure it is in the dictionary before autocoding
        '                If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
        '                    strProjectToDo = ProjDict.ProjectDictionary(strProject)
        '                    If strToDoID.Length = 2 Then
        '                        ' Change the Item's todoid to be a node of the project

        '                        strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
        '                        CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
        '                        IDList.Save(FileName_IDList)
        '                        Split_ToDoID(objItem:=Item)
        '                    End If


        '                Else 'If it is not in the dictionary, see if this is a project we should add
        '                    If strToDoID.Length = 4 Then
        '                        Dim response As MsgBoxResult = MsgBox("Add Project " & strProject & " to the Master List?", vbYesNo)
        '                        If response = vbYes Then
        '                            ProjDict.ProjectDictionary.Add(strProject, strToDoID)
        '                            SaveDict()
        '                        End If
        '                    End If
        '                End If
        '            End If

        '        ElseIf strToDoID.Length = 0 Then
        '            If IsArray(objProperty_Project.Value) Then
        '                strProject = FlattenArry(objProperty_Project.Value)
        '            Else
        '                strProject = objProperty_Project.Value
        '            End If
        '            If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
        '                strProjectToDo = ProjDict.ProjectDictionary(strProject)
        '                strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
        '                CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
        '                IDList.Save(FileName_IDList)
        '                Split_ToDoID(objItem:=Item)
        '            End If

        '        End If
        '    Else 'In this case, the project name exists but the todo id does not
        '        'Get Project Name
        '        If IsArray(objProperty_Project.Value) Then
        '            strProject = FlattenArry(objProperty_Project.Value)
        '        Else
        '            strProject = objProperty_Project.Value
        '        End If

        '        'If the project name is in our dictionary, autoadd the ToDoID to this item
        '        If strProject.Length <> 0 Then
        '            If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
        '                strProjectToDo = ProjDict.ProjectDictionary(strProject)
        '                'Add the next ToDoID available in that branch
        '                strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
        '                CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
        '                IDList.Save(FileName_IDList)
        '                Split_ToDoID(objItem:=Item)
        '                '***NEED CODE HERE***
        '                '***NEED CODE HERE***
        '                '***NEED CODE HERE***
        '            End If
        '        End If
        '    End If


        'End If



    End Sub

    Private Function OlToDoItem_IsMarkedComplete(Item As Object) As Boolean
        If TypeOf Item Is Outlook.MailItem Then
            Dim OlMail = Item
            If OlMail.FlagStatus = OlFlagStatus.olFlagComplete Then
                Return True
            Else
                Return False
            End If
        ElseIf TypeOf Item Is TaskItem Then
            Dim OlTask = Item
            If OlTask.Complete = True Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If

    End Function


    Public Sub SaveDict()
        If Not Directory.Exists(Path.GetDirectoryName(FileName_ProjectList)) Then
            Directory.CreateDirectory(Path.GetDirectoryName(FileName_ProjectList))
        End If
        Dim TestFileStream As Stream = File.Create(FileName_ProjectList)
        Dim serializer As New BinaryFormatter
        serializer.Serialize(TestFileStream, ProjDict)
        TestFileStream.Close()
    End Sub



    Private Sub OlToDoItems_ItemAdd(Item As Object) Handles OlToDoItems.ItemAdd
        Dim strToDoID As String = CustomFieldID_GetValue(Item, "ToDoID")
        If strToDoID.Length = 0 Then
            strToDoID = IDList.GetMaxToDoID
            CustomFieldID_Set(Item, "ToDoID")
        End If

    End Sub
End Class

Public Class Conditions
    Private _ConversationID As String
    Private _People As String
    Public Sub New()

    End Sub
    Public Sub New(objItem As Object)
        If TypeOf objItem Is MailItem Then
            Dim OlMail As MailItem = objItem
            _ConversationID = OlMail.ConversationID
            _People = Globals.ThisAddIn.CustomFieldID_GetValue(objItem, "TagPeople")
        End If
    End Sub

    Public Property ConversationID
        Get
            ConversationID = _ConversationID
        End Get
        Set(value)
            _ConversationID = value
        End Set
    End Property
    Public Property People
        Get
            People = _People
        End Get
        Set(value)
            _People = value
        End Set
    End Property
End Class