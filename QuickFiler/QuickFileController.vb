Imports System.Collections.Generic
Imports System.Data.SqlClient
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports ToDoModel
Imports UtilitiesVB
Imports Windows.Win32



<Assembly: log4net.Config.XmlConfigurator(Watch:=True)>


Public Class QuickFileController

    Private _useOld As Boolean = True
    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "State Variables"
    'Private state variables
    Private _folderCurrent As Folder
    Private _intUniqueItemCounter As Integer
    Private _intEmailPosition As Integer
    Private _intEmailsPerIteration As Integer
    Private _lngAcceleratorDialogueTop As Long
    Private _lngAcceleratorDialogueLeft As Long
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
    Private _initType As InitTypeEnum

    'Collections
    Private _legacy As QfcGroupOperationsLegacy
    'Public _colQFClass As Collection
    'Public ColFrames As Collection
    'Public ColMailJustMoved As Collection
    Private _colEmailsInFolder As Collection
    Friend WithEvents Frm As Panel

    'Window Handles
    Private _olAppHWnd As IntPtr
    Private _lFormHandle As IntPtr

    'Cleanup
    Delegate Sub ParentCleanupMethod()
    Private _parentCleanup As ParentCleanupMethod
#End Region

    Public Sub New(AppGlobals As IApplicationGlobals,
                   Viewer As QuickFileViewer,
                   ColEmailsInFolder As Collection,
                   ParentCleanup As ParentCleanupMethod)

        'Link viewer to controller
        _viewer = Viewer
        _viewer.SetController(Me)

        'Link model to controller
        _colEmailsInFolder = ColEmailsInFolder
        InitializeModelProcessingMetrics()

        _parentCleanup = ParentCleanup

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
        _legacy = New QfcGroupOperationsLegacy(_viewer, _initType, _globals, Me)
        _viewer.Show()

        Iterate()
    End Sub

#Region "Master Control Functions"

    Public Sub Iterate()
        _stopWatch = New cStopWatch
        _stopWatch.Start()
        Dim colEmails As Collection = DequeNextEmailGroup(_colEmailsInFolder, _intEmailsPerIteration)
        _legacy.LoadControlsAndHandlers(colEmails)
    End Sub

    Private Sub InitializeModelProcessingMetrics()
        _intEmailPosition = 0    'Reverse sort is 0   'Regular sort is 1
    End Sub

    Private Sub InitializeFormConfigurations()
        'Set conversation state variable with initial state
        BlShowInConversations = CurrentConversationState
        If BlShowInConversations Then _objViewMem = _activeExplorer.CurrentView.Name

        'Suppress events while initializing form
        _blSuppressEvents = True

        'Configure viewer for SORTING rather than FINDING items
        _initType = InitTypeEnum.InitSort

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

#End Region

#Region "Form UI Control"

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

    Friend Sub FormResize(Optional Force As Boolean = False)
        Dim intDiffy As Integer
        Dim intDiffx As Integer

        'MsgBox "App Width " & Me.Width & vbCrLf & "Screen Width " & ScreenWidth * PointsPerPixel
        If ((Not _blSuppressEvents) Or Force) Then

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

            _legacy.ResizeChildren(intDiffx)

        End If

    End Sub

#End Region

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

#End Region


