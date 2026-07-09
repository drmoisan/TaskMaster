# Remediation Inputs — Cycle 2 (Issue #292)

- **Entry timestamp:** 2026-07-09T17-45
- **Trigger:** New finding surfaced during cycle-1 execution and confirmed by the cycle-1 re-audit (feature-review `2026-07-09T17-40`). Processed as a discrete cycle per the Scope-change Rule (a new finding is handled by the next cycle, not folded into the active plan).
- **Canonical issue number:** 292
- **Prior cycle:** cycle 1 exited clean (blocking_count 0) after fixing the `UtilitiesCS.Test` shared-static parallel race with `[DoNotParallelize]`.

## Finding 1 — Test-determinism robustness gap in this PR's new regression test (non-CI-blocking; closing in-PR per reviewer recommendation)

**Severity: Major (non-blocking under the required CI gate; fixed here to eliminate deferred determinism debt in the PR's own new test).**

### Evidence

- Source: cycle-1 executor out-of-scope finding — `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/evidence/other/out-of-scope-finding-taskmaster-test-race.2026-07-09T16-05.md` — and the cycle-1 re-audit code review `code-review.2026-07-09T17-40.md`.
- `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` (the AC3/AC4 regression tests added by this PR) opens a `CurrentStoreContext` scope (via `wrapper.Init()` / `RewireAfterDeserializeAsync()`) AND is a null-baseline reader (T4/T5 assert `CurrentStoreContext.Current.Should().BeNull()`), and is NOT marked `[DoNotParallelize]`.
- It is the same defect class as the cycle-1 `UtilitiesCS.Test` race: a `CurrentStoreContext` process-global-static writer/reader that can observe pollution under class-level parallelization.
- It is **race-free under the actual required CI check**: `TaskMaster.Test` has no `[assembly: Parallelize]`, so under the CI invocation `vstest.console.exe <all *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` it runs sequentially (5/5 green passes observed in cycle 1). It flakes only under the non-gate VS Code coverage runsettings `scripts/vscode/TaskMaster.cli.runsettings`, which force `ClassLevel` parallelization on all assemblies.
- The sibling `TaskMaster.Test/AppGlobals/AppOlObjectsAttributionContextTests.cs` was already proactively marked `[DoNotParallelize]`; this new class was simply missed.

### Required remediation

- Mark `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` `[DoNotParallelize]` (matching the already-applied `AppOlObjectsAttributionContextTests` pattern), and census `TaskMaster.Test` for any other `CurrentStoreContext` scope-opening / null-baseline-reader class that is unmarked; mark any found.
- Do NOT change production code, the enumeration-phase scope, the reader assertions, or the `CurrentStoreContext` static design.
- Verify determinism under BOTH the CI invocation and the VS Code coverage runsettings (`scripts/vscode/TaskMaster.cli.runsettings`) that forces `ClassLevel` parallelization, so the class is robust under any runsettings. No sleeps/retries/timing hacks; no coverage regression.
