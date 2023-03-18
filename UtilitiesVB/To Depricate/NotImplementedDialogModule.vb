Private Module NotImplementedDialogModule
    Private Delegate Function ResponseAction() As String

    Public Function StopAtNotImplemented(functionName As String) As String
        Dim map = New Dictionary(Of String, [Delegate])
        map.Add("Throw Exception", New ResponseAction(ThrowException()))
        Return MsgBox("Function " & functionName & " is not implemented. Throw exception or keep running?")
    End Function

    Public Function ThrowException() As String
        Return "ThrowException"
    End Function

    Private Function ContinueRunning() As MsgBoxResult
        Return "KeepRunning"
    End Function
End Module
