Imports Microsoft
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip

Public Module SaveAttachmentsModule
    Public Function SaveAttachmentsFromSelection(SavePath As String,
    Optional DELFILE As Boolean = False,
    Optional Verify_Action As Boolean = False,
    Optional selItems As Variant,
    Optional save_images As Boolean = False,
    Optional SaveACopy As Boolean = False,
    Optional SaveMSG As Boolean = False,
    Optional blVerifySaveDuplicate = True) As Long

        'Procedure Naming
        Dim SubNm As String
        SubNm = "SaveAttachmentsFromSelection"

        '************Standard Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim ttrace As String
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
        ttrace = "Inside " & SubNm

        '*******************END Error Header*********************
        TraceStack.Push SubNm & " VARIABLES SavePath:=" & SavePath & " DELFILE:=" & DELFILE

    Dim objFSO As Object       ' Computer's file system object.
        Dim objShell As Object       ' Windows Shell application object.
        Dim objFolder As Object       ' The selected folder object from Browse for Folder dialog box.
        Dim objSubFolders As Object
        Dim objNewFolder As Object
        Dim objItem As Object       ' A specific member of a Collection object either by position or by key.
        'Dim selItems            As Selection    ' A collection of Outlook item objects in a folder.
        Dim Atmt As Attachment   ' A document or link to a document contained in an Outlook item.
        Dim strAtmtPath As String       ' The full saving path of the attachment.
        Dim strAtmtPath2 As String       ' The full saving path of the attachment. (With error for FLOW of +1 day)
        Dim strAtmtFullName As String       ' The full name of an attachment.
        Dim strAtmtName(1) As String       ' strAtmtName(0): to save the name; strAtmtName(1): to save the file extension. They are separated by dot of an attachment file name.
        Dim strAtmtNameTemp As String       ' To save a temporary attachment file name.
        Dim intDotPosition As Integer      ' The dot position in an attachment name.
        Dim Atmts As Attachments  ' A set of Attachment objects that represent the attachments in an Outlook item.
        Dim lCountEachItem As Long         ' The number of attachments in each Outlook item.
        Dim lCountAllItems As Long         ' The number of attachments in all Outlook items.
        Dim strFolderPath As String       ' The selected folder path.
        Dim blnIsEnd As Boolean      ' End all code execution.
        Dim blnIsSave As Boolean      ' Consider if it is need to save.
        Dim emailDate As Date         ' Holds date of email
        Dim emailDate2 As Date         ' Holds date of email + 1 day for error in FLOW
        Dim DteString As String       ' Holds string portion of time stamp of email
        Dim DteString2 As String       ' Holds string portion of time stamp of email plus one day (for error in FLOW)
        Dim objMailItem As MailItem
        Dim AlreadyExists As Boolean      ' Checks to see whether file already exists
        Dim atmtct As Integer      ' Error to count number of traces through attachment loop
        Dim FileExtExists As Boolean      ' Boolean value to check if file extension exists
        Dim MSG As Outlook.MailItem
        Dim blnFolderExists As Boolean
        Dim response As YesNoToAll
        Static ResponseSaveFile As YesNoToAll
        Static ResponseOverwriteFile As YesNoToAll

        'Dim Response            As Variant      ' Response to user input

        blnIsEnd = False
        blnIsSave = False
        blnFolderExists = True
        lCountAllItems = 0
        atmtct = 0
        FileExtExists = False
        errRaised = False

        '    On Error Resume Next

        ttrace = ttrace & vbCrLf & SubNm & "If IsMissing(selItems) Then "

        If IsMissing(selItems) Then
            ttrace = ttrace & vbCrLf & SubNm & "If IsMissing(selItems) Then IS TRUE"
            ttrace = ttrace & vbCrLf & SubNm & "Set selItems = Application.ActiveExplorer.Selection"
        Set selItems = Application.ActiveExplorer.Selection
    End If

        If Err.Number = 0 Then

            ' Get the handle of Outlook window.
            ttrace = ttrace & vbCrLf & SubNm & "Get handle of outlook window"
            lhWnd = FindWindow(olAppCLSN, vbNullString)

            ttrace = ttrace & vbCrLf & SubNm & "If handle <> 0"
            If lhWnd <> 0 Then
                ttrace = ttrace & vbCrLf & SubNm & "If handle <> 0 IS TRUE"
                ' /* Create a Shell application object to pop-up BrowseForFolder dialog box. */
                ttrace = ttrace & vbCrLf & SubNm & "Create a Shell application object"
            Set objShell = CreateObject("Shell.Application")
            Set objFSO = CreateObject("Scripting.FileSystemObject")
                            
                
                                        ttrace = ttrace & vbCrLf & SubNm & "strFolderPath = SavePath & '\'"
                strFolderPath = SavePath & "\" ''New ADDED BY DAN TO BYPASS FOLDER SELECTION

                'Check to see if destination directory exists on the file system
                'If it doesn't, ask the user what to do with it
                If Not DirectoryExists(strFolderPath) Then
                    If strFolderNotToCreate = strFolderPath Then
                        blnFolderExists = False
                    Else
                        Load FolderNotFoundAction
                    FolderNotFoundAction.FolderName.Caption = strFolderPath
                        FolderNotFoundAction.Show
                        Select Case FolderNotFoundAction.FolderAction
                            Case "Create"
                            Set objNewFolder = Email_SortToNewFolder.MakePath(strFolderPath)
                        Case "Find"
                            Case "NoToAll"
                                strFolderNotToCreate = strFolderPath
                                blnFolderExists = False
                            Case Else
                                blnFolderExists = False

                        End Select
                        Unload FolderNotFoundAction
                  End If
                End If

                ' /* Go through each item in the selection. */
                ttrace = ttrace & vbCrLf & SubNm & "For Each objItem In selItems"

                If blnFolderExists Then
                    For Each objItem In selItems
                        If TypeOf objItem Is Outlook.MailItem Then
                    Set MSG = objItem
                                        ttrace = ttrace & vbCrLf & SubNm & "lCountEachItem = objItem.Attachments.Count"
                            lCountEachItem = objItem.Attachments.Count
                            ttrace = ttrace & vbCrLf & SubNm & "lCountEachItem = " & lCountEachItem

                            ttrace = ttrace & vbCrLf & SubNm & "emailDate = objItem.SentOn"
                            emailDate = objItem.SentOn

                            ttrace = ttrace & vbCrLf & SubNm & "Add a day to catch error from flow"
                            emailDate2 = DateAdd("d", 1, emailDate) 'Add a day to catch error from flow
                            ttrace = ttrace & vbCrLf & SubNm & "Format 2 dates for string"
                            DteString = Format(emailDate, "yymmdd")
                            DteString2 = Format(emailDate2, "yymmdd") 'Add a day to catch error from flow

                            If SaveMSG = True Then
                                If MSG.Subject <> "" Then
                                    strAtmtFullName = MSG.Subject
                                    ReplaceCharsForFileName strAtmtFullName, "-"
                            strAtmtPath = strFolderPath & DteString & " " & strAtmtFullName
                                    MSG.SaveAs strAtmtPath, 3
                        End If
                                '
                                'If objFSO.FileExists(strAtmtPath) = True Then
                            End If

                            ' /* If the current item contains attachments. */
                            ttrace = ttrace & vbCrLf & SubNm & "If lCountEachItem > 0 Then"
                            If lCountEachItem > 0 Then
                                ttrace = ttrace & vbCrLf & SubNm & "If lCountEachItem > 0 Then IS TRUE"
                                ttrace = ttrace & vbCrLf & SubNm & "Set atmts = objItem.Attachments"
                        Set Atmts = objItem.Attachments
                        
                        ' /* Go through each attachment in the current item. */
                                        ttrace = ttrace & vbCrLf & SubNm & "For Each atmt In atmts"
                                For Each Atmt In Atmts
                                    atmtct = atmtct + 1
                                    ttrace = ttrace & vbCrLf & SubNm & "For Each atmt In atmts - Loop " & atmtct

                                    AlreadyExists = False
                                    ' Get the full name of the current attachment.
                                    ttrace = ttrace & vbCrLf & SubNm & "If atmt.Type <> olOLE ... " & Atmt.Type
                                    If Atmt.Type <> olOLE Then
                                        ttrace = ttrace & vbCrLf & SubNm & "strAtmtFullName = atmt.FileName"
                                        ttrace = ttrace & vbCrLf & SubNm & "atmt.FileName IS " & Atmt.filename
                                        strAtmtFullName = Atmt.filename
                                    Else
                                        ttrace = ttrace & vbCrLf & SubNm & "atmt.Type = olOLE"
                                        strAtmtFullName = "NOTHING"
                                    End If

                                    ' Is there a dot in the file extension?
                                    ttrace = ttrace & vbCrLf & SubNm & "Is there a dot in the file extension?"
                                    If InStrRev(strAtmtFullName, ".") <> 0 Then
                                        ttrace = ttrace & vbCrLf & SubNm & "Is there a dot in the file extension? TRUE"
                                        FileExtExists = True

                                        ' Find the dot postion in atmtFullName.
                                        ttrace = ttrace & vbCrLf & SubNm & "Find the dot position in atmtFullName"
                                        intDotPosition = InStrRev(strAtmtFullName, ".")

                                        ' Get the name.
                                        ttrace = ttrace & vbCrLf & SubNm & "strAtmtName(0) = Left$(strAtmtFullName, intDotPosition - 1)"
                                        strAtmtName(0) = Left$(strAtmtFullName, intDotPosition - 1)

                                        ' Get the file extension.
                                        ttrace = ttrace & vbCrLf & SubNm & "strAtmtName(1) = Right$(strAtmtFullName, Len(strAtmtFullName) - intDotPosition)"
                                        strAtmtName(1) = Right$(strAtmtFullName, Len(strAtmtFullName) - intDotPosition)

                                    Else
                                        ttrace = ttrace & vbCrLf & SubNm & "Is there a dot in the file extension? FALSE"
                                        FileExtExists = False
                                        strAtmtName(0) = strAtmtFullName
                                        strAtmtName(1) = "NONE"
                                    End If


                                    ' Get the full saving path of the current attachment.
                                    ttrace = ttrace & vbCrLf & SubNm & "Get the full saving path of the current attachment."
                                    strAtmtPath = strFolderPath & DteString & " " & strAtmtFullName
                                    strAtmtPath2 = strFolderPath & DteString2 & " " & strAtmtFullName

                                    ' /* If the length of the saving path is not larger than 260 characters.*/
                                    ttrace = ttrace & vbCrLf & SubNm & "If the length of the saving path is not larger than 260 characters"
                                    If Len(strAtmtPath) <= MAX_PATH Then
                                        ' True: This attachment can be saved.
                                        ttrace = ttrace & vbCrLf & SubNm & "TRUE -> This attachment can be saved"
                                        ttrace = ttrace & vbCrLf & SubNm & "If attachment is not an image OR we are saving images, do..."
                                        If (save_images = True Or (UCase(strAtmtName(1)) <> "PNG" And UCase(strAtmtName(1)) <> "JPG" And UCase(strAtmtName(1)) <> "GIF")) Then
                                            'True: Not a picture
                                            ttrace = ttrace & vbCrLf & SubNm & "TRUE -> Not an image or saving images"

                                            ttrace = ttrace & vbCrLf & SubNm & "If DELFILE"
                                            If DELFILE = True Then
                                                ttrace = ttrace & vbCrLf & SubNm & "If DELFILE = True IS TRUE"
                                                ttrace = ttrace & vbCrLf & SubNm & "If file exists, delete it"
                                                If objFSO.FileExists(strAtmtPath) = True Then
                                                    objFSO.DeleteFile strAtmtPath
                                        ElseIf objFSO.FileExists(strAtmtPath2) = True Then

                                                    objFSO.DeleteFile strAtmtPath2
                                        End If
                                                blnIsSave = False

                                            Else
                                                ttrace = ttrace & vbCrLf & SubNm & "If DELFILE = True IS FALSE"
                                                blnIsSave = True

                                                ' /* Loop until getting the file name which does not exist in the folder. */
                                                ttrace = ttrace & vbCrLf & SubNm & "Do While objFSO.FileExists(strAtmtPath)"
                                                Do While objFSO.FileExists(strAtmtPath)
                                                    ttrace = ttrace & vbCrLf & SubNm & "Inside Do While objFSO.FileExists(strAtmtPath)"
                                                    AlreadyExists = True

                                                    strAtmtNameTemp = strAtmtName(0) &
                                                                  Format(Now, "_mmddhhmmss") &
                                                                  Format(Timer * 1000 Mod 1000, "000")
                                                    strAtmtPath = strFolderPath & DteString & strAtmtNameTemp
                                                    If FileExtExists Then strAtmtPath = strAtmtPath & "." & strAtmtName(1)

                                                    ' /* If the length of the saving path is over 260 characters.*/
                                                    If Len(strAtmtPath) > MAX_PATH Then
                                                        lCountEachItem = lCountEachItem - 1
                                                        ' False: This attachment cannot be saved.
                                                        blnIsSave = False
                                                        Exit Do
                                                    End If
                                                Loop
                                                ttrace = ttrace & vbCrLf & SubNm & "PASSED Do While objFSO.FileExists(strAtmtPath)"
                                            End If

                                            ' /* Save the current attachment if it is a valid file name. */
                                            ttrace = ttrace & vbCrLf & SubNm & "Save the current attachment if it is a valid file name"
                                            If blnIsSave Then
                                                ttrace = ttrace & vbCrLf & SubNm & "If Verify_Action = True Then"
                                                ttrace = ttrace & vbCrLf & SubNm & "Verify_Action value = " & Verify_Action
                                                If Verify_Action = True Then

                                                    ttrace = ttrace & vbCrLf & SubNm & "Set objMailItem = objItem"
                                            Set objMailItem = objItem
                                            
                                            ttrace = ttrace & vbCrLf & SubNm & "objMailItem.Display"
                                                    If ResponseOverwriteFile + ResponseSaveFile = 0 Then
                                                        objMailItem.Display
                                                    End If

                                                    ttrace = ttrace & vbCrLf & SubNm & "If AlreadyExists = True Then"

                                                    If AlreadyExists = True Then
                                                        'Response = MsgBox("File Already Exists. Save file: " & strAtmtPath, vbCritical + vbYesNo)
                                                        If ResponseOverwriteFile = vbNull Then
                                                            response = MsgBox_YesNoToAll("File Already Exists. Save file: " & strAtmtPath)
                                                            If response = vbNoToAll Or response = vbYesToAll Then ResponseOverwriteFile = response
                                                        Else
                                                            response = ResponseOverwriteFile
                                                        End If
                                                    Else
                                                        'Response = MsgBox("Save file: " & strAtmtPath, vbYesNo + vbExclamation)
                                                        If ResponseSaveFile = vbNull Then
                                                            response = MsgBox_YesNoToAll("Save file: " & strAtmtPath)
                                                            If response = vbNoToAll Or response = vbYesToAll Then ResponseSaveFile = response
                                                        Else
                                                            response = ResponseSaveFile

                                                        End If
                                                    End If

                                                    If response = vbYes1 Or response = vbYesToAll Then
                                                        strAtmtName(0) = InputBox("Email Subject: " & MSG.Subject & vbCrLf & "Rename file: " & strAtmtPath, , strAtmtName(0))
                                                        If strAtmtName(0) = "" Then
                                                            If MsgBox("Revert to file name: " & strAtmtPath, vbOKCancel) = vbCancel Then response = vbNo1
                                                        Else
                                                            strAtmtPath = strFolderPath & DteString & " " & strAtmtName(0)
                                                            If FileExtExists Then strAtmtPath = strAtmtPath & "." & strAtmtName(1)
                                                        End If
                                                    End If

                                                    objMailItem.Close(olDiscard)
                                                Else
                                                    response = vbYes1
                                                End If
                                                If (response = vbYes1 Or response = vbYesToAll) Then Atmt.SaveAsFile strAtmtPath
                                    End If
                                        End If
                                    Else
                                        ttrace = ttrace & vbCrLf & SubNm & "lCountEachItem = lCountEachItem - 1"
                                        lCountEachItem = lCountEachItem - 1
                                    End If
                                Next
                            End If

                            ' Count the number of attachments in all Outlook items.
                            ttrace = ttrace & vbCrLf & SubNm & "lCountAllItems = lCountAllItems + lCountEachItem"
                            lCountAllItems = lCountAllItems + lCountEachItem
                        Else
                            ttrace = ttrace & vbCrLf & SubNm & "NOT A MAIL ITEM"
                        End If
                    Next
                ElseIf strFolderNotToCreate = strFolderPath Then
                Else
                    MsgBox "Canceled save due to non-existant folder"
                End If
                ''End If
            Else
                ttrace = ttrace & vbCrLf & SubNm & "Failed to get the handle of Outlook window!"
                MsgBox "Failed to get the handle of Outlook window!", vbCritical, "Error from Attachment Saver"
            blnIsEnd = True
                GoTo PROC_EXIT
            End If

            ' /* For run-time error:
            '    The Explorer has been closed and cannot be used for further operations.
            '    Review your code and restart Outlook. */
        Else
            ttrace = ttrace & vbCrLf & SubNm & "Please select an Outlook item at least."
            MsgBox "Please select an Outlook item at least.", vbExclamation, "Message from Attachment Saver"
        blnIsEnd = True
        End If

