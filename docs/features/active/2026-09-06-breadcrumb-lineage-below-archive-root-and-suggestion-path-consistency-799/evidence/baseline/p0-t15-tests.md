# [P0-T15] Pre-change status of the retargeted and must-stay-green tests

Timestamp: 2026-09-07T07-20

Command: TRX `TestDefinitions/UnitTest` to `Results/UnitTestResult` join over the two TRX documents [P0-T11]
wrote, resolving each fully qualified name as `TestMethod/@className` + `.` + `TestMethod/@name` and reading
`UnitTestResult/@outcome`

EXIT_CODE: 0

Source documents (R3-reduced filenames, no TRX content pasted):

- `TestResults\799-p0-t11-ut\<user>_<host>_2026-09-07_06_44_06_net481.trx`
- `TestResults\799-p0-t11-qft\<user>_<host>_2026-09-07_06_46_42_net481.trx`

## The two D9 retarget targets

BASELINE-PASS: UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTests.GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments
BASELINE-PASS: QuickFiler.Controllers.Tests.QfcItemController_FolderHandlingTests.ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection

## The D9 recents and projection tests that must stay green

BASELINE-PASS: UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.FolderArray_WhenSuggestionsAndRecentsExist_ReturnsSuggestionsThenRecents
BASELINE-PASS: UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.AddRecents_WhenRecentsExist_AppendsHeaderAndEntries
BASELINE-PASS: UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.Issue609_FolderPredictor_ProjectsOnlyInRootFullSuggestionPaths
BASELINE-PASS: UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath
BASELINE-PASS: UtilitiesCS.Test.OutlookObjects.Folder.FolderRowTests.FolderRowArray_WithSuggestionsAndRecents_MatchesFolderArrayTextAndTagsKinds
BASELINE-PASS: UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.GetOlSubpath_WhenAncestorEndsWithSlashOrChildrenExcluded_ReturnsExpectedSegment

## The ten tests of the partial class BreadcrumbBridgeRouterIssue439Tests (D8 NO HUNK)

BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget
BASELINE-PASS: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests.Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild

Count: 18 `BASELINE-PASS:` lines, one per named test, 0 `BASELINE-FAIL:` lines. Every one of the 18 named tests was
located in the TRX documents this plan wrote — 7 in the UtilitiesCS.Test document and 11 in the QuickFiler.Test
document — so no line is recorded from an assumption. All ten members of the `BreadcrumbBridgeRouterIssue439Tests`
partial class resolve to the single class name `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue439Tests`,
which spans the base file and the Activation partial.

Output Summary: All 18 tests pass at the base commit. The set includes
Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome, which pins today's null-chain
selectable-fallback rendering and is exactly the path [P2-T12] modifies, so the modified path is guarded on the
baseline side. Because every entry is `BASELINE-PASS:`, the Phase 2 no-newly-failing comparison reduces to a
requirement that all 18 still pass after the change, with the two D9 retarget targets excepted in the specific
respect that [P1-T13] and [P1-T14] rewrite their assertions.
