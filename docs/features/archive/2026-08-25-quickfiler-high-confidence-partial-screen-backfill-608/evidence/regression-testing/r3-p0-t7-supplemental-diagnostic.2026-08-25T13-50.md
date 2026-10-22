# Issue #608 P0-T7 supplemental full-suite diagnostic

Timestamp: 2026-08-25T13-50
Purpose: classify the unexpected second failure from the immutable P0-T7 coverage receipt before allowing one controlled retry of the same planned command.

Command boundary: source-read-only nine-assembly TRX diagnostic using the same rebuilt Debug assemblies and repository test settings as the P0-T7 coverage run.

Result: `EXIT_CODE: 1`; 6,476 total tests; 6,475 passed; 1 failed.

Sole failure: `QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem` at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs:184`. This is the known obsolete one-item assertion captured by P0-T6.

TRX: `evidence/regression-testing/r3-p0-t7-supplemental-diagnostic/full-suite.trx`.

Decision: the extra failure in the original P0-T7 coverage wrapper result is non-reproducible. Preserve the original failed receipt unchanged. Authorize exactly one retry of the same P0-T7 coverage command, without advancing task order or editing source. If the retry reports any failure beyond the expected Part2 FQN, stop at the three-cycle remediation cap.

