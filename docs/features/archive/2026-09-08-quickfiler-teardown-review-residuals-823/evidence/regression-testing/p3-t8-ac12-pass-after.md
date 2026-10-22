# Phase 3 — AC12 pass-after, whole registry test class

Timestamp: 2026-09-09T14-22

Task: [P3-T8]

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p3-t8' `
  '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupOwnerRegistryTests'
```

Running the whole class rather than the one rewritten case is deliberate: the class also owns
`AnyOpen_WithNoRegistration_ReportsFalse`, which is the issue #677 contract that the rejection
change must not disturb.

EXIT_CODE: 0

TOTAL: 6
PASSED: 6
FAILED: 0

`PASSED` equals `TOTAL` and is greater than or equal to 5.

Per-test results:

- `Register_NullControlOrNullPredicate_IsRejected` — PASSED. This is the rewritten AC12 test that
  [P3-T3] recorded failing against the pre-fix tree.
- `AnyOpen_WithNoRegistration_ReportsFalse` — PASSED. The issue #677 contract is intact.
- `AnyOpen_SingleOwnerReportingClosed_ReportsFalse` — passed.
- `AnyOpen_SingleOwnerReportingOpen_ReportsTrue` — passed.
- `AnyOpen_TwoOwnersOneReportingOpen_ReportsTrue` — passed.
- `Register_SameControlTwice_ReplacesRatherThanAppends` — passed.

Output Summary: `Test Run Successful.` 6 tests total, 6 passed, 0 failed, in 1.35 seconds. Exit
code 0. The rejection test passes and no sibling test in the class regressed.
