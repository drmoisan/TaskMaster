Imports System.IO
Imports System.Runtime
Imports System.Xml.Serialization
Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.VisualBasic.FileIO

Module Util
    Public Sub WriteDictPPL(filepath As String, ppldict As Dictionary(Of String, String))
        Dim serializer As XmlSerializer = New XmlSerializer(GetType(PeopleDict(Of String, String)))
        Dim textWriter As TextWriter = New StreamWriter(filepath)
        serializer.Serialize(textWriter, ppldict)
        textWriter.Close()
    End Sub

    Public Function GetDict(staging_path As String, filename As String) As Dictionary(Of String, String)
        Dim filepath As String = Path.Combine(staging_path, filename)
        Dim dict_return As PeopleDict(Of String, String)

        If File.Exists(filepath) Then
            dict_return = LoadDictPPL_XML(filepath)
        Else
            Dim tmpresult As MsgBoxResult = MsgBox(
                filepath & " was not loaded. Load from legacy csv?",
                vbYesNo)
            If tmpresult = MsgBoxResult.Yes Then
                Dim filename2 As String = Left(filename, Len(filename) - 3) & "csv"
                Dim tmp_dict As Dictionary(Of String, String) = LoadDictCSV(staging_path, filename2)
                dict_return = New PeopleDict(Of String, String)(tmp_dict)
                WriteDictPPL_XML(dict_return, filepath)
            Else
                dict_return = New PeopleDict(Of String, String)()
            End If
        End If

        Return dict_return
    End Function

    Public Function LoadDictCSV(staging_path, filename) As Dictionary(Of String, String)
        Dim filepath As String = Path.Combine(staging_path, filename)
        Dim dict_string As Dictionary(Of String, String) = New Dictionary(Of String, String)

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
                        dict_string.Add(key, value)
                    Catch ex As MalformedLineException
                        MsgBox("Line " & ex.Message &
                    "is not valid and will be skipped.")
                    End Try
                End While
            End Using
        Catch e As FileNotFoundException
            MsgBox("File not found error -> " & filepath)
        Catch e As FieldAccessException
            MsgBox("File is in use -> " & filepath)
        End Try

        Return dict_string

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

    Private Function LoadDictPPL_XML(filepath As String) As PeopleDict(Of String, String)
        Dim serializer As XmlSerializer = New XmlSerializer(GetType(PeopleDict(Of String, String)))
        Dim dictPPL As PeopleDict(Of String, String)
        Using textReader As TextReader = New StreamReader(filepath)
            dictPPL =
            CType(serializer.Deserialize(textReader),
            PeopleDict(Of String, String))
        End Using
        Return dictPPL
    End Function

    Private Sub WriteDictPPL_XML(dictPPL As PeopleDict(Of String, String), filepath As String)
        Dim serializer As XmlSerializer = New XmlSerializer(GetType(PeopleDict(Of String, String)))
        Using textWriter As TextWriter = New StreamWriter(filepath)
            serializer.Serialize(textWriter, dictPPL)
        End Using
    End Sub

    Public Function Mail_IsItEncrypted(item As mailItem) As Boolean

        If item.MessageClass = "IPM.Note.SMIME" Or item.MessageClass = "IPM.Note.Secure" Or item.MessageClass = "IPM.Note.Secure.Sign" Or item.MessageClass = "IPM.Outlook.Recall" Then
            Return True
        Else
            Return False
        End If

    End Function

    Public Function SearchSortedDictKeys(
        source_dict As SortedDictionary(Of String, Boolean),
        search_string As String) _
        As SortedDictionary(Of String, Boolean)

        Dim filtered_cats = (From x In source_dict
                             Where x.Key.Contains(search_string)
                             Select x).ToDictionary(
                             Function(x) x.Key,
                             Function(x) x.Value)
        Return New SortedDictionary(Of String, Boolean)(filtered_cats)
    End Function


End Module
