# [P2-T7] #472 width-fidelity tests — GREEN after the fix

Timestamp: 2026-08-27T09-45
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerNavigationDigitsTests.UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys|FullyQualifiedName~QfcCollectionControllerNavigationDigitsTests.UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys"`
EXIT_CODE: 0

The filter is byte-identical to `[P2-T3]`'s. The only change between the two runs is `[P2-T4]`'s field
plus assignment and `[P2-T5]`'s `UnregisterNavigation` body rewrite, both in
`QuickFiler/Controllers/QfcCollectionController.cs`.

## Result (verbatim)

```
Passed UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys [243 ms]
Passed UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys [< 1 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
```

| Measure | Value |
| --- | --- |
| Total | 2 |
| Passed | **2** |
| Failed | **0** |

Registering at ten items and unregistering at nine now leaves no `"0"`-prefixed key, and the only
surviving entry is the single `"10"` the shortened loop bound cannot reach — the separately-promoted
count-mismatch defect, asserted explicitly rather than absorbed. The mirror direction (register at
nine at width 1, grow to ten, unregister) leaves the registry empty.

## Acceptance evaluation

- The run reports `Passed: 2` and `Failed: 0`. PASS.

Output Summary: 2 tests run, 2 passed, 0 failed; the #472 pass-after state is captured in both the
shrink and the grow direction.
