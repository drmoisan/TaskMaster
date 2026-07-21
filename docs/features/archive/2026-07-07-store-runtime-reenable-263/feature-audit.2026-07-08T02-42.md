# Feature Audit — F3 store-runtime-reenable (#263)

- Branch: feature/store-runtime-reenable-263
- Review commit: ee46eb5d (HEAD) vs base 1724f8d0
- Work Mode: full-feature
- AC source: `spec.md` (AC1–AC11) and `user-story.md`
- Timestamp: 2026-07-08T02-42

## Scope and Baseline

Acceptance criteria are verified against the full branch diff `1724f8d0..HEAD` and the committed
evidence tree. AC1–AC11 were already checked `[x]` by the executor in `spec.md`; this audit
independently confirms each is genuinely satisfied.

## Acceptance Criteria Inventory

- Source: `docs/features/active/2026-07-07-store-runtime-reenable-263/spec.md`
- Total AC items: 11 (AC1–AC11)
- Companion: `user-story.md` outcome checklist (5 items) maps onto spec AC1–AC11; no independent AC.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 — one per-store primitive extracted from each of the three subsystems + `StoresWrapper.AddOrRestoreStore`; startup loop and rehook share it | PASS | Diff: `AppEvents.cs` loop now calls `SubscribeInboxForStore`; `LoadInboxes` calls `ResolveInboxForStore`; `RewireOlObjectsAsync` calls `AddOrRestoreStore`; `AddAllStores` calls `AddStore`. Tests: StoresWrapperRehookTests, AppEventsStoreRehookTests, sink tests; startup-regression.md. |
| AC2 — `RehookStoreAsync` re-adds store, re-registers item + folder/store handlers, invalidates tree via `MarkStale` | PASS | `PerformOneStoreHookup` drives `addOrRestore -> subscribeInbox -> sink.AddStore -> MarkStale(storeId, StoreAdded)`. Test `RehookStoreCoreAsync_WhenTransientThenReady...` asserts exact order + `MarkStale` `Times.Once`. |
| AC3 — idempotent, StoreID-keyed; second call returns `AlreadyHooked` with zero additional subscribes | PASS | Coordinator `AlreadyHooked` short-circuit (zero calls, gate never probed) — `WhenAlreadyFullyHooked...`; per-subsystem no-op tests (AppEvents second call `Times.Once`; sink already-present `SubscribeCount 0`). |
| AC4 — new `HookReadinessCoordinator` per call, store-scoped `IsReady(Store)`, no eager COM read | PASS | `StoreScopedReadinessGate` + `new HookReadinessCoordinator(...)` per call; `MaxReadinessAttempts=20`. Test `WhenGateNeverReady...NoEagerComRead` (Calls empty; gate `Times.Exactly(20)`). |
| AC5 — `bool IsReady(Outlook.Store)` added, reuses `IsTransientError`, parameterless `IsReady()` unchanged | PASS | Interface + impl diff; parameterless `IsReady()` body untouched. OutlookReadinessGateTests: ready / transient / non-transient / null. |
| AC6 — transient-timeout / store-not-found / permanent-error outcomes; log4net with identity, subsystem, HRESULT | PASS | `StoreRehookOutcome` enum + `LogOutcome`/`DescribeHResult`. Tests cover all three branches; log level per outcome verified by inspection. |
| AC7 — no exception escapes; all outcomes via result contract | PASS | Boundary catch -> `PermanentError`. Tests `...PermanentError...WithoutThrowing`, adapter `NotThrowAsync` (COM and non-COM exceptions). |
| AC8 — real coordinator injected at DI site; F1 `StoreDisableService` unmodified; clear-first-then-rehook-unconditionally; outcome logged not gating | PASS | `ApplicationGlobals.cs` diff: `new StoreDisableService(this, _storeRehookCoordinator)`. `StoreDisableService.cs` byte-unchanged (git diff --stat empty); `ReenableAsync` clears scopes then `await _rehook.RehookAsync(identity)` unconditionally (line 129). Coordinator logs outcome, returns bare Task. |
| AC9 — no compile-time dependency on `IStoreDisableService`; `StoreIdentity` dependency intended | PASS | `evidence/other/no-f1-compile-dependency.md`: zero `IStoreDisableService` references in F3 production files. Coordinator implements `IStoreRehookService.RehookAsync(StoreIdentity)`. F1 files unchanged. |
| AC10 — two-tier deterministic MSTest (COM-free orchestrator + COM-mocked primitives); no live Outlook/temp/timers | PASS | StoreRehookCoordinatorTests (tier 1, all 5 outcomes + idempotency) + sink/AppEvents/StoresWrapper/readiness tests (tier 2). MSTest+Moq+FluentAssertions; injected no-op delay; banned-pattern grep clean. |
| AC11 — startup tests still pass; full toolchain in order; no coverage regression; all files <= 500 | PASS | qa-01..03 (format/analyzers/nullable exit 0); startup-regression 4430/4430; qa-05 no regression (+0.18pp), new-code 99.6%, testable-denominator 83.23%; file-size-check all <= 500 (largest 498). |

