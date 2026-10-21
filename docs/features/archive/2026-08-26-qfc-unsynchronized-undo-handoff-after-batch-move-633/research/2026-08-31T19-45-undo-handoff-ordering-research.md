# Issue #633 — Unsynchronized undo handoff after batch move: research

- **Issue:** #633
- **Feature folder:** `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/`
- **Branch:** `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633` (base `origin/main` 9b6aff2e)
- **Author:** task-researcher
- **Timestamp:** 2026-08-31T19-45
- **Status:** Complete

---

## Executive summary

**Recommended remedy (remedy family (i), expressed as an API that makes the misuse impossible):** add a
counted, per-batch, awaitable quiesce to `QuickFiler/Controllers/FilerQueue.cs` — an outstanding-work
counter incremented inside `Enqueue` and decremented after each item's processing completes, exposed as
a `Task WhenDrainedAsync()` — and have `QfcFormController.BackGroundMoveAsync` await it immediately
after `await _groups.MoveEmailsAsync(_movedItems)` and before the `WriteMetrics` and
`CleanupBackground` dispatches. Because `MoveEmailsAsync` has already enqueued every item of the batch
by the time it returns, the counter at that instant is an exact upper bound on the batch, so the await
is a correct batch-scoped barrier rather than a heuristic wait.

A second, load-bearing finding: the existing `FilerQueue.Consumer` **cannot** serve as the quiesce
primitive. It is not merely stale-prone — the current `Enqueue`/`ConsumeAsync` handshake has a window
in which an enqueued item is orphaned with no consumer running and `Consumer` already completed
(§A.4). The fix therefore has to repair that handshake, not just add an await.

---

## A. The queue

### A.1 Declaration and ownership

| Fact | Citation |
|---|---|
| Concrete type is `QuickFiler.Controllers.FilerQueue`, a plain `public class` (no interface) | `QuickFiler/Controllers/FilerQueue.cs:14` |
| Interface property that exposes it, typed as the **concrete class**, not an abstraction | `QuickFiler/Interfaces/IFilerHomeController.cs:33` (`FilerQueue FilerQueue { get; }`) |
| `IQfcHomeController` inherits that member | `QuickFiler/Controllers/IQfcHomeController.cs:9` |
| QFC's single live instance, created eagerly, never reassigned, never null | `QuickFiler/Controllers/QfcHomeController.cs:397` (`public FilerQueue FilerQueue { get; } = new FilerQueue();`) |
| EFC's implementation throws, i.e. the queue is QFC-only | `QuickFiler/Controllers/EfcHomeController.cs:421` (`=> throw new NotImplementedException();`) |

### A.2 Storage and worker loop

- Backing store is a `BlockingCollection<FilerQueueItem>` with an internal getter:
  `QuickFiler/Controllers/FilerQueue.cs:20`.
- Two `Enqueue` overloads, both with the identical two-statement body
  (`Queue.Add(...)`, then start the consumer if the single-shot guard admits):
  `QuickFiler/Controllers/FilerQueue.cs:22-29` and `:31-38`.
- The start gate is a `ThreadSafeSingleShotGuard` field: `QuickFiler/Controllers/FilerQueue.cs:40`.
  Its only member is an `Interlocked.Exchange`-based one-shot flag:
  `UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs:24-27`.
- Worker loop: `QuickFiler/Controllers/FilerQueue.cs:44-65`. It is `Task.Run(async () => { while
  (Queue.TryTake(out var item)) { try { _ = await item.Filer.SortAsync(item.Helpers); } catch { log }
  } guard = new ThreadSafeSingleShotGuard(); })`. `TryTake` is the **non-blocking** overload, so the
  loop exits the moment the queue is momentarily empty, then installs a fresh guard so a later
  `Enqueue` can start a new worker.
- `FilerQueueItem` is a two-property carrier with `ThrowIfNull` validation and an explicit
  any-element-null guard: `QuickFiler/Controllers/FilerQueue.cs:68-82`.

### A.3 What `Consumer` is

- `public Task Consumer { get; private set; } = Task.CompletedTask;` —
  `QuickFiler/Controllers/FilerQueue.cs:42`.
- It is assigned in exactly two places, both inside `Enqueue`, both gated on
  `guard.CheckAndSetFirstCall`: `QuickFiler/Controllers/FilerQueue.cs:27` and `:36`.
- It completes when the `Task.Run` body returns, i.e. when the worker observes an empty queue.
  It is therefore **not** a lifetime task and **not** single-await: awaiting the same `Task` instance
  twice is safe (a `Task` may be awaited any number of times), and successive batches install
  successive `Consumer` instances.
- Verified default state is a completed task on a fresh queue:
  `QuickFiler.Test/Controllers/FilerQueueTests.cs:77-87`.

### A.4 Why `Consumer` is not a sound quiesce primitive (three defects in the current handshake)

