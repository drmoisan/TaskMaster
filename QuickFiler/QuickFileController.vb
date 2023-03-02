Imports Microsoft.Office.Interop.Outlook
Imports System.Windows.Forms
Imports System.Linq
Imports System.Collections.Generic
Imports UtilitiesVB
Imports ToDoModel

Public Class QuickFileController

    Private OlApp_hWnd As Long
    Private lFormHandle As Long
    Private lStyle As Long

    Public WithEvents focusListener As FormFocusListener
    Private StopWatch As cStopWatch
    Private BoolRemoteMouseApp As Boolean
    Const Modal = 0, Modeless = 1

    Private ht, wt As Long
    Private Const GWL_STYLE As Long = (-16) 'Sets a new window style  As LongPtr in 64bit version
    Private Const WS_SYSMENU As Long = &H80000 'Windows style   As LongPtr in 64bit version
    Private Const WS_THICKFRAME = &H40000 'Style that is apparently resizble
    Private Const WS_MINIMIZEBOX As Long = &H20000   'As LongPtr in 64bit version
    Private Const WS_MAXIMIZEBOX As Long = &H10000   'As LongPtr in 64bit version
    Private Const SW_SHOWMAXIMIZED = 3
    Private Const SW_FORCEMINIMIZE = 11
    Private Const WM_SETFOCUS = &H7
    Private Const GWL_HWNDPARENT = -8
    Private Const olAppCLSN As String = "rctrl_renwnd32"
    Private Const GA_PARENT = 1
    Private Const GA_ROOTOWNER = 3

    Private strTagOptions As Object
    Private strFilteredOptions As Object
    Private intFilteredMax As Integer
    Private intMaxOptions As Integer
    Private boolTagChoice() As Boolean
    Private boolFilteredChoice() As Boolean
    Public colCheckbox As Collection
    Public colCheckboxEvent As Collection
    Public blFrmKll As Boolean
    Private folderCurrent As [Folder]
    Private intUniqueItemCounter As Integer
    Private intEmailStart As Integer
    Private intEmailPosition As Integer
    Private intEmailsPerIteration As Integer
    Private lngAcceleratorDialogueTop As Long
    Private lngAcceleratorDialogueLeft As Long
    Private intAccActiveMail As Integer
    'Public blShowInConversations        As Boolean
    Public objView As Microsoft.Office.Interop.Outlook.[View]
    Private objView_Mem As String
    Public objViewTemp As Microsoft.Office.Interop.Outlook.[View]
    Public InitType As InitTypeEnum


    Public colQFClass As Collection
    Public colFrames As Collection
    Public colMailJustMoved As Collection
    Public strOptions As String
    Public intFocus As Integer

    Private blSuppressEvents As Boolean
    Public blConvView As Boolean

    'Left and Width Constants
    Const Top_Offset As Long = 6
    Const Top_Offset_C As Long = 0

    Const Left_frm As Long = 12
    Const Left_lbl1 As Long = 6
    Const Left_lbl2 As Long = 6
    Const Left_lbl3 As Long = 6
    Const Left_lbl4 As Long = 6
    Const Left_lbl5 As Long = 372           'Folder:
    Const Left_lblSender As Long = 66            '<SENDER>
    Const Left_lblSender_C As Long = 6             '<SENDER> Compact view
    Const Right_Aligned As Long = 648

    Const Left_lblTriage As Long = 181           'X Triage placeholder
    Const Left_lblActionable As Long = 198           '<ACTIONABL>

    Const Left_lblSubject As Long = 66            '<SUBJECT>
    Const Left_lblSubject_C As Long = 6             '<SUBJECT> Compact view

    Const Left_lblBody As Long = 66            '<BODY>
    Const Left_lblBody_C As Long = 6             '<BODY> Compact view

    Const Left_lblSentOn As Long = 66            '<SENTON>
    Const Left_lblSentOn_C As Long = 200           '<SENTON> Compact view

    Const Left_lblConvCt As Long = 290           'Count of Conversation Members
    Const Left_lblConvCt_C As Long = 320           'Count of Conversation Members Compact view

    Const Left_lblPos As Long = 6             'ACCELERATOR Email Position
    Const Left_cbxFolder As Long = 372           'Combo box containing Folder Suggestions
    Const Left_inpt As Long = 408           'Input for folder search

    Const Left_chbxGPConv As Long = 210           'Checkbox to Group Conversations
    Const Left_chbxGPConv_C As Long = 372           'Checkbox to Group Conversations

    Const Left_cbDelItem As Long = 588           'Delete email
    Const Left_cbKllItem As Long = 618           'Remove mail from Processing
    Const Left_cbFlagItem As Long = 569           'Flag as Task
    Const Left_lblAcF As Long = 363           'ACCELERATOR F for Folder Search
    Const Left_lblAcD As Long = 363           'ACCELERATOR D for Folder Dropdown

    Const Left_lblAcC As Long = 384           'ACCELERATOR C for Grouping Conversations
    Const Left_lblAcC_C As Long = 548           'ACCELERATOR C for Grouping Conversations

    Const Left_lblAcX As Long = 594           'ACCELERATOR X for Delete email
    Const Left_lblAcR As Long = 624           'ACCELERATOR R for remove item from list
    Const Left_lblAcT As Long = 330           'ACCELERATOR T for Task ... Flag item and make it a task

    Const Left_lblAcO As Long = 50            'ACCELERATOR O for Open Email
    Const Left_lblAcO_C As Long = 0            'ACCELERATOR O for Open Email

    Const Width_frm As Long = 655
    Const Width_lbl1 As Long = 54
    Const Width_lbl2 As Long = 54
    Const Width_lbl3 As Long = 54
    Const Width_lbl4 As Long = 52
    Const Width_lbl5 As Long = 78            'Folder:
    Const Width_lblSender As Long = 138           '<SENDER>
    Const Width_lblSender_C As Long = 174           '<SENDER> Compact view
    Const Width_lblTriage As Long = 11            'X Triage placeholder
    Const Width_lblActionable As Long = 72            '<ACTIONABL>

    Const Width_lblSubject As Long = 294           '<SUBJECT>
    Const Width_lblSubject_C As Long = 354           '<SUBJECT> Compact view

    Const Width_lblBody As Long = 294           '<BODY>
    Const Width_lblBody_C As Long = 354           '<BODY> Compact view

    Const Width_lblSentOn As Long = 80            '<SENTON>
    Const Width_lblConvCt As Long = 30            'Count of Conversation Members
    Const Width_lblPos As Long = 20            'ACCELERATOR Email Position
    Const Width_cbxFolder As Long = 276           'Combo box containing Folder Suggestions
    Const Width_inpt As Long = 156           'Input for folder search
    Const Width_chbxGPConv As Long = 96            'Checkbox to Group Conversations
    Const Width_cb As Long = 25            'Command buttons for: Delete email, Remove mail from Processing, and Flag as Task
    Const Width_lblAc As Long = 14            'ACCELERATOR Width
    Const Width_lblAcF As Long = 14            'ACCELERATOR F for Folder Search
    Const Width_lblAcD As Long = 14            'ACCELERATOR D for Folder Dropdown
    Const Width_lblAcC As Long = 14            'ACCELERATOR C for Grouping Conversations
    Const Width_lblAcX As Long = 14            'ACCELERATOR X for Delete email
    Const Width_lblAcR As Long = 14            'ACCELERATOR R for remove item from list
    Const Width_lblAcT As Long = 14            'ACCELERATOR T for Task ... Flag item and make it a task
    Const Width_lblAcO As Long = 14            'ACCELERATOR O for Open Email

    Const Height_UserForm As Long = 149          'Minimum height of Userform
    Const Width_UserForm As Long = 699.75        'Minimum width of Userform
    Const Width_FrameMain As Long = 683           'Minimum width of FrameMain

    Private Height_UserForm_Max As Long
    Private Height_UserForm_Min As Long
    Private Height_FrameMain_Max As Long
    Private Height_FrameMain_Min As Long
    Private lngFrameMain_SC_Top As Long
    Private lngFrameMain_SC_Bottom As Long

    Private lngTop_OK_BUTTON_Min As Long
    Private lngTop_CANCEL_BUTTON_Min As Long
    Private lngTop_UNDO_BUTTON_Min As Long
    Private Const OK_left As Long = 216
    Private Const CANCEL_left As Long = 354
    Private Const OK_width As Long = 120
    Private Const UNDO_left As Long = 480
    Private Const UNDO_width As Long = 42
    Private lngTop_CommandButton1_Min As Long
    Private lngTop_AcceleratorDialogue_Min As Long
    Private lngTop_spn_Min As Long
    Private Const spn_left As Long = 606
    Private lngTop_lbl_EmailPerLoad_Min As Long
    Private lng_lbl_EmailPerLoad_left As Long

    'Frame Design Constants
    Const frmHt = 72
    Const frmWd = 655
    Const frmLt = 12
    Const frmSp = 6
    Public colEmailsInFolder As Collection
    Private ActiveExlorer As [Explorer]
    Private OlApp As Microsoft.Office.Interop.Outlook.Application

    Public Sub New(OlApp As Microsoft.Office.Interop.Outlook.Application)
        Me.OlApp = OlApp
        ActiveExlorer = OlApp.ActiveExplorer()
    End Sub

    Public Sub LoadEmailDataBase(Optional colEmailsToLoad As Collection = Nothing)
        Dim OlFolder As [Folder]
        Dim objCurView As Microsoft.Office.Interop.Outlook.View
        Dim strFilter As String
        Dim OlItems As [Items]


        If colEmailsToLoad Is Nothing Then
            colEmailsToLoad = New Collection
            OlFolder = ActiveExlorer.CurrentFolder
            objCurView = ActiveExlorer.CurrentView
            strFilter = objCurView.Filter
            If strFilter <> "" Then
                strFilter = "@SQL=" & strFilter
                OlItems = OlFolder.Items.Restrict(strFilter)
            Else
                OlItems = OlFolder.Items
            End If
            colEmailsInFolder = MailItemsSort(OlItems,
                                              SortOptionsEnum.DateRecentFirst +
                                              SortOptionsEnum.TriageImportantFirst +
                                              SortOptionsEnum.ConversationUniqueOnly)

        Else
            colEmailsInFolder = colEmailsToLoad
        End If

    End Sub

    Private Sub DebugOutPutEmailCollection(colTemp As Collection)
        Dim objItem As Object
        Dim OlMail As [MailItem]
        Dim OlAppt As [MeetingItem]
        Dim strLine As String
        Dim i As Integer

        i = 0
        For Each objItem In colTemp
            i = i + 1
            strLine = ""
            If TypeOf objItem Is [MailItem] Then
                OlMail = objItem
                With OlMail
                    strLine = i & " " & CustomFieldID_GetValue(objItem, "Triage") & " " & Format(.SentOn, "General Date") & " " & .Subject
                End With
            ElseIf TypeOf objItem Is [AppointmentItem] Then
                OlAppt = objItem
                With OlAppt
                    strLine = i & " " & CustomFieldID_GetValue(objItem, "Triage") & " " & Format(.SentOn, "General Date") & " " & .Subject
                End With
            End If
            Debug.WriteLine(strLine)
        Next objItem
    End Sub

    Private Function ItemsToCollection(OlItems As Items) As Collection
        Dim colTemp As Collection
        colTemp = New Collection

        Dim objItem As Object
        For Each objItem In OlItems
            colTemp.Add(objItem)
        Next objItem
        ItemsToCollection = colTemp

    End Function

    Private Sub EliminateDuplicateConversationIDs(ByRef colTemp As Collection)
        Dim dictID As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
        Dim i As Long
        Dim max As Long

        Dim objItem As Object
        For Each objItem In colTemp
            If dictID.ContainsKey(objItem.ConversationID) Then
                dictID(objItem.ConversationID) = dictID(objItem.ConversationID) + 1
            Else
                dictID.Add(objItem.ConversationID, 0)
            End If
        Next objItem

        max = colTemp.Count

        For i = max To 1 Step -1
            objItem = colTemp(i)
            'Debug.Print dictID(objItem.ConversationID)
            If dictID(objItem.ConversationID) > 0 Then
                colTemp.Remove(objItem)
                dictID(objItem.ConversationID) = dictID(objItem.ConversationID) - 1
            End If
        Next i
    End Sub

    Public Sub Iterate()

        Dim MailCurrent As [MailItem]
        Dim dblItemCount As Double
        Dim i As Double
        Dim j As Double
        Dim max As Double
        Dim intItemsPerPage As Integer
        Dim strCurConvs() As String

        Dim colEmails As Collection
        Dim cQFC As QfcController
        Dim items As [Items]
        Dim bExit As Boolean
        Dim bCollectionFull As Boolean
        Dim strConvId_last As String
        Dim QF As QuickFileViewer



        colEmails = New Collection
        If intEmailsPerIteration < colEmailsInFolder.Count Then
            max = intEmailsPerIteration
        Else
            max = colEmailsInFolder.Count
        End If

        For i = 1 To max
            colEmails.Add(colEmailsInFolder(i))
        Next i
        For i = max To 1 Step -1
            colEmailsInFolder.Remove(colEmailsInFolder(i))
        Next i
        StopWatch = New cStopWatch
        StopWatch.Start()
        Init(colEmails)

    End Sub

    Public Sub Init(colEmails As Collection)
        Dim objItem As Object
        Dim Mail As [MailItem]
        Dim QF As QfcController
        Dim colCtrls As Collection
        Dim blDebug As Boolean

        blDebug = False

        blFrmKll = False

        colQFClass = New Collection
        colFrames = New Collection

        intUniqueItemCounter = 0

        For Each objItem In colEmails
            If TypeOf objItem Is MailItem Then
                intUniqueItemCounter = intUniqueItemCounter + 1
                Mail = objItem
                QF = New QfcController
                colCtrls = New Collection
                LoadGroupOfCtrls(colCtrls, intUniqueItemCounter)
                QF.InitCtrls(Mail, colCtrls, intUniqueItemCounter, BoolRemoteMouseApp, Caller:=Me, hwnd:=lFormHandle, InitTypeE:=InitType)

                colQFClass.Add QF
            DoEvents
            End If
        Next objItem

        If DebugLVL And vbCommand Then TraceStack.Push SubNm & "Completed initializing all emails in form. About to run conversation enumeration"
    ShowWindow lFormHandle, SW_SHOWMAXIMIZED



    If InitType And InitSort Then
            'ToggleOffline
            For Each QF In colQFClass
                QF.Init_FolderSuggestions
                QF.CountMailsInConv
                'DoEvents
            Next QF
            'ToggleOffline
        End If


        If DebugLVL And vbCommand Then TraceStack.Push SubNm & "Completed conversation enumeration. About to force focus on userform"

    intAccActiveMail = 0

        If blSuppressEvents Then
            blSuppressEvents = False
            UserForm_Resize()
            blSuppressEvents = True
        Else
            DoEvents
            UserForm_Resize()
        End If


        'Modal    SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
        EnableWindow OlApp_hWnd, Modal
    'EnableWindow lFormHandle, Modeless
        FrameMain.SetFocus


        '************Standard Error Handling Footer**************
        On Error Resume Next
        Temp = SF_Stack.Pop
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & "Finished subroutine"
    Exit Sub

