# Feature Audit — quickfiler-high-confidence-filter (Issue #169)

- Generated: 2026-06-01T17-23 (UTC)
- Work mode: full-feature (issue.md absent → fail-closed to full-feature)
- AC sources: `user-story.md` (AC1–AC7) and `spec.md` Definition of Done

## Scope and Baseline

- Base branch (resolved): `development`
- Merge-base SHA: `3322bbee6a941eaa05e8388dd78ec3998e542d75`
- Head SHA: `32de29d7748492eb0ec62219f2fe20b3d279142e`
- Diff range: `3322bbee6a941eaa05e8388dd78ec3998e542d75..32de29d7748492eb0ec62219f2fe20b3d279142e`
- Audited surface: full branch diff (19 C# files + 1 ribbon XML; docs/evidence excluded from
  behavioral evaluation). The PR-context summary's "0 core logic files" classification is inaccurate;
  the audit used the actual diff.

## Acceptance Criteria Inventory

From `user-story.md`:
1. AC1 — A new ribbon entry point launches QuickFiler in high-confidence mode.
2. AC2 — When the mode is enabled, emails whose top suggested folder probability is below the
   configured threshold are not shown.
3. AC3 — Emails with no folder suggestion at or above the threshold (including none at all) are
   excluded.
4. AC4 — The default threshold is 90% (0.90) and is persisted as a user setting.
5. AC5 — The threshold percentage is changeable at runtime via a ribbon input control, with
   validation; the value persists across sessions.
6. AC6 — With high-confidence mode disabled, QuickFiler behaves exactly as today (no filtering).
7. AC7 — New and changed logic is covered by MSTest + Moq + FluentAssertions tests; the full C#
   toolchain passes with zero regressions.

`spec.md` Definition of Done items map onto AC1–AC7 and are evaluated under the corresponding AC.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence and reasoning |
|---|---|---|
| AC1 | PARTIAL | The ribbon button (`RibbonExplorer.xml` `QuickFilerHighConfidence`), the viewer callback (`RibbonViewer.QuickFilerHighConfidence_Click`), and `RibbonController.LoadQuickFilerHighConfidenceAsync` exist and wire the launch path; `LaunchAsync` mirrors the standard flow. The entry point launches QuickFiler. However, the mechanism it uses to "launch in high-confidence mode" is a persisted setting it never resets, which is the root of the AC6 failure. The launch happens; the mode mechanism is defective. The entry-point method itself has 0% test coverage. |
| AC2 | PASS | `RemoveBelowThresholdAsync` computes `cutoff = (long)Math.Round(threshold*1000,0)` and removes groups with `TopFolderScore < cutoff`. Tests: all-above removes none, all-below removes all, mixed removes only below-threshold (`QfcCollectionControllerTests`). `TopFolderScore` reads `_folderHandler?.Suggestions?.TopScore()`; `TopScore()` returns the max score. |
| AC3 | PASS | `TopScore()` returns 0 for an empty scorer (tested: `TopScore_WhenScorerIsEmpty_ReturnsZero`). A zero-score group is removed when cutoff > 0 (tested: `RemoveBelowThresholdAsync_WhenScoreIsZeroAndThresholdPositive_RemovesGroup`). Boundary inclusivity tested (`...WhenScoreEqualsCutoff_RetainsGroup`). |
| AC4 | PASS | `Settings.settings` and `Settings.Designer.cs` declare `HighConfidenceThreshold` (System.Double, User scope, default `0.9`) and `HighConfidenceModeEnabled` (System.Boolean, User scope, default `False`). `AppQuickFilerSettings` exposes them. Tests verify defaults (`HighConfidenceThreshold_Default_IsZeroPointNine`, `HighConfidenceModeEnabled_Default_IsFalse`) and round-trip persistence. |
| AC5 | PASS | `RibbonExplorer.xml` adds an `editBox` (`HighConfidenceThreshold`); `RibbonViewer` wires `_GetText`/`_OnChange`; `RibbonController.SetHighConfidenceThresholdText` validates [0,100] and stores probability /100, rejecting non-numeric and out-of-range input (tested: valid persists, non-numeric unchanged, out-of-range unchanged). Persistence via user-scoped settings. Minor lossy round-trip for fractional percentages noted in code review (non-blocking). |
| AC6 | FAIL | The criterion requires that with the mode disabled, QuickFiler behaves exactly as today. `HighConfidenceModeEnabled` is persisted and set to `true` by `LoadQuickFilerHighConfidenceAsync` (`RibbonController.cs:132`, setter saves to `Settings.Default`). No code path resets it to `false`; `LoadQuickFilerAsync` (the standard entry point) does not clear it. After any high-confidence launch, the persisted flag remains `true`, so the standard entry point's `LoadItemsAsync` → `ApplyHighConfidenceFilterAsync` reads `HighConfidenceModeEnabled == true` (`QfcFormController.cs:958`) and applies the filter. This contradicts AC6 and the spec alternate flow. The unit test `ApplyHighConfidenceFilterAsync_WhenModeDisabled_NeverRemoves` only proves the conditional respects a *mocked* disabled setting; it does not exercise the entry-point/persistence interaction, so the defect is not caught. |
| AC7 | PARTIAL | New/changed pure logic is well covered by MSTest + Moq + FluentAssertions (24 issue-169 tests, all passing; new logic members 90–100%). Independent reviewer runs: CSharpier check PASS (0 reformatting), analyzer build PASS (0/0), nullable build PASS (0/0). However: (a) the canonical `artifacts/csharp/coverage.xml` is absent, so coverage is not independently verifiable by the required mechanism; (b) two production assemblies (QuickFiler.dll 23.40%, TaskMaster.dll 25.16%) are below the 80% repo-wide floor; (c) the entry-point method `LoadQuickFilerHighConfidenceAsync` is at 0% coverage; (d) the full instrumented suite is not green in a single pass (11 flaky failures, asserted pre-existing). "Zero regressions" is plausible but the full-suite green-in-one-pass claim is not satisfied under instrumentation. |

