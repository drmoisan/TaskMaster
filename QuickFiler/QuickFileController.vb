Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Outlook
Imports ToDoModel
Imports UtilitiesVB

Public Class QuickFileController

    Private OlApp_hWnd As Long
    Private lFormHandle As Long
    Private lStyle As Long

    Friend WithEvents frm As Windows.Forms.Panel

    Public WithEvents focusListener As FormFocusListener
    Private StopWatch As cStopWatch
    Private BoolRemoteMouseApp As Boolean
    Private Const Modal = 0, Modeless = 1

    Private ReadOnly ht, wt As Long
    Private Const GWL_STYLE As Long = -16 'Sets a new window style  As LongPtr in 64bit version
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

    Private ReadOnly strTagOptions As Object
    Private ReadOnly strFilteredOptions As Object
    Private ReadOnly intFilteredMax As Integer
    Private ReadOnly intMaxOptions As Integer
    Private ReadOnly boolTagChoice() As Boolean
    Private ReadOnly boolFilteredChoice() As Boolean
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
    Public blShowInConversations As Boolean
    Public objView As Microsoft.Office.Interop.Outlook.View
    Private objView_Mem As String
    Public objViewTemp As Microsoft.Office.Interop.Outlook.View
    Public InitType As InitTypeEnum


    Public colQFClass As Collection
    Public colFrames As Collection
    Public colMailJustMoved As Collection
    Public strOptions As String
    Public intFocus As Integer

    Private blSuppressEvents As Boolean
    Public blConvView As Boolean

    'Left and Width Constants
    Private Const Top_Offset As Long = 6
    Private Const Top_Offset_C As Long = 0

    Private Const Left_frm As Long = 12
    Private Const Left_lbl1 As Long = 6
    Private Const Left_lbl2 As Long = 6
    Private Const Left_lbl3 As Long = 6
    Private Const Left_lbl4 As Long = 6
    Private Const Left_lbl5 As Long = 372           'Folder:
    Private Const Left_lblSender As Long = 66            '<SENDER>
    Private Const Left_lblSender_C As Long = 6             '<SENDER> Compact view
    Private Const Right_Aligned As Long = 648

    Private Const Left_lblTriage As Long = 181           'X Triage placeholder
    Private Const Left_lblActionable As Long = 198           '<ACTIONABL>

    Private Const Left_lblSubject As Long = 66            '<SUBJECT>
    Private Const Left_lblSubject_C As Long = 6             '<SUBJECT> Compact view

    Private Const Left_lblBody As Long = 66            '<BODY>
    Private Const Left_lblBody_C As Long = 6             '<BODY> Compact view

    Private Const Left_lblSentOn As Long = 66            '<SENTON>
    Private Const Left_lblSentOn_C As Long = 200           '<SENTON> Compact view

    Private Const Left_lblConvCt As Long = 290           'Count of Conversation Members
    Private Const Left_lblConvCt_C As Long = 320           'Count of Conversation Members Compact view

    Private Const Left_lblPos As Long = 6             'ACCELERATOR Email Position
    Private Const Left_cbxFolder As Long = 372           'Combo box containing Folder Suggestions
    Private Const Left_inpt As Long = 408           'Input for folder search

    Private Const Left_chbxGPConv As Long = 210           'Checkbox to Group Conversations
    Private Const Left_chbxGPConv_C As Long = 372           'Checkbox to Group Conversations

    Private Const Left_cbDelItem As Long = 588           'Delete email
    Private Const Left_cbKllItem As Long = 618           'Remove mail from Processing
    Private Const Left_cbFlagItem As Long = 569           'Flag as Task
    Private Const Left_lblAcF As Long = 363           'ACCELERATOR F for Folder Search
    Private Const Left_lblAcD As Long = 363           'ACCELERATOR D for Folder Dropdown

    Private Const Left_lblAcC As Long = 384           'ACCELERATOR C for Grouping Conversations
    Private Const Left_lblAcC_C As Long = 548           'ACCELERATOR C for Grouping Conversations

    Private Const Left_lblAcX As Long = 594           'ACCELERATOR X for Delete email
    Private Const Left_lblAcR As Long = 624           'ACCELERATOR R for remove item from list
    Private Const Left_lblAcT As Long = 330           'ACCELERATOR T for Task ... Flag item and make it a task

    Private Const Left_lblAcO As Long = 50            'ACCELERATOR O for Open Email
    Private Const Left_lblAcO_C As Long = 0            'ACCELERATOR O for Open Email

    Private Const Width_frm As Long = 655
    Private Const Width_lbl1 As Long = 54
    Private Const Width_lbl2 As Long = 54
    Private Const Width_lbl3 As Long = 54
    Private Const Width_lbl4 As Long = 52
    Private Const Width_lbl5 As Long = 78            'Folder:
    Private Const Width_lblSender As Long = 138           '<SENDER>
    Private Const Width_lblSender_C As Long = 174           '<SENDER> Compact view
    Private Const Width_lblTriage As Long = 11            'X Triage placeholder
    Private Const Width_lblActionable As Long = 72            '<ACTIONABL>

    Private Const Width_lblSubject As Long = 294           '<SUBJECT>
    Private Const Width_lblSubject_C As Long = 354           '<SUBJECT> Compact view

    Private Const Width_lblBody As Long = 294           '<BODY>
    Private Const Width_lblBody_C As Long = 354           '<BODY> Compact view

    Private Const Width_lblSentOn As Long = 80            '<SENTON>
    Private Const Width_lblConvCt As Long = 30            'Count of Conversation Members
    Private Const Width_lblPos As Long = 20            'ACCELERATOR Email Position
    Private Const Width_cbxFolder As Long = 276           'Combo box containing Folder Suggestions
    Private Const Width_inpt As Long = 156           'Input for folder search
    Private Const Width_chbxGPConv As Long = 96            'Checkbox to Group Conversations
    Private Const Width_cb As Long = 25            'Command buttons for: Delete email, Remove mail from Processing, and Flag as Task
    Private Const Width_lblAc As Long = 14            'ACCELERATOR Width
    Private Const Width_lblAcF As Long = 14            'ACCELERATOR F for Folder Search
    Private Const Width_lblAcD As Long = 14            'ACCELERATOR D for Folder Dropdown
    Private Const Width_lblAcC As Long = 14            'ACCELERATOR C for Grouping Conversations
    Private Const Width_lblAcX As Long = 14            'ACCELERATOR X for Delete email
    Private Const Width_lblAcR As Long = 14            'ACCELERATOR R for remove item from list
    Private Const Width_lblAcT As Long = 14            'ACCELERATOR T for Task ... Flag item and make it a task
    Private Const Width_lblAcO As Long = 14            'ACCELERATOR O for Open Email

    Private Const Height_UserForm As Long = 149          'Minimum height of Userform
    Private Const Width_UserForm As Long = 699.75        'Minimum width of Userform
    Private Const Width_PanelMain As Long = 683           'Minimum width of _viewer.PanelMain

    Private Height_UserForm_Max As Long
    Private Height_UserForm_Min As Long
    Private Height_PanelMain_Max As Long
    Private Height_PanelMain_Min As Long
    Private lngPanelMain_SC_Top As Long
    Private ReadOnly lngPanelMain_SC_Bottom As Long

    Private lngTop_OK_BUTTON_Min As Long
    Private lngTop_CANCEL_BUTTON_Min As Long
    Private lngTop_UNDO_BUTTON_Min As Long
    Private Const OK_left As Long = 216
    Private Const CANCEL_left As Long = 354
    Private Const OK_width As Long = 120
    Private Const UNDO_left As Long = 480
    Private Const UNDO_width As Long = 42
    Private ReadOnly lngTop_Button1_Min As Long
    Private lngTop_AcceleratorDialogue_Min As Long
    Private lngTop_spn_Min As Long
    Private Const spn_left As Long = 606
    Private lngTop_lbl_EmailPerLoad_Min As Long
    Private lng_lbl_EmailPerLoad_left As Long

    'Frame Design Constants
    Private Const frmHt = 72
    Private Const frmWd = 655
    Private Const frmLt = 12
    Private Const frmSp = 6

    Public colEmailsInFolder As Collection
    Private blRunningModalCode As Boolean = False

    'Global variables
    Private _globals As IApplicationGlobals
    Private ReadOnly _activeExplorer As Outlook.Explorer
    Private _olObjects As IOlObjects
    Private ReadOnly _olApp As Outlook.Application
    Private ReadOnly _viewer As QuickFileViewer
    Private _movedMails As cStackObject



    Public Sub New(AppGlobals As IApplicationGlobals,
                   Viewer As QuickFileViewer)
        _viewer = Viewer
        _viewer.SetController(Me)

        _globals = AppGlobals
        _olObjects = AppGlobals.Ol
        _olApp = _olObjects.App
        _activeExplorer = _olObjects.App.ActiveExplorer()
        _movedMails = _olObjects.MovedMails_Stack

        Dim lngPreviousHeight As Long
        Dim lngHeightDifference As Long

        blSuppressEvents = True                                     'Suppress events until the form is initialized
        InitType = InitTypeEnum.InitSort
        folderCurrent = _activeExplorer.CurrentFolder
        lngPanelMain_SC_Top = 0

        Height_UserForm_Min = _viewer.Height + frmHt + frmSp
        Height_PanelMain_Min = frmHt + frmSp

        lngHeightDifference = Height_UserForm_Min - _viewer.Height

        'Button1.top = Button1.top + lngHeightDifference
        _viewer.Button_OK.Top = _viewer.Button_OK.Top + lngHeightDifference
        _viewer.BUTTON_CANCEL.Top = _viewer.BUTTON_CANCEL.Top + lngHeightDifference
        _viewer.Button_Undo.Top = _viewer.Button_Undo.Top + lngHeightDifference
        lngAcceleratorDialogueTop = _viewer.AcceleratorDialogue.Top + lngHeightDifference
        _viewer.AcceleratorDialogue.Top = lngAcceleratorDialogueTop
        _viewer.spn_EmailPerLoad.Top = _viewer.spn_EmailPerLoad.Top + lngHeightDifference
        lngTop_spn_Min = _viewer.spn_EmailPerLoad.Top
        lngAcceleratorDialogueLeft = _viewer.AcceleratorDialogue.Left



        lngTop_OK_BUTTON_Min = _viewer.Button_OK.Top
        lngTop_CANCEL_BUTTON_Min = _viewer.BUTTON_CANCEL.Top
        lngTop_UNDO_BUTTON_Min = _viewer.Button_Undo.Top
        'lngTop_Button1_Min = Button1.top
        lngTop_AcceleratorDialogue_Min = _viewer.AcceleratorDialogue.Top

        'MsgBox "App Width " & Me.Width & vbCrLf & "Screen Width " & ScreenWidth * PointsPerPixel

        Height_UserForm_Max = ScreenHeight()

        lngPreviousHeight = _viewer.Height
        _viewer.Height = Height_UserForm_Max
        lngHeightDifference = _viewer.Height - lngPreviousHeight

        'Button1.top = Button1.top + lngHeightDifference
        _viewer.Button_OK.Top = _viewer.Button_OK.Top + lngHeightDifference
        _viewer.BUTTON_CANCEL.Top = _viewer.BUTTON_CANCEL.Top + lngHeightDifference
        _viewer.Button_Undo.Top = _viewer.Button_Undo.Top + lngHeightDifference
        lngAcceleratorDialogueTop = _viewer.AcceleratorDialogue.Top + lngHeightDifference
        _viewer.AcceleratorDialogue.Top = lngAcceleratorDialogueTop
        lngAcceleratorDialogueLeft = _viewer.AcceleratorDialogue.Left
        _viewer.spn_EmailPerLoad.Top = _viewer.spn_EmailPerLoad.Top + lngHeightDifference


        Height_PanelMain_Max = _viewer.PanelMain.Height + lngHeightDifference
        _viewer.PanelMain.Height = Height_PanelMain_Max

        'Button1.TabStop = False
        _viewer.Button_OK.TabStop = False
        _viewer.BUTTON_CANCEL.TabStop = False
        _viewer.Button_Undo.TabStop = False
        _viewer.PanelMain.TabStop = False
        _viewer.AcceleratorDialogue.TabStop = True
        _viewer.spn_EmailPerLoad.TabStop = False

        'blShowInConversations
        If _activeExplorer.CommandBars.GetPressedMso("ShowInConversations") Then
            blShowInConversations = True
        End If


        'If blShowInConversations Then
        '    'ToggleShowAsConversation -1
        '    ExplConvView_ToggleOff
        '    DoEvents
        'End If

        'Initialize Folder Suggestions and calculate emails per page

        'Folder_Suggestions_Reload()
        blSuppressEvents = False
        blSuppressEvents = True
        intEmailStart = 0       'Reverse sort is 0   'Regular sort is 1
        intEmailPosition = 0    'Reverse sort is 0   'Regular sort is 1
        'intEmailsPerIteration = CInt(Round((Height_PanelMain_Max / (frmHt + frmSp)), 0))
        intEmailsPerIteration = CInt(Math.Round(_viewer.PanelMain.Height / (frmHt + frmSp), 0))
        _viewer.spn_EmailPerLoad.Value = intEmailsPerIteration

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

        blSuppressEvents = False                                        'End suppression of events

        LoadEmailDataBase()
        _viewer.Show()
        Iterate()

    End Sub

    Public Sub SetAPIOptions()
        'Lets find the UserForm Handle the function below retrieves the handle
        'to the top-level window whose class name ("ThunderDFrame" for Excel)
        'and window name (me.caption or UserformName caption) match the specified strings.
        lFormHandle = FindWindow("ThunderDFrame", _viewer.Text)
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
        SetWindowLongPtr(lFormHandle, GWL_STYLE, lStyle)

        'Remove >'&LT; if you want to show form Maximised
        'ShowWindow lFormHandle, SW_SHOWMAXIMIZED 'Shows Form Maximized

        'The DrawMenuBar function redraws the menu bar of the specified window.
        'We need this as we have changed the menu bar after Windows has created it.
        'All we need is the Handle.
        DrawMenuBar(lFormHandle)
        ShowWindow(lFormHandle, SW_SHOWMAXIMIZED)
        'Modal    SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
        EnableWindow(OlApp_hWnd, Modal)
        EnableWindow(lFormHandle, Modeless)


    End Sub

    Public Sub LoadEmailDataBase(Optional colEmailsToLoad As Collection = Nothing)
        Dim OlFolder As Folder
        Dim objCurView As Microsoft.Office.Interop.Outlook.View
        Dim strFilter As String
        Dim OlItems As Items


        If colEmailsToLoad Is Nothing Then
            Dim unused As New Collection
            OlFolder = _activeExplorer.CurrentFolder
            objCurView = _activeExplorer.CurrentView
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

    Public Sub Iterate()
        Dim i As Integer
        Dim max As Double

        Dim colEmails As Collection



        colEmails = New Collection
        max = If(intEmailsPerIteration < colEmailsInFolder.Count, intEmailsPerIteration, colEmailsInFolder.Count)

        For i = 1 To max
            colEmails.Add(colEmailsInFolder(i))
        Next i
        For i = max To 1 Step -1
            'colEmailsInFolder.Remove(colEmailsInFolder(i))
            colEmailsInFolder.Remove(i)
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
                intUniqueItemCounter += 1
                Mail = objItem
                colCtrls = New Collection
                LoadGroupOfCtrls(colCtrls, intUniqueItemCounter)
                QF = New QfcController(Mail, colCtrls, intUniqueItemCounter, BoolRemoteMouseApp, Caller:=Me, AppGlobals:=_globals, hwnd:=lFormHandle, InitTypeE:=InitType)

                colQFClass.Add(QF)
                _olApp.DoEvents()
            End If
        Next objItem

        Dim unused3 = ShowWindow(lFormHandle, SW_SHOWMAXIMIZED)



        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            'ToggleOffline
            For Each QF In colQFClass
                QF.Init_FolderSuggestions()
                QF.CountMailsInConv()
                'DoEvents
            Next QF
            'ToggleOffline
        End If




        intAccActiveMail = 0

        If blSuppressEvents Then
            blSuppressEvents = False
            UserForm_Resize()
            blSuppressEvents = True
        Else
            Dim unused2 = _olApp.DoEvents
            UserForm_Resize()
        End If


        'Modal    SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
        Dim unused1 = EnableWindow(OlApp_hWnd, Modal)
        'EnableWindow lFormHandle, Modeless
        Dim unused = _viewer.PanelMain.Focus()



    End Sub


    Private Sub LoadGroupOfCtrls(ByRef colCtrls As Collection,
    intItemNumber As Integer,
    Optional intPosition As Integer = 0,
    Optional blGroupConversation As Boolean = True,
    Optional blWideView As Boolean = False)


        'Dim frm As Windows.Forms.frame
        Dim lbl1 As Windows.Forms.Label
        Dim lbl2 As Windows.Forms.Label
        Dim lbl3 As Windows.Forms.Label
        Dim lbl5 As Windows.Forms.Label
        Dim lblSender As Windows.Forms.Label
        Dim lblSubject As Windows.Forms.Label
        Dim txtboxBody As Windows.Forms.TextBox
        Dim lblSentOn As Windows.Forms.Label
        Dim lblConvCt As Windows.Forms.Label
        Dim lblPos As Windows.Forms.Label
        Dim cbxFolder As Windows.Forms.ComboBox
        Dim inpt As Windows.Forms.TextBox
        Dim chbxGPConv As Windows.Forms.CheckBox
        Dim chbxSaveAttach As Windows.Forms.CheckBox
        Dim chbxSaveMail As Windows.Forms.CheckBox
        Dim chbxDelFlow As Windows.Forms.CheckBox
        Dim cbDelItem As Windows.Forms.Button
        Dim cbKllItem As Windows.Forms.Button
        Dim cbFlagItem As Windows.Forms.Button
        Dim lblAcF As Windows.Forms.Label
        Dim lblAcD As Windows.Forms.Label
        Dim lblAcC As Windows.Forms.Label
        Dim lblAcX As Windows.Forms.Label
        Dim lblAcR As Windows.Forms.Label
        Dim lblAcT As Windows.Forms.Label
        Dim lblAcO As Windows.Forms.Label
        Dim lblAcA As Windows.Forms.Label
        Dim lblAcW As Windows.Forms.Label
        Dim lblAcM As Windows.Forms.Label


        Dim lngTopOff As Long

        Dim blDebug As Boolean


        blDebug = False

        lngTopOff = If(blWideView, Top_Offset, Top_Offset_C)
        'Button_OK.top = Button_OK.top + frmHt + frmSp
        'BUTTON_CANCEL.top = BUTTON_CANCEL.top + frmHt + frmSp

        If intPosition = 0 Then intPosition = intItemNumber

        If ((intItemNumber * (frmHt + frmSp)) + frmSp) > _viewer.PanelMain.Height Then      'Was Height_PanelMain_Max but I replaced with Me.Height
            _viewer.PanelMain.AutoScroll = True
            '_viewer.PanelMain.ScrollHeight = (intItemNumber * (frmHt + frmSp)) + frmSp 'PanelMain.ScrollHeight + frmHt + frmSp
        End If

        'Min Me Size is frmSp * 2 + frmHt
        frm = New System.Windows.Forms.Panel()
        _viewer.PanelMain.Controls.Add(frm)
        With frm
            .Height = frmHt
            .Top = ((frmSp + frmHt) * (intPosition - 1)) + frmSp
            .Left = frmLt
            .Width = frmWd
            .TabStop = False

        End With
        colCtrls.Add(frm, "frm")

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
            lbl1 = New Windows.Forms.Label
            frm.Controls.Add(lbl1)
            With lbl1
                .Height = 12
                .Top = lngTopOff
                .Left = 6
                .Width = 54
                .Text = "From:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl1, "lbl1")
        End If  'blWideView

        If blWideView Then
            lbl2 = New Windows.Forms.Label
            frm.Controls.Add(lbl2)
            With lbl2
                .Height = 12
                .Top = lngTopOff + 24
                .Left = 6
                .Width = 54
                .Text = "Subject:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl2, "lbl2")
        End If  'blWideView

        If blWideView Then
            lbl3 = New Windows.Forms.Label
            frm.Controls.Add(lbl3)
            With lbl3
                .Height = 12
                .Top = lngTopOff + 36
                .Left = 6
                .Width = 54
                .Text = "Body:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl3, "lbl3")
        End If

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            'TURN OFF IF CONDIT REMINDER
            lbl5 = New Windows.Forms.Label
            frm.Controls.Add(lbl5)

            With lbl5
                .Height = 12
                .Top = lngTopOff
                .Left = 372
                .Width = 78
                .Text = "Folder:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl5, "lbl5")
        End If

        lblSender = New Windows.Forms.Label
        frm.Controls.Add(lblSender)

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


            .Text = "<SENDER>"
            .Font = New Font(.Font.FontFamily, 10)
        End With
        colCtrls.Add(lblSender, "lblSender")


        Dim lblTriage As Windows.Forms.Label = New Windows.Forms.Label
        frm.Controls.Add(lblTriage)

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


            .Text = "ABC"
            .Font = New Font(.Font.FontFamily, 10)
        End With
        colCtrls.Add(lblTriage, "lblTriage")



        Dim lblActionable As Windows.Forms.Label = New Windows.Forms.Label
        frm.Controls.Add(lblActionable)

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


            .Text = "<ACTIONABL>"
            .Font = New Font(.Font.FontFamily, 10)
        End With
        colCtrls.Add(lblActionable, "lblActionable")



        lblSubject = New Windows.Forms.Label
        frm.Controls.Add(lblSubject)

        With lblSubject
            If blWideView Then
                .Height = 12
                .Top = lngTopOff + 24
                .Left = Left_lblSubject
                .Width = Width_lblSubject
                .Font = New Font(.Font.FontFamily, 10)
            ElseIf InitType.HasFlag(InitTypeEnum.InitConditionalReminder) Then
                .Height = 18
                .Top = lngTopOff + 12
                .Left = Left_lblSubject_C
                .Width = frmWd - .Left - .Left
                .Font = New Font(.Font.FontFamily, 16)
            Else
                .Height = 18
                .Top = lngTopOff + 12
                .Left = Left_lblSubject_C
                .Width = Width_lblSubject_C
                .Font = New Font(.Font.FontFamily, 16)
            End If

            .Text = "<SUBJECT>"
        End With
        colCtrls.Add(lblSubject, "lblSubject")

        txtboxBody = New Windows.Forms.TextBox
        frm.Controls.Add(txtboxBody)
        With txtboxBody

            If blWideView Then
                .Top = lngTopOff + 36
                .Left = Left_lblBody
                .Width = Width_lblBody
                .Height = 30 + 6 - lngTopOff
            ElseIf InitType.HasFlag(InitTypeEnum.InitConditionalReminder) Then
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

            .Text = "<BODY>"
            .Font = New Font(.Font.FontFamily, 10)
            .WordWrap = True
        End With
        colCtrls.Add(txtboxBody, "lblBody")

        lblSentOn = New Windows.Forms.Label
        frm.Controls.Add(lblSentOn)
        With lblSentOn
            .Height = 12
            If blWideView Then
                .Top = lngTopOff + 12
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
            cbxFolder = New Windows.Forms.ComboBox
            frm.Controls.Add(cbxFolder)
            With cbxFolder
                .Height = 24
                .Top = 20 + lngTopOff
                .Left = Left_cbxFolder
                .Width = Width_cbxFolder
                .Font = New Font(.Font.FontFamily, 8)
                .TabStop = False
            End With
            colCtrls.Add(cbxFolder, "cbxFolder")
        End If


        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            inpt = New Windows.Forms.TextBox
            frm.Controls.Add(inpt)
            With inpt
                .Height = 18
                .Top = lngTopOff
                .Left = 408
                .Width = Width_inpt
                .Font = New Font(.Font.FontFamily, 10)
                .TabStop = False
                .BackColor = SystemColors.Control

            End With
            colCtrls.Add(inpt, "inpt")



            chbxSaveMail = New Windows.Forms.CheckBox
            frm.Controls.Add(chbxSaveMail)
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

            chbxDelFlow = New Windows.Forms.CheckBox
            frm.Controls.Add(chbxDelFlow)
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

            chbxSaveAttach = New Windows.Forms.CheckBox
            frm.Controls.Add(chbxSaveAttach)
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
            chbxGPConv = New Windows.Forms.CheckBox
            frm.Controls.Add(chbxGPConv)
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

        cbFlagItem = New Windows.Forms.Button
        frm.Controls.Add(cbFlagItem)
        With cbFlagItem
            .Height = 18
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

        cbKllItem = New Windows.Forms.Button
        frm.Controls.Add(cbKllItem)
        With cbKllItem
            .Height = 18
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

        cbKllItem = New Windows.Forms.Button
        frm.Controls.Add(cbKllItem)
        With cbDelItem
            .Height = 18
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
            lblConvCt = New Windows.Forms.Label
            frm.Controls.Add(lblConvCt)
            With lblConvCt
                .Height = 18
                .TextAlign = ContentAlignment.TopRight 'fmTextAlignRight

                If blWideView Then
                    .Left = Left_lblConvCt
                    .Top = lngTopOff
                Else
                    .Left = Left_lblConvCt_C
                    .Top = lngTopOff + 12
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

        lblPos = New Windows.Forms.Label
        frm.Controls.Add(lblPos)
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
            lblAcF = New Windows.Forms.Label
            frm.Controls.Add(lblAcF)
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

            lblAcD = New Windows.Forms.Label
            frm.Controls.Add(lblAcD)
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

            lblAcC = New Windows.Forms.Label
            frm.Controls.Add(lblAcC)
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

        lblAcR = New Windows.Forms.Label
        frm.Controls.Add(lblAcR)
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

        lblAcX = New Windows.Forms.Label
        frm.Controls.Add(lblAcX)
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

        lblAcT = New Windows.Forms.Label
        frm.Controls.Add(lblAcT)
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

        lblAcO = New Windows.Forms.Label
        frm.Controls.Add(lblAcO)
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
            lblAcA = New Windows.Forms.Label
            frm.Controls.Add(lblAcA)
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

            lblAcW = New Windows.Forms.Label
            frm.Controls.Add(lblAcW)
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

            lblAcM = New Windows.Forms.Label
            frm.Controls.Add(lblAcM)
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

        'max = colQFClass.Count
        'For i = max To 1 Step -1
        If colQFClass IsNot Nothing Then
            Do While colQFClass.Count > 0
                i = colQFClass.Count
                QF = colQFClass(i)
                QF.ctrlsRemove()                                  'Remove controls on the frame
                _viewer.PanelMain.Controls.Remove(QF.frm)           'Remove the frame
                QF.kill()                                         'Remove the variables linking to events

                'PanelMain.Controls.Remove colFrames(i).Name
                colQFClass.Remove(i)
            Loop
        End If


        '_viewer.PanelMain.ScrollHeight = Height_PanelMain_Max



    End Sub

    Public Sub MoveDownControlGroups(intPosition As Integer, intMoves As Integer)

        Dim i As Integer
        Dim QF As QfcController
        Dim ctlFrame As Windows.Forms.Panel

        For i = colQFClass.Count To intPosition Step -1

            'Shift items downward if there are any
            QF = colQFClass(i)
            QF.intMyPosition += intMoves
            ctlFrame = QF.frm
            ctlFrame.Top = ctlFrame.Top + (intMoves * (frmHt + frmSp))
        Next i
        'PanelMain.ScrollHeight = max((intMoves + colQFClass.Count) * (frmHt + frmSp), Height_PanelMain_Max)


    End Sub

    Public Sub ToggleRemoteMouseLabels()
        BoolRemoteMouseApp = Not BoolRemoteMouseApp

        Dim QF As QfcController

        For Each QF In colQFClass
            QF.ToggleRemoteMouseAppLabels()
        Next QF

    End Sub

    Public Sub MoveDownPix(intPosition As Integer, intPix As Integer)


        Dim i As Integer
        Dim QF As QfcController
        Dim ctlFrame As Windows.Forms.Panel

        For i = colQFClass.Count To intPosition Step -1

            'Shift items downward if there are any
            QF = colQFClass(i)
            ctlFrame = QF.frm
            ctlFrame.Top += intPix
        Next i
        '_viewer.PanelMain.ScrollHeight = max(max(intPix, 0) + (colQFClass.Count * (frmHt + frmSp)), _viewer.PanelMain.Height)


    End Sub

    Public Sub AddEmailControlGroup(Optional objItem As Object = Nothing, Optional posInsert As Integer = 0, Optional blGroupConversation As Boolean = True, Optional ConvCt As Integer = 0, Optional varList As Object = Nothing, Optional blChild As Boolean = False)


        Dim Mail As [MailItem]
        Dim QF As QfcController
        Dim colCtrls As Collection
        Dim items As [Items]
        Dim i As Integer

        intUniqueItemCounter += 1

        If objItem Is Nothing Then
            items = folderCurrent.Items
            objItem = items(max() - intEmailPosition)
        End If

        If posInsert = 0 Then posInsert = colQFClass.Count + 1

        If TypeOf objItem Is MailItem Then
            Mail = objItem

            colCtrls = New Collection

            LoadGroupOfCtrls(colCtrls, intUniqueItemCounter, posInsert, blGroupConversation)
            QF = New QfcController(Mail, colCtrls, posInsert, BoolRemoteMouseApp, Me, _globals)
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

            If posInsert > colQFClass.Count Then
                colQFClass.Add(QF)
            Else
                'colQFClass.Add(QF, QF.Mail.Subject & QF.Mail.SentOn & QF.Mail.Sender, posInsert)
                colQFClass.Add(QF, posInsert)
            End If

            For i = 1 To colQFClass.Count
                QF = colQFClass(i)
                'Debug.Print "colQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.mail.Subject
            Next i

        End If



    End Sub

    Public Sub ConvToggle_Group(selItems As Collection, intOrigPosition As Integer)


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



    End Sub

    Public Sub ConvToggle_UnGroup(selItems As Collection, intPosition As Integer, ConvCt As Integer, varList As Object)

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

        MoveDownControlGroups(intPosition + 1, selItems.Count)

        For i = 1 To selItems.Count
            AddEmailControlGroup(selItems(i), intPosition + i, False, ConvCt, varList, True)
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
        For i = 1 To colQFClass.Count
            QF = colQFClass(i)
            If QF.Mail.EntryID = objMail.EntryID Then GetEmailPositionInCollection = i
        Next i



    End Function

    Friend Sub RemoveSpecificControlGroup(intPosition As Integer)

        Dim blDebug As Boolean
        Dim QF As QfcController
        Dim intItemCount As Integer
        Dim i As Integer
        Dim ctlFrame As Windows.Forms.Panel
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


        QF.ctrlsRemove()                                  'Run the method that removes controls from the frame
        _viewer.PanelMain.Controls.Remove(QF.frm)           'Remove the specific frame
        QF.kill()                                         'Remove the variables linking to events

        If blDebug Then
            'Print data before movement
            Debug.Print("DEBUG DATA BEFORE MOVEMENT")

            For i = 1 To intItemCount
                If i = intPosition Then
                    Debug.Print(i & "  " & intDeletedMyPos & "  " & strDeletedDte & "  " & strDeletedSub)
                Else
                    QF = colQFClass(i)
                    Debug.Print(i & "  " & QF.intMyPosition & "  " & Format(QF.Mail.SentOn, "MM\\DD\\YY HH:MM") & "  " & QF.Mail.Subject)
                End If
            Next i
        End If

        'Shift items upward if there are any
        If intPosition < intItemCount Then
            For i = intPosition + 1 To intItemCount
                QF = colQFClass(i)
                QF.intMyPosition -= 1
                ctlFrame = QF.frm
                ctlFrame.Top = ctlFrame.Top - frmHt - frmSp
            Next i
            '_viewer.PanelMain.ScrollHeight = max(_viewer.PanelMain.ScrollHeight - frmHt - frmSp, Height_PanelMain_Max)
        End If

        colQFClass.Remove(intPosition)
        intEmailStart += 1

        If blDebug Then
            'Print data after movement
            Debug.Print("DEBUG DATA POST MOVEMENT")

            For i = 1 To colQFClass.Count
                QF = colQFClass(i)
                Debug.Print(i & "  " & QF.intMyPosition & "  " & Format(QF.Mail.SentOn, "MM\\DD\\YY HH:MM") & "  " & QF.Mail.Subject)
            Next i
        End If

        QF = Nothing



    End Sub


    Friend Sub AcceleratorDialogue_Change()


        Dim strToParse As String
        Dim i As Integer
        Dim intLen As Integer
        Dim intLastNum As Integer
        Dim intAccTmpMail As Integer
        Dim strCommand As String
        Dim QF As QfcController
        Dim blExpanded As Boolean





        If Not blSuppressEvents Then


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


                    If intAccTmpMail <> intAccActiveMail Then


                        If intAccActiveMail <> 0 And intAccActiveMail <= colQFClass.Count Then


                            QF = colQFClass(intAccActiveMail)

                            If QF.blExpanded Then



                                MoveDownPix(intAccActiveMail + 1, QF.frm.Height * -0.5)
                                QF.ExpandCtrls1()

                                blExpanded = True
                            End If

                            QF.Accel_FocusToggle()

                        End If


                        If intAccTmpMail <> 0 And intAccTmpMail <= colQFClass.Count Then


                            QF = colQFClass(intAccTmpMail)

                            QF.Accel_FocusToggle()

                            If blExpanded Then



                                MoveDownPix(intAccTmpMail + 1, QF.frm.Height)
                                QF.ExpandCtrls1()

                            End If
                            _viewer.PanelMain.ScrollControlIntoView(QF.frm)
                            'ScrollIntoView_MF(QF.frm.Top, QF.frm.Top + QF.frm.Height)
                        End If


                        If intAccTmpMail <= colQFClass.Count Then


                            intAccActiveMail = intAccTmpMail
                        End If

                    End If

                    If intLen > intLastNum And intAccActiveMail <> 0 And intAccActiveMail <= colQFClass.Count Then


                        strCommand = UCase(Mid(strToParse, intLastNum + 1, 1))

                        If blSuppressEvents = False Then


                            blSuppressEvents = True

                            _viewer.AcceleratorDialogue.Text = intAccActiveMail

                            blSuppressEvents = False
                        Else


                            _viewer.AcceleratorDialogue.Text = intAccActiveMail
                        End If

                        QF = colQFClass(intAccActiveMail)

                        Select Case strCommand
                            Case "O"


                                toggleAcceleratorDialogue()

                                Dim unused2 = EnableWindow(OlApp_hWnd, Modeless)
                                'EnableWindow lFormHandle, Modeless

                                If _activeExplorer.CurrentFolder.DefaultItemType <> OlItemType.olMailItem Then
                                    _activeExplorer.NavigationPane.CurrentModule = _activeExplorer.NavigationPane.Modules.GetNavigationModule(OlNavigationModuleType.olModuleMail)
                                End If

                                If InitType.HasFlag(InitTypeEnum.InitSort) And AreConversationsGrouped(_activeExplorer) Then ExplConvView_ToggleOff()                      'Modal
                                QF.KB(strCommand)

                                QFD_Minimize()

                                If InitType.HasFlag(InitTypeEnum.InitSort) And blShowInConversations Then ExplConvView_ToggleOn()
                        'ToggleShowAsConversation 1
                        'SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
                            Case "C"


                                toggleAcceleratorDialogue()

                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "T"


                                toggleAcceleratorDialogue()

                                Dim unused1 = EnableWindow(OlApp_hWnd, Modeless)
                                'EnableWindow lFormHandle, Modeless

                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "F"


                                toggleAcceleratorDialogue()

                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "D"


                                toggleAcceleratorDialogue()

                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "X"


                                toggleAcceleratorDialogue()

                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "R"


                                toggleAcceleratorDialogue()

                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "A"


                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "W"


                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "M"


                                QF = colQFClass(intAccActiveMail)

                                QF.KB(strCommand)
                            Case "E"


                                QF = colQFClass(intAccActiveMail)

                                If QF.blExpanded Then


                                    MoveDownPix(intAccActiveMail + 1, QF.frm.Height * -0.5)
                                    QF.ExpandCtrls1()
                                Else


                                    MoveDownPix(intAccActiveMail + 1, QF.frm.Height)
                                    QF.ExpandCtrls1()
                                End If
                                '                                                                                                    strTemp = "AcceleratorDialogue.Value = Left(AcceleratorDialogue.Value, Len(AcceleratorDialogue.Value) - 1)"
                                '                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                '                        _viewer.AcceleratorDialogue.value = Left(AcceleratorDialogue.value, Len(AcceleratorDialogue.value) - 1)
                            Case Else


                                blSuppressEvents = True
                                '                                                                                                    strTemp = "AcceleratorDialogue.Value = Left(AcceleratorDialogue.Value, Len(AcceleratorDialogue.Value) - 1)"
                                '                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                                '                        _viewer.AcceleratorDialogue.value = Left(AcceleratorDialogue.value, Len(AcceleratorDialogue.value) - 1)

                                blSuppressEvents = False
                        End Select
                    End If


                End If


            Else


                If intAccActiveMail <> 0 Then


                    Dim unused = colQFClass(intAccActiveMail).Accel_FocusToggle

                    intAccActiveMail = 0
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


                If intAccActiveMail < colQFClass.Count Then


                    _viewer.AcceleratorDialogue.Text = intAccActiveMail + 1
                End If


            Case Keys.Up


                If AreConversationsGrouped(_activeExplorer) Then

                    'Modal                                                                                                    strTemp = "ExplConvView_ToggleOff"
                    'Modal                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                    'Modal                ExplConvView_ToggleOff
                End If



                If intAccActiveMail > 1 Then


                    _viewer.AcceleratorDialogue.Text = intAccActiveMail - 1
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
                    Dim unused = _viewer.PanelMain.Focus()
                End If
                SendKeys.Send("{ESC}")
            Case Keys.Right
                If sender.Visible And intAccActiveMail <> 0 Then
                    QF = colQFClass(intAccActiveMail)
                    If QF.lblConvCt.Text <> "1" And QF.chk.Checked = True Then
                        If QF.blExpanded Then
                            blExpanded = True
                            MoveDownPix(intAccActiveMail + 1, QF.frm.Height * -0.5)
                            QF.ExpandCtrls1()
                        End If
                        toggleAcceleratorDialogue()
                        QF.KB("C")
                        toggleAcceleratorDialogue()

                        If blExpanded Then
                            MoveDownPix(intAccActiveMail + 1, QF.frm.Height)
                            QF.ExpandCtrls1()
                        End If
                    End If
                End If
            Case Keys.Left
                If sender.Visible And intAccActiveMail <> 0 Then
                    QF = colQFClass(intAccActiveMail)
                    If QF.lblConvCt.Text <> "1" And QF.chk.Checked = False Then
                        If QF.blExpanded Then
                            blExpanded = True
                            MoveDownPix(intAccActiveMail + 1, QF.frm.Height * -0.5)
                            QF.ExpandCtrls1()
                        End If
                        toggleAcceleratorDialogue()
                        QF.KB("C")
                        toggleAcceleratorDialogue()

                        If blExpanded Then
                            MoveDownPix(intAccActiveMail + 1, QF.frm.Height)
                            QF.ExpandCtrls1()
                        End If

                    End If
                    sender.SelStart = sender.TextLength
                End If
            Case Else
        End Select
    End Sub

    Friend Sub BUTTON_CANCEL_Click()

        'ExplConvView_ToggleOn

        If blShowInConversations Then
            'ExplConvView_ToggleOn
            ExplConvView_Cleanup()
        End If
        'ToggleShowAsConversation 1
        RemoveControls()
        blFrmKll = True

        _viewer.Dispose()
    End Sub

    Friend Sub BUTTON_CANCEL_KeyDown(sender As Object, e As KeyEventArgs)
        KeyDownHandler(sender, e)
    End Sub

    Friend Sub Button_OK_Click()

        Dim QF As QfcController
        Dim blReadyForMove As Boolean
        Dim strNotifications As String

        If InitType.HasFlag(InitTypeEnum.InitSort) Then
            If blRunningModalCode = False Then
                blRunningModalCode = True

                blReadyForMove = True
                strNotifications = "Can't complete actions! Not all emails assigned to folder" & vbCrLf

                For Each QF In colQFClass
                    If QF.cbo.SelectedValue = "" Then
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
                        QF.MoveMail()
                    Next QF

                    'QuickFileMetrics_WRITE "9999QuickFileMetrics.csv"
                    QuickFileMetrics_WRITE("9999TimeWritingEmail.csv")
                    RemoveControls()
                    Iterate()
                    blSuppressEvents = False
                Else
                    Dim unused = MsgBox(strNotifications, vbOKOnly + vbCritical, "Error Notification")
                End If

                _viewer.AcceleratorDialogue.Text = ""
                intAccActiveMail = 0

                blRunningModalCode = False
            Else
                'MyBoxMsg("Can't Execute While Running Modal Code")
                MsgBox("Can't Execute While Running Modal Code")
            End If

        Else
            _viewer.Dispose()
        End If



    End Sub


    Friend Sub Button_OK_KeyDown(sender As Object, e As KeyEventArgs)


        'If DebugLVL And vbProcedure Then Debug.Print "Fired Button_OK_KeyDown"

        KeyDownHandler(sender, e)
    End Sub

    Friend Sub Button_OK_KeyUp(sender As Object, e As KeyEventArgs)
        KeyUpHandler(sender, e)
    End Sub


    Friend Sub Button_Undo_Click()
        Dim i As Integer
        Dim oMail_Old As MailItem
        Dim oMail_Current As MailItem
        Dim objTemp As Object
        Dim oFolder_Current As [Folder]
        Dim oFolder_Old As [Folder]
        Dim colItems As Collection
        Dim vbUndoResponse As MsgBoxResult
        Dim vbRepeatResponse As MsgBoxResult

        '    If Not colMailJustMoved Is Nothing Then
        '        If colMailJustMoved.Count <> 0 Then
        '
        '            oFolderCurrent = Application._activeExplorer.CurrentFolder
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

    Friend Sub PanelMain_KeyDown(sender As Object, e As KeyEventArgs)


        'If DebugLVL And vbProcedure Then Debug.Print "Fired _viewer.PanelMain_KeyDown"

        KeyDownHandler(sender, e)
    End Sub

    Friend Sub PanelMain_KeyPress(sender As Object, e As KeyPressEventArgs)
        'MsgBox ("KeyPress _viewer.PanelMain")
        KeyPressHandler(sender, e)
    End Sub

    Friend Sub PanelMain_KeyUp(sender As Object, e As KeyEventArgs)
        KeyUpHandler(sender, e)
    End Sub

    Friend Sub spn_EmailPerLoad_Change()
        If _viewer.spn_EmailPerLoad.Value >= 0 Then
            intEmailsPerIteration = _viewer.spn_EmailPerLoad.Value
        End If
    End Sub

    Private Sub spn_EmailPerLoad_KeyDown(sender As Object, e As KeyEventArgs)
        KeyDownHandler(sender, e)
    End Sub

    Private Sub UserForm_Activate()

        If StopWatch IsNot Nothing Then
            If StopWatch.isPaused = True Then
                StopWatch.reStart()
            End If
        End If
    End Sub

    Public Sub toggleAcceleratorDialogue()

        Dim QF As QfcController
        Dim i As Integer

        If colQFClass IsNot Nothing Then
            For i = 1 To colQFClass.Count


                QF = colQFClass(i)


                If QF.blExpanded And i <> colQFClass.Count Then MoveDownPix(i + 1, QF.frm.Height * -0.5)
                QF.Accel_Toggle()
            Next i
        End If


        If _viewer.AcceleratorDialogue.Visible = True Then


            _viewer.AcceleratorDialogue.Visible = False
            'Modal                                                                                strTemp = "ExplConvView_ToggleOn"
            'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Modal        ExplConvView_ToggleOn

            Dim unused = _viewer.PanelMain.Focus()
        Else


            If AreConversationsGrouped(_activeExplorer) Then
                'ToggleShowAsConversation -1

                'Modal                                                                                strTemp = "ExplConvView_ToggleOff"
                'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
                'Modal            ExplConvView_ToggleOff
            Else

            End If
            _viewer.AcceleratorDialogue.Visible = True


            If intAccActiveMail <> 0 Then


                _viewer.AcceleratorDialogue.Text = intAccActiveMail

                On Error Resume Next
                QF = colQFClass(intAccActiveMail)
                If Err.Number <> 0 Then
                    Err.Clear()
                    intAccActiveMail = 1
                    QF = colQFClass(intAccActiveMail)
                End If


                QF.Accel_FocusToggle()
            Else

            End If
            'Modal                                                                                strTemp = "SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&"
            'Modal                                                                                If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Modal        SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&

            _viewer.AcceleratorDialogue.Focus()
            _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength
        End If

        QF = Nothing



    End Sub

    'Private Sub ScrollIntoView_MF(lngItemTop As Long, lngItemBottom As Long)
    '    Dim DiffY As Long

    '    If lngItemTop < lngPanelMain_SC_Top Then
    '        'Diffy = lngItemTop - lngPanelMain_SC_Top
    '        'PanelMain.Scroll , Diffy
    '        'lngPanelMain_SC_Top = lngPanelMain_SC_Top = Diffy
    '        lngPanelMain_SC_Top = lngItemTop - frmSp
    '        _viewer.PanelMain.ScrollTop = lngPanelMain_SC_Top
    '    ElseIf (frmSp + lngItemBottom) > (lngPanelMain_SC_Top + _viewer.PanelMain.Height) Then
    '        DiffY = frmSp + lngItemBottom - (lngPanelMain_SC_Top + _viewer.PanelMain.Height)
    '        'PanelMain.Scroll yAction:=CInt(Diffy)
    '        lngPanelMain_SC_Top += DiffY
    '        _viewer.PanelMain.ScrollTop = lngPanelMain_SC_Top
    '    End If
    'End Sub


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





    Private Sub UserForm_Resize()
        Dim intDiffy As Integer
        Dim intDiffx As Integer
        Dim i As Integer
        Dim QF As QfcController

        'MsgBox "App Width " & Me.Width & vbCrLf & "Screen Width " & ScreenWidth * PointsPerPixel
        If Not blSuppressEvents Then

            intDiffx = If(_viewer.Width >= Width_UserForm - 100, _viewer.Width - Width_UserForm, 0)

            intDiffy = If(_viewer.Height >= Height_UserForm_Min, _viewer.Height - Height_UserForm_Min, 0)

            _viewer.PanelMain.Width = Width_PanelMain + intDiffx
            _viewer.PanelMain.Height = Height_PanelMain_Min + intDiffy

            _viewer.Button_OK.Top = lngTop_OK_BUTTON_Min + intDiffy
            _viewer.Button_OK.Left = OK_left + (intDiffx / 2)
            _viewer.BUTTON_CANCEL.Top = lngTop_CANCEL_BUTTON_Min + intDiffy
            _viewer.BUTTON_CANCEL.Left = _viewer.Button_OK.Left + CANCEL_left - OK_left
            _viewer.Button_Undo.Top = lngTop_UNDO_BUTTON_Min + intDiffy
            _viewer.Button_Undo.Left = _viewer.Button_OK.Left + UNDO_left - OK_left
            'Button1.top = lngTop_Button1_Min + intDiffy
            _viewer.AcceleratorDialogue.Top = lngTop_AcceleratorDialogue_Min + intDiffy
            _viewer.spn_EmailPerLoad.Top = lngTop_spn_Min + intDiffy
            _viewer.spn_EmailPerLoad.Left = spn_left + intDiffx


            If colQFClass IsNot Nothing Then
                For i = 1 To colQFClass.Count
                    QF = colQFClass(i)
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


    Private Sub UserForm_KeyPress(sender As Object, e As KeyPressEventArgs)
        If Not blSuppressEvents Then KeyPressHandler(sender, e)
    End Sub

    Private Sub UserForm_KeyUp(sender As Object, e As KeyEventArgs)
        If Not blSuppressEvents Then KeyUpHandler(sender, e)
    End Sub

    Private Sub UserForm_KeyDown(sender As Object, e As KeyEventArgs)
        If Not blSuppressEvents Then KeyDownHandler(sender, e)
    End Sub


    Public Sub KeyPressHandler(sender As Object, e As KeyPressEventArgs)
        If Not blSuppressEvents Then
            Select Case e.KeyChar

                Case Else
            End Select
        End If
    End Sub

    Public Sub KeyUpHandler(sender As Object, e As KeyEventArgs)
        If Not blSuppressEvents Then
            Select Case e.KeyCode
                Case Keys.Alt
                    If _viewer.AcceleratorDialogue.Visible Then
                        _viewer.AcceleratorDialogue.Focus()
                        _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength
                    Else
                        Dim unused = _viewer.PanelMain.Focus()
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

        If Not blSuppressEvents Then


            Select Case e.KeyCode
                Case Keys.Enter
                    Button_OK_Click()
                Case Keys.Tab
                    toggleAcceleratorDialogue()
                    If _viewer.AcceleratorDialogue.Visible Then _viewer.AcceleratorDialogue.Focus()
                    '        Case vbKeyEscape
                    '            vbMsgResponse = MsgBox("Stop all filing actions and close quick-filer?", vbOKCancel)
                    '            If vbMsgResponse = vbOK Then BUTTON_CANCEL_Click
                Case 18


                    toggleAcceleratorDialogue()

                    If _viewer.AcceleratorDialogue.Visible Then


                        _viewer.AcceleratorDialogue.Focus()

                    Else


                        Dim unused = _viewer.PanelMain.Focus()
                    End If
                Case Else


                    If _viewer.AcceleratorDialogue.Visible Then


                        AcceleratorDialogue_KeyDown(sender, e)
                    Else

                    End If
            End Select
        End If



    End Sub

    Public Sub QFD_Minimize()


        'Lets find the UserForm Handle the function below retrieves the handle
        'to the top-level window whose class name ("ThunderDFrame" for Excel)
        'and window name (me.caption or UserformName caption) match the specified strings.
        lFormHandle = FindWindow("ThunderDFrame", _viewer.Text)

        'EnableWindow lFormHandle, Modal
        If StopWatch IsNot Nothing Then
            If StopWatch.isPaused = False Then
                StopWatch.Pause()
            End If
        End If
        Dim unused2 = EnableWindow(OlApp_hWnd, Modeless)
        'EnableWindow lFormHandle, Modeless
        Dim unused1 = ShowWindow(lFormHandle, SW_FORCEMINIMIZE)
        Dim unused = ShowWindow(lFormHandle, SW_FORCEMINIMIZE)
    End Sub

    Public Sub QFD_Maximize()


        'Lets find the UserForm Handle the function below retrieves the handle
        'to the top-level window whose class name ("ThunderDFrame" for Excel)
        'and window name (me.caption or UserformName caption) match the specified strings.
        lFormHandle = FindWindow("ThunderDFrame", _viewer.Text)

        Dim unused1 = ShowWindow(lFormHandle, SW_SHOWMAXIMIZED)
        'Modal    SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&
        Dim unused = EnableWindow(OlApp_hWnd, Modal)
        'EnableWindow lFormHandle, Modeless


    End Sub

    Public Sub ExplConvView_Cleanup()

        On Error Resume Next
        objView = _activeExplorer.CurrentFolder.Views(objView_Mem)
        If Err.Number = 0 Then
            'objView.Reset
            objView.Apply()
            If objViewTemp IsNot Nothing Then objViewTemp.Delete()
            blShowInConversations = False
        Else
            Err.Clear()
            objViewTemp = _activeExplorer.CurrentView.Parent("tmpNoConversation")
            If objViewTemp IsNot Nothing Then objViewTemp.Delete()
        End If
    End Sub

    Public Sub ExplConvView_ToggleOff()
        If _olApp.ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations") Then
            blShowInConversations = True
            objView = _activeExplorer.CurrentView

            If objView.Name = "tmpNoConversation" Then
                If _activeExplorer.CommandBars.GetPressedMso("ShowInConversations") Then

                    objView.XML = Replace(objView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, , vbTextCompare)
                    objView.Save()
                    objView.Apply()
                End If

            End If

            objView_Mem = objView.Name
            If objView_Mem = "tmpNoConversation" Then objView_Mem = _globals.Ol.View_Wide

            'On Error Resume Next

            objViewTemp = objView.Parent("tmpNoConversation")

            If objViewTemp Is Nothing Then
                objViewTemp = objView.Copy("tmpNoConversation", OlViewSaveOption.olViewSaveOptionThisFolderOnlyMe)
                objViewTemp.XML = Replace(objView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, , vbTextCompare)
                objViewTemp.Save()

            End If


            'On Error GoTo ErrorHandler


            objViewTemp.Apply()




            If blSuppressEvents Then
                Dim unused1 = _olApp.DoEvents()
            Else
                blSuppressEvents = True
                Dim unused = _olApp.DoEvents()
                blSuppressEvents = False
            End If


            'Modal                                                                                                    strTemp = "SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&"
            'Modal                                                                                                    If DebugLVL And vbCommand Then TraceStack.Push SubNm & strTemp Else ttrace = ttrace & vbCrLf & SubNm & strTemp
            'Modal        SendMessage lFormHandle, WM_SETFOCUS, 0&, 0&


        End If



    End Sub

    Public Sub ExplConvView_ToggleOn()

        If blShowInConversations Then
            objView = _activeExplorer.CurrentFolder.Views(objView_Mem)
            'objView.Reset
            objView.Apply()
            'objViewTemp.Delete
            blShowInConversations = False
        End If

    End Sub


    Private Sub UserForm_Terminate()
        If blShowInConversations Then ExplConvView_ToggleOn()
        'ToggleShowAsConversation 1
    End Sub


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

        'Append it to the text file
        objShell = CreateObject("Shell.Application")
        objFSO = CreateObject("Scripting.FileSystemObject")



        LOC_TXT_FILE = Path.Combine(_globals.FS.FldrMyD, filename)


        Duration = StopWatch.timeElapsed
        OlEndTime = Now()
        OlStartTime = DateAdd("S", -Duration, OlEndTime)

        If colQFClass.Count > 0 Then
            Duration /= colQFClass.Count
        End If

        durationText = Format(Duration, "##0")
        'If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

        durationMinutesText = Format(Duration / 60, "##0.00")

        'dataLineBeg = dataLineBeg & durationText & "," & durationMinutesText & ","

        infoMail = New cInfoMail
        OlEmailCalendar = GetCalendar("Email Time", _olApp.Session)
        OlAppointment = OlEmailCalendar.Items.Add(New Outlook.AppointmentItem)
        With OlAppointment
            .Subject = "Quick Filed " & colQFClass.Count & " emails"
            .Start = OlStartTime
            .End = OlEndTime
            .Categories = "@ Email"
            .ReminderSet = False
            .Sensitivity = OlSensitivity.olPrivate
            .Save()
        End With

        ReDim strOutput(colQFClass.Count)
        For k = 1 To colQFClass.Count
            QF = colQFClass(k)
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
            'If DebugLVL And vbCommand Then Debug.Print SubNm & " dataline = " & dataLine
            strOutput(k) = dataLine
            '        a.WriteLine (dataLine)

            'Add to Email Calendar



            'End If

        Next k

        Write_TextFile(filename, strOutput, _globals.FS.FldrMyD)
        '    a.Close



    End Sub


    Private Function xComma(ByVal str As String) As String
        Dim strTmp As String

        strTmp = Replace(str, ", ", "_")
        strTmp = Replace(strTmp, ",", "_")
        xComma = GetStrippedText(strTmp)
        'xComma = StripAccents(strTmp)
    End Function


End Class
