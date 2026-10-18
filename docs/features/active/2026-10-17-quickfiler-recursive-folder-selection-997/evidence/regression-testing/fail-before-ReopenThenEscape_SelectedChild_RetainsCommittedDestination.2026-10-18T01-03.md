Timestamp: 2026-10-18T01-03
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /TestCaseFilter:Name=ReopenThenEscape_SelectedChild_RetainsCommittedDestination /ResultsDirectory:SCRATCH/issue-997-fail-before-results
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:   Failed ReopenThenEscape_SelectedChild_RetainsCommittedDestination [304 ms]
  Error Message:
   Expected router.GetSelectedFolder() to be a match with the expectation, but it differs at index 0:
Total tests: 1
     Failed: 1