## Summary

- PASS: AC2, AC3, AC4, AC5 (4 criteria)
- PARTIAL: AC1, AC7 (2 criteria)
- FAIL: AC6 (1 criterion)

The core filtering logic (scoring accessor, removal pass, settings, ribbon validation) is correct,
well-isolated, and tested. The feature is not PR-ready because of a behavioral regression in the
disabled-mode path (AC6) and coverage-verification gaps (AC7), both of which are remediation
triggers. AC1 is downgraded to PARTIAL because the entry-point mechanism is the source of the AC6
defect and is untested.

Go/no-go: **No-go** pending remediation of the AC6 persisted-mode defect and the coverage gaps.

## Acceptance Criteria Check-off

Per `acceptance-criteria-tracking`, only criteria evaluated PASS may be checked off in the
authoritative source. AC1, AC6, AC7 must NOT be marked complete. The current `user-story.md` marks
all of AC1–AC7 as `[x]`; AC1, AC6, and AC7 are not substantiated by this audit and should be reverted
to `[ ]` by the remediation owner (the reviewer does not silently edit, but records the required
correction here).

| AC | Authoritative source status (current) | Audit verdict | Required source state |
|---|---|---|---|
| AC1 | `[x]` | PARTIAL | `[ ]` (not complete) |
| AC2 | `[x]` | PASS | `[x]` (correct) |
| AC3 | `[x]` | PASS | `[x]` (correct) |
| AC4 | `[x]` | PASS | `[x]` (correct) |
| AC5 | `[x]` | PASS | `[x]` (correct) |
| AC6 | `[x]` | FAIL | `[ ]` (not complete) |
| AC7 | `[x]` | PARTIAL | `[ ]` (not complete) |

This audit does not modify `user-story.md`. Per workflow constraints (no silent fixes), the corrected
states above are recorded for the remediation handoff; the source-doc edits are part of remediation,
not review.
