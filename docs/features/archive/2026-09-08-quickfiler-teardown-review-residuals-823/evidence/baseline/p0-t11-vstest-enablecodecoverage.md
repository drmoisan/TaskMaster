# Phase 0 — Confirming step-4 baseline (vstest with /EnableCodeCoverage)

Timestamp: 2026-09-09T13-53

Task: [P0-T11]

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/EnableCodeCoverage' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p0-t11' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' `
  '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

This is the literal CLAUDE.md step-4 command with the D10 execution constraints applied. It is the
CONFIRMING run per D11 and supplies counts only, because `/EnableCodeCoverage` writes a binary
`.coverage` file from which no numeric line rate can be read. The numeric coverage values come from
the measured run in [P0-T12].

EXIT_CODE: 0

BASELINE-CONFIRMING-TOTAL: 6284
BASELINE-CONFIRMING-PASSED: 6284
BASELINE-CONFIRMING-FAILED: 0
BASELINE-CONFIRMING-FAILED-SET: NONE

The three counters were read from the `ResultSummary/Counters` element of the TRX the run wrote
under the gitignored `TestResults/823-p0-t11` directory. The element additionally reports
`executed=6284`, `error=0`, `timeout=0`, `aborted=0`, `notExecuted=0` and `inconclusive=0`, and the
document carries zero `UnitTestResult` nodes whose outcome is not `Passed`. Per D13 no TRX content
is reproduced here: only parsed counters and, where applicable, fully qualified test names.

Note on the known-intermittent test: `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`
passed in this run, so it is not a member of the baseline failed set. Per [P3-T9] and [P6-T5] a
later run in which it fails is dispositioned as an R5 observation rather than as a regression of
this change.

Output Summary: `Test Run Successful.` 6284 tests total, 6284 passed, 0 failed, 0 skipped, in
39.1 seconds. Exit code 0. Baseline failed set is empty.
