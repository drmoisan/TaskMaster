Imports System.Text

Public Module CommonWordsModule
    Public Function StripCommonWords(seedString As String, commonWords As IList(Of String)) As String
        Dim input As StringBuilder = New StringBuilder(seedString)
        For Each word As String In commonWords
            input.Replace(word, "")
        Next
        Return input.ToString()
    End Function


End Module
