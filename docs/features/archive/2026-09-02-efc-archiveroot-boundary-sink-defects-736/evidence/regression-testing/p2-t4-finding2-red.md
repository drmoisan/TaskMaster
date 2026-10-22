# P2-T4 — Finding 2 overload-containment and sink-branch tests: recorded RED

Timestamp: 2026-09-03T23-52

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests.KbdExecuteAsync_" "/Logger:trx;LogFileName=p2-t4.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p2-t4
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 1
ExpectedExitCode: 1

## TRX results

Total **4**, passed **0**, failed **4**.

| Test method | Outcome |
|---|---|
| `KbdExecuteAsync_FuncTaskOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow` | Failed |
| `KbdExecuteAsync_ActionOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow` | Failed |
| `KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow` | Failed |
| `KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow` | Failed |

The class-name prefix in the filter is load-bearing: QuickFiler.Test's QfcItemController.NavigationTests.cs
declares methods beginning with the same `KbdExecuteAsync_` prefix, and a bare method-name operand
would have collected them too. The filter collected exactly the four methods P2-T2 added.

## Each failure names `NullReferenceException`

The four recorded messages, with the stack frames elided:

- `KbdExecuteAsync_FuncTaskOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow`:
  `Did not expect any exception because the keyboard dispatch overload must contain the fault, not rethrow it, but found System.NullReferenceException: Object reference not set to an instance of an object.`
- `KbdExecuteAsync_ActionOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow`:
  `Did not expect any exception because the synchronous dispatch overload must contain the fault, not rethrow it, but found System.NullReferenceException: Object reference not set to an instance of an object.`
- `KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow`:
  `Did not expect any exception because a null sink must not turn a contained fault into a rethrow, but found System.NullReferenceException: Object reference not set to an instance of an object.`
- `KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow`:
  `Did not expect any exception because a throwing sink must not escape the dispatch boundary, but found System.NullReferenceException: Object reference not set to an instance of an object.`

`NullReferenceException` is the fault the null `_homeController` raises inside `KbdExecuteAsync` at
its first statement, `await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();`. Neither
overload has any local handler today, so the fault escapes — which is finding 2 exactly as spec.md
states it.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory: `p2-t4.trx`. The empty MSTest
deployment directory that the failing run created beside it was removed immediately afterwards,
because its directory name carries the account and machine tokens D10 forbids in a committed
artifact and P6-T12's sweep rewrites file content only.

Output Summary: build exited 0; the filtered run exited 1 as expected with TRX total 4, passed 0,
failed 4. Every failure message names `System.NullReferenceException`, the uncontained fault raised
by the null `_homeController` inside `KbdExecuteAsync`. Exactly one TRX file exists under this task's
results directory.
