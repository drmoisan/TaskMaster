# P3-T13 Post-restore build and TrySave run (green)

Timestamp: 2026-10-01T21-15
Command: (1) msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p3-t13.msbuild.log); (2) vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_TrySaveAttachment_Tests" "/ResultsDirectory:coverage\test-results\956\p3-t13" "/Logger:trx;LogFileName=p3-t13.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere). EXIT_CODE is scoped to the vstest invocation (2).
EXIT_CODE: 0
Output Summary:
MSBUILD_EXIT_CODE: 0
CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
ERROR_LINES: 0
DLL_ADVANCED: True
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
VSTEST_EXIT_CODE: 0
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=11 executed=11 passed=11 failed=0
RESULT_COUNT: 11
RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
Name check: the eleven RESULT names are exactly the NAMES-T names (T1 to T11), each Passed.
Note: console streams of both tools were written to git-ignored logs and not echoed to the tool output (recorded display-only deviation, as in P3-T7 to P3-T11).
Acceptance: MSBUILD_EXIT_CODE 0 and DLL_ADVANCED True; EXIT_CODE 0; COUNTERS total=11 executed=11 passed=11 failed=0 with the eleven NAMES-T results Passed; every SANDBOX value False (all hold).
