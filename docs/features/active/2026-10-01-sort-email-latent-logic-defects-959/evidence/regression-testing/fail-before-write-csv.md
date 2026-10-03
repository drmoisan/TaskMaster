# Fail-Before: L4 WriteCSV_StartNewFileIfDoesNotExist and the Header (P2-T6)

Timestamp: 2026-10-03T08-47
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_UndoAndMoveLog_Tests" "/ResultsDirectory:coverage\test-results\959\p2-t6" "/Logger:trx;LogFileName=p2-t6.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST, TASKID p2-t6)
EXIT_CODE: 1 (the printed VSTEST_EXIT_CODE)
ExpectedExitCode: 1
Output Summary: expect-fail run after the seam step (the four-parameter overload as a pure forward of the previous body) and before the logic fix; both L4 tests failed: the existence check received the file-then-folder combination (L4a), and with the file present the inverted condition entered the branch and hit the null output array (L4b and L4c).

- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=2 executed=2 passed=0 failed=2
- RESULT_COUNT: 2
- RESULT WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader = Failed
- RESULT WriteCSV_WhenFileExists_DoesNotWrite = Failed
- MESSAGE WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader :: Expected queried to be equal to {"C:\Sortemail959Sandbox\logs\MovedMails.txt"}, but {"C:\Sortemail959Sandbox\logs"} differs at index 0.
- MESSAGE WriteCSV_WhenFileExists_DoesNotWrite :: Test method UtilitiesCS.Test.EmailIntelligence.SortEmail_UndoAndMoveLog_Tests.WriteCSV_WhenFileExists_DoesNotWrite threw exception: System.NullReferenceException: Object reference not set to an instance of an object.

Failing test methods: WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader and WriteCSV_WhenFileExists_DoesNotWrite.

Acceptance check (P2-T6): EXIT_CODE 1 is non-zero and equals ExpectedExitCode; COUNTERS total=2 executed=2 passed=0 failed=2; the two Failed rows are exactly NAMES-TUL; the first MESSAGE contains "differs at index 0" and the second contains "NullReferenceException"; every SANDBOX value False. All five hold.

## Pass-after (P2-T10)

Run: CMD-VSTEST (ASSEMBLY-UCT, FILTER-UNDO, TASKID p2-t10) after the logic fix (Listing L-U-FINAL, P2-T7) and the P2-T9 build; run at 2026-10-03T08-49.

- PASS-AFTER-VSTEST_EXIT_CODE: 0
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- COUNTERS total=2 executed=2 passed=2 failed=0
- RESULT WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader = Passed
- RESULT WriteCSV_WhenFileExists_DoesNotWrite = Passed

Acceptance check (P2-T10): PASS-AFTER-VSTEST_EXIT_CODE 0; COUNTERS total=2 executed=2 passed=2 failed=0 with the two RESULT rows exactly NAMES-TUL, each Passed; every SANDBOX value False. All three hold.
