Imports System.Drawing
Imports System.Windows.Forms

Public Class QfcViewer
    Private _tipsActive As Boolean = True
    Private ReadOnly _tipsLabels As List(Of Label) = New List(Of Label)
    Private ReadOnly _tipsColumns As Dictionary(Of ColumnStyle, Single) = New Dictionary(Of ColumnStyle, Single)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        InitTipsLabels()
        InitTipsColumns()
        ToggleAccelerator()

    End Sub

    Private Sub InitTipsLabels()
        With _tipsLabels
            .Add(Me.LblAcOpen)
            .Add(Me.LblAcPopOut)
            .Add(Me.LblAcTask)
            .Add(Me.LblAcDelete)
            .Add(Me.LblAcAttachments)
            .Add(Me.LblAcConversation)
            .Add(Me.LblAcEmail)
            .Add(Me.LblAcFolder)
            .Add(Me.LblAcSearch)
        End With
    End Sub

    Private Sub InitTipsColumns()
        With _tipsColumns
            .Add(Me.L1h1L2v.ColumnStyles.Item(0), 50)
            .Add(Me.L1h2L2v1h.ColumnStyles.Item(1), 20)
            .Add(Me.L1h2L2v2h.ColumnStyles.Item(1), 20)
            .Add(Me.L1h2L2v3h.ColumnStyles.Item(1), 20)
            .Add(Me.L1h2L2v3h.ColumnStyles.Item(5), 20)
            .Add(Me.L1h2L2v3h.ColumnStyles.Item(7), 20)
        End With
    End Sub

    Private Sub ControlGroup_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint

        If Me.BorderStyle = BorderStyle.FixedSingle Then
            Dim thickness As Integer = 2
            Dim halfThickness As Integer = thickness / 2

            Using p As Pen = New Pen(Color.Black, thickness)
                e.Graphics.DrawRectangle(p, New Rectangle(halfThickness, halfThickness, Me.ClientSize.Width - thickness, Me.ClientSize.Height - thickness))
            End Using
        End If
    End Sub

    Public Sub ToggleAccelerator()
        If _tipsActive Then
            'Make tips invisible
            For Each tip As Label In _tipsLabels
                tip.Visible = False
            Next

            Me.LblPos.Visible = False

            'Make tips columns 0 pixels in width
            For Each col As ColumnStyle In _tipsColumns.Keys
                col.Width = 0
            Next
            _tipsActive = False
        Else
            'Make tips visible
            For Each tip As Label In _tipsLabels
                tip.Visible = True
            Next
            Me.LblPos.Visible = True

            'Make tips columns 20 pixels in width
            For Each col As ColumnStyle In _tipsColumns.Keys
                col.Width = _tipsColumns(col)
            Next
            _tipsActive = True
        End If
    End Sub
End Class
