# Acceptance Criteria Checkoff (Issue #328, P4-T8)

Timestamp: 2026-07-15T21-05
AC sources (Work Mode: full-feature): spec.md (AC1–AC12), user-story.md (4 ACs), issue.md.

## spec.md AC evaluation

| AC | Status | Evidence |
|---|---|---|
| AC1 ExcludedStoreIds field / exact-match / OrdinalIgnoreCase / whitespace | PASS | StoresWrapperTests.StoreIdExclusion (exact, near-but-not-equal, case, whitespace) |
| AC2 First/authoritative precedence + attribution | PASS | Decide_WhenStoreIdMatchesAndOtherRulesAlsoMatch_ExcludesAndAttributesStoreId; ShouldIncludeStore tests |
| AC3 Decide branch + StoreFilterRule.StoreId | PASS | StoreFilterAttributionTests StoreID cases; enum-order test |
| AC4 Four inclusion surfaces in lockstep; Init omits excluded store | PASS | Init_WhenStoreIdExcluded_OmitsStoreFromProjectedSet; ShouldIncludeStore/StoreIsIncluded/Decide/Instrumented all route through Decide |
| AC5 Fail-open on unreadable StoreID | PASS | ShouldIncludeStore_WhenStoreIdReadThrows_IsFailOpenAndDoesNotExclude |
| AC6 Bypass sites route through the filter; no parallel logic | PASS | StoreFilterRoutingTests (GetToDoList/GetToDoListAsync/GetAsyncEnumerableOfToDoItemsInView); ProjectDataCoverageExpansionTests.Rebuild_WhenStoreIdExcluded; P2-T9 no-parallel-filter review. NOTE: the spec sub-clause "the two issue-named ToDoEvents methods are threaded" is SUPERSEDED by the user-approved scope expansion — those two dead methods were deleted (P2-T3), so there is no surface to route. |
| AC7 Not-yet-loaded model is fail-open (null StoresWrapper) | PASS | The identical `storesWrapper is null || storesWrapper.ShouldIncludeStore(...)` short-circuit is present at all four bypass sites (verified in the P2-T9 review); the routing tests confirm the exclusion mechanism the guard bypasses when null. |
| AC8 UI toggle binds to membership | PASS | PopulateWithCurrent_WhenStoreIdInExcludedSet_ChecksAndEnablesCheckbox; ...NotExcluded... |
| AC9 UI toggle mutates and persists; idempotency; no serialize when unchanged | PASS | SaveChanges add/remove/no-duplicate tests; AnyChanges_WhenCheckboxMatchesMembershipAfterPopulate_ReturnsFalse |
| AC10 UI fail-safe on unreadable StoreID | PASS | PopulateWithCurrent_WhenStoreIdUnreadable_DisablesAndUnchecksCheckbox; SaveChanges_WhenStoreIdUnreadable_DoesNotMutateExcludedStoreIds |
| AC11 Backward-compatible persistence | PASS | StoresWrapper round-trip + legacy-absent-key tests; StoreWrapper.StoreId round-trip + legacy tests |
| AC12 Toolchain and coverage | PASS | csharpier PASS, analyzers PASS (0 errors), nullable/TWAE PASS (0 errors), coverage targets met (per-class line >= 95%, new-code >= 90%), all touched files <= 500 (AppToDoObjects.cs at its documented 503 baseline, not grown). vstest functionally green: 4611/4611 without instrumentation; the prior scope conflict is resolved by the in-scope P4-T4 fix (handled `get_StoresWrapper` fail-open case). |

## user-story.md AC evaluation

| AC | Status |
|---|---|
| Exclude by StoreID end-to-end (not enumerated/processed by to-do tree, to-do events, project data) | PASS |
| Exclusion persists across sessions via StoresWrapper config | PASS |
| UI toggle without hand-editing JSON | PASS |
| New/changed code meets coverage thresholds; full toolchain passes | PASS (AC12 blocker resolved by P4-T4) |

## issue.md

The issue narrative requirements (exclude by StoreID, persistence, UI toggle, route the bypass sites)
are all delivered and map to the spec/user-story ACs above.

## Summary

- spec.md: 12 / 12 checked off.
- user-story.md: 4 / 4 checked off.
- Overall Phase 4 outcome: PASS. All 12 spec ACs and all 4 user-story ACs are met.

## Scope-conflict resolution (P4-T4)

Production change P2-T6 (authorized, in-scope) threads `Parent.Ol.StoresWrapper` into
`ProjectData.Rebuild` at `TaskMaster/AppGlobals/AppToDoObjects.cs:121`. The
`TaskMaster.Test` `LoadProjInfoAsync_RebuildsWhenProjectCountIsZeroAndOutlookApplicationIsAvailable`
test uses `OlObjectsProxy` (`TaskMaster.Test/AppGlobals/AppToDoObjectsTestDoubles.cs`), a reflection
proxy that previously supported only `get_App` and threw `NotSupportedException` for
`get_StoresWrapper`, causing the `Parent.Ol.StoresWrapper` argument to throw before `Rebuild` reached
the session access the test asserts.

The scope conflict was resolved by adding `AppToDoObjectsTestDoubles.cs` to the plan's Scope-Lock and
executing the in-scope P4-T4 task: `OlObjectsProxy.Invoke` now returns
`new ReturnMessage(null, null, 0, call.LogicalCallContext, call)` for `get_StoresWrapper` (fail-open,
mirroring the `get_App` branch's `ReturnMessage` shape). This preserves the test's original intent
(`Rebuild` reaches `get_Session`) via the `storesWrapper is null || storesWrapper.ShouldIncludeStore`
predicate treating the proxy as not-yet-loaded. The target test now passes and the full suite is
functionally green (4611/4611 without coverage instrumentation).
