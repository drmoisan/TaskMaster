Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB


Public Module FolderSuggestionsModule
    Public Function Folder_Suggestions(MSG As MailItem,
                                       AppGlobals As IApplicationGlobals,
                                       Optional Reload As Boolean = True,
                                       Optional ByVal InBackground As Boolean = False) As cSuggestions


        Dim Inc_Num As Integer
        Dim Matrix(,) As Object
        Dim SubjectStripped As String


        Dim Result As cSuggestions
        Result = New cSuggestions
        Dim i As Integer
        Dim SWVal, Val, Val1 As Long
        Dim ConvID As String

        Dim strTmpFldr As String
        Dim varFldrSubs As Object
        Dim objProperty As UserProperty
        Dim _globals As IApplicationGlobals = AppGlobals
        ConvID = MSG.ConversationID

        If Reload Then
            Throw New NotImplementedException("CTF_Incidence_Text_File_READ, Subject_MAP_Text_File_READ, " _
                                              & "and Common_Words_Text_File_READ are not implemented. Cannot reload")
            CTF_Incidence_Text_File_READ(_globals.FS)
            Subject_MAP_Text_File_READ(_globals.FS)
            Common_Words_Text_File_READ(_globals.FS)

            Dim strFList() As String = OlFolderlist_GetAll(_globals.Ol)
        End If

        'Is the conversationID already mapped to an email Folder. If so, grab the index of it
        Inc_Num = CTF_Incidence_FIND(ConvID)
        With CTF_Inc(Inc_Num)

            'For each Folder that already contains at least one email with the conversationID ...
            For i = 1 To .Folder_Count

                'Calculate the weight of the suggestion based on how much of the conversation is already in the folder
                Val = CLng(.Email_Conversation_Count(i))
                Val = (Val ^ _globals.AF.LngConvCtPwr) * CLng(_globals.AF.Conversation_Weight)


                'Add the folder to the suggestions list with the appropriate weight
                'Call Suggestions_ADD(Result, .Email_Folder(i), Val)
                Result.Add(.Email_Folder(i), Val)
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

        objProperty = MSG.UserProperties.Find("AutoFile")
        If Not objProperty Is Nothing Then Result.Add(objProperty.Value, (4 ^ _globals.AF.LngConvCtPwr) * CLng(_globals.AF.Conversation_Weight))


        'For i = 1 To Result.Count
        '    Debug.Print "Result " & i & " " & Result.FolderList(i) & "   " & Result.Valor(i)
        'Next i

        SubjectStripped = StripCommonWords(MSG.Subject) 'Eliminate common words from the subject


        For i = 1 To Subject_Map_Ct   'Loop through every subject of every email ever received
            'If InBackground Then DoEvents
            With Subject_Map(i)


                SWVal = Smith_Watterman.SW_Calc(SubjectStripped, .Email_Subject, Matrix, AppGlobals.AF, SW_Options.ByWords)

                '            StopWatch_Main.reStart
                'If SWVal > 1 Then Debug.Print "SWVal " & SWVal & "   SubjectStripped: " & SubjectStripped & _
                '    "   .Email_Subject: " & .Email_Subject & "  .EmailFolder " & .Email_Folder

                Val = (SWVal ^ AppGlobals.AF.LngConvCtPwr) * .Email_Subject_Count
                If .Email_Folder <> Subject_Map(i - 1).Email_Folder Then
                    '                StopWatch_Main.Pause

                    varFldrSubs = Split(.Email_Folder, "\")
                    If IsArray(varFldrSubs) Then strTmpFldr = varFldrSubs(UBound(varFldrSubs))
                    'strTmpFldr = UCase(Replace(.Email_Folder, "\", " "))   'logic wrong here. should eliminate all before the last backslash
                    Val1 = Smith_Watterman.SW_Calc(SubjectStripped, strTmpFldr, Matrix, AppGlobals.AF, SW_Options.ByWords)

                    '                StopWatch_Main.reStart

                    Val = Val1 * Val1 + Val
                End If
                'SWVal = Smith_Watterman.SW_Calc(SubjectStripped, .Email_Subject, Matrix)
                'SWVal = SWVal * .Email_Subject_Count
                'If Val > 0 Then Debug.Print (Val & ", Message Subject: " & msg.Subject & ", Subject2: " & Subject_Map(i).Email_Subject & " Folder: " & Subject_Map(i).Email_Folder)



                If Val > 5 Then

                    '                StopWatch_Main.Pause

                    'Call Suggestions_ADD(Result, .Email_Folder, Val)
                    Result.Add(.Email_Folder, Val)

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


        If InBackground Then AppGlobals.Ol.App.DoEvents()

        'result.PrintDebug
        Return Result

    End Function


    Public Sub Folder_Suggestions_Reload()
        Throw New NotImplementedException("Folder_Suggestions_Reload not implemented yet")

        'Dim blOld As Boolean
        'blOld = False
        'CTF_Incidence_Text_File_READ(_globals.FS)
        'Common_Words_Text_File_READ(_globals.FS)
        'Subject_MAP_Text_File_READ(_globals.FS)
        'strFList = Folderlist_GetAll
        'bl_SuggestionFiles_IsLoaded = True
        'Conversation_Weight = 10000
        'Subject_Weight = 1
        'If blOld Then
        '    lngConvCtPwr = 3
        '    lngSubjectCtPwr = 1
        'Else
        '    lngConvCtPwr = 2
        '    lngSubjectCtPwr = 3
        'End If


    End Sub

End Module
