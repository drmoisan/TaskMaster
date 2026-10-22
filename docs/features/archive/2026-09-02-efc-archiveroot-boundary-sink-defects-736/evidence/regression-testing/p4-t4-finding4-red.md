# P4-T4 — Finding 4 default-sink tests: recorded RED against the log-only default

Timestamp: 2026-09-04T00-10

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests.BoundaryErrorSink_DefaultDelegate_" "/Logger:trx;LogFileName=p4-t4.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p4-t4
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 1
ExpectedExitCode: 1

## TRX results

Total **3**, passed **2**, failed **1**. The shared `BoundaryErrorSink_DefaultDelegate_` name prefix
necessarily collects the pre-existing test alongside the two P4-T3 added.

| Test method | Outcome | Duration |
|---|---|---|
| `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` (pre-existing) | Passed | 00:00:00.0509175 |
| `BoundaryErrorSink_DefaultDelegate_ReturnsWithoutBlockingTheCallingThread` | Passed | 00:00:00.0008766 |
| `BoundaryErrorSink_DefaultDelegate_RoutesThroughTheUserFaultNotifier` | Failed | 00:00:00.0938472 |

## The single failure

```
Expected captured to contain a single item because the default sink must report the fault to the
user exactly once, not merely write a log line, but the collection is empty.
```

The capture received **0** messages where **1** was expected. That is the log-only default finding 4
names: `BoundaryErrorSink`'s initializer is still `(message, exception) => logger.Error(message, exception)`,
so nothing reaches the injectable user-facing notifier.

The two passes are the non-blocking test and the pre-existing default-delegate test, both of which
the log-only default already satisfies. Their sub-millisecond and 51-millisecond durations confirm
the log-only default does not block, which is the baseline the post-fix durations are compared
against in P4-T6.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory: `p4-t4.trx`. The empty MSTest
deployment directory the failing run created beside it was removed immediately afterwards, for the
D10 reason recorded in P1-T7.

Output Summary: build exited 0; the filtered run exited 1 as expected with TRX total 3, passed 2,
failed 1. The single failure is `BoundaryErrorSink_DefaultDelegate_RoutesThroughTheUserFaultNotifier`,
whose message states the capture was empty where one message was expected. Exactly one TRX file
exists under this task's results directory.
