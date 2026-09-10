# Phase 2 — Whole UtilitiesCS.Test suite after the R1 change

Timestamp: 2026-09-09T14-13

Task: [P2-T12]

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p2-t12' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' `
  '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

These are the [P0-T11] arguments with only the `UtilitiesCS.Test` assembly named, the results
directory changed, and `/EnableCodeCoverage` dropped. The four shell-icon exclusion clauses are
retained per D10.

EXIT_CODE: 0
ExpectedExitCode: 0

The declaration is keyed to this run rather than to the baseline: this run reported no failed test,
so the expectation is 0 and the observed code is 0.

POST-FAILED-SET: NONE
NEWLY-FAILING: NONE

`NEWLY-FAILING: NONE` holds because `POST-FAILED-SET` is empty, and the empty set is a subset of
the `UtilitiesCS.Test` subset of `BASELINE-CONFIRMING-FAILED-SET` from [P0-T11], which was itself
`NONE`.

Output Summary: `Test Run Successful.` 4893 tests total, 4893 passed, 0 failed, in 30.35 seconds.
Exit code 0. No test in the `UtilitiesCS.Test` assembly regressed under the R1 change.
