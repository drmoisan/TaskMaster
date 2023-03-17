Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop
Imports UtilitiesVB

Public Class cFolderHandler
    Private m_Folder As Folder
    Private m_Find As String
    Private m_Wildcard As Boolean
    Private _folderList() As String
    Public Result As cSuggestions
    Public SaveCounter As Integer
    Private UpBound As Integer
    Private _olApp As Application

    Private Const SpeedUp As Boolean = True
    Private Const StopAtFirstMatch As Boolean = False
    Public Update_Suggestions_Bool As Boolean
    Public WhConv As Boolean
    Private _globals As IApplicationGlobals

    Public ReadOnly Property FolderList As String()
        Get
            Return _folderList
        End Get
    End Property

    Public Sub New(AppGlobals As IApplicationGlobals)
        _globals = AppGlobals
        Me._olApp = AppGlobals.Ol.App
        Result = New cSuggestions
    End Sub


    ''' <summary>
    ''' Function returns a list of Outlok folders that meet search criteria and appends a list of suggested folders 
    ''' as well as appending a list of recently used folders
    ''' </summary>
    ''' <param name="Name$"></param>
    ''' <param name="Reload"></param>
    ''' <param name="strEmailFolderPath"></param>
    ''' <param name="ReCalcSuggestions"></param>
    ''' <param name="objItem"></param>
    ''' <returns></returns>
    Public Function FindFolder(Name$,
                               Optional Reload As Boolean = True,
                               Optional strEmailFolderPath As String = "",
                               Optional ReCalcSuggestions As Boolean = False,
                               Optional objItem As Object = Nothing) As String()
        ''Dim Name$
        Dim folders As Outlook.Folders
        Dim i As Integer


        Dim objMail As MailItem
        Dim objProperty As [UserProperty]
        Dim varFldrs As Object
        Dim intVarCt As Integer
        Dim strTmp As String

        ReDim _folderList(0)
        UpBound = 0

        m_Folder = Nothing
        m_Find = ""
        m_Wildcard = False

        ''Name = InputBox("Find name:", "Search folder")
        If Len(Trim$(Name)) <> 0 Then
            m_Find = Name

            m_Find = LCase$(m_Find)
            m_Find = Replace(m_Find, "%", "*")
            m_Wildcard = (InStr(m_Find, "*"))

            folders = GetFolder(strEmailFolderPath).Folders
            LoopFolders(folders, strEmailFolderPath)
        End If


        If _globals.AF.RecentsList.Count > 0 Then
            '    UpBound = UpBound + 3
            '    ReDim Preserve _folderList(UpBound)
            '    _folderList(UpBound - 3) = "============================="
            '    _folderList(UpBound - 2) = "======= RECENT SELECTIONS ========"
            '    _folderList(UpBound - 1) = "============================="

            '_folderList(0) = "==============================="             'Put in placeholder to skip over first line
            If ReCalcSuggestions Then Update_Suggestions_Bool = True

            If objItem Is Nothing Then objItem = GetCurrentItem(_olApp)
            If Not objItem Is Nothing Then
                If TypeOf objItem Is [MailItem] Then
                    If Update_Suggestions_Bool = True Then
                        If _globals.AF.SuggestionFilesLoaded = False Then Reload = True
                        objMail = objItem
                        If ReCalcSuggestions = False Then
                            objProperty = objMail.UserProperties.Find("FolderKey")
                            If objProperty Is Nothing Then
                                Result = FolderSuggestionsModule.Folder_Suggestions(objMail, _globals, Reload)
                            Else
                                varFldrs = objProperty.Value
                                If IsArray(varFldrs) = False Then
                                    If varFldrs = "Error" Then
                                        Result = FolderSuggestionsModule.Folder_Suggestions(objMail, _globals, Reload)
                                    Else
                                        strTmp = varFldrs
                                        Result.Add(strTmp, 1)
                                        'result.Count = 1
                                        'ReDim result._folderList(1) As String
                                        'result._folderList(1) = varFldrs
                                    End If
                                Else
                                    intVarCt = UBound(varFldrs)
                                    If intVarCt = 0 Then
                                        If varFldrs(0) = "Error" Then
                                            Result = FolderSuggestionsModule.Folder_Suggestions(objMail, _globals, Reload)
                                        Else
                                            strTmp = varFldrs(0)
                                            Result.ADD_END(strTmp)
                                            'result.Count = 1
                                            'ReDim result._folderList(1) As String
                                            'result._folderList(1) = varFldrs(0)
                                        End If
                                    Else
                                        'result.Count = intVarCt + 1
                                        'ReDim result._folderList(result.Count) As String
                                        For i = 0 To intVarCt
                                            'result._folderList(i + 1) = varFldrs(i)
                                            strTmp = varFldrs(i)
                                            Result.ADD_END(strTmp)
                                        Next i
                                    End If
                                End If
                            End If
                        Else
                            Result = FolderSuggestionsModule.Folder_Suggestions(objMail, _globals, Reload)
                        End If
                        Update_Suggestions_Bool = False
                    End If

                    If Result.Count > 0 Then
                        If UpBound > 0 Then UpBound = UpBound + 1
                        UpBound = UpBound + Result.Count
                        ReDim Preserve _folderList(UpBound)
                        _folderList(UpBound - Result.Count) = "========= SUGGESTIONS ========="
                        For i = 1 To Result.Count
                            _folderList(UpBound - Result.Count + i) = Result._folderList_ItemByIndex(i)
                        Next i
                    End If

                End If
            End If

            UpBound = UpBound + 1
            ReDim Preserve _folderList(UpBound)
            _folderList(UpBound) = "======= RECENT SELECTIONS ========"  'Seperator between search and recent selections


            For Each folderName As String In _globals.AF.RecentsList
                UpBound = UpBound + 1
                ReDim Preserve _folderList(UpBound)
                _folderList(UpBound) = folderName
            Next
        End If

        FindFolder = _folderList


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
            If m_Wildcard Then
                found = (LCase$(f.FolderPath) Like m_Find)
            Else
                found = (LCase$(f.FolderPath) = m_Find)
            End If

            If found Then
                If StopAtFirstMatch = False Then
                    found = False
                    UpBound = UpBound + 1
                    ReDim Preserve _folderList(UpBound)
                    '_folderList(UpBound - 1) = Right(f.FolderPath, Len(f.FolderPath) - 36) 'If starting at 0 in folder list
                    _folderList(UpBound) = Right(f.FolderPath, Len(f.FolderPath) - intRootLen - 1) 'If starting at 1 in folder list
                End If
            End If
            If found Then
                m_Folder = f
                Exit For
            Else
                LoopFolders(f.Folders, strEmailFolderPath)
                If Not m_Folder Is Nothing Then Exit For
            End If
        Next
    End Sub



End Class
