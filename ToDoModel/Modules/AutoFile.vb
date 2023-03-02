Imports System.Diagnostics
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip
Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop
Imports System.IO
Imports System.Text.RegularExpressions
Imports UtilitiesVB
Imports Tags

Public Module AutoFile
    Const NumberOfFields = 13
    Private dict_remap As Dictionary(Of String, String)

    Public Function CaptureEmailAddresses(OlMail As MailItem,
                                          emailRootFolder As String,
                                          stagingPath As String) As List(Of String)
        Dim i As Integer
        Dim j As Integer
        Dim strAddresses() As String
        Dim blContains As Boolean
        Dim emailAddressList As List(Of String) = New List(Of String)

        Dim strEmail() As String = CaptureEmailDetails(OlMail, emailRootFolder, stagingPath)

        If IsArray(strEmail) = True Then
            For i = 4 To 6
                If strEmail(i) <> "" Then
                    strAddresses = Split(strEmail(i), "; ")
                    For j = 0 To UBound(strAddresses)
                        blContains = False

                        For Each strTmp In emailAddressList

                            If LCase(Trim(strTmp)) = LCase(Trim(strAddresses(j))) Then
                                blContains = True
                            End If
                        Next strTmp

                        If blContains = False Then
                            If StrComp(strAddresses(j), "dan.moisan@planetpartnership.com", vbTextCompare) <> 0 Then
                                emailAddressList.Add(LCase(Trim(strAddresses(j))))
                            End If
                        End If

                    Next j
                End If
            Next i
        End If
        Return emailAddressList
    End Function

    Public Function CaptureEmailRecipients(OlMail As MailItem) As String()
        Dim strAry() As String
        Dim StrSMTPAddress As String
        Dim OlRecipients As [Recipients]
        Dim OlRecipient As [Recipient]
        Dim StrRecipientName As String
        Dim OlPA As [PropertyAccessor]

        Dim i As Integer


        ReDim strAry(NumberOfFields)

        Const PR_SMTP_ADDRESS As String =
            "http://schemas.microsoft.com/mapi/proptag/0x39FE001E"

        OlRecipients = OlMail.Recipients

        For Each OlRecipient In OlRecipients
            OlPA = OlRecipient.PropertyAccessor
            Try
                StrRecipientName = OlRecipient.Name
            Catch
                StrRecipientName = ""
            End Try

            Try
                StrSMTPAddress = OlPA.GetProperty(PR_SMTP_ADDRESS)
            Catch
                Try
                    StrSMTPAddress = OlRecipient.Address
                Catch
                    StrSMTPAddress = StrRecipientName
                End Try
            End Try



            If OlRecipient.Type = OlMailRecipientType.olTo Then
                strAry(1) = strAry(1) & "; " & StrRecipientName
                strAry(2) = strAry(2) & "; " & StrSMTPAddress
            ElseIf OlRecipient.Type = OlMailRecipientType.olCC Then
                strAry(3) = strAry(3) & "; " & StrRecipientName
                strAry(4) = strAry(4) & "; " & StrSMTPAddress
            End If
            Err.Clear()
        Next OlRecipient

        For i = 1 To 4
            If Len(strAry(i)) > 2 Then strAry(i) = Right(strAry(i), Len(strAry(i)) - 2)
        Next i

        If OlMail.Sender.Type = "EX" Then

            OlPA = OlMail.Sender.PropertyAccessor

            'On Error Resume Next
            Try
                strAry(5) = OlMail.Sender.Name
            Catch
                strAry(5) = ""
            End Try

            Try
                strAry(6) = OlPA.GetProperty(PR_SMTP_ADDRESS)
            Catch
                strAry(6) = strAry(5)
            End Try

        Else
            strAry(5) = OlMail.SenderEmailAddress
            strAry(6) = OlMail.SenderEmailAddress
        End If

        Return strAry

    End Function

    Public Function CaptureEmailDetails(OlMail As MailItem,
                                       emailRootFolder As String,
                                       stagingPath As String) As String()
        Dim IntAttachment_Ct As Integer
        Dim OlAtmts As [Attachments]
        Dim OlAtmt As [Attachment]
        Dim strAry() As String
        Dim StrSMTPAddress As String
        Dim OlRecipients As [Recipients]
        Dim OlRecipient As [Recipient]
        Dim OlPA As [PropertyAccessor]
        Dim OlParent As [Folder]
        Dim OlProperty As [UserProperty]
        Dim lngLastVerbExec As Integer
        Const Last_Verb_Reply_All = 103
        Const Last_Verb_Reply_Sender = 102
        Const Last_Verb_Reply_Forward = 104
        Dim root_length As Integer

        'emailRootFolder = Path.Combine(
        '    Globals.ThisAddIn._OlNS.GetDefaultFolder(OlDefaultFolders.olFolderInbox).FolderPath,
        '    "Archive")

        'Dim stagingPath As String = Globals.ThisAddIn.stagingPath

        ReDim strAry(NumberOfFields)

        Const PR_SMTP_ADDRESS As String =
        "http://schemas.microsoft.com/mapi/proptag/0x39FE001E"

        Const PR_LAST_VERB_EXECUTED As String = "http://schemas.microsoft.com/mapi/proptag/0x10810003"

        'On Error GoTo ErrorHandler

        If dict_remap Is Nothing Then dict_remap = LoadDictCSV(stagingPath, "dictRemap.csv")

        OlProperty = OlMail.UserProperties.Find("Triage")
        If OlProperty Is Nothing Then
            strAry(1) = ""
        Else
            strAry(1) = OlProperty.Value
        End If

        OlParent = OlMail.Parent
        strAry(2) = OlParent.FolderPath
        root_length = Len(emailRootFolder)
        If Len(strAry(2)) > root_length Then
            strAry(2) = Right(strAry(2), Len(strAry(2)) - root_length - 1)

            'If folder has been remapped, put the target folder
            If dict_remap.ContainsKey(strAry(2)) Then
                strAry(2) = dict_remap(strAry(2))
            End If
        End If

        strAry(3) = Format(OlMail.SentOn, "YYYY-MM-DD\Th:mm:ss\+\0\0\:\0\0")

        OlRecipients = OlMail.Recipients

        'On Error Resume Next

        For Each OlRecipient In OlRecipients
            OlPA = OlRecipient.PropertyAccessor
            Try
                StrSMTPAddress = OlPA.GetProperty(PR_SMTP_ADDRESS)
            Catch
                Try
                    StrSMTPAddress = OlRecipient.Address
                Catch
                    Try
                        StrSMTPAddress = OlRecipient.Name
                    Catch
                        StrSMTPAddress = ""
                    End Try
                End Try
            End Try

            If OlRecipient.Type = OlMailRecipientType.olTo Then
                strAry(5) = strAry(5) & "; " & StrSMTPAddress
            ElseIf OlRecipient.Type = OlMailRecipientType.olCC Then
                strAry(6) = strAry(6) & "; " & StrSMTPAddress
            End If

        Next OlRecipient

        If Len(strAry(6)) > 2 Then strAry(6) = Right(strAry(6), Len(strAry(6)) - 2)
        If Len(strAry(5)) > 2 Then strAry(5) = Right(strAry(5), Len(strAry(5)) - 2)

        If OlMail.Sender.Type = "EX" Then
            OlPA = OlMail.Sender.PropertyAccessor

            Try
                strAry(4) = OlPA.GetProperty(PR_SMTP_ADDRESS)
            Catch
                Try
                    strAry(4) = OlMail.Sender.Name
                Catch
                    strAry(4) = ""
                End Try
            End Try

        Else
            strAry(4) = OlMail.SenderEmailAddress
        End If
        strAry(7) = OlMail.Subject
        strAry(8) = OlMail.Body
        strAry(9) = Right(strAry(4), Len(strAry(4)) - InStr(strAry(4), "@"))
        strAry(10) = OlMail.ConversationID
        strAry(11) = OlMail.EntryID

        IntAttachment_Ct = OlMail.Attachments.Count
        If IntAttachment_Ct > 0 Then
            OlAtmts = OlMail.Attachments
            For Each OlAtmt In OlAtmts
                If OlAtmt.Type <> OlAttachmentType.olOLE Then
                    strAry(12) = strAry(12) & "; " & OlAtmt.FileName
                End If
            Next OlAtmt
            If Len(strAry(12)) > 2 Then strAry(12) = Right(strAry(12), Len(strAry(12)) - 2)
        End If

        If OlMail.IsMarkedAsTask = True Then
            strAry(13) = "Task"
        Else
            OlPA = OlMail.PropertyAccessor

            Try
                Dim prop_tmp_int As Integer = OlPA.GetProperty(PR_LAST_VERB_EXECUTED)
                If prop_tmp_int <> 0 Then
                    lngLastVerbExec = prop_tmp_int
                Else
                    lngLastVerbExec = 0
                End If
            Catch
                lngLastVerbExec = 0
            End Try

            Select Case lngLastVerbExec
                Case Last_Verb_Reply_All, Last_Verb_Reply_Sender, Last_Verb_Reply_Forward
                    strAry(13) = "Acted"
                Case Else
                    strAry(13) = "None"
            End Select
        End If

        Return strAry

    End Function

    Public Function AutoFindPeople(objItem As Object,
                                   ppl_dict As PeopleDict(Of String, String),
                                   emailRootFolder As String,
                                   stagingPath As String,
                                   Optional blNotifyMissing As Boolean = True,
                                   Optional blExcludeFlagged As Boolean = True) As Collection
        Dim OlMail As [MailItem]
        Dim emailAddressList As List(Of String)
        Dim colPPL As Collection = New Collection
        Dim strMissing As String = ""
        Dim strTmp As String

        If TypeOf objItem Is MailItem Then
            OlMail = objItem
            If Mail_IsItEncrypted(OlMail) = False Then
                emailAddressList = CaptureEmailAddresses(OlMail, emailRootFolder, stagingPath)
                For i = emailAddressList.Count - 1 To 0 Step -1
                    strTmp = emailAddressList(i)
                    If ppl_dict.ContainsKey(strTmp) Then

                        If blExcludeFlagged Then
                            If Not Category_IsAlreadySelected(objItem, ppl_dict(strTmp)) Then
                                colPPL.Add(ppl_dict(strTmp))
                            End If
                        Else
                            colPPL.Add(ppl_dict(strTmp))
                        End If
                    Else
                        strMissing = strMissing & "; " & strTmp
                    End If
                Next i
                If Len(strMissing) > 0 And blNotifyMissing Then
                    strMissing = Right(strMissing, Len(strMissing) - 2)
                    MsgBox("Recipients not in list of people: " & strMissing)
                End If
            End If
        End If

        Return colPPL
    End Function

    Private Function Category_IsAlreadySelected(objItem As Object, strCat As String) As Boolean
        Dim varCats As String()
        Dim i As Integer
        Dim blSelected As Boolean

        blSelected = False
        varCats = Split(objItem.Categories, ", ")
        For i = 0 To UBound(varCats)
            If strCat = varCats(i) Then
                blSelected = True
            End If
        Next i
        Return blSelected
    End Function

    Public Function dictPPL_AddMissingEntries(OlMail As Outlook.MailItem,
                                              ppl_dict As PeopleDict(Of String, String),
                                              prefixes As List(Of IPrefix),
                                              prefixKey As String,
                                              emailRootFolder As String,
                                              stagingPath As String,
                                              filename_dictppl As String) As Collection

        Dim addressList As List(Of String) = New List(Of String)
        Dim strTmp3 As String
        Dim blNew As Boolean = False
        'Dim catTmp As Outlook.Category
        Dim colReturnCatNames As Collection = New Collection
        Dim objRegex As Regex
        Dim _viewer As TagViewer
        Dim dictNAMES As SortedDictionary(Of String, Boolean)

        dictNAMES = ppl_dict.GroupBy(Function(x) x.Value) _
            .ToDictionary(Function(y) y.Key, Function(z) False) _
            .ToSortedDictionary()

        If Mail_IsItEncrypted(OlMail) = False Then
            addressList = CaptureEmailAddresses(OlMail, emailRootFolder, stagingPath)
        End If

        ' Discard any email addresses from the email that
        ' are already in the people dictionary
        addressList = addressList.Where(Function(x) Not ppl_dict.ContainsKey(x)) _
                                 .Select(Function(x) x) _
                                 .ToList()

        For Each address As String In addressList

            Dim vbR As MsgBoxResult = MsgBox("Add entry for " & address, vbYesNo)
            If vbR = vbYes Then
                objRegex = New Regex("([a-zA-z\d]+)\.([a-zA-z\d]+)@([a-zA-z\d]+)\.com",
                                     RegexOptions.Multiline)

                Dim newPplTag As String = StrConv(objRegex.Replace(address, UCase("$1 $2")), vbProperCase)
                Dim selections As List(Of String) = New List(Of String)
                selections.Add(newPplTag)

                'Check if it is a new address for existing contact
                _viewer = New TagViewer

                Dim _controller As New TagController(viewer_instance:=_viewer,
                                                     dictOptions:=dictNAMES,
                                                     autoAssigner:=Nothing,
                                                     prefixes:=prefixes,
                                                     selections:=selections,
                                                     prefix_key:=prefixKey,
                                                     objItemObject:=OlMail)

                _controller.ButtonNewActive = False
                _controller.ButtonAutoAssignActive = False
                _controller.SetSearchText(newPplTag)

                _viewer.ShowDialog()
                strTmp3 = _controller.SelectionString()

                If strTmp3 <> "" Then
                    ppl_dict.Add(address, strTmp3)
                    blNew = True
                    colReturnCatNames.Add(strTmp3)
                    'Commented out because it seems completely redundant
                    'Else
                    '    newPplTag = InputBox("Enter name for " & address, DefaultResponse:=newPplTag)
                    '    catTmp = CreateCategory(My.Settings.Prefix_People, newPplTag, Globals.ThisAddIn._OlNS)

                    '    If Not catTmp Is Nothing Then
                    '        ppl_dict.Add(address, My.Settings.Prefix_People & newPplTag)
                    '        blNew = True
                    '        colReturnCatNames.Add(My.Settings.Prefix_People & newPplTag)
                    '    End If
                End If
            End If
        Next
        If blNew Then
            WriteDictPPL(Path.Combine(stagingPath, filename_dictppl), ppl_dict)
        End If


        Return colReturnCatNames

    End Function
End Module