ErrorHandler:
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source

    ErrHandler_Execute SubNm, ttrace, errcapt, errRaised, DebugLVL

    'errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        Stop
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            Stop
            Resume
        End If

    End Sub


    Private Sub LoadGroupOfCtrls(ByRef colCtrls As Collection,
    intItemNumber As Integer,
    Optional intPosition As Integer = 0,
    Optional blGroupConversation As Boolean = True,
    Optional blWideView As Boolean = False)

        'Procedure Naming
        Dim SubNm As String
        SubNm = "LoadGroupOfCtrls"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim frm As MSForms.frame
        Dim lbl1 As MSForms.Label
        Dim lbl2 As MSForms.Label
        Dim lbl3 As MSForms.Label
        Dim lbl4 As MSForms.Label
        Dim lbl5 As MSForms.Label
        Dim lblSender As MSForms.Label
        Dim lblSubject As MSForms.Label
        Dim lblBody As MSForms.Label
        Dim lblSentOn As MSForms.Label
        Dim lblConvCt As MSForms.Label
        Dim lblPos As MSForms.Label
        Dim cbxFolder As MSForms.combobox
        Dim inpt As MSForms.textbox
        Dim chbxGPConv As MSForms.checkbox
        Dim chbxSaveAttach As MSForms.checkbox
        Dim chbxSaveMail As MSForms.checkbox
        Dim chbxDelFlow As MSForms.checkbox
        Dim cbDelItem As MSForms.commandbutton
        Dim cbKllItem As MSForms.commandbutton
        Dim cbFlagItem As MSForms.commandbutton
        Dim lblAcF As MSForms.Label
        Dim lblAcD As MSForms.Label
        Dim lblAcC As MSForms.Label
        Dim lblAcX As MSForms.Label
        Dim lblAcR As MSForms.Label
        Dim lblAcT As MSForms.Label
        Dim lblAcO As MSForms.Label
        Dim lblAcA As MSForms.Label
        Dim lblAcW As MSForms.Label
        Dim lblAcM As MSForms.Label


        Dim lngTopOff As Long

        Dim blDebug As Boolean


        blDebug = False

        If blWideView Then
            lngTopOff = Top_Offset
        Else
            lngTopOff = Top_Offset_C
        End If
        'Button_OK.top = Button_OK.top + frmHt + frmSp
        'BUTTON_CANCEL.top = BUTTON_CANCEL.top + frmHt + frmSp

        If intPosition = 0 Then intPosition = intItemNumber

        If (intItemNumber * (frmHt + frmSp) + frmSp) > FrameMain.Height Then      'Was Height_FrameMain_Max but I replaced with Me.Height
            FrameMain.ScrollHeight = intItemNumber * (frmHt + frmSp) + frmSp 'FrameMain.ScrollHeight + frmHt + frmSp
        End If

        'Min Me Size is frmSp * 2 + frmHt
        frm = FrameMain.controls.Add("Forms.Frame.1", "frm0" & intItemNumber, True)
        With frm
            .Height = frmHt
            .Top = (frmSp + frmHt) * (intPosition - 1) + frmSp
            .Left = frmLt
            .Width = frmWd
            .TabStop = False

        End With
        colCtrls.Add frm, "frm"

    'If intBefore And intAfter Then
        '    colFrames.Add frm, "frm0" & intItemNumber, intBefore, intAfter
        'ElseIf intBefore Then
        '    colFrames.Add frm, "frm0" & intItemNumber, intBefore
        'ElseIf intAfter Then
        '    colFrames.Add frm, "frm0" & intItemNumber, , intAfter
        'Else
        '    colFrames.Add frm, "frm0" & intItemNumber
        'End If


        If blWideView Then
        Set lbl1 = frm.controls.Add("Forms.Label.1", "lbl1" & intItemNumber, True)
        With lbl1
                .Height = 12
                .Top = lngTopOff
                .Left = 6
                .Width = 54
                .Caption = "From:"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10

            End With
            colCtrls.Add lbl1, "lbl1"
    End If  'blWideView

        If blWideView Then
        Set lbl2 = frm.controls.Add("Forms.Label.1", "lbl2" & intItemNumber, True)
        With lbl2
                .Height = 12
                .Top = lngTopOff + 24
                .Left = 6
                .Width = 54
                .Caption = "Subject:"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
            End With
            colCtrls.Add lbl2, "lbl2"
    End If  'blWideView

        If blWideView Then
        Set lbl3 = frm.controls.Add("Forms.Label.1", "lbl3" & intItemNumber, True)
        With lbl3
                .Height = 12
                .Top = lngTopOff + 36
                .Left = 6
                .Width = 54
                .Caption = "Body:"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
            End With
            colCtrls.Add lbl3, "lbl3"
    End If

        If InitType And InitSort Then
    'TURN OFF IF CONDIT REMINDER
        Set lbl5 = frm.controls.Add("Forms.Label.1", "lbl5" & intItemNumber, True)
    With lbl5
                .Height = 12
                .Top = lngTopOff
                .Left = 372
                .Width = 78
                .Caption = "Folder:"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
            End With
            colCtrls.Add lbl5, "lbl5"
    End If


        lblSender = frm.controls.Add("Forms.Label.1", "lblSender" & intItemNumber, True)
        With lblSender
            .Height = 12
            .Top = lngTopOff

            If blWideView Then
                .Left = Left_lblSender
                .Width = Width_lblSender
            Else
                .Left = Left_lblSender_C
                .Width = Width_lblSender_C
            End If  'blWideView


            .Caption = "<SENDER>"
            .Font.Name = "Tahoma"
            .Font.Size = 10
        End With
        colCtrls.Add lblSender, "lblSender"



    Dim lblTriage As MSForms.Label
        lblTriage = frm.controls.Add("Forms.Label.1", "lblTriage" & intItemNumber, True)
        With lblTriage
            .Height = 12
            .Top = lngTopOff

            If blWideView Then
                .Left = Left_lblSender
                .Width = Width_lblSender
            Else
                .Left = Left_lblTriage
                .Width = Width_lblTriage
            End If  'blWideView


            .Caption = "ABC"
            .Font.Name = "Tahoma"
            .Font.Size = 10
        End With
        colCtrls.Add lblTriage, "lblTriage"




    Dim lblActionable As MSForms.Label
        lblActionable = frm.controls.Add("Forms.Label.1", "lblActionable" & intItemNumber, True)
        With lblActionable
            .Height = 12
            .Top = lngTopOff

            If blWideView Then
                .Left = Left_lblSender
                .Width = Width_lblSender
            Else
                .Left = Left_lblActionable
                .Width = Width_lblActionable
            End If


            .Caption = "<ACTIONABL>"
            .Font.Name = "Tahoma"
            .Font.Size = 10
        End With
        colCtrls.Add lblActionable, "lblActionable"




    lblSubject = frm.controls.Add("Forms.Label.1", "lblSubject" & intItemNumber, True)
        With lblSubject
            If blWideView Then
                .Height = 12
                .Top = lngTopOff + 24
                .Left = Left_lblSubject
                .Width = Width_lblSubject
                .Font.Size = 10
            ElseIf InitType And InitConditionalReminder Then
                .Height = 18
                .Top = lngTopOff + 12
                .Left = Left_lblSubject_C
                .Width = frmWd - .Left - .Left
                .Font.Size = 16
            Else
                .Height = 18
                .Top = lngTopOff + 12
                .Left = Left_lblSubject_C
                .Width = Width_lblSubject_C
                .Font.Size = 16
            End If

            .Caption = "<SUBJECT>"
            .Font.Name = "Tahoma"

        End With
        colCtrls.Add lblSubject, "lblSubject"


    lblBody = frm.controls.Add("Forms.Label.1", "lblBody" & intItemNumber, True)
        With lblBody

            If blWideView Then
                .Top = lngTopOff + 36
                .Left = Left_lblBody
                .Width = Width_lblBody
                .Height = 30 + 6 - lngTopOff
            ElseIf InitType And InitConditionalReminder Then
                .Top = lngTopOff + 30
                .Left = Left_lblBody_C
                .Width = frmWd - .Left - .Left
                .Height = 36 + 6 - lngTopOff
            Else
                .Top = lngTopOff + 30
                .Left = Left_lblBody_C
                .Width = Width_lblBody_C
                .Height = 36 + 6 - lngTopOff

            End If

            .Caption = "<BODY>"
            .Font.Name = "Tahoma"
            .Font.Size = 10
            .WordWrap = True
        End With
        colCtrls.Add lblBody, "lblBody"


    lblSentOn = frm.controls.Add("Forms.Label.1", "lblSentOn" & intItemNumber, True)
        With lblSentOn
            .Height = 12
            If blWideView Then
                .Top = lngTopOff + 12
                .Left = Left_lblSentOn
                .TextAlign = fmTextAlignLeft
            Else
                .Top = lngTopOff
                .Left = Left_lblSentOn_C
                .TextAlign = fmTextAlignRight
            End If

            .Width = 156
            .Caption = "<SENTON>"
            .Font.Name = "Tahoma"
            .Font.Size = 10
        End With
        colCtrls.Add lblSentOn, "lblSentOn"


    If InitType And InitSort Then
            cbxFolder = frm.controls.Add("Forms.ComboBox.1", "cbxFolder" & intItemNumber, True)
            With cbxFolder
                .Height = 24
                .Top = 20 + lngTopOff
                .Left = Left_cbxFolder
                .Width = Width_cbxFolder
                .Font.Name = "Tahoma"
                .Font.Size = 8
                .TabStop = False
            End With
            colCtrls.Add cbxFolder, "cbxFolder"
    End If


        If InitType And InitSort Then
            inpt = frm.controls.Add("Forms.Textbox.1", "inpt" & intItemNumber, True)
            With inpt
                .Height = 18
                .Top = lngTopOff
                .Left = 408
                .Width = Width_inpt
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .TabStop = False
                .BackColor = &H8000000F
            End With
            colCtrls.Add inpt, "inpt"





    chbxSaveMail = frm.controls.Add("Forms.Checkbox.1", "chbxSaveMail" & intItemNumber, True)
            With chbxSaveMail

                .Height = 16
                .Width = 37
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .Caption = " Mail"
                .Value = False
                .TabStop = False
                If blWideView Then

                Else
                    .Top = 47 + lngTopOff
                    .Left = Right_Aligned - .Width
                End If
            End With
            colCtrls.Add chbxSaveMail, "chbxSaveMail"


    chbxDelFlow = frm.controls.Add("Forms.Checkbox.1", "chbxDelFlow" & intItemNumber, True)
            With chbxDelFlow

                .Height = 16
                .Width = 45
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .Caption = " Flow"
                .Value = False
                .TabStop = False

                If blWideView Then

                Else
                    .Top = 47 + lngTopOff
                    .Left = chbxSaveMail.Left - .Width - 1
                End If

            End With
            colCtrls.Add chbxDelFlow, "chbxDelFlow"


    chbxSaveAttach = frm.controls.Add("Forms.Checkbox.1", "chbxSaveAttach" & intItemNumber, True)
            With chbxSaveAttach

                .Height = 16
                .Width = 50
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .Caption = " Attach"
                .Value = True
                .TabStop = False

                If blWideView Then

                Else
                    .Top = 47 + lngTopOff
                    .Left = chbxDelFlow.Left - .Width - 1
                End If

            End With
            colCtrls.Add chbxSaveAttach, "chbxSaveAttach"

    chbxGPConv = frm.controls.Add("Forms.Checkbox.1", "chbxGPConv" & intItemNumber, True)
            With chbxGPConv
                .Height = 16
                .Width = 81
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .Caption = "  Conversation"
                .Value = blGroupConversation
                .TabStop = False
                If blWideView Then
                    .Top = lngTopOff
                    .Left = Left_chbxGPConv
                Else
                    .Top = 47 + lngTopOff
                    .Left = chbxSaveAttach.Left - .Width - 1
                End If
            End With
            colCtrls.Add chbxGPConv, "chbxGPConv"

