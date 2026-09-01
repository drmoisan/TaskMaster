# 2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move (Spec)

- **Issue:** #633
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T20-15
- **Status:** Approved
- **Version:** 1.0
- **Work Mode:** full-bug — this document is the **sole authoritative acceptance-criteria source**.
  No `user-story.md` exists or may be created for this issue.

## Context

The batch-move path treats the undo stack as populated by the time the move completes, but the push is
performed asynchronously on a queue worker and may not have happened yet. `MoveMailAsync` only
*enqueues* the filer (QuickFiler/Controllers/QfcItemController.MailActions.cs:136) and then returns
`await Task.CompletedTask` (:137). The push onto the global undo stack happens later, on the queue's
worker. So when `BackGroundMoveAsync` proceeds to `WriteMetrics`
(`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228-231`) and then `CleanupBackground()`
(`:233`), the undo entries for that batch may not yet exist.

> **Citation correction.** `issue.md` and the previous draft of this Context cited the enqueue and the
> completed-task return as QfcItemController.MailActions.cs:111 and :112. Those citations are
> stale: they predate intervening edits to that file (the issue was captured 2026-08-26). The current
> line numbers, verified against the working tree on 2026-08-31, are **136** and **137**, and
> `MoveMailAsync` now spans lines 105-158. All other line citations in this document were re-verified
> against the working tree on the same date.

This does not break undo in the observed configuration — the entries land eventually and are
serialized — but the handoff is unsynchronized, and nothing in the code expresses the ordering it
relies on.

Supporting analysis, including the full call path and the blast-radius derivation, is recorded in the
research record at
docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/research/2026-08-31T19-45-undo-handoff-ordering-research.md.
That record establishes two facts this specification depends on:

1. **The defect is latent, not live.** Neither `CleanupBackground()`
   (QuickFiler/Controllers/QfcCollectionController.cs:867-884) nor `WriteMetricsAsync`
   (QuickFiler/Controllers/QfcHomeController.Metrics.cs:107-180) reads the undo stack, and
   `QfcItemController.Cleanup()` does not release any object the queued filing work still needs — the
   `MailItemHelper` list is captured by value into the queue item and remains reachable. The cost is
   exactly the absent ordering constraint, not an observed failure.
2. **A batch is fully enqueued by the time the move returns.** `MoveEmailsAsync` awaits each group's
   `MoveMailAsync` sequentially, and each of those enqueues synchronously. Therefore an
   outstanding-work count observed immediately after `await _groups.MoveEmailsAsync(_movedItems)` is an
   exact upper bound on the batch, which is what makes a counted barrier correct rather than heuristic.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Framework: .NET Framework 4.8.1, VSTO Outlook add-in
- Command/flags used: not reproducible from a command line; requires a live Outlook session and a batch move
- Data source or fixture: any QuickFiler batch move of two or more emails

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Latent. The entries land eventually and are serialized, so undo works in the observed configuration.
The severity comes from the absence of any expressed ordering constraint: a future caller that reads
the stack immediately after a batch move would see an incomplete stack with no diagnostic.

## Repro & Evidence

Steps to Reproduce:
1. Select two or more emails in a QuickFiler session and assign destination folders.
2. Confirm the move, so `BackGroundMoveAsync` runs.
3. Observe the contents of the global undo stack at the moment `CleanupBackground()` is reached.

Expected:
Either the batch-move completion awaits the undo pushes for that batch, or the ordering dependency is
made explicit so that a future change to `WriteMetrics` or `CleanupBackground` cannot start depending
on entries that are not yet present.

Actual:
`CleanupBackground()` may run while some or all of the batch's undo entries are still queued. Nothing
observes the gap today, so the defect is latent rather than active.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: no captured log; identified by triage during issue #468, recorded at
  docs/features/active/qfc-collection-controller-defects-468/spec.md:1040-1049
  ("Deferred observation — unsynchronized undo handoff").

Static evidence for the race in the queue handshake (no runtime log exists; the window is established
by reading the statement interleaving, not by an observed incident):

| Statement | Location |
|---|---|
| Producer adds the item | `QuickFiler/Controllers/FilerQueue.cs:24` and `:33` |
| Producer *then* reads the one-shot guard | `QuickFiler/Controllers/FilerQueue.cs:25` and `:34` |
| Worker exits its loop when `TryTake` returns false | `QuickFiler/Controllers/FilerQueue.cs:48` |
| Worker *then* installs a fresh guard | `QuickFiler/Controllers/FilerQueue.cs:63` |

A producer whose `Add` lands between the worker's loop exit and its guard reinstall reads the
already-tripped guard, starts no worker, and leaves its item in the queue with `Consumer` already
completed.

## Scope & Non-Goals

### In scope — production (2 files)

- `QuickFiler/Controllers/FilerQueue.cs` — add a counted, per-batch, awaitable quiesce
  (`WhenDrainedAsync()`), repair the producer/consumer start-stop handshake, and add an injectable
  per-item processor seam.
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` — await the quiesce inside
  `BackGroundMoveAsync` before the metrics and cleanup dispatches, add `_parent` to that method's
  early-return guard, and delete the two now-subsumed `await _parent.FilerQueue.Consumer;` statements.

### In scope — tests

- `QuickFiler.Test/Controllers/FilerQueueTests.cs` — extend with the queue-level cases; correct the
  class comment at `:12-19`, which records that the `Enqueue`/`ConsumeAsync` path is deliberately not
  exercised. That exclusion no longer holds once the processor seam exists.
- `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` — new file for the ordering tests.
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` — reconciliation only. The test
  `MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues` reflects into a private
  instance field literally named `guard` at `:213-218` and then sets `ThreadSafeSingleShotGuard._state`
  to suppress the consumer. If the handshake repair removes or renames that field, the reflection
  returns `null` and the test throws. See "Deviation from the research record" below.
