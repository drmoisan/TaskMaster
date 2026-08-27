# [P3-T13] All four #482 tests

Timestamp: 2026-08-27T09-45
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_NavigationTests.ToggleExpansion_WhenAsyncOnThenSyncOffThenAsyncOn_DoesNotThrowAndBothRegistriesHoldOneBAndOneD|FullyQualifiedName~QfcItemController_NavigationTests.ToggleExpansion_WhenCollapsedByEitherOverload_BothRegistriesHoldNoExpansionEntries|FullyQualifiedName~QfcItemController_NavigationTests.ToggleExpansion_WhenOnCalledTwiceOnTheSameOverload_DoesNotThrow|FullyQualifiedName~QfcItemController_NavigationTests.SyncExpandedRegistrations_WhenInvokedWithTrueThenFalse_AddsThenRemovesBothRegistries"`
EXIT_CODE: 0

The four filter clauses are joined with `|`; `vstest` 18.x rejects `OR`.

## Summary (verbatim)

```
Test Run Successful.
Total tests: 4
     Passed: 4
```

| Measure | Value |
| --- | --- |
| Total | 4 |
| Passed | **4** |
| Failed | **0** |

## Every executed test, fully qualified

```
QuickFiler.Controllers.Tests.QfcItemController_NavigationTests.ToggleExpansion_WhenAsyncOnThenSyncOffThenAsyncOn_DoesNotThrowAndBothRegistriesHoldOneBAndOneD   Passed [308 ms]
QuickFiler.Controllers.Tests.QfcItemController_NavigationTests.ToggleExpansion_WhenCollapsedByEitherOverload_BothRegistriesHoldNoExpansionEntries              Passed [1 ms]
QuickFiler.Controllers.Tests.QfcItemController_NavigationTests.ToggleExpansion_WhenOnCalledTwiceOnTheSameOverload_DoesNotThrow                                 Passed [1 ms]
QuickFiler.Controllers.Tests.QfcItemController_NavigationTests.SyncExpandedRegistrations_WhenInvokedWithTrueThenFalse_AddsThenRemovesBothRegistries            Passed [1 ms]
```

## What each test establishes

| Test | Establishes |
| --- | --- |
| interleaving | async-On, sync-Off, async-On completes without `ArgumentException`, and both registries hold one `'B'` and one `'D'` |
| collapse-direction | expanding through one overload and collapsing through the other leaves both registries holding zero `'B'` and zero `'D'` |
| idempotence | two consecutive `ToggleState.On` calls on the same overload do not throw |
| direct-helper | `SyncExpandedRegistrations` driven through `QfcItemControllerTestSupport.InvokeNonPublic` with `true` then `false` adds then removes in both registries |

The direct-helper test is the direct-exercise clause of acceptance criterion AC-482-08. That criterion
is conjunctive and its `>= 90%` line-coverage clause is measured only by `[P4-T9]`, so `[P3-T27]`
records the deferral and `[P4-T20]` performs the check-off.

## Acceptance evaluation

- The run reports `Passed: 4` and `Failed: 0`. PASS.

Output Summary: 4 of 4 passed, 0 failed; every executed test name listed above.
