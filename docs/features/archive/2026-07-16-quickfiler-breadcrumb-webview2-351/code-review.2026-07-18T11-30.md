# Code Review — quickfiler-breadcrumb-webview2 (Issue #351)

- Feature folder: `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351`
- Base: `8e242692` → Head: `c80ec54a` (9 commits, 70 files, +5731/-624)
- Date: 2026-07-18T11-30
- Review basis: full branch diff (`artifacts/pr_context.appendix.txt`), CLAUDE.md General/C# code-change and unit-test policies.

## Executive Summary

Code quality is consistent with repository conventions. The design cleanly separates a host-neutral
core (state model, render projection, bridge protocol, async router, selection map) from thin,
`[ExcludeFromCodeCoverage]`-marked WebView2/WinForms seams, mirroring the tested `FolderTreeStateModel`
precedent. Public-API removals (`IQfcKeyboardHandler.CboFolders_KeyDown`, `IItemViewer.CboFolders`)
are matched by updated in-repo callers with zero dangling references. The seven files edited beyond
the plan's named list are mechanical consumer adaptations forced by removing the `CboFolders` control
and the `Theme` constructor's `ComboBox` parameter; none introduces opportunistic behavior change.
No Blocking findings. A small number of Minor observations are noted for maintainer awareness.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | QuickFiler/Interfaces/IQfcKeyboardHandler.cs | 25-34 | Public interface member `CboFolders_KeyDown` (sync) removed; `BreadcrumbArrowFallThrough` added. This is a breaking change to an internal interface. | Acceptable — all in-repo callers updated (0 remaining `CboFolders_KeyDown` callers). Keep the change note in the diff. | Breaking public-API changes must be called out and all callers updated (General Code Change §7). | `grep CboFolders_KeyDown\b` returns no callers; interface comment documents the reroute. |
| Minor | QuickFiler/Controllers/KeyboardHandler.cs | whole file | Retains the historical method name `CboFolders_KeyDownAsync` though the control is now a breadcrumb; naming residue may mislead future readers. | Optional follow-up rename to a breadcrumb-oriented name; not required this cycle (rerouting minimizes diff, as the spec authorized). | Names should be descriptive (General Code Change §5); weighed against smaller-diff reroute the spec explicitly preferred. | Spec §Constraints "prefer rerouting over removal"; file shrank 631→414. |
| Minor | QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 209 | Handler `CboFolders_SelectedIndexChanged` name retained but now wired to the breadcrumb `FolderSelectionChanged` event. | Same as above — naming residue only; behavior preserved. | Consistency/readability; non-behavioral. | `QfcItemController.EventWiring.cs:86` wires `FolderSelectionChanged += CboFolders_SelectedIndexChanged`. |
| Minor | QuickFiler/Viewers/ItemViewer.Designer.cs | (generated) | File is 6224 lines, over the 500-line ceiling; grew +6 lines by this change. | No action — generated Designer file, pre-existing debt; the ceiling exception intent covers generated code and this change did not create the condition. | File-size limit (General Code Change §4); pre-existing and generated. | Guardrail evidence G5; `wc -l` = 6224. |
| Minor | QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | 40-72 | New breadcrumb-init glue lowered the file's line-rate (68.8%→68.6%) via two newly measured glue lines, though covered count rose 95→96. | None — added lines are in `[ExcludeFromCodeCoverage]` init paths; no covered line regressed. | Changed-line coverage must not regress (General Unit Test policy). | `coverage-delta-verification.2026-07-18T11-15.md` item 3. |
| Info | QuickFiler/Controllers/EfcItemController.cs | 587-590, 655 | Consumer adaptation: `SelectedFolder` rerouted from `CboFolders.SelectedItem` to `GetSelectedFolder()`; control ref swapped to breadcrumb WebView2. Mechanical, contract-preserving. | None. | Verifies out-of-plan edit #1 is mechanical. | Diff inspected; delegates to the preserved selection contract. |
| Info | UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs | 29-83, 120-127 | `Theme` ctor parameter `ComboBox comboFolders` replaced with `WebView2 breadcrumbWebView2`; optional `Action<string> breadcrumbThemeNotifier = null` appended (non-breaking default). Field renamed accordingly. | None — forced the QfcThemeHelper/QfcThemeControlSet consumer edits. | Verifies out-of-plan edits (Theme, QfcThemeHelper, QfcThemeControlSet, Theme.Rendering) are mechanical. | Diff inspected; optional param preserves existing callers. |
| Info | QuickFiler/Viewers/WebView2Messenger.cs, ItemViewer.Breadcrumb.cs | file-level | Thin seams correctly carry `[ExcludeFromCodeCoverage]`; `BreadcrumbBridgeCoordinator.cs` is NOT exempt and is tested at 97.3%. | None. | Correct exempt/non-exempt split per spec §Architecture. | `grep ExcludeFromCodeCoverage`. |

## Design Notes

- Host-neutral core placement in `UtilitiesCS.OutlookObjects.Folder` mirrors `FolderTreeStateModel`;
  DTOs crossing the bridge are JSON-serializable primitives (no COM handles), satisfying the bridge
  contract in spec §FR-6.
- Error handling: the router validates inbound message types and produces explicit unhandled/fallback
  responses rather than silently dropping messages; setters guard nulls (pinned by
  `SetItems/AddItems null guards` tests).
- net48 safety: no `record`/`init`/`record struct`; the consumed 9101 `FolderBreadcrumbSegment` is a
  sealed immutable class with explicit ctor and get-only properties.
- The seven out-of-plan consumer edits (EfcItemController.cs, QfcCollectionController.cs,
  QfcThemeHelper.cs, QfcThemeControlSet.cs, Theme.cs, IQfcKeyboardHandler.cs, ItemViewer.cs) are all
  mechanically forced by the `CboFolders` control removal and the `Theme` ctor parameter change; none
  introduces a behavior change beyond the mechanical adaptation.

## Test Quality

- MSTest + Moq + FluentAssertions used consistently; no temp files; deterministic (completed tasks,
  no timers). 114 new tests; 4952/4952 pass. Router edge cases (cancellation, unroutable type,
  empty-chain fallback, plain-row subfolder request) and state-model boundary transitions are covered.

Blocking findings: 0
