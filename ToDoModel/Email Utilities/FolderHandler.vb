Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop
Imports UtilitiesVB
Imports Newtonsoft.Json.Linq
Imports System.Reflection

Public Class FolderHandler
    Private _matchedFolder As Folder
    Private _searchString As String
    Private _wildcardFlag As Boolean
    Private _folderList() As String
    Private _suggestions As cSuggestions
    Public SaveCounter As Integer
    Private _upBound As Integer
    Private _olApp As Application

    Private Const SpeedUp As Boolean = True
    Private Const StopAtFirstMatch As Boolean = False
    Private _blUpdateSuggestions As Boolean
    Public WhConv As Boolean
    Private _globals As IApplicationGlobals
    Private _options As Options

    Public Enum Options
        NoSuggestions = 0
        FromArrayOrString = 1
        FromField = 2
        Recalculate = 4
    End Enum

    Public Sub New(AppGlobals As IApplicationGlobals)
        _globals = AppGlobals
        _olApp = AppGlobals.Ol.App
        _options = Options.NoSuggestions
        Suggestions = New cSuggestions()
        ReDim _folderList(-1)
    End Sub

    Public Sub New(AppGlobals As IApplicationGlobals, ObjItem As System.Object, Options As Options)
        _globals = AppGlobals
        _olApp = AppGlobals.Ol.App
        _options = Options

        Suggestions = New cSuggestions()

        If Options = Options.FromArrayOrString Then
            InitializeFromArrayOrString(ObjItem)
        ElseIf Options = Options.FromField Then
            InitializeFromEmail(ObjItem)
        ElseIf Options = Options.Recalculate Then
            RecalculateSuggestions(ObjItem, False)
        ElseIf Options = Options.NoSuggestions Then
        Else
            Throw New ArgumentException("Unknown option value " + Options)
        End If

        ReDim _folderList(0)
        AddSuggestions()
        AddRecents()
    End Sub

    Private Sub InitializeFromEmail(ObjItem As Object)
        Dim OlMail = TryResolveMailItem(ObjItem)
        If OlMail Is Nothing Then
            Throw New ArgumentException("Constructor Requires the Email Object to be passed as MailItem to use this flag")
        Else
            LoadFromFolderKeyField(False, OlMail)
        End If
    End Sub

    Private Sub InitializeFromArrayOrString(ObjItem As Object)
        If ObjItem Is Nothing Then
            Throw New ArgumentException("Cannot initialize suggestions from array or string because reference is null")
        ElseIf ObjItem.[GetType]().IsArray AndAlso ("".GetType()).IsAssignableFrom(ObjItem.GetElementType()) Then
            Suggestions.FolderSuggestionsArray = DirectCast(ObjItem, String())
        ElseIf TypeOf ObjItem Is String Then
            Dim tmpString As String = DirectCast(ObjItem, String)
            Suggestions.ADD_END(tmpString)
        Else
            Throw New ArgumentException("ObjItem is of type " + TypeName(ObjItem) +
                                        ", but selected option requires a string or string array")
        End If
    End Sub

    Public ReadOnly Property FolderList As String()
        Get
            If _folderList.Length = -1 Then
                If Suggestions.Count > 0 Then AddSuggestions()
                If _globals.AF.RecentsList.Count > 0 Then AddRecents()
            End If
            Return _folderList
        End Get
    End Property

    Public Property Suggestions As cSuggestions
        Get
            Return _suggestions
        End Get
        Set(value As cSuggestions)
            _suggestions = value
        End Set
    End Property

    Public Property BlUpdateSuggestions As Boolean
        Get
            Return _blUpdateSuggestions
        End Get
        Set(value As Boolean)
            _blUpdateSuggestions = value
        End Set
    End Property


    ''' <summary>
    ''' Function returns a list of Outlook folders that meet search criteria and appends a list of suggested folders 
    ''' as well as appending a list of recently used folders
    ''' </summary>
    ''' <param name="SearchString"></param>
    ''' <param name="ReloadCTFStagingFiles"></param>
    ''' <param name="EmailSearchRoot"></param>
    ''' <param name="ReCalcSuggestions"></param>
    ''' <param name="objItem"></param>
    ''' <returns></returns>
    Public Function FindFolder(SearchString As String,
                               objItem As Object,
                               Optional ReloadCTFStagingFiles As Boolean = True,
                               Optional EmailSearchRoot As String = "ARCHIVEROOT",
                               Optional ReCalcSuggestions As Boolean = False) As String()

        If EmailSearchRoot = "ARCHIVEROOT" Then
            EmailSearchRoot = _globals.Ol.ArchiveRootPath
        End If
        ReDim _folderList(0)
        _folderList(0) = "======= SEARCH RESULTS ======="
        'TODO: Either use the embedded UBound or pass as reference. It is hard to know where it is changed
        _upBound = 0

        GetMatchingFolders(SearchString, EmailSearchRoot)

        If ReCalcSuggestions Then
            RecalculateSuggestions(objItem, ReloadCTFStagingFiles)
        End If
        AddSuggestions()
        AddRecents()

        Return _folderList


    End Function

    Private Sub AddRecents()
        _upBound = _upBound + 1
        ReDim Preserve _folderList(_upBound)
        _folderList(_upBound) = "======= RECENT SELECTIONS ========"  'Seperator between search and recent selections

        For Each folderName As String In _globals.AF.RecentsList
            _upBound = _upBound + 1
            ReDim Preserve _folderList(_upBound)
            _folderList(_upBound) = folderName
        Next
    End Sub

    Private Sub RecalculateSuggestions(ObjItem As Object, ByRef ReloadCTFStagingFiles As Boolean)
        Dim OlMail As MailItem = TryResolveMailItem(ObjItem)
        If OlMail IsNot Nothing Then
            If _globals.AF.SuggestionFilesLoaded = False Then ReloadCTFStagingFiles = True
            Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles)
            BlUpdateSuggestions = False
        Else
            Throw New ArgumentException("ObjItem passed as " + TypeName(ObjItem) + ", but should have been MailItem")
        End If
    End Sub

    Private Sub LoadFromFolderKeyField(ReloadCTFStagingFiles As Boolean, OlMail As MailItem)
        Dim i As Integer
        Dim strTmp As String

        Dim intVarCt As Integer

        Dim objProperty As UserProperty = OlMail.UserProperties.Find("FolderKey")
        If objProperty Is Nothing Then
            Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles)
        Else
            Dim varFldrs As Object = objProperty.Value

            If IsArray(varFldrs) = False Then
                If varFldrs = "Error" Then
                    Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles)
                Else
                    strTmp = varFldrs
                    Suggestions.Add(strTmp, 1)
                End If
            Else
                intVarCt = UBound(varFldrs)
                If intVarCt = 0 Then
                    If varFldrs(0) = "Error" Then
                        Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles)
                    Else
                        strTmp = varFldrs(0)
                        Suggestions.ADD_END(strTmp)
                    End If
                Else
                    For i = 0 To intVarCt
                        strTmp = varFldrs(i)
                        Suggestions.ADD_END(strTmp)
                    Next i
                End If
            End If
        End If
    End Sub

    Private Sub AddSuggestions()
        If Suggestions.Count > 0 Then
            If _upBound > 0 Then _upBound = _upBound + 1
            _upBound = _upBound + Suggestions.Count
            ReDim Preserve _folderList(_upBound)
            _folderList(_upBound - Suggestions.Count) = "========= SUGGESTIONS ========="
            For i = 1 To Suggestions.Count
                _folderList(_upBound - Suggestions.Count + i) = Suggestions.FolderList_ItemByIndex(i)
            Next i
        End If
    End Sub

    Private Function GetMatchingFolders(Name As String, strEmailFolderPath As String) As Folders
        _matchedFolder = Nothing
        _searchString = ""
        _wildcardFlag = False


        If Len(Trim$(Name)) <> 0 Then
            _searchString = Name

            _searchString = LCase$(_searchString)
            _searchString = Replace(_searchString, "%", "*")
            _wildcardFlag = (InStr(_searchString, "*"))

            Dim folders = GetFolder(strEmailFolderPath).Folders
            LoopFolders(folders, strEmailFolderPath)

            Return folders
        Else
            Return Nothing
        End If


    End Function

    Public Function GetFolder(ByVal FolderPath As String) As Outlook.Folder
        Dim TestFolder As Outlook.Folder
        Dim FoldersArray As Object
        Dim i As Integer

        If Left(FolderPath, 2) = "\\" Then
            FolderPath = Right(FolderPath, Len(FolderPath) - 2)
        End If
        'Convert folderpath to array
        FoldersArray = Split(FolderPath, "\")
        TestFolder = _olApp.Session.Folders.Item(FoldersArray(0))
        If Not TestFolder Is Nothing Then
            For i = 1 To UBound(FoldersArray, 1)
                Dim SubFolders As Outlook.Folders
                SubFolders = TestFolder.Folders
                TestFolder = SubFolders.Item(FoldersArray(i))
                If TestFolder Is Nothing Then
                    Return Nothing
                End If
            Next
        End If

        Return TestFolder

    End Function

    Private Sub LoopFolders(folders As Outlook.Folders, Optional strEmailFolderPath As String = "")
        Dim f As Outlook.Folder
        Dim found As Boolean
        Dim intRootLen As Integer

        If strEmailFolderPath = "" Then
            strEmailFolderPath = _globals.Ol.ArchiveRootPath
        End If

        If SpeedUp = False Then _olApp.DoEvents()

        intRootLen = Len(strEmailFolderPath)
        For Each f In folders
            If _wildcardFlag Then
                found = (LCase$(f.FolderPath) Like _searchString)
            Else
                found = (LCase$(f.FolderPath) = _searchString)
            End If

            If found Then
                If StopAtFirstMatch = False Then
                    found = False
                    _upBound = _upBound + 1
                    ReDim Preserve _folderList(_upBound)
                    '_folderList(_upBound - 1) = Right(f.FolderPath, Len(f.FolderPath) - 36) 'If starting at 0 in folder list
                    _folderList(_upBound) = Right(f.FolderPath, Len(f.FolderPath) - intRootLen - 1) 'If starting at 1 in folder list
                End If
            End If
            If found Then
                _matchedFolder = f
                Exit For
            Else
                LoopFolders(f.Folders, strEmailFolderPath)
                If Not _matchedFolder Is Nothing Then Exit For
            End If
        Next
    End Sub



End Class
