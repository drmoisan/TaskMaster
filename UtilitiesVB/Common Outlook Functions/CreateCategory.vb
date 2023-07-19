Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook

Public Module CreateCategoryModule
    Public Function CreateCategory(OlNS As Outlook.NameSpace, prefix As IPrefix, newCatName As String) As Category

        Dim objCategory As Category = Nothing
        'Dim OlColor As OlCategoryColor
        Dim strTemp As String

        If newCatName <> "" Then
            If prefix.Value <> "" Then
                strTemp = If(Len(newCatName) > Len(prefix),
                    If(Left(newCatName, Len(prefix.Value)) <> prefix.Value, prefix.Value & newCatName, newCatName),
                    prefix.Value & newCatName)
            Else
                strTemp = newCatName
            End If


            Dim exists As Boolean = False
            For Each objCategory In OlNS.Categories
                If objCategory.Name = strTemp Then
                    exists = True
                    Dim unused1 = MsgBox("Color category " & strTemp & " already exists. Cannot add a duplicate.")
                    Return objCategory
                End If
            Next objCategory

            If Not exists Then
                Try
                    objCategory = OlNS.Categories.Add(strTemp,
                                                      prefix.Color,
                                                      OlCategoryShortcutKey _
                                                      .olCategoryShortcutKeyNone)
                Catch ex As System.Exception
                    Debug.WriteLine(ex.Message)
                    Debug.WriteLine(ex.StackTrace.ToString())
                End Try
            End If
        Else
            Dim unused = MsgBox("Error: Parameter " & NameOf(newCatName) & " must have a value to create a category.")
        End If

        Return objCategory

    End Function
End Module