1. **Orphaned-item window.** `Enqueue` performs `Queue.Add(item)` (`:24`/`:33`) and *then* reads
   `guard` (`:25`/`:34`). The worker exits its `while` when `TryTake` returns false (`:48`) and only
   *afterwards* installs a fresh guard (`:63`). If a producer executes `Add` and reads the **old**
   (already-tripped) guard in the interval between those two worker statements, `CheckAndSetFirstCall`
   returns `false`, no worker is started, the worker then replaces the guard and completes — and the
   item sits in the queue unprocessed, with `Consumer` completed. Its undo entry is never pushed until
   some unrelated later `Enqueue` happens to restart a worker. This is a genuine lost-work window, not
   only a stale-read window.
2. **Stale-reference window.** `Consumer = ConsumeAsync()` (`:27`/`:36`) starts the `Task.Run` inside
   `ConsumeAsync` *before* the returned task is stored in the non-volatile auto-property. A reader of
   `Consumer` on another thread in that interval observes the previous, completed task.
3. **Worker overlap.** Because the guard is reset at `:63` *inside* the `Task.Run` body, a new
   `Enqueue` can legitimately start a second worker while the first task has not yet completed. The
   `Consumer` assignment then overwrites the reference to the still-running first task, so awaiting
   `Consumer` does not cover it.

### A.5 Existing completion / drain / count concepts

There is **no** completion, drain, quiesce, or outstanding-work count concept on `FilerQueue` today
beyond `Consumer`. `Queue.Count` (`BlockingCollection.Count`, reachable via the internal `Queue`
property at `:20`, read by a test at `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:234`)
counts only *un-taken* items; it excludes the item currently in flight inside `SortAsync`, so it is not
a quiesce signal either.

A structurally identical sibling exists at `TaskVisualization/FlagChangeTrainingQueue.cs:14-45`
(same `BlockingCollection` + `ThreadSafeSingleShotGuard` + `internal Task Consumer` shape, awaited at
`TaskVisualization.Test/FlagChangeTrainingQueueTests.cs:46`). It is a **different type** and is out of
scope for #633; it is recorded here only so a future reader does not mistake it for the same object.

---

## B. The undo push

| Fact | Citation |
|---|---|
| The push site | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:185-189` (`PushToUndoStack`) |
| The stack instance and owner | `Globals.AF.MovedMails` — `EmailFiler.cs:188`; declared `public SloStack<IMovedMailInfo> MovedMails` at `TaskMaster/AppGlobals/AppAutoFileObjects.cs:178-181` |
| Element type pushed | `new MovedMailInfo(beforeMove, afterMove, Globals.Ol.Root.FolderPath)` — `EmailFiler.cs:187` |
| Granularity: **per `MailItemHelper`**, not per filer and not per batch | `EmailFiler.ProcessMailHelperAsync` calls `PushToUndoStack` once per helper (`EmailFiler.cs:179`), inside the `foreach (var mailHelper in MailHelpers)` loop in `SortAsync` (`EmailFiler.cs:146-149`) |
| The push is conditional on a successful COM move | `EmailFiler.cs:175` (`if (mailItemTemp is not null)`) — a failed move pushes nothing |
| Path from the queue worker to the push | `FilerQueue.cs:52` → `EmailFiler.SortAsync(IList<MailItemHelper>)` `EmailFiler.cs:128-135` → `SortAsync()` `:137-153` → `ProcessMailHelperAsync` `:155-183` → `PushToUndoStack` `:185` |

**Serialization of pushes.** Two independent mechanisms:

1. Only one `FilerQueue` worker is intended to run at a time (single-shot guard,
   `FilerQueue.cs:25`/`:34`), and within one worker the item loop and the per-helper loop are both
   sequential and awaited (`FilerQueue.cs:48-52`; `EmailFiler.cs:146-149`).
2. The stack itself is lock-protected: `SloStack<T>.Push` → `AddFirst`
   (`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack.cs:131`) →
   `LockingObservableLinkedList.AddFirst`
   (`UtilitiesCS/ReusableTypeClasses/Locking/Observable/LinkedList/LockingObservableLinkedList.cs:108-112`)
   → `LockingLinkedList.AddFirst`, whose body is `lock (this) { base.AddFirst(item); }`
   (`UtilitiesCS/ReusableTypeClasses/Locking/LockingLinkedList.cs:54-60`).

Mechanism 2 holds unconditionally; mechanism 1 is weakened by the worker-overlap window in §A.4.3, but
mechanism 2 means overlap produces interleaving, not corruption.

---

## C. `CleanupBackground()` and `WriteMetrics` — is the defect live or latent?

### C.1 `CleanupBackground()`

Body: `QuickFiler/Controllers/QfcCollectionController.cs:867-884`. It does exactly two things:

1. For each cached move group, `group.ItemController?.Cleanup()` (`:871-874`), then resets
   `_itemGroupsToMove` to `Array.Empty<QfcItemGroup>()` (`:880`).
2. `_itemTlpToMove.Dispose()` (`:882-883`).

`QfcItemController.Cleanup()` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:447-482`) nulls
the controller's own fields, including `_mailItem = null` (`:463`), `ItemHelper = null` (`:475`) and
`_mailActions = null` (`:481`), disposes `_emailIsReadTimer` (`:478`) and unwires events (`:458`).

**It does not touch, clear, or read the undo stack.** No occurrence of `MovedMails`, `_movedItems`,
`IMovedMailInfo`, or `Push`/`Pop` appears anywhere in `CleanupBackground` or in
`QfcItemController.Cleanup`.

