# Code Review — outlook-startup-store-enumeration-com-stall (Issue #292)

- Feature: `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/`
- Branch: `bug/outlook-startup-store-enumeration-com-stall-292` @ HEAD `87ecc9a07b8a0b402558b5063a6fedf9459af4e2`
- Base: `main` @ `c9ddbf289c06f5fbf61673549911dac80917ce24`
- Timestamp: 2026-07-09T18-23
- Review context: re-audit after remediation cycle 2

## Executive Summary

The change is a minimal, causation-scoped fix to the #260 startup store-lockup resilience system, plus
deterministic regression tests. Three production C# files change: `CurrentStoreContext.cs` adds one
phase-identity `const`; `StoresWrapper.cs` extracts one `private` helper (`MaterializeFilteredStores`) that
wraps `GetFilteredStores().ToList()` in a `using (CurrentStoreContext.Begin(...))` scope and is called from
both prior inline materialization sites; `StoreLockupResponder.cs` adds one terminal phase-identity guard
that emits an attributed WARN with `autoDisabled: false` and returns before any `IStoreDisableService` call.
Code quality is consistent with the surrounding module: strong XML documentation citing the #292 rationale,
correct guard ordering, `using`-based restore-on-failure, no new public API surface, no breaking change, and
all files under the 500-line limit. The recorded toolchain is clean (format, analyzers, nullable all EXIT 0)
and the CI-equivalent full-suite run is green (5141/5141).

Two test-attribute-only remediation cycles preceded this re-audit. Cycle 1 added `[DoNotParallelize]` to 8
`UtilitiesCS.Test` scope-opener classes. Cycle 2 added `[DoNotParallelize]` to the three remaining
`TaskMaster.Test` scope-opener/null-baseline-reader classes (`StoresWrapperEnumerationScopeTests`,
`StoresWrapperTests`, `AppOlObjectsCoverageTests`), closing the Major non-blocking robustness gap recorded in
the cycle-1 re-audit. Determinism is now proven under both the required CI invocation and the VS Code
`ClassLevel` coverage runsettings. No Blocking findings and no remaining Major findings. Recommendation:
PR-ready (go).

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Resolved (was Major non-blocking) | TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs | class decl. L25-26 | The cycle-1 re-audit flagged this class as an unmarked `CurrentStoreContext` scope-opener/null-baseline reader (same defect class as the cycle-1 `UtilitiesCS.Test` race). Cycle 2 marks it `[DoNotParallelize]` and marks the two sibling `TaskMaster.Test` writers (`StoresWrapperTests`, `AppOlObjectsCoverageTests`). Determinism verified under both the CI invocation (5141/5141) and the VS Code `ClassLevel` runsettings (5/5 green, 251/251). | None — accept as-is. Gap closed. | Structural mutual-exclusion fix (MSTest serial bucket), not a timing hack; post-edit census confirms zero remaining unmarked scope-opener/reader classes in `TaskMaster.Test`. | census-confirmation.2026-07-09T17-45.md; determinism-vscode-runsettings.2026-07-09T17-45.md; qa-04-tests-ci-form.2026-07-09T17-45.md |
| Info | TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs; TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs | class declarations (L20 / L20) | Cycle-2 `[DoNotParallelize]` additions are correctly placed (one per class, below `[TestClass]`), csharpier-conformant, and add no production code. | None — accept as-is. | Extends the cycle-1 approach to the second assembly; each +1 line only. | git diff c9ddbf28..87ecc9a; census-confirmation.2026-07-09T17-45.md |
| Info | UtilitiesCS/Threading/StoreLockupResponder.cs | OnLockupDetected, phase-identity guard L103-127 | Phase guard correctly precedes all `IStoreDisableService` calls, closing the verified `InvalidOperationException` watchdog-thread crash path and the #265 UI-pollution path. Well-placed and well-documented. | None — accept as-is. | Guard ordering matches spec (blank -> unresolved -> phase-identity -> already-disabled -> disable/notify) and is verified by T3 with a `MockBehavior.Strict` mock. | UtilitiesCS/Threading/StoreLockupResponder.cs L103-127; StoreLockupResponderTests.cs T3 |
| Info | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | MaterializeFilteredStores L167-186 | Single extracted helper avoids duplicating the scope-wrapping at both call sites (DRY); `using` guarantees ambient restore on normal completion and on thrown exception; behavior-preserving (identical set/order). | None — accept as-is. | Matches design principles (reusability, restore-on-failure); verified by T4 (behavior preservation) and T5 (scope restore on failure). | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs L41-92, L167-186 |
| Info | UtilitiesCS/Threading/CurrentStoreContext.cs | StoresEnumerationPhaseIdentity L23-30 | Phase constant is distinct from the special `"<unavailable>"` sentinel that `Normalize` collapses to null, so the identity flows through and yields a non-blank attribution. | None — accept as-is. | Correct per spec design summary; the constant is a compile-time `const` with no executable IL. | UtilitiesCS/Threading/CurrentStoreContext.cs L23-30 |
| Info | UtilitiesCS.Test/OutlookObjects/Store/*.cs (8 files) | class declarations | Cycle-1 `[DoNotParallelize]` additions are correctly placed, csharpier-conformant, and introduce no production code. | None — accept as-is. | Structural mutual-exclusion fix; completeness gate proves zero unmarked scope-opening classes in `UtilitiesCS.Test`. | completeness-verification.2026-07-09T16-05.md; green-after-fix.2026-07-09T16-05.md |

## Notes

- No Python, TypeScript, or PowerShell files changed; no typed-Python review applies.
- No suppression additions, no analyzer debt, no weakened assertions, no sleeps/retries observed in the diff.
- Cycle 2 is test-attribute-only: the production `.cs` diff between the cycle-1 head (`8f391d8f`) and the
  current head (`87ecc9a`) is empty (verified via `git diff 8f391d8f..87ecc9a -- 'UtilitiesCS/**/*.cs'`).
- All new/modified files are within the 500-line limit (largest changed file: `StoresWrapper.cs` at 469).
</content>
