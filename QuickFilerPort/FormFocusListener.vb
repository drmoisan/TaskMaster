Public Class FormFocusListener
    Public Event ChangeFocus(ByVal gotFocus As Boolean)

    Public WriteOnly Property ChangeFocusMessage() As Boolean
        Set(gotFocus As Boolean)
            RaiseEvent ChangeFocus(gotFocus)
        End Set

    End Property

End Class
