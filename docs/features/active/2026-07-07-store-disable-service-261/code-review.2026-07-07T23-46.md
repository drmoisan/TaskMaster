# Code Review — Store Disable Service (F1, Issue #261)

- Timestamp: 2026-07-07T23-46
- Reviewer: feature-reviewer
- Feature branch: `feature/store-disable-service-261` @ HEAD `88366ad4`
- Base (merge-base): `8bd91d1d`
- Diff scope: `git diff 8bd91d1d..HEAD`

## Executive Summary

The implementation is well-structured, readable, and faithful to the spec. Domain concepts are
modeled as small immutable value types; the service is a thin orchestration layer over a single
source of truth (`StoresWrapper`); the pure filter decision is centralized in
`StoreFilterAttribution.Decide`; identity resolution keeps COM access confined to filter call sites.
Error handling is fail-fast with a documented fail-safe sentinel. Tests are deterministic, mock-based,
and cover the positive/negative/idempotency/edge matrix across all three filter surfaces plus a
serialization round-trip.

Findings are minor. The most material is a test-quality defect: three `ReenableAsync` exception
assertions use `.Should().ThrowAsync<...>()` without `await`, so those assertions never execute. No
correctness defect was found in production code. The file-size limit finding on
`StoresWrapperTests.cs` is tracked in the policy audit as Blocking.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Non-blocking | UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs | lines 226-229, 261-263 | `ReenableAsync` exception cases call `.Should().ThrowAsync<ArgumentException>()` / `.ThrowAsync<InvalidOperationException>()` without `await`; the returned assertion `Task` is discarded, so the assertion never runs. `ReenableAsync` is `async Task`, so its synchronous guard exceptions are captured on the returned task rather than thrown synchronously — meaning these are the only checks of the `ReenableAsync` throw paths, and they are ineffective. | `await` each `ThrowAsync` assertion. Consider enabling an analyzer/warning for discarded awaitable results in tests. | Silent no-op assertions give false confidence that `ReenableAsync` validation is verified. Behavior is still correct because the shared `ValidateIdentity`/null-model guards run first and are exercised by the two synchronous write methods. | StoreDisableServiceTests.cs; StoreDisableService.cs lines 108-111 |
| Non-blocking | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | static `StoreIsIncluded` signature | Public static method gained a trailing `bool isDisabled` parameter — a breaking signature change. | Acceptable as-is; keep the call-out. If any external consumer exists, prefer an overload. | Verified no non-test caller exists in-repo (grep). Spec §6 documents the static overload's only caller is a unit test. Change is contained. | grep of `StoreIsIncluded`; spec.md §6 |
| Advisory | UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs | lines 91-95, 98-102 | Two empty `catch { }` blocks swallow all exceptions around the guarded COM reads (`DisplayName`, `FilePath`). | Narrow to the specific COM exception type where practical, or add a debug-level log, to avoid masking non-COM faults. | This mirrors the pre-existing guarded-read pattern in `ShouldIncludeStore` and is confined to the filter-time COM overload (not the pure resolver). Fail-safe: an unresolved read yields the sentinel. Acceptable but broad. | StoreIdentity.cs; spec §3.3/§7 |
| Advisory | UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs | lines 93-95, 117-119 | Persisted-scope membership/removal uses `List<string>` linear scans (`.Any`/`.RemoveAll` with `OrdinalIgnoreCase`). | Acceptable for the expected small disabled-store count. No change needed unless the list is expected to grow large. | Disabled-store lists are small by domain; clarity over micro-optimization is the correct call per design principles. | StoreDisableService.cs |
| Advisory | UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs | line 30 | `_rehook = rehook ?? new NoOpStoreRehookService();` allocates a no-op per service instance. | Optional: a shared static `NoOpStoreRehookService` instance. Negligible impact (one service instance per app). | Immaterial allocation; current form is clear. | StoreDisableService.cs |

## Correctness Assessment (positive confirmations)

- **Idempotency is correct.** `DisableSessionOnly` relies on `HashSet.Add` (no-op on duplicate, no
  serialize). `DisableForFutureSessions` checks `Any(OrdinalIgnoreCase)` before appending and only
  serializes on a real append. `ReenableAsync` serializes only when `RemoveAll > 0`. Verified by
  tests asserting `timer.StartCount` == 0/1 as appropriate.
- **Union semantics + case-insensitivity.** `IsEffectivelyDisabled` unions the session `HashSet`
  (OrdinalIgnoreCase) and the persisted `List` (compared via `OrdinalIgnoreCase`), and rejects the
  sentinel/whitespace, matching AC and the fail-safe design.
- **Attribution order preserved.** `Decide` adds the `isDisabled` check after the four existing
  exclusion checks and before `Included`; the enum inserts `Disabled` immediately before `Included`.
  Tests assert each pre-existing rule still wins when a store is also disabled (byte-for-byte
  attribution preserved) and the enum ordering.
- **All three filter surfaces patched identically.** Instance `ShouldIncludeStore` (via `Decide`),
  static `StoreIsIncluded`, and the instrumented/`Init` path each apply the disabled check as the last
  gate. The instrumented path is tested end-to-end (`Init_ExcludesSessionAndFutureDisabledStores_...`)
  confirming `Stores` contains only the non-disabled store.
- **No COM read regression.** Filter surfaces resolve identity from primitives already read in the
  same pass; no second FilePath read is introduced (deviation #3 rationale confirmed in code).
- **Persistence path reuse.** `Model.Serialize()` (parameterless) is used, deferring to the existing
  debounced write; no new file or config key added; `DisabledStoreIdentities` is a sibling
  `[JsonProperty]`; the session set is `[JsonIgnore]` and re-initialized by the field initializer on
  deserialize (round-trip test confirms JSON omits it and it is empty-not-null after deserialize).
- **net48 constraint honored.** `StoreIdentity`/`DisabledStoreEntry` are plain `readonly struct` with
  ordinary constructors and get-only properties (no `init`/`record struct`), matching the documented
  CS0518 constraint.
- **Lazy model read.** `StoreDisableService` reads `Globals.Ol.StoresWrapper` per call and never
  caches, so construction in `LoadBasicMethod()` (before the async store-load phase) is valid;
  confirmed at ApplicationGlobals.cs line 118.

## Test-Design Assessment

Tests are deterministic (injectable never-fired timer seam, no sleeps/real timers), mock-based (no
live Outlook, no temp files), AAA-structured, and use FluentAssertions with reason strings. Coverage
of the disabled-store behavior is thorough. The one defect is the unawaited async throw assertions
noted above; recommend fixing for effective verification of the `ReenableAsync` guard paths.
