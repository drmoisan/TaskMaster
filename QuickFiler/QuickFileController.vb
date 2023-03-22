Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports ToDoModel
Imports UtilitiesVB
Imports Windows.Win32


Public Class QuickFileController

#Region "State Variables"
    'Private state variables
    Private _folderCurrent As Folder
    Private _intUniqueItemCounter As Integer
    Private _intEmailStart As Integer
    Private _intEmailPosition As Integer
    Private _intEmailsPerIteration As Integer
    Private _lngAcceleratorDialogueTop As Long
    Private _lngAcceleratorDialogueLeft As Long
    Private _intAccActiveMail As Integer
    Private _blSuppressEvents As Boolean
    Private _blRunningModalCode As Boolean = False
    Private _boolRemoteMouseApp As Boolean
    Private _stopWatch As cStopWatch

    'Public state variables
    Public BlFrmKll As Boolean
    Public BlShowInConversations As Boolean
#End Region
#Region "Outlook View Variables"
    Public ObjView As Microsoft.Office.Interop.Outlook.View
    Private _objViewMem As String
    Public ObjViewTemp As Microsoft.Office.Interop.Outlook.View
#End Region
#Region "Resizing Variables"
    'Left and Width Constants
    Private _heightFormMax As Long
    Private _heightFormMin As Long
    Private _heightPanelMainMax As Long
    Private _heightPanelMainMin As Long
    Private _lngPanelMainSCTop As Long
    Private _lngTopButtonOkMin As Long
    Private _lngTopButtonCancelMin As Long
    Private _lngTopButtonUndoMin As Long
    Private _lngTopAcceleratorDialogueMin As Long
    Private _lngTopSpnMin As Long
#End Region
#Region "Global Variables, Window Handles and Collections"
    'Globals
    Private _globals As IApplicationGlobals
    Private ReadOnly _activeExplorer As Outlook.Explorer
    Private ReadOnly _olObjects As IOlObjects
    Private ReadOnly _olApp As Outlook.Application
    Private ReadOnly _viewer As QuickFileViewer
    Private _movedMails As cStackObject
    Public InitType As InitTypeEnum

    'Collections
    Public ColQFClass As Collection
    Public ColFrames As Collection
    Public ColMailJustMoved As Collection
    Private _colEmailsInFolder As Collection
    Friend WithEvents Frm As Panel

    'Window Handles
    Private _olAppHWnd As IntPtr
    Private _lFormHandle As IntPtr
#End Region

    Public Sub New(AppGlobals As IApplicationGlobals,
                   Viewer As QuickFileViewer,
                   ColEmailsInFolder As Collection)

        'Link viewer to controller
        _viewer = Viewer
        _viewer.SetController(Me)

        'Link model to controller
        _colEmailsInFolder = ColEmailsInFolder
        InitializeModelProcessingMetrics()

        'Link controller to global variables 
        _globals = AppGlobals
        With AppGlobals
            _olObjects = .Ol
            _olApp = .Ol.App
            _activeExplorer = .Ol.App.ActiveExplorer()
            _folderCurrent = _activeExplorer.CurrentFolder
            _movedMails = .Ol.MovedMails_Stack
        End With

        'Set readonly window handles
        _lFormHandle = _viewer.Handle
        _olAppHWnd = PInvoke.GetAncestor(_lFormHandle, UI.WindowsAndMessaging.GET_ANCESTOR_FLAGS.GA_PARENT)

        InitializeFormConfigurations()
        _viewer.Show()

        Iterate()
    End Sub

    Private Sub InitializeModelProcessingMetrics()
        _intEmailStart = 0       'Reverse sort is 0   'Regular sort is 1
        _intEmailPosition = 0    'Reverse sort is 0   'Regular sort is 1
    End Sub

    Private Sub InitializeFormConfigurations()
        'Set conversation state variable with initial state
        BlShowInConversations = CurrentConversationState
        If BlShowInConversations Then _objViewMem = _activeExplorer.CurrentView.Name

        'Suppress events while initializing form
        _blSuppressEvents = True

        'Configure viewer for SORTING rather than FINDING items
        InitType = InitTypeEnum.InitSort

        RemoveControlsTabstops()
        InitializeToleranceMinimums()
        _heightPanelMainMax = ResizeForToleranceMax()

        'Calculate the emails per page based on screen settings
        _intEmailsPerIteration = CInt(Math.Round(_heightPanelMainMax / (frmHt + frmSp), 0))
        _viewer.L1v2L2h5_SpnEmailPerLoad.Value = _intEmailsPerIteration

        _blSuppressEvents = False
    End Sub

    Private ReadOnly Property CurrentConversationState As Boolean
        Get
            If _activeExplorer.CommandBars.GetPressedMso("ShowInConversations") Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Private Sub RemoveControlsTabstops()
        'Set defaults for controls on main form
        _viewer.L1v2L2h3_ButtonOK.TabStop = False
        _viewer.L1v2L2h4_ButtonCancel.TabStop = False
        _viewer.L1v2L2h4_ButtonUndo.TabStop = False
        _viewer.L1v1L2_PanelMain.TabStop = False
        _viewer.AcceleratorDialogue.TabStop = True
        _viewer.L1v2L2h5_SpnEmailPerLoad.TabStop = False
    End Sub

    Private Sub InitializeToleranceMinimums()
        _lngPanelMainSCTop = 0
        _heightFormMin = _viewer.Height + frmHt + frmSp
        _heightPanelMainMin = frmHt + frmSp
        _lngTopButtonOkMin = _viewer.L1v2L2h3_ButtonOK.Top
        _lngTopButtonCancelMin = _viewer.L1v2L2h4_ButtonCancel.Top
        _lngTopButtonUndoMin = _viewer.L1v2L2h4_ButtonUndo.Top
        _lngTopAcceleratorDialogueMin = _viewer.AcceleratorDialogue.Top
        Dim _screen = Screen.FromControl(_viewer)
        _heightFormMax = _screen.WorkingArea.Height
    End Sub

    Private Function ResizeForToleranceMax() As Long
        'Resize form
        Dim lngPreviousHeight As Long
        Dim lngHeightDifference As Long
        lngHeightDifference = _heightFormMin - _viewer.Height
        _viewer.L1v2L2h3_ButtonOK.Top = _viewer.L1v2L2h3_ButtonOK.Top + lngHeightDifference
        _viewer.L1v2L2h4_ButtonCancel.Top = _viewer.L1v2L2h4_ButtonCancel.Top + lngHeightDifference
        _viewer.L1v2L2h4_ButtonUndo.Top = _viewer.L1v2L2h4_ButtonUndo.Top + lngHeightDifference
        _lngAcceleratorDialogueTop = _viewer.AcceleratorDialogue.Top + lngHeightDifference
        _viewer.AcceleratorDialogue.Top = _lngAcceleratorDialogueTop
        _viewer.L1v2L2h5_SpnEmailPerLoad.Top = _viewer.L1v2L2h5_SpnEmailPerLoad.Top + lngHeightDifference
        _lngTopSpnMin = _viewer.L1v2L2h5_SpnEmailPerLoad.Top
        _lngAcceleratorDialogueLeft = _viewer.AcceleratorDialogue.Left

        'Resize form
        lngPreviousHeight = _viewer.Height
        _viewer.Height = _heightFormMax
        lngHeightDifference = _viewer.Height - lngPreviousHeight
        _viewer.L1v2L2h3_ButtonOK.Top = _viewer.L1v2L2h3_ButtonOK.Top + lngHeightDifference
        _viewer.L1v2L2h4_ButtonCancel.Top = _viewer.L1v2L2h4_ButtonCancel.Top + lngHeightDifference
        _viewer.L1v2L2h4_ButtonUndo.Top = _viewer.L1v2L2h4_ButtonUndo.Top + lngHeightDifference
        _lngAcceleratorDialogueTop = _viewer.AcceleratorDialogue.Top + lngHeightDifference
        _viewer.AcceleratorDialogue.Top = _lngAcceleratorDialogueTop
        _lngAcceleratorDialogueLeft = _viewer.AcceleratorDialogue.Left
        _viewer.L1v2L2h5_SpnEmailPerLoad.Top = _viewer.L1v2L2h5_SpnEmailPerLoad.Top + lngHeightDifference

        'Set Max Size of the main panel based on resizing
        _viewer.L1v1L2_PanelMain.Height += lngHeightDifference

        Return _viewer.L1v1L2_PanelMain.Height
    End Function


    Public Sub Iterate()
        _stopWatch = New cStopWatch
        _stopWatch.Start()
        Dim colEmails As Collection = DequeNextEmailGroup(_colEmailsInFolder, _intEmailsPerIteration)
        LoadControlsAndHandlers(colEmails)
    End Sub

    Private Function DequeNextEmailGroup(ByRef MasterQueue As Collection, Quantity As Integer) As Collection
        Dim i As Integer
        Dim max As Double

        Dim colEmails As Collection

        colEmails = New Collection
        max = If(Quantity < MasterQueue.Count, Quantity, MasterQueue.Count)

        For i = 1 To max
            colEmails.Add(MasterQueue(i))
        Next i
        For i = max To 1 Step -1
            MasterQueue.Remove(i)
        Next i

        Return colEmails
    End Function

    Public Sub LoadControlsAndHandlers(colEmails As Collection)
        Dim objItem As Object
        Dim Mail As [MailItem]
        Dim QF As QfcController
        Dim colCtrls As Collection
        Dim blDebug As Boolean

        blDebug = False

        BlFrmKll = False

        ColQFClass = New Collection
        ColFrames = New Collection

        _intUniqueItemCounter = 0

        For Each objItem In colEmails
            If TypeOf objItem Is MailItem Then
                _intUniqueItemCounter += 1
                Mail = objItem
                colCtrls = New Collection
                LoadGroupOfCtrls(colCtrls, _intUniqueItemCounter)
                QF = New QfcController(Mail, colCtrls, _intUniqueItemCounter, _boolRemoteMouseApp, Caller:=Me, AppGlobals:=_globals, hwnd:=_lFormHandle, InitTypeE:=InitType)

                ColQFClass.Add(QF)
            End If
        Next objItem

        _viewer.WindowState = FormWindowState.Maximized
        'ShowWindow(_lFormHandle, SW_SHOWMAXIMIZED)

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            'ToggleOffline
            For Each QF In ColQFClass
                QF.Init_FolderSuggestions()
                QF.CountMailsInConv()
                'DoEvents
            Next QF
            'ToggleOffline
        End If

        _intAccActiveMail = 0

        If _blSuppressEvents Then
            _blSuppressEvents = False
            FormResize()
            _blSuppressEvents = True
        Else
            FormResize()
        End If


        'Modal    SendMessage _lFormHandle, WM_SETFOCUS, 0&, 0&

        'EnableWindow(_olAppHWnd, Modal)
        'EnableWindow _lFormHandle, Modeless
        _viewer.L1v1L2_PanelMain.Focus()
    End Sub

