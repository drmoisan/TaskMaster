# P1-T1 Issue #439 partial-fixture split map

Timestamp: 2026-08-31T17-09

The following four contiguous activation tests move, in their existing source order, to `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs`:

1. `Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget`
2. `Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget`
3. `Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget`
4. `Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild`

The original `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` retains these six tests:

1. `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability`
2. `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`
3. `Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome`
4. `Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows`
5. `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic`
6. `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection`

It also retains the `Key`, `Chain`, and `Segment` private helpers. Both files declare the same partial MSTest class, `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests`; therefore every test retains its existing fully qualified method identity.
