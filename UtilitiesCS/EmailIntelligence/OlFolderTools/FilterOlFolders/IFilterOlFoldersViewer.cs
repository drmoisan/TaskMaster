using System;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace UtilitiesCS
{
    /// <summary>
    /// Abstracts the WinForms viewer used by <see cref="FilterOlFoldersController"/>
    /// so that the controller can be unit-tested without opening a real window.
    /// </summary>
    public interface IFilterOlFoldersViewer : IDisposable
    {
        /// <summary>Tree list view that shows folders not currently filtered.</summary>
        TreeListView TlvNotFiltered { get; }

        /// <summary>Tree list view that shows folders currently selected for filtering.</summary>
        TreeListView TlvFiltered { get; }

        /// <summary>
        /// Gets a value indicating whether the caller must use Invoke to marshal
        /// a call to the viewer's thread.
        /// </summary>
        bool InvokeRequired { get; }

        /// <summary>Registers the controller and configures the viewer's tree controls.</summary>
        void SetController(FilterOlFoldersController controller);

        /// <summary>Makes the viewer window visible.</summary>
        void Show();

        /// <summary>Closes the viewer window.</summary>
        void Close();

        /// <summary>
        /// Executes the delegate on the thread that owns the viewer's underlying
        /// window handle.
        /// </summary>
        object Invoke(Delegate method);
    }
}
