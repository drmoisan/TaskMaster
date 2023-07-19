Imports Microsoft.Office.Interop

Public Interface IToDoObj(Of T)
    Property Filename As String
    Property Filepath As String
    Property Folderpath As String
    ReadOnly Property Item As T
    Sub LoadFromFile(Folderpath As String, OlApp As Outlook.Application)
End Interface
