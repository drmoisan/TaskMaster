Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook


Public Class QfcController


    Private oParent As Object
    Private InitType As InitTypeEnum
    Private m_PassedControl As Control
    Public WithEvents chk As CheckBox         'Checkbox to Group Conversations
    Public WithEvents cbo As ComboBox         'Combo box containing Folder Suggestions
    Private WithEvents lst As ListBox
    Private WithEvents txt As TextBox          'Input for folder search
    Private WithEvents bdy As Label
    Private WithEvents cbKll As Button    'Remove mail from Processing
    Private WithEvents cbDel As Button    'Delete email
    Private WithEvents cbFlag As Button    'Flag as Task
    Private WithEvents cbTmp As Button
    Public WithEvents frm As Panel
    Public Mail As MailItem
    Private fldrOriginal As MAPIFolder
    Public intMyPosition As Integer
    Private fldrTarget As Folder
    Private lblTmp As Label
    Public lblConvCt As Label            'Count of Conversation Members
    Private lblMyPosition As Label            'ACCELERATOR Email Position
    Private suggestions As Email_AutoCategorize.suggestions
    Private strFolders() As String
    Private colCtrls As Collection
    Private selItems_InClass As Collection
    Private blAccel_FocusToggle As Boolean
    Private intEnterCounter As Integer
    Private intComboRightCtr As Integer
    Private Structure ctrlPosition
        Private blInOrigPos As Boolean
        Private topOriginal As Long
        Private topNew As Long
        Private leftOriginal As Long
        Private leftNew As Long
        Private heightOriginal As Long
        Private heightNew As Long
        Private widthOriginal As Long
        Private widthNew As Long
    End Structure

    Private chbxSaveAttach As CheckBox
    Private chbxSaveMail As CheckBox
    Private chbxDelFlow As CheckBox

    Public blExpanded As Boolean
    Public blConChild As Boolean

    Private lbl1 As Label            'From:
    Private lbl2 As Label            'Subject:
    Private lbl3 As Label            'Body:
    Private lbl4 As Label            'Sent On:
    Private lbl5 As Label            'Folder:

    Public lblSender As Label            '<SENDER>
    Public lblSubject As Label            '<SUBJECT>
    Public lblBody As Label            '<BODY>
    Public strlblTo As String                   '<TO>




    Public lblSentOn As Label            '<SENTON>
    Private lblSentOn_Left As Long                     'SentOn X% Left Position


    Public lblTriage As Label            'X as Triage placeholder
    Private lblTriage_Width As Long                     'Triage Width
    Private lblTriage_Left As Long                     'Triage Left

    Public lblActionable As Label            '<ACTIONABL>
    Private lblActionable_Width As Long                     '<ACTIONABL> Width
    Private lblActionable_Left As Long                     '<ACTIONABL> Left



    Private lblAcF As Label            'ACCELERATOR F for Folder Search
    Private lblAcD As Label            'ACCELERATOR D for Folder Dropdown
    Private lblAcC As Label            'ACCELERATOR C for Grouping Conversations
    Private lblAcX As Label            'ACCELERATOR X for Delete email
    Private lblAcR As Label            'ACCELERATOR R for remove item from list
    Private lblAcT As Label            'ACCELERATOR T for Task ... Flag item and make it a task
    Private lblAcO As Label            'ACCELERATOR O for Open Email
    Private lblAcA As Label            'ACCELERATOR A for Save Attachments
    Private lblAcW As Label            'ACCELERATOR W for Delete Flow
    Private lblAcM As Label            'ACCELERATOR M for Save Mail

    Private lblSubject_Width As Long
    Private lblBody_Width As Long                     'Body Width

    Private cbFlag_Left As Long                     'Task button X% left position
    Private lblAcT_Left As Long                     'Task accelerator X% left position
    Private lbl5_Left As Long                     'Folder label X% left position
    Private txt_Left As Long                     'Folder search box X% left position Y% Width
    Private txt_Width As Long                     'Folder search box X% left position Y% Width
    Private lblAcF_Left As Long                     'F Accelerator X% left position
    Private lblAcD_Left As Long                     'D Accelerator X% left position
    Private cbo_Left As Long                     'Dropdown box X% Left position Y% Width
    Private cbo_Width As Long                     'Dropdown box X% Left position Y% Width
    Private cbDel_Left As Long                     'Delete button X+Y% Left position
    Private cbKll_Left As Long
    Private lblAcX_Left As Long
    Private lblAcR_Left As Long
    Private lblAcC_Left As Long                     'Conversation accelerator X% Left position
    Private chk_Left As Long                     'Conversation checkbox X% Left Position
    Private lblConvCt_Left As Long                     'Conversation Count X% Left Position

    Private chbxSaveAttach_Left As Long                     'Checkbox Save Attachment X% Left Position
    Private chbxSaveMail_Left As Long                     'Checkbox Save Mail X% Left Position
    Private chbxDelFlow_Left As Long                     'Checkbox Delete Flow X% Left Position
    Private lblAcA_Left As Long                     'A Accelerator X% Left Position
    Private lblAcW_Left As Long                     'W Accelerator X% Left Position
    Private lblAcM_Left As Long                     'M Accelerator X% Left Position
    Private lngBlock_Width As Long                     'Width of block of controls that need to be right justified

    Private pos_frm As ctrlPosition
    Private pos_cbo As ctrlPosition
    Private pos_chk As ctrlPosition
    Private pos_body As ctrlPosition
    Private pos_lblAcC As ctrlPosition
    Private pos_lblAcD As ctrlPosition
    Private pos_lblAcO As ctrlPosition

    Private pos_chbxSaveAttach As ctrlPosition                     'Checkbox Save Attachment X% Left Position
    Private pos_chbxSaveMail As ctrlPosition                     'Checkbox Save Mail X% Left Position
    Private pos_chbxDelFlow As ctrlPosition                     'Checkbox Delete Flow X% Left Position
    Private pos_lblAcA As ctrlPosition                     'A Accelerator X% Left Position
    Private pos_lblAcW As ctrlPosition                     'W Accelerator X% Left Position
    Private pos_lblAcM As ctrlPosition                     'M Accelerator X% Left Position
    Private fldrHandler As cFolderHandler
    Private hWndCaller As LongPtr

    Private p_BoolRemoteMouseApp As Boolean
    Private conv As cConversation


    Friend Sub InitCtrls(m_mail As mailItem,
        col As Collection,
        intPositionArg As Integer,
        BoolRemoteMouseApp As Boolean,
        Caller As Object,
        Optional hwnd As LongPtr,
        Optional InitTypeE As InitTypeEnum = InitSort)

        'Procedure Naming
        Dim SubNm As String
        SubNm = "InitCtrls"

        '************Standard Error Handling Header**************
        On Error GoTo ErrorHandler

        Dim errcapt As Variant
        Dim errRaised As Boolean
        Dim ttrace As String
        Dim Temp As Variant
        Dim DebugLVL As DebugLevelEnum

        DebugLVL = vbCommand

        ErrorHandlingCode.ErrHandler_Init SubNm, ttrace, DebugLVL

