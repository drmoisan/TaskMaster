# AC18 Refresh-Wiring Audit — Issue #503 (P5-T8)

Timestamp: 2026-08-08T14-22

Commands (run from `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55`):
```
sed -n '/internal void InvalidateEngineCommands/,/^        }/p' TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs
sed -n '74,86p' TaskMaster/ThisAddIn.cs
grep -rn "RefreshEngineCommands()" --include=*.cs .        # call-site enumeration
```

EXIT_CODE: 0

## Fact 1 — `InvalidateEngineCommands()` returns without throwing when `_ribbon` is null

```csharp
        internal void InvalidateEngineCommands()
        {
            var ribbon = _ribbon;
            if (ribbon is null)
            {
                return;
            }

            var dispatcher = UiThread.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(
                    () => EngineCommandRefreshPlanner.InvalidateAll(ribbon.InvalidateControl)
                );
                return;
            }

            EngineCommandRefreshPlanner.InvalidateAll(ribbon.InvalidateControl);
        }
```

`_ribbon` is assigned only in `Ribbon_Load` (`RibbonViewer.cs`). Before that runs it is null, and the method returns immediately — a plain `return`, with no `throw` on any path. `_ribbon` is captured into a local `ribbon` before the null test, so the value used inside the marshalled lambda cannot be a different (or newly-nulled) instance than the one that passed the check.

`RibbonController.RefreshEngineCommands()` additionally guards its side of the call:

```csharp
        internal void RefreshEngineCommands()
        {
            _viewer?.InvalidateEngineCommands();
        }
```

so an early refresh before `SetViewer` is also a no-op rather than a `NullReferenceException`.

## Fact 2 — the `IRibbonUI` call is explicitly marshalled through `UiThread.Dispatcher`

The method reads `UiThread.Dispatcher` (declared in `UtilitiesCS\Threading\UiThread.cs`, namespace `UtilitiesCS`) and calls `CheckAccess()`. When the current thread is **not** the dispatcher thread, the invalidation is executed through `dispatcher.Invoke(...)`; only when already on the dispatcher thread does it call through directly. It does **not** rely on the ambient `SynchronizationContext`.

This is required because `IRibbonUI` is an Office COM object handed to `Ribbon_Load` on the STA and must be called back on the STA. `AppItemEngines.InitAsync()` is launched via `Task.Run` (`ApplicationGlobals.cs`), so it completes on a thread-pool thread; the continuation resumes on the STA only when a synchronization context happened to be captured, which is true on the `Application_Startup` path but not on the `LoadWhenIdle()` (`useUiThread: false`) or `LoadParallelAsync()` paths. The explicit marshalling makes the refresh correct on every load path.

## Fact 3 — `ThisAddIn` invokes the refresh exactly once, immediately after `await _globals.LoadAsync(false);`

`grep -rn "RefreshEngineCommands()" --include=*.cs .` (excluding `.claude/` worktree paths) returns exactly two lines:

```
./TaskMaster/Ribbon/RibbonController.EngineCommands.cs:76:        internal void RefreshEngineCommands()
./TaskMaster/ThisAddIn.cs:82:                    _ribbonController.RefreshEngineCommands();
```

The first is the declaration; the second is the single call site in the entire repository. Its context:

```csharp
                {
                    _currentStartupStageLabel = StartupStageLabels.Loading;
                    await _globals.LoadAsync(false);

                    // Issue #503: this refresh is load-bearing, not cosmetic. Office caches each
                    // getEnabled response per control until the add-in invalidates it, so without
                    // this call the eight engine-backed buttons remain disabled for the whole
                    // session even after AppItemEngines.InitAsync() has succeeded.
                    _ribbonController.RefreshEngineCommands();

                    logger.Debug("Finished loading globals");
                    _currentStartupStageLabel = StartupStageLabels.PostLoad;
                    _startupPostLoadReached = true;
                }
```

The call is the first statement after `await _globals.LoadAsync(false);`, inside the `IdleAsyncQueue.AddEntry(true, ...)` entry — that is, the entry enqueued with `useUiThread: true`.

Corroborating unit evidence for the decision half: `InvalidateAll_InvokesDelegateOnceForEachEngineBackedControlId` asserts the invalidated id set equals `EngineCommandCatalog.ControlIds` as a set, and `InvalidateAll_WithNullDelegate_ThrowsArgumentNullException` pins the precondition. Both `RibbonViewer.InvalidateEngineCommands` and `RibbonController.RefreshEngineCommands` are inside `[ExcludeFromCodeCoverage]` types by design and are verified here by source inspection, as AC18 specifies.

Binary outcome: **PASS** on all three facts.
