---
name: qfc-home-controller-iteration-433
description: "#433/epic #136 F7: post-#424 the 2-arg DequeueNextItemGroupAsync inherited a 12s deadline, so IterateQueueAsync's empty-batch => CompleteAdding inference is now unsound; Iterate/Iterate2 are dead production code"
metadata:
  type: project
---

Findings from the `QfcHomeController.Iteration.cs` research pass (2026-08-07, epic child F7, issue #433).

**LD3 — the load-bearing one.** Issue #424 bounded the high-confidence dequeue with a 12-second
first-batch deadline (`QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`). #424's spec
claimed the post-UI iteration call site was "left unchanged" — true of the *file*, false of the
*behavior*: `QfcDatamodel.QueueProcessing.cs` made the two-argument `DequeueNextItemGroupAsync`
delegate to the four-argument overload **with the default deadline**. `IterateQueueAsync` still
treats an empty batch as source-exhaustion and calls `QfcQueue.CompleteAddingAsync`, which reaches
the irreversible `BlockingCollection.CompleteAdding()`. A slow high-confidence scan can therefore
close the UI queue permanently while items remain.

**Why:** the regression is invisible to #424's own AC set because #424 only asserted on the
`RunAsync` call site. It surfaces only when you cross-reference the delegation change against a
call site #424 declared out of scope.

**How to apply:** when reviewing or planning anything on the QuickFiler dequeue/refill path, check
whether an "empty result means exhausted" inference still holds — it does not on any call site that
reaches the confidence gate. Do not fix this inside a coverage child; promote it.

**Second durable fact.** `QfcHomeController.Iterate()` and `Iterate2()` are dead production code
(27% of that file). `Iterate` is bound into `QfcFormController`'s private `IterateDelegate` field
which is never invoked; `Iterate2` has no caller at all. Both are on the public
`IQfcHomeController` interface, so removal is a breaking change touching an F6-owned file. Related:
[[qfc-home-controller-metrics-433]] found the same interface-obligation-only pattern for
`QuickFileMetrics_WRITE`.

**Third.** `_ = IterateQueueAsync()` in `Iterate2` discards faults; the repo has no
`ThrowUnobservedTaskExceptions` setting anywhere, so they are swallowed at finalization. The
sibling call site `RunAsync` uses `await Task.Run(...)` and has the opposite semantics.
