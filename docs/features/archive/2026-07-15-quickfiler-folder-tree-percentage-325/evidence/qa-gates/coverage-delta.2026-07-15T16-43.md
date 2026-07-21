# Final QC — Coverage Delta and Threshold Verification (P6-T5)

Timestamp: 2026-07-16T11-40
Baseline source: evidence/baseline/baseline-tests.2026-07-15T16-43.md (P0-T5)
Post-change source: evidence/qa-gates/final-tests.2026-07-15T16-43.md (P6-T4)

## Repository-wide coverage delta (no-regression gate)

| Metric | Baseline (P0-T5) | Post-change (P6-T4) | Delta |
|---|---|---|---|
| Line coverage  | 64.19% (111296/173379) | 64.31% (111869/173955) | +0.12 pt |
| Branch coverage| 33.18% (13860/41767)   | 33.34% (13958/41867)   | +0.16 pt |

No-regression result: PASS. Both line and branch coverage increased; the change did not reduce
repository coverage. (The absolute whole-solution figures are dominated by large vendored/host-bound
modules — Swordfish, SVGControl, WinForms designer/viewer code — that sit far outside the #325 seam
denominator; the #325 change adds only well-covered host-neutral seams.)

## New-seam module coverage vs thresholds

Thresholds: new-module line >= 90%; repository/touched-code floor line >= 85%, branch >= 75%.

| Seam (production) | Line | Branch | line>=90% | line>=85% | branch>=75% | Result |
|---|---|---|---|---|---|---|
| UtilitiesCS.PercentageFormatter   | 100.00% | 100.00% | PASS | PASS | PASS | PASS |
| UtilitiesCS.FolderNodeViewModel    | 100.00% | 100.00% | PASS | PASS | PASS | PASS |
| UtilitiesCS.FolderHierarchyBuilder | 96.55%  | 94.44%  | PASS | PASS | PASS | PASS |
| UtilitiesCS.FolderTreeStateModel   | 100.00% | 91.18%  | PASS | PASS | PASS | PASS |

All four host-neutral seams meet the stricter new-module line target (>= 90%) and the branch floor
(>= 75%). The single seam below 100% line is FolderHierarchyBuilder at 96.55% (one uncovered line in
a defensive `rows == null` guard branch); its branch coverage is 94.44%. FolderTreeStateModel branch
coverage is 91.18% (a small number of guard sub-branches in the arrow no-op conditions).

Host-bound glue excluded from the seam denominator per the plan/spec (COM/WinForms
[ExcludeFromCodeCoverage]): `ItemViewer.FolderSearch.cs` owner-draw/hit-test/rebind glue,
`ItemViewer.Designer.cs`, `KeyboardHandler.cs` arrow routing. `IItemViewer.cs` and
`IFolderSearchHandler.cs` are interface-only (no executable lines). The touched
`QfcItemController.AssignFolderComboBox` injection path is exercised by the new controller-injection
tests (P5-T6).

## Overall outcome

PASS. No repository coverage regression; all four new-module seams exceed the line >= 90% target and
the branch >= 75% floor. No threshold is unmet; the outcome is PASS (not remediation-required).
