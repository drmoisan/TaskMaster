# QA Gate — Base reconciliation after re-merging integration tip `5793b8c7`

Timestamp: 2026-08-28T00-14

Command: `git fetch origin epic/quickfiler-bug-family-integration && git merge --no-ff origin/epic/quickfiler-bug-family-integration`

EXIT_CODE: 0

Output Summary: Merge completed with zero conflicts. Divergence before the merge was
`11 behind / 5 ahead`; after the merge it is `0 behind / 6 ahead`. Merge commit
`9c6c0b4adfdfd64b8026484e6b4e1bea17f0d6c2`.

## Why a second base merge was required

Sibling 476 (`webview2-host-initializer-defects-476`) merged as PR #658 (commit `5793b8c7`)
after this feature's prior CI dispatch. The prior CI run therefore gated a head that is not
the head being merged. This artifact records the reconciliation of the branch onto the
current integration tip.

## Divergence proof (0 behind)

Command: `git rev-list --left-right --count origin/epic/quickfiler-bug-family-integration...HEAD`

```
0	6
```

Left column is base-only commits (0), right column is head-only commits (6). The branch
contains the integration tip in full.

## Pure-deletion review

Command: `git diff --numstat origin/epic/quickfiler-bug-family-integration..HEAD | awk '$1==0 && $2>0'`

```
0	50	QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs
```

The requirement is not that this command print nothing — a feature that legitimately relocates
code will always produce rows here. The invariant is that no file loses content the base gained.
That invariant holds for this single row, on two independent grounds:

1. **The base never wrote this file.** `git log 4f238289..5793b8c7 --name-only -- QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`
   returns no commits. None of the 11 base commits touched it, so there is no base-gained content
   in it that could have been lost.
2. **The 50 deleted lines are relocated, not removed.** The deleted span is exactly
   `SetSuggestions`, the `SuggestionsUpgrade` property, `PopulateSuggestionsAsync`, and `AddItems`.
   All four members are present in the new partial
   `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` (+123 lines), which this feature
   adds to satisfy the 500-line file limit while implementing the supersession fix. The type is
   `partial`, so the public surface is unchanged.

## Full production/test numstat

Command: `git diff --numstat origin/epic/quickfiler-bug-family-integration..HEAD -- 'QuickFiler/*' 'QuickFiler.Test/*'`

```
1	0	QuickFiler.Test/QuickFiler.Test.csproj
191	0	QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs
149	0	QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs
74	0	QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs
78	0	QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs
66	0	QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
1	0	QuickFiler/QuickFiler.csproj
123	0	QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs
0	50	QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs
52	8	QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs
39	16	QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
36	2	QuickFiler/Viewers/BreadcrumbMessengerHub.cs
```

Every file this feature edits lies under `QuickFiler/Viewers/Breadcrumb*` or its mirrored test
path. No sibling-owned file is modified.

## Merged siblings' project-file entries preserved

`QuickFiler.Test/QuickFiler.Test.csproj` after the merge carries all twelve entries the parent
verified at the tip, unreordered and unreplaced:

493 (2 entries):

```
    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixture.cs" />
    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />
```

444 (8 entries):

```
    <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerNavigationDigitsTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
    <Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468Tests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468MoveTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468ConversationTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerLayout.StaTests.cs" />
```

476 (2 entries):

```
    <Compile Include="Viewers\WebView2BreadcrumbHostContractTests.cs" />
    <Compile Include="Viewers\WebView2BreadcrumbHostTests.cs" />
```

This feature's own single added entry remains confined to the `Viewers\Breadcrumb*` region:

```
    <Compile Include="Viewers\BreadcrumbBridgeCoordinatorSupersessionTests.cs" />
```
