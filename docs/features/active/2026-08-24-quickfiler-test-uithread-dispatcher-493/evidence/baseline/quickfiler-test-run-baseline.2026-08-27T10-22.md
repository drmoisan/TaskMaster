# QuickFiler.Test Pass/Fail Baseline (P0-T12)

Timestamp: 2026-08-27T10-22
Task: [P0-T12]
Command: `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:"trx;LogFileName=quickfiler-test-baseline.trx" /ResultsDirectory:TestResults\plan-logs\p0-t12`
EXIT_CODE: 0
Output Summary: `Test Run Successful.` Total tests 1066, Passed 1066, Failed 0, Skipped 0, in
14.92 s. The base tree is fully green for `QuickFiler.Test` under the CI-parity invocation.

## Run summary

| Metric | Value |
| --- | --- |
| Verdict line | `Test Run Successful.` |
| Total tests | 1066 |
| Passed | 1066 |
| Failed | 0 |
| Skipped | 0 |

Failed and skipped are recorded as `0` because the run emitted no `Failed` and no `Skipped` result
lines and because `Total tests` equals `Passed`. `vstest.console` omits the `Failed:` and `Skipped:`
summary rows entirely when their counts are zero, so their absence from the console output is the
zero rather than a missing measurement.

## BaselineFailedTests

(empty)

**An empty list is a legitimate recorded value**, and it is the value recorded here: no test in
`QuickFiler.Test` failed at `BASE_SHA` under this invocation. Every later subset comparison against
this set — `P2-T5`, `P3-T5` — therefore reduces to an absolute `Failed: 0` requirement, which is the
strongest form those comparisons can take and is the form this baseline supports.

## Assembly and artifact notes

- Test assembly: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`, produced by the `P0-T9`
  `/t:Rebuild`, which is the most recent full rebuild at this point in the plan and which overwrote
  `P0-T8`'s output from the same sources.
- TRX name is controlled by `LogFileName=quickfiler-test-baseline.trx` so the file carries no
  account or host name. It is written under the git-ignored
  `TestResults/plan-logs/p0-t12/` tree and is not committed.
- `/EnableCodeCoverage` also produced a `.coverage` attachment whose default filename embeds the
  account and machine name. That file sits in the same git-ignored tree; its path is deliberately not
  quoted in this artifact.
- Console log: `TestResults/plan-logs/p0-t12/vstest.out.log` (git-ignored).

## Invocation note

The run was executed twice. The first execution was launched detached through
`Start-Process -PassThru -NoNewWindow` and completed in about 15 s, but the process object was not
retained long enough to read `$proc.ExitCode`. Because the whole run costs 15 s, it was repeated in
the foreground with `$LASTEXITCODE` captured directly rather than inferring the exit code from the
`Test Run Successful.` line. Both executions reported the identical summary (1066 total, 1066
passed); the recorded `EXIT_CODE: 0` is the directly observed value from the second execution.
