# Phase 3 — Defect B Scoped Regression Run, Before the Fix (P3-T3) [expect-fail]

Timestamp: 2026-09-08T08-08

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`

EXIT_CODE: 0

The rebuild reported `Build succeeded.` and `0 Error(s)`.

Command: `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /ResultsDirectory:coverage/plan812/p3-t3 "/Logger:trx;LogFileName=p3-t3.trx" /TestCaseFilter:"FullyQualifiedName~StoreWrapperController_Tests&TestCategory!=LiveOutlook"`

EXIT_CODE: 1

ExpectedExitCode: 1

`vstest.console.exe` was resolved per D5. The filter is purely conjunctive, so the D6 precedence rule is satisfied trivially.

Output Summary:

This is an `[expect-fail]` task. A failing run is the required outcome: it proves the unbounded-retry assertion genuinely fails before the Phase 4 latch lands.

Counters read from `coverage/plan812/p3-t3/p3-t3.trx` rather than from console text:

- Total: 73
- Passed: 72
- Failed: 1
- Skipped (`notExecuted`): 0

FAILING-TESTS: exactly one, and it is the expected one.

- `PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce`

That is D15 item 14. The failing set matches the P3-T3 acceptance condition exactly, with no additional member, so the new tests isolate the unbounded retry rather than disturbing unrelated behaviour and no correction is required.

PASSED, as the acceptance condition requires:

- `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore` — D15 item 15. It passes before the fix because two controllers already attempt two lookups; it is present to pin that the Phase 4 latch is per instance and does not silently become per store or per process, which would make it fail.
- `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup` — D15 item 16. It passes before the fix because the existing null check already suppresses the lookup; it is present to pin that the latch does not replace that check.
- The three existing #797 AC6 methods, all recorded as `Passed`: `PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress`, `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup`, and `PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason`. P3-T1 left `CreateDisplayFailingSmtpRootFolder` and `CreateDisplaySmtpRootFolder` unchanged, and the diff for that file is a single pure-insertion hunk at pre-image position 64 which consumes no pre-image line, so nothing inside the `:47-63` helper span was modified.

Mechanism of the single failure, for the record. `StoreWrapper.GetSmtpAddressFromStore` reads `ExchangeUser.PrimarySmtpAddress` exactly once per call, and the fixture makes that read throw `COMException`. The fallback sources then yield no address containing an at-sign, so `UserEmailAddress` remains null. Because the pre-fix guard in `StoreWrapperController.Display.cs` tests only `Current.UserEmailAddress is null`, the condition is still true on the second `PopulateWithCurrent()` and the COM lookup is attempted again. The mock therefore records two reads of `PrimarySmtpAddress` where the assertion requires one.
