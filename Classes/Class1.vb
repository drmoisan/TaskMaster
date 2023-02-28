Imports System

Public NotInheritable Class CustomCollection(Of T)
    Inherits IList(Of T)
    Private wrappedCollection As IList(Of T)

    Public Sub New(ByVal wrappedCollection As IList(Of T))
        If wrappedCollection Is Nothing Then
            Throw New ArgumentNullException("wrappedCollection")
        End If
        Me.wrappedCollection = wrappedCollection
    End Sub

    ' "hide" methods that don't make sense by explicitly implementing them and
    ' throwing a NotSupportedException
    Private Sub RemoveAt(ByVal index As Integer)
        Throw New NotSupportedException()
    End Sub

    ' Implement methods that do make sense by passing the call to the wrapped collection
    Public Sub Add(ByVal item As T)
        wrappedCollection.Add(item)
    End Sub
End Class
