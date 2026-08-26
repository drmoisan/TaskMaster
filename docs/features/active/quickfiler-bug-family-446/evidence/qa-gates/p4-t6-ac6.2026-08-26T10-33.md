# [P4-T6] AC6 Verification — CompleteAddingAsync Is Guarded by SourceExhausted

Timestamp: 2026-08-26T10-33

Task: [P4-T6]
Acceptance criterion: AC6
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC6 text (spec.md:883)

> AC6 — `QuickFiler/Controllers/QfcHomeController.Iteration.cs` calls `CompleteAddingAsync` only
> inside a branch guarded by `Stop == QfcDequeueStop.SourceExhausted`. Verified by AC2 plus
> reading the diff of that file.

## 1. Call-site census

Command: `grep -n "CompleteAddingAsync\|SourceExhausted\|IterateQueueAsync" "QuickFiler/Controllers/QfcHomeController.Iteration.cs"`
EXIT_CODE: 0

| Line | Text | Kind |
| --- | --- | --- |
| `36` | `else if (batch.Stop == QfcDequeueStop.SourceExhausted)` | guard |
| `39` | `// CompleteAddingAsync reaches BlockingCollection<T>.CompleteAdding(), which is` | comment, not a call |
| `44` | `await QfcQueue.CompleteAddingAsync(Token, 10000);` | the only invocation |

The file contains exactly one `CompleteAddingAsync` invocation, at
`QuickFiler/Controllers/QfcHomeController.Iteration.cs:44`. The only other occurrence of the
identifier is the explanatory comment at `:39`, which is not a call site. The single guard
comparison is at `:36`.

## 2. Post-change text of `IterateQueueAsync`

`QuickFiler/Controllers/QfcHomeController.Iteration.cs:12-61`, reproduced verbatim:

```csharp
        public async Task IterateQueueAsync()
        {
            Token.ThrowIfCancellationRequested();

            if (_datamodel.Complete)
            {
                return;
            }
            try
            {
                QfcDequeueBatch batch = await _datamodel.DequeueNextItemGroupWithOutcomeAsync(
                    _formController.ItemsPerIteration,
                    2000,
                    QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
                    null
                );
                IList<MailItem> listObjects = batch.Items;
                if (listObjects.Count > 0)
                {
                    //await UiThread.Dispatcher.InvokeAsync(async () => await QfcQueue.EnqueueAsync(listObjects, _formController.Groups));
                    await QfcQueue
                        .EnqueueAsync(listObjects, _formController.Groups)
                        .ConfigureAwait(false);
                }
                else if (batch.Stop == QfcDequeueStop.SourceExhausted)
                {
                    // Issue #446. Only genuine source exhaustion may close the queue:
                    // CompleteAddingAsync reaches BlockingCollection<T>.CompleteAdding(), which is
                    // irreversible. An empty batch whose stop reason is DeadlineExpired or
                    // QuantitySatisfied leaves the queue open so a later iteration can drain the
                    // items the master queue still holds.
                    //logger.Debug($"{nameof(IterateQueueAsync)} completed");
                    await QfcQueue.CompleteAddingAsync(Token, 10000);
                }
            }
            catch (OperationCanceledException)
            {
                //logger.Debug($"{nameof(IterateQueueAsync)} cancelled");
            }
            catch (System.Exception)
            {
                if (this.Token.IsCancellationRequested)
                {
                    //logger.Debug($"{nameof(IterateQueueAsync)} cancelled");
                }
                else
                {
                    throw;
                }
            }
        }
```

The invocation at `:44` is lexically inside the `else if (batch.Stop == QfcDequeueStop.SourceExhausted)`
block opened at `:36-37` and closed at `:45`. There is no other statement path to it: the block
has one entry condition and the method has no `goto`, no local function and no second
`CompleteAddingAsync` reference.

## 3. Behavioural corroboration — the `[P2-T7]` TRX

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t7/p2-t7.trx`

| Test | Outcome |
| --- | --- |
| `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` (asserts `Times.Never`) | Passed |
| `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` (asserts `Times.Once`) | Passed |

The pair discriminates: the same empty-batch arrangement differing only in `Stop` produces zero
invocations under `DeadlineExpired` and exactly one under `SourceExhausted`, which is the runtime
reading of the static guard recorded in section 2.

## Output Summary

AC6 holds. `QuickFiler/Controllers/QfcHomeController.Iteration.cs` contains exactly one
`CompleteAddingAsync` call, at `:44`, and it sits inside the branch guarded by
`batch.Stop == QfcDequeueStop.SourceExhausted` at `:36`. The `[P2-T7]` TRX records both
discriminating iteration tests as Passed. The AC6 checkbox in `spec.md` is checked.

EXIT_CODE: 0
