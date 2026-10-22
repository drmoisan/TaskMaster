# [P3-T11] Scope boundary of the changed source set

Timestamp: 2026-09-07T08-15

Command: `git add --intent-to-add -- '*.cs' '*.csproj'`; `git diff --name-only <BASE-SHA> -- '*.cs' '*.csproj'`; `git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'`; then per-path `git diff --name-only <BASE-SHA> -- <path>` and `git status --porcelain --untracked-files=all -- <path>` over the fifteen paths asserted absent

EXIT_CODE: 0

ExpectedExitCode: 0

## Anchored diff, name-listing (`git diff --name-only <BASE-SHA> -- '*.cs' '*.csproj'`)

```
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs
QuickFiler/Controllers/QfcItemController.FolderHandling.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/QuickFiler.csproj
UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs
UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs
UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs
UtilitiesCS/UtilitiesCS.csproj
```

ENUMERATED-PATH-COUNT: 20

## Porcelain status companion (`git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'`)

```
 M QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs
 M QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs
 M UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs
 M UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
 M UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs
```

## Why both are listed side by side

Neither mechanism alone is correct in both states. The anchored diff cannot see an untracked path, which is why
the `git add --intent-to-add` companion runs first. Porcelain status goes empty once a change is committed, which
is why it shows only the nine files the [P3-T1] formatter touched after the Phase 2 commit at `f50fb727` rather
than the whole footprint. The anchored diff is the authoritative enumeration here; the porcelain output is the
untracked-visibility companion and confirms the nine uncommitted rewrites are all inside the same twenty-path set.
The porcelain set is a strict subset of the anchored set, with no path present in one and absent from the other in
the direction that would indicate leakage.

## The enumerated set is exactly the twenty Write Set paths

### Nine production paths

1. `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` — CREATE
2. `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` — CREATE
3. `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` — MODIFY
4. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — MODIFY
5. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` — MODIFY
6. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` — MODIFY
7. `QuickFiler/Controllers/EfcFormController.cs` — MODIFY
8. `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` — CREATE
9. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — MODIFY

### Five new test paths

10. `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs`
11. `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs`
12. `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs`
13. `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs`
14. `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs`

### Two retargeted test paths

15. `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs`
16. `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`

### Four project files

17. `UtilitiesCS/UtilitiesCS.csproj`
18. `QuickFiler/QuickFiler.csproj`
19. `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
20. `QuickFiler.Test/QuickFiler.Test.csproj`

Twenty enumerated, twenty accounted for, none left over in either direction.

## Per-path no-hunk assertion over the fifteen paths this task names as absent

Each path was queried individually rather than inferred from absence in the list above, so the assertion is
mechanical. `TRACKED` confirms the path exists in the index at the base commit, which is what makes a zero diff a
real observation rather than the trivially empty result of naming a nonexistent path.

| Path | TRACKED | anchored diff lines | porcelain lines |
|---|---|---|---|
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | True | 0 | 0 |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs | True | 0 | 0 |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs | True | 0 | 0 |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs | True | 0 | 0 |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | True | 0 | 0 |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs | True | 0 | 0 |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs | True | 0 | 0 |
| QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs | True | 0 | 0 |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs | True | 0 | 0 |
| UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs | True | 0 | 0 |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs | True | 0 | 0 |
| UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs | True | 0 | 0 |
| UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs | True | 0 | 0 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs | True | 0 | 0 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs | True | 0 | 0 |

SIBLING-OWNED-FILES-WITH-A-HUNK: 0 of 6
D8-NO-HUNK-TEST-FILES-WITH-A-HUNK: 0 of 2
OTHER-EXCLUDED-FILES-WITH-A-HUNK: 0 of 7

The first six rows are the sibling-owned files D1 names, which a concurrent sibling item owns and which decision
D-B forbids this item from editing. The last two rows are the two #439 Efc router test files D8 marks NO HUNK: the
AC1/AC2 trim lives inside the provider's GetAncestorChainAsync, below the strict-mock boundary every test in both
files uses, so neither file could need an edit, and editing their shared `Chain` helper would have broken the
unrelated #614 boundary test Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection. All ten tests of that
partial class pass in [P2-T16] and in the [P3-T5] full run, which is the behavioural confirmation that the
no-hunk finding was correct rather than an omission.

## Scope of this enumeration (R7)

The pathspec is `'*.cs' '*.csproj'` only. This plan additionally writes evidence artifacts under
`<FEATURE>/evidence/` and checks off AC boxes in `spec.md`, and it updates `issue.md` and the plan checklist; none
of those is a source file and none is in this enumeration. That is the intended scope, not an omission. Because
the QuickFiler resources directory contains no `.cs` and no `.csproj` file, an assertion that it is absent from
THIS enumeration would be true for every possible execution and would gate nothing, which is why [P3-T20] makes
the AC8 asset check against its own separately anchored diff rather than reading it off this artifact.

Output Summary: The changed source set under the R7 pathspec is exactly the twenty Write Set paths — nine
production, five new test, two retargeted test and four project files — with nothing extra and nothing missing.
All fifteen paths this task names as absent were queried individually and every one returned zero anchored-diff
lines and zero porcelain lines while being confirmed tracked at the base commit. The six sibling-owned files carry
no hunk, and the two issue-439 test files carry no hunk.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
