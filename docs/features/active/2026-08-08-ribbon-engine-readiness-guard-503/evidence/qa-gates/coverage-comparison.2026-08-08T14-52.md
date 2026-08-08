# Phase 3 QC Step 9 — Coverage Comparison for Remediation Cycle 1 (Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P3-T9]

All three points are measured on the **same first-party nine-package denominator** (`QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, `VBFunctions`) using the **same all-descendant `<line>` counting method**. The counting method was verified commensurable at P0-T12 by reproducing the implementation-cycle `TaskMaster` package counter (`missed=1464 covered=3515`) exactly; see `evidence/remediation-baseline/coverage-projection.2026-08-08T14-52.md` for the recorded correction.

## Three-point comparison

| Point | Source artifact | LINE covered | LINE missed | LINE % | BRANCH covered | BRANCH missed | BRANCH % |
|---|---|---|---|---|---|---|---|
| **A** Implementation-cycle post-change | `evidence/qa-gates/coverage-final.jacoco.xml` | 95473 | 15734 | 85.8516 | 22131 | 5795 | 79.2487 |
| **B** Remediation baseline (P0-T12) | `evidence/remediation-baseline/coverage-remediation-baseline.jacoco.xml` | 95467 | 15740 | **85.8462** | 22133 | 5793 | **79.2559** |
| **C** Remediation final (P3-T7) | `evidence/qa-gates/coverage-remediation-final.jacoco.xml` | 95478 | 15729 | **85.8561** | 22137 | 5789 | **79.2702** |

### The binding delta: C against B

| Metric | B (baseline) | C (final) | Delta | Verdict |
|---|---|---|---|---|
| LINE % | 85.8462 | **85.8561** | **+0.0099** | **C >= B** — no regression |
| BRANCH % | 79.2559 | **79.2702** | **+0.0143** | **C >= B** — no regression |
| LINE covered | 95467 | 95478 | +11 | up |
| LINE missed | 15740 | 15729 | -11 | down |
| BRANCH covered | 22133 | 22137 | +4 | up |
| BRANCH missed | 5793 | 5789 | -4 | down |
| LINE denominator | 111207 | 111207 | 0 | identical |
| BRANCH denominator | 27926 | 27926 | 0 | identical |

Both derived percentages at C are **greater than or equal to** the B values to two decimal places (85.85 >= 85.85; 79.27 >= 79.26). There is no shortfall to explain.

## `TaskMaster` package LINE counter at all three points

| Point | `TaskMaster` LINE |
|---|---|
| A — implementation-cycle post-change | `missed=1464 covered=3515` |
| B — remediation baseline | `missed=1464 covered=3515` |
| C — remediation final | `missed=1464 covered=3515` |

**Byte-identical at all three points.** This is the direct measurement that no production line changed coverage state in this cycle, and it is the same package-level identity the feature review used to corroborate AC23.

## Why production coverage is expected to be unchanged

This cycle changes **one test file** (`TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`, the F1 assertion body) and, after the P2-T1 revert recorded in `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md`, **no embedded XML resource** — `TaskMaster\Ribbon\RibbonExplorer.xml` takes a zero-line diff. Neither a test-file edit nor an XML resource edit adds or removes a production line, so production line coverage is expected to be unchanged, and it is: the `TaskMaster` package counter is identical at all three points.

The residual +11 line and +4 branch movement in the aggregate is run-to-run instrumentation variance, concentrated in `QuickFiler` (-1 covered) and `UtilitiesCS` (+12 covered). At 11 lines out of a 111,207-line denominator this is under 0.01 percent and is of the same order as the variance already recorded between points A and B, which bracket **no change at all** to the tree. It is not attributable to this cycle's edit.

## Denominator note (restated from `coverage-artifact-substitution.2026-08-08T17-40.md`)

These figures are measured over the nine first-party solution packages. Vendored third-party assemblies (`log4net`, `Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive`, `System.Linq.Async`) are excluded by `coverage.config`, and no `*.Test` assembly appears in the denominator.

An unfiltered run that includes the vendored packages reports a materially lower figure. **That unfiltered number is not the policy denominator**: `.claude/rules/general-unit-test.md` requires coverage tooling to be configured so metrics reflect application code, and CLAUDE.md § UT2 defines the floor against the testable first-party denominator. The filtered figure is the figure of record. This is recorded because the two runs differ by more than 15 percentage points and the discrepancy would otherwise look like a measurement error.

## Documented threshold conflict — recorded, not resolved

The repository states two different coverage floors in two different governing documents:

| Source | Line floor | Branch floor | New-code floor |
|---|---|---|---|
| `CLAUDE.md` § UT2 and `.claude/rules/csharp.md` | **80%** repo-wide | not stated | **90%** for new modules/classes/methods |
| `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` | **85%** | **75%** | not stated |

This conflict is **unresolved repository governance**, and it is recorded here rather than silently resolved by selecting whichever number is convenient. It is not load-bearing for this cycle's verdict: the measured figures (LINE 85.8561, BRANCH 79.2702) clear **every** floor in both sets simultaneously.

The 90-percent new-code floor has no subject in this cycle. No new module, class, or method is introduced; the only change is the body of an existing test method, and test code is excluded from the coverage denominator by policy.

## Verdict

**No coverage regression.** LINE and BRANCH both moved up against the remediation baseline on an identical denominator, the `TaskMaster` package counter is unchanged, and every stated floor is cleared.

Binary outcome satisfied: the derived first-party LINE and BRANCH percentages at P3-T7 are greater than or equal to the P0-T12 values to two decimal places. No shortfall occurred, so none is recorded.
