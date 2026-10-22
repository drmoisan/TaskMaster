# Phase 0 — Coverage-Bearing Test Baseline (Issue #895)

Timestamp: 2026-09-17T01-15
Task: [P0-T7]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: held from `[P0-T6]` (`ACQUIRED 895`, exit 0) and released after this task
(`RELEASED by 895`, exit 0).

Tree state: before either new test file exists. The output tree under test is the one produced by
the `[P0-T6]` whole-solution `/t:Rebuild`.

Command (inside a WT-PREAMBLE payload):

```
& "scripts/vscode/Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug *> "coverage/logs/p0-t7-runner.txt"
$LASTEXITCODE
```

EXIT_CODE: 0
ExpectedExitCode: 0

The expectation is 0 because the measured run reported `COUNTERS_FAILED=0` and no threshold literal
matched.

Raw runner console log: `coverage/logs/p0-t7-runner.txt` (git-ignored, not committed). The
`.cobertura.xml` and `.trx` documents stay under the git-ignored `coverage/` directory.

Cobertura Document State: POSTPROCESSED

`RUNNER_THREW_ON_TESTS=0`, so the runner did not throw at line 262 before post-processing;
`RUNNER_THREW_ON_THRESHOLD=0` and the exit code is 0, so neither threshold branch fired and the
document on disk is the post-processed one.

## Output Summary:

```
RUNNER_THREW_ON_TESTS=0
RUNNER_THREW_ON_THRESHOLD=0
DISCOVERED_LINE=1
DOC_LINE_RATE=0.858708 DOC_LINES_VALID=65616 DOC_LINES_COVERED=56345 DOC_BRANCH_RATE=0.800493
COUNTERS_TOTAL=7293 EXECUTED=7293 PASSED=7293 FAILED=0
```

Coverage headline: repository-wide line rate 0.858708 (85.8708 percent) over 65616 valid lines with
56345 covered; branch rate 0.800493 (80.0493 percent). Test counters: 7293 total, 7293 executed,
7293 passed, 0 failed.

## Baseline Failing Set:

NONE

No test failed on the baseline run. In particular neither
`DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` (issue #780) nor either of the
two `ItemViewerBreadcrumbThreadAffinityTests` worker-thread tests failed in this run, so any failure
of those names at `[P4-T9]` would be recorded as `NEWLY-FAILING` under that task's stated rule.

No stall was observed; the run completed within the command timeout, so the execution-risk re-run
branch was not taken.

## Coverage Obligations:

- Repository-wide line coverage floor: `>= 80%` on the testable denominator (CLAUDE.md UT2).
- New module, class or method coverage floor: `>= 90%`.
- No regression on changed lines.

CHANGED-PRODUCTION-LINES-EXPECTED: 0 (this fix changes no production `.cs` file; the three edited
project files are `.csproj`, and the three `.cs` files the plan writes are all test code).

NEW-MODULE-COVERAGE-EXPECTED: N/A (the new files are test code, excluded from instrumentation by the
runner's run-time `.*\.Test\.dll$` module exclusion, so they are outside the coverage denominator).
