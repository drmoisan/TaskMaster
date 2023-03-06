Imports Microsoft.Office.Interop.Outlook

Public Module MailEncrypted
    Public Function Mail_IsItEncrypted(item As MailItem) As Boolean

        Return item.MessageClass = "IPM.Note.SMIME" Or item.MessageClass = "IPM.Note.Secure" Or item.MessageClass = "IPM.Note.Secure.Sign" Or item.MessageClass = "IPM.Outlook.Recall"

    End Function
End Module