PROC_EXIT:

        SaveAttachmentsFromSelection = lCountAllItems

        ' /* Release memory. */
        If errRaised Then Deactivate_Email_Timing_And_Velocity
        If errRaised Then Debug.Print SubNm & "Release memory"
    If Not (objFSO Is Nothing) Then Set objFSO = Nothing
    If Not (objItem Is Nothing) Then Set objItem = Nothing
    'If Not (selItems Is Nothing) Then Set selItems = Nothing
    If Not (Atmt Is Nothing) Then Set Atmt = Nothing
    If Not (Atmts Is Nothing) Then Set Atmts = Nothing
    
    ' /* End all code execution if the value of blnIsEnd is True. */
    'If blnIsEnd Then End
    
    If errRaised Then Debug.Print SubNm & "Exiting Function"
    Temp = SF_Stack.Pop
        Exit Function

ErrorHandler:
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "Variable Dumps"
    TraceStack.Push "SavePath = " & SavePath
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source

    Debug.Print ttrace
    Debug.Print "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    Debug.Print "Variable Dumps"
    Debug.Print "SavePath = " & SavePath

    errRaised = True
        ttrace = ""
        'pauseToDebug
        Deactivate_Email_Timing_And_Velocity

        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        Stop
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            errRaised = False
            Resume
        End If
        'End
    End Function

    ' #####################
    ' Convert general path.
    ' #####################
    Public Function CGPath(ByVal path As String) As String
        If Right(path, 1) <> "\" Then path = path & "\"
        CGPath = path
    End Function

    ' ######################################
    ' Run this macro for saving attachments.
    ' ######################################
    Private Sub ExecuteSavingDirect(SavePath As String)
        Dim lNum As Long

        lNum = SaveAttachmentsFromSelection(SavePath)

        If lNum > 0 Then
            MsgBox CStr(lNum) & " attachment(s) was(were) saved successfully.", vbInformation, "Message from Attachment Saver"
    Else
            MsgBox "No attachment(s) in the selected Outlook items.", vbInformation, "Message from Attachment Saver"
    End If
    End Sub


    Private Sub FolderNotFound()

    End Sub


    Private Sub othcode()
        ''Set objFolder = objShell.BrowseForFolder(lHwnd, "Select folder to save attachments:", _
        BIF_RETURNONLYFSDIRS +BIF_DONTGOBELOWDOMAIN, CSIDL_DESKTOP)
            
            ' /* Failed to create the Shell application. */
            ''If Err.Number <> 0 Then
            ''    MsgBox "Run-time error '" & CStr(Err.Number) & " (0x" & CStr(Hex(Err.Number)) & ")':" & vbNewLine & _
            ''           Err.Description & ".", vbCritical, "Error from Attachment Saver"
            ''    blnIsEnd = True
            ''    GoTo PROC_EXIT
            ''End If
            
            ''If objFolder Is Nothing Then
            ''    strFolderPath = ""
            ''    blnIsEnd = True
            ''    GoTo PROC_EXIT
            ''Else
            ''    strFolderPath = CGPath(objFolder.Self.Path)

End Sub

    Function openFileSystemDialog() As String
        Dim fd As Office.FileDialog

    Set fd = Application.FileDialog(msoFileDialogFilePicker)

   With fd

            .AllowMultiSelect = False

            ' Set the title of the dialog box.
            .Title = "Please select the file."

            ' Clear out the current filters, and add our own.
            '.Filters.Clear
            '.Filters.Add "Excel 2003", "*.xls"
            '.Filters.Add "All Files", "*.*"

            ' Show the dialog box. If the .Show method returns True, the
            ' user picked at least one file. If the .Show method returns
            ' False, the user clicked Cancel.
            If .Show = True Then
                openFileSystemDialog = .SelectedItems(1) 'replace txtFileName with your textbox
            Else
                openFileSystemDialog = ""
            End If
        End With
    End Function
End Module
