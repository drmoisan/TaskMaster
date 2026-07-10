using System;
using UtilitiesCS;

namespace TaskVisualization
{
    /// <summary>
    /// Host-neutral orchestration for the Manage Filters view. Depends only on
    /// <see cref="IManageFiltersViewer"/> and <see cref="IApplicationGlobals"/>
    /// plus an injectable edit-filter factory seam, so the filter-management logic
    /// is unit-testable against a mocked viewer with no live form.
    /// </summary>
    /// <remarks>
    /// The <see cref="EditFilterController"/> factory seam distinguishes the two
    /// production construction paths by the supplied <see cref="FilterEntry"/>: a
    /// null entry uses the add path (wires <see cref="EditFilterCallback"/> so the
    /// new entry is committed on OK); a non-null entry uses the in-place edit path.
    /// This preserves the original form's observable behavior.
    /// </remarks>
    public class ManageFiltersController
    {
        private readonly IManageFiltersViewer _viewer;
        private readonly IApplicationGlobals _globals;
        private readonly Func<
            IApplicationGlobals,
            FilterEntry,
            EditFilterController
        > _editFilterFactory;

        /// <summary>Creates a controller with the production edit-filter factory.</summary>
        public ManageFiltersController(IManageFiltersViewer viewer, IApplicationGlobals globals)
            : this(viewer, globals, null) { }

        /// <summary>
        /// Creates a controller. <c>TaskVisualization.Test</c> uses this overload to
        /// inject an <paramref name="editFilterFactory"/> stub; production passes
        /// null and the default factory is used.
        /// </summary>
        internal ManageFiltersController(
            IManageFiltersViewer viewer,
            IApplicationGlobals globals,
            Func<IApplicationGlobals, FilterEntry, EditFilterController> editFilterFactory
        )
        {
            _viewer = viewer;
            _globals = globals;
            _editFilterFactory = editFilterFactory ?? DefaultEditFilterFactory;
        }

        private EditFilterController DefaultEditFilterFactory(
            IApplicationGlobals globals,
            FilterEntry filterEntry
        )
        {
            return filterEntry is null
                ? new EditFilterController(globals, EditFilterCallback)
                : new EditFilterController(globals, filterEntry);
        }

        /// <summary>Binds the current filter set into the viewer's list.</summary>
        public void LoadFilters()
        {
            _viewer.SetFilters(_globals.AF.Filters);
        }

        /// <summary>Opens the selected filter for in-place editing.</summary>
        public void EditSelected()
        {
            var filterEntry = _viewer.SelectedFilter;
            _editFilterFactory(_globals, filterEntry);
        }

        /// <summary>Opens the add-filter dialog and refreshes the list.</summary>
        public void AddFilter()
        {
            _editFilterFactory(_globals, null);
            _viewer.SetFilters(_globals.AF.Filters);
            _viewer.RebuildList();
        }

        /// <summary>
        /// Callback invoked when a newly added filter is confirmed: commits the
        /// entry to the persisted filter set and rebuilds the viewer's list.
        /// </summary>
        internal void EditFilterCallback(EditFilterController controller, FilterEntry filterEntry)
        {
            _globals.AF.Filters.Add(filterEntry);
            _globals.AF.Filters.Serialize();
            _viewer.RebuildList();
        }

        /// <summary>
        /// Reads the selected filter. Behavior preserved from the original
        /// <c>BtnDelete_Click</c>: no deletion side effect.
        /// </summary>
        public void DeleteSelected()
        {
            _ = _viewer.SelectedFilter;
        }
    }
}
