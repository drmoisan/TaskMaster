# P3-T3 — Scoped Post-Edit Regression Run

Timestamp: 2026-09-03T11-32
Command:
1. MSBuild.exe TaskMaster.sln -t:Build -m -p:Configuration=Debug -p:Platform="Any CPU"
2. MSYS_NO_PATHCONV=1 vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
   /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
   /TestCaseFilter:"FullyQualifiedName~QfcHomeControllerMetricsTests|FullyQualifiedName~EfcHomeControllerMetricsTests"
(all executables and path arguments passed as absolute paths into the item worktree; dash-switch
MSBuild form and MSYS_NO_PATHCONV=1 used for the same git-bash mangling reasons recorded in
P0-T11/P0-T14)
EXIT_CODE: 0 (both steps)
Output Summary: Build succeeded, 5 Warning(s) (same System.Reactive packages.config notices), 0
Error(s). vstest: "Test Run Successful. Total tests: 27, Passed: 27." 0 failed. Confirms
WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps,
QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine, and
BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine (the EFC fixed-clock test) all
pass under the corrected `HH:mm` rendering and the updated test literals.
