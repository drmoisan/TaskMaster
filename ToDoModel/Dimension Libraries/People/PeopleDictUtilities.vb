Imports System.IO
Imports System.Xml.Serialization
Imports UtilitiesVB

Public Module PeopleDictUtilities
    Public Sub WriteDictPPL(filepath As String, ppldict As Dictionary(Of String, String))
        Dim serializer As XmlSerializer = New XmlSerializer(GetType(PeopleDict(Of String, String)))
        Dim textWriter As TextWriter = New StreamWriter(filepath)
        serializer.Serialize(textWriter, ppldict)
        textWriter.Close()
    End Sub

    Public Function GetDictPPL(staging_path As String, filename As String) As PeopleDict(Of String, String)
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

End Module
