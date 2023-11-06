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

        public TagController(TagViewer viewerInstance,
                             SortedDictionary<string, bool> dictOptions,
                             IAutoAssign autoAssigner,
                             IList<IPrefix> prefixes,
                             string userEmailAddress,
                             IList<string> selections = null,
                             string prefixKey = "",
                             object objItemObject = null,
                             object objCallerObj = null)
        {

            viewerInstance.SetController(this);
            _autoAssigner = autoAssigner;
            _prefixes = prefixes;
            _viewer = viewerInstance;
            _objItem = objItemObject;
            _dictOriginal = dictOptions;
            _dictOptions = _viewer.HideArchive.Checked == true ? FilterArchive(dictOptions) : dictOptions;
            _userEmailAddress = userEmailAddress;
            _selections = selections;
            _objCaller = objCallerObj;
            
            _olMail = ResolveMailItem(_objItem);
            
            if (_olMail is not null) { _isMail = true; }

            _gridTemplate = CaptureAndRemoveTemplate();
            
            ResolvePrefix(_prefixes, prefixKey);

            SetAutoAssignState(_autoAssigner);

            LoadSelections(selections);

            LoadControls(_dictOptions, _prefix.Value);

            WireEvents();

        }

        public TagController(TagViewer viewerInstance,
                             SortedDictionary<string, bool> dictOptions,
                             IList<string> selections = null,
                             IPrefix prefix = null)
        {
            viewerInstance.SetController(this);
            _viewer = viewerInstance;
            _dictOriginal = dictOptions;
            _dictOptions = dictOptions;
            _selections = selections;
            _isMail = false;

            _gridTemplate = CaptureAndRemoveTemplate();
            SetAutoAssignState(null);

            _prefix = prefix ?? GetDefaultPrefix();

            LoadSelections(selections);

            LoadControls(_dictOptions, _prefix.Value);

            WireEvents();
        }

        public MailItem ResolveMailItem(object objItem) //internal
        {
            if ((objItem is not null) && (objItem is MailItem))
            {
                return (MailItem)_objItem;
            }
            else return null;
        } 

        internal IPrefix GetDefaultPrefix() => new PrefixItem(PrefixTypeEnum.Other, "", "", OlCategoryColor.olCategoryColorNone);

        public void ResolvePrefix(IList<IPrefix> prefixes, string prefixKey) //internal
        {
            // Set default prefix if none exists
            if (prefixes is null || string.IsNullOrEmpty(prefixKey))
            {
                _prefix = GetDefaultPrefix();
            }
            // Else if it exists, set the Iprefix based on the prefixKey
            else if (prefixes.Exists(x => x.Key == prefixKey))
            {
                _prefix = prefixes.Find(x => (x.Key) == prefixKey );
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
                _viewer.ButtonAutoAssign.Visible = true;
                _viewer.ButtonAutoAssign.Enabled = true;
            }
            else
            {
                _viewer.ButtonAutoAssign.Visible = false;
                _viewer.ButtonAutoAssign.Enabled = false;
            }
        } 

        public void LoadSelections(IList<string> selections) //internal
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
        private IList<string> _selections;
        private IList<string> _filteredSelections;
        private object _objItem;
        private MailItem _olMail;
        private readonly object _objCaller;
        private IPrefix _prefix;
        private readonly IList<IPrefix> _prefixes;
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


        #region Public Functions and Properties

        public void ToggleChoice(string str_choice) => _dictOptions[str_choice] = !_dictOptions[str_choice];
        
        public void ToggleOn(string str_choice) => _dictOptions[str_choice] = true; //internal
            
        public void ToggleOff(string str_choice) => _dictOptions[str_choice] = false; //internal

        public void UpdateSelections()
        {
            _selections = _dictOptions.Where(x => x.Value).Select(x => x.Key).ToList();
            _filteredSelections = _filteredOptions.Where(x => x.Value).Select(x => x.Key).ToList();
        }

        public void SearchAndReload() //internal
        {
            // Get search strings 
            var searchStrings = ParseSearchStrings(_viewer.SearchText.Text);
            
            // Filter the dictionary based on the search strings
            var filtered = Search(_dictOptions, searchStrings);
            
            // If the filtered dictionary is different from the current filtered dictionary, then reload the controls
            if (!_filteredOptions.SequenceEqual(filtered))
            {
                RemoveControls();
                LoadControls(filtered, _prefix.Value); 
            }
        }

        public SortedDictionary<string, bool> Search(SortedDictionary<string, bool> source, List<string> searchStrings)
        {
            // If there are no search strings, then the filtered dictionary is the original dictionary
            if (searchStrings.Count == 0) { return source; }

            // Else, filter the original dictionary based on the search strings
            return searchStrings.Select(search => source
                                            .Where(x => x.Key.IndexOf(
                                                search, StringComparison.OrdinalIgnoreCase) >= 0))
                                            .SelectMany(x => x)
                                            .Distinct()
                                            .ToSortedDictionary();
            
        }

        public List<string> ParseSearchStrings(string searchText)
        {
            searchText = searchText.Trim();
            if (searchText.IsNullOrEmpty())
                return new List<string>();
            return searchText.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public string SelectionString() => string.Join(", ", _dictOptions.Where(item => item.Value).Select(item => item.Key).ToList());
        
        public bool ButtonNewActive { get => _viewer.ButtonNew.Visible; set => _viewer.ButtonNew.Visible = value; }
        
        public bool ButtonAutoAssignActive {  get => _viewer.ButtonAutoAssign.Visible; set => _viewer.ButtonAutoAssign.Visible = value; }
        
        public void SetSearchText(string searchText) => _viewer.SearchText.Text = searchText;

        public string ExitType { get => _exitType; }

        #endregion

        #region Public Events

        public void WireEvents() 
        {
            _viewer.L1v2L2_OptionsPanel.KeyDown += new System.Windows.Forms.KeyEventHandler(L1v2L2_OptionsPanel_KeyDown);
            _viewer.L1v2L2_OptionsPanel.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(OptionsPanel_PreviewKeyDown);
            _viewer.ButtonOk.Click += new System.EventHandler(ButtonOk_Click);
            _viewer.ButtonCancel.Click += new System.EventHandler(ButtonCancel_Click);
            _viewer.ButtonNew.Click += new System.EventHandler(ButtonNew_Click);
            _viewer.ButtonAutoAssign.Click += new System.EventHandler(ButtonAutoAssign_Click);
            _viewer.SearchText.TextChanged += new System.EventHandler(SearchText_TextChanged);
            _viewer.SearchText.KeyDown += new System.Windows.Forms.KeyEventHandler(SearchText_KeyDown);
            _viewer.SearchText.KeyUp += new System.Windows.Forms.KeyEventHandler(SearchText_KeyUp);
            _viewer.HideArchive.CheckedChanged += new System.EventHandler(HideArchive_CheckedChanged);
            _viewer.KeyDown += new System.Windows.Forms.KeyEventHandler(TagViewer_KeyDown);
        }

        private void L1v2L2_OptionsPanel_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void ButtonOk_Click(object sender, EventArgs e) => ButtonOk_Action();
        
        public void ButtonOk_Action() //internal
        {
            _viewer.Close();
            _exitType = "Normal";
        }

        private void ButtonNew_Click(object sender, EventArgs e) => AddColorCategory();

        private void ButtonAutoAssign_Click(object sender, EventArgs e)
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

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            _viewer.Close();
            _exitType = "Cancel";
        }

        private void SearchText_TextChanged(object sender, EventArgs e) => SearchAndReload();

        private void HideArchive_CheckedChanged(object sender, EventArgs e)
        {
            _dictOptions = _viewer.HideArchive.Checked == true ? FilterArchive(_dictOptions) : _dictOriginal;
            SearchAndReload();
        }

        #endregion

        #region Old Event Actions

        public SortedDictionary<string, bool> FilterArchive(SortedDictionary<string, bool> sourceDict) //internal
        {

            if (_autoAssigner is not null)
            {
                var exclude = _autoAssigner.FilterList;
                // Dim filtered_dict = (From x In source_dict
                // Where Not exclude.Contains(x.Key)
                // Select x).ToSortedDictionary()
                //var filteredDict = (from x in sourceDict
                //                    where exclude.FindIndex(x.Key, (int)StringComparison.OrdinalIgnoreCase) < 0
                //                    select x).ToSortedDictionary();
                var filteredDict = (from x in sourceDict
                                    where !exclude.Contains(x.Key, StringComparison.OrdinalIgnoreCase)
                                    select x).ToSortedDictionary();
                return filteredDict;
            }
            else
            {
                return sourceDict;
            }

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
                if (!string.IsNullOrEmpty(categoryName)&&_autoAssigner is not null)
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
                        ButtonOk_Action();
                        break;
                    }
            }
        }

        public void SearchText_KeyDown(object sender, KeyEventArgs e) //internal
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

        public void SearchText_KeyUp(object sender, KeyEventArgs e) //internal
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
                        ButtonOk_Action();
                        break;
                    }
            }
        }
        
        #endregion

        #region Major Actions

        public bool LoadControls(SortedDictionary<string, bool> dictOptions, string prefix) //internal
        {
            CheckBox ctrlCB;
            string strChkName;
            CheckBoxController clsCheckBox;

            _filteredOptions = dictOptions;
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
                ControlPosition.Set(ctrlCB, _gridTemplate, i, 0);
                
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

        public void AddOption(string option, bool blClickTrue = false) //internal
        {
            if (!_dictOptions.ContainsKey(option))
            {
                _dictOptions.Add(option, blClickTrue);
            }
            else
            {
                _dictOptions[option] = blClickTrue;
            }
                
            if (!_dictOptions.Equals(_filteredOptions))
            {
                if (!_filteredOptions.ContainsKey(option))
                {
                    _filteredOptions.Add(option, blClickTrue);
                }
                else
                {
                    _filteredOptions[option] = blClickTrue;
                }
            }
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

        #region Helper Functions



        #endregion

    }
}