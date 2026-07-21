# Coverage Comparison (P7-T2)

- Timestamp: 2026-07-10T00-01
- Command: `vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /InIsolation /Settings:TaskVisualization.Test\coverage.runsettings`
- EXIT_CODE: 0 (104 passed / 0 failed)
- Raw Cobertura output copied to `artifacts/csharp/coverage.xml`.
- Aggregation method: unique measured line hit/total summed per source file across all Cobertura `<class>` entries (main + compiler-generated) for that file; exempt lines are already excluded by the collector via the `<Attributes>` excludes in `coverage.runsettings`.

## Baseline (P0-T10)

- TaskVisualization production line coverage: **0.00%** — `TaskController` carried a class-level `[ExcludeFromCodeCoverage]`, so all 1861 controller lines were outside the denominator, and the disabled placeholder test exercised no production code.

## Post-change per-file (measured lines only)

| File | Covered / Total | Rate |
|---|---|---|
| `TaskController.cs` | 145 / 152 | 95.39% |
| `TaskController.Actions.cs` (incl. `AutoAssignAllAsync`) | 261 / 293 | 89.08% |
| `TaskController.Flags.cs` | 30 / 33 | 90.91% |
| `TaskController.ControlMaps.cs` (STA-measured) | 146 / 152 | 96.05% |
| `TaskController.ControlRelationships.cs` (STA-measured) | 138 / 139 | 99.28% |
| `TaskController.Accelerator.cs` (measured portion, STA) | 222 / 290 | 76.55% |
| `TaskDurationParser.cs` (new helper) | 11 / 11 | 100.00% |
| `TaskPriorityMapper.cs` (new helper) | 15 / 15 | 100.00% |
| `ITagPromptService.cs` (request/result) | 25 / 25 | 100.00% |

## Aggregates vs. thresholds

| Metric | Value | Threshold | Result |
|---|---|---|---|
| Refactored core (all six `TaskController` partials) | **942 / 1059 = 88.95%** | >= 80% | PASS |
| Core + helpers + `ITagPromptService` | 993 / 1110 = 89.46% | >= 80% | PASS |
| New helper classes (`TaskDurationParser`, `TaskPriorityMapper`) | **26 / 26 = 100.00%** | >= 90% | PASS |
| STA-measured control-identity (`ControlMaps.cs` + `ControlRelationships.cs`) | 284 / 291 = 97.59% | (informational) | measured, was file-level-exempt at baseline |

## Notes

- `FlagChangeGroup.cs`, `FlagChangeItem.cs`, `FlagChangeTrainingQueue.cs` are pre-existing (not refactored core) and carry their own `[ExcludeFromCodeCoverage]` on the COM-bound members; they are excluded from the refactored-core denominator per the P7-T2 file set.
- The measured portion of `TaskController.Accelerator.cs` (76.55%) is below 80% on its own but the aggregate refactored-core set is 88.95%; the AC evaluates the core file SET, not each file individually. The residual uncovered accelerator lines are the exempt focus/handle/pump residue (excluded) plus a few not-exempt-but-uncovered dispatch/guard lines documented in the exemption inventory.
