# QA Gate — Test Gate with Coverage (Issue #656)

Timestamp: 2026-09-01T14-52
Task: [P4-T7] (toolchain loop pass 1, step 4)
Satisfies: AC-18

Run Start: 2026-09-01T14:51:30.6252415-04:00
Run End:   2026-09-01T14:52:18.0655700-04:00

Command:
```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .
```
with console output tee'd to `TestResults\p4-t7\coverage-run.log`.

EXIT_CODE: 0

## Post-change run results

- Post-Change Total Tests: 6926
- Post-Change Passed Tests: 6926
- Post-Change Failed Tests: 0
- Post-Change Failure Set: none

Test-count reconciliation required by this task: `Baseline Total Tests:` was **6925**, and
6926 = 6925 + 1. This change adds exactly one test and removes none, so the observed total is
exactly the expected total. The added test is
`CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain`.

Failure-set condition: the recorded `Post-Change Failure Set:` is `none`, so it contains no test in
`QuickFiler.Test` and is trivially a subset of the `Baseline Failure Set:` of `none` recorded in
`evidence/baseline/test-coverage.2026-08-31T20-40.md`. As the plan notes, when the baseline failure
set is `none` this subset condition reduces to `EXIT_CODE: 0`, which is the case observed, and
`EXIT_CODE: 0` strictly implies zero failed tests because the wrapper throws when the inner vstest
exit code is non-zero.

## Post-change coverage values

- Post-Change Repo Line Rate: 0.853732
- Post-Change Repo Lines Covered: 54965
- Post-Change Repo Lines Valid: 64382
- Post-Change Coordinator Line Rate: 0.983193
- Post-Change Coordinator Lines Covered: 234
- Post-Change Coordinator Lines Valid: 238

Derivation is identical to the baseline artifact: the three repository values are the `line-rate`,
`lines-covered` and `lines-valid` attributes of the root `/coverage` element of
`coverage\coverage.cobertura.xml`; the three coordinator values come from the single `class` node
whose `filename` equals `QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs`, with the line
counts taken from the class-relative XPath `./lines/line` and, for covered, those whose `hits`
attribute exceeds 0. Exactly one such `class` node exists. No `lines-covered` or `lines-valid`
attribute exists on a `class` node; those two attributes are set on the root `coverage` node only.

## Wrapper Filter Lines:

- `/TestCaseFilter:TestCategory!=LiveOutlook` — line **76** of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- `/InIsolation` — line **76** of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`

Both `Select-String` results report line 76, which is the acceptance condition. Both switches appear
on the same source line:

```
) + @($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation', '/TestCaseFilter:TestCategory!=LiveOutlook')
```

This is the recorded evidence that AC-18's "`/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook`
in effect" condition holds for this run: both are unconditionally appended to the vstest argument
list by the wrapper this gate invoked.

## No TRX for this run

The wrapper passes `/Settings:`, `/InIsolation` and `/TestCaseFilter:` and no `/Logger:trx`, and the
referenced runsettings declares no logger, so a wrapper run writes no `.trx` file. The four run
fields above are read from the vstest run summary in the tee'd console log
`TestResults\p4-t7\coverage-run.log`, which reports `Total tests: 6926` and `Passed: 6926` with no
`Failed:` line.

Output Summary: Full test gate passed. 6926 tests total, 6926 passed, 0 failed, no failure set,
which is the baseline total plus exactly the one added test. Repository line rate 0.853732 (54965 of
64382 lines); coordinator line rate 0.983193 (234 of 238 lines). Both wrapper protections confirmed
present at line 76. AC-18 is satisfied.
