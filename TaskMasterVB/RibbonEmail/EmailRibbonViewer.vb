Imports Microsoft.Office.Tools.Ribbon
Imports UtilitiesVB

Public Class EmailRibbonViewer
    Private _controller As EmailRibbonController

    Public Sub SetController(Controller As EmailRibbonController)
        _controller = Controller
    End Sub

    Private Sub FlagAsTask_Click(sender As Object, e As RibbonControlEventArgs) Handles FlagAsTask.Click
        _controller.FlagAsTask()
    End Sub
End Class
