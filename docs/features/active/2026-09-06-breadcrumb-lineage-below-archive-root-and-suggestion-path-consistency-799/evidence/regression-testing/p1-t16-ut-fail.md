# [P1-T16] [expect-fail] UtilitiesCS.Test fail-before run

Timestamp: 2026-09-07T07-15

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\799-p1-t16 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:FullyQualifiedName~ArchiveStemProjectionTests|FullyQualifiedName~ArchiveChainProjectionTests|FullyQualifiedName~OutlookFolderHierarchyProviderTrimTests|FullyQualifiedName~FolderPredictorRecentsProjectionTests|FullyQualifiedName~OutlookFolderHierarchyProviderTests`

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

`Test Run Failed.` — total 43, passed 15, failed 28. The failing set is the intended Phase 1 red
set: 28 tests fail before the Phase 2 production change, and no test fails for an unrelated reason.

## TRX read

TRX file (name reduced per R3): `<user>_<host>_2026-09-07_07_15_03_net481.trx`, the most recently
modified TRX under the results directory for this task. Counter values are read from the
`ResultSummary/Counters` element. No raw TRX content is pasted (R3).

- TOTAL: 43
- PASSED: 15
- FAILED: 28

## Suite selection (R13)

This is a SINGLE-assembly run driven by an explicit `FullyQualifiedName~` class filter over five
UtilitiesCS.Test classes. None of the four environmentally-hanging shell-icon classes
(`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`,
`HelperClasses.SysImageListHelperTests`, `EmailIntelligence.OSBrowser_Tests`) is selected by that
filter, so the R13 exclusion clauses are unnecessary here and the reduced denominator is the 43
tests of the five named classes. The filter contains no `&` clause, so the `&`-binds-tighter-than-`|`
precedence hazard does not apply.

## Failing tests, by fully qualified name and cause

`SEAM-BLOCKED` means the test fails because a Phase 1 declaration seam throws
`NotImplementedException`; `NEW` means the test is new and fails because the production behaviour is
not yet changed; `RETARGETED` means an existing test was retargeted to the new specification and
fails against the unchanged production behaviour.

### SEAM-BLOCKED (19) — `ArchiveStemProjection.ToDisplayStem` seam

- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_PathStrictlyUnderRoot_ReturnsArchiveRelativeStem
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_PathEqualsRoot_ReturnsInputUnchanged
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_FalsePrefixSiblingArchive2_ReturnsInputUnchanged
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_RootWithOneTrailingSeparator_ReturnsArchiveRelativeStem
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_RootWithTwoTrailingSeparators_ReturnsArchiveRelativeStem
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_EmptyRoot_ReturnsInputUnchanged
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_WhitespaceOnlyRoot_ReturnsInputUnchanged
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_NullPath_ReturnsNull
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_EmptyPath_ReturnsInputUnchanged
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_ForwardSlashSeparators_ReturnsArchiveRelativeStem
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemProjectionTests.ToDisplayStem_MixedCaseRoot_ReturnsArchiveRelativeStem

### SEAM-BLOCKED — `ArchiveChainProjection.TryTrimBelowArchiveRoot` seam

- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_ChainPassesThroughRoot_ReturnsSegmentsAfterTheRoot
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_ChainMissesTheRoot_ReturnsFalseAndEmptyOutput
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_LeafIsTheRoot_ReturnsFalseAndEmptyOutput
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_EmptyChain_ReturnsFalseAndEmptyOutput
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_SingleElementChainIsTheRoot_ReturnsFalse
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_RootWithTrailingSeparator_ReturnsSegmentsAfterTheRoot
- UtilitiesCS.Test.OutlookObjects.Folder.ArchiveChainProjectionTests.TryTrimBelowArchiveRoot_FalsePrefixSiblingArchive2_ReturnsFalse

### SEAM-BLOCKED — `OutlookFolderHierarchyProvider.IsAbsentLabel` seam

- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport

### NEW (8)

- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot
- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty
- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty
- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence
- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence
- UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem
- UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderRowArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem
- UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderArray_OutOfRootRecentEntry_IsLeftUnchanged

### RETARGETED (1)

- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTests.GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath

## Acceptance check

- `EXIT_CODE: 1` — met.
- The recorded failure set includes `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot` — met.
- The recorded failure set includes `FolderArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem` — met.

## New tests that are GREEN at the end of Phase 1 (3), recorded so the inventory is not misread

These three new tests pin behaviour that already holds today, so a red result for them would have
been a finding rather than a result. They are NOT part of the [P1-T18] red inventory.

- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_WithoutRootAccessor_ReturnsTheUntrimmedChain
- UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTrimTests.GetAncestorChainAsync_RootAccessorThrows_DoesNotThrowAndReturnsTheUntrimmedChain
- UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorRecentsProjectionTests.FolderRowArray_AndFolderArray_AgreeOnRecentTextAfterProjection

The remaining 12 passing tests are the untouched tests of
UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyProviderTests, which the [P1-T13]
retarget left unchanged.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact. The TRX file
name is recorded with `<user>` and `<host>` substituted. No raw TRX content is pasted.
