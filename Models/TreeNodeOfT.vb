Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports BrightIdeasSoftware
Imports System.Collections
Imports System.IO
Imports Microsoft.Office.Interop.Outlook
Imports System.Collections.Generic
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports Microsoft.Office.Core
Imports System.Linq.Expressions

Public Class TreeNode(Of T)
    Private _Parent As TreeNode(Of T)
    Private ReadOnly _value As T
    'Private ReadOnly _children As List(Of TreeNode(Of T)) = New List(Of TreeNode(Of T))()
    Private _children As List(Of TreeNode(Of T)) = New List(Of TreeNode(Of T))()
    Public ID As String

    Public Sub New(ByVal value As T)
        _value = value
    End Sub

    Public Sub New(ByVal value As T, ByVal strID As String)
        _value = value
        ID = strID
    End Sub

    Default Public ReadOnly Property Item(ByVal i As Integer) As TreeNode(Of T)
        Get
            Return _children(i)
        End Get
    End Property

    Public ReadOnly Property ParentID As String
        Get
            If _Parent Is Nothing Then
                Return "Root"
            Else
                Return _Parent.ID
            End If
        End Get
    End Property
    Public Property Parent As TreeNode(Of T)
        Get
            Return _Parent
        End Get
        Set(value As TreeNode(Of T))
            _Parent = value
        End Set
    End Property

    Public ReadOnly Property Value As T
        Get
            Return _value
        End Get
    End Property

    Public Function IsAncestor(ByVal model As TreeNode(Of T)) As Boolean
        If Me Is model Then Return True
        If _Parent Is Nothing Then Return False
        Return _Parent.IsAncestor(model)
    End Function

    Public ReadOnly Property ChildCount As Integer
        Get
            Return _children.Count
        End Get
    End Property

    'Public ReadOnly Property NextChildId As String
    '    Get
    Public Function NextChildID() As String
        Dim strMaxID As String = ID & "00"
        Dim lngMaxID As Long = ConvertToDecimal(125, strMaxID)
        Dim strTmpID As String = ""
        Dim lngTmpID As Long = 0
        For Each child In Children
            strTmpID = child.ID
            lngTmpID = ConvertToDecimal(125, strTmpID)
            If lngTmpID > lngMaxID Then
                lngMaxID = lngTmpID
            End If
        Next child

        Dim blContinue As Boolean = True
        While blContinue
            lngMaxID += 1
            strMaxID = ConvertToBase(125, lngMaxID)
            If Globals.ThisAddIn.UsedIDList.Contains(strMaxID) = False Then
                blContinue = False
            End If
        End While
        Globals.ThisAddIn.UsedIDList_Append(strMaxID)
        Return strMaxID
    End Function


    Public Property Children As List(Of TreeNode(Of T))
        Get
            Return _children
        End Get
        Set(value As List(Of TreeNode(Of T)))
            _children = value
        End Set
    End Property

    'Public ReadOnly Property Children As ReadOnlyCollection(Of TreeNode(Of T))
    '    Get
    '        Return _children.AsReadOnly()
    '    End Get
    'End Property

    Public Function AddChild(ByVal value As T) As TreeNode(Of T)
        Dim node = New TreeNode(Of T)(value) With {
            .Parent = Me
            }
        node.ID = NextChildID()
        _children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal node As TreeNode(Of T)) As TreeNode(Of T)
        'node.Parent = Me
        node.ID = NextChildID()
        _children.Add(node)
        Return node
    End Function
    Public Function AddChild(ByVal node As TreeNode(Of T), ByVal strID As String) As TreeNode(Of T)
        node.Parent = Me
        node.ID = strID
        _children.Add(node)
        Return node
    End Function
    Public Function InsertChild(ByVal node As TreeNode(Of T), ByVal strID As String) As TreeNode(Of T)
        node.Parent = Me
        node.ID = strID
        _children.Insert(0, node)
        Return node
    End Function
    Public Function AddChild(ByVal value As T, ByVal strID As String) As TreeNode(Of T)
        Dim node = New TreeNode(Of T)(value) With {
                .Parent = Me
            }
        node.ID = strID
        _children.Add(node)
        Return node
    End Function
    Public Function AddChildren(ParamArray values As T()) As TreeNode(Of T)()
        Return values.[Select](New Func(Of T, TreeNode(Of T))(AddressOf AddChild)).ToArray()
    End Function

    Public Function RemoveChild(ByVal node As TreeNode(Of T)) As Boolean
        Return _children.Remove(node)
    End Function
    'Public Sub SubsituteIDPrefix(ByVal strOld, ByVal strNew)
    '    If Mid(ID, 1, strOld.Length) = strOld Then
    '        ID = strNew & Mid(ID, strOld.Length + 1, ID.Length - strOld.Length)
    '    End If

    '    For Each child In _children
    '        child.SubsituteIDPrefix(strOld, strNew)
    '    Next

    'End Sub
    Public Sub Traverse(ByVal action As Action(Of T))
        action(Value)

        For Each child In _children
            child.Traverse(action)
        Next
    End Sub

    Public Function FindChildByID(strID As String) As TreeNode(Of T)
        Dim node As TreeNode(Of T)
        Dim rnode As TreeNode(Of T)
        If ID = strID Then
            Return Me
        Else
            For Each node In Children
                rnode = node.FindChildByID(strID)
                If Not rnode Is Nothing Then
                    Return rnode
                End If
            Next
            Return Nothing
        End If
    End Function

    Public Function Flatten() As IEnumerable(Of T)
        Return {Value}.Concat(_children.SelectMany(Function(x) x.Flatten()))
    End Function
End Class