#Region "Data Model Manipulation"

    Private Sub EliminateDuplicateConversationIDs(ByRef colTemp As Collection)
        Dim dictID As New Dictionary(Of String, Integer)
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
                Dim unused = colTemp.Remove(objItem)
                dictID(objItem.ConversationID) = dictID(objItem.ConversationID) - 1
            End If
        Next i
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

    Private Sub DebugOutPutEmailCollection(colTemp As Collection)
        Dim objItem As Object
        Dim OlMail As MailItem
        Dim OlAppt As MeetingItem
        Dim strLine As String
        Dim i As Integer

        i = 0
        For Each objItem In colTemp
            i += 1
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



#End Region

#Region "Control Group UI Operations"
    Private Sub LoadGroupOfCtrls(ByRef colCtrls As Collection,
    intItemNumber As Integer,
    Optional intPosition As Integer = 0,
    Optional blGroupConversation As Boolean = True,
    Optional blWideView As Boolean = False)


        'Dim Frm As frame
        Dim lbl1 = New Label
        Dim lbl2 = New Label
        Dim lbl3 = New Label
        Dim lbl5 = New Label
        Dim lblSender = New Label
        Dim lblSubject = New Label
        Dim txtboxBody = New TextBox
        Dim lblSentOn = New Label
        Dim lblConvCt = New Label
        Dim lblPos = New Label
        Dim cbxFolder = New ComboBox
        Dim inpt = New TextBox
        Dim chbxGPConv = New CheckBox
        Dim chbxSaveAttach = New CheckBox
        Dim chbxSaveMail = New CheckBox
        Dim chbxDelFlow = New CheckBox
        Dim cbDelItem = New Button
        Dim cbKllItem = New Button
        Dim cbFlagItem = New Button
        Dim lblAcF = New Label
        Dim lblAcD = New Label
        Dim lblAcC = New Label
        Dim lblAcX = New Label
        Dim lblAcR = New Label
        Dim lblAcT = New Label
        Dim lblAcO = New Label
        Dim lblAcA = New Label
        Dim lblAcW = New Label
        Dim lblAcM = New Label


        Dim lngTopOff As Long

        Dim blDebug As Boolean


        blDebug = False

        lngTopOff = If(blWideView, Top_Offset, Top_Offset_C)
        'Button_OK.top = Button_OK.top + frmHt + frmSp
        'BUTTON_CANCEL.top = BUTTON_CANCEL.top + frmHt + frmSp

        If intPosition = 0 Then intPosition = intItemNumber

        If ((intItemNumber * (frmHt + frmSp)) + frmSp) > _viewer.L1v1L2_PanelMain.Height Then      'Was _heightPanelMainMax but I replaced with Me.Height
            _viewer.L1v1L2_PanelMain.AutoScroll = True
            '_viewer.L1v1L2_PanelMain.ScrollHeight = (intItemNumber * (frmHt + frmSp)) + frmSp 'PanelMain.ScrollHeight + frmHt + frmSp
        End If

        'Min Me Size is frmSp * 2 + frmHt
        Frm = New Panel()
        _viewer.L1v1L2_PanelMain.Controls.Add(Frm)
        With Frm
            .Height = frmHt
            .Top = ((frmSp + frmHt) * (intPosition - 1)) + frmSp
            .Left = frmLt
            .Width = frmWd
            .TabStop = False

        End With
        colCtrls.Add(Frm, "frm")

        If blWideView Then
            lbl1 = New Label
            Frm.Controls.Add(lbl1)
            With lbl1
                .Height = 16
                .Top = lngTopOff
                .Left = 6
                .Width = 54
                .Text = "From:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl1, "lbl1")
        End If  'blWideView

        If blWideView Then
            lbl2 = New Label
            Frm.Controls.Add(lbl2)
            With lbl2
                .Height = 16
                .Top = lngTopOff + 32
                .Left = 6
                .Width = 54
                .Text = "Subject:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl2, "lbl2")
        End If  'blWideView

        If blWideView Then
            lbl3 = New Label
            Frm.Controls.Add(lbl3)
            With lbl3
                .Height = 16
                .Top = lngTopOff + 48
                .Left = 6
                .Width = 54
                .Text = "Body:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl3, "lbl3")
        End If

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            'TURN OFF IF CONDITIONAL REMINDER
            lbl5 = New Label
            Frm.Controls.Add(lbl5)

            With lbl5
                .Height = 16
                .Top = lngTopOff
                .Left = 372
                .Width = 78
                .Text = "Folder:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl5, "lbl5")
        End If

        lblSender = New Label
        Frm.Controls.Add(lblSender)

        With lblSender
            .Height = 16
            .Top = lngTopOff

            If blWideView Then
                .Left = Left_lblSender
                .Width = Width_lblSender
            Else
                .Left = Left_lblSender_C
                .Width = Width_lblSender_C
            End If  'blWideView


            .Text = "<SENDER>"
            .Font = New Font(.Font.FontFamily, 10)
        End With
        colCtrls.Add(lblSender, "lblSender")


        Dim lblTriage As Label = New Label
        Frm.Controls.Add(lblTriage)

        With lblTriage
            .Height = 16
            .Top = lngTopOff

            If blWideView Then
                .Left = Left_lblSender
                .Width = Width_lblSender
            Else
                .Left = Left_lblTriage
                .Width = Width_lblTriage
            End If  'blWideView


            .Text = "ABC"
            .Font = New Font(.Font.FontFamily, 10)
        End With
        colCtrls.Add(lblTriage, "lblTriage")



        Dim lblActionable As Label = New Label
        Frm.Controls.Add(lblActionable)

        With lblActionable
            .Height = 16
            .Top = lngTopOff

            If blWideView Then
                .Left = Left_lblSender
                .Width = Width_lblSender
            Else
                .Left = Left_lblActionable
                .Width = Width_lblActionable
            End If


            .Text = "<ACTIONABL>"
            .Font = New Font(.Font.FontFamily, 10)
        End With
        colCtrls.Add(lblActionable, "lblActionable")



        lblSubject = New Label
        Frm.Controls.Add(lblSubject)

        With lblSubject
            If blWideView Then
                .Height = 16
                .Top = lngTopOff + 32
                .Left = Left_lblSubject
                .Width = Width_lblSubject
                .Font = New Font(.Font.FontFamily, 10)
            ElseIf InitType.HasFlag(InitTypeEnum.InitConditionalReminder) Then
                .Height = 24
                .Top = lngTopOff + 16
                .Left = Left_lblSubject_C
                .Width = frmWd - .Left - .Left
                .Font = New Font(.Font.FontFamily, 16)
            Else
                .Height = 24
                .Top = lngTopOff + 16
                .Left = Left_lblSubject_C
                .Width = Width_lblSubject_C
                .Font = New Font(.Font.FontFamily, 16)
            End If

            .Text = "<SUBJECT>"
        End With
        colCtrls.Add(lblSubject, "lblSubject")

        txtboxBody = New TextBox
        Frm.Controls.Add(txtboxBody)
        With txtboxBody

            If blWideView Then
                .Top = lngTopOff + 36
                .Left = Left_lblBody
                .Width = Width_lblBody
                .Height = 40 + 8 - lngTopOff
            ElseIf InitType.HasFlag(InitTypeEnum.InitConditionalReminder) Then
                .Top = lngTopOff + 40
                .Left = Left_lblBody_C
                .Width = frmWd - .Left - .Left
                .Height = 36 + 8 - lngTopOff
            Else
                .Top = lngTopOff + 40
                .Left = Left_lblBody_C
                .Width = Width_lblBody_C
                .Height = 36 + 8 - lngTopOff

            End If

            .Text = "<BODY>"
            .Font = New Font(.Font.FontFamily, 10)
            .WordWrap = True
        End With
        colCtrls.Add(txtboxBody, "lblBody")

        lblSentOn = New Label
        Frm.Controls.Add(lblSentOn)
        With lblSentOn
            .Height = 16
            If blWideView Then
                .Top = lngTopOff + 16
                .Left = Left_lblSentOn
                .TextAlign = ContentAlignment.TopLeft 'fmTextAlignLeft
            Else
                .Top = lngTopOff
                .Left = Left_lblSentOn_C
                .TextAlign = ContentAlignment.TopRight 'fmTextAlignRight
            End If

            .Width = 156
            .Text = "<SENTON>"
            .Font = New Font(.Font.FontFamily, 10)
        End With
        colCtrls.Add(lblSentOn, "lblSentOn")

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            cbxFolder = New ComboBox
            Frm.Controls.Add(cbxFolder)
            With cbxFolder
                .Height = 24
                .Top = 27 + lngTopOff
                .Left = Left_cbxFolder
                .Width = Width_cbxFolder
                .Font = New Font(.Font.FontFamily, 8)
                .TabStop = False
            End With
            colCtrls.Add(cbxFolder, "cbxFolder")
        End If


        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            inpt = New TextBox
            Frm.Controls.Add(inpt)
            With inpt
                .Height = 24
                .Top = lngTopOff
                .Left = 408
                .Width = Width_inpt
                .Font = New Font(.Font.FontFamily, 10)
                .TabStop = False
                .BackColor = SystemColors.Control

            End With
            colCtrls.Add(inpt, "inpt")



            chbxSaveMail = New CheckBox
            Frm.Controls.Add(chbxSaveMail)
            With chbxSaveMail

                .Height = 16
                .Width = 37
                .Font = New Font(.Font.FontFamily, 10)
                .Text = " Mail"
                .Checked = False
                .TabStop = False
                If blWideView Then

                Else
                    .Top = 47 + lngTopOff
                    .Left = Right_Aligned - .Width
                End If
            End With
            colCtrls.Add(chbxSaveMail, "chbxSaveMail")

            chbxDelFlow = New CheckBox
            Frm.Controls.Add(chbxDelFlow)
            With chbxDelFlow

                .Height = 16
                .Width = 45
                .Font = New Font(.Font.FontFamily, 10)
                .Text = " Flow"
                .Checked = False
                .TabStop = False

                If blWideView Then

                Else
                    .Top = 47 + lngTopOff
                    .Left = chbxSaveMail.Left - .Width - 1
                End If

            End With
            colCtrls.Add(chbxDelFlow, "chbxDelFlow")

            chbxSaveAttach = New CheckBox
            Frm.Controls.Add(chbxSaveAttach)
            With chbxSaveAttach

                .Height = 16
                .Width = 50
                .Font = New Font(.Font.FontFamily, 10)
                .Text = " Attach"
                .Checked = True
                .TabStop = False

                If blWideView Then

                Else
                    .Top = 47 + lngTopOff
                    .Left = chbxDelFlow.Left - .Width - 1
                End If

            End With
            colCtrls.Add(chbxSaveAttach, "chbxSaveAttach")
            chbxGPConv = New CheckBox
            Frm.Controls.Add(chbxGPConv)
            With chbxGPConv
                .Height = 16
                .Width = 81
                .Font = New Font(.Font.FontFamily, 10)
                .Text = "  Conversation"
                .Checked = blGroupConversation
                .TabStop = False
                If blWideView Then
                    .Top = lngTopOff
                    .Left = Left_chbxGPConv
                Else
                    .Top = 47 + lngTopOff
                    .Left = chbxSaveAttach.Left - .Width - 1
                End If
            End With
            colCtrls.Add(chbxGPConv, "chbxGPConv")
        End If

        cbFlagItem = New Button
        Frm.Controls.Add(cbFlagItem)
        With cbFlagItem
            .Height = 24
            .Top = lngTopOff
            .Left = Left_cbFlagItem
            .Width = Width_cb
            .Font = New Font(.Font.FontFamily, 8)
            .Text = "|>"
            .BackColor = SystemColors.Control
            .ForeColor = SystemColors.ControlText
            .TabStop = False
        End With
        colCtrls.Add(cbFlagItem, "cbFlagItem")

        cbKllItem = New Button
        Frm.Controls.Add(cbKllItem)
        With cbKllItem
            .Height = 24
            .Top = lngTopOff
            .Left = cbFlagItem.Left + Width_cb + 2
            .Width = Width_cb
            .Font = New Font(.Font.FontFamily, 8)
            .Text = "-->"
            .BackColor = SystemColors.Control
            .ForeColor = SystemColors.ControlText
            .TabStop = False
        End With
        colCtrls.Add(cbKllItem, "cbKllItem")

        cbDelItem = New Button
        Frm.Controls.Add(cbDelItem)
        With cbDelItem
            .Height = 24
            .Top = lngTopOff
            .Left = cbKllItem.Left + Width_cb + 2
            .Width = Width_cb
            .Font = New Font(.Font.FontFamily, 8)
            .Text = "X"
            .BackColor = Color.Red
            .ForeColor = Color.White
            .TabStop = False
        End With
        colCtrls.Add(cbDelItem, "cbDelItem")

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            lblConvCt = New Label
            Frm.Controls.Add(lblConvCt)
            With lblConvCt
                .Height = 24
                .TextAlign = ContentAlignment.TopRight 'fmTextAlignRight

                If blWideView Then
                    .Left = Left_lblConvCt
                    .Top = lngTopOff
                Else
                    .Left = Left_lblConvCt_C
                    .Top = lngTopOff + 16
                End If
                .Width = 36
                .Text = "<#>"
                If blWideView Then
                    .Font = New Font(.Font.FontFamily, 12)
                Else
                    .Font = New Font(.Font.FontFamily, 16)
                End If

                .Enabled = blGroupConversation

            End With
            colCtrls.Add(lblConvCt, "lblConvCt")
        End If

        lblPos = New Label
        Frm.Controls.Add(lblPos)
        With lblPos
            .Height = 20
            .Top = lngTopOff

            .Left = If(blWideView, 6, 0)

            .Width = 20
            .Text = "<Pos#>"
            .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            .BackColor = SystemColors.ControlText
            .ForeColor = SystemColors.Control
            .Enabled = False
            .Visible = blDebug
        End With
        colCtrls.Add(lblPos, "lblPos")

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            lblAcF = New Label
            Frm.Controls.Add(lblAcF)
            With lblAcF
                .Height = 14
                .Top = max(lngTopOff - 2, 0)
                .Left = 363
                .Width = 14
                .Text = "F"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
                .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
                .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
                '.SpecialEffect = fmSpecialEffectBump
                .BackColor = SystemColors.ControlText
                .ForeColor = SystemColors.Control
                .Visible = blDebug

            End With
            colCtrls.Add(lblAcF, "lblAcF")

            lblAcD = New Label
            Frm.Controls.Add(lblAcD)
            With lblAcD
                .Height = 14
                .Top = 20 + lngTopOff
                .Left = 363
                .Width = 14
                .Text = "D"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
                .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
                .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
                '.SpecialEffect = fmSpecialEffectBump
                .BackColor = SystemColors.ControlText
                .ForeColor = SystemColors.Control
                .Visible = blDebug
            End With
            colCtrls.Add(lblAcD, "lblAcD")

            lblAcC = New Label
            Frm.Controls.Add(lblAcC)
            With lblAcC
                .Height = 14
                .Top = lngTopOff + 47
                .Left = chbxGPConv.Left + 12
                .Width = 14
                .Text = "C"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
                .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
                .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
                '.SpecialEffect = fmSpecialEffectBump
                .BackColor = SystemColors.ControlText
                .ForeColor = SystemColors.Control
                .Visible = blDebug
            End With
            colCtrls.Add(lblAcC, "lblAcC")
        End If

        lblAcR = New Label
        Frm.Controls.Add(lblAcR)
        With lblAcR
            .Height = 14
            .Top = 2 + lngTopOff
            .Left = cbKllItem.Left + 6
            .Width = 14
            .Text = "R"
            .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
            .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
            '.SpecialEffect = fmSpecialEffectBump
            .BackColor = SystemColors.ControlText
            .ForeColor = SystemColors.Control
            .Visible = blDebug
        End With
        colCtrls.Add(lblAcR, "lblAcR")

        lblAcX = New Label
        Frm.Controls.Add(lblAcX)
        With lblAcX
            .Height = 14
            .Top = 2 + lngTopOff
            .Left = cbDelItem.Left + 6
            .Width = 14
            .Text = "X"
            .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
            .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
            '.SpecialEffect = fmSpecialEffectBump
            .BackColor = SystemColors.ControlText
            .ForeColor = SystemColors.Control
            .Visible = blDebug
        End With
        colCtrls.Add(lblAcX, "lblAcX")

        lblAcT = New Label
        Frm.Controls.Add(lblAcT)
        With lblAcT
            .Height = 14
            .Top = 2 + lngTopOff
            .Left = cbFlagItem.Left + 6
            .Width = 14
            .Text = "T"
            .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
            .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
            '.SpecialEffect = fmSpecialEffectBump
            .BackColor = SystemColors.ControlText
            .ForeColor = SystemColors.Control
            .Visible = blDebug
        End With
        colCtrls.Add(lblAcT, "lblAcT")

        lblAcO = New Label
        Frm.Controls.Add(lblAcO)
        With lblAcO
            .Height = 14

            If blWideView Then
                .Top = 36 + lngTopOff
                .Left = Left_lblAcO_C
            Else
                .Top = txtboxBody.Top
                .Left = Left_lblAcO_C
            End If
            .Width = 14
            .Text = "O"
            .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
            .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
            '.SpecialEffect = fmSpecialEffectBump
            .BackColor = SystemColors.ControlText
            .ForeColor = SystemColors.Control
            .Visible = blDebug
        End With
        colCtrls.Add(lblAcO, "lblAcO")

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            lblAcA = New Label
            Frm.Controls.Add(lblAcA)
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
                .Text = "A"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
                .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
                .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
                '.SpecialEffect = fmSpecialEffectBump
                .BackColor = SystemColors.ControlText
                .ForeColor = SystemColors.Control
                .Visible = blDebug
            End With
            colCtrls.Add(lblAcA, "lblAcA")

            lblAcW = New Label
            Frm.Controls.Add(lblAcW)
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
                .Text = "W"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
                .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
                .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
                '.SpecialEffect = fmSpecialEffectBump
                .BackColor = SystemColors.ControlText
                .ForeColor = SystemColors.Control
                .Visible = blDebug
            End With
            colCtrls.Add(lblAcW, "lblAcW")

            lblAcM = New Label
            Frm.Controls.Add(lblAcM)
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
                .Text = "M"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
                .BorderStyle = BorderStyle.Fixed3D 'fmBorderStyleSingle
                .TextAlign = ContentAlignment.TopCenter  'fmTextAlignCenter
                '.SpecialEffect = fmSpecialEffectBump
                .BackColor = SystemColors.ControlText
                .ForeColor = SystemColors.Control
                .Visible = blDebug
            End With
            colCtrls.Add(lblAcM, "lblAcM")
        End If



    End Sub

    Private Sub RemoveControls()


        Dim QF As QfcController
        Dim i As Integer

        'max = ColQFClass.Count
        'For i = max To 1 Step -1
        If ColQFClass IsNot Nothing Then
            Do While ColQFClass.Count > 0
                i = ColQFClass.Count
                QF = ColQFClass(i)
                QF.ctrlsRemove()                                  'Remove controls on the frame
                _viewer.L1v1L2_PanelMain.Controls.Remove(QF.frm)           'Remove the frame
                QF.kill()                                         'Remove the variables linking to events

                'PanelMain.Controls.Remove ColFrames(i).Name
                ColQFClass.Remove(i)
            Loop
        End If


        '_viewer.L1v1L2_PanelMain.ScrollHeight = _heightPanelMainMax



    End Sub

    Public Sub MoveDownControlGroups(intPosition As Integer, intMoves As Integer)

        Dim i As Integer
        Dim QF As QfcController
        Dim ctlFrame As Panel

        For i = ColQFClass.Count To intPosition Step -1

            'Shift items downward if there are any
            QF = ColQFClass(i)
            QF.intMyPosition += intMoves
            ctlFrame = QF.frm
            ctlFrame.Top = ctlFrame.Top + (intMoves * (frmHt + frmSp))
        Next i
        'PanelMain.ScrollHeight = max((intMoves + ColQFClass.Count) * (frmHt + frmSp), _heightPanelMainMax)


    End Sub

    Public Sub ToggleRemoteMouseLabels()
        _boolRemoteMouseApp = Not _boolRemoteMouseApp

        Dim QF As QfcController

        For Each QF In ColQFClass
            QF.ToggleRemoteMouseAppLabels()
        Next QF

    End Sub

    Public Sub MoveDownPix(intPosition As Integer, intPix As Integer)


        Dim i As Integer
        Dim QF As QfcController
        Dim ctlFrame As Panel

        For i = ColQFClass.Count To intPosition Step -1

            'Shift items downward if there are any
            QF = ColQFClass(i)
            ctlFrame = QF.frm
            ctlFrame.Top += intPix
        Next i
        '_viewer.L1v1L2_PanelMain.ScrollHeight = max(max(intPix, 0) + (ColQFClass.Count * (frmHt + frmSp)), _viewer.L1v1L2_PanelMain.Height)


    End Sub

    Public Sub AddEmailControlGroup(Optional objItem As Object = Nothing, Optional posInsert As Integer = 0, Optional blGroupConversation As Boolean = True, Optional ConvCt As Integer = 0, Optional varList As Object = Nothing, Optional blChild As Boolean = False)


        Dim Mail As MailItem
        Dim QF As QfcController
        Dim colCtrls As Collection
        Dim items As Items
        Dim i As Integer

        _intUniqueItemCounter += 1

        If objItem Is Nothing Then
            items = _folderCurrent.Items
            objItem = items(max() - _intEmailPosition)
        End If

        If posInsert = 0 Then posInsert = ColQFClass.Count + 1

        If TypeOf objItem Is MailItem Then
            Mail = objItem

            colCtrls = New Collection

            LoadGroupOfCtrls(colCtrls, _intUniqueItemCounter, posInsert, blGroupConversation)
            QF = New QfcController(Mail, colCtrls, posInsert, _boolRemoteMouseApp, Me, _globals)
            If blChild Then QF.blConChild = True
            If IsArray(varList) = True Then
                If UBound(varList) = 0 Then
                    QF.Init_FolderSuggestions()
                Else
                    QF.Init_FolderSuggestions(varList)
                End If
            Else
                QF.Init_FolderSuggestions(varList)
            End If
            QF.CountMailsInConv(ConvCt)

            If posInsert > ColQFClass.Count Then
                ColQFClass.Add(QF)
            Else
                'ColQFClass.Add(QF, QF.Mail.Subject & QF.Mail.SentOn & QF.Mail.Sender, posInsert)
                ColQFClass.Add(QF, posInsert)
            End If

            For i = 1 To ColQFClass.Count
                QF = ColQFClass(i)
                'Debug.Print "ColQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.mail.Subject
            Next i

        End If



    End Sub

    Public Sub ConvToggle_Group(selItems As Collection, intOrigPosition As Integer)


        Dim objEmail As MailItem
        Dim objItem As Object
        Dim i As Integer
        Dim QF As QfcController
        Dim QF_Orig As QfcController
        Dim intPosition As Integer
        Dim blDebug As Boolean

        blDebug = True

        QF_Orig = ColQFClass(intOrigPosition)

        If blDebug Then
            For i = 1 To ColQFClass.Count
                QF = ColQFClass(i)
                'Debug.Print "ColQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.mail.Subject
            Next i
        End If

        For Each objItem In selItems
            objEmail = objItem
            intPosition = GetEmailPositionInCollection(objEmail)
            'If intPosition < intOrigPosition Then QF_Orig.intMyPosition = intPosition
            RemoveSpecificControlGroup(intPosition)
        Next objItem



    End Sub

    Public Sub ConvToggle_UnGroup(selItems As Collection, intPosition As Integer, ConvCt As Integer, varList As Object)

        Dim i As Integer
        Dim QF As QfcController
        Dim blDebug As Boolean

        blDebug = False

        If blDebug Then
            'Print data after movement
            'Debug.Print "DEBUG DATA BEFORE UNGROUP"
            For i = 1 To ColQFClass.Count
                QF = ColQFClass(i)
                'Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.mail.SentOn, "MM\DD\YY HH:MM") & "  " & QF.mail.Subject
            Next i
        End If

        MoveDownControlGroups(intPosition + 1, selItems.Count)

        For i = 1 To selItems.Count
            AddEmailControlGroup(selItems(i), intPosition + i, False, ConvCt, varList, True)
        Next i

        If blDebug Then
            'Print data after movement
            'Debug.Print "DEBUG DATA AFTER UNGROUP"
            For i = 1 To ColQFClass.Count
                QF = ColQFClass(i)
                'Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.mail.SentOn, "MM\DD\YY HH:MM") & "  " & QF.mail.Subject
            Next i
        End If
        FormResize()


    End Sub

    Private Function DoesCollectionHaveConvID(objItem As Object, col As Collection) As Integer



        Dim objItemInCol As Object
        Dim objMailInCol As [MailItem]
        Dim objMail As [MailItem]
        Dim i As Integer

        DoesCollectionHaveConvID = 0

        If TypeOf objItem Is MailItem Then
            objMail = objItem
            If col IsNot Nothing Then
                For i = 1 To col.Count
                    objItemInCol = col(i)
                    If TypeOf objItemInCol Is MailItem Then
                        objMailInCol = objItemInCol
                        If objMailInCol.ConversationID = objMail.ConversationID Then DoesCollectionHaveConvID = i
                    End If
                Next i
            End If
        End If



    End Function

    Private Function GetEmailPositionInCollection(objMail As [MailItem]) As Integer



        Dim QF As QfcController
        Dim i As Integer

        GetEmailPositionInCollection = 0
        For i = 1 To ColQFClass.Count
            QF = ColQFClass(i)
            If QF.Mail.EntryID = objMail.EntryID Then GetEmailPositionInCollection = i
        Next i



    End Function

    Friend Sub RemoveSpecificControlGroup(intPosition As Integer)

        Dim blDebug As Boolean
        Dim QF As QfcController
        Dim intItemCount As Integer
        Dim i As Integer
        Dim ctlFrame As Panel
        Dim strDeletedSub As String
        Dim strDeletedDte As String
        Dim intDeletedMyPos As Integer

        blDebug = False

        intItemCount = ColQFClass.Count

        QF = ColQFClass(intPosition)                'Set class equal to specific member of collection
        On Error Resume Next
        strDeletedSub = QF.Mail.Subject
        strDeletedDte = Format(QF.Mail.SentOn, "mm\\dd\\yyyy hh:mm")
        intDeletedMyPos = QF.intMyPosition


        QF.ctrlsRemove()                                  'Run the method that removes controls from the frame
        _viewer.L1v1L2_PanelMain.Controls.Remove(QF.frm)           'Remove the specific frame
        QF.kill()                                         'Remove the variables linking to events

        If blDebug Then
            'Print data before movement
            Debug.Print("DEBUG DATA BEFORE MOVEMENT")

            For i = 1 To intItemCount
                If i = intPosition Then
                    Debug.Print(i & "  " & intDeletedMyPos & "  " & strDeletedDte & "  " & strDeletedSub)
                Else
                    QF = ColQFClass(i)
                    Debug.Print(i & "  " & QF.intMyPosition & "  " & Format(QF.Mail.SentOn, "MM\\DD\\YY HH:MM") & "  " & QF.Mail.Subject)
                End If
            Next i
        End If

        'Shift items upward if there are any
        If intPosition < intItemCount Then
            For i = intPosition + 1 To intItemCount
                QF = ColQFClass(i)
                QF.intMyPosition -= 1
                ctlFrame = QF.frm
                ctlFrame.Top = ctlFrame.Top - frmHt - frmSp
            Next i
            '_viewer.L1v1L2_PanelMain.ScrollHeight = max(_viewer.L1v1L2_PanelMain.ScrollHeight - frmHt - frmSp, _heightPanelMainMax)
        End If

        ColQFClass.Remove(intPosition)
        _intEmailStart += 1

        If blDebug Then
            'Print data after movement
            Debug.Print("DEBUG DATA POST MOVEMENT")

            For i = 1 To ColQFClass.Count
                QF = ColQFClass(i)
                Debug.Print(i & "  " & QF.intMyPosition & "  " & Format(QF.Mail.SentOn, "MM\\DD\\YY HH:MM") & "  " & QF.Mail.Subject)
            Next i
        End If

        QF = Nothing
    End Sub

    Public Sub toggleAcceleratorDialogue()
        Dim QF As QfcController
        Dim i As Integer

        If ColQFClass IsNot Nothing Then
            For i = 1 To ColQFClass.Count
                QF = ColQFClass(i)
                If QF.blExpanded And i <> ColQFClass.Count Then MoveDownPix(i + 1, QF.frm.Height * -0.5)
                QF.Accel_Toggle()
            Next i
        End If

        If _viewer.AcceleratorDialogue.Visible = True Then
            _viewer.AcceleratorDialogue.Visible = False
            'Modal                                                                                strTemp = "ExplConvView_ToggleOn"
            'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Modal        ExplConvView_ToggleOn
            Dim unused = _viewer.L1v1L2_PanelMain.Focus()
        Else
            If AreConversationsGrouped(_activeExplorer) Then
                'ToggleShowAsConversation -1
                'Modal                                                                                strTemp = "ExplConvView_ToggleOff"
                'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                'Modal            ExplConvView_ToggleOff
            End If
            _viewer.AcceleratorDialogue.Visible = True
            If _intAccActiveMail <> 0 Then
                _viewer.AcceleratorDialogue.Text = _intAccActiveMail
                Try
                    QF = ColQFClass(_intAccActiveMail)
                Catch ex As System.Exception
                    _intAccActiveMail = 1
                    QF = ColQFClass(_intAccActiveMail)
                End Try
                QF.Accel_FocusToggle()
            End If
            'Modal                                                                                strTemp = "SendMessage _lFormHandle, WM_SETFOCUS, 0&, 0&"
            'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Modal        SendMessage _lFormHandle, WM_SETFOCUS, 0&, 0&

            _viewer.AcceleratorDialogue.Focus()
            _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength
        End If

        QF = Nothing
    End Sub

    Private Sub FormResize()
        Dim intDiffy As Integer
        Dim intDiffx As Integer
        Dim i As Integer
        Dim QF As QfcController

        'MsgBox "App Width " & Me.Width & vbCrLf & "Screen Width " & ScreenWidth * PointsPerPixel
        If Not _blSuppressEvents Then

            intDiffx = If(_viewer.Width >= Width_UserForm - 100, _viewer.Width - Width_UserForm, 0)

            intDiffy = If(_viewer.Height >= _heightFormMin, _viewer.Height - _heightFormMin, 0)

            _viewer.L1v1L2_PanelMain.Width = Width_PanelMain + intDiffx
            _viewer.L1v1L2_PanelMain.Height = _heightPanelMainMin + intDiffy

            _viewer.L1v2L2h3_ButtonOK.Top = _lngTopButtonOkMin + intDiffy
            _viewer.L1v2L2h3_ButtonOK.Left = OK_left + (intDiffx / 2)
            _viewer.L1v2L2h4_ButtonCancel.Top = _lngTopButtonCancelMin + intDiffy
            _viewer.L1v2L2h4_ButtonCancel.Left = _viewer.L1v2L2h3_ButtonOK.Left + CANCEL_left - OK_left
            _viewer.L1v2L2h4_ButtonUndo.Top = _lngTopButtonUndoMin + intDiffy
            _viewer.L1v2L2h4_ButtonUndo.Left = _viewer.L1v2L2h3_ButtonOK.Left + UNDO_left - OK_left
            'Button1.top = lngTop_Button1_Min + intDiffy
            _viewer.AcceleratorDialogue.Top = _lngTopAcceleratorDialogueMin + intDiffy
            _viewer.L1v2L2h5_SpnEmailPerLoad.Top = _lngTopSpnMin + intDiffy
            _viewer.L1v2L2h5_SpnEmailPerLoad.Left = spn_left + intDiffx


            If ColQFClass IsNot Nothing Then
                For i = 1 To ColQFClass.Count
                    QF = ColQFClass(i)
                    If QF.blConChild Then
                        QF.frm.Left = frmLt * 2
                        QF.frm.Width = Width_frm + intDiffx - frmLt
                        QF.ResizeCtrls(intDiffx - frmLt)
                    Else
                        QF.frm.Width = Width_frm + intDiffx
                        QF.ResizeCtrls(intDiffx)
                    End If
                Next i
            End If

        End If 'blSupressEvents

    End Sub

