#nullable enable
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace UtilitiesCS.Threading
{
    /// <summary>
    /// The narrow surface <see cref="UiThread"/> uses to capture the UI thread's synchronization
    /// context, auto-scale factor, dispatcher and managed thread id during initialization.
    /// </summary>
    /// <remarks>
    /// The interface exists so initialization can be driven by a test double instead of a live
    /// WinForms form. It declares exactly the members <c>UiThread.Initialize()</c> consumes and
    /// nothing more; <c>SyncContextForm</c> satisfies it without gaining a member, because the four
    /// capture properties and <see cref="CaptureUiVariables"/> are already declared on it and the
    /// remaining four members are inherited from <see cref="Form"/>.
    /// </remarks>
    internal interface IUiCaptureSource
    {
        /// <summary>Gets or sets whether the capture object appears in the taskbar.</summary>
        bool ShowInTaskbar { get; set; }

        /// <summary>Gets or sets the capture object's window state.</summary>
        FormWindowState WindowState { get; set; }

        /// <summary>Displays the capture object, which is what realizes its UI context.</summary>
        void Show();

        /// <summary>Hides the capture object once its values have been read.</summary>
        void Hide();

        /// <summary>
        /// Reads the four capture values from the calling thread into the properties below.
        /// </summary>
        void CaptureUiVariables();

        /// <summary>Gets the synchronization context captured by <see cref="CaptureUiVariables"/>.</summary>
        SynchronizationContext UiSyncContext { get; }

        /// <summary>Gets the auto-scale factor captured by <see cref="CaptureUiVariables"/>.</summary>
        System.Drawing.SizeF FormAutoScaleFactor { get; }

        /// <summary>Gets the dispatcher captured by <see cref="CaptureUiVariables"/>.</summary>
        Dispatcher UiDispatcher { get; }

        /// <summary>Gets the managed thread id captured by <see cref="CaptureUiVariables"/>.</summary>
        int UiThreadId { get; }
    }
}
