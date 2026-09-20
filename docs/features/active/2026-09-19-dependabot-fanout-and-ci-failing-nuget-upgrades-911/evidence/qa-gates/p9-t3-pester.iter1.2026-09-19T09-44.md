# P9-T3 — PowerShell QA step 3, full Pester suite with coverage (iteration 1)

Timestamp: 2026-09-20T09-44

Command: CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p9-t3-pester-coverage.iter1.xml`.

```
pwsh -NoProfile -Command 'Set-Location "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies","tests/scripts/vscode"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p9-t3-pester-coverage.iter1.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=302 Failed=0 Skipped=0 Total=302
AGGREGATE LINE covered=1598 missed=104 pct=93.89
```

This is one of the nine unfiltered runs. The invocation carries no `$c.Filter.FullName`, so the
`Run.Path` restriction limits discovery rather than execution, nothing is marked `NotRun`, and the
executed population and `TotalCount` coincide at 302. `Total` is therefore kept as written.

The Batch D close-out measured the same 302 passed. The suite grew from 268 at P6-T3 with the
Batch D additions: the 30-case repair suite and the AC17 and AC26 extensions to the Dependabot
configuration suite.

## Aggregate JaCoCo LINE coverage

| Counter | Value |
|---|---|
| covered | 1598 |
| missed | 104 |
| **Aggregate LINE percentage** | **93.89** |

The floor is **80**, the figure the execution worktree `CLAUDE.md` states under issue #563, per gate
rule 13, and deliberately in preference to the 85 in `.claude/rules/general-unit-test.md`. The
discrepancy between the two documents is tracked as open issue #668 and is not resolved here.

## Per-file LINE coverage, every instrumented file

| `sourcefile` name | Covered | Missed | LINE percentage |
|---|---|---|---|
| `dependencies/AnalyzerItemRepair.psm1` | 106 | 0 | 100.00 |
| `dependencies/ConsistencyVerifier.psm1` | 157 | 2 | 98.74 |
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | 100.00 |
| `dependencies/PackageGraph.psm1` | 164 | 0 | 100.00 |
| `dependencies/ProjectConsistency.psm1` | 88 | 0 | 100.00 |
| `dependencies/Repair-PackageManifestConsistency.ps1` | 224 | 14 | 94.12 |
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

Each percentage is read from the `sourcefile` element whose `name` attribute equals the module file
name, taking its `counter` child with `type="LINE"` and computing covered divided by the sum of
covered and missed, times 100.

## Sync-PackageReferences.ps1 against its P0-T18 baseline

| Measurement | P0-T18 baseline | P9-T3 | Required |
|---|---|---|---|
| `vscode/Sync-PackageReferences.ps1` LINE counter | 0 covered of 84 | 95 covered of 127 | strictly greater than 0 covered |

95 is strictly greater than 0, so the clause holds. The denominator moved from 84 to 127 because the
file was rewritten by this change; the acceptance is stated against the covered count rather than a
ratio for exactly that reason.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Failed` | 0 | 0 | PASS |
| Aggregate JaCoCo LINE percentage, two decimals | at least 80 | **93.89** | PASS |
| `PackageGraph.psm1` LINE percentage | at least 90 | **100.00** | PASS |
| `PackageCompatibility.psm1` LINE percentage | at least 90 | **100.00** | PASS |
| `AnalyzerItemRepair.psm1` LINE percentage | at least 90 | **100.00** | PASS |
| `ProjectConsistency.psm1` LINE percentage | at least 90 | **100.00** | PASS |
| `ConsistencyVerifier.psm1` LINE percentage | at least 90 | **98.74** | PASS |
| `Repair-PackageManifestConsistency.ps1` LINE percentage | at least 90 | **94.12** | PASS |
| `Sync-PackageReferences.ps1` covered lines | strictly greater than 0 | **95** | PASS |
| Branch threshold | no figure claimed | none claimed | PASS |

The at-least-90 per-module figures are this change's own stricter requirement on its own new code,
which the 80 floor does not displace.

The entry point measured 91.12 at the Batch D close-out and 94.12 here. The difference is the six
lines the Batch D suite reached once the whole suite ran together rather than under the Batch D
scope; the figure is recorded as measured on this run and not carried forward from the earlier one.

## Branch coverage

Pester emits no branch counter in any output format. The JaCoCo document carries no `BRANCH` counter
at all, so the branch threshold is **unevaluable** for PowerShell and **no branch figure is claimed
here**. That is a capability limit on an unevaluable threshold, not an exclusion of any file from
measurement: every production PowerShell file under the two coverage paths is in the denominator
above.

## Gate rule 12 — this artifact records a figure that stands in for a permitted form

This is one of the six tasks in this plan that records a JaCoCo LINE figure — P0-T18, P1-T6, P2-T3,
P4-T3, P6-T3 and P9-T3 — so the standing-in statement is required of it.

The `## Committed Test Evidence Format` section of the authoritative `CLAUDE.md` defines three
permitted evidence forms, and **all three are defined against the C# route and its post-processed
Cobertura document**: a package-level JaCoCo projection of that document, the one-line first-party
coverage summary, and a trx-derived test-result summary. A Pester run emits JaCoCo directly with no
Cobertura stage, and `ConvertTo-JacocoPackageProjection` accepts Cobertura only, so **none of the
three can be produced for this route**. The figures recorded in this artifact are therefore a fourth
form the section does not define, and they **stand in for a permitted form that does not exist for
the PowerShell route rather than satisfying one that does**. The gap is stated rather than closed:
closing it would mean either committing the prohibited collector document or building a Cobertura
stage this change has no reason to build, and an unstated gap would read as compliance in the
artifact a reviewer actually reads for PowerShell coverage.

The collector document is at `coverage/p9-t3-pester-coverage.iter1.xml`, which `.gitignore:144`
covers. No `.xml` is written under the evidence tree by this task and no commit pathspec carries one.

## AC24

This task checks off **AC24**, together with the format and analyze results of the same single
toolchain pass, cited by path:

- Format: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t1-poshqc-format.iter2.2026-09-19T09-44.md` — `EXIT_CODE: 0`, rewrite count 0, `REVERT-SET: empty`.
- Analyze: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t2-poshqc-analyze.iter2.2026-09-19T09-44.md` — 13 findings, all P0-T17 baseline members, 0 in the fifteen owned files.
- Test: this artifact — `EXIT_CODE: 0`, 302 passed, 0 failed, aggregate LINE 93.89.

The three ran in that order inside one pass. Iteration 1 of the format and analyze steps is retained
at `p9-t1-poshqc-format.iter1.2026-09-19T09-44.md` and
`p9-t2-poshqc-analyze.iter1.2026-09-19T09-44.md`; that pass failed at the analyze step and is not the
pass this attestation cites.
