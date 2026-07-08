# Coverage Consistency Cross-Reference — Issue #171

- **Task:** [P2-T4]
- **Date:** 2026-06-02T10-36
- **Finding:** R2
- **Machine-readable source:** `artifacts/csharp/coverage.xml` (Cobertura)
- **Human-readable source:** `evidence/coverage/coverage-comparison-171.2026-06-02T10-26.md`

## Purpose

Confirm that the artifact-derived per-file and per-module figures are consistent with the
previously reported human-readable comparison.

## Basis reconciliation

The prior comparison uses a **range-line** basis (Microsoft.CodeCoverage `line_coverage`
ranges). The canonical Cobertura artifact uses a **distinct-instrumented-line** basis.
The two bases produce slightly different absolute percentages for the same file/module;
consistency is therefore evaluated on the conclusions (new-file gate, no changed-line
regression, module direction), which are basis-independent.

## Per-file cross-check

| File | Prior (range-line) | Artifact (instr-line) | Consistent conclusion |
|------|--------------------|-----------------------|------------------------|
| QfcHighConfidencePreFilter.cs | 100.00% (new) | 100.00% | YES — new file 100%, gate >= 90% MET in both |
| QfcHomeController.cs | 52.22% (rose from 50.51) | 56.41% | YES — both show rise vs baseline; high-confidence branch + seam covered |
| QfcFormController.cs | 39.73% (rose from 39.64) | 39.24% | YES — both show no changed-line regression; only the form load/show COM/WinForms block uncovered |
| QfcCollectionController.cs | 3.65% (note 1) | 3.58% | YES — both attribute the small aggregate dip to the added COM/WinForms carrier overload; changed `EncapsulateItemGroup` line was 0% at baseline, no regression |
| QfcItemController.cs | 7.29% (rose from 7.02) | 7.73% | YES — both show rise; `PopulateAndSelectFolder` seam covered, constructor/ComboBox UI uncovered |
| QfcItemGroup.cs | 84.62% (rose from 53.85) | 81.82% | YES — both show large rise; new `PredeterminedFolder` property covered |

## Per-module cross-check

| Module | Prior | Artifact | Consistent conclusion |
|--------|-------|----------|------------------------|
| UtilitiesCS | 87.58% | 87.45% | YES — unchanged, >= 80% floor met |
| QuickFiler | 24.32% (rose from 24.11) | 25.31% | YES — both show improvement vs the 24.11% baseline |

## Reconciliation notes

- Absolute percentages differ by small amounts (e.g., QuickFiler 24.32% vs 25.31%,
  QfcHomeController 52.22% vs 56.41%) solely because of the range-line vs distinct-line
  basis difference. The direction and the gate outcomes match in every case.
- No conclusion in the prior human-readable comparison is contradicted by the canonical
  artifact: new file 100% (>= 90%), no changed-line regression, no module regression,
  QuickFiler improved.

## Conclusion

The artifact-derived figures are **consistent** with the previously reported human-readable
comparison. The remaining numeric differences are explained by the documented basis
difference and do not change any gate result.
