# QA Gate — Standing-Guard Regression Run (Issue #656)

Timestamp: 2026-09-01T14-48
Task: [P3-T4]

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
New-Item -ItemType Directory -Force -Path 'TestResults\p3-t4' | Out-Null
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook&(FullyQualifiedName~PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose|FullyQualifiedName~SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired|FullyQualifiedName~RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync|FullyQualifiedName~CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce|FullyQualifiedName~PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen)' '/Logger:trx' '/ResultsDirectory:TestResults\p3-t4'
```

EXIT_CODE: 0

## TRX counter values

- `total` = 5
- `passed` = 5
- `failed` = 0

Console summary: `Test Run Successful.` / `Total tests: 5` / `Passed: 5`.

## Per-test outcomes (from the TRX `UnitTestResult` nodes)

| Test | Outcome | Contract it guards |
|---|---|---|
| `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` | Passed | Repeated-close suppression while the host open is still pending. `hostOpen` is `false` on the second drive, the added conjunct is `true`, and suppression is retained. |
| `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` | Passed | Repeated-close suppression after an accepted close. `ControlledHost.Close` sets `IsOpen = false`, so `hostOpen` is `false` on the second drive and suppression is retained. |
| `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync` | Passed | The `RequestOpen` path after the same `SetOpen(true)` bypass the new test uses. Exercises `RequestOpen`, not `CloseCore`, and is unaffected by the change. |
| `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce` | Passed | The standing guard that rules out clearing the flag on the successful-close path. No reopen occurs, `hostOpen` is `false`, and the close reaches the host exactly once. |
| `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` | Passed | Closing while the host reports not open is required behavior. `_closeCompleted` is `false` on that first close, so the added conjunct cannot suppress it. This is why a bare `!_host.IsOpen` gate would have been wrong. |

All five recorded names appear in the list above, and every outcome is `Passed`.

## Filter shape

The `FullyQualifiedName` disjunction is parenthesised so that the `TestCategory!=LiveOutlook`
conjunct applies to the whole group rather than binding only to the final disjunct. Without the
parentheses the category exclusion would have covered one test out of five, and the remaining four
could have selected a `LiveOutlook`-categorised test. `/InIsolation` is passed as well, matching the
wrapper's protections.

## Relationship to the footprint proof

Passing these five tests shows the guards were not broken. That they were not *edited* is proved
separately and mechanically by P4-T13: the test-footprint diff lists only
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`, and neither
`BreadcrumbDropDownOpenCoordinatorTests.cs` nor `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`
appears, which is where four of these five tests live. The two together satisfy AC-5 through AC-8;
this artifact alone satisfies AC-9.

Output Summary: All five standing-guard tests pass after the production change. Exit code 0, 5
total, 5 passed, 0 failed. No repeated-close suppression contract regressed, and the required close
while the host reports not open still reaches the host.
