# P11-T2 — CSharpier format pass (loop iteration 1)

Timestamp: 2026-08-28T02-14
Command: dotnet tool run csharpier format .
EXIT_CODE: 0

Loop iteration: **1**. This is the first stage of the Phase 11 toolchain loop.

The form is the one P11-T1 selected: the repo-wide policy command, run from the worktree root, under
Branch A.

## Files rewritten: 0

CSharpier reported `Formatted 1547 files in 5053ms.` That figure is the count of files **processed**,
not rewritten, and is not used as a gate here. The rewrite count is derived from SHA-256 comparison,
as the task requires.

### Before/after SHA-256 manifest over every targeted file

A manifest of `path  SHA-256` rows was written outside the repository, under the system temporary
directory, immediately before the format command and again immediately after it. The manifest
enumerates every tracked file CSharpier can process — `*.cs`, `*.xml`, and `packages.config` — which
is a **superset** of the 1547 files CSharpier reported checking, because it does not subtract the
`.csharpierignore` exclusions (`**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`,
`*.trx`) that CSharpier applies for itself. A superset is the correct choice for a "did anything
change" gate: a rewrite anywhere in the processed set must appear in it.

```
Files in manifest:        1868
Manifest SHA-256 before:  08f65c828d5164450251d581065fa6b8344dc7bbf60cb9977466dc764f567c75
Manifest SHA-256 after:   08f65c828d5164450251d581065fa6b8344dc7bbf60cb9977466dc764f567c75
Rows differing:           0
```

The two manifests are byte-identical, so **no file in the targeted set has a different SHA-256 after
the format pass than before it**. Recording the aggregate manifest hash is the complete form of the
per-file record for 1868 files: an equal manifest hash entails an equal hash for every row in it,
and any single differing file would change it.

### Per-file SHA-256 for the 25 paths in the P10-T2 scope list

`before` values are read from the pre-format manifest; `after` values are computed directly from disk
after the format pass.

| Path | SHA-256 before | SHA-256 after | Differ |
|---|---|---|---|
| `QuickFiler/Viewers/ItemViewer.cs` | `cd2b9b888c0bb40ea12a60fd33bc9056cfa0f1f21cdac3217ecef5c04ae837fa` | `cd2b9b888c0bb40ea12a60fd33bc9056cfa0f1f21cdac3217ecef5c04ae837fa` | No |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | `297f65d9c43a3d5fa8252aa90595c052f0fbee6d8ec879a4820c0af5dd798981` | `297f65d9c43a3d5fa8252aa90595c052f0fbee6d8ec879a4820c0af5dd798981` | No |
| `QuickFiler/Viewers/ItemViewer.DisplayState.cs` | `dc487a42949aa104fa87f25225d7f2773f2a26a4f0470ecb916133cf72f0c3be` | `dc487a42949aa104fa87f25225d7f2773f2a26a4f0470ecb916133cf72f0c3be` | No |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | `e609fd01270579cf727be1b797d50bdcbfbae70525fad8cd563192a2a0c7dddd` | `e609fd01270579cf727be1b797d50bdcbfbae70525fad8cd563192a2a0c7dddd` | No |
| `QuickFiler/Viewers/ItemViewerExpanded.cs` | `9e7fd5f9ceb358dad3b346e1fffce66c3202ba95efc748ab4dd626bc7d2c60fa` | `9e7fd5f9ceb358dad3b346e1fffce66c3202ba95efc748ab4dd626bc7d2c60fa` | No |
| `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` | `2adde782d2aabf8e831a31e23443f0cfadcc3b96cd7e06e1ed4348920f41031f` | `2adde782d2aabf8e831a31e23443f0cfadcc3b96cd7e06e1ed4348920f41031f` | No |
| `QuickFiler/Viewers/IItemViewer.cs` | `fd9c26e222e881ff67fecd30f0c042099f17d94ae5c2c9d5a1898fc7ba1687ff` | `fd9c26e222e881ff67fecd30f0c042099f17d94ae5c2c9d5a1898fc7ba1687ff` | No |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | `43be8fa945565bd2c5223ed3f9f33f59d24093f9dc2db3fc187393a2c95ba2d9` | `43be8fa945565bd2c5223ed3f9f33f59d24093f9dc2db3fc187393a2c95ba2d9` | No |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | `8ba3d84a62bdbf80f794af8eaab9ec4c73404b3832f791ab288264f475f038a4` | `8ba3d84a62bdbf80f794af8eaab9ec4c73404b3832f791ab288264f475f038a4` | No |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | `12c32f955c43701894a6f277e5bd113a58c48fcaad217535af10942ad8cdb2f7` | `12c32f955c43701894a6f277e5bd113a58c48fcaad217535af10942ad8cdb2f7` | No |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | `7bf07ce89cd9e0cffcaf906d8dcffce9aab771b2fdd39e5d777e1395197a86e1` | `7bf07ce89cd9e0cffcaf906d8dcffce9aab771b2fdd39e5d777e1395197a86e1` | No |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | `f4acc2e65abea87bd13c2e3816e5ee34bfa84c62a8effe8f51baf1642d141fa4` | `f4acc2e65abea87bd13c2e3816e5ee34bfa84c62a8effe8f51baf1642d141fa4` | No |
| `QuickFiler.Test/QuickFiler.Test.csproj` | not a CSharpier target — see note below | `954e7d9b8d4d4c45447c2e03cb50e0748172ccbf0700163367cf4dc60fd2e11c` | No |
| `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` | `e0e84518e93edd8df1ef7ce70707e021314e555b94683fc6268a1de3041d209f` | `e0e84518e93edd8df1ef7ce70707e021314e555b94683fc6268a1de3041d209f` | No |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | `600f71b4a813f45bc0f5cd777283d99b371cd4b6eeebf5253f9f44ef7fba7107` | `600f71b4a813f45bc0f5cd777283d99b371cd4b6eeebf5253f9f44ef7fba7107` | No |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `32473db3d02002d764d76c3987dd067b38b7550882409eced79194a0f929e9fb` | `32473db3d02002d764d76c3987dd067b38b7550882409eced79194a0f929e9fb` | No |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | `16329fa5492d4956d803c42ad6776037f430064174421ab608919f6f35f50945` | `16329fa5492d4956d803c42ad6776037f430064174421ab608919f6f35f50945` | No |
| `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` | `6b49f4fd0449126228bdd3b08ae74d6bece90fb91a1045cb017805c56d940746` | `6b49f4fd0449126228bdd3b08ae74d6bece90fb91a1045cb017805c56d940746` | No |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | `25a4674c52e5fb41b302eb68281245d99b6a76a9c1b88d3f1c6df6a19b0c4c7c` | `25a4674c52e5fb41b302eb68281245d99b6a76a9c1b88d3f1c6df6a19b0c4c7c` | No |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | `a2077f87251e36aea17c4aed0dd37f8e84ce74d61777b3f7a733ae61ce63dcd1` | `a2077f87251e36aea17c4aed0dd37f8e84ce74d61777b3f7a733ae61ce63dcd1` | No |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | `48a305b30d3856153192af6fa68a9a148852e39eb66e52580b635eaf47e9035b` | `48a305b30d3856153192af6fa68a9a148852e39eb66e52580b635eaf47e9035b` | No |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` | `6a7dba6cc9adeb8b1d673e5df585a743e7b643619588af919ff455c5face8e51` | `6a7dba6cc9adeb8b1d673e5df585a743e7b643619588af919ff455c5face8e51` | No |
| `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs` | `2c10f186d50eb3b950de8b8c6106ec2340e3e99e98854916cd695428609e3809` | `2c10f186d50eb3b950de8b8c6106ec2340e3e99e98854916cd695428609e3809` | No |
| `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs` | `09609c24309f3f6ac1677086324e22ee26c4416eb4ceba92fc1e8b3915c09cfa` | `09609c24309f3f6ac1677086324e22ee26c4416eb4ceba92fc1e8b3915c09cfa` | No |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` | `4070961769a076729897d21273c5b8528a3baec75d326bdd47f1858a379fcfa7` | `4070961769a076729897d21273c5b8528a3baec75d326bdd47f1858a379fcfa7` | No |

