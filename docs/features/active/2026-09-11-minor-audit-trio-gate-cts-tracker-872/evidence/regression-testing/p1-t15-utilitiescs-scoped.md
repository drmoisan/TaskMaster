# Phase 1 — Scoped Run Of The Three New Disposal Tests

Timestamp: 2026-09-13T15-20
Task: [P1-T15]

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~Dispose_WhenPackageConstructedTheSource_ReleasesIt|FullyQualifiedName~Dispose_WhenCallerSuppliedTheSource_LeavesItUsable|FullyQualifiedName~Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource" "/Logger:trx;LogFileName=p1-t15-scoped-utilitiescs.trx" /ResultsDirectory:TestResults\vstest\p1-t15
EXIT_CODE: 0

TotalTests: 3
Passed: 3

Output Summary: the run printed `Test Run Successful.`, `Total tests: 3` and `     Passed: 3`, and
exited 0 after `Total time: 1.5395 Seconds`. The three per-test result lines read:

```
  Passed Dispose_WhenPackageConstructedTheSource_ReleasesIt [61 ms]
  Passed Dispose_WhenCallerSuppliedTheSource_LeavesItUsable [1 ms]
  Passed Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource [< 1 ms]
```

## Why The Total Is Asserted Explicitly

A run whose total is anything other than 3 is a failure and not a pass: vstest reports a zero-match
filter without an obvious error, so an exit code of 0 on its own does not distinguish three passing
tests from zero discovered tests. The observed total is 3 and the observed passed count is 3, so all
three named tests were discovered and executed.

Filter clauses are joined with the vertical bar because this version of the test platform rejects the
word OR inside a test case filter.

## Determinism Note

No test in this set creates a temporary file, sleeps, waits on a wall clock or touches an external
process. Each passes a non-null progress tracker and an explicit stop watch to InitializeAsync, so no
dispatcher is touched and no background task is started, and each probes release through the
cancellation token getter rather than through a timer or a finalizer.

## Artifact Retention

Per D10 the TRX is written to the git-ignored results directory
`TestResults/vstest/p1-t15/p1-t15-scoped-utilitiescs.trx` under an explicit log file name, so the
default account-and-host TRX name is never composed. Only the transcribed counts above are committed.

## Environment Note

The shared build lock was acquired before this single command and released immediately after it
returned.