#End Region

#Region "Keyboard event handlers"
    Friend Sub AcceleratorDialogue_Change()

        Dim strToParse As String
        Dim i As Integer
        Dim intLen As Integer
        Dim intLastNum As Integer
        Dim intAccTmpMail As Integer
        Dim strCommand As String
        Dim QF As QfcController
        Dim blExpanded As Boolean

        If Not _blSuppressEvents Then
            intLastNum = 0
            strToParse = _viewer.AcceleratorDialogue.Text
            If strToParse <> "" Then
                intLen = Len(strToParse)
                For i = 1 To intLen
                    If IsNumeric(Mid(strToParse, i, 1)) Then
                        intLastNum = i
                    Else
                        Exit For
                    End If
                Next i
                If intLastNum > 0 Then
                    intAccTmpMail = CInt(Mid(strToParse, 1, intLastNum))
                    If intAccTmpMail <> _intAccActiveMail Then
                        If _intAccActiveMail <> 0 And _intAccActiveMail <= ColQFClass.Count Then
                            QF = ColQFClass(_intAccActiveMail)
                            If QF.blExpanded Then
                                MoveDownPix(_intAccActiveMail + 1, QF.frm.Height * -0.5)
                                QF.ExpandCtrls1()
                                blExpanded = True
                            End If
                            QF.Accel_FocusToggle()
                        End If
                        If intAccTmpMail <> 0 And intAccTmpMail <= ColQFClass.Count Then
                            QF = ColQFClass(intAccTmpMail)
                            QF.Accel_FocusToggle()
                            If blExpanded Then
                                MoveDownPix(intAccTmpMail + 1, QF.frm.Height)
                                QF.ExpandCtrls1()
                            End If
                            _viewer.L1v1L2_PanelMain.ScrollControlIntoView(QF.frm)
                            'ScrollIntoView_MF(QF.Frm.Top, QF.Frm.Top + QF.Frm.Height)
                        End If

                        If intAccTmpMail <= ColQFClass.Count Then
                            _intAccActiveMail = intAccTmpMail
                        End If
                    End If
                    If intLen > intLastNum And _intAccActiveMail <> 0 And _intAccActiveMail <= ColQFClass.Count Then
                        strCommand = UCase(Mid(strToParse, intLastNum + 1, 1))
                        If _blSuppressEvents = False Then
                            _blSuppressEvents = True
                            _viewer.AcceleratorDialogue.Text = _intAccActiveMail
                            _blSuppressEvents = False
                        Else
                            _viewer.AcceleratorDialogue.Text = _intAccActiveMail
                        End If

                        QF = ColQFClass(_intAccActiveMail)

                        Select Case strCommand
                            Case "O"
                                toggleAcceleratorDialogue()
                                'EnableWindow(_olAppHWnd, Modeless)
                                'EnableWindow _lFormHandle, Modeless
                                If _activeExplorer.CurrentFolder.DefaultItemType <> OlItemType.olMailItem Then
                                    _activeExplorer.NavigationPane.CurrentModule = _activeExplorer.NavigationPane.Modules.GetNavigationModule(OlNavigationModuleType.olModuleMail)
                                End If
                                If InitType.HasFlag(InitTypeEnum.InitSort) And AreConversationsGrouped(_activeExplorer) Then ExplConvView_ToggleOff()                      'Modal
                                QF.KB(strCommand)
                                QFD_Minimize()
                                If InitType.HasFlag(InitTypeEnum.InitSort) And BlShowInConversations Then ExplConvView_ToggleOn()
                        'ToggleShowAsConversation 1
                        'SendMessage _lFormHandle, WM_SETFOCUS, 0&, 0&
                            Case "C"
                                toggleAcceleratorDialogue()
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "T"
                                toggleAcceleratorDialogue()
                                'EnableWindow(_olAppHWnd, Modeless)
                                'EnableWindow _lFormHandle, Modeless
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "F"
                                toggleAcceleratorDialogue()
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "D"
                                toggleAcceleratorDialogue()
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "X"
                                toggleAcceleratorDialogue()
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "R"
                                toggleAcceleratorDialogue()
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "A"
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "W"
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "M"
                                QF = ColQFClass(_intAccActiveMail)
                                QF.KB(strCommand)
                            Case "E"
                                QF = ColQFClass(_intAccActiveMail)
                                If QF.blExpanded Then
                                    MoveDownPix(_intAccActiveMail + 1, QF.frm.Height * -0.5)
                                    QF.ExpandCtrls1()
                                Else
                                    MoveDownPix(_intAccActiveMail + 1, QF.frm.Height)
                                    QF.ExpandCtrls1()
                                End If
                            Case Else
                        End Select
                    End If
                End If
            Else
                If _intAccActiveMail <> 0 Then
                    Dim unused = ColQFClass(_intAccActiveMail).Accel_FocusToggle
                    _intAccActiveMail = 0
                End If
            End If
        Else

        End If

    End Sub

    Friend Sub AcceleratorDialogue_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Alt
                'Debug.Print "Alt Key Pressed"
                toggleAcceleratorDialogue()
            Case Keys.Down
                If AreConversationsGrouped(_activeExplorer) Then
                    'Modal                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    'Modal                ExplConvView_ToggleOff
                    '            Else
                    '                                                                                                    strTemp = "If AreConversationsGrouped Then IS FALSE"
                    '                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                End If
                If _intAccActiveMail < ColQFClass.Count Then
                    _viewer.AcceleratorDialogue.Text = _intAccActiveMail + 1
                End If
            Case Keys.Up
                If AreConversationsGrouped(_activeExplorer) Then
                    'Modal                                                                                                    strTemp = "ExplConvView_ToggleOff"
                    'Modal                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    'Modal                ExplConvView_ToggleOff
                End If
                If _intAccActiveMail > 1 Then
                    _viewer.AcceleratorDialogue.Text = _intAccActiveMail - 1
                End If
                _viewer.AcceleratorDialogue.Focus()
            Case Keys.A
                If ((Control.ModifierKeys And Keys.Shift) = Keys.Shift) And
                    ((Control.ModifierKeys And Keys.Control) = Keys.Control) Then
                    ToggleRemoteMouseLabels()
                End If
                '        Case Else
                '                                                                                                    strTemp = "Case Else"
                '                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
        End Select
    End Sub

    Friend Sub AcceleratorDialogue_KeyUp(sender As Object, e As KeyEventArgs)
        Dim QF As QfcController
        Dim blExpanded As Boolean

        Select Case e.KeyCode
            Case 18
                If sender.Visible Then
                    Dim unused1 = sender.Focus()
                    sender.SelStart = sender.TextLength
                Else
                    Dim unused = _viewer.L1v1L2_PanelMain.Focus()
                End If
                SendKeys.Send("{ESC}")
            Case Keys.Right
                If sender.Visible And _intAccActiveMail <> 0 Then
                    QF = ColQFClass(_intAccActiveMail)
                    If QF.lblConvCt.Text <> "1" And QF.chk.Checked = True Then
                        If QF.blExpanded Then
                            blExpanded = True
                            MoveDownPix(_intAccActiveMail + 1, QF.frm.Height * -0.5)
                            QF.ExpandCtrls1()
                        End If
                        toggleAcceleratorDialogue()
                        QF.KB("C")
                        toggleAcceleratorDialogue()

                        If blExpanded Then
                            MoveDownPix(_intAccActiveMail + 1, QF.frm.Height)
                            QF.ExpandCtrls1()
                        End If
                    End If
                End If
            Case Keys.Left
                If sender.Visible And _intAccActiveMail <> 0 Then
                    QF = ColQFClass(_intAccActiveMail)
                    If QF.lblConvCt.Text <> "1" And QF.chk.Checked = False Then
                        If QF.blExpanded Then
                            blExpanded = True
                            MoveDownPix(_intAccActiveMail + 1, QF.frm.Height * -0.5)
                            QF.ExpandCtrls1()
                        End If
                        toggleAcceleratorDialogue()
                        QF.KB("C")
                        toggleAcceleratorDialogue()

                        If blExpanded Then
                            MoveDownPix(_intAccActiveMail + 1, QF.frm.Height)
                            QF.ExpandCtrls1()
                        End If

                    End If
                    sender.SelStart = sender.TextLength
                End If
            Case Else
        End Select
    End Sub

    Friend Sub ButtonCancel_KeyDown(sender As Object, e As KeyEventArgs)
        KeyDownHandler(sender, e)
    End Sub

    Friend Sub Button_OK_KeyDown(sender As Object, e As KeyEventArgs)
        'If DebugLVL And vbProcedure Then Debug.Print "Fired Button_OK_KeyDown"
        KeyDownHandler(sender, e)
    End Sub

    Friend Sub Button_OK_KeyUp(sender As Object, e As KeyEventArgs)
        KeyUpHandler(sender, e)
    End Sub

    Friend Sub PanelMain_KeyDown(sender As Object, e As KeyEventArgs)
        KeyDownHandler(sender, e)
    End Sub

    Friend Sub PanelMain_KeyPress(sender As Object, e As KeyPressEventArgs)
        KeyPressHandler(sender, e)
    End Sub

    Friend Sub PanelMain_KeyUp(sender As Object, e As KeyEventArgs)
        KeyUpHandler(sender, e)
    End Sub

    Private Sub SpnEmailPerLoad_KeyDown(sender As Object, e As KeyEventArgs)
        KeyDownHandler(sender, e)
    End Sub

    Private Sub UserForm_KeyPress(sender As Object, e As KeyPressEventArgs)
        If Not _blSuppressEvents Then KeyPressHandler(sender, e)
    End Sub

    Private Sub UserForm_KeyUp(sender As Object, e As KeyEventArgs)
        If Not _blSuppressEvents Then KeyUpHandler(sender, e)
    End Sub

    Private Sub UserForm_KeyDown(sender As Object, e As KeyEventArgs)
        If Not _blSuppressEvents Then KeyDownHandler(sender, e)
    End Sub

    Public Sub KeyPressHandler(sender As Object, e As KeyPressEventArgs)
        If Not _blSuppressEvents Then
            Select Case e.KeyChar

                Case Else
            End Select
        End If
    End Sub

    Public Sub KeyUpHandler(sender As Object, e As KeyEventArgs)
        If Not _blSuppressEvents Then
            Select Case e.KeyCode
                Case Keys.Alt
                    If _viewer.AcceleratorDialogue.Visible Then
                        _viewer.AcceleratorDialogue.Focus()
                        _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength
                    Else
                        Dim unused = _viewer.L1v1L2_PanelMain.Focus()
                    End If
                    SendKeys.Send("{ESC}")
                Case Keys.Up
                    If _viewer.AcceleratorDialogue.Visible Then _viewer.AcceleratorDialogue.Focus()
                Case Keys.Down
                    If _viewer.AcceleratorDialogue.Visible Then _viewer.AcceleratorDialogue.Focus()
                Case Else
            End Select
        End If
    End Sub

    Public Sub KeyDownHandler(sender As Object, e As KeyEventArgs)

        If Not _blSuppressEvents Then
            Select Case e.KeyCode
                Case Keys.Enter
                    ButtonOK_Click()
                Case Keys.Tab
                    toggleAcceleratorDialogue()
                    If _viewer.AcceleratorDialogue.Visible Then _viewer.AcceleratorDialogue.Focus()
                    '        Case vbKeyEscape
                    '            vbMsgResponse = MsgBox("Stop all filing actions and close quick-filer?", vbOKCancel)
                    '            If vbMsgResponse = vbOK Then ButtonCancel_Click
                Case Keys.Alt
                    toggleAcceleratorDialogue()
                    If _viewer.AcceleratorDialogue.Visible Then
                        _viewer.AcceleratorDialogue.Focus()
                    Else
                        Dim unused = _viewer.L1v1L2_PanelMain.Focus()
                    End If
                Case Else
                    If _viewer.AcceleratorDialogue.Visible Then
                        AcceleratorDialogue_KeyDown(sender, e)
                    Else
                    End If
            End Select
        End If
    End Sub

