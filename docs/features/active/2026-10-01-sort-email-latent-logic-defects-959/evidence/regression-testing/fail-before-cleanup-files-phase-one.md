# Fail-Before: L3 Phase One, Cleanup_Files Misses _attachmentsAltName (P1-T7)

Timestamp: 2026-10-03T08-40
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests" "/ResultsDirectory:coverage\test-results\959\p1-t7" "/Logger:trx;LogFileName=p1-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST, TASKID p1-t7)
EXIT_CODE: 1 (the printed VSTEST_EXIT_CODE)
ExpectedExitCode: 1
Output Summary: expect-fail run against the unfixed Cleanup_Files; the _attachmentsAltName row failed (YesToAll read back after cleanup) and the other three rows passed.

- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=4 executed=4 passed=3 failed=1
- RESULT_COUNT: 4
- RESULT Cleanup_Files_ResetsEveryPromptAnswerField [_responseSaveFile] = Passed
- RESULT Cleanup_Files_ResetsEveryPromptAnswerField [_picturesOverwrite] = Passed
- RESULT Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName] = Failed
- RESULT Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsOverwrite] = Passed
- MESSAGE Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName] :: Expected field.GetValue(null) to be YesNoToAllResponse.Empty {value: 0}, but found YesNoToAllResponse.YesToAll {value: 4}.

Failing test method: Cleanup_Files_ResetsEveryPromptAnswerField (row _attachmentsAltName).

Acceptance check (P1-T7): EXIT_CODE 1 is non-zero and equals ExpectedExitCode; COUNTERS total=4 executed=4 passed=3 failed=1; the Failed row is exactly the _attachmentsAltName row and the three Passed rows are the other NAMES-TAS-P1 rows; the MESSAGE contains "but found" and "YesToAll"; every SANDBOX value False. All five hold.

## Pass-after (P1-T12)

Run: CMD-VSTEST (ASSEMBLY-UCT, FILTER-ATTSAVE, TASKID p1-t12) after the one-line L3 fix (Edit E-A-L3, P1-T9) and the P1-T10 build; run at 2026-10-03T08-42.

- PASS-AFTER-VSTEST_EXIT_CODE: 0
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- COUNTERS total=4 executed=4 passed=4 failed=0
- RESULT Cleanup_Files_ResetsEveryPromptAnswerField [_picturesOverwrite] = Passed
- RESULT Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName] = Passed
- RESULT Cleanup_Files_ResetsEveryPromptAnswerField [_responseSaveFile] = Passed
- RESULT Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsOverwrite] = Passed

Acceptance check (P1-T12): PASS-AFTER-VSTEST_EXIT_CODE 0; COUNTERS total=4 executed=4 passed=4 failed=0 with the four RESULT rows exactly NAMES-TAS-P1, each Passed; every SANDBOX value False. All three hold.
