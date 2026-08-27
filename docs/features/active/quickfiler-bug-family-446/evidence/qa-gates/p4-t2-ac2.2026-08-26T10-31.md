# [P4-T2] AC2 Verification — #446 Caller-Side CompleteAdding Guard

Timestamp: 2026-08-26T10-31

Task: [P4-T2]
Acceptance criterion: AC2
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC2 text (spec.md:876)

> AC2 — #446 caller: `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding`
> asserts `IQfcQueue.CompleteAddingAsync` was invoked `Times.Never`, and
> `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` asserts `Times.Once`.
> Both live in `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`.

## 1. Both tests live in the named file, with the required `Times` expressions

Command: `grep -n "IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding\|IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce\|CompleteAddingAsync\|Times\." "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

| Test method | Declaration site | `Times` expression | Site |
| --- | --- | --- | --- |
| `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` | `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs:415` | `Times.Never` | `:423` |
| `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` | `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs:433` | `Times.Once` | `:441` |

Both `Times` values are passed to the shared helper `VerifyCompleteAdding`
(`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs:160-170`), which forwards them
to `queue.Verify(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()), times, because)`
at `:165-169`. The verified member is therefore `IQfcQueue.CompleteAddingAsync` as AC2 requires
(the mock is typed `Mock<IQfcQueue>` at `:162`).

Recorded verbatim from the source:

- `:423` — `                Times.Never,` in the deadline-expired test, with the reason string
  `"a deadline-bounded empty batch must not close the queue"`.
- `:441` — `                Times.Once,` in the source-exhausted test, with the reason string
  `"a drained source is the one empty-batch case that may close the queue"`.

The two tests are driven by the same arrangement helper `ArrangeIterate` differing only in the
`stop:` argument (`QfcDequeueStop.DeadlineExpired` at `:417` versus
`QfcDequeueStop.SourceExhausted` at `:435`), so the pair is a discriminating gate rather than
two independent assertions.

## 2. Post-fix outcomes

| Test | Post-fix TRX | Outcome |
| --- | --- | --- |
| `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t7/p2-t7.trx` | Passed |
| `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t7/p2-t7.trx` | Passed |

Fail-before pairing (recorded for completeness; AC2 itself asserts the `Times` expressions and
the post-fix pass):

- `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` — `Failed` in
  `evidence/regression-testing/p1-t16/p1-t16.trx` (the `[P1-T16]` red step) and again in
  `evidence/regression-testing/p1-t19/p1-t19.trx`.
- `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` — `Passed` in
  `evidence/regression-testing/p1-t17/p1-t17.trx`; it is the negative control added green by
  `[P1-T17]` and was never expected to be red.

Both are also `Passed` in the whole-assembly runs
`evidence/regression-testing/p2-t8/p2-t8.trx` and `evidence/regression-testing/p3-t8/p3-t8.trx`.

## Output Summary

AC2 holds. Both tests live in `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`
(`:415` and `:433`), assert `Times.Never` (`:423`) and `Times.Once` (`:441`) respectively
against `IQfcQueue.CompleteAddingAsync`, and both are recorded `Passed` post-fix in
`evidence/regression-testing/p2-t7/p2-t7.trx`. The AC2 checkbox in `spec.md` is checked.

EXIT_CODE: 0
