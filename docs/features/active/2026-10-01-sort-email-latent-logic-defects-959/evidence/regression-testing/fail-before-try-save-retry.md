# Fail-Before: L2 Unbounded Retry Under a Held YesToAll (P3-T3)

Timestamp: 2026-10-03T08-53
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_TrySaveAttachment_Tests" "/ResultsDirectory:coverage\test-results\959\p3-t3" "/Logger:trx;LogFileName=p3-t3.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST, TASKID p3-t3)
EXIT_CODE: 1 (the printed VSTEST_EXIT_CODE)
ExpectedExitCode: 1
Output Summary: expect-fail run against the unfixed five-argument TrySaveAttachmentAsync; T12 failed because the recursive retry kept clearing and retrying until the CreateDirectoryLimit tripwire threw the sentinel InvalidOperationException on the third directory creation (the unbounded loop, ended deterministically); the eleven pre-existing tests passed.

- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=12 executed=12 passed=11 failed=1
- RESULT_COUNT: 12
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
- RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear = Failed
- RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
- RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
- RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
- RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
- MESSAGE TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear :: Expected a <System.UnauthorizedAccessException> to be thrown, but found <System.InvalidOperationException>: System.InvalidOperationException: retry bound exceeded at UtilitiesCS.Test.EmailIntelligence.SortEmail_TrySaveAttachment_Tests.Seams.CreateDirectory(String path) in <repo-root>\UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs:line 393 at UtilitiesCS.SortEmail.<TrySaveAttachmentAsync>d__31.MoveNext() in <repo-root>\UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:line 158 ... at UtilitiesCS.SortEmail.<TrySaveAttachmentAsync>d__31.MoveNext() in <repo-root>\UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:line 134 (recursion frame, three times) ... at FluentAssertions.Specialized.AsyncFunctionAssertions`2.<InvokeWithInterceptionAsync>d__15.MoveNext() in /_/Src/FluentAssertions/Specialized/AsyncFunctionAssertions.cs:line 378.

Failing test method: TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear (T12).

Acceptance check (P3-T3): EXIT_CODE 1 is non-zero and equals ExpectedExitCode; COUNTERS total=12 executed=12 passed=11 failed=1; the Failed row is exactly T12 and the eleven Passed rows are exactly NAMES-T; the MESSAGE contains "InvalidOperationException"; every SANDBOX value False. All five hold.

## Pass-after (P3-T6)

Runs: CMD-VSTEST (ASSEMBLY-UCT, FILTER-TRYSAVE, TASKID p3-t6) and CMD-VSTEST (ASSEMBLY-UCT, FILTER-TST1, TASKID p3-t6-tst1) after the bounded private core (Listing L-T-FINAL, P3-T4) and the P3-T5 build; run at 2026-10-03T08-55. No test was edited.

First run (FILTER-TRYSAVE):
- PASS-AFTER-VSTEST_EXIT_CODE: 0
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- COUNTERS total=12 executed=12 passed=12 failed=0
- RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear = Passed
- RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
- RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
- RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
- RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed

Second run (FILTER-TST1):
- TST1-VSTEST_EXIT_CODE: 0
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- COUNTERS total=14 executed=14 passed=14 failed=0
- RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
- RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
- RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
- RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments = Passed
- RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
- RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
- RESULT Cleanup_Files_DoesNotThrow = Passed
- RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
- RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
- RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments = Passed
- RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
- RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
- RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
- RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed

Acceptance check (P3-T6): PASS-AFTER-VSTEST_EXIT_CODE 0 with 12 of 12 and the rows exactly NAMES-T12, each Passed; TST1-VSTEST_EXIT_CODE 0 with 14 of 14 and the rows exactly NAMES-TST1-MID (including the two AC5 try-save pins), each Passed; every SANDBOX value False; no test was edited. All four hold.
