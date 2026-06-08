# Remediation Inputs Read — Issue #171

- **Task:** [P0-T2]
- **Date:** 2026-06-02T10-36
- **Findings covered:** R1, R2

## Sources read

1. `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/remediation-inputs.2026-06-02T10-36.md` (authoritative remediation spec).
2. `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/coverage-baseline-171.2026-06-02T14-05.txt` (coverage baseline).

## Three findings

- **R1 (BLOCKING):** The canonical machine-readable C# coverage artifact `artifacts/csharp/coverage.xml` is absent. Must be produced (Cobertura XML with per-line counters) from vstest `/EnableCodeCoverage` over `QuickFiler.Test.dll` and `UtilitiesCS.Test.dll`, so the coverage gate can confirm `QfcHighConfidencePreFilter.cs` >= 90% and no changed-file regression vs baseline.
- **R2 (SUPPORTING):** From the artifact, verify changed lines for the six touched files are covered or are legitimate COM/WinForms boundaries, and document the repo-wide / per-module figure with a pre-existing-condition justification.
- **R3 (LOW):** Restore `TaskMaster/TaskMaster.csproj` to its base-branch (`development`) form (multi-line attribute formatting and trailing newline); keep only any change Issue #171 genuinely requires (none expected).

## Baseline per-file figures (range-line basis) for the six touched files

| File | Covered | NotCovered | Total | Pct |
|------|---------|-----------|-------|-----|
| QfcHomeController.cs | 249 | 244 | 493 | 50.51% |
| QfcFormController.cs | 306 | 466 | 772 | 39.64% |
| QfcCollectionController.cs | 66 | 1666 | 1732 | 3.81% |
| QfcItemController.cs | 131 | 1735 | 1866 | 7.02% |
| QfcItemGroup.cs | 7 | 6 | 13 | 53.85% |
| QfcHighConfidencePreFilter.cs | (new file — not present at baseline) | | | |

(`FolderScorer.cs` baseline 93.29% is unchanged by Issue #171 and reported for completeness only.)

## Per-module baseline (line_coverage attribute)

| Module | Covered | NotCovered | Total | Pct |
|--------|---------|-----------|-------|-----|
| QuickFiler.dll | 3699 | 11645 | 15344 | 24.11% |
| UtilitiesCS.dll | 35826 | 5080 | 40906 | 87.58% |

Other production modules (TaskMaster.dll 6.66%, ToDoModel.dll 0.00%, Tags.dll 0.00%) are pre-existing low-coverage modules not exercised by the two in-scope test assemblies and not introduced by Issue #171.

## Coverage gate interpretation

- New file `QfcHighConfidencePreFilter.cs` must reach >= 90% line coverage.
- Changed lines in the touched files must show no coverage regression vs the per-file numbers above.
- The whole-repo sub-80% figure is a documented pre-existing condition not introduced by Issue #171.
