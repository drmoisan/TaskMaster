Option Explicit On
Imports System.Diagnostics
Imports System.Numerics

Module BaseChanger
    Public Function ConvertToBase(nbase As Integer, ByVal num As BigInteger, Optional intMinDigits As Integer = 2) As String
        Dim chars As String
        Dim r As Long
        Dim newNumber As String
        Dim maxBase As Integer
        Dim i As Integer

        chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
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
                num /= nbase
            End While

            newNumber = Mid(chars, num + 1, 1) & newNumber

            For i = 1 To (Len(newNumber) Mod intMinDigits)
                newNumber = CStr(0) & newNumber
            Next i

            ConvertToBase = newNumber
        End If
    End Function

    Public Function ConvertToDecimal(nbase As Integer, ByVal strBase As String) As BigInteger
        Dim chars As String
        Dim i As Long
        Dim lngLoc As Long
        Dim lngTmp As Long
        Dim bigint As BigInteger = New BigInteger

        chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
        bigint.Equals(0)

        Try
            For i = 1 To Len(strBase)
                bigint *= nbase
                lngLoc = InStr(chars, Mid(strBase, i, 1))
                bigint += lngLoc - 1
            Next i
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Debug.WriteLine(ex.Source)
            Debug.WriteLine(ex.StackTrace)
            Debug.WriteLine("")

        End Try

        ConvertToDecimal = lngTmp
    End Function

End Module
