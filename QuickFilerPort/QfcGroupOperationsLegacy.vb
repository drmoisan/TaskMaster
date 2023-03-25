Imports System.Windows.Forms
Imports System.Drawing
Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB
Imports ToDoModel
Imports System.Data.SqlClient

''' <summary>
''' Class manages UI interactions with the collection of Qfc controllers and viewers
''' </summary>
Friend Class QfcGroupOperationsLegacy
    Private ReadOnly _viewer As QuickFileViewer
    Private ReadOnly _initType As InitTypeEnum
    Private _globals As IApplicationGlobals
    Private _colQFClass As Collection
    Private _colFrames As Collection
    Private _intUniqueItemCounter As Integer
    Private _intActiveSelection As Integer
    Private _boolRemoteMouseApp As Boolean = False
    Private _lFormHandle As IntPtr
    Private _suppressAcceleratorEvents As Boolean = False
    Private _parent As QuickFileController

    Public Sub New(viewerInstance As QuickFileViewer,
                   InitType As InitTypeEnum,
                   AppGlobals As IApplicationGlobals,
                   ParentObject As QuickFileController)

        _viewer = viewerInstance
        _initType = InitType
        _globals = AppGlobals
        _parent = ParentObject
    End Sub

#Region "Viewer Operations"

    Friend Sub LoadControlsAndHandlers(colEmails As Collection)
        Dim objItem As Object
        Dim Mail As MailItem
        Dim QF As QfcController
        Dim colCtrls As Collection
        Dim blDebug As Boolean

        blDebug = False

        _colQFClass = New Collection
        _colFrames = New Collection

        _intUniqueItemCounter = 0

        For Each objItem In colEmails
            If TypeOf objItem Is MailItem Then
                _intUniqueItemCounter += 1
                Mail = objItem
                colCtrls = New Collection
                LoadGroupOfCtrls(colCtrls, _intUniqueItemCounter)

                QF = New QfcController(Mail,
                                       colCtrls,
                                       _intUniqueItemCounter,
                                       _boolRemoteMouseApp,
                                       Caller:=Me,
                                       AppGlobals:=_globals,
                                       hwnd:=_lFormHandle, InitTypeE:=_initType)
                _colQFClass.Add(QF)
            End If
        Next objItem

        _viewer.WindowState = FormWindowState.Maximized
        'ShowWindow(_lFormHandle, SW_SHOWMAXIMIZED)

        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            'ToggleOffline
            For Each QF In _colQFClass
                QF.Init_FolderSuggestions()
                QF.CountMailsInConv()
                'DoEvents
            Next QF
            'ToggleOffline
        End If

        _intActiveSelection = 0

        _parent.FormResize(True)
        _viewer.L1v1L2_PanelMain.Focus()
    End Sub

    Friend Sub LoadGroupOfCtrls(ByRef colCtrls As Collection,
    intItemNumber As Integer,
    Optional intPosition As Integer = 0,
    Optional blGroupConversation As Boolean = True,
    Optional blWideView As Boolean = False)

        Dim lngTopOff As Long
        Dim blDebug As Boolean = False

        lngTopOff = If(blWideView, Top_Offset, Top_Offset_C)
        If intPosition = 0 Then intPosition = intItemNumber

        If ((intItemNumber * (frmHt + frmSp)) + frmSp) > _viewer.L1v1L2_PanelMain.Height Then      'Was _heightPanelMainMax but I replaced with Me.Height
            _viewer.L1v1L2_PanelMain.AutoScroll = True

        End If

        'Min Me Size is frmSp * 2 + frmHt
        Dim Frm As New Panel()
        _viewer.L1v1L2_PanelMain.Controls.Add(Frm)
        With Frm
            .Height = frmHt
            .Top = ((frmSp + frmHt) * (intPosition - 1)) + frmSp + 16
            .Left = frmLt
            .Width = frmWd
            .TabStop = False
            .BorderStyle = BorderStyle.FixedSingle

        End With
        colCtrls.Add(Frm, "frm")

        If blWideView Then
            Dim lbl1 As New Label
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
            Dim lbl2 As New Label()
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
            Dim lbl3 As New Label
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

        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            'TURN OFF IF CONDITIONAL REMINDER
            Dim lbl5 As New Label
            Frm.Controls.Add(lbl5)

            With lbl5
                .Height = 16
                .Top = lngTopOff
                .Left = 372
                .Width = 60
                .Text = "Folder:"
                .Font = New Font(.Font.FontFamily, 10, FontStyle.Bold)
            End With
            colCtrls.Add(lbl5, "lbl5")
        End If

        Dim lblSender As New Label
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

        Dim lblTriage As New Label
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

        Dim lblActionable As New Label
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

        Dim lblSubject As New Label
        Frm.Controls.Add(lblSubject)

        With lblSubject
            If blWideView Then
                .Height = 16
                .Top = lngTopOff + 32
                .Left = Left_lblSubject
                .Width = Width_lblSubject
                .Font = New Font(.Font.FontFamily, 10)
            ElseIf _initType.HasFlag(InitTypeEnum.InitConditionalReminder) Then
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

        Dim txtboxBody As New TextBox
        Frm.Controls.Add(txtboxBody)
        With txtboxBody

            If blWideView Then
                .Top = lngTopOff + 36
                .Left = Left_lblBody
                .Width = Width_lblBody
                .Height = 40 + 8 - lngTopOff
            ElseIf _initType.HasFlag(InitTypeEnum.InitConditionalReminder) Then
                .Top = lngTopOff + 40
                .Left = Left_lblBody_C
                .Width = frmWd - .Left - .Left
                .Height = 48 + 8 - lngTopOff
            Else
                .Top = lngTopOff + 40
                .Left = Left_lblBody_C
                .Width = Width_lblBody_C
                .Height = 48 + 8 - lngTopOff

            End If

            .Text = "<BODY>"
            .Font = New Font(.Font.FontFamily, 10)
            .WordWrap = True
            .Multiline = True
            .ReadOnly = True
            .BorderStyle = BorderStyle.None
        End With
        colCtrls.Add(txtboxBody, "lblBody")

        Dim lblSentOn As New Label
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

        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            Dim cbxFolder As New ComboBox
            Frm.Controls.Add(cbxFolder)
            With cbxFolder
                .Height = 24
                .Top = 27 + lngTopOff
                .Left = Left_cbxFolder
                .Width = Width_cbxFolder
                .Font = New Font(.Font.FontFamily, 8)
                .TabStop = False
                .DropDownStyle = ComboBoxStyle.DropDownList
            End With
            colCtrls.Add(cbxFolder, "cbxFolder")
        End If

        Dim chbxGPConv As New CheckBox
        Dim chbxSaveAttach As New CheckBox
        Dim chbxDelFlow As New CheckBox
        Dim chbxSaveMail As New CheckBox
        Dim inpt As New TextBox
        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            Frm.Controls.Add(inpt)
            With inpt
                .Height = 24
                .Top = lngTopOff
                .Left = Left_inpt
                .Width = Width_inpt
                .Font = New Font(.Font.FontFamily, 10)
                .TabStop = False
                .BackColor = SystemColors.Control

            End With
            colCtrls.Add(inpt, "inpt")

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

        Dim cbFlagItem As New Button
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

        Dim cbKllItem As New Button
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

        Dim cbDelItem As New Button
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

        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            Dim lblConvCt As New Label
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

        Dim lblPos As New Label
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

        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            Dim lblAcF As New Label
            Frm.Controls.Add(lblAcF)
            With lblAcF
                .Height = 14
                .Top = Math.Max(lngTopOff - 2, 0)
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

            Dim lblAcD As New Label
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

            Dim lblAcC As New Label
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

        Dim lblAcR As New Label
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

        Dim lblAcX As New Label
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

        Dim lblAcT As New Label
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

        Dim lblAcO As New Label
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

        If _initType.HasFlag(InitTypeEnum.InitSort) Then
            Dim lblAcA As New Label
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

            Dim lblAcW As New Label
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
                .BackColor = SystemColors.ControlText
                .ForeColor = SystemColors.Control
                .Visible = blDebug
            End With
            colCtrls.Add(lblAcW, "lblAcW")

            Dim lblAcM As New Label
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
                .BackColor = SystemColors.ControlText
                .ForeColor = SystemColors.Control
                .Visible = blDebug
            End With
            colCtrls.Add(lblAcM, "lblAcM")
        End If



    End Sub

    Friend Sub RemoveControls()

        Dim QF As QfcController
        Dim i As Integer

        'max = _colQFClass.Count
        'For i = max To 1 Step -1
        If _colQFClass IsNot Nothing Then
            Do While _colQFClass.Count > 0
                i = _colQFClass.Count
                QF = _colQFClass(i)
                QF.ctrlsRemove()                                  'Remove controls on the frame
                _viewer.L1v1L2_PanelMain.Controls.Remove(QF.frm)           'Remove the frame
                QF.kill()                                         'Remove the variables linking to events

                'PanelMain.Controls.Remove _colFrames(i).Name
                _colQFClass.Remove(i)
            Loop
        End If

        '_viewer.L1v1L2_PanelMain.ScrollHeight = _heightPanelMainMax



    End Sub

    Friend Sub MoveDownControlGroups(intPosition As Integer, intMoves As Integer)

        Dim i As Integer
        Dim QF As QfcController
        Dim ctlFrame As Panel

        For i = _colQFClass.Count To intPosition Step -1

            'Shift items downward if there are any
            QF = _colQFClass(i)
            QF.Position += intMoves
            ctlFrame = QF.frm
            ctlFrame.Top = ctlFrame.Top + (intMoves * (frmHt + frmSp))
        Next i
        'PanelMain.ScrollHeight = max((intMoves + _colQFClass.Count) * (frmHt + frmSp), _heightPanelMainMax)


    End Sub

    Public Sub ToggleRemoteMouseLabels()
        _boolRemoteMouseApp = Not _boolRemoteMouseApp

        Dim QF As QfcController

        For Each QF In _colQFClass
            QF.ToggleRemoteMouseAppLabels()
        Next QF

    End Sub

    Public Sub MoveDownPix(intPosition As Integer, intPix As Integer)

        Dim i As Integer
        Dim QF As QfcController
        Dim ctlFrame As Panel

        For i = _colQFClass.Count To intPosition Step -1

            'Shift items downward if there are any
            QF = _colQFClass(i)
            ctlFrame = QF.frm
            ctlFrame.Top += intPix
        Next i

    End Sub

    Public Sub AddEmailControlGroup(objItem As Object, Optional posInsert As Integer = 0, Optional blGroupConversation As Boolean = True, Optional ConvCt As Integer = 0, Optional varList As Object = Nothing, Optional blChild As Boolean = False)

        Dim Mail As MailItem
        Dim QF As QfcController
        Dim colCtrls As Collection

        _intUniqueItemCounter += 1
        If posInsert = 0 Then posInsert = _colQFClass.Count + 1
        If TypeOf objItem Is MailItem Then
            Mail = objItem
            colCtrls = New Collection
            LoadGroupOfCtrls(colCtrls, _intUniqueItemCounter, posInsert, blGroupConversation)
            QF = New QfcController(Mail, colCtrls, posInsert, _boolRemoteMouseApp, Me, _globals)
            If blChild Then QF.blHasChild = True
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

            If posInsert > _colQFClass.Count Then
                _colQFClass.Add(QF)
            Else
                '_colQFClass.Add(QF, QF.Mail.Subject & QF.Mail.SentOn & QF.Mail.Sender, posInsert)
                _colQFClass.Add(QF, Before:=posInsert)
            End If

            'For i = 1 To _colQFClass.Count
            '    QF = _colQFClass(i)
            '    Debug.WriteLine("_colQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.Mail.Subject)
            'Next i

        End If

    End Sub

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

        intItemCount = _colQFClass.Count

        QF = _colQFClass(intPosition)                'Set class equal to specific member of collection
        On Error Resume Next
        strDeletedSub = QF.Mail.Subject
        strDeletedDte = Format(QF.Mail.SentOn, "mm\\dd\\yyyy hh:mm")
        intDeletedMyPos = QF.Position


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
                    QF = _colQFClass(i)
                    Debug.Print(i & "  " & QF.Position & "  " & Format(QF.Mail.SentOn, "MM\\DD\\YY HH:MM") & "  " & QF.Mail.Subject)
                End If
            Next i
        End If

        'Shift items upward if there are any
        If intPosition < intItemCount Then
            For i = intPosition + 1 To intItemCount
                QF = _colQFClass(i)
                QF.Position -= 1
                ctlFrame = QF.frm
                ctlFrame.Top = ctlFrame.Top - frmHt - frmSp
            Next i
            '_viewer.L1v1L2_PanelMain.ScrollHeight = max(_viewer.L1v1L2_PanelMain.ScrollHeight - frmHt - frmSp, _heightPanelMainMax)
        End If

        _colQFClass.Remove(intPosition)

        If blDebug Then
            'Print data after movement
            Debug.Print("DEBUG DATA POST MOVEMENT")

            For i = 1 To _colQFClass.Count
                QF = _colQFClass(i)
                Debug.Print(i & "  " & QF.Position & "  " & Format(QF.Mail.SentOn, "MM\\DD\\YY HH:MM") & "  " & QF.Mail.Subject)
            Next i
        End If

        QF = Nothing
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

        QF_Orig = _colQFClass(intOrigPosition)

        If blDebug Then
            For i = 1 To _colQFClass.Count
                QF = _colQFClass(i)
                'Debug.Print "_colQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.mail.Subject
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
            For i = 1 To _colQFClass.Count
                QF = _colQFClass(i)
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
            For i = 1 To _colQFClass.Count
                QF = _colQFClass(i)
                'Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.mail.SentOn, "MM\DD\YY HH:MM") & "  " & QF.mail.Subject
            Next i
        End If
        _parent.FormResize(False)


    End Sub

    Friend Sub ResizeChildren(intDiffx As Integer)
        If _colQFClass IsNot Nothing Then
            For Each QF As QfcController In _colQFClass
                If QF.blHasChild Then
                    QF.frm.Left = frmLt * 2
                    QF.frm.Width = Width_frm + intDiffx - frmLt
                    QF.ResizeCtrls(intDiffx - frmLt)
                Else
                    QF.frm.Width = Width_frm + intDiffx
                    QF.ResizeCtrls(intDiffx)
                End If
            Next
        End If
    End Sub

#End Region

#Region "Keyboard UI"
    Public Sub toggleAcceleratorDialogue()
        Dim QF As QfcController
        Dim i As Integer

        If _colQFClass IsNot Nothing Then
            For i = 1 To _colQFClass.Count
                QF = _colQFClass(i)
                If QF.blExpanded And i <> _colQFClass.Count Then MoveDownPix(i + 1, QF.frm.Height * -0.5)
                QF.Accel_Toggle()
            Next i
        End If

        If _viewer.AcceleratorDialogue.Visible = True Then
            _viewer.AcceleratorDialogue.Visible = False
            _viewer.L1v1L2_PanelMain.Focus()
        Else
            If AreConversationsGrouped(_globals.Ol.App.ActiveExplorer) Then

            End If
            _viewer.AcceleratorDialogue.Visible = True
            If _intActiveSelection <> 0 Then
                _viewer.AcceleratorDialogue.Text = _intActiveSelection
                Try
                    QF = _colQFClass(_intActiveSelection)
                Catch ex As System.Exception
                    _intActiveSelection = 1
                    QF = _colQFClass(_intActiveSelection)
                End Try
                QF.Accel_FocusToggle()
            End If

            _viewer.AcceleratorDialogue.Focus()
            _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength
        End If

        QF = Nothing
    End Sub

    Friend Sub ParseAcceleratorText()
        Dim parser As New AcceleratorParser(Me)
        parser.ParseAndExecute(_viewer.AcceleratorDialogue.Text, _intActiveSelection)
    End Sub

    Friend Sub ResetAcceleratorSilently()
        Dim blTemp As Boolean = _suppressAcceleratorEvents
        _suppressAcceleratorEvents = True
        If _intActiveSelection > 0 Then
            _viewer.AcceleratorDialogue.Text = _intActiveSelection
        Else
            _viewer.AcceleratorDialogue.Text = ""
        End If
        _suppressAcceleratorEvents = blTemp
    End Sub

    Friend Function ActivateByIndex(intNewSelection As Integer, blExpanded As Boolean) As Integer
        If intNewSelection > 0 And intNewSelection <= _colQFClass.Count Then
            Dim QF As QfcController = _colQFClass(intNewSelection)
            QF.Accel_FocusToggle()
            If blExpanded Then
                MoveDownPix(intNewSelection + 1, QF.frm.Height)
                QF.ExpandCtrls1()
            End If
            _intActiveSelection = intNewSelection
            _viewer.L1v1L2_PanelMain.ScrollControlIntoView(QF.frm)
            Return _intActiveSelection
        Else
            'Procedure failed so return current selection unaltered
            Return _intActiveSelection
        End If
    End Function

    Friend Function ToggleOffActiveItem(parentBlExpanded As Boolean) As Boolean
        Dim blExpanded As Boolean = parentBlExpanded
        If _intActiveSelection <> 0 Then

            Dim QF As QfcController = _colQFClass(_intActiveSelection)
            If QF.blExpanded Then
                MoveDownPix(_intActiveSelection + 1, QF.frm.Height * -0.5)
                QF.ExpandCtrls1()
                blExpanded = True
            End If
            QF.Accel_FocusToggle()


            _intActiveSelection = 0
        End If
        Return blExpanded
    End Function

    Friend Sub SelectPreviousItem()
        If _intActiveSelection > 1 Then
            _viewer.AcceleratorDialogue.Text = _intActiveSelection - 1
        End If
        _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength
    End Sub

    Friend Sub SelectNextItem()
        If _intActiveSelection < _colQFClass.Count Then
            _viewer.AcceleratorDialogue.Text = _intActiveSelection + 1
        End If
        _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength
    End Sub

    Friend Sub MakeSpaceToEnumerateConversation()
        Dim blExpanded As Boolean = False
        If _intActiveSelection <> 0 Then
            Dim QF As QfcController = _colQFClass(_intActiveSelection)
            If QF.lblConvCt.Text <> "1" And QF.chk.Checked = True Then
                If QF.blExpanded Then
                    blExpanded = True
                    MoveDownPix(_intActiveSelection + 1, QF.frm.Height * -0.5)
                    QF.ExpandCtrls1()
                End If
                toggleAcceleratorDialogue()
                'QF.KB toggles the conversation checkbox which triggers enumeration of conversation
                QF.KB("C")
                toggleAcceleratorDialogue()

                If blExpanded Then
                    MoveDownPix(_intActiveSelection + 1, QF.frm.Height)
                    QF.ExpandCtrls1()
                End If
            End If
        End If
    End Sub

    Friend Sub RemoveSpaceToCollapseConversation()
        If _intActiveSelection <> 0 Then
            Dim blExpanded As Boolean = False
            Dim QF As QfcController = _colQFClass(_intActiveSelection)
            If QF.lblConvCt.Text <> "1" And QF.chk.Checked = False Then
                If QF.blExpanded Then
                    blExpanded = True
                    MoveDownPix(_intActiveSelection + 1, QF.frm.Height * -0.5)
                    QF.ExpandCtrls1()
                End If
                toggleAcceleratorDialogue()
                QF.KB("C")
                toggleAcceleratorDialogue()

                If blExpanded Then
                    MoveDownPix(_intActiveSelection + 1, QF.frm.Height)
                    QF.ExpandCtrls1()
                End If

            End If
            _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength
        End If
    End Sub

    Friend Function IsSelectionBelowMax(intNewSelection As Integer) As Boolean
        If intNewSelection <= _colQFClass.Count Then
            Return True
        Else
            Return False
        End If
    End Function

#End Region

#Region "Properties and Helper Functions"
    Friend ReadOnly Property Parent As QuickFileController
        Get
            Return _parent
        End Get
    End Property

    Friend ReadOnly Property EmailsLoaded
        Get
            Return _colQFClass.Count
        End Get
    End Property

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
        For i = 1 To _colQFClass.Count
            QF = _colQFClass(i)
            If QF.Mail.EntryID = objMail.EntryID Then GetEmailPositionInCollection = i
        Next i



    End Function

    Friend Function TryGetQfc(index) As QfcController
        Try
            Return _colQFClass(index)
        Catch ex As System.Exception
            Return Nothing
        End Try
    End Function

#End Region

#Region "Email Filing"
    Friend ReadOnly Property ReadyForMove() As Boolean
        Get
            Dim blReadyForMove As Boolean = True
            Dim strNotifications As String = "Can't complete actions! Not all emails assigned to folder" & vbCrLf

            For Each QF In _colQFClass
                If QF.cbo.SelectedValue = "" Then
                    blReadyForMove = False
                    strNotifications = strNotifications & QF.intMyPosition &
                "  " & Format(QF.Mail.SentOn, "mm\\dd\\yyyy") &
                "  " & QF.Mail.Subject & vbCrLf
                End If
            Next QF
            strNotifications = Mid(strNotifications, 1, Len(strNotifications) - 1)
            If Not blReadyForMove Then MsgBox(strNotifications, vbOKOnly + vbCritical, "Error Notification")
            Return blReadyForMove
        End Get
    End Property

    Friend Sub MoveEmails(ByRef MovedMails As cStackObject)
        If _viewer.AcceleratorDialogue.Visible = True Then
            _viewer.AcceleratorDialogue.Text = ""
            toggleAcceleratorDialogue()
        Else
            _intActiveSelection = 0
        End If
        For Each QF As QfcController In _colQFClass
            QF.MoveMail()
            MovedMails.Push(QF.Mail)
        Next QF
    End Sub

    Friend Function GetMoveDiagnostics(durationText As String,
                                    durationMinutesText As String,
                                    Duration As Double,
                                    dataLineBeg As String,
                                    OlEndTime As Date,
                                    ByRef OlAppointment As AppointmentItem) As String()
        Dim k As Integer
        Dim strOutput(EmailsLoaded) As String
        For k = 1 To EmailsLoaded
            Dim QF As QfcController = _colQFClass(k)

            On Error Resume Next
            Dim infoMail As New cInfoMail
            If infoMail.Init_wMail(QF.Mail, OlEndTime:=OlEndTime, lngDurationSec:=Duration) Then
                If OlAppointment.Body = "" Then
                    OlAppointment.Body = infoMail.ToString
                    OlAppointment.Save()
                Else
                    OlAppointment.Body = OlAppointment.Body & vbCrLf & infoMail.ToString
                    OlAppointment.Save()
                End If
            End If
            Dim dataLine As String = dataLineBeg & xComma(QF.LblSubject.Text)
            dataLine = dataLine & "," & "QuickFiled"
            dataLine = dataLine & "," & durationText
            dataLine = dataLine & "," & durationMinutesText
            dataLine = dataLine & "," & xComma(QF.StrlblTo)
            dataLine = dataLine & "," & xComma(QF.Sender)
            dataLine = dataLine & "," & "Email"
            dataLine = dataLine & "," & xComma(QF.cbo.SelectedItem.ToString())           'Target Folder
            dataLine = dataLine & "," & QF.lblSentOn.Text
            dataLine = dataLine & "," & Format(QF.Mail.SentOn, "hh:mm")
            strOutput(k) = dataLine
        Next k
    End Function

    Private Function xComma(ByVal str As String) As String
        Dim strTmp As String

        strTmp = Replace(str, ", ", "_")
        strTmp = Replace(strTmp, ",", "_")
        xComma = GetStrippedText(strTmp)
        'xComma = StripAccents(strTmp)
    End Function
#End Region

End Class
