# Code Review — F3 store-runtime-reenable (#263)

- Branch: feature/store-runtime-reenable-263
- Review commit: ee46eb5d (HEAD) vs base 1724f8d0
- Timestamp: 2026-07-08T02-42

## Executive Summary

The change set is well-structured and closely follows the spec's Approach B: a single per-store
primitive per startup subsystem, reused by both the startup loop body and a thin runtime rehook
coordinator. Seam-based dependency injection makes the decision logic fully testable without live
Outlook, and the idempotency and STA-safety invariants are implemented as designed. Error handling
is fail-fast at the AC7 boundary and observable via log4net. Two non-blocking observations are
recorded; no correctness or policy defects that block merge were found.

Overall code-quality verdict: PASS. Blocking findings: 0.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| PARTIAL (non-blocking) | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | `AddOrRestoreStore` XML doc (~lines 100–120) | The method carries two separate `<remarks>` blocks (grep count = 2), a malformed/duplicated XML-doc section. | Merge the two `<remarks>` blocks into one. | CLAUDE.md §5/C#6 require synchronized, well-formed docs; duplicate sections are a documentation defect. Not a compiler error (build passed). | `grep -c "<remarks>"` returned 2; diff hunk shows two `/// <remarks>` blocks around `<returns>`. |
| PARTIAL (non-blocking) | TaskMaster/AppGlobals/AppOlObjects.StoreRehook.cs | `ResolveInboxForStore` catch block (~lines 200–224) | The transient-vs-permanent HRESULT classification branch lives inside a `[ExcludeFromCodeCoverage]` COM-bound member. `ApplicationGlobals.StoreRehook.cs:ResolveLiveStore` similarly contains a DisplayName match loop inside an excluded member. | Optional: leave as-is (mirrors the pre-existing untested `LoadInboxes` body). If future refactoring is done, the HRESULT-classification decision could be lifted above the COM seam. | The excluded members are genuinely COM-bound (every delegate crosses live Outlook COM with no seam below), and the transient-HRESULT set is independently tested in `OutlookReadinessGateTests`. Floor is met (83.23%) without the exclusions, so this is not load-bearing. | qa-04-test-coverage.md; OutlookReadinessGateTests.cs cases for transient/non-transient HRESULT. |
| PASS (observation) | evidence/qa-gates/qa-04-test-coverage.md | coverage-instrumented run | The coverage-instrumented run reports 17/22 failing tests; all are pre-existing Deedle/DataFrame tests destabilized by instrumentation, not F3 regressions. | None. | The non-instrumented regression run is 4430/4430 green; the failures are outside F3's touched files and reproduce only under instrumentation. | startup-regression.md (4430/4430); qa-04 test-count note. |

## Detailed Notes

### Design and separation of concerns

`StoreRehookCoordinator` is `internal sealed`, depends only on injected narrow delegates/interfaces,
and isolates the run-once/transient decision logic (`RehookStoreCoreAsync`) from the COM
composition root in `ApplicationGlobals.StoreRehook.cs`. The `StoreScopedReadinessGate` private
adapter cleanly reuses the existing `HookReadinessCoordinator` per call rather than making the
run-once singleton reentrant, matching the spec's STA-safety design. This is a clear, simple design
consistent with the repository precedent (`EmitPerStoreInboxAttribution`).

### Idempotency and concurrency

StoreID-keyed idempotency is implemented in each subsystem and guarded consistently:
- `AppEvents.SubscribeInboxForStore` checks `_hookedInboxItemsByStoreId` under `lock (OlInboxes)`
  (documented reentrant-lock rationale) before `AddLast`, closing the pre-existing double-subscribe
  risk in the startup loop, which now also routes through the primitive.
- `OutlookFolderNotificationSink.AddStore` performs a cheap already-present guard, and the
  authoritative guard is in `AddStoreSubscriptions` under `_gate`, atomic with the subscribe.
- `StoresWrapper.AddOrRestoreStore` relies on the existing DisplayName `Find` lookup (restore, not
  duplicate).
- The coordinator's `AlreadyHooked` short-circuit is a pure predicate over the three trackers and
  performs zero COM touches on the second invocation (verified by test).

The mutable, StoreID-keyed refactor of the sink (`_appLevelSubscriptions` +
`_storeSubscriptions` under `_gate`) preserves `Start()`/`Dispose()` whole-collection semantics and
adds thread-safe per-store add/remove.

### Error handling and logging

The single boundary catch in `RehookStoreCoreAsync` guarantees no exception escapes (AC7);
`ResolveInboxForStore` correctly re-throws transient HRESULTs (so the readiness gate routes to retry)
and logs-and-skips permanent ones, preserving the issue #207 policy verbatim from the extracted body.
`LogOutcome` emits identity, StoreID, failing subsystem, and HRESULT at the correct log4net level per
outcome.

### Tests

Tier-1 COM-free tests (`StoreRehookCoordinatorTests`) exercise all five `StoreRehookOutcome` values,
primitive ordering (`addOrRestore -> subscribeInbox -> addStore -> markStale`), idempotency (zero
calls on already-hooked; gate never probed), the bounded window
(`Times.Exactly(MaxReadinessAttempts)`), no-eager-COM-read, and the public adapter's
`StoreIdentity.Value` extraction. Tier-2 COM-mocked tests cover the sink add/remove/idempotency,
AppEvents subscribe idempotency, `StoresWrapper.AddOrRestoreStore` both branches, and the
`IsReady(Store)` overload (ready/transient/permanent/null). Assertions use FluentAssertions with
clear failure messages; Arrange-Act-Assert structure is followed. The three updated
`StoresWrapperTests` regression tests re-target the extracted `AddOrRestoreStore` method and preserve
behavioral intent (ordered iteration, cooperative yield, restore-vs-create branch, one increment per
store); no behavioral assertion was weakened.