**Does it release objects the queued work still needs?** No. `MoveMailAsync` captures the helper list
by value into the queue item at `QuickFiler/Controllers/QfcItemController.MailActions.cs:114`
(`IList<MailItemHelper> helpers = PackageItems();`) and `:136`
(`_homeController.FilerQueue.Enqueue(filer, helpers);`), and `FilerQueueItem` stores that same list
reference (`QuickFiler/Controllers/FilerQueue.cs:73`, `:81`). `PackageItems`
(`QfcItemController.MailActions.cs:192-197`) returns either the conversation resolver's list or a new
`List<MailItemHelper> { ItemHelper }`. Setting the controller's `ItemHelper` field to `null` drops the
*controller's* reference; the `MailItemHelper` objects themselves remain reachable from the queue item
and are unaffected. No `Dispose`/`ReleaseComObject` of the helper or its `MailItem` occurs in
`Cleanup()`.

**Conclusion: the defect is latent, not live.** Nothing downstream of the missing barrier reads the
undo stack today, and nothing in cleanup invalidates state the pending pushes depend on. The cost is
exactly what the issue states: the ordering constraint is not expressed anywhere, so a future edit to
either downstream step can silently start depending on entries that are not yet present.

### C.2 `WriteMetrics`

`WriteMetrics` is a delegate bound to `parent.WriteMetricsAsync` in the constructor
(`QuickFiler/Controllers/QfcFormController.cs:47`, declared `:82-83`). The target is
`QfcHomeController.WriteMetricsAsync` (`QuickFiler/Controllers/QfcHomeController.Metrics.cs:107-180`).
It reads the stopwatch (`:137`), `_formController.Groups.EmailsToMove` (`:143`), writes a calendar
appointment (`:154-160`), calls `GetMoveDiagnostics` (`:162-169`) and flushes through the
`MetricsFileWriter` seam (`:179`).

