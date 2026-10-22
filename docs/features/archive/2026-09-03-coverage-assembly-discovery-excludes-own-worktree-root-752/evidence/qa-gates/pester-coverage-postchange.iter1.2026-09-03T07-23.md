# Post-change Pester and Coverage, Iteration 1 ([P3-T5])

Timestamp: 2026-09-03T12-15

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; "COVERAGE LinePercent=$($r.CodeCoverage.CoveragePercent)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`

EXIT_CODE: 0

## Emitted lines, verbatim

```
PESTER Passed=95 Failed=0 Skipped=0 Total=95
COVERAGE LinePercent=78.3312577833126
```

POST-CHANGE LINE COVERAGE PERCENT: 78.3312577833126

Output Summary: 95 passed, 0 failed over the whole `tests/scripts/vscode` scope. The `Passed=` count exceeds the `[P0-T10]` baseline `Passed=92` by exactly 3, which is the number of `It` blocks this plan adds, so no pre-existing test was lost and no unexpected test was gained. Pester's own detailed console summary for this run reads `Covered 78.33% / 75%. 803 analyzed Commands in 11 Files.` against the baseline run's `Covered 78.3% / 75%. 802 analyzed Commands in 11 Files.` The coverage XML for this iteration is at `evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.xml`; the comparison against the baseline figure is recorded by `[P3-T6]`.
