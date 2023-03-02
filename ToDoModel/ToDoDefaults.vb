Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

Public Class ToDoDefaults
    Private _list As List(Of IPrefix)
    Private _defaultTaskLength As Integer

    Public Sub New()
        _list = New List(Of IPrefix) From {
            New PrefixItem(key:="People", value:=My.Settings.Prefix_People, color:=OlCategoryColor.olCategoryColorDarkGray),
            New PrefixItem(key:="Project", value:=My.Settings.Prefix_Project, color:=OlCategoryColor.olCategoryColorTeal),
            New PrefixItem(key:="Topic", value:=My.Settings.Prefix_Topic, color:=OlCategoryColor.olCategoryColorDarkTeal),
            New PrefixItem(key:="Context", value:=My.Settings.Prefix_Context, color:=OlCategoryColor.olCategoryColorNone),
            New PrefixItem(key:="Today", value:=My.Settings.Prefix_Today, color:=OlCategoryColor.olCategoryColorDarkRed),
            New PrefixItem(key:="Bullpin", value:=My.Settings.Prefix_Bullpin, color:=OlCategoryColor.olCategoryColorOrange),
            New PrefixItem(key:="KB", value:=My.Settings.Prefix_KB, color:=OlCategoryColor.olCategoryColorRed)
        }
        _defaultTaskLength = My.Settings.Default_Task_Length
    End Sub

    Public ReadOnly Property DefaultTaskLength As Integer
        Get
            Return _defaultTaskLength
        End Get
    End Property

    Public ReadOnly Property PrefixList() As List(Of IPrefix)
        Get
            Return _list
        End Get
    End Property

End Class

Public Class PrefixItem
    Implements IPrefix

    Private _key As String
    Private _value As String
    Private _color As OlCategoryColor

    Public Sub New(key As String, value As String, color As OlCategoryColor)
        _key = key
        _value = value
        _color = color
    End Sub

    Public Property Key As String Implements IPrefix.Key
        Get
            Return _key
        End Get
        Set(value As String)
            _key = value
        End Set
    End Property

    Public Property Value As String Implements IPrefix.Value
        Get
            Return _value
        End Get
        Set(value As String)
            _value = value
        End Set
    End Property

    Public Property Color As OlCategoryColor Implements IPrefix.Color
        Get
            Return _color
        End Get
        Set(value As OlCategoryColor)
            _color = value
        End Set
    End Property
End Class