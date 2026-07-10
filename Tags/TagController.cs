using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace Tags
{
    /// <summary>
    /// Orchestrates the tag-selection dialog. Depends only on the <see cref="ITagViewer"/> seam,
    /// the <see cref="IUserPrompt"/> dialog seam, and the host-neutral <see cref="TagSelectionModel"/>
    /// for selection/search/filter/prefix state. Rendering and keyboard-navigation logic live in the
    /// partial <c>TagController.Rendering.cs</c>.
    /// </summary>
    public partial class TagController
    {
        #region Contructors and Initializers

        public TagController(
            ITagViewer viewerInstance,
            SortedDictionary<string, bool> dictOptions,
            IAutoAssign autoAssigner,
            IList<IPrefix> prefixes,
            string userEmailAddress,
            IList<string> selections = null,
            string prefixKey = "",
            object objItemObject = null,
            object objCallerObj = null,
            IUserPrompt prompt = null,
            Action<CheckBox> drawFocus = null
        )
        {
            viewerInstance.SetController(this);
            _autoAssigner = autoAssigner;
            _prefixes = prefixes;
            _viewer = viewerInstance;
            _prompt = prompt ?? new WinFormsUserPrompt();
            _drawFocus = drawFocus ?? DrawFocusDefault;
            _objItem = objItemObject;
            _model = new TagSelectionModel(dictOptions, autoAssigner, selections);
            if (_viewer.HideArchiveChecked)
            {
                _model.SetDictOptions(_model.FilterArchive(dictOptions));
            }
            _userEmailAddress = userEmailAddress;
            _objCaller = objCallerObj;

            _olMail = ResolveMailItem(_objItem);

            if (_olMail is not null)
            {
                _isMail = true;
            }

            _gridTemplate = _viewer.CaptureAndRemoveTemplate();

            _model.ResolvePrefix(_prefixes, prefixKey);

            SetAutoAssignState(_autoAssigner);

            LoadSelections(selections);

            LoadControls(_model.DictOptions, _model.Prefix.Value);

            WireEvents();
        }

        public TagController(
            ITagViewer viewerInstance,
            SortedDictionary<string, bool> dictOptions,
            IList<string> selections = null,
            IPrefix prefix = null,
            IUserPrompt prompt = null,
            Action<CheckBox> drawFocus = null
        )
        {
            viewerInstance.SetController(this);
            _viewer = viewerInstance;
            _prompt = prompt ?? new WinFormsUserPrompt();
            _drawFocus = drawFocus ?? DrawFocusDefault;
            _model = new TagSelectionModel(dictOptions, null, selections);
            _isMail = false;

            _gridTemplate = _viewer.CaptureAndRemoveTemplate();
            SetAutoAssignState(null);

            _model.Prefix = prefix ?? _model.GetDefaultPrefix();

            LoadSelections(selections);

            LoadControls(_model.DictOptions, _model.Prefix.Value);

            WireEvents();

            _viewer.FocusSearch();
        }

        public MailItem ResolveMailItem(object objItem) //internal
        {
            if ((objItem is not null) && (objItem is MailItem))
            {
                return (MailItem)_objItem;
            }
            else
                return null;
        }

        internal IPrefix GetDefaultPrefix() => _model.GetDefaultPrefix();

        public void ResolvePrefix(IList<IPrefix> prefixes, string prefixKey) => //internal
            _model.ResolvePrefix(prefixes, prefixKey);

        public void SetAutoAssignState(IAutoAssign autoAssigner) //internal
        {
            // Determine if the autoAssign button should be visible and active
            if (autoAssigner is not null & _isMail)
            {
                _viewer.AutoAssignVisible = true;
                _viewer.AutoAssignEnabled = true;
            }
            else
            {
                _viewer.AutoAssignVisible = false;
                _viewer.AutoAssignEnabled = false;
            }
        }

        public void LoadSelections(IList<string> selections) //internal
        {
            if ((selections is not null) && (_model.Selections.Count > 0))
            {
                var addPrefix = _model.IsPrefixMissing(_model.Prefix, _model.Selections[0]);

                foreach (string rawchoice in _model.Selections)
                {
                    string choice = rawchoice;
                    if (addPrefix)
                        choice = string.Concat(_model.Prefix.Value, choice);
                    if (_model.ContainsOption(choice))
                    {
                        _model.ToggleChoice(choice);
                    }
                    else
                    {
                        var tmpResponse = _prompt.ShowYesNo(
                            $"{choice} does not exist. Would you like to add it?",
                            "Dialog"
                        );
                        if (tmpResponse == DialogResult.Yes)
                        {
                            AddColorCategory(rawchoice);
                        }
                    }
                }
            }
        }

        public bool IsPrefixMissing(IPrefix prefix, string sample) => //internal
            _model.IsPrefixMissing(prefix, sample);

        private readonly ITagViewer _viewer;
        private readonly IUserPrompt _prompt;
        private readonly Action<CheckBox> _drawFocus;
        private readonly TagSelectionModel _model;
        private object _objItem;
        private MailItem _olMail;
        private readonly object _objCaller;
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

        #endregion Contructors and Initializers


        #region Public Functions and Properties

        public void ToggleChoice(string str_choice) => _model.ToggleChoice(str_choice);

        public void ToggleOn(string str_choice) => _model.ToggleOn(str_choice); //internal

        public void ToggleOff(string str_choice) => _model.ToggleOff(str_choice); //internal

        public void UpdateSelections() => _model.UpdateSelections();

        public void AddOption(string option, bool blClickTrue = false) => //internal
            _model.AddOption(option, blClickTrue);

        public void SearchAndReload() //internal
        {
            // Get search strings
            var searchStrings = _model.ParseSearchStrings(_viewer.SearchTextValue);

            // Filter the dictionary based on the search strings
            var filtered = _model.Search(_model.DictOptions, searchStrings);

            // If the filtered dictionary is different from the current filtered dictionary, then reload the controls
            if (!_model.FilteredOptions.SequenceEqual(filtered))
            {
                RemoveControls();
                LoadControls(filtered, _model.Prefix.Value);
            }
        }

        public SortedDictionary<string, bool> Search(
            SortedDictionary<string, bool> source,
            List<string> searchStrings
        ) => _model.Search(source, searchStrings);

        public List<string> ParseSearchStrings(string searchText) =>
            _model.ParseSearchStrings(searchText);

        public string SelectionAsString() => _model.SelectionAsString();

        public List<string> SelectionAsList() => _model.SelectionAsList();

        public bool ButtonNewActive
        {
            get => _viewer.ButtonNewVisible;
            set => _viewer.ButtonNewVisible = value;
        }

        public bool ButtonAutoAssignActive
        {
            get => _viewer.AutoAssignVisible;
            set => _viewer.AutoAssignVisible = value;
        }

        public void SetSearchText(string searchText) => _viewer.SearchTextValue = searchText;

        public void SetCaption(string caption)
        {
            if (_viewer != null)
                _viewer.Caption = caption;
        }

        public string ExitType
        {
            get => _exitType;
        }

        #endregion

        #region Public Events

        public void WireEvents()
        {
            _viewer.OptionsKeyDown += L1v2L2_OptionsPanel_KeyDown;
            _viewer.OptionsPreviewKeyDown += OptionsPanel_PreviewKeyDown;
            _viewer.OkClicked += ButtonOk_Click;
            _viewer.CancelClicked += ButtonCancel_Click;
            _viewer.NewClicked += ButtonNew_Click;
            _viewer.AutoAssignClicked += ButtonAutoAssign_Click;
            _viewer.SearchTextChanged += SearchText_TextChanged;
            _viewer.SearchKeyDown += SearchText_KeyDown;
            _viewer.SearchKeyUp += SearchText_KeyUp;
            _viewer.HideArchiveChanged += HideArchive_CheckedChanged;
            _viewer.ViewKeyDown += TagViewer_KeyDown;
        }

        private void L1v2L2_OptionsPanel_KeyDown(object sender, KeyEventArgs e) { }

        private void ButtonOk_Click(object sender, EventArgs e) => ButtonOk_Action();

        public void ButtonOk_Action() //internal
        {
            _viewer.Close();
            _exitType = "Normal";
        }

        private void ButtonNew_Click(object sender, EventArgs e) => AddColorCategory();

        private async void ButtonAutoAssign_Click(object sender, EventArgs e)
        {
            await ButtonAutoAssign_Action().ConfigureAwait(true);
        }

        internal async Task ButtonAutoAssign_Action()
        {
            try
            {
                var col_choices = await _autoAssigner.AutoFindAsync(_objItem).ConfigureAwait(true);
                foreach (string str_choice in col_choices)
                {
                    if (_model.DictOptions.ContainsKey(str_choice))
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
            catch (System.Exception)
            {
                throw;
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            _viewer.Close();
            _exitType = "Cancel";
        }

        private void SearchText_TextChanged(object sender, EventArgs e) => SearchAndReload();

        private void HideArchive_CheckedChanged(object sender, EventArgs e)
        {
            _model.SetDictOptions(
                _viewer.HideArchiveChecked
                    ? _model.FilterArchive(_model.DictOptions)
                    : _model.DictOriginal
            );
            SearchAndReload();
        }

        #endregion

        #region Old Event Actions

        public SortedDictionary<string, bool> FilterArchive(
            SortedDictionary<string, bool> sourceDict
        ) => _model.FilterArchive(sourceDict); //internal

        internal bool TryGetAutoAssignment(out IList<string> assignments)
        {
            bool autoAdded = false;
            assignments = [];

            // Check to see if can be automatically created
            if (_autoAssigner is not null & _isMail)
            {
                // Ask user if they want to auto-add
                var vbR = _prompt.ShowYesNo("Auto-add new from email details?", "Dialog");

                if (vbR == DialogResult.Yes)
                {
                    assignments = _autoAssigner.AddChoicesToDict(
                        _olMail,
                        _prefixes,
                        _model.Prefix.Key,
                        _userEmailAddress
                    );

                    foreach (string newCatName in assignments)
                    {
                        AddOption(newCatName, blClickTrue: true);
                        autoAdded = true;
                    }
                }
            }
            return autoAdded;
        }

        public void AddColorCategory(string categoryName = "") //internal
        {
            // Only create category if we can't auto-assign to an existing
            if (!TryGetAutoAssignment(out var assignments))
            {
                // Get the category name from the user
                categoryName = GetUserInputCategory(categoryName);

                // If the user entered a category name, add it to the options
                if (!string.IsNullOrEmpty(categoryName))
                {
                    // If the _autoAssigner is not null, use its delegate to add the category
                    if (_autoAssigner is not null)
                    {
                        var newCategory = _autoAssigner.AddColorCategory(
                            _model.Prefix,
                            categoryName
                        );
                        if (newCategory is null)
                        {
                            return;
                        }
                        categoryName = newCategory.Name;
                    }
                    AddOption(categoryName, blClickTrue: true);
                    assignments.Add(categoryName);
                }
            }

            if (assignments.Count > 0)
                FilterToSelected();
        }

        internal string GetUserInputCategory(string categoryName)
        {
            if (!string.IsNullOrEmpty(categoryName))
            {
                categoryName = _prompt.GetCategoryInput(
                    "The following category name will be added:",
                    "Add Category Dialog",
                    categoryName
                );
            }
            else
            {
                bool advance = false;
                string msg = "Enter new category name:";
                while (!advance)
                {
                    categoryName = _prompt.GetCategoryInput(msg, "Add Category Dialog", " ");
                    if (categoryName != " ")
                        advance = true;
                    msg = "Please enter a name or hit cancel:";
                }
            }

            return categoryName;
        }

        #endregion

        #region Helper Functions

        public List<string> GetSelections() => _model.GetSelections();

        #endregion
    }
}
