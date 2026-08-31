Timestamp: 2026-08-31T10:43:55-04:00
Command: `pwsh -NoProfile -Command '<resolved vstest command> /TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue637Tests&TestCategory!=LiveOutlook /Logger:trx /ResultsDirectory:coverage\\testresults\\p3-t5'`
EXIT_CODE: 0
Output Summary: 10 tests total; 10 passed; 0 failed; 0 skipped.

The five former failures now pass:

- `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected`
- `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection`
- `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`
- `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`
- `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`
