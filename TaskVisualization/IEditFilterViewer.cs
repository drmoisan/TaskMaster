using System;
using UtilitiesCS.Interfaces.IWinForm;

namespace TaskVisualization
{
    /// <summary>
    /// Behavioral facade over the <see cref="EditFilterViewer"/> form that
    /// <see cref="EditFilterController"/> reads from and writes to. Derives from
    /// <see cref="IForm"/> so the Form-level surface the controller uses
    /// (<c>Text</c>, <c>Show()</c>, <c>Hide()</c>, <c>Dispose()</c>,
    /// <c>ShowDialog()</c>, <c>Close()</c>, <c>DialogResult</c>) resolves through
    /// the base interface chain; only the members that are additive over
    /// <see cref="IForm"/> are declared here.
    /// </summary>
    /// <remarks>
    /// Primitives-only: no <c>System.Windows.Forms</c> control types appear in any
    /// member signature, so a Moq mock satisfies the interface without
    /// instantiating a <c>Control</c>. The concrete form binds these to its live
    /// controls; <see cref="EditFilterController"/> depends only on this interface.
    /// </remarks>
    public interface IEditFilterViewer : IForm
    {
        /// <summary>Context selection text (backed by the ContextSelection label).</summary>
        string ContextSelectionText { get; set; }

        /// <summary>People selection text (backed by the PeopleSelection label).</summary>
        string PeopleSelectionText { get; set; }

        /// <summary>Project selection text (backed by the ProjectSelection label).</summary>
        string ProjectSelectionText { get; set; }

        /// <summary>Topic selection text (backed by the TopicSelection label).</summary>
        string TopicSelectionText { get; set; }

        /// <summary>Filter name text (backed by the FilterName text box).</summary>
        string FilterNameText { get; set; }

        /// <summary>Raised when the context selection control is clicked.</summary>
        event EventHandler ContextSelectionClick;

        /// <summary>Raised when the people selection control is clicked.</summary>
        event EventHandler PeopleSelectionClick;

        /// <summary>Raised when the project selection control is clicked.</summary>
        event EventHandler ProjectSelectionClick;

        /// <summary>Raised when the topic selection control is clicked.</summary>
        event EventHandler TopicSelectionClick;

        /// <summary>Raised when the folders-selected control is clicked.</summary>
        event EventHandler FoldersSelectedClick;

        /// <summary>Raised when the OK button is clicked.</summary>
        event EventHandler OkClick;

        /// <summary>Raised when the Cancel button is clicked.</summary>
        event EventHandler CancelClick;

        /// <summary>Toggles every tip label to the Off state (initial view state).</summary>
        void ResetTips();
    }
}
