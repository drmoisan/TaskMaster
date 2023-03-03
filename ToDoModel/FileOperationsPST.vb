Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

Public Module FileOperationsPST
    Public Function GetOutlookPSTFolderByPath(ByVal FolderPath As String,
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

    Private Sub ToDoPST_HookEvents(Application As Outlook.Application, listToDoItems As List(Of Outlook.Items))
        Dim ns As Outlook.[NameSpace] = Nothing
        Dim stores As Outlook.Stores = Nothing
        Dim store As Outlook.Store = Nothing



        ns = Application.Session
        stores = ns.Stores

        For i As Integer = 1 To stores.Count
            store = stores(i)
            If Right(store.FilePath, 3) = "pst" Then
                'Dim OlFolder As Outlook.Folder = GetOutlookPSTFolderByPath(store.GetRootFolder().FolderPath + "\search folders\FLAGGED")
                Dim OlFolder As Outlook.Folder = GetSearchFolder(store, "FLAGGED")
                Dim items As Outlook.Items = OlFolder.Items
                'PSTtoDo = OlFolder.Items
                AddHandler items.ItemChange, AddressOf H_ItemChange
                AddHandler items.ItemAdd, AddressOf H_ItemChange
                listToDoItems.Add(items)
                'storeList += String.Format("{0} - {1}{2}", store.DisplayName, (If(store.IsDataFileStore, ".pst", ".ost")), Environment.NewLine)
            End If
        Next
    End Sub

    Private Sub ToDoPST_UnHookEvents()

        Dim max As Integer = listToDoItems.Count
        For i As Integer = max To 1 Step -1
            Dim items As Outlook.Items = listToDoItems.Item(i)
            RemoveHandler items.ItemChange, AddressOf OlToDoItems_ItemChange
            RemoveHandler items.ItemAdd, AddressOf OlToDoItems_ItemAdd
            listToDoItems.Remove(items)
        Next

    End Sub

    Private Class PSTEvents
        Private WithEvents _itemsPST As Outlook.Items
        Private _store As Outlook.Store
        Private _globals As IApplicationGlobals

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
                            strProject = todo.Project

                            'If IsArray(objProperty_Project.Value) Then
                            '    strProject = FlattenArry(objProperty_Project.Value)
                            'Else
                            '    strProject = objProperty_Project.Value
                            'End If

                            'Check to see if the Project name returned a value before attempting to autocode
                            If strProject.Length <> 0 Then

                                'Check to ensure it is in the dictionary before autocoding
                                If _globals.ToDo.ProjInfo.Contains_ProjectName(strProject) Then
                                    'If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                    'strProjectToDo = ProjDict.ProjectDictionary(strProject)

                                    If strToDoID.Length = 2 Then
                                        ' Change the Item's todoid to be a node of the project
                                        If todo.Context <> "Tag PROJECTS" Then
                                            strProjectToDo = _globals.ToDo.ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                                            todo.TagProgram = _globals.ToDo.ProjInfo.Find_ByProjectName(strProject).First().ProgramName
                                            todo.ToDoID = _globals.ToDo.IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                            'strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                            'CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                            _globals.ToDo.IDList.Save(FileName_IDList)
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
                                            ProjInfo.Add(New ToDoProjectInfoEntry(strProject, strToDoID, strProgram))
                                            ProjInfo.Save()
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
                            If ProjInfo.Contains_ProjectName(strProject) Then
                                strProjectToDo = ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                                todo.TagProgram = ProjInfo.Find_ByProjectName(strProject).First().ProgramName
                                'If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                'strProjectToDo = ProjDict.ProjectDictionary(strProject)
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
                            'If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                            If ProjInfo.Contains_ProjectName(strProject) Then
                                'strProjectToDo = ProjDict.ProjectDictionary(strProject)
                                strProjectToDo = ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                                'Add the next ToDoID available in that branch
                                todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                todo.TagProgram = ProjInfo.Find_ByProjectName(strProject).First().ProgramName
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


End Module
