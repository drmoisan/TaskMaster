# Source-Active Streaming Regression Verification

- Timestamp: 2026-07-03T18:58:19-04:00
- Issue: 233
- Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /Tests:DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives,DequeueAsync_SourceExhaustion_ReturnsEmptyAndPartialResults,DequeueAsync_PropagatesCancellationBeforeTakingSourceItem,DequeueAsync_ScansManyToYieldFew_BackfillsUntilQuantityMet,DequeueNextItemGroupAsync_HighConfidenceMode_UsesStreamingGate,TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring`
- Exit code: 0

## Result

PASS. VSTest reported `Test Run Successful` with 6 total tests and 6 passed.

## Acceptance Coverage

- Source-active streaming: `DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives`
- Source exhaustion: `DequeueAsync_SourceExhaustion_ReturnsEmptyAndPartialResults`
- Cancellation: `DequeueAsync_PropagatesCancellationBeforeTakingSourceItem`
- Scan-many-to-yield-few: `DequeueAsync_ScansManyToYieldFew_BackfillsUntilQuantityMet`
- Datamodel streaming-gate routing: `DequeueNextItemGroupAsync_HighConfidenceMode_UsesStreamingGate`
- Disabled-mode parity: `TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring`
