using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesVB;
using UtilitiesCS;


namespace Tags
{

    public class TagController
    {

        private readonly TagViewer _viewer;
        private readonly SortedDictionary<string, bool> _dict_original;
        private SortedDictionary<string, bool> _dict_options;
        private SortedDictionary<string, bool> _filtered_options;
        private List<string> _selections;
        private List<string> _filtered_selections;
        private readonly object _obj_item;
        private readonly MailItem _olMail;
        private readonly object _obj_caller;
        private readonly IPrefix _prefix;
        private readonly List<IPrefix> _prefixes;
        private List<CheckBox> _col_cbx_ctrl = new List<CheckBox>();
        private List<object> _col_cbx_event = new List<object>();
        private readonly List<object> _col_colorbox = new List<object>();
        private readonly bool _isMail;
        public string _exit_type = "Cancel";
        private int _cursor_position;
        private string _userEmailAddress;
        internal int int_focus;
        private readonly IAutoAssign _autoAssigner;



        #region Public Functions
        public TagController(TagViewer viewer_instance,
                             SortedDictionary<string, bool> dictOptions,
                             IAutoAssign autoAssigner,
                             List<IPrefix> prefixes,
                             string userEmailAddress,
                             List<string> selections = null,
                             string prefix_key = "",
                             object objItemObject = null,
                             object objCallerObj = null)
        {

            viewer_instance.SetController(this);
            _autoAssigner = autoAssigner;
            _prefixes = prefixes;

            _viewer = viewer_instance;
            _obj_item = objItemObject;
            _dict_original = dictOptions;
            _dict_options = _viewer.Hide_Archive.Checked == true ? FilterArchive(dictOptions) : dictOptions;
            _userEmailAddress = userEmailAddress;
            _selections = selections;

            if (_obj_item is not null)
            {
                if (_obj_item is MailItem)
                {
                    _olMail = (MailItem)_obj_item;
                    _isMail = true;
                }
                else
                {
                    _isMail = false;
                    _olMail = null;
                }
            }

            _obj_caller = objCallerObj;
            if (string.IsNullOrEmpty(prefix_key))
            {
                _prefix = new PrefixItem("", "", OlCategoryColor.olCategoryColorNone);
            }

            else if (prefixes.Exists(x => (x.Key ?? "") == (prefix_key ?? "")))
            {
                _prefix = prefixes.Find(x => (x.Key ?? "") == (prefix_key ?? ""));
            }

            else
            {
                throw new ArgumentException(nameof(prefixes) + " must contain " + nameof(prefix_key) + " value " + prefix_key);

            }

            if (autoAssigner is not null & _isMail)
            {
                _viewer.button_autoassign.Visible = true;
                _viewer.button_autoassign.Enabled = true;
            }
            else
            {
                _viewer.button_autoassign.Visible = false;
                _viewer.button_autoassign.Enabled = false;
            }

            bool _addPrefix = false;

            if (selections is not null)
            {
                if (_selections.Count > 0)
                {
                    int prefixLength = _prefix.Value.Length;
                    if (prefixLength > 0)
                    {
                        if ((_selections[0]!=null) && (_selections[0].Length > prefixLength))
                        {
                            if (_selections[0].Substring(0, prefixLength -1) != _prefix.Value )
                            {
                                _addPrefix = true;
                            }
                        }
                        else
                        {
                            _addPrefix = true;
                        }
                    }

                    foreach (string rawchoice in _selections)
                    {
                        string choice = rawchoice;
                        if (_addPrefix)
                            choice = string.Concat(_prefix.Value, choice);
                        if (_dict_options.Keys.Contains(choice))
                        {
                            _dict_options[choice] = !_dict_options[choice];
                        }
                        else
                        {
                            var tmp_response = MessageBox.Show($"{choice} does not exist. Would you like to add it?", "Dialog", MessageBoxButtons.YesNo);
                            if (tmp_response == DialogResult.Yes)
                            {
                                AddColorCategory(rawchoice);
                            }
                        }
                    }
                }
            }

            LoadControls(_dict_options, _prefix.Value);
        }

        public void ToggleChoice(string str_choice)
        {
            _dict_options[str_choice] = !_dict_options[str_choice];
        }

        internal void ToggleOn(string str_choice)
        {
            _dict_options[str_choice] = true;
        }

        internal void ToggleOff(string str_choice)
        {
            _dict_options[str_choice] = false;
        }

