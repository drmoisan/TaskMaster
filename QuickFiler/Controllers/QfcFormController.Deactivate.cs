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
        /// Delegates to <see cref="ParkFocusAndCancelSelectors"/>, which issue #791 extracted so the
        /// Cancel path can run the same routine.
        /// </remarks>
        internal void FormViewer_Deactivated(object sender, EventArgs e) =>
            ParkFocusAndCancelSelectors(honourSelfInflictedGuard: true);

        /// <summary>
        /// Issue #796 (AC6): renders the one-line entry diagnostic for
        /// <see cref="ParkFocusAndCancelSelectors"/>.
        /// </summary>
        /// <param name="webView2Focused">Whether a WebView2 child window held focus at entry.</param>
        /// <param name="activeFormIsNull">
        /// Whether <c>Form.ActiveForm</c> was null at entry. It was proposed as a discriminator on
        /// the reasoning that a <c>ToolStripDropDown</c> is not a <c>Form</c>, so a null active form
        /// would evidence a self-inflicted deactivation. The manual observation recorded for this
        /// item refutes that reasoning: all three self-inflicted popup gestures reported
        /// <c>ActiveFormNull=False</c> and the one deactivation caused by focus leaving the form
        /// reported <c>ActiveFormNull=True</c>, so the values run opposite to the predicted
        /// direction on all four observations. The field is retained as observed diagnostic data
        /// only and is not read as evidence in either direction. See the AC2 item-viewer wiring
        /// line of evidence/other/close-ordering-decision.md.
        /// </param>
        /// <param name="groupCount">The number of item groups the cancel loop will visit.</param>
        /// <returns>A single line carrying a sentence prefix and three Key=Value pairs.</returns>
        /// <remarks>
        /// Pure and static so the AC6 evidence rests on a deterministic managed-seam assertion
        /// rather than a source-text scan.
        /// </remarks>
        internal static string FormatDeactivationDiagnostics(
            bool webView2Focused,
            bool activeFormIsNull,
            int groupCount
        ) =>
            "Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. "
            + $"WebView2Focused={webView2Focused} ActiveFormNull={activeFormIsNull} "
            + $"Groups={groupCount}";

        /// <summary>
        /// Issue #796 (AC6): renders the one-line per-item diagnostic for the cancel loop in
        /// <see cref="ParkFocusAndCancelSelectors"/>.
        /// </summary>
        /// <param name="itemNumber">The item's own number.</param>
        /// <param name="selectorWasOpen">
        /// Whether that item's breadcrumb selector was open, or null when the value could not be
        /// observed. The parameter is nullable so the unavailable case is produced here rather than
        /// at the call site, which keeps the per-item log statement a single unconditional call.
        /// </param>
        /// <returns>
        /// A single line carrying a sentence prefix and two Key=Value pairs. An unobserved
        /// selector state renders as <c>SelectorWasOpen=unavailable</c>, never as a fabricated
        /// boolean.
        /// </returns>
        internal static string FormatItemCancelDiagnostics(int itemNumber, bool? selectorWasOpen) =>
            "Issue #796: QfcFormController.ParkFocusAndCancelSelectors reached item. "
            + $"ItemNumber={itemNumber} "
            + "SelectorWasOpen="
            + (selectorWasOpen?.ToString() ?? "unavailable");

        /// <summary>
        /// Parks focus off any focused WebView2 and cancels every item's breadcrumb selector.
        /// </summary>
        /// <remarks>
        /// Issue #791 added the <c>_formViewer</c> null guard. Before the extraction this routine
        /// was reachable only through <c>_formViewer.FormDeactivated</c>, so a null-viewer branch
        /// was unreachable and none was written. The Cancel path now calls it directly, and it is
        /// reachable there with the viewer already released — a second Cancel, or a Cancel after a
        /// partially failed launch — so the guard is live code rather than defensive padding.
        /// </remarks>
        internal void ParkFocusAndCancelSelectors(bool honourSelfInflictedGuard)
        {
            logger.Debug(
                FormatDeactivationDiagnostics(
                    _formViewer?.IsWebView2Focused == true,
                    System.Windows.Forms.Form.ActiveForm == null,
                    _groups?.ItemGroups?.Count ?? 0
                )
            );
            if (_formViewer?.IsWebView2Focused == true)
            {
                _formViewer.ParkFocusOffWebView2();
            }

            List<QfcItemGroup> groups = _groups?.ItemGroups;
            if (groups == null)
            {
                return;
            }

            // Issue #796 (AC2): a deactivation this form's own breadcrumb popup caused must not
            // cancel the selector the gesture just opened. The guard is scoped to the cancel loop
            // and deliberately not to the focus-parking step above, because the observation
            // recorded for this item shows parking did not run on two of the three defective
            // gestures and so cannot be what produces the defect. A viewer that reports nothing
            // reports false, which is the genuine case, so the issue #677 contract is unchanged for
            // every deactivation that is not self-inflicted.
            // Issue #810 (AC1): the predicate is meaningful only for a deactivation, so whether to
            // honour it is a property of the caller rather than of this routine, and it is supplied
            // as an argument. The teardown path passes false, because a cancel must cancel every
            // selector whatever opened the popup that took focus.
            if (
                honourSelfInflictedGuard
                && _formViewer?.IsDeactivationSelfInflictedByOwnPopup == true
            )
            {
                return;
            }

            foreach (QfcItemGroup group in groups)
            {
                logger.Debug(
                    FormatItemCancelDiagnostics(
                        group.ItemController?.ItemNumber ?? 0,
                        (group.ItemController as QfcItemController)?.IsBreadcrumbSelectorOpen
                    )
                );
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
