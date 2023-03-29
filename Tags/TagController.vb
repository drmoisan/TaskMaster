Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Outlook
Imports UtilitiesVB

Public Class TagController

    Private ReadOnly _viewer As TagViewer
    Private ReadOnly _dict_original As SortedDictionary(Of String, Boolean)
    Private _dict_options As SortedDictionary(Of String, Boolean)
    Private _filtered_options As SortedDictionary(Of String, Boolean)
    Private _selections As List(Of String)
    Private _filtered_selections As List(Of String)
    Private ReadOnly _obj_item As Object
    Private ReadOnly _olMail As MailItem
    Private ReadOnly _obj_caller As Object
    Private ReadOnly _prefix As IPrefix
    Private ReadOnly _prefixes As List(Of IPrefix)
    Private _col_cbx_ctrl As New List(Of Object)
    Private _col_cbx_event As New List(Of Object)
    Private ReadOnly _col_colorbox As New List(Of Object)
    Private ReadOnly _isMail As Boolean
    Public _exit_type As String = "Cancel"
    Private _cursor_position As Integer
    Friend int_focus As Integer
    Private ReadOnly _autoAssigner As IAutoAssign



#Region "Public Functions"
    Public Sub New(viewer_instance As TagViewer,
                   dictOptions As SortedDictionary(Of String, Boolean),
                   autoAssigner As IAutoAssign,
                   prefixes As List(Of IPrefix),
                   Optional selections As List(Of String) = Nothing,
                   Optional prefix_key As String = "",
                   Optional objItemObject As Object = Nothing,
                   Optional objCallerObj As Object = Nothing)

        viewer_instance.SetController(Me)
        _autoAssigner = autoAssigner
        _prefixes = prefixes

        _viewer = viewer_instance
        _obj_item = objItemObject
        _dict_original = dictOptions
        _dict_options = If(_viewer.Hide_Archive.Checked = True, FilterArchive(dictOptions), dictOptions)

        _selections = selections

        If _obj_item IsNot Nothing Then
            If TypeOf _obj_item Is MailItem Then
                _olMail = _obj_item
                _isMail = True
            Else
                _isMail = False
                _olMail = Nothing
            End If
        End If

        _obj_caller = objCallerObj
        If prefix_key = "" Then
            _prefix = New PrefixItem("", "", OlCategoryColor.olCategoryColorNone)

        ElseIf prefixes.Exists(Function(x) x.Key = prefix_key) Then
            _prefix = prefixes.Find(Function(x) x.Key = prefix_key)

        Else
            Throw New ArgumentException(NameOf(prefixes) & " must contain " &
                                        NameOf(prefix_key) & " value " & prefix_key)

        End If

        If (autoAssigner IsNot Nothing) And _isMail Then
            _viewer.button_autoassign.Visible = True
            _viewer.button_autoassign.Enabled = True
        Else
            _viewer.button_autoassign.Visible = False
            _viewer.button_autoassign.Enabled = False
        End If

        Dim _addPrefix As Boolean = False

        If selections IsNot Nothing Then
            If _selections.Count > 0 Then
                If Len(_prefix.Value) > 0 Then
                    If Len(_selections(0)) > Len(_prefix.Value) Then
                        If Left(_selections(0), Len(_prefix.Value)) <> _prefix.Value Then
                            _addPrefix = True
                        End If
                    Else
                        _addPrefix = True
                    End If
                End If

                For Each rawchoice As String In _selections
                    Dim choice As String = rawchoice
                    If _addPrefix Then choice = String.Concat(_prefix.Value, choice)
                    If _dict_options.Keys.Contains(choice) Then
                        _dict_options(choice) = Not _dict_options(choice)
                    Else
                        Dim tmp_response As MsgBoxResult = MsgBox(choice & " does not exist. Would you like to add it?", vbYesNo)
                        If tmp_response = vbYes Then
                            AddColorCategory(rawchoice)
                        End If
                    End If
                Next
            End If
        End If

        LoadControls(_dict_options, _prefix.Value)
    End Sub

    Public Sub ToggleChoice(str_choice As String)
        _dict_options(str_choice) = Not _dict_options(str_choice)
    End Sub

    Friend Sub ToggleOn(str_choice As String)
        _dict_options(str_choice) = 1
    End Sub

    Friend Sub ToggleOff(str_choice As String)
        _dict_options(str_choice) = 0
    End Sub

    Public Sub UpdateSelections()
        ' Need to test function. Might not work
        _selections = _dict_options.Where(Function(x) x.Value = 1).Select(Function(x) x.Key)
        _filtered_selections = _filtered_options.Where(Function(x) x.Value = 1).Select(Function(x) x.Key)
    End Sub

    Friend Sub SearchAndReload()
        RemoveControls()

        Dim filtered_options = _dict_options.Where(
            Function(x) x.Key.IndexOf(
            _viewer.TextBox1.Text,
            StringComparison.OrdinalIgnoreCase) >= 0).ToSortedDictionary

        Dim unused = LoadControls(filtered_options, _prefix.Value)
    End Sub

    Public Function SelectionString() As String
        Dim Tmp = _dict_options.Where(Function(item) item.Value) _
                               .[Select](Function(item) item.Key) _
                               .ToList()
        Return String.Join(", ", Tmp)
    End Function

    Public Property ButtonNewActive() As Boolean
        Get
            Return _viewer.button_new.Visible
        End Get
        Set(value As Boolean)
            _viewer.button_new.Visible = value
        End Set
    End Property

    Public Property ButtonAutoAssignActive() As Boolean
        Get
            Return _viewer.button_autoassign.Visible
        End Get
        Set(value As Boolean)
            _viewer.button_autoassign.Visible = value
        End Set
    End Property

    Public Sub SetSearchText(searchText As String)
        _viewer.TextBox1.Text = searchText
    End Sub
