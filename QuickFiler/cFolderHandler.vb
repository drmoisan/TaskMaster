Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop
Imports UtilitiesVB

Public Class cFolderHandler
    Private m_Folder As [MAPIFolder]
    Private m_Find As String
    Private m_Wildcard As Boolean
    Private FolderList() As String
    Public Result As cSuggestions
    Public SaveCounter As Integer
    Private UpBound As Integer
    Private OlApp As Application

    Private Const SpeedUp As Boolean = True
    Private Const StopAtFirstMatch As Boolean = False
    Public Update_Suggestions_Bool As Boolean
    Public WhConv As Boolean

    Public Sub New(OlApp As Application)
        Me.OlApp = OlApp
    End Sub

    Public Function FindFolder(Name$,
                               Optional Reload As Boolean = True,
                               Optional strEmailFolderPath As String = Archive_Root,
                               Optional ReCalcSuggestions As Boolean = False,
                               Optional objItem As Object) As String()
        ''Dim Name$
        Dim folders As [Folders]
        Dim i As Integer
        Dim z As Integer

        'Dim objItem As Object
        Dim objApp As [Application]
        Dim objMail As MailItem
        Dim objProperty As [UserProperty]
        Dim varFldrs As Object
        Dim intVarCt As Integer
        Dim strTmp As String

        ReDim FolderList(0)
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

        'If Email_SortToExistingFolder.UpBoundText = 0 Then Email_SortToExistingFolder.Initialize_Read_Text_File
        'If Email_SortToExistingFolder.UpBoundText > 0 Then
        '    UpBound = UpBound + 3
        '    ReDim Preserve FolderList(UpBound)
        '    FolderList(UpBound - 3) = "============================="
        '    FolderList(UpBound - 2) = "======= RECENT SELECTIONS ========"
        '    FolderList(UpBound - 1) = "============================="

        'FolderList(0) = "==============================="             'Put in placeholder to skip over first line
        If ReCalcSuggestions Then Update_Suggestions_Bool = True

        If objItem Is Nothing Then objItem = GetCurrentItem(OlApp)
        If Not objItem Is Nothing Then
            If TypeOf objItem Is [MailItem] Then
                If Update_Suggestions_Bool = True Then
                    If bl_SuggestionFiles_IsLoaded = False Then Reload = True
                    objMail = objItem
                    If ReCalcSuggestions = False Then
                        objProperty = objMail.UserProperties.Find("FolderKey")
                        If objProperty Is Nothing Then
                            Result = Email_AutoCategorize.Folder_Suggestions(objMail, Reload)
                        Else
                            varFldrs = objProperty.Value
                            If IsArray(varFldrs) = False Then
                                If varFldrs = "Error" Then
                                    Result = Email_AutoCategorize.Folder_Suggestions(objMail, Reload)
                                Else
                                    strTmp = varFldrs
                                    Result.Add(strTmp, 1)
                                    'result.Count = 1
                                    'ReDim result.FolderList(1) As String
                                    'result.FolderList(1) = varFldrs
                                End If
                            Else
                                intVarCt = UBound(varFldrs)
                                If intVarCt = 0 Then
                                    If varFldrs(0) = "Error" Then
                                        Result = Email_AutoCategorize.Folder_Suggestions(objMail, Reload)
                                    Else
                                        strTmp = varFldrs(0)
                                        Result.ADD_END(strTmp)
                                        'result.Count = 1
                                        'ReDim result.FolderList(1) As String
                                        'result.FolderList(1) = varFldrs(0)
                                    End If
                                Else
                                    'result.Count = intVarCt + 1
                                    'ReDim result.FolderList(result.Count) As String
                                    For i = 0 To intVarCt
                                        'result.FolderList(i + 1) = varFldrs(i)
                                        strTmp = varFldrs(i)
                                        Result.ADD_END(strTmp)
                                    Next i
                                End If
                            End If
                        End If
                    Else
                        Result = Email_AutoCategorize.Folder_Suggestions(objMail, Reload)
                    End If
                    Update_Suggestions_Bool = False
                End If

                If Result.Count > 0 Then
                    If UpBound > 0 Then UpBound = UpBound + 1
                    UpBound = UpBound + Result.Count
                    ReDim Preserve FolderList(UpBound)
                    FolderList(UpBound - Result.Count) = "========= SUGGESTIONS ========="
                    For i = 1 To Result.Count
                        FolderList(UpBound - Result.Count + i) = Result.FolderList_ItemByIndex(i)
                    Next i
                End If

            End If
        End If

        UpBound = UpBound + 1
        ReDim Preserve FolderList(UpBound)
        FolderList(UpBound) = "======= RECENT SELECTIONS ========"  'Seperator between search and recent selections


        For i = 0 To Email_SortToExistingFolder.UpBoundText - 1
            UpBound = UpBound + 1
            ReDim Preserve FolderList(UpBound)
            'FolderList(UpBound - 1) = RecentsList(i)
            FolderList(UpBound) = Email_SortToExistingFolder.RecentsList(i)
        Next i
        End If

        FindFolder = FolderList


    End Function

    Public Function GetFolder(ByVal FolderPath As String) As Outlook.Folder
        Dim TestFolder As Outlook.Folder
        Dim FoldersArray As Variant
        Dim i As Integer

        On Error GoTo GetFolder_Error
        If Left(FolderPath, 2) = "\\" Then
            FolderPath = Right(FolderPath, Len(FolderPath) - 2)
        End If
        'Convert folderpath to array
        FoldersArray = Split(FolderPath, "\")
 Set TestFolder = Application.Session.folders.item(FoldersArray(0))
 If Not TestFolder Is Nothing Then
            For i = 1 To UBound(FoldersArray, 1)
                Dim SubFolders As Outlook.folders
        Set SubFolders = TestFolder.folders
        Set TestFolder = SubFolders.item(FoldersArray(i))
        If TestFolder Is Nothing Then
            Set GetFolder = Nothing
        End If
            Next
        End If
 
 'Return the TestFolder
 Set GetFolder = TestFolder
 Exit Function

GetFolder_Error:
 Set GetFolder = Nothing
 Exit Function
    End Function

    Private Sub LoopFolders(folders As Outlook.folders, Optional strEmailFolderPath = Archive_Root)
        Dim f As Outlook.Folder
        Dim Fpath As String
        Dim found As Boolean
        Dim intRootLen As Integer

        If SpeedUp = False Then DoEvents

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
                    ReDim Preserve FolderList(UpBound)
                    'FolderList(UpBound - 1) = Right(f.FolderPath, Len(f.FolderPath) - 36) 'If starting at 0 in folder list
                    FolderList(UpBound) = Right(f.FolderPath, Len(f.FolderPath) - intRootLen - 1) 'If starting at 1 in folder list
                End If
            End If
            If found Then
      Set m_Folder = f
      Exit For
            Else
                LoopFolders f.folders, strEmailFolderPath
      If Not m_Folder Is Nothing Then Exit For
            End If
        Next
    End Sub

    Private Sub Class_Initialize()
    Set Result = New cSuggestions
    
End Sub

End Class
