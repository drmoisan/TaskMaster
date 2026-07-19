using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

#nullable enable

namespace UtilitiesCS.Dialogs
{
    /// <summary>
    /// Internal, non-<c>using</c>-scoped modeless composition of the three-button store-lockup
    /// notification (issue #264, epic #260). Kept as a sibling helper (not a method on
    /// <see cref="MyBox"/>) so <see cref="MyBox"/> stays cohesive and under the file-size cap; it
    /// lives in the <c>UtilitiesCS</c> assembly because the button-wiring helper
    /// <see cref="MyBox.ReplaceButtons(MyBoxViewer, IList{ActionButton})"/> is <c>internal</c>. The
    /// existing modal <c>ShowDialog</c>/<c>DialogInvoker</c> overloads are not modified. The viewer
    /// is constructed directly (no <c>using</c> block), owns its own lifetime through a
    /// <c>FormClosed</c> handler, and is shown through an injectable <c>showAction</c> defaulting to
    /// <c>viewer =&gt; viewer.Show()</c>, mirroring <c>EfcHomeController.ViewerShowAction</c>.
    /// </summary>
    internal static class MyBoxModeless
    {
        /// <summary>
        /// Shows the modeless notification using the default show action (<c>viewer.Show()</c>).
        /// Signature matches <c>StoreLockupNotifier</c> so it is the responder's default notify seam.
        /// </summary>
        /// <param name="identity">The cached store identity displayed in the message.</param>
        /// <param name="disableSessionOnly">Action for the "Disable This Session Only" button.</param>
        /// <param name="disableForFutureSessions">Action for the "Disable for Future Sessions" button.</param>
        /// <param name="reenable">Action for the "Reenable" button.</param>
        /// <remarks>
        /// Host-bound WinForms wiring: this default entry point resolves the real
        /// <c>viewer.Show()</c> show action, so it cannot be unit-tested without displaying a window.
        /// It is a thin delegation to the injectable 5-argument overload (which is fully tested via a
        /// non-displaying stub), and is exempt from coverage per the CLAUDE.md WinForms exemption.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        internal static void ShowStoreLockupNotification(
            string identity,
            Action disableSessionOnly,
            Action disableForFutureSessions,
            Action reenable
        )
        {
            ShowStoreLockupNotification(
                identity,
                disableSessionOnly,
                disableForFutureSessions,
                reenable,
                showAction: null
            );
        }

        /// <summary>
        /// Shows the modeless notification through an injectable <paramref name="showAction"/>
        /// (defaulting to <c>viewer =&gt; viewer.Show()</c> when null). The viewer owns its own
        /// lifetime via a <c>FormClosed</c> handler that disposes it, so it stays on screen until the
        /// user clicks a button (no <c>using</c> block).
        /// </summary>
        /// <param name="identity">The cached store identity displayed in the message.</param>
        /// <param name="disableSessionOnly">Action for the "Disable This Session Only" button.</param>
        /// <param name="disableForFutureSessions">Action for the "Disable for Future Sessions" button.</param>
        /// <param name="reenable">Action for the "Reenable" button.</param>
        /// <param name="showAction">The show seam; null uses the default non-blocking <c>Show()</c>.</param>
        internal static void ShowStoreLockupNotification(
            string identity,
            Action disableSessionOnly,
            Action disableForFutureSessions,
            Action reenable,
            Action<MyBoxViewer>? showAction
        )
        {
            var viewer = new MyBoxViewer();
            viewer.FormClosed += (sender, e) => viewer.Dispose();

            var buttons = BuildButtons(disableSessionOnly, disableForFutureSessions, reenable);
            MyBox.ReplaceButtons(viewer, buttons);

            viewer.Text = "Mailbox not responding";
            viewer.TextMessage.Text = BuildMessage(identity);
            viewer.TopMost = true;

            var show = showAction ?? (v => v.Show());
            show(viewer);
        }

        /// <summary>
        /// Builds the three notification buttons wired to the supplied F1 actions. Exposed for unit
        /// testing so each button <see cref="ActionButton.Delegate"/> can be invoked without a real
        /// window.
        /// </summary>
        /// <param name="disableSessionOnly">Action for the "Disable This Session Only" button.</param>
        /// <param name="disableForFutureSessions">Action for the "Disable for Future Sessions" button.</param>
        /// <param name="reenable">Action for the "Reenable" button.</param>
        /// <returns>The three-button action set in display order.</returns>
        internal static IList<ActionButton> BuildButtons(
            Action disableSessionOnly,
            Action disableForFutureSessions,
            Action reenable
        )
        {
            return new List<ActionButton>
            {
                new ActionButton(
                    "StoreLockupDisableSession",
                    "Disable This Session Only",
                    DialogResult.OK,
                    disableSessionOnly
                ),
                new ActionButton(
                    "StoreLockupDisableFuture",
                    "Disable for Future Sessions",
                    DialogResult.OK,
                    disableForFutureSessions
                ),
                new ActionButton("StoreLockupReenable", "Reenable", DialogResult.Cancel, reenable),
            };
        }

        /// <summary>Builds the user-facing message text from the cached identity (no COM read).</summary>
        /// <param name="identity">The cached store identity.</param>
        /// <returns>The notification message.</returns>
        internal static string BuildMessage(string identity)
        {
            var name = string.IsNullOrEmpty(identity) ? "<null>" : identity;
            return $"The mailbox '{name}' has stopped responding and has been disabled for this session.";
        }
    }
}
