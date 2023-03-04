Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary

Public Module ToDoProjectInfoUtilities
    Public Function LoadToDoProjectInfo(filePath As String) As ProjectInfo
        Dim ProjInfo As ProjectInfo
        If File.Exists(filePath) Then
            'Dim TestFileStream As Stream = File.OpenRead(filePath)
            Dim deserializer As New BinaryFormatter
            Try
                Using TestFileStream As Stream = File.OpenRead(filePath)
                    ProjInfo = CType(deserializer.Deserialize(TestFileStream), ProjectInfo)
                End Using
            Catch ex As UnauthorizedAccessException
                MsgBox("Unexpected Access Error. Duplicate Instance Running?")
                Throw ex
            Catch ex As IOException
                MsgBox("Unexpected IO Error. Is Project Info File Corrupt?")
                Throw ex
            End Try

            ProjInfo.FileName = filePath
            ProjInfo.Sort()
            Return ProjInfo
        Else
            ProjInfo = New ProjectInfo
            ProjInfo.Save(filePath)
            Return ProjInfo
        End If

    End Function
End Module
