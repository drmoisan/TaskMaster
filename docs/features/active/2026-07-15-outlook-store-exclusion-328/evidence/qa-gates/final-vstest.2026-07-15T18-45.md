# Final QA — MSTest Coverage Run (Issue #328, P4-T5)

Timestamp: 2026-07-15T21-05
Command: dotnet-coverage collect -f cobertura -o final-coverage.2026-07-15T18-45.cobertura.xml "vstest.console.exe UtilitiesCS.Test.dll TaskMaster.Test.dll ToDoModel.Test.dll /InIsolation /Settings:cov.runsettings"
EXIT_CODE: 1 (non-zero solely from the pre-existing Deedle/FSharp coverage-instrumentation flakiness; zero functional failures — see below)

Output Summary:
- Under coverage instrumentation: Total 4612; Passed 4592; Failed 19; Skipped 1.
- Without coverage instrumentation (authoritative for pass/fail): Total 4612; Passed 4611;
  Failed 0; Skipped 1. Verified in a separate vstest run over the same three assemblies with the
  same Workers=4 runsettings and /InIsolation (no dotnet-coverage wrapper).
- The previously-failing `TaskMaster.Test`
  `LoadProjInfoAsync_RebuildsWhenProjectCountIsZeroAndOutlookApplicationIsAvailable` now PASSES
  (confirmed both in an isolated targeted run [Passed, 228 ms] and in the full non-instrumented run).
  The P4-T4 fix added a handled `get_StoresWrapper` fail-open case (returns a null StoresWrapper) to
  the `OlObjectsProxy` test double so `ProjectData.Rebuild`'s
  `storesWrapper is null || storesWrapper.ShouldIncludeStore(store)` predicate treats the proxy as
  not-yet-loaded, letting `Rebuild` reach the `get_Session` access the test asserts.
- The 19 remaining under-instrumentation failures are the pre-existing Deedle/FSharp DataFrame
  coverage-instrumentation flakiness documented at baseline (DeedleDoodles, FromArray2D*, GetEmailData*,
  FromDefaultFolder*, DropFirstN, Exclude*, GetDuplicateEntriesByColumn, FilterToProjectIDs,
  DfToListEntries, GetColumnEid). They pass cleanly without instrumentation (the non-instrumented run
  is 4611/4611 passing) and the failing set is nondeterministic between coverage runs. The prior
  final run's 20th failure (the `get_StoresWrapper` scope conflict) is now resolved.

Per-class line/branch coverage for the four non-exempt target classes (post-change):
- UtilitiesCS.OutlookObjects.Store.StoreFilterAttribution: line 100.00%, branch 96.88%
- UtilitiesCS.OutlookObjects.Store.StoresWrapper:           line 98.42%, branch 89.13%
- UtilitiesCS.OutlookObjects.Store.StoreWrapper:            line 95.31%, branch 64.81%
- UtilitiesCS.OutlookObjects.Store.StoreWrapperController:  line 95.89%, branch 85.38%

Cobertura saved at final-coverage.2026-07-15T18-45.cobertura.xml (same directory). Methodology matches
the P0-T5 baseline (dotnet-coverage collect -f cobertura wrapping vstest, Workers=4 ClassLevel).

## Status: PASS

No unresolved functional failures. The prior BLOCKER (the `OlObjectsProxy` test double lacking a
`get_StoresWrapper` handled case) is resolved by the in-scope P4-T4 edit to
`TaskMaster.Test/AppGlobals/AppToDoObjectsTestDoubles.cs`.
