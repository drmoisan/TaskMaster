using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tags;
using ToDoModel;
using UtilitiesCS;

namespace TaskVisualization
{
    internal class EditFilterController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Seams

        // Viewer factory seam: production creates the concrete WinForms form; tests
        // inject a Mock<IEditFilterViewer> so no live form is constructed.
        private Func<IEditFilterViewer> _viewerFactory = DefaultViewerFactory;

        // Tag-dialog seam: production shows the Tags dialog; tests inject a canned
        // (cancelled, selection) result so no popup is shown. Narrow per-call
        // delegate chosen over the broader ITagPromptService (see Phase 0 P0-T3).
        private Func<
            SortedDictionary<string, bool>,
            (bool cancelled, string selection)
        > _tagSelector = DefaultTagSelector;

        [ExcludeFromCodeCoverage]
        private static IEditFilterViewer DefaultViewerFactory() => new EditFilterViewer();

        // Outlook/UI-bound: constructs the Tags viewer/controller and shows a modal
        // dialog; not unit-testable without a live form.
        [ExcludeFromCodeCoverage]
        private static (bool cancelled, string selection) DefaultTagSelector(
            SortedDictionary<string, bool> dictOptions
        )
        {
            using (var viewer = new TagViewer())
            {
                var controller = new TagController(viewer, dictOptions);
                viewer.ShowDialog();
                if (controller.ExitType != "Cancel")
                {
                    return (false, controller.SelectionAsString());
                }
                return (true, null);
            }
        }

        #endregion Seams

        #region Constructors and Initializers

        public EditFilterController() { }

        public EditFilterController(
            IApplicationGlobals appGlobals,
            Action<EditFilterController, FilterEntry> callback
        )
            : this(appGlobals, null, callback, null, null) { }

        public EditFilterController(IApplicationGlobals appGlobals, FilterEntry filterEntry)
            : this(appGlobals, filterEntry, null, null, null) { }

        /// <summary>
        /// Core constructor. Public constructors funnel here with default seams so
        /// their behavior is unchanged; <c>TaskVisualization.Test</c> uses this
        /// overload (via <c>InternalsVisibleTo</c>) to inject the viewer factory and
        /// tag-selector seams. When <paramref name="filterEntry"/> is null a fresh
        /// <see cref="FilterEntry"/> is created (add-filter path); otherwise the
        /// supplied entry is used and a revert copy is cloned (edit-filter path).
        /// </summary>
        internal EditFilterController(
            IApplicationGlobals appGlobals,
            FilterEntry filterEntry,
            Action<EditFilterController, FilterEntry> callback,
            Func<IEditFilterViewer> viewerFactory,
            Func<SortedDictionary<string, bool>, (bool cancelled, string selection)> tagSelector
        )
        {
            if (viewerFactory is not null)
            {
                _viewerFactory = viewerFactory;
            }
            if (tagSelector is not null)
            {
                _tagSelector = tagSelector;
            }
            _callback = callback;
            if (filterEntry is not null)
            {
                _filterEntryCopy = (FilterEntry)filterEntry.Clone();
                _filterEntry = filterEntry;
            }
            else
            {
                _filterEntry = new FilterEntry();
            }
            _globals = appGlobals;
            Initialize();
        }

        private FilterEntry _filterEntryCopy;
        private FilterEntry _filterEntry;
        Action<EditFilterController, FilterEntry> _callback;
        private IEditFilterViewer _viewer;
        private IApplicationGlobals _globals;
        private FlagClassNoItem _olFlags;
        private ToDoDefaults _defaults;

        internal void Initialize()
        {
            _viewer = _viewerFactory();

            _defaults = new ToDoDefaults();

            _olFlags = new FlagClassNoItem(_globals.Ol.NamespaceMAPI.Categories);

            ApplySelectionText();

            _viewer.ResetTips();

            RegisterEventHandlers();

            _viewer.Show();
        }

        internal IEditFilterViewer InitializeFactory()
        {
            _viewer = _viewerFactory();

            _defaults = new ToDoDefaults();

            _olFlags = new FlagClassNoItem(_globals.Ol.NamespaceMAPI.Categories);

            ApplySelectionText();

            return _viewer;
        }

        private void ApplySelectionText()
        {
            if (!_filterEntry.Flags.Context.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.ContextSelectionText = _filterEntry.Flags.Context.AsStringNoPrefix;
            if (!_filterEntry.Flags.People.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.PeopleSelectionText = _filterEntry.Flags.People.AsStringNoPrefix;
            if (!_filterEntry.Flags.Projects.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.ProjectSelectionText = _filterEntry.Flags.Projects.AsStringNoPrefix;
            if (!_filterEntry.Flags.Topics.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.TopicSelectionText = _filterEntry.Flags.Topics.AsStringNoPrefix;
        }

        #endregion Constructors and Initializers

        #region Major Actions

        public void SelectItems(
            FlagTranslator options,
            FlagTranslator selections,
            IPrefix prefix,
            Action<string> setText
        )
        {
            var dictOptions = options
                .AsListWithPrefix.Select(s => new KeyValuePair<string, bool>(s, false))
                .ToSortedDictionary();

            var (cancelled, selection) = _tagSelector(dictOptions);
            if (!cancelled)
            {
                selections.AsStringNoPrefix = selection;
                setText(selections.AsStringNoPrefix);
            }
        }

        #endregion Major Actions

        #region Event Handlers

        internal void RegisterEventHandlers()
        {
            _viewer.ContextSelectionClick += CategorySelection_Click;
            _viewer.PeopleSelectionClick += PeopleSelection_Click;
            _viewer.ProjectSelectionClick += ProjectSelection_Click;
            _viewer.TopicSelectionClick += TopicSelection_Click;
            _viewer.FoldersSelectedClick += FoldersSelected_Click;
            _viewer.OkClick += BtnOk_Click;
            _viewer.CancelClick += BtnCancel_Click;
        }

        private void CategorySelection_Click(object sender, EventArgs e)
        {
            var prefix = _defaults.PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.Context);
            SelectItems(
                _olFlags.Context,
                _filterEntry.Flags.Context,
                prefix,
                s => _viewer.ContextSelectionText = s
            );
        }

        private void PeopleSelection_Click(object sender, EventArgs e)
        {
            var prefix = _defaults.PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.People);
            SelectItems(
                _olFlags.People,
                _filterEntry.Flags.People,
                prefix,
                s => _viewer.PeopleSelectionText = s
            );
        }

        private void ProjectSelection_Click(object sender, EventArgs e)
        {
            var prefix = _defaults.PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.Project);
            SelectItems(
                _olFlags.Projects,
                _filterEntry.Flags.Projects,
                prefix,
                s => _viewer.ProjectSelectionText = s
            );
        }

        private void TopicSelection_Click(object sender, EventArgs e)
        {
            var prefix = _defaults.PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.Topic);
            SelectItems(
                _olFlags.Topics,
                _filterEntry.Flags.Topics,
                prefix,
                s => _viewer.TopicSelectionText = s
            );
        }

        private void FoldersSelected_Click(object sender, EventArgs e) { }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (_callback is null)
            {
                _viewer.Close();
                _filterEntry.RevertToCopy(_filterEntryCopy);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            _viewer.Hide();
            _filterEntry.Name = _viewer.FilterNameText;
            if (_callback is not null)
            {
                _callback(this, _filterEntry);
            }
            _viewer.Dispose();
        }

        #endregion Event Handlers
    }
}
