# Focused Issue #218 Pass-After (Post-Split) — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcDatamodelTests|FullyQualifiedName~RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch|FullyQualifiedName~RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter"`

EXIT_CODE: 0

Results: Total tests 7, Passed 7, Failed 0.
- TryQueueRemainingMailItemAsync_HighConfidenceEnabled_ScoresBeforeQueueAdmission — PASS
- TryQueueRemainingMailItemAsync_ScoreEqualsThreshold_AddsAndHooksMailItem — PASS
- TryQueueRemainingMailItemAsync_ScoreBelowThreshold_DoesNotAddOrHookMailItem — PASS
- TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring — PASS
- TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook — PASS (new in P4-T2)
- RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch — PASS
- RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter — PASS

Output Summary: All 7 focused issue #218 tests pass after completing the test split and adding the null-mailItem admission test. The high-confidence queue-admission behavior (enabled scoring-before-admission, inclusive-threshold admit, below-threshold reject, disabled add/hook, null-item reject) and the initial-load ownership (RunAsync no longer pre-filters only the first GUI batch) are preserved. The 5 issue #218 acceptance criteria remain satisfied.
