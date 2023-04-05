Imports Microsoft
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip
Imports UtilitiesVB
Imports System.IO

Public Module SaveAttachmentsModule
    Public Function SaveAttachmentsFromSelection(AppGlobals As IApplicationGlobals,
                                                 SavePath As String,
                                                 Optional DELFILE As Boolean = False,
                                                 Optional Verify_Action As Boolean = False,
                                                 Optional selItems As IList = Nothing,
                                                 Optional save_images As Boolean = False,
                                                 Optional SaveACopy As Boolean = False,
                                                 Optional SaveMSG As Boolean = False,
                                                 Optional blVerifySaveDuplicate As Boolean = True) As Long

        Dim _globals As IApplicationGlobals = AppGlobals
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

        'Dim Response            As Variant      ' Response to user input

        blnIsEnd = False
        blnIsSave = False
        blnFolderExists = True
        lCountAllItems = 0
        atmtct = 0
        FileExtExists = False

        '    On Error Resume Next


        If IsMissing(selItems) Then
            selItems = _globals.Ol.App.ActiveExplorer.Selection
        End If


        ' Get the handle of Outlook window.
        lhWnd = FindWindow(olAppCLSN, vbNullString)

        If lhWnd <> 0 Then
            strFolderPath = SavePath & "\" ''New ADDED BY DAN TO BYPASS FOLDER SELECTION

            'Check to see if destination directory exists on the file system
            'If it doesn't, ask the user what to do with it
            If Not Directory.Exists(strFolderPath) Then
                If strFolderNotToCreate = strFolderPath Then
                    blnFolderExists = False
                Else
                    Load(FolderNotFoundAction)
                    FolderNotFoundAction.FolderName.Caption = strFolderPath
                    FolderNotFoundAction.Show
                    Select Case FolderNotFoundAction.FolderAction
                        Case "Create"
                            objNewFolder = Email_SortToNewFolder.MakePath(strFolderPath)
                        Case "Find"
                        Case "NoToAll"
                            strFolderNotToCreate = strFolderPath
                            blnFolderExists = False
                        Case Else
                            blnFolderExists = False

                    End Select
                    Unload(FolderNotFoundAction)
                End If
            End If

            ' /* Go through each item in the selection. */

            If blnFolderExists Then
                For Each objItem In selItems
                    If TypeOf objItem Is Outlook.MailItem Then
                        MSG = objItem
                        lCountEachItem = objItem.Attachments.Count
                        emailDate = objItem.SentOn
                        emailDate2 = DateAdd("d", 1, emailDate) 'Add a day to catch error from flow
                        DteString = Format(emailDate, "yymmdd")
                        DteString2 = Format(emailDate2, "yymmdd") 'Add a day to catch error from flow

                        If SaveMSG = True Then
                            If MSG.Subject <> "" Then
                                strAtmtFullName = MSG.Subject
                                ReplaceCharsForFileName(strAtmtFullName, "-")
                                strAtmtPath = strFolderPath & DteString & " " & strAtmtFullName
                                MSG.SaveAs(strAtmtPath, 3)
                            End If
                            '
                            'If objFSO.FileExists(strAtmtPath) = True Then
                        End If

                        ' /* If the current item contains attachments. */
                        If lCountEachItem > 0 Then
                            Atmts = objItem.Attachments
                            For Each Atmt In Atmts
                                atmtct = atmtct + 1

                                AlreadyExists = False
                                ' Get the full name of the current attachment.
                                If Atmt.Type <> olOLE Then
                                    strAtmtFullName = Atmt.FileName
                                Else
                                    strAtmtFullName = "NOTHING"
                                End If

                                ' Is there a dot in the file extension?
                                If InStrRev(strAtmtFullName, ".") <> 0 Then
                                    FileExtExists = True

                                    ' Find the dot postion in atmtFullName.
                                    intDotPosition = InStrRev(strAtmtFullName, ".")

                                    ' Get the name.
                                    strAtmtName(0) = Left$(strAtmtFullName, intDotPosition - 1)

                                    ' Get the file extension.
                                    strAtmtName(1) = Right$(strAtmtFullName, Len(strAtmtFullName) - intDotPosition)

                                Else
                                    FileExtExists = False
                                    strAtmtName(0) = strAtmtFullName
                                    strAtmtName(1) = "NONE"
                                End If


                                ' Get the full saving path of the current attachment.
                                strAtmtPath = strFolderPath & DteString & " " & strAtmtFullName
                                strAtmtPath2 = strFolderPath & DteString2 & " " & strAtmtFullName

                                ' /* If the length of the saving path is not larger than 260 characters.*/
                                If Len(strAtmtPath) <= MAX_PATH Then
                                    ' True: This attachment can be saved.
                                    If (save_images = True Or (UCase(strAtmtName(1)) <> "PNG" And UCase(strAtmtName(1)) <> "JPG" And UCase(strAtmtName(1)) <> "GIF")) Then
                                        'True: Not a picture
                                        If DELFILE = True Then
                                            If objFSO.FileExists(strAtmtPath) = True Then
                                                objFSO.DeleteFile(strAtmtPath)
                                            ElseIf objFSO.FileExists(strAtmtPath2) = True Then

                                                objFSO.DeleteFile(strAtmtPath2)
                                            End If
                                            blnIsSave = False

                                        Else
                                            blnIsSave = True

                                            ' /* Loop until getting the file name which does not exist in the folder. */
                                            Do While objFSO.FileExists(strAtmtPath)
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
                                        End If

                                        ' /* Save the current attachment if it is a valid file name. */
                                        If blnIsSave Then
                                            If Verify_Action = True Then

                                                objMailItem = objItem

                                                If ResponseOverwriteFile + ResponseSaveFile = 0 Then
                                                    objMailItem.Display()
                                                End If


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
                                            If (response = vbYes1 Or response = vbYesToAll) Then Atmt.SaveAsFile(strAtmtPath)
                                        End If
                                    End If
                                Else
                                    lCountEachItem = lCountEachItem - 1
                                End If
                            Next
                        End If

                        ' Count the number of attachments in all Outlook items.
                        lCountAllItems = lCountAllItems + lCountEachItem
                    Else
                    End If
                Next
            ElseIf (strFolderNotToCreate = strFolderPath) Then
            Else
                MsgBox("Canceled save due to non-existant folder")
            End If
            ''End If
        Else
            MsgBox("Failed to get the handle of Outlook window!", vbCritical, "Error from Attachment Saver")
            blnIsEnd = True
        End If

        ' /* For run-time error:
        '    The Explorer has been closed and cannot be used for further operations.
        '    Review your code and restart Outlook. */
        Else
        MsgBox("Please select an Outlook item at least.", vbExclamation, "Message from Attachment Saver")
        blnIsEnd = True
        End If

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
            MsgBox(CStr(lNum) & " attachment(s) was(were) saved successfully.", vbInformation, "Message from Attachment Saver")
        Else
            MsgBox("No attachment(s) in the selected Outlook items.", vbInformation, "Message from Attachment Saver")
        End If
    End Sub


    Private Sub FolderNotFound()

    End Sub



    Function openFileSystemDialog() As String
        Dim fd As Office.FileDialog

        fd = Application.FileDialog(msoFileDialogFilePicker)

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
