Imports System.Runtime.Remoting.Contexts
Imports System.Windows
Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook
Imports System
Imports System.Linq
Imports System.Collections.Generic

Public Class TagController

    Private ReadOnly _viewer As TagViewer
    Private ReadOnly _dict_original As SortedDictionary(Of String, Boolean)
    Private _dict_options As SortedDictionary(Of String, Boolean)
    Private _filtered_options As SortedDictionary(Of String, Boolean)
    Private _selections As List(Of String)
    Private _filtered_selections As List(Of String)
    Private ReadOnly _obj_item As Object
    Private ReadOnly _ol_mail As MailItem
    Private ReadOnly _obj_caller As Object
    Private ReadOnly _prefix As String
    Private ReadOnly _col_cbx_ctrl As List(Of Object) = New List(Of Object)
    Private ReadOnly _col_cbx_event As List(Of Object) = New List(Of Object)
    Private ReadOnly _col_colorbox As List(Of Object) = New List(Of Object)
    Private ReadOnly _ismail As Boolean
    Public _exit_type As String = "Cancel"
    Private _cursor_position As Integer
    Public int_focus As Integer


#Region "Public Functions"
    Public Sub New(viewer_instance As TagViewer,
                   dictOptions As SortedDictionary(Of String, Boolean),
                   Optional selections As List(Of String) = Nothing,
                   Optional tag_prefix As String = "",
                   Optional objItemObject As Object = Nothing,
                   Optional objCallerObj As Object = Nothing)

        viewer_instance.SetController(Me)

        _viewer = viewer_instance
        _obj_item = objItemObject
        _dict_original = dictOptions
        If _viewer.Hide_CCO.Checked = True Then
            _dict_options = FilterCCO(dictOptions)
        Else
            _dict_options = dictOptions
        End If

        _selections = selections


        If Not _obj_item Is Nothing Then
            If TypeOf _obj_item Is MailItem Then
                _ol_mail = _obj_item
                _ismail = True
            Else
                _ismail = False
                _ol_mail = Nothing
            End If
        End If

        _obj_caller = objCallerObj
        _prefix = tag_prefix
        If _prefix = "Tag PPL " And _ismail Then
            _viewer.button_autoassign.Visible = True
            _viewer.button_autoassign.Enabled = True
        Else
            _viewer.button_autoassign.Visible = False
            _viewer.button_autoassign.Enabled = False
        End If

        Dim _add_prefix As Boolean = False

        If _selections.Count > 0 Then
            If Len(_prefix) > 0 Then
                If Len(_selections(0)) > Len(_prefix) Then
                    If Left(_selections(0), Len(_prefix)) <> _prefix Then
                        _add_prefix = True
                    End If
                Else
                    _add_prefix = True
                End If
            End If

            For Each rawchoice As String In _selections
                Dim choice As String = rawchoice
                If _add_prefix Then choice = String.Concat(_prefix, choice)
                If _dict_options.Keys.Contains(choice) Then
                    _dict_options(choice) = Not _dict_options(choice)
                Else
                    Dim tmp_response As MsgBoxResult = MsgBox(choice & " does not exist. Would you like to add it?", vbYesNo)
                    If tmp_response = vbYes Then
                        If AddColorCategory(_prefix, rawchoice) Then
                            _dict_options.Add(choice, True)
                        End If
                    End If
                End If
            Next
        End If

        LoadControls(_dict_options, _prefix)
    End Sub

    Public Sub ToggleChoice(str_choice As String)
        _dict_options(str_choice) = Not _dict_options(str_choice)
    End Sub

    Public Sub ToggleOn(str_choice As String)
        _dict_options(str_choice) = 1
    End Sub

    Public Sub ToggleOff(str_choice As String)
        _dict_options(str_choice) = 0
    End Sub

    Public Sub UpdateSelections()
        ' Need to test function. Might not work
        _selections = _dict_options.Where(Function(x) x.Value = 1).Select(Function(x) x.Key)
        _filtered_selections = _filtered_options.Where(Function(x) x.Value = 1).Select(Function(x) x.Key)
    End Sub

    Public Sub SearchAndReload()
        RemoveControls()

        Dim filtered_options = _dict_options.Where(
            Function(x) x.Key.IndexOf(
            _viewer.TextBox1.Text,
            StringComparison.OrdinalIgnoreCase) >= 0).ToSortedDictionary

        LoadControls(filtered_options, _prefix)
    End Sub

    Public Function SelectionString() As String
        Dim Tmp = _dict_options.Where(Function(item) item.Value) _
                               .[Select](Function(item) item.Key) _
                               .ToList()
        Return String.Join(", ", Tmp)
    End Function

