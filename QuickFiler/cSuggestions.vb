Public Class cSuggestions

    Private intCount As Integer
    Private strFolderList() As String
    Private lngValor() As Long
    Const MaxSuggestions = 5

    'Public Property Set Class(cls As cSuggestions)
    '    intCount = cls.Count
    '    strFolderList = cls.FolderList
    '    lngValor = cls.Valor
    'End Property

    Public ReadOnly Property Count() As Integer
        Get
            Return intCount
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
            FolderList = strFolderList
        End Get
        Set(value As String())
            strFolderList = value
        End Set
    End Property

    Public ReadOnly Property FolderList_ItemByIndex(idx As Integer) As String
        Get
            Return strFolderList(idx)
        End Get

    End Property


    Private Function find(strFolderName As String) As Integer

        Dim i As Integer
        find = 0
        For i = 1 To UBound(strFolderList)
            If strFolderList(i) = strFolderName Then find = i
        Next i

    End Function

    Public Sub ADD_END(fldr As String)
        Dim i As Integer

        intCount = intCount + 1
        ReDim Preserve strFolderList(intCount)
        ReDim Preserve lngValor(intCount)

        strFolderList(intCount) = fldr
        lngValor(intCount) = 0

        For i = 1 To intCount
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

        If intCount = 0 Then                                                       '
            ReDim strFolderList(1)
            ReDim lngValor(1)
            intCount = 1
            strFolderList(1) = fldr
            lngValor(1) = Val

        Else

            found = find(fldr)
            If found = 0 Then                                                           ' Check to see if folder has already been captured in results
                ' If not, run the code below
                '_____________________________________________________________________
                '------ Case where we add a new folder entry--------------------------

                If intCount < mxsug Then                                  'If there are less results than the max, add a result
                    intCount = intCount + 1
                    ReDim Preserve strFolderList(intCount)
                    ReDim Preserve lngValor(intCount)
                End If

                For i = 1 To intCount - 1                                          'Put the result into the right sequence based on
                    If Val > lngValor(i) Then                                       'highest score to lowest score
                        added = True
                        For j = intCount - 1 To i Step -1                          'Loop shifts every entry down one for middle insertion
                            strFolderList(j + 1) = strFolderList(j)
                            lngValor(j + 1) = lngValor(j)
                        Next j
                        strFolderList(i) = fldr
                        lngValor(i) = Val
                        Exit For
                    End If
                Next i

                If added = False Then                                                   'If it was not at the beginning or in the middle,
                    If Val > lngValor(intCount) Then                            'Check to see if it goes at the end
                        strFolderList(intCount) = fldr                          'and replace the last entry if it is better
                        lngValor(intCount) = Val
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
                            tempStr = strFolderList(i - 1)
                            strFolderList(i - 1) = strFolderList(i)
                            strFolderList(i) = tempStr
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
        For i = 1 To intCount
            Debug.WriteLine("Folder: " & strFolderList(i) & "   Value: " & lngValor(i))
        Next i
    End Sub


End Class
