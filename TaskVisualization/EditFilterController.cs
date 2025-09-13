using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tags;
using ToDoModel;
using UtilitiesCS;

namespace TaskVisualization
{
    internal class EditFilterController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Initializers

        public EditFilterController() { }

        private EditFilterController(IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
        }

        public EditFilterController(
            IApplicationGlobals appGlobals,
            Action<EditFilterController, FilterEntry> callback)
        {
            _callback = callback;
            _filterEntry = new FilterEntry();
            _globals = appGlobals;
            Initialize();
        }

        public EditFilterController(
            IApplicationGlobals appGlobals, 
            FilterEntry filterEntry)
        {
            _filterEntryCopy = (FilterEntry)filterEntry.Clone();
            _filterEntry = filterEntry;
            _globals = appGlobals;
            Initialize();
        }

        public static bool DeleteFilterDialog(
            IApplicationGlobals appGlobals,
            FilterEntry filterEntry)
        {
            var fd = new EditFilterController(appGlobals);
            var viewer = fd.InitializeFactory();
            viewer.Text = "Are you sure you want to delete this filter?";
            DialogResult result = viewer.ShowDialog();
            if (result == DialogResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private FilterEntry _filterEntryCopy;
        private FilterEntry _filterEntry;
        Action<EditFilterController, FilterEntry> _callback;
        private EditFilterViewer _viewer;
        private IApplicationGlobals _globals;
        private FlagClassNoItem _olFlags;
        private ToDoDefaults _defaults;
        private List<QfcTipsDetails> _tips;

        internal void Initialize() 
        {
            _viewer = new EditFilterViewer();

            _defaults = new ToDoDefaults();

            _olFlags = new FlagClassNoItem(_globals.Ol.NamespaceMAPI.Categories);
            
            if (!_filterEntry.Flags.Context.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.ContextSelection.Text = _filterEntry.Flags.Context.AsStringNoPrefix;
            if (!_filterEntry.Flags.People.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.PeopleSelection.Text = _filterEntry.Flags.People.AsStringNoPrefix;
            if (!_filterEntry.Flags.Projects.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.ProjectSelection.Text = _filterEntry.Flags.Projects.AsStringNoPrefix;
            if (!_filterEntry.Flags.Topics.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.TopicSelection.Text = _filterEntry.Flags.Topics.AsStringNoPrefix;

            _tips = _viewer.GetTips().Select(label => new QfcTipsDetails(label)).ToList();
            _tips.ForEach(tip => tip.Toggle(Enums.ToggleState.Off));

            RegisterEventHandlers();

            _viewer.Show();
        }

        internal EditFilterViewer InitializeFactory()
        {
            _viewer = new EditFilterViewer();

            _defaults = new ToDoDefaults();

            _olFlags = new FlagClassNoItem(_globals.Ol.NamespaceMAPI.Categories);

            if (!_filterEntry.Flags.Context.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.ContextSelection.Text = _filterEntry.Flags.Context.AsStringNoPrefix;
            if (!_filterEntry.Flags.People.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.PeopleSelection.Text = _filterEntry.Flags.People.AsStringNoPrefix;
            if (!_filterEntry.Flags.Projects.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.ProjectSelection.Text = _filterEntry.Flags.Projects.AsStringNoPrefix;
            if (!_filterEntry.Flags.Topics.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.TopicSelection.Text = _filterEntry.Flags.Topics.AsStringNoPrefix;

            return _viewer;
        }

        #endregion Constructors and Initializers

        #region Major Actions

        public void SelectItems(
            FlagTranslator options, 
            FlagTranslator selections, 
            IPrefix prefix, 
            System.Windows.Forms.Label label)
        {
            var dictOptions = options.AsListWithPrefix
                                     .Select(s => new KeyValuePair<string, bool>(s, false))
                                     .ToSortedDictionary();

            using (var viewer = new TagViewer())
            {
                var controller = new TagController(viewer, dictOptions);
                viewer.ShowDialog();
                if (controller.ExitType != "Cancel")
                {
                    selections.AsStringNoPrefix = controller.SelectionAsString();
                    label.Text = selections.AsStringNoPrefix;
                }
            }
        }

        internal void SetUpDeleteDialog() 
        { 

        }

        #endregion Major Actions

        #region Event Handlers

        internal void RegisterEventHandlers()
        {
            _viewer.ContextSelection.Click += CategorySelection_Click;
            _viewer.PeopleSelection.Click += PeopleSelection_Click;
            _viewer.ProjectSelection.Click += ProjectSelection_Click;
            _viewer.TopicSelection.Click += TopicSelection_Click;
            _viewer.FoldersSelected.Click += FoldersSelected_Click;
            _viewer.BtnOk.Click += BtnOk_Click;
            _viewer.BtnCancel.Click += BtnCancel_Click;
        }

        private void CategorySelection_Click(object sender, EventArgs e)
        {
            var prefix = _defaults.PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.Context);
            SelectItems(_olFlags.Context, _filterEntry.Flags.Context, prefix, _viewer.ContextSelection);
        }

        private void PeopleSelection_Click(object sender, EventArgs e)
        {
            var prefix = _defaults.PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.People);
            SelectItems(_olFlags.People, _filterEntry.Flags.People, prefix, _viewer.PeopleSelection);
        }

        private void ProjectSelection_Click(object sender, EventArgs e)
        {
            var prefix = _defaults.PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.Project);
            SelectItems(_olFlags.Projects, _filterEntry.Flags.Projects, prefix, _viewer.ProjectSelection);
        }

        private void TopicSelection_Click(object sender, EventArgs e)
        {
            var prefix = _defaults.PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.Topic);
            SelectItems(_olFlags.Topics, _filterEntry.Flags.Topics, prefix, _viewer.TopicSelection);
        }

        private void FoldersSelected_Click(object sender, EventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if(_callback is null) 
            {
                _viewer.Close();
                _filterEntry.RevertToCopy(_filterEntryCopy);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            _viewer.Hide();
            _filterEntry.Name = _viewer.FilterName.Text;
            if (_callback is not null)
            {
                _callback(this, _filterEntry);
            }
            _viewer.Dispose();
        }

        #endregion Event Handlers

    }
}
