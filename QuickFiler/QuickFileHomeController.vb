Imports UtilitiesVB
Imports ToDoModel

Public Class QuickFileHomeController
    Private _viewer As QuickFileViewer
    Private _controller As QuickFileController
    Private _globals As IApplicationGlobals

    Public Sub New(AppGlobals As IApplicationGlobals)
        _globals = AppGlobals
        _viewer = New QuickFileViewer()
        ReloadFolderSuggestionStagingFiles()
        Dim colEmailsInFolder As Collection = LoadEmailDataBase(_globals.Ol.App.ActiveExplorer())
        _controller = New QuickFileController(_globals, _viewer, colEmailsInFolder)
    End Sub

    Public Sub Run()
        '_viewer.Show()
    End Sub

    Public ReadOnly Property Loaded As Boolean
        Get
            If _viewer IsNot Nothing Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property
End Class
