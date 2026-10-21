# P0-T6 Issue #439 fixture inventory

Timestamp: 2026-08-31T17-08

Command: `Get-Content QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs; rg test methods and helpers; rg BreadcrumbBridgeRouterIssue439Tests.cs QuickFiler.Test/QuickFiler.Test.csproj`

EXIT_CODE: 0

Output Summary: The pre-split fixture has 694 lines and ten MSTest methods. Its only explicit project compile item is `Controllers\BreadcrumbBridgeRouterIssue439Tests.cs`.

Line count: 694

Fully qualified test method identities:

1. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability`
2. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`
3. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome`
4. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget`
5. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget`
6. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget`
7. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild`
8. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows`
9. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic`
10. `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection`

Private helper signatures retained in the original fixture:

- `private static FolderTreeNodeKey Key(string path)`
- `private static IReadOnlyList<FolderBreadcrumbSegment> Chain(string leafPath, string middleName, string leafName)`
- `private static FolderBreadcrumbSegment Segment(string path, string name, bool hasChildren)`

Current explicit compile include: `<Compile Include="Controllers\BreadcrumbBridgeRouterIssue439Tests.cs" />`.
