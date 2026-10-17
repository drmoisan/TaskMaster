# Issue 995 issue-995-enter-red

Timestamp: 2026-10-17T23-03
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /TestCaseFilter:FullyQualifiedName~Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView /Settings:scripts/vscode/TaskMaster.cli.runsettings /ResultsDirectory:SCRATCH/issue-995-red-results
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Exactly1 regression failed against unchanged production BridgeJs because expected rowConfirmed dispatch is absent. Compilation succeeded EXIT0; test is discovered and assertion fails for the reported behavior. Full TRX/log retained under SCRATCH. No EFC production source changed before this failure.
