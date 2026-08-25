Timestamp: 2026-08-25T12-55
Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-25T11-36\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "/Settings:C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-25T11-36\scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Tests:QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem" "/Logger:trx;LogFileName=docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r2-repeatability/focused-608-failure-2.trx"`
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Focused repetition 2 ran the same parsed failing gate test. Total tests `1`; Failed `1`; the same assertion at `QfcStreamingDequeueConfidenceGateTests.Part2.cs:line 184` received two items instead of one.

Artifacts: `focused-608-failure-2.trx`; `focused-608-failure-2-output.txt`.
