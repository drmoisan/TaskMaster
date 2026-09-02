# Feature Audit — issue #633, unsynchronized undo handoff after batch move

- Timestamp: 2026-09-01T11-32
- Branch: `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633`, head `efd939cf`
- Baseline: `origin/main` = `06b1e02e` (equals the merge base)
- Work mode: `full-bug` — `spec.md` is the sole acceptance-criteria source, per the marker at
  `issue.md:15` and the restatement at `spec.md:9-10`
- AC source: `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/spec.md`,
  section `## Acceptance Criteria` (lines 574-657)
- `user-story.md`: absent by design for `full-bug`. Not a gap and not reported as one.

## Verdict

**PASS.** All 20 acceptance criteria are met. Each was verified against a primary source — the working
tree, a TRX file, an MSBuild log, or the Cobertura XML — rather than accepted from the executor's summary
markdown. No criterion is UNVERIFIED.

## Acceptance criteria evaluation

| # | Criterion (abbreviated) | Verdict | Verification performed by this reviewer |
|---|---|---|---|
| AC1 | `FilerQueue.cs` exposes `public Task WhenDrainedAsync()`, returning an already-completed task when nothing is outstanding | **PASS** | Declaration read at `FilerQueue.cs:104`; the `_outstanding == 0` branch returns `Task.CompletedTask` at `:110`. `WhenDrainedAsync_OnFreshQueue_ReturnsCompletedTask` present at `FilerQueueTests.cs:107-120`; passing in `evidence/regression-testing/p5-t10/p5-t10.trx` (12 passed, 0 failed). |
| AC2 | The drain task does not complete while any enqueued item is still processing | **PASS** | `WhenDrainedAsync_WithGatedItem_DoesNotCompleteBeforeItemCompletes` (`:122-151`) and `WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete` (`:187-230`). Both drive the queue through `TaskCompletionSource` gates on `ItemProcessor`, both assert `IsCompleted == false` while a gate is closed, both pass in `p5-t10.trx`. Mechanism confirmed in source: the increment at `FilerQueue.cs:54` is inside the same lock as `Queue.Add` at `:55`. |
| AC3 | The drain completes once every item has completed, each processor having run exactly once | **PASS** | `WhenDrainedAsync_AfterGateReleased_CompletesAndItemRanOnce` (`:153-185`) asserts `invocations == 1`; `WhenDrainedAsync_WithTwoGatedItems_...` asserts `invocations == 2`. Both pass. |
| AC4 | `WhenDrainedAsync()` is idempotent; repeated and concurrent waiters all complete; a post-idle call returns a completed task | **PASS** | `WhenDrainedAsync_AwaitedTwice_BothWaitersComplete` (`:232-275`) obtains two waiters before release, awaits both via `Task.WhenAll`, then asserts a third post-idle call is already completed. Passing. Mechanism confirmed: the single lazily created `_drainSignal` is shared by all waiters (`FilerQueue.cs:113-116`) and cleared on completion (`:174`). |
| AC5 | The orphaned-item window is closed: an item enqueued after a previous drain is processed with no further enqueue | **PASS** | `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` (`:277-320`) passing. Mechanism confirmed independently: `_consumerRunning` is cleared inside the same critical section in which `Queue.TryTake` fails (`FilerQueue.cs:128-137`), so no producer can observe a stale "running" state after a worker has decided to stop. |
| AC6 | A throwing item still decrements, the loop continues, and the drain completes rather than faulting or hanging | **PASS** | `ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes` (`:322-356`) asserts `drain.IsFaulted == false`, `drain.IsCompleted == true`, and `invocations == 2`. Passing. Mechanism confirmed: `CompleteItem()` is invoked from a `finally` at `FilerQueue.cs:152-155`. The existing `catch` and its `logger.Error` call are preserved verbatim; the test asserts the observable behaviour rather than the log text, which is what the Test Strategy specifies. |
| AC7 | `BackGroundMoveAsync` awaits `WhenDrainedAsync()` after `MoveEmailsAsync` and before both dispatches | **PASS** | Read directly: `QfcFormController.EventHandlers.cs:228` awaits `MoveEmailsAsync`, `:234` awaits `_parent.FilerQueue.WhenDrainedAsync()`, `:237` and `:242` are the two `UiThread.Dispatcher.InvokeAsync` calls. No statement between `:228` and `:234` bypasses the barrier. `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` and `..._DoesNotWriteMetricsBeforeDrain` both pass in `evidence/qa-gates/p7-t8/p7-t8.trx` (5 passed, 0 failed) and both **failed** against the pre-fix tree in `evidence/regression-testing/p2-t5/p2-t5.trx`. |
| AC8 | Metrics-before-cleanup ordering preserved; each runs once after the drain | **PASS** | `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp` asserts `CountOf(MetricsToken) == 1`, `CountOf(CleanupToken) == 1`, and `RecordedOrder().Equal([metrics, cleanup])`. Passing. Source order at `:237-242` is unchanged relative to `origin/main`; the diff inserts only above it. |
| AC9 | The early-return guard includes a `_parent` null check | **PASS** | Guard read at `:217-225`: `_groups is null \|\| _parent is null \|\| _globals?.FS?.Filenames is null \|\| WriteMetrics is null`. `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing` sets `_parent` to null via reflection, re-reads it to prove the arrangement took, asserts the task is not faulted, and verifies `MoveEmailsAsync` was never invoked. Passing. |
| AC10 | The two production `Consumer` reads are removed; `Grep "\.Consumer\b"` over `QuickFiler/**/*.cs` returns zero matches | **PASS** | Reviewer ran the grep: **zero matches**. Diff confirms deletion of `await _parent.FilerQueue.Consumer;` at the former `:167` (catch path) and `:193` (terminal branch). Subsumption verified: each deleted statement was immediately preceded by an await of the `BackGroundMoveAsync` task, which now contains the barrier. |
| AC11 | `Consumer` retains its type, accessibility and completed-task default; the pinning test passes unmodified | **PASS** | Declaration unchanged at `FilerQueue.cs:76`: `public Task Consumer { get; private set; } = Task.CompletedTask;`. The diff of `FilerQueueTests.cs` shows the body of `FilerQueue_NewInstance_HasCompletedConsumerByDefault` is untouched — only the class doc comment changed and new methods were appended. Passing in `evidence/qa-gates/p6-t10/p6-t10.trx` (1 passed, 0 failed). |
| AC12 | `Enqueue(EmailFiler, IList<MailItemHelper>)` still raises `ArgumentNullException` synchronously in the caller's frame | **PASS** | The overload constructs `new FilerQueueItem(filer, helpers)` in its own frame at `FilerQueue.cs:73` before delegating; the constructor's `ThrowIfNull` and any-null guard are at `:186-191`. `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException` passes in `evidence/qa-gates/p6-t8/p6-t8.trx` (1 passed, 0 failed), and its file `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` does not appear in the branch diff, so it is unmodified. |
| AC13 | `QfcItemController.SeamFactoryTests.cs` reconciled; no longer reflects into a private `FilerQueue` field | **PASS** | Diff shows the `GetField("guard", ...)` reflection and the `ThreadSafeSingleShotGuard._state` write removed, along with the now-unused `using System.Reflection;` and `using UtilitiesCS.Threading;`. Replaced by a gated `ItemProcessor` that records received items. `MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues` passes in `evidence/qa-gates/p6-t9/p6-t9.trx` (1 passed, 0 failed). The replacement assertion is stronger than the one it replaced. |
| AC14 | No banned wait API in the three named test files | **PASS** | Reviewer ran `grep -nE "Thread\.Sleep\|Task\.Delay\|\.Wait\(\|\.Result\b\|DateTime\.(Now\|UtcNow)"` over `FilerQueueTests.cs`, `QfcFormControllerUndoHandoffTests.cs` and `QfcItemController.SeamFactoryTests.cs`: **zero matches**, exit 1. |
| AC15 | No `init`, `record`, or `record struct`; compiles on net481 without CS0518 | **PASS** | Reviewer ran `grep -nE "\binit\s*[;{]\|\brecord\b"` over both changed production files: **zero matches**, exit 1. `evidence/qa-gates/p7-t5-nullable.msbuild.txt` reports `0 Error(s)`; the recorded CS0518 count is 0. |
| AC16 | The production diff touches no file other than the two named files; nothing outside `QuickFiler.Test/` and `docs/` | **PASS** | Reviewer ran `git diff --name-status origin/main...HEAD`: 74 paths — 2 under `QuickFiler/` (exactly the two named files), 4 under `QuickFiler.Test/` (the three test files plus the `.csproj`), 68 under `docs/`, 0 elsewhere. No `.claude/` path and no `artifacts/` path appears. The final commit `efd939cf` explicitly restored `.claude/agent-memory/` to base to honour this criterion. Working tree is clean. |
| AC17 | `QuickFiler.Test.csproj` carries a `<Compile Include>` entry for the new test file, and the new tests appear in run output | **PASS** | Diff shows `<Compile Include="Controllers\QfcFormControllerUndoHandoffTests.cs" />` added after the `FilerQueueTests.cs` entry. `evidence/qa-gates/p7-t8/p7-t8.trx` contains 5 `outcome="Passed"` results, one per new ordering test, and 0 failed. The full-suite total rose from 6912 to 6924, exactly the 12 added tests. |
| AC18 | Both changed production files remain under 500 lines | **PASS** | Reviewer counted with `awk 'END{print NR}'`: `FilerQueue.cs` = **197**, `QfcFormController.EventHandlers.cs` = **408**. Both under 500. The three touched test files are also under 500 (358, 428, 470). |
| AC19 | The full C# toolchain passes in a single uninterrupted pass, in order, with non-vacuous build gates | **PASS** | Format: reviewer re-ran `dotnet tool run csharpier check .` against the committed head — exit 0, `Checked 1566 files in 4637ms.`, 0 unformatted. Analyze: `p7-t4-analyze.msbuild.txt` — `Build succeeded.`, `5 Warning(s)`, `0 Error(s)`, reviewer-counted `Skipping target "CoreCompile"` = **0**. Type-check: `p7-t5-nullable.msbuild.txt` — same result, `Skipping target "CoreCompile"` = **0**. Test: `Test Run Successful. Total tests: 6924, Passed: 6924`, exit 0. Both commands use `/t:Rebuild` and omit `/p:Nullable=enable`. The first Phase 7 attempt failed on an unrelated `UtilitiesCS.Test` flake and the loop was restarted from the top, which is the behaviour the mandatory toolchain loop requires; the pass this criterion rests on is the second one. |
| AC20 | Coverage does not regress on any changed line, and the members added or modified in `FilerQueue.cs` reach at least 90 % line coverage | **PASS** | Reviewer parsed `coverage/post-change.cobertura.xml` and `coverage/baseline.cobertura.xml` directly. `FilerQueue.cs` per-file line coverage moved from 18/49 = 36.73 % to **96/96 = 100.00 %**, with an empty uncovered set, so every added and modified member — `WhenDrainedAsync`, both `Enqueue` overloads, `ConsumeAsync`, `CompleteItem` — clears 90 %. Changed-line coverage: 138 changed lines, 65 instrumented, **0 uncovered**. Repository-wide line coverage rose from 85.3172 % to 85.3910 % and branch coverage from 79.3172 % to 79.4014 %, both over an identical nine-package first-party denominator, so the repo-wide figure was not lowered. |

