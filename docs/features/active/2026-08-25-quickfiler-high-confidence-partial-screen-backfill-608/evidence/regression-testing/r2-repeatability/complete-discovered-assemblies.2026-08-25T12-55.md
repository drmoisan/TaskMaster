Timestamp: 2026-08-25T12-55
Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" <the nine absolute Debug assembly paths recorded in r2-vstest-discovery.2026-08-25T12-55.md> "/Settings:C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-25T11-36\scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r2-repeatability/complete-discovered-assemblies.trx"`
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Complete discovered-assembly repetition ran 6,476 tests: 6,475 passed and 1 failed. The failed gate test remains `QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem`.

Artifacts: `complete-discovered-assemblies.trx`; `complete-discovered-assemblies-output.txt`.
