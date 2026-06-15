# Increment 2 — MSTest with Coverage (QuickFiler.Test)

Timestamp: 2026-06-14T08-22

Command: vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~KaCharTests|FullyQualifiedName~KaKeyTests|FullyQualifiedName~KaStringAsyncTests|FullyQualifiedName~KbdActionsRemainingBranchesTests|FullyQualifiedName~FilerQueueTests|FullyQualifiedName~QfcQueuePurePathsTests"
(vstest 18.7.0; raw .coverage merged to artifacts/csharp/inc2.cobertura.xml, gitignored.)

EXIT_CODE: 0

## Output Summary

Total tests: 46. Passed: 46. Failed: 0. Total time: ~2.47s. Deterministic; no Outlook, no WinForms
message loop, no timing dependency (async delegates complete synchronously).
Breakdown: KaCharTests 9, KaKeyTests 9, KaStringAsyncTests 8, KbdActionsRemainingBranchesTests 11,
FilerQueueTests 5, QfcQueuePurePathsTests 4.

One iteration failed initially: KeyEquals_ContainsMatchWhileActivated asserted Activated == false,
but the contains-match branch returns BEFORE the trailing Activated = false reset; the assertion
was corrected to expect Activated == true and the toolchain loop was restarted from csharpier.

Production-class line-rate after Increment 2 (from inc2.cobertura.xml):
- QuickFiler.Controllers.KaChar: 0.8235 (82.35%)
- QuickFiler.Controllers.KaCharAsync: 0.8125 (81.25%)
- QuickFiler.Controllers.KaKey: 0.8235 (82.35%)
- QuickFiler.Controllers.KaKeyAsync: 0.8125 (81.25%)
- QuickFiler.Controllers.KaStringAsync: 1.0 (100%)
- QuickFiler.Controllers.KbdActions<TKey,UClass,VDelegate>: 0.8795 (87.95%)
- QuickFiler.Controllers.FilerQueueItem: 1.0 (100%)
- QuickFiler.Controllers.FilerQueue: 0.30 (pure subset only; Enqueue/ConsumeAsync excluded)
- QuickFiler.Controllers.QfcQueue: 0.1453 (pure subset only; TLP/Outlook/dispatcher excluded)
