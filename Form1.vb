Imports System.Windows.Forms
Imports System.Diagnostics
'Imports System.Collections.Generic
Imports System.IO
Imports System
Imports System.Drawing


Public Class frm_TaskTree

    Private TreeItems As Outlook.Items
    Private colItems As Collection



    Private Sub TaskTree_ItemDrag(sender As Object, e As ItemDragEventArgs) Handles TaskTree.ItemDrag
        'Move the dragged node when the left mouse button Is used.
        Debug.WriteLine("Fired ItemDrag")
        If (e.Button = MouseButtons.Left) Then
            DoDragDrop(e.Item, DragDropEffects.Move)
            'Copy the dragged node when the right mouse button Is used.
        ElseIf (e.Button = MouseButtons.Right) Then
            DoDragDrop(e.Item, DragDropEffects.Copy)
        End If
    End Sub

    '// Set the target drop effect to the effect 
    '// specified in the ItemDrag event handler.
    Private Sub TaskTree_DragEnter(sender As Object, e As DragEventArgs) Handles TaskTree.DragEnter
        Debug.WriteLine("Fired DragEnter")
        e.Effect = e.AllowedEffect
    End Sub

    '// Select the node under the mouse pointer to indicate the 
    '// expected drop location.
    Private Sub TaskTree_DragOver(sender As Object, e As DragEventArgs) Handles TaskTree.DragOver
        Debug.WriteLine("Fired DragOver")
        '// Retrieve the client coordinates of the mouse position.
        Dim targetPoint As Point = TaskTree.PointToClient(New Point(e.X, e.Y))

        '// Select the node at the mouse position.
        TaskTree.SelectedNode = TaskTree.GetNodeAt(targetPoint)
    End Sub

    Private Sub TaskTree_DragDrop(sender As Object, e As DragEventArgs) Handles TaskTree.DragDrop
        Debug.WriteLine("Fired DragDrop")
        '// Retrieve the client coordinates of the drop location.
        Dim targetPoint As Point = TaskTree.PointToClient(New Point(e.X, e.Y))

        '// Retrieve the node at the drop location.
        Dim targetNode As TreeNode = TaskTree.GetNodeAt(targetPoint)

        '// Retrieve the node that was dragged.
        'TreeNode draggedNode = e.Data.GetData(TypeOf (TreeNode))
        Dim draggedNode As TreeNode =
          CType(e.Data.GetData("System.Windows.Forms.TreeNode"),
          TreeNode)

        '// Confirm that the node at the drop location Is Not 
        '// the dragged node Or a descendant of the dragged node.
        If Not draggedNode.Equals(targetNode) And Not ContainsNode(draggedNode, targetNode) Then

            '// If it Is a move operation, remove the node from its current 
            '// location And add it to the node at the drop location.
            If (e.Effect = DragDropEffects.Move) Then
                'Dim strNewName As String = AssignNextNodeID_SameTreeLevel(targetNode)
                GraftInBranch(targetNode, draggedNode, blMove:=True)
                'draggedNode.Remove()
                'targetNode.Nodes.Add(draggedNode)


                '// If it Is a copy operation, clone the dragged node 
                '// And add it to the node at the drop location.
            ElseIf (e.Effect = DragDropEffects.Copy) Then
                targetNode.Nodes.Add(draggedNode.Clone())
            End If

            '// Expand the node at the location 
            '// to show the dropped node.
            targetNode.Expand()
        End If
    End Sub

    '// Determine whether one node Is a parent 
    '// Or ancestor of a second node.
    Private Function ContainsNode(node1 As TreeNode, node2 As TreeNode) As Boolean

        '// Check the parent node of the second node.
        If (node2.Parent Is Nothing) Then Return False
        If (node2.Parent.Equals(node1)) Then Return True

        '// If the parent node Is Not null Or equal to the first node, 
        '// call the ContainsNode method recursively using the parent of 
        '// the second node.
        Return ContainsNode(node1, node2.Parent)
    End Function

    Friend Function Init()
        Dim objItem As Object

        Dim strTemp As String
        Dim strPrev As String
        Dim lngDepthTemp As Integer
        Dim lngDepthPrev As Integer
        Dim intMatchDepth As Integer
        Dim ParentNode As TreeNode

        strPrev = ""
        strTemp = ""



        Try
            TreeItems = Globals.ThisAddIn.GetItemsInView_ToDo
            TreeItems.Sort("ToDoID")
            colItems = New Collection
            Dim colNoID = New Collection

            Dim sw = New StreamWriter("C:\Users\03311352\Documents\TreeList.csv")
            For Each objItem In TreeItems
                sw.WriteLine(Globals.ThisAddIn.CustomFieldID_GetValue(objItem, "ToDoID") & " " & GetTaskSubject(objItem))
            Next
            sw.Close()

            For Each objItem In TreeItems
                strTemp = Globals.ThisAddIn.CustomFieldID_GetValue(objItem, "ToDoID")
                If strTemp.Length <> 0 Then
                    colItems.Add(objItem, CStr(ConvertToDecimal(125, strTemp)))
                    If strTemp.Length <= 2 Then
                        TaskTree.Nodes.Add(strTemp, strTemp & " " & GetTaskSubject(objItem))
                    Else
                        ParentNode = GetNodeByHierarchy(TaskTree.Nodes, Mid(strTemp, 1, strTemp.Length - 2))
                        If Not ParentNode Is Nothing Then
                            TaskTree.SelectedNode = ParentNode
                            TaskTree.SelectedNode.Nodes.Add(strTemp, strTemp & " " & GetTaskSubject(objItem))
                        Else
                            TaskTree.Nodes.Add(strTemp, strTemp & " " & GetTaskSubject(objItem))
                        End If
                    End If
                Else
                    colNoID.Add(objItem)
                End If
            Next
            If colNoID.Count > 0 Then

            End If
        Catch ex As Exception
            Debug.WriteLine(Err.Description)

        End Try
    End Function

    Private Function GetTaskSubject(objItem As Object) As String
        Dim OlMail As Outlook.MailItem
        Dim OlTask As Outlook.TaskItem

        If TypeOf objItem Is Outlook.MailItem Then
            OlMail = objItem
            GetTaskSubject = OlMail.TaskSubject
        ElseIf TypeOf objItem Is Outlook.TaskItem Then
            OlTask = objItem
            GetTaskSubject = OlTask.Subject
        Else
            GetTaskSubject = ""
        End If

    End Function


    'Private Function GetNodeByName(nodes As TreeNodeCollection, searchtext As String) As TreeNode

    '    Dim n_found_node As TreeNode = Nothing
    '    Dim b_node_found As Boolean = False
    '    Dim node As TreeNode = Nothing

    '    GetNodeByName = Nothing
    '    For Each node In nodes
    '        If node.Name = searchtext Then
    '            b_node_found = True
    '            n_found_node = node
    '            GetNodeByName = n_found_node
    '        End If
    '        If Not b_node_found Then
    '            n_found_node = GetNodeByName(node.Nodes, searchtext)
    '            If Not n_found_node Is Nothing Then
    '                GetNodeByName = n_found_node
    '            End If
    '        End If
    '    Next
    'End Function
    Private Function GraftInBranch(targetNode As TreeNode, draggedNode As TreeNode, Optional blMove As Boolean = False)
        Dim childnode As TreeNode
        Dim children As TreeNodeCollection
        Dim strNewName As String
        If blMove = True Then
            strNewName = AssignNextNodeID_SameTreeLevel(targetNode)
        Else
            strNewName = targetNode.Name & Mid(draggedNode.Name, draggedNode.Name.Length - 1, 2)
        End If
        Dim strNewNameDec As String = CStr(ConvertToDecimal(125, strNewName))
        Dim strOldName As String = draggedNode.Name
        Dim strOldNameDec As String = CStr(ConvertToDecimal(125, strOldName))
        Dim strTargetNameDec As String = CStr(ConvertToDecimal(125, targetNode.Name))
        Dim strNewDetails As String = strNewName & " " & Mid(draggedNode.Text, strOldName.Length + 1,
                                                             draggedNode.Text.Length - (strOldName.Length))

        Dim objItem As Object = colItems(strOldNameDec)
        Globals.ThisAddIn.CustomFieldID_Set("ToDoID", strNewName, SpecificItem:=objItem)
        colItems.Remove(strOldNameDec)
        colItems.Add(objItem, strNewNameDec, After:=strTargetNameDec)

        If blMove Then
            draggedNode.Remove()
            targetNode.Nodes.Add(draggedNode)
        End If

        draggedNode.Name = strNewName
        draggedNode.Text = strNewDetails
        'targetNode.Nodes.Add(draggedNode)

        children = draggedNode.Nodes
        For Each childnode In children
            GraftInBranch(draggedNode, childnode)
        Next


    End Function

    Private Function GetNodeByHierarchy(nodes As TreeNodeCollection, ByVal searchtext As String) As TreeNode
        Dim nodestemp() As TreeNode
        'Dim nodesfiltered As IEnumerable(Of TreeNode)
        While searchtext.Length >= 2
            nodestemp = nodes.Find(searchtext, True)
            nodestemp = nodestemp.Where(Function(a, index) a.Name = searchtext).ToArray

            If nodestemp.Length <> 0 Then
                If nodestemp.Length = 1 Then
                    Return nodestemp(0)
                Else
                    MsgBox("Multiple Nodes Matching " & searchtext)
                    Return Nothing
                End If
            Else
                searchtext = Mid(searchtext, 1, searchtext.Length - 2)
            End If
        End While

        Return Nothing
        'Dim strSearch As String
        'Dim node As TreeNode
        'Dim i As Integer = 1
        'Dim blContinue As Boolean = True

        'While blContinue And i * 2 <= searchtext.Length
        '    strSearch = Mid(searchtext, 1, i * 2)
        '    For Each node In nodes
        '        If node.Name = strSearch Then
        '            If strSearch.Length <> searchtext.Length Then
        '                blContinue = False
        '                Return GetNodeByHierarchy(node.Nodes, Mid(searchtext, i * 2 + 1, searchtext.Length - (i * 2 + 1)))
        '            Else
        '                blContinue = False
        '                Return node
        '            End If

        '        End If
        '    Next
        '    i += 1
        'End While
        'Return Nothing
    End Function

    Private Function AssignNextNodeID_SameTreeLevel(ParentNode As TreeNode) As String
        Dim node As TreeNode = Nothing
        Dim nodes As TreeNodeCollection = ParentNode.Nodes
        Dim lngMax As Long = 0
        Dim lngTmp As Long = 0
        Dim strResp As String

        Try
            If nodes.Count = 0 Then
                strResp = ParentNode.Name & "01"
                Return strResp
            Else
                For Each node In nodes
                    lngTmp = ConvertToDecimal(125, node.Name)
                    If lngTmp > lngMax Then lngMax = lngTmp
                Next
                lngMax += 1
                strResp = ConvertToBase(125, lngMax)
                Return strResp
            End If
        Catch
            Debug.WriteLine(Err.Description)
        End Try
    End Function

    Private Function GetNodeByText(nodes As TreeNodeCollection, searchtext As String) As TreeNode
        Dim n_found_node As TreeNode = Nothing
        Dim b_node_found As Boolean = False
        Dim node As TreeNode = Nothing

        GetNodeByText = Nothing

        For Each node In nodes
            If node.Name = searchtext Then
                b_node_found = True
                n_found_node = node
                Return n_found_node
            End If

            If (Not b_node_found) Then
                n_found_node = GetNodeByText(node.Nodes, searchtext)

                If Not n_found_node Is Nothing Then
                    Return n_found_node
                End If
            End If

        Next
    End Function


End Class