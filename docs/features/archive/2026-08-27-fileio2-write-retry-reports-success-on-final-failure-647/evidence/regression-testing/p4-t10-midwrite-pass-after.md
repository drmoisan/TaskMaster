# P4-T10 — Mid-Write Regression: Pass-After Run

Timestamp: 2026-08-31T20-02
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName=UtilitiesCS.Test.HelperClasses.FileIO2_Tests.WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying /Logger:trx /ResultsDirectory:coverage\testresults\p4-t10
EXIT_CODE: 0
ExpectedExitCode: 0

Matching fail-before record: `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/regression-testing/p3-t2-midwrite-fail-before.md`.

## Result

- Total tests: 1
- Passed: 1
- Failed: 0

`WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying` is reported **Passed**, in 51 ms.

## Observed count

OBSERVED_DELAY_INVOCATION_COUNT: 0

The assertion `midWriteDelayCalls.Should().Be(0);` passed. A FluentAssertions numeric equality assertion fails and reports the observed value whenever it differs from the expected one, so the assertion passing is the observation that the delay delegate was invoked exactly zero times. The sibling assertion `midWriteFactoryCalls.Should().Be(1);` also passed, so the writer was obtained exactly once and never re-obtained.

## The pair, side by side

| | Fail-before (P3-T2) | Pass-after (P4-T10) |
|---|---|---|
| Result | Failed | Passed |
| Exit code | 1 | 0 |
| Observed delay invocations | 1 | 0 |
| Observed writer-factory invocations | 1 | 1 |

The same test method, unchanged between the two runs, against the same seam. The only thing that changed is the loop structure inside `WriteTextFileAsync`. Pre-fix, a mid-write `IOException` reached the catch with the success flag already set, took the retry branch once, awaited one delay, then exited the loop reporting success. Post-fix, the per-attempt `opened` local is true when the catch is entered, so the handler logs with the bound exception and returns `false` immediately, consuming no retry budget and awaiting no delay.

That zero is the observable proof that no retry occurred, which is what the append-duplication hazard requires: the file is opened in append mode, so a retry after a partial flush would duplicate the lines already written.

Output Summary: The test passed with exit code 0 and an observed delay-invocation count of 0, completing the fail-before / pass-after pair for defect 2.
