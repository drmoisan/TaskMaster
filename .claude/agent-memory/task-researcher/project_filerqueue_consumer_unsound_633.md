---
name: filerqueue-consumer-unsound-633
description: "Issue #633: FilerQueue.Consumer has an orphaned-item race so it cannot be used as a quiesce primitive; QFC form-controller move tests are vacuous; UiThread.Dispatcher is null in the test process"
metadata:
  type: project
---

Issue #633 (QFC unsynchronized undo handoff after batch move) research, 2026-08-31.

**Fact 1 — `QuickFiler/Controllers/FilerQueue.cs` `Consumer` is NOT a usable quiesce primitive.**
`Enqueue` does `Queue.Add(item)` and *then* reads the `ThreadSafeSingleShotGuard`; the worker exits its
`while (Queue.TryTake(...))` loop and *then* installs a fresh guard. A producer that lands between those
two worker statements adds an item, sees the still-tripped old guard, starts no worker — and the item is
orphaned with `Consumer` already completed. There are also a stale-reference window (`Consumer =
ConsumeAsync()` assigns after `Task.Run` has started) and a worker-overlap window. Any fix that just
inserts `await FilerQueue.Consumer` installs a barrier that is only usually right.

**Why:** the two existing `await _parent.FilerQueue.Consumer` sites in
`QfcFormController.EventHandlers.cs` look like a ready-made quiesce and are the obvious cheap fix.

**How to apply:** for any QFC filing/undo ordering work, treat `Consumer` as a diagnostic handle only.
A correct barrier needs an outstanding-work counter incremented in `Enqueue` and decremented after each
item, plus a repaired consumer start/stop handshake under one monitor. The identical shape exists in
`TaskVisualization/FlagChangeTrainingQueue.cs` — a different type, same latent race.

**Fact 2 — the existing `BackGroundMoveAsync` / `MoveAndIterate` tests in
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` are vacuous.** The controller is built without
`Init()`, so `_groups` is null and both methods hit their null guard and return before any behaviour.
Do not cite them as coverage; do not assume a change is exercised because they pass.

**Fact 3 — `UtilitiesCS.UiThread.Dispatcher`'s getter returns the raw static with no lazy Init, so it is
`null` in a bare test process,** and `UiThreadDispatcherFixture.EnsureDispatcher()` installs a
deliberately **non-pumping** parked dispatcher. Awaiting anything routed through
`UiThread.Dispatcher.InvokeAsync` requires
`QfcItemControllerTestSupport.StartRunningDispatcher()` installed via
`UiThreadDispatcherFixture.BeginTransactionAsync()` + `Install(...)`.

**Fact 4 — `IFilerHomeController.FilerQueue` is typed as the concrete class and every `FilerQueue`
member is non-virtual, so Moq cannot substitute it.** The established repo workaround is to hand the
mock a *real* `FilerQueue` and reflect into its private `guard` field
(`QfcItemController.SeamFactoryTests.cs`). An `internal Func<FilerQueueItem, Task> ItemProcessor` seam
in the `UndoItemProcessor` / `MoveFailureNotifier` idiom removes the need for both the reflection and
an interface extraction.

Related: [[qfc-collection-controller-defects-468]], [[qfc-item-controller-defects-484]],
[[uithread-dispatcher-restore-scope-493]].
