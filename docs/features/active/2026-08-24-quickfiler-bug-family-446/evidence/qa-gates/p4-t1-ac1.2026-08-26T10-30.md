# [P4-T1] AC1 Verification — #446 Gate Stop-Reason Discrimination

Timestamp: 2026-08-26T10-30

Task: [P4-T1]
Acceptance criterion: AC1
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC1 text (spec.md:875)

> AC1 — #446 gate: `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` and
> `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` exist in
> `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs`, are driven by
> `FakeTimeProvider`, fail against the pre-fix gate and pass after.

## 1. Presence in the named file

Command: `grep -n "DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop\|DequeueAsync_SourceDrained_ReportsSourceExhaustedStop\|FakeTimeProvider" "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs"`
EXIT_CODE: 0

| Test method | Declaration site | `FakeTimeProvider` drive site |
| --- | --- | --- |
| `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs:165` | `:168` `var fakeTime = new FakeTimeProvider();`, passed as `timeProvider: fakeTime` at `:185` and advanced one second per score at `:178` |
| `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs:206` | `:213` `timeProvider: new FakeTimeProvider(),` |

Both methods carry `[TestMethod]` (`:164` and `:205`). Neither uses a real clock, `Thread.Sleep`
or `Task.Delay`; time advances only through `fakeTime.Advance(TimeSpan.FromSeconds(1))`.

## 2. Fail-before / pass-after pairing

| Test | Pre-fix TRX (Failed) | Post-fix TRX (Passed) |
| --- | --- | --- |
| `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t9/p1-t9.trx` — outcome `Failed` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t1/p2-t1.trx` — outcome `Passed` |
| `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t10/p1-t10.trx` — outcome `Failed` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t1/p2-t1.trx` — outcome `Passed` |

Recorded pre-fix failure messages (verbatim from the cited TRX `ErrorInfo/Message` nodes):

- `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` (p1-t9.trx):
  `Expected batch.Stop to be QfcDequeueStop.DeadlineExpired {value: 2} because an empty batch caused by the first-batch deadline is not source exhaustion, but found QfcDequeueStop.QuantitySatisfied {value: 0}.`
- `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` (p1-t10.trx):
  `Expected batch.Stop to be QfcDequeueStop.SourceExhausted {value: 1} because a drained source with no active producer is genuine exhaustion, but found QfcDequeueStop.QuantitySatisfied {value: 0}.`

Both pre-fix messages are FluentAssertions assertion failures. Neither is a timeout, a hang or a
compile error.

Corroborating later runs (not required by the acceptance condition, recorded for completeness):
both tests are also `Passed` in `evidence/regression-testing/p2-t8/p2-t8.trx` and
`evidence/regression-testing/p3-t8/p3-t8.trx`.

## Output Summary

AC1 holds. Both named tests exist in
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` at `:165` and
`:206`, are driven exclusively by `FakeTimeProvider`, each has a recorded pre-fix `Failed`
outcome (p1-t9.trx and p1-t10.trx respectively) with an assertion-failure message, and each has a
recorded post-fix `Passed` outcome in p2-t1.trx. The AC1 checkbox in `spec.md` is checked.

EXIT_CODE: 0
