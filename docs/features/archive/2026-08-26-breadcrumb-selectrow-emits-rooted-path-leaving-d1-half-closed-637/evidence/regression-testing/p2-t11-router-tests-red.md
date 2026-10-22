Timestamp: 2026-08-31T10:41:32-04:00
Command: `pwsh -NoProfile -Command '<resolved vstest command> /TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue637Tests&TestCategory!=LiveOutlook /Logger:trx /ResultsDirectory:coverage\\testresults\\p2-t11'`
ExpectedExitCode: 1
EXIT_CODE: 1
Output Summary: 10 tests executed; 5 passed and 5 failed. No "No test matches the given testcase filter" output occurred.

Expected failing tests:

- `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected`
- `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection`
- `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`
- `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`
- `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`

Expected passing tests:

- `RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim`
- `RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim`
- `RowSelected_OutOfRootRootedTarget_IsStillRejected`
- `RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected`
- `RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim`
