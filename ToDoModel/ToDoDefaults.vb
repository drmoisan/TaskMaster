Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

Public Class ToDoDefaults
    Public Sub New()
        PrefixList = New List(Of IPrefix) From {
            New PrefixItem(key:="People", value:=My.Settings.Prefix_People, color:=OlCategoryColor.olCategoryColorDarkGray),
            New PrefixItem(key:="Project", value:=My.Settings.Prefix_Project, color:=OlCategoryColor.olCategoryColorTeal),
            New PrefixItem(key:="Topic", value:=My.Settings.Prefix_Topic, color:=OlCategoryColor.olCategoryColorDarkTeal),
            New PrefixItem(key:="Context", value:=My.Settings.Prefix_Context, color:=OlCategoryColor.olCategoryColorNone),
            New PrefixItem(key:="Today", value:=My.Settings.Prefix_Today, color:=OlCategoryColor.olCategoryColorDarkRed),
            New PrefixItem(key:="Bullpin", value:=My.Settings.Prefix_Bullpin, color:=OlCategoryColor.olCategoryColorOrange),
            New PrefixItem(key:="KB", value:=My.Settings.Prefix_KB, color:=OlCategoryColor.olCategoryColorRed)
        }
        DefaultTaskLength = My.Settings.Default_Task_Length
    End Sub

    Public ReadOnly Property DefaultTaskLength As Integer

    Public ReadOnly Property PrefixList() As List(Of IPrefix)

End Class

Public Class PrefixItem
    Implements IPrefix

    Public Sub New(key As String, value As String, color As OlCategoryColor)
        Me.Key = key
        Me.Value = value
        Me.Color = color
    End Sub

    Public Property Key As String Implements IPrefix.Key

    Public Property Value As String Implements IPrefix.Value

    Public Property Color As OlCategoryColor Implements IPrefix.Color
End Class