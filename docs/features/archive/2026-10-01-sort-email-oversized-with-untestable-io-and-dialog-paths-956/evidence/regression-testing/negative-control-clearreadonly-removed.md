# P3-T11 Negative control: clearReadOnly call removed (expect-fail)

Timestamp: 2026-10-01T21-13
Command: (1) msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p3-t11.msbuild.log); (2) vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_TrySaveAttachment_Tests" "/ResultsDirectory:coverage\test-results\956\p3-t11" "/Logger:trx;LogFileName=p3-t11.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere). EXIT_CODE is scoped to the vstest invocation (2).
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
MSBUILD_EXIT_CODE: 0
ERROR_LINES: 0
(build: CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True)
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
VSTEST_EXIT_CODE: 1
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=11 executed=11 passed=5 failed=6
RESULT_COUNT: 11
RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Failed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Failed
RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Failed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Failed
RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Failed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Failed
MESSAGE TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer :: Expected seams.ClearedDirectories to be equal to {"C:\Sortemail956Sandbox\attachments"}, but found empty collection.
MESSAGE TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer :: Expected saved to be False, but found True.
MESSAGE TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer :: Expected saved to be False, but found True.
MESSAGE TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt :: Expected seams.ClearedDirectories to be equal to {"C:\Sortemail956Sandbox\attachments", "C:\Sortemail956Sandbox\attachments"}, but found empty collection.
MESSAGE TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse :: Expected seams.ClearedDirectories to be equal to {"C:\Sortemail956Sandbox\attachments"}, but found empty collection.
MESSAGE TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer :: Expected seams.ClearedDirectories to be equal to {"C:\Sortemail956Sandbox\attachments"}, but found empty collection.
Prediction check (Control Prediction table, by method name): Failed set observed = T2 (WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer), T3 (WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer), T4 (WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt), T8 (WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer), T9 (WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer), T11 (WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse); Passed set observed = T1, T5, T6, T7, T10. The failure messages also match the predicted mechanisms (T2, T3, T4, T11 on the ClearedDirectories equality; T8, T9 on BeFalse because the clear no longer throws).
CONTROL-OUTCOME: MATCHES PREDICTION
Note: the sandbox literal `C:\Sortemail956Sandbox` in the MESSAGE lines is an in-memory test literal, not a host path (plan Artifact hygiene convention); no directory was created (both SANDBOX AFTER values False). The vstest console stream was teed to the git-ignored log and not echoed to the tool output; the MSBuild console stream was piped to Out-Null; both are recorded deviations of output display only.
Acceptance: MSBUILD_EXIT_CODE 0 and ERROR_LINES 0; COUNTERS total=11 executed=11 passed=5 failed=6; Failed exactly T2, T3, T4, T8, T9, T11 and Passed exactly T1, T5, T6, T7, T10; EXIT_CODE non-zero (1) and equal to ExpectedExitCode; every SANDBOX value False (all hold).
