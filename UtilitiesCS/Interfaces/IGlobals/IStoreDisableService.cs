using System.Collections.Generic;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS
{
    /// <summary>The persistence scope of a disabled-store entry.</summary>
    public enum DisableScope
    {
        /// <summary>Disabled for the current process only; never persisted.</summary>
        SessionOnly,

        /// <summary>Disabled for the current and all future sessions; persisted.</summary>
        FutureSessions,
    }

    /// <summary>
    /// A disabled store's identity paired with the scope under which it is disabled.
    /// </summary>
    /// <remarks>
    /// Declared as a plain <c>readonly struct</c> with an ordinary constructor and get-only
    /// (<c>{ get; }</c>) properties rather than a <c>record struct</c> or a type with an
    /// <c>init</c> accessor, because <c>init</c> accessors require
    /// <c>System.Runtime.CompilerServices.IsExternalInit</c>, which is not available on this .NET
    /// Framework 4.8 target (CS0518). Mirrors the <c>ResourceTimingRow</c> pattern in
    /// <c>UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs</c>. Constructed via its constructor
    /// (no object initializer).
    /// </remarks>
    public readonly struct DisabledStoreEntry
    {
        /// <summary>Creates a disabled-store entry from an identity and its scope.</summary>
        /// <param name="identity">The resolved identity of the disabled store.</param>
        /// <param name="scope">The scope under which the store is disabled.</param>
        public DisabledStoreEntry(StoreIdentity identity, DisableScope scope)
        {
            Identity = identity;
            Scope = scope;
        }

        /// <summary>The resolved identity of the disabled store.</summary>
        public StoreIdentity Identity { get; }

        /// <summary>The scope under which the store is disabled.</summary>
        public DisableScope Scope { get; }
    }

    /// <summary>
    /// Orchestrates disabling, reenabling, and querying disabled stores (issue #261, epic #260).
    /// Exposed on <see cref="IApplicationGlobals"/> as the read-only member <c>StoreDisable</c>. It is
    /// a thin layer over the disabled-scope collections on <c>StoresWrapper</c> (the single source of
    /// truth) and reads that model lazily per call. F4/F5 call this service only; they do not call F3
    /// directly.
    /// </summary>
    public interface IStoreDisableService
    {
        /// <summary>
        /// Disables the store for the current session only. Adds the identity to the in-memory
        /// session set. Never persists. Idempotent: disabling an already-session-disabled identity
        /// is a no-op. Throws <see cref="System.ArgumentException"/> if the identity is
        /// unresolved/empty.
        /// </summary>
        /// <param name="identity">The identity of the store to disable for this session.</param>
        void DisableSessionOnly(StoreIdentity identity);

        /// <summary>
        /// Disables the store for the current and future sessions. Adds the identity to the persisted
        /// list and persists via <c>Model.Serialize()</c>. Because filtering unions both scopes, this
        /// also disables the store for the remainder of the current session with no session-set
        /// write. Idempotent: if the identity is already in the persisted list, does not append a
        /// duplicate and does not call <c>Serialize()</c> again. Throws
        /// <see cref="System.ArgumentException"/> if the identity is unresolved/empty.
        /// </summary>
        /// <param name="identity">The identity of the store to disable persistently.</param>
        void DisableForFutureSessions(StoreIdentity identity);

        /// <summary>
        /// Reenables the store by clearing it from BOTH scopes, persisting when the persisted list
        /// changed, then awaiting the injected rehook collaborator (a no-op in wave 0; F3 supplies
        /// the real <see cref="IStoreRehookService"/>). Idempotent: reenabling a non-disabled identity
        /// changes no collection, calls neither <c>Serialize()</c> nor a state mutation, and still
        /// awaits the collaborator. Throws <see cref="System.ArgumentException"/> if the identity is
        /// unresolved/empty.
        /// </summary>
        /// <param name="identity">The identity of the store to reenable.</param>
        /// <returns>A task that completes after state is cleared and the rehook collaborator awaited.</returns>
        Task ReenableAsync(StoreIdentity identity);

        /// <summary>
        /// Returns true when the identity is present in either scope (case-insensitive). Read-only;
        /// never mutates and never persists. Returns false when the store model is not yet populated.
        /// </summary>
        /// <param name="identity">The identity to test for disablement.</param>
        /// <returns>True when disabled in either scope; otherwise false.</returns>
        bool IsDisabled(StoreIdentity identity);

        /// <summary>
        /// Returns all currently disabled stores as identity+scope entries. An identity present in
        /// both scopes is reported once with <see cref="DisableScope.FutureSessions"/> (the stronger,
        /// persisted scope). Returns an empty collection (never null) when the store model is not yet
        /// populated.
        /// </summary>
        /// <returns>The disabled stores as identity+scope entries; empty when the model is null.</returns>
        IReadOnlyCollection<DisabledStoreEntry> GetDisabledStores();
    }
}
