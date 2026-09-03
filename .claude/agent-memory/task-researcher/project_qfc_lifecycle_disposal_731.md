---
name: qfc-lifecycle-disposal-731
description: "#731 research: sharing one EmailMoveMonitor would DROP move actions (FirstOrDefault dispatch); `volatile` on the reentrancy counter is a CS0420 build break; QfcRemainingQueueAdmission has TWO dead ctor params"
metadata:
  type: project
---

Issue #731 (consolidates #620/#621/#622/#634/#683), researched 2026-09-02. Four non-obvious findings
that contradict the issue's own proposed fixes.

**1. Sharing one `IEmailMoveMonitor` is a behaviour regression, not a cleanup.**
`EmailMoveMonitor.BeforeItemMove` (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:212-218`) resolves
`_hookedItems.FirstOrDefault(x => x.Mail.EntryID == mail.EntryID)` and runs **one** action. The same
`MailItem` is hooked by three owners with three *different* actions (`QfcDatamodel` ->
`_masterQueue.Remove`, `QfcCollectionController` -> `RemovedItemMonitor`, `QfcQueue` -> `RemoveItem`).
Three separate monitors = three `folder.BeforeItemMove` subscriptions = all three actions fire. One
shared monitor = one subscription = two actions silently lost. `UnhookAll` is also instance-scoped
(two call sites: `QfcDatamodel.cs:80` and `QfcCollectionController.cs:751`), so sharing lets one
owner's teardown unhook another's items. Take the issue's second option (document, don't share).
Secondary blocker: `IEmailMoveMonitor` is `internal` while all three owner classes and their
interfaces are `public` -> CS0051 on any ctor param; the repo's own workaround shape is an
`internal { get; set; }` property seam (as `QfcDatamodel.TimeProvider`).

**2. `volatile` on `removespecificcontrolgroupcounter` would break the build.** The field is passed by
`ref` to `Interlocked.Increment`/`Decrement` (`QfcCollectionController.cs:913`/`:1008`), so `volatile`
emits **CS0420** at both sites, and the repo's step-3 gate runs `/p:TreatWarningsAsErrors=true` with
no `NoWarn` in either QuickFiler csproj. Use `Volatile.Read(ref …)` at the single read site (`:991`).
In-assembly precedent for exactly this shape (int field + Interlocked writes + Volatile.Read guard):
`QuickFiler/Viewers/WebView2Messenger.cs:25/:75/:127`.

**3. `QfcRemainingQueueAdmission` has TWO dead ctor params, not one.** The issue names `scoreLoader`;
`IApplicationGlobals globals` (`:16`) is equally never stored or used. Also: the dead `scoreLoader` is
*deliberate* #233 design (scoring moved to the dequeue-time gate) and its non-invocation is pinned by
`QfcDatamodelTests.TryQueueRemainingMailItemAsync_HighConfidenceEnabled_IgnoresThresholdAtAdmission`
(`:76-95`). Removing the param destroys that pin — replace it with a structural assertion, don't just
delete it.

**4. `QfcFormController.Cleanup()` runs on the UI thread**, from its single production caller
`ActionCancelAsync()` (`QfcFormController.EventHandlers.cs:93`, right after
`await _formViewer.UiSyncContext`). So a blocking `_undoConsumerTask.Wait(timeout)` deadlocks against
`ProcessUndoItemAsync`'s `UiThread.Dispatcher.InvokeAsync` (`Actions.cs:255`). Use
`CompleteAdding()` (the stop signal the loop already reads at `Actions.cs:322`) + deferred `Dispose()`
on a continuation that observes the fault. Today's dispose-under-TryTake throws `ObjectDisposedException`
(MS docs declare it on all four `TryTake` overloads; `Dispose` is documented "not thread-safe"), which
faults an unawaited `Task.Run` task and is dropped silently — no
`<ThrowUnobservedTaskExceptions>` anywhere in the repo.

**Why:** these four all invert what the issue text proposes, so a planner working from the issue body
alone would land a regression (1), a build break (2), a lost test pin (3), or a UI deadlock (4).

**How to apply:** re-read this before planning any QuickFiler move-monitor consolidation, any
"just mark it volatile" concurrency fix, or any `Cleanup()`-blocks-on-a-task teardown fix.

Ceiling notes: `QfcFormControllerSeamTests.cs` is **497/500 lines** (new undo tests need a new file);
`QfcCollectionController.cs` is 2327 lines (pre-existing debt, no split); `QuickFiler.Test.csproj` is
a legacy non-SDK project so every new `.cs` needs a `<Compile Include>` entry. No
`docs/features/active/*-683` folder exists — #683 is promoted-potential only.

Related: [[qfc-high-confidence-dual-pipeline]] (the #233 admission-never-scores design),
[[unobserved-task-fault-670]] (same unobserved-fault class), [[qfc-collection-controller-defects-468]].
