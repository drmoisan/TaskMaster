Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

Public Module ExtToChar
    <Extension()>
    Public Function ToChar(ByVal key As Keys) As Char
        Dim c As Char = vbNullChar

        If (key >= Keys.A) AndAlso (key <= Keys.Z) Then
            c = ChrW(AscW("a"c) + (key - Keys.A))
        ElseIf (key >= Keys.D0) AndAlso (key <= Keys.D9) Then
            c = ChrW(AscW("0"c) + (key - Keys.D0))
        End If

        Return c
    End Function
End Module
