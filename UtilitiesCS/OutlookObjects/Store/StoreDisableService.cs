using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Default <see cref="IStoreDisableService"/> implementation (issue #261, epic #260). A thin
    /// orchestration layer over the disabled-scope collections on <see cref="StoresWrapper"/> (the
    /// single source of truth). Mirrors <c>StoreWrapperController</c>: it takes the aggregate and
    /// reads <c>Globals.Ol.StoresWrapper</c> per call, never caching it, so it can be constructed in
    /// <c>LoadBasicMethod()</c> before the store model is populated by the later async load phase.
    /// </summary>
    public sealed class StoreDisableService : IStoreDisableService
    {
        private readonly IApplicationGlobals _globals;
        private readonly IStoreRehookService _rehook;

        /// <summary>
        /// Creates the service over the application aggregate. The rehook collaborator is the sole
        /// F1&#8596;F3 seam; when none is supplied it defaults to the wave-0
        /// <see cref="NoOpStoreRehookService"/>, so F1 ships without any forward dependency on F3.
        /// </summary>
        /// <param name="globals">The application aggregate; the store model is read from it lazily per call.</param>
        /// <param name="rehook">The rehook collaborator; defaults to a no-op when null.</param>
        public StoreDisableService(IApplicationGlobals globals, IStoreRehookService rehook = null)
        {
            _globals = globals;
            _rehook = rehook ?? new NoOpStoreRehookService();
        }

        /// <summary>
        /// Reads the store model from the aggregate. Never cached: read per call so the service can
        /// be constructed before the async store-load phase populates the model. Returns null when
        /// the aggregate, its Outlook objects, or the store model are not yet available.
        /// </summary>
        private StoresWrapper GetModelOrNull() => _globals?.Ol?.StoresWrapper;

        /// <summary>
        /// Throws <see cref="ArgumentException"/> when the identity is unresolved (equals the
        /// documented sentinel) or null/whitespace. Used by the three write methods; reads do not
        /// validate.
        /// </summary>
        private static void ValidateIdentity(StoreIdentity identity)
        {
            var value = identity.Value;
            if (
                string.IsNullOrWhiteSpace(value)
                || string.Equals(value, StoreIdentity.UnresolvedSentinel, StringComparison.Ordinal)
            )
            {
                throw new ArgumentException(
                    "Store identity is unresolved or empty and cannot be disabled or reenabled.",
                    nameof(identity)
                );
            }
        }

        /// <summary>
        /// Fails fast when a write is attempted before the store model is available. A write cannot
        /// record persistable state on a null model.
        /// </summary>
        private StoresWrapper GetModelForWriteOrThrow()
        {
            var model = GetModelOrNull();
            if (model is null)
            {
                throw new InvalidOperationException(
                    "The store model is not yet available; a disable/reenable write cannot be recorded."
                );
            }

            return model;
        }

        /// <inheritdoc/>
        public void DisableSessionOnly(StoreIdentity identity)
        {
            ValidateIdentity(identity);
            var model = GetModelForWriteOrThrow();

            // HashSet.Add is idempotent: a second call for the same identity is a no-op. Never persists.
            model.SessionDisabledStoreIdentities.Add(identity.Value);
        }

        /// <inheritdoc/>
        public void DisableForFutureSessions(StoreIdentity identity)
        {
            ValidateIdentity(identity);
            var model = GetModelForWriteOrThrow();

            var alreadyPersisted = model.DisabledStoreIdentities.Any(x =>
                string.Equals(x, identity.Value, StringComparison.OrdinalIgnoreCase)
            );

            if (alreadyPersisted)
            {
                // Idempotent: do not append a duplicate and do not serialize again.
                return;
            }

            model.DisabledStoreIdentities.Add(identity.Value);
            model.Serialize();
        }

        /// <inheritdoc/>
        public async Task ReenableAsync(StoreIdentity identity)
        {
            ValidateIdentity(identity);
            var model = GetModelForWriteOrThrow();

            // Clear the session scope (never persisted, so it never triggers Serialize()).
            model.SessionDisabledStoreIdentities.Remove(identity.Value);

            // Clear the persisted scope; serialize exactly once only when the persisted list changed.
            var removedFromPersisted = model.DisabledStoreIdentities.RemoveAll(x =>
                string.Equals(x, identity.Value, StringComparison.OrdinalIgnoreCase)
            );

            if (removedFromPersisted > 0)
            {
                model.Serialize();
            }

            // Await the rehook collaborator AFTER disabled state has been cleared. In wave 0 this is
            // the no-op default; F3 supplies the real implementation. A non-disabled reenable still
            // awaits the collaborator.
            await _rehook.RehookAsync(identity);
        }

        /// <inheritdoc/>
        public bool IsDisabled(StoreIdentity identity)
        {
            var model = GetModelOrNull();
            if (model is null)
            {
                return false;
            }

            return model.IsEffectivelyDisabled(identity);
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<DisabledStoreEntry> GetDisabledStores()
        {
            var model = GetModelOrNull();
            if (model is null)
            {
                return Array.Empty<DisabledStoreEntry>();
            }

            var entries = new List<DisabledStoreEntry>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Persisted (FutureSessions) first so an identity present in both scopes is reported once
            // with the stronger, persisted scope.
            if (model.DisabledStoreIdentities is not null)
            {
                foreach (var value in model.DisabledStoreIdentities)
                {
                    if (string.IsNullOrWhiteSpace(value) || !seen.Add(value))
                    {
                        continue;
                    }

                    entries.Add(
                        new DisabledStoreEntry(
                            StoreIdentity.Resolve(value),
                            DisableScope.FutureSessions
                        )
                    );
                }
            }

            if (model.SessionDisabledStoreIdentities is not null)
            {
                foreach (var value in model.SessionDisabledStoreIdentities)
                {
                    if (string.IsNullOrWhiteSpace(value) || !seen.Add(value))
                    {
                        continue;
                    }

                    entries.Add(
                        new DisabledStoreEntry(
                            StoreIdentity.Resolve(value),
                            DisableScope.SessionOnly
                        )
                    );
                }
            }

            return entries;
        }
    }
}
