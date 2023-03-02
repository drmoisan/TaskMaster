

Imports System.Xml
Imports System.Xml.Schema
Imports System.Xml.Serialization


<XmlRoot("PeopleDictionary")>
Public Class PeopleDict(Of TKey, TValue)
    Inherits Dictionary(Of TKey, TValue)
    Implements IXmlSerializable

    Public Sub New(dict As Dictionary(Of TKey, TValue))
        For Each pair As KeyValuePair(Of TKey, TValue) In dict
            Me.Add(pair.Key, pair.Value)
        Next
    End Sub

    Public Sub New()

    End Sub

    Public Sub ReadXml(reader As XmlReader) Implements IXmlSerializable.ReadXml
        If reader.IsEmptyElement Then
            Return
        End If

        reader.Read()

        While reader.NodeType <> XmlNodeType.EndElement
            Dim key As Object = reader.GetAttribute("Email")
            Dim value As Object = reader.GetAttribute("Tag")
            Me.Add(CType(key, TKey), CType(value, TValue))
            reader.Read()
        End While
    End Sub

    Public Sub WriteXml(writer As XmlWriter) Implements IXmlSerializable.WriteXml
        For Each key In Me.Keys
            writer.WriteStartElement("Person")
            writer.WriteAttributeString("Email", key.ToString())
            writer.WriteAttributeString("Tag", Me(key).ToString())
            writer.WriteEndElement()
        Next
    End Sub

    Public Function GetSchema() As XmlSchema Implements IXmlSerializable.GetSchema
        Return Nothing
    End Function

End Class
