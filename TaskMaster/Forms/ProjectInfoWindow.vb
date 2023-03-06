Imports ToDoModel

Public Class ProjectInfoWindow
    Public pi As ProjectInfo
    Private ReadOnly rs As New Resizer
    Private blEditingCell As Boolean = False

    Public Sub New(ProjInfo As ProjectInfo)


        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        pi = ProjInfo

    End Sub

    Private Sub ProjectInfoWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        olvProjInfo.SetObjects(pi)

        rs.FindAllControls(Me)
        Dim unused2 = rs.SetResizeDimensions(SplitContainer1.Panel2, Resizer.ResizeDimensions.Position, True)
        Dim unused1 = rs.SetResizeDimensions(SplitContainer1, Resizer.ResizeDimensions.None, True)
        Dim unused = rs.SetResizeDimensions(SplitContainer1.Panel1, Resizer.ResizeDimensions.Position Or Resizer.ResizeDimensions.Size, True)
        rs.PrintDict()
    End Sub

    Private Sub BTN_OK_Click(sender As Object, e As EventArgs) Handles BTN_OK.Click
        pi.Save()
        Close()
    End Sub

    Private Sub BTN_CANCEL_Click(sender As Object, e As EventArgs) Handles BTN_CANCEL.Click
        Close()
    End Sub

    Private Sub ProjectInfoWindow_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        rs.ResizeAllControls(Me)
        'TreeListView1.AutoResizeColumns()
        olvProjInfo.AutoScaleColumnsToContainer()
    End Sub

    Private Sub olvProjInfo_KeyUp(sender As Object, e As Windows.Forms.KeyEventArgs) Handles olvProjInfo.KeyUp

        If blEditingCell = False Then
            If e.KeyData = Windows.Forms.Keys.Delete Then
                Dim selection As System.Collections.ArrayList = olvProjInfo.SelectedObjects
                If selection IsNot Nothing Then
                    For Each entry As ToDoProjectInfoEntry In selection
                        Dim unused = pi.Remove(entry)
                    Next
                    pi.Save()
                    olvProjInfo.RemoveObjects(olvProjInfo.SelectedObjects)
                End If
            End If
        End If
    End Sub

    Private Sub olvProjInfo_CellEditStarting(sender As Object, e As BrightIdeasSoftware.CellEditEventArgs) Handles olvProjInfo.CellEditStarting
        blEditingCell = True
    End Sub

    Private Sub olvProjInfo_CellEditFinishing(sender As Object, e As BrightIdeasSoftware.CellEditEventArgs) Handles olvProjInfo.CellEditFinishing
        blEditingCell = False
    End Sub
End Class