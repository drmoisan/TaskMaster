# P10-T2 — Complete scope-lock diff over the full C# project set

Timestamp: 2026-08-28T01-49
Command: git diff --name-only cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/ QuickFiler.Test/ UtilitiesCS/ UtilitiesCS.Test/ ToDoModel/ ToDoModel.Test/ Tags/ Tags.Test/ TaskMaster/ TaskMaster.Test/ TaskTree/ TaskTree.Test/ TaskVisualization/ TaskVisualization.Test/ TaskVisualizer/ SVGControl/ SVGControl.Test/ VBFunctions/ VBFunctions.Test/
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Verbatim output

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs
QuickFiler/Controllers/QfcItemController.EventHandlers.cs
QuickFiler/Controllers/QfcItemController.EventWiring.cs
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs
QuickFiler/Controllers/QfcItemController.FolderHandling.cs
QuickFiler/Controllers/QfcItemController.MailActions.cs
QuickFiler/Viewers/IItemViewer.cs
QuickFiler/Viewers/ItemViewer.Designer.cs
QuickFiler/Viewers/ItemViewer.DisplayState.cs
QuickFiler/Viewers/ItemViewer.FolderSearch.cs
QuickFiler/Viewers/ItemViewer.cs
QuickFiler/Viewers/ItemViewerExpanded.Designer.cs
QuickFiler/Viewers/ItemViewerExpanded.cs
```

PathCount: **25**

## Acceptance — the recorded list contains only the permitted paths and no others

The plan enumerates 25 permitted paths. The diff contains 25. The correspondence is exact in both
directions: every permitted path is present, and every present path is permitted.

| # | Permitted path (plan group) | In diff |
|---|---|---|
| 1 | `QuickFiler/Viewers/ItemViewer.cs` | Yes |
| 2 | `QuickFiler/Viewers/ItemViewer.Designer.cs` | Yes |
| 3 | `QuickFiler/Viewers/ItemViewer.DisplayState.cs` | Yes |
| 4 | `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | Yes |
| 5 | `QuickFiler/Viewers/ItemViewerExpanded.cs` | Yes |
| 6 | `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` | Yes |
| 7 | `QuickFiler/Viewers/IItemViewer.cs` | Yes |
| 8 | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | Yes |
| 9 | `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | Yes |
| 10 | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | Yes |
| 11 | `QuickFiler/Controllers/QfcItemController.MailActions.cs` | Yes |
| 12 | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | Yes |
| 13 | `QuickFiler.Test/QuickFiler.Test.csproj` | Yes |
| 14 | `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` | Yes |
| 15 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | Yes |
| 16 | `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | Yes |
| 17 | `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | Yes |
| 18 | `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` | Yes |
| 19 | `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` (line-neutral `partial` modifier edit, P1-T4) | Yes |
| 20 | `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | Yes |
| 21 | `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | Yes |
| 22 | `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` | Yes |
| 23 | `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs` | Yes |
| 24 | `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs` | Yes |
| 25 | `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` | Yes |

**Unpermitted paths in the diff: none.** Set-differencing the 25 observed paths against the 25
permitted paths yields the empty set in both directions.

## Forbidden paths confirmed absent

| Forbidden path | Owner / reason | In diff |
|---|---|---|
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 488 | Absent |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 501 | Absent |
| `QuickFiler/Viewers/Breadcrumb*` (any other) | 501, live | Absent |
| `QuickFiler/Viewers/WebView2*` | 476, live | Absent |
| `QuickFiler/Controllers/Efc*` | 464, live | Absent |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 444 | Absent |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 484 | Absent |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | consumed, not edited | Absent |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | pinned at 497 lines | Absent |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | pinned by 468 | Absent |
| `QuickFiler/QuickFiler.csproj` | no new production file added | Absent |
| Any `UtilitiesCS/` or `UtilitiesCS.Test/` file | out of scope | Absent |

Every one of the eighteen non-`QuickFiler` project directories in the pathspec — `UtilitiesCS/`,
`UtilitiesCS.Test/`, `ToDoModel/`, `ToDoModel.Test/`, `Tags/`, `Tags.Test/`, `TaskMaster/`,
`TaskMaster.Test/`, `TaskTree/`, `TaskTree.Test/`, `TaskVisualization/`, `TaskVisualization.Test/`,
`TaskVisualizer/`, `SVGControl/`, `SVGControl.Test/`, `VBFunctions/`, `VBFunctions.Test/` —
contributes **zero** paths. All 25 observed paths lie under `QuickFiler/` or `QuickFiler.Test/`.

## Pathspec width

This gate uses the **full C# project set** defined in the plan's § Execution conventions: every
directory holding a tracked `*.csproj` (eighteen of them) plus `TaskVisualizer/`. A narrower
three-directory pathspec could not observe an edit or a CSharpier rewrite landing in any of the other
fifteen project directories, so the width is load-bearing for this task specifically.

`docs/` and `.claude/agent-memory/` remain deliberately outside the pathspec. `.claude/agent-memory/`
is tracked rather than gitignored and the executing agent writes into it during a run of this size,
so including it would make the gate unsatisfiable by construction. No file count is asserted for that
directory.

Output Summary: The scope lock **holds**. `git diff --name-only <BASELINE_SHA>` over the full
nineteen-directory C# project set returns exactly **25** paths, which correspond one-to-one with the
25 paths the plan permits — no path is missing and no path is extra. All 25 lie under `QuickFiler/`
or `QuickFiler.Test/`; the other seventeen project directories in the pathspec contribute nothing.
Every forbidden path is confirmed absent, including `ItemViewer.Breadcrumb.cs` (488),
`BreadcrumbBridgeCoordinator.cs` (501), `QfcItemController.Navigation.cs` (444),
`QfcItemController.ViewerSetup.cs` (484), `QfcItemController.TestSupport.cs`,
`QfcItemController.FocusAndThemeTests.cs`, `QfcCollectionControllerTests.cs`,
`QuickFiler/QuickFiler.csproj`, and every `UtilitiesCS` file.