'*******************END Error Header*********************

        Dim ctlTmp As MSForms.control
        Dim i As Integer
        Dim strBodyText As String
        Dim Sel As Collection

        InitType = InitTypeE
    Set oParent = Caller
    intMyPosition = intPositionArg        'call back position in collection
    Set Mail = m_mail
    Set fldrOriginal = Mail.Parent
    hWndCaller = hwnd
    Set colCtrls = col
    For Each ctlTmp In col
            Select Case TypeName(ctlTmp)
                Case "Frame"
                Set frm = ctlTmp
            Case "CheckBox"
                    Select Case ctlTmp.Caption
                        Case "  Conversation"
                        Set chk = ctlTmp
                    Case " Attach"
                        Set chbxSaveAttach = ctlTmp
                    Case " Flow"
                        Set chbxDelFlow = ctlTmp
                    Case " Mail"
                        Set chbxSaveMail = ctlTmp
                End Select
                Case "ComboBox"
                Set cbo = ctlTmp
            Case "ListBox"
                Set lst = ctlTmp
            Case "OptionButton"
                Set opt = ctlTmp
            Case "SpinButton"
                Set spn = ctlTmp
            Case "TextBox"
                Set txt = ctlTmp
            Case "Label"
                Set lblTmp = ctlTmp
                Select Case lblTmp.Caption
                        Case "From:"
                        Set lbl1 = lblTmp
                    Case "Subject:"
                        Set lbl2 = lblTmp
                    Case "Body:"
                        Set lbl3 = lblTmp
                    Case "Sent On:"
                        Set lbl4 = lblTmp
                    Case "Folder:"
                        Set lbl5 = lblTmp
                    Case "<SENDER>"
                            If Mail.Sent = True Then
                                lblTmp.Caption = Mail.Sender
                            Else
                                lblTmp.Caption = "Draft Message"
                            End If
                        Set lblSender = lblTmp
                    Case "<SUBJECT>"
                            lblTmp.Caption = Mail.Subject
                        Set lblSubject = lblTmp
                    Case "ABC"
                            lblTmp.Caption = CustomFieldID_GetValue(Mail, "Triage")
                        Set lblTriage = lblTmp
                    Case "<ACTIONABL>"
                            lblTmp.Caption = CustomFieldID_GetValue(Mail, "Actionable")
                        Set lblActionable = lblTmp
                    Case "<#>"
                        Set lblConvCt = lblTmp
                    Case "<Pos#>"
                        Set lblMyPosition = lblTmp
                    Case "<BODY>"
                            strBodyText = Replace(Mail.Body, vbCrLf, " ")
                            strBodyText = Replace(strBodyText, "  ", " ")
                            strBodyText = Replace(strBodyText, "  ", " ") & "<EOM>"
                            lblTmp.Caption = strBodyText
                        Set bdy = lblTmp
                        Set lblBody = lblTmp
                    Case "<SENTON>"
                            lblTmp.Caption = Format(Mail.SentOn, "MM/DD/YY HH:MM")
                        Set lblSentOn = lblTmp
                    Case "F"
                        Set lblAcF = lblTmp
                        
                    Case "D"
                        Set lblAcD = lblTmp
                    Case "C"
                        Set lblAcC = lblTmp
                    Case "X"
                        Set lblAcX = lblTmp
                    Case "R"
                        Set lblAcR = lblTmp
                    Case "T"
                        Set lblAcT = lblTmp
                    Case "O"
                        Set lblAcO = lblTmp
                    Case "A"
                        Set lblAcA = lblTmp
                    Case "W"
                        Set lblAcW = lblTmp
                    Case "M"
                        Set lblAcM = lblTmp
                End Select
                Case "CommandButton"
                Set cbTmp = ctlTmp
                If cbTmp.Caption = "X" Then
                    Set cbDel = ctlTmp
                ElseIf cbTmp.Caption = "-->" Then
                    Set cbKll = ctlTmp
                ElseIf cbTmp.Caption = "|>" Then
                    Set cbFlag = ctlTmp
                End If
            End Select

        Next ctlTmp

        If Mail.UnRead = True Then
            lblSubject.ForeColor = &H800000
            lblSubject.Font.Bold = True
            lblSender.ForeColor = &H800000
            lblSender.Font.Bold = True
        End If
        lblSubject_Width = lblSubject.Width
        lblBody_Width = lblBody.Width
        cbFlag_Left = cbFlag.Left
        lblAcT_Left = lblAcT.Left

        lblTriage_Width = lblTriage.Width
        lblTriage_Left = lblTriage.Left
        lblActionable_Left = lblActionable.Left
        lblActionable_Width = lblActionable.Width


        cbDel_Left = cbDel.Left
        cbKll_Left = cbKll.Left
        lblAcX_Left = lblAcX.Left
        lblAcR_Left = lblAcR.Left


        lblSentOn_Left = lblSentOn.Left                 'SentOn X% Left Position



        If InitType And InitSort Then
            lbl5_Left = lbl5.Left
            lblAcF_Left = lblAcF.Left
            lblAcD_Left = lblAcD.Left
            cbo_Left = cbo.Left
            cbo_Width = cbo.Width
            lblAcC_Left = lblAcC.Left                       'Conversation accelerator X% Left position
            chk_Left = chk.Left                             'Conversation checkbox X% Left Position
            chbxSaveAttach_Left = chbxSaveAttach.Left       'Checkbox Save Attachment X% Left Position
            chbxSaveMail_Left = chbxSaveMail.Left           'Checkbox Save Mail X% Left Position
            chbxDelFlow_Left = chbxDelFlow.Left             'Checkbox Delete Flow X% Left Position
            lblAcA_Left = lblAcA.Left                       'A Accelerator X% Left Position
            lblAcW_Left = lblAcW.Left                       'W Accelerator X% Left Position
            lblAcM_Left = lblAcM.Left                       'M Accelerator X% Left Position
            txt_Left = txt.Left
            txt_Width = txt.Width
            lblConvCt_Left = lblConvCt.Left                 'Conversation Count X% Left Position
        End If

        lngBlock_Width = frm.Width - chbxSaveAttach_Left 'Width of block of right justified controls

        strlblTo = Mail.To

        If BoolRemoteMouseApp Then ToggleRemoteMouseAppLabels()

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

    errcapt = MsgBox("Error in " & SubNm & ": " & Err.Number & " -> " & Err.Description & " ->" & Err.Source, vbOKOnly + vbCritical)

        errcapt = MsgBox("What should happen next?", vbAbortRetryIgnore + vbExclamation)
        If errcapt = vbAbort Then
            End
            'Resume PROC_EXIT
        ElseIf errcapt = vbRetry Then
            reactivateAfterDebug
            Err.Clear()
            Stop
            Resume
        ElseIf errcapt = vbIgnore Then
            Err.Clear()
            Resume Next
        End If


    End Sub

    Friend Sub ToggleRemoteMouseAppLabels()
        p_BoolRemoteMouseApp = Not p_BoolRemoteMouseApp
        If p_BoolRemoteMouseApp Then

            lblAcX.Caption = "^-"       'ACCELERATOR X for Delete email
            lblAcX.Width = lblAcX.Width * 2
            lblAcR.Caption = "F3"       'ACCELERATOR R for remove item from list
            lblAcT.Caption = "F2"       'ACCELERATOR T for Task ... Flag item and make it a task
            lblAcO.Caption = "^0"       'ACCELERATOR O for Open Email
            lblAcO.Width = lblAcO.Width * 2
            lblAcM.Width = lblAcM.Width * 2
            If InitType And InitSort Then
                lblAcF.Caption = "F1"   'ACCELERATOR F for Folder Search
                lblAcD.Caption = "F4"   'ACCELERATOR D for Folder Dropdown
                lblAcC.Caption = "F7"   'ACCELERATOR C for Grouping Conversations
                lblAcA.Caption = "F8"   'ACCELERATOR A for Save Attachments
                lblAcW.Caption = "F9"   'ACCELERATOR W for Delete Flow
                lblAcM.Caption = "^="   'ACCELERATOR M for Save Mail
            End If
        Else
            lblAcX.Caption = "X"        'ACCELERATOR X for Delete email
            lblAcX.Width = lblAcX.Width / 2
            lblAcR.Caption = "R"        'ACCELERATOR R for remove item from list
            lblAcT.Caption = "T"        'ACCELERATOR T for Task ... Flag item and make it a task
            lblAcO.Caption = "O"        'ACCELERATOR O for Open Email
            lblAcO.Width = lblAcO.Width / 2
            lblAcM.Width = lblAcM.Width / 2
            If InitType And InitSort Then
                lblAcF.Caption = "F"   'ACCELERATOR F for Folder Search
                lblAcD.Caption = "D"   'ACCELERATOR D for Folder Dropdown
                lblAcC.Caption = "C"   'ACCELERATOR C for Grouping Conversations
                lblAcA.Caption = "A"   'ACCELERATOR A for Save Attachments
                lblAcW.Caption = "W"   'ACCELERATOR W for Delete Flow
                lblAcM.Caption = "M"   'ACCELERATOR M for Save Mail
            End If
        End If
    End Sub

    Friend Sub Init_FolderSuggestions(Optional varList As Variant)
        'Procedure Naming
        Dim SubNm As String
        SubNm = "Init_FolderSuggestions"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        Dim i As Integer
        Dim objProperty As UserProperty

        If Not IsArray(varList) Then
		Set objProperty = Mail.UserProperties.find("FolderKey")
		If Not objProperty Is Nothing Then varList = objProperty.Value
        End If
        If IsArray(varList) And IsArrayAllocated(varList) Then
            ReDim strFolders(UBound(varList)) As String
			'For i = LBound(varList) To UBound(varList)
			'    strFolders(i) = varList(i)
			'Next i
			'strFolders = varList
			'cbo.List = strFolders
			cbo.List = varList
            cbo.Value = cbo.List(0)
        Else

            suggestions = Email_AutoCategorize.Folder_Suggestions2(Mail, False)

            If suggestions.Count > 0 Then
                ReDim Preserve strFolders(suggestions.Count)
                For i = 1 To suggestions.Count
                    strFolders(i) = suggestions.FolderList(i)
                Next i
                cbo.List = strFolders
                cbo.Value = cbo.List(1)
            Else
                Call Email_SortToExistingFolder.FindFolder("", True)
                cbo.List = Email_SortToExistingFolder.FolderList
                If cbo.ListCount >= 2 Then cbo.Value = cbo.List(2)
            End If

        End If

        '    Set fldrHandler = New cFolderHandler
        '    cbo.List = fldrHandler.FindFolder("", True, ReCalcSuggestions:=True, objItem:=mail)
        '    If cbo.ListCount >= 2 Then cbo.Value = cbo.List(2)

        '    Set objProperty = mail.UserProperties.FIND("AutoFile")
        '    If Not objProperty Is Nothing Then txt.Value = objProperty.Value


        'Call Email_SortToExistingFolder.FindFolder("", True, objItem:=mail)

        Temp = SF_Stack.Pop


    End Sub

    Friend Sub CountMailsInConv(Optional ct As Integer = 0)
        'Procedure Naming
        Dim SubNm As String
        SubNm = "CountMailsInConv"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************


        'Dim Sel As Collection

        If ct <> 0 Then
            lblConvCt.Caption = CStr(ct)
        Else
        Set conv = New cConversation
        Set conv.item = Mail
        Set selItems_InClass = conv.ToCollection(True)
        'Set Sel = New Collection
        'Sel.Add Mail
        'Set selItems_InClass = Email_SortToExistingFolder.DemoConversation(selItems_InClass, Sel)
        lblConvCt.Caption = CStr(selItems_InClass.Count)
        End If


        Temp = SF_Stack.Pop
        'Debug.Print ""
    End Sub


    'Property Set ctl(PassedControl As MSForms.Control)
    'Set m_PassedControl = PassedControl
    '
    'Select Case TypeName(PassedControl)
    'Case "CheckBox"
    '    Set chk = PassedControl
    'Case "ComboBox"
    '    Set cbo = PassedControl
    'Case "ListBox"
    '    Set lst = PassedControl
    'Case "OptionButton"
    '    Set opt = PassedControl
    'Case "SpinButton"
    '    Set spn = PassedControl
    'Case "TextBox"
    '    Set txt = PassedControl
    '
    'End Select
    'End Property
    '
    'Private Sub cbo_Change()
    'PrintControlName
    'End Sub
    '
    'Private Sub chk_Click()
    'PrintControlName
    'End Sub
    '
    'Private Sub lst_Change()
    'PrintControlName
    'End Sub
    '
    'Private Sub opt_Click()
    'PrintControlName
    'End Sub
    '
    'Private Sub spn_Change()
    'PrintControlName
    'End Sub
    '
    'Private Sub txt_Change()
    'PrintControlName
    'End Sub
    '
    'Sub PrintControlName()
    'Debug.Print m_PassedControl.Name
    'End Sub
    '

    Sub Accel_Toggle()
        If lblMyPosition.Enabled = True Then
            If blAccel_FocusToggle Then
                If blExpanded = True Then ExpandCtrls1()
                Accel_FocusToggle()
            End If
            lblMyPosition.Enabled = False
            lblMyPosition.Visible = False
        Else
            lblMyPosition.Caption = intMyPosition
            lblMyPosition.Enabled = True
            lblMyPosition.Visible = True
        End If
    End Sub

    Sub Accel_FocusToggle()
        Dim ctlTmp As MSForms.control

        If blAccel_FocusToggle Then
            blAccel_FocusToggle = False
            For Each ctlTmp In colCtrls
                Select Case TypeName(ctlTmp)
                    Case "Frame"
                        ctlTmp.BackColor = &H8000000F
                    Case "CheckBox"
                        ctlTmp.BackColor = &H8000000F
                    Case "Label"
                        If Len(ctlTmp.Caption) <= 2 Then
                            ctlTmp.Visible = False
                        Else
                            ctlTmp.BackColor = &H8000000F
                        End If
                    Case "TextBox"
                        ctlTmp.BackColor = &H8000000F
                End Select
            Next ctlTmp
            If InitType And InitSort Then
                lblConvCt.Visible = True
                lblConvCt.BackColor = &H8000000F
                lblTriage.Visible = True
                lblTriage.BackColor = &H8000000F
            End If
            lblMyPosition.Visible = True
            lblMyPosition.BackColor = &H8000000D

        Else
            blAccel_FocusToggle = True
            For Each ctlTmp In colCtrls
                Select Case TypeName(ctlTmp)
                    Case "Frame"
                        ctlTmp.BackColor = &HFFFFC0
                    Case "CheckBox"
                        ctlTmp.BackColor = &HFFFFC0
                    Case "Label"
                        If Len(ctlTmp.Caption) <= 2 Then
                            ctlTmp.Visible = True
                        Else
                            ctlTmp.BackColor = &HFFFFC0
                        End If
                    Case "TextBox"
                        ctlTmp.BackColor = &HFFFFC0
                End Select
            Next ctlTmp
            If InitType And InitSort Then
                lblConvCt.BackColor = &HFFFFC0
                lblTriage.BackColor = &HFFFFC0
            End If
            lblMyPosition.BackColor = &H8000&
            'Modal        With ActiveExplorer
            'Modal            .ClearSelection
            'Modal            If .IsItemSelectableInView(mail) Then .AddToSelection mail
            'Modal            'DoEvents
            'Modal        End With
        End If
    End Sub
    Sub Mail_Activate()
        Dim objModule As Outlook.MailModule

        On Error Resume Next
        With ActiveExplorer


            If .CurrentFolder.DefaultItemType <> olMailItem Then
                Set .NavigationPane.CurrentModule = .NavigationPane.Modules.GetNavigationModule(olModuleMail)
            End If
            If .CurrentView <> "tmpNoConversation" Then
                .CurrentView = "tmpNoConversation"
            End If
            .ClearSelection
            If .IsItemSelectableInView(Mail) Then .AddToSelection Mail
            'DoEvents
        End With
        If Err.Number <> 0 Then
            MsgBox("Error in QF.Mail_Activate: " & Err.Description)
            Deactivate_Email_Timing_And_Velocity
            Stop
            Err.Clear()
        End If
    End Sub
    Sub KB(AccelCode As String)
        Dim f As Outlook.MAPIFolder

        Select Case AccelCode
            Case "O"

                lblSubject.ForeColor = &H80000012
                lblSubject.Font.Bold = False
                lblSender.ForeColor = &H80000012
                lblSender.Font.Bold = False
                If InitType And InitSort Then
                    Mail_Activate()       'For modal code
                Else
                    Mail.Display
                End If
            'oParent.QFD_Minimize
            'Email_SortToExistingFolder.MailsSelect Email_SortToExistingFolder.MailToCollection(Mail)

            Case "C"
                If InitType And InitSort Then chk.Value = Not chk.Value
            Case "A"
                If InitType And InitSort Then chbxSaveAttach.Value = Not chbxSaveAttach.Value
            Case "W"
                If InitType And InitSort Then chbxDelFlow.Value = Not chbxDelFlow.Value
            Case "M"
                If InitType And InitSort Then chbxSaveMail.Value = Not chbxSaveMail.Value
            Case "T"
                cbFlag_Click()
            Case "F"
                If InitType And InitSort Then txt.SetFocus
            Case "D"
                If InitType And InitSort Then cbo.SetFocus
            Case "X"
                cbDel_Click()
            Case "R"
                cbKll_Click()
        End Select
    End Sub

    Sub ResizeCtrls(intPxChg As Integer)
        Dim ctlTmp As MSForms.control
        Dim i As Integer
        Dim strBodyText As String
        Dim X1pct As Double
        Dim X2pct As Double
        Dim X3pct As Double
        Dim X1px As Long
        Dim X2px As Long
        Dim X3px As Long
        Dim lngTmp As Long

        X1pct = 0.6
        X3pct = X1pct / 2
        X2pct = 1 - X1pct

        X1pct = X1pct * intPxChg
        X2pct = X2pct * intPxChg
        X3pct = X3pct * intPxChg
        X1px = Round(X1pct, 0)
        X2px = Round(X2pct, 0)
        X3px = Round(X3pct, 0)

        lblSubject.Width = lblSubject_Width + X1px                      'Subject Width X%
        cbFlag.Left = cbFlag_Left + X1px + X2px                         'Task button X% + Y% left position
        lblAcT.Left = lblAcT_Left + X1px + X2px                         'Task accelerator X% + Y% left position
        cbDel.Left = cbDel_Left + X1px + X2px                           'Delete button X+Y% Left position
        cbKll.Left = cbKll_Left + X1px + X2px                           'Kill button X+Y% Left position
        lblAcX.Left = lblAcX_Left + X1px + X2px
        lblAcR.Left = lblAcR_Left + X1px + X2px
        lblSentOn.Left = lblSentOn_Left + X1px                          'SentOn X% Left Position
        lblActionable.Left = lblActionable_Left + X3px                  '<ACTIONABL> left position + X3px
        lblTriage.Left = lblTriage_Left + X3px                          'Triage left position + X3px


        If InitType And InitSort Then
            txt.Left = txt_Left + X1px                                  'Folder search box X% left position Y% Width
            txt.Width = txt_Width + X2px                                'Folder search box X% left position Y% Width
            lbl5.Left = lbl5_Left + X1px                                'Folder label X% left position
            lblAcF.Left = lblAcF_Left + X1px                            'F Accelerator X% left position
            lblConvCt.Left = lblConvCt_Left + X1px                      'Conversation Count X% Left Position
            chbxSaveAttach.Left = chbxSaveAttach_Left + X1px + X2px     'Checkbox Save Attachment X% Left Position
            chbxSaveMail.Left = chbxSaveMail_Left + X1px + X2px         'Checkbox Save Mail X% Left Position
            chbxDelFlow.Left = chbxDelFlow_Left + X1px + X2px           'Checkbox Delete Flow X% Left Position
            lblAcA.Left = lblAcA_Left + X1px + X2px                     'A Accelerator X% Left Position
            lblAcW.Left = lblAcW_Left + X1px + X2px                     'W Accelerator X% Left Position
            lblAcM.Left = lblAcM_Left + X1px + X2px                     'M Accelerator X% Left Position

            If blExpanded Then

                cbo.Width = frm.Width - cbo.Left - lngBlock_Width - 5
                pos_cbo.leftOriginal = cbo_Left + X1px                   'Dropdown box X% Left position Y% Width
                pos_cbo.widthOriginal = cbo_Width + X2px                 'Dropdown box X% Left position Y% Width
                pos_lblAcD.leftOriginal = lblAcD_Left + X1px             'D Accelerator X% left position
                pos_lblAcC.leftOriginal = lblAcC_Left + X1px             'Conversation accelerator X% Left position
                pos_chk.leftOriginal = chk_Left + X1px                   'Conversation checkbox X% Left Position
                lngTmp = chk.Left
                chk.Left = lblConvCt.Left - 10
                lblAcC.Left = lblAcC.Left + chk.Left - lngTmp
                lblBody.Width = frm.Width - lblBody.Left - 5
                pos_body.widthOriginal = lblBody_Width + X1px            'Body Width X%

            Else

                cbo.Left = cbo_Left + X1px                               'Dropdown box X% Left position Y% Width
                cbo.Width = cbo_Width + X2px                             'Dropdown box X% Left position Y% Width
                lblAcD.Left = lblAcD_Left + X1px                         'D Accelerator X% left position
                lblAcC.Left = lblAcC_Left + X1px + X2px                  'Conversation accelerator X% Left position
                chk.Left = chk_Left + X1px + X2px                        'Conversation checkbox X% Left Position
                lblBody.Width = lblBody_Width + X1px                     'Body Width X%

            End If

        Else
            lblBody.Width = lblBody_Width + X1px + X2px                   'Body Width X%
        End If

    End Sub

    Sub ExpandCtrls1()

        Dim lngShift As Long
        'Private pos_lblAcC          As ctrlPosition
        'Private pos_lblAcD          As ctrlPosition
        'Private pos_lblAcO          As ctrlPosition

        If InitType And InitSort Then
            If blExpanded = False Then
                blExpanded = True
                frm.Height = frm.Height * 2
                lngShift = lblSubject.Top + lblSubject.Height - cbo.Top + 1

                pos_cbo.topOriginal = cbo.Top
                pos_cbo.topNew = pos_cbo.topOriginal + lngShift
                cbo.Top = pos_cbo.topNew

                pos_lblAcD.topOriginal = lblAcD.Top
                lblAcD.Top = pos_lblAcD.topOriginal + lngShift

                pos_cbo.leftOriginal = cbo.Left
                cbo.Left = lblBody.Left

                pos_lblAcD.leftOriginal = lblAcD.Left
                lblAcD.Left = max(0, cbo.Left - pos_cbo.leftOriginal + pos_lblAcD.leftOriginal)

                pos_cbo.widthOriginal = cbo.Width
                pos_cbo.widthNew = pos_cbo.leftOriginal - cbo.Left + pos_cbo.widthOriginal - lngBlock_Width
                cbo.Width = pos_cbo.widthNew

                lngShift = cbo.Top + cbo.Height - lblBody.Top + 1

                With pos_body
                    .topOriginal = lblBody.Top
                    .topNew = .topOriginal + lngShift
                    lblBody.Top = .topNew

                    pos_lblAcO.topOriginal = lblAcO.Top
                    lblAcO.Top = lblAcO.Top + lngShift

                    .heightOriginal = lblBody.Height
                    .heightNew = frm.Height - .topNew - 5
                    lblBody.Height = .heightNew
                    .widthOriginal = lblBody.Width
                    .widthNew = frm.Width - lblBody.Left - 5
                    lblBody.Width = .widthNew
                End With

                chk.Caption = ""
                pos_chk.leftOriginal = chk.Left
                chk.Left = lblConvCt.Left - 10
                pos_lblAcC.leftOriginal = lblAcC.Left
                lblAcC.Left = chk.Left - pos_chk.leftOriginal + pos_lblAcC.leftOriginal

                pos_chk.topOriginal = chk.Top
                chk.Top = lblConvCt.Top

                pos_lblAcC.topOriginal = lblAcC.Top
                lblAcC.Top = lblConvCt.Top

                pos_chk.widthOriginal = chk.Width
                chk.Width = 10


                pos_chbxSaveAttach.topOriginal = chbxSaveAttach.Top
                chbxSaveAttach.Top = pos_cbo.topNew

                pos_chbxSaveMail.topOriginal = chbxSaveMail.Top
                chbxSaveMail.Top = pos_cbo.topNew

                pos_chbxDelFlow.topOriginal = chbxDelFlow.Top
                chbxDelFlow.Top = pos_cbo.topNew

                pos_lblAcA.topOriginal = lblAcA.Top
                lblAcA.Top = pos_cbo.topNew

                pos_lblAcW.topOriginal = lblAcW.Top
                lblAcW.Top = pos_cbo.topNew

                pos_lblAcM.topOriginal = lblAcM.Top
                lblAcM.Top = pos_cbo.topNew






            Else
                blExpanded = False
                frm.Height = frm.Height / 2

                cbo.Top = pos_cbo.topOriginal
                cbo.Left = pos_cbo.leftOriginal
                cbo.Width = pos_cbo.widthOriginal

                lblAcD.Top = pos_lblAcD.topOriginal
                lblAcD.Left = pos_lblAcD.leftOriginal

                lblBody.Top = pos_body.topOriginal
                lblBody.Height = pos_body.heightOriginal
                lblBody.Width = pos_body.widthOriginal
                lblAcO.Top = pos_lblAcO.topOriginal

                chk.Caption = "  Conversation"
                chk.Left = pos_chk.leftOriginal
                chk.Top = pos_chk.topOriginal
                chk.Width = pos_chk.widthOriginal
                lblAcC.Left = pos_lblAcC.leftOriginal
                lblAcC.Top = pos_lblAcC.topOriginal

                chbxSaveAttach.Top = pos_chbxSaveAttach.topOriginal
                chbxSaveMail.Top = pos_chbxSaveMail.topOriginal
                chbxDelFlow.Top = pos_chbxDelFlow.topOriginal
                lblAcA.Top = pos_lblAcA.topOriginal
                lblAcW.Top = pos_lblAcW.topOriginal
                lblAcM.Top = pos_lblAcM.topOriginal


            End If
        Else
            If blExpanded = False Then
                blExpanded = True
                frm.Height = frm.Height * 2
                With pos_body
                    .topOriginal = lblBody.Top
                    pos_lblAcO.topOriginal = lblAcO.Top
                    .heightOriginal = lblBody.Height
                    .heightNew = frm.Height - .topOriginal - 5
                    lblBody.Height = .heightNew
                End With
            Else
                blExpanded = False
                frm.Height = frm.Height / 2
                With pos_body
                    lblBody.Top = pos_body.topOriginal
                    lblBody.Height = pos_body.heightOriginal
                    lblAcO.Top = pos_lblAcO.topOriginal
                End With
            End If


        End If

    End Sub

    Sub MoveMail()

        'Procedure Naming
        Dim SubNm As String
        SubNm = "MoveMail"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************



        Dim selItems As Collection
        Dim loc As String
        Dim myFolder As Outlook.Folder
        Dim MSG As mailItem
        Dim Sel As Collection
        Dim Attchments As Boolean
        Dim blRepullConv As Boolean
        Dim blDoMove As Boolean

        blRepullConv = False

        If Not Mail Is Nothing Then
            If chk.Value = True Then
                If Not selItems_InClass Is Nothing Then
                    If selItems_InClass.Count = CInt(lblConvCt.Caption) And selItems_InClass.Count <> 0 Then
                    Set selItems = selItems_InClass
                Else
                        blRepullConv = True
                    End If
                Else
                    blRepullConv = True
                End If

                If blRepullConv Then
                'Set selItems = New Collection
                'Set Sel = New Collection
                'Sel.Add Mail
                'Set selItems = Email_SortToExistingFolder.DemoConversation(selItems, Sel)
                
                Set conv = New cConversation
                Set conv.item = Mail
                Set selItems = conv.ToCollection(True)
            End If
            Else
            Set selItems = New Collection
            selItems.Add Mail
        End If

            If cbo.Value = "Trash to Delete" Then
                Attchments = False
            Else
                Attchments = chbxSaveAttach.Value
            End If

            blDoMove = True
            On Error Resume Next
            If fldrOriginal <> Mail.Parent Then blDoMove = False
            If Err.Number <> 0 Then
                Err.Clear()
                blDoMove = False
            End If

            If blDoMove Then
                Email_SortToExistingFolder.Load_CTF_AND_Subjects_AND_Recents
                Email_SortToExistingFolder.MASTER_SortEmailsToExistingFolder selItems:=selItems,
                    Pictures_Checkbox:=False,
                    SortFolder:=cbo.Value,
                    Save_MSG:=chbxSaveMail.Value,
                    Attchments:=Attchments,
                    Remove_Flow_File:=chbxDelFlow.Value
            Email_SortToExistingFolder.Cleanup_Files
            End If 'blDoMove



        End If

        On Error Resume Next
        Temp = SF_Stack.Pop


    End Sub

    Public Sub ctrlsRemove()

        'Procedure Naming
        Dim SubNm As String
        SubNm = "ctrlsRemove"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************

        Do While colCtrls.Count > 1
            frm.controls.Remove colCtrls.Item(colCtrls.Count).Name
        colCtrls.Remove colCtrls.Count
    Loop

    Set fldrHandler = Nothing
    Temp = SF_Stack.Pop
    End Sub

    Public Sub kill()
        'Procedure Naming
        Dim SubNm As String
        SubNm = "kill"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