End If

        cbFlagItem = frm.controls.Add("Forms.CommandButton.1", "cbFlagItem" & intItemNumber, True)
        With cbFlagItem
            .Height = 18
            .Top = lngTopOff
            .Left = Left_cbFlagItem
            .Width = Width_cb
            .Font.Name = "Tahoma"
            .Font.Size = 8
            .Caption = "|>"
            .BackColor = &H8000000F
            .ForeColor = &H80000012
            .TabStop = False
        End With
        colCtrls.Add cbFlagItem, "cbFlagItem"


    cbKllItem = frm.controls.Add("Forms.CommandButton.1", "cbKllItem" & intItemNumber, True)
        With cbKllItem
            .Height = 18
            .Top = lngTopOff
            .Left = cbFlagItem.Left + Width_cb + 2
            .Width = Width_cb
            .Font.Name = "Tahoma"
            .Font.Size = 8
            .Caption = "-->"
            .BackColor = &H8000000F
            .ForeColor = &H80000012
            .TabStop = False
        End With
        colCtrls.Add cbKllItem, "cbKllItem"


    cbDelItem = frm.controls.Add("Forms.CommandButton.1", "cbDelItem" & intItemNumber, True)
        With cbDelItem
            .Height = 18
            .Top = lngTopOff
            .Left = cbKllItem.Left + Width_cb + 2
            .Width = Width_cb
            .Font.Name = "Tahoma"
            .Font.Size = 8
            .Caption = "X"
            .BackColor = &HC0&
            .ForeColor = &H8000000E
            .TabStop = False
        End With
        colCtrls.Add cbDelItem, "cbDelItem"






If InitType And InitSort Then
            lblConvCt = frm.controls.Add("Forms.Label.1", "lblConvCt" & intItemNumber, True)
            With lblConvCt
                .Height = 18
                .TextAlign = fmTextAlignRight

                If blWideView Then
                    .Left = Left_lblConvCt
                    .Top = lngTopOff
                Else
                    .Left = Left_lblConvCt_C
                    .Top = lngTopOff + 12
                End If
                .Width = 36
                .Caption = "<#>"
                .Font.Name = "Tahoma"
                If blWideView Then
                    .Font.Size = 12
                Else
                    .Font.Size = 16
                End If

                .Enabled = blGroupConversation

            End With
            colCtrls.Add lblConvCt, "lblConvCt"
End If

        lblPos = frm.controls.Add("Forms.Label.1", "lblPos" & intItemNumber, True)
        With lblPos
            .Height = 20
            .Top = lngTopOff

            If blWideView Then
                .Left = 6
            Else
                .Left = 0
            End If

            .Width = 20
            .Caption = "<Pos#>"
            .Font.Bold = True
            .Font.Name = "Tahoma"
            .Font.Size = 14
            .BackColor = &H8000000D
            .ForeColor = &H8000000E
            .Enabled = False
            .Visible = blDebug
        End With
        colCtrls.Add lblPos, "lblPos"

If InitType And InitSort Then
            lblAcF = frm.controls.Add("Forms.Label.1", "lblAcF" & intItemNumber, True)
            With lblAcF
                .Height = 14
                .Top = max(lngTopOff - 2, 0)
                .Left = 363
                .Width = 14
                .Caption = "F"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .BorderStyle = fmBorderStyleSingle
                .TextAlign = fmTextAlignCenter
                .SpecialEffect = fmSpecialEffectBump
                .BackColor = &H80000012
                .ForeColor = &H8000000E
                .Visible = blDebug

            End With
            colCtrls.Add lblAcF, "lblAcF"

    lblAcD = frm.controls.Add("Forms.Label.1", "lblAcD" & intItemNumber, True)
            With lblAcD
                .Height = 14
                .Top = 20 + lngTopOff
                .Left = 363
                .Width = 14
                .Caption = "D"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .BorderStyle = fmBorderStyleSingle
                .TextAlign = fmTextAlignCenter
                .SpecialEffect = fmSpecialEffectBump
                .BackColor = &H80000012
                .ForeColor = &H8000000E
                .Visible = blDebug
            End With
            colCtrls.Add lblAcD, "lblAcD"

    lblAcC = frm.controls.Add("Forms.Label.1", "lblAcC" & intItemNumber, True)
            With lblAcC
                .Height = 14
                .Top = lngTopOff + 47
                .Left = chbxGPConv.Left + 12
                .Width = 14
                .Caption = "C"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .BorderStyle = fmBorderStyleSingle
                .TextAlign = fmTextAlignCenter
                .SpecialEffect = fmSpecialEffectBump
                .BackColor = &H80000012
                .ForeColor = &H8000000E
                .Visible = blDebug
            End With
            colCtrls.Add lblAcC, "lblAcC"
End If


        lblAcR = frm.controls.Add("Forms.Label.1", "lblAcR" & intItemNumber, True)
        With lblAcR
            .Height = 14
            .Top = 2 + lngTopOff
            .Left = cbKllItem.Left + 6
            .Width = 14
            .Caption = "R"
            .Font.Bold = True
            .Font.Name = "Tahoma"
            .Font.Size = 10
            .BorderStyle = fmBorderStyleSingle
            .TextAlign = fmTextAlignCenter
            .SpecialEffect = fmSpecialEffectBump
            .BackColor = &H80000012
            .ForeColor = &H8000000E
            .Visible = blDebug
        End With
        colCtrls.Add lblAcR, "lblAcR"

    lblAcX = frm.controls.Add("Forms.Label.1", "lblAcX" & intItemNumber, True)
        With lblAcX
            .Height = 14
            .Top = 2 + lngTopOff
            .Left = cbDelItem.Left + 6
            .Width = 14
            .Caption = "X"
            .Font.Bold = True
            .Font.Name = "Tahoma"
            .Font.Size = 10
            .BorderStyle = fmBorderStyleSingle
            .TextAlign = fmTextAlignCenter
            .SpecialEffect = fmSpecialEffectBump
            .BackColor = &H80000012
            .ForeColor = &H8000000E
            .Visible = blDebug
        End With
        colCtrls.Add lblAcX, "lblAcX"

    lblAcT = frm.controls.Add("Forms.Label.1", "lblAcT" & intItemNumber, True)
        With lblAcT
            .Height = 14
            .Top = 2 + lngTopOff
            .Left = cbFlagItem.Left + 6
            .Width = 14
            .Caption = "T"
            .Font.Bold = True
            .Font.Name = "Tahoma"
            .Font.Size = 10
            .BorderStyle = fmBorderStyleSingle
            .TextAlign = fmTextAlignCenter
            .SpecialEffect = fmSpecialEffectBump
            .BackColor = &H80000012
            .ForeColor = &H8000000E
            .Visible = blDebug
        End With
        colCtrls.Add lblAcT, "lblAcT"

    lblAcO = frm.controls.Add("Forms.Label.1", "lblAcO" & intItemNumber, True)
        With lblAcO
            .Height = 14

            If blWideView Then
                .Top = 36 + lngTopOff
                .Left = Left_lblAcO_C
            Else
                .Top = lblBody.Top
                .Left = Left_lblAcO_C
            End If
            .Width = 14
            .Caption = "O"
            .Font.Bold = True
            .Font.Name = "Tahoma"
            .Font.Size = 10
            .BorderStyle = fmBorderStyleSingle
            .TextAlign = fmTextAlignCenter
            .SpecialEffect = fmSpecialEffectBump
            .BackColor = &H80000012
            .ForeColor = &H8000000E
            .Visible = blDebug
        End With
        colCtrls.Add lblAcO, "lblAcO"


If InitType And InitSort Then
            lblAcA = frm.controls.Add("Forms.Label.1", "lblAcA" & intItemNumber, True)
            With lblAcA
                .Height = 14

                If blWideView Then
                    .Top = 36 + lngTopOff
                    .Left = chbxSaveAttach.Left + 10
                Else
                    .Top = chbxSaveAttach.Top
                    .Left = chbxSaveAttach.Left + 10
                End If
                .Width = 14
                .Caption = "A"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .BorderStyle = fmBorderStyleSingle
                .TextAlign = fmTextAlignCenter
                .SpecialEffect = fmSpecialEffectBump
                .BackColor = &H80000012
                .ForeColor = &H8000000E
                .Visible = blDebug
            End With
            colCtrls.Add lblAcA, "lblAcA"

    lblAcW = frm.controls.Add("Forms.Label.1", "lblAcW" & intItemNumber, True)
            With lblAcW
                .Height = 14

                If blWideView Then
                    .Top = 36 + lngTopOff
                    .Left = chbxDelFlow.Left + 29
                Else
                    .Top = chbxDelFlow.Top
                    .Left = chbxDelFlow.Left + 29
                End If
                .Width = 14
                .Caption = "W"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .BorderStyle = fmBorderStyleSingle
                .TextAlign = fmTextAlignCenter
                .SpecialEffect = fmSpecialEffectBump
                .BackColor = &H80000012
                .ForeColor = &H8000000E
                .Visible = blDebug
            End With
            colCtrls.Add lblAcW, "lblAcW"

    lblAcM = frm.controls.Add("Forms.Label.1", "lblAcM" & intItemNumber, True)
            With lblAcM
                .Height = 14

                If blWideView Then
                    .Top = 36 + lngTopOff
                    .Left = chbxSaveMail.Left + 10
                Else
                    .Top = chbxSaveMail.Top
                    .Left = chbxSaveMail.Left + 10
                End If
                .Width = 14
                .Caption = "M"
                .Font.Bold = True
                .Font.Name = "Tahoma"
                .Font.Size = 10
                .BorderStyle = fmBorderStyleSingle
                .TextAlign = fmTextAlignCenter
                .SpecialEffect = fmSpecialEffectBump
                .BackColor = &H80000012
                .ForeColor = &H8000000E
                .Visible = blDebug
            End With
            colCtrls.Add lblAcM, "lblAcM"
End If

        Temp = SF_Stack.Pop

        If blDebug Then Stop

    End Sub

    Private Sub RemoveControls()

        'Procedure Naming
        Dim SubNm As String
        SubNm = "RemoveControls"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************

        Dim QF As QfcController
        Dim i As Integer
        Dim max As Integer

        'max = colQFClass.Count
        'For i = max To 1 Step -1
        If Not colQFClass Is Nothing Then
            Do While colQFClass.Count > 0
                i = colQFClass.Count
                QF = colQFClass(i)
                QF.ctrlsRemove                                  'Remove controls on the frame
                FrameMain.controls.Remove QF.frm.Name           'Remove the frame
                QF.kill                                         'Remove the variables linking to events

                'FrameMain.Controls.Remove colFrames(i).Name
                colQFClass.Remove i
    Loop
        End If

        QF = Nothing                                    'Free up the QfcController class memory

        FrameMain.ScrollHeight = Height_FrameMain_Max

        Temp = SF_Stack.Pop

    End Sub

    Sub MoveDownControlGroups(intPosition As Integer, intMoves As Integer)
        'Procedure Naming
        Dim SubNm As String
        SubNm = "MoveDownControlGroups"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim i As Integer
        Dim QF As QfcController
        Dim intItemCount As Integer
        Dim ctlFrame As MSForms.frame
        Dim blDebug As Boolean

        blDebug = False

        For i = colQFClass.Count To intPosition Step -1

            'Shift items downward if there are any
            QF = colQFClass(i)
            QF.intMyPosition = QF.intMyPosition + intMoves
            ctlFrame = QF.frm
            ctlFrame.Top = ctlFrame.Top + intMoves * (frmHt + frmSp)
        Next i
        'FrameMain.ScrollHeight = max((intMoves + colQFClass.Count) * (frmHt + frmSp), Height_FrameMain_Max)

        Temp = SF_Stack.Pop
    End Sub

    Sub ToggleRemoteMouseLabels()
        'Procedure Naming
        Dim SubNm As String
        SubNm = "ToggleRemoteMouseLabels"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
        Dim ttrace As String
        Dim errcapt As Variant

        '*******************END Error Header*********************

        BoolRemoteMouseApp = Not BoolRemoteMouseApp

        Dim QF As QfcController

        For Each QF In colQFClass
            QF.ToggleRemoteMouseAppLabels
        Next QF

        '************Standard Error Handling Footer**************
        On Error Resume Next
        Temp = SF_Stack.Pop
        Exit Sub

