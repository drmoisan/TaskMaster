# Regression Testing — Fail-Before Run (Issue #656)

Timestamp: 2026-09-01T14-42
Task: [P1-T3] [expect-fail]

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
New-Item -ItemType Directory -Force -Path 'TestResults\p1-t3' | Out-Null
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain' '/Logger:trx' '/ResultsDirectory:TestResults\p1-t3'
```

EXIT_CODE: 1
ExpectedExitCode: 1

## TRX counter values

- `total` = 1
- `passed` = 0
- `failed` = 1

Read from `TestRun/ResultSummary/Counters` of the TRX written to
`<repo-root>/TestResults/p1-t3/<user>_<host>_2026-09-01_14_42_14_net481.trx`.

## Failure message (copied from the TRX `UnitTestResult` node)

```
Expected harness.Host.CloseReasons to be equal to {BreadcrumbDropDownCloseReason.Uncommitted {value: 1}, BreadcrumbDropDownCloseReason.Uncommitted {value: 1}} because the close after a bypassing reopen must reach _host.Close a second time, but {BreadcrumbDropDownCloseReason.Uncommitted {value: 1}} contains 1 item(s) less.
```

## Why a direct vstest invocation rather than the wrapper

Neither `scripts/vscode/Invoke-MSTest.ps1` nor `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
accepts a `TestCaseFilter` override, and editing either script is outside this item's authorized
footprint. The direct invocation reproduces both wrapper protections explicitly: `/InIsolation` is
passed, and the `TestCategory!=LiveOutlook` conjunct is the first term of the filter, so no real
Outlook process can be launched. The run is scoped to the single new test by name because the full
suite is not run while a test is deliberately failing — a full-suite gate could not exit 0 in that
state.

Output Summary: The new regression test ran alone and failed as expected. Exit code 1, which equals
the declared expectation, so this gate is normalized to pass. The TRX reports 1 total, 0 passed, 1
failed. The failure is a runtime assertion failure, not a compile failure: the assembly built
cleanly in P1-T2 against unmodified production code.

## Red Cause:

The observed `CloseReasons` collection held exactly **one** element (`Uncommitted`) where **two**
were expected, which is the outcome the fail-before requirement predicts. The suppressing guard is
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:316`, the line `if (_closeCompleted)`
inside `CloseCore`'s `lock (_sync)` critical section, followed by `return true;` at `:317`.

Mechanism: the test drives a successful open, then a close the host accepts, which sets
`_closeCompleted` to `true` at `:335`. It then reopens the host through
`harness.Host.SetOpen(true)`, a path that reaches neither `RequestOpen` nor `Invalidate` and
therefore does not clear `_closeCompleted`. The second `SetDroppedDown(false)` reaches `CloseCore`,
the guard at `:316` observes `_closeCompleted == true`, and the method returns `true` without ever
calling `_host.Close(reason)` at `:323`. No second reason is appended to `CloseReasons`, so the
collection is short by exactly one element — precisely what the failure message reports.

Production file still unmodified at this point:
`@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'if (_closeCompleted)').Count`
equals **1**, unchanged from the P0-T12 baseline. The red is therefore observed against HEAD
production code, and no part of the Phase 2 fix had been applied when it was recorded.
