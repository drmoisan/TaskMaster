# Fail-Before Dossier (Cycle 2, Issue #292)

Timestamp: 2026-07-09T17-45
Command: `vstest.console.exe TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /EnableCodeCoverage`
EXIT_CODE: 1 (Test Run Failed — 2 of 251 failed, pre-fix)

## WhyFailingRunImpossible

The defect is a probabilistic shared-static data race, not a deterministic assertion failure. Under the VS Code
`TaskMaster.cli.runsettings` (which force `ClassLevel` parallelization), whether the unmarked
`StoresWrapperEnumerationScopeTests` null-baseline reads observe pollution depends on the MSTest worker
scheduling relative to concurrent `CurrentStoreContext` scope-openers in sibling classes. A single run is
therefore not guaranteed to fail. Under the required CI invocation form the failure is structurally impossible
because `TaskMaster.Test` carries no `[assembly: Parallelize]` and runs sequentially.

Note: on this machine the pre-fix ClassLevel run DID reproduce the race deterministically enough to fail twice
in a row (see below), so an actual failing run was captured in addition to this dossier.

## Alternative static proof

- The target class `StoresWrapperEnumerationScopeTests` opens a `CurrentStoreContext` process-global-static scope
  via `wrapper.Init()` (L43) and `RewireAfterDeserializeAsync()` (L74), and reads the null baseline
  (`CurrentStoreContext.Current ... BeNull()`, L115/L136), yet is not marked `[DoNotParallelize]`.
- This is the same defect class as the cycle-1 `UtilitiesCS.Test` race, which produced Blocking CI failures and
  was fixed with `[DoNotParallelize]`.
- The sibling `AppOlObjectsAttributionContextTests` was already proactively marked `[DoNotParallelize]`; this new
  class and two other confirmed scope-openers (`StoresWrapperTests`, `AppOlObjectsCoverageTests`) were missed.

## Observed failing run (captured)

Pre-fix ClassLevel run produced Failed: 2 of 251, both in `StoresWrapperEnumerationScopeTests`:
`Init_HealthyMultiStore_PreservesIncludedSetAndOrder_AndClearsContextAfterReturn` and
`Init_EnumerationThrowsMidStream_LeavesCurrentStoreContextNull`. Reproduced identically on a second run.