'*******************END Error Header*********************
    
    
    Set m_PassedControl = Nothing
    Set chk = Nothing
    Set cbo = Nothing
    Set lst = Nothing
    Set opt = Nothing
    Set spn = Nothing
    Set txt = Nothing
    Set frm = Nothing
    Set cbKll = Nothing
    Set Mail = Nothing
    Set fldrTarget = Nothing
    Set lblTmp = Nothing
    'Set suggestions = Nothing
    'Set strFolders = Nothing
    Set colCtrls = Nothing
    Set fldrHandler = Nothing

    Temp = SF_Stack.Pop

    End Sub

    Private Sub bdy_Click()
        lblSubject.ForeColor = &H80000012
        lblSubject.Font.Bold = False
        lblSender.ForeColor = &H80000012
        lblSender.Font.Bold = False
        Mail.Display
        oParent.QFD_Minimize
        If blShowAsConversations Then oParent.ExplConvView_ToggleOn
    End Sub

    Private Sub cbDel_Click()
        cbo.Value = "Trash to Delete"
    End Sub


    Private Sub cbDel_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub cbDel_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        KeyPressHandler_Class KeyAscii
End Sub

    Private Sub cbDel_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        'Select Case KeyCode
        '    Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
    '    Case Else
        'End Select
    End Sub

    Private Sub cbFlag_Click()
        'Procedure Naming
        Dim SubNm As String
        SubNm = "cbFlag_Click"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************




        Dim Sel As Collection
    
    Set Sel = New Collection
    Sel.Add Mail
    Flag_Task Sel, False, hWndCaller:=hWndCaller
    cbFlag.Caption = "!"

        Temp = SF_Stack.Pop
    End Sub

    Private Sub cbFlag_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub cbFlag_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        KeyPressHandler_Class KeyAscii