#End Region

#Region "Public Mouse Events"
    Public Sub Cancel_Action()
        _viewer.Hide()
        _exit_type = "Cancel"
        _viewer.Dispose()
    End Sub

    Public Sub OK_Action()
        _viewer.Hide()
        _exit_type = "Normal"
        _viewer.Dispose()
    End Sub

    Public Sub AutoAssign()
        Dim col_people As Collection = AutoFile.AutoFindPeople(_obj_item, blExcludeFlagged:=False)
        For Each person As String In col_people
            If _dict_options.ContainsKey(person) Then
                ToggleOn(person)
            Else
                AddOption(person, blClickTrue:=True)
            End If
        Next
        If col_people.Count > 0 Then FilterToSelected()
    End Sub

    Public Function FilterCCO(
            source_dict As SortedDictionary(Of String, Boolean)) _
            As SortedDictionary(Of String, Boolean)

        If Globals.ThisAddIn.CCOCatList Is Nothing Then
            Flag_Fields_Categories.CCOCatList_Load()
        End If
        Dim exclude As List(Of String) = Globals.ThisAddIn.CCOCatList
        'Dim filtered_dict = (From x In source_dict
        '                     Where Not exclude.Contains(x.Key)
        '                     Select x).ToSortedDictionary()
        Dim filtered_dict = (From x In source_dict
                             Where exclude.IndexOf(x.Key,
                                 StringComparison.OrdinalIgnoreCase) < 0
                             Select x).ToSortedDictionary()



        Return filtered_dict
    End Function

    Public Sub ToggleCCO()
        If _viewer.Hide_CCO.Checked = True Then
            _dict_options = FilterCCO(_dict_options)
        Else
            _dict_options = _dict_original
        End If
        SearchAndReload()
    End Sub

    Public Sub New_Action()
        MsgBox("Need to Implement New Action")
    End Sub

    Public Sub FocusCheckbox(ctrl As Windows.Forms.Control)
        int_focus = _col_cbx_ctrl.IndexOf(ctrl)
        Select_Ctrl_By_Number(0)
    End Sub
#End Region

