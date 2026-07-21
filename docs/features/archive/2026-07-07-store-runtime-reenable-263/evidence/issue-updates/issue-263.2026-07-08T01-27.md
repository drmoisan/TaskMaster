# Issue #263 — Acceptance Criteria Update Mirror

Timestamp: 2026-07-08T01-27
PostedAs: unknown (local mirror only; not posted to GitHub in this execution)

The following updated `## Acceptance Criteria` state (with delivery-evidence annotations) was written to `docs/features/active/2026-07-07-store-runtime-reenable-263/spec.md`. All AC1–AC11 are checked.

## Acceptance Criteria (checked state)

- [x] AC1: A new per-store primitive is extracted from each of the three startup hookup subsystems plus a `StoresWrapper.AddOrRestoreStore` primitive, sharing one per-store implementation between startup and runtime rehook.
- [x] AC2: `RehookStoreAsync`/`RehookStoreCoreAsync` re-adds the resolved live store to `StoresWrapper.Stores`, re-registers item-level (`AppEvents`) and folder/store-level (`OutlookFolderNotificationSink`) handlers, and invalidates the cached folder-tree snapshot via `IOutlookFolderTreeService.MarkStale`.
- [x] AC3: The operation is idempotent, keyed by `StoreID`: a second rehook for a fully-hooked store returns `AlreadyHooked` and makes zero additional subscribe/AddStore/AddLast calls.
- [x] AC4: Reuses the readiness-gate/transient-retry shape by constructing a new `HookReadinessCoordinator` per call, uses the store-scoped `IsReady(Outlook.Store)` overload, and introduces no synchronous expensive COM read before the gate reports ready.
- [x] AC5: The store-scoped `bool IsReady(Outlook.Store store)` overload is added, reusing `IsTransientError(COMException)` unchanged, without altering the parameterless `IsReady()`.
- [x] AC6: Transient-not-ready → `TransientTimeout` within a bounded window; unresolved identity → `StoreNotFound`; non-transient exception → `PermanentError`; `TransientTimeout`/`PermanentError` logged via log4net with identity, failing subsystem, and HRESULT (when COM-derived).
- [x] AC7: `RehookAsync`/`RehookStoreCoreAsync` never lets an exception escape uncaught; all outcomes reported through `StoreRehookResult`/`StoreRehookOutcome`.
- [x] AC8 (reconciled): The real `StoreRehookCoordinator` is injected as F1's `IStoreRehookService` collaborator at the DI construction site (`ApplicationGlobals.cs`, `new StoreDisableService(this, <coordinator>)`), replacing the wave-0 no-op. F1's shipped `ReenableAsync` (clear session then persisted scope, then await the collaborator unconditionally) is unchanged; the coordinator's outcome is logged, not used to gate scope-clearing.
- [x] AC9 (reconciled): F3 takes no compile-time dependency on `IStoreDisableService`. The `StoreIdentity` dependency is expected and permitted.
- [x] AC10: Deterministic MSTest coverage in two tiers — COM-free orchestrator (`StoreRehookCoordinatorTests`, all five outcomes + idempotency) and COM-mocked primitives (`OutlookFolderNotificationSinkTests`, `AppEventsStoreRehookTests`, `StoresWrapperRehookTests`, `OutlookReadinessGateTests`) — with no live Outlook, temp files, or real timers.
- [x] AC11: Existing startup-path tests pass; full C# toolchain passes in order (CSharpier → analyzers → nullable/TWAE → MSTest with coverage, `TestCategory!=LiveOutlook`) with no repo-wide coverage regression; all touched/new files <= 500 lines.

Evidence annotations for each AC are recorded in spec.md under "### Acceptance Criteria — Delivery Evidence (F3 #263, verified 2026-07-08)".
