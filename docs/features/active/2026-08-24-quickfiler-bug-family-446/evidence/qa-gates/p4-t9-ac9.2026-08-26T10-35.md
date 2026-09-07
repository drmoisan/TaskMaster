# [P4-T9] AC9 Verification — Idle Timer Resets After Every Successful Take

Timestamp: 2026-08-26T10-35

Task: [P4-T9]
Acceptance criterion: AC9
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC9 text (spec.md:886)

> AC9 — the `UndoConsumer` idle timer is **reset after every successful take**. Verified by
> `UndoConsumer_SuccessfulTake_ResetsIdleTimer`, which advances the fake clock past the threshold
> in aggregate while keeping every idle gap below it and asserts the loop kept draining.

## 1. The take branch, post-change

`QuickFiler/Controllers/QfcFormController.Actions.cs:323-331`, reproduced verbatim:

```csharp
                    if (_undoQueue.TryTake(out var item))
                    {
                        await UndoItemProcessor(item).ConfigureAwait(false);

                        // Reset on every successful take so the threshold measures idle time. The
                        // previous code started one stopwatch for the whole session, so a consumer
                        // busy for ten seconds exited while items were still arriving.
                        start = TimeProvider.GetTimestamp();
                    }
```

| Element | Line | Note |
| --- | --- | --- |
| Take guard `if (_undoQueue.TryTake(out var item))` | `:323` | branch entered only on a successful take |
| Item processing `await UndoItemProcessor(item).ConfigureAwait(false);` | `:325` | the item is consumed here |
| Timestamp reassignment `start = TimeProvider.GetTimestamp();` | `:330` | **after** `:325`, so the reset follows processing |
| Idle comparison that reads `start` | `:332` | `else if (TimeProvider.GetElapsedTime(start) > UndoConsumerIdleTimeout)` |

`start` is declared once at `:318` (`long start = TimeProvider.GetTimestamp();`) and reassigned at
exactly one site, `:330`, which lies inside the take branch. The threshold at `:332` therefore
measures time since the last successful take rather than time since the consumer started, which is
the behaviour AC9 requires. The idle bound itself is
`private static readonly TimeSpan UndoConsumerIdleTimeout = TimeSpan.FromSeconds(10);` at `:314`.

## 2. Recorded outcome — the `[P3-T5]` run

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t5/p3-t5.trx`

- Recorded outcome for `UndoConsumer_SuccessfulTake_ResetsIdleTimer`: **Passed**
- `<Counters>`: `total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0"`

The test is declared at `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:435`, carrying
`[TestMethod]` at `:433` and `[Timeout(10000)]` at `:434`. Its documented arrangement
(`:427-432`) is three takes that advance the clock six seconds each — eighteen seconds in
aggregate, past the ten-second threshold — while every idle gap stays at zero, so a session-wide
timer would have exited and a per-take reset does not.

Corroborating later runs: `Passed` in `evidence/regression-testing/p3-t7/p3-t7.trx` and
`evidence/regression-testing/p3-t8/p3-t8.trx`.

## Output Summary

AC9 holds. The take branch at
`QuickFiler/Controllers/QfcFormController.Actions.cs:323-331` reassigns the idle timestamp at
`:330`, after the item is processed at `:325`, and that is the only reassignment of `start` in the
method. `UndoConsumer_SuccessfulTake_ResetsIdleTimer` is recorded **Passed** in
`evidence/regression-testing/p3-t5/p3-t5.trx` with `timeout="0"`. The AC9 checkbox in `spec.md` is
checked.

EXIT_CODE: 0
