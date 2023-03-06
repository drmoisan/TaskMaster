Imports System.IO
Imports Microsoft.Office.Interop.Outlook


Public Class DataModel_ToDoTree
    Public Enum LoadOptions
        vbLoadAll = 0
        vbLoadInView = 1
    End Enum

    Public Sub New()
        ListOfToDoTree = New List(Of TreeNode(Of ToDoItem))
    End Sub
    Public Sub New(DM_ToDoTree As List(Of TreeNode(Of ToDoItem)))
        ListOfToDoTree = DM_ToDoTree
    End Sub
    Public Sub LoadTree(LoadType As LoadOptions, Application As Application)
        Dim objItem As Object

        Dim strTemp As String
        Dim strPrev As String
        Dim colItems As Collection
        strPrev = ""
        strTemp = ""

        Try
            '***STEP 1: LOAD RAW [ITEMS] TO A LIST AND SORT THEM***
            Dim TreeItems As List(Of Object) = GetToDoList(LoadType, Application)
            TreeItems = MergeSort(Of Object)(TreeItems, AddressOf CompareItemsByToDoID)

            colItems = New Collection
            Dim colNoID = New Collection
            Dim tmpToDo As ToDoItem = Nothing
            Dim ToDoNode As TreeNode(Of ToDoItem)
            Dim NodeParent As TreeNode(Of ToDoItem)


            '***STEP 2: ADD ITEMS TO A FLAT TREE & ASSIGN IDs TO THOSE THAT DON'T HAVE THEM***
            ' Iterate through ToDo items in List
            For Each objItem In TreeItems

                'Cast objItem to temporary ToDoItem
                If TypeOf objItem Is MailItem Then
                    tmpToDo = New ToDoItem(CType(objItem, MailItem))
                ElseIf TypeOf objItem Is TaskItem Then
                    tmpToDo = New ToDoItem(CType(objItem, TaskItem))
                End If

                'Add the temporary ToDoItem to the tree, assigning an ID if missing
                'If tmpToDo.ToDoID = "nothing" Then
                'ToDoTree.AddChild(tmpToDo)
                ListOfToDoTree.Add(New TreeNode(Of ToDoItem)(tmpToDo))
                'Else
                'ToDoTree.AddChild(tmpToDo, tmpToDo.ToDoID)
                'ToDoTree.Add(New TreeNode(Of ToDoItem)(tmpToDo, tmpToDo.ToDoID))
                'End If
            Next

            '***STEP 3: MAKE TREE HIERARCHICAL
            Dim max As Integer = ListOfToDoTree.Count - 1
            Dim i As Integer

            'Loop through the tree from the end to the beginning
            For i = max To 0 Step -1
                ToDoNode = ListOfToDoTree(i)

                'If the ID is bigger than 2 digits, it is a child of someone. 
                'So in that case link it to the proper parent
                'First try cutting off the last two digits, but in the case of
                'Filtered Items, it is possible that the parent is not visible.
                'If the parent is not visible, work recursively to find the next 
                'closest visible parent until you get to the root
                If ToDoNode.Value.ToDoID.Length > 2 Then
                    Dim strID As String = ToDoNode.Value.ToDoID
                    Dim strParentID As String = Mid(strID, 1, strID.Length - 2)
                    Dim blContinue As Boolean = True

                    While blContinue
                        NodeParent = FindChildByID(strParentID, ListOfToDoTree)
                        'NodeParent = F
                        If NodeParent IsNot Nothing Then
                            Dim unused2 = NodeParent.InsertChild(ToDoNode)
                            Dim unused1 = ListOfToDoTree.Remove(ToDoNode)
                            blContinue = False
                        End If
                        If strParentID.Length > 2 Then
                            strParentID = Mid(strParentID, 1, strParentID.Length - 2)
                        Else
                            blContinue = False
                        End If
                    End While
                End If
            Next i


        Catch
            Debug.WriteLine(Err.Description)
            Dim unused = MsgBox(Err.Description)
        End Try
    End Sub
    Public ReadOnly Property ListOfToDoTree As List(Of TreeNode(Of ToDoItem)) = New List(Of TreeNode(Of ToDoItem))



    Public Function CompareToDoID(item As ToDoItem, strToDoID As String) As Boolean
        Return item.ToDoID = strToDoID
    End Function

    Public Sub AddChild(ByVal Child As TreeNode(Of ToDoItem), Parent As TreeNode(Of ToDoItem), IDList As ListOfIDs)
        Parent.Children.Add(Child)
        Dim strSeed = If(Parent.Children.Count > 1, Parent.Children(Parent.Children.Count - 2).Value.ToDoID, Parent.Value.ToDoID & "00")

        If IDList.UsedIDList.Contains(Child.Value.ToDoID) Then
            Dim unused = IDList.UsedIDList.Remove(Child.Value.ToDoID)
        End If
        Child.Value.ToDoID = IDList.GetNextAvailableToDoID(strSeed)
        If Child.Children.Count > 0 Then
            ReNumberChildrenIDs(Child.Children, IDList)
        End If
        IDList.Save()
    End Sub

    Public Sub ReNumberIDs(IDList As ListOfIDs)
        'WriteTreeToDisk()


        For Each RootNode In ListOfToDoTree
            For Each Child In RootNode.Children
                If Child.Children.Count > 0 Then ReNumberChildrenIDs(Child.Children, IDList)
            Next
        Next
        'WriteTreeToDisk()
    End Sub
    Public Sub ReNumberChildrenIDs(Children As List(Of TreeNode(Of ToDoItem)), IDList As ListOfIDs)

        Dim i As Integer
        Dim max As Integer = Children.Count - 1
        If max >= 0 Then
            Dim strParentID As String = Children(i).Parent.Value.ToDoID
            For i = 0 To max
                If IDList.UsedIDList.Contains(Children(i).Value.ToDoID) Then Dim unused = IDList.UsedIDList.Remove(Children(i).Value.ToDoID)
            Next i
            For i = 0 To max
                Dim NextID As String = IDList.GetNextAvailableToDoID(strParentID & "00")
                'Dim LevelChange As Boolean = (Children(i).Value.ToDoID.Length = NextID.Length)
                Children(i).Value.ToDoID = NextID
                'Children(i).Value.VisibleTreeState = 67
                'Children(i).Value.ToDoID = Children(i).Value.ToDoID
                If Children(i).Children.Count > 0 Then ReNumberChildrenIDs(Children(i).Children, IDList)
            Next
            IDList.Save()
        End If
    End Sub


    Public Function FindChildByID(ID As String, nodes As List(Of TreeNode(Of ToDoItem))) As TreeNode(Of ToDoItem)
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
    Public Function GetToDoList(LoadType As LoadOptions,
                                Application As Application) As List(Of Object)

        Dim OlItems As Items
        Dim objView As View
        Dim OlFolder As Folder
        Dim strFilter As String
        Dim oStore As [Store]
        Dim objItem As Object
        Dim ListObjects As New List(Of Object)

        objView = Application.ActiveExplorer.CurrentView
        strFilter = "@SQL=" & objView.Filter

        For Each oStore In Application.Session.Stores
            OlItems = Nothing
            OlFolder = oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo)
            OlItems = If(strFilter = "@SQL=" Or LoadType = LoadOptions.vbLoadAll, OlFolder.Items, OlFolder.Items.Restrict(strFilter))
            For Each objItem In OlItems
                ListObjects.Add(objItem)
            Next
        Next

        Return ListObjects
    End Function

    Private Function IsHeader(TagContext As String) As String
        If InStr(TagContext, "@PROJECTS", CompareMethod.Text) Then
            Return True
        ElseIf InStr(TagContext, "HEADER", CompareMethod.Text) Then
            Return True
        ElseIf InStr(TagContext, "DELIVERABLE", CompareMethod.Text) Then
            Return True
        ElseIf InStr(TagContext, "@PROGRAMS", CompareMethod.Text) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub HideEmptyHeadersInView()
        Dim action As Action(Of TreeNode(Of ToDoItem)) = Sub(node)
                                                             If node.ChildCount = 0 Then
                                                                 If IsHeader(node.Value.Context) Then
                                                                     node.Value.ActiveBranch = False
                                                                 End If
                                                             End If
                                                         End Sub

        For Each node As TreeNode(Of ToDoItem) In ListOfToDoTree
            node.Traverse(action)
        Next
    End Sub

    Private Function CompareItemsByToDoID(ByVal objItemLeft As Object, ByVal objItemRight As Object)
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
    Private Function MergeSort(Of T)(ByVal coll As IList(Of T), ByVal comparison As Comparison(Of T)) As IList(Of T)
        Dim Result As New List(Of T)()
        Dim Left As New Queue(Of T)()
        Dim Right As New Queue(Of T)()
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

        Dim Result As New List(Of T)()

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
    Public Sub WriteTreeToDisk()
        Dim filename As String = "C:\Users\03311352\Documents\DebugTreeDump.csv"

        Using sw As New StreamWriter(filename)
            sw.WriteLine("File Dump")
        End Using

        LoopTreeToWrite(ListOfToDoTree, filename, "")
    End Sub
    Private Sub LoopTreeToWrite(nodes As List(Of TreeNode(Of ToDoItem)), filename As String, lineprefix As String)
        If nodes IsNot Nothing Then
            For Each node As TreeNode(Of ToDoItem) In nodes
                AppendLineToCSV(filename, lineprefix & node.Value.ToDoID & " " & node.Value.TaskSubject)
                LoopTreeToWrite(node.Children, filename, lineprefix & node.Value.ToDoID & ",")
            Next
        End If
    End Sub
    Private Sub AppendLineToCSV(filename As String, line As String)
        Using sw As StreamWriter = File.AppendText(filename)
            sw.WriteLine(line)
        End Using
    End Sub


End Class