- `QuickFiler.Test/QuickFiler.Test.csproj` — one `<Compile Include>` entry for the new test file. The
  project uses explicit compile items (existing entries at `:113`, `:147-148`), so a new file is not
  picked up automatically.

### Out of scope / non-goals

The paths in this subsection are written as bare prose, not as code spans, deliberately: a downstream
tool derives the change footprint from backticked repository paths, so backticking an out-of-scope
path would falsely widen the recorded blast radius. Do not reformat them.

- **No change to QuickFiler/Controllers/QfcItemController.MailActions.cs.** The single production
  enqueue call site at :136 needs no edit; the fix is entirely behind the queue's own API.
- **No change to UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs.** The undo push at
  :185-189 and its per-helper granularity are correct as they stand.
- **No change to QuickFiler/Controllers/QfcCollectionController.cs.** Neither `MoveEmailsAsync` nor
  `CleanupBackground` nor `GetMoveDiagnostics` is modified.
- **No interface extraction.** QuickFiler/Interfaces/IFilerHomeController.cs:33 keeps its concrete
  `FilerQueue` type; QuickFiler/Controllers/QfcHomeController.cs:397 and
  QuickFiler/Controllers/EfcHomeController.cs:421 are untouched.
- **No behavioural change to what `WriteMetrics` writes.** The barrier is inserted before the metrics
  dispatch; the metrics payload, its inputs, and the existing metrics-before-cleanup statement order
  are all preserved.
- **No opportunistic refactor of surrounding QuickFiler code.** No renaming, no reformatting of
  untouched members, no cleanup of adjacent commented-out code.
- **TaskVisualization/FlagChangeTrainingQueue.cs is not touched.** It has the same structural shape
  (`BlockingCollection` plus `ThreadSafeSingleShotGuard` plus a `Consumer` task) and the same latent
  handshake window, but it is a different type with a different consumer and is not part of #633.
  Whether it warrants the same repair is a separate question, to be raised as its own issue if desired.
- **No new production file.** QuickFiler/QuickFiler.csproj also uses explicit compile items; keeping
  the change inside the existing `FilerQueue.cs` avoids a project-file edit on the production side.

### Why the handshake repair is in scope

CLAUDE.md's Bugfix Workflow requires the minimal targeted fix and warns against opportunistic
refactors. The `Enqueue`/`ConsumeAsync` handshake repair is **a prerequisite for a sound barrier, not
an opportunistic refactor**, for two reasons:

1. **A barrier over the current handshake would be only usually correct.** The orphaned-item window
   documented above lets an item sit in the queue with no worker running. A drain signal computed over
   that queue would report "drained" while an item is stranded, or would never complete at all. Adding
   a barrier that reads as a guarantee but is not one is a worse outcome than the present state, in
   which the ordering constraint is at least honestly unexpressed. The issue asks for an ordering
   constraint that a future change cannot silently violate; only a repaired handshake delivers that.
2. **It does not widen the blast radius.** The repair is confined to `QuickFiler/Controllers/FilerQueue.cs`,
   a file already in scope for the barrier itself. The research record's derivation (reproduced and
   independently re-verified for this specification) shows the queue has exactly one production
   `Enqueue` call site and exactly two production `Consumer` read sites, so no additional production
   file is drawn in by changing the queue's internals.

## Root Cause Analysis

The defect has two independent parts. Neither alone would produce the reported condition.

### Part 1 — the caller holds no handle on the work it started

`MoveMailAsync` (QuickFiler/Controllers/QfcItemController.MailActions.cs:105-158) hands the filer and
its helper list to the queue at :136 and then returns `await Task.CompletedTask` at :137. The returned
task is already complete when the caller receives it, so awaiting it conveys nothing about the filing
work — including the undo push, which happens later on the queue worker at
UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:185-189. This is structurally the same
hazard as an `async void` boundary: the operation looks synchronous to its awaiter while the real work
is still pending. The `issue.md` "Suspected Cause" states this correctly.

### Part 2 — no barrier exists between batch-move completion and the downstream steps

`BackGroundMoveAsync` (`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:215-234`) awaits
`_groups.MoveEmailsAsync(_movedItems)` at `:225` and then proceeds directly to the `WriteMetrics`
dispatch at `:228-231` and the `CleanupBackground()` dispatch at `:233`. Because of Part 1, the await
at `:225` completes as soon as the last item has been *enqueued*, not when it has been *filed*. There
is no statement between `:225` and `:228` that observes the queue at all.

Two waits on the queue do exist in the same file, but neither closes the gap:
- `:167` is on the catch path of `MoveAndIterate`, reached only when `LoadUiFromQueue` or
  `IterateQueueAsync` throws.
- `:193` is on the terminal branch, and it runs *after* `BackGroundMoveAsync` has already returned —
  that is, after `CleanupBackground` has already been dispatched. The wait exists but is placed one
  level up and one step too late.

The main batch branch (`:154-177`) never waits on the queue at all.

### Why a naive barrier is unsound

The obvious minimal fix — inserting `await _parent.FilerQueue.Consumer;` after `:225` — is rejected.
`Consumer` (`QuickFiler/Controllers/FilerQueue.cs:42`) is not a lifetime task; it completes whenever a
worker observes a momentarily empty queue. Three properties of the current handshake make it unusable
as a quiesce primitive:

