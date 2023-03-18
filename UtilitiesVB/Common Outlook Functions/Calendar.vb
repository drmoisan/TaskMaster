Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook

Public Module Calendar
    Public Function GetCalendar(CalendarName As String, Session As Outlook.NameSpace) As Folder
        Dim OlCalendar As Outlook.Folder
        Dim OlCalendars As Outlook.Folders
        Dim foundCalendar As Outlook.Folder = Nothing

        OlCalendars = Session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar).Folders
        For Each OlCalendar In OlCalendars
            If OlCalendar.Name = CalendarName Then foundCalendar = OlCalendar
        Next OlCalendar

        Return foundCalendar

    End Function
End Module
