# P3-T4 — Retry-Exhaustion Characterization Run (pre-fix)

Timestamp: 2026-08-31T19-38
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName=UtilitiesCS.Test.HelperClasses.FileIO2_Tests.WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget /Logger:trx /ResultsDirectory:coverage\testresults\p3-t4
EXIT_CODE: 0
ExpectedExitCode: 0

## Result

- Total tests: 1
- Passed: 1
- Failed: 0

`WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget` is reported **Passed**, in 55 ms.

This run is expected to pass against pre-fix source. It **characterizes** defect 1 rather than failing on it. The pre-fix method returns the non-generic `Task` and therefore exposes no value that could report failure, so no assertion written against pre-fix source can distinguish an exhausted retry budget from a completed write. What the test can and does observe is the shape of the exhausted budget.

## Observed counts

OBSERVED_WRITER_FACTORY_INVOCATION_COUNT: 100
OBSERVED_DELAY_INVOCATION_COUNT: 99

These are the values asserted verbatim by `exhaustionFactoryCalls.Should().Be(100);` and `exhaustionDelayCalls.Should().Be(99);`, both of which passed. A FluentAssertions numeric equality assertion fails and reports the observed value whenever it differs from the expected one, so the assertions passing is the observation that the counts are exactly 100 and 99. The third assertion in the test, `await act.Should().NotThrowAsync();`, also passed: the pre-fix method returns normally after exhausting its budget, which is precisely the defect — a write that never happened is indistinguishable from one that did.

## Determinism

The 55 ms runtime is the evidence that no wall-clock wait occurred. The pre-fix production path performs 99 real `Task.Delay(100)` awaits and takes approximately 9.9 seconds, as `evidence/qa-gates/p2-t4-utilitiescs-tests.md` records for the locked-fixture test at 10 s. This test drives the same 99 iterations through an injected delay delegate returning `Task.CompletedTask`, so the loop completes in milliseconds. No file, no directory, no temporary path and no `Thread.Sleep` is involved.

Output Summary: The test passed, exit code 0, with the writer factory observed at exactly 100 invocations and the delay delegate at exactly 99. This artifact is the alternative proof cited by the P3-T5 fail-before exception dossier for defect 1.
