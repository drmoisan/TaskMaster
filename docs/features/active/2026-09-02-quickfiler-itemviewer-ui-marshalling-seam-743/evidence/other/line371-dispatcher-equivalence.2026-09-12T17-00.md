# Marshalling-equivalence finding for the single converted site (P1-T2)

Task: [P1-T2]
Timestamp: 2026-09-13T02-38
Command: none (read-only findings; every citation below was read directly in the item worktree during Phase 1, at HEAD `e362cc6f0`, before any Write Set edit)
EXIT_CODE: 0
Output Summary: five findings recorded, each with a file-and-line citation.

## Finding 1 — what line 371 does today

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, lines 371-373, inside `AssignControlsAsync` (declared at line 360):

```
await _itemViewer.UiDispatcher.InvokeAsync(() =>
    AssignControls(itemInfo, viewerPosition)
);
```

The site reads the viewer's WPF `Dispatcher` through `IItemViewer.UiDispatcher` (declared at `QuickFiler/Viewers/IItemViewer.cs` line 36) and calls `Dispatcher.InvokeAsync` with an `Action` delegate. Line 365 of the same member is a commented-out copy of the same call and is not live.

## Finding 2 — where the injected seam's production default is constructed

`QuickFiler/Controllers/QfcItemController.Initialization.cs`, line 391, inside `SaveParameters` (declared at line 357):

```
_uiDispatcher ??= new UtilitiesCS.Threading.WpfUiDispatcher();
```

The field itself is `private UtilitiesCS.Threading.IUiDispatcher _uiDispatcher;` at line 66 of the core controller partial (QfcItemController.cs).

## Finding 3 — the seam's parameterless constructor and `InvokeAsync(Action)` forward to the same primitive

`UtilitiesCS/Threading/WpfUiDispatcher.cs`:

- lines 24-25: `public WpfUiDispatcher() : this(() => UiThread.Dispatcher) { }` — the parameterless constructor resolves the dispatcher lazily through `() => UiThread.Dispatcher`;
- lines 33-37: the private provider constructor stores the delegate and the `Dispatcher` property evaluates it on each access;
- line 43: `public Task InvokeAsync(Action action) => Dispatcher.InvokeAsync(action).Task;`

On the non-null path the converted site therefore invokes the same `Dispatcher.InvokeAsync(Action)` primitive with the same delegate on the same dispatcher queue as line 371 does today; the substitution is like-for-like there.

## Finding 4 — the converted site must carry the existing seam sites' null tolerance, and why the null path is reachable

Spec section 6.2, second risk bullet, requires the same null tolerance at every converted site. The reference shape is `NotifyMoveFailure` at `QuickFiler/Controllers/QfcItemController.MailActions.cs` lines 35-46: `var dispatcher = _uiDispatcher;` (line 38), `if (dispatcher is null)` (line 39), direct call and `return;` (lines 41-42), `dispatcher.Invoke(...)` (line 45); lines 33-34 record that `_uiDispatcher` is null in the seam-factory tests.

The null path is reachable only through the parameterless harness constructor:

- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` lines 162-165 record that the parameterless `Theme` constructor path leaves `_uiDispatcher` null and that the harness deliberately does not default it;
- `QuickFiler/Controllers/QfcItemController.Initialization.cs` line 27, `protected QfcItemController() { }`, assigns nothing;
- the production path assigns the field before any control assignment: line 59 (`_uiDispatcher = uiDispatcher;` in the primary constructor, which then calls `SaveParameters` at line 67) or line 391 (the `??=` default inside `SaveParameters`); the static factories pre-assign it at lines 438 and 480 (`controller._uiDispatcher = uiDispatcher;`) and then call `SaveParameters` at lines 441 and 483, which reaches the line 391 default when the injected value is null.

## Finding 5 — the existing test that will take the null path after the conversion

`QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` lines 309-344, `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher` (`[TestMethod]` at line 308): it builds `new HarnessController()` at line 322, injects `_itemViewer` at line 323 and `_globals` at lines 324-328 only, sets up the viewer mock's `UiDispatcher` at line 321, and never injects `_uiDispatcher`. This plan never writes that file. After the P3-T2 conversion the test's controller has a null `_uiDispatcher`, so it takes the null-tolerance branch, which calls `AssignControls` directly; that member self-marshals through the viewer's `InvokeRequired`/`Invoke` pair at ViewerSetup.cs lines 379-383 (the mock returns `InvokeRequired == false` at test line 320), so the assertions at lines 337-338 continue to hold.