`GetMoveDiagnostics` (`QuickFiler/Controllers/QfcCollectionController.cs:2359-2425`) reads
`_itemGroupsToMove` and each group's `ItemController.ItemHelper` (`:2394`, `:2417-2419`).
**No undo-stack dependency in either.** It does, however, confirm a second, already-correct ordering
constraint: metrics must run *before* `CleanupBackground` resets `_itemGroupsToMove` and nulls
`ItemHelper`, which the current statement order at
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228-233` already satisfies. Any fix must not
reorder those two.

### C.3 The call path, restated with current line numbers

- `ActionOkAsync` → `MoveAndIterate` — `QfcFormController.EventHandlers.cs:110-134`, `:145-213`.
- Batch branch: `_groups.CacheMoveObjects()` `:156`; `var moveTask = BackGroundMoveAsync();` `:157`;
  `await LoadUiFromQueue()` `:161`; `await _parent.IterateQueueAsync()` `:162`; `await moveTask` `:175`.
  **This branch never awaits the filer queue at all.**
- Catch path: `await moveTask` `:166`, then `await _parent.FilerQueue.Consumer` `:167`.
- Terminal branch: `await BackGroundMoveAsync()` `:192`, then `await _parent.FilerQueue.Consumer`
  `:193` — i.e. after `CleanupBackground` has already run. This is the "lead" from the delegation
  prompt, confirmed: the wait exists but is placed one level up and one step too late, and rests on the
  unreliable primitive analysed in §A.4.
- `BackGroundMoveAsync` `:215-234`: guard `:219-222`; `await _groups.MoveEmailsAsync(_movedItems)`
  `:225`; `WriteMetrics` dispatch `:228-231`; `CleanupBackground` dispatch `:233`.
- `MoveEmailsAsync` (`QfcCollectionController.cs:2262-2290`) awaits `TryMoveEmailByGroupIndexAsync`
  for each cached group sequentially (`:2282-2285`), each of which awaits
  `group.ItemController.MoveMailAsync()` (`:2316`). Its `stackMovedItems` parameter is explicitly
  discarded (`:2269`) and documented as not carrying the undo records (`:2250-2261`).
- `MoveMailAsync` (`QfcItemController.MailActions.cs:105-158`) enqueues at `:136` and returns
  `await Task.CompletedTask` at `:137`.

**Load-bearing consequence:** by the time `await _groups.MoveEmailsAsync(...)` returns at line 225,
every item of the batch has already been passed to `FilerQueue.Enqueue` synchronously. A quiesce
observed at line 226 therefore covers the whole batch. It may also cover a still-draining earlier
batch; that is a superset, which is sound (it can only wait longer, never less).

---

## D. Remedy families

### D.1 Recommended — (i) the batch-move completion awaits the batch's undo pushes

**Shape.** Two production edits.

**(a) `QuickFiler/Controllers/FilerQueue.cs`** — replace the guard-based handshake with a
lock-protected outstanding-work counter plus a drain signal, and add a per-item processor seam:

- Private `readonly object` monitor, `private int _outstanding;`, and
  `private TaskCompletionSource<bool> _drained;` (null when idle).
  `TaskCompletionSource<bool>` and `TaskCreationOptions.RunContinuationsAsynchronously` both exist on
  net481; the parameterless non-generic `TaskCompletionSource` does **not** and must not be used.
- `Enqueue(FilerQueueItem item)`: under the monitor, `_outstanding++`, `Queue.Add(item)`, and decide
  whether a worker must be started from a `_consumerRunning` flag rather than from a one-shot guard.
  `Queue.Add` on an unbounded `BlockingCollection` never blocks, so holding the monitor across it is
  safe.
- `Enqueue(EmailFiler filer, IList<MailItemHelper> helpers)` delegates to the item overload
  (`Enqueue(new FilerQueueItem(filer, helpers))`). The `FilerQueueItem` constructor is still evaluated
  inside this overload's frame, so the `ArgumentNullException` propagation that
  `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs:362-364` depends on is unchanged.
- Worker loop: take the item under the monitor and clear `_consumerRunning` in the *same* critical
  section when `TryTake` fails, closing the §A.4.1 orphan window. In a `finally` after each item,
  decrement under the monitor and, on reaching zero, complete and clear `_drained`.
- `public Task WhenDrainedAsync()`: under the monitor, return `Task.CompletedTask` when
  `_outstanding == 0`, otherwise the lazily created `_drained.Task`. Idempotent and safe to await
  concurrently or repeatedly.
- `internal Func<FilerQueueItem, Task> ItemProcessor { get; set; } = item =>
  item.Filer.SortAsync(item.Helpers);` — the injectable seam (§E.2). The existing `catch` and its
  `item.Helpers.First()` diagnostic (`FilerQueue.cs:54-61`) stay wrapped around the seam call, so error
  behaviour is unchanged. A static-lambda initializer is legal here because it does not reference
  `this`, so the CS0236 workaround documented at
  `QuickFiler/Controllers/QfcFormController.Actions.cs:224-228` is not needed.
- `Consumer` (`:42`) is **retained** and still assigned, so
  `QuickFiler.Test/Controllers/FilerQueueTests.cs:77-87` and any out-of-assembly reader keep working.

**(b) `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`** —

- Add `_parent` to the early-return guard at `:219` (the method does not dereference `_parent` today,
  and `Cleanup()` sets `_parent = null` at `QfcFormController.SetupDisposal.cs:224`).
- Insert `await _parent.FilerQueue.WhenDrainedAsync();` between `:225` and `:228`.
- Delete the two now-subsumed `await _parent.FilerQueue.Consumer;` statements at `:167` and `:193`.
  After the fix both are strictly redundant (each is preceded by an await of the same
  `BackGroundMoveAsync` task, which now contains the barrier), and leaving them keeps a second, racy
  quiesce concept live in the batch-move path — which is precisely what the issue asks to eliminate.
  Both deletions are in the same file, so blast radius is unchanged.

**What could break.**

- *Behaviour change, main batch branch.* `await moveTask` at `:175` now waits for the filing work.
  Previously that branch never waited. The wait happens **after** `LoadUiFromQueue` (`:161`) and
  `IterateQueueAsync` (`:162`), so the next group is already on screen; `ButtonOK_Click` is
  `async void` (`:96-108`) and yields the UI thread, so no message-loop block is introduced. Nothing in
  the repo gates on `ActionOkAsync` completing.
- *Deadlock risk: low.* The filing path does not require the UI thread. `ConsumeAsync` runs its loop
  inside `Task.Run` where `SynchronizationContext.Current` is null, and `ProcessMailHelperAsync` is
  awaited with `ConfigureAwait(false)` (`EmailFiler.cs:148`, `:169-170`). The COM work is itself
  wrapped in `Task.Run` (`EmailFiler.cs:324`, `:301`, `:251`). The barrier at `:226` is awaited before
  the two `UiThread.Dispatcher.InvokeAsync` calls, not while holding the dispatcher.
- *Metrics ordering preserved.* The barrier is inserted before `WriteMetrics`, and the existing
  `WriteMetrics`-then-`CleanupBackground` order (§C.2) is untouched.
- *Unit-testable:* yes — see §E.

**Why this is remedy (ii) as well as remedy (i).** The issue permits either. This shape satisfies both
without documentation: after the change there is no code path from a completed batch move to
`WriteMetrics` or `CleanupBackground` that does not pass through the barrier, so a future edit to
either step cannot observe a partially populated stack. The constraint is enforced by control flow,
not by a comment.

### D.2 Rejected alternatives (brief)

- **Rejected: `await _parent.FilerQueue.Consumer;` inserted at line 226.** This is the smallest
  possible diff and needs no `FilerQueue` change, and it is what the terminal branch already does at
  `:193`. Rejected because `Consumer` is unsound on three counts (§A.4): a batch item can be orphaned
  with `Consumer` already completed, the property is a non-volatile assignment made after the worker
  starts, and a second worker can overwrite the reference to a still-running first worker. It would
  install a barrier that is *usually* right, which is a worse outcome than the present state because it
  would read as an expressed guarantee. It is also not deterministically unit-testable: with no
  per-item seam, driving `Consumer` requires the real `EmailFiler.SortAsync(IList<MailItemHelper>)`,
  which is non-virtual (`EmailFiler.cs:128`) and immediately casts to a COM `Folder`
  (`EmailFiler.cs:133`).
- **Rejected: a fail-fast guard/assert in `CleanupBackground` or `WriteMetrics` ("the stack must be
  populated").** Rejected because §C established that neither step reads the stack, so the guard would
  assert a property nothing depends on, and because a throwing guard on a benign latent condition
  converts a non-bug into a production crash on the UI thread. It also cannot be stated soundly: a
  failed COM move legitimately pushes nothing (`EmailFiler.cs:175`), so there is no correct expected
  count to assert against.
- **Rejected: make `Enqueue` return a `Task` per item and thread it back through `MoveMailAsync` →
  `MoveEmailsAsync` → `BackGroundMoveAsync` with `Task.WhenAll`.** This is the most explicit expression
  of the dependency and is a clean API. Rejected on blast radius: it changes the signature of
  `MoveMailAsync`, which is declared on an interface and called from
  `QfcCollectionController.TryMoveEmailByGroupAsync` (`:2316`), and would touch at least four
  production files plus interface declarations, against the two-file alternative that delivers the same
  guarantee.

---

## E. Testability

### E.1 Test project and existing coverage

- Test project: **`QuickFiler.Test`**, at `QuickFiler.Test/QuickFiler.Test.csproj`
  (`<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`, `:18`). It uses explicit
  `<Compile Include="..." />` entries (e.g. `:113` for `Controllers\FilerQueueTests.cs`, `:147-148` for
  the two form-controller test files), so any **new** test file requires a csproj edit.
- `InternalsVisibleTo("QuickFiler.Test")` is present twice —
  `QuickFiler/Properties/AssemblyInfo.cs:5` and `QuickFiler/Controllers/QfcHomeController.cs:15` — so
  `internal` seams on `FilerQueue` are directly reachable from tests, as is
  `InternalsVisibleTo("DynamicProxyGenAssembly2")` (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`)
  for Moq proxies of internal types.
