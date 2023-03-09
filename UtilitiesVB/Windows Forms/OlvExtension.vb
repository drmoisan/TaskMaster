Imports System.Runtime.CompilerServices
Imports BrightIdeasSoftware

Public Module OlvExtension
    <Extension()>
    Public Sub AutoScaleColumnsToContainer(ByVal olv As ObjectListView)
        Dim containerwidth As Integer = olv.Width
        Dim colswidth = 0
        For Each c As OLVColumn In olv.Columns
            colswidth += c.Width
        Next
        If colswidth <> 0 Then
            For Each c As OLVColumn In olv.Columns
                c.Width = CInt(Math.Round(c.Width * CDbl(containerwidth) / colswidth))
            Next
        End If
    End Sub
End Module
