Imports Microsoft.VisualBasic.FileIO
Imports System.IO
Imports UtilitiesVB

Public Class AppFileSystemFolderPaths
    Implements IFileSystemFolderPaths

    Private _appStaging As String
    Private _stagingPath As String
    Private _myD As String
    Private _oneDrive As String
    Private _flow As String
    Private _prereads As String
    Private _remap As String
    Private _fldrPythonStaging As String

    Public Sub New()
        LoadFolders()
    End Sub

    Public Sub Reload() Implements IFileSystemFolderPaths.Reload
        LoadFolders()
    End Sub

    Private Sub CreateMissingPaths(filepath As String)
        If Not Directory.Exists(filepath) Then
            Directory.CreateDirectory(filepath)
        End If
    End Sub

    Private Sub LoadFolders()
        _appStaging = Path.Combine(Environment.GetFolderPath(
                                       Environment.SpecialFolder.LocalApplicationData),
                                       "TaskMaster")
        CreateMissingPaths(_appStaging)

        _stagingPath = SpecialDirectories.MyDocuments
        _myD = SpecialDirectories.MyDocuments
        _oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial")
        _flow = Path.Combine(_oneDrive, "Email attachments from Flow")
        CreateMissingPaths(_flow)

        _prereads = Path.Combine(_oneDrive, "_  Workflow", "_ Pre-Reads")
        CreateMissingPaths(_prereads)

        _remap = Path.Combine(_stagingPath, "dictRemap.csv")
        _fldrPythonStaging = Path.Combine(_flow, "Combined", "data")
    End Sub

    Public ReadOnly Property FldrAppData As String Implements IFileSystemFolderPaths.FldrAppData
        Get
            Return _appStaging
        End Get
    End Property

    Public ReadOnly Property FldrStaging As String Implements IFileSystemFolderPaths.FldrStaging
        Get
            Return _stagingPath
        End Get
    End Property

    Public ReadOnly Property FldrMyD As String Implements IFileSystemFolderPaths.FldrMyD
        Get
            Return _myD
        End Get
    End Property

    Public ReadOnly Property FldrRoot As String Implements IFileSystemFolderPaths.FldrRoot
        Get
            Return _oneDrive
        End Get
    End Property

    Public ReadOnly Property FldrFlow As String Implements IFileSystemFolderPaths.FldrFlow
        Get
            Return _flow
        End Get
    End Property

    Public ReadOnly Property FldrPreReads As String Implements IFileSystemFolderPaths.FldrPreReads
        Get
            Return _prereads
        End Get
    End Property

    Public Property FldrPythonStaging As String Implements IFileSystemFolderPaths.FldrPythonStaging
        Get
            Return _fldrPythonStaging
        End Get
        Set(value As String)
            _fldrPythonStaging = value
        End Set
    End Property
End Class