ErrorHandler:
        ExplConvView_Cleanup()
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "END BREAK - PROCEDURE COMMANDS OUTPUT. RESUME PROCEDURE TRACING"
    Debug.Print ttrace
    errRaised = True
        Deactivate_Email_Timing_And_Velocity
        Tracing_WRITE
        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        Stop
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            Stop
            Resume
        End If

        '*******************END Standard Error Footer*********************

    End Sub
    Sub MoveDownPix(intPosition As Integer, intPix As Integer)
        'Procedure Naming
        Dim SubNm As String
        SubNm = "MoveDownPix"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim i As Integer
        Dim QF As QfcController
        Dim intItemCount As Integer
        Dim ctlFrame As MSForms.frame
        Dim blDebug As Boolean

        blDebug = False

        For i = colQFClass.Count To intPosition Step -1

            'Shift items downward if there are any
            QF = colQFClass(i)
            ctlFrame = QF.frm
            ctlFrame.Top = ctlFrame.Top + intPix
        Next i
        FrameMain.ScrollHeight = max(max(intPix, 0) + (colQFClass.Count * (frmHt + frmSp)), FrameMain.Height)

        Temp = SF_Stack.Pop
    End Sub


    Sub AddEmailControlGroup(Optional objItem As Object,
    Optional posInsert As Integer = 0,
    Optional blGroupConversation As Boolean = True,
    Optional ConvCt As Integer = 0,
    Optional varList As Variant,
    Optional blChild As Boolean)

        'Procedure Naming
        Dim SubNm As String
        SubNm = "AddEmailControlGroup"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim Mail As [MailItem]
        Dim QF As QfcController
        Dim colCtrls As Collection
        Dim items As [Items]
        Dim i As Integer

        intUniqueItemCounter = intUniqueItemCounter + 1

        If objItem Is Nothing Then
            items = folderCurrent.Items
            objItem = items(max - intEmailPosition)
        End If

        If posInsert = 0 Then posInsert = (colQFClass.Count + 1)

        If TypeOf objItem Is MailItem Then
            Mail = objItem
            QF = New QfcController
            colCtrls = New Collection

            LoadGroupOfCtrls colCtrls, intUniqueItemCounter, posInsert, blGroupConversation

        QF.InitCtrls Mail, colCtrls, posInsert, BoolRemoteMouseApp, Me
        If blChild Then QF.blConChild = True
            If IsArray(varList) = True Then
                If UBound(varList) = 0 Then
                    QF.Init_FolderSuggestions
                Else
                    QF.Init_FolderSuggestions varList
            End If
            Else
                QF.Init_FolderSuggestions varList
        End If
            QF.CountMailsInConv ConvCt

        If posInsert > colQFClass.Count Then
                colQFClass.Add QF
        Else
                colQFClass.Add QF, QF.Mail.Subject & QF.Mail.SentOn & QF.Mail.Sender, posInsert
        End If

            For i = 1 To colQFClass.Count
                QF = colQFClass(i)
                'Debug.Print "colQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.mail.Subject
            Next i

        End If

        Temp = SF_Stack.Pop

    End Sub

    Sub ConvToggle_Group(selItems As Collection, intOrigPosition As Integer)
        'Procedure Naming
        Dim SubNm As String
        SubNm = "ConvToggle_Group"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim objEmail As [MailItem]
        Dim objItem As Object
        Dim i As Integer
        Dim QF As QfcController
        Dim QF_Orig As QfcController
        Dim intPosition As Integer
        Dim blDebug As Boolean

        blDebug = True

        QF_Orig = colQFClass(intOrigPosition)

        If blDebug Then
            For i = 1 To colQFClass.Count
                QF = colQFClass(i)
                'Debug.Print "colQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.mail.Subject
            Next i
        End If

        For Each objItem In selItems
            objEmail = objItem
            intPosition = GetEmailPositionInCollection(objEmail)
            'If intPosition < intOrigPosition Then QF_Orig.intMyPosition = intPosition
            RemoveSpecificControlGroup(intPosition)
        Next objItem

        Temp = SF_Stack.Pop

    End Sub

    Sub ConvToggle_UnGroup(selItems As Collection, intPosition As Integer, ConvCt As Integer, varList As Variant)
        'Procedure Naming
        Dim SubNm As String
        SubNm = "ConvToggle_UnGroup"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim objEmail As [MailItem]
        Dim objItem As Object
        Dim i As Integer
        Dim QF As QfcController
        Dim blDebug As Boolean

        blDebug = False

        If blDebug Then
            'Print data after movement
            'Debug.Print "DEBUG DATA BEFORE UNGROUP"
            For i = 1 To colQFClass.Count
                QF = colQFClass(i)
                'Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.mail.SentOn, "MM\DD\YY HH:MM") & "  " & QF.mail.Subject
            Next i
        End If

        MoveDownControlGroups intPosition + 1, selItems.Count
    For i = 1 To selItems.Count
            AddEmailControlGroup selItems(i), intPosition + i, False, ConvCt, varList, True
    Next i

        If blDebug Then
            'Print data after movement
            'Debug.Print "DEBUG DATA AFTER UNGROUP"
            For i = 1 To colQFClass.Count
                QF = colQFClass(i)
                'Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.mail.SentOn, "MM\DD\YY HH:MM") & "  " & QF.mail.Subject
            Next i
        End If
        UserForm_Resize()
        Temp = SF_Stack.Pop

    End Sub
    Private Function DoesCollectionHaveConvID(objItem As Object, col As Collection) As Integer
        'Procedure Naming
        Dim SubNm As String
        SubNm = "DoesCollectionHaveConvID"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim objItemInCol As Object
        Dim objMailInCol As [MailItem]
        Dim objMail As [MailItem]
        Dim i As Integer

        DoesCollectionHaveConvID = 0

        If TypeOf objItem Is MailItem Then
            objMail = objItem
            If Not col Is Nothing Then
                For i = 1 To col.Count
                    objItemInCol = col(i)
                    If TypeOf objItemInCol Is MailItem Then
                        objMailInCol = objItemInCol
                        If objMailInCol.ConversationID = objMail.ConversationID Then DoesCollectionHaveConvID = i
                    End If
                Next i
            End If
        End If

        Temp = SF_Stack.Pop

    End Function

    Private Function GetEmailPositionInCollection(objMail As [MailItem]) As Integer
        'Procedure Naming
        Dim SubNm As String
        SubNm = "GetEmailPositionInCollection"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim QF As QfcController
        Dim i As Integer

        GetEmailPositionInCollection = 0
        For i = 1 To colQFClass.Count
            QF = colQFClass(i)
            If QF.Mail.EntryID = objMail.EntryID Then GetEmailPositionInCollection = i
        Next i

        Temp = SF_Stack.Pop

    End Function

    Sub RemoveSpecificControlGroup(intPosition As Integer)

        'Procedure Naming
        Dim SubNm As String
        SubNm = "RemoveSpecificControlGroup"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************

        Dim blDebug As Boolean
        Dim QF As QfcController
        Dim intItemCount As Integer
        Dim i As Integer
        Dim ctlFrame As MSForms.frame
        Dim strDeletedSub As String
        Dim strDeletedDte As String
        Dim intDeletedMyPos As Integer

        blDebug = False

        intItemCount = colQFClass.Count

        QF = colQFClass(intPosition)                'Set class equal to specific member of collection
        On Error Resume Next
        strDeletedSub = QF.Mail.Subject
        strDeletedDte = Format(QF.Mail.SentOn, "mm\\dd\\yyyy hh:mm")
        intDeletedMyPos = QF.intMyPosition


        QF.ctrlsRemove                                  'Run the method that removes controls from the frame
2    FrameMain.controls.Remove QF.frm.Name           'Remove the specific frame
    QF.kill                                         'Remove the variables linking to events

        If blDebug Then
            'Print data before movement
            Debug.Print "DEBUG DATA BEFORE MOVEMENT"
        For i = 1 To intItemCount
                If i = intPosition Then
                    Debug.Print i & "  " & intDeletedMyPos & "  " & strDeletedDte & "  " & strDeletedSub
            Else
                    QF = colQFClass(i)
                    Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.Mail.SentOn, "MM\\DD\\YY HH:MM") & "  " & QF.Mail.Subject
            End If
            Next i
        End If

        'Shift items upward if there are any
        If intPosition < intItemCount Then
            For i = intPosition + 1 To intItemCount
                QF = colQFClass(i)
                QF.intMyPosition = QF.intMyPosition - 1
                ctlFrame = QF.frm
                ctlFrame.Top = ctlFrame.Top - frmHt - frmSp
            Next i
            FrameMain.ScrollHeight = max(FrameMain.ScrollHeight - frmHt - frmSp, Height_FrameMain_Max)
        End If

        colQFClass.Remove intPosition
    intEmailStart = intEmailStart + 1

        If blDebug Then
            'Print data after movement
            Debug.Print "DEBUG DATA POST MOVEMENT"
        For i = 1 To colQFClass.Count
                QF = colQFClass(i)
                Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.Mail.SentOn, "MM\\DD\\YY HH:MM") & "  " & QF.Mail.Subject
        Next i
        End If

        QF = Nothing

        Temp = SF_Stack.Pop

    End Sub


    Private Sub AcceleratorDialogue_Change()
        'Procedure Naming
        Dim SubNm As String
        SubNm = "AcceleratorDialogue_Change"

        '************Standard Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim ttrace As String
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
        ttrace = "Inside " & SubNm

        '*******************END Error Header*********************

        Dim strToParse As String
        Dim i As Integer
        Dim intLen As Integer
        Dim intLastNum As Integer
        Dim intAccTmpMail As Integer
        Dim strCommand As String
        Dim QF As QfcController
        Dim blExpanded As Boolean
        Dim strTemp As String
        Dim DebugLVL As DebugLevelEnum

        DebugLVL = vbProcedure + vbCommand

        strTemp = "If Not blSuppressEvents Then"
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
If Not blSuppressEvents Then
            strTemp = "If Not blSuppressEvents Then IS TRUE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "intLastNum = 0"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
    intLastNum = 0
            strTemp = "strToParse = AcceleratorDialogue.Value"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
    strToParse = AcceleratorDialogue.Value
            strTemp = "If strToParse <> '' Then"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
    If strToParse <> "" Then
                strTemp = "If strToParse <> '' Then IS TRUE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "intLen = Len(strToParse)"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        intLen = Len(strToParse)
                strTemp = "SKIPPING LOOP For i = 1 To intLen"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        For i = 1 To intLen
                    If IsNumeric(Mid(strToParse, i, 1)) Then
                        intLastNum = i
                    Else
                        Exit For
                    End If
                Next i
                strTemp = "If intLastNum > 0 Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If intLastNum > 0 Then
                    strTemp = "If intLastNum > 0 Then IS TRUE"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "intAccTmpMail = CInt(Mid(strToParse, 1, intLastNum))"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

            intAccTmpMail = CInt(Mid(strToParse, 1, intLastNum))

                    strTemp = "If intAccTmpMail <> intAccActiveMail Then"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If intAccTmpMail <> intAccActiveMail Then
                        strTemp = "If intAccTmpMail <> intAccActiveMail Then IS TRUE"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "If intAccActiveMail <> 0 And intAccActiveMail <= colQFClass.Count Then"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                If intAccActiveMail <> 0 And intAccActiveMail <= colQFClass.Count Then
                            strTemp = "If intAccActiveMail <> 0 And intAccActiveMail <= colQFClass.Count Then IS TRUE"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "QF = colQFClass(intAccActiveMail)"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    QF = colQFClass(intAccActiveMail)
                            strTemp = "If QF.blExpanded Then"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    If QF.blExpanded Then
                                strTemp = "If QF.blExpanded Then IS TRUE"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

                                                                                                    strTemp = "MoveDownPix intAccActiveMail + 1, QF.frm.Height * -0.5"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        MoveDownPix intAccActiveMail + 1, QF.frm.Height * -0.5
                                                                                                    strTemp = "QF.ExpandCtrls1"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.ExpandCtrls1
                                strTemp = "blExpanded = True"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        blExpanded = True
                            End If
                            strTemp = "QF.Accel_FocusToggle"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    QF.Accel_FocusToggle

                        End If

                        strTemp = "If intAccTmpMail <> 0 And intAccTmpMail <= colQFClass.Count Then"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                If intAccTmpMail <> 0 And intAccTmpMail <= colQFClass.Count Then
                            strTemp = "If intAccTmpMail <> 0 And intAccTmpMail <= colQFClass.Count Then IS TRUE"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "QF = colQFClass(intAccTmpMail)"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    QF = colQFClass(intAccTmpMail)
                            strTemp = "QF.Accel_FocusToggle"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    QF.Accel_FocusToggle
                            strTemp = "If blExpanded Then"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    If blExpanded Then
                                strTemp = "If blExpanded Then IS TRUE"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "MoveDownPix intAccTmpMail + 1, QF.frm.Height"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

                        MoveDownPix intAccTmpMail + 1, QF.frm.Height
                                                                                                    strTemp = "QF.ExpandCtrls1"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.ExpandCtrls1
                                strTemp = "blExpanded = False"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        blExpanded = False
                            End If
                            strTemp = "ScrollIntoView_MF QF.frm.Top, QF.frm.Top + QF.frm.Height"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    ScrollIntoView_MF QF.frm.Top, QF.frm.Top + QF.frm.Height

                End If

                        strTemp = "ScrollIntoView_MF QF.frm.Top, QF.frm.Top + QF.frm.Height"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                If intAccTmpMail <= colQFClass.Count Then
                            strTemp = "ScrollIntoView_MF QF.frm.Top, QF.frm.Top + QF.frm.Height IS TRUE"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "intAccActiveMail = intAccTmpMail"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    intAccActiveMail = intAccTmpMail
                        End If

                    End If
                    strTemp = "If intLen > intLastNum And intAccActiveMail <> 0 And intAccActiveMail <= colQFClass.Count Then"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If intLen > intLastNum And intAccActiveMail <> 0 And intAccActiveMail <= colQFClass.Count Then
                        strTemp = "If intLen > intLastNum Then IS TRUE"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "strCommand = UCase(Mid(strToParse, intLastNum + 1, 1))"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                strCommand = UCase(Mid(strToParse, intLastNum + 1, 1))
                        strTemp = "If blSuppressEvents = False Then"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                If blSuppressEvents = False Then
                            strTemp = "If blSuppressEvents = False Then IS TRUE"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "blSuppressEvents = True"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    blSuppressEvents = True
                            strTemp = "AcceleratorDialogue.Value = intAccActiveMail"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    AcceleratorDialogue.Value = intAccActiveMail
                            strTemp = "blSuppressEvents = False"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    blSuppressEvents = False
                        Else
                            strTemp = "If intLen > intLastNum Then IS FALSE"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "AcceleratorDialogue.Value = intAccActiveMail"
                            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    AcceleratorDialogue.Value = intAccActiveMail
                        End If
                        strTemp = "QF = colQFClass(intAccActiveMail)"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                QF = colQFClass(intAccActiveMail)
                        strTemp = "Select Case strCommand"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                Select Case strCommand
                            Case "O"
                                strTemp = "Case 'O'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "toggleAcceleratorDialogue"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        toggleAcceleratorDialogue()
                                strTemp = "EnableWindow lFormHandle, Modeless"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        EnableWindow OlApp_hWnd, Modeless
                        'EnableWindow lFormHandle, Modeless
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        If ActiveExplorer.CurrentFolder.DefaultItemType <> olMailItem Then
                            Set ActiveExplorer.NavigationPane.CurrentModule = ActiveExplorer.NavigationPane.Modules.GetNavigationModule(olModuleMail)
                        End If

                                If (InitType And InitSort) And AreConversationsGrouped Then ExplConvView_ToggleOff()                      'Modal
                                QF.KB strCommand

                                                                                                    strTemp = "QFD_Minimize"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QFD_Minimize()
                                strTemp = "If blShowAsConversations Then ExplConvView_ToggleOn"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        If (InitType And InitSort) And blShowAsConversations Then ExplConvView_ToggleOn()
                        'ToggleShowAsConversation 1
                        'SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
                            Case "C"
                                strTemp = "Case 'C'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "toggleAcceleratorDialogue"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        toggleAcceleratorDialogue()
                                strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "T"
                                strTemp = "Case 'T'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "toggleAcceleratorDialogue"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        toggleAcceleratorDialogue()
                                strTemp = "EnableWindow lFormHandle, Modeless"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        EnableWindow OlApp_hWnd, Modeless
                        'EnableWindow lFormHandle, Modeless
                                strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "F"
                                strTemp = "Case 'F'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "toggleAcceleratorDialogue"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        toggleAcceleratorDialogue()
                                strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "D"
                                strTemp = "Case 'D'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "toggleAcceleratorDialogue"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        toggleAcceleratorDialogue()
                                strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "X"
                                strTemp = "Case 'X'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "toggleAcceleratorDialogue"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        toggleAcceleratorDialogue()
                                strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "R"
                                strTemp = "Case 'R'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "toggleAcceleratorDialogue"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        toggleAcceleratorDialogue()
                                strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "A"
                                strTemp = "Case 'A'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "W"
                                strTemp = "Case 'W'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "M"
                                strTemp = "Case 'M'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "QF.KB strCommand"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF.KB strCommand
                    Case "E"
                                strTemp = "Case 'E'"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "QF = colQFClass(intAccActiveMail)"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        QF = colQFClass(intAccActiveMail)
                                strTemp = "If QF.blExpanded Then"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        If QF.blExpanded Then
                                    strTemp = "If QF.blExpanded Then IS TRUE"
                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "MoveDownPix intAccActiveMail + 1, QF.frm.Height * -0.5"
                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                            MoveDownPix intAccActiveMail + 1, QF.frm.Height * -0.5
                                                                                                    strTemp = "QF.ExpandCtrls1"
                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                            QF.ExpandCtrls1
                                Else
                                    strTemp = "Case 'Else'"
                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "MoveDownPix intAccActiveMail + 1, QF.frm.Height"
                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                            MoveDownPix intAccActiveMail + 1, QF.frm.Height
                                                                                                    strTemp = "QF.ExpandCtrls1"
                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                            QF.ExpandCtrls1
                                End If
                                '                                                                                                    strTemp = "AcceleratorDialogue.Value = Left(AcceleratorDialogue.Value, Len(AcceleratorDialogue.Value) - 1)"
                                '                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                '                        AcceleratorDialogue.value = Left(AcceleratorDialogue.value, Len(AcceleratorDialogue.value) - 1)
                            Case Else
                                strTemp = "Case Else"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "blSuppressEvents = True"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        blSuppressEvents = True
                                '                                                                                                    strTemp = "AcceleratorDialogue.Value = Left(AcceleratorDialogue.Value, Len(AcceleratorDialogue.Value) - 1)"
                                '                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                '                        AcceleratorDialogue.value = Left(AcceleratorDialogue.value, Len(AcceleratorDialogue.value) - 1)
                                strTemp = "blSuppressEvents = False"
                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                        blSuppressEvents = False
                        End Select
                    End If
                    strTemp = "End If 'intLen > intLastNum Then"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

        End If
                strTemp = "End If 'intLastNum > 0 Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

    Else
                strTemp = "If strToParse <> '' Then FALSE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "If intAccActiveMail <> 0 Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If intAccActiveMail <> 0 Then
                    strTemp = "If intAccActiveMail <> 0 Then IS TRUE"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "colQFClass(intAccActiveMail).Accel_FocusToggle"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            colQFClass(intAccActiveMail).Accel_FocusToggle
                    strTemp = "intAccActiveMail = 0"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            intAccActiveMail = 0
                End If
            End If
        Else
            strTemp = "If Not blSuppressEvents Then IS FALSE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

