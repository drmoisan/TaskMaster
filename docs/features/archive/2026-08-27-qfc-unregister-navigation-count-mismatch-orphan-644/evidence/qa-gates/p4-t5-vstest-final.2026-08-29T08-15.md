# QA gate — Full test gate for the touched assembly ([P4-T5])

- Issue: #644
- Task: `[P4-T5]`
- Timestamp: 2026-08-29T08-15

**Restarted pass.** This artifact records the re-run triggered by the `[P4-T8]` net-line finding
described in `evidence/qa-gates/p4-t1-csharpier-format.2026-08-29T08-15.md`. It supersedes the
first pass's run and is the authoritative `[P4-T5]` TRX for every Phase 5 check-off that cites one.

Command: `<resolved-vstest.console.exe> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p4-t5r /TestCaseFilter:"TestCategory!=LiveOutlook"`
Runner: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

TRX written under `coverage\trx\p4-t5r\` as `<account>_<HOST>_2026-08-29_14_13_xx_net481.trx`, with
a binary `.coverage` attachment alongside it. The default `vstest.console.exe` TRX filename embeds
the account and machine name, so both are redacted here. `coverage/*` is gitignored, so neither
artifact dirties the tree. The results directory is `p4-t5r` rather than `p4-t5` so the restarted
pass does not overwrite the superseded first-pass TRX.

## TRX `Counters` element

```
COUNTERS total=1254 passed=1254 failed=0 error=0 aborted=0
```

- **total: 1254**
- **passed: 1254**
- **failed: 0** — the acceptance clause `failed="0"` holds

Independent enumeration of every `UnitTestResult` whose `outcome` is not `Passed`:

```
nonpassed=0
```

The loop therefore does **not** restart from `[P4-T1]`.

## Comparison against the `[P0-T11]` baseline

| Measure | `[P0-T11]` baseline | `[P4-T5]` final | Delta |
|---|---|---|---|
| total | 1248 | **1254** | **+6** |
| passed | 1248 | **1254** | +6 |
| failed | 0 | **0** | 0 |

The `+6` is exactly the six new ledger tests. No pre-existing test was lost, and none regressed.

## The six ledger tests, each `Passed`

This is what makes a missing `Compile Include` item detectable: the project is legacy non-SDK
style, so an unregistered `.cs` file is silently not compiled and its tests would be **absent** from
the TRX rather than failing. All six are present and passing, which is the AC-8 evidence.

```
Passed :: UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey
Passed :: UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow
Passed :: RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty
Passed :: UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged
Passed :: UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow
Passed :: UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys
```

## The seven named reconciliation tests, each `Passed`

```
Passed :: LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix
Passed :: LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys
Passed :: SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey
Passed :: RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException
Passed :: UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys
Passed :: UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys
Passed :: RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter
```

Each of the thirteen names above was located in the TRX by exact `testName` match; none is missing.

## Note on the coverage figure

`/EnableCodeCoverage` emits a binary `.coverage` file and prints **no coverage percentage**, so no
numeric coverage value can be read from this run. The numeric figure the no-regression comparison
needs is produced by `[P4-T6]`'s Cobertura run instead.

Output Summary: Full test gate **green**. TRX `Counters` reports **total="1254", passed="1254",
failed="0"** with zero errors and zero aborted, EXIT_CODE 0. All six ledger tests and all seven
named reconciliation tests appear with outcome `Passed`. Total rose by exactly 6 over the
`[P0-T11]` baseline of 1248, accounting for the new tests with no regression. This is the fourth
and last artifact of the uninterrupted `[P4-T1]`-`[P4-T5]` pass that `[P5-T16]` requires for AC-15;
no step in that pass rewrote a tracked file.
