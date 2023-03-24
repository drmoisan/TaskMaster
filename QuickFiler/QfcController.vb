Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop
Imports ToDoModel
Imports UtilitiesVB
Imports TaskVisualization


Public Class QfcController


    Private _parent As QfcGroupOperationsLegacy
    Private _initType As InitTypeEnum
    Private _mPassedControl As Control
    Public WithEvents chk As CheckBox         'Checkbox to Group Conversations
    Public WithEvents cbo As ComboBox         'Combo box containing Folder Suggestions
    Private WithEvents _lst As ListBox
    Private WithEvents _txt As TextBox          'Input for folder search
    Private WithEvents _bdy As TextBox
    Private WithEvents _cbKll As Button    'Remove mail from Processing
    Private WithEvents _cbDel As Button    'Delete email
    Private WithEvents _cbFlag As Button    'Flag as Task
    Private WithEvents _cbTmp As Button
    Public WithEvents frm As Panel
    Public Mail As MailItem
    Private _fldrOriginal As Folder
    Public intMyPosition As Integer
    Private _fldrTarget As Folder
    Private _lblTmp As Label
    Public lblConvCt As Label            'Count of Conversation Members
    Private _lblMyPosition As Label            'ACCELERATOR Email Position
    Private _suggestions = New cSuggestions()
    'Private _suggestions As Email_AutoCategorize._suggestions
    Private _strFolders() As String
    Private _colCtrls As Collection
    Private _selItemsInClass As Collection
    Private _blAccelFocusToggle As Boolean
    Private _intEnterCounter As Integer
    Private _intComboRightCtr As Integer
    Public Structure ctrlPosition
        Public blInOrigPos As Boolean
        Public topOriginal As Long
        Public topNew As Long
        Public leftOriginal As Long
        Public leftNew As Long
        Public heightOriginal As Long
        Public heightNew As Long
        Public widthOriginal As Long
        Public widthNew As Long
    End Structure

    Private _chbxSaveAttach As CheckBox
    Private _chbxSaveMail As CheckBox
    Private _chbxDelFlow As CheckBox

    Public blExpanded As Boolean
    Public blHasChild As Boolean

    Private _lbl1 As Label            'From:
    Private _lbl2 As Label            'Subject:
    Private _lbl3 As Label            'Body:
    Private _lbl4 As Label            'Sent On:
    Private _lbl5 As Label            'Folder:

    Public LblSender As Label            '<SENDER>
    Public LblSubject As Label            '<SUBJECT>
    Public TxtBoxBody As TextBox            '<BODY>
    Public StrlblTo As String                   '<TO>




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
    Private opt As System.Windows.Forms.RadioButton
    Private spn As System.Windows.Forms.NumericUpDown


    Private _fldrHandler As cFolderHandler
    Private hWndCaller As IntPtr

    Private p_BoolRemoteMouseApp As Boolean
    Private conv As cConversation

    Private _globals As IApplicationGlobals
    Private _activeExplorer As Outlook.Explorer

    Friend Sub New(m_mail As MailItem,
        col As Collection,
        intPositionArg As Integer,
        BoolRemoteMouseApp As Boolean,
        Caller As QfcGroupOperationsLegacy,
        AppGlobals As IApplicationGlobals,
        Optional hwnd As IntPtr = Nothing,
        Optional InitTypeE As InitTypeEnum = InitTypeEnum.InitSort)

        'QFD_Minimize
        'KeyDownHandler
        'KeyUpHandler
        'KeyPressHandler
        'toggleAcceleratorDialogue
        'RemoveSpecificControlGroup
        'ExplConvView_ToggleOn
        'ConvToggle_Group
        'ConvToggle_UnGroup

        _globals = AppGlobals
        _activeExplorer = AppGlobals.Ol.App.ActiveExplorer()

        Dim ctlTmp As System.Windows.Forms.Control
        Dim strBodyText As String

        _initType = InitTypeE
        _parent = Caller
        intMyPosition = intPositionArg        'call back position in collection
        Mail = m_mail
        _fldrOriginal = Mail.Parent
        hWndCaller = hwnd
        _colCtrls = col
        For Each ctlTmp In col
            Select Case TypeName(ctlTmp)
                Case "Panel"
                    frm = ctlTmp
                Case "CheckBox"
                    Select Case ctlTmp.Text
                        Case "  Conversation"
                            chk = ctlTmp
                        Case " Attach"
                            _chbxSaveAttach = ctlTmp
                        Case " Flow"
                            _chbxDelFlow = ctlTmp
                        Case " Mail"
                            _chbxSaveMail = ctlTmp
                    End Select
                Case "ComboBox"
                    cbo = ctlTmp
                Case "ListBox"
                    _lst = ctlTmp
                Case "OptionButton"
                    opt = ctlTmp
                Case "SpinButton"
                    spn = ctlTmp
                Case "TextBox"
                    If ctlTmp.Text = "<BODY>" Then
                        strBodyText = Replace(Mail.Body, vbCrLf, " ")
                        strBodyText = Replace(strBodyText, "  ", " ")
                        strBodyText = Replace(strBodyText, "  ", " ") & "<EOM>"
                        ctlTmp.Text = strBodyText
                        _bdy = ctlTmp
                        TxtBoxBody = ctlTmp
                    Else
                        _txt = ctlTmp
                    End If

                Case "Label"
                    _lblTmp = ctlTmp
                    Select Case _lblTmp.Text
                        Case "From:"
                            _lbl1 = _lblTmp
                        Case "Subject:"
                            _lbl2 = _lblTmp
                        Case "Body:"
                            _lbl3 = _lblTmp
                        Case "Sent On:"
                            _lbl4 = _lblTmp
                        Case "Folder:"
                            _lbl5 = _lblTmp
                        Case "<SENDER>"
                            _lblTmp.Text = If(Mail.Sent = True, GetSenderAddress(Mail), "Draft Message")
                            LblSender = _lblTmp
                        Case "<SUBJECT>"
                            _lblTmp.Text = Mail.Subject
                            LblSubject = _lblTmp
                        Case "ABC"
                            _lblTmp.Text = CustomFieldID_GetValue(Mail, "Triage")
                            lblTriage = _lblTmp
                        Case "<ACTIONABL>"
                            _lblTmp.Text = CustomFieldID_GetValue(Mail, "Actionable")
                            lblActionable = _lblTmp
                        Case "<#>"
                            lblConvCt = _lblTmp
                        Case "<Pos#>"
                            _lblMyPosition = _lblTmp
                        Case "<BODY>"

                        Case "<SENTON>"
                            _lblTmp.Text = Format(Mail.SentOn, "MM/dd/yy HH:MM")
                            lblSentOn = _lblTmp
                        Case "F"
                            lblAcF = _lblTmp

                        Case "D"
                            lblAcD = _lblTmp
                        Case "C"
                            lblAcC = _lblTmp
                        Case "X"
                            lblAcX = _lblTmp
                        Case "R"
                            lblAcR = _lblTmp
                        Case "T"
                            lblAcT = _lblTmp
                        Case "O"
                            lblAcO = _lblTmp
                        Case "A"
                            lblAcA = _lblTmp
                        Case "W"
                            lblAcW = _lblTmp
                        Case "M"
                            lblAcM = _lblTmp
                    End Select
                Case "Button"
                    _cbTmp = ctlTmp
                    If _cbTmp.Text = "X" Then
                        _cbDel = ctlTmp
                    ElseIf _cbTmp.Text = "-->" Then
                        _cbKll = ctlTmp
                    ElseIf _cbTmp.Text = "|>" Then
                        _cbFlag = ctlTmp
                    End If
            End Select

        Next ctlTmp

        If Mail.UnRead = True Then
            LblSubject.ForeColor = Drawing.Color.DarkBlue
            LblSubject.Font = New Font(LblSubject.Font, FontStyle.Bold)
            LblSender.ForeColor = Drawing.Color.DarkBlue
            LblSender.Font = New Font(LblSender.Font, FontStyle.Bold)
        End If
        lblSubject_Width = LblSubject.Width
        lblBody_Width = TxtBoxBody.Width
        cbFlag_Left = _cbFlag.Left
        lblAcT_Left = lblAcT.Left

        lblTriage_Width = lblTriage.Width
        lblTriage_Left = lblTriage.Left
        lblActionable_Left = lblActionable.Left
        lblActionable_Width = lblActionable.Width


        cbDel_Left = _cbDel.Left
        cbKll_Left = _cbKll.Left
        lblAcX_Left = lblAcX.Left
        lblAcR_Left = lblAcR.Left


        lblSentOn_Left = lblSentOn.Left                 'SentOn X% Left Position



        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            lbl5_Left = _lbl5.Left
            lblAcF_Left = lblAcF.Left
            lblAcD_Left = lblAcD.Left
            cbo_Left = cbo.Left
            cbo_Width = cbo.Width
            lblAcC_Left = lblAcC.Left                       'Conversation accelerator X% Left position
            chk_Left = chk.Left                             'Conversation checkbox X% Left Position
            chbxSaveAttach_Left = _chbxSaveAttach.Left       'Checkbox Save Attachment X% Left Position
            chbxSaveMail_Left = _chbxSaveMail.Left           'Checkbox Save Mail X% Left Position
            chbxDelFlow_Left = _chbxDelFlow.Left             'Checkbox Delete Flow X% Left Position
            lblAcA_Left = lblAcA.Left                       'A Accelerator X% Left Position
            lblAcW_Left = lblAcW.Left                       'W Accelerator X% Left Position
            lblAcM_Left = lblAcM.Left                       'M Accelerator X% Left Position
            txt_Left = _txt.Left
            txt_Width = _txt.Width
            lblConvCt_Left = lblConvCt.Left                 'Conversation Count X% Left Position
        End If

        lngBlock_Width = frm.Width - chbxSaveAttach_Left 'Width of block of right justified controls

        StrlblTo = Mail.To

        If BoolRemoteMouseApp Then ToggleRemoteMouseAppLabels()




    End Sub

    Friend Sub ToggleRemoteMouseAppLabels()
        p_BoolRemoteMouseApp = Not p_BoolRemoteMouseApp
        If p_BoolRemoteMouseApp Then

            lblAcX.Text = "^-"       'ACCELERATOR X for Delete email
            lblAcX.Width *= 2
            lblAcR.Text = "F3"       'ACCELERATOR R for remove item from list
            lblAcT.Text = "F2"       'ACCELERATOR T for Task ... Flag item and make it a task
            lblAcO.Text = "^0"       'ACCELERATOR O for Open Email
            lblAcO.Width *= 2
            lblAcM.Width *= 2
            If _initType.HasFlag(InitTypeEnum.InitSort) Then
                lblAcF.Text = "F1"   'ACCELERATOR F for Folder Search
                lblAcD.Text = "F4"   'ACCELERATOR D for Folder Dropdown
                lblAcC.Text = "F7"   'ACCELERATOR C for Grouping Conversations
                lblAcA.Text = "F8"   'ACCELERATOR A for Save Attachments
                lblAcW.Text = "F9"   'ACCELERATOR W for Delete Flow
                lblAcM.Text = "^="   'ACCELERATOR M for Save Mail
            End If
        Else
            lblAcX.Text = "X"        'ACCELERATOR X for Delete email
            lblAcX.Width /= 2
            lblAcR.Text = "R"        'ACCELERATOR R for remove item from list
            lblAcT.Text = "T"        'ACCELERATOR T for Task ... Flag item and make it a task
            lblAcO.Text = "O"        'ACCELERATOR O for Open Email
            lblAcO.Width /= 2
            lblAcM.Width /= 2
            If _initType.HasFlag(InitTypeEnum.InitSort) Then
                lblAcF.Text = "F"   'ACCELERATOR F for Folder Search
                lblAcD.Text = "D"   'ACCELERATOR D for Folder Dropdown
                lblAcC.Text = "C"   'ACCELERATOR C for Grouping Conversations
                lblAcA.Text = "A"   'ACCELERATOR A for Save Attachments
                lblAcW.Text = "W"   'ACCELERATOR W for Delete Flow
                lblAcM.Text = "M"   'ACCELERATOR M for Save Mail
            End If
        End If
    End Sub

    Friend Sub Init_FolderSuggestions(Optional varList As Object = Nothing)

        Dim i As Integer
        Dim objProperty As UserProperty

        If Not IsArray(varList) Then
            objProperty = Mail.UserProperties.Find("FolderKey")
            If objProperty IsNot Nothing Then varList = objProperty.Value
        End If
        If IsArray(varList) Then
            If IsAllocated(varList) Then

                'For i = LBound(varList) To UBound(varList)
                cbo.Items.AddRange(varList)
                cbo.SelectedIndex = 0
                'Next i
            End If


        Else
            'TODO: cSuggestions and cFolderHandler are to mixed up with functionality. Need to clean up.
            _suggestions = Folder_Suggestions(Mail, _globals, False)

            If _suggestions.Count > 0 Then
                ReDim Preserve _strFolders(_suggestions.Count)
                For i = 1 To _suggestions.Count
                    _strFolders(i) = _suggestions.FolderList(i)
                Next i
                cbo.Items.AddRange(_strFolders)
                cbo.SelectedIndex = 1
            Else
                _fldrHandler = New cFolderHandler(_globals)
                cbo.Items.AddRange(_fldrHandler.FindFolder("", True, ReCalcSuggestions:=True, objItem:=Mail))

                If cbo.Items.Count >= 2 Then cbo.SelectedIndex = 2
            End If

        End If

        '    Set _fldrHandler = New cFolderHandler
        '    cbo.List = _fldrHandler.FindFolder("", True, ReCalcSuggestions:=True, objItem:=mail)
        '    If cbo.ListCount >= 2 Then cbo.Value = cbo.List(2)

        '    Set objProperty = mail.UserProperties.FIND("AutoFile")
        '    If Not objProperty Is Nothing Then _txt.Value = objProperty.Value


        'Call Email_SortToExistingFolder.FindFolder("", True, objItem:=mail)




    End Sub

    Friend Sub CountMailsInConv(Optional ct As Integer = 0)



        'Dim Sel As Collection

        If ct <> 0 Then
            lblConvCt.Text = CStr(ct)
        Else
            conv = New cConversation(_globals.Ol.App) With {.item = Mail}
            _selItemsInClass = conv.ToCollection(True)
            'Set Sel = New Collection
            'Sel.Add Mail
            'Set _selItemsInClass = Email_SortToExistingFolder.DemoConversation(_selItemsInClass, Sel)
            lblConvCt.Text = CStr(_selItemsInClass.Count)
        End If



    End Sub

    Public Sub Accel_Toggle()
        If _lblMyPosition.Enabled = True Then
            If _blAccelFocusToggle Then
                If blExpanded = True Then ExpandCtrls1()
                Accel_FocusToggle()
            End If
            _lblMyPosition.Enabled = False
            _lblMyPosition.Visible = False
            _lblMyPosition.SendToBack()
        Else
            _lblMyPosition.Text = intMyPosition
            _lblMyPosition.Enabled = True
            _lblMyPosition.Visible = True
            _lblMyPosition.BackColor = Drawing.Color.Blue
            _lblMyPosition.BringToFront()
        End If
    End Sub

    Public Sub Accel_FocusToggle()
        Dim ctlTmp As System.Windows.Forms.Control

        If _blAccelFocusToggle Then
            _blAccelFocusToggle = False
            For Each ctlTmp In _colCtrls
                Select Case TypeName(ctlTmp)
                    Case "Panel"
                        ctlTmp.BackColor = Drawing.SystemColors.Control
                    Case "CheckBox"
                        ctlTmp.BackColor = Drawing.SystemColors.Control
                    Case "Label"
                        If Len(ctlTmp.Text) <= 2 Then
                            ctlTmp.Visible = False
                            ctlTmp.SendToBack()
                        Else
                            ctlTmp.BackColor = Drawing.SystemColors.Control
                        End If
                    Case "TextBox"
                        ctlTmp.BackColor = Drawing.SystemColors.Control
                End Select
            Next ctlTmp
            If _initType.HasFlag(InitTypeEnum.InitSort) Then
                lblConvCt.Visible = True
                lblConvCt.BackColor = Drawing.SystemColors.Control
                lblConvCt.BringToFront()
                lblTriage.Visible = True
                lblTriage.BackColor = Drawing.SystemColors.Control
                lblTriage.BringToFront()
            End If
            _lblMyPosition.Visible = True
            _lblMyPosition.BackColor = Drawing.Color.Blue
            _lblMyPosition.BringToFront()

        Else
            _blAccelFocusToggle = True
            For Each ctlTmp In _colCtrls
                Select Case TypeName(ctlTmp)
                    Case "Panel"
                        ctlTmp.BackColor = Drawing.Color.PaleTurquoise
                    Case "CheckBox"
                        ctlTmp.BackColor = Drawing.Color.PaleTurquoise
                    Case "Label"
                        If Len(ctlTmp.Text) <= 2 Then
                            ctlTmp.Visible = True
                            ctlTmp.BringToFront()
                        Else
                            ctlTmp.BackColor = Drawing.Color.PaleTurquoise
                        End If
                    Case "TextBox"
                        ctlTmp.BackColor = Drawing.Color.PaleTurquoise
                End Select
            Next ctlTmp
            If _initType.HasFlag(InitTypeEnum.InitSort) Then
                lblConvCt.BackColor = Drawing.Color.PaleTurquoise
                lblTriage.BackColor = Drawing.Color.PaleTurquoise
            End If
            _lblMyPosition.BackColor = Drawing.Color.DarkGreen
            'Modal        With _activeExplorer
            'Modal            .ClearSelection
            'Modal            If .IsItemSelectableInView(mail) Then .AddToSelection mail
            'Modal            'DoEvents
            'Modal        End With
        End If
    End Sub

    Public Sub Mail_Activate()
        Dim objModule As Outlook.MailModule

        On Error Resume Next
        With _activeExplorer


            If .CurrentFolder.DefaultItemType <> OlItemType.olMailItem Then
                .NavigationPane.CurrentModule = .NavigationPane.Modules.GetNavigationModule(OlNavigationModuleType.olModuleMail)
            End If
            If .CurrentView <> "tmpNoConversation" Then
                .CurrentView = "tmpNoConversation"
            End If
            .ClearSelection()
            If .IsItemSelectableInView(Mail) Then .AddToSelection(Mail)
            'DoEvents
        End With
        If Err.Number <> 0 Then
            MsgBox("Error in QF.Mail_Activate: " & Err.Description)
            'Deactivate_Email_Timing_And_Velocity
            Stop
            Err.Clear()
        End If
    End Sub

    Public Sub KB(AccelCode As String)
        Select Case AccelCode
            Case "O"

                LblSubject.ForeColor = Drawing.Color.FromArgb(&H80000012)
                LblSender.ForeColor = Drawing.Color.FromArgb(&H80000012)
                LblSubject.Font = New Font(LblSubject.Font, FontStyle.Regular)
                LblSender.Font = New Font(LblSender.Font, FontStyle.Regular)


            Case "C"
                If _initType.HasFlag(InitTypeEnum.InitSort) Then chk.Checked = Not chk.Checked
            Case "A"
                If _initType.HasFlag(InitTypeEnum.InitSort) Then _chbxSaveAttach.Checked = Not _chbxSaveAttach.Checked
            Case "W"
                If _initType.HasFlag(InitTypeEnum.InitSort) Then _chbxDelFlow.Checked = Not _chbxDelFlow.Checked
            Case "M"
                If _initType.HasFlag(InitTypeEnum.InitSort) Then _chbxSaveMail.Checked = Not _chbxSaveMail.Checked
            Case "T"
                cbFlag_Click()
            Case "F"
                If _initType.HasFlag(InitTypeEnum.InitSort) Then _txt.Focus()
            Case "D"
                If _initType.HasFlag(InitTypeEnum.InitSort) Then cbo.Focus()
            Case "X"
                cbDel_Click()
            Case "R"
                cbKll_Click()
        End Select
    End Sub

    Public Sub ResizeCtrls(intPxChg As Integer)
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

        X1pct *= intPxChg
        X2pct *= intPxChg
        X3pct *= intPxChg
        X1px = Math.Round(X1pct, 0)
        X2px = Math.Round(X2pct, 0)
        X3px = Math.Round(X3pct, 0)

        LblSubject.Width = lblSubject_Width + X1px                      'Subject Width X%
        _cbFlag.Left = cbFlag_Left + X1px + X2px                         'Task button X% + Y% left position
        lblAcT.Left = lblAcT_Left + X1px + X2px                         'Task accelerator X% + Y% left position
        _cbDel.Left = cbDel_Left + X1px + X2px                           'Delete button X+Y% Left position
        _cbKll.Left = cbKll_Left + X1px + X2px                           'Kill button X+Y% Left position
        lblAcX.Left = lblAcX_Left + X1px + X2px
        lblAcR.Left = lblAcR_Left + X1px + X2px
        lblSentOn.Left = lblSentOn_Left + X1px                          'SentOn X% Left Position
        lblActionable.Left = lblActionable_Left + X3px                  '<ACTIONABL> left position + X3px
        lblTriage.Left = lblTriage_Left + X3px                          'Triage left position + X3px


        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            _txt.Left = txt_Left + X1px                                  'Folder search box X% left position Y% Width
            _txt.Width = txt_Width + X2px                                'Folder search box X% left position Y% Width
            _lbl5.Left = lbl5_Left + X1px                                'Folder label X% left position
            lblAcF.Left = lblAcF_Left + X1px                            'F Accelerator X% left position
            lblConvCt.Left = lblConvCt_Left + X1px                      'Conversation Count X% Left Position
            _chbxSaveAttach.Left = chbxSaveAttach_Left + X1px + X2px     'Checkbox Save Attachment X% Left Position
            _chbxSaveMail.Left = chbxSaveMail_Left + X1px + X2px         'Checkbox Save Mail X% Left Position
            _chbxDelFlow.Left = chbxDelFlow_Left + X1px + X2px           'Checkbox Delete Flow X% Left Position
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
                TxtBoxBody.Width = frm.Width - TxtBoxBody.Left - 5
                pos_body.widthOriginal = lblBody_Width + X1px            'Body Width X%

            Else

                cbo.Left = cbo_Left + X1px                               'Dropdown box X% Left position Y% Width
                cbo.Width = cbo_Width + X2px                             'Dropdown box X% Left position Y% Width
                lblAcD.Left = lblAcD_Left + X1px                         'D Accelerator X% left position
                lblAcC.Left = lblAcC_Left + X1px + X2px                  'Conversation accelerator X% Left position
                chk.Left = chk_Left + X1px + X2px                        'Conversation checkbox X% Left Position
                TxtBoxBody.Width = lblBody_Width + X1px                     'Body Width X%

            End If

        Else
            TxtBoxBody.Width = lblBody_Width + X1px + X2px                   'Body Width X%
        End If

    End Sub

    Public Sub ExpandCtrls1()

        Dim lngShift As Long
        'Private pos_lblAcC          As ctrlPosition
        'Private pos_lblAcD          As ctrlPosition
        'Private pos_lblAcO          As ctrlPosition

        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            If blExpanded = False Then
                blExpanded = True
                frm.Height = frm.Height * 2
                lngShift = LblSubject.Top + LblSubject.Height - cbo.Top + 1

                pos_cbo.topOriginal = cbo.Top
                pos_cbo.topNew = pos_cbo.topOriginal + lngShift
                cbo.Top = pos_cbo.topNew

                pos_lblAcD.topOriginal = lblAcD.Top
                lblAcD.Top = pos_lblAcD.topOriginal + lngShift

                pos_cbo.leftOriginal = cbo.Left
                cbo.Left = TxtBoxBody.Left

                pos_lblAcD.leftOriginal = lblAcD.Left
                lblAcD.Left = max(0, cbo.Left - pos_cbo.leftOriginal + pos_lblAcD.leftOriginal)

                pos_cbo.widthOriginal = cbo.Width
                pos_cbo.widthNew = pos_cbo.leftOriginal - cbo.Left + pos_cbo.widthOriginal - lngBlock_Width
                cbo.Width = pos_cbo.widthNew

                lngShift = cbo.Top + cbo.Height - TxtBoxBody.Top + 1

                With pos_body
                    .topOriginal = TxtBoxBody.Top
                    .topNew = .topOriginal + lngShift
                    TxtBoxBody.Top = .topNew

                    pos_lblAcO.topOriginal = lblAcO.Top
                    lblAcO.Top += lngShift

                    .heightOriginal = TxtBoxBody.Height
                    .heightNew = frm.Height - .topNew - 5
                    TxtBoxBody.Height = .heightNew
                    .widthOriginal = TxtBoxBody.Width
                    .widthNew = frm.Width - TxtBoxBody.Left - 5
                    TxtBoxBody.Width = .widthNew
                End With

                chk.Text = ""
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


                pos_chbxSaveAttach.topOriginal = _chbxSaveAttach.Top
                _chbxSaveAttach.Top = pos_cbo.topNew

                pos_chbxSaveMail.topOriginal = _chbxSaveMail.Top
                _chbxSaveMail.Top = pos_cbo.topNew

                pos_chbxDelFlow.topOriginal = _chbxDelFlow.Top
                _chbxDelFlow.Top = pos_cbo.topNew

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

                TxtBoxBody.Top = pos_body.topOriginal
                TxtBoxBody.Height = pos_body.heightOriginal
                TxtBoxBody.Width = pos_body.widthOriginal
                lblAcO.Top = pos_lblAcO.topOriginal

                chk.Text = "  Conversation"
                chk.Left = pos_chk.leftOriginal
                chk.Top = pos_chk.topOriginal
                chk.Width = pos_chk.widthOriginal
                lblAcC.Left = pos_lblAcC.leftOriginal
                lblAcC.Top = pos_lblAcC.topOriginal

                _chbxSaveAttach.Top = pos_chbxSaveAttach.topOriginal
                _chbxSaveMail.Top = pos_chbxSaveMail.topOriginal
                _chbxDelFlow.Top = pos_chbxDelFlow.topOriginal
                lblAcA.Top = pos_lblAcA.topOriginal
                lblAcW.Top = pos_lblAcW.topOriginal
                lblAcM.Top = pos_lblAcM.topOriginal


            End If
        Else
            If blExpanded = False Then
                blExpanded = True
                frm.Height = frm.Height * 2
                With pos_body
                    .topOriginal = TxtBoxBody.Top
                    pos_lblAcO.topOriginal = lblAcO.Top
                    .heightOriginal = TxtBoxBody.Height
                    .heightNew = frm.Height - .topOriginal - 5
                    TxtBoxBody.Height = .heightNew
                End With
            Else
                blExpanded = False
                frm.Height = frm.Height / 2
                With pos_body
                    TxtBoxBody.Top = pos_body.topOriginal
                    TxtBoxBody.Height = pos_body.heightOriginal
                    lblAcO.Top = pos_lblAcO.topOriginal
                End With
            End If


        End If

    End Sub

    Public Sub MoveMail()


        Dim selItems = New Collection()
        Dim loc As String
        Dim myFolder As Outlook.Folder
        Dim MSG As MailItem
        Dim Sel As Collection
        Dim Attchments As Boolean
        Dim blRepullConv As Boolean
        Dim blDoMove As Boolean

        blRepullConv = False

        If Mail IsNot Nothing Then
            If chk.Checked = True Then
                If _selItemsInClass IsNot Nothing Then
                    If _selItemsInClass.Count = CInt(lblConvCt.Text) And _selItemsInClass.Count <> 0 Then
                        selItems = _selItemsInClass
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

                    conv = New cConversation(_globals.Ol.App) With {.item = Mail}
                    selItems = conv.ToCollection(True)
                End If
            Else
                selItems = New Collection From {
                    Mail
                }
            End If

            Attchments = If(cbo.SelectedItem = "Trash to Delete", False, _chbxSaveAttach.Checked)

            blDoMove = True
            On Error Resume Next
            If _fldrOriginal IsNot Mail.Parent Then blDoMove = False
            If Err.Number <> 0 Then
                Err.Clear()
                blDoMove = False
            End If

            If blDoMove Then
                Load_CTF_AND_Subjects_AND_Recents()
                MASTER_SortEmailsToExistingFolder(selItems:=selItems,
                    Pictures_Checkbox:=False,
                    SortFolder:=cbo.SelectedItem,
                    Save_MSG:=_chbxSaveMail.Checked,
                    Attchments:=Attchments,
                    Remove_Flow_File:=_chbxDelFlow.Checked,
                    OlArchiveRootPath:=_globals.Ol.ArchiveRootPath)
                Cleanup_Files()
            End If 'blDoMove

        End If

    End Sub

    Public Sub ctrlsRemove()



        Do While _colCtrls.Count > 1
            frm.Controls.Remove(_colCtrls.Item(_colCtrls.Count))
            _colCtrls.Remove(_colCtrls.Count)
        Loop

        _fldrHandler = Nothing

    End Sub

    Public Sub kill()



        _mPassedControl = Nothing
        chk = Nothing
        cbo = Nothing
        _lst = Nothing
        opt = Nothing
        spn = Nothing
        _txt = Nothing
        frm = Nothing
        _cbKll = Nothing
        Mail = Nothing
        _fldrTarget = Nothing
        _lblTmp = Nothing
        'Set _suggestions = Nothing
        'Set _strFolders = Nothing
        _colCtrls = Nothing
        _fldrHandler = Nothing



    End Sub

    Private Sub bdy_Click()
        LblSubject.ForeColor = Drawing.Color.FromArgb(&H80000012)
        LblSubject.Font = New Font(LblSubject.Font, FontStyle.Regular)
        LblSender.ForeColor = Drawing.Color.FromArgb(&H80000012)
        LblSender.Font = New Font(LblSender.Font, FontStyle.Regular)
        Mail.Display()
        _parent.Parent.QFD_Minimize()
        If _parent.Parent.BlShowInConversations Then _parent.Parent.ExplConvView_ToggleOn()
    End Sub

    Private Sub cbDel_Click()
        cbo.SelectedItem = "Trash to Delete"
    End Sub


    Private Sub cbDel_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub cbDel_KeyPress(sender As Object, e As KeyPressEventArgs)
        KeyPressHandler_Class(sender, e)
    End Sub

    Private Sub cbDel_KeyUp(sender As Object, e As KeyEventArgs)
        'Select Case KeyCode
        '    Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '    Case Else
        'End Select
    End Sub

    Private Sub cbFlag_Click()

        Dim Sel As Collection

        Sel = New Collection From {
            Mail
        }
        Dim flagTask = New TaskVisualization.FlagTasks(AppGlobals:=_globals,
                                                       ItemCollection:=Sel,
                                                       blFile:=False, hWndCaller:=hWndCaller)
        flagTask.Run()
        _cbFlag.Text = "!"

    End Sub

    Private Sub cbFlag_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub cbFlag_KeyPress(sender As Object, e As KeyPressEventArgs)
        KeyPressHandler_Class(sender, e)
    End Sub

    Private Sub cbFlag_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub cbKll_Click()
        _parent.RemoveSpecificControlGroup(intMyPosition)
    End Sub

    Private Sub cbKll_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub cbKll_KeyPress(sender As Object, e As KeyPressEventArgs)
        KeyPressHandler_Class(sender, e)
    End Sub

    Private Sub cbKll_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub cbo_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Return
                If _intEnterCounter = 1 Then
                    _intEnterCounter = 0
                    _parent.Parent.KeyboardHandler_KeyDown(sender, e)
                Else
                    _intEnterCounter = 1
                    _intComboRightCtr = 0
                End If
            Case Else
                _parent.Parent.KeyboardHandler_KeyDown(sender, e)
        End Select
    End Sub



    Private Sub cbo_KeyUp(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Alt
                _parent.Parent.KeyUpHandler(sender, e)
            Case Keys.Escape
                _intEnterCounter = 0
                _intComboRightCtr = 0
            Case Keys.Right
                _intEnterCounter = 0
                If _intComboRightCtr = 0 Then
                    cbo.DroppedDown = True
                    _intComboRightCtr = 1
                ElseIf _intComboRightCtr = 1 Then

                    InitializeSortToExisting(InitType:="Sort",
                        QuickLoad:=False,
                        WholeConversation:=False,
                        strSeed:=cbo.SelectedItem,
                        objItem:=Mail)
                    cbKll_Click()
                Else
                    MsgBox("Error in intComboRightCtr ... setting to 0 and continuing")
                    _intComboRightCtr = 0
                End If
            Case Keys.Left
                _intEnterCounter = 0
                _intComboRightCtr = 0
            Case Keys.Down
                _intEnterCounter = 0
            Case Keys.Up
                _intEnterCounter = 0
        End Select
    End Sub


    Private Sub cbTmp_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub cbTmp_KeyUp(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyUpHandler(sender, e)
    End Sub

    Private Sub chk_Click()

        Dim selItems As Collection
        Dim objItem As Object
        Dim objMail As Outlook.MailItem
        Dim i As Integer
        Dim varList As String()

        'Create a collection with all of the mail items in the conversation in the current folder
        selItems = New Collection

        If _selItemsInClass Is Nothing Then CountMailsInConv()

        For i = 1 To _selItemsInClass.Count
            objItem = _selItemsInClass(i)
            objMail = objItem
            If objMail.EntryID <> Mail.EntryID Then selItems.Add(objItem)
        Next i


        'Set sel = New Collection
        'sel.Add mail
        'Set selItems = Email_SortToExistingFolder.DemoConversation(selItems, sel)

        'Remove the current email from the collection because we will add or subtract the others
        'For i = selItems.Count To 1 Step -1
        '    Set objMail = selItems(i)
        '    If objMail.EntryID = mail.EntryID Then selItems.Remove i
        'Next i

        If chk.Checked = True Then
            _parent.ConvToggle_Group(selItems, intMyPosition)
            lblConvCt.Enabled = True
        Else
            varList = cbo.Items.Cast(Of Object)().[Select](Function(item) item.ToString()).ToArray()
            _parent.ConvToggle_UnGroup(selItems, intMyPosition, CInt(lblConvCt.Text), varList)
            lblConvCt.Enabled = False
        End If



    End Sub

    Private Sub chk_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub chk_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub frm_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub frm_KeyPress(sender As Object, e As KeyPressEventArgs)
        _parent.Parent.KeyPressHandler(sender, e)
    End Sub

    Private Sub frm_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub lst_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub lst_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub opt_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub opt_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub spn_KeyDown(sender As Object, e As KeyEventArgs)
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub spn_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub txt_Change()

        cbo.Items.Clear()
        cbo.Items.AddRange(_fldrHandler.FindFolder("*" & _txt.Text & "*", True, ReCalcSuggestions:=False, objItem:=Mail))

        If cbo.Items.Count >= 2 Then cbo.SelectedIndex = 1

    End Sub


    Private Sub KeyPressHandler_Class(sender As Object, e As KeyPressEventArgs)

    End Sub


    Private Sub txt_KeyDown(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        _parent.Parent.KeyboardHandler_KeyDown(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub txt_KeyPress(sender As Object, e As KeyPressEventArgs)
        _parent.Parent.KeyPressHandler(sender, e)
    End Sub

    Private Sub txt_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        '_parent.toggleAcceleratorDialogue
        _parent.Parent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub


End Class
