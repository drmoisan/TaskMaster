Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook
Imports Microsoft.Office.Interop
Imports ToDoModel
Imports UtilitiesVB
Imports TaskVisualization


Public Class QfcControllerOrig


    Private oParent As Object
    Private InitType As InitTypeEnum
    Private m_PassedControl As Control
    Public WithEvents chk As CheckBox         'Checkbox to Group Conversations
    Public WithEvents cbo As ComboBox         'Combo box containing Folder Suggestions
    Private WithEvents lst As ListBox
    Private WithEvents txt As TextBox          'Input for folder search
    Private WithEvents bdy As TextBox
    Private WithEvents cbKll As Button    'Remove mail from Processing
    Private WithEvents cbDel As Button    'Delete email
    Private WithEvents cbFlag As Button    'Flag as Task
    Private WithEvents cbTmp As Button
    Public WithEvents frm As Panel
    Public Mail As MailItem
    Private fldrOriginal As Folder
    Public intMyPosition As Integer
    Private fldrTarget As Folder
    Private lblTmp As Label
    Public lblConvCt As Label            'Count of Conversation Members
    Private lblMyPosition As Label            'ACCELERATOR Email Position
    Private _suggestions = New cSuggestions()
    'Private _suggestions As Email_AutoCategorize._suggestions
    Private strFolders() As String
    Private colCtrls As Collection
    Private selItems_InClass As Collection
    Private blAccel_FocusToggle As Boolean
    Private intEnterCounter As Integer
    Private intComboRightCtr As Integer
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
    Public txtboxBody As TextBox            '<BODY>
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
    Private opt As System.Windows.Forms.RadioButton
    Private spn As System.Windows.Forms.NumericUpDown


    Private _fldrHandler As cFolderHandler
    Private hWndCaller As IntPtr

    Private p_BoolRemoteMouseApp As Boolean
    Private conv As cConversation

    Private _globals As IApplicationGlobals
    Private _activeExplorer As Outlook.Explorer

    Public Sub New(m_mail As MailItem,
        col As Collection,
        intPositionArg As Integer,
        BoolRemoteMouseApp As Boolean,
        Caller As Object,
        AppGlobals As IApplicationGlobals,
        Optional hwnd As IntPtr = Nothing,
        Optional InitTypeE As InitTypeEnum = InitTypeEnum.InitSort)

        'Procedure Naming

        _globals = AppGlobals
        _activeExplorer = AppGlobals.Ol.App.ActiveExplorer

        Dim ctlTmp As System.Windows.Forms.Control
        Dim strBodyText As String

        InitType = InitTypeE
        oParent = Caller
        intMyPosition = intPositionArg        'call back position in collection
        Mail = m_mail
        fldrOriginal = Mail.Parent
        hWndCaller = hwnd
        colCtrls = col
        For Each ctlTmp In col
            Select Case TypeName(ctlTmp)
                Case "Panel"
                    frm = ctlTmp
                Case "CheckBox"
                    Select Case ctlTmp.Text
                        Case "  Conversation"
                            chk = ctlTmp
                        Case " Attach"
                            chbxSaveAttach = ctlTmp
                        Case " Flow"
                            chbxDelFlow = ctlTmp
                        Case " Mail"
                            chbxSaveMail = ctlTmp
                    End Select
                Case "ComboBox"
                    cbo = ctlTmp
                Case "ListBox"
                    lst = ctlTmp
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
                        bdy = ctlTmp
                        txtboxBody = ctlTmp
                    Else
                        txt = ctlTmp
                    End If

                Case "Label"
                    lblTmp = ctlTmp
                    Select Case lblTmp.Text
                        Case "From:"
                            lbl1 = lblTmp
                        Case "Subject:"
                            lbl2 = lblTmp
                        Case "Body:"
                            lbl3 = lblTmp
                        Case "Sent On:"
                            lbl4 = lblTmp
                        Case "Folder:"
                            lbl5 = lblTmp
                        Case "<SENDER>"
                            lblTmp.Text = If(Mail.Sent = True, GetSenderAddress(Mail), "Draft Message")
                            lblSender = lblTmp
                        Case "<SUBJECT>"
                            lblTmp.Text = Mail.Subject
                            lblSubject = lblTmp
                        Case "ABC"
                            lblTmp.Text = CustomFieldID_GetValue(Mail, "Triage")
                            lblTriage = lblTmp
                        Case "<ACTIONABL>"
                            lblTmp.Text = CustomFieldID_GetValue(Mail, "Actionable")
                            lblActionable = lblTmp
                        Case "<#>"
                            lblConvCt = lblTmp
                        Case "<Pos#>"
                            lblMyPosition = lblTmp
                        Case "<BODY>"

                        Case "<SENTON>"
                            lblTmp.Text = Format(Mail.SentOn, "MM/dd/yy HH:MM")
                            lblSentOn = lblTmp
                        Case "F"
                            lblAcF = lblTmp

                        Case "D"
                            lblAcD = lblTmp
                        Case "C"
                            lblAcC = lblTmp
                        Case "X"
                            lblAcX = lblTmp
                        Case "R"
                            lblAcR = lblTmp
                        Case "T"
                            lblAcT = lblTmp
                        Case "O"
                            lblAcO = lblTmp
                        Case "A"
                            lblAcA = lblTmp
                        Case "W"
                            lblAcW = lblTmp
                        Case "M"
                            lblAcM = lblTmp
                    End Select
                Case "Button"
                    cbTmp = ctlTmp
                    If cbTmp.Text = "X" Then
                        cbDel = ctlTmp
                    ElseIf cbTmp.Text = "-->" Then
                        cbKll = ctlTmp
                    ElseIf cbTmp.Text = "|>" Then
                        cbFlag = ctlTmp
                    End If
            End Select

        Next ctlTmp

        If Mail.UnRead = True Then
            lblSubject.ForeColor = Drawing.Color.Blue
            lblSubject.Font = New Font(lblSubject.Font, FontStyle.Bold)
            lblSender.ForeColor = Drawing.Color.Blue
            lblSender.Font = New Font(lblSender.Font, FontStyle.Bold)
        End If
        lblSubject_Width = lblSubject.Width
        lblBody_Width = txtboxBody.Width
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



        If InitType.HasFlag(InitTypeEnum.InitSort) Then
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
            If InitType.HasFlag(InitTypeEnum.InitSort) Then
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
            If InitType.HasFlag(InitTypeEnum.InitSort) Then
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
                'Next i
            End If


        Else
            'TODO: cSuggestions and cFolderHandler are to mixed up with functionality. Need to clean up.
            _suggestions = Folder_Suggestions(Mail, _globals, False)

            If _suggestions.Count > 0 Then
                ReDim Preserve strFolders(_suggestions.Count)
                For i = 1 To _suggestions.Count
                    strFolders(i) = _suggestions.FolderList(i)
                Next i
                cbo.Items.AddRange(strFolders)
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
        '    If Not objProperty Is Nothing Then txt.Value = objProperty.Value


        'Call Email_SortToExistingFolder.FindFolder("", True, objItem:=mail)




    End Sub

    Friend Sub CountMailsInConv(Optional ct As Integer = 0)



        'Dim Sel As Collection

        If ct <> 0 Then
            lblConvCt.Text = CStr(ct)
        Else
            conv = New cConversation(_globals.Ol.App) With {.item = Mail}
            selItems_InClass = conv.ToCollection(True)
            'Set Sel = New Collection
            'Sel.Add Mail
            'Set selItems_InClass = Email_SortToExistingFolder.DemoConversation(selItems_InClass, Sel)
            lblConvCt.Text = CStr(selItems_InClass.Count)
        End If



    End Sub

    Public Sub Accel_Toggle()
        If lblMyPosition.Enabled = True Then
            If blAccel_FocusToggle Then
                If blExpanded = True Then ExpandCtrls1()
                Accel_FocusToggle()
            End If
            lblMyPosition.Enabled = False
            lblMyPosition.Visible = False
        Else
            lblMyPosition.Text = intMyPosition
            lblMyPosition.Enabled = True
            lblMyPosition.Visible = True
        End If
    End Sub

    Public Sub Accel_FocusToggle()
        Dim ctlTmp As System.Windows.Forms.Control

        If blAccel_FocusToggle Then
            blAccel_FocusToggle = False
            For Each ctlTmp In colCtrls
                Select Case TypeName(ctlTmp)
                    Case "Frame"
                        ctlTmp.BackColor = Drawing.Color.Blue
                    Case "CheckBox"
                        ctlTmp.BackColor = Drawing.Color.Blue
                    Case "Label"
                        If Len(ctlTmp.Text) <= 2 Then
                            ctlTmp.Visible = False
                        Else
                            ctlTmp.BackColor = Drawing.Color.Blue
                        End If
                    Case "TextBox"
                        ctlTmp.BackColor = Drawing.Color.Blue
                End Select
            Next ctlTmp
            If InitType.HasFlag(InitTypeEnum.InitSort) Then
                lblConvCt.Visible = True
                lblConvCt.BackColor = Drawing.Color.Blue
                lblTriage.Visible = True
                lblTriage.BackColor = Drawing.Color.Blue
            End If
            lblMyPosition.Visible = True
            lblMyPosition.BackColor = Drawing.Color.LightBlue

        Else
            blAccel_FocusToggle = True
            For Each ctlTmp In colCtrls
                Select Case TypeName(ctlTmp)
                    Case "Frame"
                        ctlTmp.BackColor = Drawing.Color.FromArgb(&HFFFFC0)
                    Case "CheckBox"
                        ctlTmp.BackColor = Drawing.Color.FromArgb(&HFFFFC0)
                    Case "Label"
                        If Len(ctlTmp.Text) <= 2 Then
                            ctlTmp.Visible = True
                        Else
                            ctlTmp.BackColor = Drawing.Color.FromArgb(&HFFFFC0)
                        End If
                    Case "TextBox"
                        ctlTmp.BackColor = Drawing.Color.FromArgb(&HFFFFC0)
                End Select
            Next ctlTmp
            If InitType.HasFlag(InitTypeEnum.InitSort) Then
                lblConvCt.BackColor = Drawing.Color.FromArgb(&HFFFFC0)
                lblTriage.BackColor = Drawing.Color.FromArgb(&HFFFFC0)
            End If
            lblMyPosition.BackColor = Drawing.Color.FromArgb(&H8000&)
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

                lblSubject.ForeColor = Drawing.Color.FromArgb(&H80000012)
                lblSender.ForeColor = Drawing.Color.FromArgb(&H80000012)
                lblSubject.Font = New Font(lblSubject.Font, FontStyle.Regular)
                lblSender.Font = New Font(lblSender.Font, FontStyle.Regular)
                If InitType.HasFlag(InitTypeEnum.InitSort) Then
                    Mail_Activate()       'For modal code
                Else
                    Mail.Display()
                End If
            'oParent.QFD_Minimize
            'Email_SortToExistingFolder.MailsSelect Email_SortToExistingFolder.MailToCollection(Mail)

            Case "C"
                If InitType.HasFlag(InitTypeEnum.InitSort) Then chk.Checked = Not chk.Checked
            Case "A"
                If InitType.HasFlag(InitTypeEnum.InitSort) Then chbxSaveAttach.Checked = Not chbxSaveAttach.Checked
            Case "W"
                If InitType.HasFlag(InitTypeEnum.InitSort) Then chbxDelFlow.Checked = Not chbxDelFlow.Checked
            Case "M"
                If InitType.HasFlag(InitTypeEnum.InitSort) Then chbxSaveMail.Checked = Not chbxSaveMail.Checked
            Case "T"
                cbFlag_Click()
            Case "F"
                If InitType.HasFlag(InitTypeEnum.InitSort) Then txt.Focus()
            Case "D"
                If InitType.HasFlag(InitTypeEnum.InitSort) Then cbo.Focus()
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


        If InitType.HasFlag(InitTypeEnum.InitSort) Then
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
                txtboxBody.Width = frm.Width - txtboxBody.Left - 5
                pos_body.widthOriginal = lblBody_Width + X1px            'Body Width X%

            Else

                cbo.Left = cbo_Left + X1px                               'Dropdown box X% Left position Y% Width
                cbo.Width = cbo_Width + X2px                             'Dropdown box X% Left position Y% Width
                lblAcD.Left = lblAcD_Left + X1px                         'D Accelerator X% left position
                lblAcC.Left = lblAcC_Left + X1px + X2px                  'Conversation accelerator X% Left position
                chk.Left = chk_Left + X1px + X2px                        'Conversation checkbox X% Left Position
                txtboxBody.Width = lblBody_Width + X1px                     'Body Width X%

            End If

        Else
            txtboxBody.Width = lblBody_Width + X1px + X2px                   'Body Width X%
        End If

    End Sub

    Public Sub ExpandCtrls1()

        Dim lngShift As Long
        'Private pos_lblAcC          As ctrlPosition
        'Private pos_lblAcD          As ctrlPosition
        'Private pos_lblAcO          As ctrlPosition

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
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
                cbo.Left = txtboxBody.Left

                pos_lblAcD.leftOriginal = lblAcD.Left
                lblAcD.Left = max(0, cbo.Left - pos_cbo.leftOriginal + pos_lblAcD.leftOriginal)

                pos_cbo.widthOriginal = cbo.Width
                pos_cbo.widthNew = pos_cbo.leftOriginal - cbo.Left + pos_cbo.widthOriginal - lngBlock_Width
                cbo.Width = pos_cbo.widthNew

                lngShift = cbo.Top + cbo.Height - txtboxBody.Top + 1

                With pos_body
                    .topOriginal = txtboxBody.Top
                    .topNew = .topOriginal + lngShift
                    txtboxBody.Top = .topNew

                    pos_lblAcO.topOriginal = lblAcO.Top
                    lblAcO.Top += lngShift

                    .heightOriginal = txtboxBody.Height
                    .heightNew = frm.Height - .topNew - 5
                    txtboxBody.Height = .heightNew
                    .widthOriginal = txtboxBody.Width
                    .widthNew = frm.Width - txtboxBody.Left - 5
                    txtboxBody.Width = .widthNew
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

                txtboxBody.Top = pos_body.topOriginal
                txtboxBody.Height = pos_body.heightOriginal
                txtboxBody.Width = pos_body.widthOriginal
                lblAcO.Top = pos_lblAcO.topOriginal

                chk.Text = "  Conversation"
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
                    .topOriginal = txtboxBody.Top
                    pos_lblAcO.topOriginal = lblAcO.Top
                    .heightOriginal = txtboxBody.Height
                    .heightNew = frm.Height - .topOriginal - 5
                    txtboxBody.Height = .heightNew
                End With
            Else
                blExpanded = False
                frm.Height = frm.Height / 2
                With pos_body
                    txtboxBody.Top = pos_body.topOriginal
                    txtboxBody.Height = pos_body.heightOriginal
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
                If selItems_InClass IsNot Nothing Then
                    If selItems_InClass.Count = CInt(lblConvCt.Text) And selItems_InClass.Count <> 0 Then
                        selItems = selItems_InClass
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

            Attchments = If(cbo.SelectedItem = "Trash to Delete", False, chbxSaveAttach.Checked)

            blDoMove = True
            On Error Resume Next
            If fldrOriginal IsNot Mail.Parent Then blDoMove = False
            If Err.Number <> 0 Then
                Err.Clear()
                blDoMove = False
            End If

            If blDoMove Then
                Load_CTF_AND_Subjects_AND_Recents()
                MASTER_SortEmailsToExistingFolder(selItems:=selItems,
                    Pictures_Checkbox:=False,
                    SortFolder:=cbo.SelectedItem,
                    Save_MSG:=chbxSaveMail.Checked,
                    Attchments:=Attchments,
                    Remove_Flow_File:=chbxDelFlow.Checked,
                    OlArchiveRootPath:=_globals.Ol.ArchiveRootPath)
                Cleanup_Files()
            End If 'blDoMove

        End If

    End Sub

    Public Sub ctrlsRemove()



        Do While colCtrls.Count > 1
            frm.Controls.Remove(colCtrls.Item(colCtrls.Count).Name)
            colCtrls.Remove(colCtrls.Count)
        Loop

        _fldrHandler = Nothing

    End Sub

    Public Sub kill()



        m_PassedControl = Nothing
        chk = Nothing
        cbo = Nothing
        lst = Nothing
        opt = Nothing
        spn = Nothing
        txt = Nothing
        frm = Nothing
        cbKll = Nothing
        Mail = Nothing
        fldrTarget = Nothing
        lblTmp = Nothing
        'Set _suggestions = Nothing
        'Set strFolders = Nothing
        colCtrls = Nothing
        _fldrHandler = Nothing



    End Sub

    Private Sub bdy_Click()
        lblSubject.ForeColor = Drawing.Color.FromArgb(&H80000012)
        lblSubject.Font = New Font(lblSubject.Font, FontStyle.Regular)
        lblSender.ForeColor = Drawing.Color.FromArgb(&H80000012)
        lblSender.Font = New Font(lblSender.Font, FontStyle.Regular)
        Mail.Display()
        Dim unused1 = oParent.QFD_Minimize
        'TODO: Pass a handler to this class with a known interface
        If oParent.blShowInConversations Then oParent.ExplConvView_ToggleOn
    End Sub

    Private Sub cbDel_Click()
        cbo.SelectedItem = "Trash to Delete"
    End Sub


    Private Sub cbDel_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub cbDel_KeyPress(sender As Object, e As KeyPressEventArgs)
        KeyPressHandler_Class(sender, e)
    End Sub

    Private Sub cbDel_KeyUp(sender As Object, e As KeyEventArgs)
        'Select Case KeyCode
        '    Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
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
        cbFlag.Text = "!"

    End Sub

    Private Sub cbFlag_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub cbFlag_KeyPress(sender As Object, e As KeyPressEventArgs)
        KeyPressHandler_Class(sender, e)
    End Sub

    Private Sub cbFlag_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub cbKll_Click()
        oParent.RemoveSpecificControlGroup(intMyPosition)
    End Sub

    Private Sub cbKll_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub cbKll_KeyPress(sender As Object, e As KeyPressEventArgs)
        KeyPressHandler_Class(sender, e)
    End Sub

    Private Sub cbKll_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub cbo_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Return
                If intEnterCounter = 1 Then
                    intEnterCounter = 0
                    oParent.KeyPressHandler(sender, e)
                Else
                    intEnterCounter = 1
                    intComboRightCtr = 0
                End If
            Case Else
                oParent.KeyDownHandler(sender, e)
        End Select
    End Sub



    Private Sub cbo_KeyUp(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Alt
                oParent.KeyUpHandler(sender, e)
            Case Keys.Escape
                intEnterCounter = 0
                intComboRightCtr = 0
            Case Keys.Right
                intEnterCounter = 0
                If intComboRightCtr = 0 Then
                    cbo.DroppedDown = True
                    intComboRightCtr = 1
                ElseIf intComboRightCtr = 1 Then

                    InitializeSortToExisting(InitType:="Sort",
                        QuickLoad:=False,
                        WholeConversation:=False,
                        strSeed:=cbo.SelectedItem,
                        objItem:=Mail)
                    cbKll_Click()
                Else
                    MsgBox("Error in intComboRightCtr ... setting to 0 and continuing")
                    intComboRightCtr = 0
                End If
            Case Keys.Left
                intEnterCounter = 0
                intComboRightCtr = 0
            Case Keys.Down
                intEnterCounter = 0
            Case Keys.Up
                intEnterCounter = 0
        End Select
    End Sub


    Private Sub cbTmp_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub cbTmp_KeyUp(sender As Object, e As KeyEventArgs)
        oParent.KeyUpHandler(sender, e)
    End Sub

    Private Sub chk_Click()

        Dim selItems As Collection
        Dim objItem As Object
        Dim objMail As Outlook.MailItem
        Dim i As Integer
        Dim varList As String()

        'Create a collection with all of the mail items in the conversation in the current folder
        selItems = New Collection

        If selItems_InClass Is Nothing Then CountMailsInConv()

        For i = 1 To selItems_InClass.Count
            objItem = selItems_InClass(i)
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
            oParent.ConvToggle_Group(selItems, intMyPosition)
            lblConvCt.Enabled = True
        Else
            varList = cbo.Items.Cast(Of Object)().[Select](Function(item) item.ToString()).ToArray()
            oParent.ConvToggle_UnGroup(selItems, intMyPosition, CInt(lblConvCt.Text), varList)
            lblConvCt.Enabled = False
        End If



    End Sub

    Private Sub chk_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub chk_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub frm_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub frm_KeyPress(sender As Object, e As KeyPressEventArgs)
        oParent.KeyPressHandler(sender, e)
    End Sub

    Private Sub frm_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub lst_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub lst_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub opt_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub opt_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub spn_KeyDown(sender As Object, e As KeyEventArgs)
        oParent.KeyDownHandler(sender, e)
    End Sub

    Private Sub spn_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub txt_Change()

        cbo.Items.Clear()
        cbo.Items.AddRange(_fldrHandler.FindFolder("*" & txt.Text & "*", True, ReCalcSuggestions:=False, objItem:=Mail))

        If cbo.Items.Count >= 2 Then cbo.SelectedIndex = 1

    End Sub


    Private Sub KeyPressHandler_Class(sender As Object, e As KeyPressEventArgs)

    End Sub


    Private Sub txt_KeyDown(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        oParent.KeyDownHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub

    Private Sub txt_KeyPress(sender As Object, e As KeyPressEventArgs)
        oParent.KeyPressHandler(sender, e)
    End Sub

    Private Sub txt_KeyUp(sender As Object, e As KeyEventArgs)
        '    Select Case KeyCode
        '        Case 18
        'oParent.toggleAcceleratorDialogue
        oParent.KeyUpHandler(sender, e)
        '        Case Else
        '    End Select
    End Sub
End Class
