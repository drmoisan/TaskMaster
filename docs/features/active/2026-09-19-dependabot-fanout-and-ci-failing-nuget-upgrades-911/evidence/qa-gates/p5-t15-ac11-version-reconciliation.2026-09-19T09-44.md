# P5-T15 — AC11 version reconciliation across all four dependent element kinds

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1"); $c.Filter.FullName = "*AC11-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t15-ac11-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=4 Failed=0 Skipped=0 Total=13
EXECUTED=4
NOTRUN=9
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| `Total` | exactly 4 | **executed 4**; discovered 13 |

The exact-4 clause is evaluated against the **executed** population, per the discrepancy
P5-T5 records: `$r.TotalCount` counts filtered-out tests as `NotRun`, so it reads 13 here
and is invariant under the filter, which makes it unable to detect either the
matched-nothing case or the over-matching case the clause exists to detect. The executed
population is `Passed + Failed + Skipped` and reads exactly 4, matching the `AC11-` `It`
count P5-T4 pinned. `NOTRUN=9` accounts for the remaining cases in the file.

## Named cases in the Detailed output

```
PASSED: AC11- reconciles the Import guard to the manifest version
PASSED: AC11- reconciles the Error guard to the manifest version
PASSED: AC11- reconciles the Reference assembly version to the manifest version
PASSED: AC11- reconciles the HintPath to the manifest version
```

One passing case per element kind, as the criterion requires. Each asserts on the line
carrying its own element and additionally asserts the stale literal is absent from that
line, so a reconciler that handled two kinds and left two alone fails here rather than
passing on an aggregate. The fixture enters the run with four **different** versions —
`<Import>` 1.0.1, `<Reference>` 1.0.2, `<HintPath>` 1.0.3, `<Error>` 1.0.4 — against a
manifest declaring 2.0.0, so no kind can pass by accident of sharing a version with
another.

## Coverage document

`coverage/p5-t15-ac11-coverage.xml`, under the gitignored `coverage/` tree. This task
records no coverage figure.

**This task checks off AC11** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
