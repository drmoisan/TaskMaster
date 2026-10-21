# P5-T14 — AC13 sibling survival in the analyzer item group

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1"); $c.Filter.FullName = "*AC13-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t14-ac13-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=3 Failed=0 Skipped=0 Total=13
EXECUTED=3
NOTRUN=10
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| `Total` | at least 3 | 3 executed (13 discovered) |

The `Total` at least 3 clause guards against a filter that matched nothing. The executed
population is 3, so the filter matched exactly the `AC13-` cases and nothing else; the
discovered figure of 13 is the whole file and, as P5-T5 records, is invariant under the
filter.

## Named cases in the Detailed output

```
PASSED: AC13- leaves the AdditionalFiles element and the preceding comment in place
PASSED: AC13- returns a project with no analyzer item group byte-identical
PASSED: AC13- repairs the items in every analyzer item group rather than the first
```

All three cases this task requires to be named are present:

- the `<AdditionalFiles>` survival case, which also asserts the explanatory comment
  preceding the items survives. Dropping the banned-symbols list silently disables an
  analyzer, which is why the element is asserted rather than assumed.
- the byte-identity case for a project with no analyzer item group — the SVGControl shape —
  which additionally asserts no `<Analyzer` element was synthesised and that the examined
  item count is 0.
- the two-item-group case, modelled on `VBFunctions.Test/VBFunctions.Test.csproj` lines
  263-265 and 287-294. It asserts two repairs, an examined item count of 2, zero residual
  lines at the stale version and exactly two lines at the manifest version, so a
  single-group implementation fails rather than silently leaving the second group stale.

## Coverage document

`coverage/p5-t14-ac13-coverage.xml`, under the gitignored `coverage/` tree. This task
records no coverage figure.

**This task checks off AC13** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
