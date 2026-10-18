Timestamp: 2026-10-18T01-14
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /TestCaseFilter:FullyQualifiedName~Issue997 /Logger:console;verbosity=minimal /ResultsDirectory:SCRATCH/issue-997-pass-after-results
EXIT_CODE: 0
ExpectedExitCode: 0
Output Summary: VSTest version 18.10.0 (x64)

Starting test execution, please wait...
A total of 2 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    25, Skipped:     0, Total:    25, Duration: 360 ms - UtilitiesCS.Test.dll (net481)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 1 s - QuickFiler.Test.dll (net481)

Original fail-before records: fail-before-ActivateImmediateChild_BoundArchiveRoot_CommitsArchiveRelativeDestination.2026-10-18T01-03.md, fail-before-CollapseAncestor_SelectedChild_PreservesCommittedDestination.2026-10-18T01-03.md, fail-before-RenderDescendantWithChildren_OffersIndependentExpansionControl.2026-10-18T01-03.md, fail-before-ReopenThenEscape_SelectedChild_RetainsCommittedDestination.2026-10-18T01-03.md
Only arrangement change: ActivateImmediateChild_BoundArchiveRoot_CommitsArchiveRelativeDestination now explicitly binds the fixture root. All original destination/collapse/Escape/control assertions remain. Behavioral host tests and compiled asset wiring/syntax contracts; no live Outlook verification.