End If 'Suppress Events

        '************Standard Error Handling Footer**************
        On Error Resume Next
        Temp = SF_Stack.Pop
        Exit Sub

ErrorHandler:
        ExplConvView_Cleanup()
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "END BREAK - PROCEDURE COMMANDS OUTPUT. RESUME PROCEDURE TRACING"
    Debug.Print ttrace
    errRaised = True
        Deactivate_Email_Timing_And_Velocity
        Tracing_WRITE
        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        Stop
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            Stop
            reactivateAfterDebug
            Err.Clear()
            Resume
        End If

        '*******************END Standard Error Footer*********************


    End Sub



    Private Sub AcceleratorDialogue_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        'Procedure Naming
        Dim SubNm As String
        SubNm = "AcceleratorDialogue_KeyDown"

        '************Standard Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim ttrace As String
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
        ttrace = "Inside " & SubNm

        '*******************END Error Header*********************

        Dim QF As QfcController
        Dim blExpanded As Boolean
        Dim DebugLVL As DebugLevelEnum
        Dim strTemp As String

        DebugLVL = vbProcedure '+ vbCommand

        'If DebugLVL And vbProcedure Then Debug.Print "Fired AcceleratorDialogue_KeyDown"

        strTemp = "Select Case KeyCode " & KeyCode
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
    Select Case KeyCode
            Case 18
                strTemp = "Case 18"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Debug.Print "Alt Key Pressed"
                strTemp = "toggleAcceleratorDialogue"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            toggleAcceleratorDialogue()

            Case vbKeyDown
                strTemp = "Case 18"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "If AreConversationsGrouped Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If AreConversationsGrouped Then
                    strTemp = "If AreConversationsGrouped Then IS TRUE"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
'Modal                                                                                                    strTemp = "ExplConvView_ToggleOff"
                    'Modal                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    'Modal                ExplConvView_ToggleOff
                    '            Else
                    '                                                                                                    strTemp = "If AreConversationsGrouped Then IS FALSE"
                    '                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                End If
                strTemp = "End If 'AreConversationsGrouped Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "If intAccActiveMail < colQFClass.Count Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If intAccActiveMail < colQFClass.Count Then
                    strTemp = "If intAccActiveMail < colQFClass.Count Then IS TRUE"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "AcceleratorDialogue.Value = intAccActiveMail + 1"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                AcceleratorDialogue.Value = intAccActiveMail + 1
                End If
                strTemp = "End If 'intAccActiveMail < colQFClass.Count Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

        Case vbKeyUp
                strTemp = "Case vbKeyUp"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "If AreConversationsGrouped Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If AreConversationsGrouped Then
                    strTemp = "If AreConversationsGrouped Then IS TRUE"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
'Modal                                                                                                    strTemp = "ExplConvView_ToggleOff"
                    'Modal                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    'Modal                ExplConvView_ToggleOff
                End If
                strTemp = "End If 'AreConversationsGrouped Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

                                                                                                    strTemp = "If intAccActiveMail > 1 Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If intAccActiveMail > 1 Then
                    strTemp = "If intAccActiveMail > 1 Then IS TRUE"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "AcceleratorDialogue.Value = intAccActiveMail - 1"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                AcceleratorDialogue.Value = intAccActiveMail - 1
                End If
                strTemp = "End If 'intAccActiveMail > 1 Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

                                                                                                    strTemp = "AcceleratorDialogue.SetFocus"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            AcceleratorDialogue.SetFocus

            Case vbKeyA
                strTemp = "Case vbKeyA ... Shift value is " & CStr(Shift)
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If (Shift And acShiftMask = acShiftMask) And (Shift And acCtrlMask = acCtrlMask) Then
                    strTemp = "If (Shift And acShiftMask = acShiftMask) And (Shift And acCtrlMask = acCtrlMask) IS TRUE"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "ToggleRemoteMouseLabels"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                ToggleRemoteMouseLabels()

                End If
                '        Case Else
                '                                                                                                    strTemp = "Case Else"
                '                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

        End Select
        strTemp = "End Select"
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

'************Standard Error Handling Footer**************
        Temp = SF_Stack.Pop
        Exit Sub

ErrorHandler:
        ExplConvView_Cleanup()
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "END BREAK - PROCEDURE COMMANDS OUTPUT. RESUME PROCEDURE TRACING"
    Debug.Print ttrace
    errRaised = True
        Deactivate_Email_Timing_And_Velocity
        Tracing_WRITE
        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        Stop
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            Resume Next
        End If

        '*******************END Standard Error Footer*********************


    End Sub


    Private Sub AcceleratorDialogue_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Dim QF As QfcController
        Dim blExpanded As Boolean

        Select Case KeyCode
            Case 18
                If AcceleratorDialogue.Visible Then
                    AcceleratorDialogue.SetFocus
                    AcceleratorDialogue.SelStart = AcceleratorDialogue.TextLength
                Else
                    FrameMain.SetFocus
                End If
                SendKeys "{ESC}"
        Case vbKeyRight
                If AcceleratorDialogue.Visible And intAccActiveMail <> 0 Then
                    QF = colQFClass(intAccActiveMail)
                    If QF.lblConvCt <> "1" And QF.chk = True Then
                        If QF.blExpanded Then
                            blExpanded = True
                            MoveDownPix intAccActiveMail + 1, QF.frm.Height * -0.5
                        QF.ExpandCtrls1
                        End If
                        toggleAcceleratorDialogue()
                        QF.KB "C"
                    toggleAcceleratorDialogue()

                        If blExpanded Then
                            MoveDownPix intAccActiveMail + 1, QF.frm.Height
                        QF.ExpandCtrls1
                        End If
                    End If
                End If
            Case vbKeyLeft
                If AcceleratorDialogue.Visible And intAccActiveMail <> 0 Then
                    QF = colQFClass(intAccActiveMail)
                    If QF.lblConvCt <> "1" And QF.chk = False Then
                        If QF.blExpanded Then
                            blExpanded = True
                            MoveDownPix intAccActiveMail + 1, QF.frm.Height * -0.5
                        QF.ExpandCtrls1
                        End If
                        toggleAcceleratorDialogue()
                        QF.KB "C"
                    toggleAcceleratorDialogue()

                        If blExpanded Then
                            MoveDownPix intAccActiveMail + 1, QF.frm.Height
                        QF.ExpandCtrls1
                        End If

                    End If
                    AcceleratorDialogue.SelStart = AcceleratorDialogue.TextLength
                End If
            Case Else
        End Select
    End Sub

    Private Sub BUTTON_CANCEL_Click()

        'Procedure Naming
        Dim SubNm As String
        SubNm = "BUTTON_CANCEL_Click"
        Dim Temp As Variant
        Dim ttrace As String
        Dim errcapt As Variant


        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************

        'ExplConvView_ToggleOn
        If blShowAsConversations Then
            'ExplConvView_ToggleOn
            ExplConvView_Cleanup()
        End If
        'ToggleShowAsConversation 1
        RemoveControls()
        blFrmKll = True
        'ErrHandler_Execute SubNm, ttrace, errcapt, errRaised, vbProcedure
        Unload QuickFileDyn

    On Error Resume Next
        Temp = SF_Stack.Pop

    End Sub

    Private Sub BUTTON_CANCEL_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Dim DebugLVL As DebugLevelEnum
        DebugLVL = vbProcedure

        'If DebugLVL And vbProcedure Then Debug.Print "Fired BUTTON_CANCEL_KeyDown"

        KeyDownHandler KeyCode, Shift

End Sub

    Private Sub Button_OK_Click()

        'Procedure Naming
        Dim SubNm As String
        SubNm = "Button_OK_Click"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim QF As QfcController
        Dim blReadyForMove As Boolean
        Dim strNotifications As String
        Dim oMailTmp As [MailItem]

        If InitType And InitSort Then
            If blRunningModalCode = False Then
                blRunningModalCode = True

                blReadyForMove = True
                strNotifications = "Can't complete actions! Not all emails assigned to folder" & vbCrLf

                For Each QF In colQFClass
                    If QF.cbo.Value = "" Then
                        blReadyForMove = False
                        strNotifications = strNotifications & QF.intMyPosition &
                        "  " & Format(QF.Mail.SentOn, "mm\\dd\\yyyy") &
                        "  " & QF.Mail.Subject & vbCrLf
                    End If
                Next QF
                strNotifications = Mid(strNotifications, 1, Len(strNotifications) - 1)

                If blReadyForMove Then
                    blSuppressEvents = True
                    intAccActiveMail = 1
                    colMailJustMoved = New Collection

                    For Each QF In colQFClass
                        QF.MoveMail
                    Next QF

                    'QuickFileMetrics_WRITE "9999QuickFileMetrics.csv"
                    QuickFileMetrics_WRITE "9999TimeWritingEmail.csv"
            RemoveControls()
                    Iterate()
                    blSuppressEvents = False
                Else
                    MsgBox strNotifications, vbOKOnly + vbCritical, "Error Notification"
        End If

                AcceleratorDialogue.Value = ""
                intAccActiveMail = 0

                blRunningModalCode = False
            Else
                MyBoxMsg "Can't Execute While Running Modal Code"
    End If

        Else
            Unload Me
End If

        On Error Resume Next
        Temp = SF_Stack.Pop
        'Debug.Print "tmp"

    End Sub


    Private Sub Button_OK_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Dim DebugLVL As DebugLevelEnum
        DebugLVL = vbProcedure

        'If DebugLVL And vbProcedure Then Debug.Print "Fired Button_OK_KeyDown"

        KeyDownHandler KeyCode, Shift
End Sub

    Private Sub Button_OK_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        KeyUpHandler KeyCode, Shift