- Existing `MoveMailAsync` tests: **`QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:150-235`**
  (three tests: null-helper early return, missing-OneDrive early return, and the enqueue happy path).
  A fourth family lives in `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs:362-364`.
- Existing `FilerQueue` tests: `QuickFiler.Test/Controllers/FilerQueueTests.cs`, 89 lines. Its class
  comment explicitly records that the `Enqueue`/`ConsumeAsync` path is **not** exercised because it
  dispatches to `EmailFiler.SortAsync` on a background task (`:12-19`). That exclusion is exactly what
  the proposed `ItemProcessor` seam removes, and the comment must be updated by the fix.
- Existing `BackGroundMoveAsync` test: `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:444-455`.
  It is **vacuous** — the controller is constructed without `Init()`, so `_groups` is null and the
  method returns at `QfcFormController.EventHandlers.cs:219-222` before reaching any behaviour.
  `MoveAndIterate_ShouldMoveAndIterate` (`:431-442`) is vacuous for the same reason via the guard at
  `EventHandlers.cs:149-152`. Neither will be affected by the change.

### E.2 The seam, matching the established repo idiom

The prompt's reference idiom is `MoveFailureNotifier` at
`QuickFiler/Controllers/QfcItemController.MailActions.cs:30-31` — a settable property carrying a
production default, with an XML comment naming its sibling precedents. The same idiom recurs at:

- `QuickFiler/Controllers/QfcFormController.Actions.cs:217`
  (`internal Func<Func<Task>, Task> UndoConsumerStarter { get; set; } = body => Task.Run(body);`) —
  a **start seam** that converts a background task into an inline one for tests;
- `QuickFiler/Controllers/QfcFormController.Actions.cs:229-233` (`UndoItemProcessor`) — a **per-item
  processor seam** that replaces the COM/dispatcher-bound body wholesale;
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:28-34` (`MetricsFileWriter`) — an I/O seam;
- `QuickFiler/Controllers/QfcFormController.Actions.cs:210` and
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs:19` — `internal TimeProvider TimeProvider
  { get; set; } = TimeProvider.System;`.

The `UndoItemProcessor` precedent is a direct structural match for the queue: same "producer/consumer
over a `BlockingCollection` whose per-item body is Outlook-bound" problem, solved with one settable
`Func<T, Task>`. Proposed seam, in that idiom:

```csharp
/// <summary>
/// Issue #633. Per-item seam for the queue worker. Defaults to the filer's SortAsync so production
/// behaviour is unchanged; tests assign a fake so no live Outlook COM call is made, which
/// `.claude/rules/general-unit-test.md` UT4 prohibits. Mirrors
/// QfcFormController.UndoItemProcessor and QfcItemController.MoveFailureNotifier.
/// </summary>
internal Func<FilerQueueItem, Task> ItemProcessor { get; set; } =
    item => item.Filer.SortAsync(item.Helpers);
```

No `init`, no `record`, no `record struct` — net481 has no `IsExternalInit`, as the repository itself
documents at `QuickFiler/Interfaces/IQfcDatamodel.cs:46-47` and
`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:14-15`.

### E.3 How the COM-bound collaborators are faked today

