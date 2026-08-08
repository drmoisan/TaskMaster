# Research: `QuickFiler/Controllers/QfcQueue.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/QfcQueue.cs` (610 lines, verified by direct read)
- Evidence basis: direct read of the file on disk in this worktree; direct read of
  `QuickFiler.Test/Controllers/QfcQueueTests.cs`, `QfcQueuePurePathsTests.cs`,
  `QfcQueueCoverageExpansionTests.cs`; grep across `QuickFiler.Test` for constructor/seam usage;
  read of `QuickFiler/Interfaces/IFilerHomeController.cs`, `QuickFiler/Controllers/QfcHomeController.cs`,
  `QuickFiler/Helper Classes/ItemViewerQueue.cs`, `QuickFiler/Viewers/ItemViewer.cs`.

## Current structure

- Declaration: `public class QfcQueue(CancellationToken token, QfcHomeController homeController, IApplicationGlobals appGlobals) : IQfcQueue` — a C# 12 primary constructor. Public surface implements `IQfcQueue` (see `IQfcQueue.cs` research). `_homeController` is typed as the **concrete** `QfcHomeController`, not an interface, even though `QfcHomeController` also implements `IFilerHomeController`.
- Constructor-injected: `CancellationToken token`, `QfcHomeController homeController` (concrete, not interface), `IApplicationGlobals appGlobals` (interface — already an established seam; existing tests pass `new Mock<IApplicationGlobals>().Object`).
- Newed-up (not injected): `_queue` (`BlockingCollection<...>`, field initializer), `_moveMonitor = new EmailMoveMonitor()` (field initializer; `IEmailMoveMonitor` interface already exists but the field is not constructor-injected — existing tests override it via reflection, see below).
- No direct `Microsoft.Office.Interop.Outlook.Application`/`Store`/`MAPIFolder` dependency. `MailItem` appears only as a parameter/data type flowing through (`RemoveItem(MailItem)`, `EnqueueAsync(IList<MailItem>, ...)`, `AddAsync(..., MailItem, ...)`); it is the COM interface type, which the whole test suite already mocks via `new Mock<MailItem>()`. This is not the UT2 exemption case (no *construction* of live Outlook objects).
- Concurrency/ordering constructs: `BlockingCollection<(TableLayoutPanel, List<QfcItemGroup>)> _queue`; `Interlocked.Increment/Decrement/Exchange` on `_jobsRunning`; `CancellationTokenSource` linking in `CompleteAddingAsync`/`TryDequeueAsync`; polling loops using `Task.Delay(100/pollInterval, token)` (real `Task.Delay`, not an injected `TimeProvider` — see Determinism below); `UiThread.Dispatcher.InvokeAsync(..., DispatcherPriority.ContextIdle)` for UI-thread marshaling in `UiIdleCallAsync`/`UiIdleAsyncCallAsync`.
- No RNG. No direct wall-clock reads (`DateTime.Now` etc.) in this file.
- File is organized in four `#region`s: "Constructors and Private Members" (lines 20-42), "Queue Functions" (44-291: `CompleteAddingAsync`, `Dequeue`, `TryDequeueAsync`, `RemoveItem`, `EnqueueAsync`, `JobsToFinish`, `Count`, `JobsRunning`), "Tlp Manipulation" (293-556: `TlpTemplate`, `ActivateTlpTemplate`, `TlpStates`, `AddAsync`, `AddViewerToTlp`, `AdjustTlp`, `LoadControllersViewersAsync`, `ChangeIterationSize`, `RenumberGroups`, `GrowEntry`), "INotify" (558-573: `NotifyPropertyChanged`, `CollectionChanged`, `PropertyChanged` events), "Helper Methods" (575-608: `UiIdleCallAsync` x2 overloads, `UiIdleAsyncCallAsync`).

## Existing test coverage

Three dedicated test files exist:

