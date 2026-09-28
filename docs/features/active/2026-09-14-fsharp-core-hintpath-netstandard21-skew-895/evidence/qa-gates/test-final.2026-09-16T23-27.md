# Phase 4 — Toolchain Step 4: Tests With Coverage (Issue #895)

Timestamp: 2026-09-17T01-27
Task: [P4-T9]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: held from `[P4-T5]` (`ACQUIRED 895`, exit 0) and released after this task
(`RELEASED by 895`, exit 0).

Command (inside a WT-PREAMBLE payload):

```
& "scripts/vscode/Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug *> "coverage/logs/p4-t9-runner.txt"
$LASTEXITCODE
```

EXIT_CODE: 0
ExpectedExitCode: 0

The expectation is keyed to the measured run: it reported `COUNTERS_FAILED=0` and no threshold
literal matched.

Raw runner console log: `coverage/logs/p4-t9-runner.txt` (git-ignored, not committed).

Cobertura Document State: POSTPROCESSED

`RUNNER_THREW_ON_TESTS=0`, so the runner did not throw before post-processing;
`RUNNER_THREW_ON_THRESHOLD=0` with exit 0, so neither threshold branch fired.

## Output Summary:

```
RUNNER_THREW_ON_TESTS=0
RUNNER_THREW_ON_THRESHOLD=0
DISCOVERED_LINE=1
DOC_LINE_RATE=0.858678 DOC_LINES_VALID=65616 DOC_LINES_COVERED=56343 DOC_BRANCH_RATE=0.800317
COUNTERS_TOTAL=7311 EXECUTED=7311 PASSED=7311 FAILED=0
NEW_TEST_RESULT_COUNT=18
NEW_TEST_PASSED_COUNT=18
```

Coverage headline: repository-wide line rate 0.858678 (85.8678 percent) over 65616 valid lines with
56343 covered; branch rate 0.800317 (80.0317 percent).

Test counters: 7311 total, 7311 executed, 7311 passed, 0 failed. The total is the `[P0-T7]` baseline
of 7293 plus the 18 results the two new classes contribute.

## Failing Test Names:

NONE

## NEWLY-FAILING:

NONE

No test failed, so the set of failing names absent from the `[P0-T7]` `Baseline Failing Set:` is
empty. The re-run rule was not triggered: this step was run once, and the single run is the measured
run. Nothing was serialised, retried or relaxed.

## RUNSETTINGS-UNCHANGED:

NONE

`git diff --numstat origin/main -- scripts/vscode/TaskMaster.cli.runsettings` printed no output, so
the settings file is byte-identical to `origin/main` and the run executed under the repository's
standard `Workers=0` / `Scope=ClassLevel` configuration. No `[DoNotParallelize]` attribute was added
anywhere by this change: `[P4-T12]` measures that directly and reads 0, 0 and 2 for the three files
this plan touches, the 2 being the pre-existing pinning that issue #879 placed on
`NetstandardBindChildDomainTests`.

## New tests among the passing results

The TRX carries 18 results whose names begin `DeployedFSharpCore_ReferencesNetstandard20`,
`Detector_OnPackageNetstandard21Binary_Reports21`, `SolutionHasExactlySixFSharpCoreHintPaths` or
`EveryFSharpCoreHintPath_SelectsNetstandard20`. `NEW_TEST_RESULT_COUNT=18` and
`NEW_TEST_PASSED_COUNT=18`, so all eighteen are present and all eighteen passed inside the
full-suite run, not only inside the scoped runs.

## THRESHOLD-BREACH:

NONE. `RUNNER_THREW_ON_THRESHOLD=0`, so neither the 80 percent line threshold nor the 75 percent
branch threshold was breached, and no repository-wide finding arises from this run.

## Acceptance

- The artifact exists with every numeric figure present as a number: yes.
- `RUNSETTINGS-UNCHANGED: NONE`: yes.
- `Cobertura Document State:` carries one of its two permitted values: `POSTPROCESSED`.
- `NEWLY-FAILING:` is present: yes, `NONE`.
- The measured run's `COUNTERS_FAILED=0` with every new test among its `Passed` results: yes, 0
  failed and all 18 new results passed.

AC4-TEST-CLAUSE: MET
