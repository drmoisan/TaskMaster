Imports System
Imports System.CodeDom
Imports System.Collections
Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Windows.Forms
Imports BrightIdeasSoftware
Imports Microsoft.Office.Interop.Outlook


Public Class TaskTreeForm

    'Public ToDoTree As TreeNode(Of ToDoItem)
    Public ToDoTree As List(Of TreeNode(Of ToDoItem)) = New List(Of TreeNode(Of ToDoItem))
    Public DM As DataModel_ToDoTree = New DataModel_ToDoTree(New List(Of TreeNode(Of ToDoItem)))

    ''Original
    'Friend Function Init_DataModel()
    '    Dim objItem As Object

    '    Dim strTemp As String
    '    Dim strPrev As String
    '    Dim ParentNode As TaskMaster.TreeNode(Of TaskMaster.ToDoItem)
    '    Dim TreeItems As Outlook.Items
    '    Dim colItems As Collection
    '    strPrev = ""
    '    strTemp = ""

    '    Try
    '        TreeItems = Globals.ThisAddIn.GetItemsInView_ToDo
    '        TreeItems.Sort("ToDoID")
    '        colItems = New Collection
    '        Dim colNoID = New Collection
    '        Dim tmpToDo As ToDoItem = Nothing
    '        Dim ToDoNode As TreeNode(Of ToDoItem)
    '        Dim NodeParent As TreeNode(Of ToDoItem)


    '        'Dim ToDoTree As List(Of TreeNode(Of ToDoItem)) = New List(Of TreeNode(Of ToDoItem))
    '        tmpToDo = New ToDoItem("00")
    '        tmpToDo.TaskSubject = "Root"
    '        ToDoTree = New TreeNode(Of ToDoItem)(tmpToDo, "00")

    '        ' Iterate through all visible ToDo items 
    '        For Each objItem In TreeItems

    '            'Cast objItem to temporary ToDoItem
    '            If TypeOf objItem Is MailItem Then
    '                tmpToDo = New ToDoItem(CType(objItem, MailItem))
    '            ElseIf TypeOf objItem Is TaskItem Then
    '                tmpToDo = New ToDoItem(CType(objItem, TaskItem))
    '            End If

    '            'If there is no ToDoID, add it to a list to assign later
    '            If tmpToDo.ToDoID = "nothing" Then
    '                ToDoTree.AddChild(tmpToDo)
    '            Else
    '                ToDoTree.AddChild(tmpToDo, tmpToDo.ToDoID)
    '            End If
    '        Next

    '        'Transform tree to be hierarchical
    '        Dim max As Integer = ToDoTree.ChildCount - 1
    '        Dim i As Integer

    '        For i = max To 0 Step -1
    '            ToDoNode = ToDoTree.Item(i)
    '            If ToDoNode.ID.Length > 2 Then
    '                Dim strID As String = ToDoNode.ID
    '                Dim strParentID As String = Mid(strID, 1, strID.Length - 2)

    '                Dim blContinue As Boolean = True

    '                While blContinue
    '                    NodeParent = ToDoTree.FindChildByID(strParentID)
    '                    If Not NodeParent Is Nothing Then
    '                        NodeParent.AddChild(ToDoNode, ToDoNode.ID)
    '                        ToDoTree.RemoveChild(ToDoNode)
    '                        blContinue = False
    '                    End If
    '                    If strParentID.Length > 2 Then
    '                        strParentID = Mid(strParentID, 1, strParentID.Length - 2)
    '                    Else
    '                        blContinue = False
    '                    End If
    '                End While
    '            End If
    '        Next i


    '    Catch
    '        Debug.WriteLine(Err.Description)
    '        MsgBox(Err.Description)
    '    End Try
    'End Function

    Friend Function Init_DataModel()
        DM.LoadTree(DataModel_ToDoTree.LoadOptions.vbLoadInView)
        ToDoTree = DM.ListOfToDoTree
    End Function

    'Friend Function Init_DataModel()
    '    Dim objItem As Object

    '    Dim strTemp As String
    '    Dim strPrev As String
    '    Dim ParentNode As TaskMaster.TreeNode(Of TaskMaster.ToDoItem)
    '    'Dim TreeItems As Outlook.Items

    '    Dim colItems As Collection
    '    strPrev = ""
    '    strTemp = ""

    '    Try
    '        'TreeItems = Globals.ThisAddIn.GetItemsInView_ToDo
    '        Dim TreeItems As List(Of Object) = Globals.ThisAddIn.GetListOfItemsInView_ToDo
    '        'TreeItems.Sort("ToDoID")
    '        TreeItems = MergeSort(Of Object)(TreeItems, AddressOf CompareItemsByToDoID)

    '        colItems = New Collection
    '        Dim colNoID = New Collection
    '        Dim tmpToDo As ToDoItem = Nothing
    '        Dim ToDoNode As TreeNode(Of ToDoItem)
    '        Dim NodeParent As TreeNode(Of ToDoItem)


    '        'Dim ToDoTree As List(Of TreeNode(Of ToDoItem)) = New List(Of TreeNode(Of ToDoItem))
    '        tmpToDo = New ToDoItem("00")
    '        tmpToDo.TaskSubject = "Root"
    '        'ToDoTree = New TreeNode(Of ToDoItem)(tmpToDo, "00")

    '        ' Iterate through all visible ToDo items 
    '        For Each objItem In TreeItems

    '            'Cast objItem to temporary ToDoItem
    '            If TypeOf objItem Is MailItem Then
    '                tmpToDo = New ToDoItem(CType(objItem, MailItem))
    '            ElseIf TypeOf objItem Is TaskItem Then
    '                tmpToDo = New ToDoItem(CType(objItem, TaskItem))
    '            End If

    '            'If there is no ToDoID, add it to a list to assign later
    '            If tmpToDo.ToDoID = "nothing" Then
    '                'ToDoTree.AddChild(tmpToDo)
    '                ToDoTree.Add(New TreeNode(Of ToDoItem)(tmpToDo))
    '            Else
    '                'ToDoTree.AddChild(tmpToDo, tmpToDo.ToDoID)
    '                ToDoTree.Add(New TreeNode(Of ToDoItem)(tmpToDo, tmpToDo.ToDoID))
    '            End If
    '        Next

    '        'Transform tree to be hierarchical
    '        Dim max As Integer = ToDoTree.Count - 1
    '        Dim i As Integer

    '        For i = max To 0 Step -1
    '            ToDoNode = ToDoTree(i)
    '            If ToDoNode.ID.Length > 2 Then
    '                Dim strID As String = ToDoNode.ID
    '                Dim strParentID As String = Mid(strID, 1, strID.Length - 2)

    '                Dim blContinue As Boolean = True

    '                While blContinue
    '                    'NodeParent = ToDoTree.FindChildByID(strParentID)
    '                    NodeParent = FindChildByID(strParentID, ToDoTree)
    '                    If Not NodeParent Is Nothing Then
    '                        NodeParent.AddChild(ToDoNode, ToDoNode.ID)
    '                        'ToDoTree.RemoveChild(ToDoNode)
    '                        ToDoTree.Remove(ToDoNode)
    '                        blContinue = False
    '                    End If
    '                    If strParentID.Length > 2 Then
    '                        strParentID = Mid(strParentID, 1, strParentID.Length - 2)
    '                    Else
    '                        blContinue = False
    '                    End If
    '                End While
    '            End If
    '        Next i


    '    Catch
    '        Debug.WriteLine(Err.Description)
    '        MsgBox(Err.Description)
    '    End Try
    'End Function

    'Private Sub MergeSort(Of T, TKey)(ByVal coll As IList(Of T), ByVal selector As Func(Of T, TKey))
    '    Dim comparer = comparer(Of TKey).[Default]
    '    Dim cmp As Integer = comparer(selector(coll(i)), selector(coll(j)))
    'End Sub

    'MergeSort(coll, Function(p, q) p.F.CompareTo(q.F))
    Private Function CompareItemsByToDoID(ByVal objItemLeft As Object, ByVal objItemRight As Object)
        Dim ToDoIDLeft As String = Globals.ThisAddIn.CustomFieldID_GetValue(objItemLeft, "ToDoID")
        Dim ToDoIDRight As String = Globals.ThisAddIn.CustomFieldID_GetValue(objItemRight, "ToDoID")
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
    Private Function MergeSort(Of T)(ByVal coll As IList(Of T), ByVal comparison As Comparison(Of T)) As IList(Of T)
        Dim Result As List(Of T) = New List(Of T)()
        Dim Left As Queue(Of T) = New Queue(Of T)()
        Dim Right As Queue(Of T) = New Queue(Of T)()
        If coll.Count <= 1 Then Return coll
        Dim midpoint As Integer = coll.Count / 2

        For i As Integer = 0 To midpoint - 1
            Left.Enqueue(coll(i))
        Next

        For i As Integer = midpoint To coll.Count - 1
            Right.Enqueue(coll(i))
        Next


        Left = New Queue(Of T)(MergeSort(Left.ToList(), comparison))
        Right = New Queue(Of T)(MergeSort(Right.ToList(), comparison))
        Result = Merge(Left, Right, comparison)
        Return Result
    End Function


    Private Function Merge(Of T)(ByVal Left As Queue(Of T), ByVal Right As Queue(Of T), ByVal comparison As Comparison(Of T)) As List(Of T)
        'Dim cmp As Integer = comparison(coll(i), coll(j))

        Dim Result As List(Of T) = New List(Of T)()

        While Left.Count > 0 AndAlso Right.Count > 0
            Dim cmp As Integer = comparison(Left.Peek(), Right.Peek())
            If cmp < 0 Then
                Result.Add(Left.Dequeue())
            Else
                Result.Add(Right.Dequeue())
            End If
        End While

        While Left.Count > 0
            Result.Add(Left.Dequeue())
        End While

        While Right.Count > 0
            Result.Add(Right.Dequeue())
        End While

        Return Result
    End Function


    Private Sub TaskTreeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load


        TreeListView1.CanExpandGetter = Function(ByVal x) CType(x, TreeNode(Of ToDoItem)).ChildCount > 0
        TreeListView1.ChildrenGetter = Function(ByVal x) CType(x, TreeNode(Of ToDoItem)).Children
        TreeListView1.Roots = DM.ListOfToDoTree

        Dim sink1 = CType(Me.TreeListView1.DropSink, SimpleDropSink)
        sink1.AcceptExternal = True
        sink1.CanDropBetween = True
        sink1.CanDropOnBackground = True

    End Sub

    Private Sub HandleModelCanDrop(ByVal sender As Object, ByVal e As ModelDropEventArgs) Handles TreeListView1.ModelCanDrop
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

    'Private Sub HandleCanDrop(ByVal sender As Object, ByVal e As OlvDropEventArgs)
    '    ' This will only be triggered if HandleModelCanDrop doesn't set Handled to true.
    '    ' In practice, this will only be called when the source of the drag is not an ObjectListView

    '    Dim data As IDataObject = TryCast(e.DataObject, IDataObject)
    '    If data Is Nothing OrElse Not data.GetDataPresent(DataFormats.UnicodeText) Then Return
    '    Dim str As String = TryCast(data.GetData(DataFormats.UnicodeText), String)
    '    e.Effect = If(String.IsNullOrEmpty(str), DragDropEffects.None, DragDropEffects.Copy)

    '    Select Case e.DropTargetLocation
    '        Case DropTargetLocation.AboveItem, DropTargetLocation.BelowItem
    '            e.InfoMessage = "Cannot drop between items -- because I haven't written the logic :)"
    '        Case DropTargetLocation.Background
    '            e.InfoMessage = "Drop here to create a new root item called '" & str & "'"
    '        Case DropTargetLocation.Item
    '            e.InfoMessage = "Drop here to create a new child item called '" & str & "'"
    '        Case Else
    '            Return
    '    End Select
    'End Sub


    Private Sub HandleModelDropped(ByVal sender As Object, ByVal e As ModelDropEventArgs) Handles TreeListView1.ModelDropped
        e.Handled = True

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
    End Sub




    Private Shared Sub MoveObjectsToRoots(ByVal targetTree As TreeListView, ByVal sourceTree As TreeListView, ByVal toMove As IList)
        If sourceTree Is targetTree Then                'Data Model: Check to see if the desination tree roots are in the same tree
            For Each x As TreeNode(Of ToDoItem) In toMove

                If x.Parent IsNot Nothing Then
                    x.Parent.RemoveChild(x)             'Data Model: Remove pointer to node from parent.children list
                    'x.Parent.Children.Remove(x)         'Data Model: Remove pointer to node from parent.children list
                    'x.Parent = Nothing                  'Data Model: Set the pointer to the parent inside the node to nothing
                    sourceTree.AddObject(x)             'TreeListView: Add the node to the source tree as a Root node
                End If
            Next
        Else                                            'Data Model: If the destination tree is different than the source tree

            For Each x As TreeNode(Of ToDoItem) In toMove

                If x.Parent Is Nothing Then             'Data Model: If the node was a root in the old tree
                    sourceTree.RemoveObject(x)          'TreeListView: Delete the pointer in the tree to the node
                Else                                    'Data Model: If the node was NOT a root in the old tree
                    x.Parent.RemoveChild(x)             'Data Model: Grab the parent node and delete the pointer from the list of children
                End If

                x.Parent = Nothing                      'Data Model: Delete the pointer in the node to the parent
                targetTree.AddObject(x)                 'TreeListView: Add the node to the new tree as a root
            Next
        End If
    End Sub

    Private Sub MoveObjectsToSibling(ByVal targetTree As TreeListView,
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
                If DM.ListOfToDoTree.Contains(x) Then
                    DM.ListOfToDoTree.Remove(x)         'Data Model: Remove node from roots
                Else
                    MsgBox("Error in MoveObjectsToSibling: TreeListView and DataModel out of sync at roots")
                End If
            Else
                x.Parent.RemoveChild(x)                 'Data Model: Remove Child from old Parent
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
            DM.ListOfToDoTree.AddRange(toMove)
            Dim i
            Dim strSeed As String = ""
            If DM.ListOfToDoTree.Count > toMove.Count Then
                strSeed = DM.ListOfToDoTree(DM.ListOfToDoTree.Count - toMove.Count - 2).Value.ToDoID
            Else
                strSeed = "00"
            End If
            For i = DM.ListOfToDoTree.Count - toMove.Count - 1 To DM.ListOfToDoTree.Count - 1
                strSeed = Globals.ThisAddIn.IDList.GetNextAvailableToDoID(strSeed)
                DM.ListOfToDoTree(i).Value.ToDoID = strSeed
            Next
        Else
            'Insert moved object into DATAMODEL children of new parent
            Dim idx As Integer = target.Parent.Children.IndexOf(target) + siblingOffset
            'Inconsistent with case of Parent is nothing
            target.Parent.Children.InsertRange(idx, toMove.Cast(Of TreeNode(Of ToDoItem))()) 'DataModel: Inserted into new data model tree. 
            DM.ReNumberChildrenIDs(target.Parent.Children, Globals.ThisAddIn.IDList)         'DataModel: Renumber IDs of new branch order

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

    Private Sub MoveObjectsToChildren(ByVal targetTree As TreeListView,
                                      ByVal sourceTree As TreeListView,
                                      ByVal target As TreeNode(Of ToDoItem),
                                      ByVal toMove As IList)

        'Dim strID_PrefixOld As String
        'Dim strID_PrefixNew As String

        For Each x As TreeNode(Of ToDoItem) In toMove

            'strID_PrefixOld = x.ID 'Kill this line

            If x.Parent Is Nothing Then
                sourceTree.RemoveObject(x)              'TreeListView: Remove from Visual Tree
                If DM.ListOfToDoTree.Contains(x) Then
                    DM.ListOfToDoTree.Remove(x)         'Data Model: Remove node from roots
                Else
                    MsgBox("Error in MoveObjectsToChildren: TreeListView and DataModel out of sync at roots")
                End If
            Else
                x.Parent.Children.Remove(x)             'Data Model: Remove pointer to child from parent
                '***NO REFERENCE TO TREELISTVIEW. INCONSISTENT WITH TREATMENT OF ROOTS
            End If

            x.Parent = target                                   'Data Model: Add pointer to new Parent in data model
            DM.AddChild(x, target, Globals.ThisAddIn.IDList)    'Data Model: Add child to parent and renumber all affected

            '***OLD Code to add child to target parent and renumber
            'target.AddChild(x)                      'Data Model: Add child to target parent and renumber grandchildren
            'target.Children.Add(x)
            'x.Value.ToDoID = x.ID
            'strID_PrefixNew = x.ID
            'For Each y As TreeNode(Of ToDoItem) In x.Children
            '    SubstituteIDPrefix(y, strID_PrefixOld, strID_PrefixNew)
            'Next
        Next
        'WriteTreeToDisk()

        'Curious ... this is inconsistent with MoveObjectsToSibling
    End Sub



    'Private Sub SubstituteIDPrefix(node As TreeNode(Of ToDoItem), strOld As String, strNew As String)
    '    If Mid(node.ID, 1, strOld.Length) = strOld Then
    '        Dim OldID As String = node.ID
    '        node.ID = strNew & Mid(node.ID, strOld.Length + 1, node.ID.Length - strOld.Length)
    '        node.Value.ToDoID = node.ID
    '        If Globals.ThisAddIn.UsedIDList.Contains(OldID) Then
    '            Globals.ThisAddIn.UsedIDList.Remove(OldID)
    '        End If
    '    End If

    '    For Each child In node.Children
    '        SubstituteIDPrefix(child, strOld, strNew)
    '    Next
    'End Sub

    Private Function FindChildByID(ID As String, nodes As List(Of TreeNode(Of ToDoItem))) As TreeNode(Of ToDoItem)
        Dim node As TreeNode(Of ToDoItem)
        Dim rnode As TreeNode(Of ToDoItem)

        For Each node In nodes
            If node.ID = ID Then
                Return node
            Else
                rnode = FindChildByID(ID, node.Children)
                If Not rnode Is Nothing Then
                    Return rnode
                End If
            End If
        Next

        Return Nothing

    End Function
    'Private Function NextChildId(nodes As List(Of TreeNode(Of ToDoItem))) As String

    '    Dim strMaxID As String = "00"
    '    Dim lngMaxID As Long = 0
    '    Dim strTmpID As String = ""
    '    Dim lngTmpID As Long = 0
    '    For Each child In nodes
    '        strTmpID = child.ID
    '        lngTmpID = ConvertToDecimal(125, strTmpID)
    '        If lngTmpID > lngMaxID Then
    '            lngMaxID = lngTmpID
    '        End If
    '    Next child

    '    Dim blContinue As Boolean = True
    '    While blContinue
    '        lngMaxID += 1
    '        strMaxID = ConvertToBase(125, lngMaxID)
    '        If Globals.ThisAddIn.UsedIDList.Contains(strMaxID) = False Then
    '            blContinue = False
    '        End If
    '    End While
    '    Globals.ThisAddIn.UsedIDList_Append(strMaxID)
    '    Return strMaxID

    'End Function

    Public Sub WriteTreeToDisk()
        Dim filename As String = "C:\Users\03311352\Documents\DebugTreeDump.csv"

        Using sw As StreamWriter = New StreamWriter(filename)
            sw.WriteLine("File Dump")
        End Using

        'LoopTreeToWrite(ToDoTree.Children, filename, "")
        LoopTreeToWrite(ToDoTree, filename, "")
    End Sub
    Public Sub LoopTreeToWrite(nodes As List(Of TreeNode(Of ToDoItem)), filename As String, lineprefix As String)
        If Not nodes Is Nothing Then
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

    Private Sub TreeListView1_ItemActivate(sender As Object, e As EventArgs) Handles TreeListView1.ItemActivate
        Dim item = TreeListView1.GetItem(TreeListView1.SelectedIndex).RowObject
        Dim node As TreeNode(Of ToDoItem) = TryCast(item, TreeNode(Of ToDoItem))
        If Not node Is Nothing Then
            Dim objItem As Object = node.Value.GetItem()
            If TypeOf objItem Is Outlook.MailItem Then
                Dim OlMail As Outlook.MailItem = TryCast(objItem, Outlook.MailItem)
                If Not OlMail Is Nothing Then
                    Dim OlExplorer As Outlook.Explorer = Globals.ThisAddIn.Application.ActiveExplorer
                    If OlExplorer.IsItemSelectableInView(OlMail) Then
                        OlExplorer.ClearSelection()
                        OlExplorer.AddToSelection(OlMail)
                    Else
                        OlMail.Display()
                    End If
                End If
            ElseIf TypeOf objItem Is Outlook.TaskItem Then
                Dim OlTask As Outlook.TaskItem = TryCast(objItem, Outlook.TaskItem)
                If Not OlTask Is Nothing Then
                    Dim OlExplorer As Outlook.Explorer = Globals.ThisAddIn.Application.ActiveExplorer
                    If OlExplorer.IsItemSelectableInView(OlTask) Then
                        OlExplorer.ClearSelection()
                        OlExplorer.AddToSelection(OlTask)
                    Else
                        OlTask.Display()
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click

    End Sub

    Private Sub FormatRow(sender As Object, e As FormatRowEventArgs) Handles TreeListView1.FormatRow
        Console.WriteLine("In Format Row")
        'Dim node As TreeNode(Of ToDoItem)
        'node = TryCast(e.Model.Value, TreeNode(Of ToDoItem))
        Dim objToDo As ToDoItem = TryCast(e.Model.Value, ToDoItem)
        'If e.Model.Value.Complete Then
        If objToDo.Complete Then
            e.Item.Font = New Font(e.Item.Font, e.Item.Font.Style Or FontStyle.Strikeout)
        Else
            e.Item.Font = New Font(e.Item.Font, e.Item.Font.Style And Not FontStyle.Strikeout)
        End If
    End Sub
End Class