# Negative control: createDirectory call removed (P1-T15, expect-fail, AC5)

Timestamp: 2026-09-30T12-29
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger; TASKID p1-t15-build), then vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_Tests.TrySaveAttachmentAsync" "/ResultsDirectory:coverage\test-results\945\p1-t15-run" "/Logger:trx;LogFileName=p1-t15-run.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; TASKID p1-t15-run)
EXIT_CODE: 1
ExpectedExitCode: 1
MUTATED-FILE: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs
MUTATION: removed the statement `createDirectory(Path.GetDirectoryName(filePathSave));`

EXIT_CODE is scoped to the VSTEST_EXIT_CODE of the run.

Output Summary:
MSBUILD_EXIT_CODE: 0
PROD_CSC_OUT_LINES: 2
DLL_ADVANCED: True
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-EXISTS-BEFORE: False
SANDBOX-EXISTS-AFTER: False
VSTEST_EXIT_CODE: 1
COUNTERS total=2 executed=2 passed=0 failed=2
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Failed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Failed
MESSAGE TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile :: Expected events to be equal to {"mkdir:C:\Sortemail945Sandbox\attachments", "save:C:\Sortemail945Sandbox\attachments\saved.txt"}, but {"save:C:\Sortemail945Sandbox\attachments\saved.txt"} contains 1 item(s) less.
MESSAGE TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave :: Expected a <System.IO.IOException> to be thrown, but no exception was thrown.

Reading: with the createDirectory call removed, the success test fails on its recorded-event assertion (the message contains `mkdir:`), and the IOException test fails because the throwing delegate is never called (the message contains `IOException`). The run has no side effect: the sandbox directory does not exist before or after. This matches the D-6 prediction.
