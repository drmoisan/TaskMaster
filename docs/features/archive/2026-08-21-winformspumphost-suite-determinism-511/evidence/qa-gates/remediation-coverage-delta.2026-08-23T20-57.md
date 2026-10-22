# Remediation QA Gate — Coverage Delta Against the Baseline

Timestamp: 2026-08-23T19-27

Baseline source:
`docs/features/active/winformspumphost-suite-determinism-511/evidence/baseline/coverage.2026-08-21T18-10.md`
Post-change source:
`docs/features/active/winformspumphost-suite-determinism-511/evidence/qa-gates/remediation-coverage.2026-08-23T20-57.md`

Both measurements were produced by the same script
(`scripts\vscode\Invoke-MSTestWithCoverage.ps1`) against a post-processed Cobertura XML, and the
changed-module figure was computed with the identical per-class-deduplicated `<line>` counting method
the baseline artifact mandates.

## The four measured figures

| Figure | Baseline | Post-change | Signed delta (percentage points) |
| --- | --- | --- | --- |
| Repository headline line rate (root `line-rate`) | 85.55% (`0.855531`) | 85.59% (`0.855916`) | **+0.04** |
| Repository headline branch rate (root `branch-rate`) | 79.03% (`0.790312`) | 79.06% (`0.790598`) | **+0.03** |
| `QuickFiler` package line rate | 80.93% (`0.8092566619915849`) | 81.08% (`0.81084081028582`) | **+0.15** |
| Changed-module aggregate (`QuickFiler\Controllers\QfcItemController*`, 10 classes) | 86.34% (1410 / 1633) | 86.34% (1410 / 1633) | **+0.00** |

The `QuickFiler` package `line-rate` delta is **+0.15 percentage points**, which is greater than or
equal to 0. The strict, no-tolerance condition on that figure is satisfied.

## Per-class deltas for the changed module

| Filename | Baseline line-rate | Post-change line-rate | Signed delta (pp) |
| --- | --- | --- | --- |
| `QuickFiler\Controllers\QfcItemController.cs` | 1 (100.00%) | 1 (100.00%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.Initialization.cs` | 0.949612 (94.96%) | 0.949612 (94.96%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` | 0.850829 (85.08%) | 0.850829 (85.08%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.Conversation.cs` | 0.882353 (88.24%) | 0.882353 (88.24%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.FolderHandling.cs` | 0.952381 (95.24%) | 0.952381 (95.24%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.EventWiring.cs` | 0.815182 (81.52%) | 0.815182 (81.52%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.EventHandlers.cs` | 0.7865168539325843 (78.65%) | 0.7865168539325843 (78.65%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.Navigation.cs` | 0.90678 (90.68%) | 0.90678 (90.68%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs` | 0.793249 (79.32%) | 0.793249 (79.32%) | +0.00 |
| `QuickFiler\Controllers\QfcItemController.MailActions.cs` | 0.768 (76.80%) | 0.768 (76.80%) | +0.00 |

Every per-class `line-rate` delta is exactly +0.00 percentage points, comfortably above the
-0.50 percentage-point floor. The floor exists because `dotnet-coverage` denominators are not
bit-stable between runs; in this instance no per-class rate moved at all, so no measurement-noise
allowance had to be drawn on. The matched-class count is 10 in both measurements.

## Changed-line coverage

| Row | Value |
| --- | --- |
| Executable lines changed by this cycle's diff against the evidence-producing source (`02983a70`) | **0** |
| Comment lines changed | 13 added, 7 removed, across two files |
| Files with any change | `QfcItemController.InitializationTests.Part2.cs`, `QfcItemController.ViewerSetupTests.cs` |
| Changed-line coverage | vacuously non-regressed |

This cycle's diff against the source that produced the Phase 4 determinism evidence consists of
comment lines only. `git diff --numstat 02983a70` on the three touched files reports `7 5` and `6 2`
with `QfcItemController.InitializationTests.Part3.cs` absent entirely, and filtering that diff to
changed lines that are not `//` comment lines returns the empty set — the verification is recorded in
`docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2-narrowing-rationale.2026-08-23T20-57.md`.
Zero executable lines changed, so there is no changed-line coverage denominator to regress and the
no-regression-on-changed-lines requirement is satisfied vacuously rather than by measurement.

## Cell-value audit

Every cell in every table above holds a numeric value or an explicit textual value derived from a
measurement. No cell holds a placeholder, and the token `UNVERIFIED` appears nowhere in this
artifact.

## Acceptance conditions

1. `QuickFiler` package `line-rate` delta greater than or equal to 0 (strict, no tolerance) — met at
   **+0.15 pp**.
2. Every `QfcItemController` class `line-rate` delta greater than or equal to -0.50 pp — met; all ten
   are exactly +0.00 pp.
3. Every cell holds a numeric value rather than a placeholder — met.
