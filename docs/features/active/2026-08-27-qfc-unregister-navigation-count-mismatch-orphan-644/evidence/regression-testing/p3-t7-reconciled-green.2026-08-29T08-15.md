# Regression testing — Three reconciled test classes green ([P3-T7])

- Issue: #644
- Task: `[P3-T7]`
- Timestamp: 2026-08-29T08-15

## Rebuild

Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

```
    5 Warning(s)
    0 Error(s)
```

0 errors; the 5 warnings are the pre-existing `System.Reactive` `packages.config` advisory,
unchanged from every baseline.

## Test run

Command: `<resolved-vstest.console.exe> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p3-t7 /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerTests|FullyQualifiedName~QfcCollectionControllerNavigationDigitsTests|FullyQualifiedName~QfcCollectionControllerDefects468Tests"`
Runner: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
EXIT_CODE: 0

TRX written to `coverage\trx\p3-t7\<account>_<HOST>_2026-08-29_13_59_52_net481.trx` (default
filename embeds the account and machine name, redacted here).

## TRX `Counters` element

```
COUNTERS total=24 passed=24 failed=0
```

**`failed="0"`**, as the acceptance requires.

## The seven named tests, each with outcome `Passed`

```
Passed :: LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix
Passed :: LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys
Passed :: SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey
Passed :: RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException
Passed :: UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys
Passed :: UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys
Passed :: RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter
```

Each was located in the TRX by exact `testName` match; none is missing.

## What each result establishes

- The three amended characterisation tests
  (`…ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix`,
  `…SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys`,
  `SwapItemGroups_ThenSkipGuardedTrailingRegister_…`) pass with their assertions preserved verbatim
  and their arrangement routed through the real `RegisterNavigation()` path.
- `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` needed no
  change and still passes, which confirms the `[P2-T2]` ordering rule: the ledger append sits
  strictly after `StringActionsAsync.Add`, so a duplicate-key `ArgumentException` leaves the ledger
  unpolluted. Had that ordering been inverted, this test would be the one to fail.
- Both digits-file tests pass, including the one whose assertion `[P3-T4]` flipped from
  `Equal(new[] { "10" })` to `BeEmpty(…)`. The `…GrowingToTen_RemovesTheOneDigitKeys` sibling
  passes unchanged, so #472's guarantee is intact under the ledger.
- `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` passes with
  its assertions untouched. The expected `NullReferenceException` still occurs and still
  propagates; only its originating statement moved, which is what `[P3-T5]`'s comment corrections
  record.

Output Summary: The solution rebuilt with **exit 0 and 0 errors**, and the three reconciled test
classes ran with TRX `Counters` reporting **total="24", passed="24", failed="0"**. All seven test
names the acceptance enumerates appear in the TRX with outcome `Passed`. Phase 3 reconciliation is
complete; the whole-assembly gate runs in `[P4-T5]`.
