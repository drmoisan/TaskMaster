# P6-T6 — Full-Suite Test and Coverage Run (post-change)

Timestamp: 2026-08-31T20-50
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
EXIT_CODE: 0
ExpectedExitCode: 0
Iteration: 1

DISCOVERED_ASSEMBLY_COUNT: 9

## Test counts

- Total: 6899
- Passed: 6899
- Failed: 0
- Skipped: 0

Failed test names: none.

The runner reported `Discovered 9 test assemblies.`, then `Test Run Successful.` with `Total tests: 6899` and `Passed: 6899`, then completed its coverage stage with `Post-processing coverage XML for Koverage compatibility...` and `Done. Coverage artifact: ...\coverage\coverage.cobertura.xml`. The run was started detached and polled to completion; no partial result was recorded.

## Expectation selection

`ExpectedExitCode:` is 0, selected by applying the task's three rules in order.

1. **First rule — not taken.** It applies only when the run reports at least one Failed test. This run reported none.
2. **Second rule — not taken.** It applies only when P0-T15 recorded `BASELINE_COVERAGE_BELOW_FLOOR:`. `evidence/baseline/p0-t15-full-suite-coverage.md` records that field as not applicable, because the baseline runner exited 0 without `Assert-CoberturaLineCoverageThreshold` throwing. Independently, this run's derived `POST_LINE_RATE:` of 0.852919 is above 0.80, so the rule's second condition also fails.
3. **Third rule — taken.** `ExpectedExitCode: 0`.

The observed `EXIT_CODE:` is 0 and equals the declared expectation.

## Post-change coverage figures, governing derivation

DERIVATION_BRANCH: the on-disk `coverage\coverage.cobertura.xml` already contained a `<sources>` element, so it is the post-processed output the successful runner wrote and its root `coverage` attributes were read directly. This is the same branch the P0-T16 baseline derivation took, so baseline and post-change figures are on one identical denominator.

POST_LINE_RATE: 0.852919
POST_LINES_COVERED: 54835
POST_LINES_VALID: 64291
POST_BRANCH_RATE: 0.792754
POST_BRANCHES_COVERED: 13063
POST_BRANCHES_VALID: 16478

All six hold numbers.

## Corroboration, not a second measurement

`Invoke-MSTestWithCoverage.ps1` line 341 calls `Assert-CoberturaLineCoverageThreshold` on the output of the same `ConvertTo-KoverageCoberturaXml` call at line 340, and that assertion reads the root `line-rate` attribute and throws below 80 percent. It did not throw, which corroborates `POST_LINE_RATE:` 0.852919 being above 0.80. That is one figure observed twice on one denominator, not two measurements. Every number recorded above comes from the governing derivation; none is taken from the runner's console output.

Output Summary: 6899 of 6899 tests passed across 9 assemblies, the runner exited 0, and all six numeric coverage fields were derived and recorded.
