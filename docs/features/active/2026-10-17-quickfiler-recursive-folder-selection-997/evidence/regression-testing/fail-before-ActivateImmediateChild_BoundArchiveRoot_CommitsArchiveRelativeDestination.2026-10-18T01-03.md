Timestamp: 2026-10-18T01-03
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /TestCaseFilter:Name=ActivateImmediateChild_BoundArchiveRoot_CommitsArchiveRelativeDestination /ResultsDirectory:SCRATCH/issue-997-fail-before-results
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:   Failed ActivateImmediateChild_BoundArchiveRoot_CommitsArchiveRelativeDestination [307 ms]
  Error Message:
   Expected router.GetSelectedFolder() to be a match with the expectation, but it differs at index 0:
Total tests: 1
     Failed: 1