End Sub

    Private Sub cbFlag_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
    '        Case Else
        '    End Select
    End Sub

    Private Sub cbKll_Click()
        oParent.RemoveSpecificControlGroup intMyPosition
End Sub

    Private Sub cbKll_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub cbKll_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        KeyPressHandler_Class KeyAscii
End Sub

    Private Sub cbKll_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
    '        Case Else
        '    End Select
    End Sub

    Private Sub cbo_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Select Case KeyCode
            Case vbKeyReturn
                If intEnterCounter = 1 Then
                    intEnterCounter = 0
                    oParent.KeyPressHandler KeyCode
            Else
                    intEnterCounter = 1
                    intComboRightCtr = 0
                End If
            Case Else
                oParent.KeyDownHandler KeyCode, Shift
    End Select
    End Sub



    Private Sub cbo_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Select Case KeyCode
            Case 18
                oParent.KeyUpHandler KeyCode, Shift
        Case vbKeyEscape
                intEnterCounter = 0
                intComboRightCtr = 0
            Case vbKeyRight
                intEnterCounter = 0
                If intComboRightCtr = 0 Then
                    cbo.DropDown
                    intComboRightCtr = 1
                ElseIf intComboRightCtr = 1 Then

                    Email_SortToExistingFolder.InitializeSortToExisting InitType:="Sort",
                        QuickLoad:=False,
                        WholeConversation:=False,
                        strSeed:=cbo.Value,
                        objItem:=Mail
                cbKll_Click()
                Else
                    MsgBox "Error in intComboRightCtr ... setting to 0 and continuing"
                intComboRightCtr = 0
                End If
            Case vbKeyLeft
                intEnterCounter = 0
                intComboRightCtr = 0
            Case vbKeyDown
                intEnterCounter = 0
            Case vbKeyUp
                intEnterCounter = 0
        End Select
    End Sub


    Private Sub cbTmp_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub cbTmp_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyUpHandler KeyCode, Shift
