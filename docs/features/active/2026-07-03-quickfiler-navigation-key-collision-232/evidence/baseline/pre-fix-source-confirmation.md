# Pre-Fix Source Confirmation (Issue #232)

Timestamp: 2026-07-03T11-27

Confirms the pre-fix source state matches the research/spec citations before any edit.

## Citation 1 — `QuickFiler/Controllers/QfcCollectionController.cs:252-262`
Confirmed present as described. `LoadControlsAndHandlers_01(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups)` (lines 252-262) performs the `_moveMonitor.HookItem` loop, `_formViewer.SuspendLayout()`, `ActivateQueuedTlp(tlp)`, then calls `ActivateQueuedItemGroups(itemGroups)` **directly** (line 259) with no `UnregisterNavigation()`/`RegisterNavigation()` pairing, followed by `_formViewer.ResumeLayout()` and trailing `ActiveIndex = -1;`. This is the defective swap path.

## Citation 2 — `QuickFiler/Controllers/QfcCollectionController.cs:870-878` (SwapItemGroups)
Confirmed present as described. `internal void SwapItemGroups(List<QfcItemGroup> itemGroups)` (lines 870-878) implements the correct pattern: `UnregisterNavigation();` -> `CacheItemGroupsForMove();` -> `ActivateQueuedItemGroups(itemGroups);` -> `RegisterNavigation();`. It has no other in-file caller (currently dead code).

## Citation 3 — `QuickFiler/Controllers/QfcCollectionController.cs:1139-1221` (RemoveSpecificControlGroupAsync)
Confirmed present as described. The zero-item branch's `await ((QfcFormController)_parent).SkipGroupAsync();` is at line 1209 (inside the `if (_itemGroups.Count == 0)` block within the trailing `UiThread.Dispatcher.InvokeAsync` lambda). The unconditional trailing `RegisterNavigation();` is at line 1219. The static reentrancy counter `removespecificcontrolgroupcounter` is declared at line 1139 and Interlocked-incremented at 1143 / decremented at 1220.

## Citation 4 — `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` (no logger field)
Confirmed present as described. The `QfcHighConfidencePreFilter` static class (lines 23-80) has **no** existing `log4net.ILog logger` field. The file's only other members are `QfcPreScoredItem` (struct), `IFolderScoringService` (interface), and `FolderScoringService` (sealed class, `[ExcludeFromCodeCoverage]`).

## logger convention reference
Confirmed `QfcCollectionController.cs:23-25` uses the `private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);` convention that P4-T4 will replicate into `QfcHighConfidencePreFilter.cs`.
