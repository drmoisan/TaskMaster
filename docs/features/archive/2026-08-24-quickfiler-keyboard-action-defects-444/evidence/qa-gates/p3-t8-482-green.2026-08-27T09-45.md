# [P3-T8] #482 interleaving test — GREEN after the fix

Timestamp: 2026-08-27T09-45
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_NavigationTests.ToggleExpansion_WhenAsyncOnThenSyncOffThenAsyncOn_DoesNotThrowAndBothRegistriesHoldOneBAndOneD"`
EXIT_CODE: 0

The filter is byte-identical to `[P3-T3]`'s. The only change between the two runs is `[P3-T4]`'s new
`SyncExpandedRegistrations` helper plus `[P3-T5]`'s and `[P3-T6]`'s rewiring of the two `ToggleState`
overload bodies, all in `QuickFiler/Controllers/QfcItemController.Navigation.cs`.

## Result (verbatim)

```
Passed ToggleExpansion_WhenAsyncOnThenSyncOffThenAsyncOn_DoesNotThrowAndBothRegistriesHoldOneBAndOneD [309 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
```

| Measure | Value |
| --- | --- |
| Total | 1 |
| Passed | **1** |
| Failed | **0** |

The three-step sequence `ToggleExpansionAsync(On)` then `ToggleExpansion(Off)` then
`ToggleExpansionAsync(On)` now completes without throwing `ArgumentException`, and both
`_kbdHandler.CharActions` and `_kbdHandler.CharActionsAsync` hold exactly one `'B'` and one `'D'`
entry for the helper's entry id.

## Acceptance evaluation

- The run reports `Passed: 1` and `Failed: 0`. PASS.

Output Summary: 1 test run, 1 passed, 0 failed; the #482 pass-after state is captured.
