# Final test run with coverage (P7-T6)

Timestamp: 2026-09-01T11-10
Task: [P7-T6], second and clean attempt
Working directory: WORKTREE

Command:

```
pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\post-change.cobertura.xml
```

EXIT_CODE: 0

Run window: started 2026-09-01T11-10-01, ended 2026-09-01T11-10-54.

## Verbatim vstest result summary

```
Test Run Successful.
Total tests: 6924
     Passed: 6924
 Total time: 30.5352 Seconds
```

## Counts

| Metric | Value | Baseline (P0-T10) |
|---|---|---|
| Total | 6924 | 6912 |
| Passed | 6924 | 6912 |
| **Failed** | **0** | 0 |
| Skipped | 0 | 0 |

The total rose by exactly 12: the seven queue-level tests from P5-T2 through P5-T8 and the five ordering
tests in `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`. No test was removed and no
test was skipped. A scan of the 6951-line transcript for lines beginning with the vstest outcome tokens
`Failed` or `Error` returns 0 matches.

## Coverage post-processing

```
Code coverage results: WORKTREE\coverage\post-change.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: WORKTREE\coverage\post-change.cobertura.xml
```

The wrapper reached `Done.`, which is the observable signal that `Invoke-DotnetCoverageCollection`
returned and `Assert-CoberturaLineCoverageThreshold` did not throw, so `Set-Content` ran and the file on
disk is the first-party-filtered artifact. P7-T7 verifies that classification directly.

## Relationship to the first attempt

This is the second Phase 7 attempt. The first attempt failed with exactly one test failure,
`DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` in `UtilitiesCS.Test`, which is a
pre-existing `Console.SetOut` parallelism flake in a different assembly with no dependency path to any
file this change touches. That analysis, including a scoped run showing the test passes in isolation and
the repository's own prior mitigation of the identical hazard on a sibling class, is recorded in
`FEATURE/evidence/other/p7-loop-attempt-1-failure.2026-09-01T11-08.md`.

Per the Phase 7 restart rule the loop was restarted from P7-T1 with **no file edited in between**. The
same test passed on this attempt, which is the behaviour a scheduling-dependent flake exhibits and is
not the behaviour a genuine regression exhibits. No unrelated pre-existing failure was fixed, and
nothing outside the authorized blast radius was touched.

The `BASELINE_FAILURE_SET` from P0-T12 is `NONE`, and the failed count in this run is 0, so no
comparison against that set was required and the `REMEDIATION-REQUIRED` branch was not taken.

This artifact is one of the four that the AC19 check-off in P8-T23 depends on. All four gates of this
uninterrupted pass — P7-T3 format check, P7-T4 analyzer, P7-T5 type check, and this test run — completed
with no intervening file edit.
