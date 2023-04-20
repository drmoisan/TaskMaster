Imports Microsoft.Office.Interop.Outlook

Public Module MailResolution
    Public Function IsMailUnReadable(item As MailItem) As Boolean
        Return (item.MessageClass = "IPM.Note.SMIME") Or (item.MessageClass = "IPM.Note.Secure") Or (item.MessageClass = "IPM.Note.Secure.Sign") Or (item.MessageClass = "IPM.Outlook.Recall")
    End Function

    Public Function TryResolveMailItemDep(objItem As Object) As MailItem
        Dim OlMail As MailItem = Nothing
        If TypeOf objItem Is MailItem Then
            OlMail = objItem
            If IsMailUnReadable(OlMail) = True Then
                OlMail = Nothing
            End If
        End If
        Return OlMail
    End Function

End Module
