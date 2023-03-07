Imports System.Xml
Imports System.Xml.Schema

Public Interface IPeopleDict
    Inherits IDictionary
    Sub ReadXml(reader As XmlReader)
    Sub WriteXml(writer As XmlWriter)
    Function GetSchema() As XmlSchema
End Interface
