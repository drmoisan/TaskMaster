

<Serializable()>
Public Class ProjectList

    Public ProjectDictionary As Dictionary(Of String, String)

    Public Sub New(ByVal dictProjectList As Dictionary(Of String, String))
        ProjectDictionary = dictProjectList
    End Sub

    Public Sub ToCSV(FileName As String)
        Dim csv As String = String.Join(
            Environment.NewLine,
            ProjectDictionary.[Select](Function(d) $"{d.Key};{d.Value};"))
        System.IO.File.WriteAllText(FileName, csv)
    End Sub

End Class
