Imports System.IO
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.VisualBasic.FileIO
Imports Newtonsoft.Json
Imports ToDoModel
Imports UtilitiesVB

Public Class ApplicationGlobals
    Implements IApplicationGlobals

    Private _fs As FileSystemFolderPaths
    Private _olObjects As OlObjectsClass
    Private _toDoObjects As ToDoObjects

    Public Sub New(OlApp As Application)
        _fs = New FileSystemFolderPaths
        _olObjects = New OlObjectsClass(OlApp)
        _toDoObjects = New ToDoObjects(Me)
    End Sub

    Public ReadOnly Property FS As IFileSystemFolderPaths Implements IApplicationGlobals.FS
        Get
            Return _fs
        End Get
    End Property

    Public ReadOnly Property Ol As IOlObjects Implements IApplicationGlobals.Ol
        Get
            Return _olObjects
        End Get
    End Property

    Public ReadOnly Property ToDo As IToDoObjects Implements IApplicationGlobals.ToDo
        Get
            Return _toDoObjects
        End Get
    End Property


    Public Class ToDoObjects
        Implements IToDoObjects

        Private _projInfo As ProjectInfo
        Private _dictPPL As Dictionary(Of String, String)
        Private _IDList As ListOfIDs
        Private _parent As ApplicationGlobals
        Private _dictRemap As Dictionary(Of String, String)

        Public Sub New(ParentInstance As ApplicationGlobals)
            _parent = ParentInstance
        End Sub

        Public ReadOnly Property Parent As IApplicationGlobals Implements IToDoObjects.Parent
            Get
                Return _parent
            End Get
        End Property

        Public ReadOnly Property ProjInfo_Filename As String Implements IToDoObjects.ProjInfo_Filename
            Get
                Return My.Settings.FileName_ProjInfo
            End Get
        End Property

        Public ReadOnly Property ProjInfo As IProjectInfo Implements IToDoObjects.ProjInfo
            Get
                If _projInfo Is Nothing Then
                    _projInfo = LoadToDoProjectInfo(Path.Combine(Parent.FS.AppData, My.Settings.FileName_ProjInfo))
                End If
                Return _projInfo
            End Get
        End Property


        Public ReadOnly Property DictPPL_Filename As String Implements IToDoObjects.DictPPL_Filename
            Get
                Return My.Settings.FilenameDictPpl
            End Get
        End Property

        Public ReadOnly Property DictPPL As Dictionary(Of String, String) Implements IToDoObjects.DictPPL
            Get
                If _dictPPL Is Nothing Then
                    _dictPPL = LoadDictJSON(Parent.FS.StagingPath, DictPPL_Filename)
                End If
                Return _dictPPL
            End Get
        End Property

        Public Sub DictPPL_Save() Implements IToDoObjects.DictPPL_Save
            File.WriteAllText(
                Path.Combine(Parent.FS.StagingPath, DictPPL_Filename),
                JsonConvert.SerializeObject(_dictPPL, Formatting.Indented))
        End Sub

        Public ReadOnly Property FnameIDList As String Implements IToDoObjects.FnameIDList
            Get
                Return My.Settings.FileName_IDList
            End Get
        End Property

        Public ReadOnly Property IDList As IListOfIDs Implements IToDoObjects.IDList
            Get
                If _IDList Is Nothing Then
                    _IDList = LoadIDList(Path.Combine(Parent.FS.AppData,
                    My.Settings.FileName_IDList),
                    _parent.Ol.App)
                End If
                Return _IDList
            End Get
        End Property

        Public ReadOnly Property FnameDictRemap As String Implements IToDoObjects.FnameDictRemap
            Get
                Return My.Settings.FileName_DictRemap
            End Get
        End Property

        Public ReadOnly Property DictRemap As Dictionary(Of String, String) Implements IToDoObjects.DictRemap
            Get
                If _dictRemap Is Nothing Then
                    _dictRemap = LoadDictCSV(Parent.FS.StagingPath, My.Settings.FileName_DictRemap)
                End If
                Return _dictRemap
            End Get
        End Property

        Private Function LoadDictCSV(fpath As String,
                                     filename As String) _
                                     As Dictionary(Of String, String)
            Dim dict As Dictionary(Of String, String) = UtilitiesVB.LoadDictCSV(fpath, filename.Split(".")(0) & ".csv")
            If dict IsNot Nothing Then WriteDictJSON(dict, Path.Combine(fpath, filename))
            Return dict
        End Function

        Private Function LoadDictJSON(fpath As String,
                                      filename As String) _
                                      As Dictionary(Of String, String)

            Dim filepath As String = Path.Combine(fpath, filename)
            Dim dict As Dictionary(Of String, String) = Nothing
            Dim response As MsgBoxResult = MsgBoxResult.Ignore

            Try
                dict = JsonConvert.DeserializeObject(Of Dictionary(Of String, String)) _
                        (File.ReadAllText(Path.Combine(Parent.FS.StagingPath, DictPPL_Filename)))
            Catch ex As FileNotFoundException
                response = MsgBox(filepath & "not found. Load from CSV?", vbYesNo)
            Catch ex As System.Exception
                response = MsgBox(filepath & "encountered a problem. " & ex.Message & "Load from CSV?", vbYesNo)
            Finally
                If response = vbYes Then
                    dict = LoadDictCSV(fpath, filename)
                ElseIf response = vbNo Then
                    response = MsgBox("Start a new blank dictionary?", vbYesNo)
                    If response = vbYes Then
                        dict = New Dictionary(Of String, String)
                    Else
                        Throw New ArgumentNullException("Cannot proceed without dictionary: " & filename)
                    End If
                End If
            End Try
            Return dict
        End Function

        Public Sub WriteDictJSON(dict As Dictionary(Of String, String), filepath As String)
            File.WriteAllText(filepath, JsonConvert.SerializeObject(dict, Formatting.Indented))
        End Sub
    End Class

    Public Class OlObjectsClass
        Implements IOlObjects

        Private _olApp As Outlook.Application
        Private _olEmailRootPath As String

        Public Sub New(OlApp As Application)
            _olApp = OlApp
        End Sub

        Public ReadOnly Property App As Application Implements IOlObjects.App
            Get
                Return _olApp
            End Get
        End Property

        Public ReadOnly Property NamespaceMAPI As Outlook.NameSpace Implements IOlObjects.NamespaceMAPI
            Get
                Return _olApp.Application.GetNamespace("MAPI")
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
                Return _olApp.Reminders
            End Get
        End Property

        Public ReadOnly Property OlEmailRoot As Folder Implements IOlObjects.OlEmailRoot
            Get
                Return _olApp.Session.DefaultStore.GetRootFolder()
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

    End Class

    Public Class FileSystemFolderPaths
        Implements IFileSystemFolderPaths

        Private _appStaging As String
        Private _stagingPath As String
        Private _myD As String
        Private _oneDrive As String
        Private _flow As String
        Private _prereads As String
        Private _remap As String

        Public Sub New()
            LoadFolders()
        End Sub

        Public Sub Reload() Implements IFileSystemFolderPaths.Reload
            LoadFolders()
        End Sub

        Private Sub LoadFolders()
            _appStaging = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            _stagingPath = SpecialDirectories.MyDocuments
            _myD = SpecialDirectories.MyDocuments
            _oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial")
            _flow = Path.Combine(_oneDrive, "Email attachments from Flow")
            _prereads = Path.Combine(_oneDrive, "_  Workflow", "_ Pre-Reads")
            _remap = Path.Combine(_stagingPath, "dictRemap.csv")
        End Sub

        Public ReadOnly Property AppData As String Implements IFileSystemFolderPaths.AppData
            Get
                Return _appStaging
            End Get
        End Property

        Public ReadOnly Property StagingPath As String Implements IFileSystemFolderPaths.StagingPath
            Get
                Return _stagingPath
            End Get
        End Property

        Public ReadOnly Property MyD As String Implements IFileSystemFolderPaths.MyD
            Get
                Return _myD
            End Get
        End Property

        Public ReadOnly Property Root As String Implements IFileSystemFolderPaths.Root
            Get
                Return _oneDrive
            End Get
        End Property

        Public ReadOnly Property Flow As String Implements IFileSystemFolderPaths.Flow
            Get
                Return _flow
            End Get
        End Property

        Public ReadOnly Property PreReads As String Implements IFileSystemFolderPaths.PreReads
            Get
                Return _prereads
            End Get
        End Property


    End Class

#Region "Legacy Definitions and Constants"


#End Region

End Class
