# Code Review — outlook-startup-store-enumeration-com-stall (Issue #292)

- Feature: `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/`
- Branch: `bug/outlook-startup-store-enumeration-com-stall-292` @ HEAD `8f391d8f59afc74fdc9aacd3e53c59174e414884`
- Base: `main` @ `c9ddbf289c06f5fbf61673549911dac80917ce24`
- Timestamp: 2026-07-09T17-40
- Review context: re-audit after remediation cycle 1

## Executive Summary

The change is a minimal, causation-scoped fix to the #260 startup store-lockup resilience system, plus
deterministic regression tests. Three production C# files change: `CurrentStoreContext.cs` adds one
phase-identity `const`; `StoresWrapper.cs` extracts one `private` helper (`MaterializeFilteredStores`) that
wraps `GetFilteredStores().ToList()` in a `using (CurrentStoreContext.Begin(...))` scope and is called from
both prior inline materialization sites; `StoreLockupResponder.cs` adds one terminal phase-identity guard
that emits an attributed WARN with `autoDisabled: false` and returns before any `IStoreDisableService`
call. Code quality is consistent with the surrounding module: strong XML documentation citing the #292
rationale, correct guard ordering, `using`-based restore-on-failure, no new public API surface, no breaking
change, and all files under the 500-line limit. The recorded toolchain is clean (format, analyzers,
nullable all EXIT 0) and the CI-equivalent full-suite run is green and deterministic (5141/5141 across 4+
passes).

Remediation cycle 1 removed a process-global-static test-isolation race by adding `[DoNotParallelize]` to 8
`UtilitiesCS.Test` scope-opening classes; the fix is structural (MSTest serial-bucket mutual exclusion), not
a timing hack, and is proven deterministic. One residual, non-blocking robustness gap remains: the new
`TaskMaster.Test` regression class is an unmarked null-baseline reader of the same defect class, race-free
under the actual CI invocation but flaky under the non-gate VS Code coverage runsettings. No Blocking
findings. Recommendation: PR-ready (go), with one recommended non-gating follow-up.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major (non-blocking) | TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs | class decl. line 24-25; T4 line 115; T5 line 136 | New regression class both opens a `CurrentStoreContext` scope and asserts `Current == null`, but is not `[DoNotParallelize]`. Second instance of the same process-global-static defect class fixed in cycle 1 for `UtilitiesCS.Test`. Race-free under the required CI invocation (`TaskMaster.Test` has no `[assembly: Parallelize]`, runs sequentially); flakes only under the non-gate VS Code coverage runsettings that force `ClassLevel` parallelization. | Add `[DoNotParallelize]` to this class and audit sibling `TaskMaster.Test` writers (`StoresWrapperTests`, `AppOlObjectsTests`, `AppOlObjectsCoverageTests`), extending cycle-1 approach (A). Non-gating; may be handled as a separate follow-up. | Determinism policy (`general-unit-test.md`) prohibits flakiness; the sequential-run safety is an incidental config fact, not a designed guarantee. The team already marks the sibling `AppOlObjectsAttributionContextTests` `[DoNotParallelize]`, so this is a consistency gap this PR introduced. Not a live CI-gate failure, hence non-blocking. | evidence/other/out-of-scope-finding-taskmaster-test-race.2026-07-09T16-05.md; final-test-coverage.2026-07-09T16-05.md |
| Info | UtilitiesCS/Threading/StoreLockupResponder.cs | OnLockupDetected, phase-identity guard L111-127 | Phase guard correctly precedes all `IStoreDisableService` calls, closing the verified `InvalidOperationException` watchdog-thread crash path and the #265 UI-pollution path. Well-placed and well-documented. | None — accept as-is. | Guard ordering matches spec (blank -> unresolved -> phase-identity -> already-disabled -> disable/notify) and is verified by T3 with a `MockBehavior.Strict` mock. | UtilitiesCS/Threading/StoreLockupResponder.cs L103-127; StoreLockupResponderTests.cs T3 |
| Info | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | MaterializeFilteredStores L167-186 | Single extracted helper avoids duplicating the scope-wrapping at both call sites (DRY); `using` guarantees ambient restore on normal completion and on thrown exception; behavior-preserving (identical set/order). | None — accept as-is. | Matches design principles (reusability, restore-on-failure); verified by T4 (behavior preservation) and T5 (scope restore on failure). | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs L41-92, L167-186 |
| Info | UtilitiesCS/Threading/CurrentStoreContext.cs | StoresEnumerationPhaseIdentity L23-30 | Phase constant is distinct from the special `"<unavailable>"` sentinel that `Normalize` collapses to null, so the identity flows through and yields a non-blank attribution. | None — accept as-is. | Correct per spec design summary; the constant is a compile-time `const` with no executable IL. | UtilitiesCS/Threading/CurrentStoreContext.cs L30, L61-74 |
| Info | UtilitiesCS.Test/OutlookObjects/Store/*.cs (8 files) | class declarations | Cycle-1 `[DoNotParallelize]` additions are correctly placed (one per class, below `[TestClass]`; single attribute on the `[TestClass]`-bearing partial part for `StoreWrapperController_Tests`), csharpier-conformant, and introduce no production code. | None — accept as-is. | Structural mutual-exclusion fix; completeness gate proves zero unmarked scope-opening classes in `UtilitiesCS.Test`. | remediation-plan.2026-07-09T16-05.md; completeness-verification.2026-07-09T16-05.md; green-after-fix.2026-07-09T16-05.md |

## Notes

- No Python, TypeScript, or PowerShell files changed; no typed-Python review applies.
- No suppression additions, no analyzer debt, no weakened assertions, no sleeps/retries observed in the diff.
- All new/modified files are within the 500-line limit.
