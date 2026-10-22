# Reconciled SeamFactory test (P6-T9)

Timestamp: 2026-09-01T11-01
Task: [P6-T9]
Working directory: WORKTREE

Command (leading executable substituted with the absolute path recorded by P0-T14):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues" "/Logger:trx;LogFileName=p6-t9.trx" /ResultsDirectory:FEATURE\evidence\qa-gates\p6-t9
```

EXIT_CODE: 0

Count of `outcome="Passed"` occurrences in the produced TRX: **1**.
Count of `outcome="Failed"` occurrences: 0.

| Outcome | Test |
|---|---|
| Passed | `MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues` |

## What the reconciliation replaced, and why it could not be a relaxation

Before this change the test reflected into a private `FilerQueue` instance field literally named `guard`
and set `ThreadSafeSingleShotGuard._state` to 1, in order to pre-trip the one-shot start gate so that
`Enqueue` would record the item without starting the background consumer. It then asserted
`filerQueue.Queue.Count == 1`.

The handshake repair removes the `guard` field, so that reflection would have resolved to `null` and the
test would have thrown a `NullReferenceException` inside a reflection call — a failure that reads as
unrelated to the change that caused it. This is risk 5 in the specification's risk table.

The `Queue.Count` assertion could not be retained either, and was not merely relaxed. After P3-T3 and
P3-T5, `Enqueue` starts a worker whose `TryTake` removes the item from the collection *before*
`ItemProcessor` is invoked, so a gated processor parks with `Queue.Count` equal to 0 rather than 1, and
reading that count at any other moment is a thread-pool race. Keeping the old assertion would have made
this task and P3-T8 mutually unsatisfiable.

The replacement observes the item the queue actually handed to the seam, which is a strictly stronger
claim than a count: the test asserts that exactly one item was received and that the received item's
`Filer` is the same instance the factory produced. Determinism comes from a
`TaskCompletionSource<FilerQueueItem>` that the queue worker itself completes, so awaiting it carries no
timing assumption, and a second gate holds the processor open for the duration of the assertions and is
released in a `finally`.

This artifact, together with the P3-T8 zero-match check for `GetField("guard"`, supplies the evidence
for the AC13 check-off in P8-T17.
