Imports System.Text.RegularExpressions
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports Tags
Imports UtilitiesVB

Public Module AutoFile
    Private Const NumberOfFields = 13

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

    Public Function AreConversationsGrouped(ActiveExplorer As Outlook.Explorer) As Boolean
        Dim blTemp As Boolean
        If ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations") Then
            blTemp = True
        Else
            blTemp = False
        End If

        AreConversationsGrouped = blTemp
    End Function


    Public Function AutoFindPeople(objItem As Object,
                                   ppl_dict As Dictionary(Of String, String),
                                   emailRootFolder As String,
                                   dictRemap As Dictionary(Of String, String),
                                   Optional blNotifyMissing As Boolean = True,
                                   Optional blExcludeFlagged As Boolean = True) As Collection
        Dim OlMail As [MailItem]
        Dim emailAddressList As List(Of String)
        Dim colPPL As New Collection
        Dim strMissing As String = ""
        Dim strTmp As String

        If TypeOf objItem Is MailItem Then
            OlMail = objItem
            If Mail_IsItEncrypted(OlMail) = False Then
                emailAddressList = CaptureEmailAddresses(OlMail, emailRootFolder, dictRemap)
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
                    Dim unused = MsgBox("Recipients not in list of people: " & strMissing)
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

    Public Delegate Sub DictPPL_Save()

    Public Function dictPPL_AddMissingEntries(OlMail As Outlook.MailItem,
                                              ppl_dict As Dictionary(Of String, String),
                                              prefixes As List(Of IPrefix),
                                              prefixKey As String,
                                              emailRootFolder As String,
                                              stagingPath As String,
                                              dictRemap As Dictionary(Of String, String),
                                              filename_dictppl As String,
                                              dictPPLSave As DictPPL_Save) As Collection

        Dim addressList As New List(Of String)
        Dim strTmp3 As String
        Dim blNew As Boolean = False
        'Dim catTmp As Outlook.Category
        Dim colReturnCatNames As New Collection
        Dim objRegex As Regex
        Dim _viewer As TagViewer
        Dim dictNAMES As SortedDictionary(Of String, Boolean)

        dictNAMES = ppl_dict.GroupBy(Function(x) x.Value) _
            .ToDictionary(Function(y) y.Key, Function(z) False) _
            .ToSortedDictionary()

        If Mail_IsItEncrypted(OlMail) = False Then
            addressList = CaptureEmailAddresses(OlMail, emailRootFolder, dictRemap)
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
                Dim selections As New List(Of String) From {
                    newPplTag
                }

                'Check if it is a new address for existing contact
                _viewer = New TagViewer

                Dim _controller As New TagController(viewer_instance:=_viewer,
                                                     dictOptions:=dictNAMES,
                                                     autoAssigner:=Nothing,
                                                     prefixes:=prefixes,
                                                     selections:=selections,
                                                     prefix_key:=prefixKey,
                                                     objItemObject:=OlMail) With {
                    .ButtonNewActive = False,
                    .ButtonAutoAssignActive = False
                                                     }
                _controller.SetSearchText(newPplTag)

                Dim unused = _viewer.ShowDialog()
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
            dictPPLSave()
            'WriteDictPPL(Path.Combine(stagingPath, filename_dictppl), ppl_dict)
        End If


        Return colReturnCatNames

    End Function
End Module
