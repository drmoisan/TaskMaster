# Phase 5 — Acceptance Criteria Check-off Mapping (Issue #202)

Timestamp: 2026-06-15T12-15

AC sources (Work Mode: full-feature): `spec.md` and `user-story.md` `## Acceptance Criteria`
sections (mirrored from `issue.md` `## Acceptance Criteria (early draft)`).

Each AC is marked PASS only when its implementation and verification tasks completed and the
corresponding evidence exists.

## AC-to-task mapping

| AC | Description | Impl tasks | Verify tasks | Status |
|---|---|---|---|---|
| AC1 | Flag enables/disables; no change when off | P1-T1, P1-T2, P3-T3, P3-T4, P3-T6 | P4-T2, P4-T5 | PASS |
| AC2 | Per-sub-component elapsed captured when on | P3-T4, P3-T5 | P4-T3 | PASS |
| AC3 | Formatted table with TOTAL emitted after startup | P2-T2, P3-T6 | P2-T7, P4-T4 | PASS |
| AC4 | Testable recorder, >= 90% new-code coverage | P2-T1, P2-T2, P2-T3 | P2-T4..P2-T8, P5-T4, P5-T5 | PASS |
| AC5 | Existing logging/deps only, no functional change | P2-T2, P3-T6 | P4-T5, P5-T2, P5-T3 | PASS |

## Evidence per AC

- AC1: `StartupTimingEnabled` setting added (`Settings.settings` / `Settings.Designer.cs`);
  `ApplicationGlobals.LoadAsync` reads the flag once and selects concrete vs no-op recorder
  (phase3-toolchain artifact). Verified by `LoadAsync_WhenTimingDisabled_RecordsNothingAndEmitsNoTable`
  and `LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff`
  (phase4-toolchain artifact). PASS.
- AC2: LoadBasic measured via Stopwatch; six sequential phases measured/recorded in order
  (phase3-toolchain). Verified by
  `LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrderWithLoadBasicFirst` asserting the
  recorded sequence `["LoadBasic","IntelConfig","OlObjects","ToDo","AutoFile","Engines","Events"]`
  (phase4-toolchain). PASS.
- AC3: `StartupTimingRecorder.FormatTable` builds the table via `PrettyPrinters.ToFormattedText`
  with a summed TOTAL row; `EmitTable` logs it with the `[Startup timing]` prefix at the end of
  `LoadAsync` (phase2/phase3 artifacts). Verified by
  `FormatTable_ContainsHeadersPhaseNamesAndTotalEqualToSumOfInjectedSpans` and
  `LoadAsync_WhenTimingEnabled_EmitsExactlyOneTableWithPhaseNamesAndTotal`
  (phase2/phase4 artifacts). PASS.
- AC4: Recorder is a pure unit with no Outlook/COM/filesystem/network dependency; 7 recorder
  tests; new-code coverage 100% (>= 90%) per final-test-coverage and coverage-delta artifacts.
  PASS.
- AC5: Uses existing `log4net` logger, `PrettyPrinters.ToFormattedText`, and
  `System.Diagnostics.Stopwatch` (no new dependencies); no `DateTime.Now`/`UtcNow`/`Thread.Sleep`/
  `Task.Delay`/`Random.Shared`. No functional startup change verified by the phase-ordering /
  yield-count regression guard `LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff`
  and the unchanged source-text COM-thread / yield regression tests, plus green analyzer
  (final-analyzer) and nullable (final-typecheck) gates. PASS.

All five acceptance criteria: PASS.