#End Region

#Region "Other Event Handlers"

    Friend Sub ButtonCancel_Click()
        'ExplConvView_ToggleOn
        If BlShowInConversations Then
            'ExplConvView_ToggleOn
            ExplConvView_Cleanup()
        End If
        'ToggleShowAsConversation 1
        RemoveControls()
        BlFrmKll = True

        _viewer.Dispose()
    End Sub

    Friend Sub ButtonOK_Click()

        Dim QF As QfcController
        Dim blReadyForMove As Boolean
        Dim strNotifications As String

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            If _blRunningModalCode = False Then
                _blRunningModalCode = True

                blReadyForMove = True
                strNotifications = "Can't complete actions! Not all emails assigned to folder" & vbCrLf

                For Each QF In ColQFClass
                    If QF.cbo.SelectedValue = "" Then
                        blReadyForMove = False
                        strNotifications = strNotifications & QF.intMyPosition &
                        "  " & Format(QF.Mail.SentOn, "mm\\dd\\yyyy") &
                        "  " & QF.Mail.Subject & vbCrLf
                    End If
                Next QF
                strNotifications = Mid(strNotifications, 1, Len(strNotifications) - 1)

                If blReadyForMove Then
                    _blSuppressEvents = True
                    _intAccActiveMail = 1
                    ColMailJustMoved = New Collection

                    For Each QF In ColQFClass
                        QF.MoveMail()
                    Next QF

                    'QuickFileMetrics_WRITE "9999QuickFileMetrics.csv"
                    QuickFileMetrics_WRITE("9999TimeWritingEmail.csv")
                    RemoveControls()
                    Iterate()
                    _blSuppressEvents = False
                Else
                    Dim unused = MsgBox(strNotifications, vbOKOnly + vbCritical, "Error Notification")
                End If

                _viewer.AcceleratorDialogue.Text = ""
                _intAccActiveMail = 0

                _blRunningModalCode = False
            Else
                'MyBoxMsg("Can't Execute While Running Modal Code")
                MsgBox("Can't Execute While Running Modal Code")
            End If

        Else
            _viewer.Dispose()
        End If
    End Sub

    Friend Sub ButtonUndo_Click()
        Dim i As Integer
        Dim oMail_Old As MailItem = Nothing
        Dim oMail_Current As MailItem = Nothing
        Dim objTemp As Object
        Dim oFolder_Current As [Folder]
        Dim oFolder_Old As [Folder]
        Dim colItems As Collection
        Dim vbUndoResponse As MsgBoxResult
        Dim vbRepeatResponse As MsgBoxResult

        If _movedMails Is Nothing Then _movedMails = New cStackObject
        vbRepeatResponse = vbYes

        i = _movedMails.Count
        colItems = _movedMails.ToCollection()

        While (i > 1) And (vbRepeatResponse = vbYes)
            objTemp = colItems(i)
            'objTemp = _movedMails.Pop
            If TypeOf objTemp Is MailItem Then oMail_Current = objTemp
            'objTemp = _movedMails.Pop
            objTemp = colItems(i - 1)
            If TypeOf objTemp Is MailItem Then oMail_Old = objTemp

            'oMail_Old = _movedMails.Pop
            If (Mail_IsItEncrypted(oMail_Current) = False) And (Mail_IsItEncrypted(oMail_Old) = False) Then
                oFolder_Current = oMail_Current.Parent
                oFolder_Old = oMail_Old.Parent
                vbUndoResponse = MsgBox("Undo Move of email?" & vbCrLf & "Sent On: " &
                Format(oMail_Current.SentOn, "mm/dd/yyyy") & vbCrLf &
                oMail_Current.Subject, vbYesNo)
                If vbUndoResponse = vbYes And (oFolder_Current IsNot oFolder_Old) Then
                    Dim unused = oMail_Current.Move(oFolder_Old)

                    _movedMails.Pop(i)
                    _movedMails.Pop(i - 1)
                End If
            End If
            i -= 2
            vbRepeatResponse = MsgBox("Continue Undoing Moves?", vbYesNo)
        End While
    End Sub

    Friend Sub SpnEmailPerLoad_Change()
        If _viewer.L1v2L2h5_SpnEmailPerLoad.Value >= 0 Then
            _intEmailsPerIteration = _viewer.L1v2L2h5_SpnEmailPerLoad.Value
        End If
    End Sub

    Friend Sub Viewer_Activate()
        If _stopWatch IsNot Nothing Then
            If _stopWatch.isPaused = True Then
                _stopWatch.reStart()
            End If
        End If
    End Sub

    Private Sub focusListener_ChangeFocus(ByVal gotFocus As Boolean)
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
            Debug.Print("Lost Focus")
            '        'GoingAway

        End If
    End Sub

    Friend Sub Form_Dispose()
        ExplConvView_ReturnState()
    End Sub

