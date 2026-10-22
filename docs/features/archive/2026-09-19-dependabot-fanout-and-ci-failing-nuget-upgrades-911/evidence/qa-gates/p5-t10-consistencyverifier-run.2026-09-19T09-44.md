# P5-T10 — ConsistencyVerifier suite run

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t10-verifier-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=11 Failed=0 Skipped=0 Total=11
EXECUTED=11
```

No filter is applied by this task, so the discovered population and the executed population
are the same and both read 11.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| `Total` | at least 11 | 11 |

The bound matches P5-T9's: at 9 it would be satisfied by a run over a suite that omits the
fifth surface.

## Named cases in the Detailed output

```
PASSED: reports no disagreement and a non-zero examined count for an agreeing project
PASSED: reports a disagreement for a project whose analyzer item names a stale version
PASSED: reports no orphan and a non-zero examined count when the manifest declares the package
PASSED: reports an orphan when the manifest declares no matching package
PASSED: reports no missing reference and a non-zero examined count for a complete project
PASSED: reports a missing reference when the hint path for a resolved asset is absent
PASSED: reports nothing absent and a non-zero examined count when every element has an entry
PASSED: reports an element whose package the manifest does not declare
PASSED: aggregates no finding and a non-zero examined item count when the repair returned no record
PASSED: aggregates and counts the records the analyzer repair returned
PASSED: reports exactly 2 guarded imports of an unmanifested package and still succeeds
```

The three cases this task names explicitly are present and passing:

- the absent-from-manifest case asserting exactly 2 reported instances and a success
  result — `reports exactly 2 guarded imports of an unmanifested package and still succeeds`;
- the two missing-Roslyn-segment aggregation cases — `aggregates no finding and a non-zero
  examined item count when the repair returned no record` and `aggregates and counts the
  records the analyzer repair returned`.

## Coverage document

The JaCoCo document is at `coverage/p5-t10-verifier-coverage.xml`, which `.gitignore:144`
covers. This task records no coverage figure, so gate rule 12's standing-in obligation does
not fall on it; that obligation belongs to the six tasks that record a JaCoCo LINE figure,
of which P6-T3 is the one in this batch.