1. **Orphaned-item window.** Both `Enqueue` overloads perform `Queue.Add(...)` (`:24`, `:33`) *before*
   reading `guard` (`:25`, `:34`), while the worker exits its `while (Queue.TryTake(out var item))`
   loop (`:48`) *before* installing a fresh guard (`:63`). A producer interleaving between those two
   worker statements adds its item, reads the still-tripped guard, starts no worker, and the item is
   stranded with `Consumer` already completed.
2. **Stale-reference window.** `Consumer = ConsumeAsync()` starts the `Task.Run` inside `ConsumeAsync`
   before the returned task is stored in the non-volatile auto-property, so a concurrent reader can
   observe the previous, completed task.
3. **Worker overlap.** The guard is reset at `:63` *inside* the `Task.Run` body, so a new `Enqueue` can
   start a second worker while the first is still running; the `Consumer` assignment then overwrites
   the reference to the still-running first worker.

This is why the handshake repair is a precondition of the fix rather than an adjacent improvement.

## Proposed Fix

### Design summary (what changes where)

Add a counted, per-batch, awaitable quiesce to the queue and await it at the one place in the
batch-move path where the batch is known to be fully enqueued.

- `QuickFiler/Controllers/FilerQueue.cs` gains an outstanding-work counter that is incremented inside
  `Enqueue` and decremented after each item's processing completes, exposed as `Task WhenDrainedAsync()`.
  The one-shot-guard start gate is replaced by a start/stop decision taken under a single monitor, so
  the enqueue and the "is a worker running" decision are atomic with respect to the worker's loop exit.
  An `internal Func<FilerQueueItem, Task> ItemProcessor` seam replaces the hard-coded call to
  `item.Filer.SortAsync(item.Helpers)`.
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` awaits `WhenDrainedAsync()` between
  `:225` and `:228`, adds `_parent` to the guard at `:219`, and drops the two subsumed `Consumer`
  awaits at `:167` and `:193`.

After the change there is no control-flow path from a completed batch move to `WriteMetrics` or
`CleanupBackground` that does not pass through the barrier. The ordering constraint is enforced by
control flow rather than by a comment, which satisfies both remedies the issue offers.

### Boundaries and invariants to preserve

- **Metrics run before cleanup.** `WriteMetricsAsync` reads `_itemGroupsToMove` and each group's
  `ItemController.ItemHelper`, both of which `CleanupBackground()` resets. The existing statement order
  at `:228-233` already satisfies this and must not be reordered. The barrier is inserted *before*
  both, so the relative order of the two dispatches is unchanged.
- **`Consumer` remains on the public surface.** `FilerQueue` is a `public` class in a class library, so
  an out-of-tree consumer cannot be ruled out by repository search. `Consumer` keeps its type, its
  accessibility, and its `Task.CompletedTask` default; `WhenDrainedAsync()` is purely additive and
  `ItemProcessor` is `internal`. The change is additive on the public surface.
- **Enqueue-time argument validation stays in the caller's frame.** The
  `Enqueue(EmailFiler, IList<MailItemHelper>)` overload must continue to construct the `FilerQueueItem`
  within its own frame so that a null helper still surfaces as a synchronous `ArgumentNullException` to
  the caller. `QfcItemController.MoveMailAsync` wraps that into an `InvalidOperationException`, and a
  test depends on it.
- **Error behaviour inside the worker is unchanged.** The existing `catch` and its
  `item.Helpers.First()` diagnostic (`QuickFiler/Controllers/FilerQueue.cs:54-61`) stay wrapped around
  the seam call, so a failing item is still logged and the loop still continues.
- **The barrier is awaited off the UI thread.** It is placed before the two
  `UiThread.Dispatcher.InvokeAsync` calls, never while a dispatcher operation is in flight.

### Dependencies or blocked work

None. The change depends on no other issue and blocks none. It is independent of the #468 defect
family, which owns QuickFiler/Controllers/QfcCollectionController.cs and does not touch either
in-scope file.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

Production:
- `QuickFiler/Controllers/FilerQueue.cs`
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`

Tests and project files:
- `QuickFiler.Test/Controllers/FilerQueueTests.cs`
- `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` (new)
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`

#### Functions/classes/CLI commands impacted

Added to `FilerQueue` in `QuickFiler/Controllers/FilerQueue.cs`:

| Member | Accessibility | Purpose |
|---|---|---|
| `WhenDrainedAsync()` returning `Task` | `public` | Completes when the outstanding-work count reaches zero. Returns an already-completed task when the count is already zero. Idempotent, and safe to call and await repeatedly or concurrently. |
| `ItemProcessor` of type `Func<FilerQueueItem, Task>` | `internal` get/set | Per-item processing seam. Production default is `item => item.Filer.SortAsync(item.Helpers)`, so production behaviour is unchanged. |
| A private monitor object | `private readonly` | Serializes the counter, the queue add, and the worker start/stop decision. |
| A private outstanding-work counter (`int`) | `private` | Incremented in `Enqueue`, decremented in a `finally` after each item. |
| A private drain signal (`TaskCompletionSource<bool>`, null when idle) | `private` | Lazily created by `WhenDrainedAsync()` when work is outstanding; completed and cleared when the counter reaches zero. |
| A private "consumer running" flag | `private` | Replaces the `ThreadSafeSingleShotGuard` start gate. Set when a worker is started and cleared in the same critical section in which `TryTake` fails, which closes the orphaned-item window. |

Modified in `FilerQueue`: both `Enqueue` overloads and `ConsumeAsync`. The
`Enqueue(EmailFiler, IList<MailItemHelper>)` overload delegates to the item overload after constructing
the `FilerQueueItem` in its own frame. `Consumer` is retained and still assigned.

Modified in `QfcFormController` in `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`:
`BackGroundMoveAsync` (guard at `:219` plus the inserted await) and `MoveAndIterate` (deletion of the
`Consumer` awaits at `:167` and `:193`). No signature changes.

#### Data flow and validation changes

No data-flow change. The barrier observes a count; it neither reads nor writes the undo stack, the
helper lists, or the metrics inputs. `FilerQueueItem` construction and validation are unchanged.

The one behavioural change visible to a user is timing: on the main batch branch, `await moveTask` at
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:175` now waits for the batch's filing work,
which it previously never did. That wait occurs *after* `LoadUiFromQueue` (`:161`) and
`IterateQueueAsync` (`:162`), so the next group is already displayed, and `ButtonOK_Click` (`:96-108`)
is `async void` and has already yielded the UI thread, so no message-loop block is introduced.

