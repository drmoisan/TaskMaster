Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

Public Class FileOperationsPST
    Private ReadOnly _globals As IApplicationGlobals
    Private ReadOnly _emailFolderPST As Outlook.Folder
    Private ReadOnly _handlerList As List(Of PSTEvents)

    Public Sub New(appGlobals As IApplicationGlobals, EmailFolderpathPST As String)
        _globals = appGlobals
        _emailFolderPST = GetOutlookPSTFolderByPath(EmailFolderpathPST, _globals.Ol.App)
        _handlerList = InstantiateHandlers()
    End Sub

    Private Function GetOutlookPSTFolderByPath(ByVal FolderPath As String,
                                              Application As Outlook.Application) As Outlook.Folder
        If Left(FolderPath, 2) = "\\" Then
            FolderPath = Right(FolderPath, Len(FolderPath) - 2)
        End If
        Dim FoldersArray() As String = FolderPath.Split("\")

        Try
            Dim OlFolder As Outlook.Folder = Application.Session.Folders(FoldersArray(0))
            Dim OlFolders As Outlook.Folders = OlFolder.Folders
            Debug.WriteLine(OlFolder.FolderPath & " has " & OlFolders.Count.ToString & " folders")
            For Each OlFolder In OlFolders
                Debug.WriteLine(OlFolder.FolderPath)
            Next

            For i As Integer = 1 To UBound(FoldersArray)
                OlFolder = OlFolder.Folders(FoldersArray(i))
            Next
            Return OlFolder
        Catch
            Debug.WriteLine(Err.Description)
            Debug.WriteLine("Folder Does Not Exist")
            Return Nothing
        End Try

    End Function

    Public Sub HookEvents()
        For Each handler As PSTEvents In _handlerList
            handler.HookEvents()
        Next
    End Sub

    Public Sub UnHookEvents()
        For Each handler As PSTEvents In _handlerList
            handler.UnHookEvents()
        Next
    End Sub

    Private Function InstantiateHandlers() As List(Of PSTEvents)
        Dim olSession As Outlook.NameSpace = _globals.Ol.App.Session
        Dim stores As Outlook.Stores = olSession.Stores
        Dim handlerList As New List(Of PSTEvents)

        For Each store As Outlook.Store In stores
            If Right(store.FilePath, 3) = "pst" Then
                Dim OlFolder As Outlook.Folder = GetSearchFolder(store, "FLAGGED")
                Dim items As Outlook.Items = OlFolder.Items
                Dim handlerPST As New PSTEvents(store, items, _globals)
                handlerList.Add(handlerPST)
            End If
        Next

        Return handlerList
    End Function

    Private Class PSTEvents
        Private WithEvents _itemsPST As Outlook.Items
        Private ReadOnly _store As Outlook.Store
        Private ReadOnly _globals As IApplicationGlobals

        Public Sub New(Store As Outlook.Store, ItemsPST As Outlook.Items, Globals As IApplicationGlobals)
            _itemsPST = ItemsPST
            _globals = Globals
        End Sub

        Public Sub HookEvents()
            AddHandler _itemsPST.ItemChange, AddressOf _itemsPST_ItemChange
            AddHandler _itemsPST.ItemAdd, AddressOf _itemsPST_ItemChange
        End Sub

        Public Sub UnHookEvents()
            RemoveHandler _itemsPST.ItemChange, AddressOf _itemsPST_ItemChange
            RemoveHandler _itemsPST.ItemAdd, AddressOf _itemsPST_ItemChange
        End Sub

        Private Sub _itemsPST_ItemChange(Item As Object)
            'Handles _itemsPST.ItemChange

            Static blIsRunning As Boolean
            'TODO: Morph Functionality to handle proactively rather than reactively
            If blIsRunning = False Then

                blIsRunning = True
                Dim todo As New ToDoItem(Item, OnDemand:=True)
                Dim objProperty_ToDoID As Outlook.UserProperty = Item.UserProperties.Find("ToDoID")
                Dim objProperty_Project As Outlook.UserProperty = Item.UserProperties.Find("TagProject")


                'AUTOCODE ToDoID based on Project
                'Check to see if the project exists before attempting to autocode the id
                If objProperty_Project IsNot Nothing Then

                    Dim strProject As String
                    Dim strProjectToDo As String
                    'Check to see whether there is an existing ID
                    If objProperty_ToDoID IsNot Nothing Then
                        Dim strToDoID As String = objProperty_ToDoID.Value

                        'Don't autocode branches that existed to another project previously
                        If strToDoID.Length <> 0 And strToDoID.Length <= 4 Then

                            'Get Project Name
                            strProject = todo.Project

                            'If IsArray(objProperty_Project.Value) Then
                            '    strProject = FlattenArry(objProperty_Project.Value)
                            'Else
                            '    strProject = objProperty_Project.Value
                            'End If

                            'Check to see if the Project name returned a value before attempting to autocode
                            If strProject.Length <> 0 Then

                                'Check to ensure it is in the dictionary before autocoding
                                If _globals.TD.ProjInfo.Contains_ProjectName(strProject) Then
                                    'If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                    'strProjectToDo = ProjDict.ProjectDictionary(strProject)

                                    If strToDoID.Length = 2 Then
                                        ' Change the Item's todoid to be a node of the project
                                        If todo.Context <> "Tag PROJECTS" Then
                                            strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                                            todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName
                                            todo.ToDoID = _globals.TD.IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                            'strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                            'CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                            _globals.TD.IDList.Save(_globals.TD.FnameIDList)
                                            'Split_ToDoID(objItem:=Item)
                                            todo.SplitID()
                                        End If
                                    End If


                                Else 'If it is not in the dictionary, see if this is a project we should add
                                    If strToDoID.Length = 4 Then
                                        Dim response As MsgBoxResult = MsgBox("Add Project " & strProject & " to the Master List?", vbYesNo)
                                        If response = vbYes Then
                                            'ProjDict.ProjectDictionary.Add(strProject, strToDoID)
                                            'SaveDict()
                                            Dim strProgram As String = InputBox("What is the program name for " & strProject & "?", DefaultResponse:="")
                                            Dim unused2 = _globals.TD.ProjInfo.Add(New ToDoProjectInfoEntry(strProject, strToDoID, strProgram))
                                            _globals.TD.ProjInfo.Save()
                                        End If
                                    End If
                                End If
                            End If

                        ElseIf strToDoID.Length = 0 Then
                            strProject = todo.Project
                            'If IsArray(objProperty_Project.Value) Then
                            '    strProject = FlattenArry(objProperty_Project.Value)
                            'Else
                            '    strProject = objProperty_Project.Value
                            'End If
                            If _globals.TD.ProjInfo.Contains_ProjectName(strProject) Then
                                strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                                todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName
                                'If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                'strProjectToDo = ProjDict.ProjectDictionary(strProject)
                                todo.ToDoID = _globals.TD.IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                'strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                'CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                _globals.TD.IDList.Save(_globals.TD.FnameIDList)
                                'Split_ToDoID(objItem:=Item)
                                todo.SplitID()
                            End If

                        End If
                    Else 'In this case, the project name exists but the todo id does not
                        'Get Project Name
                        strProject = If(IsArray(objProperty_Project.Value), FlattenArry(objProperty_Project.Value), DirectCast(objProperty_Project.Value, String))

                        'If the project name is in our dictionary, autoadd the ToDoID to this item
                        If strProject.Length <> 0 Then
                            'If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                            If _globals.TD.ProjInfo.Contains_ProjectName(strProject) Then
                                'strProjectToDo = ProjDict.ProjectDictionary(strProject)
                                strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                                'Add the next ToDoID available in that branch
                                todo.ToDoID = _globals.TD.IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName
                                'strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                'CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                _globals.TD.IDList.Save(_globals.TD.FnameIDList)
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
                        Dim unused1 = Item.Save
                        todo.KB = "Completed"
                    End If
                ElseIf todo.KB = "Completed" Then
                    Dim strCats As String = Item.Categories

                    'Strip Completed from categories
                    If InStr(strCats, "Tag KB Completed") = True Then
                        strCats = Replace(Replace(strCats, "Tag KB Completed", ""), ",,", ",")
                    End If

                    Dim strReplace As String
                    Dim strKB As String
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
                    Dim unused = Item.Save
                    todo.KB = strKB

                End If
                blIsRunning = False
            End If


        End Sub
    End Class

    Private Function GetSearchFolder(store As Outlook.Store, name As String) As Folder
        Try
            Dim searchfolders As Folders = store.GetSearchFolders
            For Each OlFolder As Folder In searchfolders
                Debug.WriteLine(OlFolder.Name)
                If OlFolder.Name = name Then
                    Return OlFolder
                End If
            Next
            Return Nothing
        Catch
            Debug.WriteLine(Err.Description)
            Return Nothing
        End Try
    End Function


End Class