- `QfcQueueTests.cs` (68 lines, 1 test): `RemoveItem_WhenTokenPreCancelled_DoesNotThrow` — covers the pre-cancelled-token guard branch inside `RemoveItem` (via `JobsToFinish`'s `OperationCanceledException` catch added by an earlier fix).
- `QfcQueuePurePathsTests.cs` (137 lines, 5 tests): `NewQueue_HasZeroCountAndZeroJobsRunning` (Count/JobsRunning getters); `TryDequeueAsync_EmptyQueueNoJobs_ReturnsDefault` (early-return branch, `_queue.Count == 0 && _jobsRunning == 0`); `CompleteAddingAsync_NoJobsRunning_CompletesWithoutThrowing`; `JobsToFinish_NoJobsRunning_CompletesImmediately`; and `DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue`, which is **misfiled** — it exercises `QfcDatamodel.DequeueNextItemGroupAsync`, not any member of `QfcQueue`. It contributes zero coverage to this file.
- `QfcQueueCoverageExpansionTests.cs` (291 lines, 8 tests): `Dequeue_WithQueuedEntry_UnhooksItemsRaisesRemoveAndUpdatesCount`; `TryDequeueAsync_WithCompletedPendingEntry_UnhooksItemsAndRaisesRemove`; `TryDequeueAsync_WithRunningJobAndCancellation_ReturnsDefault`; `CompleteAddingAsync_WhenFunctionTimeoutExpires_ThrowsAndLeavesQueueOpen`; `Dequeue_WithHighConfidenceCarrier_PreservesPredeterminedFolder`; `AdjustTlp_WhenRowsIncrease_GrowsRowCountAndMinimumHeight`; `RenumberGroups_WithTenItems_UsesTwoDigitNumbersAndSequentialIndexes`; `GrowEntry_WhenTargetHasCapacity_MovesControlAndGroupThenResetsSourceState`.

Members with real coverage today: `Count`, `JobsRunning`, `Dequeue()` (queued + high-confidence-carrier variants), `TryDequeueAsync` (empty/no-jobs, completed-pending-entry, running-job+cancellation), `CompleteAddingAsync` (no-jobs success, timeout-throws), `JobsToFinish` (no-jobs), `RemoveItem` (pre-cancelled-token guard only), `AdjustTlp`, `RenumberGroups`, `GrowEntry`.

Test-side seam pattern already established and reusable for this file's remaining gaps: reflection-based private-field injection (`SetPrivateField` on `_queue`, `_moveMonitor`, `_jobsRunning`) and a `NewQueue()` helper that passes `(QfcHomeController)null` for the concrete constructor parameter, since none of the existing tests need `_homeController` populated.

## Coverage gap

Not exercised by any existing test:

- `RemoveItem` — the success path (a matching entry found, `UiIdleCallAsync` invoked, row removed, `RenumberGroups` called, `_jobsRunning` incremented/decremented) and the non-matching-entry path. Only the pre-cancelled-token early-return is covered.
- `EnqueueAsync` — entirely uncovered: null-items guard, empty-items guard, the hook-and-hydrate happy path, the `OperationCanceledException` swallow branch, the generic-exception logged branch, and the `finally` block's `Interlocked.Decrement` + `CollectionChanged` raise.
- `ChangeIterationSize` — entirely uncovered (grow/shrink across multiple queue entries, the trailing dequeue-from-datamodel branch, the "discard duplicate top element" step).
- `AddAsync`, `AddViewerToTlp` — entirely uncovered (both depend on `ItemViewerQueue.Dequeue`, a static call, and on constructing/manipulating a concrete `ItemViewer`).
- `LoadControllersViewersAsync` — entirely uncovered; constructs `QfcItemController` directly (`new QfcItemController(...)`) and calls `InitializeAsync()`.
- `ActivateTlpTemplate` — trivial no-op body (all lines commented out); not exercised, but there is nothing to assert beyond "does not throw."
- `TlpTemplate` property setter/getter — not exercised (the setter clones the incoming panel and renames it).
- `NotifyPropertyChanged` — not exercised (no production caller currently raises it either, per this file; it exists for the `INotifyPropertyChanged` contract).
- `UiIdleCallAsync` (both overloads) and `UiIdleAsyncCallAsync` — not exercised directly; they are covered only as pass-through wrappers, if the WPF `Dispatcher`-marshaled callers above are exercised.

## `[ExcludeFromCodeCoverage]` disposition

Not applicable — this file carries no such attribute.

## Seam requirements

Per the seam hierarchy (interface seam > injectable delegate > adapter):

1. **`ItemViewerQueue.Dequeue` static call (`QfcQueue.cs:336`, inside `AddAsync`).** `ItemViewerQueue` is a `public static class` owned by F4 (`quickfiler-helper-classes-coverage`); F2 must not modify it. Recommended seam, scoped entirely inside `QfcQueue.cs`: add a private field `private Func<CancellationToken, ItemViewer> _itemViewerFactory = ItemViewerQueue.Dequeue;` (method-group default, same behavior as today) and route `AddAsync` through it. This is an injectable-delegate seam that lets tests substitute a lightweight/pre-built `ItemViewer` without touching the real pool, and does not change `ItemViewerQueue.cs` or cross into F4's file set.
2. **`QfcItemController` direct construction (`QfcQueue.cs:405-414`, inside `LoadControllersViewersAsync`).** Constructed directly with `new QfcItemController(...)`. A full seam (factory delegate for the controller) would be a larger change touching a file owned by F10 (`quickfiler-item-controller-coverage`); recommend leaving this as-is for F2 and treating `LoadControllersViewersAsync`'s controller-construction line as part of the file's irreducible remainder unless F1's ledger or a later coordination step says otherwise.
3. **`_homeController` typed as concrete `QfcHomeController` instead of `IFilerHomeController`.** `IFilerHomeController.DataModel` is commented out (`QuickFiler/Interfaces/IFilerHomeController.cs:29`), so the interface cannot substitute for `ChangeIterationSize`'s need for `_homeController.DataModel`. Do **not** widen `IFilerHomeController` from F2 — that interface is consumed across F6/F7/F10 and widening it is exactly the kind of cross-child contract change the epic's decomposition rationale warns against. Instead, use the established uninitialized-object + reflection pattern already proven in this codebase (`QfcDatamodelTests.CreateUninitializedDatamodel`, `QfcQueuePurePathsTests.CreateUninitializedDatamodel`): construct `QfcHomeController` via `FormatterServices.GetUninitializedObject(typeof(QfcHomeController))`, then reflectively set its private `_datamodel` field (`QfcHomeController.cs:428`) to a `Mock<IQfcDatamodel>.Object`, then pass that instance as `homeController` to `QfcQueue`'s constructor. `QfcHomeController.DataModel` is a plain field-backed getter (`QfcHomeController.cs:429-431`), so this is safe and does not execute any COM-bound constructor logic. No production change required for this seam.
4. **`_moveMonitor` field-initialized instead of constructor-injected.** Existing tests already override it by reflection (`SetPrivateField(queue, "_moveMonitor", moveMonitor.Object)`), which works today. No change required; noted only so the atomic-planner does not attempt an unnecessary constructor-injection refactor.
5. **`UiThread.Dispatcher` static WPF dispatcher.** `UiIdleCallAsync`/`UiIdleAsyncCallAsync` marshal through `UtilitiesCS.UiThread.Dispatcher`, a static property. `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` already provides `EnsureUiThreadDispatcher()`, `StartRunningDispatcher()`, and `ShutdownDispatcher()` helpers that seed a dedicated background `System.Windows.Threading.Dispatcher` for exactly this static seam. Reuse that existing test-support helper rather than inventing a new one; it is the established precedent in this codebase (also used by `EmailMoveMonitorTests.cs`).
6. **`ItemViewer` construction for `AddViewerToTlp`.** `ItemViewer : UserControl, IItemViewer, ...` (`QuickFiler/Viewers/ItemViewer.cs:21`) — a `UserControl`, not a `Form`. Per this repository's prior research (issue #227), headless (never-shown) `ItemViewer` construction is already confirmed safe, following the `ProgressPane` precedent. `AddViewerToTlp` only manipulates WinForms layout properties (`Parent`, `SetCellPosition`, `SetColumnSpan`, `AutoSize`, `Dock`) on an already-obtained `ItemViewer`; a real, never-shown instance obtained via the delegate seam in item (1) is appropriate here under the STA last-resort clause (WinForms **control**, not a shown form). Any such test must live in a dedicated `*.StaTests.cs` file per the epic's STA convention, and must document why no pure seam suffices (the `ItemViewer.Parent`/`SetCellPosition` calls require a live WinForms control instance).

## 500-line compliance (partial-class split)

`QfcQueue.cs` is 610 lines, 110 over the 500-line limit. This codebase's established partial-class naming convention is `<TypeName>.<Concern>.cs` in the same folder and namespace (`QfcItemController.Initialization.cs`, `QfcItemController.ViewerSetup.cs`, `QfcItemController.FolderHandling.cs`, `QfcFormController.EventHandlers.cs`, `QfcFormController.Actions.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`).

Proposed split, no behavior change:

- **`QfcQueue.cs` (retained, primary constructor + core lifecycle).** Keep: `using` directives, class declaration with the primary constructor (`public partial class QfcQueue(...) : IQfcQueue`), the "Constructors and Private Members" region, the entire "Queue Functions" region (`CompleteAddingAsync`, `Dequeue`, `TryDequeueAsync`, `RemoveItem`, `EnqueueAsync`, `JobsToFinish`, `Count`, `JobsRunning`), the "INotify" region, and the "Helper Methods" region (`UiIdleCallAsync` x2, `UiIdleAsyncCallAsync`). Estimated size: ~19 (usings/namespace) + ~13 (ctor block) + ~250 (Queue Functions) + ~16 (INotify) + ~34 (Helper Methods) + closing braces ≈ 335-350 lines.
- **`QfcQueue.TlpManipulation.cs` (new partial).** Move the entire "Tlp Manipulation" region verbatim: `_tlpTemplate` field, `TlpTemplate` property, `ActivateTlpTemplate`, `_tlpStates` field, `TlpStates` property, `AddAsync`, `AddViewerToTlp`, `AdjustTlp`, `LoadControllersViewersAsync`, `ChangeIterationSize`, `RenumberGroups`, `GrowEntry`. Declared as `public partial class QfcQueue` (no primary-constructor parameter list — only one partial declaration may carry it, matching the existing `QfcItemController`/`QfcDatamodel` precedent where only the file with the constructor declares parameters). Estimated size: ~10 (usings/namespace/class wrapper) + ~264 (region body) + closing braces ≈ 280-290 lines.

Both resulting files are comfortably under 500 lines. No member moves across assembly/namespace boundaries; `internal`/`private` members remain visible to both partials because they are the same type. Required `using` directives for the new file: `System`, `System.Collections.Generic`, `System.Linq`, `System.Windows.Forms`, `Microsoft.Office.Interop.Outlook`, `QuickFiler.Helper_Classes` (for `ItemViewerQueue`/`ItemViewer` types), `QuickFiler.Interfaces`, `UtilitiesCS` — a subset of the current file's usings; the `static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox` using and `System.Collections.Concurrent`/`System.ComponentModel`/`System.Diagnostics`/`System.Text`/`System.Threading` are only needed by the retained "Queue Functions" file and should not be duplicated into the new file unless actually referenced there.

## Candidate test cases

| # | Case | Type | Notes |
|---|---|---|---|
| 1 | `RemoveItem` with a matching entry present removes the row, renumbers remaining groups, unhooks the removed item, and raises `CollectionChanged` | Positive | Reuses `SetPrivateField` + mocked `IEmailMoveMonitor` pattern from `QfcQueueCoverageExpansionTests` |
| 2 | `RemoveItem` with no matching entry leaves the queue content unchanged and still raises `CollectionChanged` once per re-added entry | Positive/edge | Exercises the `Any(...)` false branch |
| 3 | `EnqueueAsync` with `null` items throws `ArgumentNullException` | Negative | Guard clause at top of method |
| 4 | `EnqueueAsync` with an empty item list throws `ArgumentException` | Negative | Guard clause |
| 5 | `EnqueueAsync` happy path hooks every item via the move monitor, increments then decrements `_jobsRunning`, adds one queue entry, and raises `CollectionChanged` with `Add` | Positive | Requires the dispatcher seam (item 5 above) and the item-viewer-factory seam (item 1 above) |
| 6 | `EnqueueAsync` when `LoadControllersViewersAsync` throws `OperationCanceledException` is swallowed without rethrow, `_jobsRunning` still decremented in `finally` | Error-handling | Assert via injected controller/viewer factory throwing |
| 7 | `EnqueueAsync` when `LoadControllersViewersAsync` throws a generic exception is logged and swallowed, `_jobsRunning` still decremented | Error-handling | |
| 8 | `ChangeIterationSize` growing row count across two existing entries moves controls/groups via `GrowEntry` and appends a final dequeue-backed entry when short | Positive/state-transition | Uses the uninitialized-`QfcHomeController` + reflection seam (item 3) with a mocked `IQfcDatamodel.DequeueNextItemGroupAsync` |
| 9 | `ChangeIterationSize` when the datamodel dequeue returns zero items still discards the duplicate top element and completes | Boundary | |
| 10 | `TlpTemplate` setter clones the assigned panel and renames it to `"TemplateTableLayout"`, leaving the original panel's name unchanged | Positive | Pure WinForms property assertion, no dispatcher needed |
| 11 | `ActivateTlpTemplate` does not throw for any panel argument (documents the current no-op) | Boundary | |
| 12 | `NotifyPropertyChanged` raises `PropertyChanged` with the caller member name when a subscriber is attached, and is a no-op when no subscriber is attached | Positive/edge | Direct call via `[CallerMemberName]` default |
| 13 | `AddAsync` obtains a viewer through the injected factory, assigns it to the returned `QfcItemGroup`, and calls `AddViewerToTlp` on the UI-idle dispatcher | Positive | STA last-resort + dispatcher seam; document why no further seam is feasible |
| 14 | `AddViewerToTlp` sets `Parent`, cell position/column span, `AutoSize`, `AutoSizeMode`, `Dock`, and `BorderStyle` on the viewer exactly once | Positive | Real headless `ItemViewer` per the #227 precedent |
| 15 | `UiIdleCallAsync(Action)` and `UiIdleCallAsync<T>(Func<T>)` execute the delegate on the dedicated dispatcher and return its result | Positive | Uses `QfcItemController.TestSupport`'s dispatcher helpers |
| 16 | `UiIdleAsyncCallAsync<T>` awaits the inner task and yields before returning | Positive/concurrency | Assert ordering with a `TaskCompletionSource`-gated inner func |

## Determinism constraints

- `EnqueueAsync`'s `Task.Run(...)` for hooking items and the WPF-dispatcher-marshaled calls must not rely on real thread scheduling races in assertions; use `TaskCompletionSource`/explicit `await` ordering, never `Thread.Sleep`.
- `JobsToFinish`/`CompleteAddingAsync` poll with real `Task.Delay(100, token)` / `Task.Delay(pollInterval, token)` — **not** an injected `TimeProvider**. Existing tests avoid the wait by keeping `_jobsRunning == 0` or by forcing a fast `OperationCanceledException`/timeout. Any new test that must wait through a non-zero `_jobsRunning` polling loop should drive it via cancellation (as `QfcQueueCoverageExpansionTests.TryDequeueAsync_WithRunningJobAndCancellation_ReturnsDefault` already does with a short real `CancelAfter`) rather than waiting out real milliseconds; this file has no `TimeProvider` seam today and introducing one for the 100ms poll intervals is out of scope for a coverage-only child (no behavior change). Keep any necessarily-real waits under ~50ms as the existing tests already do.
- No RNG is used in this file; no seeded-RNG requirement.
- The WPF `Dispatcher` used via `UiThread.Dispatcher` must be the dedicated background dispatcher from `QfcItemController.TestSupport`, never the test-runner's own thread, to avoid deadlocks with `DispatcherPriority.ContextIdle` posts.
