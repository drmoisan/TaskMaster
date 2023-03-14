Imports Microsoft.Office.Interop.Outlook
Public Module FolderSuggestionsModule
    Public Function Folder_Suggestions(MSG As MailItem,
    Optional Reload As Boolean = True,
    Optional ByVal InBackground As Boolean = False) As cSuggestions


        Dim Inc_Num As Integer
        Dim Matrix() As Object
        Dim SubjectStripped As String
        Dim TT As String

        Dim Result As cSuggestions
        Result = New cSuggestions
        Dim i As Integer
        Dim SWVal, Val, Val1 As Long
        Dim ConvID As String
        Dim Tim1, Tim2, Tim3, Tim4, Tim5, TimT, Tim3P, Tim4P, Tim5P As Double

        Dim strTmp As String

        Dim strWrite As String
        Dim StopWatch_Tot As cStopWatch
        Dim strTmpFldr As String
        Dim varFldrSubs As Object
        Dim objProperty As UserProperty

        ConvID = MSG.ConversationID

        If Reload Then
            Call CTF_Incidence_Text_File_READ
            'If InBackground Then DoEvents
            Call Subject_MAP_Text_File_READ
            'If InBackground Then DoEvents
            Call Common_Words_Text_File_READ
            'If InBackground Then DoEvents
            strFList = Folderlist_GetAll
        End If

        If Bottleneck Then Tim3 = MicroTimer.MicroTimer

        'Is the conversationID already mapped to an email Folder. If so, grab the index of it
        Inc_Num = CTF_Incidence_FIND(ConvID)
        With CTF_Inc(Inc_Num)

            'For each Folder that already contains at least one email with the conversationID ...
            For i = 1 To .Folder_Count

                'Calculate the weight of the suggestion based on how much of the conversation is already in the folder
                Val = CLng(.Email_Conversation_Count(i))
                Val = (Val ^ lngConvCtPwr) * CLng(Conversation_Weight)

                If DebugLVL And vbVariable Then
                    strWrite = "convID, " &
                    ", " &
                    ", " &
                    ", " &
                    ", " &
                    .Email_Folder(i) & ", " &
                    .Email_Conversation_Count(i) & ", " &
                    Val
                    stackDebug.Push strWrite
            End If

                'Add the folder to the suggestions list with the appropriate weight
                'Call Suggestions_ADD(Result, .Email_Folder(i), Val)
                Result.Add.Email_Folder(i), Val
            'If InBackground Then DoEvents
        Next i
            'These lines written for refiling old emails that were deleted by retention policy and then recovered
            '        If .Folder_Count = 0 Then
            '            Suggestions_ADD Result, "Trash to Delete", 10000
            '            strWrite = "convID, " & _
            '                    ", " & _
            '                    ", " & _
            '                    ", " & _
            '                    ", " & _
            '                    "Trash to Delete" & ", " & _
            '                    47 & ", " & _
            '                    10000
            '            stackDebug.Push strWrite
            '        End If
        End With
    
    Set objProperty = MSG.UserProperties.find("AutoFile")
    If Not objProperty Is Nothing Then Result.Add objProperty.Value, (4 ^ lngConvCtPwr) * CLng(Conversation_Weight)


    'For i = 1 To Result.Count
        '    Debug.Print "Result " & i & " " & Result.FolderList(i) & "   " & Result.Valor(i)
        'Next i

        If Bottleneck Then Tim4 = MicroTimer.MicroTimer

        SubjectStripped = StripCommonWords(MSG.Subject) 'Eliminate common words from the subject


        For i = 1 To Subject_Map_Ct   'Loop through every subject of every email ever received
            'If InBackground Then DoEvents
            With Subject_Map(i)
                'cloc = 0
                'sloc = InStr(1, .Email_Subject, " 155056", 1)
                'If sloc <> cloc Then Debug.Print Format(i, "0,000") & "  " & .Email_Subject

                'Use the Smith_Watterman DNA Sequencing Algorithm to find similarities
                '            StopWatch_Main.Pause

                SWVal = Smith_Watterman.SW_Calc(SubjectStripped, .Email_Subject, Matrix, ByWords)

                '            StopWatch_Main.reStart
                'If SWVal > 1 Then Debug.Print "SWVal " & SWVal & "   SubjectStripped: " & SubjectStripped & _
                '    "   .Email_Subject: " & .Email_Subject & "  .EmailFolder " & .Email_Folder

                Val = (SWVal ^ lngSubjectCtPwr) * .Email_Subject_Count
                If .Email_Folder <> Subject_Map(i - 1).Email_Folder Then
                    '                StopWatch_Main.Pause

                    varFldrSubs = Split(.Email_Folder, "\")
                    If IsArray(varFldrSubs) Then strTmpFldr = varFldrSubs(UBound(varFldrSubs))
                    'strTmpFldr = UCase(Replace(.Email_Folder, "\", " "))   'logic wrong here. should eliminate all before the last backslash
                    Val1 = Smith_Watterman.SW_Calc(SubjectStripped, strTmpFldr, Matrix, ByWords)

                    '                StopWatch_Main.reStart

                    Val = Val1 * Val1 + Val
                End If
                'SWVal = Smith_Watterman.SW_Calc(SubjectStripped, .Email_Subject, Matrix)
                'SWVal = SWVal * .Email_Subject_Count
                'If Val > 0 Then Debug.Print (Val & ", Message Subject: " & msg.Subject & ", Subject2: " & Subject_Map(i).Email_Subject & " Folder: " & Subject_Map(i).Email_Folder)
                If DebugLVL And vbVariable Then
                    strWrite = "SubjectMap, " &
                    xComma(SubjectStripped) & ", " &
                    xComma(.Email_Subject) & ", " &
                    SWVal & ", " &
                    Val1 & ", " &
                    .Email_Folder & ", " &
                    .Email_Subject_Count & ", " &
                    Val

                    stackDebug.Push strWrite
            End If

                If Val > 5 Then

                    '                StopWatch_Main.Pause

                    'Call Suggestions_ADD(Result, .Email_Folder, Val)
                    Result.Add.Email_Folder, Val

'                StopWatch_Main.reStart

                End If
            End With
        Next i

        'For i = 1 To UBound(strFlist)
        '    strTmp = strFlist(i)
        '    strTmp = Replace(strTmp, "\", " ")
        '    strTmp = Replace(strTmp, ".", " ")
        '    strTmp = Replace(strTmp, "_", " ")
        '    Val = Smith_Watterman.SW_Calc(SubjectStripped, strTmp, Matrix)
        '    If Val > 10 Then Call Suggestions_ADD(Result, strFlist(i), Val)
        'Next i

        If Bottleneck Then Tim5 = MicroTimer.MicroTimer

        If DebugLVL And vbVariable Then stackDebug.Write_TextFile "SWDump.csv", FileSystem_FLOW
    If InBackground Then DoEvents
    
    'result.PrintDebug
    Set Folder_Suggestions = Result

    End Function

End Module
