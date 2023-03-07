Imports Microsoft.Office.Interop.Outlook

Public Interface IOlObjects
    ReadOnly Property App As Application
    ReadOnly Property EmailRootPath As String
    ReadOnly Property Inbox As Folder
    ReadOnly Property NamespaceMAPI As [NameSpace]
    ReadOnly Property OlEmailRoot As Folder
    ReadOnly Property OlReminders As Reminders
    ReadOnly Property ToDoFolder As Folder
End Interface
