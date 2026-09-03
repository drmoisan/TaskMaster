# P1-T4 [expect-fail]: RED -- New Regression Test Fails Against Pre-Fix Defect

Timestamp: 2026-09-03T11-30

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~CreateFolder_WhenParentBranchPathIsEmpty_DoesNotThrowIndexOutOfRangeException" /Logger:trx /ResultsDirectory:coverage\trx\p1-t4
ExpectedExitCode: 1
EXIT_CODE: 1

Output Summary:
"Test Run Failed. Total tests: 1 Failed: 1." (Passed: 0, implicit from "Failed: 1" of
"Total tests: 1"). Error Message: "Did not expect any exception, but found
System.IndexOutOfRangeException: Index was outside the bounds of the array." at
UtilitiesCS.FolderPredictor.CreateFolder(...) FolderPredictor.cs:line 691. This
confirms the RED half of the RED/GREEN pair required by AC5: the new test fails against
the pre-fix defect, and the failure message contains the single-line token
`IndexOutOfRangeException`. TRX results file:
coverage\trx\p1-t4\DanMoisan_MEGALODON4_2026-09-03_07_30_26_net481.trx (gitignored
under coverage/*). EXIT_CODE 1 is the expected outcome for this [expect-fail] task, not
a toolchain failure.
