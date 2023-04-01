Imports Microsoft.Office.Interop.Outlook
Imports ToDoModel
Imports UtilitiesVB


Public Module ModuleMailItemsSort
    Public Function MailItemsSort(OlItems As Items, options As SortOptionsEnum) As IList
        Dim strFilter As String
        Dim strFilter2 As String
        'Dim OlRow                   As Outlook.Row
        'Dim OlTable                 As Outlook.Table
        Dim OlFolder As Folder
        'Dim OlItems                 As Outlook.Items
        Dim OlItemsTmp As Items
        Dim OlItemsRemainder As Items
        Dim objItem As Object
        Dim OlMailTmp As MailItem
        Dim OlMailTmp2 As MailItem
        'Dim OlNameSpace             As Outlook.NameSpace
        Dim listEmails As IList
        Dim StrTriageOpts(3) As String
        Dim i As Integer
        Dim j As Integer
        Dim BlUniqueConv As Boolean
        Dim intFrom As Integer
        Dim intTo As Integer
        Dim intStep As Integer
        Dim blTriage As Boolean


        'Originally written to maintain filter of the view
        'Need to add in the option to eliminate the filter
        'for two cases: 1) If it is not called from the active view,
        'and 2) in the case that we want to see all emails anyway

        StrTriageOpts(1) = "A"
        StrTriageOpts(2) = "B"
        StrTriageOpts(3) = "C"

        listEmails = New List(Of MailItem)
        'OlFolder = _activeExplorer.CurrentFolder
        'objCurView = _activeExplorer.CurrentView
        'strFilter = objCurView.Filter
        'If strFilter <> "" Then
        '    strFilter = "@SQL=" & strFilter
        '    OlItems = OlFolder.Items.Restrict(strFilter)
        'Else
        '    OlItems = OlFolder.Items
        'End If

        If options And SortOptionsEnum.DateRecentFirst Then
            OlItems.Sort("Received", True)
        ElseIf options And SortOptionsEnum.DateOldestFirst Then
            OlItems.Sort("Received", False)
        End If

        'Output_Items OlItems

        If options And SortOptionsEnum.TriageImportantFirst Then
            blTriage = True
            intFrom = 1
            intTo = 3
            intStep = 1
        ElseIf options And SortOptionsEnum.TriageImportantLast Then
            blTriage = True
            intFrom = 3
            intTo = 1
            intStep = -1
        End If

        If blTriage Then
            OlItemsRemainder = OlItems
            For i = intFrom To intTo Step intStep
                strFilter = "[Triage] = " & Chr(34) & StrTriageOpts(i) & Chr(34)
                strFilter2 = "[Triage] <> " & Chr(34) & StrTriageOpts(i) & Chr(34)
                OlItemsTmp = OlItems.Restrict(strFilter)
                OlItemsRemainder = OlItemsRemainder.Restrict(strFilter2)


                For Each objItem In OlItemsTmp
                    BlUniqueConv = True
                    If TypeOf objItem Is MailItem Then
                        OlMailTmp = objItem
                        If Not IsMailUnReadable(OlMailTmp) Then
                            If options And SortOptionsEnum.ConversationUniqueOnly Then
                                For j = 0 To listEmails.Count - 1
                                    OlMailTmp2 = listEmails(j)
                                    If OlMailTmp.ConversationID = OlMailTmp2.ConversationID Then
                                        BlUniqueConv = False
                                    End If
                                Next j
                            End If 'Options And ConversationUniqueOnly Then

                            If BlUniqueConv Then listEmails.Add(OlMailTmp)

                        End If 'If IsMailUnReadable
                    End If 'If TypeOf ObjItem Is mailItem Then
                Next objItem 'For Each ObjItem In OlItemsTmp
            Next i 'For i = 1 To 4

            For Each objItem In OlItemsRemainder
                BlUniqueConv = True
                If TypeOf objItem Is MailItem Then
                    OlMailTmp = objItem
                    If Not IsMailUnReadable(OlMailTmp) Then
                        If options And SortOptionsEnum.ConversationUniqueOnly Then
                            For j = 0 To listEmails.Count - 1
                                OlMailTmp2 = listEmails(j)
                                If OlMailTmp.ConversationID = OlMailTmp2.ConversationID Then
                                    BlUniqueConv = False
                                End If
                            Next j
                        End If 'Options And ConversationUniqueOnly Then

                        If BlUniqueConv Then listEmails.Add(OlMailTmp)

                    End If 'If IsMailUnReadable
                End If 'If TypeOf ObjItem Is mailItem Then
            Next objItem 'For Each ObjItem In OlItemsRemainder

        Else
            For Each objItem In OlItems
                BlUniqueConv = True
                If TypeOf objItem Is MailItem Then
                    OlMailTmp = objItem
                    If Not IsMailUnReadable(OlMailTmp) Then
                        If options And SortOptionsEnum.ConversationUniqueOnly Then
                            For j = 1 To listEmails.Count
                                OlMailTmp2 = listEmails(j)
                                If OlMailTmp.ConversationID = OlMailTmp2.ConversationID Then
                                    BlUniqueConv = False
                                End If
                            Next j
                        End If 'Options And ConversationUniqueOnly Then

                        If BlUniqueConv Then listEmails.Add(OlMailTmp)
                    End If 'Not IsMailUnReadable(OlMailTmp) Then
                End If 'If TypeOf ObjItem Is mailItem Then
            Next objItem
        End If

        Return listEmails


        OlFolder = Nothing
        OlItems = Nothing
        OlItemsTmp = Nothing
        objItem = Nothing
        OlMailTmp = Nothing
        OlMailTmp2 = Nothing


    End Function

    Public Enum SortOptionsEnum
        TriageIgnore = 1
        TriageImportantFirst = 2
        TriageImportantLast = 4
        DateRecentFirst = 8
        DateOldestFirst = 16
        ConversationUniqueOnly = 32
    End Enum


End Module
