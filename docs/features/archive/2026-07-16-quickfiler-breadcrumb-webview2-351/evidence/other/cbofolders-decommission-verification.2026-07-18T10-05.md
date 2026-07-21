# CboFolders Decommission Verification (P5-T10, AC-5)

Timestamp: 2026-07-18T10-05

Command: grep -rEn "\b_cboFolders\b|\bCboFolders\b" --include="*.cs" QuickFiler UtilitiesCS TaskMaster TaskVisualization ToDoModel Tags | grep -v ".Test" | grep -vE "<nine dead-variant Designer files>"
Command: grep -rn "FolderHierarchyBuilder" --include="*.cs" QuickFiler UtilitiesCS TaskMaster TaskVisualization ToDoModel Tags | grep -v ".Test" | grep -v "FolderHierarchyBuilder.cs"
SearchScope: all production projects in the repository (`QuickFiler`, `UtilitiesCS`, `TaskMaster`, `TaskVisualization`, `ToDoModel`, `Tags`), excluding test projects; word-boundary regex so identifiers that merely contain the substring (e.g. `CboFoldersBackColor` theme palette properties, `CboFolders_KeyDownAsync` handler name) are correctly not counted as control references.

## (a) `_cboFolders` / `CboFolders` control references

SearchResult:
- Comment-only mentions (not code references): `QfcItemController.EventWiring.cs:48`, `IItemViewer.cs:77` (pre-existing interface doc comment; interface file intentionally unchanged per P5-T3 acceptance), `ItemViewer.Breadcrumb.cs:20`, `ItemViewer.FolderSearch.cs:11` — all four are explanatory comments describing the replacement; none compiles to a reference.
- `QuickFiler/Viewers/QfcItemViewerLightSelected.cs:54,58` — a dead-variant CODE-BEHIND file referencing its own Designer-declared `_cboFolders` field (declared in `QfcItemViewerLightSelected.Designer.cs`, one of the nine G8-untouched dead variants). This is internal to the dead variant type, which is never constructed in production (research §1); it is part of the dead-variant set the plan explicitly leaves untouched, not a live-path reference.
- Live production path (ItemViewer, controllers, keyboard handler, theme helpers): ZERO code references. The `ItemViewer.CboFolders` property, `_cboFolders` Designer field, owner-draw handlers (`CboFolders_DrawItem`, `CboFolders_MouseDown`), `FolderPercentColumnWidth`, and the legacy `CboFolders_KeyDown`/`CboFolders_KeyDownAsyncOld` handlers were all removed; `Theme`/`QfcThemeControlSet`/`QfcThemeHelper` now carry the breadcrumb WebView2 + themeChange notifier instead of the ComboBox.

Verdict (a): PASS — zero production code references to the `CboFolders` control outside the nine dead-variant files (Designer declarations plus the one dead-variant code-behind that consumes its own Designer field).

## (b) `FolderHierarchyBuilder.Build` production call sites

SearchResult:
- `QfcItemController.FolderHandling.cs:175` — comment only.
- `ItemViewer.FolderSearch.cs:12` — comment only.
- `UtilitiesCS/OutlookObjects/Folder/FolderTreeStateModel.cs:19` — XML doc `<see cref>` (documentation reference, not a call).
- Zero production call sites. The sole former caller (`ItemViewer.FolderSearch.cs` `SetFolderSuggestions`) now delegates to `BreadcrumbBridgeCoordinator.SetSuggestions`, whose ancestor chains come from the injected `IFolderHierarchyProvider`.
- `FolderHierarchyBuilder.cs` and its tests (`UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyBuilderTests.cs`) remain in place, unreferenced by the live QuickFiler path, exactly as the plan requires.

Verdict (b): PASS — `FolderHierarchyBuilder.Build` has zero production call sites.
