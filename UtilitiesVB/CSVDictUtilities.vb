Imports System.IO
Imports Microsoft.VisualBasic.FileIO

Public Module CSVDictUtilities

    Public Function LoadDictCSV(stagingPath, filename) As Dictionary(Of String, String)
        Dim filepath As String = Path.Combine(stagingPath, filename)
        Dim dictString As New Dictionary(Of String, String)

        Try
            Using MyReader As New TextFieldParser(filepath)
                MyReader.TextFieldType = FileIO.FieldType.Delimited
                MyReader.SetDelimiters(",")

                Dim currentRow As String()
                While Not MyReader.EndOfData
                    Try
                        currentRow = MyReader.ReadFields()
                        Dim key As Object = currentRow(0)
                        Dim value As Object = currentRow(1)
                        dictString.Add(key, value)
                    Catch ex As MalformedLineException
                        Dim unused2 = MsgBox("Line " & ex.Message &
                    "is not valid and will be skipped.")
                    End Try
                End While
            End Using
        Catch e As FileNotFoundException
            Dim unused1 = MsgBox("File not found error -> " & filepath)
        Catch e As FieldAccessException
            Dim unused = MsgBox("File is in use -> " & filepath)
        End Try

        Return dictString

    End Function

    Public Sub WriteDictCSV(dict_str As Dictionary(Of String, String),
                            staging_path As String,
                            filename As String)
        Dim filepath As String = Path.Combine(staging_path, filename)
        Dim csv As String = String.Join(
            Environment.NewLine,
            dict_str.[Select](Function(d) $"{d.Key};{d.Value};"))
        System.IO.File.WriteAllText(filepath, csv)
    End Sub

End Module
