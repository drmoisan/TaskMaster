Timestamp: 2026-08-25T14-02
Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test/bin/Debug/QuickFiler.Test.dll" "/Settings:scripts/vscode/TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Tests:QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem" "/Logger:trx;LogFileName=docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r3-in-flight-score-pass-after.2026-08-25T13-32.trx"
EXIT_CODE: 0
Output Summary: The P1-T2-rebuilt Debug DLL ran one focused test and it passed. The named TRX was copied from the VSTest TestResults staging location to the canonical evidence path.
Focused FQN: QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem
Fail-Before Evidence: evidence/regression-testing/r3-in-flight-score-fail-before.2026-08-25T13-32.md
