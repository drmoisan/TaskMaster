# P5-T16 — AC14 binding redirects reconciled to the resolved assembly version

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1"); $c.Filter.FullName = "*AC14-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t16-ac14-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=2 Failed=0 Skipped=0 Total=13
EXECUTED=2
NOTRUN=11
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| `Total` | exactly 2 | **executed 2**; discovered 13 |

The exact-2 clause is evaluated against the executed population for the reason P5-T5
records.

## Named cases in the Detailed output

```
PASSED: AC14- writes the resolved version into both the oldVersion upper bound and newVersion
PASSED: AC14- returns an app.config carrying no redirect for the assembly unchanged
```

Both cases this task requires to be named are present.

- The redirect-reconciled case asserts the resolved version appears in **both** positions:
  `oldVersion="0.0.0.0-2.0.0.0"` and `newVersion="2.0.0.0"`. The fixture entered the run at
  `0.0.0.0-1.0.3.0` and `1.0.3.0`, so an implementation that moved `newVersion` alone and
  left the range bound behind fails.
- The no-redirect case asserts the input is returned unchanged, verified with a
  case-sensitive `-BeExactly` comparison against the fixture and a zero repair count. The
  function still examines the one `<dependentAssembly>` block present — recorded as
  `ExaminedCount=1` at P5-T7 — so the unchanged result is a decision rather than an early
  return that looked at nothing.

## Coverage document

`coverage/p5-t16-ac14-coverage.xml`, under the gitignored `coverage/` tree. This task
records no coverage figure.

**This task checks off AC14** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
