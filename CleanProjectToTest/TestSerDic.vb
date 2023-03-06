'Imports Microsoft.VisualStudio.Services.Common
'Imports System.Xml
'Imports System.Xml.Schema
'Imports System.Xml.Serialization
Imports System.IO
Imports Newtonsoft.Json

Public Module TestSerDic

    'Public Sub WriteDictXML(dict As Dictionary(Of String, String), filepath As String)
    '    Dim serializer As XmlSerializer = New XmlSerializer(GetType(Dictionary(Of String, String)))
    '    Using textWriter As TextWriter = New StreamWriter(filepath)
    '        serializer.Serialize(textWriter, dict)
    '    End Using
    'End Sub

    Public Sub WriteDictJSON(dict As Dictionary(Of String, String), filepath As String)
        File.WriteAllText(filepath, JsonConvert.SerializeObject(dict, Formatting.Indented))
    End Sub

    Public Function GetDictJSON(filepath As String) As Dictionary(Of String, String)
        'JsonConvert.DeserializeObject(Of Movie)(File.ReadAllText("c:\movie.json"))
        Return JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(File.ReadAllText(filepath))
    End Function

    'Public Function GetSerializableDict(staging_path As String, filename As String) As SerializableDictionary(Of String, String)
    '    Dim filepath As String = Path.Combine(staging_path, filename)
    '    Dim dict_return As SerializableDictionary(Of String, String)

    '    If File.Exists(filepath) Then
    '        dict_return = LoadDictPPL_XML(filepath)
    '    Else
    '        Dim tmpresult As MsgBoxResult = MsgBox(
    '            filepath & " was not loaded. Load from legacy csv?",
    '            vbYesNo)
    '        If tmpresult = MsgBoxResult.Yes Then
    '            Dim filename2 As String = Left(filename, Len(filename) - 3) & "csv"
    '            Dim tmp_dict As Dictionary(Of String, String) = LoadDictCSV(staging_path, filename2)
    '            dict_return = New SerializableDictionary(Of String, String)(tmp_dict)
    '            WriteDictPPL_XML(dict_return, filepath)
    '        Else
    '            dict_return = New PeopleDict(Of String, String)()
    '        End If
    '    End If

    '    Return dict_return

    'End Function

    'Public Function LoadDict_XML(filepath As String) As SerializableDictionary(Of String, String)
    '    Dim serializer As XmlSerializer = New XmlSerializer(GetType(SerializableDictionary(Of String, String)))
    '    Dim dict As SerializableDictionary(Of String, String)
    '    Using textReader As TextReader = New StreamReader(filepath)
    '        dict = CType(serializer.Deserialize(textReader), SerializableDictionary(Of String, String))
    '    End Using
    '    Return dict
    'End Function

    'Public Sub WriteDictPPL_XML(dict As SerializableDictionary(Of String, String), filepath As String)
    '    Dim serializer As XmlSerializer = New XmlSerializer(GetType(SerializableDictionary(Of String, String)))
    '    Using textWriter As TextWriter = New StreamWriter(filepath)
    '        serializer.Serialize(textWriter, dict)
    '    End Using
    'End Sub

End Module



'<XmlRoot("PeopleDictionary")>
'Public Class PeopleDict2(Of TKey, TValue)
'    Inherits SerializableDictionary(Of TKey, TValue)


'    Public Sub New(dict As Dictionary(Of TKey, TValue))
'        For Each pair As KeyValuePair(Of TKey, TValue) In dict
'            Me.Add(pair.Key, pair.Value)
'        Next
'    End Sub

'    Public Shadows Sub ReadXml(reader As XmlReader)
'        If reader.IsEmptyElement Then
'            Return
'        End If

'        reader.Read()

'        While reader.NodeType <> XmlNodeType.EndElement
'            Dim key As Object = reader.GetAttribute("Email")
'            Dim value As Object = reader.GetAttribute("Tag")
'            Me.Add(CType(key, TKey), CType(value, TValue))
'            reader.Read()
'        End While
'    End Sub

'    Public Shadows Sub WriteXml(writer As XmlWriter)
'        For Each key In Me.Keys
'            writer.WriteStartElement("Person")
'            writer.WriteAttributeString("Email", key.ToString())
'            writer.WriteAttributeString("Tag", Me(key).ToString())
'            writer.WriteEndElement()
'        Next
'    End Sub



'End Class


