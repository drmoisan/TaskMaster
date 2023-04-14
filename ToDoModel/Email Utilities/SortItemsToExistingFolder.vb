
Imports System.IO
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB
Imports UtilitiesCS

Public Module SortItemsToExistingFolder
    Public Sub InitializeSortToExisting(Optional InitType As String = "Sort",
        Optional QuickLoad As Boolean = False,
        Optional WholeConversation As Boolean = True,
        Optional strSeed As String = "",
        Optional objItem As Object = Nothing)
        Throw New NotImplementedException
    End Sub

    Public Sub MASTER_SortEmailsToExistingFolder(selItems As IList,
                                                 Pictures_Checkbox As Boolean,
                                                 SortFolderpath As String,
                                                 Save_MSG As Boolean,
                                                 Attchments As Boolean,
                                                 Remove_Flow_File As Boolean,
                                                 AppGlobals As IApplicationGlobals,
                                                 Optional StrRoot As String = "")
        Dim loc As String
        Dim FileSystem_LOC As String
        Dim FileSystem_LOC2 As String
        Dim FileSystem_DelLOC As String
        'Dim selItems            As Selection    ' A collection of Outlook item objects in a folder.
        'Dim selItems            As Collection
        Dim objItem As Object
        Dim MSG As MailItem
        Dim objFSO As Object       ' Computer's file system object.
        Dim objShell As Object       ' Windows Shell application object.
        Dim objFolder As Object       ' The selected folder object from Browse for Folder dialog box.
        Dim objSubFolders As Object
        Dim objNewFolder As Object
        Dim sortFolder As Outlook.Folder
        Dim folderCurrent As Outlook.Folder
        Dim strFolderPath As String = ""
        Dim i As Integer
        Dim oMailTmp As MailItem
        Dim strTemp As String
        Dim strAry() As String
        Dim strOutput(1) As String

        '******************
        '***INITIALIZE*****
        '******************
        Dim _globals As IApplicationGlobals = AppGlobals
        If (StrRoot = "") Then
            StrRoot = _globals.Ol.ArchiveRootPath
        End If
        'TODO: Eliminate following line once Path.Combine used below
        loc = StrRoot & "\"

        Dim _olApp As Outlook.Application = _globals.Ol.App
        Dim OlNS As Outlook.NameSpace = _globals.Ol.NamespaceMAPI


        If selItems.Count > 0 Then
            folderCurrent = GetCurrentExplorerFolder(_globals.Ol.App.ActiveExplorer(), selItems(0))
        Else
            folderCurrent = GetCurrentExplorerFolder(_globals.Ol.App.ActiveExplorer())
        End If
        If InStr(folderCurrent.FolderPath, _globals.Ol.Inbox.FolderPath) Then
            strFolderPath = _globals.FS.FldrFlow
        ElseIf InStr(folderCurrent.FolderPath, StrRoot) And folderCurrent.FolderPath <> StrRoot Then
            strFolderPath = folderCurrent.ToFsFolder(OlFolderRoot:=_globals.Ol.ArchiveRootPath, FsFolderRoot:=_globals.FS.FldrRoot)
            'strFolderPath = _globals.FS.FldrRoot & Right(folderCurrent.FolderPath, Len(folderCurrent.FolderPath) - Len(_globals.Ol.ArchiveRootPath) - 1)
        Else

        End If



        '*************************************************************************
        '************** SAVE ATTACHMENTS IF ENABLED*******************************
        '*************************************************************************
        Dim strTemp2 As String = ""
        'QUESTION: Original code allowed path to be an optional variable and then did something if a value was supplied that didn't match the archive root. Need to determine why and if new treatment loses functionality
        If StrRoot <> _globals.Ol.ArchiveRootPath Then
            strTemp2 = Right(_globals.Ol.ArchiveRootPath, Len(_globals.Ol.ArchiveRootPath) - Len(_globals.Ol.EmailRootPath) - 1)
            FileSystem_LOC = _globals.FS.FldrRoot & strTemp2 & "\" & SortFolderpath  'Parent Directory
        Else
            FileSystem_LOC = Path.Combine(_globals.FS.FldrRoot, SortFolderpath)
        End If

        FileSystem_DelLOC = _globals.FS.FldrRoot

        'If Save_PDF = True Then
        'Call SaveAsPDF.SaveMessageAsPDF(FileSystem_LOC, selItems)
        'End If

        If Save_MSG = True Then
            Call SaveMessageAsMSG(FileSystem_LOC, selItems)
        End If
        '



        '****Save Attachment to OneDrive directory****

        If Attchments = True Then
            'Email_SortSaveAttachment.SaveAttachmentsFromSelection(SavePath:=FileSystem_LOC, Verify_Action:=Pictures_Checkbox, selItems:=selItems, save_images:=Pictures_Checkbox, SaveMSG:=Save_MSG)
            SaveAttachmentsFromSelection(AppGlobals:=AppGlobals,
                                         SavePath:=FileSystem_LOC,
                                         Verify_Action:=Pictures_Checkbox,
                                         selItems:=selItems,
                                         save_images:=Pictures_Checkbox,
                                         SaveMSG:=Save_MSG)
        End If



        If Remove_Flow_File = True Then
            Call SaveAttachmentsFromSelection(AppGlobals:=AppGlobals, SavePath:=strFolderPath, DELFILE:=True, selItems:=selItems)
        End If



        '*************************************************************************
        '*********** LABEL EMAIL AS AUTOSORTED AND MOVE TO EMAIL FOLDER***********
        '*************************************************************************

        'If strTemp2 = "" Then Add_Recent(SortFolderpath)
        If strTemp2 = "" Then _globals.AF.RecentsList.AddRecent(SortFolderpath)
        loc = Path.Combine(StrRoot, SortFolderpath)
        sortFolder = New FolderHandler(_globals).GetFolder(loc) 'Call Function to turn text to Folder

        'Call Flag_Fields_Categories.SetCategory("Autosort")
        'Call Flag_Fields_Categories.CustomFieldID_Set("Autosort", "True")
        If sortFolder Is Nothing Then
            MsgBox(loc & " does not exist, skipping email move.")
        Else

            For i = selItems.Count - 1 To 0 Step -1
                If TypeOf selItems(i) Is Outlook.MailItem Then
                    If Not TypeOf selItems(i) Is Outlook.MeetingItem Then
                        MSG = selItems(i)
                        If strTemp2 = "" Then
                            'Email_AutoCategorize.UpdateForMove(MSG, SortFolderpath)
                            UpdateForMove(MSG, SortFolderpath, AppGlobals.AF.CTFList)
                        End If
                        On Error Resume Next
                        CustomFieldID_Set("Autosort", "True", SpecificItem:=MSG)
                        MSG.UnRead = False
                        MSG.Save()

                        oMailTmp = MSG.Move(sortFolder)

                        If Err.Number <> 0 Then
                            'TODO: ERROR LOGGING
                            'MsgBox("Error in " & SubNm & "-> MailItem.Move: " & Err.Number & " -> " & Err.Description & " ->" & Err.Source)
                            Err.Clear()
                        Else
                            If _globals.Ol.MovedMails_Stack Is Nothing Then _globals.Ol.MovedMails_Stack = New StackObjectVB
                            _globals.Ol.MovedMails_Stack.Push(MSG)
                            _globals.Ol.MovedMails_Stack.Push(oMailTmp)

                            'TODO: Change this into a JSON file
                            WriteCSV_StartNewFileIfDoesNotExist(_globals.FS.Filenames.EmailMoves, _globals.FS.FldrMyD)
                            strAry = CaptureEmailDetails(oMailTmp, _globals.Ol.ArchiveRootPath)
                            strOutput(1) = SanitizeArrayLineTSV(strAry)
                            FileIO2.Write_TextFile(_globals.FS.Filenames.EmailMoves, strOutput, _globals.FS.FldrMyD)
                        End If

                    End If
                End If

                'FireTimerReset
            Next i
        End If
    End Sub

    Private Function SanitizeArrayLineTSV(ByRef strOutput() As String) As String
        Dim strBuild As String = ""
        If strOutput.IsAllocated() Then
            Dim max As Integer = UBound(strOutput)
            For i = 1 To max
                Dim strTemp As String = strOutput(i)
                strTemp = Replace$(Trim$(strTemp), vbTab, "")
                strTemp = Replace$(strTemp, vbCrLf, " ")
                strTemp = Replace$(strTemp, vbLf, " ")

                strBuild = strBuild & vbTab & strTemp

            Next i
            If Len(strBuild) > 0 Then strBuild = Right(strBuild, Len(strBuild) - 1)
            Return strBuild
        Else
            Return ""
        End If
    End Function

    Private Sub WriteCSV_StartNewFileIfDoesNotExist(strFileName As String, strFileLocation As String)
        Dim strOutput() As String
        Dim strAryOutput(,) As String
        Dim objFSO As Object
        strOutput = Nothing
        If File.Exists(Path.Combine(strFileName, strFileLocation)) Then
            ReDim strAryOutput(13, 1)

            strAryOutput(1, 1) = "Triage"
            strAryOutput(2, 1) = "FolderName"
            strAryOutput(3, 1) = "Sent_On"
            strAryOutput(4, 1) = "From"
            strAryOutput(5, 1) = "To"
            strAryOutput(6, 1) = "CC"
            strAryOutput(7, 1) = "Subject"
            strAryOutput(8, 1) = "Body"
            strAryOutput(9, 1) = "fromDomain"
            strAryOutput(10, 1) = "Conversation_ID"
            strAryOutput(11, 1) = "EntryID"
            strAryOutput(12, 1) = "Attachments"
            strAryOutput(13, 1) = "FlaggedAsTask"

            Sanitize_Array(strAryOutput, strOutput)
            Write_TextFile(strFileName, strOutput, strFileLocation:=strFileLocation)

        End If
        Erase strOutput
        Erase strAryOutput
        objFSO = Nothing

    End Sub

    Private Sub Sanitize_Array(strAryOutput(,) As String, strOutput() As String)
        Dim i As Integer
        Dim j As Integer
        Dim maxi As Integer
        Dim maxj As Integer
        Dim strTemp As String

        If strAryOutput.IsAllocated() Then
            maxi = UBound(strAryOutput, 1)
            maxj = UBound(strAryOutput, 2)
            ReDim strOutput(maxj)

            For j = 1 To maxj
                For i = 1 To maxi
                    strTemp = strAryOutput(i, j)
                    strTemp = Replace$(Trim$(strTemp), vbTab, "")
                    strTemp = Replace$(strTemp, vbCrLf, " ")
                    strTemp = Replace$(strTemp, vbLf, " ")
                    strAryOutput(i, j) = strTemp
                    strOutput(j) = strOutput(j) & vbTab & strTemp
                Next i
                strOutput(j) = Right(strOutput(j), Len(strOutput(j)) - 1)
            Next j
        Else
            MsgBox("Empty Array in Sub Sanitize_Array")
        End If
    End Sub

    Private Sub UpdateForMove(MSG As MailItem, fldr As String, CTFList As CtfIncidenceList)
        Dim Inc_Num As Integer
        Dim i, j As Integer
        Dim tmp_CTF_Map As Conversation_To_Folder
        Dim tmpCCT, tmpFDR As String
        Dim updated As Boolean


        updated = False
        Inc_Num = CTFList.CTF_Incidence_FIND(MSG.ConversationID)                        'Check to see if the conversation id is already in the incidence matrix

        If Inc_Num = 0 Then                                                     'If it is not in the matrix,
            CTFList.CTF_Inc_Ct += 1                                         'increase matrix record count
            ReDim Preserve CTFList.CTF_Inc(CTFList.CTF_Inc_Ct)                                  'and expand the matrix

            tmp_CTF_Map.Email_Conversation_Count = 1
            tmp_CTF_Map.Email_Conversation_ID = MSG.ConversationID
            tmp_CTF_Map.Email_Folder = fldr

            Call CTFList.CTF_Incidence_INIT(CTFList.CTF_Inc_Ct)                                 'Initialize Variable
            Call CTFList.CTF_Incidence_SET(CTFList.CTF_Inc_Ct, 1, 1, tmp_CTF_Map)                'Map Variable in top position

        Else

            With CTFList.CTF_Inc(Inc_Num)

                For i = 1 To .Folder_Count
                    If .Email_Folder(i) = fldr Then
                        .Email_Conversation_Count(i) = 1 + .Email_Conversation_Count(i)
                        updated = True
                        If i > 1 Then
                            For j = i To 2 Step -1
                                If .Email_Conversation_Count(j) > .Email_Conversation_Count(j - 1) Then
                                    tmpCCT = .Email_Conversation_Count(j)
                                    tmpFDR = .Email_Folder(j)
                                    .Email_Conversation_Count(j) = .Email_Conversation_Count(j - 1)
                                    .Email_Folder(j) = .Email_Folder(j - 1)
                                    .Email_Conversation_Count(j - 1) = tmpCCT
                                    .Email_Folder(j - 1) = tmpFDR
                                Else
                                    Exit For
                                End If
                            Next j
                        End If
                        If updated = True Then Exit For
                    End If
                Next i

            End With

            If updated = False Then

                With tmp_CTF_Map
                    .Email_Conversation_Count = 1
                    .Email_Conversation_ID = MSG.ConversationID
                    .Email_Folder = fldr
                End With

                Call CTFList.CTF_Inc_Position_ADD(Inc_Num, tmp_CTF_Map)                     'If it is in the matrix, add it in the right slot

            End If

        End If

        Call Subject_Map_Add(MSG.Subject, fldr)
    End Sub

    'Private Sub Add_Recent(sortFolder As String)
    '    Throw New NotImplementedException()
    'End Sub

    'Private Sub SaveAttachmentsFromSelection(strFolderPath As String, v As Boolean, Optional value As Object = Nothing, Optional selItems As IList = Nothing)
    '    Throw New NotImplementedException()
    'End Sub

    'Private Sub SaveAttachmentsFromSelection(SavePath As String, Verify_Action As Boolean, selItems As IList, save_images As Boolean, SaveMSG As Boolean)
    '    Throw New NotImplementedException()
    'End Sub

    Private Sub SaveMessageAsMSG(fileSystem_LOC As String, selItems As IList)
        Throw New NotImplementedException()
    End Sub

    Private Function GetCurrentExplorerFolder(ActiveExplorer As Outlook.Explorer, Optional objItem As Object = Nothing) As Folder
        If objItem Is Nothing Then
            objItem = ActiveExplorer.Selection.Item(0)
        End If

        If TypeOf objItem Is MailItem Then
            Dim OlMail As MailItem = objItem
            Return OlMail.Parent

        ElseIf TypeOf objItem Is AppointmentItem Then
            Dim OlAppointment As Outlook.AppointmentItem = objItem
            Return OlAppointment.Parent

        ElseIf TypeOf objItem Is MeetingItem Then
            Dim OlMeeting As Outlook.MeetingItem = objItem
            Return OlMeeting.Parent

        ElseIf TypeOf objItem Is TaskItem Then
            Dim OlTask As TaskItem = objItem
            Return OlTask.Parent

        Else
            Return Nothing
        End If

    End Function

    Public Sub Cleanup_Files()
        Throw New NotImplementedException
    End Sub

    'Public Function DialogueThrowNotImplemented() As Boolean
    '    Return MsgBox("")
    'End Function

End Module
