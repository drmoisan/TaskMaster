Imports System.Windows.Forms

Public Class MouseDownFilter
    Implements IMessageFilter

    Public Event FormClicked As EventHandler
    Private WM_LBUTTONDOWN As Integer = &H201
    Private form As Form = Nothing

    Declare Auto Function IsChild Lib "user32.dll" (
        ByVal hWndParent As IntPtr,
        ByVal hWnd As IntPtr) As Boolean

    Public Sub New(ByVal f As Form)
        form = f
    End Sub

    Private Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        If m.Msg = WM_LBUTTONDOWN Then

            If Form.ActiveForm IsNot Nothing AndAlso Form.ActiveForm.Equals(form) Then
                OnFormClicked()
            End If
        End If

        Return False
    End Function

    Protected Sub OnFormClicked()
        RaiseEvent FormClicked(form, EventArgs.Empty)
    End Sub


End Class


