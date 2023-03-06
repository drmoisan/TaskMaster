Imports Microsoft.Office.Tools.Ribbon

Public Class EmailRibbon


    Private Sub FT_EM_Click(sender As Object, e As RibbonControlEventArgs) Handles FT_EM.Click
        Dim FT As New Flag_Tasks(Globals.ThisAddIn._globals)
        FT.Run()
    End Sub
End Class
