Imports BrightIdeasSoftware
Imports System.Collections
Imports System.Drawing
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms
Imports ToDoModel
Imports UtilitiesVB

Public Class TaskTreeController
    Public ToDoTree As New List(Of TreeNode(Of ToDoItem))
    Private ReadOnly rs As Resizer = New Resizer()
    Private ReadOnly rscol As Resizer = New Resizer()
    Private expanded As Boolean = False
    Private filtercompleted As Boolean = True
    Private _viewer As TaskTreeForm
    Public _dataModel As New TreeOfToDoItems(New List(Of TreeNode(Of ToDoItem)))

    Public Sub New(Viewer As TaskTreeForm, DataModel As TreeOfToDoItems)
        _viewer = Viewer
        _dataModel = DataModel
        _viewer.SetController(Me)
    End Sub

    Public Sub InitializeTreeListView()

        With _viewer.TLV
            .CanExpandGetter = Function(ByVal x) CType(x, TreeNode(Of ToDoItem)).ChildCount > 0
            .ChildrenGetter = Function(ByVal x) CType(x, TreeNode(Of ToDoItem)).Children
            .ModelFilter = New ModelFilter(Function(ByVal x) CType(x, TreeNode(Of ToDoItem)).Value.Complete = False)
            .Roots = _dataModel.ListOfToDoTree
            .Sort(_viewer.OlvToDoID, SortOrder.Ascending)
        End With

        Dim sink1 = CType(_viewer.TLV.DropSink, SimpleDropSink)
        sink1.AcceptExternal = True
        sink1.CanDropBetween = True
        sink1.CanDropOnBackground = True

        rs.FindAllControls(_viewer)
        rs.SetResizeDimensions(_viewer.SplitContainer1, Resizer.ResizeDimensions.None, True)
        rs.SetResizeDimensions(_viewer.SplitContainer1.Panel2, Resizer.ResizeDimensions.Position Or Resizer.ResizeDimensions.Size, True)
        rs.PrintDict()
    End Sub

    'Friend Function Init_DataModel()
    '    _dataModel = New TreeOfToDoItems(New List(Of TreeNode(Of ToDoItem))) 'Added for the second use of function which was appending
    '    _dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, Globals.ThisAddIn.Application)
    '    ToDoTree = _dataModel.ListOfToDoTree
    '    Return True
    'End Function


    Private Function CompareItemsByToDoID(ByVal objItemLeft As Object, ByVal objItemRight As Object)
        'TODO: This belongs in the datamodel. Not in the controller.
        Dim ToDoIDLeft As String = CustomFieldID_GetValue(objItemLeft, "ToDoID")
        Dim ToDoIDRight As String = CustomFieldID_GetValue(objItemRight, "ToDoID")
        Dim LngLeft As Long = ConvertToDecimal(125, ToDoIDLeft)
        Dim LngRight As Long = ConvertToDecimal(125, ToDoIDRight)

        If ToDoIDRight.Length = 0 Then
            Return -1
        ElseIf ToDoIDLeft.Length = 0 Then
            Return 1
        ElseIf LngLeft < LngRight Then
            Return -1
        Else
            Return 1
        End If
    End Function

    Friend Sub HandleModelCanDrop(ByVal sender As Object, ByVal e As ModelDropEventArgs)
        e.Handled = True
        e.Effect = DragDropEffects.None

        If e.SourceModels.Contains(e.TargetModel) Then
            e.InfoMessage = "Cannot drop on self"
        Else
            Dim sourceModels As IEnumerable(Of TreeNode(Of ToDoItem)) = e.SourceModels.Cast(Of TreeNode(Of ToDoItem))()

            If e.DropTargetLocation = DropTargetLocation.Background Then
                If e.SourceListView Is e.ListView AndAlso sourceModels.All(Function(x) x.Parent Is Nothing) Then
                    e.InfoMessage = "Dragged objects are already roots"
                Else
                    e.Effect = DragDropEffects.Move
                    e.InfoMessage = "Drop on background to promote to roots"
                End If
            ElseIf e.DropTargetLocation = DropTargetLocation.AboveItem Then
                e.Effect = DragDropEffects.Move
                e.InfoMessage = "Drop above item to reorder"
            Else
                Dim target = CType(e.TargetModel, TreeNode(Of ToDoItem))

                If sourceModels.Any(Function(x) target.IsAncestor(x)) Then
                    e.InfoMessage = "Cannot drop on descendant (think of the temporal paradoxes!)"
                Else
                    e.Effect = DragDropEffects.Move
                End If
            End If
        End If
    End Sub


    Friend Sub HandleModelDropped(ByVal sender As Object, ByVal e As ModelDropEventArgs)
        e.Handled = True
        Debug.WriteLine("Fired HandleModelDropped")

        Select Case e.DropTargetLocation
            Case DropTargetLocation.AboveItem
                MoveObjectsToSibling(TryCast(e.ListView, TreeListView), TryCast(e.SourceListView, TreeListView), CType(e.TargetModel, TreeNode(Of ToDoItem)), e.SourceModels, 0)
            Case DropTargetLocation.BelowItem
                MoveObjectsToSibling(TryCast(e.ListView, TreeListView), TryCast(e.SourceListView, TreeListView), CType(e.TargetModel, TreeNode(Of ToDoItem)), e.SourceModels, 1)
            Case DropTargetLocation.Background
                MoveObjectsToRoots(TryCast(e.ListView, TreeListView), TryCast(e.SourceListView, TreeListView), e.SourceModels)
            Case DropTargetLocation.Item
                MoveObjectsToChildren(TryCast(e.ListView, TreeListView), TryCast(e.SourceListView, TreeListView), CType(e.TargetModel, TreeNode(Of ToDoItem)), e.SourceModels)
            Case Else
                Return
        End Select
        e.RefreshObjects()
        If filtercompleted Then _viewer.TLV.ModelFilter = New ModelFilter(Function(ByVal x) CType(x, TreeNode(Of ToDoItem)).Value.Complete = False)
        _viewer.TLV.Sort()
        'this.lastSortColumn = Column;
        'this.lastSortOrder = order;
    End Sub




    Friend Sub MoveObjectsToRoots(ByVal targetTree As TreeListView, ByVal sourceTree As TreeListView, ByVal toMove As IList)
        If sourceTree Is targetTree Then                'Data Model: Check to see if the desination tree roots are in the same tree
            For Each x As TreeNode(Of ToDoItem) In toMove

                If x.Parent IsNot Nothing Then
                    Dim unused1 = x.Parent.RemoveChild(x)             'Data Model: Remove pointer to node from parent.children list
                    'x.Parent.Children.Remove(x)         'Data Model: Remove pointer to node from parent.children list
                    'x.Parent = Nothing                  'Data Model: Set the pointer to the parent inside the node to nothing
                    sourceTree.AddObject(x)             'TreeListView: Add the node to the source tree as a FldrRoot node
                End If
            Next
        Else                                            'Data Model: If the destination tree is different than the source tree

            For Each x As TreeNode(Of ToDoItem) In toMove

                If x.Parent Is Nothing Then             'Data Model: If the node was a root in the old tree
                    sourceTree.RemoveObject(x)          'TreeListView: Delete the pointer in the tree to the node
                Else                                    'Data Model: If the node was NOT a root in the old tree
                    Dim unused = x.Parent.RemoveChild(x)             'Data Model: Grab the parent node and delete the pointer from the list of children
                End If

                x.Parent = Nothing                      'Data Model: Delete the pointer in the node to the parent
                targetTree.AddObject(x)                 'TreeListView: Add the node to the new tree as a root
            Next
        End If
    End Sub

    Friend Sub MoveObjectsToSibling(ByVal targetTree As TreeListView,
                                     ByVal sourceTree As TreeListView,
                                     ByVal target As TreeNode(Of ToDoItem),
                                     ByVal toMove As IList,
                                     ByVal siblingOffset As Integer)

        ' There are lots of things to get right here:
        ' - sourceTree and targetTree may be the same
        ' - target may be a root (which means that all moved objects will also become roots)
        ' - one or more moved objects may be roots (which means the roots of the sourceTree will change)
        '***Why does this sub adjust sourceRoots and targetRoots Directly? I would think the underlying datamodel
        '***change would flow through the rest of the TreeListView

        'Dim sourceRoots As ArrayList = TryCast(sourceTree.Roots, ArrayList)
        'Dim targetRoots As ArrayList = If(targetTree Is sourceTree, sourceRoots, TryCast(targetTree.Roots, ArrayList))
        'Dim sourceRootsChanged = False
        'Dim targetRootsChanged = False

        ' We want to make the moved objects to be siblings of the target. So, we have to 
        ' remove the moved objects from their old parent and give them the same parent as the target.
        ' If the target is a root, then the moved objects have to become roots too.
        For Each x As TreeNode(Of ToDoItem) In toMove

            If x.Parent Is Nothing Then
                'sourceRootsChanged = True               'TreeListView: 
                'sourceRoots.Remove(x)                   'TreeListView: Remove node from roots
                If _dataModel.ListOfToDoTree.Contains(x) Then
                    Dim unused2 = _dataModel.ListOfToDoTree.Remove(x)         'Data Model: Remove node from roots
                Else
                    Dim unused1 = MsgBox("Error in MoveObjectsToSibling: TreeListView and DataModel out of sync at roots")
                End If
            Else
                Dim unused = x.Parent.RemoveChild(x)                 'Data Model: Remove Child from old Parent
                'TreeListView: Where is the action here? Is this automatic?
                'TreeListView: If it is automatic, why did I have to change for the roots?
            End If

            x.Parent = target.Parent                    'Data Model: give the Child a new Parent. Parent doesn't yet recognize child
        Next

        ' Now add to the moved objects to children of their parent (or to the roots collection
        ' if the target is a root)
        If target.Parent Is Nothing Then
            'targetRootsChanged = True                   'TreeListview:
            'targetRoots.InsertRange(targetRoots.IndexOf(target) + siblingOffset, toMove) 'TreeListview: Inserted into new tree
            'DataModel: Nothing here. Is this dealt with?
            _dataModel.ListOfToDoTree.AddRange(toMove)
            Dim i
            Dim strSeed = If(_dataModel.ListOfToDoTree.Count > toMove.Count, _dataModel.ListOfToDoTree(_dataModel.ListOfToDoTree.Count - toMove.Count - 2).Value.ToDoID, "00")

            For i = _dataModel.ListOfToDoTree.Count - toMove.Count - 1 To _dataModel.ListOfToDoTree.Count - 1
                strSeed = Globals.ThisAddIn.IDList.GetNextAvailableToDoID(strSeed)
                _dataModel.ListOfToDoTree(i).Value.ToDoID = strSeed
            Next
        Else
            'Insert moved object into DATAMODEL children of new parent
            Dim idx As Integer = target.Parent.Children.IndexOf(target) + siblingOffset
            'Inconsistent with case of Parent is nothing
            target.Parent.Children.InsertRange(idx, toMove.Cast(Of TreeNode(Of ToDoItem))()) 'DataModel: Inserted into new data model tree. 
            _dataModel.ReNumberChildrenIDs(target.Parent.Children, Globals.ThisAddIn.IDList)         'DataModel: Renumber IDs of new branch order

            ''Renumber IDs for inserted node
            ''Eliminate current top level IDs from UsedIDList
            'Dim i
            'For i = idx To (target.Parent.Children.Count - 1)
            '    Dim tmpNode As TreeNode(Of ToDoItem) = target.Parent.Children.Item(i)
            '    Dim tmpID As String = tmpNode.ID
            '    If Globals.ThisAddIn.UsedIDList.Contains(tmpID) = True Then
            '        Globals.ThisAddIn.UsedIDList.Remove(tmpID)
            '    End If
            'Next
            ''Assign new IDs for children and then substitute ID Prefx in grandchildren
            'For i = idx To (target.Parent.Children.Count - 1)
            '    Dim tmpNode As TreeNode(Of ToDoItem) = target.Parent.Children.Item(i)
            '    Dim tmpID As String = tmpNode.ID
            '    tmpNode.ID = tmpNode.Parent.NextChildID
            '    tmpNode.Value.ToDoID = tmpNode.ID
            '    If Globals.ThisAddIn.UsedIDList.Contains(tmpID) = True Then
            '        Globals.ThisAddIn.UsedIDList.Remove(tmpID)
            '    End If
            '    For Each y As TreeNode(Of ToDoItem) In tmpNode.Children
            '        SubstituteIDPrefix(y, tmpID, tmpNode.ID)
            '    Next
            'Next
        End If

        'Update TreeListView with new
        'If targetTree Is sourceTree Then
        '    If sourceRootsChanged OrElse targetRootsChanged Then sourceTree.Roots = sourceRoots
        'Else
        '    If sourceRootsChanged Then sourceTree.Roots = sourceRoots
        '    If targetRootsChanged Then targetTree.Roots = targetRoots
        'End If
    End Sub

    Friend Sub MoveObjectsToChildren(ByVal targetTree As TreeListView,
                                      ByVal sourceTree As TreeListView,
                                      ByVal target As TreeNode(Of ToDoItem),
                                      ByVal toMove As IList)

        'Dim strID_PrefixOld As String
        'Dim strID_PrefixNew As String

        For Each x As TreeNode(Of ToDoItem) In toMove

            'strID_PrefixOld = x.ID 'Kill this line

            If x.Parent Is Nothing Then
                sourceTree.RemoveObject(x)              'TreeListView: Remove from Visual Tree
                If _dataModel.ListOfToDoTree.Contains(x) Then
                    Dim unused2 = _dataModel.ListOfToDoTree.Remove(x)         'Data Model: Remove node from roots
                Else
                    Dim unused1 = MsgBox("Error in MoveObjectsToChildren: TreeListView and DataModel out of sync at roots")
                End If
            Else
                Dim unused = x.Parent.Children.Remove(x)             'Data Model: Remove pointer to child from parent
                '***NO REFERENCE TO TREELISTVIEW. INCONSISTENT WITH TREATMENT OF ROOTS
            End If

            x.Parent = target                                   'Data Model: Add pointer to new Parent in data model
            _dataModel.AddChild(x, target, Globals.ThisAddIn.IDList)    'Data Model: Add child to parent and renumber all affected

            '***OLD Code to add child to target parent and renumber
            'target.AddChild(x)                      'Data Model: Add child to target parent and renumber grandchildren
            'target.Children.Add(x)
            'x.Value.ToDoID = x.ID
            'strID_PrefixNew = x.ID
            'For Each y As TreeNode(Of ToDoItem) In x.Children
            '    SubstituteIDPrefix(y, strID_PrefixOld, strID_PrefixNew)
            'Next
        Next
        'WriteTreeToCSVDebug()

        'Curious ... this is inconsistent with MoveObjectsToSibling
    End Sub



    Private Function FindChildByID(ID As String, nodes As List(Of TreeNode(Of ToDoItem))) As TreeNode(Of ToDoItem)
        Dim node As TreeNode(Of ToDoItem)
        Dim rnode As TreeNode(Of ToDoItem)

        For Each node In nodes
            If node.Value.ToDoID = ID Then
                Return node
            Else
                rnode = FindChildByID(ID, node.Children)
                If rnode IsNot Nothing Then
                    Return rnode
                End If
            End If
        Next

        Return Nothing

    End Function

    Public Sub WriteTreeToDisk(filepath)
        Dim filename As String = Path.Combine(filepath, "DebugTreeDump.csv")

        Using sw As New StreamWriter(filename)
            sw.WriteLine("File Dump")
        End Using

        'LoopTreeToWrite(ToDoTree.Children, filename, "")
        LoopTreeToWrite(ToDoTree, filename, "")
    End Sub

    Public Sub LoopTreeToWrite(nodes As List(Of TreeNode(Of ToDoItem)), filename As String, lineprefix As String)
        If nodes IsNot Nothing Then
            For Each node As TreeNode(Of ToDoItem) In nodes
                AppendLineToCSV(filename, lineprefix & node.Value.ToDoID & " " & node.Value.TaskSubject)
                LoopTreeToWrite(node.Children, filename, lineprefix & node.Value.ToDoID & ",")
            Next
        End If
    End Sub
    Public Sub AppendLineToCSV(filename As String, line As String)
        Using sw As StreamWriter = File.AppendText(filename)
            sw.WriteLine(line)
        End Using
    End Sub

    Friend Sub TLV_ItemActivate()
        Dim item = _viewer.TLV.GetItem(_viewer.TLV.SelectedIndex).RowObject
        Dim node As TreeNode(Of ToDoItem) = TryCast(item, TreeNode(Of ToDoItem))
        If node IsNot Nothing Then
            Dim objItem As Object = node.Value.GetItem()
            If TypeOf objItem Is Outlook.MailItem Then
                Dim OlMail As Outlook.MailItem = TryCast(objItem, Outlook.MailItem)
                If OlMail IsNot Nothing Then
                    Dim OlExplorer As Outlook.Explorer = Globals.ThisAddIn.Application.ActiveExplorer
                    If OlExplorer.IsItemSelectableInView(OlMail) Then
                        OlExplorer.ClearSelection()
                        OlExplorer.AddToSelection(OlMail)
                        OlMail.Display()
                    Else
                        OlMail.Display()
                    End If
                End If
            ElseIf TypeOf objItem Is Outlook.TaskItem Then
                Dim OlTask As Outlook.TaskItem = TryCast(objItem, Outlook.TaskItem)
                If OlTask IsNot Nothing Then
                    Dim OlExplorer As Outlook.Explorer = Globals.ThisAddIn.Application.ActiveExplorer
                    If OlExplorer.IsItemSelectableInView(OlTask) Then
                        OlExplorer.ClearSelection()
                        OlExplorer.AddToSelection(OlTask)
                        OlTask.Display()
                    Else
                        OlTask.Display()
                    End If
                End If
            End If
        End If
    End Sub



    Friend Sub FormatRow(sender As Object, e As FormatRowEventArgs)
        Console.WriteLine("In Format Row")
        Dim objToDo As ToDoItem = TryCast(e.Model.Value, ToDoItem)
        e.Item.Font = If(objToDo.Complete,
            New Font(e.Item.Font, e.Item.Font.Style Or FontStyle.Strikeout),
            New Font(e.Item.Font, e.Item.Font.Style And Not FontStyle.Strikeout))
    End Sub

    Friend Sub ToggleExpandCollapseAll()
        If expanded Then
            _viewer.TLV.CollapseAll()
        Else
            _viewer.TLV.ExpandAll()
        End If
        expanded = Not expanded

    End Sub

    Friend Sub ResizeForm()
        rs.ResizeAllControls(_viewer)
        _viewer.TLV.AutoScaleColumnsToContainer()
    End Sub

    Friend Sub ToggleHideComplete()
        '_viewer.TLV.ChildrenGetter = Function(ByVal x) CType(x, TreeNode(Of ToDoItem)).Children
        If filtercompleted Then
            _viewer.TLV.ModelFilter = Nothing
            filtercompleted = False
        Else
            _viewer.TLV.ModelFilter = New ModelFilter(Function(ByVal x) CType(x, TreeNode(Of ToDoItem)).Value.Complete = False)
            filtercompleted = True
        End If
    End Sub

    Friend Sub RebuildTreeVisual()
        _viewer.TLV.Roots = _dataModel.ListOfToDoTree
        _viewer.TLV.RebuildAll(preserveState:=False)
    End Sub
End Class

