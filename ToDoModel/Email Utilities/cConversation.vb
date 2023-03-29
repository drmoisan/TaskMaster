Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook

Public Class cConversation
    Private _item As Object
    Private pConversation As Outlook.Conversation
    Private pTable As Outlook.Table
    Private pCollection As Collection
    Private _olApp As Outlook.Application
    'Private Const PR_STORE_ENTRYID As String = "https://schemas.microsoft.com/mapi/proptag/0x0FFB0102"
    'Private Const FOLDERNAME As String = "http://schemas.microsoft.com/mapi/proptag/0x0e05001f"

    Public Sub New(OlApp As Outlook.Application)
        _olApp = OlApp
    End Sub

    Public WriteOnly Property item
        Set(value)
            _item = value
            pConversation = value.GetConversation
            If pConversation IsNot Nothing Then
                pTable = pConversation.GetTable
                pTable.Columns.Add("http://schemas.microsoft.com/mapi/proptag/0x0e05001f")
                _item = value
            End If
        End Set
    End Property

    Public Sub Enumerate()
        Dim oRow As Outlook.Row
        Do Until pTable.EndOfTable
            oRow = pTable.GetNextRow
            ' Use EntryID and StoreID to open the item.
            Debug.Print(oRow("Subject"))
            Debug.Print(oRow("http://schemas.microsoft.com/mapi/proptag/0x0e05001f"))
        Loop
    End Sub

    Public ReadOnly Property Count(Optional OnlySameFolder As Boolean = False) As Long
        Get
            If _item IsNot Nothing Then
                If OnlySameFolder Then
                    pCollection = ToCollection(OnlySameFolder)
                    Count = pCollection.Count
                Else
                    Count = pTable.GetRowCount
                End If
            Else
                Count = 0
            End If
        End Get
    End Property

    Public ReadOnly Property ToList(Optional OnlySameFolder As Boolean = False, Optional MailOnly As Boolean = True) As IList
        Get
            If Not _item Is Nothing Then
                Dim oRow As Outlook.Row
                Dim objItem As Object
                Dim listObjects As New List(Of Object)
                Dim listEmail As New List(Of MailItem)
                pTable.Sort("[ReceivedTime]", True)

                Do Until pTable.EndOfTable
                    oRow = pTable.GetNextRow
                    ' Use EntryID and StoreID to open the item.
                    objItem = _olApp.Session.GetItemFromID(oRow("EntryID"))
                    If MailOnly Then
                        AddEmailToList(OnlySameFolder, objItem, listEmail)
                    Else
                        AddObjectToList(OnlySameFolder, objItem, listObjects)
                    End If
                Loop

                If MailOnly Then
                    Return listEmail
                Else
                    Return listObjects
                End If

            Else
                Return Nothing
            End If

        End Get
    End Property

    Private Sub AddObjectToList(OnlySameFolder As Boolean, objItem As Object, listObjects As List(Of Object))
        If OnlySameFolder Then
            If objItem.Parent.Name = _item.Parent.Name Then
                listObjects.Add(objItem)
            End If
        Else
            listObjects.Add(objItem)
        End If
    End Sub

    Private Sub AddEmailToList(OnlySameFolder As Boolean, objItem As Object, ByRef listEmails As List(Of MailItem))
        If TypeOf objItem Is MailItem Then
            If OnlySameFolder Then
                If objItem.Parent.Name = _item.Parent.Name Then
                    listEmails.Add(objItem)
                End If
            Else
                listEmails.Add(objItem)
            End If
        End If
    End Sub

    Public ReadOnly Property ToCollection(Optional OnlySameFolder As Boolean = False) As Collection
        Get
            If _item IsNot Nothing Then
                Dim oRow As Outlook.Row
                Dim objItem As Object
                pCollection = New Collection
                pTable.Sort("[ReceivedTime]", True)

                Do Until pTable.EndOfTable
                    oRow = pTable.GetNextRow
                    ' Use EntryID and StoreID to open the item.
                    objItem = _olApp.Session.GetItemFromID(oRow("EntryID"))
                    If OnlySameFolder Then
                        If objItem.Parent.Name = _item.Parent.Name Then
                            pCollection.Add(objItem)
                        End If
                    Else
                        pCollection.Add(objItem)
                    End If
                Loop
                Return pCollection
            Else
                Return Nothing
            End If
        End Get
    End Property



    Private Sub DemoConversationTable()
        Dim oConv As Outlook.Conversation
        Dim oTable As Outlook.Table
        Dim oRow As Outlook.Row
        Dim oMail As Outlook.MailItem
        Dim oItem As Outlook.MailItem
        Const PR_STORE_ENTRYID As String = "https://schemas.microsoft.com/mapi/proptag/0x0FFB0102"

        On Error Resume Next
        ' Obtain the current item for the active inspector.
        oMail = _olApp.ActiveInspector.CurrentItem

        If oMail IsNot Nothing Then
            ' Obtain the Conversation object.
            oConv = oMail.GetConversation
            If oConv IsNot Nothing Then
                oTable = oConv.GetTable
                Dim unused = oTable.Columns.Add(PR_STORE_ENTRYID)
                Do Until oTable.EndOfTable
                    oRow = oTable.GetNextRow
                    ' Use EntryID and StoreID to open the item.
                    oItem = _olApp.Session.GetItemFromID(
                   oRow("EntryID"),
                   oRow.BinaryToString(PR_STORE_ENTRYID))
                    Debug.Print(oItem.Subject,
                    "Attachments.Count=" & oItem.Attachments.Count)
                Loop
            End If
        End If
    End Sub


End Class
