# Part A narrowing rationale — Write-Set scope finding that fixes D4 (P1-T5)

Task: [P1-T5]
Timestamp: 2026-09-13T02-39
Command: none (read-only findings from the UtilitiesCS tooltips helper and the injected seam interface, read in the item worktree at HEAD `e362cc6f0`)
EXIT_CODE: 0
Output Summary: the tooltip factory signature, its file and line, its Write-Set status, and the six seam members are recorded below.

## The tooltip factory

Declaring file: UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs, lines 80-84.

Signature (three parameters):

```
public static async ValueTask<IQfcTipsDetails> CreateAsync(
    System.Windows.Forms.Label labelControl,
    SynchronizationContext uiContext,
    CancellationToken token
)
```

- First parameter type: `System.Windows.Forms.Label`
- Second parameter type: `SynchronizationContext`
- Third parameter type: `CancellationToken`

That file lives in the `UtilitiesCS` project and is **outside** the binding Write Set (the seven paths listed in the plan's `## Write Set` section and spec `## Write Set` lines 492-502, all of which are under `QuickFiler` or `QuickFiler.Test`). The three sites at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` lines 282, 298 and 303 pass `_itemViewer.UiSyncContext` as that second argument; converting them to the injected dispatcher seam would require changing this public UtilitiesCS API, which is out of scope. They are argument-passing sites, not marshals the controller performs.

## The injected seam interface

Declaring file: UtilitiesCS/Threading/IUiDispatcher.cs, members at lines 15-42 (`public interface IUiDispatcher` at line 15, closing brace at line 42). Exactly six members:

1. `void Invoke(Action action);` — line 18
2. `Task InvokeAsync(Action action);` — line 21
3. `Task InvokeAsync(Action action, DispatcherPriority priority, CancellationToken token);` — line 27
4. `IAsyncResult BeginInvoke(Action action);` — line 30
5. `Task<TResult> InvokeAsync<TResult>(Func<TResult> func);` — line 35
6. `Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func);` — line 41

None of these six is a context hop: every member takes a delegate and executes it on the UI thread, returning to the caller's context afterwards. The statement at ViewerSetup.cs line 287, `await itemViewer.UiSyncContext;`, is a hop after which every subsequent statement of the member runs on the UI thread; an equivalent conversion would have to wrap lines 288-328 inside a delegate, which is a restructure of a file with 33 lines of headroom that a sibling item is concurrently editing, and spec section 6.2 requires ordering equivalence to be established before the edit. The site is therefore deferred, as D4 records. Line 371 remains the single converted site.
