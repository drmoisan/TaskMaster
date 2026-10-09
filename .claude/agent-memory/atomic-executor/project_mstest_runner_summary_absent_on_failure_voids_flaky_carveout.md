---
name: mstest-runner-summary-absent-on-failure-voids-flaky-carveout
description: Invoke-MSTestWithCoverageMain writes mstest-coverage-run.summary.txt only after a zero collector exit, so a flaky-test carve-out keyed on its "Failed tests:" line can never fire; key it on the raw trx instead
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws `MSTest with coverage failed with exit code N`
inside `Invoke-DotnetCoverageCollection` (line 262 as of 2026-10-09) before post-processing, the
projection, and the summary file are produced. The `.summary.txt` holding
`Failed tests: <names>` therefore exists only on a green run, and it always reads `Failed tests: none`.

**Why:** plan #985 r0 C7 allowed one re-measurement when `RUNNER_RESULT=THREW` and the
`TRX-SUMMARY: Failed tests:` line named only the known flaky test (#780). On a red run that line is
never printed (the wrapper deletes the stale summary first), so the rule was unreachable and a
single flaky failure would have become a hard STOP.

**How to apply:** at preflight, a flaky carve-out over this runner must read failed names from
the raw `coverage\test-results\mstest-coverage-run.trx` (still present after the throw), e.g.
`//*[local-name()='UnitTestResult'][@outcome='Failed']` -> `testName`, printed as
`TRX-FAILED-TEST:` lines (names only, no paths). Related: [[flaky-test-carveout-added-to-one-task-only]].
