# Research: QuickFiler High Confidence deadline policy and Cancel teardown (#791)

- Date: 2026-09-06
- Author: task-researcher agent
- Scope: static analysis and design research only; no code changes
- Canonical issue: #791
- Base commit read: `7c8ac9ae` (worktree `TaskMaster-wt/2026-09-06T09-59`, clean)

## Summary

Two independent defects share one feature folder. AC1 concerns
`QfcStreamingDequeueConfidenceGate.DequeueAsync`, whose first-batch deadline terminates the scan
while `accepted.Count == 0`, producing an empty High Confidence dialog even though unscanned
candidates remain in the master queue. AC2 concerns the Cancel teardown, which is unordered: the
background queue loader is never awaited before `QfcDatamodel.Cleanup()` nulls the fields the loader
still dereferences, the keyboard-active flag and WebView2 focus are reset only on paths the Cancel
path unsubscribes or never reaches, and the ribbon release callback is not protected by a `finally`.
Every established fact supplied to this research was re-verified against the current tree and is
cited below with file:line. Two prior acceptance criteria (#424 and #608) explicitly ratified the
behavior AC1 now changes; they are identified so the plan supersedes them deliberately rather than
regressing them silently.

## Current State Analysis

### The gate and its deadline

- Cutoff conversion to per-mille: `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:129`
  (`_cutoff = (long)Math.Round(threshold * 1000, 0)`). The scan loop is `:168-237`.
- The deadline is evaluated only while nothing has been accepted:
  `:172-176` guards on `deadlineEnabled && accepted.Count == 0 && elapsed >= _firstBatchDeadline`,
  then returns `QfcDequeueStop.DeadlineExpired` at `:179`. `#608` deliberately restricted the test to
  the zero-accepted case (documented at `:88-95`).
- `scanned++` occurs at `:205`, after `_scoreLoader` returns (`:199-203`), so a logged
  `Scanned=38 Accepted=0` means 38 completed scores all strictly below `_cutoff` — the observation in
  the issue is consistent with the code.
- Rejected candidates leave the session queue permanently: the take at `:182` removes the item and the
  reject branch (`:215-232`) only unhooks it. A rerun therefore rescans the same view prefix, matching
  the reported determinism.
- `DefaultFirstBatchDeadline = TimeSpan.FromSeconds(12)` at `:56`.
- `LogDeadlineExpiry` (`:242-250`) emits `Accepted`, `Scanned` and `Deadline`; it does not emit
  `_cutoff`, and there is no launch-time log line at all. `LogScore` (`:252-260`) logs each score.
- Logging idiom confirmed: `private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);`
  at `QfcStreamingDequeueConfidenceGate.cs:45-47`, `QfcHomeController.cs:21-23`, `QfcDatamodel.cs:28-30`,
  `QfcFormController.cs:21-23`. `QfcDatamodel.cs:97-99` and `QfcFormController.cs:67-69` each declare a
  second identical logger named `log`; both names are live in those files.

### Who consumes `DeadlineExpired`

Located by two independent searches (a repository-wide content search for `DeadlineExpired`, and a
declaration search for `enum QfcDequeueStop` followed by reading each referencing production file):

- Declaration and XML docs: `QuickFiler/Interfaces/IQfcDatamodel.cs:30-40`.
- Producer: gate `:179`, projected verbatim by
  `QfcDatamodel.QueueProcessing.cs:177-200` (`DequeueWithHighConfidenceGateWithOutcomeAsync`).
- Consumer 1 — `QfcHomeController.RunAsync` (`QfcHomeController.cs:271-341`): calls the outcome
  member at `:300-305` with `DefaultFirstBatchDeadline` and the scan-progress sink, then loads
  `preScored` at `:322`. It does not read `batch.Stop`; an empty accepted set simply loads zero rows.
- Consumer 2 — `QfcHomeController.IterateQueueAsync` (`QfcHomeController.Iteration.cs:22-48`): reads
  `batch.Stop` and calls `QfcQueue.CompleteAddingAsync` only under `SourceExhausted` (`:39-47`). This
  branch is pinned by #446 AC-6, so any new stop reason must not be routed into it.

### Streaming loader and the `_remainingLoadActive` refill signal

- `_remainingLoadActive` is declared `volatile` at `QfcDatamodel.QueueProcessing.cs:23`, set true
  immediately before `worker.RunWorkerAsync()` (`QfcDatamodel.cs:256`, `:283`) and cleared in the
  `finally` of `Worker_DoWork` (`:193-200`).
- The gate receives it as `sourceActive` (`QfcDatamodel.QueueProcessing.cs:190`) and uses it at
  `gate:185-196`: when `_tryTakeNext()` returns null and the producer is still live, the gate waits
  `timeOut` ms through `TimeProvider.Delay` and retries; `SourceExhausted` is reported only when
  `timeOut <= 0` or the second consecutive empty take coincides with a dead producer.
  **Consequence for AC1:** "queue exhausted" is already honest while the loader refills, but the wait
  loop does not increment `scanned`, so an item-count cap alone does not bound the wait.
- `Worker_DoWork` is `async void` (`QfcDatamodel.cs:175-213`); no handle to the loader task is
  retained. `SetupWorker` registers `worker.CancelAsync` on the token (`:170`), which sets
  `CancellationPending` but cannot stop an `async` body that never reads it.
- `LoadRemainingEmailsToQueueAsync` observes the token at `:322` and `:324` only, then calls
  `TryQueueRemainingMailItemAsync` (`:350-361`), which constructs
  `new QfcRemainingQueueAdmission(_masterQueue.AddLast, _moveMonitor.HookItem, x => _masterQueue.Remove(x))`
  at `:355-359`. When either field is already null, delegate construction throws
  `ArgumentException: Delegate to an instance method cannot have null 'this'` — exactly the logged
  error. `QfcRemainingQueueAdmission` itself is sound (`QfcRemainingQueueAdmission.cs:14-38`) and no
  longer carries the dead constructor parameters #731 identified.
- `QfcDatamodel.Cleanup()` (`:75-91`) cancels, calls `_worker?.CancelAsync()`, then unconditionally
  dereferences `_globals.Ol.App` and `_moveMonitor` (`:79-80`) and nulls `_moveMonitor`, `_globals`,
  `_masterQueue`, `_worker` (`:81-90`) without awaiting anything.

### Cancel path

- `ButtonCancel_Click` is `async void` and rethrows after logging
  (`QfcFormController.EventHandlers.cs:70-82`), so an escaping exception becomes an unhandled
  UI-thread failure inside Outlook.
- `ActionCancelAsync` (`:84-94`): cancel token, `await _formViewer.UiSyncContext`, `Hide()`,
  `_groups?.Cleanup()`, `Cleanup()`. No `try`/`finally`, no `KbdActive` reset, no focus parking, no
  logging. It is also the completion path: `MoveAndIterate` calls it at `:169` (error) and `:208`
  ("Finished Moving Emails"), so the same defects apply to normal completion.
- The OK path does reset the keyboard flag (`:125-128`,
  `if (_parent.KeyboardHandler.KbdActive) _parent.KeyboardHandler.ToggleKeyboardDialog();`).
- `RegisterFormEventHandlers` subscribes `FormDeactivated` at `SetupDisposal.cs:175`;
  `UnregisterFormEventHandlers` unsubscribes it at `:204`. `FormViewer_Deactivated`
  (`QfcFormController.Deactivate.cs:26-58`) is the only caller of `ParkFocusOffWebView2()` and of the
  per-item `CancelBreadcrumbSelector()` loop. The Cancel path removes that subscription (through
  `Cleanup` → `UnregisterFormEventHandlers`) and never invokes the routine directly.
- Ordering defect: `QfcFormController.Cleanup()` calls `UnregisterFormEventHandlers()` at `:220`,
  but `_groups.Cleanup()` already ran (`EventHandlers.cs:92`). `QfcCollectionController.Cleanup`
  (`:2128-2140`) delegates to `RemoveControls()` (`:737-757`), which removes the rows from the
  `TableLayoutPanel` at `:745` and clears `_itemGroups` at `:751`. The recursive
  `Controls.ForAllControls` unsubscribe at `SetupDisposal.cs:185-197` therefore no longer reaches the
  item controls whose `PreviewKeyDown`/`KeyDown` were attached at `:156-168`. The guard at `:180-183`
  additionally returns early once `_formViewer?.Controls` or `_parent?.KeyboardHandler` is null.
- `QfcCollectionController.Cleanup` touches neither `_kbdHandler`/`KbdActive` nor
  `UnregisterNavigation()`; `UnregisterNavigation` is public on the interface
  (`IQfcCollectionController.cs:109`) and implemented at `QfcCollectionController.cs:1080-1089`
  (#644 ledger replay).
- `QfcHomeController.Cleanup()` (`:370-379`) calls `_datamodel.Cleanup()` first and
  `ParentCleanup.Invoke()` last with no `try`/`finally`; `_tokenSource` is never disposed and
  `Worker_RunWorkerCompleted` (subscribed at `:131`) is never detached. Note that by the time this
  runs, `QfcFormController.Cleanup` has already disposed the viewer (`SetupDisposal.cs:251`) and only
  then invoked `_parentCleanup` (`:259`), so any viewer access here must be defensive.
- `RibbonController.ReleaseQuickFiler` (`TaskMaster/Ribbon/RibbonController.cs:148-153`) is the
  `ParentCleanup` delegate; it clears `_quickFiler`, `_quickFilerLoaded` and the high-confidence
  launch flag. It is `private` with no test seam, and both launch guards depend on it (`:114`, `:135`).

### Reachable seams in tests

- Gate: `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:27-121` builds the gate
  by reflection against an **exact** nine-parameter constructor signature and asserts the constructor
  is found (`:74-77`), i.e. it fails closed. Any added constructor parameter requires updating this
  helper. `FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`) is already the clock seam
  (`Part3.cs:178-195`), so no new time seam is needed.
- Datamodel: `FormatterServices.GetUninitializedObject` plus private-field injection, with
  `TimeProvider`, `ScoringServiceFactory` and `RemainingEmailLoader` as public/internal seams
  (`QfcDatamodelLivenessTests.cs:35-45,83-100`; `QfcQueuePurePathsTests.cs:205-250`).
  `RemainingEmailLoader` (`QfcDatamodel.cs:130`) is the injection point that makes the loader
  controllable without COM.
- Form controller: real constructor plus `Mock<IQfcFormViewer>`, `Mock<IQfcHomeController>`,
  `Mock<IQfcCollectionController>` injected into `_groups` by reflection, and `Mock.Raise` for viewer
  events (`QfcFormControllerDeactivateTests.cs:36-92`); `TimeProvider`/`UndoConsumerStarter`
  overrides in `QfcFormControllerCleanupTests.cs:60-77`.
- Existing cancel coverage is vacuous: `QfcFormControllerTests.cs:392-403`
  (`ButtonCancel_Click_ShouldCancelAction`) awaits `ActionCancelAsync()` and asserts nothing.

## Behavior Semantics

AC1 (deadline policy):

- Success: with `HighConfidenceModeEnabled` and zero acceptances at the first checkpoint, scanning
  continues; the call returns as soon as one candidate scores `>= _cutoff` (subsequent behavior is
  unchanged #608 fill-or-exhaust), or when the source is genuinely exhausted, or when the hard cap is
  reached. A zero-row dialog is legal only for exhaustion or cap.
- Failure/edge: cancellation during the extended scan must still surface `OperationCanceledException`
  from `:170`/`:204`; a transient empty queue while `_remainingLoadActive` is true must not be read as
  exhaustion; `quantity <= 0` must still short-circuit (`:159-162`); a non-empty prefix must keep
  #608 semantics (the deadline must remain inert once `accepted.Count > 0`).
- Ordering: the checkpoint decision is evaluated before the take, as today; the cap must be checked in
  the same place so a capped scan cannot take an extra item.

AC2 (Cancel teardown) — the required order, each step observable:

1. Log entry to the teardown.
2. Signal cancellation (`_parent?.TokenSource?.Cancel()`).
3. Marshal to the UI context (`await _formViewer.UiSyncContext`).
4. Reset `KbdActive` (toggle only when active, mirroring `:125-128`).
5. Park focus off WebView2 and cancel every open breadcrumb selector (the `FormViewer_Deactivated`
   routine), while `_groups.ItemGroups` still exists.
6. `_groups?.UnregisterNavigation()` and `UnregisterFormEventHandlers()` — before rows are removed.
7. `Hide()`.
8. Await the background loader to quiesce, bounded; only then allow datamodel field nulling.
9. `_groups?.Cleanup()`.
10. `Cleanup()` → `_parentCleanup` → `QfcHomeController.Cleanup` → `_datamodel.Cleanup()` and
    `ParentCleanup.Invoke()` under `finally`.

Failure semantics: any step may throw; every later step must still run, the release callback must run,
and each exception must be logged with its stage. Repeat invocation (double Cancel, or Cancel after
`MoveAndIterate`'s completion path) must be inert rather than throwing.

## Recommended Approach

### AC1 — advisory checkpoint plus a hard scan bound

Change the zero-acceptance branch (`gate:172-180`) from a return into a checkpoint:

- Keep `_firstBatchDeadline` but re-purpose it as the **checkpoint interval**: on expiry, log the
  cutoff, `scanned`, `accepted.Count`, the elapsed time and the remaining bound, reset the interval
  origin, and continue scanning.
- Add two bounds, both injected through the constructor with internal defaults on the gate:
  `maxScanWithoutAcceptance` (recommended default 250 scored candidates) and
  `zeroAcceptanceCeiling` (recommended default 120 s). The item cap answers the AC's "hard cap on
  items scanned"; the time ceiling is required in addition because the empty-queue wait path
  (`gate:185-196`) does not increment `scanned`, so an item cap alone leaves the pre-UI wait unbounded
  while `_remainingLoadActive` is true.
- Add `QfcDequeueStop.ScanCapReached` for the bounded exit and treat it exactly as `DeadlineExpired`
  is treated today (queue stays open; `IterateQueueAsync` still calls `CompleteAddingAsync` only under
  `SourceExhausted`, preserving #446 AC-6 verbatim). Retain the `DeadlineExpired` member with an
  updated XML doc recording that #791 made the deadline advisory; retaining it avoids touching the
  public enum's existing members and keeps both existing stop-reason tests meaningful after
  retargeting.
- Add a launch log line at the top of `DequeueAsync` carrying cutoff, quantity, checkpoint interval
  and both bounds, satisfying "logged at launch".

Configuration location: keep the bounds as gate-internal `internal static readonly` constants with the
constructor seam, following the precedent set by #424, whose ratified acceptance criterion states the
deadline is "an internal constant with an internal test seam; no new `QfSettings`/
`IAppQuickFilerSettings` member, no `Settings.Designer.cs` change, and no ribbon plumbing"
(`docs/features/archive/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md:239`).
`TaskMaster/Properties/Settings.Designer.cs:1-9` is auto-generated and must not be hand-edited, and
`AppQuickFilerSettings` (`TaskMaster/AppGlobals/AppQuickFilerSettings.cs:48-66`) exposes only the two
high-confidence settings; adding a third would also require `IAppQuickFilerSettings`, `app.config`,
`Settings.settings` and ribbon plumbing for a value with no user story.

Prior-AC reconciliation (must be stated explicitly in the plan, not discovered at review):

- #424 spec AC "When zero candidates reach the cutoff before the deadline, `DequeueAsync` returns an
  empty list at the deadline bound, and the `RunAsync` path proceeds to show the form with an empty
  first group" (`.../424/spec.md:231`) is **superseded** by #791 AC1.
- #608 spec AC "Deadline expiry with `accepted.Count == 0` retains the current empty-result behavior"
  (`docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/spec.md:184`)
  is **superseded** by #791 AC1. #608's other criteria (`:181-183`, `:185`) concern the non-empty
  prefix and must remain green.
- #446 AC-6 (`CompleteAddingAsync` only under `SourceExhausted`) is **preserved** by routing the new
  stop reason away from that branch.

Rejected alternatives for AC1:

- *Raise the deadline constant (e.g. 12 s → 60 s).* Rejected: it re-parameterises the same defect and
  still yields an empty dialog on any view whose qualifying items sit past the new bound.
- *Rank-and-take the best-scoring candidate when nothing clears the cutoff.* Rejected: it silently
  files below-threshold suggestions, defeating the purpose of High Confidence mode and contradicting
  the inclusive `score >= _cutoff` rule ratified by #608 (`spec.md:185`).
- *Show the dialog immediately and back-fill rows asynchronously.* Rejected: the row-loading path
  (`RunAsync` → `LoadItemsAsync`) is single-shot per iteration, so this is a UI-architecture change far
  wider than the defect, and it does not remove the empty first screen.
- *Replace the deadline parameter with a bounds struct.* Rejected: it churns every existing gate call
  site and the fail-closed reflection helper for no behavioral gain over two optional parameters.

### AC2 — ordered, logged, exception-safe teardown

1. **Make the loader awaitable without blocking.** In `Worker_DoWork`, capture the task before
   awaiting it (`_remainingLoadTask = RemainingEmailLoader(_token); e.Result = await _remainingLoadTask;`)
   and expose `Task QuiesceLoaderAsync(TimeSpan timeout)` on `IQfcDatamodel` that cancels, then awaits
   `Task.WhenAny(_remainingLoadTask, TimeProvider.Delay(timeout, CancellationToken.None))`, logging
   whether the loader completed or the bound expired. Declare the field and the method in
   `QfcDatamodel.QueueProcessing.cs` (partial class) to protect `QfcDatamodel.cs`'s remaining headroom.
   Call it from `ActionCancelAsync` through `_parent.DataModel` (`IQfcHomeController.DataModel`,
   `QuickFiler/Controllers/IQfcHomeController.cs:11`), i.e. from an `async` method — **never** a
   blocking wait inside `Cleanup()`, which #731 established runs on the UI thread.
2. **Guard the admission construction as defence in depth.** Relocate
   `TryQueueRemainingMailItemAsync` into `QfcDatamodel.QueueProcessing.cs`, snapshot `_masterQueue` and
   `_moveMonitor` into locals, and return `false` when either is null or cancellation is requested.
   This makes the reported crash impossible even if a future path skips the quiesce, and it is
   directly unit-testable through the uninitialized-object pattern.
3. **Null-guard `QfcDatamodel.Cleanup()`** (`:79-80` currently unguarded) so a second Cancel, or a
   Cancel after a partially-failed launch, cannot throw before the fields are released.
4. **Extract the deactivate routine.** Split `FormViewer_Deactivated` (`Deactivate.cs:26-58`) into the
   event handler plus `internal void ParkFocusAndCancelSelectors()`; call the latter from both the
   event and the Cancel path. This is the "same routine" the AC requires and keeps the per-item
   boundary catch intact.
5. **Reorder `ActionCancelAsync`** to the ten steps in Behavior Semantics, with a `try`/`catch`/
   `finally` per stage-group so a throwing stage cannot skip the release callback. Call
   `_groups?.UnregisterNavigation()` from `ActionCancelAsync` rather than adding it to
   `QfcCollectionController.Cleanup`, because that file is already 2329 lines and adding to it worsens
   an existing 500-line violation; `UnregisterNavigation` is on the interface, so no new seam is needed.
   Keep the existing `UnregisterFormEventHandlers()` call inside `Cleanup()`: it is idempotent
   (`-=` on absent handlers is a no-op) and preserves the non-Cancel call shape.
6. **`QfcHomeController.Cleanup()`**: wrap the datamodel cleanup, the field nulling and the
   `Worker_RunWorkerCompleted` detach in `try`/`catch` with logging, and invoke `ParentCleanup` in a
   `finally`; dispose `_tokenSource` there as well. The viewer is already disposed by the caller, so
   the detach must be inside its own guarded block.
7. **Do not rethrow from `ButtonCancel_Click`** (`EventHandlers.cs:70-82`): an `async void` rethrow
   becomes an unhandled Outlook UI-thread exception, which is precisely the failure mode the AC's
   logging requirement exists to replace. This is a deliberate behavior change and should be called out
   in the spec.

`UnregisterNavigation` on the Cancel path (#644): recommended **yes**, at step 6 of the order. The
navigation actions are digit-string entries in the shared `KeyboardHandler.StringActionsAsync` ledger
(`QfcCollectionController.cs:1080-1099`); `QfcCollectionController.Cleanup` never drains it, and the
handler instance is per-launch, so leaving it is not a cross-session leak — but draining it before the
rows disappear keeps the #644 ledger invariant true through teardown and costs one call.

#731 reconciliation (already on base; must not be duplicated or undone): the deferred undo-queue
disposal via `_undoQueueDisposal` (`SetupDisposal.cs:207-249`) stays exactly as is; the one-monitor-per-
owner comment and design (`QfcDatamodel.cs:104-105`) stays; `QfcRemainingQueueAdmission`'s three-delegate
constructor is final. In particular, do not "simplify" step 1 into a blocking wait inside `Cleanup()` —
that is the deadlock #731 finding 4 rejected.

Rejected alternatives for AC2:

- *Make `Cleanup()` async throughout (`IQfcDatamodel.CleanupAsync`, `IFilerHomeController.CleanupAsync`).*
  Rejected: it changes three interfaces and the `System.Action parentCleanup` contract that
  `RibbonController` supplies, for no behavior the bounded quiesce in `ActionCancelAsync` does not give.
- *Have the loader poll a `_cleanupRequested` flag only.* Rejected: it narrows the race window without
  closing it and provides no observable completion point for a deterministic test.
- *Move focus parking into `QfcCollectionController.Cleanup`.* Rejected: wrong owner (the routine is
  viewer/form scoped) and it grows an already oversized file.

## Requirements Mapping

| File | Change | Current lines |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | checkpoint-instead-of-return; two bounds + defaults; launch log; cutoff in both log lines | 262 |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | add `ScanCapReached`; doc `DeadlineExpired` as superseded; declare `QuiesceLoaderAsync` | 133 |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | `_remainingLoadTask` field, `QuiesceLoaderAsync`, relocated + guarded `TryQueueRemainingMailItemAsync` | 298 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | capture loader task in `Worker_DoWork`; null-guard `Cleanup()`; remove relocated method | 480 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | ordered `ActionCancelAsync`; no rethrow in `ButtonCancel_Click`; stage logging | 408 |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | extract `ParkFocusAndCancelSelectors()` | 60 |
| `QuickFiler/Controllers/QfcHomeController.cs` | `Cleanup()` try/finally, token-source dispose, worker detach, logging | 469 |

Not touched, deliberately: `QfcCollectionController.cs` (2329 lines, pre-existing violation),
`QfcHomeController.Iteration.cs` (the `SourceExhausted`-only branch is already correct),
`TaskMaster/Ribbon/RibbonController.cs`, `Settings.Designer.cs`, `AppQuickFilerSettings.cs`.

Interface/API deltas: one new enum member, one new interface method, one new internal method, two new
optional constructor parameters on an internal class. `QuickFiler.csproj` and `QuickFiler.Test.csproj`
are legacy non-SDK projects with explicit `<Compile Include>` items (e.g.
`QuickFiler.csproj:321-325`, `QuickFiler.Test.csproj:155`), so every new file needs an entry.

## Testing Implications

MSTest + Moq + FluentAssertions, no temp files, no wall-clock waits. `FakeTimeProvider` is already the
established clock seam, so no new time injection is required for the gate; `QfcDatamodel.TimeProvider`
covers the quiesce timeout.

AC1 (new tests; suggested home `QfcStreamingDequeueConfidenceGateTests.Part4.cs`, new file, because
Part1 is 477 and Part2 465 lines):

- `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance` — 40 below-cutoff candidates
  then one at 950; fake clock advances 1 s per score with a 12 s checkpoint. Fails before (returns
  empty at the checkpoint), passes after (returns the qualifying item).
- `DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted` — cap not reached, producer dead.
- `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached` — cap injected as a small
  value; asserts no take occurs after the cap.
- `DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling` — `sourceActive` true and
  `tryTakeNext` always null; asserts the ceiling terminates the wait loop.
- `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts` and
  `DequeueAsync_Launch_LogsCutoffQuantityAndBounds` — assert through the injected `debugLog` delegate,
  not a log4net appender (existing convention).
- `DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint` — #608 regression pin.

AC1 test-maintenance obligations (these currently encode the superseded behavior and will fail after
the change; retarget, do not delete): `QfcStreamingDequeueConfidenceGateTests.Part3.cs:174-208`
(`DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop`),
`QfcQueuePurePathsTests.cs:201-260`
(`DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop`), and the
fail-closed reflection helper `QfcStreamingDequeueConfidenceGateTests.cs:27-92`, which asserts the
exact nine-parameter constructor. `QfcHomeControllerIterationTests.cs:395-402` should gain a sibling
asserting `ScanCapReached` also leaves the queue open.

AC2 (new file `QfcFormControllerCancelTeardownTests.cs`; `QfcFormControllerTests.cs` is 792 lines and
`QfcFormControllerSeamTests.cs` 496, both unsuitable):

- `ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive` / `..._DoesNotToggle_WhenInactive`.
- `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors` — `Mock<IQfcFormViewer>` with
  `IsWebView2Focused` true; verify `ParkFocusOffWebView2` and per-item `CancelBreadcrumbSelector`.
- `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup` — Moq `MockSequence` or a shared
  invocation-order list; fails before (order inverted).
- `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup` — `Mock<IQfcDatamodel>` on
  `_parent.DataModel`; verify call order and that a timed-out quiesce still proceeds.
- `ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup` and
  `ButtonCancel_Click_ActionThrows_DoesNotRethrow`.
- `QfcHomeControllerCleanupTests`: `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup`,
  `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted`.
- `QfcDatamodelTeardownTests`: `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing`
  (fails before with the exact `ArgumentException` from the log),
  `QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout`,
  `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` (both driven by
  `RemainingEmailLoader` + `FakeTimeProvider`), `Cleanup_CalledTwice_DoesNotThrow`.

Not proposed: any test of `RibbonController.ReleaseQuickFiler` itself. It is `private`, has no seam,
and the guarantee that matters is expressible at the `QfcHomeController.ParentCleanup` boundary.

## Logging Plan

All lines through the existing `log4net.ILog` idiom on the class (no new logger shape). Levels chosen
so a normal Cancel is readable at INFO and diagnosis is available at DEBUG.

| Stage | Level | Content |
| --- | --- | --- |
| Gate launch | DEBUG | cutoff (per-mille and fraction), quantity, checkpoint interval, scan cap, ceiling |
| Gate checkpoint | DEBUG | accepted, scanned, cutoff, elapsed, remaining cap/ceiling, decision (continue / stop) |
| Cancel entry | INFO | trigger (button vs. completion path), token already cancelled? |
| Token cancelled | DEBUG | — |
| Keyboard flag reset | DEBUG | previous `KbdActive` value |
| Focus parked / selectors cancelled | DEBUG | whether a WebView2 held focus; item count cancelled |
| Handlers unregistered | DEBUG | navigation ledger drained, form handlers removed |
| Loader quiesce | INFO | completed vs. timed out, elapsed, bound |
| Datamodel cleanup | DEBUG | — |
| Groups cleanup | DEBUG | rows removed |
| Release callback invoked | INFO | — |
| Any stage exception | ERROR | stage name + exception (`logger.Error(message, e)`) |

## Automation Feasibility

Automatable, deterministically, with no Outlook process:

- Every AC1 behavior: the gate takes `Func<MailItem>`, a score-loader delegate, a `TimeProvider` and a
  `debugLog` delegate through its constructor, and `MailItem` is mocked with Moq throughout the
  existing suite. Continuation past the checkpoint, first-acceptance return, exhaustion, cap, ceiling
  and both log lines are all assertable headlessly.
- The AC2 *ordering* and *exception-safety* properties: handler unregistration before row removal,
  quiesce before cleanup, `KbdActive` reset, park-focus invocation, selector cancellation, and
  `ParentCleanup` under `finally` are all observable through `Mock<IQfcFormViewer>`,
  `Mock<IQfcCollectionController>`, `Mock<IQfcHomeController>` and `Mock<IQfcDatamodel>` with
  invocation-order verification — the pattern `QfcFormControllerDeactivateTests` already uses.
- The loader-crash regression: reproducible exactly, because the failing construction
  (`QfcDatamodel.cs:355-359`) depends only on private fields a test can null.

Requires a human with a live Outlook process (the manual evidence note AC2 asks for):

- That the Outlook keyboard is actually usable after Cancel. The mechanism identified by #677 is
  WebView2 runtime focus retention (WebView2Feedback #951), which is a runtime behavior of real
  browser child windows on Outlook's shared UI thread; no mock reproduces it. A unit test can prove
  `ParkFocusOffWebView2()` was called, not that focus moved.
- That the breadcrumb `ToolStripDropDown` is really closed and WinForms modal menu mode has exited.
- That no `Delegate to an instance method cannot have null 'this'` error follows a real Cancel, and
  that the new Cancel-stage log lines appear in `TaskMaster\bin\Debug\logs\debug_<date>.log` in the
  documented order.
- End-to-end AC1 confirmation against a real Explorer view whose first ~40 items score below cutoff,
  including that the pre-UI wait remains tolerable with real scoring throughput (~2-3 items/s
  observed), and that the progress band advances during the extended scan.
- Relaunch-after-Cancel behavior (both ribbon buttons functional), because `_quickFilerLoaded` lives in
  the VSTO ribbon controller.

Recommended manual evidence artifact:
`docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/evidence/regression-testing/live-outlook-cancel-teardown.<timestamp>.md`,
carrying the log excerpt with the teardown stages and the absence of the loader error, following the
#677 precedent.

## Provenance and Unknowns

- Every file:line citation above was read in this session from the worktree at `7c8ac9ae`. No shell
  command, build, or test run was performed (tooling restricted to read/search for this task).
- The recommended default bounds (250 scanned candidates, 120 s ceiling) are engineering proposals
  derived from the observed ~2-3 items/s throughput reported in the issue; they are not measured in
  this session and should be confirmed during live verification.
- Unknown: whether the 09:05 keyboard lock cleared on Escape, on focus change, or only on restart
  (the issue records the user could not reproduce it). The Cancel-stage logging added by AC2 is what
  makes a future occurrence diagnosable.
- Unknown: whether any consumer outside this repository reads `QfcDequeueStop`; `IQfcDatamodel` is
  public, but no other project in the solution references the enum in the searches performed here.
