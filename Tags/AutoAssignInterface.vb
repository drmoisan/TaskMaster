Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

Public Interface IAutoAssign

    Function AutoFind(objItem As Object) As IList(Of String)

    Function AddChoicesToDict(olMail As Microsoft.Office.Interop.Outlook.MailItem,
                              prefixes As List(Of IPrefix),
                              prefixKey As String) As IList(Of String)

    Function AddColorCategory(prefix As IPrefix, categoryName As String) As Category

    ReadOnly Property FilterList As List(Of String)

End Interface
