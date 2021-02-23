Imports System.ComponentModel

Public Class ProjectInfoWindow
    Public pi As ProjectInfo
    Private rs As New Resizer

    Public Sub New(ProjInfo As ProjectInfo)


        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        pi = ProjInfo

    End Sub

    Private Sub ProjectInfoWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        olvProjInfo.SetObjects(pi)

        rs.FindAllControls(Me)
        rs.SetResizeDimensions(Me.SplitContainer1.Panel2, Resizer.ResizeDimensions.Position, True)
        rs.SetResizeDimensions(Me.SplitContainer1, Resizer.ResizeDimensions.None, True)
        rs.SetResizeDimensions(Me.SplitContainer1.Panel1, Resizer.ResizeDimensions.Position Or Resizer.ResizeDimensions.Size, True)
        rs.PrintDict()
    End Sub

    Private Sub BTN_OK_Click(sender As Object, e As EventArgs) Handles BTN_OK.Click
        pi.Save()
        Me.Close()
    End Sub

    Private Sub BTN_CANCEL_Click(sender As Object, e As EventArgs) Handles BTN_CANCEL.Click
        Me.Close()
    End Sub

    Private Sub ProjectInfoWindow_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        rs.ResizeAllControls(Me)
        'TreeListView1.AutoResizeColumns()
        Me.olvProjInfo.AutoScaleColumnsToContainer()
    End Sub
End Class