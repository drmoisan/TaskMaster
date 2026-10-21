# P3-T2 — Mid-Write Regression: Fail-Before Run `[expect-fail]`

Timestamp: 2026-08-31T19-35
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName=UtilitiesCS.Test.HelperClasses.FileIO2_Tests.WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying /Logger:trx /ResultsDirectory:coverage\testresults\p3-t2
EXIT_CODE: 1
ExpectedExitCode: 1

A failing run is the expected and required outcome of this task. This is the genuine fail-before evidence for defect 2, the mid-write success report. It is achievable only because Phase 2 landed the seam carrying the defect verbatim, so a test can drive the pre-fix control flow deterministically.

## Result

- Total tests: 1
- Failed: 1
- Passed: 0

The test `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying` is reported **Failed**.

## Transcribed assertion failure message, verbatim

```
Expected midWriteDelayCalls to be 0, but found 1 (difference of 1).
```

OBSERVED_DELAY_INVOCATION_COUNT: 1

## The assertion-ordering invariant held

The failure originates at `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` line 82, which the captured stack trace names directly. Line 82 is `midWriteDelayCalls.Should().Be(0);`. Line 81 is `midWriteFactoryCalls.Should().Be(1);`.

MSTest and FluentAssertions report the first failing assertion in a test body and stop, so only the failing assertion produces a message. Against pre-fix source the writer-factory count assertion at line 81 **passes**, because the loop obtains a writer exactly once, and the delay-count assertion at line 82 **fails**. The two assertions are therefore in the fixed order this plan requires, and the observed count transcribed above is readable from the failure message precisely because the delay-count assertion is the one that failed. No later task may reorder those two assertions.

## Why the observed value is 1 rather than 0

The mechanism, traced against the pre-fix control flow recorded in `evidence/baseline/p1-t1-pre-change-loop.md`: the fake writer opens successfully, so the success flag is assigned inside the `using` block before any write executes. The first `WriteLineAsync` then raises `IOException`, which reaches the catch with the flag already true. The catch increments `attempts` to 1, takes the `attempts < 100` branch, and awaits exactly one delay. Control then falls out of the catch, the `while (!success)` condition is false, and the loop exits reporting success. One delay, no retry, no log entry.

Post-fix the method returns immediately with zero delays, which is what `evidence/regression-testing/p4-t10-midwrite-pass-after.md` records.

Output Summary: The test failed as required, the transcribed message is the one raised by `midWriteDelayCalls.Should().Be(0);`, and the recorded observed delay-invocation count is 1.
