# Code Review — issue #633, unsynchronized undo handoff after batch move

- Timestamp: 2026-09-01T11-32
- Branch: `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633`, head `efd939cf`
- Base: `origin/main` = `06b1e02e` (equals the merge base)
- Reviewed diff: `git diff origin/main...HEAD`

## Verdict

**Approve.** Zero blocking findings. The design is sound, the concurrency reasoning is correct on every
path the change is responsible for, and the tests are genuinely discriminating rather than decorative.
Four non-blocking findings follow, one of them Major.

## What changed

| File | Change | Lines after |
|---|---|---|
| `QuickFiler/Controllers/FilerQueue.cs` | Monitor-guarded outstanding-work counter, lazily created `TaskCompletionSource<bool>` drain signal exposed as `public Task WhenDrainedAsync()`, `internal Func<FilerQueueItem, Task> ItemProcessor` seam, and replacement of the `ThreadSafeSingleShotGuard` start gate with a monitor-protected `_consumerRunning` flag. | 197 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | `BackGroundMoveAsync` awaits the drain before the metrics and cleanup dispatches; `_parent` added to the early-return guard; the two subsumed `await _parent.FilerQueue.Consumer;` statements deleted. | 408 |
| `QuickFiler.Test/Controllers/FilerQueueTests.cs` | Seven queue-level tests added; class comment corrected. | 358 |
| `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` | New — five ordering tests. | 428 |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | Reflection into the removed private `guard` field replaced with the `ItemProcessor` seam. | 470 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | One `<Compile Include>` entry. | — |

## Design assessment

### The counted barrier is correct

The increment happens inside the same critical section as `Queue.Add`, so no window exists in which an
item is queued but uncounted. `WhenDrainedAsync()` reads the counter under the same monitor, so a caller
can never observe zero while an item is in flight. `CompleteItem()` captures the signal under the monitor
and completes it outside, which is the right shape: it holds no lock across the continuation, and the
`TaskCreationOptions.RunContinuationsAsynchronously` on the signal prevents a waiter's continuation from
running inline on the queue worker thread. That flag is load-bearing here, not decorative — without it,
`BackGroundMoveAsync`'s remainder, including two `UiThread.Dispatcher.InvokeAsync` calls, could resume on
the worker thread.

The XML comment on `_sync` claims the monitor is "Never held across an `await`". Verified: all four
critical sections (`Enqueue`, `WhenDrainedAsync`, the `TryTake` block in `ConsumeAsync`, `CompleteItem`)
contain no `await`.

### The handshake repair genuinely closes the orphaned-item window

`_consumerRunning` is cleared in the same critical section in which `Queue.TryTake` fails
(`FilerQueue.cs:128-138`). A producer holding the monitor therefore observes either "a worker is running
and will see my item" or "no worker is running, so I must start one". There is no interleaving that
produces a queued item with no worker. That is a real improvement over the previous `Queue.Add`-then-read
`guard` ordering, and it is a precondition for the barrier rather than an opportunistic refactor: a drain
signal over the old handshake could report drained while an item was stranded, or never complete at all.

### The `_parent` guard clause is necessary and correctly ordered

`BackGroundMoveAsync` now dereferences `_parent.FilerQueue`, so the guard must cover `_parent`. It does
(`QfcFormController.EventHandlers.cs:217-225`). The clause is placed after `_groups`, so the pre-existing
vacuous tests that rely on the `_groups` short circuit are unaffected.

### Removal of the two `Consumer` awaits is a strict subsumption

Both deleted statements were immediately preceded by an await of the `BackGroundMoveAsync` task — at
`:166` (`await moveTask;` on the catch path of `MoveAndIterate`) and at `:191` (`await
BackGroundMoveAsync();` on the terminal branch). Both now include the barrier, which waits on the whole
outstanding count rather than on one worker task. Verified: `Grep "\.Consumer\b"` over `QuickFiler/**/*.cs`
returns zero matches, so no production read of `Consumer` survives, and `Consumer` itself is retained on
the public surface.

### The `ItemProcessor` seam is the right testability mechanism

