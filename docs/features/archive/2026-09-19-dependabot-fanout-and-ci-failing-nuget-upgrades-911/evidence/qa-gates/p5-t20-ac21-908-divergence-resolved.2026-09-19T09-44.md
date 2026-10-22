# P5-T20 — AC21 the #908 three-way divergence resolved

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1"); $c.Filter.FullName = "*AC21-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t20-ac21-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=1 Failed=0 Skipped=0 Total=13
EXECUTED=1
NOTRUN=12
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| `Total` exactly 1 and equal to the `Total` P5-T5 recorded | 1 = 1 | **executed 1**, and P5-T5's executed figure is also 1 |

The equality is taken on the **executed** population, and it holds on both readings for
completeness: P5-T5 recorded `Total=13 EXECUTED=1 NOTRUN=12` and this run records
`Total=13 EXECUTED=1 NOTRUN=12`. The executed figure is the one that carries the property
the clause exists to prove — that the same single case ran red and then green — because
the discovered figure is invariant under the filter and would read 13 whatever the filter
selected. See P5-T5 for the discrepancy record.

## Named case in the Detailed output

```
PASSED: Project consistency reconciliation and verification.Three-way divergence from pull request 908.AC21- reports separate guard and analyzer disagreements before repair and reconciles all three locations after
```

The fully expanded path is identical to the one P5-T5 recorded as failing, so the red run
and the green run are the same test rather than two tests with similar names.

## What the case asserts after repair

The fixture's manifest declares `3.0.235`; its `<Import>` and `<Error>` guards name
`3.0.259` and its `<Analyzer Include>` names `3.0.203`. Before repair the verifier reports
the guard disagreement and the analyzer-item disagreement **separately**, with found
versions `3.0.259` and `3.0.203` respectively. After repair:

- `Find-VersionDisagreement` over the repaired text reports 0 findings;
- the `<Import>` line names `Meziantou.Analyzer.3.0.235`;
- the `<Error>` line names `Meziantou.Analyzer.3.0.235`;
- the `<Analyzer Include>` line names
  `Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`.

The analyzer assertion is on the full path, not the version alone, so the preserve rule is
asserted here too: the injected listing offers `roslyn4.14`, `roslyn5.0` and `roslyn5.9`,
and `roslyn5.0` is neither the first nor the highest, so a selection implementation would
move the folder segment and fail this case.

## Coverage document

`coverage/p5-t20-ac21-coverage.xml`, under the gitignored `coverage/` tree. This task
records no coverage figure.

**This task checks off AC21** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
