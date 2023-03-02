Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook

Public Module ExplorerActions
    Function GetCurrentItem(OlApp As Outlook.Application) As Object

        Dim oMail As MailItem
        Dim obj As Object


        On Error Resume Next
        Select Case TypeName(OlApp.ActiveWindow)
            Case "Explorer"
                obj = OlApp.ActiveExplorer.Selection.Item(1)
            Case "Inspector"
                obj = OlApp.ActiveInspector.CurrentItem
            Case Else
                obj = Nothing
        End Select

        If TypeOf obj Is Outlook.MailItem Then
            oMail = obj
            If Mail_IsItEncrypted(oMail) Then
                obj = Nothing
            End If
        End If

        Return obj

    End Function

End Module
