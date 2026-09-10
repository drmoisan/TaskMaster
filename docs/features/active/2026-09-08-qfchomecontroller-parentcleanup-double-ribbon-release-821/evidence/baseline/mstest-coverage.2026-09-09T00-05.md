# Phase 0 — Baseline tests with coverage

Timestamp: 2026-09-09T12-44
Task: [P0-T10]

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/coverage.cobertura.xml`
EXIT_CODE: 0

The run was executed detached and its console output sampled, per the plan's hang-risk rule. It
completed on its own without stalling; the twenty-minute-no-output fallback was **not** triggered and
the per-assembly fallback runs were **not** used. Captured output is 7225 lines; the material tail is
reproduced verbatim below.

```text
Test Run Successful.
Total tests: 7196
     Passed: 7196
 Total time: 28.1501 Seconds
Code coverage results: <repo-root>\coverage\coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
First-party coverage: lines 55968/65376 (85.61%), branches 13475/16886 (79.80%)
Done. Coverage artifact: <repo-root>\coverage\coverage.cobertura.xml
```

## The three required summary items

**(a) Is the literal `Post-processing coverage XML for Koverage compatibility...` present?**
**Yes**, at captured line 7222. This literal prints only after the inner vstest run exited 0, so it
is the success-only observable for a zero-failure run. Its presence establishes the baseline as
green, which in turn makes `[P6-T5]`'s first branch the governing one: the post-change run must
record it as present too and must record a failed count of `0`.

**(b) Is the literal `Done. Coverage artifact:` present?**
**Yes**, at captured line 7224. This literal prints only after `Assert-CoberturaLineCoverageThreshold`
passes, so the repository-wide 80% line threshold was met at baseline and the assert did not throw.

**(c) vstest counts.**

| Count | Value | Source |
|---|---|---|
| Total | **7196** | `Total tests: 7196` |
| Passed | **7196** | `Passed: 7196` |
| Failed | **0** | see note |
| Skipped | **0** | see note |

Note on the failed and skipped counts: a fully passing vstest run prints `Total tests:` and
`Passed:` and prints **no `Failed:` line and no `Skipped:` line at all**. A `Select-String
-SimpleMatch` search of the captured output for those two literals returned zero matches. Both
counts are therefore recorded as `0` because **the lines were absent**, not because the values were
unreadable.

The run also reported `Test Run Successful.` at captured line 7217, and no occurrence of
`Test Run Failed.`.

## Baseline failing-test set

**Empty.** No test in any assembly failed. This is recorded explicitly because `[P6-T5]`'s second
branch — the subset-of-baseline-failures branch — applies only when the baseline is red. It is not
red, so that branch does not apply and the strict zero-failure condition governs.

Output Summary: baseline test state is 7196 total, 7196 passed, 0 failed, 0 skipped, with
`Test Run Successful.` present and both success-only literals present. Exit code 0, so the
repository-wide 80% line-coverage assert passed and no threshold exception was raised. The runner's
own first-party coverage line reads `lines 55968/65376 (85.61%), branches 13475/16886 (79.80%)`.

Coverage Headline: line-rate=0.856094 lines-valid=65376 lines-covered=55968

The three headline values above are the Cobertura root-element attributes read by `[P0-T11]` from
`coverage/coverage.cobertura.xml`, not values transcribed from the runner's console line. The
runner's `85.61%` console figure is the same quantity rounded to two decimal places.