End Sub


    Private Sub Button_Undo_Click()
        Dim i As Integer
        Dim oMail As MailItem
        Dim oMailTmp As MailItem
        Dim oMail_Old As MailItem
        Dim oMail_Current As MailItem
        Dim objTemp As Object
        Dim oFolder_Current As [Folder]
        Dim oFolder_Old As [Folder]
        Dim oItemFolder As [Folder]
        Dim colItems As Collection
        Dim col As Collection
        Dim vbUndoResponse As VbMsgBoxResult
        Dim vbRepeatResponse As VbMsgBoxResult

        '    If Not colMailJustMoved Is Nothing Then
        '        If colMailJustMoved.Count <> 0 Then
        '
        '            oFolderCurrent = Application.ActiveExplorer.CurrentFolder
        '
        '            For i = 1 To colMailJustMoved.Count
        '                If TypeOf colMailJustMoved(i) Is mailItem Then
        '                    oMail = colMailJustMoved(i)
        '                    If Mail_IsItEncrypted(oMail) = False Then
        '                        vbUndoResponse = MsgBox("Undo Move of email: " & oMail.Subject & "?", vbYesNo)
        '                        If vbUndoResponse = vbYes Then
        '                            colItems = New Collection
        '                            col = New Collection
        '                            col.Add oMail
        '
        '                            On Error Resume Next
        '                            colItems = Email_SortToExistingFolder.DemoConversation(colItems, col)
        '
        '                            If Err.Number <> 0 Or colItems.Count = 0 Then
        '                                Err.Clear
        '                                colItems = New Collection
        '                                colItems.Add oMail
        '                            End If
        '
        '                            For Each oMailTmp In colItems
        '                                oItemFolder = oMailTmp.Parent
        '                                If oItemFolder <> oFolderCurrent Then
        '                                    oMailTmp.Move oFolderCurrent
        '                                End If
        '                            Next oMailTmp
        '                        End If   'If vbUndoResponse = vbYes
        '                    End If    'If Mail_IsItEncrypted(oMail) = False
        '                End If   'If TypeOf colMailJustMoved(i) Is mailItem
        '            Next i
        '        End If  'If colMailJustMoved.Count <> 0
        '    End If  'If Not colMailJustMoved Is Nothing
        '

        If MovedMails_Stack Is Nothing Then MovedMails_Stack = New cStackObject
        vbRepeatResponse = vbYes

        i = MovedMails_Stack.Count
        colItems = MovedMails_Stack.ToCollection

        While (i > 1) And (vbRepeatResponse = vbYes)
            objTemp = colItems(i)
            'objTemp = MovedMails_Stack.Pop
            If TypeOf objTemp Is MailItem Then oMail_Current = objTemp
            'objTemp = MovedMails_Stack.Pop
            objTemp = colItems(i - 1)
            If TypeOf objTemp Is MailItem Then oMail_Old = objTemp

            'oMail_Old = MovedMails_Stack.Pop
            If (Mail_IsItEncrypted(oMail_Current) = False) And (Mail_IsItEncrypted(oMail_Old) = False) Then
                oFolder_Current = oMail_Current.Parent
                oFolder_Old = oMail_Old.Parent
                vbUndoResponse = MsgBox("Undo Move of email?" & vbCrLf & "Sent On: " &
                Format(oMail_Current.SentOn, "mm/dd/yyyy") & vbCrLf &
                oMail_Current.Subject, vbYesNo)
                If vbUndoResponse = vbYes And oFolder_Current <> oFolder_Old Then
                    oMail_Current.Move oFolder_Old
                MovedMails_Stack.Pop i
                MovedMails_Stack.Pop(i - 1)
                End If
            End If
            i = i - 2
            vbRepeatResponse = MsgBox("Continue Undoing Moves?", vbYesNo)
    Wend
    
    
End Sub

    Private Sub FrameMain_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Dim DebugLVL As DebugLevelEnum
        DebugLVL = vbProcedure

        'If DebugLVL And vbProcedure Then Debug.Print "Fired FrameMain_KeyDown"

        KeyDownHandler KeyCode, Shift
End Sub

    Private Sub FrameMain_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        'MsgBox ("KeyPress FrameMain")
        KeyPressHandler KeyAscii
End Sub

    Private Sub FrameMain_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        KeyUpHandler KeyCode, Shift
End Sub

    Private Sub SpinButton1_Change()

    End Sub

    Private Sub lbl_EmailPerLoad_Click()

    End Sub

    Private Sub spn_EmailPerLoad_Change()
        If spn_EmailPerLoad.Value >= 0 Then
            intEmailsPerIteration = spn_EmailPerLoad.Value
            lbl_EmailPerLoad.Caption = intEmailsPerIteration
        End If
    End Sub

    Private Sub spn_EmailPerLoad_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Dim DebugLVL As DebugLevelEnum
        DebugLVL = vbProcedure

        'If DebugLVL And vbProcedure Then Debug.Print "Fired BUTTON_CANCEL_KeyDown"

        KeyDownHandler KeyCode, Shift

End Sub

    Private Sub UserForm_Activate()

        If Not StopWatch Is Nothing Then
            If StopWatch.isPaused = True Then
                StopWatch.reStart()
            End If
        End If
    End Sub

    Public Sub toggleAcceleratorDialogue()
        'Procedure Naming
        Dim SubNm As String
        SubNm = "toggleAcceleratorDialogue"

        '************Standard Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim ttrace As String
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
        ttrace = "Inside " & SubNm

        '*******************END Error Header*********************



        Dim QF As QfcController
        Dim i As Integer
        Dim DebugLVL As DebugLevelEnum
        Dim strTemp As String

        DebugLVL = vbCommand + vbVariable

        'If DebugLVL AndvbProcedure Or DebugLVL = vbCommand Then
        '
        '    SubNm = "toggleAcceleratorDialogue"
        '    ErrHandler_Init SubNm, ttrace, DebugLVL
        '    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
        '
        'End If

        strTemp = "For i = 1 To colQFClass.Count"
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

    If Not colQFClass Is Nothing Then
            For i = 1 To colQFClass.Count
                strTemp = "For i = 1 To colQFClass.Count = i = " & i
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                strTemp = "QF = colQFClass(i)"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        QF = colQFClass(i)

                strTemp = "If QF.blExpanded And i <> colQFClass.Count Then MoveDownPix i + 1, QF.frm.Height * -0.5"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If QF.blExpanded And i <> colQFClass.Count Then MoveDownPix i + 1, QF.frm.Height * -0.5
                                                                                strTemp = "QF.Accel_Toggle"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        QF.Accel_Toggle
            Next i
        End If

        strTemp = "If AcceleratorDialogue.Visible = True Then"
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
    If AcceleratorDialogue.Visible = True Then
            strTemp = "If AcceleratorDialogue.Visible = True Then IS TRUE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                strTemp = "AcceleratorDialogue.Visible = False"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        AcceleratorDialogue.Visible = False
            'Modal                                                                                strTemp = "ExplConvView_ToggleOn"
            'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Modal        ExplConvView_ToggleOn
            strTemp = "FrameMain.SetFocus"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        FrameMain.SetFocus
        Else
            strTemp = "If AcceleratorDialogue.Visible = True Then IS FALSE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                strTemp = "If AreConversationsGrouped Then ExplConvView_ToggleOff"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If AreConversationsGrouped Then
                'ToggleShowAsConversation -1
                strTemp = "AreConversationsGrouped = TRUE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
'Modal                                                                                strTemp = "ExplConvView_ToggleOff"
                'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                'Modal            ExplConvView_ToggleOff
            Else
                strTemp = "AreConversationsGrouped = FALSE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        End If
            AcceleratorDialogue.Visible = True
            strTemp = "If intAccActiveMail <> 0 Then"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                strTemp = "VARIABLE: intAccActiveMail == " & intAccActiveMail
            If DebugLVL And vbVariable Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If intAccActiveMail <> 0 Then
                strTemp = "If intAccActiveMail <> 0 Then IS TRUE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                strTemp = "AcceleratorDialogue.Value = intAccActiveMail"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            AcceleratorDialogue.Value = intAccActiveMail
                strTemp = "QF = colQFClass(intAccActiveMail)"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            On Error Resume Next
                QF = colQFClass(intAccActiveMail)
                If Err.Number <> 0 Then
                    Err.Clear()
                    intAccActiveMail = 1
                    QF = colQFClass(intAccActiveMail)
                End If
                On Error GoTo ErrorHandler
                strTemp = "QF.Accel_FocusToggle"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            QF.Accel_FocusToggle
            Else
                strTemp = "If intAccActiveMail <> 0 Then IS FALSE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        End If
            'Modal                                                                                strTemp = "SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&"
            'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Modal        SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
            strTemp = "AcceleratorDialogue.SetFocus"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        AcceleratorDialogue.SetFocus
            AcceleratorDialogue.SelStart = AcceleratorDialogue.TextLength
        End If

        QF = Nothing

        '************Standard Error Handling Footer**************
        On Error Resume Next
        Temp = SF_Stack.Pop
        Exit Sub

ErrorHandler:
        ExplConvView_Cleanup()
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "END BREAK - PROCEDURE COMMANDS OUTPUT. RESUME PROCEDURE TRACING"
    Debug.Print ttrace
    errRaised = True
        Deactivate_Email_Timing_And_Velocity
        Tracing_WRITE
        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        Stop
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            Stop
            Resume
        End If

        '*******************END Standard Error Footer*********************


    End Sub

    Private Sub ScrollIntoView_MF(lngItemTop As Long, lngItemBottom As Long)
        Dim DiffY As Long

        If lngItemTop < lngFrameMain_SC_Top Then
            'Diffy = lngItemTop - lngFrameMain_SC_Top
            'FrameMain.Scroll , Diffy
            'lngFrameMain_SC_Top = lngFrameMain_SC_Top = Diffy
            lngFrameMain_SC_Top = lngItemTop - frmSp
            FrameMain.ScrollTop = lngFrameMain_SC_Top
        ElseIf (frmSp + lngItemBottom) > (lngFrameMain_SC_Top + FrameMain.Height) Then
            DiffY = (frmSp + lngItemBottom) - (lngFrameMain_SC_Top + FrameMain.Height)
            'FrameMain.Scroll yAction:=CInt(Diffy)
            lngFrameMain_SC_Top = lngFrameMain_SC_Top + DiffY
            FrameMain.ScrollTop = lngFrameMain_SC_Top
        End If
    End Sub


    Private Sub focusListener_ChangeFocus(ByVal gotFocus As Boolean)
        Dim tn As String, AC As Chart
        If gotFocus Then
            'Debug.Print "Gained Focus"
            'tn = TypeName(selection)
            'CopyButton.Enabled = IIf(tn = "Series", True, False)
            'On Error Resume Next
            '    AC = ActiveChart
            'On Error GoTo 0
            'If AC Is Nothing Then
            '    PasteButton.Enabled = False
            'Else
            '    PasteButton.Enabled = readyToPaste 'TRUE if curve has been copied
            'End If
        Else
            Debug.Print "Lost Focus"
