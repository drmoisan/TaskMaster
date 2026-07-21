# Code Review — outlook-startup-store-enumeration-com-stall (Issue #292)

- Feature: `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/`
- Branch: `bug/outlook-startup-store-enumeration-com-stall-292` @ HEAD `d971d717`
- Base: `main` @ `c9ddbf28` (merge-base)
- Work Mode: `full-bug`
- Reviewer: feature-review agent
- Timestamp: 2026-07-09T15-37

## Executive Summary

The change is small, cohesive, and well-scoped to the confirmed root cause. It introduces one phase-identity
`const` on `CurrentStoreContext`, one private `MaterializeFilteredStores()` helper on `StoresWrapper` that
wraps `GetFilteredStores().ToList()` in an ambient `using (CurrentStoreContext.Begin(...))` scope and is
called from both materialization sites (`Init()` line 44 and `RewireOlObjectsAsync` line 89), and one
terminal phase-identity guard branch in `StoreLockupResponder.OnLockupDetected` placed before any
`IStoreDisableService` call. The design honors the stated invariants: the scope is observational only
(included set and enumeration order unchanged), the `using` guarantees restore-on-failure, and no public API
is added.

Code quality is consistent with the surrounding code: descriptive naming, XML documentation and inline
comments that explain the non-obvious "why" (the pre-yield COM block and the watchdog-thread crash path),
correct guard ordering, and use of the established `StoreLockupAttribution.FormatLine(autoDisabled:false)`
overload. Tests are deterministic MSTest + Moq + FluentAssertions using the existing `ReflectionRealProxy`
seams with no live Outlook and no temporary files, and they establish a clean RED-before-GREEN transition
(T1/T2/T3 RED on HEAD, all five GREEN after the fix). No Blocking or Major findings were identified. Two
minor, non-blocking observations are recorded. Recommendation: approve.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` | `MaterializeFilteredStores()` L169-184 | The helper correctly centralizes both materialization sites inside a single enumeration-phase scope; the two call sites now delegate to it (L44, L89), removing duplication and guaranteeing consistent attribution at both sites. | None. | Reuse and single-responsibility satisfy the General Code Change Policy (§1 Reusability, §4 Separation of concerns). | Production diff; coverage `line_coverage=100.00` (5/5) |
| Info | `UtilitiesCS/Threading/StoreLockupResponder.cs` | `OnLockupDetected` phase-identity branch L104-131 | The phase branch is placed after the blank/unresolved guards and before the already-disabled guard and every disable-service call, matching the required guard order; it emits one WARN with `autoDisabled:false` and returns, avoiding the `GetModelForWriteOrThrow` crash path and #265 UI pollution. | None. | Correct guard ordering is the load-bearing crash-safety property (spec Root Cause §1.3, §4). | Production diff; T3 Strict-mock verification (zero disable calls) |
| Info | `UtilitiesCS/Threading/CurrentStoreContext.cs` | `StoresEnumerationPhaseIdentity` L23-30 | The new `const` value `"<Stores-enumeration>"` is distinct from the special `"<unavailable>"` value that `Normalize` collapses to null, so the phase identity flows through unchanged and yields a non-blank attribution. | None. | Correctness of the attribution contract depends on the value not colliding with the special-cased token. | Production diff; XML doc cites the `Normalize` interaction |
| Info | `UtilitiesCS/Threading/StoreLockupResponder.cs` | Phase-branch string comparison L110-115 | The branch uses `string.Equals(displayName, ..., StringComparison.Ordinal)`, the correct culture-invariant comparison for an internal sentinel token. | None. | Ordinal comparison is appropriate for a fixed non-localized identity; avoids culture-sensitive matching. | Production diff |
| Minor (non-blocking) | `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` | T4 `Init_HealthyMultiStore...` L94-118 | T4 asserts the included set/order and post-return null context but does not assert the recorded ambient value during a healthy multi-store enumeration (only T1/T2 assert the identity, using an excluded store). Behavior preservation is covered, but the healthy-store path does not additionally confirm the identity is observed. | Optional: add an assertion that the ambient identity is observed during the healthy multi-store enumeration, or note explicitly that T1/T2 already cover the observation. | Slightly strengthens the behavior-preservation test; not required because T1/T2 already prove the observation and T4 proves set/order and cleanup. | Test file review |
| Minor (non-blocking) | `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` | `ThrowingEnumerator.Current` L312 | `Current` returns `null` (a boxed `object` property). This is acceptable for the throwing seam because `MoveNext` throws before `Current` is read, but the value is never exercised. | None required. | The seam is intentionally minimal; the uncovered getter is inert scaffolding, not production code. | Test file review |

Severity legend: Blocking (must fix before merge), Major (should fix), Minor (optional), Info
(observation, no action).

## Detailed Notes

### Design and structure

- The fix is causation-scoped and additive. It does not attempt the rejected approaches (worker-thread
  offload, timeout/cancellation of the blocked `Next()`, indexed access), consistent with the spec's
  explicitly-excluded list.
- `MaterializeFilteredStores()` is `private` and returns `List<Outlook.Store>`; there is no new public
  surface. `CurrentStoreContext` gains one `const`; `StoreLockupResponder` gains one terminal branch.
- File sizes remain within the 500-line limit (largest touched production file `StoresWrapper.cs` at 469
  lines; new test file 320 lines; the sibling-file split keeps `StoresWrapperTests.cs` at 466).

### Error handling and resource safety

- `MaterializeFilteredStores()` uses `using (CurrentStoreContext.Begin(...))`, which restores the prior
  ambient value on both normal completion and exception. T5 verifies that a thrown enumeration leaves
  `CurrentStoreContext.Current == null`, closing the phase-identity-leak risk.
- The responder phase branch performs no disable-service write and returns after a single WARN, which
  matches the crash-safety contract for the fresh-build window where the disabled-store model is not yet
  constructed.

### Tests

- MSTest + Moq (`MockBehavior.Strict` in T3) + FluentAssertions throughout; `[TestClass]`/`[TestMethod]`;
  Arrange-Act-Assert structure with descriptive names and rationale strings.
- Determinism: no wall-clock reads, no sleeps, no real timers; T3 passes an explicit `TimeSpan`. No
  temporary files; all Outlook boundaries are `ReflectionRealProxy` seams.
- RED-before-GREEN is demonstrated: T1/T2/T3 fail on HEAD (recorded EXIT 1) and pass after the fix; T4/T5
  are GREEN before and after. Full suite 4519/4519 after the change.

### Architecture boundaries

- The No-COM architecture rules (`architecture-boundaries.md`) target *new* No-COM runtime code. These edits
  modify existing legacy VSTO/Outlook-interop code (`UtilitiesCS`) that already depends on
  `Microsoft.Office.Interop.Outlook`; no new COM-visible interface, VSTO API, or ribbon callback is
  introduced. The test file's interop reference is test code, not production runtime code. Not a boundary
  violation in this legacy context.

## Verdict

Approve. No Blocking or Major findings. Two Minor, non-blocking observations recorded; neither requires
remediation.
