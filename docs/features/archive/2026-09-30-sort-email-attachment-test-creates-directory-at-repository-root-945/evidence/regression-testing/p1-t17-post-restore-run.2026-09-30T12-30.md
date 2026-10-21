# P1-T17 confirmation run after the restore

Timestamp: 2026-09-30T12-30
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger; TASKID p1-t17-build), then vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_Tests.TrySaveAttachmentAsync" "/ResultsDirectory:coverage\test-results\945\p1-t17-run" "/Logger:trx;LogFileName=p1-t17-run.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; TASKID p1-t17-run)
EXIT_CODE: 0

EXIT_CODE is scoped to the VSTEST_EXIT_CODE of the run.

Rebuild note: the first build after the restore returned exit 0 with PROD_CSC_OUT_LINES 0 and DLL_ADVANCED False, because the restore copied the backup with the backup's older last-write time, so the incremental check skipped compilation and the test output still held the mutated production code. The last-write time of the restored SortEmail.cs was refreshed (metadata only; SHA256 unchanged at 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B) and the build was re-run; the values below are from that re-run.

Output Summary:
MSBUILD_EXIT_CODE: 0
PROD_CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
DLL_ADVANCED: True
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-EXISTS-AFTER: False
COUNTERS total=2 executed=2 passed=2 failed=0
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