'        'GoingAway
            AAA
        End If
    End Sub

    Private Sub UserForm_Error(ByVal Number As Integer, ByVal Description As MSForms.ReturnString, ByVal SCode As Long, ByVal Source As String, ByVal HelpFile As String, ByVal HelpContext As Long, ByVal CancelDisplay As MSForms.ReturnBoolean)

    End Sub

    Private Sub UserForm_Initialize()
        Dim lngPreviousHeight As Long
        Dim lngHeightDifference As Long
        Dim items As [Items]
        Dim objItem As Object
        Dim oMail As MailItem
        Dim colTemp As Collection
        Dim intSort() As Integer
        Dim i As Integer
        Dim j As Integer
        Dim objPropertyNew As [UserProperty]
        Dim objPropertyExisting As [UserProperty]

        'Procedure Naming
        Dim SubNm As String
        SubNm = "QuickFileDyn.Initialize"

        '************Standard Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim ttrace As String
        Dim Tempv As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
        ttrace = "Inside " & SubNm

        '*******************END Error Header*********************


        blSuppressEvents = True                                     'Suppress events until the form is initialized
        InitType = InitSort
        folderCurrent = Application.ActiveExplorer.CurrentFolder
        lngFrameMain_SC_Top = 0

        Height_UserForm_Min = Me.Height + frmHt + frmSp
        Height_FrameMain_Min = frmHt + frmSp

        lngHeightDifference = Height_UserForm_Min - Me.Height

        'CommandButton1.top = CommandButton1.top + lngHeightDifference
        Button_OK.Top = Button_OK.Top + lngHeightDifference
        BUTTON_CANCEL.Top = BUTTON_CANCEL.Top + lngHeightDifference
        Button_Undo.Top = Button_Undo.Top + lngHeightDifference
        lngAcceleratorDialogueTop = AcceleratorDialogue.Top + lngHeightDifference
        AcceleratorDialogue.Top = lngAcceleratorDialogueTop
        spn_EmailPerLoad.Top = spn_EmailPerLoad.Top + lngHeightDifference
        lngTop_spn_Min = spn_EmailPerLoad.Top
        lngAcceleratorDialogueLeft = AcceleratorDialogue.Left
        lbl_EmailPerLoad.Top = lbl_EmailPerLoad.Top + lngHeightDifference
        lngTop_lbl_EmailPerLoad_Min = lbl_EmailPerLoad.Top
        lng_lbl_EmailPerLoad_left = lbl_EmailPerLoad.Left


        lngTop_OK_BUTTON_Min = Button_OK.Top
        lngTop_CANCEL_BUTTON_Min = BUTTON_CANCEL.Top
        lngTop_UNDO_BUTTON_Min = Button_Undo.Top
        'lngTop_CommandButton1_Min = CommandButton1.top
        lngTop_AcceleratorDialogue_Min = AcceleratorDialogue.Top

        'MsgBox "App Width " & Me.Width & vbCrLf & "Screen Width " & ScreenWidth * PointsPerPixel

        Height_UserForm_Max = ScreenHeight * PointsPerPixel * 0.85

        lngPreviousHeight = Me.Height
        Me.Height = Height_UserForm_Max
        lngHeightDifference = Me.Height - lngPreviousHeight

        'CommandButton1.top = CommandButton1.top + lngHeightDifference
        Button_OK.Top = Button_OK.Top + lngHeightDifference
        BUTTON_CANCEL.Top = BUTTON_CANCEL.Top + lngHeightDifference
        Button_Undo.Top = Button_Undo.Top + lngHeightDifference
        lngAcceleratorDialogueTop = AcceleratorDialogue.Top + lngHeightDifference
        AcceleratorDialogue.Top = lngAcceleratorDialogueTop
        lngAcceleratorDialogueLeft = AcceleratorDialogue.Left
        spn_EmailPerLoad.Top = spn_EmailPerLoad.Top + lngHeightDifference
        lbl_EmailPerLoad.Top = lbl_EmailPerLoad.Top + lngHeightDifference

        Height_FrameMain_Max = FrameMain.Height + lngHeightDifference
        FrameMain.Height = Height_FrameMain_Max
        FrameMain.ScrollHeight = Height_FrameMain_Max
        FrameMain.ZOrder 0

    'CommandButton1.TabStop = False
        Button_OK.TabStop = False
        BUTTON_CANCEL.TabStop = False
        Button_Undo.TabStop = False
        FrameMain.TabStop = False
        AcceleratorDialogue.TabStop = True
        spn_EmailPerLoad.TabStop = False


        'Lets find the UserForm Handle the function below retrieves the handle
        'to the top-level window whose class name ("ThunderDFrame" for Excel)
        'and window name (me.caption or UserformName caption) match the specified strings.
        lFormHandle = FindWindow("ThunderDFrame", Me.Caption)
        'OlApp_hWnd = FindWindow(olAppCLSN, vbNullString) 'Grabs handle on everything including vba
        'Dim OlApp_hWnd2 As LongPtr
        'OlApp_hWnd2 = GetAncestor(lFormHandle, GA_ROOTOWNER)
        OlApp_hWnd = GetAncestor(lFormHandle, GA_PARENT)


        'Debug.Print "lFormHandle " & lFormHandle
        'Debug.Print "OlApp_hWnd " & OlApp_hWnd
        'Debug.Print "OlApp_hWnd2 " & OlApp_hWnd2
        'Stop
        'The GetWindowLong function retrieves information about the specified window.
        'The function also retrieves the 32-bit (long) value at the specified offset
        'into the extra window memory of a window.
        lStyle = GetWindowLong(lFormHandle, GWL_STYLE)
        'lStyle is the New window style so lets set it up with the following
        'lStyle = lStyle Or WS_SYSMENU 'SystemMenu
        lStyle = lStyle Or WS_THICKFRAME 'Resizeable
        lStyle = lStyle Or WS_MINIMIZEBOX 'With MinimizeBox
        lStyle = lStyle Or WS_MAXIMIZEBOX 'and MaximizeBox

        'Now lets set up our New window the SetWindowLong function changes
        'the attributes of the specified window , given as lFormHandle,
        'GWL_STYLE = New windows style, and our Newly defined style = lStyle
        SetWindowLong lFormHandle, GWL_STYLE, (lStyle)

    'Remove >'&LT; if you want to show form Maximised
        'ShowWindow lFormHandle, SW_SHOWMAXIMIZED 'Shows Form Maximized

        'The DrawMenuBar function redraws the menu bar of the specified window.
        'We need this as we have changed the menu bar after Windows has created it.
        'All we need is the Handle.
        DrawMenuBar lFormHandle
    'blShowAsConversations
        blShowAsConversations = AreConversationsGrouped

        'If blShowAsConversations Then
        '    'ToggleShowAsConversation -1
        '    ExplConvView_ToggleOff
        '    DoEvents
        'End If

        'Initialize Folder Suggestions and calculate emails per page

        Folder_Suggestions_Reload
        blSuppressEvents = False
        ShowWindow lFormHandle, SW_SHOWMAXIMIZED
    blSuppressEvents = True
        intEmailStart = 0       'Reverse sort is 0   'Regular sort is 1
        intEmailPosition = 0    'Reverse sort is 0   'Regular sort is 1
        'intEmailsPerIteration = CInt(Round((Height_FrameMain_Max / (frmHt + frmSp)), 0))
        intEmailsPerIteration = CInt(Round((FrameMain.Height / (frmHt + frmSp)), 0))
        spn_EmailPerLoad.Value = intEmailsPerIteration
        lbl_EmailPerLoad.Caption = intEmailsPerIteration

        '***********************************************************************************
        '************New code to listen for window lost focus*******************************
        '    ' Set our event extender
        '    focusListener = New FormFocusListener
        '    'subclass the userform to catch WM_NCACTIVATE msgs
        '    #If VBA7 Then
        '        Dim lhWnd As LongPtr
        '        lhWnd = FindWindow("ThunderDFrame", Me.Caption)
        '        lPrevWnd = SetWindowLongPtr(lhWnd, GWL_WNDPROC, AddressOf myWindowProc)
        '    #Else
        '        Dim lhWnd As Long
        '        lhWnd = FindWindow("ThunderDFrame", Me.Caption)
        '        lPrevWnd = SetWindowLong(lhWnd, GWL_WNDPROC, AddressOf myWindowProc)
        '    #End If
        '**********************End listen code**********************************************
        '***********************************************************************************

        'Modal    SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
        EnableWindow OlApp_hWnd, Modal                                   'Make Window Modal
        EnableWindow lFormHandle, Modeless
    blSuppressEvents = False                                        'End suppression of events


        '************Standard Error Handling Footer**************
        On Error Resume Next
        Tempv = SF_Stack.Pop
        Exit Sub

ErrorHandler:
        ExplConvView_Cleanup()
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "END BREAK - PROCEDURE COMMANDS OUTPUT. RESUME PROCEDURE TRACING"
    Debug.Print ttrace
    errRaised = True
        Deactivate_Email_Timing_And_Velocity
        Tracing_WRITE
        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            Stop
            Resume
        End If

        '*******************END Standard Error Footer*********************


    End Sub



    Private Sub UserForm_Resize()
        Dim intDiffy As Integer
        Dim intDiffx As Integer
        Dim intChgx As Integer
        Dim i As Integer
        Dim QF As QfcController

        'MsgBox "App Width " & Me.Width & vbCrLf & "Screen Width " & ScreenWidth * PointsPerPixel
        If Not blSuppressEvents Then

            If Me.Width >= Width_UserForm - 100 Then
                intDiffx = Me.Width - Width_UserForm
            Else
                intDiffx = 0
            End If

            If Me.Height >= Height_UserForm_Min Then
                intDiffy = Me.Height - Height_UserForm_Min
            Else
                intDiffy = 0
            End If

            FrameMain.Width = Width_FrameMain + intDiffx
            FrameMain.Height = Height_FrameMain_Min + intDiffy

            Button_OK.Top = lngTop_OK_BUTTON_Min + intDiffy
            Button_OK.Left = OK_left + intDiffx / 2
            BUTTON_CANCEL.Top = lngTop_CANCEL_BUTTON_Min + intDiffy
            BUTTON_CANCEL.Left = Button_OK.Left + CANCEL_left - OK_left
            Button_Undo.Top = lngTop_UNDO_BUTTON_Min + intDiffy
            Button_Undo.Left = Button_OK.Left + UNDO_left - OK_left
            'CommandButton1.top = lngTop_CommandButton1_Min + intDiffy
            AcceleratorDialogue.Top = lngTop_AcceleratorDialogue_Min + intDiffy
            spn_EmailPerLoad.Top = lngTop_spn_Min + intDiffy
            spn_EmailPerLoad.Left = spn_left + intDiffx
            lbl_EmailPerLoad.Top = lngTop_lbl_EmailPerLoad_Min + intDiffy
            lbl_EmailPerLoad.Left = lng_lbl_EmailPerLoad_left + intDiffx

            If Not colQFClass Is Nothing Then
                For i = 1 To colQFClass.Count
                    QF = colQFClass(i)
                    If QF.blConChild Then
                        QF.frm.Left = frmLt * 2
                        QF.frm.Width = Width_frm + intDiffx - frmLt
                        QF.ResizeCtrls intDiffx - frmLt
            Else
                        QF.frm.Width = Width_frm + intDiffx
                        QF.ResizeCtrls intDiffx
            End If
                Next i

                QF = Nothing
            End If

        End If 'blSupressEvents

    End Sub


    Private Sub UserForm_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        If Not blSuppressEvents Then KeyPressHandler KeyAscii
End Sub

    Private Sub UserForm_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        If Not blSuppressEvents Then KeyUpHandler KeyCode, Shift
End Sub

    Private Sub UserForm_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Dim DebugLVL As DebugLevelEnum
        DebugLVL = vbProcedure

        'If DebugLVL And vbProcedure Then Debug.Print "Fired UserForm_KeyDown"
        If Not blSuppressEvents Then KeyDownHandler KeyCode, Shift
