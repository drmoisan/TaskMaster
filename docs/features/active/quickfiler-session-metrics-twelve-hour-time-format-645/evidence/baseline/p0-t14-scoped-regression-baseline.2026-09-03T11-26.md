# P0-T14 — Scoped Pre-Edit Regression Baseline

Timestamp: 2026-09-03T11-26
Command: MSYS_NO_PATHCONV=1 vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
/Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
/TestCaseFilter:"FullyQualifiedName~QfcHomeControllerMetricsTests|FullyQualifiedName~EfcHomeControllerMetricsTests"
(vstest.console.exe and all path arguments passed as absolute paths into the item worktree;
MSYS_NO_PATHCONV=1 prefix used so git-bash does not path-convert the `/Settings:`, `/InIsolation`,
and `/TestCaseFilter:` switches)
EXIT_CODE: 0
Output Summary: "Test Run Successful. Total tests: 27, Passed: 27." 0 failed. Confirms the
two affected test classes (QfcHomeControllerMetricsTests, EfcHomeControllerMetricsTests) — and
in particular WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps,
QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine, and
BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine — currently pass under the
pre-fix 12-hour `hh:mm` literal. This is the fail-before-alternative evidence per Plan-Level
Decision 3.