End Sub

    Private Sub chk_Click()
        'Procedure Naming
        Dim SubNm As String
        SubNm = "chk_Click"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************



        Dim selItems As Collection
        Dim Sel As Collection
        Dim objItem As Object
        Dim objMail As Outlook.mailItem
        Dim i As Integer
        Dim strHashMail, strHashTest As String
        Dim intIDX As Integer
        Dim varList As Variant
    
    'Create a collection with all of the mail items in the conversation in the current folder
    Set selItems = New Collection
    
    If selItems_InClass Is Nothing Then CountMailsInConv()

        For i = 1 To selItems_InClass.Count
        Set objItem = selItems_InClass(i)
        Set objMail = objItem
        If objMail.EntryID <> Mail.EntryID Then selItems.Add objItem
    Next i


        'Set sel = New Collection
        'sel.Add mail
        'Set selItems = Email_SortToExistingFolder.DemoConversation(selItems, sel)

        'Remove the current email from the collection because we will add or subtract the others
        'For i = selItems.Count To 1 Step -1
        '    Set objMail = selItems(i)
        '    If objMail.EntryID = mail.EntryID Then selItems.Remove i
        'Next i

        If chk.Value = True Then
            oParent.ConvToggle_Group selItems, intMyPosition
        lblConvCt.Enabled = True
        Else
            varList = cbo.List
            oParent.ConvToggle_UnGroup selItems, intMyPosition, CInt(lblConvCt.Caption), varList
        lblConvCt.Enabled = False
        End If


        Temp = SF_Stack.Pop
    End Sub

    Private Sub chk_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub chk_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
