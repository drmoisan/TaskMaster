Imports System.IO
Imports UtilitiesVB

Public Class ToDoObj(Of T)
    Implements IToDoObj(Of T)

    Private _filename As String
    Private _folderpath As String
    Private _filepath As String
    Public Delegate Function LoadToDoObj(Folderpath As String, OlApp As Outlook.Application) As T
    Private _loadFunction As LoadToDoObj
    Private _item As T

    Public Sub New(ByVal FileName As String,
                   ByVal FolderPath As String,
                   ByVal LoadFunction As LoadToDoObj)
        _filename = FileName
        _folderpath = FolderPath
        _filepath = Path.Combine(FolderPath, FileName)
        _loadFunction = LoadFunction
    End Sub

    Public Sub New(ByVal Filepath As String,
                   ByVal LoadFunction As LoadToDoObj)
        _filepath = Filepath
        _filename = Path.GetFileName(Filepath)
        _folderpath = Path.GetDirectoryName(Filepath)

        _loadFunction = LoadFunction
    End Sub

    Public Sub LoadFromFile(Folderpath As String, OlApp As Outlook.Application) Implements IToDoObj(Of T).LoadFromFile
        _item = _loadFunction(Folderpath, OlApp)
    End Sub

    Public Property Filename As String Implements IToDoObj(Of T).Filename
        Get
            Return _filename
        End Get
        Set(value As String)
            _filename = value
            _filepath = Path.Combine(_folderpath, _filename)
        End Set
    End Property

    Public Property Folderpath As String Implements IToDoObj(Of T).Folderpath
        Get
            Return _folderpath
        End Get
        Set(value As String)
            _folderpath = value
            _filepath = Path.Combine(_folderpath, _filename)
        End Set
    End Property

    Public Property Filepath As String Implements IToDoObj(Of T).Filepath
        Get
            Return _filepath
        End Get
        Set(value As String)
            _filepath = value
            _filename = Path.GetFileName(value)
            _folderpath = Path.GetDirectoryName(value)
        End Set
    End Property

    Public ReadOnly Property Item As T Implements IToDoObj(Of T).Item
        Get
            Return _item
        End Get
    End Property
End Class