#### Error handling and logging updates

- The per-item `try`/`catch` and its `logger.Error` diagnostic are preserved verbatim around the seam
  call. No new log statements are required.
- The counter decrement must be in a `finally`, so a throwing item still decrements. A leaked count
  would leave `WhenDrainedAsync()` permanently incomplete and hang the batch-move path, which is a
  strictly worse failure than the defect being fixed.
- The added `_parent` null check at `:219` is an early return, consistent with the existing guard
  clause. It does not throw and does not log; a null `_parent` means the controller has already been
  cleaned up and there is nothing to do.

#### Rollback/feature-flag considerations (if applicable)

**A feature flag is not warranted, and none will be added.** Three reasons:

1. The change has no user-visible configuration surface and no persisted state, so there is nothing to
   toggle at runtime that would not itself become a second code path to test.
2. The behavioural delta is a single added wait whose worst case is bounded by the batch's own filing
   time, which the code was already performing. A flag would mostly serve to keep the unsound path
   alive.
3. VSTO add-in deployment in this repository is per-build; rollback is a revert of the commit, which is
   clean because the change is additive on the public surface and confined to two production files.

Rollback procedure if the change must be withdrawn: revert the commit. No data migration, no
configuration cleanup, and no compatibility shim is required.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

`Task WhenDrainedAsync()` — no parameters.

| Condition at call time | Result |
|---|---|
| Outstanding count is zero | An already-completed `Task` |
| Outstanding count is greater than zero | A `Task` that completes when the count next reaches zero |
| Called repeatedly, or concurrently from several threads | Every returned task completes; no caller can starve another |
| An item's processing throws | The item still decrements; the drain still completes |

The task completes; it does not carry a result and does not fault. Item faults are logged inside the
worker, exactly as today, and are not propagated to a drain waiter — propagating them would convert a
logged filing failure into an unhandled exception on the batch-move path, which is a behavioural
regression the issue does not ask for.

`internal Func<FilerQueueItem, Task> ItemProcessor { get; set; }` — takes the dequeued item, returns a
task representing its processing. The production default preserves the current call. Tests assign a
fake, which is what makes the concurrency assertions deterministic: the real
`EmailFiler.SortAsync(IList<MailItemHelper>)` is non-virtual and casts to a COM `Folder`, so it cannot
be driven from a unit test.

#### Required configuration keys and defaults

None. No configuration key, `.config` entry, or settings-store value is added or read.

#### Backward-compatibility expectations

- `Consumer` retains its declaration, accessibility and default. The existing assertion that a fresh
  queue exposes a completed consumer (`QuickFiler.Test/Controllers/FilerQueueTests.cs:76-87`) must
  continue to pass without modification.
- Both `Enqueue` overloads retain their signatures and their exception behaviour.
- `WhenDrainedAsync()` is new; `ItemProcessor` is `internal` and therefore not part of the public
  surface. `QuickFiler/Properties/AssemblyInfo.cs:5` already grants `InternalsVisibleTo("QuickFiler.Test")`,
  so no new attribute is needed.
- The private field named `guard` is removed or renamed by the handshake repair. It is not part of any
  contract, but one test reflects into it by name; see the Test Strategy.

#### Performance constraints (latency/throughput/memory)

No numeric latency target is set, because no timing telemetry for `EmailFiler.SortAsync` exists in the
repository from which one could be derived. The constraint is stated structurally instead:

- The added wait is bounded by the time the batch's filing work already takes; it introduces no new
  work and no polling.
- It is incurred after the next group has been loaded and displayed, so it is not on the path between
  the user's click and the next visible frame.
- Memory: one counter, one monitor object, and at most one `TaskCompletionSource<bool>` per queue
  instance while work is outstanding. The `TaskCompletionSource` is cleared when the count reaches zero.
- The monitor is held across `Queue.Add` on an unbounded `BlockingCollection`, which never blocks, and
  is never held across an `await`.

## Assumptions, Constraints, Dependencies

### Assumptions (environment, data, access)

- QuickFiler is the only production consumer of `FilerQueue`; `EfcHomeController.FilerQueue` throws
  `NotImplementedException`, so the enterprise-filer path never touches the queue.
- The undo stack's own thread-safety is unchanged and adequate: `SloStack<T>.Push` reaches a
  `lock (this)`-protected `AddFirst`, so concurrent pushes interleave but do not corrupt.
- No out-of-tree assembly depends on the private internals of `FilerQueue`. The public surface is
  preserved regardless, so this assumption carries no compatibility cost.

### Constraints (budget, performance, compatibility)

