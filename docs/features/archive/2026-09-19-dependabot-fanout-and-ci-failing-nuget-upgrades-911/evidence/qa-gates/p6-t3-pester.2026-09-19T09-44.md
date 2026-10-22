# P6-T3 — Pester with coverage, Batch C close-out

Timestamp: 2026-09-19T09-44

Command: CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p6-t3-pester-coverage.xml`.

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies","tests/scripts/vscode"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p6-t3-pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=268 Failed=0 Skipped=0 Total=268
AGGREGATE LINE covered=1374 missed=90 pct=93.85
```

No filter is applied, so the discovered and executed populations are the same and both read
268. The Batch B close-out at P4-T3 reported 227 passed; the 41 added here are the six
Batch C AnalyzerItemRepair-suite additions and the rest of the new dependency cases.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| Aggregate JaCoCo LINE percentage | at least 80 | **93.85** |
| `AnalyzerItemRepair.psm1` LINE percentage | at least 90 | **100.00** |
| `ProjectConsistency.psm1` LINE percentage | at least 90 | **100.00** |
| `ConsistencyVerifier.psm1` LINE percentage | at least 90 | **98.74** |

The floor of 80 is the figure the execution worktree's `CLAUDE.md` states under issue #563,
per gate rule 13, and not the 85 in `.claude/rules/general-unit-test.md`; the discrepancy is
tracked as open issue #668 and is not resolved here. The at-least-90 per-module figures are
this change's own stricter requirement on its new code, which no floor displaces.

## Per-file LINE coverage, every instrumented file

| Source file | Covered | Missed | Percent |
|---|---|---|---|
| `dependencies/AnalyzerItemRepair.psm1` | 106 | 0 | 100.00 |
| `dependencies/ConsistencyVerifier.psm1` | 157 | 2 | 98.74 |
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | 100.00 |
| `dependencies/PackageGraph.psm1` | 164 | 0 | 100.00 |
| `dependencies/ProjectConsistency.psm1` | 88 | 0 | 100.00 |
| `vscode/Install-RepoDotNetSdk.ps1` | 13 | 20 | 39.39 |
| `vscode/Invoke-MSTest.ps1` | 49 | 7 | 87.50 |
| `vscode/Invoke-MSTest.TrxSummary.ps1` | 40 | 2 | 95.24 |
| `vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 0 | 100.00 |
| `vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 | 96.97 |
| `vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 8 | 96.23 |
| `vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 0 | 100.00 |
| `vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 1 | 97.50 |
| `vscode/Invoke-MSTestWithCoverage.ps1` | 113 | 13 | 89.68 |
| `vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 0 | 100.00 |
| `vscode/Invoke-Restore.ps1` | 22 | 1 | 95.65 |
| `vscode/Invoke-VSBuild.ps1` | 46 | 3 | 93.88 |
| `vscode/Sync-PackageReferences.ps1` | 95 | 32 | 74.80 |
| `vscode/TestProcessCleanup.ps1` | 29 | 0 | 100.00 |

No `scripts/vscode` file regressed against the figures P4-T3 recorded; the only files whose
coverage changed are the three Batch C modules, which did not exist at P4-T3.

## The per-module floor was reached, not assumed

The first run of this task measured `ProjectConsistency.psm1` at **86.36** — 76 covered, 12
missed — which is below the at-least-90 clause. The clause did the work it was written for.
The twelve uncovered lines were four distinct behaviours with no test:

- the guard returning empty project text unchanged with a zero examined count;
- the branch taking an explicitly supplied `-AssemblyVersion` rather than the manifest
  version, which is the ordinary case for a package whose assembly version does not track
  it;
- the guard returning a document that is not an application configuration unchanged;
- the branch replacing an `oldVersion` written as a single version rather than a range.

Four cases were added to
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1` under a new `Context` named
`Guard clauses and explicit overrides in the reconciliation surface`, taking the file to 17
`It` blocks and 453 lines. The module then measured **100.00** with 88 covered and 0
missed. The coverage was raised by testing the behaviour, not by excluding the file or
lowering the clause.

Adding those four changed no pinned population: no new `It` name begins with any `AC<N>-`
token and the new `Context` name does not match `AC\d`, so the per-token counts P5-T4
pinned are unchanged at `AC11-` 4, `AC14-` 2, `AC16-` 2, `AC21-` 1, `AC8-` 2 and `AC23-` 2.
The P5-T4 artifact records the re-measurement.

## Gate rule 12 — this artifact records a figure that stands in for a permitted form

This is one of the six tasks in this plan that records a JaCoCo LINE figure — P0-T18,
P1-T6, P2-T3, P4-T3, P6-T3 and P9-T3 — so the standing-in statement is required of it.

The `## Committed Test Evidence Format` section of the authoritative `CLAUDE.md` defines
three permitted evidence forms, and **all three are defined against the C# route and its
post-processed Cobertura document**: a package-level JaCoCo projection of that document,
the one-line first-party coverage summary, and a trx-derived test-result summary. A Pester
run emits JaCoCo directly with no Cobertura stage, and
`ConvertTo-JacocoPackageProjection` accepts Cobertura only, so **none of the three can be
produced for this route**. The figures recorded in this artifact are therefore a fourth
form the section does not define, and they stand in for a permitted form that does not
exist for the PowerShell route rather than satisfying one that does. The gap is stated
rather than closed: closing it would mean either committing the prohibited collector
document or building a Cobertura stage this change has no reason to build, and an unstated
gap would read as compliance.

The collector's own document is at `coverage/p6-t3-pester-coverage.xml`, which
`.gitignore:144` covers. No `.xml` is written under the evidence tree and no commit
pathspec carries one.

Pester emits no branch counter in any output format — the document's `BRANCH` counter count
is 0 — so no branch-coverage figure is available for PowerShell and none is demanded. That
is a capability limit on an unevaluable threshold, not an exclusion of any file from
measurement.
