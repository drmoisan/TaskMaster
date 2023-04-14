Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

Public Class cSuggestions

    Private _count As Integer
    Private _strFolderArray() As String
    Private lngValor() As Long
    Const MaxSuggestions = 5

    Public Sub New()

    End Sub

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

    Public Property FolderSuggestionsArray() As String()
        Get
            FolderSuggestionsArray = _strFolderArray
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

            End If
        End If

    End Sub

    Public Sub PrintDebug()
        Dim i As Integer
        For i = 1 To _count
            Debug.WriteLine("Folder: " & _strFolderArray(i) & "   Value: " & lngValor(i))
        Next i
    End Sub


    Public Sub RefreshSuggestions(OlMail As MailItem,
                                  AppGlobals As IApplicationGlobals,
                                  Optional ReloadCTFStagingFiles As Boolean = True,
                                  Optional ByVal InBackground As Boolean = False)

        Dim _globals As IApplicationGlobals = AppGlobals

        'QUESION: Will reloading staging files for CTF ever be necessary. I think not.
        If ReloadCTFStagingFiles Then ReloadStagingFiles(_globals)

        ClearSuggestions()
        AddConversationBasedSuggestions(OlMail, _globals)
        AddAnythingInAutoFileField(OlMail, _globals)
        AddWordSequenceSuggestions(OlMail, AppGlobals)
    End Sub

    Private Sub ClearSuggestions()
        Array.Clear(_strFolderArray, 0, _strFolderArray.Length)
    End Sub

    Private Sub AddWordSequenceSuggestions(OlMail As MailItem, AppGlobals As IApplicationGlobals)
        Dim i As Integer
        Dim Matrix(,) As Object = Nothing
        Dim SubjectStripped As String
        Dim SWVal, Val, Val1 As Long
        Dim strTmpFldr As String
        Dim varFldrSubs As Object

        SubjectStripped = StripCommonWords(OlMail.Subject) 'Eliminate common words from the subject
        For i = 1 To SubjectMapCt   'Loop through every subject of every email ever received
            With SubjectMap(i)
                SWVal = Smith_Watterman.SW_Calc(SubjectStripped, .Email_Subject, Matrix, AppGlobals.AF, SW_Options.ByWords)
                Val = (SWVal ^ AppGlobals.AF.LngConvCtPwr) * .Email_Subject_Count
                If .Email_Folder <> SubjectMap(i - 1).Email_Folder Then
                    varFldrSubs = Split(.Email_Folder, "\")
                    If IsArray(varFldrSubs) Then
                        strTmpFldr = varFldrSubs(UBound(varFldrSubs))
                    Else
                        strTmpFldr = varFldrSubs
                    End If

                    Val1 = Smith_Watterman.SW_Calc(SubjectStripped, strTmpFldr, Matrix, AppGlobals.AF, SW_Options.ByWords)
                    Val = Val1 * Val1 + Val
                End If

                If Val > 5 Then
                    Add(.Email_Folder, Val)
                End If
            End With
        Next i
    End Sub

    Private Sub AddAnythingInAutoFileField(OlMail As MailItem, _globals As IApplicationGlobals)
        'TODO: Determine if this property still exists
        Dim objProperty As UserProperty = OlMail.UserProperties.Find("AutoFile")
        If objProperty IsNot Nothing Then
            Add(objProperty.Value, (4 ^ _globals.AF.LngConvCtPwr) * CLng(_globals.AF.Conversation_Weight))
            Throw New NotImplementedException("Please investigate what this is and why it fired")
        End If
    End Sub

    Private Sub AddConversationBasedSuggestions(OlMail As MailItem, _globals As IApplicationGlobals)
        'Is the conversationID already mapped to an email Folder. If so, grab the index of it
        Dim Inc_Num As Integer = _globals.AF.CTFList.CTF_Incidence_FIND(OlMail.ConversationID)
        If Inc_Num > 0 Then
            With _globals.AF.CTFList.CTF_Inc(Inc_Num)
                'For each Folder that already contains at least one email with the conversationID ...
                For i = 1 To .Folder_Count
                    'Calculate the weight of the suggestion based on how much of the conversation is already in the folder
                    Dim Val As Long = CLng(.Email_Conversation_Count(i))
                    Val = (Val ^ _globals.AF.LngConvCtPwr) * CLng(_globals.AF.Conversation_Weight)
                    Add(.Email_Folder(i), Val)
                Next i
            End With
        End If
    End Sub

    Private Shared Sub ReloadStagingFiles(_globals As IApplicationGlobals)
        'Throw New NotImplementedException("CTF_Incidence_Text_File_READ, Subject_MAP_Text_File_READ, " _
        '                                              & "and Common_Words_Text_File_READ are not implemented. Cannot reload")
        'CTF_Incidence_Text_File_READ(_globals.FS)
        Subject_MAP_Text_File_READ(_globals.FS)
        Common_Words_Text_File_READ(_globals.FS)

        Dim strFList() As String = OlFolderlist_GetAll(_globals.Ol)
    End Sub
End Class