- `_globals` (`IApplicationGlobals`) → `Mock<IApplicationGlobals>` with `FS.SpecialFolders` seeded from
  a real `ConcurrentDictionary` and `Ol.ArchiveRootPath` stubbed:
  `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:334-355`
  (`InjectFilingCollaborators`), and inline at `SeamFactoryTests.cs:199-208`.
- `_homeController` (`IFilerHomeController`) → `Mock<IFilerHomeController>` whose `FilerQueue` getter
  returns a **real** `FilerQueue` instance: `TestSupport.cs:349-350`, `SeamFactoryTests.cs:219-220`.
- `_groups` (`IQfcCollectionController`) → `Mock<IQfcCollectionController>`, e.g.
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`.
- `_parent` (`IQfcHomeController`) → `Mock<IQfcHomeController>`:
  `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:105`.
- Private fields are injected by reflection helpers: `TestSupport.SetField`, and
  `SetPrivateField`/`GetPrivateField` in `QfcFormControllerSeamTests.cs:43-47`.

**Can `FilerQueue` be mocked as-is?** No, and it does not need to be. `IFilerHomeController.FilerQueue`
is typed as the concrete class (`QuickFiler/Interfaces/IFilerHomeController.cs:33`), and every member
of `FilerQueue` is non-virtual (`FilerQueue.cs:22`, `:31`, `:42`, `:44`), so Moq cannot intercept it.
The established workaround is to hand the mock a **real** `FilerQueue` and control its behaviour from
the inside — today by reflecting into the private `guard` field to pre-trip it
(`SeamFactoryTests.cs:212-218`), which the `ItemProcessor` seam replaces with a supported mechanism.

**Therefore no interface extraction is required.** If one were later wanted, the minimum member set
would be: `void Enqueue(EmailFiler, IList<MailItemHelper>)`, `void Enqueue(FilerQueueItem)`, and
`Task WhenDrainedAsync()`. Extracting it now would additionally require changing
`IFilerHomeController.FilerQueue`'s declared type, `QfcHomeController.cs:397`,
`EfcHomeController.cs:421`, and every test that hands back a real `FilerQueue` — a strictly larger
diff for no test capability that the seam does not already provide. Not recommended.

### E.4 Deterministic-scheduler / fake-clock precedent

Yes, `FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`) is already in use in this test project:
imported at `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:11`, subclassed as
`CountingTimeProvider` at `:359-368`, and driven with `clock.Advance(...)` at `:421`, `:445`, `:457`,
`:485`. The dispatcher side has its own fixture:
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` owns every mutation of the
process-wide `UtilitiesCS.UiThread._dispatcher` static (`:29-141`), offering
`EnsureDispatcher()` (`:99-115`, installs a **non-pumping parked** dispatcher) and
`BeginTransactionAsync()` + `Install(...)` (`:122-126`, `:242-254`). A **pumping** STA dispatcher is
available from `QfcItemControllerTestSupport.StartRunningDispatcher()` /
`ShutdownDispatcher(...)` (`QfcItemController.TestSupport.cs:251-280`), and the combination is already
used at `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:128`.

This matters because `BackGroundMoveAsync` reaches `UiThread.Dispatcher.InvokeAsync`
(`EventHandlers.cs:228`, `:233`) and `UiThread.Dispatcher`'s getter returns the raw static without
initializing (`UtilitiesCS/Threading/UiThread.cs:135-140`), so it is `null` in a bare test process and
never completes against the parked dispatcher.

### E.5 Proposed test strategy (no test code written)

**Tier 1 — `FilerQueue`, fully deterministic, no dispatcher.** Extend
`QuickFiler.Test/Controllers/FilerQueueTests.cs` (89 lines today; ample room under the 500-line limit)
using `ItemProcessor` plus a `TaskCompletionSource<bool>` gate — no `Task.Delay`, no `Thread.Sleep`, no
polling:

1. `WhenDrainedAsync` on a fresh queue returns an already-completed task.
2. With a gated processor, `WhenDrainedAsync()` obtained after one `Enqueue` is not complete; after
   the gate is released, awaiting it completes and the processor ran exactly once.
