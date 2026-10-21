# [P1-T9] Pass-after — both assertions pass once the mutation is reverted

Timestamp: 2026-09-06T01-40

Command:

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1

& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
    '/ResultsDirectory:TestResults\782-r1-p1t9' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:FullyQualifiedName~YieldAsync_WithoutDispatcher_RemainsStrict|FullyQualifiedName~Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize'
```

This is the [P1-T4] command with a third results directory, run after [P1-T8] reverted the mutation.

EXIT_CODE: 0

Output Summary: both tests passed. The counts below are read from the TRX `ResultSummary/Counters`
element in `TestResults\782-r1-p1t9`, which contains exactly one `.trx` file and records
`outcome="Completed"` with `error="0"`, `timeout="0"`, `aborted="0"`, `inconclusive="0"`, and
`notExecuted="0"`.

```text
Total tests: 2
Passed: 2
Failed: 0
```

## What the [P1-T7] and [P1-T9] pair establishes together

The same two tests, the same command, and the same assembly differ in outcome only by the presence of
the appended tail at the `WpfDispatcherYield` throw site:

| Run | Mutation | `YieldAsync_WithoutDispatcher_RemainsStrict` | `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` | Exit |
|---|---|---|---|---|
| [P1-T7] | applied | Failed | Passed | 1 |
| [P1-T9] | reverted | Passed | Passed | 0 |

The assertion therefore distinguishes the delivered message from a tail-restored one. It does so at
the `WpfDispatcherYield` throw site specifically: the sibling test passed in both runs, because the
mutation did not touch the `UiThread.Dispatcher` throw site the sibling reaches.

Neither run says anything about an edit to the constant's own wording. An assertion written against
the constant moves with the constant, so both assertions would continue to pass after such an edit.
The one part of that wording a test holds is the substring `UiThread.Init()`, asserted at
`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:196` with
`Message.Should().Contain("UiThread.Init()")`, which this remediation does not change.
