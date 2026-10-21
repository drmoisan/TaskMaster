# [P1-T4] The two assertion tests pass with the constant-reference form

Timestamp: 2026-09-06T01-37

Command:

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1

& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
    '/ResultsDirectory:TestResults\782-r1-p1t4' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:FullyQualifiedName~YieldAsync_WithoutDispatcher_RemainsStrict|FullyQualifiedName~Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize'
```

The `/Blame:` switch is single-quoted so PowerShell does not truncate it at the first semicolon. The
`/TestCaseFilter:` expression uses `|` as its disjunction operator; `OR` is not a vstest filter
operator and would select nothing.

EXIT_CODE: 0

Output Summary: both targeted tests passed. The counts below are read from the TRX
`ResultSummary/Counters` element in `TestResults\782-r1-p1t4`, which contains exactly one `.trx`
file and records `outcome="Completed"` with `error="0"`, `timeout="0"`, `aborted="0"`,
`inconclusive="0"`, and `notExecuted="0"`.

```text
Total tests: 2
Passed: 2
Failed: 0
```

The two fully-qualified test identifiers selected and executed are:

- `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`
- `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`

## Why `Total tests: 2` is asserted and not `Passed: 2` alone

An over-broad filter would raise the total above 2 while still reporting every selected test as
passed, so a `Passed:` assertion alone could not detect it. Asserting the total pins the selection as
well as the outcome.

## What this run establishes and what it does not

It establishes that both assertions pass against the message the shared constant currently supplies.
It does not by itself establish that either assertion can fail; that is what [P1-T5] through [P1-T9]
observe, by appending the removed tail at the `WpfDispatcherYield` throw site and recording which of
the two tests fails.
