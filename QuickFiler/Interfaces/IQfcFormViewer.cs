using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;

namespace QuickFiler
{
    public interface IQfcFormViewer : IForm
    {
        List<Control> Buttons { get; }
        List<Control> Panels { get; }
        TaskScheduler UiScheduler { get; }
        SynchronizationContext UiSyncContext { get; }
        System.ComponentModel.BackgroundWorker Worker { get; }

        void SetController(IFilerFormController controller);
        void SetKeyboardHandler(IQfcKeyboardHandler keyboardHandler);

        // Item layout — setter removed by Seam C (swap performed via SwapItemTableLayout)
        TableLayoutPanel L1v0L2L3v_TableLayout { get; }
        TableLayoutPanel L1v_TableLayout { get; }
        Panel L1v0L2_PanelMain { get; }

        // Seam C — TLP swap intent method
        void SwapItemTableLayout(TableLayoutPanel newTlp);

        // Seam D — item-viewer template snapshot intents (replaces the raw template properties)
        TlpCellStates CaptureTlpCellStates();
        IReadOnlyList<Control> GetKeyEventExclusionControls();
        Padding ItemViewerTemplateMargin { get; }

        // Seam B — intent command events (replaces the four raw Button properties)
        event EventHandler OkClicked;
        event EventHandler CancelClicked;
        event EventHandler UndoClicked;
        event EventHandler SkipClicked;

        // Seam B — skip button state
        string SkipButtonText { get; set; }
        bool SkipButtonEnabled { get; set; }

        // Seam B — items-per-load spinner state/event (replaces the NumericUpDown property)
        decimal ItemsPerLoadValue { get; set; }
        event EventHandler ItemsPerLoadValueChanged;
        bool ItemsPerLoadEnabled { get; set; }

        // Seam B — issue #677 deactivation intents.

        /// <summary>
        /// Raised when the form loses activation. Mirrors <see cref="Form.Deactivate"/> and carries
        /// no additional state; the controller decides what to do about the departure.
        /// </summary>
        event EventHandler FormDeactivated;

        /// <summary>
        /// Whether the leaf control currently active within this form is a WebView2 surface.
        /// Reported so the controller can park focus off it before activation leaves the process's
        /// shared UI thread (MicrosoftEdge/WebView2Feedback #951).
        /// </summary>
        bool IsWebView2Focused { get; }

        /// <summary>
        /// Moves WinForms focus onto a benign non-WebView2 control of this form, so no WebView2
        /// child window is left holding the Outlook UI thread's Win32 keyboard focus.
        /// </summary>
        void ParkFocusOffWebView2();
    }
}
