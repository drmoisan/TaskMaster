using Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{

    public static class Calendar
    {
        public static Folder GetCalendar(string CalendarName, NameSpace Session)
        {
            TraceUtility.LogMethodCall(CalendarName, Session);

            Folders OlCalendars;
            Folder foundCalendar = null;

            OlCalendars = Session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar).Folders;
            foreach (Folder OlCalendar in OlCalendars)
            {
                if ((OlCalendar.Name ?? "") == (CalendarName ?? ""))
                    foundCalendar = OlCalendar;
            }

            return foundCalendar;

        }
    }
}