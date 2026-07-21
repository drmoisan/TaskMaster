# Baseline Full-Suite Test Run (pre-fix, Issue #354)

Timestamp: 2026-07-18T14:12:32Z

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /EnableCodeCoverage` (run from repo root on branch `bug/stale-app-config-binding-redirects-354`, pre-fix state)

EXIT_CODE: 0

Output Summary:
- **Total tests: 5468. Passed: 5468. Failed: 0.** "Test Run Successful." No failing test class or method was reported by this run, including `QfcHomeControllerMetricsTests` and `QfcStreamingDequeueConfidenceGateTests` (both present in `QuickFiler.Test` and both passed all their test methods, e.g. `QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow`, `GetMoveDiagnostics_NullAppointment_DoesNotThrow`, `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`).
- Note: the issue narrative (`issue.md`) describes 8 of 21 tests failing with `FileLoadException` for `Microsoft.Bcl.TimeProvider`; in this checked-out working-tree state, `QuickFiler.Test.csproj`'s `Microsoft.Bcl.TimeProvider` `<Reference Version=...>` (`10.0.0.7`) already matches `app.config`'s `bindingRedirect newVersion` (`10.0.0.7`) for that specific package, so that specific reproduction does not fail here. The broader defect (57 stale redirects across other packages/projects per the issue's root-cause analysis) is still present and is what Phase 1's `fix_binding_redirects.py` run targets. This baseline is recorded faithfully as observed: 0 failing tests solution-wide.
- Coverage: `.coverage` file produced at `TestResults\5701da79-30d4-4820-8aa6-922c54e6979f\DanMoisan_MEGALODON4_2026-07-18.10_13_18.coverage`, converted via `dotnet-coverage merge ... -f cobertura` for a numeric headline. Aggregate Cobertura `line-rate="0.7104764851155075"` (lines-covered 133198 / lines-valid 187477) => **71.05% aggregate line coverage** across all instrumented assemblies (first-party + vendored). This is the pre-fix coverage baseline for comparison against the post-fix run.
- Total time: 45.04 seconds.