`EmailFiler.SortAsync` is non-virtual and casts to a COM folder, so it cannot be driven from a unit test.
A `Func<FilerQueueItem, Task>` property with a production default that reproduces the previous call is
the minimum surface that makes the queue drivable. `internal` accessibility keeps it off the public
surface, and `QuickFiler/Properties/AssemblyInfo.cs` already grants `InternalsVisibleTo("QuickFiler.Test")`,
so no new attribute was needed.

## Test assessment

The tests are discriminating. Three points support that beyond the pass/fail counts:

1. **The fail-before witness failed on the predicted assertion, not incidentally.**
   `evidence/regression-testing/p2-t5.trx` records both barrier tests failing with
   `Expected CountOf(MetricsToken) to be 0 ... but found 1`, which is the defect itself: the metrics
   dispatch had already been made while an item sat in the queue behind a closed gate. `p4-t6.trx` shows
   the same two tests passing after the fix.

2. **The determinism argument is structural, not empirical.** The mocked `MoveEmailsAsync` returns an
   already-completed task, so pre-fix the metrics operation is enqueued at `ContextIdle` synchronously
   before `BackGroundMoveAsync()` returns; a probe posted afterwards at the same priority cannot complete
   until that operation has run. The tests do not depend on elapsed time anywhere. Reviewer grep for
   `Thread.Sleep`, `Task.Delay`, `.Wait(`, `.Result`, and `DateTime.Now/UtcNow` across all three touched
   test files returned zero matches.

3. **The `SeamFactoryTests` reconciliation strengthened the assertion rather than weakening it.** The old
   test asserted `filerQueue.Queue.Count == 1`, which only observed that an item had not yet been
   consumed. The replacement asserts that the queued item's `Filer` is reference-equal to the instance the
   factory produced, which is a stronger claim about the same behaviour.

`QfcFormControllerUndoHandoffTests` covers both barrier cases with work outstanding, the post-drain
metrics-then-cleanup ordering, the newly added `_parent` guard clause, and the pre-existing `_groups`
guard clause. `FilerQueueTests` covers the fresh queue, one gated item, two gated items completing in
sequence, repeated and concurrent waiters, a second batch after a drain, and a throwing processor. That
matches the scenario-completeness requirement in `.claude/rules/general-unit-test.md`.

## Non-blocking findings

### NB-1 (Major) — the worker loop does not clear `_consumerRunning` when its own `catch` handler throws

- File: `QuickFiler/Controllers/FilerQueue.cs`
- Locations: the `while (true)` loop at `:124-156`, the `catch` handler at `:144-151` (specifically
  `var first = item.Helpers.First();` at `:146`), and the flag-clearing statement at `:135`
- Rule engaged: `.claude/rules/general-code-change.md` § Error Handling and Logging — "Do not silently
  ignore errors"; and `spec.md` Risks 1 and 2, which name a hang on the batch-move path as a strictly
  worse outcome than the latent defect being fixed

`_consumerRunning = false` executes on exactly one path: the normal `TryTake`-fails exit at `:133-137`.
If any statement in the loop body throws outside the `try`/`catch`/`finally` triple — and the `catch`
handler's own body is outside it — the exception propagates out of the `while (true)` loop, out of the
`Task.Run` lambda, and faults the task assigned to `Consumer`, leaving `_consumerRunning` permanently
`true`.

The concrete route is `item.Helpers.First()` at `:146`. `FilerQueueItem`'s constructor rejects a null
`Helpers` list and a list containing null, but permits an *empty* list, and `First()` on an empty sequence
throws `InvalidOperationException`. A second route is `Enqueue((FilerQueueItem)null)`, which is a public
overload with no null guard: the default processor NREs on `item.Filer`, and the catch handler then NREs
on `item.Helpers`.

The consequence is worse after this change than before it. Pre-fix, a wedged worker meant the guard was
never reinstalled and items were silently stranded — a delayed or lost undo push. Post-fix, every
subsequent `Enqueue` increments `_outstanding`, starts no worker because `_consumerRunning` is still
`true`, and `WhenDrainedAsync()` never completes; `BackGroundMoveAsync` then awaits forever and neither
metrics nor cleanup runs for the remainder of the session. The faulted `Consumer` task is also never
observed by anything, so the original exception is not logged either.

