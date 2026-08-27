# Six Regression Tests Pass (P2-T4)

Timestamp: 2026-08-27T11-01
Task: [P2-T4]
Command: `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings /TestCaseFilter:"FullyQualifiedName~QfcItemController_UiThreadDispatcherFixtureTests" /Logger:"trx;LogFileName=regression.trx" /ResultsDirectory:TestResults\plan-logs\p2-t4`
EXIT_CODE: 0
Output Summary: `Test Run Successful.` Total tests 6, **Passed: 6**, **Failed: 0**, in 1.353 s. All six
R1-R6 tests pass under the class-level parallelized runsettings.

## Acceptance verification

| Item | Required | Observed |
| --- | --- | --- |
| `EXIT_CODE` | 0 | 0 |
| Passed | 6 | 6 |
| Failed | 0 | 0 |
| Fully-qualified names | exactly the six R1-R6 names | six, listed below |

`Failed: 0` is recorded from the run reporting `Total tests: 6` with `Passed: 6` and emitting no
`Failed` result line and no `Failed:` summary row; `vstest.console` omits that row when the count is
zero.

## The six fully-qualified test names

Read from the TRX `TestDefinitions` rather than transcribed from the console, so the class and
namespace are exact:

```
QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt
QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose
QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.EnsureDispatcher_ScopeDisposedTwice_IsIdempotent
QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores
QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_DisposedTwice_DoesNotOverReleaseTheGate
QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException
```

Mapping to the § Regression Tests table, with the observed per-test durations:

| # | Method name | Result | Duration |
| --- | --- | --- | --- |
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | Passed | 71 ms |
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | Passed | 1 ms |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | Passed | 3 ms |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | Passed | 3 ms |
| R5 | `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | Passed | 2 ms |
| R6 | `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` | Passed | 4 ms |

The set is exactly the six names the plan's § Regression Tests table specifies; there is no seventh
name and none is missing.

## Run configuration

The run used `/Settings:scripts\vscode\TaskMaster.cli.runsettings`, and the runner reported
`Test Parallelization enabled for <repo-root>/QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
(Workers: 24, Scope: ClassLevel)`, confirming that class-level parallelization was active — the
configuration in which the #493 race is reachable. Every test completed well inside its
`[Timeout(GateTimeoutMs)]` bound of 60 000 ms; the slowest was R1 at 71 ms.

TRX name is controlled by `LogFileName=regression.trx`, so it carries no account or host name. Raw
artifacts live under the git-ignored `TestResults/plan-logs/p2-t4/` tree and are not committed.
