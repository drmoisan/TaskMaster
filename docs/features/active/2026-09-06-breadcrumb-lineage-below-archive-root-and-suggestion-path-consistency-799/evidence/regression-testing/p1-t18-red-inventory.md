# [P1-T18] End-of-Phase-1 red inventory

Timestamp: 2026-09-07T07-17

Command: consolidation of the two fail-before artifacts written by [P1-T16] and [P1-T17]; no new
command was run.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

33 tests are red at the end of Phase 1: 28 from the [P1-T16] UtilitiesCS.Test run and 5 from the
[P1-T17] QuickFiler.Test run. This is the set Phase 2 must turn green, and nothing else.

## Arithmetic check

- [P1-T16] recorded failure count: 28
- [P1-T17] recorded failure count: 5
- Sum: 33
- Entries in this inventory: 33

## Tag totals

- `SEAM-BLOCKED`: 19
- `NEW`: 11
- `RETARGETED`: 3
- Total: 33

## Inventory

Every entry carries exactly one tag.

| # | Fully qualified name | Tag |
|---|---|---|
| 1 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_PathStrictlyUnderRoot_ReturnsArchiveRelativeStem | SEAM-BLOCKED |
| 2 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_PathEqualsRoot_ReturnsInputUnchanged | SEAM-BLOCKED |
| 3 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_FalsePrefixSiblingArchive2_ReturnsInputUnchanged | SEAM-BLOCKED |
| 4 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_RootWithOneTrailingSeparator_ReturnsArchiveRelativeStem | SEAM-BLOCKED |
| 5 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_RootWithTwoTrailingSeparators_ReturnsArchiveRelativeStem | SEAM-BLOCKED |
| 6 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_EmptyRoot_ReturnsInputUnchanged | SEAM-BLOCKED |
| 7 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_WhitespaceOnlyRoot_ReturnsInputUnchanged | SEAM-BLOCKED |
| 8 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_NullPath_ReturnsNull | SEAM-BLOCKED |
| 9 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_EmptyPath_ReturnsInputUnchanged | SEAM-BLOCKED |
| 10 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_ForwardSlashSeparators_ReturnsArchiveRelativeStem | SEAM-BLOCKED |
| 11 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_MixedCaseRoot_ReturnsArchiveRelativeStem | SEAM-BLOCKED |
| 12 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_ChainPassesThroughRoot_ReturnsSegmentsAfterTheRoot | SEAM-BLOCKED |
| 13 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_ChainMissesTheRoot_ReturnsFalseAndEmptyOutput | SEAM-BLOCKED |
| 14 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_LeafIsTheRoot_ReturnsFalseAndEmptyOutput | SEAM-BLOCKED |
| 15 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_EmptyChain_ReturnsFalseAndEmptyOutput | SEAM-BLOCKED |
| 16 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_SingleElementChainIsTheRoot_ReturnsFalse | SEAM-BLOCKED |
| 17 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_RootWithTrailingSeparator_ReturnsSegmentsAfterTheRoot | SEAM-BLOCKED |
| 18 | UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_FalsePrefixSiblingArchive2_ReturnsFalse | SEAM-BLOCKED |
| 19 | UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport | SEAM-BLOCKED |
| 20 | UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot | NEW |
| 21 | UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty | NEW |
| 22 | UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty | NEW |
| 23 | UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence | NEW |
| 24 | UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence | NEW |
| 25 | UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem | NEW |
| 26 | UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderRowArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem | NEW |
| 27 | UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderArray_OutOfRootRecentEntry_IsLeftUnchanged | NEW |
| 28 | UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTests.GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath | RETARGETED |
| 29 | QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage | NEW |
| 30 | QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey | NEW |
| 31 | QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned | NEW |
| 32 | QuickFiler.Controllers.Tests.QfcItemController_FolderHandlingTests.ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection | RETARGETED |
| 33 | QuickFiler.Controllers.Tests.QfcItemController_FolderHandlingTests.AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder | RETARGETED |

## Notes for Phase 2

- Entry 33 was not named as a retarget target by decision D9, which named only entries 28 and 32.
  It was carried into the retarget by [P1-T14]'s explicit instruction to re-derive the assertions of
  the following test whose prose encoded the removed empty-root strip. Its assertions encoded that
  strip, so they were re-derived. This is recorded here rather than left for a reviewer to discover.
- Seven new tests are GREEN at the end of Phase 1 and are deliberately absent from this inventory,
  because they pin behaviour that already holds. They are enumerated in the [P1-T16] and [P1-T17]
  artifacts.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