#End Region

#Region "Outlook View UI Actions"

    Public Sub QFD_Minimize()
        If _stopWatch IsNot Nothing Then
            If _stopWatch.isPaused = False Then
                _stopWatch.Pause()
            End If
        End If
        _viewer.WindowState = FormWindowState.Minimized
    End Sub

    Public Sub QFD_Maximize()
        _viewer.WindowState = FormWindowState.Maximized
    End Sub

    Public Sub ExplConvView_Cleanup()

        On Error Resume Next
        ObjView = _activeExplorer.CurrentFolder.Views(_objViewMem)
        If Err.Number = 0 Then
            'ObjView.Reset
            ObjView.Apply()
            If ObjViewTemp IsNot Nothing Then ObjViewTemp.Delete()
            BlShowInConversations = False
        Else
            Err.Clear()
            ObjViewTemp = _activeExplorer.CurrentView.Parent("tmpNoConversation")
            If ObjViewTemp IsNot Nothing Then ObjViewTemp.Delete()
        End If
    End Sub

    Public Sub ExplConvView_ToggleOff()
        If _olApp.ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations") Then
            BlShowInConversations = True
            ObjView = _activeExplorer.CurrentView

            If ObjView.Name = "tmpNoConversation" Then
                If _activeExplorer.CommandBars.GetPressedMso("ShowInConversations") Then

                    ObjView.XML = Replace(ObjView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, , vbTextCompare)
                    ObjView.Save()
                    ObjView.Apply()
                End If

            End If

            _objViewMem = ObjView.Name
            If _objViewMem = "tmpNoConversation" Then _objViewMem = _globals.Ol.View_Wide

            ObjViewTemp = ObjView.Parent("tmpNoConversation")

            If ObjViewTemp Is Nothing Then
                ObjViewTemp = ObjView.Copy("tmpNoConversation", OlViewSaveOption.olViewSaveOptionThisFolderOnlyMe)
                ObjViewTemp.XML = Replace(ObjView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, , vbTextCompare)
                ObjViewTemp.Save()

            End If
            ObjViewTemp.Apply()
            If _blSuppressEvents Then
                Dim unused1 = _olApp.DoEvents()
            Else
                _blSuppressEvents = True
                Dim unused = _olApp.DoEvents()
                _blSuppressEvents = False
            End If
        End If

    End Sub

    Public Sub ExplConvView_ToggleOn()

        If BlShowInConversations Then
            ObjView = _activeExplorer.CurrentFolder.Views(_objViewMem)
            'ObjView.Reset
            ObjView.Apply()
            'ObjViewTemp.Delete
            BlShowInConversations = False
        End If

    End Sub

    Friend Sub ExplConvView_ReturnState()
        If BlShowInConversations Then ExplConvView_ToggleOn()
    End Sub

