# P2-T8 — Finding 2 full six-method set: recorded RED against the unguarded seam

Timestamp: 2026-09-03T23-55

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests.KbdExecuteAsync_|FullyQualifiedName~EfcFormControllerTests.RunKbdGuardedAsync_" "/Logger:trx;LogFileName=p2-t8.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p2-t8
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 1
ExpectedExitCode: 1

## TRX results

Total **6**, passed **0**, failed **6**.

| Test method | Outcome |
|---|---|
| `KbdExecuteAsync_FuncTaskOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow` | Failed |
| `KbdExecuteAsync_ActionOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow` | Failed |
| `KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow` | Failed |
| `KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow` | Failed |
| `RunKbdGuardedAsync_WhenBodyThrowsOperationCanceled_DoesNotReportAsFault` | Failed |
| `RunKbdGuardedAsync_WhenBodyThrowsInvalidOperation_ReportsExactlyOnce` | Failed |

## The two classification failures record the exception each body threw

Both classification bodies threw their own exception, and both escaped the still-unguarded
`RunKbdGuardedAsync` seam that P2-T5 landed with the defect preserved:

- `RunKbdGuardedAsync_WhenBodyThrowsOperationCanceled_DoesNotReportAsFault`:
  `Did not expect any exception because cancellation must not propagate out of the guard, but found System.OperationCanceledException: The operation was canceled.`
- `RunKbdGuardedAsync_WhenBodyThrowsInvalidOperation_ReportsExactlyOnce`:
  `Did not expect any exception because the guard must contain the fault, not rethrow it, but found System.InvalidOperationException: the dispatched action failed`

The other four failures continue to name `System.NullReferenceException`, the uncontained fault the
null `_homeController` raises inside the guarded body the two `KbdExecuteAsync` overloads now supply.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory: `p2-t8.trx`. The empty MSTest
deployment directory the failing run created beside it was removed immediately afterwards, for the
D10 reason recorded in P1-T7 and P2-T4.

Output Summary: build exited 0; the filtered run exited 1 as expected with TRX total 6, passed 0,
failed 6. The two classification failures name `System.OperationCanceledException` and
`System.InvalidOperationException` respectively — the exact exception each body threw, escaping the
unguarded seam. Exactly one TRX file exists under this task's results directory.
