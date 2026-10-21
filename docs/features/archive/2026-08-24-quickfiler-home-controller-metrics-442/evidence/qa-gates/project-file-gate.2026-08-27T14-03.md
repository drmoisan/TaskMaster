# Phase 7 — Project-File and New-Source Gate (re-run)

Timestamp: 2026-08-27T14-03
Task: [P7-T7]
Command: three commands, recorded individually below
EXIT_CODE: 0

## Result: PASS against the merge base

All three commands produce no output lines when run against the merge base, which is the comparison
point that isolates this feature's contribution.

`git merge-base HEAD origin/epic/quickfiler-bug-family-integration` resolves to
`0ddab4107b3b147e706a6c15856888b3b5d6404b`.
`git rev-list --left-right --count origin/epic/quickfiler-bug-family-integration...HEAD` reports
`0 6`: the branch is 6 ahead and 0 behind, so the merge base equals the current origin integration
tip.

| # | Command | Output lines |
| --- | --- | --- |
| 1 | `git diff --name-only 0ddab4107b3b147e706a6c15856888b3b5d6404b -- "*.csproj" "*.props" "*.targets"` | 0 |
| 2 | `git diff --name-only --diff-filter=A 0ddab4107b3b147e706a6c15856888b3b5d6404b -- "*.cs"` | 0 |
| 3 | `git ls-files --others --exclude-standard -- "*.cs"` | 0 |

The two-dot form is deliberately omitted from commands 1 and 2 so the comparison includes
uncommitted working-tree changes as well as committed ones. Command 3 is required because
`git diff` never lists untracked files, so a forbidden newly created `.cs` file would otherwise be
invisible.

## Why `BASELINE_SHA` is recorded but not used as the gate

The plan names `BASELINE_SHA` (`363bfcdd4da5a24743ee665ea9fd124bc42239ff`, recorded by [P0-T2]).
That was the branch point, but the branch has since merged the epic integration branch through merge
commit `c1826965`, so a diff against `BASELINE_SHA` reports every project file and every new source
file that *integration* gained from its own siblings. Both figures are recorded here so the
distinction is auditable and neither is hidden.

**Command 1 against `BASELINE_SHA` — six output lines, none of them this feature's:**

```
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/QuickFiler.csproj
TaskMaster.Test/TaskMaster.Test.csproj
TaskMaster/TaskMaster.csproj
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/UtilitiesCS.csproj
```

**Command 2 against `BASELINE_SHA` — twenty output lines, none of them this feature's:**

```
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs
QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs
QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs
QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs
QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs
QuickFiler/Controllers/EfcSelectionGuard.cs
TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs
TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs
TaskMaster/AppGlobals/ArchiveRootPathGuard.cs
UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs
UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs
```

Every one of those twenty files arrived through the merge and belongs to a sibling epic child
(breadcrumb/router work, the 468 collection-controller work, and the #614 archive-root work). This
feature added no `.cs` file and touched no project file: it delivered all of its production and test
code inside the seven pre-existing owned files, which is exactly what AC-20 requires.

## Effect on acceptance criteria

AC-20 ("no project-file edit, no new source file") is satisfied. The evidence pointer for its
check-off is this artifact.