#End Region

#Region "Public Mouse Events"
    Friend Sub Cancel_Action()
        _viewer.Hide()
        _exit_type = "Cancel"
        _viewer.Dispose()
    End Sub

    Friend Sub OK_Action()
        _viewer.Hide()
        _exit_type = "Normal"
        _viewer.Dispose()
    End Sub

    Friend Sub AutoAssign()
        Dim col_choices As Collection = _autoAssigner.AutoFind(_obj_item)
        For Each str_choice As String In col_choices
            If _dict_options.ContainsKey(str_choice) Then
                ToggleOn(str_choice)
            Else
                AddOption(str_choice, blClickTrue:=True)
            End If
        Next
        If col_choices.Count > 0 Then FilterToSelected()
    End Sub

    Friend Function FilterArchive(
            source_dict As SortedDictionary(Of String, Boolean)) _
            As SortedDictionary(Of String, Boolean)

        If _autoAssigner IsNot Nothing Then
            Dim exclude As List(Of String) = _autoAssigner.FilterList
            'Dim filtered_dict = (From x In source_dict
            '                     Where Not exclude.Contains(x.Key)
            '                     Select x).ToSortedDictionary()
            Dim filtered_dict = (From x In source_dict
                                 Where exclude.IndexOf(x.Key,
                                 StringComparison.OrdinalIgnoreCase) < 0
                                 Select x).ToSortedDictionary()
            Return filtered_dict
        Else
            Return source_dict
        End If

    End Function

    Friend Sub ToggleArchive()
        _dict_options = If(_viewer.Hide_Archive.Checked = True, FilterArchive(_dict_options), _dict_original)
        SearchAndReload()
    End Sub

    Friend Sub AddColorCategory(Optional categoryName As String = "")
        Dim autoAdded As Boolean = False
        Dim colCatName As New Collection()

        'Check to see if can be automatically created
        If (_autoAssigner IsNot Nothing) And _isMail Then
            'Ask user if they want to auto-add
            Dim vbR As MsgBoxResult = MsgBox("Auto-add new from email details?", vbYesNo)

            If vbR = vbYes Then
                colCatName = _autoAssigner.AddChoicesToDict(_olMail, _prefixes, _prefix.Key)
                'Dim colChoices As Collection = AutoFile.dictPPL_AddMissingEntries(_olMail)
                For Each newCatName As String In colCatName
                    AddOption(newCatName, blClickTrue:=True)
                    autoAdded = True
                Next newCatName
            End If
        End If

        If Not autoAdded Then
            If categoryName <> "" Then
                categoryName = InputBox("The following category name will be added:", DefaultResponse:=categoryName)
            Else
                Dim advance As Boolean = False
                Dim msg As String = "Enter new category name:"
                While Not advance
                    categoryName = InputBox(msg, DefaultResponse:=" ")
                    If categoryName <> " " Then advance = True
                    msg = "Please enter a name or hit cancel:"
                End While
            End If
            If categoryName <> "" Then
                Dim newCategory As Category = _autoAssigner.AddColorCategory(_prefix, categoryName)
                If newCategory IsNot Nothing Then
                    AddOption(newCategory.Name, blClickTrue:=True)
                    colCatName.Add(newCategory.Name)
                End If
            End If
        End If

        If colCatName.Count > 0 Then FilterToSelected()
    End Sub

    Friend Sub FocusCheckbox(ctrl As Windows.Forms.Control)
        int_focus = _col_cbx_ctrl.IndexOf(ctrl)
        Select_Ctrl_By_Offset(0)
    End Sub
