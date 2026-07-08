using System;

#nullable enable

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// The distinct outcomes of a single-store runtime rehook attempt (issue #263, epic #260).
    /// Returned inside <see cref="StoreRehookResult"/> by the rehook coordinator so the caller and
    /// the logs can distinguish success, an idempotent no-op, and each failure mode without an
    /// exception ever escaping the rehook path.
    /// </summary>
    public enum StoreRehookOutcome
    {
        /// <summary>The store was newly hooked in all three subsystems.</summary>
        Success,

        /// <summary>
        /// Idempotent no-op: all three subsystems already had this store hooked, so no COM
        /// re-touch and no additional subscription were performed. A success variant.
        /// </summary>
        AlreadyHooked,

        /// <summary>
        /// The identity did not resolve to any live <c>Store</c> in the current MAPI namespace. A
        /// failure variant; the caller must leave the disabled scope set.
        /// </summary>
        StoreNotFound,

        /// <summary>
        /// The store-scoped readiness gate never reported ready within the bounded retry window. A
        /// failure variant; logged via log4net with identity and the failing subsystem.
        /// </summary>
        TransientTimeout,

        /// <summary>
        /// A non-transient exception (a non-transient <see cref="System.Runtime.InteropServices.COMException"/>
        /// or any other exception) was raised while hooking the store. A failure variant; logged via
        /// log4net with identity, failing subsystem, and HRESULT when COM-derived.
        /// </summary>
        PermanentError,
    }

    /// <summary>
    /// The structured result of a single-store runtime rehook attempt (issue #263, epic #260).
    /// Carries the <see cref="Outcome"/>, the resolved <see cref="StoreId"/> and originating
    /// <see cref="Identity"/> for logging, and the causing <see cref="Error"/> when applicable.
    /// This is the rehook coordinator's internal result type: F1's shipped
    /// <c>IStoreRehookService.RehookAsync(StoreIdentity)</c> returns a bare <see cref="System.Threading.Tasks.Task"/>
    /// (no outcome value), so this type is not the interface return type; the coordinator's public
    /// adapter logs the outcome rather than propagating it to F1.
    /// </summary>
    /// <remarks>
    /// Declared as a <c>sealed record</c> with get-only properties initialized through the
    /// constructor (not <c>init</c> accessors), mirroring the <c>FolderHierarchyNode</c> precedent,
    /// because <c>init</c> accessors require <c>System.Runtime.CompilerServices.IsExternalInit</c>,
    /// which is not available on this .NET Framework 4.8 target (CS0518).
    /// </remarks>
    public sealed record StoreRehookResult
    {
        /// <summary>The outcome of the rehook attempt.</summary>
        public StoreRehookOutcome Outcome { get; }

        /// <summary>
        /// The resolved Outlook <c>StoreID</c> when the identity resolved to a live store; null
        /// when the store could not be resolved (<see cref="StoreRehookOutcome.StoreNotFound"/>).
        /// </summary>
        public string? StoreId { get; }

        /// <summary>The originating store identity string (F1's DisplayName-primary key). Never null.</summary>
        public string Identity { get; }

        /// <summary>
        /// The causing exception for <see cref="StoreRehookOutcome.PermanentError"/>; null for all
        /// other outcomes.
        /// </summary>
        public Exception? Error { get; }

        /// <summary>
        /// Initializes a new <see cref="StoreRehookResult"/>.
        /// </summary>
        /// <param name="outcome">The rehook outcome.</param>
        /// <param name="identity">The originating store identity string; must not be null.</param>
        /// <param name="storeId">The resolved StoreID, or null when the store did not resolve.</param>
        /// <param name="error">The causing exception for a permanent error, or null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/> is null.</exception>
        public StoreRehookResult(
            StoreRehookOutcome outcome,
            string identity,
            string? storeId = null,
            Exception? error = null
        )
        {
            Outcome = outcome;
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            StoreId = storeId;
            Error = error;
        }
    }
}
