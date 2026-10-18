Timestamp: 2026-10-18T01-03
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /TestCaseFilter:Name=CollapseAncestor_SelectedChild_PreservesCommittedDestination /ResultsDirectory:SCRATCH/issue-997-fail-before-results
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:   Failed CollapseAncestor_SelectedChild_PreservesCommittedDestination [223 ms]
  Error Message:
   Test method UtilitiesCS.Test.OutlookObjects.Folder.BreadcrumbDescendantSelectionIssue997Tests.CollapseAncestor_SelectedChild_PreservesCommittedDestination threw exception:
System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection.
Total tests: 1
     Failed: 1