**Note on the one path with no `before` row.** `QuickFiler.Test/QuickFiler.Test.csproj` is excluded
from CSharpier by `.csharpierignore`'s `*.csproj` pattern, so it is not in the targeted set and the
manifest does not carry it. It is listed here only because it is one of the 25 scope paths. That it
is unchanged is established independently by the porcelain result below, which reports zero modified
paths across the whole nineteen-directory project set; a rewritten `.csproj` would appear there.

## Scope guard — porcelain immediately after the format pass

```
git status --porcelain -- QuickFiler/ QuickFiler.Test/ UtilitiesCS/ UtilitiesCS.Test/ ToDoModel/ ToDoModel.Test/ Tags/ Tags.Test/ TaskMaster/ TaskMaster.Test/ TaskTree/ TaskTree.Test/ TaskVisualization/ TaskVisualization.Test/ TaskVisualizer/ SVGControl/ SVGControl.Test/ VBFunctions/ VBFunctions.Test/
```

Output: **zero lines**, exit code `0`.

This condition can fail rather than being vacuous: P10-T18 committed every source change this feature
made, so the tree was clean before the format pass and any file this pass rewrote — inside the scope
list or outside it — would appear as modified. None did. Consequently the porcelain lists no path
outside the P10-T2 scope list, because it lists no path at all.

The pathspec is the full C# project set from § Execution conventions: every directory holding a
tracked `*.csproj`, plus `TaskVisualizer/`. `docs/` and `.claude/agent-memory/` are deliberately
outside it.

## Loop consequence

No stage failed and **no file was rewritten**, so this stage does not trigger a restart. The loop
proceeds to P11-T3.

Output Summary: The format stage **passes** at loop iteration 1. `dotnet tool run csharpier format .`
exited `0` and reported `Formatted 1547 files in 5053ms.`, which is a processed count and is
deliberately not used as a gate. The SHA-256 comparison over an 1868-file superset of the targeted set
shows **0 files whose before/after hash differ**; the two manifests are byte-identical at
`08f65c82…f567c75`. All 25 scope paths carry identical before and after hashes. The porcelain guard
over the nineteen-directory C# project set returned **zero lines**, so no path outside the P10-T2
scope list was touched and no restart is triggered.
