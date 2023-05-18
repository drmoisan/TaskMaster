Imports UtilitiesVB

Public Class EmailRibbonController
    Private _viewer As RibbonViewer
    Private _globals As IApplicationGlobals

    'Friend Sub New(EmailRibbon As Ribbon, AppGlobals As IApplicationGlobals)
    '    _viewer = EmailRibbon
    '    _globals = AppGlobals
    'End Sub

    Public Sub New()

    End Sub

    Friend Sub SetGlobals(AppGlobals As IApplicationGlobals)
        _globals = AppGlobals
    End Sub

    Friend Sub SetViewer(Viewer As RibbonViewer)
        _viewer = Viewer
    End Sub

    Friend Sub Activate()
        _viewer.SetController(Me)
    End Sub

    Friend Sub FlagAsTask()
        Dim FT As New FlagTasks(_globals)
        FT.Run()
    End Sub

End Class
