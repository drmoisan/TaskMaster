Public Module FileIO2
    Public Function DELETE_TextFile(strFileName As String, strFileLocation As String) As Boolean
        Dim objFSO As Object


        objFSO = CreateObject("Scripting.FileSystemObject")
        If objFSO.FileExists(strFileLocation & "\" & strFileName) = True Then
            objFSO.DeleteFile(strFileLocation & "\" & strFileName)
        End If

        DELETE_TextFile = True
        objFSO = Nothing

    End Function

End Module
