#nullable enable
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Issue #438: the popup-open entry points, carrying the explicit <c>takeFocus</c> intent.
    /// <para>
    /// Held on a second partial-class part so <c>BreadcrumbDropDownHost.cs</c> (480 lines) stays
    /// clear of the repository's 500-line ceiling.
    /// </para>
    /// </summary>
    public sealed partial class BreadcrumbDropDownHost
    {
        /// <inheritdoc />
        public Task<bool> OpenAsync(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize
        ) => OpenWithFocusIntentAsync(anchorScreenBounds, workingArea, desiredSize, true);

        /// <inheritdoc />
        /// <remarks>
        /// Implemented explicitly so the concrete host keeps exactly one public <c>OpenAsync</c>.
        /// The focus intent is a pipeline contract between the open coordinator and the host, and
        /// every consumer reaches it through <see cref="IBreadcrumbDropDownHost"/>, so exposing a
        /// second public overload on the concrete type would widen the public surface for no caller
        /// (`.claude/rules/csharp.md`: keep the public surface intentional and minimal).
        /// </remarks>
        Task<bool> IBreadcrumbDropDownHost.OpenAsync(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize,
            bool takeFocus
        ) => OpenWithFocusIntentAsync(anchorScreenBounds, workingArea, desiredSize, takeFocus);

        /// <summary>
        /// Opens the popup, honoring the caller's focus intent.
        /// </summary>
        /// <param name="anchorScreenBounds">The collapsed anchor's screen rectangle.</param>
        /// <param name="workingArea">The working area the popup must be placed within.</param>
        /// <param name="desiredSize">The requested popup size.</param>
        /// <param name="takeFocus">False to open without moving focus onto the popup surface.</param>
        /// <returns>True once the popup is open for this request.</returns>
        /// <remarks>
        /// Relocated from the original 3-parameter <c>OpenAsync</c> body. The only behavioral
        /// addition is the <paramref name="takeFocus"/> guard on the already-open branch; the
        /// disposal check, the open-result contract, and the
        /// <c>LastInitializationException = null</c> reset are unchanged.
        /// </remarks>
        private Task<bool> OpenWithFocusIntentAsync(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize,
            bool takeFocus
        )
        {
            ThrowIfDisposed();
            if (OpenState)
            {
                // An open request on an already-open popup is defined as "focus the popup again".
                // A search-driven refresh must not do that, or every keystroke would pull the caret
                // out of the search textbox (issue #438 AC-2). The open result is unchanged.
                // Issue #677: scheduling FocusPending rather than the raw _focusPending delegate
                // moves the focus-permission check inside the scheduled action, so the predicate is
                // read when the refocus executes rather than when it is queued.
                if (takeFocus)
                {
                    // Issue #680: a takeFocus: true reopen on a popup that was shown non-capturing
                    // is the Down-arrow handoff. Standard popup semantics resume there, so the
                    // AutoClose default is restored before focus moves onto the popup surface
                    // (spec Proposed Fix item 2a).
                    // Issue #677: scheduling FocusPending() rather than the raw _focusPending
                    // delegate moves the focus-permission check inside the scheduled action, so
                    // the predicate is read when the refocus executes rather than when it is
                    // queued.
                    _openLifetime.Schedule(() =>
                    {
                        DropDown.AutoClose = true;
                        FocusPending();
                    });
                }
                return Task.FromResult(true);
            }
            LastInitializationException = null;
            return _openLifetime.OpenAsync(anchorScreenBounds, workingArea, desiredSize, takeFocus);
        }

        /// <summary>
        /// Issue #796 (AC3): whether a selection commit has been requested for the popup lifetime
        /// that is currently open, so a close arriving with an <c>Uncommitted</c> reason must not
        /// cancel the selection the commit is in the middle of making.
        /// </summary>
        /// <remarks>
        /// A settable internal property rather than a constructor parameter, matching the issue #677
        /// may-take-focus precedent, so every constructor keeps its baseline arity and the
        /// reflection-based constructor binding the existing tests rely on is not disturbed. It is
        /// declared on this part rather than on the main part because the main part stands close to
        /// the repository's 500-line ceiling and this one does not.
        /// <para>
        /// The lifetime is one popup opening: <see cref="ShowPopup"/> clears it as each fresh native
        /// show begins, and nothing else clears it. That is deliberate — once a commit has been
        /// requested for a given open popup, every later uncommitted-reason close of that same popup
        /// is a close racing the commit, whichever order the two arrive in.
        /// </para>
        /// </remarks>
        internal bool IsCommitPending { get; set; }

        // Issue #680: AutoClose == false is the WinForms framework's own opt-out from
        // ModalMenuFilter menu-mode entry. Menu mode retargets every keystroke to the popup's window
        // handle whenever the popup does not contain focus, which is exactly the state a
        // search-driven (takeFocus: false) open produces — so typing a second character never
        // reaches the search textbox. The write must precede the show call because menu mode is
        // entered inside ToolStripDropDown.SetVisibleCore(true); placing it here guarantees that
        // ordering by statement order.
        internal void ShowPopup(Point location, bool takeFocus)
        {
            // Issue #796 (AC3): a fresh native show starts a new popup lifetime, which by definition
            // has no commit in flight yet. Clearing here rather than at a close keeps the latch's
            // meaning tied to the popup that is open, not to whichever close happened to run last.
            IsCommitPending = false;
            DropDown.AutoClose = takeFocus;
            _showPopup(DropDown, Anchor, location);
        }

        internal void PublishPopupMessengerReady() =>
            PopupMessengerReady?.Invoke(this, EventArgs.Empty);
    }
}
