Imports System.Runtime.CompilerServices

Public Module ArrayIsAllocated
    <Extension()>
    Public Function IsAllocated(ByVal inArray As System.Array) As Boolean
        Dim FlagEx As Boolean = True
        Try
            If inArray Is Nothing Then
                FlagEx = False
            ElseIf inArray.Length <= 0 Then
                FlagEx = False
            ElseIf inArray(0) Is Nothing Then
                FlagEx = False
            End If
        Catch ex As Exception
            FlagEx = False
        End Try
        Return FlagEx
    End Function
End Module
