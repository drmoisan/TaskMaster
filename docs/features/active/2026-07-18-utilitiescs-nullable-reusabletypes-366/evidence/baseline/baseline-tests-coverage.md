# Phase 0 — Baseline Tests + Coverage (P0-T6)

Timestamp: 2026-07-19T09-06

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-coverage.cobertura.xml`

EXIT_CODE: 0

## Output Summary

- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 41.38 s
- Test assemblies exercised (8): QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskTree.Test,
  TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test.

## Coverage headline (Cobertura, whole run)

- Line coverage (line-rate): 0.837874 = 83.79% (lines-covered 86570 / lines-valid 103321)
- Branch coverage (branch-rate): 0.763563 = 76.36% (branches-covered 19535 / branches-valid 25584)

Cobertura XML written to
`docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-coverage.cobertura.xml`.
This is the authoritative baseline for the AC4 changed-line no-regression comparison (P9-T5, out of
this run's scope). Per-batch coverage artifacts are captured under `evidence/regression-testing/`.
