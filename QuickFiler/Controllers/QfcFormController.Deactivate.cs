using System;
using System.Collections.Generic;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Issue #677: the <c>Form.Deactivate</c>-routed focus-parking and selector-cancel handler.
    /// <para>
    /// Two things must be true the moment activation leaves the QuickFiler form. First, no WebView2
    /// child window may keep the shared Outlook UI thread's Win32 keyboard focus, or every
    /// keystroke the user types into a native Outlook window is silently consumed by that browser
    /// surface (MicrosoftEdge/WebView2Feedback #951). Second, no breadcrumb <c>ToolStripDropDown</c>
    /// may stay open, or WinForms modal menu mode keeps redirecting thread keyboard messages to the
    /// popup after the user has left.
    /// </para>
    /// </summary>
    internal partial class QfcFormController
    {
        /// <summary>
        /// Parks focus off any focused WebView2 and cancels every item's breadcrumb selector.
        /// </summary>
        /// <remarks>
        /// No <c>_formViewer</c> null guard is written: this handler is reachable only through
        /// <c>_formViewer.FormDeactivated</c>, so a null-viewer branch would be unreachable code.
        /// </remarks>
        internal void FormViewer_Deactivated(object sender, EventArgs e)
        {
            if (_formViewer.IsWebView2Focused)
            {
                _formViewer.ParkFocusOffWebView2();
            }

            List<QfcItemGroup> groups = _groups?.ItemGroups;
            if (groups == null)
            {
                return;
            }

            foreach (QfcItemGroup group in groups)
            {
                try
                {
                    group.ItemController?.CancelBreadcrumbSelector();
                }
                catch (Exception exception)
                {
                    // Deliberate per-item boundary catch. This is a WinForms event handler, so an
                    // escaping exception surfaces as an unhandled UI-thread failure inside Outlook;
                    // and catching per item rather than around the loop guarantees the remaining
                    // items are still cancelled when one of them fails.
                    logger.Error(
                        "Issue #677: cancelling a breadcrumb selector on form deactivation failed. "
                            + "Remaining items are still cancelled.",
                        exception
                    );
                }
            }
        }
    }
}