## Cross-Feature Correctness (Epic)

- F1 files unchanged: `StoreDisableService.cs`, `IStoreRehookService.cs`, `IApplicationGlobals.cs`,
  `StoreIdentity.cs` all report no changes in `git diff --stat 1724f8d0..HEAD`. The only F1-adjacent
  production edit is the DI construction site in `ApplicationGlobals.cs`. CONFIRMED.
- `StoreRehookCoordinator` implements F1's `UtilitiesCS.IStoreRehookService` via the public
  `RehookAsync(StoreIdentity)` adapter over the internal `RehookStoreCoreAsync(string)`. CONFIRMED.
- Attribution set/clear points for F4: the extraction preserves `EmitPerStoreInboxAttribution`
  usage inside `ResolveInboxForStore`; `StoreWrapper.Init`/`StoresWrapper.RewireOlObjectsAsync`
  per-store attribution paths are unchanged except for delegating the loop body to the shared
  primitive (startup-regression.md confirms the pre-existing suites pass). CONFIRMED intact.

## Concurrency / Idempotency Invariants

StoreID-keyed idempotency is enforced across all three subsystems (AppEvents
`_hookedInboxItemsByStoreId` under `lock(OlInboxes)`; sink `_storeSubscriptions` under `_gate`;
`StoresWrapper` implicit DisplayName lookup). A second rehook for the same StoreID performs zero
additional subscribes — verified at the coordinator level (`AlreadyHooked`, zero calls) and per
subsystem (`Times.Once`/`SubscribeCount 0` assertions). No leaked or double subscriptions identified.

## Scrutiny Items

- (a) `[ExcludeFromCodeCoverage]` on COM-bound members: ACCEPTABLE (non-blocking). Each excluded
  member is genuinely COM-bound composition-root/wrapper code with no seam below the live-Outlook
  boundary; the testable decision seams (`AddStoreSubscriptions`, coordinator delegates,
  `EmitPerStoreInboxAttribution`) are not excluded and are tested. No testability-shortcut exclusion
  hiding testable decision logic. Floor met (83.23%) without the exclusions. One residual observation
  (HRESULT branch inside excluded `ResolveInboxForStore`) recorded as code-review CR-2, non-blocking.
- (b) `internal -> public` widening of `StoresWrapper.AddOrRestoreStore` and
  `OutlookFolderNotificationSink.IsStoreHooked`: MINIMAL AND NECESSARY (non-blocking). `UtilitiesCS`
  grants no `InternalsVisibleTo("TaskMaster")`, so cross-assembly access from the coordinator requires
  public. The COM-free test seam `AddStoreSubscriptions` correctly remains internal. Not an
  over-broad public API expansion.

## Summary

- Total AC items: 11
- PASS: 11
- PARTIAL / FAIL / UNVERIFIED: 0
- Total blocking findings: 0
- Non-blocking findings: 2 (duplicate `<remarks>` XML doc on `AddOrRestoreStore`; residual HRESULT
  classification branch inside a coverage-excluded COM member). Plus one benign observation
  (instrumentation-only Deedle test failures, not F3).
- Scrutiny (a): acceptable — genuinely COM-bound exclusions, not testability shortcuts.
- Scrutiny (b): acceptable — minimal necessary public surface.

Overall verdict: READY.

### Acceptance Criteria Check-off

All AC1–AC11 in `spec.md` were already checked `[x]` by the executor and are confirmed PASS by this
audit; no change to checkbox state is required. The `user-story.md` outcome checklist maps onto the
confirmed spec ACs.

### Acceptance Criteria Status

- Source: spec.md (AC1–AC11); user-story.md (companion outcomes)
- Total AC items: 11
- Checked off (delivered): 11
- Remaining (unchecked): 0
- Items remaining: none
