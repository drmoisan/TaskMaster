using System;

namespace TaskMaster
{
    /// <summary>
    /// Decides which ribbon controls must be invalidated once engine initialization completes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Office caches each <c>getEnabled</c> response per control until the add-in invalidates it,
    /// so without an explicit invalidation the engine-backed buttons would remain disabled for the
    /// whole session even after <c>InitAsync()</c> succeeds. This type owns the decision of
    /// <em>which</em> controls to invalidate; the COM call itself
    /// (<c>IRibbonUI.InvalidateControl</c>) stays behind the injected delegate in the
    /// coverage-exempt ribbon shim.
    /// </para>
    /// <para>
    /// The invalidation set is derived from <see cref="EngineCommandCatalog.ControlIds"/> rather
    /// than duplicated as a literal list, so the catalog remains the single source of truth.
    /// </para>
    /// <para>
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
    /// decision logic with no COM and no <c>Microsoft.Office.*</c> reference, and is fully
    /// unit-tested.
    /// </para>
    /// </remarks>
    internal static class EngineCommandRefreshPlanner
    {
        /// <summary>
        /// Requests invalidation of every engine-backed ribbon control exactly once.
        /// </summary>
        /// <param name="invalidateControl">
        /// Invoked once per <see cref="EngineCommandCatalog.ControlIds"/> entry with that control
        /// id. Must not be null.
        /// </param>
        /// <remarks>
        /// Office documents callback ordering as unspecified, so no ordering guarantee is made or
        /// required; only the set of invalidated ids is meaningful. The operation is idempotent —
        /// invalidating an already-invalidated control is harmless — so a second refresh after
        /// <c>RestartEngineAsync</c> is safe.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="invalidateControl"/> is null.
        /// </exception>
        internal static void InvalidateAll(Action<string> invalidateControl)
        {
            if (invalidateControl is null)
            {
                throw new ArgumentNullException(nameof(invalidateControl));
            }

            foreach (var controlId in EngineCommandCatalog.ControlIds)
            {
                invalidateControl(controlId);
            }
        }
    }
}
