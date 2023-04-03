Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB
Imports System.IO

Public Class AppOlObjects
    Implements IOlObjects

    Private _olEmailRootPath As String
    Private _olArchiveRootPath As String
    Private _movedMails_Stack As StackObjectVB

    Public Sub New(OlApp As Application)
        App = OlApp
    End Sub

    Public ReadOnly Property App As Application Implements IOlObjects.App

    Public ReadOnly Property View_Wide As String Implements IOlObjects.View_Wide
        Get
            Return My.Settings.View_Wide
        End Get
    End Property

    Public ReadOnly Property View_Compact
        Get
            Return My.Settings.View_Wide
        End Get
    End Property

    Public ReadOnly Property NamespaceMAPI As Outlook.NameSpace Implements IOlObjects.NamespaceMAPI
        Get
            Return App.Application.GetNamespace("MAPI")
        End Get
    End Property

    Public ReadOnly Property ToDoFolder As Outlook.Folder Implements IOlObjects.ToDoFolder
        Get
            Return NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
        End Get
    End Property

    Public ReadOnly Property Inbox As Outlook.Folder Implements IOlObjects.Inbox
        Get
            Return NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderInbox)
        End Get
    End Property

    Public ReadOnly Property OlReminders As Outlook.Reminders Implements IOlObjects.OlReminders
        Get
            Return App.Reminders
        End Get
    End Property

    Public ReadOnly Property OlEmailRoot As Folder Implements IOlObjects.OlEmailRoot
        Get
            Return App.Session.DefaultStore.GetRootFolder()
        End Get
    End Property

    Public ReadOnly Property EmailRootPath As String Implements IOlObjects.EmailRootPath
        Get
            If _olEmailRootPath Is Nothing Then
                _olEmailRootPath = OlEmailRoot.FolderPath
            End If
            Return _olEmailRootPath
        End Get
    End Property

    Public ReadOnly Property ArchiveRootPath As String Implements IOlObjects.ArchiveRootPath
        Get
            If _olArchiveRootPath Is Nothing Then
                _olArchiveRootPath = Path.Combine(OlEmailRoot.FolderPath, "Archive")
            End If
            Return _olArchiveRootPath
        End Get
    End Property

    Public Property MovedMails_Stack As StackObjectVB Implements IOlObjects.MovedMails_Stack
        Get
            Return _movedMails_Stack
        End Get
        Set(value As StackObjectVB)
            _movedMails_Stack = value
        End Set
    End Property

    Public Property ShowInConversations As Boolean Implements IOlObjects.ShowInConversations
        Get
            If App.ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations") Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(value As Boolean)
            Dim objView As View = App.ActiveExplorer.CurrentView
            If (value = False) And (App.ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations") = True) Then
                'Turn Off Conversation View
                objView.XML = Replace(objView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, , vbTextCompare)
                objView.Save()
            ElseIf (value = True) And (App.ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations") = False) Then
                'Turn On Conversation View
                Dim strReplace As String = "<arrangement>" & vbCrLf & "        <upgradetoconv>1</upgradetoconv>"
                objView.XML = Replace(objView.XML, "<arrangement>", strReplace, 1, , vbTextCompare)
                objView.Save()
            End If
        End Set
    End Property
End Class
