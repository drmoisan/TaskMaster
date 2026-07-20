using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;
using Outlook = Microsoft.Office.Interop.Outlook;

#nullable enable

namespace TaskMaster
{
    /// <summary>
    /// Runtime rehook orchestrator for a single Outlook store (issue #263, epic #260). Implements
    /// F1's <see cref="IStoreRehookService"/> seam: F1's <c>StoreDisableService.ReenableAsync</c>
    /// awaits <see cref="RehookAsync(StoreIdentity)"/> after clearing the disabled scope. The public
    /// adapter extracts <see cref="StoreIdentity.Value"/>, drives the rich decision logic in
    /// <see cref="RehookStoreCoreAsync(string)"/>, logs the resulting <see cref="StoreRehookResult"/>,
    /// and returns a bare <see cref="Task"/> (no outcome is propagated to F1, whose seam is
    /// void-returning by design).
    /// </summary>
    /// <remarks>
    /// The decision path depends only on injected narrow delegates and interfaces (a store-lookup
    /// seam, the <see cref="StoresWrapper.AddOrRestoreStore"/> gateway, the AppEvents inbox-hookup
    /// seam, <see cref="IOutlookFolderNotificationSink"/>, <see cref="IOutlookFolderTreeService"/>,
    /// and the store-scoped <see cref="IOutlookReadinessGate"/>), so every branch is exercised
    /// deterministically with Moq and no live Outlook. Expensive COM reads run only behind the
    /// store-scoped readiness gate, never eagerly. No exception escapes the rehook path.
    /// </remarks>
    internal sealed class StoreRehookCoordinator : IStoreRehookService
    {
        /// <summary>
        /// Maximum readiness attempts per rehook call (Binding Decision 1). Bounded as an attempt
        /// count (not wall-clock time) so the decision logic is deterministic and COM-free-testable;
        /// exceeding the bound yields <see cref="StoreRehookOutcome.TransientTimeout"/>. Maps to
        /// roughly 60 s under the production cadence (1 s initial, 5 s after 10 attempts).
        /// </summary>
        internal const int MaxReadinessAttempts = 20;

        private const int SlowCadenceThreshold = 10;
        private static readonly TimeSpan InitialCadence = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan SlowCadence = TimeSpan.FromSeconds(5);

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private readonly IOutlookReadinessGate _readinessGate;
        private readonly Func<string, Outlook.Store?> _resolveStore;
        private readonly Func<string, bool> _isAlreadyFullyHooked;
        private readonly Action<Outlook.Store> _addOrRestoreStore;
        private readonly Action<Outlook.Store> _subscribeInboxForStore;
        private readonly Func<IOutlookFolderNotificationSink> _folderNotificationSink;
        private readonly Func<IOutlookFolderTreeService> _folderTreeService;
        private readonly Func<TimeSpan, Task> _delay;

