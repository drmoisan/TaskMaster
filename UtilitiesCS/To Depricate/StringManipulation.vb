Imports UtilitiesCS
Imports System.Reflection

Public Module StringManipulation
    Public Function GetStrippedText(strTmp As String) As String
        If NotImplementedDialog.StopAtNotImplemented(MethodBase.GetCurrentMethod().Name) Then
            Throw New NotImplementedException
        End If
        Return strTmp
    End Function
End Module
