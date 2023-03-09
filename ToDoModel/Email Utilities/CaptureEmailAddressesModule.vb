Imports Microsoft.Office.Interop.Outlook

Public Module CaptureEmailAddressesModule
    Public Function CaptureEmailAddresses(OlMail As MailItem,
                                          emailRootFolder As String,
                                          dictRemap As Dictionary(Of String, String)) _
                                          As List(Of String)
        Dim i As Integer
        Dim j As Integer
        Dim strAddresses() As String
        Dim blContains As Boolean
        Dim emailAddressList As New List(Of String)

        Dim strEmail() As String = CaptureEmailDetails(OlMail, emailRootFolder, dictRemap)

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

End Module
