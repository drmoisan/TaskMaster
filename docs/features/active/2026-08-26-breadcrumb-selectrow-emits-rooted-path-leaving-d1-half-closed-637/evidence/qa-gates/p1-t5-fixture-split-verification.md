# P1-T5 Issue #439 fixture split verification

Timestamp: 2026-08-31T17-10

Command: `Get-Content fixture line counts; rg public test methods and Compile Include entries; git diff --exit-code -- docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637/spec.md; parse QuickFiler.Test/QuickFiler.Test.csproj as XML`

EXIT_CODE: 0

Output Summary: The original partial fixture is 455 lines and the activation partial fixture is 253 lines. Each is within the 500-line limit. The two partial files provide exactly the ten pre-split test identities, with one explicit compile include each. `spec.md` is byte-for-byte unchanged from `HEAD`, including AC21.

Fixture line counts:

- `BreadcrumbBridgeRouterIssue439Tests.cs`: 455
- `BreadcrumbBridgeRouterIssue439Tests.Activation.cs`: 253

Fully qualified test identities (each appears exactly once):

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

Explicit project items:

- `<Compile Include="Controllers\BreadcrumbBridgeRouterIssue439Tests.cs" />` appears once.
- `<Compile Include="Controllers\BreadcrumbBridgeRouterIssue439Tests.Activation.cs" />` appears once.

The project file parsed as well-formed XML. `git diff --exit-code` for `spec.md` returned 0; AC21 remains byte-for-byte unchanged.
