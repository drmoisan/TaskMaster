Timestamp: 2026-08-31T10:41:32-04:00
Command: `pwsh -NoProfile -Command '<resolved vstest command> /TestCaseFilter:FullyQualifiedName~EfcDataModelIssue637Tests&TestCategory!=LiveOutlook /Logger:trx /ResultsDirectory:coverage\\testresults\\p2-t12'`
ExpectedExitCode: 1
EXIT_CODE: 1
Output Summary: 8 tests executed; 6 passed and 2 failed. No "No test matches the given testcase filter" output occurred.

Expected failing tests:

- `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem`
- `ToFilingStemOrVerbatim_RootedUnderCaseDifferingAncestor_ReturnsTheStem`

The other six helper tests passed, confirming that the seam already returns the candidate input verbatim for non-normalizable cases.