- **.NET Framework 4.8.1 / VSTO.** `init` accessors, `record`, and `record struct` are unavailable —
  this repository has no `IsExternalInit` polyfill, so any of them fails with CS0518. Use ordinary
  properties and classes. `TaskCompletionSource<bool>` and
  `TaskCreationOptions.RunContinuationsAsynchronously` do exist on net481; the parameterless
  non-generic `TaskCompletionSource` does not and must not be used.
- **Tests: MSTest, Moq, FluentAssertions** per CLAUDE.md CUT1/CUT2.
- **Determinism — the single most important constraint on this fix.**
  `.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay`, and all real wall-clock waits
  in test code. Every concurrency assertion in this change must be driven deterministically — by
  `TaskCompletionSource` gates injected through the `ItemProcessor` seam — never by a timeout, a sleep,
  a spin, or a polling loop. A test that "waits a bit and then asserts" is not acceptable here even if
  it passes locally: it would make the barrier's correctness unfalsifiable, which is the whole point of
  the change.
- **Test layout.** Tests live under `QuickFiler.Test/`, mirroring the production structure.
  `QuickFiler.Test/QuickFiler.Test.csproj` uses explicit `<Compile Include>` entries, so a new test
  file requires a project entry or it will not compile into the assembly.
- **No temporary files in tests**, without exception.
- **File size.** No production or test file may exceed 500 lines.
  `QuickFiler/Controllers/FilerQueue.cs` is 84 lines and
  `QuickFiler.Test/Controllers/FilerQueueTests.cs` is 90; both have room. The Tier-2 ordering tests go
  in a new file because QfcFormControllerTests.cs (827 lines) and QfcFormControllerSeamTests.cs
  (496 lines) cannot absorb them.
- **Policy files are read-only for this change.** Do not edit anything under `.claude/rules/`,
  `.claude/skills/`, or CLAUDE.md.

### External dependencies (services, libraries, releases)

None added. `Microsoft.Extensions.Time.Testing` is already referenced by the test project and is
available if a fake clock is wanted, though the design deliberately requires no clock at all.

## Data / API / Config Impact

- **User-facing or API changes:** one added public method, `FilerQueue.WhenDrainedAsync()`. No removed
  or changed public member. No UI change; the only user-observable difference is that the batch-move
  task now completes after the batch has been filed rather than after it has been enqueued.
- **Data or migration considerations:** none. No persisted format, no schema, no stored settings.
- **Logging/telemetry updates:** none. The existing per-item error log is preserved unchanged, and the
  metrics payload written by `WriteMetrics` is unchanged.
- **Compatibility notes:** no CLI flags, no config schema, no versioning impact. The change is additive
  on the assembly's public surface.

## Test Strategy

