Public Class cSuggestions

    Private _count As Integer
    Private _strFolderArray() As String
    Private lngValor() As Long
    Const MaxSuggestions = 5

    Public ReadOnly Property Count() As Integer
        Get
            Return _count
        End Get
    End Property

    Public Property Valor() As Long()
        Get
            Valor = lngValor
        End Get
        Set(value As Long())
            lngValor = value
        End Set
    End Property

    Public Property FolderList() As String()
        Get
            FolderList = _strFolderArray
        End Get
        Set(value As String())
            _strFolderArray = value
        End Set
    End Property

    Public ReadOnly Property FolderList_ItemByIndex(idx As Integer) As String
        Get
            Return _strFolderArray(idx)
        End Get

    End Property


    Private Function find(strFolderName As String) As Integer

        Dim i As Integer
        find = 0
        For i = 1 To UBound(_strFolderArray)
            If _strFolderArray(i) = strFolderName Then find = i
        Next i

    End Function

    Public Sub ADD_END(fldr As String)
        Dim i As Integer

        _count = _count + 1
        ReDim Preserve _strFolderArray(_count)
        ReDim Preserve lngValor(_count)

        _strFolderArray(_count) = fldr
        lngValor(_count) = 0

        For i = 1 To _count
            lngValor(i) = lngValor(i) + 1
        Next i
    End Sub
    Public Sub Add(fldr As String, ByVal Val As Long, Optional mxsug As Integer = MaxSuggestions)
        Dim i, j As Integer
        Dim added As Boolean
        Dim found As Integer
        Dim tempStr As String
        Dim tempVal As Long

        added = False

        If _count = 0 Then                                                       '
            ReDim _strFolderArray(1)
            ReDim lngValor(1)
            _count = 1
            _strFolderArray(1) = fldr
            lngValor(1) = Val

        Else

            found = find(fldr)
            If found = 0 Then                                                           ' Check to see if folder has already been captured in results
                ' If not, run the code below
                '_____________________________________________________________________
                '------ Case where we add a new folder entry--------------------------

                If _count < mxsug Then                                  'If there are less results than the max, add a result
                    _count = _count + 1
                    ReDim Preserve _strFolderArray(_count)
                    ReDim Preserve lngValor(_count)
                End If

                For i = 1 To _count - 1                                          'Put the result into the right sequence based on
                    If Val > lngValor(i) Then                                       'highest score to lowest score
                        added = True
                        For j = _count - 1 To i Step -1                          'Loop shifts every entry down one for middle insertion
                            _strFolderArray(j + 1) = _strFolderArray(j)
                            lngValor(j + 1) = lngValor(j)
                        Next j
                        _strFolderArray(i) = fldr
                        lngValor(i) = Val
                        Exit For
                    End If
                Next i

                If added = False Then                                                   'If it was not at the beginning or in the middle,
                    If Val > lngValor(_count) Then                            'Check to see if it goes at the end
                        _strFolderArray(_count) = fldr                          'and replace the last entry if it is better
                        lngValor(_count) = Val
                    End If
                End If
                '_____________________________________________________________________

            Else
                '_____________________________________________________________________
                '------ Case where we add the value to an existing entry and resort---
                lngValor(found) = lngValor(found) + Val
                If found > 1 Then
                    For i = found To 2 Step -1


                        If lngValor(i) > lngValor(i - 1) Then                           'If the entry above has a lower value, switch them
                            tempVal = lngValor(i - 1)
                            lngValor(i - 1) = lngValor(i)
                            lngValor(i) = tempVal
                            tempStr = _strFolderArray(i - 1)
                            _strFolderArray(i - 1) = _strFolderArray(i)
                            _strFolderArray(i) = tempStr
                        Else
                            Exit For                                                'Stop reordering when it is in order
                        End If


                    Next i                                                              'End loop to raise up higher values to top
                End If
                '-----End case--------------------------------------------------------
                '_____________________________________________________________________


            End If
        End If
        '*******************END BLOCK for where we already have values***************
        '****************************************************************************
        '    StopWatch_ADD.Pause
    End Sub

    Public Sub PrintDebug()
        Dim i As Integer
        For i = 1 To _count
            Debug.WriteLine("Folder: " & _strFolderArray(i) & "   Value: " & lngValor(i))
        Next i
    End Sub


End Class
