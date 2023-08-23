using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;


namespace Tags
{
    public class TagController
    {
        #region Contructors and Initializers

        public TagController(TagViewer viewer_instance,
                             SortedDictionary<string, bool> dictOptions,
                             IAutoAssign autoAssigner,
                             List<IPrefix> prefixes,
                             string userEmailAddress,
                             List<string> selections = null,
                             string prefixKey = "",
                             object objItemObject = null,
                             object objCallerObj = null)
        {

            viewer_instance.SetController(this);
            _autoAssigner = autoAssigner;
            _prefixes = prefixes;
            _viewer = viewer_instance;
            _objItem = objItemObject;
            _dictOriginal = dictOptions;
            _dictOptions = _viewer.HideArchive.Checked == true ? FilterArchive(dictOptions) : dictOptions;
            _userEmailAddress = userEmailAddress;
            _selections = selections;
            _objCaller = objCallerObj;
            
            _olMail = ResolveMailItem(_objItem);
            
            if (_olMail is not null) { _isMail = true; }

            _gridTemplate = CaptureAndRemoveTemplate();
            
            ResolvePrefix(prefixes, prefixKey);

            SetAutoAssignState(autoAssigner);

            LoadSelections(selections);

            LoadControls(_dictOptions, _prefix.Value);

            
        }

        public MailItem ResolveMailItem(object objItem) //internal
        {
            if ((objItem is not null) && (objItem is MailItem))
            {
                return (MailItem)_objItem;
            }
            else return null;
        } 

        public void ResolvePrefix(List<IPrefix> prefixes, string prefixKey) //internal
        {
            // Set default prefix if none exists
            if (string.IsNullOrEmpty(prefixKey))
            {
                _prefix = new PrefixItem("", "", OlCategoryColor.olCategoryColorNone);
            }
            // Else if it exists, set the Iprefix based on the prefixKey
            else if (prefixes.Exists(x => (x.Key ?? "") == (prefixKey ?? "")))
            {
                _prefix = prefixes.Find(x => (x.Key ?? "") == (prefixKey ?? ""));
            }
            // Else throw an error
            else
            {
                throw new ArgumentException(nameof(prefixes) + " must contain " + nameof(prefixKey) + " value " + prefixKey);
            }
        } 

        public void SetAutoAssignState(IAutoAssign autoAssigner) //internal
        {
            // Determine if the autoAssign button should be visible and active
            if (autoAssigner is not null & _isMail)
            {
                _viewer.ButtonAutoassign.Visible = true;
                _viewer.ButtonAutoassign.Enabled = true;
            }
            else
            {
                _viewer.ButtonAutoassign.Visible = false;
                _viewer.ButtonAutoassign.Enabled = false;
            }
        } 

