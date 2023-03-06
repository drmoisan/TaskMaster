Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports Microsoft.Office.Interop.Outlook

Public Module IDListUtilities
    Public Function LoadIDList(filePath As String, Application As Application) As ListOfIDs
        Dim IDList As ListOfIDs
        If File.Exists(filePath) Then

            Dim deserializer As New BinaryFormatter
            Try
                Using TestFileStream As Stream = File.OpenRead(filePath)
                    IDList = CType(deserializer.Deserialize(TestFileStream), ListOfIDs)
                End Using
            Catch ex As UnauthorizedAccessException
                Dim unused1 = MsgBox("Unexpected Access Error. Duplicate Instance Running?")
                Throw ex
            Catch ex As IOException
                Dim unused = MsgBox("Unexpected IO Error. Is IDList File Corrupt?")
                Throw ex
            End Try

            IDList.pFileName = filePath

            Return IDList
        Else
            IDList = New ListOfIDs(New List(Of String))
            IDList.RePopulate(Application)
            IDList.Save(filePath)
            Return IDList
        End If
    End Function
End Module
