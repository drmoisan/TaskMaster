using System;
using System.Collections.Generic;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;

namespace TaskVisualization
{
    /// <summary>
    /// Behavioral facade over the <see cref="ManageFilters"/> form used by
    /// <see cref="ManageFiltersController"/>. Derives from <see cref="IForm"/> so
    /// the Form-level surface (including <c>Show()</c>) resolves through the base
    /// chain; only members additive over <see cref="IForm"/> are declared here.
    /// </summary>
    /// <remarks>
    /// References only <see cref="FilterEntry"/>, <see cref="IEnumerable{T}"/>, and
    /// <see cref="EventHandler"/> — no <c>System.Windows.Forms</c> control types —
    /// so a Moq mock satisfies the interface without instantiating any list control.
    /// </remarks>
    public interface IManageFiltersViewer : IForm
    {
        /// <summary>The currently selected filter row in the list view.</summary>
        FilterEntry SelectedFilter { get; }

        /// <summary>Binds the supplied filters as the list view's objects.</summary>
        void SetFilters(IEnumerable<FilterEntry> filters);

        /// <summary>Rebuilds the list view from its current object source.</summary>
        void RebuildList();

        /// <summary>Raised when the Add Filter button is clicked.</summary>
        event EventHandler AddFilterClick;

        /// <summary>Raised when the Edit Filter button is clicked.</summary>
        event EventHandler EditFilterClick;

        /// <summary>Raised when the Delete button is clicked.</summary>
        event EventHandler DeleteClick;
    }
}
