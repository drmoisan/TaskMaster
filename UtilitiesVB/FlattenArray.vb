Public Module FlattenArray
    Public Function FlattenArry(varBranch() As Object) As String
        'CLEANUP: Move to a library 
        Dim i As Integer
        Dim strTemp As String

        strTemp = ""

        For i = 0 To UBound(varBranch)
            strTemp = If(IsArray(varBranch(i)), strTemp & ", " & FlattenArry(varBranch(i)), DirectCast(strTemp & ", " & varBranch(i), String))
        Next i
        If strTemp.Length <> 0 Then strTemp = Right(strTemp, Len(strTemp) - 2)
        FlattenArry = strTemp
    End Function
End Module
