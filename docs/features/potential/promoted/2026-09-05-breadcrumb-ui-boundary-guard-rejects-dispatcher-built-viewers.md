# breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers (Issue #781)

- Date captured: 2026-09-05
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers/ (Issue #781)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #781
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/781
- Last Updated: 2026-09-05
## Summary

Launching QuickFiler in high-confidence mode fails with an unhandled `InvalidOperationException` from `ItemViewer.ThrowIfOffUiBoundary` ("InitializeBreadcrumbPipeline must be called on the thread that owns this ItemViewer"). The guard added by issue #488 defect D4 compares `SynchronizationContext.Current` by reference against the context captured in the `ItemViewer` constructor. Every production `ItemViewer` is constructed inside a WPF `Dispatcher.Invoke` operation (through `ItemViewerQueue` and `UiThread.Dispatcher`), and WPF installs a `DispatcherSynchronizationContext` for the duration of each dispatcher operation. The viewer therefore captures a context that is never the thread's ambient context again, so the guard rejects the call even when it is made on the UI thread.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, Outlook VSTO add-in, .NET Framework 4.8 (CLR 4.0.30319.42000)
- Python version: n/a
- Command/flags used: Ribbon button `QuickFilerHighConfidence_Click` -> `RibbonController.LoadQuickFilerHighConfidenceAsync` -> `QfcHomeController.LaunchAsync`; run under the Visual Studio debugger
- Data source or fixture: live Outlook mailbox with at least one item that produces folder suggestions (`_folderHandler.FolderArray.Length > 0`)

## Steps to Reproduce

1. Start Outlook with the TaskMaster add-in on `main` at or after commit `5c841d1f` (issue #488 defect D4).
2. Click the QuickFiler high-confidence ribbon button.
3. Wait for `QfcFormController.LoadItemsAsync` to show the form and call `QfcCollectionController.LoadSecondaryAsync`.
4. The first completed folder task calls `QfcItemController.AssignFolderComboBox` -> `EnsureBreadcrumbPipeline` -> `ItemViewer.InitializeBreadcrumbPipeline`, which throws from `ThrowIfOffUiBoundary`.

## Expected Behavior

`InitializeBreadcrumbPipeline` succeeds when it is called on the thread that owns the `ItemViewer`, regardless of which `SynchronizationContext` instance happens to be ambient at the call site (the persistent `WindowsFormsSynchronizationContext` of the UI thread, a `DispatcherSynchronizationContext` installed by a WPF dispatcher operation, or a WinForms context copy). The guard still rejects a genuine cross-thread call. The breadcrumb pipeline initializes, folder suggestions populate, and QuickFiler opens.

## Actual Behavior

The launch chain terminates with the exception below. Because `RibbonViewer.QuickFilerHighConfidence_Click` is `async void` and its builder captured no synchronization context, the exception is rethrown on a thread-pool work item (`AsyncMethodBuilderCore.ThrowAsync` -> `ThreadPool.QueueUserWorkItem`), which is what the bottom frames of the trace show.

```
System.InvalidOperationException
  HResult=0x80131509
  Message=InitializeBreadcrumbPipeline must be called on the thread that owns this ItemViewer. The current synchronization context is not the one captured when the viewer was constructed.
  Source=QuickFiler
  StackTrace:
   at QuickFiler.ItemViewer.ThrowIfOffUiBoundary(String operation) in QuickFiler\Viewers\ItemViewer.Breadcrumb.cs:line 436
   at QuickFiler.ItemViewer.InitializeBreadcrumbPipeline(IFolderHierarchyProvider provider, BreadcrumbPopupUiOperations operations) in QuickFiler\Viewers\ItemViewer.Breadcrumb.cs:line 51
   at QuickFiler.ItemViewer.InitializeBreadcrumbPipeline(IFolderHierarchyProvider provider) in QuickFiler\Viewers\ItemViewer.Breadcrumb.cs:line 44
   at QuickFiler.Controllers.QfcItemController.EnsureBreadcrumbPipeline() in QuickFiler\Controllers\QfcItemController.ViewerSetup.cs:line 150
   at QuickFiler.Controllers.QfcItemController.AssignFolderComboBox() in QuickFiler\Controllers\QfcItemController.FolderHandling.cs:line 206
   at QuickFiler.Controllers.QfcCollectionController.<LoadSecondaryAsync>d__66.MoveNext() in QuickFiler\Controllers\QfcCollectionController.cs:line 548
   at QuickFiler.Controllers.QfcFormController.<LoadItemsAsync>d__84.MoveNext() in QuickFiler\Controllers\QfcFormController.Actions.cs:line 161
   at QuickFiler.Controllers.QfcFormController.<LoadItemsAsync>d__83.MoveNext() in QuickFiler\Controllers\QfcFormController.Actions.cs:line 117
   at QuickFiler.Controllers.QfcHomeController.<RunAsync>d__47.MoveNext() in QuickFiler\Controllers\QfcHomeController.cs:line 322
   at QuickFiler.Controllers.QfcHomeController.<LaunchAsync>d__3.MoveNext() in QuickFiler\Controllers\QfcHomeController.cs:line 69
   at TaskMaster.RibbonController.<LoadQuickFilerHighConfidenceAsync>d__25.MoveNext() in TaskMaster\Ribbon\RibbonController.cs:line 139
   at TaskMaster.RibbonViewer.<QuickFilerHighConfidence_Click>d__24.MoveNext() in TaskMaster\Ribbon\RibbonViewer.cs:line 155
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Threading.ExecutionContext.RunInternal(...)
   at System.Threading.QueueUserWorkItemCallback.System.Threading.IThreadPoolWorkItem.ExecuteWorkItem()
   at System.Threading.ThreadPoolWorkQueue.Dispatch()
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: runtime probe executed 2026-09-05 in Windows PowerShell 5.1 (.NET Framework 4.8, STA), installing a `WindowsFormsSynchronizationContext` as the ambient context and then invoking callbacks through `Dispatcher.CurrentDispatcher`:

```
CLR: 4.0.30319.42000  ApartmentState: STA
ReuseDispatcherSynchronizationContextInstance: True
outer ambient type            : System.Windows.Forms.WindowsFormsSynchronizationContext
inside Dispatcher.Invoke type : System.Windows.Threading.DispatcherSynchronizationContext
inside InvokeAsync type       : System.Windows.Threading.DispatcherSynchronizationContext
Invoke ctx == outer ambient   : False
Invoke#1 ctx == Invoke#2 ctx  : True
InvokeAsync ctx == Invoke#1   : True
ambient after ops == outer    : True
WinForms CreateCopy == self   : False
```

The decisive line is `Invoke ctx == outer ambient : False`. Whether the dispatcher context instance is reused across operations depends on `BaseCompatibilityPreferences.ReuseDispatcherSynchronizationContextInstance` (true in this probe host; it follows the host AppDomain target framework), but under either setting the context captured inside a dispatcher operation is a different object from the UI thread's ambient WinForms context.

## Impact / Severity

- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

QuickFiler cannot open in high-confidence mode. The standard launch path (`LoadQuickFilerAsync`) reaches the same `LoadSecondaryAsync` -> `AssignFolderComboBox` -> `EnsureBreadcrumbPipeline` sequence for every viewer with folder suggestions and is expected to fail identically; every pooled viewer is built through the same dispatcher path.

## Suspected Cause / Notes

Root cause (verified by code reading and the runtime probe above):

1. `ItemViewer()` (`QuickFiler/Viewers/ItemViewer.cs` lines 23-29) runs `InitializeComponent()` and then captures `_context = SynchronizationContext.Current` and `_uiDispatcher = Dispatcher.CurrentDispatcher`.
2. Production viewers come only from `ItemViewerQueue.Dequeue` (`QuickFiler/Helper Classes/ItemViewerQueue.cs`). No production code calls `BuildQueue*`, so the queue is empty and `ViewerQueueCore.Dequeue` takes the `CreateWithPriority` path, whose production scheduler is `UiThread.Dispatcher.Invoke(action, DispatcherPriority.Render)`; the replacement viewer is built with `UiThread.Dispatcher.InvokeAsync(action, DispatcherPriority.ContextIdle)`. Both are WPF dispatcher operations.
3. WPF's `DispatcherOperation.InvokeImpl` (and the `Dispatcher.Invoke` same-thread fast path) sets `SynchronizationContext.Current` to a `DispatcherSynchronizationContext` for the duration of the callback and restores the previous context afterwards. The viewer therefore captures a `DispatcherSynchronizationContext`, not the `WindowsFormsSynchronizationContext` that is the UI thread's persistent ambient context (`QfcHomeController.LaunchAsync` installs it when absent; `QfcFormViewer` captures it at `QfcFormViewer.cs` line 23).
4. `ItemViewer.ThrowIfOffUiBoundary` (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` lines 420-436, commit `5c841d1f`, issue #488 D4) requires `ReferenceEquals(SynchronizationContext.Current, UiSyncContext)`. On the UI thread outside a dispatcher operation `Current` is the WinForms context, so the comparison fails and the guard throws. Marshalling the call through `UiDispatcher.InvokeAsync` or `await UiSyncContext` does not help either: a dispatcher callback runs under a `DispatcherSynchronizationContext` that is only the same instance when the runtime reuses it, and a WinForms `Post` callback runs under the thread's WinForms context.
5. The same guard protects `ConfigureBreadcrumbDropDown` (both overloads) and `EnsureBreadcrumbResourceOwnership`, so `InitializeWebViewAsync` and the drop-down configuration path are affected by the same comparison.

The failure is 100 percent deterministic for dispatcher-built viewers; it is not a race. Existing tests pass because `ItemViewerBreadcrumbLifecycleRegressionTests.ViewerScope` installs a plain `SynchronizationContext` and constructs the viewer under it on the test thread, which is the one shape production never produces.

Assessment of the earlier hypothesis that `LoadSecondaryAsync` continues on a thread-pool thread and `Control.InvokeRequired` returns a false negative because the viewer has no window handle:

- The bottom frames of the trace (`QueueUserWorkItemCallback` -> `ExecutionContext.Run` -> `ExceptionDispatchInfo.Throw`) are the `async void` rethrow path for a method whose builder captured a null synchronization context. They identify the thread that rethrew the unhandled exception, not the thread that executed `ThrowIfOffUiBoundary`. A debugger observation of a null context on a thread-pool thread at the unhandled-exception break is consistent with that rethrow, so it does not establish the original throw site.
- `QfcHomeController.LaunchAsync` installs a `WindowsFormsSynchronizationContext` on the launching thread before any await, and WinForms re-installs the UI thread's own context inside marshaled callbacks, so continuations of `LoadItemsAsync` and `LoadSecondaryAsync` resume on the UI thread under that context.
- `QfcFormController.LoadItemsAsync` calls `_formViewer.Show()` before `LoadSecondaryAsync`, and `LoadItemToTlp` parents each viewer into the form's live `TableLayoutPanel`, so the viewers have created handles by the time `AssignFolderComboBox` runs and `InvokeRequired` is reliable there.
- Even if a continuation did run off the UI thread, the guard would also fail on the UI thread for the reason above, so changing the call-site marshalling alone does not fix the defect.

Related observations, not in scope for the fix:

- `UtilitiesCS.UiThread.SynchronizationContextAwaiter.IsCompleted` (`UtilitiesCS/Threading/UiThread.cs` line 100) uses the same reference comparison, so `await viewer.UiSyncContext` always posts for a dispatcher-built viewer instead of continuing inline. This is an extra hop, not a failure.
- `BreadcrumbUiDispatcher.IsCurrentBoundary` (`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` lines 255-278) also compares contexts by reference, but it first accepts a currently executing dispatcher callback, and its own `Post` path always routes through the captured context, so it remains self-consistent.
- The #488 D4 remark that "a thread id is not a boundary proof" because a recycled pool thread could share the UI thread's managed id is not correct while the UI thread is alive: managed thread ids are unique among live threads.

## Proposed Fix / Validation Ideas

- [ ] Change `ItemViewer.ThrowIfOffUiBoundary` to prove thread ownership by owner-thread identity (for example `_uiDispatcher.CheckAccess()` on the `Dispatcher` captured in the constructor, or the constructing thread's managed thread id) instead of `SynchronizationContext` reference equality; keep the diagnostic message and the fail-fast contract for genuine cross-thread calls.
- [ ] Replace the ambient-context proxy tests in `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` (`InitializeBreadcrumbPipeline_AmbientNull...`, `..._DifferentNonNullContext_ThrowsBoundaryDiagnostic`) with deterministic tests that (a) construct the viewer under one context and call `InitializeBreadcrumbPipeline` on the same thread under a different or null ambient context and expect success, and (b) call it from a `Task.Run` worker and expect the boundary exception; the worker case needs no message pump because the guard throws before any control is touched.
- [ ] Add a regression test that constructs the viewer inside a `Dispatcher.Invoke` callback and initializes the pipeline afterwards under the thread's ambient WinForms or plain context, reproducing the production shape.
- [ ] Manual verification: launch QuickFiler in both standard and high-confidence mode and confirm the breadcrumb selector populates for every row without the boundary exception.
- [ ] Retest the other guarded members (`ConfigureBreadcrumbDropDown`, `EnsureBreadcrumbResourceOwnership`) through `InitializeWebViewAsync`.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