#Region "Public Keyboard Events"
    Public Sub OptionsPanel_PreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs)
        Select Case e.KeyCode
            Case Keys.Down
                e.IsInputKey = True
            Case Keys.Up
                e.IsInputKey = True
        End Select
    End Sub

    Public Sub OptionsPanel_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Down
                Select_Ctrl_By_Number(1)
            Case Keys.Up
                Select_Ctrl_By_Number(-1)
        End Select
    End Sub

    Public Sub TagViewer_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Enter
                OK_Action()
        End Select
    End Sub

    Public Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Right
                _cursor_position = _viewer.TextBox1.SelectionStart
        End Select
    End Sub

    Public Sub TextBox1_KeyUp(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Right
                If _viewer.TextBox1.SelectionStart = _cursor_position Then
                    FilterToSelected()
                End If
            Case Keys.Down
                Select_Ctrl_By_Number(1)
            Case Keys.Enter
                OK_Action()
        End Select
    End Sub

    Public Sub Select_Ctrl_By_Number(increment As Integer)
        Dim newpos As Integer = int_focus + increment
        If newpos = -1 Then
            _viewer.TextBox1.Select()
            int_focus = newpos
        ElseIf newpos <= (_col_cbx_ctrl.Count - 1) Then
            _col_cbx_ctrl.Item(newpos).Focus()
            Dim cbx As Windows.Forms.CheckBox = _col_cbx_ctrl.Item(newpos)
            ControlPaint.DrawFocusRectangle(Drawing.Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle)
            int_focus = newpos
        End If
    End Sub
#End Region

#Region "Private Helper Functions"

    Private Function LoadControls(dict_options As SortedDictionary(Of String, Boolean),
                                 prefix As String) As Boolean
        Dim ctrlCB As Windows.Forms.CheckBox
        Dim strChkName As String
        Dim clsCheckBox As cCheckBoxClass

        Const cHt_var = 18
        Const cHt_fxd = 6
        Const cLt = 6
        Const cWt = 300

        _filtered_options = dict_options
        int_focus = 0

        For i = 0 To _filtered_options.Count - 1
            strChkName = Format(i, "00") & " ChkBx"
            ctrlCB = New System.Windows.Forms.CheckBox
            Try
                _viewer.OptionsPanel.Controls.Add(ctrlCB)
            Catch
                MsgBox("Error adding checkbox in Tags.LoadControls")
                Return False
                Exit Function
            End Try

            Dim strTemp As String = Right(_filtered_options.Keys(i),
                                          Len(_filtered_options.Keys(i)) - Len(prefix))

            ctrlCB.Text = strTemp
            ctrlCB.Checked = _filtered_options.Values(i)

            Try
                clsCheckBox = New cCheckBoxClass
                clsCheckBox.Init(Me, prefix)
                clsCheckBox.ctrlCB = ctrlCB
            Catch
                MsgBox("Error wiring checkbox event in Tags.LoadControls")
                Return False
                Exit Function
            End Try

            'ctrlCB.AutoSize = True
            ctrlCB.Height = cHt_var
            ctrlCB.Top = (cHt_var * i) + cHt_fxd
            ctrlCB.Left = cLt
            ctrlCB.Width = cWt

            '_viewer.OptionsPanel.ScrollHeight = ctrlCB.Top + cHt_var
            Try
                _col_cbx_ctrl.Add(ctrlCB)
                _col_cbx_event.Add(clsCheckBox)
            Catch
                MsgBox("Error saving checkbox control and event to collection")
                Return False
                Exit Function
            End Try
        Next
        Return True
    End Function

    Private Sub RemoveControls()
        Dim max As Integer = _col_cbx_ctrl.Count - 1
        For i = max To 0 Step -1
            _viewer.OptionsPanel.Controls.Remove(_col_cbx_ctrl.Item(i))
            _col_cbx_ctrl.Remove(i)
            _col_cbx_event.Remove(i)
        Next i

        max = _col_colorbox.Count - 1
        For i = max To 0 Step -1
            _viewer.OptionsPanel.Controls.Remove(_col_colorbox.Item(i))
            _col_colorbox.Remove(i)
        Next i
    End Sub

    Private Sub AddOption(strOption As String, Optional blClickTrue As Boolean = False)
        _dict_options.Add(strOption, blClickTrue)
        _filtered_options.Add(strOption, blClickTrue)
    End Sub

    Private Sub FilterToSelected()
        RemoveControls()
        '_filtered_options = _dict_options.Where(Function(x) x.Value = True).Select(Function(x) x)
        Dim tmp = (From x In _dict_options
                   Where x.Value
                   Select x).ToDictionary(
                   Function(x) x.Key,
                   Function(x) x.Value)
        _filtered_options = New SortedDictionary(Of String, Boolean)(tmp)
        LoadControls(_filtered_options, _prefix)
    End Sub

    Private Function GetSelections() As List(Of String)
        Return From x In _dict_options Where x.Value = True Select x.Key
    End Function

    Private Function AddColorCategory(prefix As String, rawchoice As String) As Boolean
        Dim choice As String = String.Concat(prefix, rawchoice)

        Dim olcolor As OlCategoryColor = OlCategoryColor.olCategoryColorNone
        If prefix = "Tag PPL " Then
            olcolor = OlCategoryColor.olCategoryColorDarkGray
        ElseIf prefix = "Tag PROJECT " Then
            olcolor = OlCategoryColor.olCategoryColorTeal
        ElseIf prefix = "Tag TOPIC " Then
            olcolor = OlCategoryColor.olCategoryColorDarkTeal
        End If

        Try
            Globals.ThisAddIn._OlNS.Categories.Add(
                choice,
                olcolor,
                OlCategoryShortcutKey.olCategoryShortcutKeyNone)
            Return True
        Catch
            MsgBox("Error Adding Category: """ & choice & """. Please ensure the category name is unique.")
            Return False
        End Try

    End Function

#End Region



End Class