        public void LoadSelections(List<string> selections) //internal
        {
            if ((selections is not null) && (_selections.Count > 0))
            {
                var _addPrefix = IsPrefixMissing(_prefix, _selections[0]);

                foreach (string rawchoice in _selections)
                {
                    string choice = rawchoice;
                    if (_addPrefix)
                        choice = string.Concat(_prefix.Value, choice);
                    if (_dictOptions.Keys.Contains(choice))
                    {
                        _dictOptions[choice] = !_dictOptions[choice];
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

        public bool IsPrefixMissing(IPrefix prefix, string sample) //internal
        {
            bool addPrefix = false;
            int prefixLength = prefix.Value.Length;
            if (prefixLength > 0)
            {
                if ((sample != null) && (sample.Length > prefixLength))
                {
                    if (sample.Substring(0, prefixLength - 1) != prefix.Value)
                    {
                        addPrefix = true;
                    }
                }
                else
                {
                    addPrefix = true;
                }
            }

            return addPrefix;
        } 

        public ControlPosition CaptureAndRemoveTemplate() //internal
        {
            var cp = ControlPosition.CreateTemplate(_viewer.TemplateCheckBox);
            _viewer.L1v2L2_OptionsPanel.Controls.Remove(_viewer.TemplateCheckBox);
            return cp;
        }

        private readonly TagViewer _viewer;
        private readonly SortedDictionary<string, bool> _dictOriginal;
        private SortedDictionary<string, bool> _dictOptions;
        private SortedDictionary<string, bool> _filteredOptions;
        private List<string> _selections;
        private List<string> _filteredSelections;
        private object _objItem;
        private MailItem _olMail;
        private readonly object _objCaller;
        private IPrefix _prefix;
        private readonly List<IPrefix> _prefixes;
        private List<CheckBox> _colCbxCtrl = new List<CheckBox>();
        private List<CheckBoxController> _colCbxEvent = new();
        private readonly List<object> _colColorbox = new List<object>();
        private bool _isMail;
        private string _exitType = "Cancel";
        private int _cursorPosition;
        private string _userEmailAddress;
        internal int intFocus;
        private readonly IAutoAssign _autoAssigner;
        private ControlPosition _gridTemplate;

        #endregion


        #region Public Functions

        public void ToggleChoice(string str_choice) => _dictOptions[str_choice] = !_dictOptions[str_choice];
        
        public void ToggleOn(string str_choice) => _dictOptions[str_choice] = true; //internal
            
        public void ToggleOff(string str_choice) => _dictOptions[str_choice] = false; //internal

        public void UpdateSelections()
        {
            _selections = _dictOptions.Where(x => x.Value).Select(x => x.Key).ToList();
            _filteredSelections = _filteredOptions.Where(x => x.Value).Select(x => x.Key).ToList();
        }

        internal void SearchAndReload() //internal
        {
            RemoveControls();

            var filtered_options = _dictOptions.Where(x => x.Key.IndexOf(_viewer.SearchText.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToSortedDictionary();

            LoadControls(filtered_options, _prefix.Value);
        }

        public string SelectionString() => string.Join(", ", _dictOptions.Where(item => item.Value).Select(item => item.Key).ToList());
        
        public bool ButtonNewActive { get => _viewer.ButtonNew.Visible; set => _viewer.ButtonNew.Visible = value; }
        
        public bool ButtonAutoAssignActive {  get => _viewer.ButtonAutoassign.Visible; set => _viewer.ButtonAutoassign.Visible = value; }
        
        public void SetSearchText(string searchText) => _viewer.SearchText.Text = searchText;

        public string ExitType { get => _exitType; }

        #endregion

        #region Public Mouse Events

        public void Cancel_Action() //internal
        {
            _viewer.Hide();
            _exitType = "Cancel";
            _viewer.Dispose();
        }

        public void OK_Action() //internal
        {
            _viewer.Hide();
            _exitType = "Normal";
            _viewer.Dispose();
        }

        public void AutoAssign() //internal
        {
            var col_choices = _autoAssigner.AutoFind(_objItem);
            foreach (string str_choice in col_choices)
            {
                if (_dictOptions.ContainsKey(str_choice))
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

        public SortedDictionary<string, bool> FilterArchive(SortedDictionary<string, bool> source_dict) //internal
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

        public void ToggleArchive() //internal
        {
            _dictOptions = _viewer.HideArchive.Checked == true ? FilterArchive(_dictOptions) : _dictOriginal;
            SearchAndReload();
        }

        public void AddColorCategory(string categoryName = "") //internal
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

        public void FocusCheckbox(CheckBox cbx) //internal
        {
            intFocus = _colCbxCtrl.IndexOf(cbx);
            Select_Ctrl_By_Offset(0);
        }

        #endregion

        #region Keyboard Events

        public void OptionsPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) //internal
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

        public void OptionsPanel_KeyDown(object sender, KeyEventArgs e) //internal
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

        public void TagViewer_KeyDown(object sender, KeyEventArgs e) //internal
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

        public void TextBox1_KeyDown(object sender, KeyEventArgs e) //internal
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    {
                        _cursorPosition = _viewer.SearchText.SelectionStart;
                        break;
                    }
                case Keys.Down:
                    {
                        Select_Ctrl_By_Offset(1);
                        break;
                    }
            }
        }

        public void TextBox1_KeyUp(object sender, KeyEventArgs e) //internal
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    {
                        if (_viewer.SearchText.SelectionStart == _cursorPosition)
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

        
        #endregion

        #region Major Actions

        public bool LoadControls(SortedDictionary<string, bool> dict_options, string prefix) //internal
        {
            CheckBox ctrlCB;
            string strChkName;
            CheckBoxController clsCheckBox;

            //const int cHt_var = 18;
            //const int cHt_fxd = 6;
            //const int cLt = 6;
            //const int cWt = 300;

            _filteredOptions = dict_options;
            intFocus = 0;
            _colCbxCtrl = new();
            _colCbxEvent = new();

            for (int i = 0, loopTo = _filteredOptions.Count - 1; i <= loopTo; i++)
            {
                strChkName = i.ToString("00") + " ChkBx";
                ctrlCB = new CheckBox();
                try
                {
                    _viewer.L1v2L2_OptionsPanel.Controls.Add(ctrlCB);
                }
                catch
                {
                    MessageBox.Show($"Error adding {nameof(CheckBox)} in {nameof(Tags)}.{nameof(LoadControls)}");
                    return false;
                }
                
                ctrlCB.Text = _filteredOptions.Keys.ElementAt(i).Substring(prefix.Length);
                ctrlCB.Checked = _filteredOptions.Values.ElementAt(i);

                try
                {
                    clsCheckBox = new CheckBoxController();
                    clsCheckBox.Init(this, prefix);
                    clsCheckBox.CtrlCB = ctrlCB;
                }
                catch
                {
                    MessageBox.Show("Error wiring checkbox event in Tags.LoadControls");
                    return false;
                }

                // ctrlCB.AutoSize = True
                _gridTemplate.Set(ctrlCB, i, 0);
                
                // _viewer.OptionsPanel.ScrollHeight = ctrlCB.Top + cHt_var
                try
                {
                    _colCbxCtrl.Add(ctrlCB);
                    _colCbxEvent.Add(clsCheckBox);
                }
                catch
                {
                    MessageBox.Show("Error saving checkbox control and event to collection");
                    return false;
                }
            }
            return true;
        }

        public void RemoveControls() //internal
        {
            int max = _colCbxCtrl.Count - 1;
            for (int i = max; i >= 0; i -= 1)
            {
                _viewer.L1v2L2_OptionsPanel.Controls.Remove((Control)_colCbxCtrl[i]);
                _colCbxCtrl.RemoveAt(i);
                _colCbxEvent.RemoveAt(i);
            }

            max = _colColorbox.Count - 1;
            for (int i = max; i >= 0; i -= 1)
            {
                _viewer.L1v2L2_OptionsPanel.Controls.Remove((Control)_colColorbox[i]);
                bool unused = _colColorbox.Remove(i);
            }
        }

        public void AddOption(string strOption, bool blClickTrue = false) //internal
        {
            _dictOptions.Add(strOption, blClickTrue);
            _filteredOptions.Add(strOption, blClickTrue);
        }

        public void FilterToSelected() //internal
        {
            RemoveControls();
            // _filtered_options = _dict_options.Where(Function(x) x.Value = True).Select(Function(x) x)
            var tmp = (from x in _dictOptions
                       where x.Value
                       select x).ToDictionary(x => x.Key, x => x.Value);
            _filteredOptions = new SortedDictionary<string, bool>(tmp);
            bool unused = LoadControls(_filteredOptions, _prefix.Value);
        }

        public List<string> GetSelections()
        {
            return (from x in _dictOptions
                    where x.Value == true
                    select x.Key).ToList();
        }

        #endregion

        #region UI Navigation

        public void Select_Ctrl_By_Offset(int increment) //internal
        {
            int newpos = intFocus + increment;
            if (newpos == -1)
            {
                _viewer.SearchText.Select();
                intFocus = newpos;
            }
            else if (newpos <= _colCbxCtrl.Count - 1)
            {
                _colCbxCtrl[newpos].Focus();
                CheckBox cbx = (CheckBox)_colCbxCtrl[newpos];
                ControlPaint.DrawFocusRectangle(System.Drawing.Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle);
                intFocus = newpos;
            }
        }

        public void Select_Last_Control() //internal
        {
            Select_Ctrl_By_Position(_colCbxCtrl.Count - 1);
        }

        public void Select_First_Control() //internal
        {
            Select_Ctrl_By_Position(0);
        }

        public void Select_PageDown() //internal
        {

            if (_viewer.L1v2L2_OptionsPanel.VerticalScroll.Maximum > _viewer.L1v2L2_OptionsPanel.Height)
            {
                int start = Math.Max(intFocus, 0);
                int y = _viewer.L1v2L2_OptionsPanel.Height;
                var filteredIEnumerable = _colCbxCtrl.Select((n, i) =>
                                                       new { Value = n, Index = i })
                                                       .Where(p =>
                                                       (p.Index > intFocus) &
                                                       (p.Value.Bottom > y));

                if (filteredIEnumerable.Count() == 0)
                {
                    Select_Last_Control();
                }

                else
                {
                    int idx = filteredIEnumerable.First().Index;

                    Select_Ctrl_By_Position(idx);

                    int y_scroll = _colCbxCtrl[idx].Top - _viewer.L1v2L2_OptionsPanel.AutoScrollPosition.Y;

                    _viewer.L1v2L2_OptionsPanel.AutoScrollPosition = new System.Drawing.Point(
                        _viewer.L1v2L2_OptionsPanel.AutoScrollPosition.X, y_scroll);

                }

            }
        }

        public void Select_PageUp() //internal
        {
            if (_viewer.L1v2L2_OptionsPanel.VerticalScroll.Maximum > _viewer.L1v2L2_OptionsPanel.Height)
            {
                int start = Math.Max(intFocus, 0);
                int idx_top;

                var filteredIEnumerable = _colCbxCtrl.Select((n, i) => new { Value = n, Index = i })
                                                       .Where(p => p.Value.Top < 0);

                if (filteredIEnumerable.Count() == 0)
                {
                    Select_First_Control();
                }

                else
                {
                    idx_top = filteredIEnumerable.Last().Index;
                    Select_Ctrl_By_Position(idx_top);
                    int y_scroll = (-1 * _viewer.L1v2L2_OptionsPanel.AutoScrollPosition.Y)
                        - (_viewer.L1v2L2_OptionsPanel.Height - _colCbxCtrl[idx_top].Height);

                    _viewer.L1v2L2_OptionsPanel.AutoScrollPosition = new System.Drawing.Point(
                        _viewer.L1v2L2_OptionsPanel.AutoScrollPosition.X, y_scroll);

                }

            }
        }

        public void Select_Ctrl_By_Position(int position) //internal
        {
            if (position < -1 | position > _colCbxCtrl.Count - 1)
            {
                throw new ArgumentOutOfRangeException("Cannot select control with postition " + position);
            }

            else if (position == -1)
            {
                _viewer.SearchText.Select();
                intFocus = position;
            }

            else
            {
                _colCbxCtrl[position].Focus();
                CheckBox cbx = (CheckBox)_colCbxCtrl[position];
                ControlPaint.DrawFocusRectangle(System.Drawing.Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle);
                intFocus = position;
            }
        }


        #endregion


    }
}