#End Region

#Region "Public Keyboard Events"
    Friend Sub OptionsPanel_PreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs)
        Select Case e.KeyCode
            Case Keys.Down
                e.IsInputKey = True
            Case Keys.Up
                e.IsInputKey = True
        End Select
    End Sub

    Friend Sub OptionsPanel_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Down
                Select_Ctrl_By_Offset(1)
            Case Keys.Up
                Select_Ctrl_By_Offset(-1)
        End Select
    End Sub

    Friend Sub TagViewer_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Enter
                OK_Action()
        End Select
    End Sub

    Friend Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Right
                _cursor_position = _viewer.TextBox1.SelectionStart
            Case Keys.Down
                Select_Ctrl_By_Offset(1)
        End Select
    End Sub

    Friend Sub TextBox1_KeyUp(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Right
                If _viewer.TextBox1.SelectionStart = _cursor_position Then
                    FilterToSelected()
                End If
            Case Keys.Enter
                OK_Action()
        End Select
    End Sub

    Friend Sub Select_Ctrl_By_Offset(increment As Integer)
        Dim newpos As Integer = int_focus + increment
        If newpos = -1 Then
            _viewer.TextBox1.Select()
            int_focus = newpos
        ElseIf newpos <= (_col_cbx_ctrl.Count - 1) Then
            Dim unused = _col_cbx_ctrl.Item(newpos).Focus()
            Dim cbx As Windows.Forms.CheckBox = _col_cbx_ctrl.Item(newpos)
            ControlPaint.DrawFocusRectangle(Drawing.Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle)
            int_focus = newpos
        End If
    End Sub

    Friend Sub Select_Last_Control()
        Select_Ctrl_By_Position(_col_cbx_ctrl.Count - 1)
    End Sub

    Friend Sub Select_First_Control()
        Select_Ctrl_By_Position(0)
    End Sub

    Friend Sub Select_PageDown()

        If _viewer.OptionsPanel.VerticalScroll.Maximum > _viewer.OptionsPanel.Height Then
            Dim start As Integer = Math.Max(int_focus, 0)
            Dim y As Integer = _viewer.OptionsPanel.Height
            Dim filteredIEnumerable = _col_cbx_ctrl.[Select](Function(n, i) New With
                                                    {Key .Value = n, Key .Index = i}) _
                                                    .Where(Function(p) p.Index > int_focus And
                                                    p.Value.Bottom > y)

            If filteredIEnumerable.Count = 0 Then
                Select_Last_Control()

            Else
                Dim idx As Integer = filteredIEnumerable.First().Index

                Select_Ctrl_By_Position(idx)

                Dim y_scroll As Integer = _col_cbx_ctrl.Item(idx).Top _
                                          - _viewer.OptionsPanel.AutoScrollPosition.Y

                _viewer.OptionsPanel.AutoScrollPosition = New Drawing.Point(
                    _viewer.OptionsPanel.AutoScrollPosition.X, y_scroll)

            End If

        End If
    End Sub

    Friend Sub Select_PageUp()

        If _viewer.OptionsPanel.VerticalScroll.Maximum > _viewer.OptionsPanel.Height Then
            Dim start As Integer = Math.Max(int_focus, 0)
            Dim idx_top As Integer

            Dim filteredIEnumerable = _col_cbx_ctrl.[Select](Function(n, i) New With
                                                    {Key .Value = n, Key .Index = i}) _
                                                    .Where(Function(p) p.Value.Top < 0)

            If filteredIEnumerable.Count = 0 Then
                Select_First_Control()

            Else
                idx_top = filteredIEnumerable.Last().Index
                Select_Ctrl_By_Position(idx_top)
                Dim y_scroll As Integer = (-1 * _viewer.OptionsPanel.AutoScrollPosition.Y) _
                    - (_viewer.OptionsPanel.Height - _col_cbx_ctrl(idx_top).Height)

                _viewer.OptionsPanel.AutoScrollPosition = New Drawing.Point(
                    _viewer.OptionsPanel.AutoScrollPosition.X, y_scroll)

            End If

        End If
    End Sub

    Friend Sub Select_Ctrl_By_Position(position As Integer)
        If position < -1 Or position > _col_cbx_ctrl.Count - 1 Then
            Throw New ArgumentOutOfRangeException("Cannot select control with postition " & position)

        ElseIf position = -1 Then
            _viewer.TextBox1.Select()
            int_focus = position

        Else
            Dim unused = _col_cbx_ctrl.Item(position).Focus()
            Dim cbx As Windows.Forms.CheckBox = _col_cbx_ctrl.Item(position)
            ControlPaint.DrawFocusRectangle(Drawing.Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle)
            int_focus = position
        End If
    End Sub



#End Region

#Region "Private Helper Functions"

    Private Function LoadControls(dict_options As SortedDictionary(Of String, Boolean),
                                 prefix As String) As Boolean
        Dim ctrlCB As Windows.Forms.CheckBox
        Dim strChkName As String
        Dim clsCheckBox As CheckBoxController

        Const cHt_var = 18
        Const cHt_fxd = 6
        Const cLt = 6
        Const cWt = 300

        _filtered_options = dict_options
        int_focus = 0
        _col_cbx_ctrl = New List(Of Object)
        _col_cbx_event = New List(Of Object)

        For i = 0 To _filtered_options.Count - 1
            strChkName = Format(i, "00") & " ChkBx"
            ctrlCB = New System.Windows.Forms.CheckBox
            Try
                _viewer.OptionsPanel.Controls.Add(ctrlCB)
            Catch
                Dim unused3 = MsgBox("Error adding checkbox in Tags.LoadControls")
                Return False
                Exit Function
            End Try

            Dim strTemp As String = Right(_filtered_options.Keys(i),
                                          Len(_filtered_options.Keys(i)) - Len(prefix))

            ctrlCB.Text = strTemp
            ctrlCB.Checked = _filtered_options.Values(i)

            Try
                clsCheckBox = New CheckBoxController
                Dim unused2 = clsCheckBox.Init(Me, prefix)
                clsCheckBox.ctrlCB = ctrlCB
            Catch
                Dim unused1 = MsgBox("Error wiring checkbox event in Tags.LoadControls")
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
                Dim unused = MsgBox("Error saving checkbox control and event to collection")
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
            Dim unused2 = _col_cbx_ctrl.Remove(i)
            Dim unused1 = _col_cbx_event.Remove(i)
        Next i

        max = _col_colorbox.Count - 1
        For i = max To 0 Step -1
            _viewer.OptionsPanel.Controls.Remove(_col_colorbox.Item(i))
            Dim unused = _col_colorbox.Remove(i)
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
        Dim unused = LoadControls(_filtered_options, _prefix.Value)
    End Sub

    Public Function GetSelections() As List(Of String)
        Return (From x In _dict_options Where x.Value = True Select x.Key).ToList()
    End Function

    Private Class PrefixItem
        Implements IPrefix

        Public Sub New(key As String, value As String, color As OlCategoryColor)
            Me.Key = key
            Me.Value = value
            Me.Color = color
        End Sub

        Public Property Key As String Implements IPrefix.Key

        Public Property Value As String Implements IPrefix.Value

        Public Property Color As OlCategoryColor Implements IPrefix.Color
    End Class

#End Region


End Class