3. Two enqueues, one gate each: the drain task completes only after **both** processors have run
   (this is the exact scenario the spec's Test Strategy names at `spec.md:125-126`).
4. Awaiting `WhenDrainedAsync()` twice, and two concurrent waiters, both complete — the property is
   idempotent.
5. Enqueue-after-drain starts a fresh worker and the second batch is processed (regression for the
   §A.4.1 orphan window — arrange by releasing the first gate, awaiting the drain, then enqueueing
   again and awaiting the new drain).
6. A processor that throws is caught, the item still decrements the counter, and the drain completes
   (regression guarding against a leak that would hang the production UI).

**Tier 2 — `QfcFormController.BackGroundMoveAsync` ordering.** New file, e.g.
`QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` (`QfcFormControllerSeamTests.cs` is
496 lines and `QfcFormControllerTests.cs` is 827, so neither can absorb it). Arrange: mocks per §E.3,
`_groups` injected by reflection with `MoveEmailsAsync` returning `Task.CompletedTask`, `_parent`
returning a real `FilerQueue` with a gated `ItemProcessor` and one pre-enqueued item, and a pumping
dispatcher installed through `UiThreadDispatcherFixture.BeginTransactionAsync()` +
`Install(QfcItemControllerTestSupport.StartRunningDispatcher())`.

- **Negative (deterministic without any pumping at all):** `Task t = controller.BackGroundMoveAsync();`
  — because `MoveEmailsAsync` completes synchronously, the async method runs straight through to the
  barrier before returning to the caller, so on return `t.IsCompleted` is `false` and
  `groups.Verify(g => g.CleanupBackground(), Times.Never)` holds. No race, no wait.
- **Positive:** release the gate, `await t`, then assert `CleanupBackground` was invoked once and the
  metrics writer was invoked once. Completion is observed by awaiting, not by polling.
- **Guard:** with `_groups` null the method still returns early and touches neither the queue nor the
  dispatcher (pins the existing vacuous tests' behaviour).

All of the above satisfy MSTest + Moq + FluentAssertions (`CLAUDE.md` CUT1/CUT2), UT1 determinism, and
UT4 (no external process, no temp files).

---

## F. Blast radius — minimal file list

**A fix can be confined to two production files.**

### Production (2)

1. `QuickFiler/Controllers/FilerQueue.cs` — outstanding-work counter, `WhenDrainedAsync()`, repaired
   consumer start/stop handshake, `ItemProcessor` seam. File is 83 lines today; the change keeps it
   well under the 500-line limit and adds no new production file (which matters because
   `QuickFiler/QuickFiler.csproj` uses explicit `<Compile Include>` entries, e.g. `:304`).
2. `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` — barrier inserted at line 226,
   `_parent` added to the guard at line 219, the two subsumed `await ...Consumer;` statements at lines
   167 and 193 removed.

Not touched: `QfcItemController.MailActions.cs` (the enqueue call site needs no change),
`QfcCollectionController.cs`, `EmailFiler.cs`, `IFilerHomeController.cs`, `IQfcCollectionController.cs`,
`QfcHomeController.cs`, `AppAutoFileObjects.cs`.

### Test (3)

1. `QuickFiler.Test/Controllers/FilerQueueTests.cs` — extended with the Tier-1 cases; the class comment
   at `:12-19` must be corrected, since the `Enqueue`/`ConsumeAsync` exclusion it records no longer
   applies.
2. `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` — **new**, Tier-2 ordering tests.
3. `QuickFiler.Test/QuickFiler.Test.csproj` — one `<Compile Include>` entry for the new test file.

---

## Numeric Derivation Evidence

Two counts in this document are load-bearing for the blast-radius list and are derived below. No
numeric acceptance criterion is proposed for `spec.md` beyond these.

### Claim 1 — `FilerQueue.Consumer` has exactly two production read sites

- **Complete Family:** every read of the `Consumer` member of `QuickFiler.Controllers.FilerQueue`
  (property declared once at `QuickFiler/Controllers/FilerQueue.cs:42`; the member has no overloads and
  no other accessor), in first-party production code across the whole repository.
- **Exhaustive Search Scope:** all `*.cs` files in the repository working tree, both production and
  test, with production and test results separated afterwards. Both writes (`:27`, `:36`) and reads are
  surfaced, so the search cannot miss a read by matching only a call syntax.
- **Inclusion Rules:** a source expression that evaluates `FilerQueue.Consumer` (read), in a file under
  a production project directory.
- **Exclusion Rules:** the two assignment sites inside `Enqueue` (writes, not reads); files under
  `*.Test/`; files under `docs/`; the identically named but structurally distinct
  `TaskVisualization.FlagChangeTrainingQueue.Consumer`
  (`TaskVisualization/FlagChangeTrainingQueue.cs:34`), which is a different type.
- **Primary Search Strategy or Query Expression:** symbol-name sweep — `Grep pattern="FilerQueue"`,
  `glob` unset (all files), which returns every declaration, every property, every construction and
  every member access reached through a `FilerQueue`-typed expression.
- **Primary Member Set:** `{ QuickFiler/Controllers/QfcFormController.EventHandlers.cs:167,
  QuickFiler/Controllers/QfcFormController.EventHandlers.cs:193 }`.
- **Primary Count:** 2.
- **Cross-check Search Strategy or Query Expression:** member-access sweep independent of the type
  name — `Grep pattern="\.Consumer\b" glob="*.cs"`, which finds `Consumer` reads regardless of whether
  the receiver expression mentions `FilerQueue`, then filters by receiver type.
- **Cross-check Member Set:** raw hits were
  `{ QuickFiler.Test/Controllers/FilerQueueTests.cs:83, QuickFiler.Test/Controllers/FilerQueueTests.cs:85,
  QuickFiler/Controllers/QfcFormController.EventHandlers.cs:167,
  QuickFiler/Controllers/QfcFormController.EventHandlers.cs:193,
  TaskVisualization.Test/FlagChangeTrainingQueueTests.cs:46 }`. Applying the exclusion rules removes the
  two `QuickFiler.Test` hits (test project) and the `TaskVisualization.Test` hit (different type,
  `TaskVisualization/FlagChangeTrainingQueue.cs:34`), leaving
  `{ QuickFiler/Controllers/QfcFormController.EventHandlers.cs:167,
  QuickFiler/Controllers/QfcFormController.EventHandlers.cs:193 }`.
- **Cross-check Count:** 2.
- **Member-set Comparison:** the normalized primary and cross-check member sets are identical —
  both are exactly `{QfcFormController.EventHandlers.cs:167, QfcFormController.EventHandlers.cs:193}`.
  The two strategies are distinct (type-name sweep vs. member-name sweep) and neither is a subset
  query of the other. Counts agree at 2.

### Claim 2 — `FilerQueue.Enqueue` has exactly one production call site

- **Complete Family:** every invocation of either `Enqueue` overload on
  `QuickFiler.Controllers.FilerQueue` — `Enqueue(FilerQueueItem)`
  (`QuickFiler/Controllers/FilerQueue.cs:22`) and `Enqueue(EmailFiler, IList<MailItemHelper>)`
  (`QuickFiler/Controllers/FilerQueue.cs:31`) — in first-party production code. Both overloads are in
  scope; a search matching only one signature would be non-exhaustive.
- **Exhaustive Search Scope:** all `*.cs` files in the repository working tree for the primary; all
  `*.cs` files under `QuickFiler/` for the cross-check (the only production project that references
  the type, since `EfcHomeController.FilerQueue` throws at
  `QuickFiler/Controllers/EfcHomeController.cs:421` and no other project names `FilerQueue`).
- **Inclusion Rules:** a call expression whose receiver is a `FilerQueue` instance and whose method is
  either `Enqueue` overload, in a production file.
- **Exclusion Rules:** the two declaration sites; files under `*.Test/`; files under `docs/`;
  `Enqueue` calls on unrelated queues — `ViewerQueueCore._queue`
  (`QuickFiler/Helper Classes/ViewerQueueCore.cs:59`, `:146`), `BreadcrumbOutboundQueue._pending`
  (`QuickFiler/Controllers/BreadcrumbOutboundQueue.cs:50`), `QfcLauncher.MasterQueue`
  (`QuickFiler/Legacy/QfcLauncher.cs:28`), and `Globals.Ol.EmailMoveWriter`
  (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:460`).
- **Primary Search Strategy or Query Expression:** type-name sweep — `Grep pattern="FilerQueue"` over
  all files, then select lines whose expression is a call rather than a declaration or a property
  access. This surfaces both overloads because it keys on the receiver's type name, not on a signature.
- **Primary Member Set:** `{ QuickFiler/Controllers/QfcItemController.MailActions.cs:136 }`.
- **Primary Count:** 1.
- **Cross-check Search Strategy or Query Expression:** call-syntax sweep independent of the type
  name — `Grep pattern="Enqueue\(" glob="QuickFiler/**/*.cs"`, which matches every `Enqueue` invocation
  and both declarations in the project regardless of receiver, then filters by receiver identity.
- **Cross-check Member Set:** raw hits were
  `{ QuickFiler/Helper Classes/ViewerQueueCore.cs:59, QuickFiler/Helper Classes/ViewerQueueCore.cs:146,
  QuickFiler/Controllers/BreadcrumbOutboundQueue.cs:50, QuickFiler/Legacy/QfcLauncher.cs:28,
  QuickFiler/Controllers/FilerQueue.cs:22, QuickFiler/Controllers/FilerQueue.cs:31,
  QuickFiler/Controllers/QfcItemController.MailActions.cs:136 }`. Applying the exclusion rules removes
  the four unrelated-queue calls and the two declarations, leaving
  `{ QuickFiler/Controllers/QfcItemController.MailActions.cs:136 }`.
- **Cross-check Count:** 1.
- **Member-set Comparison:** the normalized primary and cross-check member sets are identical — both
  are exactly `{QfcItemController.MailActions.cs:136}`. The strategies are distinct (receiver-type
  sweep vs. call-syntax sweep) and cover both overloads. Counts agree at 1. This is what makes the
  two-file production blast radius sound: repairing the enqueue handshake inside `FilerQueue` cannot
  require an edit at any other producer.

---

## Items I could not determine

- **Whether the §A.4.1 orphan window has ever fired in production.** No log evidence was located; the
  window is established by reading the interleaving of `FilerQueue.cs:24-25`/`:33-34` against `:48`
  and `:63`, not by an observed incident. It is presented as a code-level race, not an incident report.
- **Whether `FilerQueue` is referenced by any consumer outside this repository.** The type is `public`
  (`FilerQueue.cs:14`) and `QuickFiler` is a class library, so an out-of-tree consumer cannot be ruled
  out by repository search. The recommendation is additive on the public surface (`Consumer` is
  retained, `WhenDrainedAsync` is new, `ItemProcessor` is `internal`) precisely so this uncertainty
  carries no compatibility cost.
- **Actual production duration of the added wait at `EventHandlers.cs:175`.** No timing telemetry for
  `EmailFiler.SortAsync` was located in the repository, so the added latency is characterized
  qualitatively (bounded by the batch's filing time, incurred after the next group is already
  displayed) rather than numerically.
