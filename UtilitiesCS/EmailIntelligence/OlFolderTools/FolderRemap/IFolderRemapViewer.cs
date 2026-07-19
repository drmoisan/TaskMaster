#nullable enable
using System;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace UtilitiesCS.EmailIntelligence.FolderRemap
{
    /// <summary>
    /// Abstracts the WinForms viewer used by <see cref="FolderRemapController"/>
    /// so that the controller can be unit-tested without opening a real window.
    /// </summary>
    public interface IFolderRemapViewer : IDisposable
    {
        /// <summary>Tree list view that shows the original folder hierarchy.</summary>
        TreeListView TlvOriginal { get; }

        /// <summary>Fast object list view that shows the current remap entries.</summary>
        FastObjectListView OlvMap { get; }

        /// <summary>
        /// Gets a value indicating whether the caller must use Invoke to marshal
        /// a call to the viewer's thread.
        /// </summary>
        bool InvokeRequired { get; }

        /// <summary>Registers the controller and configures the viewer's tree controls.</summary>
        void SetController(FolderRemapController controller);

        /// <summary>Makes the viewer window visible.</summary>
        void Show();

        /// <summary>Closes the viewer window.</summary>
        void Close();

        /// <summary>Forces the viewer to repaint.</summary>
        void Refresh();

        /// <summary>
        /// Executes the delegate on the thread that owns the viewer's underlying
        /// window handle.
        /// </summary>
        object Invoke(Delegate method);
    }
}
