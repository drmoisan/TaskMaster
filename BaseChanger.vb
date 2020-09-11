Option Explicit On

Module BaseChanger
    Public Function ConvertToBase(nbase As Integer, ByVal num As Long, Optional intMinDigits As Integer = 2) As String
        Dim chars As String
        Dim r As Long
        Dim newNumber As String
        Dim maxBase As Integer
        Dim i As Integer

        chars = "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
        maxBase = Len(chars)

        ' check if we can convert to this base
        If (nbase > maxBase) Then
            ConvertToBase = ""
        Else

            ' in r we have the offset of the char that was converted to the new base
            newNumber = ""
            While num >= nbase
                r = num Mod nbase
                newNumber = Mid(chars, r + 1, 1) & newNumber
                num \= nbase
            End While

            newNumber = Mid(chars, num + 1, 1) & newNumber

            For i = 1 To (Len(newNumber) Mod intMinDigits)
                newNumber = CStr(0) & newNumber
            Next i

            ConvertToBase = newNumber
        End If
    End Function

    Public Function ConvertToDecimal(nbase As Integer, ByVal strBase As String) As Long
        Dim chars As String
        Dim i As Integer
        Dim intLoc As Integer
        Dim lngTmp As Long

        chars = "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
        lngTmp = 0

        For i = 1 To Len(strBase)
            lngTmp *= nbase
            intLoc = InStr(chars, Mid(strBase, i, 1))
            lngTmp += intLoc - 1
        Next i

        ConvertToDecimal = lngTmp
    End Function

End Module