        /// <summary>
        /// Creates a coordinator over its injected seams.
        /// </summary>
        /// <param name="readinessGate">The store-scoped readiness gate (uses <c>IsReady(Store)</c>).</param>
        /// <param name="resolveStore">Resolves a store identity to a live store, or null if none matches.</param>
        /// <param name="isAlreadyFullyHooked">Pure predicate over the three idempotency trackers, keyed by StoreID.</param>
        /// <param name="addOrRestoreStore">The StoresWrapper per-store primitive (<see cref="StoresWrapper.AddOrRestoreStore"/>).</param>
        /// <param name="subscribeInboxForStore">The AppEvents per-store inbox-subscribe primitive.</param>
        /// <param name="folderNotificationSink">
        /// Lazy accessor for the folder/store notification sink (<c>AddStore</c>). Resolved only when
        /// the gate reports ready, so the sink's expensive folder-traversal construction is never
        /// forced eagerly at startup or before readiness.
        /// </param>
        /// <param name="folderTreeService">Lazy accessor for the folder-tree cache service (<c>MarkStale</c>).</param>
        /// <param name="delay">
        /// Non-blocking inter-attempt delay; defaults to <see cref="NonBlockingDelay.WaitAsync"/>.
        /// Tests supply a no-op so the bounded window resolves without real time passing.
        /// </param>
        public StoreRehookCoordinator(
            IOutlookReadinessGate readinessGate,
            Func<string, Outlook.Store?> resolveStore,
            Func<string, bool> isAlreadyFullyHooked,
            Action<Outlook.Store> addOrRestoreStore,
            Action<Outlook.Store> subscribeInboxForStore,
            Func<IOutlookFolderNotificationSink> folderNotificationSink,
            Func<IOutlookFolderTreeService> folderTreeService,
            Func<TimeSpan, Task>? delay = null
        )
        {
            _readinessGate =
                readinessGate ?? throw new ArgumentNullException(nameof(readinessGate));
            _resolveStore = resolveStore ?? throw new ArgumentNullException(nameof(resolveStore));
            _isAlreadyFullyHooked =
                isAlreadyFullyHooked
                ?? throw new ArgumentNullException(nameof(isAlreadyFullyHooked));
            _addOrRestoreStore =
                addOrRestoreStore ?? throw new ArgumentNullException(nameof(addOrRestoreStore));
            _subscribeInboxForStore =
                subscribeInboxForStore
                ?? throw new ArgumentNullException(nameof(subscribeInboxForStore));
            _folderNotificationSink =
                folderNotificationSink
                ?? throw new ArgumentNullException(nameof(folderNotificationSink));
            _folderTreeService =
                folderTreeService ?? throw new ArgumentNullException(nameof(folderTreeService));
            _delay = delay ?? NonBlockingDelay.WaitAsync;
        }

        /// <summary>
        /// F1's interface method. Rehooks the store identified by <paramref name="identity"/>,
        /// logs the resulting outcome, and returns without throwing for any hookup failure.
        /// </summary>
        /// <param name="identity">F1's stable store identity (DisplayName primary).</param>
        public async Task RehookAsync(StoreIdentity identity)
        {
            var result = await RehookStoreCoreAsync(identity.Value).ConfigureAwait(false);
            LogOutcome(result);
        }

        /// <summary>
        /// The rich decision core: resolves the identity to a live store, short-circuits an
        /// already-hooked store, runs the bounded store-scoped readiness/retry loop, and drives the
        /// four per-store primitives in order behind the gate. Returns one of the five
        /// <see cref="StoreRehookOutcome"/> values; never lets an exception escape.
        /// </summary>
        /// <param name="storeIdentity">The DisplayName-primary identity string to rehook.</param>
        internal async Task<StoreRehookResult> RehookStoreCoreAsync(string storeIdentity)
        {
            string? storeId = null;
            try
            {
                // 1. Resolve the identity to a live store (no COM read beyond the enumeration).
                var store = _resolveStore(storeIdentity);
                if (store is null)
                {
                    return new StoreRehookResult(StoreRehookOutcome.StoreNotFound, storeIdentity);
                }

                storeId = store.StoreID;

                // 2. Idempotency: a store already hooked in all three subsystems needs no COM touch.
                if (storeId != null && _isAlreadyFullyHooked(storeId))
                {
                    return new StoreRehookResult(
                        StoreRehookOutcome.AlreadyHooked,
                        storeIdentity,
                        storeId
                    );
                }

                // 3. Bounded, store-scoped readiness/retry loop. A NEW HookReadinessCoordinator per
                // call (the run-once singleton is never made reentrant), driven with the store-scoped
                // gate so no expensive COM read occurs before the gate reports ready.
                var scopedGate = new StoreScopedReadinessGate(_readinessGate, store);
                var currentStoreId = storeId;
                var readiness = new HookReadinessCoordinator(
                    scopedGate,
                    () => PerformOneStoreHookup(store, currentStoreId)
                );

                for (int attempt = 0; attempt < MaxReadinessAttempts; attempt++)
                {
                    if (readiness.Tick() == HookReadinessTickResult.Completed)
                    {
                        return new StoreRehookResult(
                            StoreRehookOutcome.Success,
                            storeIdentity,
                            storeId
                        );
                    }

                    if (attempt < MaxReadinessAttempts - 1)
                    {
                        await _delay(attempt < SlowCadenceThreshold ? InitialCadence : SlowCadence)
                            .ConfigureAwait(false);
                    }
                }

                return new StoreRehookResult(
                    StoreRehookOutcome.TransientTimeout,
                    storeIdentity,
                    storeId
                );
            }
            catch (Exception e)
            {
                // AC7 boundary: every COM boundary the coordinator crosses is wrapped so a
                // non-transient exception is reported as PermanentError and never escapes.
                return new StoreRehookResult(
                    StoreRehookOutcome.PermanentError,
                    storeIdentity,
                    storeId,
                    e
                );
            }
        }

