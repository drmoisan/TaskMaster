Timestamp: 2026-08-25T14-02
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/baseline/r3-csharp-coverage.2026-08-25T13-32.cobertura.xml"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Controlled retry after the source-read-only supplemental diagnostic. The run executed 6476 tests: 6475 passed and 1 failed. The sole failure was QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem at QfcStreamingDequeueConfidenceGateTests.Part2.cs:184, the expected obsolete one-item assertion. No additional failure recurred.
Repository Line Coverage: 70.16059825916391%
QfcStreamingDequeueConfidenceGate Coverage: 97.87234042553191%
Coverage Report: docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/baseline/r3-csharp-coverage.2026-08-25T13-32.cobertura.xml
Decision: PASS_EXPECTED_FAILURE_ONLY
