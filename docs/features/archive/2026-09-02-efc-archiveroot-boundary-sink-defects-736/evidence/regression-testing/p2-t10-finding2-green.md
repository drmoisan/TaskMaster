# P2-T10 — Finding 2 six-method set: recorded GREEN after the minimal fix

Timestamp: 2026-09-03T23-56

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests.KbdExecuteAsync_|FullyQualifiedName~EfcFormControllerTests.RunKbdGuardedAsync_" "/Logger:trx;LogFileName=p2-t10.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p2-t10
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 0

## TRX results

Total **6**, passed **6**, failed **0**. `Test Run Successful.`

All six methods that P2-T8 recorded red are green: the two overload-containment tests, the null-sink
and throwing-sink tests, and the two classification tests. Exactly **one** TRX file exists under this
task's results directory: `p2-t10.trx`.

## P2-T9's recorded observations

P2-T9 is a source edit that writes no evidence artifact of its own. Its three counted observations,
measured in `QuickFiler/Controllers/EfcFormController.cs` after the fix and after the file was
formatted:

1. **`catch (` line count: 12** — exactly two greater than the 10 P0-T9 recorded. The two added arms
   are `catch (OperationCanceledException)` and `catch (System.Exception ex)`, both inside
   `RunKbdGuardedAsync`. Neither arm rethrows.
2. **`TryReportBoundaryFault` occurrence count: 8** — exactly one greater than the recorded 7. The
   single added occurrence is the invocation in `RunKbdGuardedAsync`'s general arm.
3. **Post-fix line count: 1247**, within the D7 budgeted ceiling of 1330.

Supporting observations recorded alongside: the file still carries exactly **two** `KbdExecuteAsync`
declarations, exactly **one** `RunKbdGuardedAsync` declaration and exactly **two** invocations of it,
and its `logger.Debug` count rose from 2 to 3 for the cancellation arm.

## AC5 note — the two `TryReportBoundaryFault` branches

`KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow` and
`KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow`, both authored in P2-T2 and both recorded
green above, are the tests that reach the null-sink branch and the throwing-sink branch of
`TryReportBoundaryFault`. Those two branches were uncovered before this item.

Output Summary: the build exited 0 and the filtered run exited 0 with TRX total 6, passed 6, failed
0. Exactly one TRX file exists under this task's results directory. P2-T9's post-fix counts in
`QuickFiler/Controllers/EfcFormController.cs` are `catch (` = 12, `TryReportBoundaryFault` = 8, and a
file line count of 1247.