#End Region




    Private Sub QuickFileMetrics_WRITE(filename As String, Optional FileWriteType As Integer = 8)

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

        LOC_TXT_FILE = Path.Combine(_globals.FS.FldrMyD, filename)

        Duration = _stopWatch.timeElapsed
        OlEndTime = Now()
        OlStartTime = DateAdd("S", -Duration, OlEndTime)

        If ColQFClass.Count > 0 Then
            Duration /= ColQFClass.Count
        End If

        durationText = Format(Duration, "##0")
        'If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

        durationMinutesText = Format(Duration / 60, "##0.00")

        'dataLineBeg = dataLineBeg & durationText & "," & durationMinutesText & ","

        infoMail = New cInfoMail
        OlEmailCalendar = GetCalendar("Email Time", _olApp.Session)
        OlAppointment = OlEmailCalendar.Items.Add(New Outlook.AppointmentItem)
        With OlAppointment
            .Subject = "Quick Filed " & ColQFClass.Count & " emails"
            .Start = OlStartTime
            .End = OlEndTime
            .Categories = "@ Email"
            .ReminderSet = False
            .Sensitivity = OlSensitivity.olPrivate
            .Save()
        End With

        ReDim strOutput(ColQFClass.Count)
        For k = 1 To ColQFClass.Count
            QF = ColQFClass(k)
            'If Mail_IsItEncrypted(QF.mail) = False Then
            On Error Resume Next
            If infoMail.Init_wMail(QF.Mail, OlEndTime:=OlEndTime, lngDurationSec:=Duration) Then
                If OlAppointment.Body = "" Then
                    OlAppointment.Body = infoMail.ToString
                    OlAppointment.Save()
                Else
                    OlAppointment.Body = OlAppointment.Body & vbCrLf & infoMail.ToString
                    OlAppointment.Save()
                End If
            End If
            dataLine = dataLineBeg & xComma(QF.lblSubject.Text)
            dataLine = dataLine & "," & "QuickFiled"
            dataLine = dataLine & "," & durationText
            dataLine = dataLine & "," & durationMinutesText
            dataLine = dataLine & "," & xComma(QF.strlblTo)
            dataLine = dataLine & "," & xComma(QF.lblSender.Text)
            dataLine = dataLine & "," & "Email"
            dataLine = dataLine & "," & xComma(QF.cbo.SelectedItem.ToString())           'Target Folder
            dataLine = dataLine & "," & QF.lblSentOn.Text
            dataLine = dataLine & "," & Format(QF.Mail.SentOn, "hh:mm")
            strOutput(k) = dataLine
        Next k

        Write_TextFile(filename, strOutput, _globals.FS.FldrMyD)

    End Sub


    Private Function xComma(ByVal str As String) As String
        Dim strTmp As String

        strTmp = Replace(str, ", ", "_")
        strTmp = Replace(strTmp, ",", "_")
        xComma = GetStrippedText(strTmp)
        'xComma = StripAccents(strTmp)
    End Function


End Class
