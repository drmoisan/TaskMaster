# Phase 6 — Toolchain step 4 (confirming): vstest with /EnableCodeCoverage

Timestamp: 2026-09-09T14-43

Task: [P6-T5]

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/EnableCodeCoverage' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p6-t5' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' `
  '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

This is the [P0-T11] command with the results directory changed. It is the literal
`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` step that CLAUDE.md and AC25 name,
run over the two in-scope assemblies with the D10 execution constraints applied.

EXIT_CODE: 0
ExpectedExitCode: 0

The declaration is keyed to this run rather than to the baseline: this run reported no failed test,
so the expectation is 0 and the observed code is 0.

CONFIRMING-TOTAL: 6286
CONFIRMING-PASSED: 6286
CONFIRMING-FAILED: 0
CONFIRMING-FAILED-SET: NONE
NEWLY-FAILING: NONE

Counters read from the TRX `ResultSummary/Counters` element: `total=6286`, `executed=6286`,
`passed=6286`, `failed=0`, `error=0`, `timeout=0`, `aborted=0`, `notExecuted=0`,
`inconclusive=0`, with zero `UnitTestResult` nodes whose outcome is not `Passed`. Per D13 no TRX
content is reproduced.

`NEWLY-FAILING: NONE` holds because `CONFIRMING-FAILED-SET` is empty and the empty set is a subset
of `BASELINE-CONFIRMING-FAILED-SET` from [P0-T11], which was itself `NONE`. No failed name absent
from that baseline set appeared, so the stop-and-report branch was not taken.

Test-count reconciliation: the baseline total was 6284 and this run's total is 6286, a difference
of exactly the two tests this plan adds,
`PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce` and
`PopulateWithCurrent_OnOneFailingStoreReselectedThreeTimes_RetriesLookupOnlyOnce`. The R3 test was
rewritten in place rather than added, so it does not change the count.

Flake observation: the known-intermittent
`Transaction_SecondCallerCannotInstallUntilTheFirstRestores` passed in this run on the first
attempt. The gate was not re-run.

This artifact records no coverage percentage, because `/EnableCodeCoverage` writes a binary
`.coverage` file from which none can be read; the numeric values come from [P6-T6], the measured
run (D11).

Output Summary: `Test Run Successful.` 6286 tests total, 6286 passed, 0 failed, 0 skipped, in 38.24
seconds. Exit code 0, matching the declared expectation. No newly failing test.