        /// <summary>
        /// Drives the four per-store primitives in order once the readiness gate reports ready:
        /// re-add the store, subscribe its inbox item handler, register its folder/store
        /// subscriptions, then invalidate the cached folder-tree snapshot.
        /// </summary>
        private void PerformOneStoreHookup(Outlook.Store store, string? storeId)
        {
            _addOrRestoreStore(store);
            _subscribeInboxForStore(store);
            _folderNotificationSink().AddStore(store);
            _folderTreeService()
                .MarkStale(storeId ?? string.Empty, FolderTreeRefreshReason.StoreAdded);
        }

        private void LogOutcome(StoreRehookResult result)
        {
            switch (result.Outcome)
            {
                case StoreRehookOutcome.Success:
                case StoreRehookOutcome.AlreadyHooked:
                    logger.Debug(
                        $"[store-rehook] identity='{result.Identity}' storeId='{result.StoreId}' outcome={result.Outcome}"
                    );
                    break;
                case StoreRehookOutcome.StoreNotFound:
                    logger.Warn(
                        $"[store-rehook] identity='{result.Identity}' outcome=StoreNotFound; no live store matched the identity."
                    );
                    break;
                case StoreRehookOutcome.TransientTimeout:
                    logger.Error(
                        $"[store-rehook] identity='{result.Identity}' storeId='{result.StoreId}' outcome=TransientTimeout; "
                            + $"the store-scoped readiness gate never reported ready within {MaxReadinessAttempts} attempts."
                    );
                    break;
                case StoreRehookOutcome.PermanentError:
                    logger.Error(
                        $"[store-rehook] identity='{result.Identity}' storeId='{result.StoreId}' outcome=PermanentError; "
                            + $"subsystem=store-hookup; hresult={DescribeHResult(result.Error)}",
                        result.Error
                    );
                    break;
            }
        }

        private static string DescribeHResult(Exception? error)
        {
            if (error is COMException com)
            {
                return "0x" + unchecked((uint)com.ErrorCode).ToString("X8");
            }

            return "n/a";
        }

        /// <summary>
        /// Adapts the store-scoped readiness gate to the parameterless
        /// <see cref="IOutlookReadinessGate.IsReady()"/> contract that
        /// <see cref="HookReadinessCoordinator"/> consults, so the coordinator's run-once/transient
        /// decision logic is reused per call for one specific store.
        /// </summary>
        private sealed class StoreScopedReadinessGate : IOutlookReadinessGate
        {
            private readonly IOutlookReadinessGate _inner;
            private readonly Outlook.Store _store;

            public StoreScopedReadinessGate(IOutlookReadinessGate inner, Outlook.Store store)
            {
                _inner = inner;
                _store = store;
            }

            public bool IsReady() => _inner.IsReady(_store);

            // This file has no project-level <Nullable> element and no whole-file #nullable
            // pragma; IOutlookReadinessGate.IsReady(Store? store) declares a nullable parameter,
            // so this implementation's parameter must match to avoid CS8767. Scoping narrowly to
            // annotations-only avoids introducing new CS86xx diagnostics elsewhere in this file
            // (no behavior change per AC7 — the body passes the parameter through unchanged).
#nullable enable annotations
            public bool IsReady(Outlook.Store? store) => _inner.IsReady(store);

#nullable restore annotations

            public bool IsTransientError(COMException e) => _inner.IsTransientError(e);
        }
    }
}
