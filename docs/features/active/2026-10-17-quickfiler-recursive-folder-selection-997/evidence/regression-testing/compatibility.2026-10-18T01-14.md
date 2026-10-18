Timestamp: 2026-10-18T01-14
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /TestCaseFilter:FullyQualifiedName~Breadcrumb|FullyQualifiedName~ItemViewerBreadcrumb /Logger:console;verbosity=minimal /ResultsDirectory:SCRATCH/issue-997-compatibility-results
EXIT_CODE: 0
ExpectedExitCode: 0
Output Summary: VSTest version 18.10.0 (x64)

Starting test execution, please wait...
A total of 2 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   332, Skipped:     0, Total:   332, Duration: 460 ms - UtilitiesCS.Test.dll (net481)
Passed!  - Failed:     0, Passed:   513, Skipped:     0, Total:   513, Duration: 6 s - QuickFiler.Test.dll (net481)


