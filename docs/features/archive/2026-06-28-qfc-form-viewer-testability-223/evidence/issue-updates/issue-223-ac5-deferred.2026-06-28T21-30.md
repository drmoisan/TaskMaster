# P3-T3 — AC5 Deferral (FLOOR-BELOW) (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-50
PostedAs: not posted (local disposition only; no GitHub issue edit this step)

## Disposition
AC5 in `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/issue.md` remains UNCHECKED (`[ ]`) pending the orchestrator's authority-scoped exception decision.

AC5 text (verbatim, unchanged):
> AC5: New MSTest coverage verifies, via Moq event raising / `VerifySet` / `Verify`, that command events route to the correct controller methods, that the skip flow toggles `SkipButtonText`/`SkipButtonEnabled`, and that `CaptureItemSettings` handles both the populated and null `CaptureTlpCellStates()` results. New non-exempt code meets the >= 90% coverage floor; changed lines do not regress coverage; repo-wide coverage stays >= 80%.

## Why AC5 stays unchecked
- AC5's first three sub-claims are satisfied (new-code 100% >= 90%; changed lines +12.62 pp no-regression; new MSTest routing/skip/null-path tests present).
- The fourth sub-claim, "repo-wide coverage stays >= 80%", is now MEASURED at 73.35% (authoritative #197 method) / 74.11% (Cobertura root) — below the `>= 80%` floor (FLOOR-BELOW). It cannot be confirmed, so AC5 cannot be fully checked.

## Reference
- Escalation finding: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/other/repo-wide-floor-escalation-finding.2026-06-28T21-30.md`
- Floor decision: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/qa-gates/repo-wide-floor-decision.2026-06-28T21-30.md`
- Measurement: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/qa-gates/repo-wide-coverage-measurement.2026-06-28T21-30.md`

## Skipped FLOOR-PASS-only tasks
- P3-T1 (AC5 `[ ]` -> `[x]` re-check): SKIPPED — FLOOR-PASS-only; AC5 stays `[ ]`.
- P3-T2 (AC5 issue-update mirror): SKIPPED — FLOOR-PASS-only.

Output Summary:
FLOOR-BELOW: AC5 remains unchecked. The repo-wide `>= 80%` sub-claim is measured at 73.35%/74.11%, below floor. The accept-as-pre-existing-debt vs. require-uplift decision is routed to the orchestrator (authority-scoped). No issue checkbox was changed; the gate is not weakened.
