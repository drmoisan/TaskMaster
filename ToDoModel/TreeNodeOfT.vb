Public Class TreeNode(Of T)
    'Public ID As String

    Public Sub New(ByVal value As T)
        Me.Value = value
    End Sub

    Default Public ReadOnly Property Item(ByVal i As Integer) As TreeNode(Of T)
        Get
            Return Children(i)
        End Get
    End Property


    Public Property Parent As TreeNode(Of T)

    Public ReadOnly Property Value As T

    Public Function IsAncestor(ByVal model As TreeNode(Of T)) As Boolean
        If Me Is model Then Return True
        If Parent Is Nothing Then Return False
        Return Parent.IsAncestor(model)
    End Function

    Public ReadOnly Property ChildCount As Integer
        Get
            Return Children.Count
        End Get
    End Property


    Public Property Children As New List(Of TreeNode(Of T))()


    Public Function AddChild(ByVal value As T) As TreeNode(Of T)
        Dim node = New TreeNode(Of T)(value) With {
            .Parent = Me
            }
        'node.ID = NextChildID()
        Children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal node As TreeNode(Of T)) As TreeNode(Of T)
        'node.Parent = Me
        'node.ID = NextChildID()
        Children.Add(node)
        Return node
    End Function

    Public Function InsertChild(ByVal node As TreeNode(Of T)) As TreeNode(Of T)
        node.Parent = Me
        'node.ID = strID
        Children.Insert(0, node)
        Return node
    End Function
    Public Function AddChild(ByVal value As T, ByVal strID As String) As TreeNode(Of T)
        Dim node = New TreeNode(Of T)(value) With {
                .Parent = Me
            }
        'node.ID = strID
        Children.Add(node)
        Return node
    End Function
    Public Function AddChildren(ParamArray values As T()) As TreeNode(Of T)()
        Return values.[Select](New Func(Of T, TreeNode(Of T))(AddressOf AddChild)).ToArray()
    End Function

    Public Function RemoveChild(ByVal node As TreeNode(Of T)) As Boolean
        Return Children.Remove(node)
    End Function

    Public Sub Traverse(ByVal action As Action(Of T))
        action(Value)

        For Each child In Children
            child.Traverse(action)
        Next
    End Sub

    Public Sub Traverse(ByVal action As Action(Of TreeNode(Of T)))
        action(Me)

        For Each child In Children
            child.Traverse(action)
        Next
    End Sub

    Public Function FindByDelegate(comparator As Func(Of T, String, Boolean), StringToCompare As String)
        Dim node As TreeNode(Of T)

        For Each node In Children
            If comparator(Value, StringToCompare) Then
                Return node
            End If
        Next
        Return Nothing
    End Function

    Public Function FindByDelegate(comparator As Func(Of T, T, Boolean), T2 As T)
        Dim node As TreeNode(Of T)

        For Each node In Children
            If comparator(Value, T2) Then
                Return node
            End If
        Next
        Return Nothing
    End Function

    Public Function Flatten() As IEnumerable(Of T)
        Return {Value}.Concat(Children.SelectMany(Function(x) x.Flatten()))
    End Function
End Class
