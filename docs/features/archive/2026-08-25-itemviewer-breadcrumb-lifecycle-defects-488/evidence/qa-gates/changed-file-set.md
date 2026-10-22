# Changed-File Set ([P7-T2])

Timestamp: 2026-08-28T06-12

Command:

```
git add -N <the new test file>
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- . ":(exclude).claude/agent-memory"
git status --porcelain
```

`git add -N` had already been run on
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` so that the new file appears
in the diff; without it an untracked file contributes no path and the check would be silently
incomplete.
EXIT_CODE: 0

## Result

The diff reports **78** paths. **Zero** of them fall outside the permitted set.

That was established mechanically rather than by eye: filtering the 78 paths through a pattern that
admits only the four owned production files, the three owned test files, `QuickFiler.Test.csproj`,
paths under `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/`, and paths under
`docs/features/potential/` leaves **0** remaining lines.

## The 78 paths by category

| Category | Count | Paths |
| --- | --- | --- |
| Owned production files | 4 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`, `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` |
| Owned test files | 3 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs`, `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` |
| Project file | 1 | `QuickFiler.Test/QuickFiler.Test.csproj` |
| Under the feature folder | 69 | `spec.md`, `plan.2026-08-25T09-53.md`, and 67 evidence artifacts and TRX files under `evidence/` |
| Under `docs/features/potential/` | 1 | `docs/features/potential/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md` |

The last category is admitted by decision D-14: the scope criterion enumerates the **source** files
this feature may change, while the follow-up criterion compels creating potential entries, so the two
are reconciled by reading the scope criterion as constraining source files and admitting the
feature-folder and potential-entry documentation paths alongside them.

## `git status --porcelain`

```
?? docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/constraining-tests-final.md
?? docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/trx-p7-t1/
```

Both entries are under
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/` and are this task's immediate
predecessor `[P7-T1]`'s outputs, written moments before this check ran. They are permitted paths.

**No path under `.claude/agent-memory/` appears.** That tolerance was available under the plan's rules
but was not needed: this executor wrote nothing to agent memory.

## What is NOT in the set

No file outside the eight owned source paths was changed. In particular none of the forbidden files
appears — `BreadcrumbDropDownOpenCoordinator.cs`, `BreadcrumbMessengerHub.cs`,
`BreadcrumbBridgeCoordinator.cs`, `BreadcrumbCoordinatorUpgradeLifetime.cs`, `ItemViewer.cs`,
`ItemViewer.Designer.cs`, `ItemViewer.WebViewThread.cs`, `ItemViewer.FolderSearch.cs`,
`ItemViewer.DisplayState.cs`, `ItemViewer.Commands.cs`, every `QfcItemController` partial,
`IBreadcrumbDropDownHost.cs`, `IItemViewer.cs`, `IQfcItemController.cs`, `QuickFiler/QuickFiler.csproj`,
or `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`. `[P7-T3]` verifies that
independently with a targeted diff.

Output Summary: `git diff --name-only <BASE_SHA> -- . ":(exclude).claude/agent-memory"` reports **78**
paths, every one of which is an owned production file, an owned test file,
`QuickFiler.Test/QuickFiler.Test.csproj`, a path under the feature folder, or the single new potential
entry. A mechanical filter for anything outside that set returns **0** paths. `git status --porcelain`
reports only two untracked paths, both `[P7-T1]` outputs under the feature folder.
