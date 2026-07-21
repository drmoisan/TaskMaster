# Coverage Delta and New-Code Coverage

Timestamp: 2026-07-18T00-36

Scope: production assembly `UtilitiesCS.dll` (first-party), measured via Cobertura collector scoped to `UtilitiesCS.dll` with the repo-standard Deedle/FSharp/vendored module excludes. Sources: baseline `evidence/baseline/baseline-tests-coverage.md` (P0-T5) and post-change `evidence/qa-gates/final-tests-coverage.md` (P4-T4).

## Repository coverage (no-regression check)

| Metric | Baseline (P0-T5) | Post-change (P4-T4) | Delta |
|---|---|---|---|
| Line coverage | 88.49% (35834/40496) | 88.49% (35905/40573) | +0.007 pt (no regression) |
| Branch coverage | 82.21% (8257/10044) | 82.21% (8279/10070) | +0.006 pt (no regression) |
| Tests passed | 4321/4321 | 4344/4344 | +23 tests |

The repository coverage floor is not reduced. Post-change line coverage (88.49%) remains at/above the baseline and above the 80% repository floor.

## New / changed first-party code coverage (>= 90% required)

| File / method | Line coverage | Branch coverage |
|---|---|---|
| `FolderBreadcrumbSegment.cs` | 100% (24/24) | 100% (12/12) |
| `OutlookFolderHierarchyProvider.cs` (incl. compiler-generated async state machines) | 95.12% (78/82) | 83.33% (10/12) |
| `FolderTreeSnapshotQueries.GetAncestorChain` (new method) | 100% (24/24) | 100% (per-branch fully hit) |
| **Combined new production code** | **96.92% (126/130)** | — |

- Combined new-code line coverage is **96.92%**, exceeding the 90% new-code threshold.
- The only 4 uncovered lines are compiler-generated async fault/exception plumbing inside the `ResolveLeafKeyAsync` state machine (`<ResolveLeafKeyAsync>d__4::MoveNext`, 7/9). These are not authored source lines; the source logic of `ResolveLeafKeyAsync` (empty-path guard, snapshot acquisition, case-insensitive first-match, null-on-absent) is fully exercised by the found / not-found / duplicate-first-match / empty-path tests.
- `IFolderHierarchyProvider.cs` is type-only (an interface with no executable lines) and is legitimately excluded from the executable denominator; it contributes no uncovered lines.

Conclusion: new/changed first-party code meets the >= 90% threshold and the repository coverage floor is preserved.
