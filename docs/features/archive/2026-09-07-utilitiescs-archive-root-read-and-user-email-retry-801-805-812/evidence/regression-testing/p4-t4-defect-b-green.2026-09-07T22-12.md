# Phase 4 — Defect B Scoped Regression Run, After the Fix (P4-T4)

Timestamp: 2026-09-08T08-14

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`

EXIT_CODE: 0

Command: `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /ResultsDirectory:coverage/plan812/p4-t4 "/Logger:trx;LogFileName=p4-t4.trx" /TestCaseFilter:"FullyQualifiedName~StoreWrapperController_Tests&TestCategory!=LiveOutlook"`

EXIT_CODE: 0

`vstest.console.exe` was resolved per D5. The filter is character-for-character the expression P3-T3 used, so the only variable between the two runs is the Phase 4 latch.

Output Summary:

Counters read from `coverage/plan812/p4-t4/p4-t4.trx` rather than from console text:

- Total: 73
- Passed: 73
- Failed: **0**
- Skipped (`notExecuted`): 0

The three new AC4 methods, all recorded as `Passed`:

- `PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce`
- `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore`
- `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup`

The three existing #797 AC6 methods, all recorded as `Passed`:

- `PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress`
- `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup`
- `PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason`

Comparison against the P3-T3 expect-fail run:

| Figure | P3-T3, before the fix | P4-T4, after the fix |
| --- | --- | --- |
| Total | 73 | 73 |
| Passed | 72 | 73 |
| Failed | 1 | 0 |
| Exit code | 1 | 0 |

The single test that failed at P3-T3, `PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce`, now passes. The two sibling AC4 tests that passed before the fix still pass, which is the discrimination that matters here: had the latch been made static or keyed on the store rather than on the controller instance, `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore` would have turned red, because it requires a second controller over the same failing store to get its own attempt. Had the latch replaced the null check rather than joining it, `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup` would have turned red.

The three #797 AC6 tests are unchanged and still green, so the #797 retry behaviour this change bounds was not removed: a first attempt still runs and still renders either the resolved address or the specific failure message.
