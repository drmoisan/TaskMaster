# Fail-Before: L1 SaveCase Labels (P1-T6)

Timestamp: 2026-10-03T08-40
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_SaveCase_Tests" "/ResultsDirectory:coverage\test-results\959\p1-t6" "/Logger:trx;LogFileName=p1-t6.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST, TASKID p1-t6)
EXIT_CODE: 1 (the printed VSTEST_EXIT_CODE)
ExpectedExitCode: 1
Output Summary: expect-fail run against the unfixed SaveCase; the four positive rows failed because the mocked SaveAsFile was never called (both combined case labels are dead), and the Empty control passed.

- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=5 executed=5 passed=1 failed=4
- RESULT_COUNT: 5
- RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll] = Failed
- RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes] = Failed
- RESULT SaveCase_WhenAnswerIsEmpty_DoesNotSave = Passed
- RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No] = Failed
- RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll] = Failed
- MESSAGE SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll] :: Test method UtilitiesCS.Test.EmailIntelligence.SortEmail_SaveCase_Tests.SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath threw exception: Moq.MockException: Expected invocation on the mock once, but was 0 times: x => x.SaveAsFile("C:\Sortemail959Sandbox\attachments\report.pdf") Performed invocations: Mock<Attachment:4> (x): No invocations performed.
- MESSAGE SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes] :: Test method UtilitiesCS.Test.EmailIntelligence.SortEmail_SaveCase_Tests.SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath threw exception: Moq.MockException: Expected invocation on the mock once, but was 0 times: x => x.SaveAsFile("C:\Sortemail959Sandbox\attachments\report.pdf") Performed invocations: Mock<Attachment:3> (x): No invocations performed.
- MESSAGE SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No] :: Test method UtilitiesCS.Test.EmailIntelligence.SortEmail_SaveCase_Tests.SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath threw exception: Moq.MockException: Expected invocation on the mock once, but was 0 times: x => x.SaveAsFile("C:\Sortemail959Sandbox\attachments\report_alt.pdf") Performed invocations: Mock<Attachment:1> (x): No invocations performed.
- MESSAGE SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll] :: Test method UtilitiesCS.Test.EmailIntelligence.SortEmail_SaveCase_Tests.SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath threw exception: Moq.MockException: Expected invocation on the mock once, but was 0 times: x => x.SaveAsFile("C:\Sortemail959Sandbox\attachments\report_alt.pdf") Performed invocations: Mock<Attachment:2> (x): No invocations performed.

Failing test methods: SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath (rows No and NoToAll) and SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath (rows Yes and YesToAll).

Acceptance check (P1-T6): EXIT_CODE 1 is non-zero and equals ExpectedExitCode; COUNTERS total=5 executed=5 passed=1 failed=4; the Failed rows are exactly the four rows of the two positive tests and the Passed row is SaveCase_WhenAnswerIsEmpty_DoesNotSave; every MESSAGE contains "but was 0 times"; every SANDBOX value False. All five hold.

## Pass-after (P1-T11)

Run: CMD-VSTEST (ASSEMBLY-UCT, FILTER-SAVECASE, TASKID p1-t11) after the L1 fix (Edit E-A-L1, P1-T8) and the P1-T10 build; run at 2026-10-03T08-42.

- PASS-AFTER-VSTEST_EXIT_CODE: 0
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- COUNTERS total=5 executed=5 passed=5 failed=0
- RESULT SaveCase_WhenAnswerIsEmpty_DoesNotSave = Passed
- RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll] = Passed
- RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll] = Passed
- RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes] = Passed
- RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No] = Passed

Acceptance check (P1-T11): PASS-AFTER-VSTEST_EXIT_CODE 0; COUNTERS total=5 executed=5 passed=5 failed=0 with the five RESULT rows exactly NAMES-TSC-P1, each Passed; every SANDBOX value False. All three hold.