#Region "Keyboard event handlers"
    Friend Sub AcceleratorDialogue_Change()
        If Not _blSuppressEvents Then _legacy.ParseAcceleratorText()
    End Sub

    Friend Sub AcceleratorDialogue_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Alt
                _legacy.toggleAcceleratorDialogue()
            Case Keys.Down
                _legacy.SelectNextItem()
            Case Keys.Up
                _legacy.SelectPreviousItem()
            Case Keys.A
                If ((Control.ModifierKeys And Keys.Shift) = Keys.Shift) And
                    ((Control.ModifierKeys And Keys.Control) = Keys.Control) Then
                    _legacy.ToggleRemoteMouseLabels()
                End If
        End Select
    End Sub

    Friend Sub AcceleratorDialogue_KeyUp(sender As Object, e As KeyEventArgs)

        If e.Alt Then
            If sender.Visible Then
                sender.Focus()
                Dim txtbox As TextBox = DirectCast(sender, TextBox)
                txtbox.SelectionStart = txtbox.TextLength
            Else
                _viewer.L1v1L2_PanelMain.Focus()
            End If
            SendKeys.Send("{ESC}")
        Else
            Select Case e.KeyCode
                Case Keys.Right
                    If sender.Visible Then
                        _legacy.MakeSpaceToEnumerateConversation()
                    End If
                Case Keys.Left
                    If sender.Visible Then
                        _legacy.RemoveSpaceToCollapseConversation()
                    End If
                Case Else
            End Select
        End If

    End Sub

    Friend Sub ButtonCancel_KeyDown(sender As Object, e As KeyEventArgs)
        KeyboardHandler_KeyDown(sender, e)
    End Sub

    Friend Sub Button_OK_KeyDown(sender As Object, e As KeyEventArgs)
        'If DebugLVL And vbProcedure Then Debug.Print "Fired Button_OK_KeyDown"
        KeyboardHandler_KeyDown(sender, e)
    End Sub

    Friend Sub Button_OK_KeyUp(sender As Object, e As KeyEventArgs)
        KeyUpHandler(sender, e)
    End Sub

    Friend Sub PanelMain_KeyDown(sender As Object, e As KeyEventArgs)
        KeyboardHandler_KeyDown(sender, e)
    End Sub

    Friend Sub PanelMain_KeyPress(sender As Object, e As KeyPressEventArgs)
        KeyPressHandler(sender, e)
    End Sub

    Friend Sub PanelMain_KeyUp(sender As Object, e As KeyEventArgs)
        KeyUpHandler(sender, e)
    End Sub

    Private Sub SpnEmailPerLoad_KeyDown(sender As Object, e As KeyEventArgs)
        KeyboardHandler_KeyDown(sender, e)
    End Sub

    Private Sub UserForm_KeyPress(sender As Object, e As KeyPressEventArgs)
        If Not _blSuppressEvents Then KeyPressHandler(sender, e)
    End Sub

    Private Sub UserForm_KeyUp(sender As Object, e As KeyEventArgs)
        If Not _blSuppressEvents Then KeyUpHandler(sender, e)
    End Sub

    Private Sub UserForm_KeyDown(sender As Object, e As KeyEventArgs)
        If Not _blSuppressEvents Then KeyboardHandler_KeyDown(sender, e)
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

    Public Sub KeyboardHandler_KeyDown(sender As Object, e As KeyEventArgs)

        If Not _blSuppressEvents Then

            If e.Alt Then
                _legacy.toggleAcceleratorDialogue()
                If _viewer.AcceleratorDialogue.Visible Then
                    _viewer.AcceleratorDialogue.Focus()
                Else
                    _viewer.L1v1L2_PanelMain.Focus()
                End If

            Else
                Select Case e.KeyCode
                    Case Keys.Enter
                        ButtonOK_Click()
                    Case Keys.Tab
                        _legacy.toggleAcceleratorDialogue()
                        If _viewer.AcceleratorDialogue.Visible Then _viewer.AcceleratorDialogue.Focus()
                        '        Case vbKeyEscape
                        '            vbMsgResponse = MsgBox("Stop all filing actions and close quick-filer?", vbOKCancel)
                        '            If vbMsgResponse = vbOK Then ButtonCancel_Click
                    Case Else
                        If _viewer.AcceleratorDialogue.Visible Then
                            AcceleratorDialogue_KeyDown(sender, e)
                        Else
                        End If
                End Select
            End If
        End If
    End Sub

#End Region

