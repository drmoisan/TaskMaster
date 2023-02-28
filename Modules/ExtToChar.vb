Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

Module ExtToChar
    <Extension()>
    Public Function ToChar(ByVal key As Keys) As Char
        Dim c As Char = vbNullChar

        If (key >= Keys.A) AndAlso (key <= Keys.Z) Then
            c = ChrW((AscW("a"c) + CInt((key - Keys.A))))
        ElseIf (key >= Keys.D0) AndAlso (key <= Keys.D9) Then
            c = ChrW((AscW("0"c) + CInt((key - Keys.D0))))
        End If

        Return c
    End Function
End Module
