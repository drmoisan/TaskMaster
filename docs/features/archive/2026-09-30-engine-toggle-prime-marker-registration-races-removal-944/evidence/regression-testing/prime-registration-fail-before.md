# Prime Registration Fail-Before (P1-T4, expect-fail)

Timestamp: 2026-09-30T13-32
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\944\p1-t4" "/Logger:trx;LogFileName=p1-t4.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, ASSEMBLY-TM, FILTER-COORD, NAMES-944; vstest resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
VSTEST_EXIT_CODE: 1 (13-32-01 to 13-32-03 UTC); TRX_PRESENT: True; SEQUENCE_FILES: 0
COUNTERS total=28 executed=28 passed=27 failed=1
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Failed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
FAILED GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns
MESSAGE GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns :: Expected handleCompletedDuringRead to be False because the prime handle must be registered before the activation read runs, so a prime that completes on any thread finds its own marker, but found True.

## Verification against the acceptance

- TRX_PRESENT: True
- SEQUENCE_FILES: 0 (no hang or timeout)
- EXIT_CODE: 1 (non-zero)
- COUNTERS total 28 = BASELINE-TOTAL 25 plus 3 (the three new tests were discovered; the assembly compiled and loaded with them)
- The program-order test is Failed, and its message contains the reason fragment `must be registered before the activation read runs` (the not-completed-during-read assertion), so the failure is that assertion and not a compile error, an assembly-load error or a timeout.
- GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
- The only FAILED name is the program-order test; BASELINE-FAILED was NONE.

## Environment of the control:

The production file TaskMaster/Ribbon/EngineToggleStateCoordinator.cs is byte-identical to the anchor ANCHOR-SHA b305903e275b8abf58e8e65831c189f517568fe4: its CMD-HASH value is recorded here as PROD-HASH-AT-CONTROL: D9C915AE9B00BB7AAB80183A7A0BA11748DE393781D7E5ADE2BDE29073B7002B, which equals ANCHOR-HASH-PROD: D9C915AE9B00BB7AAB80183A7A0BA11748DE393781D7E5ADE2BDE29073B7002B from P0-T19. The new partial (CMD-HASH 6D28EBF80B5D4AE7C3C0099A8A329F7B8DF463E4258FF1B14D921CEB12C91A7C) and its compile entry are present. The run settings are unchanged: `git diff --exit-code ANCHOR-SHA -- TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings` exited 0 (RUNSETTINGS_DIFF_EXIT=0).

## Informational (no gate)

- INFORMATIONAL-REPRIME-FAULTED: Passed (no message)
- INFORMATIONAL-REPRIME-CANCELED: Passed (no message)
- OBSERVED-VALUE-FRAGMENT: present (the program-order test message matches the case-insensitive pattern found\s+true: "... but found True.")

The two re-prime tests passed before the fix on this run; their pre-fix failure depends on a thread-pool thread winning the race (plan D-5), so this outcome is recorded without a gate.
