# [P2-T16] Pass-after run of every test in the [P1-T18] red inventory

Timestamp: 2026-09-07T07-36

Command:

```
<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\799-p2-t16-ut /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:FullyQualifiedName~ArchiveStemProjectionTests|FullyQualifiedName~ArchiveChainProjectionTests|FullyQualifiedName~OutlookFolderHierarchyProviderTrimTests|FullyQualifiedName~FolderPredictorRecentsProjectionTests|FullyQualifiedName~OutlookFolderHierarchyProviderTests
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\799-p2-t16-qft /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterScoreJoinTests|FullyQualifiedName~QfcItemController_FolderHandlingTests|FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests
```

`<vstest>` is the vswhere-resolved vstest.console.exe path pinned by [P0-T7].

EXIT_CODE: 0

ExpectedExitCode: 0

EXIT-CODE-UT: 0

EXIT-CODE-QFT: 0

FAILED-UT: 0

FAILED-QFT: 0

P2-T16-TOTAL-RUN: 83

P2-T16-TOTAL-PASSED: 83

## Output Summary

Both invocations printed `Test Run Successful.` The UtilitiesCS.Test run reported
`Total tests: 43 / Passed: 43`; the QuickFiler.Test run reported `Total tests: 40 / Passed: 40`.
`FAILED-UT` and `FAILED-QFT` are read from each run's TRX `ResultSummary/Counters` `failed`
attribute, not from the console, because vstest prints no `Failed:` line at all on a fully passing
run.

All 33 tests of the [P1-T18] red inventory are `Passed`. The two run totals are larger than 33
because the filters select whole classes and therefore also re-run tests that were already green at
the end of Phase 1; per the task text those totals are recorded but not asserted against the
inventory count.

## TRX documents read (R3-reduced names)

- UtilitiesCS.Test: `TestResults\799-p2-t16-ut\<user>_<host>_2026-09-07_07_36_26_net481.trx`
- QuickFiler.Test: `TestResults\799-p2-t16-qft\<user>_<host>_2026-09-07_07_36_38_net481.trx`

Each results directory held exactly one TRX at read time.

## PASS-AFTER lines (33, one per [P1-T18] inventory entry)

PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_PathStrictlyUnderRoot_ReturnsArchiveRelativeStem
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_PathEqualsRoot_ReturnsInputUnchanged
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_FalsePrefixSiblingArchive2_ReturnsInputUnchanged
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_RootWithOneTrailingSeparator_ReturnsArchiveRelativeStem
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_RootWithTwoTrailingSeparators_ReturnsArchiveRelativeStem
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_EmptyRoot_ReturnsInputUnchanged
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_WhitespaceOnlyRoot_ReturnsInputUnchanged
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_NullPath_ReturnsNull
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_EmptyPath_ReturnsInputUnchanged
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_ForwardSlashSeparators_ReturnsArchiveRelativeStem
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_MixedCaseRoot_ReturnsArchiveRelativeStem
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_ChainPassesThroughRoot_ReturnsSegmentsAfterTheRoot
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_ChainMissesTheRoot_ReturnsFalseAndEmptyOutput
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_LeafIsTheRoot_ReturnsFalseAndEmptyOutput
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_EmptyChain_ReturnsFalseAndEmptyOutput
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_SingleElementChainIsTheRoot_ReturnsFalse
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_RootWithTrailingSeparator_ReturnsSegmentsAfterTheRoot
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_FalsePrefixSiblingArchive2_ReturnsFalse
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderRowArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderArray_OutOfRootRecentEntry_IsLeftUnchanged
PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTests.GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned
PASS-AFTER: QuickFiler.Controllers.Tests.QfcItemController_FolderHandlingTests.ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection
PASS-AFTER: QuickFiler.Controllers.Tests.QfcItemController_FolderHandlingTests.AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder

PASS-AFTER line count: 33. [P1-T18] inventory count: 33. Equal.

## The seven deliberately-green pins are still green

These are not inventory entries; they are recorded here because a red result for any of them would
be a regression introduced by Phase 2 rather than an expected transition.

- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_WithoutRootAccessor_ReturnsTheUntrimmedChain — Passed
- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_RootAccessorThrows_DoesNotThrowAndReturnsTheUntrimmedChain — Passed
- UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderRowArray_AndFolderArray_AgreeOnRecentTextAfterProjection — Passed
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage — Passed
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged — Passed
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_MixedRowSet_RendersLineageOnFolderRowsOnly — Passed
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_AmbiguousLabel_IsNotSuppressed — Passed

## The ten D8 no-hunk #439 tests are still green

The QuickFiler.Test filter deliberately includes `BreadcrumbBridgeRouterIssue439Tests`, because
[P2-T12] states their continued passing as the observable proof that suppression is inert behind a
provider mock that does not implement the absence report. All ten are `Passed`:
Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability,
Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively,
Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome,
Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows,
Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic,
Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection,
Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget,
Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget,
Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget and
Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild.

## Supplementary run: the two ToDoModel.Test assertions named by [P2-T9]

[P2-T9]'s acceptance condition names two ToDoModel.Test assertions in directory Email Utilities,
file FolderHandlerTests_Written.cs, and the plan records that no Phase 2 command covers that
assembly. They were therefore run separately so the condition rests on evidence rather than on
deferral to [P3-T5]:

```
<vstest> ToDoModel.Test\bin\Debug\ToDoModel.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\799-p2-t9-todomodel /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:FullyQualifiedName~GetRelevantOlPathPortion
```

`Test Run Successful.` with `Total tests: 2 / Passed: 2`, exit code 0:

- PASS-AFTER: GetRelevantOlPathPortion_StateUnderTest_ExpectedBehavior1
- PASS-AFTER: GetRelevantOlPathPortion_StateUnderTest_ExpectedBehavior2

TRX read (R3-reduced name):
`TestResults\799-p2-t9-todomodel\<user>_<host>_2026-09-07_07_41_15_net481.trx`.

These two are not [P1-T18] inventory entries and are not counted in the 33 above; they are the
must-stay-green side of the [P2-T9] rewrite. [P3-T5] remains the scheduled full-set check.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact. The TRX file
names are recorded with the account and machine segments replaced by `<user>` and `<host>`, and no
TRX content was pasted: only parsed counter values and test names were read.
