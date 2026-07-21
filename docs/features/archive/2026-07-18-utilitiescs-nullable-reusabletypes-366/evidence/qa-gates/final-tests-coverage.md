# Final QC — Full Test Suite with Coverage (P9-T4)

Timestamp: 2026-07-19T22-03

## Command

`pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-reusabletypes-366/evidence/qa-gates/final-coverage.cobertura.xml`
(pwsh 7). A full `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug` was run immediately
before this to restore all test assemblies (the preceding P9-T3 `/t:Rebuild` warnings-as-errors
gate cleans-then-fails at vendored SVGControl, leaving a partial output tree; the restore build
succeeded EXIT 0).

EXIT_CODE: 0

## Output Summary

- Total tests: 5702
- Passed: 5702
- Failed: 0
- Skipped: 0
- Total time: ~34 s
- Test assemblies exercised (8): QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskTree.Test,
  TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test.

## Coverage headline (Cobertura, whole run)

- Line coverage (line-rate): 0.838827 = 83.88%
- Branch coverage (branch-rate): 0.763528 = 76.35%
- Cobertura XML: `evidence/qa-gates/final-coverage.cobertura.xml`

## Comparison to baseline (P0-T6)

- Baseline line-rate: 0.837874 (83.79%) -> Final: 0.838827 (83.88%). Delta +0.000953 (improved).
- Baseline branch-rate: 0.763563 (76.36%) -> Final: 0.763528 (76.35%). Delta -0.000035 (stable;
  within measurement nondeterminism).
- No test regression (AC3); pass count unchanged at 5702/5702.
