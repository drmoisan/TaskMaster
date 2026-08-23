# [P1-T3] Fail-Before Exception Dossier — New-Seam Regressions

- **Issue:** #438
- **Task:** [P1-T3]
- **Timestamp:** 2026-08-08T11-41
- **Scope of this dossier:** AC-2 (search-driven variant), AC-3, AC-4, AC-5, AC-8, AC-9
- **Plan authority:** Decisions Record D5 (fail-before split)

## Command

N/A — this artifact documents why a pre-fix *failing run* is structurally impossible for the criteria listed above. The observable fail-before run for AC-1 is recorded separately in `fail-before.2026-08-08T11-41.md` (EXIT_CODE 1, 4 of 5 tests failed).

- **EXIT_CODE:** N/A (documentation artifact)

## WhyFailingRunImpossible

The regressions for AC-2 (search variant), AC-3, AC-4, AC-5, AC-8, and AC-9 assert against members that **do not exist in the pre-change source**. A test that references them does not compile, so the pre-fix outcome is a C# compile error (`CS1061` / `CS1501`), not a failing test run. A build error is not a fail-before observation: it proves nothing about runtime behavior and yields no test result to record.

The absent members, each verified against the pre-change tree at HEAD `904b4c38dba0f9f41707c3c0f077e123c78de59c`:

| Member | Declaring type | File (pre-change) | Status pre-change |
|---|---|---|---|
| `PresentFolderSearchResults(string[])` | `IItemViewer` | `QuickFiler/Viewers/IItemViewer.cs` (133 lines, members at `:80-100`) | absent |
| `PresentSearchResults(IReadOnlyList<string>)` | `BreadcrumbBridgeCoordinator` | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | absent |
| `HighlightRow(int)` | `BreadcrumbSelectionSession` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | absent |
| `ReplaceItemsPreservingSession(IReadOnlyList<string>)` | `FolderBreadcrumbBridgeRouter` | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | absent (only the private `ReplaceRowsPreservingSession` at `:474-478` and the unreachable public `SetItems` at `:119-135` exist) |
| `OpenAsync(Rectangle, Rectangle, Size, bool takeFocus)` | `IBreadcrumbDropDownHost` / `BreadcrumbDropDownHost` | `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs:31`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:228-242` | absent — only the 3-parameter overload exists |

`SearchScope:` `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/regression-testing/`, `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/`
`SearchPatterns:` `fail-before*.md`
`SearchResult:` `fail-before.2026-08-08T11-41.md` (observed failing run, AC-1), this dossier.

## Alternative proof

### 1. Observed failing run at the controller seam

`evidence/regression-testing/fail-before.2026-08-08T11-41.md` records EXIT_CODE 1 with four Moq `MockException` failures on a byte-clean production tree. The failures prove the defective composition is live:

```
Expected invocation on the mock should never have been performed, but was 1 times: v => v.SetFolderDroppedDown(It.IsAny<bool>())
Expected invocation on the mock should never have been performed, but was 1 times: v => v.SetFolderSelectedIndex(It.IsAny<int>())
```

`SetFolderDroppedDown(true)` is the entry point to the entire open pipeline that AC-2, AC-3, AC-8, and AC-9 constrain, and `SetFolderSelectedIndex(1)` is the committed-selection mutation that AC-4 and AC-5 constrain. A single controller-seam observation therefore anchors all six deferred criteria to a demonstrated live defect.

### 2. A second observed failing run precedes the behavior flip

Per D5 and plan task P5-T2, the finalized regressions (the rewritten `TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PopulatesAndSelectsFolder` plus the `PresentFolderSearchResults(...)` intent assertion) are compiled against the new surface but run **before** the `QfcItemController.EventHandlers.cs` behavior flip, and are observed failing. That run is recorded at `evidence/regression-testing/fail-before-controller.<ts>.md`, giving a second, post-surface fail-before observation for the intent half of AC-1 and, transitively, for the presentation composite the deferred criteria assert on.

### 3. Code-read citations of the defective path (research §1–§2, all `[VERIFIED]`)

| AC | Defective behavior in the pre-change source | Citation |
|---|---|---|
| AC-2 (search variant) | A fresh open ends in `FocusCurrentSurface(lease)` -> `_host.FocusPending()`, focusing the popup unconditionally. A re-issued open on an open popup runs `_openLifetime.Schedule(_focusPending)`. | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:287-305`; `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:228-242` |
| AC-3 | The leading `ClearFolderItems()` cancels the open session (`ClearSelector` -> `Cancel` + `OpenStateChanged`), driving `CloseCore` -> `_host.Close(...)`, so every second keystroke closes and reopens the popup. | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs:161-174`; `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:119-132`; `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:385-399, 427-437` |
| AC-4 | `SetFolderSelectedIndex(1)` -> `BreadcrumbSelectionSession.SelectRow` mutates `_model.SelectRow(index)` — the committed selection behind the collapsed surface and `GetSelectedFolder()` — and always returns `SelectionChanged \| RenderRequired`. | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs:176-183`; `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:213-214` |
| AC-5 | `CancelSelector` emits `Handled \| OpenStateChanged \| RenderRequired` **without** `SelectionChanged`, so after Escape the controller's `_selectedFolder` retains the mid-search row-1 value cached by `CboFolders_SelectedIndexChanged`. | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs:297-304`; `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:209-212` |
| AC-8 | The current `Clear()` + `AddItems(...)` pair emits at least two renders per keystroke, versus the one-render-per-surface contract of #400 AC-12. | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:150-157, 130-147`; `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:138-152, 168-173` |
| AC-9 | `SetFolderDroppedDown(true)` is issued unconditionally regardless of result-set size — reproduced live by the two edge-case failures in the P1-T2 run (empty result set and single-row result set both invoked it once). | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:177`; `evidence/regression-testing/fail-before.2026-08-08T11-41.md` |

## Result

- **Output Summary:** A pre-fix failing run is structurally impossible for AC-2 (search variant), AC-3, AC-4, AC-5, AC-8, and AC-9 because each targets a member that does not exist before the change, making the pre-fix outcome a compile error rather than a test failure. The requirement is discharged by three alternative proofs: the observed EXIT_CODE 1 controller-seam failing run in `fail-before.2026-08-08T11-41.md`, the second observed failing run scheduled at P5-T2 before the behavior flip, and `file:line` code-read citations of the defective path for each deferred criterion. Accept criteria met.
