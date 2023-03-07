Imports System.Collections
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports BrightIdeasSoftware
Imports ToDoModel
Imports UtilitiesVB


Public Class TaskTreeForm
    Private _controller As TaskTreeController

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Public Sub SetController(Controller As TaskTreeController)
        _controller = Controller
    End Sub

    Private Sub TaskTreeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _controller IsNot Nothing Then _controller.InitializeTreeListView()
    End Sub

    Private Sub HandleModelCanDrop(ByVal sender As Object, ByVal e As ModelDropEventArgs) Handles TLV.ModelCanDrop
        If _controller IsNot Nothing Then _controller.HandleModelCanDrop(sender, e)
    End Sub

    Private Sub HandleModelDropped(ByVal sender As Object, ByVal e As ModelDropEventArgs) Handles TLV.ModelDropped
        If _controller IsNot Nothing Then _controller.HandleModelDropped(sender, e)
    End Sub

    Private Sub MoveObjectsToRoots(ByVal targetTree As TreeListView, ByVal sourceTree As TreeListView, ByVal toMove As IList)
        If _controller IsNot Nothing Then _controller.MoveObjectsToRoots(targetTree, sourceTree, toMove)
    End Sub

    Private Sub MoveObjectsToSibling(ByVal targetTree As TreeListView,
                                     ByVal sourceTree As TreeListView,
                                     ByVal target As TreeNode(Of ToDoItem),
                                     ByVal toMove As IList,
                                     ByVal siblingOffset As Integer)

        If _controller IsNot Nothing Then _controller.MoveObjectsToSibling(targetTree, sourceTree, target, toMove, siblingOffset)
    End Sub

    Private Sub MoveObjectsToChildren(ByVal targetTree As TreeListView,
                                      ByVal sourceTree As TreeListView,
                                      ByVal target As TreeNode(Of ToDoItem),
                                      ByVal toMove As IList)

        If _controller IsNot Nothing Then _controller.MoveObjectsToChildren(targetTree, sourceTree, target, toMove)
    End Sub



    Private Sub TLV_ItemActivate(sender As Object, e As EventArgs) Handles TLV.ItemActivate
        If _controller IsNot Nothing Then _controller.TLV_ItemActivate()
    End Sub


    Private Sub FormatRow(sender As Object, e As FormatRowEventArgs) Handles TLV.FormatRow
        If _controller IsNot Nothing Then _controller.FormatRow(sender, e)
    End Sub

    Private Sub But_ExpandCollapse_Click(sender As Object, e As EventArgs) Handles But_ExpandCollapse.Click
        If _controller IsNot Nothing Then _controller.ToggleExpandCollapseAll()
    End Sub

    Private Sub TaskTreeForm_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If _controller IsNot Nothing Then _controller.ResizeForm()
    End Sub

    Private Sub But_ShowHideComplete_Click(sender As Object, e As EventArgs) Handles But_ShowHideComplete.Click
        If _controller IsNot Nothing Then _controller.ToggleHideComplete()
    End Sub

    Private Sub But_ReloadTree_Click(sender As Object, e As EventArgs) Handles But_ReloadTree.Click
        If _controller IsNot Nothing Then _controller.RebuildTreeVisual()
    End Sub
End Class