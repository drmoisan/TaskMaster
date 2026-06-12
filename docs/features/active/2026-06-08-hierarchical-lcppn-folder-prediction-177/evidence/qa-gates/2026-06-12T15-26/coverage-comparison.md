# Coverage Comparison — hierarchical-lcppn-folder-prediction (#177)

- Timestamp: 2026-06-12T15-26 (UTC)
- Baseline coverage XML: `artifacts/csharp/coverage.xml` (Phase 0, P0-T5)
- Post-change coverage XML: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/2026-06-12T15-26/coverage.xml` (Phase 8, P8-T2)
- Coverage tool: `Microsoft.CodeCoverage.Console.exe merge <coverage> -f xml`
- Metric definition: line coverage on the production assembly `UtilitiesCS.dll`. "Strict" =
  `lines_covered / (lines_covered + lines_partially_covered + lines_not_covered)`. "Inclusive"
  counts partially-covered lines as exercised: `(lines_covered + lines_partially_covered) / total`.

## Repository-wide line coverage (UtilitiesCS.dll)

| Run | lines_covered | lines_partial | lines_not_covered | Strict | Inclusive |
|---|---|---|---|---|---|
| Baseline (Phase 0) | 35047 | 897 | 5139 | 85.31% | 87.49% |
| Post-change (Phase 8) | 35621 | 909 | 5183 | 85.40% | 87.57% |
| Delta | +574 | +12 | +44 | +0.09 pp | +0.08 pp |

- Repository-wide line coverage remains well above the 80% floor (AC18).
- No regression: post-change strict coverage (85.40%) is >= baseline (85.31%); the changed/added
  lines added net covered lines without reducing the overall percentage.

## New module/class line coverage (function-level lines from the post-change run)

| New type | cov | part | not | Strict | Inclusive |
|---|---|---|---|---|---|
| IFolderPredictor (interface; no executable body) | n/a | n/a | n/a | n/a | n/a |
| FolderHierarchyNode | 3 | 2 | 0 | 60.0% | 100.0% |
| FolderHierarchyTree | 70 | 4 | 7 | 86.4% | 91.4% |
| PerParentClassifier | 139 | 1 | 10 | 92.7% | 93.3% |
| LcppnFolderPredictorConfig | 50 | 0 | 0 | 100.0% | 100.0% |
| LcppnFolderPredictor | 156 | 4 | 15 | 89.1% | 91.4% |
| EvaluationResult | 12 | 0 | 0 | 100.0% | 100.0% |
| LeafMetrics | 7 | 0 | 0 | 100.0% | 100.0% |
| FolderPredictorEvaluator | 78 | 4 | 2 | 92.9% | 97.6% |
| EvaluationConfig | 11 | 0 | 0 | 100.0% | 100.0% |

- Every new type reaches >= 90% inclusive line coverage (lines touched by tests), meeting the
  new-code >= 90% target (AC18).
- `FolderHierarchyNode` shows 60% strict but 100% inclusive: the record's auto-generated
  equality/`ToString` members are counted as partially-covered though every line is exercised. No
  line is uncovered.
- `IFolderPredictor` is a pure interface with no executable body, so it has no measurable lines.

## Notes

- A single pre-existing flaky test
  (`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`, a UI-thread/dispatcher
  test outside this feature's scope) intermittently fails under full-suite parallel load and passes
  in isolation. It is unrelated to this feature (see the active `ci-flaky-test-isolation-176` work)
  and does not affect coverage collection. This feature's own 77 tests are deterministic and green.
