# Coverage Reconciliation — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-13-45
- Task: [P5-T9]
- Finding: R2
- EXIT_CODE: 0

This artifact is a **consumer** of the coverage documents. It read none itself and therefore
carries no gate rule 12 standing-in statement; the five artifacts that did read one carry it.

## C#

| Measurement | Baseline, [P0-T12] | Post-change, [P5-T7] | Delta |
|---|---|---|---|
| Line covered / instrumented | 56,482 / 65,737 | **56,486 / 65,737** | +4 lines |
| **Line rate** | **0.8592** | **0.8593** | **+0.0001** |
| Branch covered / instrumented | 13,657 / 17,052 | **13,658 / 17,052** | +1 branch |
| **Branch rate** | **0.8009** | **0.8010** | **+0.0001** |
| Tests | 7,343 passed, 0 failed | 7,343 passed, 0 failed | 0 |

| Clause | Required | Measured | Result |
|---|---|---|---|
| Post-change line | at least 0.80 | **0.8593** | PASS |
| Post-change branch | at least 0.75 | **0.8010** | PASS |
| Line delta | at least -0.005 | **+0.0001** | PASS |
| Branch delta | at least -0.005 | **+0.0001** | PASS |

Both deltas are **positive**, so neither reaches the measurement-noise band between -0.005 and
0, let alone a regression. The denominators are identical at 65,737 lines and 17,052 branches,
which is the expected result: this cycle changed no `.cs`, `.csproj`, `packages.config`,
`app.config` or `.csharpierignore` file. On an unchanged denominator a movement of 4 lines and 1
branch is run-to-run variation in which tests happen to touch which lines.

### The Baseline Is [P0-T12] and Not the Delivered `p9-t7`

Recorded explicitly, per decision **D6** and **gate rule 14**.

Every C# figure the review recorded was measured at `794d34f02`, **before** this branch took its
clean merge of `origin/main`. The merge brought C# changes this branch had never built, so those
figures describe a tree that no longer exists here. Comparing Phase 5 against the delivered
`evidence/qa-gates/p9-t7-coverage-projection.2026-09-19T09-44.jacoco.xml` would be exactly the
defect gate rule 14 names: a baseline an intervening transformation invalidated.

[P0-T12] re-took all four C# gates after the merge and is the authoritative baseline for this
cycle. It records the supersession in its own text.

For context only, and not used in any comparison above: the review re-derived 85.91 percent line
and 80.07 percent branch from the committed `p9-t7` projection. The post-merge baseline measured
85.92 and 80.09 on identical denominators, so the merge moved the C# figures by 0.01 and 0.02
points.

## PowerShell

| Measurement | Baseline, [P0-T8] | Post-change, [P5-T3] | Delta |
|---|---|---|---|
| Aggregate covered / instrumented | 1,598 / 1,702 | **1,611 / 1,706** | +13 lines |
| **Aggregate line percent** | **93.89** | **94.43** | **+0.54** |

| Clause | Required | Measured | Result |
|---|---|---|---|
| Aggregate at least 80 | >= 80 | **94.43** | PASS |
| Aggregate at least the [P0-T8] value | >= 93.89 | **94.43** | PASS |

The instrumented total rose by 4 because this cycle added executable production lines: one
resolver call in `ConsistencyVerifier.psm1` and the verbose record in the composition root, net
of the 35-line function that moved between two measured files.

### Per-Module Figures Under `scripts/dependencies/`

| Module | [P0-T8] | [P5-T3] | At least 90 |
|---|---|---|---|
| `AnalyzerItemRepair.psm1` | 100.00 | **100.00** | PASS |
| `ConsistencyVerifier.psm1` | 98.74 | **98.75** | PASS |
| `PackageCompatibility.psm1` | 100.00 | **100.00** | PASS |
| `PackageGraph.psm1` | 100.00 | **100.00** | PASS |
| `ProjectConsistency.psm1` | 100.00 | **100.00** | PASS |
| `Repair-PackageManifestConsistency.ps1` | 94.12 | **93.81** | PASS |

Every module under `scripts/dependencies/` is at or above 90. The lowest, 93.81, fell 0.31
points because [P2-T1] moved a fully covered 35-line function out of it, leaving its 14
uncovered lines as a slightly larger share of a smaller denominator. `ProjectConsistency.psm1`
received that function and holds at 100.00 on a denominator 15 lines larger.

### `scripts/vscode/Sync-PackageReferences.ps1`, Before and After

| Measurement | Before | After |
|---|---|---|
| Covered / instrumented | **95 / 127** | **104 / 127** |
| **Percent** | **74.80** | **81.89** |
| Delta | | **+9 lines, +7.09 points** |

81.89 clears the authoritative floor of 80 and does not clear the superseded 85. The arithmetic
and the two prohibited routes to 85 are recorded in full at [P1-T11]; the conflict between the
two floor readings is **open issue #668** and is not resolved by this cycle.

All nine target logic lines are covered at [P5-T3], confirmed line by line after every later
phase's edits.

## No Branch Figure Exists for PowerShell

There is no PowerShell branch-coverage figure in this reconciliation, and the reason is a
**tooling capability limit** rather than an omission.

Pester measures **command coverage and line coverage only**. The JaCoCo document it emits
contains no `BRANCH` counter in any output format, so there is no branch figure to read and no
branch threshold that could be evaluated. `.claude/rules/quality-tiers.md` states the same and
exempts PowerShell from the branch threshold on exactly that ground.

The exemption is a threshold exemption only. PowerShell production files remain in the coverage
denominator under the Coverage Exclusion Policy, and no file was excluded from measurement by
this cycle.

## Output Summary

Every figure is a number. C# post-change line 0.8593 and branch 0.8010, both above their floors,
both moving **upward** by 0.0001 against the post-merge [P0-T12] baseline on identical
denominators. PowerShell aggregate 94.43 percent, above 80 and above the 93.89 baseline; every
`scripts/dependencies/` module at or above 90; `Sync-PackageReferences.ps1` from 74.80 to 81.89.
The C# baseline is [P0-T12], not the delivered `p9-t7`, per decision D6 and gate rule 14. No
PowerShell branch figure exists because Pester emits no BRANCH counter.
