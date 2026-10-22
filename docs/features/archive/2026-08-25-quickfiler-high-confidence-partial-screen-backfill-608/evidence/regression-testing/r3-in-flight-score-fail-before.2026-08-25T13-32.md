Timestamp: 2026-08-25T13-53
Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test/bin/Debug/QuickFiler.Test.dll" "/Settings:scripts/vscode/TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Tests:QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem" "/Logger:trx;LogFileName=docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r3-in-flight-score-fail-before.2026-08-25T13-32.trx"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: The single focused FQN failed as expected at QfcStreamingDequeueConfidenceGateTests.Part2.cs:184. The obsolete assertion expected a single item but the result contained the in-flight accepted item followed by the available qualifying item. The named TRX was copied from the VSTest TestResults staging location to the canonical evidence path.
Focused FQN: QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem
Failure Location: QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs:184
Initial Invocation Note: `vstest.console.exe` was not on PATH and launched no test; the recorded command above used the Visual Studio test-platform executable and produced the required expected test failure.
