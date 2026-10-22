# Baseline — MSTest run with coverage

- Timestamp: 2026-09-02T01-08
- Issue: #678
- Task: [P0-T8]

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .
```

`-SearchRoot .` is mandatory; without it the runner's assembly discovery does not start from
the worktree root.

EXIT_CODE: 0

## Post-processing signal

The run printed the literal `Done. Coverage artifact:`. That line is emitted only after
both the Koverage post-processing step and the on-disk write of the final report succeed, so
the report at `coverage/coverage.cobertura.xml` is a post-processed document and Derivation
D4 is not required for this baseline.

Preceding lines, in order:

```
Code coverage results: <worktree>/coverage/coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <worktree>/coverage/coverage.cobertura.xml
```

(The absolute host path the runner printed is replaced by `<worktree>` here; the runner's
own stdout carried the full path.)

## `R_BASELINE_TOTALS`

```
Test Run Successful.
Total tests: 6946
     Passed: 6946
 Total time: 45.1910 Seconds
```

| Metric | Value |
|---|---|
| Total | 6946 |
| Passed | 6946 |
| Failed | 0 |
| Skipped | 0 |

The runner prints a `Failed:` line and a `Skipped:` line only when those counts are
non-zero; neither line appears in the output, and the header is `Test Run Successful.`
rather than `Test Run Failed.`, so both counts are 0.

## `R_BASELINE_FAILURE_SET`

```
R_BASELINE_FAILURE_SET = (empty set)
```

No test failed. P2-T5's subset clause is therefore satisfiable only by an equally empty
post-change failure set, which makes that gate strictly stronger at this baseline than the
subset form alone would suggest.

## Output Summary

EXIT_CODE 0. `Test Run Successful.` with 6946 total, 6946 passed, 0 failed, 0 skipped in
45.1910 seconds. The run printed `Done. Coverage artifact:`, so the coverage report is
post-processed. `R_BASELINE_FAILURE_SET` is the empty set.
