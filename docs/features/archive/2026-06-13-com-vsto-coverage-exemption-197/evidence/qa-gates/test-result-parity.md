# Test Result Parity — Baseline (P0-T6) vs Final (P7-T4)

Timestamp: 2026-06-13T14-27

## Baseline (Phase 0, pre-change)
- Total tests: 4068
- Passed: 4066
- Failed: 2
  - AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException
  - RequestTask_WithProvidedTask_InvokesTaskAfterInterval

## Final (Phase 7, post-change)
- Total tests: 4068
- Passed: 4066
- Failed: 2
  - AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException
  - RequestTask_WithProvidedTask_InvokesTaskAfterInterval

## Comparison
- Total test count: identical (4068).
- Failing set: IDENTICAL to baseline — the same 2 pre-existing flaky timing/threading tests (roadmap §0.1).
- No new persistent failure introduced by the [ExcludeFromCodeCoverage] annotation/config/doc changes.
- Per-phase intermediate runs showed the flaky timing tests plus a flaky AppQuickFilerSettings shared-static (Settings.Default.HighConfidenceMode) parallel-race test failing intermittently in varying combinations. These are pre-existing test-isolation/timing weaknesses (UT4 shared-global-state and timing), independent of this feature's non-behavioral attribute changes. The final run reproduced exactly the baseline failing set, confirming behavior parity.

## Verdict
PASS — behavior parity preserved. Post-change pass/fail set equals the Phase 0 baseline (allowing the 2 documented pre-existing failures). No production behavior change (AC7 / spec §Invariants "No production behavior change").
