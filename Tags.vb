Option Explicit On

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

    Public Sub ToggleChoice(strChoice As String)
        Dim pos As Integer
        pos = FindChoice(strChoice)
        If pos <> 0 Then
            boolTagChoice(pos) = Not boolTagChoice(pos)
        End If
    End Sub

    Public Function FindChoice(strChoice As String) As Integer
        On Error GoTo ErrorHandler
        Dim i As Integer
        Dim stp As Boolean

        i = 0
        Do
            i = i + 1
            If strTagOptions(i) = strChoice Then
                stp = True
            End If
            If i = intMaxOptions Then stp = True
        Loop Until stp
        FindChoice = i

        Exit Function

ErrorHandler:

        Console.WriteLine("Error: " & Err.Description)
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
            'If strOptions <> "" Then strOptions = Right(strOptions, Len(strOptions) - 2)
            If strOptions <> "" Then strOptions = Mid(strOptions, 3)
        End If

    End Sub

    'Private Sub LoadControls(ByRef strOptions As Variant, ByRef boolChoice() As Boolean, Optional varColors As Variant)
    '    Dim i As Integer
    '    Dim max As Integer
    '    Dim intPreLen As Integer
    '    Dim strTemp As String

    '    'Dim ctrlCB          As MSForms.Control
    '    Dim ctrlCB As MSForms.checkbox
    '    Dim strChkName As String
    '    Dim clsCheckBox As cCheckBoxClass

    '    Dim ctrlLbl As MSForms.Label

    '    Const cHt_var = 18
    '    Const cHt_fxd = 6
    '    Const cLt = 6
    '    Const cWt = 300
    '    'CatCol_To_UserformCol

    '    max = UBound(strOptions)
    '    intPreLen = Len(strPrefix)


    '    For i = 1 To max
    '        strChkName = Format(i, "00") & " ChkBx"
    '    Set ctrlCB = Me.OptionsFrame.controls.Add("Forms.CheckBox.1", strChkName, True)
    '    strTemp = Right(strOptions(i), Len(strOptions(i)) - intPreLen)
    '        ctrlCB.Caption = strTemp
    '        If boolChoice(i) Then ctrlCB.value = True
    '    Set clsCheckBox = New cCheckBoxClass
    '    clsCheckBox.Init Me, strPrefix
    '    Set clsCheckBox.ctrlCB = ctrlCB

    '    'ctrlCB.AutoSize = True
    '    ctrlCB.Height = cHt_var
    '        ctrlCB.Top = (cHt_var * (i - 1)) + cHt_fxd
    '        ctrlCB.Left = cLt
    '        ctrlCB.Width = cWt
    '        ctrlCB.Accelerator = i

    '        OptionsFrame.ScrollHeight = ctrlCB.Top + cHt_var

    '        colCheckbox.Add ctrlCB, ctrlCB.Caption
    '    colCheckboxEvent.Add clsCheckBox, ctrlCB.Caption


    ' Next i

    '    intFocus = 1

    'End Sub

    'Private Sub RemoveControls()
    '    Do While colCheckbox.Count > 0
    '        OptionsFrame.Controls.Remove colCheckbox.Item(1).Name
    '    colCheckbox.Remove 1
    '    colCheckboxEvent.Remove 1
    'Loop
    '    Do While colColorBox.Count > 0
    '        OptionsFrame.Controls.Remove colColorBox.Item(1).Name
    '    colColorBox.Remove 1
    'Loop
    'End Sub
    'Public Sub Call_OK()
    '    Button_OK_Click
    'End Sub

End Class