#Region "Other Event Handlers"

    Friend Sub Cleanup()
        ExplConvView_ReturnState()
        _olAppHWnd = Nothing
        _lFormHandle = Nothing
        _parentCleanup.Invoke()
    End Sub

    Friend Sub ButtonCancel_Click()
        'ExplConvView_ToggleOn
        If BlShowInConversations Then
            'ExplConvView_ToggleOn
            ExplConvView_Cleanup()
        End If
        'ToggleShowAsConversation 1
        _legacy.RemoveControls()
        BlFrmKll = True

        _viewer.Close()
    End Sub

    Friend Sub ButtonOK_Click()

        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            If _blRunningModalCode = False Then
                _blRunningModalCode = True

                If _legacy.ReadyForMove() Then
                    _blSuppressEvents = True
                    _legacy.MoveEmails(_movedMails)
                    QuickFileMetrics_WRITE("9999TimeWritingEmail.csv")
                    _legacy.RemoveControls()
                    Iterate()
                    _blSuppressEvents = False
                End If
                _blRunningModalCode = False
            Else
                MsgBox("Can't Execute While Running Modal Code")
            End If
        Else
            _viewer.Close()
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

    'Friend Sub Form_Dispose()
    '    Cleanup()
    'End Sub

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

    Friend Sub OpenQFMail(OlMail As MailItem)
        NavigateToOutlookFolder(OlMail)
        If _initType.HasFlag(InitTypeEnum.InitSort) And AreConversationsGrouped(_activeExplorer) Then ExplConvView_ToggleOff()
        QFD_Minimize()
        _activeExplorer.ClearSelection()
        If _activeExplorer.IsItemSelectableInView(OlMail) Then _activeExplorer.AddToSelection(OlMail)
        If _initType.HasFlag(InitTypeEnum.InitSort) And BlShowInConversations Then ExplConvView_ToggleOn()
    End Sub

    Private Sub NavigateToOutlookFolder(olMail As MailItem)
        If _globals.Ol.App.ActiveExplorer.CurrentFolder.FolderPath <> olMail.Parent.FolderPath Then
            ExplConvView_ReturnState()
            _globals.Ol.App.ActiveExplorer.CurrentFolder = olMail.Parent
            BlShowInConversations = AreConversationsGrouped(_activeExplorer)
        End If
        'If _globals.Ol.App.ActiveExplorer.CurrentFolder.DefaultItemType <> OlItemType.olMailItem Then
        '    _globals.Ol.App.ActiveExplorer.NavigationPane.CurrentModule =
        '        _globals.Ol.App.ActiveExplorer.NavigationPane.Modules _
        '        .GetNavigationModule(OlNavigationModuleType.olModuleMail)
        'End If
    End Sub

#End Region

#Region "Action Tracking"

    Private Sub QuickFileMetrics_WRITE(filename As String)

        Dim LOC_TXT_FILE As String
        Dim curDateText, curTimeText, durationText, durationMinutesText As String
        Dim Duration As Double
        Dim dataLineBeg As String
        Dim OlEndTime As Date
        Dim OlStartTime As Date
        Dim OlAppointment As AppointmentItem
        Dim OlEmailCalendar As Folder


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

        If _legacy.EmailsLoaded > 0 Then
            Duration /= _legacy.EmailsLoaded
        End If

        durationText = Format(Duration, "##0")
        'If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

        durationMinutesText = Format(Duration / 60, "##0.00")

        OlEmailCalendar = GetCalendar("Email Time", _olApp.Session)
        OlAppointment = OlEmailCalendar.Items.Add(New Outlook.AppointmentItem)
        With OlAppointment
            .Subject = "Quick Filed " & _legacy.EmailsLoaded & " emails"
            .Start = OlStartTime
            .End = OlEndTime
            .Categories = "@ Email"
            .ReminderSet = False
            .Sensitivity = OlSensitivity.olPrivate
            .Save()
        End With

        Dim strOutput() As String = _legacy.GetMoveDiagnostics(durationText, durationMinutesText, Duration, dataLineBeg, OlEndTime, OlAppointment)

        Write_TextFile(filename, strOutput, _globals.FS.FldrMyD)

    End Sub

    Private Sub GetDetails(durationText As String, durationMinutesText As String, Duration As Double, ByRef dataLine As String, dataLineBeg As String, ByRef QF As QfcController, OlEndTime As Date, infoMail As cInfoMail, ByRef OlAppointment As AppointmentItem, strOutput() As String)

    End Sub


#End Region

End Class
