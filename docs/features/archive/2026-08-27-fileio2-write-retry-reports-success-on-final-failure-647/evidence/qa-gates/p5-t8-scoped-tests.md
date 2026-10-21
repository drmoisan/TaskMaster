# P5-T8 — Scoped Test Run Across the Three Touched Test Assemblies

Timestamp: 2026-08-31T20-12
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Logger:trx /ResultsDirectory:coverage\testresults\p5-t8
EXIT_CODE: 0
ExpectedExitCode: 0

This is the artifact every later task in this plan reads when it refers to "the P5-T8 artifact": P7-T1, P7-T3, P7-T4, P7-T5, P7-T8, P7-T9 and P7-T10 all read a recorded test result and therefore read this file. The companion format artifact is `evidence/qa-gates/p5-t8-format-check.md` and is read by no task.

`vstest.console.exe` was resolved through `vswhere.exe`. `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` were both passed, the latter so the single live-Outlook integration test does not start an external process.

## Counts

- Total: 6422
- Passed: 6422
- Failed: 0
- Skipped: 0

Failed test names: none. vstest omits the `Failed:` and `Skipped:` summary lines when those counts are zero, and neither appeared.

## Expectation selection

`ExpectedExitCode:` is 0. The rule this task states selects 1 only when the run reports at least one Failed test and every Failed name appears on `BASELINE_FAILURE_SET:`. This run reported no Failed test, so the expectation is 0 by the rule's "and of 0 otherwise" clause.

## Acceptance evaluation against the recorded baseline

The set of Failed test names is empty. `BASELINE_FAILURE_SET:` recorded in `evidence/baseline/p0-t19-baseline-failure-set.md` is the literal word `none`, so the required subset relation holds trivially and the clause that then applies requires `EXIT_CODE:` to be 0. It is 0.

CARRIED_BASELINE_FAILURES: not applicable. The recorded baseline is `none` rather than a name list, so no carried-failure branch is available and no non-zero test-run exit code was authorized. None was needed.

## Individual result of each of the six named FileIO2_Tests methods

| Test method | Result | Duration |
|---|---|---|
| `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying` | Passed | 2 ms |
| `WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget` | Passed | 2 ms |
| `WriteTextFileAsync_WhenTransientOpenFailureThenSucceeds_ShouldReturnTrueAndWriteAllLines` | Passed | 1 ms |
| `WriteTextFileAsync_WhenTokenAlreadyCancelled_ShouldThrowBeforeOpening` | Passed | 6 ms |
| `WriteTextFileAsync_WhenCancelledDuringRetryWindow_ShouldThrowPromptly` | Passed | 2 ms |
| `WriteTextFileAsync_WhenRetrying_ShouldPassCallerTokenToDelay` | Passed | 38 ms |

All six are recorded **Passed**.

The durations are themselves evidence of the determinism requirement. The suite previously carried a single `WriteTextFileAsync` test that took approximately 10 seconds, because it locked a real fixture file and let the loop run its 99 real 100-millisecond delays. These six tests cover strictly more behavior — retry exhaustion, mid-write failure, the transient-then-success path, both cancellation entry points, and token propagation — and together take 51 milliseconds, because every timing-dependent branch is driven through the injected delay delegate. No test creates a file or a directory, uses a temporary path, or waits on the wall clock.

Output Summary: 6422 of 6422 tests passed across the three touched assemblies with exit code 0, and all six named `FileIO2_Tests` methods are recorded Passed.