        public void UpdateSelections()
        {
            _selections = _dict_options.Where(x => x.Value).Select(x => x.Key).ToList();
            _filtered_selections = _filtered_options.Where(x => x.Value).Select(x => x.Key).ToList();
        }

        internal void SearchAndReload()
        {
            RemoveControls();

            var filtered_options = _dict_options.Where(x => x.Key.IndexOf(_viewer.TextBox1.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToSortedDictionary();

            bool unused = LoadControls(filtered_options, _prefix.Value);
        }

        public string SelectionString()
        {
            var Tmp = _dict_options.Where(item => item.Value).Select(item => item.Key).ToList();

            return string.Join(", ", Tmp);
        }

        public bool ButtonNewActive
        {
            get
            {
                return _viewer.button_new.Visible;
            }
            set
            {
                _viewer.button_new.Visible = value;
            }
        }

        public bool ButtonAutoAssignActive
        {
            get
            {
                return _viewer.button_autoassign.Visible;
            }
            set
            {
                _viewer.button_autoassign.Visible = value;
            }
        }

        public void SetSearchText(string searchText)
        {
            _viewer.TextBox1.Text = searchText;
        }
        #endregion

        #region Public Mouse Events
        internal void Cancel_Action()
        {
            _viewer.Hide();
            _exit_type = "Cancel";
            _viewer.Dispose();
        }

        internal void OK_Action()
        {
            _viewer.Hide();
            _exit_type = "Normal";
            _viewer.Dispose();
        }

        internal void AutoAssign()
        {
            var col_choices = _autoAssigner.AutoFind(_obj_item);
            foreach (string str_choice in col_choices)
            {
                if (_dict_options.ContainsKey(str_choice))
                {
                    ToggleOn(str_choice);
                }
                else
                {
                    AddOption(str_choice, blClickTrue: true);
                }
            }
            if (col_choices.Count > 0)
                FilterToSelected();
        }

        internal SortedDictionary<string, bool> FilterArchive(SortedDictionary<string, bool> source_dict)
        {

            if (_autoAssigner is not null)
            {
                var exclude = _autoAssigner.FilterList;
                // Dim filtered_dict = (From x In source_dict
                // Where Not exclude.Contains(x.Key)
                // Select x).ToSortedDictionary()
                var filtered_dict = (from x in source_dict
                                     where exclude.IndexOf(x.Key, (int)StringComparison.OrdinalIgnoreCase) < 0
                                     select x).ToSortedDictionary();
                return filtered_dict;
            }
            else
            {
                return source_dict;
            }

        }

        internal void ToggleArchive()
        {
            _dict_options = _viewer.Hide_Archive.Checked == true ? FilterArchive(_dict_options) : _dict_original;
            SearchAndReload();
        }

        internal void AddColorCategory(string categoryName = "")
        {
            bool autoAdded = false;
            IList<string> colCatName = new List<string>();

            // Check to see if can be automatically created
            if (_autoAssigner is not null & _isMail)
            {
                // Ask user if they want to auto-add
                var vbR = MessageBox.Show("Auto-add new from email details?", "Dialog", MessageBoxButtons.YesNo);

                if (vbR == DialogResult.Yes)
                {
                    colCatName = _autoAssigner.AddChoicesToDict(_olMail, _prefixes, _prefix.Key, _userEmailAddress);
                    // Dim colChoices As Collection = AutoFile.dictPPL_AddMissingEntries(_olMail)
                    foreach (string newCatName in colCatName)
                    {
                        AddOption(newCatName, blClickTrue: true);
                        autoAdded = true;
                    }
                }
            }

            if (!autoAdded)
            {
                if (!string.IsNullOrEmpty(categoryName))
                {
                    categoryName = InputBox.ShowDialog("The following category name will be added:","Add Category Dialog", DefaultResponse: categoryName);
                }
                else
                {
                    bool advance = false;
                    string msg = "Enter new category name:";
                    while (!advance)
                    {
                        categoryName = InputBox.ShowDialog(msg, "Add Category Dialog", DefaultResponse: " ");
                        if (categoryName != " ")
                            advance = true;
                        msg = "Please enter a name or hit cancel:";
                    }
                }
                if (!string.IsNullOrEmpty(categoryName))
                {
                    var newCategory = _autoAssigner.AddColorCategory(_prefix, categoryName);
                    if (newCategory is not null)
                    {
                        AddOption(newCategory.Name, blClickTrue: true);
                        colCatName.Add(newCategory.Name);
                    }
                }
            }

            if (colCatName.Count > 0)
                FilterToSelected();
        }

        internal void FocusCheckbox(CheckBox cbx)
        {
            int_focus = _col_cbx_ctrl.IndexOf(cbx);
            Select_Ctrl_By_Offset(0);
        }
        #endregion

        #region Public Keyboard Events
        internal void OptionsPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        e.IsInputKey = true;
                        break;
                    }
                case Keys.Up:
                    {
                        e.IsInputKey = true;
                        break;
                    }
            }
        }