## Baseline comparison

| Measure | `origin/main` (`06b1e02e`) | Head (`efd939cf`) | Delta |
|---|---|---|---|
| Total tests | 6912 | 6924 | +12 |
| Failing tests | 0 | 0 | 0 |
| MSBuild warnings (analyze) | 5 | 5 | 0 |
| MSBuild errors | 0 | 0 | 0 |
| CSharpier unformatted files | 0 | 0 | 0 |
| Repo-wide line coverage | 85.3172 % | 85.3910 % | +0.0738 pt |
| Repo-wide branch coverage | 79.3172 % | 79.4014 % | +0.0842 pt |
| `FilerQueue.cs` line coverage | 36.73 % | 100.00 % | +63.27 pt |
| `QfcFormController.EventHandlers.cs` line coverage | 45.38 % | 49.41 % | +4.03 pt |
| `FilerQueue.cs` line count | 83 | 197 | +114 |
| `QfcFormController.EventHandlers.cs` line count | 399 | 408 | +9 |
| Production reads of `FilerQueue.Consumer` | 2 | 0 | −2 |

No regression on any measure.

## Does the change fix the reported defect?

Yes. The issue asks for one of two remedies: either the batch-move completion awaits the undo pushes for
that batch, or the ordering dependency is made explicit so a future change to `WriteMetrics` or
`CleanupBackground` cannot start depending on entries that are not yet present. The delivered change
provides the first, which subsumes the second: after `QfcFormController.EventHandlers.cs:234` there is no
control-flow path from a completed batch move to either downstream dispatch that does not pass through
the barrier. The ordering is enforced by control flow rather than by a comment.

The soundness of that barrier depends on the queue's start/stop handshake, and the handshake repair
delivered alongside it is a genuine precondition rather than an opportunistic refactor: a drain computed
over the previous one-shot guard could report drained while an item was stranded. This reviewer verified
that argument independently from the source rather than accepting it from the specification.

One residual hardening gap in the repaired worker is recorded as NB-1 in
`code-review.2026-09-01T11-32.md`. It is not reachable from any current production call site and no
acceptance criterion covers it, so it does not affect this verdict.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/spec.md`
- Total AC items: 20
- Checked off (delivered): 20
- Remaining (unchecked): 0
- Items remaining: none

All 20 checkboxes were already marked `[x]` in `spec.md` by commit `2b33cecc` before this review. This
reviewer verified each one independently and confirms every check-off is supported by evidence. No
checkbox was added, altered, or reverted by this review, and no criterion text was modified.
