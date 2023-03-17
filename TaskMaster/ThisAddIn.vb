Option Explicit On
Imports System.Diagnostics
Imports System.IO
Imports Microsoft.Office.Core
Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Tools
Imports Microsoft.VisualBasic.FileIO
Imports ToDoModel
Imports UtilitiesVB

Public Class ThisAddIn

    Private _globals As ApplicationGlobals

    Public CatFilterList As List(Of String)
    Private WithEvents OlToDoItems As Outlook.Items
    Private WithEvents ListOfPSTtodo As New List(Of Outlook.Items)
    Private WithEvents ListToDoItems As New List(Of Outlook.Items)
    Private WithEvents OlInboxItems As Outlook.Items
    Private WithEvents OlReminders As Outlook.Reminders
    Private OlNS As Outlook.NameSpace

    Private _ribbonController As RibbonController

    Private ReadOnly _filenameProjectList As String
    Private ReadOnly _filenameProjInfo2 As String
    Private ReadOnly _filenameProjInfo As String = "ProjInfo.bin"
    Public ReadOnly FilenameDictPpl As String = "pplkey.xml"
    Public ReadOnly StagingPath As String = SpecialDirectories.MyDocuments
    Public EmailRoot As String
    Private Const _appDataFolder = "TaskMaster"

    Public ProjInfo As ProjectInfo
    Public DictPPL As Dictionary(Of String, String)
    Public WithEvents IDList As ListOfIDs
    Public DM_CurView As TreeOfToDoItems
    Public Cats As FlagParser

    Private Sub ThisAddIn_Startup() Handles Me.Startup
        _globals = New ApplicationGlobals(Application)

        With _globals
            OlNS = .Ol.NamespaceMAPI
            OlToDoItems = .Ol.ToDoFolder.Items
            OlInboxItems = .Ol.Inbox.Items
            OlReminders = .Ol.OlReminders
            ProjInfo = .TD.ProjInfo
            DictPPL = .TD.DictPPL
            IDList = .TD.IDList
            EmailRoot = .Ol.EmailRootPath
        End With

        _ribbonController.SetGlobals(_globals)

    End Sub

    Protected Overrides Function CreateRibbonExtensibilityObject() As Microsoft.Office.Core.IRibbonExtensibility
        _ribbonController = New RibbonController()
        Return New RibbonViewer(_ribbonController)
    End Function

#Region "Explorer Event Hooks"
    Friend Sub Events_Hook()
        With _globals
            OlToDoItems = .Ol.ToDoFolder.Items
            OlInboxItems = .Ol.Inbox.Items
            OlReminders = .Ol.OlReminders
        End With
    End Sub

    Friend Sub Events_Unhook()
        OlToDoItems = Nothing
        OlInboxItems = Nothing
        OlReminders = Nothing
    End Sub

    Private Sub OlToDoItems_ItemAdd(Item As Object) Handles OlToDoItems.ItemAdd
        ToDoEvents.OlToDoItems_ItemAdd(Item, _globals)
    End Sub

    Private Sub OlToDoItems_ItemChange(Item As Object) Handles OlToDoItems.ItemChange
        ToDoEvents.OlToDoItems_ItemChange(Item, OlToDoItems, _globals)
    End Sub
#End Region

End Class
