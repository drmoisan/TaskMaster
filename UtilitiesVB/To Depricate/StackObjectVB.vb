Imports System.Collections.Generic

Public Class StackObjectVB
    Private _colObj As New Collection

    Public Sub Push(obj As Object)
        _colObj.Add(obj)
    End Sub

    Public Function Pop(Optional idx As Integer = 0) As Object
        Dim objTmp As Object
        If idx = 0 Then idx = _colObj.Count
        If idx > 0 Then
            objTmp = _colObj(idx)
            _colObj.Remove(idx)
            Pop = objTmp
        Else
            Pop = Nothing
        End If
    End Function

    Public Function Count() As Integer
        Count = _colObj.Count
    End Function

    Public Function ToCollection() As Collection
        ToCollection = _colObj
    End Function

    Public Function ToList() As List(Of Object)
        Dim listObj As New List(Of Object)()
        For Each objItem In _colObj
            listObj.Add(objItem)
        Next
        Return listObj
    End Function



End Class
