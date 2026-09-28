# P5-T13 — AC12 analyzer derivation and the preserve rule

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1"); $c.Filter.FullName = "*AC12-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t13-ac12-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=10 Failed=0 Skipped=0 Total=13
EXECUTED=10
NOTRUN=3
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| `Total` | at least 9 | 10 executed (13 discovered) |

`Total` as CMD-PESTER-ALL emits it is `$r.TotalCount`, which counts filtered-out tests as
`NotRun`; the executed population is `Passed + Failed + Skipped`. Both readings satisfy
this lower bound, so the clause is met either way. P5-T5 records the distinction and why
the executed population is the reading used where the clause is an exact equality.

## Named cases in the Detailed output

```
PASSED: AC12- derives the item path for a plain language-folder shape
PASSED: AC12- derives the item path for a Roslyn-qualified shape
PASSED: AC12- derives every assembly for a multi-assembly shape whose names differ from the package
PASSED: AC12- derives the item path for a shape with no intermediate folders
PASSED: AC12- excludes non-C-sharp language folders and satellite resource assemblies
PASSED: AC12- contributes no items for a package whose listing has no analyzer directory
PASSED: AC12- preserves roslyn5.0 rather than selecting the highest offered folder
PASSED: AC12- preserves roslyn4.7 rather than selecting the highest offered folder
PASSED: AC12- leaves the item unmodified and records the missing segment when it is absent
PASSED: AC12- throws rather than guessing when the restored package directory is absent
```

Every case this task requires to be named individually is present:

- the four shape cases — plain language-folder, Roslyn-qualified, multi-assembly, and no
  intermediate folders;
- the two folder-preservation cases — Meziantou-shaped at `roslyn5.0` and Roslynator-shaped
  at `roslyn4.7`;
- the missing-segment case;
- the exclusion case;
- the no-analyzer-directory case.

The tenth, the absent-restored-directory throw, exceeds the required set.

## The three implementations this run falsifies

- An implementation computing the path from the package identifier fails the multi-assembly
  case, in which none of the four assemblies is named after the package, and fails the
  bare-directory case, in which there is no intermediate folder to assume.
- An implementation selecting the highest Roslyn-qualified folder fails **both**
  preservation cases: in the Meziantou fixture the listing offers `roslyn5.6` and
  `roslyn5.9` above the `roslyn5.0` the item names, and in the Roslynator fixture it offers
  `roslyn5.0` above the `roslyn4.7` the item names.
- An implementation guessing a replacement when the preserved segment is absent fails the
  missing-segment case, which asserts the text is returned byte-identical and that a record
  is raised instead.

## Coverage document

`coverage/p5-t13-ac12-coverage.xml`, under the gitignored `coverage/` tree. This task
records no coverage figure, so gate rule 12's standing-in obligation does not fall on it.

**This task checks off AC12** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
