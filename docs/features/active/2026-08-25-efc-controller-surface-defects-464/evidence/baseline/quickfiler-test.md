# Phase 0 — baseline QuickFiler.Test result

Timestamp: 2026-08-27T23-24
Task: [P0-T12]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=baseline-quickfiler-test.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\baseline\trx\p0-t12` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

The vstest.console path is the one resolved in `[P0-T4]`.

## Result

```
Test Run Successful.
Total tests: 1099
     Passed: 1099
 Total time: 10.6728 Seconds
```

TRX `<Counters>` element, verbatim:

```
total="1099" executed="1099" passed="1099" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

| Metric | Value |
|---|---|
| Total executed | 1099 |
| Passed | 1099 |
| Failed | 0 |
| Skipped / not executed | 0 |

BASELINE_PASSED: 1099

## BASELINE_FAILED

```
BASELINE_FAILED: (none)
```

Cardinality: **0**.

No test failed at the baseline. `[P10-T6]` consumes this as a set: the final-QC run is permitted to fail
only on a test named in this set, and because the set is empty the final run must report **zero**
failures.

## Non-vacuity

The total executed count is 1099, which is greater than zero, so the run discovered and executed tests
rather than silently matching none.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/baseline/trx/p0-t12/baseline-quickfiler-test.trx`
exists.

The TRX was sanitised in place before being retained: every absolute worktree path was replaced with
`<repo-root>` (2199 substitutions), and the account name, machine name and deployment-root string in the
`<TestRun>`, `<Deployment>` and per-result `computerName` attributes were replaced with `<user>` and
`<host>`. A case-insensitive search of the retained file for the account name and the machine name now
returns zero matches. No test name, outcome, duration or counter value was altered by the sanitisation.

Output Summary: vstest exits 0 with 1099 of 1099 tests passing and zero failures. BASELINE_PASSED is
1099 and BASELINE_FAILED is the empty set. TRX retained and sanitised of host-identifying strings.
