# P3-T2 — Finding 5 breadcrumb negative sibling: recorded RED

Timestamp: 2026-09-03T23-58

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests.BindBreadcrumbRowsAsync_WhenArchiveRootThrows" "/Logger:trx;LogFileName=p3-t2.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p3-t2
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 1
ExpectedExitCode: 1

## TRX results

Total **1**, passed **0**, failed **1**.

| Test method | Outcome |
|---|---|
| `BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow` | Failed |

## The recorded failure message

```
Expected sinkCallCount to be 1 because the breadcrumb bind must report through the controller's
boundary reporter exactly once, not merely write a log line, but found 0 (difference of -1).
```

The observed sink invocation count was **0** where **1** was expected. That is the log-only bypass
this test exists to pin: `BindBreadcrumbRowsAsync`'s general exception arm writes a bare
`logger.Error` line and never reaches `BoundaryErrorSink`. The method itself did **not** throw, which
is why the failure is on the count rather than on the no-throw clause — the bypass is a reporting
gap, not a containment gap.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory: `p3-t2.trx`. The empty MSTest
deployment directory the failing run created beside it was removed immediately afterwards, for the
D10 reason recorded in P1-T7.

Output Summary: build exited 0; the filtered run exited 1 as expected with TRX total 1, passed 0,
failed 1. The failure states the sink invocation count was 0 where 1 was expected, which is finding
5's breadcrumb log-only bypass. Exactly one TRX file exists under this task's results directory.