The implementation is aware of this hazard: `FilerQueueTests.cs:99-103` documents it verbatim as the
reason every test helper enqueues a real helper. Working around it in the test fixture rather than
hardening the worker leaves the production hazard in place.

Neither route is reachable from the single production call site today —
`QfcItemController.MailActions.cs:136` passes `PackageItems()` under an `ItemHelper is not null` guard,
and no in-tree caller passes a null item — which is why this is Major rather than blocking. It is a
latent hazard in a change whose entire purpose is to remove a latent hazard.

Recommended remediation (a separate issue; not for this branch):

1. Wrap the loop body in a `try`/`finally` (or the whole `while`) whose `finally` clears
   `_consumerRunning` under `_sync` and drains the counter, so no escape path can wedge the queue.
2. Make the diagnostic in the `catch` null- and empty-safe, for example
   `item?.Helpers?.FirstOrDefault()` with a null-tolerant message.
3. Consider adding a null guard to `Enqueue(FilerQueueItem)`.

### NB-2 (Minor) — `ConsumeAsync()` is public and can now corrupt an invariant it does not own

- File: `QuickFiler/Controllers/FilerQueue.cs:120`

`ConsumeAsync()` was `public` before this change and remains so, but it now participates in an invariant
it does not establish. An external caller invoking it directly starts a second concurrent worker without
setting `_consumerRunning`; when that worker's `TryTake` fails it clears `_consumerRunning` while the
legitimate worker is still running, reopening exactly the orphaned-item window this change closed. The
sibling implementation `TaskVisualization/FlagChangeTrainingQueue.cs:37` declares the same method
`internal`, which is the accessibility that matches the responsibility.

Sealing it would be a breaking public-API change, which `spec.md`'s "additive on the public surface"
boundary forbids for this issue, so no action is recommended here. The minimum useful step is an XML
`<remarks>` warning that the method must not be called directly. The same applies more weakly to the
public `Consumer` property: it is now dead in-tree and still reads like a quiesce primitive, and the
stale-reference window described in `spec.md:215-217` — `Task.Run` starts inside `ConsumeAsync()` before
the returned task is stored in the non-volatile auto-property at `:65` — still exists. That window is
harmless in-tree precisely because production no longer reads `Consumer`.

### NB-3 (Minor) — `WhenDrainedAsync()` is queue-wide, not per-batch

- File: `QuickFiler/Controllers/FilerQueue.cs:104-118`

The specification repeatedly calls the barrier "counted, per-batch". The implementation is counted and
awaitable but not per-batch: it waits for the queue's entire outstanding count, so a concurrent producer's
items would extend the wait. This is correct in the current topology — `spec.md` establishes that
QuickFiler is the only production consumer and that `EfcHomeController.FilerQueue` throws
`NotImplementedException` — and the XML documentation on the method describes the actual semantics
accurately ("has no outstanding work"), so only the specification prose is loose. Worth noting because a
future second producer would silently change the barrier's meaning.

Related: the awaited barrier accepts no `CancellationToken` and has no upper bound. `QfcFormController`
has a `Token` available. If a filing item hangs — a plausible failure mode for COM work — the batch-move
path hangs with it, and cancelling the QuickFiler session does not release it. `spec.md` Risk 4 accepts a
pause bounded by filing time; an unbounded hang is a different case and is not covered there.

### NB-4 (Minor) — pre-existing parallelism flake in `UtilitiesCS.Test`, confirmed

- File: `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`
- Executor's analysis: `evidence/other/p7-loop-attempt-1-failure.2026-09-01T11-08.md`

The executor's reasoning holds. Each premise was checked against primary sources rather than accepted:

