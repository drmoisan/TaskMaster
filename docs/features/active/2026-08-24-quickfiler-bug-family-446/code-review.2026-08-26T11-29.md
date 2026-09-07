# Code Review — quickfiler-bug-family-446

- Reviewer: feature-review agent
- Timestamp: 2026-08-26T11-29
- Scope: full branch diff `61edc19b...fd746f55` (13 C# files; production changes in `QfcStreamingDequeueConfidenceGate.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.cs`, `QfcFormController.Actions.cs`, `QfcHomeController.Iteration.cs`, `IQfcDatamodel.cs`)

## Findings

| ID | Severity | File | Finding |
| --- | --- | --- | --- |
| CR-1 | Minor (non-blocking) | `QuickFiler/Controllers/QfcFormController.Actions.cs` | The coverage carve-out for this file is accepted, but its untestable regions (`UndoDialog` with three raw `MessageBox.Show` calls, `ProcessUndoItemAsync` COM/dispatcher body, `LoadItems*` overloads at lines 29–160) have no routed follow-up. None of the four promoted potential documents added by this branch covers a `MessageBox`/dialog seam or loader-seam uplift for this file. Recommendation: route a seam-introduction follow-up (dialog-service seam plus loader seams) through the promotion lifecycle at epic close, so the carve-out debt does not survive as prose only. |
| CR-2 | Info | `QfcStreamingDequeueConfidenceGate.cs` + `QfcDatamodel.QueueProcessing.cs` | Rejected-hook release is double-guarded: the gate wraps `_onRejected` invocation in try/catch, and the sink `TryReleaseRejectedHook` wraps `UnhookItem` in its own try/catch. Both log via log4net. The redundancy is harmless and each guard is justified in place (the gate defends against arbitrary sinks; the sink defends its own COM call), but only one layer will ever observe a given failure. No change requested. |
| CR-3 | Minor (non-blocking) | `QuickFiler/Controllers/QfcFormController.Actions.cs:4` | Dead `using System.Diagnostics;` — sole consumer (`Stopwatch`) was removed by the `UndoConsumer` rewrite. Analyzer gate passes because IDE0005 is not error-severity here. Remove on the next authorized touch of the file. Leaving it was correct executor discipline (no plan task authorized the removal). |
| CR-4 | Minor (non-blocking) | `docs/features/active/quickfiler-bug-family-446/issue.md:5` | `- Also closes: #426, #427, #448` contradicts AC17 and decision D1 (#427 must remain open; only 427-A is delivered). The line pre-dates the merge base, so it is not a defect of this change set, and evidence `evidence/issue-updates/p4-t17-pr-closing-keyword-constraint.2026-08-26T10-41.md` already instructs the PR author not to transcribe it. Residual risk: the pr-author skill reads `issue.md`. Recommendation: correct the line to `- Also closes: #426, #448 (advances #427, which remains open)` before or during PR authoring, and verify the PR body against the P4-T17 constraint note. |
| CR-5 | Info | `QfcStreamingDequeueConfidenceGate.cs` | File branch rate moved 92.50% to 90.91% — not a covered-branch loss (covered branches rose 37 to 40) but a denominator effect from four new branches of which one is uncovered (the defensive null-accepted arm in `QfcGateBatch.Accepted` or a rejection-guard arm). Above every floor; recorded for baseline continuity in later epic reviews. |
| CR-6 | Info | `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | [P3-T7] compaction removed the blank line before `// Act` / `// Assert` markers in the four new tests. AAA structure and markers retained; noted only so later reviewers do not mistake the tightened layout for missing structure. |

## Design and correctness assessment

**#446 (queue closed on deadline expiry).** The fix is structurally sound. `QfcGateBatch`/`QfcDequeueBatch` are net481-safe readonly structs whose null-tolerant accessors make a defaulted Moq return inert — a deliberate, documented choice that prevents NRE traps in loosely-mocked tests. The gate maps all four exits to explicit stop reasons (`quantity<=0` and loop-completion to `QuantitySatisfied`, deadline to `DeadlineExpired`, drained source to `SourceExhausted`), and `IterateQueueAsync` now guards `CompleteAddingAsync` behind `Stop == QfcDequeueStop.SourceExhausted` with a why-comment naming the irreversibility of `BlockingCollection.CompleteAdding`. The non-gate path's inference (short direct batch implies source exhausted) is correct given `WaitForQueue`'s loop condition (`_remainingLoadActive && count < quantity`): a short result after the wait implies the producer is no longer active.

**#448 (non-terminating `UndoConsumer`).** The old defect is real and the fix is minimal: the base loop's `while (!_undoQueue.IsCompleted || exit)` disjunction kept spinning forever once `exit` was set, and the conditional `if (exit)` reset meant `_undoConsumerTask` was effectively never cleared. The rewrite measures idle time (reset on every successful take), waits through the injected `TimeProvider`, breaks on threshold, and clears `_undoConsumerTask` in `finally` — covering the exception exit that disposal mid-take can produce. All three seams (`TimeProvider`, `UndoConsumerStarter`, `UndoItemProcessor`) default to production behavior; `ProcessUndoItemAsync` is a verbatim extraction of the take branch. Production behavior change is exactly the intended fix, nothing more.

**#426 (rejected-hook retention).** `onRejected` is an optional constructor parameter defaulting to null, so no existing call site changes behavior; the datamodel supplies `TryReleaseRejectedHook`, giving exactly one `UnhookItem` per rejected candidate, preserving the drop-on-reject contract (`DequeueAsync_BelowThresholdItemsAreDiscarded` byte-unchanged) and the STA marshal contract (`EmailMoveMonitor.cs` untouched).

**#427-A (producer side).** `ScoreRemainingQueueMailItemAsync` now returns `(long Score, string TopFolder)` and moved to `QfcDatamodel.QueueProcessing.cs` with an injectable `ScoringServiceFactory`; the admission call site adapts with a lambda projecting `.Score`, so `QfcRemainingQueueAdmission`'s signature is untouched. Accepted candidates carry `TopFolder` to the boundary as `QfcDequeueBatch.PreScored`. The consumer side is deliberately not delivered; #427 remains open.

**Test quality.** New tests are deterministic (FakeTimeProvider/CountingTimeProvider; zero banned APIs), well-documented, and assert through FluentAssertions with reasons. The fail-closed `CreateGate` rewrite (single exact nine-type constructor lookup guarded by `Should().NotBeNull`) removes a real fail-open hazard the old descending fallback chain carried. Reflection-based access to private members (`GetPrivateField`/`SetPrivateField`, `InvokeScoreRemainingQueueMailItemAsync`) is pre-existing project style for this COM-bound surface; acceptable here.

## [P3-T7] compaction — independent re-derivation (caller-referred)

The reduction from 576 to 496 lines used four in-file changes beyond the named `ArrangeUndoConsumer` extraction. Verified against the base file text:

1. `GetPrivateField<T>`/`SetPrivateField<T>` collapsed to expression bodies over `private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance` — identical flag set to the base inline expressions; semantics unchanged (including the same NRE failure mode on a missing field).
2. `ReadControllerSource` expression-bodied (byte-equivalent call chain) and `ResolveRepositoryPath`'s manual `foreach` accumulator replaced with `pathParts.Aggregate(dir.FullName, Path.Combine)` — a left fold seeded with the repo root, which is exactly what the removed loop computed; the `Should().NotBeNull` repo-root guard is retained.
3. `UndoConsumer_OnExit_ResetsUndoConsumerTask` restructured to arrange both exit paths once — this test was authored by this same branch ([P3-T6]), so no pre-existing behavior existed to preserve; the restructured form still plants sentinels on both controllers and asserts both exit paths clear `_undoConsumerTask`.
4. Doc-comment tightening and blank-line removal — cosmetic.

No assertion was removed or weakened by any of the four; the scoped post-reduction run (`p3-t7.trx`) records 16/16 passed (12 pre-existing seam tests plus the 4 new ones). **Verdict: behavior-preserving, confirmed.** The deviation-beyond-named-task was forced by arithmetic (532 after the named extraction against a 500 cap) under D4's no-new-file constraint, and is documented with unusual thoroughness in `evidence/other/p3-t7-line-counts.2026-08-26T10-50.md`.

## Blocking findings

**0.** CR-1 through CR-6 are Minor or Info; none blocks merge to the integration branch.
