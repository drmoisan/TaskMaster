Public Interface OldIPrefix
    Property People As String
    Property Project As String
    Property Topic As String
    Property Context As String
    Property Today As String
    Property Bullpin As String
    Property KB As String
    Property elements As Dictionary(Of String, String)
    Property Default_Task_Length As Integer
End Interface

Public Interface IPrefix
    Property Key As String
    Property Value As String
    Property Color As Microsoft.Office.Interop.Outlook.OlCategoryColor
End Interface

