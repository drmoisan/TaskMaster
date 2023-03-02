Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop
Imports ToDoModel
Imports System.IO
Imports Microsoft.VisualBasic.FileIO


Public Class ApplicationGlobals
    Private _olApp As Outlook.Application
    Private _projInfo As ProjectInfo
    Private _dictPPL As PeopleDict(Of String, String)
    Private _IDList As IDListClass
    Private _olEmailRootPath As String
    Private _fs As FileSystemFolderPaths
    Private _olObjects As OlObjectsClass

    Public Sub New(OlApp As Application)
        _olApp = OlApp
        _fs = New FileSystemFolderPaths
        _olObjects = New OlObjectsClass(OlApp)
    End Sub

    Public ReadOnly Property NamespaceMAPI As Outlook.NameSpace
        Get
            Return _olApp.Application.GetNamespace("MAPI")
        End Get
    End Property

    Public ReadOnly Property FS As FileSystemFolderPaths
        Get
            Return _fs
        End Get
    End Property

    Public ReadOnly Property Ol As OlObjectsClass
        Get
            Return _olObjects
        End Get
    End Property

    Public ReadOnly Property OlToDoFolder As Outlook.Folder
        Get
            Return NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
        End Get
    End Property

    Public ReadOnly Property OlInbox As Outlook.Folder
        Get
            Return NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderInbox)
        End Get
    End Property

    Public ReadOnly Property OlReminders As Outlook.Reminders
        Get
            Return _olApp.Reminders
        End Get
    End Property

    Public ReadOnly Property ProjInfo As ProjectInfo
        Get
            If _projInfo Is Nothing Then
                LoadToDoProjectInfo(Path.Combine(FS.AppData, My.Settings.FileName_ProjInfo))
            End If
            Return _projInfo
        End Get
    End Property

    Public ReadOnly Property DictPPL As PeopleDict(Of String, String)
        Get
            If _dictPPL Is Nothing Then
                _dictPPL = GetDictPPL(FS.StagingPath, My.Settings.FilenameDictPpl)
            End If
            Return _dictPPL
        End Get
    End Property

    Public ReadOnly Property IDList As IDListClass
        Get
            If _IDList Is Nothing Then
                _IDList = LoadIDList(Path.Combine(FS.AppData, My.Settings.FileName_IDList))
            End If
            Return _IDList
        End Get
    End Property

    Public ReadOnly Property OlEmailRoot As Folder
        Get
            Return _olApp.Session.DefaultStore.GetRootFolder()
        End Get
    End Property

    Public ReadOnly Property OlEmailRootPath As String
        Get
            If _olEmailRootPath Is Nothing Then
                _olEmailRootPath = OlEmailRoot.FolderPath
            End If
            Return _olEmailRootPath
        End Get
    End Property

    Public Class OlObjectsClass
        Private _olApp As Outlook.Application

        Public Sub New(OlApp As Application)
            _olApp = OlApp
        End Sub

        Public ReadOnly Property App As Application
            Get
                Return _olApp
            End Get
        End Property
    End Class

    Public Class FileSystemFolderPaths
        Private _appStaging As String
        Private _stagingPath As String
        Private _myD As String
        Private _oneDrive As String
        Private _flow As String
        Private _prereads As String

        Public Sub New()
            LoadFolders()
        End Sub

        Public Sub Reload()
            LoadFolders()
        End Sub

        Private Sub LoadFolders()
            _appStaging = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            _stagingPath = SpecialDirectories.MyDocuments
            _myD = SpecialDirectories.MyDocuments
            _oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial")
            _flow = Path.Combine(_oneDrive, "Email attachments from Flow")
            _prereads = Path.Combine(_oneDrive, "_  Workflow", "_ Pre-Reads")
        End Sub

        Public ReadOnly Property AppData As String
            Get
                Return _appStaging
            End Get
        End Property

        Public ReadOnly Property StagingPath As String
            Get
                Return _stagingPath
            End Get
        End Property

        Public ReadOnly Property MyD As String
            Get
                Return _myD
            End Get
        End Property

        Public ReadOnly Property Root As String
            Get
                Return _oneDrive
            End Get
        End Property

        Public ReadOnly Property Flow As String
            Get
                Return _flow
            End Get
        End Property

        Public ReadOnly Property PreReads As String
            Get
                Return _prereads
            End Get
        End Property
    End Class

#Region "Legacy Definitions and Constants"


#End Region

End Class
