Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary

<Serializable()>
Public Class ProjectList

    Public ProjectDictionary As Dictionary(Of String, String)

    Public Sub New(ByVal dictProjectList As Dictionary(Of String, String))
        Me.ProjectDictionary = dictProjectList
    End Sub

End Class

