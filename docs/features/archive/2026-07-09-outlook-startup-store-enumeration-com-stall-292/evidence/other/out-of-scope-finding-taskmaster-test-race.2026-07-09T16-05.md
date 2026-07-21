# NEW Out-of-Scope Finding — TaskMaster.Test CurrentStoreContext race (issue #292 cycle 1)

- Timestamp: 2026-07-09T16-05
- Discovered during: [P3-T4] final coverage/test verification
- Status: NOT remediated (outside the plan's `UtilitiesCS.Test`-only scope). Reported for a follow-up plan revision.

## Summary

`TaskMaster.Test` contains a second, independent instance of the same #292 process-global-static
`CurrentStoreContext` race that the plan fixed in `UtilitiesCS.Test`. The plan (and the
remediation-inputs) scoped the fix to `UtilitiesCS.Test` because CI run 29046195330 reported all 10
failures in `UtilitiesCS.Test`. This finding shows the defect class also exists in `TaskMaster.Test`.

## Evidence

- Class: `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` — a #292 regression
  test class that BOTH opens a `CurrentStoreContext` scope (`StoresWrapper.Init()` at lines 43/105,
  `StoresWrapper.RewireAfterDeserializeAsync()` at line 74 -> `MaterializeFilteredStores` -> `Begin`)
  AND asserts `CurrentStoreContext.Current.Should().BeNull(...)` (T4 line 115, T5 line 136). It is NOT
  marked `[DoNotParallelize]` (line 24 is `[TestClass]` only).
- Additional `TaskMaster.Test` null-baseline reader: `TaskMaster.Test/AppGlobals/AppOlObjectsAttributionContextTests.cs`
  (asserts `CurrentStoreContext.Current`), also unmarked. Additional `TaskMaster.Test` type-referencing
  classes: `AppOlObjectsTests`, `AppOlObjectsCoverageTests`, `StoresWrapperTests` (some execute
  `RewireAfterDeserializeAsync`/reflection source analysis).
- Failure reproduction (instrumented `dotnet-coverage collect` path with `TaskMaster.cli.runsettings`):
  - Full 7-assembly set, post-fix: 2 failures — `Init_HealthyMultiStore_...ClearsContextAfterReturn`,
    `Init_EnumerationThrowsMidStream_LeavesCurrentStoreContextNull` (both assert `Current == null`,
    found `"<Stores-enumeration>"`).
  - `TaskMaster.Test` ALONE (single assembly, no `UtilitiesCS.Test`): run 1 = 3 failures
    (adds `RewireOlObjectsAsync_MaterializingStores_...InsideMoveNext`), run 2 = 2 failures.
  - Because it reproduces with `TaskMaster.Test` run ALONE, the pollution is INTRA-`TaskMaster.Test`
    (a `TaskMaster.Test` writer class running in the parallel bucket alongside these null-baseline
    readers), NOT a cross-assembly leak from `UtilitiesCS.Test`.

## Not caused by this remediation

This remediation only added `[DoNotParallelize]` to `UtilitiesCS.Test` classes; it never touches
`TaskMaster.Test`. `StoresWrapperEnumerationScopeTests` and its `Current == null` assertions predate this
change. The race is pre-existing and independent of the fix.

## Root of the exposure difference: TaskMaster.Test has NO `[assembly: Parallelize]`

`TaskMaster.Test` contains no `[assembly: Parallelize]` attribute (grep confirms none in the assembly or
`Properties/AssemblyInfo.cs`), so by MSTest default it runs its classes SEQUENTIALLY. Under that default
there is no concurrent writer, so `StoresWrapperEnumerationScopeTests` is race-free.

The failures appear ONLY under the coverage-measurement invocation, which passes
`/Settings:scripts/vscode/TaskMaster.cli.runsettings`. That runsettings declares
`<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`, which
FORCE-imposes class-level parallelization on every assembly in the run, including `TaskMaster.Test`. Only
then does the parallel bucket exist for `TaskMaster.Test`, activating the latent race.

Consequently:
- Under the CI-equivalent `/EnableCodeCoverage /InIsolation` invocation (NO `/Settings:`), `TaskMaster.Test`
  runs sequentially: 5 of 5 post-fix full-suite passes were green, and the original CI run 29046195330
  showed no `TaskMaster.Test` failures. This is NOT a CI-gate risk under the current configuration.
- The race is exposed only when a runsettings (the repo's coverage-measurement `TaskMaster.cli.runsettings`)
  force-parallelizes `TaskMaster.Test`. It is a latent robustness gap, not a live CI failure.

Note: `TaskMaster.Test/AppGlobals/AppOlObjectsAttributionContextTests.cs` was already proactively marked
`[DoNotParallelize]` ("because it reads the process-global"), showing the team is aware of this hazard in
`TaskMaster.Test`; `StoresWrapperEnumerationScopeTests` (and any `TaskMaster.Test` writer) were simply
missed.

## Recommended follow-up (separate remediation / plan revision)

Extend the plan's approach (A) to `TaskMaster.Test`: add `[DoNotParallelize]` to every `TaskMaster.Test`
class that opens a `CurrentStoreContext` scope or asserts `CurrentStoreContext.Current` (at minimum
`StoresWrapperEnumerationScopeTests` and `AppOlObjectsAttributionContextTests`, plus a census of
`AppOlObjectsTests`/`StoresWrapperTests`/`AppOlObjectsCoverageTests` writers). Because the pollution is
intra-assembly, approach (A) applies directly and is provably correct there too. This requires a plan
revision because the current plan hard-constrains changes to `UtilitiesCS.Test` only; per the execution
directive, this executor did not widen scope to make the change.
