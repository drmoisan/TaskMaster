#nullable enable

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Issue #438: the fresh-open focus step, made conditional on the caller's focus intent.
    /// <para>
    /// Held on a second partial-class part so <c>BreadcrumbDropDownOpenLifetime.cs</c> (477 lines)
    /// stays clear of the repository's 500-line ceiling.
    /// </para>
    /// </summary>
    internal sealed partial class BreadcrumbDropDownOpenLifetime
    {
        /// <summary>
        /// Completes a fresh open by optionally moving focus onto the popup surface and clearing the
        /// retained initialization failure.
        /// </summary>
        /// <param name="lease">The open generation this step belongs to.</param>
        /// <param name="takeFocus">
        /// <see langword="false"/> for a search-driven open, which must leave the caret in the
        /// search textbox; <see langword="true"/> for every explicit gesture.
        /// </param>
        /// <returns>True when the lease is still current and the popup is still open.</returns>
        /// <remarks>
        /// Relocated verbatim from <c>BreadcrumbDropDownOpenLifetime.cs</c> with one added guard:
        /// the <c>_host.FocusPending()</c> call is skipped when <paramref name="takeFocus"/> is
        /// false. Every other step — the two lease-currency checks, the <c>OpenState</c> precondition,
        /// the return contract, and the <c>LastInitializationException = null</c> step — is unchanged,
        /// so a non-focusing open still reports the same open result as a focusing one
        /// (issue #438 AC-2; issue #400 AC-13 remains in force for gesture opens).
        /// </remarks>
        private bool FocusCurrentSurface(BreadcrumbDropDownOpenLease lease, bool takeFocus) =>
            RunIfCurrent(
                lease,
                () =>
                {
                    if (!_host.OpenState)
                        return false;
                    if (takeFocus)
                        _host.FocusPending();
                    return IsCurrent(lease) && _host.OpenState;
                }
            )
            && RunIfCurrent(
                lease,
                () =>
                {
                    _host.LastInitializationException = null;
                    return true;
                }
            );
    }
}