Tests use MSTest with Moq and FluentAssertions, per CLAUDE.md CUT1/CUT2. All concurrency is driven by
`TaskCompletionSource` gates through the `ItemProcessor` seam. No `Thread.Sleep`, no `Task.Delay`, no
polling, no timeout-based assertion. (The scaffold this document replaces named "Unit tests (pytest)";
that was an incorrect template artifact for a C# project and has been removed.)

### Disposition of existing tests

- `MoveAndIterate_ShouldMoveAndIterate` (QuickFiler.Test/Controllers/QfcFormControllerTests.cs:431-442)
  and `BackGroundMoveAsync_ShouldMoveEmails` (:444-455) are **vacuous**, and this was verified
  independently rather than taken from the research record. `CreateQfcFormController` (:75-87) calls
  the constructor only; `_groups` is assigned nowhere except the three `Init` overloads in
  QuickFiler/Controllers/QfcFormController.Actions.cs (:49, :83, :139) and `Cleanup()`. Because `Init`
  is never called, `_groups` is null, so `MoveAndIterate` returns at its guard
  (`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:149-152`) and `BackGroundMoveAsync`
  returns at its guard (`:219-222`) before reaching any behaviour. Both have empty Assert sections.
  They therefore neither cover nor obstruct this change and require no edit. The added `_parent` clause
  does not affect them: `_parent` is a non-null mock in that fixture, and the `_groups` clause short
  circuits first.
- `FilerQueue_NewInstance_HasCompletedConsumerByDefault`
  (`QuickFiler.Test/Controllers/FilerQueueTests.cs:76-87`) pins the retained `Consumer` default and
  must keep passing unmodified.
- `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException`
  (QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs:366-386) pins that a null helper
  surfaces synchronously from `Enqueue`. It must keep passing unmodified, which constrains the
  overload-delegation shape described above.
- `MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues`
  (`QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:190-235`) **will break** if the
  handshake repair removes the private `guard` field, because the test reflects into it by name at
  `:213-215` and would dereference `null`. Its `filerQueue.Queue.Count.Should().Be(1)` assertion at
  `:234` additionally depends on no worker having consumed the item. The reconciliation is to replace
  the reflection with an `ItemProcessor` that never completes (or that records the item), which is the
  supported mechanism the seam exists to provide. If the implementation instead retains a private field
  of the same name and type with the same semantics, no edit is needed — but the test must be run and
  seen to pass either way.

### Regression tests to add or update — `QuickFiler.Test/Controllers/FilerQueueTests.cs`

Extend the existing class, and correct its class comment at `:12-19`, which currently records that the
`Enqueue`/`ConsumeAsync` path is deliberately not exercised.

| Test | Scenario | Expected outcome |
|---|---|---|
| `WhenDrainedAsync_OnFreshQueue_ReturnsCompletedTask` | No work ever enqueued | The returned task is already completed |
| `WhenDrainedAsync_WithGatedItem_DoesNotCompleteBeforeItemCompletes` | One enqueue, `ItemProcessor` returns an ungated `TaskCompletionSource.Task` | The drain task's `IsCompleted` is false; the processor has been entered |
| `WhenDrainedAsync_AfterGateReleased_CompletesAndItemRanOnce` | Release the gate, await the drain | The await completes and the processor ran exactly once |
| `WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete` | Two enqueues, one gate each; release the first only | The drain task is not complete; after the second gate releases, it completes and both processors ran |
| `WhenDrainedAsync_AwaitedTwice_BothWaitersComplete` | Two waiters obtained before the gate releases, plus a second await after completion | Both complete; the post-completion call returns a completed task |
| `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` | Release the first gate, await the drain, enqueue again, await the new drain | The second item is processed. This is the regression for the orphaned-item window |
| `ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes` | Processor throws | The drain completes, the loop continues, and the failure is logged rather than propagated to the waiter |

### Regression tests to add — `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` (new)

Arrangement: `_groups` is a `Mock<IQfcCollectionController>` whose `MoveEmailsAsync` returns
`Task.CompletedTask`; `_parent` is a `Mock<IQfcHomeController>` whose `FilerQueue` getter returns a real
`FilerQueue` with a gated `ItemProcessor` and one pre-enqueued item; `_globals` is a
`Mock<IApplicationGlobals>` with `FS.Filenames` populated; `WriteMetrics` is a recording delegate.
Private fields are injected by the existing reflection helpers. A pumping dispatcher is installed
through the existing `UiThreadDispatcherFixture` transaction API, because `UiThread.Dispatcher` returns
the raw static without initializing it and is null in a bare test process.

| Test | Scenario | Expected outcome |
|---|---|---|
| `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` | Call the method and inspect the returned task without releasing the gate | The returned task is not complete, and `CleanupBackground` was never invoked. Deterministic without pumping: `MoveEmailsAsync` completes synchronously, so the method runs to the barrier before returning |
| `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain` | Same arrangement | The metrics delegate was never invoked |
| `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp` | Release the gate and await the returned task | The metrics delegate was invoked once and `CleanupBackground` was invoked once, in that order |
| `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing` | Null `_parent` (the post-`Cleanup()` state) with `_groups`, `_globals.FS.Filenames`, and `WriteMetrics` all non-null | The method returns without throwing; the queue and the dispatcher are untouched |
| `BackGroundMoveAsync_WhenGroupsIsNull_ReturnsWithoutTouchingQueue` | Null `_groups` | Early return preserved; pins the existing vacuous tests' behaviour |

### Edge cases and negative scenarios

Covered by the tables above: zero outstanding items; a single item; several items completing out of
order; repeated and concurrent waiters; a faulting item; a second batch after a drain; each null-guard
branch of `BackGroundMoveAsync`, including the newly added `_parent` clause.

### Error handling and logging verification

The faulting-item test asserts that the drain still completes and that the exception does not surface
to the waiter. The existing worker `catch` and its `logger.Error` call are unchanged, so no new logging
assertion is introduced; the test verifies the behaviour the log accompanies, not the log text.

### Coverage impact and targets for changed lines/modules

- Every member added to `QuickFiler/Controllers/FilerQueue.cs` — `WhenDrainedAsync`, the modified
  `Enqueue` overloads, and the modified `ConsumeAsync` — must reach at least 90% line coverage, per
  CLAUDE.md UT2 for new modules and methods.
- Coverage must not regress on any line changed by this fix.
- No repository-wide coverage floor is asserted as a blocking condition here. No merge-base coverage
  baseline exists in this feature folder, so a repo-wide threshold could not be shown to be
  satisfiable; the repo-wide figure is instead a record-and-report obligation, captured as evidence and
  compared before and after, with the requirement that this change does not lower it.

### Toolchain commands to run (format, lint, type-check, test)

Run in this exact order, restarting from the top if any step fails or modifies a file:

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <QuickFiler.Test assembly path> /EnableCodeCoverage`

Notes that make these gates honest rather than vacuous:

- Both MSBuild steps must be run with `/fl "/flp:logfile=<log path>;verbosity=normal"` and the log must
  contain **zero** occurrences of the literal `Skipping target "CoreCompile"`. Exit code alone cannot
  distinguish a real compile from a warm incremental no-op, and counting `csc.exe` invocations or
  `CoreCompile:` header lines does not work at `verbosity=normal`.
- Trust MSBuild's own `N Error(s)` summary line for error counts; a raw `Select-String 'error CS'` over
  the log double-counts, because each error prints inline and again in the summary block.
- Do not add `/p:Nullable=enable` and do not substitute `/t:Build`. CLAUDE.md § C#1.2 and § C#1.3 state
  both prohibitions in line, with the reasons.
- For a local `vstest.console.exe` run, exclude assemblies under `\.claude\` worktrees and pass
  `/InIsolation`; without both, assembly-load failures appear as sub-millisecond, empty-message test
  failures that are not real regressions.

### Manual validation steps (if required)

Optional, and not a gate. In a live Outlook session, perform a batch move of several emails and then
immediately open the undo dialog; confirm every moved item is present and that the metrics written for
the session are identical in shape and content to a pre-change run.

## Acceptance Criteria

Each criterion below is checkable by a named test or a named command. Test names are the ones specified
in the Test Strategy; if an implementer chooses different names, the criterion is satisfied by the test
that covers the named scenario, and the substitution must be recorded.

- [ ] `QuickFiler/Controllers/FilerQueue.cs` exposes `public Task WhenDrainedAsync()`, which returns an
      already-completed task on a queue with no outstanding work. Verified by
      `WhenDrainedAsync_OnFreshQueue_ReturnsCompletedTask`.
- [ ] The drain task does not complete while any enqueued item is still being processed. Verified by
      `WhenDrainedAsync_WithGatedItem_DoesNotCompleteBeforeItemCompletes` and
      `WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete`, both driven by
      `TaskCompletionSource` gates through `ItemProcessor`.
- [ ] The drain task completes once every enqueued item has completed, and each item's processor ran
      exactly once. Verified by `WhenDrainedAsync_AfterGateReleased_CompletesAndItemRanOnce` and
      `WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete`.
- [ ] `WhenDrainedAsync()` is idempotent: repeated and concurrent waiters all complete, and a call made
      after the queue is idle returns a completed task. Verified by
      `WhenDrainedAsync_AwaitedTwice_BothWaitersComplete`.
- [ ] The orphaned-item window is closed: an item enqueued after a previous batch has drained is
      processed without requiring any further unrelated enqueue. Verified by
      `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch`.
- [ ] An item whose processing throws still decrements the outstanding-work count, the worker loop
      continues, and the drain task completes rather than faulting or hanging. Verified by
      `ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes`.
- [ ] `BackGroundMoveAsync` in `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` awaits
      `WhenDrainedAsync()` after `await _groups.MoveEmailsAsync(_movedItems)` and before both the
      `WriteMetrics` dispatch and the `CleanupBackground()` dispatch. Verified by
      `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` and
      `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain`.
- [ ] The existing metrics-before-cleanup ordering is preserved: after the drain, `WriteMetrics` is
      invoked once and `CleanupBackground()` is invoked once, in that order. Verified by
      `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp`.
- [ ] The early-return guard in `BackGroundMoveAsync` includes a `_parent` null check, so the method
      returns without throwing on the post-`Cleanup()` path where `_parent` has been set to null.
      Verified by `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing`.
- [ ] The two production reads of `FilerQueue.Consumer` — at
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:167` and `:193` — are removed, and
      `Grep pattern="\.Consumer\b" glob="QuickFiler/**/*.cs"` returns zero matches. The count of two is
      the complete production population, derived and independently re-verified against the working
      tree on 2026-08-31; the derivation is recorded as Claim 1 under `## Numeric Derivation Evidence`
      in the research record for this issue.
- [ ] `FilerQueue.Consumer` remains declared with the same type, accessibility and completed-task
      default. Verified by `FilerQueue_NewInstance_HasCompletedConsumerByDefault` in
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`, which passes unmodified.
- [ ] `Enqueue(EmailFiler, IList<MailItemHelper>)` still raises `ArgumentNullException` synchronously in
      the caller's frame for a helper list containing null. Verified by
      `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException`, which passes unmodified.
- [ ] `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` is reconciled with the new
      queue internals: `MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues` passes
      and no longer reflects into a private `FilerQueue` field that the fix removes or renames.
- [ ] The added and modified test code contains no banned wait API. `Grep` over
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`,
      `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` and
      `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)` returns zero matches.
- [ ] No `init` accessor, `record`, or `record struct` is introduced. `Grep` over
      `QuickFiler/Controllers/FilerQueue.cs` and
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` for `\binit\s*[;{]|\brecord\b` returns
      zero matches, and the solution compiles on net481 without CS0518.
- [ ] The production diff touches no file other than `QuickFiler/Controllers/FilerQueue.cs` and
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`. Verified by
      `git diff --name-only <merge-base>..HEAD` showing no other path outside `QuickFiler.Test/` and
      `docs/`.
- [ ] `QuickFiler.Test/QuickFiler.Test.csproj` contains a `<Compile Include>` entry for
      `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`, and the new tests appear in
      the `vstest.console.exe` run output.
- [ ] Both changed production files remain under 500 lines. Verified by a line count on
      `QuickFiler/Controllers/FilerQueue.cs` and
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`.
- [ ] The full C# toolchain passes in a single uninterrupted pass, in the order format, analyze,
      type-check, test: `dotnet tool run csharpier check .` reports no unformatted file; both
      `msbuild TaskMaster.sln /t:Rebuild ...` invocations exit 0 with zero
      `Skipping target "CoreCompile"` occurrences in their `/fl` logs; and
      `vstest.console.exe <QuickFiler.Test assembly path> /EnableCodeCoverage` reports zero failures.
      Logs are recorded under
      `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/evidence/qa-gates/`.
- [ ] Coverage does not regress on any line changed by this fix, and the members added or modified in
      `QuickFiler/Controllers/FilerQueue.cs` reach at least 90% line coverage. Before-and-after coverage
      artifacts are recorded under
      `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/evidence/baseline/`
      and
      `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/evidence/qa-gates/`,
      and the repository-wide figure is recorded and shown not to be lowered by this change.

## Risks & Mitigations

### Technical or operational risks

| # | Risk | Likelihood | Mitigation |
|---|---|---|---|
| 1 | **The barrier deadlocks because the worker faults and the outstanding count never reaches zero.** A leaked count would leave `WhenDrainedAsync()` permanently incomplete and hang the batch-move path — a strictly worse failure than the latent defect being fixed. | Low, but the highest-consequence risk in this change | The decrement is in a `finally` around the processor call, so it runs whether the item succeeds, throws, or is cancelled; the existing `catch` around the processor already prevents the loop from terminating on an item fault. `ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes` is the regression that pins this, and it is an acceptance criterion in its own right. |
| 2 | **The barrier never completes because no worker was started for an enqueued item.** This is the pre-existing orphaned-item window; a barrier over the unrepaired handshake would convert it from a delayed push into a hang. | Would be moderate without the repair | The handshake repair takes the enqueue, the counter increment, and the worker start/stop decision under a single monitor, and clears the "running" flag in the same critical section in which `TryTake` fails. `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` is the regression. |
| 3 | **Deadlock against the UI thread.** | Low | The filing path does not require the UI thread: `ConsumeAsync` runs its loop inside `Task.Run` where `SynchronizationContext.Current` is null, the per-helper processing is awaited with `ConfigureAwait(false)`, and the COM work is itself wrapped in `Task.Run`. The barrier is awaited *before* the two `UiThread.Dispatcher.InvokeAsync` calls, never while a dispatcher operation is in flight, and the monitor is never held across an `await`. |
| 4 | **A user-visible pause on the batch-move path**, since `await moveTask` at `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:175` now waits for filing work it previously never waited for. | Moderate likelihood, low impact | The wait occurs after `LoadUiFromQueue` and `IterateQueueAsync`, so the next group is already displayed; `ButtonOK_Click` is `async void` and has already yielded, so the message loop is not blocked. No numeric latency budget is asserted because no timing telemetry exists to derive one from. |
| 5 | **`QfcItemController.SeamFactoryTests` breaks on the removed private `guard` field**, producing a `NullReferenceException` inside a reflection call that reads as an unrelated failure. | High if unaddressed — the reflection is by field name | The reconciliation is scoped and named in the Test Strategy and carries its own acceptance criterion, so it cannot be discovered late. |
| 6 | **A future contributor adds a timing-based wait to make a flaky concurrency test pass**, which would silently make the barrier's correctness unfalsifiable. | Low | The determinism constraint is stated in Assumptions/Constraints and enforced by a grep-based acceptance criterion over the three named test files. |
| 7 | **Removing the two `Consumer` awaits changes behaviour on the catch and terminal branches.** | Low | Both are strictly subsumed: each is immediately preceded by an await of the same `BackGroundMoveAsync` task, which now contains the barrier, and the barrier waits on the whole outstanding count rather than on one worker task — a superset of what `Consumer` covered. `Consumer` itself is retained on the public surface. |
| 8 | **The sibling queue TaskVisualization/FlagChangeTrainingQueue.cs keeps the same latent handshake window**, so a reader may assume it was fixed too. | Low | Explicitly recorded as a non-goal above, with the note that it is a different type. It should be raised as its own issue if the same repair is wanted there. |

### Mitigations and rollbacks

- Rollback is a revert of the commit. The change is additive on the public surface, adds no persisted
  state, and touches two production files, so a revert is clean and requires no migration.
- If the added wait proves to cause an unacceptable pause in live use, the correct follow-up is to
  move the barrier's position within `BackGroundMoveAsync` or to shorten the filing work — not to
  remove the barrier, which would restore the unexpressed ordering this issue exists to eliminate.

## Rollout & Follow-up

### Release/rollout steps

1. Deliver on the branch `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633` (base `origin/main`
   at 9b6aff2e).
2. Run the four-stage toolchain to a clean pass and record the gate artifacts under
   `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/evidence/qa-gates/`.
3. Open the pull request referencing issue #633. No staged rollout, no feature flag, no configuration
   change is required; the add-in ships as a single build.

### Post-fix monitoring or clean-up tasks

- Watch for any report of a perceived pause after confirming a batch move; correlate against risk 4
  before treating it as a new defect.
- If a hang is ever reported on the batch-move path, the first thing to inspect is the outstanding-work
  counter and the worker start/stop flag in `QuickFiler/Controllers/FilerQueue.cs` (risks 1 and 2).

### Follow-up candidates (not delivered by this issue)

- Apply the same handshake repair to TaskVisualization/FlagChangeTrainingQueue.cs, which has the same
  structural window. Promote through the issue lifecycle rather than absorbing it here.
- Consider whether QuickFiler/Interfaces/IFilerHomeController.cs should eventually expose an
  abstraction rather than the concrete `FilerQueue` type. Not needed for testability once the
  `ItemProcessor` seam exists, and a strictly larger diff.

### Links

- Issue: https://github.com/drmoisan/TaskMaster/issues/633
- Requirements source: docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/issue.md
- Research record: docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/research/2026-08-31T19-45-undo-handoff-ordering-research.md
- Origin: issue #468 defect family, follow-up candidate 7, task [P14-T5]

## Deviation from the research record

One deviation, recorded so a reviewer does not read it as an omission.

**The research record's test blast radius is incomplete.** Section F lists three test-side files. It
omits `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`, which reflects into the
private `FilerQueue` field named `guard` at `:213-215` and sets `ThreadSafeSingleShotGuard._state` at
`:216-218` in order to suppress the consumer, then asserts `filerQueue.Queue.Count == 1` at `:234`.
Under the recommended repair — which replaces the one-shot guard with a monitor-protected running flag
— that reflection resolves to `null` and the test throws, and the `Queue.Count` assertion additionally
becomes dependent on no worker having consumed the item. Section E.1 mentions this test file as
existing coverage and section E.3 mentions the reflection as "the established workaround", but neither
carries the consequence into the file list. This specification therefore adds that file to the
test-side scope and to the acceptance criteria. This does not change the production blast radius, which
remains the two files the research identifies.

The research record's recommended remedy is otherwise adopted in full: the counted per-batch awaitable
quiesce, the handshake repair as a precondition rather than a refactor, the `_parent` guard clause, the
`ItemProcessor` seam, and the removal of the two subsumed `Consumer` awaits.
