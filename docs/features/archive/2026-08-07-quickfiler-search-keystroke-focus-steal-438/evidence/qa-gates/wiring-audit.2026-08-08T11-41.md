# [P6-T7] Wiring, Discovery, and Clean-Tree Audit

- **Issue:** #438
- **Task:** [P6-T7]
- **Timestamp:** 2026-08-08T11-41

## Command

`pwsh -NoProfile -Command "git ls-files --others --exclude-standard -- '*.cs'"` cross-referenced against each owning `.csproj` with an exact `<Compile Include="..." />` string match, plus `git status --porcelain`.

- **EXIT_CODE:** 0

## (1) Every new `.cs` file has a matching `<Compile Include>` entry (AC-14)

All 13 new files verified by exact-string match. `QuickFiler.Test`, `UtilitiesCS.Test`, `QuickFiler`, and `UtilitiesCS` are legacy non-SDK projects with no glob include, so an omitted entry means the file silently does not compile.

| # | New file | Owning `.csproj` | `<Compile Include>` |
|---|---|---|---|
| 1 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs` | `UtilitiesCS/UtilitiesCS.csproj` | OK |
| 2 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | `UtilitiesCS/UtilitiesCS.csproj` | OK |
| 3 | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | `QuickFiler/QuickFiler.csproj` | OK |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs` | `QuickFiler/QuickFiler.csproj` | OK |
| 5 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` | `QuickFiler/QuickFiler.csproj` | OK |
| 6 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs` | `QuickFiler/QuickFiler.csproj` | OK |
| 7 | `QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs` | `QuickFiler.Test/QuickFiler.Test.csproj` | OK |
| 8 | `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` | `QuickFiler.Test/QuickFiler.Test.csproj` | OK |
| 9 | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` | `QuickFiler.Test/QuickFiler.Test.csproj` | OK |
| 10 | `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs` | `QuickFiler.Test/QuickFiler.Test.csproj` | OK |
| 11 | `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.Part2.cs` | `QuickFiler.Test/QuickFiler.Test.csproj` | OK |
| 12 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionHighlightTests.cs` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | OK |
| 13 | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | OK |

**Missing entries: 0.**

No new external package and no persisted configuration was introduced: `packages.config`, `app.config`, and `.runsettings` files are all unmodified (they do not appear in `git status --porcelain`).

## (2) Every new test class executed with count > 0 in the P6-T5 run

Counts taken from the final P6-T5 full-suite log (`Passed <method>` lines), not from a discovery listing:

| Prefix | Suite(s) | Executed |
|---|---|---:|
| `HighlightRow_*` | `BreadcrumbSelectionSessionHighlightTests` | 11 |
| `ReplaceItemsPreservingSession_*` | `FolderBreadcrumbBridgeRouterReplaceItemsTests` | 9 |
| `TextBoxSearch_TextChanged_*` | `QfcItemController_SearchFocusRegressionTests` + the rewritten `EventHandlersTests` method | 8 |
| `SearchThenCancel_*` | `QfcItemController_SearchFocusRegressionTests` | 1 |
| `PresentFolderSearchResults_*` | `BreadcrumbDropDownSearchIntegrationTests` | 7 |
| `PresentSearchResults_*` | `BreadcrumbDropDownSearchIntegrationTests.Part2` | 2 |
| `EightCharacterQueryTypedThroughTheSeam*` | `BreadcrumbDropDownSearchIntegrationTests.Part2` | 1 |
| `OpenAsync_FreshOpen*`, `OpenAsync_Reissued*`, `OpenAsync_Consecutive*`, `OpenAsync_NonFocusing*`, `OpenAsync_ThreeParameter*`, `Close_AfterANonFocusing*` | `BreadcrumbDropDownHostTests.Part2` | 8 |
| `Latch*` | `BreadcrumbDropDownOpenCoordinatorTests.Part3` | 5 |

Every new test class executed. No class was discovered-but-skipped, and no new file failed to compile into its assembly.

## (3) `git status --porcelain` contains only intended paths

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/prd-feature/project_promotion_scaffold_metadata_defects.md
 M .claude/agent-memory/task-researcher/MEMORY.md
 M QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs
 M QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs
 M QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs
 M QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs
 M QuickFiler/Controllers/QfcItemController.EventHandlers.cs
 M QuickFiler/QuickFiler.csproj
 M QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs
 M QuickFiler/Viewers/BreadcrumbDropDownHost.cs
 M QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
 M QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs
 M QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs
 M QuickFiler/Viewers/IBreadcrumbDropDownHost.cs
 M QuickFiler/Viewers/IItemViewer.cs
 M QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
 M QuickFiler/Viewers/ItemViewer.FolderSearch.cs
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs
 M UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs
 M UtilitiesCS/UtilitiesCS.csproj
 D docs/features/potential/promoted/2026-08-07-quickfiler-search-keystroke-focus-steal.md
?? .claude/agent-memory/... (5 files)
?? <13 new .cs files listed above>
?? docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/
```

Classification — every entry falls in an intended bucket:

| Bucket | Count | Notes |
|---|---:|---|
| Production `.cs` of this plan | 12 modified + 6 new | exactly the 12 files estimated by research §3 Option 3 |
| Test `.cs` of this plan | 5 modified + 7 new | 5 modified are the D3/D4/D7 sanctioned files |
| `.csproj` wiring | 4 | `<Compile Include>` entries only |
| `<FEATURE>/` evidence and docs | 1 tree | `docs/features/active/2026-08-07-...-438/` |
| `.claude/agent-memory/` | 4 modified + 5 new | allowlisted |
| Promotion-lifecycle deletion | 1 | pre-existing at P0-T3; expected |

**Entries outside the intended set: 0.** No build output, no `bin/`/`obj/`, no scratch file, no temporary file, and no coverage artifact outside `<FEATURE>/evidence/` appears.

Note: the working tree is intentionally uncommitted at this point; P8-T2 records the committed clean-tree state.

## Result

- **Output Summary:** All 13 new `.cs` files have an exact matching `<Compile Include>` entry in their owning legacy `.csproj` (zero missing), and no new package or persisted configuration was introduced. Every new test class executed with count > 0 in the final P6-T5 full-suite run, confirmed from `Passed` lines rather than a discovery listing. `git status --porcelain` contains only production, test, `.csproj` wiring, `<FEATURE>/`, agent-memory, and the expected pre-existing promotion-lifecycle deletion — zero unintended paths. Accept criteria met.
