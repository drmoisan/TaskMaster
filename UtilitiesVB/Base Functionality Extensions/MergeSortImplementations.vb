Public Module MergeSortImplementations
    Private Function MergeSort(Of T)(ByVal coll As IList(Of T), ByVal comparison As Comparison(Of T)) As IList(Of T)
        'DONE: 2023-03-06 Move To UtilitiesVB. A generic merge sort(of T) doesn't belong in a form controller
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
        'DONE: 2023-03-06 Move To UtilitiesVB. A generic merge sort(of T) doesn't belong in a form controller

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
End Module
