using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// A small, immutable value type identifying a store by its stable resolved key. Store identity
    /// is the key by which a store is disabled, tested for disablement, and reenabled (issue #261,
    /// epic #260). Created only through the <see cref="Resolve(string, string)"/> factory so callers
    /// cannot fabricate an identity from an unresolved input. Equality for storage and lookup is
    /// performed case-insensitively by the collections that hold identities; the resolved
    /// <see cref="Value"/> preserves original casing.
    /// </summary>
    /// <remarks>
    /// Declared as a plain <c>readonly struct</c> with a private constructor and a get-only
    /// auto-property rather than a <c>record struct</c> or a type with an <c>init</c> accessor,
    /// because <c>init</c> accessors require <c>System.Runtime.CompilerServices.IsExternalInit</c>,
    /// which is not available on this .NET Framework 4.8 target (CS0518). Mirrors the
    /// <c>ResourceTimingRow</c> pattern in
    /// <c>UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs</c>.
    /// </remarks>
    public readonly struct StoreIdentity
    {
        /// <summary>
        /// Documented sentinel returned by <see cref="Resolve(string, string)"/> when neither a
        /// DisplayName nor a FilePath fallback is available. It is deliberately NOT
        /// <see cref="string.Empty"/> (which existing exclusion-list code treats as a benign no-op
        /// token via <c>IsNullOrWhiteSpace</c> guards). The embedded NUL characters cannot appear in
        /// a real Outlook DisplayName or file-system path, so the sentinel can never equal a
        /// well-formed identity. This is fail-safe: an unresolvable store is never accidentally
        /// disabled and never accidentally reenabled by a stray match.
        /// </summary>
        public const string UnresolvedSentinel = "\0__UNRESOLVED_STORE_IDENTITY__\0";

        private StoreIdentity(string value)
        {
            Value = value;
        }

        /// <summary>
        /// The resolved identity string (original casing preserved). Equals
        /// <see cref="UnresolvedSentinel"/> when the store could not be resolved.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Resolves a stable store identity from already-cached primitives. Performs no COM access
        /// and no I/O; safe to call from any thread, including a background monitor.
        /// </summary>
        /// <param name="displayName">
        /// The store DisplayName (the persisted key on <c>StoreWrapper</c>). Primary source.
        /// </param>
        /// <param name="filePathFallback">
        /// Optional fallback used only when <paramref name="displayName"/> is null/whitespace.
        /// Callers that do not already hold a cheap FilePath pass null.
        /// </param>
        /// <returns>
        /// A <see cref="StoreIdentity"/> whose <see cref="Value"/> is
        /// <paramref name="displayName"/> when non-null/non-whitespace; otherwise
        /// <paramref name="filePathFallback"/> when non-null/non-whitespace; otherwise
        /// <see cref="UnresolvedSentinel"/>.
        /// </returns>
        public static StoreIdentity Resolve(string displayName, string filePathFallback = null)
        {
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return new StoreIdentity(displayName);
            }

            if (!string.IsNullOrWhiteSpace(filePathFallback))
            {
                return new StoreIdentity(filePathFallback);
            }

            return new StoreIdentity(UnresolvedSentinel);
        }

        /// <summary>
        /// Convenience overload for filter-time call sites that already read DisplayName and FilePath
        /// from a live <see cref="Outlook.Store"/> in the same pass. Reads <c>store.DisplayName</c>
        /// and a guarded <c>store.FilePath</c> (mirroring the existing try/catch in
        /// <c>StoresWrapper.ShouldIncludeStore</c>) and forwards to the pure
        /// <see cref="Resolve(string, string)"/> overload. Reserved for filter-time call sites only;
        /// F3/F4/F5 use the pure string overload because a locked-up store's FilePath read is the
        /// blocking COM call the epic prohibits during detection and attribution.
        /// </summary>
        /// <param name="store">The live Outlook store to resolve an identity for.</param>
        /// <returns>The resolved <see cref="StoreIdentity"/> (see the pure overload's contract).</returns>
        public static StoreIdentity Resolve(Outlook.Store store)
        {
            string displayName = null;
            try
            {
                displayName = store.DisplayName;
            }
            catch { }

            string filePath = null;
            try
            {
                filePath = store.FilePath;
            }
            catch { }

            return Resolve(displayName, filePath);
        }
    }
}
