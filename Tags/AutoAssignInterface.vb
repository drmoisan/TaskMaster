Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

Public Interface IAutoAssign

    Function AutoFind(objItem As Object) As Collection

    Function AddChoicesToDict(olMail As Microsoft.Office.Interop.Outlook.MailItem,
                              prefixes As List(Of IPrefix),
                              prefixKey As String) As Collection

    Function AddColorCategory(prefix As IPrefix, categoryName As String) As Category

    ReadOnly Property FilterList As List(Of String)

End Interface
