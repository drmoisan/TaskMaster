# Phase 5 Step 4 — Tests With Coverage, Final Toolchain Loop

Recorded by `[P5-T7]`. This artifact records the Revision R7 re-execution of the loop; each
attempt overwrites its own artifact.

Timestamp: 2026-09-14T12-53

Build lock: ACQUIRED 879 at 2026-09-14T12:50:37, RELEASED by 879 at 2026-09-14T12:53:12.

Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>
& "scripts/vscode/Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug'`

EXIT_CODE: 0
ExpectedExitCode: 0

`-CoverageOutput` was left at its default so the raw Cobertura document is retained, for the
issue 873 reason recorded at `## R4.6`: `Test-RawCoverageDocumentRetained` deletes the raw
document unless its parent directory is exactly `<repoRoot>\coverage`, by equality and not by
containment.

The runner's repository-wide threshold assertion runs after the Cobertura document has been
written. It did not fire on this run: zero tests failed, so the runner did not throw on a
non-zero collection exit code, post-processing ran, and
`Assert-CoberturaLineCoverageThreshold` asserted against the post-processed first-party
document-level rate of `0.858647`, which clears its hard-coded 80 percent threshold. Issue #891
remains unfixed by this plan; this run simply did not meet its failing condition.

Output Summary:

```
DOC_LINE_RATE=0.858647
DOC_LINES_VALID=65616
DOC_LINES_COVERED=56341
DOC_BRANCH_RATE=0.800376
DOC_BRANCHES_VALID=17022
DOC_BRANCHES_COVERED=13624
TRX_MATCH_COUNT=1
RESULT_SUMMARY_OUTCOME=Completed
COUNTERS_TOTAL=7293
COUNTERS_PASSED=7293
COUNTERS_FAILED=0
```

The document-level `line-rate`, `lines-valid` and `lines-covered` are read from
`coverage/coverage.cobertura.xml`. The total, failed and passed counts are read from
`coverage/test-results/mstest-coverage-run.trx`. The runner's own console line reports the same
first-party figures: lines 56341/65616 (85.86%), branches 13624/17022 (80.04%).

Failing Test Names: NONE

The failed count is zero, so the list is recorded as the literal `NONE`. An empty set is a
subset of the two baseline failures recorded at `[P0-T8]`,
`ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` and
`InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic`, so the acceptance
condition is satisfied. Both of those tests passed on this run. They are intermittent, which is
why the acceptance condition is written as a subset rather than as an equality on the count: a
run in which the pair happens to pass must not block for that reason either.

Cobertura Document State: POSTPROCESSED. The runner reached its post-processing step because no
test failed, so the retained document is first-party only rather than raw collector output.
This differs from the `[P0-T8]` baseline document state, and `[P5-T10]` records the consequence
for the rate comparison.

No raw `.trx` and no raw `.cobertura.xml` is added to git by this task. `coverage/` is
git-ignored by `coverage/*` at `.gitignore` line 144 and `TestResults/` by the
`[Tt]est[Rr]esult*/` pattern at line 39.
