Option Explicit On
Imports System.Diagnostics
Imports System.IO
Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.VisualBasic.FileIO
Imports ToDoModel
Imports UtilitiesVB

Public Class ThisAddIn

    Public _globals As ApplicationGlobals

    Public CCOCatList As List(Of String)
    Public WithEvents OlToDoItems As Outlook.Items
    Public WithEvents ListOfPSTtodo As New List(Of Outlook.Items)
    Public WithEvents ListToDoItems As New List(Of Outlook.Items)
    Public WithEvents OlInboxItems As Outlook.Items
    Private WithEvents OlReminders As Outlook.Reminders
    Public OlNS As Outlook.NameSpace
    'Private WithEvents OlExplorer As Outlook.Explorer

    Private _ribTM As TaskMasterRibbon
    Private _ribEM As EmailRibbon
    Private ReadOnly _filenameProjectList As String
    Private ReadOnly _filenameIDList As String
    Private ReadOnly _filenameProjInfo2 As String
    Private ReadOnly _filenameProjInfo As String = "ProjInfo.bin"
    Public ReadOnly FilenameDictPpl As String = "pplkey.xml"
    Public ReadOnly StagingPath As String = SpecialDirectories.MyDocuments
    Public EmailRoot As String
    Private Const _appDataFolder = "TaskMaster"
    'Public ProjDict As ProjectList
    Public ProjInfo As ProjectInfo
    Public DictPPL As Dictionary(Of String, String)
    Public WithEvents IDList As ListOfIDs
    Public DM_CurView As DataModel_ToDoTree
    Public Cats As FlagParser

    Private Sub ThisAddIn_Startup() Handles Me.Startup
        _globals = New ApplicationGlobals(Application)

        With _globals
            OlNS = .Ol.NamespaceMAPI
            OlToDoItems = .Ol.ToDoFolder.Items
            OlInboxItems = .Ol.Inbox.Items
            OlReminders = .Ol.OlReminders
            ProjInfo = .ToDo.ProjInfo
            DictPPL = .ToDo.DictPPL
            IDList = .ToDo.IDList
            EmailRoot = .Ol.EmailRootPath

        End With
        Access_Ribbons_By_Explorer()
    End Sub

    Public Sub Events_Hook()
        'Debug_OutputNsStores()
        With _globals
            OlToDoItems = .Ol.ToDoFolder.Items
            OlInboxItems = .Ol.Inbox.Items
            OlReminders = .Ol.OlReminders
        End With

    End Sub

    Public Sub Events_Unhook()
        OlToDoItems = Nothing
        OlInboxItems = Nothing
        OlReminders = Nothing
    End Sub

    Private Sub Access_Ribbons_By_Explorer()
        Dim ribbonCollection As ThisRibbonCollection = Globals.Ribbons _
            (Globals.ThisAddIn.Application.ActiveExplorer())
        _ribTM = ribbonCollection.Ribbon_TM
        _ribEM = ribbonCollection.Ribbon_EM
    End Sub

    Private Sub Debug_OutputNsStores()
        Dim storeList As String = String.Empty

        Dim ns As [NameSpace] = Application.Session
        Dim stores As Stores = ns.Stores

        For i As Integer = 1 To stores.Count
            Dim store As Store = stores(i)
            If Right(store.FilePath, 3) = "pst" Then
                Dim fldrtmp As Folder = store.GetRootFolder()

                Debug.WriteLine(fldrtmp.FolderPath)
                Dim fldrs As Outlook.Folders = store.GetSearchFolders
                For Each fldr As Outlook.Folder In fldrs
                    Debug.WriteLine(fldr.FolderPath)
                    '\\03 LATAM CCO\search folders\FLAGGED
                Next
                'Dim fldr As Outlook.Folder = store.GetSearchFolders.
                'Dim items As Outlook.Items
                'storeList += String.Format("{0} - {1}{2}", store.DisplayName, (If(store.IsDataFileStore, ".pst", ".ost")), Environment.NewLine)
            End If
        Next

        Debug.WriteLine(storeList)

    End Sub

    Public Function RefreshIDList() As Long
        IDList = New ListOfIDs(New List(Of String))
        IDList.RePopulate(Application)
        IDList.Save(_filenameIDList)
        WriteToCSV("C:\Users\03311352\Documents\UsedIDList.csv", IDList.UsedIDList.ToArray)
        Return 1
    End Function

    Public Sub WriteToCSV(filename As String, strOutput() As String, Optional overwrite As Boolean = False)
        'CLEANUP: Determine if ThisAddIn.WriteToCSV function is needed. If so, move it to a library
        If overwrite Or IO.File.Exists(filename) = False Then
            Using sw As New StreamWriter(filename)
                For i As Long = LBound(strOutput) To UBound(strOutput)
                    sw.WriteLine(strOutput(i))
                Next
            End Using
        Else
            Using sw As New StreamWriter(filename, append:=True)
                For i As Long = LBound(strOutput) To UBound(strOutput)
                    sw.WriteLine(strOutput(i))
                Next
            End Using
        End If

    End Sub

    Public Sub WriteToCSV(filename As String, strOutput As String, Optional overwrite As Boolean = False)
        'CLEANUP: Determine if ThisAddIn.WriteToCSV function is needed. If so, move it to a library
        If overwrite Or IO.File.Exists(filename) = False Then
            Using sw As New StreamWriter(filename)
                sw.WriteLine(strOutput)
            End Using
        Else
            Using sw As New StreamWriter(filename, append:=True)
                sw.WriteLine(strOutput)
            End Using
        End If

    End Sub

    Public Sub CompressToDoIDs()
        'DOC: Add documentation to CompressToDoIDs
        'TESTING: Add integration testing for CompressToDoIDs
        'CLEANUP: Move CompressToDoIDs to either a Module or include in ToDoTree DataModel
        Dim DM As New DataModel_ToDoTree()
        'QUESTION: Does DataModel_ToDoTree.LoadOptions.vbLoadAll require all items to be visible in the current view?
        DM.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadAll, Application)
        'DM.WriteTreeToDisk()
        DM.ReNumberIDs(IDList)
        DM.WriteTreeToDisk()
    End Sub


    Public Function CustomFieldID_Set(ByVal UserDefinedFieldName As String,
                               Optional ByVal Value As String = "",
                               Optional ByRef SpecificItem As Object = Nothing,
                               Optional ByVal olUPType As Outlook.OlUserPropertyType =
                               Outlook.OlUserPropertyType.olText) As Boolean
        'QUESTION: Duplicate function??? ThisAddin.CustomFieldID_Set
        Dim oTask As Outlook.TaskItem
        Dim oMail As Outlook.MailItem
        Dim OlAppointment As Outlook.AppointmentItem
        Dim objProperty As Outlook.UserProperty


        Try
            If SpecificItem IsNot Nothing Then
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
        End Try

    End Function

    Public Function GetListOfItemsInView_ToDo() As List(Of Object)
        Dim OlItems As Items
        Dim objView As View
        Dim OlFolder As Folder
        Dim strFilter As String
        Dim oStore As Outlook.Store
        Dim objItem As Object
        'QUESTION: ThisAddin.GetListOfItemsInView_ToDo When is this called? Is it needed?
        'CLEANUP: ThisAddin.GetListOfItemsInView_ToDo Move to a Class, Module or a Library depending on how it is used. 

        objView = Application.ActiveExplorer.CurrentView
        strFilter = "@SQL=" & objView.Filter

        OlItems = Nothing
        For Each oStore In Application.Session.Stores
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            OlItems = If(strFilter = "@SQL=", OlFolder.Items, OlFolder.Items.Restrict(strFilter))
        Next
        Dim ListObjects As New List(Of Object)
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

        'QUESTION: Depricated? Previous function was GetList. Do we need both?
        objView = Application.ActiveExplorer.CurrentView
        strFilter = "@SQL=" & objView.Filter

        OlItems = Nothing
        For Each oStore In Application.Session.Stores
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            OlItems = If(strFilter = "@SQL=", OlFolder.Items, OlFolder.Items.Restrict(strFilter))
        Next
        GetItemsInView_ToDo = OlItems
    End Function

    Public Function IsChild(strParent As String, strChild As String) As Integer
        Dim count As Integer = 0
        Dim unbroken As Boolean = True
        Dim i As Integer
        'QUESTION: Duplicate? If not, move to a class, module or library.
        For i = 1 To strParent.Length / 2
            If unbroken Then
                If Mid(strParent, (i * 2) - 1, 2) = Mid(strChild, (i * 2) - 1, 2) Then
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
        'QUESTION: Duplicate? If not, move to a class, module or library.
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
        'QUESTION: Duplicate? If not, move to a class, module or library.
        For Each objItem In OlItems
            todo = New ToDoItem(objItem, OnDemand:=True)
            todo.SplitID()
        Next
    End Sub



    Private ReadOnly _blItemChangeRunning As Boolean = False

    'Public Sub HideEmptyHeaders()
    '    Dim DMtmp = New DataModel_ToDoTree(New List(Of TreeNode(Of ToDoItem)))
    '    DMtmp.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadInView)
    '    For Each node As TreeNode(Of ToDoItem) In DMtmp.ListOfToDoTree

    '    Next
    'End Sub

    Private Sub OlToDoItems_ItemChange(Item As Object) Handles OlToDoItems.ItemChange

        'TODO: Morph Functionality to handle proactively rather than reactively
        'If _blItemChangeRunning = False Then

        '_blItemChangeRunning = True
        Dim todo As New ToDoItem(Item, OnDemand:=True)
        Dim objProperty_ToDoID As Outlook.UserProperty = Item.UserProperties.Find("ToDoID")
        Dim objProperty_Project As Outlook.UserProperty = Item.UserProperties.Find("TagProject")


        Dim blTmp As Boolean = todo.EC2 'This reads the button and keeps the other field in sync if there is a change
        'Check to see if change was in the EC
        If todo.EC_Change Then
            Dim strEC As String = todo.ExpandChildren
            ' Extremely expensive. I wonder why it is done this way?
            If todo.ToDoID <> "" Then
                Dim strChFilter As String = "@SQL=" & Chr(34) & "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/ToDoID" & Chr(34) & " like '" & todo.ToDoID & "%'"
                Dim OlChildren As Outlook.Items = OlToDoItems.Restrict(strChFilter)

                'Identify the tree depth of the current ToDoID (Length of ToDoID / 2)
                Dim intLVL As Integer = Math.Truncate(todo.ToDoID.Length / 2)
                Dim objItem As Object
                For Each objItem In OlChildren
                    Dim todoTmp As New ToDoItem(objItem, OnDemand:=True)

                    'Set the toggle for that level to + or - for all descendants on the binary number
                    If todoTmp.ToDoID <> todo.ToDoID Then
                        'Added if statement to correct for the fact that Restrict is not case sensitive
                        If Left(todoTmp.ToDoID, todo.ToDoID.Length) = todo.ToDoID Then
                            If strEC = "-" Then
                                todoTmp.VisibleTreeStateLVL(intLVL + 1) = True
                            ElseIf strEC = "+" Then
                                todoTmp.VisibleTreeStateLVL(intLVL + 1) = False
                            End If
                            'Check to see if visible
                            Dim VisibleMask As Integer = Math.Pow(2, todoTmp.ToDoID.Length / 2) - 1
                            Dim blnewAB = (todoTmp.VisibleTreeState And VisibleMask) = VisibleMask
                            If blnewAB <> todoTmp.ActiveBranch Then
                                todoTmp.ActiveBranch = blnewAB
                            End If
                        End If
                    End If

                Next
            End If
            todo.EC_Change = False
        End If

        'AUTOCODE ToDoID based on Project
        'Check to see if the project exists before attempting to autocode the id
        If objProperty_Project IsNot Nothing Then

            'Get Project Name
            Dim strProject As String = todo.Project

            'Code the Program name
            If ProjInfo.Contains_ProjectName(strProject) Then
                Dim strProgram = ProjInfo.Programs_ByProjectNames(strProject)
                If todo.TagProgram <> strProgram Then
                    todo.TagProgram = strProgram
                End If
            End If

            Dim strProjectToDo As String
            'Check to see whether there is an existing ID
            If objProperty_ToDoID IsNot Nothing Then
                Dim strToDoID As String = objProperty_ToDoID.Value

                'Don't autocode branches that existed in another project previously
                If strToDoID.Length <> 0 And strToDoID.Length <= 4 Then
                    If strProject.Length <> 0 Then

                        'Check to ensure it is in the dictionary before autocoding
                        If ProjInfo.Contains_ProjectName(strProject) Then

                            If strToDoID.Length = 2 Then
                                ' Change the Item's todoid to be a node of the project
                                If todo.Context <> "@PROJECTS" Then
                                    strProjectToDo = ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                                    todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                    IDList.Save(_filenameIDList)
                                    todo.SplitID()
                                    todo.EC2 = True
                                End If
                            End If


                        Else 'If it is not in the dictionary, see if this is a project we should add
                            If strToDoID.Length = 4 Then
                                Dim response As MsgBoxResult = MsgBox("Add Project " & strProject & " to the Master List?", vbYesNo)
                                If response = vbYes Then
                                    Dim strProgram As String = InputBox("What is the program name for " & strProject & "?", DefaultResponse:="")
                                    ProjInfo.Add(New ToDoProjectInfoEntry(strProject, strToDoID, strProgram))
                                    ProjInfo.Save()
                                End If
                            End If
                        End If
                    End If

                ElseIf strToDoID.Length = 0 Then
                    strProject = todo.Project
                    If ProjInfo.Contains_ProjectName(strProject) Then
                        strProjectToDo = ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                        todo.TagProgram = ProjInfo.Find_ByProjectName(strProject).First().ProgramName
                        todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                        IDList.Save(_filenameIDList)
                        todo.SplitID()
                    End If

                End If
            Else 'In this case, the project name exists but the todo id does not
                'Get Project Name
                strProject = If(IsArray(objProperty_Project.Value), FlattenArry(objProperty_Project.Value), DirectCast(objProperty_Project.Value, String))

                'If the project name is in our dictionary, autoadd the ToDoID to this item
                If strProject.Length <> 0 Then
                    'If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                    If ProjInfo.Contains_ProjectName(strProject) Then
                        strProjectToDo = ProjInfo.Find_ByProjectName(strProject).First().ProjectID
                        'Add the next ToDoID available in that branch
                        todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                        todo.TagProgram = ProjInfo.Find_ByProjectName(strProject).First().ProgramName
                        IDList.Save(_filenameIDList)
                        todo.SplitID()
                        todo.EC2 = True
                    End If
                End If
            End If


        End If

        'If OlToDoItem_IsMarkedComplete(Item) Then
        'Check to see if todo was just marked complete 
        'If So, adjust Kan Ban fields and categories
        If todo.Complete Then
            If InStr(Item.Categories, "Tag KB Completed") = False Then
                Dim strCats As String = Item.Categories
                strCats = strCats.Replace("Tag KB Backlog", "").Replace(",,", ",")
                strCats = strCats.Replace("Tag KB InProgress", "").Replace(",,", ",")
                strCats = strCats.Replace("Tag KB Planned", "").Replace(",,", ",")
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
        '_blItemChangeRunning = False
        'End If

    End Sub

    Private Function OlToDoItem_IsMarkedComplete(Item As Object) As Boolean
        'QUESTION: Duplicate Function??? I beleive this is already in the ToDoItem class
        If TypeOf Item Is Outlook.MailItem Then
            Dim OlMail = Item
            Return OlMail.FlagStatus = OlFlagStatus.olFlagComplete
        ElseIf TypeOf Item Is TaskItem Then
            Dim OlTask = Item
            Return OlTask.Complete = True
        Else
            Return False
        End If

    End Function

    Private Sub OlToDoItems_ItemAdd(Item As Object) Handles OlToDoItems.ItemAdd
        'CLEANUP: Move this to a class, module or library
        Dim todo As New ToDoItem(Item, OnDemand:=True)
        If todo.ToDoID.Length = 0 Then
            If todo.Project.Length <> 0 Then
                If ProjInfo.Contains_ProjectName(todo.Project) Then
                    Dim strProjectToDo As String = ProjInfo.Find_ByProjectName(todo.Project).First().ProjectID
                    'Add the next ToDoID available in that branch
                    todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                    todo.TagProgram = ProjInfo.Find_ByProjectName(todo.Project).First().ProgramName
                    IDList.Save(_filenameIDList)
                    todo.SplitID()
                End If
            Else
                todo.ToDoID = IDList.GetMaxToDoID
            End If
        End If
        todo.VisibleTreeState = 63
        'Dim strToDoID As String = CustomFieldID_GetValue(Item, "ToDoID")
        'If strToDoID.Length = 0 Then
        '    strToDoID = IDList.GetMaxToDoID
        '    CustomFieldID_Set("ToDoID", Value:=strToDoID, SpecificItem:=Item)
        'End If

    End Sub

    ''' <summary>
    ''' This is a helper procedure to migrate ToDoIDs from one framework to another
    ''' </summary>
    Public Sub MigrateToDoIDs()
        'TODO: Move MigrateToDoIDs to a class, module, or library
        Dim ToDoItems As Outlook.Items = Application.GetNamespace("MAPI").GetDefaultFolder(OlDefaultFolders.olFolderToDo).Items
        Dim max As Long = ToDoItems.Count
        Dim j As Long = 0
        For i As Long = 1 To max
            Dim Item As New ToDoItem(ToDoItems.Item(i), True)
            j += 1
            If Item.CustomField("NewID") <> "Done" Then
                Dim strToDoID As String = Item.ToDoID
                If strToDoID.Length > 0 Then
                    Dim strToDoIDnew As String = SubstituteCharsInID(strToDoID)
                    Item.ToDoID = strToDoIDnew
                    Item.CustomField("NewID") = "Done"
                End If
            End If
            If j = 40 Then
                j = 0
                System.Windows.Forms.Application.DoEvents()
            End If
        Next



    End Sub

    Private Function SubstituteCharsInID(strToDoID As String) As String
        'Dim charsorig As String = "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
        'Dim charsnew As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
        '"0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
        '"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
        Dim charsorig As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
        Dim charsnew As String = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ"

        Dim strBuild As String = ""


        Dim c As Char
        For Each c In strToDoID
            Dim intLoc As Integer = InStr(charsorig, c)
            strBuild += Mid(charsnew, intLoc, 1)
        Next

        Return strBuild

    End Function

    Public Sub TestProjectInfo()
        'TODO: Migrate Function To Unit Test
        'Dim ftmp As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _appDataFolder, "ProjInfo.csv")
        'For Each entry As ProjectInfoEntry In ProjInfo
        '    WriteToCSV(ftmp, entry.ToCSV)
        'Next
        If ProjInfo.Contains_ProgramName("Digital Transformation LATAM") Then
            Dim unused As New List(Of IToDoProjectInfoEntry)
            Dim lst As List(Of IToDoProjectInfoEntry) = ProjInfo.Find_ByProgramName("Digital Transformation LATAM")
            For Each entry As ToDoProjectInfoEntry In lst
                Debug.WriteLine(entry.ToCSV)
            Next
        End If
        If ProjInfo.Contains_ProgramName("Pete") Then
            Dim lst As List(Of IToDoProjectInfoEntry)
            lst = ProjInfo.Find_ByProgramName("Digital Transformation LATAM")
            For Each entry As ToDoProjectInfoEntry In lst
                Debug.WriteLine(entry.ToCSV)
            Next
        End If
        If ProjInfo.Contains_ProjectID("1308") Then
            Dim lst As List(Of IToDoProjectInfoEntry)
            lst = ProjInfo.Find_ByProjectID("1308")
            For Each entry As ToDoProjectInfoEntry In lst
                Debug.WriteLine(entry.ToCSV)
            Next
        End If
        If ProjInfo.Contains_ProjectID("980H") Then
            Dim lst As List(Of IToDoProjectInfoEntry)
            lst = ProjInfo.Find_ByProjectID("980H")
            For Each entry As ToDoProjectInfoEntry In lst
                Debug.WriteLine(entry.ToCSV)
            Next
        End If
        If ProjInfo.Contains_ProjectID("abcd") Then
            Dim lst As List(Of IToDoProjectInfoEntry)
            lst = ProjInfo.Find_ByProjectID("abcd")
            For Each entry As ToDoProjectInfoEntry In lst
                Debug.WriteLine(entry.ToCSV)
            Next
        End If
        '5 CCO Org Design and Functions
        If ProjInfo.Contains_ProjectName("5 CCO Org Design and Functions") Then
            Dim lst As List(Of IToDoProjectInfoEntry)
            lst = ProjInfo.Find_ByProjectName("5 CCO Org Design and Functions")
            For Each entry As ToDoProjectInfoEntry In lst
                Debug.WriteLine(entry.ToCSV)
            Next
        End If
        If ProjInfo.Contains_ProjectName("pete") Then
            Dim lst As List(Of IToDoProjectInfoEntry)
            lst = ProjInfo.Find_ByProgramName("5 CCO Org Design and Functions")
            For Each entry As ToDoProjectInfoEntry In lst
                Debug.WriteLine(entry.ToCSV)
            Next
        End If
    End Sub

    'Private Sub OlExplorer_ViewSwitch() Handles OlExplorer.ViewSwitch
    '    If OlExplorer.CurrentFolder.Name = "To-Do List" Or OlExplorer.CurrentFolder.Name = "Tasks" Then
    '        Debug.Print(OlExplorer.CurrentFolder.Name)
    '        DM_CurView = New DataModel_ToDoTree(New List(Of TreeNode(Of ToDoItem)))
    '        DM_CurView.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadInView)

    '    End If
    'End Sub
End Class
