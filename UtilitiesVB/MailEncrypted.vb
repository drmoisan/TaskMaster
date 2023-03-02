Imports Microsoft.Office.Interop.Outlook

Public Module MailEncrypted
    Public Function Mail_IsItEncrypted(item As MailItem) As Boolean

        If item.MessageClass = "IPM.Note.SMIME" Or item.MessageClass = "IPM.Note.Secure" Or item.MessageClass = "IPM.Note.Secure.Sign" Or item.MessageClass = "IPM.Outlook.Recall" Then
            Return True
        Else
            Return False
        End If

    End Function
End Module
