# P6-T3 — External-Output End-to-End Run (acceptance MET)

Timestamp: 2026-09-13T14-59

Task: [P6-T3]

Command: `pwsh -NoProfile -Command '. <worktree-root>/scripts/vscode/Invoke-MSTestWithCoverage.ps1; Invoke-MSTestWithCoverageMain -CoverageOutput "coverage\tm873-external-output\coverage.cobertura.xml" -ResultsDirectory "coverage\tm873-external-output"'`

EXIT_CODE: 0

ExpectedExitCode: 0

Output Summary: The run completed end to end with both the coverage output and the results directory pointed beneath `coverage\tm873-external-output`. 7222 tests executed, 7222 passed, 0 failed, in 32.62 seconds. After the run the raw coverage document is ABSENT from the chosen output directory and the projection and the test-result summary are both PRESENT there, which is the discard branch this task exists to observe. This task's acceptance is MET.

VERDICT: PASS.

## Directories Used

Both directories are named repository-relative so that no absolute host path enters this artifact, as the task requires.

COVERAGE_OUTPUT: `coverage\tm873-external-output\coverage.cobertura.xml`

RESULTS_DIRECTORY: `coverage\tm873-external-output`

Both parameters were pointed at the same directory. The results directory is set explicitly rather than left at its default, because it is an independent parameter and its default would have written the summary back beneath the repository coverage directory itself, where this task's acceptance could not observe it.

The value is stated repository-root-relative rather than drive-rooted, because both parameters are resolved with `Join-Path` against the repository root and `Join-Path` concatenates without analysing the child, so a drive-rooted value would resolve to a composite path that the output-directory creation could not create.

## How The Parameters Were Supplied

The task calls for running the entry point with both parameters set. The top-level `param()` block of `scripts/vscode/Invoke-MSTestWithCoverage.ps1` exposes `SearchRoot`, `Configuration`, `CoverageOutput` and `NoExecute`, but not `ResultsDirectory`; `ResultsDirectory` is a parameter of `Invoke-MSTestWithCoverageMain`, which carries the default `coverage\test-results`.

The script provides the seam for this explicitly. Its final block reads:

```
if ($MyInvocation.InvocationName -ne '.') {
    Invoke-MSTestWithCoverageMain @PSBoundParameters
}
```

Dot-sourcing the script therefore suppresses the auto-invocation and exposes `Invoke-MSTestWithCoverageMain` for a direct call with both parameters bound. That is the route used, and it exercises exactly the same function body, on exactly the same code path, as a plain script invocation. This is recorded rather than left implicit so a reviewer does not have to rediscover why the invocation shape differs from P6-T2's.

## Acceptance Observations

RAW_COVERAGE_DOCUMENT_PRESENT_IN_OUTPUT_DIRECTORY: false

PROJECTION_FILE_PRESENT: true

TEST_RESULT_SUMMARY_PRESENT: true

Full listing of the chosen directory after the run, repository-relative:

```
coverage\tm873-external-output\coverage.cobertura.jacoco.xml
coverage\tm873-external-output\mstest-coverage-run.summary.txt
coverage\tm873-external-output\mstest-coverage-run.trx
```

SUBDIRECTORY_COUNT: 0

The raw `coverage.cobertura.xml` the collector wrote to this directory is absent from the listing, which is the discard. The projection and the summary remain.

## Why This Directory Exercises The Discard Branch

The operative obligation is the spec's invariant 4: the raw document is kept when the resolved output directory is the repository coverage directory and discarded when it is any other directory. `coverage\tm873-external-output` is a subdirectory of the coverage tree, not the coverage directory itself, so `Test-RawCoverageDocumentRetained` returns false for the resolved output path and the discard runs. The listing above confirms it did.

The directory was chosen to sit beneath the already-ignored coverage tree deliberately, so that neither of this phase's runs adds a path the P7-T9 footprint inventory would have to admit or that the P7-T16 clean-tree gate would report. That property is confirmed in the P6-T4 artifact: the post-merge changed-path union contains no path under `coverage/`.

## Ordering Obligation

The task discharges the ordering obligation for this path by reference rather than by observation, because an end-to-end run emits no ordered progress output from which an order could be read.

The in-process call-order test named `discards only after the threshold assertion, the projection write and the reconciliation assertion` is recorded as passed in the Phase 3 toolchain artifact `evidence/qa-gates/p3-t9-phase3-toolchain.md`. That test, not this run, is the evidence that the discard happens after the threshold assertion, the projection write and the reconciliation assertion.

## First-Party Coverage Headline Printed By The Run

```
First-party coverage: lines 56071/65416 (85.71%), branches 13500/16896 (79.90%)
```

The covered-line figure differs by 5 from the P6-T2 run's 56066 against an identical valid-line denominator of 65416. That small run-to-run movement in the covered-line count is a known property of this collector and is not a change in the code under test; no file changed between the two runs. Both figures clear the `CLAUDE.md` floors of 80% line and 75% branch.

## Elapsed Time

RUN_START: 2026-09-13T14:57:54-04:00

RUN_END: 2026-09-13T14:59:08-04:00

ELAPSED_SECONDS: 74

## Build Lock

The shared build lock was acquired for item 873 at 2026-09-13T14:57:48-04:00, held across this single run only, and released at 2026-09-13T14:59:15-04:00 immediately after the run returned.
