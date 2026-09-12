---
name: qfc-collection-controller-454
description: Issue #454 / epic #136 F11 research — QfcCollectionController split into 13 partials + 4 seam files; #444 defect is in DEAD code; 12 unreachable members; UiThread.Dispatcher is null in tests
metadata:
  type: project
---

Research completed 2026-08-07 for issue #454 (epic #136 child F11), target
`QuickFiler/Controllers/QfcCollectionController.cs` (2,349 lines, `[ExcludeFromCodeCoverage]` at :21).
Artifact: `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`.

Findings that are NOT re-derivable by reading the file alone:

- **Issue #444's duplicate-`KaKey` defect is DORMANT.** `WireUpKeyboardHandler` (:1254) has no caller
  anywhere. Production wires keys via `WireUpAsyncKeyboardHandler` -> `RegisterAsyncKeyActions`, which
  registers `Keys.Up`/`Keys.Down` once each. Also: `KbdActions(IEnumerable<T>)` (`KbdActions.cs:26-29`)
  does NO duplicate check, so construction succeeds; only `Find`/`FindIndex`/indexer throw.
- **12 members are unreachable** (no caller in production or test): `WireUpKeyboardHandler`,
  `AnyOpenDropDownsAsync`, `LoadGroups_02bAsync`, `LoadGroups_02cAsync`, `LoadGroup_03bAsync`,
  `LoadConversationsAndFoldersAsync`, `LoadItemGroup`, `LoadSequentialAsync`, `LoadGroupSequential`,
  `CacheTlpForMove`, `SwapTlp`, `CaptureTlpTemplate`. ~227 lines of coverage denominator.
- **`UtilitiesCS.UiThread.Dispatcher` is `null` in any unit test** — static, `private set`, initialized
  `null!`, assigned only in `Init()` (`UtilitiesCS/Threading/UiThread.cs:135-140, 61`). Every
  `UiThread.Dispatcher.InvokeAsync` call NREs. The fix is the EXISTING public `IUiDispatcher` /
  `WpfUiDispatcher` seam already used by `QfcItemController.Initialization.cs:38` — no new file.
- **`await someSyncContext` throws `ArgumentNullException` when the mock returns null** —
  `SynchronizationContextAwaiter` ctor guards at `UtilitiesCS/Threading/UiThread.cs:93-96`. Tests must
  set `IQfcFormViewer.UiSyncContext` to a real context whose `Post` runs inline.
- **Seams stay private/internal; `IQfcCollectionController` is untouched**, so F7's (#433) "needs no
  contract additions" conclusion holds. The only public change is optional trailing ctor parameters;
  the three call sites at `QfcFormController.Actions.cs:49,83,139` compile unchanged.
- **The `DynamicProxyGenAssembly2` grant (what lets Moq mock `internal` types such as
  `IEmailMoveMonitor`) is declared in an F2-owned file** — `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`
  (also duplicated in the non-compiled `QuickFiler/Legacy/IAcceleratorCallbacks.cs:5`). It is NOT in
  `Properties/AssemblyInfo.cs`. Do not delete or relocate it while refactoring; a child that removes
  it breaks Moq for every internal QuickFiler interface.
- **`async public Task CleanupAsync()` at :2178 uses reversed modifier order**, so a `public async`
  grep misses it. Grep for the member name, not the modifier pair, when inventorying this file.
- **Cross-child pins:** `xComma` must stay `public static` (`EfcHomeController.Metrics.cs:79`, F8);
  `QfcItemGroup.cs` is F2-owned so `ItemViewer` cannot be retyped to `IItemViewer`.
- **14 latent defects to promote**, incl. an `EliminateSpaceForItems` sign error (:2017-2026 grows the
  panel on removal), an unreachable null-guard in `GetMoveDiagnostics` (:2288 vs :2313), a leaking
  `static` counter without `finally` (:1157/:1247), and `UnregisterNavigation` re-evaluating the
  side-effecting `Digits` per iteration while `RegisterNavigation` captures it once (:1332 vs :1347).

**Why:** #454 execution happens in a different worktree later; these are the conclusions a re-read of
the file would take a full pass to reproduce.

**How to apply:** if asked to plan, review, or fix anything in `QfcCollectionController`, start from the
research artifact rather than re-reading 2,349 lines; verify the file still has the same shape first.

Related: [[quickfiler-test-sta-and-ivt]], [[quickfiler-percoverage-epic-136]]
