Public Class cStackObject
    Private ColObj As Collection

    Private Sub Class_Initialize()
        ColObj = New Collection
    End Sub

    Public Sub Push(obj As Object)
        ColObj.Add(obj)
    End Sub

    Public Function Pop(Optional idx As Integer = 0) As Object
        Dim objTmp As Object
        If idx = 0 Then idx = ColObj.Count
        If idx > 0 Then
            objTmp = ColObj(idx)
            ColObj.Remove(idx)
            Pop = objTmp
        Else
            Pop = Nothing
        End If
    End Function

    Public Function Count() As Integer
        Count = ColObj.Count
    End Function

    Public Function ToCollection() As Collection
        ToCollection = ColObj
    End Function
    Private Sub Class_Terminate()
        ColObj = Nothing
    End Sub

End Class
