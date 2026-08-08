# Phase 3 QC Step 7 — Final Coverage Projection to First-Party JaCoCo Summary (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P3-T7]
Command: `pwsh -NoProfile -File <SCRATCH>\ConvertCoberturaToJacoco.ps1 -Source coverage\remediation-final.cobertura.xml -Destination docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\evidence\qa-gates\coverage-remediation-final.jacoco.xml`
EXIT_CODE: 0

The script is identical to the one recorded verbatim in `evidence/remediation-baseline/coverage-projection.2026-08-08T14-52.md` (P0-T12), including its all-descendant `<line>` counting method, so the baseline and final figures are measured on the same denominator and are directly comparable.

## Output Summary

```text
PACKAGES=9
LINE_COVERED=95478
LINE_MISSED=15729
BRANCH_COVERED=22137
BRANCH_MISSED=5789
LINE_PCT=85.8561
BRANCH_PCT=79.2702
PKG QuickFiler LINE 14344/17435 BRANCH 3044/4038
PKG UtilitiesCS LINE 69217/76742 BRANCH 16111/19236
PKG TaskVisualization LINE 2736/3012 BRANCH 649/768
PKG SVGControl LINE 1696/3532 BRANCH 594/1248
PKG ToDoModel LINE 2032/3442 BRANCH 468/928
PKG Tags LINE 1374/1480 BRANCH 342/374
PKG TaskMaster LINE 3515/4979 BRANCH 749/1138
PKG TaskTree LINE 556/577 BRANCH 180/196
PKG VBFunctions LINE 8/8 BRANCH 0/0
```

### Aggregate first-party totals (remediation final)

| Counter | Covered | Missed | Total | Percentage |
|---|---|---|---|---|
| LINE | **95478** | **15729** | 111207 | **85.8561%** |
| BRANCH | **22137** | **5789** | 27926 | **79.2702%** |

The derived `LINE_PCT` of 85.8561 equals the root `<coverage line-rate="0.858561">` attribute of the source Cobertura report exactly, and `BRANCH_PCT` of 79.2702 equals `branch-rate="0.792702"`, confirming the projection is lossless against the report's own aggregate.

The `TaskMaster` package LINE counter is `missed=1464 covered=3515` — unchanged from both the P0-T12 remediation baseline and the implementation-cycle final summary, which is the expected result for a cycle that changes only a test file.

## Artifact checks

- `evidence/qa-gates/coverage-remediation-final.jacoco.xml` exists, is **39 lines** (under the 100-line cap), and contains exactly **9** `<package>` elements.
- A recursive search for `*.cobertura.xml` anywhere under `<FEATURE>\evidence\` returns **0** matches. The raw 10 MB / 187,000-line reports stay in the gitignored `coverage\` directory.

Binary outcome satisfied on all four conditions.
