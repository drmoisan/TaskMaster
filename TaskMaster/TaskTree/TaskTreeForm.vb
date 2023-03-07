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

    Public Sub SetController(Controller As TaskTreeController)
        _controller = Controller
    End Sub

    Private Sub TaskTreeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _controller.InitializeTreeListView()
    End Sub

    Private Sub HandleModelCanDrop(ByVal sender As Object, ByVal e As ModelDropEventArgs) Handles TLV.ModelCanDrop
        _controller.HandleModelCanDrop(sender, e)
    End Sub

    Private Sub HandleModelDropped(ByVal sender As Object, ByVal e As ModelDropEventArgs) Handles TLV.ModelDropped
        _controller.HandleModelDropped(sender, e)
    End Sub

    Private Sub MoveObjectsToRoots(ByVal targetTree As TreeListView, ByVal sourceTree As TreeListView, ByVal toMove As IList)
        _controller.MoveObjectsToRoots(targetTree, sourceTree, toMove)
    End Sub

    Private Sub MoveObjectsToSibling(ByVal targetTree As TreeListView,
                                     ByVal sourceTree As TreeListView,
                                     ByVal target As TreeNode(Of ToDoItem),
                                     ByVal toMove As IList,
                                     ByVal siblingOffset As Integer)

        _controller.MoveObjectsToSibling(targetTree, sourceTree, target, toMove, siblingOffset)
    End Sub

    Private Sub MoveObjectsToChildren(ByVal targetTree As TreeListView,
                                      ByVal sourceTree As TreeListView,
                                      ByVal target As TreeNode(Of ToDoItem),
                                      ByVal toMove As IList)

        _controller.MoveObjectsToChildren(targetTree, sourceTree, target, toMove)
    End Sub



    Private Sub TLV_ItemActivate(sender As Object, e As EventArgs) Handles TLV.ItemActivate
        _controller.TLV_ItemActivate()
    End Sub


    Private Sub FormatRow(sender As Object, e As FormatRowEventArgs) Handles TLV.FormatRow
        _controller.FormatRow(sender, e)
    End Sub

    Private Sub But_ExpandCollapse_Click(sender As Object, e As EventArgs) Handles But_ExpandCollapse.Click
        _controller.ToggleExpandCollapseAll()
    End Sub

    Private Sub TaskTreeForm_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        _controller.ResizeForm()
    End Sub

    Private Sub But_ShowHideComplete_Click(sender As Object, e As EventArgs) Handles But_ShowHideComplete.Click
        _controller.ToggleHideComplete()
    End Sub

    Private Sub But_ReloadTree_Click(sender As Object, e As EventArgs) Handles But_ReloadTree.Click
        _controller.RebuildTreeVisual()
    End Sub
End Class