Option Explicit On

Imports System
Imports System.Collections
Imports System.Diagnostics
Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook

Public Class Tags
    Private strTagOptions As Object
    Private strFilteredOptions As Object
    Private intFilteredMax As Integer
    Private intMaxOptions As Integer
    Private boolTagChoice() As Boolean
    Private boolFilteredChoice() As Boolean
    Public colCheckbox As Collection
    Public colCheckboxEvent As Collection
    Public colColorBox As Collection
    Public strOptions As String
    Public intFocus As Integer
    Public strPrefix As String
    Private objItem As Object
    Private objCaller As Object
    Private boolIsEmail As Boolean
    Private OlMail As Outlook.MailItem
    Private intCursorPosition As Integer
    Private dictOptions As Dictionary(Of String, String)
    Private strOriginalOptions As Object
    Private Const vbDblQuote = """"

    Public Function ToggleChoice(strChoice As String,
    Optional intIndex As Integer = 0,
    Optional intValue As Integer = 3) As Boolean

        Dim pos As Integer
        If intIndex = 0 Then
            pos = FindChoice(strChoice)
        Else
            pos = intIndex
        End If
        If pos <> 0 Then
            If intValue = 0 Then
                boolTagChoice(pos) = False
            ElseIf intValue = 1 Then
                boolTagChoice(pos) = True
            Else
                boolTagChoice(pos) = Not boolTagChoice(pos)
            End If
            ToggleChoice = True
        Else
            ToggleChoice = False
        End If
    End Function

    Public Function FindChoice(strChoice As String) As Integer
        On Error GoTo ErrorHandler
        Dim i As Integer
        Dim stp As Boolean

        i = 0
        Do
            i = i + 1
            If strTagOptions(i) = strChoice Then
                FindChoice = i
                stp = True
            End If
            If i = intMaxOptions And stp = False Then
                FindChoice = 0
                stp = True
            End If

        Loop Until stp


        Exit Function

ErrorHandler:

        Debug.Writeline("Error: " & Err.Description)
        Err.Clear()
        FindChoice = 0

    End Function

    Private Sub ShowChoices()
        Dim i As Integer
        Dim addition As Boolean
        strOptions = ""

        If IsArray(strTagOptions) Then
            For i = 1 To UBound(strTagOptions)
                If boolTagChoice(i) Then strOptions = strOptions & ", " & strTagOptions(i)
            Next i
            If strOptions <> "" Then strOptions = Strings.Right(strOptions, Len(strOptions) - 2)
        End If

    End Sub

    Private Function AddColorCategory(strPrefix As String, strNewCat As String) As Boolean
        Dim objNameSpace As Outlook.NameSpace
        Dim objCategory As Outlook.Category
        Dim OlColor As OlCategoryColor
        Dim strTemp As String

        objNameSpace = Globals.ThisAddIn._OlNS

        If strPrefix <> "" Then
            strTemp = strPrefix & strNewCat
        Else
            strTemp = strNewCat
        End If

        If strPrefix = "Tag PPL " Then
            OlColor = OlCategoryColor.olCategoryColorDarkGray
        ElseIf strPrefix = "Tag PROJECT " Then
            OlColor = OlCategoryColor.olCategoryColorTeal
        ElseIf strPrefix = "Tag TOPIC " Then
            OlColor = OlCategoryColor.olCategoryColorDarkTeal
        Else
            OlColor = OlCategoryColor.olCategoryColorNone
        End If

        On Error Resume Next

        objCategory = objNameSpace.Categories.Add(strTemp, OlColor,
        OlCategoryShortcutKey.olCategoryShortcutKeyNone)
        If Err.Number <> 0 Then
            MsgBox("Error Adding Category: " & vbDblQuote & strTemp & vbDblQuote & ". Please ensure the category name is unique.")
            AddColorCategory = False
        Else
            AddOption(strPrefix & strNewCat, blClickTrue:=True)
            AddColorCategory = True
        End If

    End Function

    Private Sub LoadControls(ByRef strOptions As Object, ByRef boolChoice() As Boolean, Optional varColors As Object)
        Dim i As Integer
        Dim max As Integer
        Dim intPreLen As Integer
        Dim strTemp As String

        'Dim ctrlCB          As MSForms.Control
        Dim ctrlCB As CheckBox
        Dim strChkName As String
        Dim clsCheckBox As cCheckBoxClass

        Dim ctrlLbl As Label

        Const cHt_var = 18
        Const cHt_fxd = 6
        Const cLt = 6
        Const cWt = 300
        'CatCol_To_UserformCol

        On Error GoTo ErrorHandler

        max = UBound(strOptions)
        intPreLen = Len(strPrefix)


        For i = 1 To max
            strChkName = Format(i, "00") & " ChkBx"
            strTemp = Strings.Right(strOptions(i), Len(strOptions(i)) - intPreLen)
            ctrlCB = New CheckBox()
            ctrlCB.Name = strChkName
            ctrlCB.Text = strTemp
            OptionsFrame.Controls.Add(ctrlCB)
            'ctrlCB = Me.OptionsFrame.Controls.Add("Forms.CheckBox.1", strChkName, True)

            If boolChoice(i) Then ctrlCB.Checked = True
            clsCheckBox = New cCheckBoxClass
            clsCheckBox.Init(Me, strPrefix)
            clsCheckBox.ctrlCB = ctrlCB

            'ctrlCB.AutoSize = True
            ctrlCB.Height = cHt_var
            ctrlCB.Top = (cHt_var * (i - 1)) + cHt_fxd
            ctrlCB.Left = cLt
            ctrlCB.Width = cWt
            'ctrlCB.Accelerator = i

            'NEED THIS FOR SCROLL HEIGHT
            'OptionsFrame.ScrollHeight = ctrlCB.Top + cHt_var


            colCheckbox.Add(ctrlCB, ctrlCB.Text)
            colCheckboxEvent.Add(clsCheckBox, ctrlCB.Text)


        Next i

        intFocus = 1

        Exit Sub
ErrorHandler:

        MsgBox("Error in Tags.LoadControls: " & Err.Description)
        Stop
        Resume

    End Sub

    Private Sub RemoveControls()
        Do While colCheckbox.Count > 0
            OptionsFrame.Controls.Remove colCheckbox.Item(1).Name
        colCheckbox.Remove 1
        colCheckboxEvent.Remove 1
    Loop
        Do While colColorBox.Count > 0
            OptionsFrame.Controls.Remove colColorBox.Item(1).Name
        colColorBox.Remove 1
    Loop
    End Sub
    Public Sub Call_OK()
        Button_OK_Click()
    End Sub

    Private Sub Button_AutoAssign_Click()
        Dim colPPL As Collection
        Dim strNewCat As String
        Dim objTmp As Object
        Dim ObjItem As Object
        Dim varNewCat As Variant
        Dim intIndex As Integer

        ObjItem = OlMail
        colPPL = Flag_Fields_Categories.dictPPL_AutoAddCatsInDictionary(ObjItem)
        For Each varNewCat In colPPL
            strNewCat = varNewCat
            intIndex = FindChoice(strNewCat)
            If intIndex Then
                ToggleChoice strNewCat, intIndex, 1
        Else
                AddOption strNewCat, blClickTrue:=True
        End If
        Next varNewCat

        If colPPL.Count > 0 Then FilterToSelected()
    End Sub

    Private Sub Button_New_Click()
        Dim strNewCat As String
        Dim objNameSpace As NameSpace
    Dim objCategory As Category
        Dim OlColor As OlCategoryColor
        Dim strTemp As String
        Dim vbR As VbMsgBoxResult
        Dim colPPL As Collection
        Dim varNewCat As Variant
        Dim blAltEntry As Boolean

        blAltEntry = True
        If strPrefix = "Tag PPL " And boolIsEmail Then
            vbR = MsgBox("Auto-add new from email addresses?", vbYesNo)
            If vbR = vbYes Then
                blAltEntry = False
                colPPL = Flag_Fields_Categories.dictPPL_AddMissingEntries(OlMail)
                For Each varNewCat In colPPL
                    strNewCat = varNewCat
                    AddOption strNewCat, blClickTrue:=True
            Next varNewCat
                If colPPL.Count > 0 Then FilterToSelected()
            End If
        End If

        If blAltEntry Then
            strNewCat = InputBox("What is the new category name?")
            If strNewCat <> "" Then
                If AddColorCategory(strPrefix:=strPrefix, strNewCat:=strNewCat) = True Then
                    'AddOption strPrefix & strNewCat, blClickTrue:=True
                    Me.TextBox1 = strNewCat
                End If
            End If
        End If

    End Sub

    Private Sub Button_OK_Click()
        ShowChoices()
        'MsgBox strOptions
        Me.Hide()
    End Sub


    Private Sub CommandButton3_Click()
        strOptions = ""
        Me.Hide()
    End Sub





    Private Sub OptionsFrame_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Dim newpos As Integer
        Select Case KeyCode
        'Case vbKeyRight
        '    newpos = lActivePos + 1
        '    If newpos <= colLabelEvent.Count Then _
        '        colLabelEvent(newpos).GenerateClick
        'Case vbKeyLeft
        '    newpos = lActivePos - 1
        '    If newpos >= 1 Then _
        '        colLabelEvent(newpos).GenerateClick
            Case vbKeyDown
                newpos = intFocus + 1
                If newpos <= colCheckbox.Count Then
                    colCheckbox.Item(newpos).SetFocus
                    intFocus = newpos
                End If
            Case vbKeyUp
                newpos = intFocus - 1
                If newpos >= 1 Then
                    colCheckbox.Item(newpos).SetFocus
                    intFocus = newpos
                End If
            Case Else
        End Select

    End Sub
    Private Sub AddOption(strOption As String, Optional blClickTrue As Boolean = False)
        Dim max As Integer
        max = UBound(strTagOptions)
        ReDim Preserve strTagOptions(max + 1)
        ReDim Preserve boolTagChoice(max + 1)

        strTagOptions(max + 1) = strOption
        boolTagChoice(max + 1) = blClickTrue
        intMaxOptions = UBound(strTagOptions)

    End Sub
    Private Sub FilterToSelected()
        Dim i As Integer
        Dim ct As Integer
        Dim strOptionBuilder() As String
        Dim boolChoiceBuilder() As Boolean

        RemoveControls()

        ct = 0
        ReDim strOptionBuilder(ct)
        ReDim boolChoiceBuilder(ct)

        For i = 1 To UBound(strTagOptions)
            If boolTagChoice(i) Then
                ct = ct + 1
                ReDim Preserve strOptionBuilder(ct)
                ReDim Preserve boolChoiceBuilder(ct)
                strOptionBuilder(ct) = strTagOptions(i)
                boolChoiceBuilder(ct) = boolTagChoice(i)
            End If
        Next i
        strFilteredOptions = strOptionBuilder
        intFilteredMax = ct
        boolFilteredChoice = boolChoiceBuilder

        If ct > 0 Then
            LoadControls(strFilteredOptions, boolFilteredChoice)
        End If


    End Sub
    Private Sub TextBox1_Change()
        Dim strTemp As String
        Dim i As Integer
        Dim loc As Integer

        'Debug.Writeline( Format(Now(), "h:mm Tags textbox change"))
        RemoveControls()
        strFilteredOptions = SearchArry4Str(strTagOptions, "*" & TextBox1.Value & "*")
        If IsArray(strFilteredOptions) Then
            strFilteredOptions = SortArray_Alphabetically(strFilteredOptions, SortOriginal)
            intFilteredMax = UBound(strFilteredOptions)
            ReDim boolFilteredChoice(intFilteredMax) As Boolean
    
        For i = 1 To intFilteredMax
                strTemp = strFilteredOptions(i)
                loc = FindChoice(strTemp)
                boolFilteredChoice(i) = boolTagChoice(loc)
            Next i
            LoadControls strFilteredOptions, boolFilteredChoice
    Else
            intFilteredMax = 0

        End If


    End Sub

    Private Sub TextBox1_KeyDown(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Select Case KeyCode
            Case vbKeyRight
                intCursorPosition = TextBox1.SelStart
        End Select
    End Sub

    Private Sub TextBox1_KeyUp(ByVal KeyCode As MSForms.ReturnInteger, ByVal Shift As Integer)
        Select Case KeyCode
            Case vbKeyRight
                If TextBox1.SelStart = intCursorPosition Then
                    FilterToSelected()
                End If
        End Select

    End Sub


    Private Sub TextBox1_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        Select Case KeyAscii
            Case vbKeyReturn
                Call_OK()
                '        Case Else
                '            TextBox1_Change
        End Select
    End Sub



    Public Sub Init(OlApp As Outlook.Application,
                    ByRef varStrAry As Object,
                    Optional varSelections As Object = Nothing,
                    Optional strTagPrefix As String = "",
                    Optional objItemObject As Object = Nothing,
                    Optional objCallerObj As Object = Nothing)

        If CCOCatList Is Nothing Then CCOCatList_Load
        strOriginalOptions = varStrAry
        If HideArchiveList.Value = True Then
            varStrAry = StripListFromArry(varStrAry, CCOCatList)
        End If
        Init2 varStrAry, varSelections, strTagPrefix, objItemObject, objCallerObj
End Sub

    Private Sub HideArchiveList_Click()
        If HideArchiveList.Value = True Then
            RemoveControls()
            Init2 StripListFromArry(strOriginalOptions, CCOCatList), strOptions, strPrefix, objItem, objCaller
    Else
            RemoveControls()
            Init2 strOriginalOptions, strOptions, strPrefix, objItem, objCaller
    End If
    End Sub

    Public Sub Init2(ByRef varStrAry As Variant,
    Optional varSelections As Variant = Nothing,
    Optional strTagPrefix As String = "",
    Optional objItemObject As Object,
    Optional objCallerObj As Object)

        If LBound(varStrAry) = 0 Then
            If Len(varStrAry(0)) > 0 Then
                MsgBox "Critical Error in Tags.Init: Array containing selection is zero based. Please use base 1", vbCritical
            Err.Raise 999, "Tags.Init", "varStrAry is zero based. Please use base 1"
            Exit Sub
            End If
        End If

        Dim i As Integer
        Dim strTemp As String
        Dim loc As Integer
        Dim blAddPrefixToSelectedItemArray As Boolean

        objItem = objItemObject
        If Not objItem Is Nothing Then
            If TypeOf objItem Is mailItem Then
                OlMail = objItem
                If Mail_IsItEncrypted(OlMail) = False Then
                    boolIsEmail = True
                Else
                    boolIsEmail = False
                    OlMail = Nothing
                End If
            End If
        End If
        objCaller = objCallerObj

        strPrefix = strTagPrefix
        strTagOptions = varStrAry
        'strTagOptions = SortArray_Alphabetically(strTagOptions, SortCopy)
        intMaxOptions = UBound(strTagOptions)
        ReDim boolTagChoice(intMaxOptions)
        colCheckbox = New Collection
        colCheckboxEvent = New Collection
        colColorBox = New Collection

        If strPrefix = "Tag PPL " And boolIsEmail Then
            Button_AutoAssign.Visible = True
            Button_AutoAssign.Enabled = True
        Else
            Button_AutoAssign.Visible = False
            Button_AutoAssign.Enabled = False
        End If

        'If Not varSelections Is Nothing Then
        If IsArray(varSelections) Then
            'If UBound(varSelections) <> 0 Then
            If Len(strPrefix) > 0 Then
                If Len(varSelections(0)) > Len(strPrefix) Then
                    If Left(varSelections(0), Len(strPrefix)) <> strPrefix Then
                        blAddPrefixToSelectedItemArray = True
                    End If
                Else
                    blAddPrefixToSelectedItemArray = True
                End If
            End If
            For i = LBound(varSelections) To UBound(varSelections)
                If Len(varSelections(i)) > 0 Then
                    If blAddPrefixToSelectedItemArray Then
                        strTemp = strPrefix & varSelections(i)
                    Else
                        strTemp = varSelections(i)
                    End If
                    If ToggleChoice(strTemp) = False Then
                        Dim tmpResult As VbMsgBoxResult
                        tmpResult = MsgBox(strTemp & " does not exist. Would you like to add it?", vbYesNo)
                        If tmpResult = vbYes Then
                            AddColorCategory strPrefix:=strPrefix, strNewCat:=CStr(varSelections(i))
                        End If
                    End If
                End If
            Next i
            'End If
        End If

        strFilteredOptions = SortArray_Alphabetically(strTagOptions, SortCopy)
        intFilteredMax = UBound(strFilteredOptions)
        ReDim boolFilteredChoice(intFilteredMax) As Boolean
    
    For i = 1 To intFilteredMax
            strTemp = strFilteredOptions(i)
            loc = FindChoice(strTemp)
            boolFilteredChoice(i) = boolTagChoice(loc)
        Next i
        LoadControls strFilteredOptions, boolFilteredChoice

    'LoadControls strTagOptions, boolTagChoice

    End Sub


    Private Sub UserForm_KeyPress(ByVal KeyAscii As MSForms.ReturnInteger)
        Select Case KeyAscii
            Case vbKeyReturn
                Call_OK()
            Case Else
        End Select
    End Sub


End Class