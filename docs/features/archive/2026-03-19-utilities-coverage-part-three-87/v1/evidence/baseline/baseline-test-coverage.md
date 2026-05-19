# Baseline Test + Coverage Evidence

Timestamp: 2026-03-19T22:19
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
EXIT_CODE: 0

## Output Summary

- **Total tests:** 1273
- **Passed:** 1271
- **Failed:** 0
- **Skipped:** 2
- **Test execution time:** 15.1632 seconds
- **Coverage file:** `coverage/coverage.cobertura.xml` (12,621,980 bytes)

### Coverage by Package

| Package | Line Rate |
|---|---|
| Overall (repo-wide) | 44.67% |
| **UtilitiesCS** | **34.27%** |
| UtilitiesCS.Test | 97.13% |
| QuickFiler | 19.84% |
| QuickFiler.Test | 84.38% |
| Swordfish.NET.General | 44.21% |
| SVGControl | 14.49% |
| TaskMaster | 8.43% |
| TaskMaster.Test | 88.07% |
| TaskVisualization.Test | 1.35% |
| ToDoModel | 8.57% |
| ToDoModel.Test | 53.79% |
| Tags | 0% |
| VBFunctions | 100% |
| VBFunctions.Test | 100% |

### Key Baseline Metrics

- **UtilitiesCS line coverage: 34.27%** (target: >=80% per file)
- All 1271 executed tests passed; 0 failures
- 2 tests skipped (pre-existing)
