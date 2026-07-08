# Coverage Comparison — Remediation Cycle 1 (#177)

- Timestamp: 2026-06-12T17-18 (UTC)
- Task: [P3-T2]
- Baseline coverage XML: `evidence/baseline/coverage-p0/baseline.xml` (P0-T6)
- Post-change coverage XML: `evidence/qa-gates/2026-06-12T15-54/p2-coverage.xml`
- Canonical post-change coverage: `artifacts/csharp/coverage.xml` (updated)

## Strict per-type line coverage (F2 targets)

Strict = partially-covered lines counted as NOT covered.

| Type | Baseline strict | Post-change strict | New/changed-code strict | Gate (>= 90%) |
|---|---|---|---|---|
| `FolderHierarchyTree` | 86.42% | **100.00%** | 100.00% (all previously uncovered members/branches now covered) | PASS |
| `LcppnFolderPredictor` | 89.14% | **97.71%** | 97.71% (Build null-config, DescendBeam terminal/empty-score branches, beam-trim, UnTrain missing-parent now covered; remaining 4 lines are partial, 0 not-covered) | PASS |

Per-type line tallies (post-change):
- `FolderHierarchyTree`: covered=81, partial=0, not-covered=0, total=81.
- `LcppnFolderPredictor`: covered=171, partial=4, not-covered=0, total=175.

## Repo-wide (production assembly) strict total

| Scope | Baseline strict | Post-change strict | No regression |
|---|---|---|---|
| `UtilitiesCS.dll` (production assembly under UtilitiesCS.Test) | 85.31% / 85.40% (reviewer) | **85.45%** | PASS (>= 80% floor; no regression vs baseline) |

## Result

Both F2 target types are at >= 90% strict line coverage (100.00% and 97.71%). The production assembly
strict total did not regress (85.45% post-change vs 85.31%-85.40% baseline) and remains above the 80%
repository floor. The canonical `artifacts/csharp/coverage.xml` is updated with the post-change data.

Note: F2 changes are test-only additions (no production line changes in the two target files), so the
coverage increase comes entirely from newly exercising existing production branches; no production code
in `FolderHierarchyTree.cs` or `LcppnFolderPredictor.cs` was modified.