'        Case Else
        '    End Select
    End Sub

    Private Sub frm_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub frm_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        oParent.KeyPressHandler KeyAscii
End Sub

    Private Sub frm_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
    '        Case Else
        '    End Select
    End Sub

    Private Sub lst_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub lst_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
    '        Case Else
        '    End Select
    End Sub

    Private Sub opt_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub opt_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
    '        Case Else
        '    End Select
    End Sub

    Private Sub spn_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        oParent.KeyDownHandler KeyCode, Shift
End Sub

    Private Sub spn_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
    '        Case Else
        '    End Select
    End Sub

    Private Sub txt_Change()
        'Procedure Naming
        Dim SubNm As String
        SubNm = "chk_Click"
        Dim Temp As Variant

        If SF_Stack Is Nothing Then Set SF_Stack = New cStackGeneric
    If TraceStack Is Nothing Then Set TraceStack = New cStackGeneric
    
    SF_Stack.Push SubNm
    strSubs = SF_Stack.GetString(True)
        TraceStack.Push strSubs

    SubNm = Format(Now(), "hh:mm:ss") & " " & SubNm & " "


        '*******************END Error Header*********************

        '    cbo.List = fldrHandler.FindFolder("*" & txt.Value & "*", True, ReCalcSuggestions:=False, objItem:=mail)
        Call Email_SortToExistingFolder.FindFolder("*" & txt.Value & "*", True)
        cbo.List = Email_SortToExistingFolder.FolderList
        If cbo.ListCount >= 2 Then cbo.Value = cbo.List(1)

        On Error Resume Next
        Temp = SF_Stack.Pop
    End Sub


    Private Sub KeyPressHandler_Class(ByVal KeyAscii As MSForms.ReturnInteger)
        Select Case KeyAscii
            Case vbKeyReturn
                oParent.KeyPressHandler KeyAscii
        Case vbKeyTab
                oParent.KeyPressHandler KeyAscii
        Case vbKeyEscape
                oParent.KeyPressHandler KeyAscii
    End Select

    End Sub


    Private Sub txt_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        oParent.KeyDownHandler KeyCode, Shift
    '        Case Else
        '    End Select
    End Sub

    Private Sub txt_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        oParent.KeyPressHandler KeyAscii
End Sub

    Private Sub txt_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler KeyCode, Shift
    '        Case Else
        '    End Select
    End Sub
End Class
