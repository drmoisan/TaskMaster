# Regression testing — Six ledger tests green after the fix ([P2-T5])

- Issue: #644
- Task: `[P2-T5]`
- Timestamp: 2026-08-29T08-15

Command: `<resolved-vstest.console.exe> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t5 /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerNavigationLedgerTests"`
Runner: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

TRX written to `coverage\trx\p2-t5\<account>_<HOST>_2026-08-29_13_53_40_net481.trx` (default
filename embeds the account and machine name, redacted here).

## TRX `Counters` element

```
COUNTERS total=6 passed=6 failed=0
```

- **total: 6** — matches the required `total="6"`
- **passed: 6** — matches the required `passed="6"`
- **failed: 0** — matches the required `failed="0"`

## Per-test outcomes — all six names from `[P1-T1]` present with outcome `Passed`

```
Passed :: RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty
Passed :: UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey
Passed :: UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow
Passed :: UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys
Passed :: UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow
Passed :: UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged
```

## Red-to-green transition, against the `[P1-T4]` fail-before run

| Test | `[P1-T4]` (unmodified production code) | `[P2-T5]` (after the ledger fix) |
|---|---|---|
| T1 `…RemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey` | **Failed** — orphaned `{"10"}` | **Passed** |
| T2 `…UnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow` | **Failed** — `ArgumentException` on key `5` | **Passed** |
| T3 `RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty` | Passed | Passed |
| T4 `…WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged` | Passed | Passed |
| T5 `…AfterItemGroupsSetToNull_DoesNotThrow` | **Failed** — `NullReferenceException` at the loop bound | **Passed** |
| T6 `…AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys` | **Failed** — orphaned `{"10"}` | **Passed** |

Four red, then six green, with the only intervening change being the Phase 2 production edit to
`QuickFiler/Controllers/QfcCollectionController.cs`.

## Scope note recorded by the task text

The three characterisation tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
and the pinned residual assertion in
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` are expected to be
red at this point: the ledger changes the outcome those tests were written to characterise. They
are reconciled in Phase 3, which is why no whole-assembly gate runs before `[P3-T7]`.

Output Summary: All six ledger tests pass after the production fix. TRX `Counters` reports
**total="6", passed="6", failed="0"**, EXIT_CODE 0, and each of the six test names listed in
`[P1-T1]` appears in the TRX with outcome `Passed`. Taken with the `[P1-T4]` fail-before artifact,
this establishes the red-to-green transition for T1, T2, T5, and T6.