End Sub


    Public Sub KeyPressHandler(ByVal KeyAscii As MSForms.ReturnInteger)
        If Not blSuppressEvents Then
            Dim vbMsgResponse As VbMsgBoxResult

            Select Case KeyAscii
                Case vbKeyReturn
                    Button_OK_Click()
                Case vbKeyTab
                    toggleAcceleratorDialogue()
                    If AcceleratorDialogue.Visible Then AcceleratorDialogue.SetFocus
                    '        Case vbKeyEscape
                    '            vbMsgResponse = MsgBox("Stop all filing actions and close quick-filer?", vbOKCancel)
                    '            If vbMsgResponse = vbOK Then BUTTON_CANCEL_Click

                Case Else
            End Select
        End If
    End Sub

    Public Sub KeyUpHandler(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        If Not blSuppressEvents Then
            Select Case KeyCode
                Case 18
                    If AcceleratorDialogue.Visible Then
                        AcceleratorDialogue.SetFocus
                        AcceleratorDialogue.SelStart = AcceleratorDialogue.TextLength
                    Else
                        FrameMain.SetFocus
                    End If
                    SendKeys "{ESC}"
        Case vbKeyUp
                    If AcceleratorDialogue.Visible Then AcceleratorDialogue.SetFocus
                Case vbKeyDown
                    If AcceleratorDialogue.Visible Then AcceleratorDialogue.SetFocus
                Case Else
            End Select
        End If
    End Sub

    Public Sub KeyDownHandler(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        'Procedure Naming
        Dim SubNm As String
        SubNm = "KeyDownHandler"

        '************Standard Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim ttrace As String
        Dim Temp As Variant

        If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
        If TraceStack Is Nothing Then TraceStack = New cStackGeneric

        SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
        ttrace = "Inside " & SubNm

        '*******************END Error Header*********************

        Dim strTemp As String
        Dim DebugLVL As DebugLevelEnum
        DebugLVL = vbProcedure + vbCommand




        'If DebugLVL And vbProcedure Then Debug.Print "Fired KeyDownHandler " & KeyCode

        strTemp = "If Not blSuppressEvents Then"
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
If Not blSuppressEvents Then
            strTemp = "If Not blSuppressEvents Then IS TRUE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "Select Case KeyCode"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
    Select Case KeyCode
                Case 18
                    strTemp = "Case 18"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "toggleAcceleratorDialogue"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            toggleAcceleratorDialogue()
                    strTemp = "If AcceleratorDialogue.Visible Then"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If AcceleratorDialogue.Visible Then
                        strTemp = "If AcceleratorDialogue.Visible Then IS TRUE"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "AcceleratorDialogue.SetFocus"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                AcceleratorDialogue.SetFocus

                    Else
                        strTemp = "If AcceleratorDialogue.Visible Then IS FALSE"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "FrameMain.SetFocus"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                FrameMain.SetFocus
                    End If
                Case Else
                    strTemp = "KeyDownHandler Case = Else"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "If AcceleratorDialogue.Visible Then"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If AcceleratorDialogue.Visible Then
                        strTemp = "AcceleratorDialogue.Visible IS TRUE"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "AcceleratorDialogue_KeyDown KeyCode, Shift"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                AcceleratorDialogue_KeyDown KeyCode, Shift
            Else
                        strTemp = "AcceleratorDialogue.Visible IS FALSE"
                        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            End If
            End Select
        End If
        strTemp = "End of KeyDownHandler " & KeyCode
        'If DebugLVL And vbProcedure Then Debug.Print "End of KeyDownHandler " & KeyCode
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

'************Standard Error Handling Footer**************
        On Error Resume Next
        Temp = SF_Stack.Pop
        Exit Sub

ErrorHandler:
        ExplConvView_Cleanup()
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "END BREAK - PROCEDURE COMMANDS OUTPUT. RESUME PROCEDURE TRACING"
    Debug.Print ttrace
    errRaised = True
        Deactivate_Email_Timing_And_Velocity
        Tracing_WRITE
        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        Stop
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            Resume Next
        End If

        '*******************END Standard Error Footer*********************


    End Sub

    Public Sub QFD_Minimize()


        'Lets find the UserForm Handle the function below retrieves the handle
        'to the top-level window whose class name ("ThunderDFrame" for Excel)
        'and window name (me.caption or UserformName caption) match the specified strings.
        lFormHandle = FindWindow("ThunderDFrame", Me.Caption)

        'EnableWindow lFormHandle, Modal
        If Not StopWatch Is Nothing Then
            If StopWatch.isPaused = False Then
                StopWatch.Pause()
            End If
        End If
        EnableWindow OlApp_hWnd, Modeless
    'EnableWindow lFormHandle, Modeless
        ShowWindow lFormHandle, SW_FORCEMINIMIZE
    ShowWindow lFormHandle, SW_FORCEMINIMIZE

End Sub

    Public Sub QFD_Maximize()


        'Lets find the UserForm Handle the function below retrieves the handle
        'to the top-level window whose class name ("ThunderDFrame" for Excel)
        'and window name (me.caption or UserformName caption) match the specified strings.
        lFormHandle = FindWindow("ThunderDFrame", Me.Caption)

        ShowWindow lFormHandle, SW_SHOWMAXIMIZED
'Modal    SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
        EnableWindow OlApp_hWnd, Modal
    'EnableWindow lFormHandle, Modeless


    End Sub

    Public Sub ExplConvView_Cleanup()

        On Error Resume Next
        objView = ActiveExplorer.CurrentFolder.Views(objView_Mem)
        If Err.Number = 0 Then
            'objView.Reset
            objView.Apply()
            If Not objViewTemp Is Nothing Then objViewTemp.Delete()
            blShowInConversations = False
        Else
            Err.Clear()
            objViewTemp = ActiveExplorer.CurrentView.Parent("tmpNoConversation")
            If Not objViewTemp Is Nothing Then objViewTemp.Delete()
        End If
    End Sub
    Public Sub ExplConvView_ToggleOff()
        'Procedure Naming
        Dim SubNm As String
        SubNm = "ExplConvView_ToggleOff"

        '************ALTERED Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim ttrace As String
        Dim Temp As Variant
        Dim DebugLVL As DebugLevelEnum

        DebugLVL = vbProcedure + vbCommand

        If DebugLVL And vbProcedure Then
            If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
            If TraceStack Is Nothing Then TraceStack = New cStackGeneric

            SF_Stack.Push SubNm
        strSubs = SF_Stack.GetString(True)
            TraceStack.Push strSubs

        SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
            ttrace = "Inside " & SubNm
        End If

        '*******************END Error Header*********************

        Dim strTemp As String

        strTemp = "If ActiveExplorer.CommandBars.GetPressedMso('ShowInConversations') Then"
        If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

    If ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations") Then
            strTemp = "If ActiveExplorer.CommandBars.GetPressedMso('ShowInConversations') Then IS TRUE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

                                                                                                    strTemp = "blShowInConversations = True"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

        blShowInConversations = True
            strTemp = "objView = ActiveExplorer.CurrentView"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

        objView = ActiveExplorer.CurrentView

            strTemp = "If objView.Name = 'tmpNoConversation' Then"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If objView.Name = "tmpNoConversation" Then

                strTemp = "If objView.Name = 'tmpNoConversation' Then IS TRUE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "If ActiveExplorer.CommandBars.GetPressedMso('ShowInConversations') Then"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            If ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations") Then
                    strTemp = "If ActiveExplorer.CommandBars.GetPressedMso('ShowInConversations') Then IS TRUE"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "objView.XML = Replace(objView.XML, '<upgradetoconv>1</upgradetoconv>', '', 1, , vbTextCompare)"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                objView.XML = Replace(objView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, , vbTextCompare)
                    strTemp = "objView.Save"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                objView.Save()
                    'objView.Reset
                    strTemp = "objView.Apply"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                objView.Apply()

                    strTemp = "End If 'objView.Name = 'tmpNoConversation' Then"
                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            End If
            Else
                strTemp = "If objView.Name = 'tmpNoConversation' Then IS FALSE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        End If
            strTemp = "End If 'objView.Name = 'tmpNoConversation' Then"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp


                                                                                                    strTemp = "objView_Mem = objView.Name"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        objView_Mem = objView.Name
            strTemp = "If objView_Mem = 'tmpNoConversation' Then objView_Mem = View_Wide"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If objView_Mem = "tmpNoConversation" Then objView_Mem = View_Wide
            strTemp = "If objView_Mem = 'tmpNoConversation' Then objView_Mem = View_Wide IS TRUE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        On Error Resume Next
            strTemp = "objViewTemp = objView.Parent('tmpNoConversation')"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        objViewTemp = objView.Parent("tmpNoConversation")
            strTemp = "If objViewTemp Is Nothing Then"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If objViewTemp Is Nothing Then
                strTemp = "If objViewTemp Is Nothing Then IS TRUE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "objViewTemp = objView.Copy('tmpNoConversation', olViewSaveOptionThisFolderOnlyMe)"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            objViewTemp = objView.Copy("tmpNoConversation", olViewSaveOptionThisFolderOnlyMe)
                strTemp = "objViewTemp.XML = Replace(objView.XML, '<upgradetoconv>1</upgradetoconv>', '', 1, , vbTextCompare)"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            objViewTemp.XML = Replace(objView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, , vbTextCompare)
                strTemp = "objViewTemp.Save"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    If DebugLVL And vbVariable Then TraceStack.Push objViewTemp.XML
            objViewTemp.Save()
            Else
                strTemp = "If objViewTemp Is Nothing Then IS FALSE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        End If
            strTemp = "End If 'objViewTemp Is Nothing Then IS FALSE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

        On Error GoTo ErrorHandler
            'objViewTemp.Reset
            strTemp = "objViewTemp.Apply"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        objViewTemp.Apply()

            If DebugLVL And vbVariable Then
                TraceStack.Push "objViewTemp Variable Details"
            TraceStack.Push objViewTemp.XML
        End If
            strTemp = "If blSuppressEvents Then"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        If blSuppressEvents Then
                strTemp = "If blSuppressEvents Then IS TRUE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "DoEvents"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            DoEvents
            Else
                strTemp = "If blSuppressEvents Then IS FALSE"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                                                                                    strTemp = "blSuppressEvents = True"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            blSuppressEvents = True
                strTemp = "DoEvents"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            DoEvents
                strTemp = "blSuppressEvents = False"
                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            blSuppressEvents = False
            End If
            strTemp = "End If 'blSuppressEvents Then"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp

'Modal                                                                                                    strTemp = "SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&"
            'Modal                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Modal        SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&

        Else
            strTemp = "If ActiveExplorer.CommandBars.GetPressedMso('ShowInConversations') Then IS FALSE"
            If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
    End If

        '************ALTERED Error Handling Footer**************
        On Error Resume Next
        If DebugLVL And vbProcedure Then Temp = SF_Stack.Pop
        Exit Sub

ErrorHandler:
        ExplConvView_Cleanup()
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "END BREAK - PROCEDURE COMMANDS OUTPUT. RESUME PROCEDURE TRACING"
    Debug.Print ttrace
    errRaised = True
        Deactivate_Email_Timing_And_Velocity
        Tracing_WRITE
        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)

        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            Stop
            Resume
        End If

        '*******************END Standard Error Footer*********************


    End Sub

    Public Sub ExplConvView_ToggleOn()

        If blShowInConversations Then
            objView = ActiveExplorer.CurrentFolder.Views(objView_Mem)
            'objView.Reset
            objView.Apply()
            'objViewTemp.Delete
            blShowInConversations = False
        End If

    End Sub


    Private Sub UserForm_Terminate()
        If blShowAsConversations Then ExplConvView_ToggleOn()
        'ToggleShowAsConversation 1
    End Sub


    Private Sub QuickFileMetrics_WRITE(filename As String, Optional FileWriteType As Integer = 8)
        Dim tmpDebugLevel As DebugLevelEnum

        Dim SubNm As String
        SubNm = "QuickFileDyn.QuickFileMetrics_WRITE"
        'tmpDebugLevel = DebugLevel


        '************ALTERED Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim ttrace As String
        Dim Temp As Variant
        Dim DebugLVL As DebugLevelEnum

        DebugLVL = vbProcedure '+ vbCommand + vbVariable

        If DebugLVL And vbProcedure Then
            If SF_Stack Is Nothing Then SF_Stack = New cStackGeneric
            If TraceStack Is Nothing Then TraceStack = New cStackGeneric

            SF_Stack.Push SubNm
        strSubs = SF_Stack.GetString(True)
            TraceStack.Push strSubs

        SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "
            ttrace = "Inside " & SubNm
        End If

        '*******************END Error Header*********************



        Dim objFSO As Object       ' Computer's file system object.
        Dim objShell As Object       ' Windows Shell application object.
        Dim LOC_TXT_FILE As String
        Dim a
        Dim i, j, k As Integer
        Dim curDateText, curTimeText, durationText, durationMinutesText As String
        Dim Duration As Double
        Dim dataLine As String
        Dim dataLineBeg As String
        Dim QF As QfcController
        Dim Elapsed As Double
        Dim OlEndTime As Date
        Dim OlStartTime As Date
        Dim infoMail As cInfoMail
        Dim OlAppointment As AppointmentItem
        Dim OlEmailCalendar As Folder
        Dim strOutput() As String

        'Create a line of comma seperated valued to store data
        curDateText = Format(Now(), "mm/dd/yyyy")
        'If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable curDateText = " & curDateText

        curTimeText = Format(Now(), "hh:mm")
        'If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable curTimeText = " & curTimeText

        dataLineBeg = curDateText & "," & curTimeText & ","

        'Append it to the text file
        objShell = CreateObject("Shell.Application")
        objFSO = CreateObject("Scripting.FileSystemObject")



        LOC_TXT_FILE = FileSystem_MyD & filename
        'If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable LOC_TXT_FILE = " & LOC_TXT_FILE
        'If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable FileWriteType = " & FileWriteType

        ' If DebugLVL And vbCommand Then Debug.Print SubNm & " If FileWriteType = 8 Then "
        '    If FileWriteType = 8 Then
        '        a = objFSO.OpenTextFile(LOC_TXT_FILE, FileWriteType, 0)
        '    Else
        '        a = objFSO.CreateTextFile(LOC_TXT_FILE, True)
        '    End If

        Duration = StopWatch.timeElapsed
        OlEndTime = Now()
        OlStartTime = DateAdd("S", -Duration, OlEndTime)

        If colQFClass.Count > 0 Then
            Duration = Duration / colQFClass.Count
        End If

        durationText = Format(Duration, "##0")
        'If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

        durationMinutesText = Format(Duration / 60, "##0.00")

        'dataLineBeg = dataLineBeg & durationText & "," & durationMinutesText & ","

        infoMail = New cInfoMail
        OlEmailCalendar = GetCalendar("Email Time")
        OlAppointment = OlEmailCalendar.Items.Add(olAppointmentItem)
        With OlAppointment
            .Subject = "Quick Filed " & colQFClass.Count & " emails"
            .Start = OlStartTime
            .End = OlEndTime
            .Categories = "@ Email"
            .ReminderSet = False
            .Sensitivity = olPrivate
            .Save()
        End With

        ReDim strOutput(colQFClass.Count)
        For k = 1 To colQFClass.Count
            QF = colQFClass(k)
            'If Mail_IsItEncrypted(QF.mail) = False Then
            On Error Resume Next
            If infoMail.Init_wMail(QF.Mail, OlEndTime:=OlEndTime, lngDurationSec:=CLng(Duration)) Then
                If OlAppointment.Body = "" Then
                    OlAppointment.Body = infoMail.ToString
                    OlAppointment.Save
                Else
                    OlAppointment.Body = OlAppointment.Body & vbCrLf & infoMail.ToString
                    OlAppointment.Save
                End If
            End If
            dataLine = dataLineBeg & xComma(QF.lblSubject.Caption)
            dataLine = dataLine & "," & "QuickFiled"
            dataLine = dataLine & "," & durationText
            dataLine = dataLine & "," & durationMinutesText
            dataLine = dataLine & "," & xComma(QF.strlblTo)
            dataLine = dataLine & "," & xComma(QF.lblSender.Caption)
            dataLine = dataLine & "," & "Email"
            dataLine = dataLine & "," & xComma(QF.cbo.Value)           'Target Folder
            dataLine = dataLine & "," & QF.lblSentOn
            dataLine = dataLine & "," & Format(QF.Mail.SentOn, "hh:mm")
            'If DebugLVL And vbCommand Then Debug.Print SubNm & " dataline = " & dataLine
            strOutput(k) = dataLine
            '        a.WriteLine (dataLine)
            On Error GoTo ErrorHandler
            'Add to Email Calendar



            'End If

        Next k

        Write_TextFile filename, strOutput, FileSystem_MyD
'    a.Close

        '************ALTERED Error Handling Footer**************
        On Error Resume Next
        If DebugLVL And vbProcedure Then Temp = SF_Stack.Pop
        Exit Sub

ErrorHandler:
        ExplConvView_Cleanup()
        SF_Stack.Push "ErrorHandler: " & SubNm
    TraceStack.Push SF_Stack.GetString(True)
    TraceStack.Push "Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source
    TraceStack.Push "BREAK - PROCEDURE COMMANDS EXECUTED BEFORE ERROR:"
    TraceStack.Push ttrace
    TraceStack.Push "END BREAK - PROCEDURE COMMANDS OUTPUT. RESUME PROCEDURE TRACING"
    Debug.Print ttrace
    errRaised = True
        Deactivate_Email_Timing_And_Velocity
        Tracing_WRITE
        errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)
        Stop
        errcapt = MsgBox("What should happen next?", vbRetryCancel + vbExclamation)
        If errcapt = vbCancel Then
            'Resume PROC_EXIT
        Else
            reactivateAfterDebug
            Err.Clear()
            Stop
            Resume
        End If

        '*******************END Standard Error Footer*********************



    End Sub


    Private Function xComma(ByVal str As String) As String
        Dim strTmp As String

        strTmp = Replace(str, ", ", "_")
        strTmp = Replace(strTmp, ",", "_")
        xComma = GetStrippedText(strTmp)
        'xComma = StripAccents(strTmp)
    End Function


End Class
