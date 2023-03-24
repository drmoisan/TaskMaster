Imports UtilitiesVB
Imports ToDoModel

Public Class QuickFileHomeController
    Private _viewer As QuickFileViewer
    Private _controller As QuickFileController
    Private _globals As IApplicationGlobals
    Delegate Sub ParentCleanupFunction()
    Private _parentCleanup As ParentCleanupFunction

    Public Sub New(AppGlobals As IApplicationGlobals, ParentCleanup As ParentCleanupFunction)
        _globals = AppGlobals
        _parentCleanup = ParentCleanup
        _viewer = New QuickFileViewer()
        'ReloadFolderSuggestionStagingFiles()
        Dim colEmailsInFolder As Collection = LoadEmailDataBase(_globals.Ol.App.ActiveExplorer())
        _controller = New QuickFileController(_globals, _viewer, colEmailsInFolder, AddressOf Cleanup)
    End Sub

    Public Sub Run()
        '_viewer.Show()
    End Sub

    Public ReadOnly Property Loaded As Boolean
        Get
            If _viewer IsNot Nothing Then
                'If _viewer.IsDisposed = False Then
                Return True
                'Else
                '   Return False
                'End If
            Else
                Return False
            End If
        End Get
    End Property

    Friend Sub Cleanup()
        _viewer = Nothing
        _controller = Nothing
        _globals = Nothing
        _parentCleanup.Invoke()
    End Sub
End Class
