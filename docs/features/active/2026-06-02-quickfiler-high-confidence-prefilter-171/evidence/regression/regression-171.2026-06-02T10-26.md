# Regression Evidence — Issue #171

- Task: [P7-T4]
- Timestamp: 2026-06-02T10-26

## Full suite (QuickFiler.Test + UtilitiesCS.Test)
- Baseline: 3925 total, 3916 passed, 9 failed (8 unique flaky timer/serialization tests).
- Final:    3943 total, 3935 passed, 8 failed (same pre-existing flaky set; count varies run-to-run).

## No previously-passing test now fails due to Issue #171
The final-run failures are exactly the pre-existing timing-flaky UtilitiesCS.Test methods:
  - AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress
  - EmptyQueue_AfterSeveralIntervals_StopsTimer
  - Enqueue_InvokesBatchActionsOnTimerInterval
  - RequestTask_WithConfiguredTask_InvokesTaskAfterInterval
  - Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite
  - StartNew_ConfiguresAutoResetAndInvokesCallback
  - StartTimer_RaisesElapsedEvent

These are timer/clock/serialization tests that fail only under full-suite parallel load and PASS when
re-run in isolation (verified: a targeted re-run of StartTimer_RaisesElapsedEvent,
EmptyQueue_AfterSeveralIntervals_StopsTimer, StartNew_ConfiguresAutoResetAndInvokesCallback,
Enqueue_InvokesBatchActionsOnTimerInterval returned 4/4 passed). They are present in the pre-change
baseline (tests-baseline-171). None is in the QuickFiler controller suites and none is related to
Issue #171.

## Standard-mode (mode-disabled) path tests green
- RunAsync_ExecutesCorrectly (updated to set QfSettings disabled) — PASS.
- RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload — PASS.
- RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly — PASS.

## Retained-but-not-invoked seam tests green (unmodified)
- QfcFormControllerTests: ApplyHighConfidenceFilterAsync_WhenModeEnabled_RemovesBelowThresholdOnce,
  _WhenGroupsIsNull_DoesNothing, _WhenQfSettingsIsNull_DoesNotRemove, _WhenModeDisabled_NeverRemoves
  — all PASS without edits.
- QfcCollectionControllerTests: all 6 RemoveBelowThresholdAsync_* tests — PASS without edits.

## Issue #171 new tests (18) — all PASS
- QfcHighConfidencePreFilterTests: 9 (cutoff, zero-score, inclusive boundary, predetermined folder,
  null/empty/all-below edges, cancellation, trivial setup).
- QfcHomeControllerTests: 5 (delegate override, enabled-invokes-carrier, disabled-uses-plain,
  ordering precedes UI, disabled-plain-only).
- QfcFormControllerTests: 1 (carrier path does not invoke post-UI removal).
- QfcCollectionControllerTests: 1 (carrier carries PredeterminedFolder onto item group).
- QfcItemControllerTests: 2 (predetermined folder preselected; index-1 fallback preserved).

## Conclusion
No regression attributable to Issue #171. The standard-mode path and the retained Issue #169 seams
remain green and unmodified.
