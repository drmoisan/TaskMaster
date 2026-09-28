# Final QA Step 7 — MSTest With Coverage, Iteration 1

- Timestamp: 2026-09-20T09-13-17
- Task: [P5-T7]
- Command: CMD-MSTEST-COVERAGE
- EXIT_CODE: 0

## Command

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot .'
```

`-SearchRoot .` is mandatory; without it the script's single-search-root defect discovers test
assemblies from a sibling worktree.

## Numeric Coverage, Verbatim One-Line First-Party Report

```
First-party coverage: lines 56486/65737 (85.93%), branches 13658/17052 (80.10%)
```

| Measurement | Value | Floor | Result |
|---|---|---|---|
| Line coverage | **0.8593** (56,486 of 65,737) | 0.80 | PASS |
| Branch coverage | **0.8010** (13,658 of 17,052) | 0.75 | PASS |

Both recorded as numbers, not placeholders. The runner enforces its own floors of 0.80 line and
0.75 branch and exited 0, an independent confirmation of the same two comparisons.

## Test Counts

```
Test Run Successful.
Total tests: 7343
     Passed: 7343
 Total time: 47.9638 Seconds
```

| Measurement | Value |
|---|---|
| Passed | **7343** |
| Failed | **0** |
| Skipped | **0** |

The copied test-result summary records the same figures and how `Skipped` was derived:

```
Test run outcome: Completed
Total 7343, executed 7343, passed 7343, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

## Copied Evidence Forms

The run printed both optional destination lines.

| Line printed by the run | Copied to | Mandatory | Bytes |
|---|---|---|---|
| `Coverage projection: ...\coverage\coverage.cobertura.jacoco.xml` | `evidence/qa-gates/p5-t7-coverage-projection.2026-09-20T01-37.jacoco.xml` | yes | 1,467 |
| `Test-result summary: ...\coverage\test-results\mstest-coverage-run.summary.txt` | `evidence/qa-gates/p5-t7-test-results.2026-09-20T01-37.summary.txt` | conditional, and the line appeared | 298 |

`TEST-RESULT-SUMMARY: produced.` The not-produced branch was not taken and no
`TEST-RESULT-SUMMARY: not produced` line is recorded.

Both copies were checked with the run-time-derived pattern of **gate rule 17** and carry
**0** host-path occurrences, so neither disturbs the [P4-T3] residual.

The projection copy is the **permitted evidence form gate rule 12 requires be committed** in
place of the prohibited collector document. [P5-T14] asserts it appears in the committed set.

## Comparison Against the [P0-T12] Post-Merge Baseline

| Measurement | [P0-T12] baseline | [P5-T7] post-change | Delta |
|---|---|---|---|
| Line covered / instrumented | 56,482 / 65,737 | **56,486 / 65,737** | **+4 lines** |
| Line rate | 0.8592 | **0.8593** | **+0.0001** |
| Branch covered / instrumented | 13,657 / 17,052 | **13,658 / 17,052** | **+1 branch** |
| Branch rate | 0.8009 | **0.8010** | **+0.0001** |
| Tests | 7,343 passed | **7,343 passed** | 0 |

The denominators are identical, which is the expected result: this cycle changed no `.cs`,
`.csproj`, `packages.config`, `app.config` or `.csharpierignore` file. Both deltas are
**positive** and are inside run-to-run variation on an unchanged denominator; [P5-T9] records
them against the tolerance.

## Output Summary

Exit 0. 7,343 tests passed, 0 failed, 0 skipped. Line coverage 0.8593 and branch coverage
0.8010, both above the runner floors. Projection and test-result summary both copied into the
evidence tree and both free of host-path occurrences. Step 7 of the final loop passes.
