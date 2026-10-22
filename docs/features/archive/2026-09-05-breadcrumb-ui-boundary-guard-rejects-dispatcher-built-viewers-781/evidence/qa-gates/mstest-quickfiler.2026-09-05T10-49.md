# Final QC Step 5 — Scoped QuickFiler.Test gate (issue #781)

Timestamp: 2026-09-05T17-03

Task: [P2-T5]

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx "/ResultsDirectory:TestResults\final-quickfiler-781" "/TestCaseFilter:TestCategory!=LiveOutlook" "/Settings:scripts\vscode\TaskMaster.cli.runsettings"`

Issued from the repository root inside a `pwsh -NoProfile -Command` process, using the
`vstest.console.exe` resolved by [P0-T4]. `/InIsolation` is mandatory: without it the assembly
binding redirects in the test assembly's `.config` are ignored and the run produces mass
empty-message sub-millisecond failures that are an invocation defect rather than a regression.

EXIT_CODE: 0

No `ExpectedExitCode:` line is written, because the failed count is 0 and the task's mechanical
rule prescribes exactly that outcome for this case.

## Output Summary

- Total tests: **1339**
- Passed: **1339**
- Failed: **0**
- Skipped: **0**
- Total time: 13.7464 seconds
- Run result: `Test Run Successful.`

Fully-qualified names of failed tests: **none**. The runner printed no `Failed ` line.

Both acceptance conditions hold:

1. Every failed test name recorded here is a member of `BASELINE_FAILURE_SET` from [P0-T8]. The
   set of failed names is empty, so the condition holds vacuously in the only way it can: the
   failure set did not grow, because there are no failures.
2. The exit-code expectation follows the task's mechanical rule. The failed count is 0, so
   `EXIT_CODE:` is 0 and no `ExpectedExitCode:` line is written. The other two branches of that
   rule were not reached.

The stall condition the task warns about did not occur: the run completed in under 14 seconds on
the first attempt, so no CPU-time sampling, no scoped process termination, and no rerun were
required. No file in the Write Set changed during this step.

The `.trx` produced under `TestResults\final-quickfiler-781\` was not copied into this evidence
folder, per the plan's convention on host tokens in TRX files.
