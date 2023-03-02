Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary

Public Module IDListUtilities
    Public Function LoadIDList(filePath) As IDListClass
        Dim IDList As IDListClass
        If File.Exists(filePath) Then

            Dim deserializer As New BinaryFormatter
            Try
                Using TestFileStream As Stream = File.OpenRead(filePath)
                    IDList = CType(deserializer.Deserialize(TestFileStream), IDListClass)
                End Using
            Catch ex As UnauthorizedAccessException
                MsgBox("Unexpected Access Error. Duplicate Instance Running?")
                Throw ex
            Catch ex As IOException
                MsgBox("Unexpected IO Error. Is IDList File Corrupt?")
                Throw ex
            End Try

            IDList.pFileName = filePath

            Return IDList
        Else
            IDList = New IDListClass(New List(Of String))
            IDList.RePopulate()
            IDList.Save(filePath)
            Return IDList
        End If
    End Function
End Module
