# Phase 6 — Tests with coverage

Timestamp: 2026-09-09T13-53
Task: [P6-T5]

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/coverage.cobertura.xml`
EXIT_CODE: 0

The run was executed detached and its console output sampled, per the plan's hang-risk rule. It
completed on its own without stalling; the twenty-minute-no-output fallback was **not** triggered and
the per-assembly fallback runs were **not** used. Captured output is 7237 lines; the material tail is
reproduced verbatim below.

```text
Test Run Successful.
Total tests: 7208
     Passed: 7208
 Total time: 29.4461 Seconds
Code coverage results: <repo-root>\coverage\coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
First-party coverage: lines 56021/65435 (85.61%), branches 13481/16892 (79.81%)
Done. Coverage artifact: <repo-root>\coverage\coverage.cobertura.xml
```

## The three required summary items

**(a) Is the literal `Post-processing coverage XML for Koverage compatibility...` present?**
**Yes.** It prints only after the inner vstest run exited 0, so it is the success-only observable for
a zero-failure run.

**(b) Is the literal `Done. Coverage artifact:` present?**
**Yes.** It prints only after `Assert-CoberturaLineCoverageThreshold` passes, so the repository-wide
80% line threshold was met and the assert did not throw.

**(c) vstest counts.**

| Count | Value | Source |
|---|---|---|
| Total | **7208** | `Total tests: 7208` |
| Passed | **7208** | `Passed: 7208` |
| Failed | **0** | see note |
| Skipped | **0** | see note |

Note on the failed and skipped counts: a fully passing vstest run prints `Total tests:` and `Passed:`
and prints **no `Failed:` line and no `Skipped:` line at all**. A `Select-String -SimpleMatch` search
of the captured output for `Failed:` and for `Skipped:` each returned **0 matches**, and a search for
`Test Run Failed.` also returned 0. Both counts are therefore recorded as `0` because **the lines were
absent**, not because the values were unreadable.

## Acceptance check

The `[P0-T10]` baseline artifact records `Post-processing coverage XML for Koverage compatibility...`
as **present**, so the plan's **first** branch governs: this run must record it as present too, and
the recorded failed count must be `0`. Both hold. The second branch — the
subset-of-baseline-failures branch — does not apply, because the baseline was not red.

| Condition | Required | Observed | Met |
|---|---|---|---|
| `Post-processing coverage XML for Koverage compatibility...` present | yes | **yes** | yes |
| Recorded failed count | `0` | **0** | yes |
| Failures in `QuickFiler.Test.dll` | none | **none** | yes |
| Failures in `UtilitiesCS.Test.dll` | none | **none** | yes |

## Test-count reconciliation against the baseline

| Run | Total | Passed | Failed |
|---|---|---|---|
| `[P0-T10]` baseline | 7196 | 7196 | 0 |
| this run | **7208** | **7208** | **0** |
| delta | **+12** | +12 | 0 |

The delta of +12 is exactly the number of test methods this plan added: one in
`QfcHomeControllerCleanupTests.cs`, one in `EfcHomeControllerLifecycleTests.cs`, five in
`ProgressViewer_Tests.cs` and five in `ProgressPane_Tests.cs`.
`Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` was modified rather than added, so it does not
contribute to the delta. No pre-existing test was removed, renamed or skipped.

Output Summary: 7208 total, 7208 passed, 0 failed, 0 skipped, with `Test Run Successful.` present and
both success-only literals present. Exit code 0, so the repository-wide 80% line-coverage assert
passed. The test-count delta of +12 against the baseline accounts exactly for the twelve tests this
plan added, with no pre-existing test lost.

Coverage Headline: line-rate=0.856132 lines-valid=65435 lines-covered=56021

The three headline values above are the Cobertura root-element attributes read by `[P6-T9]` from
`coverage/coverage.cobertura.xml`, not values transcribed from the runner's console line. The
runner's `85.61%` console figure is the same quantity rounded to two decimal places.
