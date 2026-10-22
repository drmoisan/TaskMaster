# MSTest Coverage Baseline, Post-Merge — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-37-30
- Task: [P0-T12]
- Command: CMD-MSTEST-COVERAGE
- EXIT_CODE: 0

## Command

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot .'
```

`-SearchRoot .` is mandatory. Without it the script's single-search-root defect discovers test
assemblies from a sibling worktree and the figures describe a repository this cycle is not
changing.

## Numeric Coverage, Verbatim One-Line First-Party Report

```
First-party coverage: lines 56482/65737 (85.92%), branches 13657/17052 (80.09%)
```

| Measurement | Value | Runner floor | Result |
|---|---|---|---|
| Line coverage | **0.8592** (56,482 of 65,737) | 0.80 | PASS |
| Branch coverage | **0.8009** (13,657 of 17,052) | 0.75 | PASS |

Both recorded as numbers, not placeholders. The script enforces its own floors of 0.80 line and
0.75 branch and exited 0, which is an independent confirmation of the same two comparisons.

## Test Counts

```
Test Run Successful.
Total tests: 7343
     Passed: 7343
 Total time: 39.5118 Seconds
```

| Measurement | Value |
|---|---|
| Passed | **7343** |
| Failed | **0** |
| Skipped | **0** |

The test-result summary records the same figures and states how `Skipped` was derived:

```
Test run outcome: Completed
Total 7343, executed 7343, passed 7343, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

## Copied Evidence Forms

The run printed both optional destination lines.

| Line printed by the run | Copied to | Mandatory |
|---|---|---|
| `Coverage projection: <execution-worktree-root>\coverage\coverage.cobertura.jacoco.xml` | `evidence/remediation-baseline/p0-t12-coverage-projection.2026-09-20T01-37.jacoco.xml` | yes |
| `Test-result summary: <execution-worktree-root>\coverage\test-results\mstest-coverage-run.summary.txt` | `evidence/remediation-baseline/p0-t12-test-results.2026-09-20T01-37.summary.txt` | conditional, and the line appeared |

Both copies exist, at 1,467 and 298 bytes. Neither carries a host-path occurrence: both were
checked with the run-time-derived pattern of **gate rule 17** and returned 0 matches, so neither
needs sanitisation and neither will appear in the [P4-T3] residual.

`TEST-RESULT-SUMMARY: produced.` The not-produced branch was not taken.

## Package Breakdown of the Projection

Read from the copied projection. The nine first-party packages sum to the figures above.

| Package | LINE covered | LINE missed | BRANCH covered | BRANCH missed |
|---|---|---|---|---|
| `QuickFiler` | 10,461 | 2,293 | 2,518 | 699 |
| `UtilitiesCS` | 39,213 | 4,211 | 9,473 | 1,796 |
| `TaskVisualization` | 1,426 | 143 | 333 | 67 |
| `SVGControl` | 877 | 977 | 300 | 338 |
| `ToDoModel` | 1,061 | 762 | 248 | 260 |
| `Tags` | 702 | 56 | 174 | 16 |
| `TaskMaster` | 2,443 | 802 | 517 | 211 |
| `TaskTree` | 295 | 11 | 94 | 8 |
| `VBFunctions` | 4 | 0 | 0 | 0 |
| **Total** | **56,482** | **9,255** | **13,657** | **3,395** |

## This Supersedes the Delivered `p9-t7` Figures

**This is the post-merge C# baseline for this cycle and it supersedes the delivered `p9-t7`
figures.** Those were measured at `794d34f02`, before the branch took its clean merge of
`origin/main`. The merge brought C# changes this branch had never built, so the earlier figures
describe a tree that no longer exists here.

This is decision **D6** and it is the defect **gate rule 14** names: a baseline is assertable later
only if it is invariant under the transformations in between, and the merge is exactly such a
transformation.

Recorded for comparison rather than for assertion: the review re-derived 85.91 percent line and
80.07 percent branch from the committed `p9-t7` projection. This post-merge run measures 85.92 and
80.09 on the same 65,737-line and 17,052-branch denominators. The denominators are identical and
the rates move by 0.01 and 0.02 points, which is inside run-to-run variation. [P5-T9] compares
Phase 5 against **this** artifact, not against `p9-t7`.

This cycle changes no `.cs`, `.csproj`, `packages.config`, `app.config` or `.csharpierignore` file,
so the Phase 5 re-run should agree with these figures within the same noise band; a disagreement
would be a finding about the merge, not about this remediation.

## Output Summary

Exit 0. 7,343 tests passed, 0 failed, 0 skipped. Line coverage 0.8592 and branch coverage 0.8009,
both above the runner floors of 0.80 and 0.75. Coverage projection and test-result summary both
copied into the evidence tree, both free of host-path occurrences. This is the post-merge C#
baseline and supersedes `p9-t7` per decision D6.
