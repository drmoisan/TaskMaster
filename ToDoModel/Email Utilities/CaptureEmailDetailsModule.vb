Imports Microsoft.Office.Interop.Outlook

Public Module CaptureEmailDetailsModule
    Private Const NumberOfFields = 13
    Private ReadOnly dict_remap As Dictionary(Of String, String)

    Public Function CaptureEmailDetails(OlMail As MailItem,
                                       emailRootFolder As String,
                                       Optional dictRemap As Dictionary(Of String, String) _
                                       = Nothing) As String()
        Dim strAry() As String

        ReDim strAry(NumberOfFields)

        Const PR_SMTP_ADDRESS As String =
        "http://schemas.microsoft.com/mapi/proptag/0x39FE001E"

        Const PR_LAST_VERB_EXECUTED As String = "http://schemas.microsoft.com/mapi/proptag/0x10810003"

        strAry(1) = GetTriage(OlMail)
        strAry(2) = GetEmailFolderPath(OlMail, emailRootFolder)
        strAry(3) = Format(OlMail.SentOn, "YYYY-MM-DD\Th:mm:ss\+\0\0\:\0\0")

        Dim recipients = GetRecipients(OlMail, PR_SMTP_ADDRESS)
        strAry(5) = recipients.recipientsTo
        strAry(6) = recipients.recipientsCC
        strAry(4) = GetSenderAddress(OlMail, PR_SMTP_ADDRESS)
        strAry(7) = OlMail.Subject
        strAry(8) = OlMail.Body
        strAry(9) = Right(strAry(4), Len(strAry(4)) - InStr(strAry(4), "@"))
        strAry(10) = OlMail.ConversationID
        strAry(11) = OlMail.EntryID
        strAry(12) = GetAttachmentNames(OlMail)
        strAry(13) = GetActionTaken(OlMail, PR_LAST_VERB_EXECUTED)

        Return strAry

    End Function

    Private Function GetActionTaken(OlMail As MailItem, PR_LAST_VERB_EXECUTED As String) As String
        Dim lngLastVerbExec As Integer
        Const Last_Verb_Reply_All = 103
        Const Last_Verb_Reply_Sender = 102
        Const Last_Verb_Reply_Forward = 104
        Dim action As String

        If OlMail.IsMarkedAsTask = True Then
            action = "Task"
        Else
            Dim OlPA As PropertyAccessor = OlMail.PropertyAccessor

            Try
                Dim prop_tmp_int As Integer = OlPA.GetProperty(PR_LAST_VERB_EXECUTED)
                lngLastVerbExec = If(prop_tmp_int <> 0, prop_tmp_int, 0)
            Catch
                lngLastVerbExec = 0
            End Try

            Select Case lngLastVerbExec
                Case Last_Verb_Reply_All, Last_Verb_Reply_Sender, Last_Verb_Reply_Forward
                    action = "Acted"
                Case Else
                    action = "None"
            End Select
        End If

        Return action
    End Function

    Private Function GetAttachmentNames(OlMail As MailItem) As String
        Dim IntAttachment_Ct As Integer
        Dim attachmentNames As String = ""

        IntAttachment_Ct = OlMail.Attachments.Count
        If IntAttachment_Ct > 0 Then
            Dim OlAtmts As Attachments = OlMail.Attachments
            For Each OlAtmt As Attachment In OlAtmts
                If OlAtmt.Type <> OlAttachmentType.olOLE Then
                    attachmentNames = attachmentNames & "; " & OlAtmt.FileName
                End If
            Next OlAtmt
            If Len(attachmentNames) > 2 Then
                attachmentNames = Right(attachmentNames, Len(attachmentNames) - 2)
            End If
        End If
        Return attachmentNames
    End Function

    Private Function GetSenderAddress(OlMail As MailItem, PR_SMTP_ADDRESS As String) As String
        Dim senderAddress As String

        If OlMail.Sender.Type = "EX" Then
            Dim OlPA As PropertyAccessor = OlMail.Sender.PropertyAccessor
            Try
                senderAddress = OlPA.GetProperty(PR_SMTP_ADDRESS)
            Catch
                Try
                    senderAddress = OlMail.Sender.Name
                Catch
                    senderAddress = ""
                End Try
            End Try

        Else
            senderAddress = OlMail.SenderEmailAddress
        End If
        Return senderAddress
    End Function

    Private Function GetEmailFolderPath(OlMail As MailItem, emailRootFolder As String) As String
        Dim OlParent As Folder = OlMail.Parent
        Dim folderPath As String = OlParent.FolderPath
        Dim root_length As Integer = Len(emailRootFolder)
        If Len(folderPath) > root_length Then
            folderPath = Right(folderPath, Len(folderPath) - root_length - 1)

            'If folder has been remapped, put the target folder
            If dict_remap IsNot Nothing Then
                If dict_remap.ContainsKey(folderPath) Then
                    folderPath = dict_remap(folderPath)
                End If
            End If
        End If
        Return folderPath
    End Function

    Private Function GetTriage(OlMail As MailItem) As String
        Dim OlProperty As UserProperty = OlMail.UserProperties.Find("Triage")
        Return If(OlProperty Is Nothing, "", DirectCast(OlProperty.Value, String))
    End Function

    Private Function GetRecipients(OlMail As MailItem,
                                   PR_SMTP_ADDRESS As String) As _
                                   (recipientsTo As String,
                                   recipientsCC As String)

        Dim StrSMTPAddress As String
        Dim OlRecipients As Recipients
        Dim OlRecipient As Recipient
        Dim recipientsTo As String = ""
        Dim recipientsCC As String = ""

        OlRecipients = OlMail.Recipients

        For Each OlRecipient In OlRecipients
            StrSMTPAddress = ExtractRecipient(PR_SMTP_ADDRESS, OlRecipient)

            If OlRecipient.Type = OlMailRecipientType.olTo Then
                recipientsTo = recipientsTo & "; " & StrSMTPAddress
            ElseIf OlRecipient.Type = OlMailRecipientType.olCC Then
                recipientsCC = recipientsCC & "; " & StrSMTPAddress
            End If
        Next OlRecipient

        'Trim off extra semicolon if any values were set
        If Len(recipientsCC) > 2 Then recipientsCC = Right(recipientsCC, Len(recipientsCC) - 2)
        If Len(recipientsTo) > 2 Then recipientsTo = Right(recipientsTo, Len(recipientsTo) - 2)

        Return (recipientsTo, recipientsCC)
    End Function

    Private Function ExtractRecipient(PR_SMTP_ADDRESS As String, OlRecipient As Recipient) As String
        Dim OlPA As PropertyAccessor = OlRecipient.PropertyAccessor
        Dim StrSMTPAddress As String
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
        Return StrSMTPAddress
    End Function
End Module
