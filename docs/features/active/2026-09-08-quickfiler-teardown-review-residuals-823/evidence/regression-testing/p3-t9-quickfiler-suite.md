# Phase 3 — Whole QuickFiler.Test suite after the R3 change

Timestamp: 2026-09-09T14-23

Task: [P3-T9]

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p3-t9' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' `
  '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

These are the [P0-T11] arguments with only the `QuickFiler.Test` assembly named and
`/EnableCodeCoverage` dropped. The four shell-icon exclusion clauses are retained harmlessly,
because those four classes live in `UtilitiesCS.Test`.

EXIT_CODE: 0
ExpectedExitCode: 0

The declaration is keyed to this run rather than to the baseline: this run reported no failed test,
so the expectation is 0 and the observed code is 0.

POST-FAILED-SET: NONE
NEWLY-FAILING: NONE

`NEWLY-FAILING: NONE` holds because `POST-FAILED-SET` is empty, and the empty set is a subset of
the `QuickFiler.Test` subset of `BASELINE-CONFIRMING-FAILED-SET` from [P0-T11], which was itself
`NONE`.

R5 observation: the known-intermittent
`Transaction_SecondCallerCannotInstallUntilTheFirstRestores` did NOT fail in this run, so no
observation row is added to the flake-watch log on account of it here. The task's carve-out branch
was therefore not taken.

Output Summary: `Test Run Successful.` 1393 tests total, 1393 passed, 0 failed, in 13.23 seconds.
Exit code 0. No test in the `QuickFiler.Test` assembly regressed under the R3 change.
