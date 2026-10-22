# P4-T8 — Coverage delta, baseline versus post-change (AC14)

Timestamp: 2026-09-01T20-18
Command: comparison of the counters derived in P0-T13 and P4-T6, plus a per-line differential between `evidence/baseline/baseline.cobertura.xml` and `evidence/qa-gates/postchange.cobertura.xml`
EXIT_CODE: 0

## The seven required numbers

    BASELINE_LINES_COVERED    = 54983
    BASELINE_LINES_VALID      = 64393
    BASELINE_LINE_PERCENT     = 85.3866

    POSTCHANGE_LINES_COVERED  = 54988
    POSTCHANGE_LINES_VALID    = 64406
    POSTCHANGE_LINE_PERCENT   = 85.3771

    NEWFILE_LINE_PERCENT      = 92.3077

## Post-processing state — the two sides are comparable

    BASELINE_POSTPROCESSED   = yes
    POSTCHANGE_POSTPROCESSED = yes

The two flags are **equal**, so no normalization step was required and none was performed. Both documents were produced by a runner exit of 0, which means both reached the Koverage post-processing step: both carry repository-relative filenames and both have had the third-party `<package>` nodes removed.

This check is not a formality. An un-post-processed document carries absolute filenames and retains third-party packages, so its denominator is not the other's denominator, and comparing across post-processing states would produce a meaningless delta. Both sides also report 9 `package` nodes, which corroborates the flags independently.

## Gate 1 — covered-line count did not regress

    POSTCHANGE_LINES_COVERED (54988) >= BASELINE_LINES_COVERED (54983)   →  PASS
    Covered-line delta = +5

**PASS.**

## Gate 2 — ratio within the 0.10-point band

    BASELINE_LINE_PERCENT - 0.10 = 85.2866
    POSTCHANGE_LINE_PERCENT      = 85.3771
    85.3771 >= 85.2866                                                   →  PASS
    Percentage-point delta = -0.0095

**PASS.** The ratio moved down by 0.0095 percentage points, well inside the 0.10-point band.

Neither gate asserts a repository-wide percentage as a pass/fail number, per the plan's section 4 and the note in `spec.md`. Two repository authorities state different repository-wide floors (80% in CLAUDE.md and `.claude/rules/csharp.md`; 85% line in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`), the divergence is unresolved, and #670 does not resolve it.

## Where the movement actually came from

The covered-line delta is +5, but the new file alone contributes 12 covered lines. That arithmetic does not close on its own, so the difference was measured per line rather than explained by assumption. A line-by-line differential between the two documents gives:

**Lines that gained coverage (17 total):**

| File | Lines gained |
| --- | --- |
| `QuickFiler\Controllers\QfcItemController.WebViewFaultBoundary.cs` | 12 |
| `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs` | 4 |
| `QuickFiler\Controllers\EfcHomeController.cs` | 1 |

**Lines that lost coverage (12 total):**

| File | Lines lost |
| --- | --- |
| `UtilitiesCS\HelperClasses\SegmentStopWatch.cs` | 6 |
| `UtilitiesCS\Interfaces\IWinForm\PropertyStore.cs` | 6 |

Net: +17 − 12 = **+5**, which reconciles exactly with the counter delta.

The denominator grew by exactly 13, which is the new file's instrumented line count and nothing else, so this change added no other instrumented line anywhere.

**None of the four files that moved incidentally is touched by this change.** The changed-file set is five paths under `QuickFiler/` and `QuickFiler.Test/` (P4-T11); `SegmentStopWatch.cs`, `PropertyStore.cs`, `OlTableExtensions.Etl.cs` and `EfcHomeController.cs` are not among them, and the first two are in a different assembly entirely. `SegmentStopWatch` is a timing helper whose covered branches depend on elapsed-time thresholds, which is the kind of code whose covered-line set varies between runs of a shared instrumented suite under differing machine load. This is precisely the run-to-run variation the 0.10-point band exists to absorb, and it is recorded here as an observation rather than dismissed.

## Changed-line coverage did not regress

The repository policy additionally requires that a change not reduce coverage for the lines it changed. The three substituted call sites were checked directly in both documents:

| Line in `QfcItemController.Initialization.cs` | Baseline hits | Post-change hits |
| --- | --- | --- |
| 192 | 1 | 1 |
| 288 | 1 | 1 |
| 324 | 1 | 1 |

All three changed lines are covered before and after. No changed line regressed.

The other changed production file, `QuickFiler/QuickFiler.csproj`, is a project file and carries no instrumented lines. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` has zero changed lines (P2-T4), so it has no changed-line obligation.

## AC14 verdict

AC14 requires that repository-wide line coverage not regress relative to the Phase 0 baseline captured before any edit. Both stated gates pass, the two documents are in the same post-processing state, the covered-line count rose, and no changed line lost coverage. **AC14 passes.**

AC14's criterion text names `evidence/coverage/` as the storage location. That is not a canonical evidence kind, so the instruction is rejected and the canonical kinds are substituted, per the non-overridable evidence-path clause of `atomic-plan-contract`. The criterion text itself is not edited. See `evidence/other/p4-t26-ac14-path-override.md`.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
