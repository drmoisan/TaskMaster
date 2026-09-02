# Baseline — Test and Coverage State (Issue #656)

Timestamp: 2026-09-01T14-39
Task: [P0-T11]

Run Start: 2026-09-01T14:38:47.4405464-04:00
Run End:   2026-09-01T14:39:35.3403481-04:00

Command:
```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .
```
with console output tee'd to `TestResults\p0-t10\coverage-run.log`.

EXIT_CODE: 0

## Baseline run results

- Baseline Total Tests: 6925
- Baseline Passed Tests: 6925
- Baseline Failed Tests: 0
- Baseline Failure Set: none

`EXIT_CODE: 0` strictly implies zero failed tests, because
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws when the inner vstest exit code is non-zero.
The run summary in the tee'd log reports `Total tests: 6925` and `Passed: 6925` with no `Failed:`
line, which agrees. The `BASELINE FAILURES PRESENT` branch of this task was not taken, and the
`BLOCKED` branch for a failing `QuickFiler.Test` was not taken.

The wrapper produces no TRX: it passes `/Settings:`, `/InIsolation` and
`/TestCaseFilter:TestCategory!=LiveOutlook` and no `/Logger:trx`, and the referenced runsettings
declares no logger. Test counts and failure names for a wrapper run are therefore read from the
tee'd console log, as the plan's execution preconditions record.

## Baseline coverage values

- Baseline Repo Line Rate: 0.853792
- Baseline Repo Lines Covered: 54968
- Baseline Repo Lines Valid: 64381
- Baseline Coordinator Line Rate: 0.983122
- Baseline Coordinator Lines Covered: 233
- Baseline Coordinator Lines Valid: 237

Derivation: the three repository values are the `line-rate`, `lines-covered` and `lines-valid`
attributes of the root `/coverage` element of `coverage\coverage.cobertura.xml`. The three
coordinator values come from the single `class` node whose `filename` attribute equals
`QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs`; exactly one such node exists, because the
wrapper's post-processing collapses a file to a single `class` node and rewrites `filename` to the
repo-relative backslash form. `Coordinator Lines Valid` is the count of `line` elements selected by
the class-relative XPath `./lines/line`, and `Coordinator Lines Covered` is the count of those whose
`hits` attribute exceeds 0. The class-relative rollup is used rather than the descendant axis
because Cobertura repeats every line under `./methods/method/lines`, which would roughly double a
descendant count. No `lines-covered` or `lines-valid` attribute exists on a `class` node; those two
attributes are set on the root `coverage` node only.

The repository line rate of 0.853792 is above the 0.80 floor that P4-T8 asserts and above the 0.85
floor in `.claude/rules/general-unit-test.md`.

Output Summary: Baseline test-and-coverage run passed. 6925 tests total, 6925 passed, 0 failed, no
failure set. Repository line rate 0.853792 (54968 of 64381 lines). Coordinator line rate 0.983122
(233 of 237 lines). All six numeric coverage fields and all four run fields are present and numeric.
