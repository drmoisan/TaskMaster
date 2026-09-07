# [P2-T10] The four pre-existing navigation tests after the #472 fix

Timestamp: 2026-08-27T09-45
Command: the `[P0-T22]` filter, byte-identical: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix|FullyQualifiedName~LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys|FullyQualifiedName~RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException|FullyQualifiedName~SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey"`
EXIT_CODE: 0

## Baseline restated from `[P0-T22]`

```
BaselineNavTestCount = 4

BaselineNavTestResults:
  LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix = Passed
  LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys          = Passed
  RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException   = Passed
  SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey   = Passed
```

## Post-fix results

```
Test Run Successful.
Total tests: 4
     Passed: 4
```

| Test | Baseline verdict | Post-fix verdict | Identical |
| --- | --- | --- | --- |
| `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` | Passed | Passed [277 ms] | yes |
| `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` | Passed | Passed [2 ms] | yes |
| `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` | Passed | Passed [< 1 ms] | yes |
| `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey` | Passed | Passed [< 1 ms] | yes |

These four tests build their controllers with `FormatterServices.GetUninitializedObject`, so
`_registeredDigits` is `0` in every one of them. The `_registeredDigits == 2 ? "00" : ""` formulation
treats 0 as single-digit, which is the width these one- and two-item pages already used, so none of
them required an injection of the new field and none changed outcome.

## Acceptance evaluation

- The executed test count (4) equals `BaselineNavTestCount` (4). PASS.
- The per-test verdict of each of the four named tests is identical to the verdict recorded in
  `BaselineNavTestResults`. PASS.
- No test that passed at baseline fails after the fix. PASS.

Output Summary: 4 of 4 discovered and passed; every per-test verdict identical to the Phase 0 baseline;
no regression.
