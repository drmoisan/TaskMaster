# Phase 3 — AC12 fail-before (expect-fail)

Timestamp: 2026-09-09T14-19

Task: [P3-T3] [expect-fail]

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p3-t3' `
  '/TestCaseFilter:FullyQualifiedName~Register_NullControlOrNullPredicate_IsRejected'
```

EXIT_CODE: 1
ExpectedExitCode: 1

TOTAL: 1
FAILED: 1

FAILURE-REASON: The silent-return guard at `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs:44`
returned without throwing, so the first FluentAssertions throw assertion observed no exception at
all. The verbatim message the run printed was:

```
Expected a <System.ArgumentNullException> to be thrown because a null owner is rejected, not ignored, but no exception was thrown.
```

The failing frame is the `registerNullControl` throw assertion in
`QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`. The absolute paths that appeared in
the printed stack trace are not reproduced here (D14). The run was executed twice, once to capture
the tail of the console output and once to capture its head, because the FluentAssertions community
licence banner is written to standard output after the failure block; both runs produced the same
verdict, exit code and message, and the second wrote to `TestResults\823-p3-t3b`.

Output Summary: `Test Run Failed.` 1 test total, 1 failed, in 1.26 seconds. Exit code 1, matching
the declared expectation. The failure is the intended contract gap: the production guard returns
silently where the rewritten test requires an `ArgumentNullException`.
