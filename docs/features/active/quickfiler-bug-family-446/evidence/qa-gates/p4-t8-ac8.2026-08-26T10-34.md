# [P4-T8] AC8 Verification — UndoConsumer Terminates on the Idle Threshold

Timestamp: 2026-08-26T10-34

Task: [P4-T8]
Acceptance criterion: AC8
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC8 text (spec.md:885)

> AC8 — `UndoConsumer` terminates once the queue is drained and the idle threshold elapses.
> Verified by `UndoConsumer_IdleBeyondThreshold_Completes` completing without a `[Timeout]` trip
> on a `FakeTimeProvider`.

## 1. The test and its clock

Declaration: `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:414`, carrying
`[TestMethod]` at `:412` and `[Timeout(10000)]` at `:413`.

Body (`:414-425`), reproduced verbatim:

```csharp
        public async Task UndoConsumer_IdleBeyondThreshold_Completes()
        {
            // Arrange
            var clock = new FakeTimeProvider();
            QfcFormController controller = ArrangeUndoConsumer(clock);
            // Act
            Task consumer = controller.UndoConsumerStarter(controller.UndoConsumer);
            clock.Advance(TimeSpan.FromSeconds(11));
            await consumer.ConfigureAwait(false);
            // Assert
            consumer.Status.Should().Be(TaskStatus.RanToCompletion, "an idle consumer must exit");
        }
```

The eleven seconds that carry the consumer past the ten-second idle threshold are supplied by
`clock.Advance(...)` on a `FakeTimeProvider`, not by wall-clock waiting.

## 2. Recorded outcome — the `[P3-T4]` run

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t4/p3-t4.trx`

- Recorded outcome for `UndoConsumer_IdleBeyondThreshold_Completes`: **Passed**
- Recorded duration: `00:00:00.2891620`

## 3. No `[Timeout]` trip

TRX `<Counters>` from the same file, verbatim:

```
total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0"
```

`timeout="0"` and `aborted="0"`: the `[Timeout(10000)]` attribute did not fire. The observed
duration of 0.289 s is 0.0289 of the 10 s allowance, so the pass is not a marginal one. The
outcome is `Passed` rather than `Timeout`, which is the outcome MSTest records when the attribute
trips.

Corroborating later runs: `Passed` in `evidence/regression-testing/p3-t7/p3-t7.trx` and
`evidence/regression-testing/p3-t8/p3-t8.trx`.

## Output Summary

AC8 holds. `UndoConsumer_IdleBeyondThreshold_Completes`
(`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:414`) is recorded **Passed** in
`evidence/regression-testing/p3-t4/p3-t4.trx` in 0.289 s, driving all elapsed time through
`FakeTimeProvider.Advance`. That TRX reports `timeout="0"` and `aborted="0"`, so no `[Timeout]`
trip occurred. The AC8 checkbox in `spec.md` is checked.

EXIT_CODE: 0
