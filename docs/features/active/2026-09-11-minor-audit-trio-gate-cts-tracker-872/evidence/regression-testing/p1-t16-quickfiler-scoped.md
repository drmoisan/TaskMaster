# Phase 1 — Scoped Run Of The Two New Log-Assertion Tests

Timestamp: 2026-09-13T15-21
Task: [P1-T16]

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision|FullyQualifiedName~DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound" "/Logger:trx;LogFileName=p1-t16-scoped-quickfiler.trx" /ResultsDirectory:TestResults\vstest\p1-t16
EXIT_CODE: 0

TotalTests: 2
Passed: 2

Output Summary: the run printed `Test Run Successful.`, `Total tests: 2` and `     Passed: 2`, and
exited 0 after `Total time: 1.3692 Seconds`. The two per-test result lines read:

```
  Passed DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision [189 ms]
  Passed DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound [5 ms]
```

## Why The Total Is Asserted Explicitly

A run whose total is anything other than 2 is a failure and not a pass, for the reason recorded in the
P1-T15 artifact: vstest reports a zero-match filter without an obvious error, so the exit code alone
does not distinguish two passing tests from zero discovered tests. The observed total is 2 and the
observed passed count is 2.

## What The Two Tests Assert

Both filter the captured debug-log list on the full four-word opening phrase of the scan-bound message
before asserting any field, because the sibling checkpoint message opens with the same first word and
the launch line carries `Cutoff=900` together with a scan-cap field spelled `ScanCap`. The scan-cap
test then asserts `Accepted=0`, `Scanned=4`, `Cutoff=900`, `Bound=scan-cap` and `Decision=stop` on the
single matching line. The ceiling test asserts `Bound=zero-acceptance-ceiling` present and
`Bound=scan-cap` absent on the single matching line; the absence assertion is the discriminating one,
because a regression that collapsed the two bounds to a single value would emit the item-cap token and
would still satisfy a presence-only assertion.

Defect A is test-only. The gate's production source already emits every field asserted here and is not
edited by this delivery, so both tests were expected to pass immediately and did.

## Determinism Note

Both tests drive all time through FakeTimeProvider and use a mocked MailItem. Neither creates a
temporary file, sleeps, waits on a wall clock, touches an external process or starts Outlook.

## Artifact Retention

Per D10 the TRX is written to the git-ignored results directory
`TestResults/vstest/p1-t16/p1-t16-scoped-quickfiler.trx` under an explicit log file name, so the
default account-and-host TRX name is never composed. Only the transcribed counts above are committed.

## Environment Note

The shared build lock was acquired before this single command and released immediately after it
returned.