        internal void OptionsPanel_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        Select_Ctrl_By_Offset(1);
                        break;
                    }
                case Keys.Up:
                    {
                        Select_Ctrl_By_Offset(-1);
                        break;
                    }
            }
        }

        internal void TagViewer_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        OK_Action();
                        break;
                    }
            }
        }

        internal void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    {
                        _cursor_position = _viewer.TextBox1.SelectionStart;
                        break;
                    }
                case Keys.Down:
                    {
                        Select_Ctrl_By_Offset(1);
                        break;
                    }
            }
        }

        internal void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    {
                        if (_viewer.TextBox1.SelectionStart == _cursor_position)
                        {
                            FilterToSelected();
                        }

                        break;
                    }
                case Keys.Enter:
                    {
                        OK_Action();
                        break;
                    }
            }
        }

        internal void Select_Ctrl_By_Offset(int increment)
        {
            int newpos = int_focus + increment;
            if (newpos == -1)
            {
                _viewer.TextBox1.Select();
                int_focus = newpos;
            }
            else if (newpos <= _col_cbx_ctrl.Count - 1)
            {
                _col_cbx_ctrl[newpos].Focus();
                CheckBox cbx = (CheckBox)_col_cbx_ctrl[newpos];
                ControlPaint.DrawFocusRectangle(System.Drawing.Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle);
                int_focus = newpos;
            }
        }

        internal void Select_Last_Control()
        {
            Select_Ctrl_By_Position(_col_cbx_ctrl.Count - 1);
        }

        internal void Select_First_Control()
        {
            Select_Ctrl_By_Position(0);
        }

        internal void Select_PageDown()
        {

            if (_viewer.OptionsPanel.VerticalScroll.Maximum > _viewer.OptionsPanel.Height)
            {
                int start = Math.Max(int_focus, 0);
                int y = _viewer.OptionsPanel.Height;
                var filteredIEnumerable = _col_cbx_ctrl.Select((n, i) => 
                                                       new { Value = n, Index = i })
                                                       .Where(p => 
                                                       (p.Index > int_focus) & 
                                                       (p.Value.Bottom > y));

                if (filteredIEnumerable.Count() == 0)
                {
                    Select_Last_Control();
                }

                else
                {
                    int idx = filteredIEnumerable.First().Index;

                    Select_Ctrl_By_Position(idx);

                    int y_scroll = _col_cbx_ctrl[idx].Top - _viewer.OptionsPanel.AutoScrollPosition.Y; 

                    _viewer.OptionsPanel.AutoScrollPosition = new System.Drawing.Point(
                        _viewer.OptionsPanel.AutoScrollPosition.X, y_scroll);

                }

            }
        }

        internal void Select_PageUp()
        {

            if (_viewer.OptionsPanel.VerticalScroll.Maximum > _viewer.OptionsPanel.Height)
            {
                int start = Math.Max(int_focus, 0);
                int idx_top;

                var filteredIEnumerable = _col_cbx_ctrl.Select((n, i) => new { Value = n, Index = i })
                                                       .Where(p => p.Value.Top < 0);

                if (filteredIEnumerable.Count() == 0)
                {
                    Select_First_Control();
                }

                else
                {
                    idx_top = filteredIEnumerable.Last().Index;
                    Select_Ctrl_By_Position(idx_top);
                    int y_scroll = (-1 * _viewer.OptionsPanel.AutoScrollPosition.Y) 
                        - (_viewer.OptionsPanel.Height - _col_cbx_ctrl[idx_top].Height);

                    _viewer.OptionsPanel.AutoScrollPosition = new System.Drawing.Point(
                        _viewer.OptionsPanel.AutoScrollPosition.X, y_scroll);

                }

            }
        }

        internal void Select_Ctrl_By_Position(int position)
        {
            if (position < -1 | position > _col_cbx_ctrl.Count - 1)
            {
                throw new ArgumentOutOfRangeException("Cannot select control with postition " + position);
            }

            else if (position == -1)
            {
                _viewer.TextBox1.Select();
                int_focus = position;
            }

            else
            {
                _col_cbx_ctrl[position].Focus();
                CheckBox cbx = (CheckBox)_col_cbx_ctrl[position];
                ControlPaint.DrawFocusRectangle(System.Drawing.Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle);
                int_focus = position;
            }
        }



        #endregion

        #region Private Helper Functions

        private bool LoadControls(SortedDictionary<string, bool> dict_options, string prefix)
        {
            CheckBox ctrlCB;
            string strChkName;
            CheckBoxController clsCheckBox;

            const int cHt_var = 18;
            const int cHt_fxd = 6;
            const int cLt = 6;
            const int cWt = 300;

            _filtered_options = dict_options;
            int_focus = 0;
            _col_cbx_ctrl = new List<CheckBox>();
            _col_cbx_event = new List<object>();

            for (int i = 0, loopTo = _filtered_options.Count - 1; i <= loopTo; i++)
            {
                strChkName = i.ToString("00") + " ChkBx";
                ctrlCB = new CheckBox();
                try
                {
                    _viewer.OptionsPanel.Controls.Add(ctrlCB);
                }
                catch
                {
                    MessageBox.Show("Error adding checkbox in Tags.LoadControls");
                    return false;
                }
                
                ctrlCB.Text = _filtered_options.Keys.ElementAt(i).Substring(prefix.Length);
                ctrlCB.Checked = _filtered_options.Values.ElementAt(i);

                try
                {
                    clsCheckBox = new CheckBoxController();
                    clsCheckBox.Init(this, prefix);
                    clsCheckBox.ctrlCB = ctrlCB;
                }
                catch
                {
                    MessageBox.Show("Error wiring checkbox event in Tags.LoadControls");
                    return false;
                }

                // ctrlCB.AutoSize = True
                ctrlCB.Height = cHt_var;
                ctrlCB.Top = cHt_var * i + cHt_fxd;
                ctrlCB.Left = cLt;
                ctrlCB.Width = cWt;

                // _viewer.OptionsPanel.ScrollHeight = ctrlCB.Top + cHt_var
                try
                {
                    _col_cbx_ctrl.Add(ctrlCB);
                    _col_cbx_event.Add(clsCheckBox);
                }
                catch
                {
                    MessageBox.Show("Error saving checkbox control and event to collection");
                    return false;
                }
            }
            return true;
        }

        private void RemoveControls()
        {
            int max = _col_cbx_ctrl.Count - 1;
            for (int i = max; i >= 0; i -= 1)
            {
                _viewer.OptionsPanel.Controls.Remove((Control)_col_cbx_ctrl[i]);
                _col_cbx_ctrl.RemoveAt(i);
                _col_cbx_event.RemoveAt(i);
            }

            max = _col_colorbox.Count - 1;
            for (int i = max; i >= 0; i -= 1)
            {
                _viewer.OptionsPanel.Controls.Remove((Control)_col_colorbox[i]);
                bool unused = _col_colorbox.Remove(i);
            }
        }

        private void AddOption(string strOption, bool blClickTrue = false)
        {
            _dict_options.Add(strOption, blClickTrue);
            _filtered_options.Add(strOption, blClickTrue);
        }

        private void FilterToSelected()
        {
            RemoveControls();
            // _filtered_options = _dict_options.Where(Function(x) x.Value = True).Select(Function(x) x)
            var tmp = (from x in _dict_options
                       where x.Value
                       select x).ToDictionary(x => x.Key, x => x.Value);
            _filtered_options = new SortedDictionary<string, bool>(tmp);
            bool unused = LoadControls(_filtered_options, _prefix.Value);
        }

        public List<string> GetSelections()
        {
            return (from x in _dict_options
                    where x.Value == true
                    select x.Key).ToList();
        }

        private class PrefixItem : IPrefix
        {

            public PrefixItem(string key, string value, OlCategoryColor color)
            {
                Key = key;
                Value = value;
                Color = color;
            }

            public string Key { get; set; }

            public string Value { get; set; }

            public OlCategoryColor Color { get; set; }
        }

        #endregion


    }
}