| Premise | Verification |
|---|---|
| The test redirects process-global `Console.Out` | Confirmed: `Console.SetOut(writer)` at `:102`, restored at `:111`. |
| `TaskMaster.runsettings` enables parallel execution | Confirmed: a `<Parallelize>` element is present. |
| The repository already mitigates the identical hazard on a sibling | Confirmed: `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs:19` carries `[DoNotParallelize]`, with an explanatory comment at `:14-18`. |
| `DASLFilterParserTests` carries no such attribute | Confirmed: no `DoNotParallelize` token anywhere in the file. |
| No dependency path from the change to the failing test | Confirmed: `UtilitiesCS` sits below `QuickFiler` in the dependency graph and cannot reference `QuickFiler.Controllers.FilerQueue`. |
| The same test passed on the clean rerun with no file edited in between | Confirmed: `evidence/qa-gates/p7-t6-test-coverage.2026-09-01T11-10.md` records 6924/6924 passed, and `p7-pass2-gates` records a zero-entry porcelain set difference across the format step. |

The conclusion — a pre-existing, scheduling-dependent `Console.SetOut` flake, not a regression from this
change — is **confirmed**, not refuted. Adding `[DoNotParallelize]` to `DASLFilterParserTests` would touch
`UtilitiesCS.Test/`, which AC16 excludes, so leaving it unfixed on this branch was correct. It should be
filed as its own issue from a separate branch.

## Evidence-quality observations

These are not code findings; they concern the audit trail this branch commits.

1. **Host-token leakage in committed evidence.** `evidence/qa-gates/p8-t1-sanitisation.2026-09-01T11-15.md`
   states that the committed evidence tree "carries no absolute host path in any file's content". That
   claim is broader than the sweep it describes, which replaced only the three spellings of the *worktree*
   path. Sixteen committed evidence files plus `plan.2026-08-31T19-35.md` still contain the developer
   account token; `evidence/qa-gates/p7-t4-analyze.msbuild.txt` contains 36 occurrences, including
   `/analyzerconfig:` pointing at the main checkout outside the worktree, and the eight TRX files contain
   `<account>@<HOST>` and a run name of the form `<account>_<HOST>_<timestamp>`. No repository policy
   document mandates sanitisation, so this is not graded as a policy violation; it is graded as an
   accuracy defect in an evidence artifact plus a hygiene item worth fixing. The corrective sweep would
   touch only files under `docs/`, so it is inside the AC16 footprint if it is done on this branch.

2. **The primary coverage XMLs are not committed.** `coverage/baseline.cobertura.xml` and
   `coverage/post-change.cobertura.xml` live in a gitignored directory. This reviewer parsed them directly
   in-session and every figure reconciled exactly, so the coverage claims are verified now; but after the
   working tree is cleaned, only the derived markdown remains and the figures become unre-derivable from
   the repository alone.

3. **Phase 7 pass-2 evidence granularity.** The two MSBuild file logs on disk are the pass-2 logs, because
   the file logger overwrote pass 1. The pass-2 CSharpier `check` result, however, exists only as a quoted
   summary line inside `p7-pass2-gates.2026-09-01T11-10.md`; the standalone `p7-t3` artifact is pass 1.
   This reviewer removed the gap by re-running `dotnet tool run csharpier check .` against the committed
   head: exit 0, `Checked 1566 files in 4637ms.`, zero unformatted files.

## Positive observations worth preserving

- The `finally { CompleteItem(); }` placement is correct and is the single most important detail in the
  change: a leaked count would hang the batch-move path, and the throwing-processor test pins it.
- The counter increment is inside the same lock as `Queue.Add`, not adjacent to it. That ordering is what
  makes `WhenDrainedAsync()` sound rather than approximately sound.
- `Enqueue(EmailFiler, IList<MailItemHelper>)` delegating to the item overload *after* constructing the
  item in its own frame preserves the synchronous `ArgumentNullException` contract that
  `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException` depends on. That constraint is easy to lose
  in a delegation refactor and it was not lost.
- The class comment on `FilerQueueTests` was corrected rather than left stale after the seam made the
  previously excluded path reachable.
- The `spec.md` "Deviation from the research record" section records the inverted fail-before split and
  the test file the research record omitted, both of which a reviewer would otherwise have to discover.
